using DPUruNet;

namespace RVMSService.Helpers
{
    public class DPUruHelper
    {
        private const int MinEnrollmentSamples = 4;
        private const int MaxEnrollmentSamples = 10;
        private const int IdentifyThreshold = 21474; // ~1/100 000 false-accept rate
        private const int CaptureTimeoutMs = 5000;

        private Reader? _reader;
        private CancellationTokenSource? _cts;
        private Task? _captureTask;
        private readonly List<Fmd> _enrollFmds = new();
        private readonly Dictionary<string, Fmd> _templates = new();
        private string _currentUser = string.Empty;
        private bool _isEnrolling;

        public event Action<string>? OnStatusUpdate;
        public event Action<byte[], int, int>? OnFingerprintCaptured; // rawGray8, width, height
        public event Action<int, int>? OnEnrollmentProgress;
        public event Action<string, bool>? OnEnrollmentComplete;
        public event Action<string?, bool>? OnVerificationResult;

        public bool IsCapturing { get; private set; }

        /// <summary>
        /// Discovers connected readers and opens the first one.
        /// </summary>
        public void Init()
        {
            try
            {
                var readers = ReaderCollection.GetReaders();
                if (readers == null || readers.Count == 0)
                {
                    RaiseStatus("No fingerprint readers found. Please connect a DigitalPersona 4500.");
                    return;
                }

                _reader = readers[0];
                var openResult = _reader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);
                if (openResult != Constants.ResultCode.DP_SUCCESS)
                {
                    RaiseStatus($"Failed to open reader: {openResult}");
                    _reader = null;
                    return;
                }

                RaiseStatus($"Reader ready: {_reader.Description.Name}");
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

        public void Cleanup()
        {
            StopCapture();
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        #region Private helpers

        private void StartCaptureLoop()
        {
            if (_reader == null)
            {
                RaiseStatus("Reader not initialized.");
                return;
            }

            if (!_reader.Capabilities.CanCapture)
            {
                RaiseStatus("This reader does not support capture.");
                return;
            }

            _cts = new CancellationTokenSource();
            IsCapturing = true;
            var token = _cts.Token;
            int resolution = _reader.Capabilities.Resolutions[0];

            _captureTask = Task.Run(() => CaptureLoop(resolution, token));
        }

        private void CaptureLoop(int resolution, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                CaptureResult result;
                try
                {
                    result = _reader!.Capture(
                        Constants.Formats.Fid.ANSI,
                        Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT,
                        CaptureTimeoutMs,
                        resolution);
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                        RaiseStatus($"Capture error: {ex.Message}");
                    break;
                }

                if (token.IsCancellationRequested)
                    break;

                if (result.Quality == Constants.CaptureQuality.DP_QUALITY_CANCELED)
                    break;

                if (result.Quality == Constants.CaptureQuality.DP_QUALITY_TIMED_OUT)
                    continue;

                if (result.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    RaiseStatus($"Capture error: {result.ResultCode}");
                    continue;
                }

                if (result.Quality != Constants.CaptureQuality.DP_QUALITY_GOOD)
                {
                    RaiseStatus($"Poor quality ({result.Quality}). Try again.");
                    continue;
                }

                var fid = result.Data;
                if (fid == null || fid.Views.Count == 0)
                {
                    RaiseStatus("No image data received. Try again.");
                    continue;
                }

                var view = fid.Views[0];
                OnFingerprintCaptured?.Invoke(view.RawImage, view.Width, view.Height);

                var fmdResult = FeatureExtraction.CreateFmdFromFid(fid,
                    _isEnrolling
                        ? Constants.Formats.Fmd.DP_PRE_REGISTRATION
                        : Constants.Formats.Fmd.DP_VERIFICATION);
                if (fmdResult.ResultCode != Constants.ResultCode.DP_SUCCESS || fmdResult.Data == null)
                {
                    RaiseStatus("Feature extraction failed. Try again.");
                    continue;
                }

                if (_isEnrolling)
                {
                    bool done = ProcessEnrollment(fmdResult.Data);
                    if (done) break;
                }
                else
                {
                    ProcessVerification(fmdResult.Data);
                }
            }

            IsCapturing = false;
        }

        private void StopCapture()
        {
            if (_cts == null) return;

            _cts.Cancel();

            try { _reader?.CancelCapture(); } catch { }

            try { _captureTask?.Wait(2000); } catch { }

            _cts.Dispose();
            _cts = null;
            _captureTask = null;
            IsCapturing = false;
        }

        /// <returns>true when enrollment is finished (success or failure) and the loop should stop.</returns>
        private bool ProcessEnrollment(Fmd fmd)
        {
            _enrollFmds.Add(fmd);
            int completed = _enrollFmds.Count;

            OnEnrollmentProgress?.Invoke(
                Math.Min(completed, MinEnrollmentSamples),
                MinEnrollmentSamples);

            if (completed < MinEnrollmentSamples)
            {
                RaiseStatus($"Good scan. {MinEnrollmentSamples - completed} more touch(es) needed.");
                return false;
            }

            var enrollResult = DPUruNet.Enrollment.CreateEnrollmentFmd(
                Constants.Formats.Fmd.DP_REGISTRATION, _enrollFmds);

            if (enrollResult.ResultCode == Constants.ResultCode.DP_SUCCESS && enrollResult.Data != null)
            {
                _templates[_currentUser] = enrollResult.Data;
                RaiseStatus($"Enrollment complete for '{_currentUser}'!");
                OnEnrollmentComplete?.Invoke(_currentUser, true);
                return true;
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

        private void ProcessVerification(Fmd fmd)
        {
            var enrolledList = _templates.Values.ToArray();
            var identifyResult = Comparison.Identify(fmd, 0, enrolledList, IdentifyThreshold, enrolledList.Length);

            if (identifyResult.ResultCode == Constants.ResultCode.DP_SUCCESS
                && identifyResult.Indexes != null
                && identifyResult.Indexes.Length > 0
                && identifyResult.Indexes[0].Length > 0)
            {
                int matchIndex = identifyResult.Indexes[0][0];
                string matchedUser = _templates.Keys.ElementAt(matchIndex);
                RaiseStatus($"Match found: {matchedUser}");
                OnVerificationResult?.Invoke(matchedUser, true);
            }
            else
            {
                RaiseStatus("Fingerprint not recognized.");
                OnVerificationResult?.Invoke(null, false);
            }
        }

        private void RaiseStatus(string message) => OnStatusUpdate?.Invoke(message);

        #endregion
    }
}

