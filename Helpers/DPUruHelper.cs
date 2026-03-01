using System.Runtime.InteropServices;

namespace RVMSService.Helpers
{
    /// <summary>
    /// Manages a DigitalPersona 4500 fingerprint reader via the native
    /// dpfpdd.dll / dpfp.dll SDK. Replaces the .NET-Framework-only DPUruNet package.
    /// </summary>
    public sealed class DPUruHelper : IDisposable
    {
        private const int  MinEnrollmentSamples = 4;
        private const int  MaxEnrollmentSamples = 10;
        private const uint IdentifyThreshold    = 21474;  // ~1/100 000 false-accept rate
        private const uint CaptureTimeoutMs     = 5000;

        private IntPtr                                          _dev = IntPtr.Zero;
        private uint                                            _resolution;
        private CancellationTokenSource?                        _cts;
        private Task?                                           _captureTask;
        private readonly List<(byte[] Data, uint Size)>         _enrollFmds = new();
        private readonly Dictionary<string, (byte[] Data, uint Size)> _templates = new();
        private string                                          _currentUser = string.Empty;
        private bool                                            _isEnrolling;
        private bool                                            _disposed;

        // ── Public events ────────────────────────────────────────────────────────
        public event Action<string>?          OnStatusUpdate;
        public event Action<byte[], int, int>? OnFingerprintCaptured;  // rawGray8, width, height
        public event Action<int, int>?        OnEnrollmentProgress;
        public event Action<string, bool>?    OnEnrollmentComplete;
        public event Action<string?, bool>?   OnVerificationResult;

        public bool IsCapturing { get; private set; }

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        /// <summary>Initialises the SDK and opens the first detected reader.</summary>
        public void Init()
        {
            try
            {
                if (DpfpddNative.dpfpdd_init() != DpfpddNative.DPFPDD_SUCCESS)
                {
                    RaiseStatus("dpfpdd_init failed.");
                    return;
                }

                if (DpfpddNative.dpfp_init() != DpfpddNative.DPFPDD_SUCCESS)
                {
                    RaiseStatus("dpfp_init failed.");
                    DpfpddNative.dpfpdd_exit();
                    return;
                }

                // Query device count first.
                uint count = 0;
                DpfpddNative.dpfpdd_query_devices(ref count, IntPtr.Zero);

                if (count == 0)
                {
                    RaiseStatus("No fingerprint readers found. Please connect a DigitalPersona 4500.");
                    return;
                }

                // Allocate unmanaged array and enumerate devices.
                int    infoSize = Marshal.SizeOf<DpfpddNative.DPFPDD_DEV_INFO>();
                IntPtr infoArr  = Marshal.AllocHGlobal(infoSize * (int)count);
                try
                {
                    // The SDK requires the size field to be pre-filled on every element.
                    for (int i = 0; i < (int)count; i++)
                        Marshal.WriteInt32(IntPtr.Add(infoArr, i * infoSize), infoSize);

                    DpfpddNative.dpfpdd_query_devices(ref count, infoArr);

                    var info = Marshal.PtrToStructure<DpfpddNative.DPFPDD_DEV_INFO>(infoArr);

                    int rc = DpfpddNative.dpfpdd_open_ext(
                        info.id, DpfpddNative.PRIORITY_COOPERATIVE, out _dev);

                    if (rc != DpfpddNative.DPFPDD_SUCCESS)
                    {
                        RaiseStatus($"Failed to open reader (code 0x{rc:X8}).");
                        _dev = IntPtr.Zero;
                        return;
                    }

                    var caps = new DpfpddNative.DPFPDD_DEV_CAPS
                    {
                        size        = (uint)Marshal.SizeOf<DpfpddNative.DPFPDD_DEV_CAPS>(),
                        resolutions = new uint[8]
                    };
                    DpfpddNative.dpfpdd_get_device_capabilities(_dev, ref caps);
                    _resolution = caps.num_resolutions > 0 ? caps.resolutions[0] : 500;

                    RaiseStatus($"Reader ready: {info.descr}");
                }
                finally
                {
                    Marshal.FreeHGlobal(infoArr);
                }
            }
            catch (Exception ex)
            {
                RaiseStatus($"Failed to initialize reader: {ex.Message}");
            }
        }

        public void StartEnrollment(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                RaiseStatus("Enter a user name first.");
                return;
            }

            StopCapture();
            _currentUser = userName;
            _isEnrolling = true;
            _enrollFmds.Clear();
            OnEnrollmentProgress?.Invoke(0, MinEnrollmentSamples);
            StartCaptureLoop();
            RaiseStatus($"Enrolling '{userName}': touch the reader (at least {MinEnrollmentSamples} times).");
        }

        public void StartVerification()
        {
            if (_templates.Count == 0)
            {
                RaiseStatus("No enrolled users. Please enroll a fingerprint first.");
                return;
            }

            StopCapture();
            _isEnrolling = false;
            StartCaptureLoop();
            RaiseStatus("Place your finger on the reader to verify.");
        }

        public void Stop()
        {
            StopCapture();
            RaiseStatus("Capture stopped.");
        }

        public void Cleanup() => Dispose();

        // ── IDisposable ───────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopCapture();

            if (_dev != IntPtr.Zero)
            {
                DpfpddNative.dpfpdd_close(_dev);
                _dev = IntPtr.Zero;
            }

            DpfpddNative.dpfp_exit();
            DpfpddNative.dpfpdd_exit();
        }

        // ── Private: capture loop ─────────────────────────────────────────────────

        private void StartCaptureLoop()
        {
            if (_dev == IntPtr.Zero)
            {
                RaiseStatus("Reader not initialized.");
                return;
            }

            _cts        = new CancellationTokenSource();
            IsCapturing = true;
            var token   = _cts.Token;
            _captureTask = Task.Run(() => CaptureLoop(token));
        }

        private void CaptureLoop(CancellationToken token)
        {
            var param = new DpfpddNative.DPFPDD_CAPTURE_PARAM
            {
                size       = (uint)Marshal.SizeOf<DpfpddNative.DPFPDD_CAPTURE_PARAM>(),
                image_fmt  = DpfpddNative.IMG_FMT_ANSI381,
                image_proc = DpfpddNative.IMG_PROC_DEFAULT,
                image_res  = _resolution
            };

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!TryCaptureImage(ref param, out var imageData, out var captureResult))
                        continue;

                    if (token.IsCancellationRequested) break;

                    if (captureResult.quality == DpfpddNative.QUALITY_TIMED_OUT)
                        continue;

                    if (captureResult.success == 0 || captureResult.quality != DpfpddNative.QUALITY_GOOD)
                    {
                        RaiseStatus($"Poor quality ({captureResult.quality}). Try again.");
                        continue;
                    }

                    OnFingerprintCaptured?.Invoke(
                        imageData,
                        (int)captureResult.info.width,
                        (int)captureResult.info.height);

                    uint fmdFormat = _isEnrolling
                        ? DpfpddNative.FMD_DP_PRE_REG
                        : DpfpddNative.FMD_DP_VERIFICATION;

                    int fmdRc = DpfpddNative.dpfp_create_fmd_from_fid(
                        fmdFormat, imageData, (uint)imageData.Length,
                        out IntPtr fmdPtr, out uint fmdSize);

                    if (fmdRc != DpfpddNative.DPFPDD_SUCCESS || fmdPtr == IntPtr.Zero)
                    {
                        RaiseStatus("Feature extraction failed. Try again.");
                        continue;
                    }

                    // Copy FMD into managed memory then immediately free the SDK buffer.
                    var fmdBytes = new byte[fmdSize];
                    Marshal.Copy(fmdPtr, fmdBytes, 0, (int)fmdSize);
                    DpfpddNative.dpfp_free(fmdPtr);

                    bool done = _isEnrolling
                        ? ProcessEnrollment(fmdBytes, fmdSize)
                        : ProcessVerification(fmdBytes, fmdSize);

                    if (done) break;
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                        RaiseStatus($"Capture error: {ex.Message}");
                    break;
                }
            }

            IsCapturing = false;
        }

        /// <summary>
        /// Two-step capture: probe the required buffer size (blocking), then fill it.
        /// Returns false only on an unexpected SDK error.
        /// </summary>
        private bool TryCaptureImage(
            ref DpfpddNative.DPFPDD_CAPTURE_PARAM    param,
            out byte[]                                imageData,
            out DpfpddNative.DPFPDD_CAPTURE_RESULT   captureResult)
        {
            imageData     = Array.Empty<byte>();
            captureResult = NewCaptureResult();
            uint imgSize  = 0;

            // Step 1 – blocking call; waits up to CaptureTimeoutMs for a finger.
            //          Returns DPFPDD_E_MORE_DATA on success (no buffer supplied).
            int rc = DpfpddNative.dpfpdd_capture(
                _dev, ref param, CaptureTimeoutMs, ref captureResult, ref imgSize, null);

            if (rc == DpfpddNative.DPFPDD_E_TIMED_OUT || imgSize == 0)
            {
                captureResult.quality = DpfpddNative.QUALITY_TIMED_OUT;
                return true;
            }

            if (rc != DpfpddNative.DPFPDD_E_MORE_DATA)
                return false;

            // Step 2 – retrieve the captured image into an allocated buffer.
            captureResult = NewCaptureResult();
            imageData     = new byte[imgSize];
            DpfpddNative.dpfpdd_capture(
                _dev, ref param, CaptureTimeoutMs, ref captureResult, ref imgSize, imageData);

            return true;
        }

        private static DpfpddNative.DPFPDD_CAPTURE_RESULT NewCaptureResult() =>
            new DpfpddNative.DPFPDD_CAPTURE_RESULT
            {
                size = (uint)Marshal.SizeOf<DpfpddNative.DPFPDD_CAPTURE_RESULT>(),
                info = new DpfpddNative.DPFPDD_IMAGE_INFO
                {
                    size = (uint)Marshal.SizeOf<DpfpddNative.DPFPDD_IMAGE_INFO>()
                }
            };

        private void StopCapture()
        {
            if (_cts == null) return;

            _cts.Cancel();

            if (_dev != IntPtr.Zero)
                try { DpfpddNative.dpfpdd_capture_cancel(_dev); } catch { }

            try { _captureTask?.Wait(2000); } catch { }

            _cts.Dispose();
            _cts         = null;
            _captureTask = null;
            IsCapturing  = false;
        }

        // ── Enrollment ────────────────────────────────────────────────────────────

        /// <returns>true when the capture loop should stop.</returns>
        private bool ProcessEnrollment(byte[] fmdBytes, uint fmdSize)
        {
            _enrollFmds.Add((fmdBytes, fmdSize));
            int completed = _enrollFmds.Count;

            OnEnrollmentProgress?.Invoke(
                Math.Min(completed, MinEnrollmentSamples),
                MinEnrollmentSamples);

            if (completed < MinEnrollmentSamples)
            {
                RaiseStatus($"Good scan. {MinEnrollmentSamples - completed} more touch(es) needed.");
                return false;
            }

            // Pin each pre-registration FMD buffer to get stable native pointers.
            var handles = _enrollFmds.Select(f => GCHandle.Alloc(f.Data, GCHandleType.Pinned)).ToArray();
            try
            {
                var ptrs = handles.Select(h => h.AddrOfPinnedObject()).ToArray();

                int rc = DpfpddNative.dpfp_create_enrollment_fmd(
                    DpfpddNative.FMD_DP_REGISTRATION,
                    ptrs,
                    (uint)ptrs.Length,
                    out IntPtr enrollPtr,
                    out uint   enrollSize);

                if (rc == DpfpddNative.DPFPDD_SUCCESS && enrollPtr != IntPtr.Zero)
                {
                    var enrollBytes = new byte[enrollSize];
                    Marshal.Copy(enrollPtr, enrollBytes, 0, (int)enrollSize);
                    DpfpddNative.dpfp_free(enrollPtr);

                    _templates[_currentUser] = (enrollBytes, enrollSize);
                    _enrollFmds.Clear();
                    RaiseStatus($"Enrollment complete for '{_currentUser}'!");
                    OnEnrollmentComplete?.Invoke(_currentUser, true);
                    return true;
                }
            }
            finally
            {
                foreach (var h in handles) h.Free();
            }

            if (completed >= MaxEnrollmentSamples)
            {
                _enrollFmds.Clear();
                RaiseStatus($"Enrollment failed after {completed} attempts. Try again.");
                OnEnrollmentComplete?.Invoke(_currentUser, false);
                return true;
            }

            RaiseStatus($"Need more variation ({completed} scans so far). Touch again with slight angle change.");
            return false;
        }

        // ── Verification ──────────────────────────────────────────────────────────

        private bool ProcessVerification(byte[] sampleFmdBytes, uint sampleFmdSize)
        {
            var users    = _templates.Keys.ToArray();
            var tmplList = _templates.Values.ToArray();
            uint count   = (uint)tmplList.Length;

            var handles = tmplList.Select(t => GCHandle.Alloc(t.Data, GCHandleType.Pinned)).ToArray();
            try
            {
                var ptrs    = handles.Select(h => h.AddrOfPinnedObject()).ToArray();
                var sizes   = tmplList.Select(t => t.Size).ToArray();
                var indices = new uint[count];

                int rc = DpfpddNative.dpfp_identify(
                    sampleFmdBytes, sampleFmdSize,
                    0,
                    ptrs, sizes, count,
                    IdentifyThreshold,
                    out uint resultCount,
                    indices);

                if (rc == DpfpddNative.DPFPDD_SUCCESS && resultCount > 0)
                {
                    string matchedUser = users[indices[0]];
                    RaiseStatus($"Match found: {matchedUser}");
                    OnVerificationResult?.Invoke(matchedUser, true);
                }
                else
                {
                    RaiseStatus("Fingerprint not recognized.");
                    OnVerificationResult?.Invoke(null, false);
                }
            }
            finally
            {
                foreach (var h in handles) h.Free();
            }

            return false;  // verification loop keeps running
        }

        private void RaiseStatus(string message) => OnStatusUpdate?.Invoke(message);
    }
}

