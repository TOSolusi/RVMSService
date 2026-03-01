using System.Runtime.InteropServices;

namespace RVMSService.Helpers
{
    /// <summary>
    /// P/Invoke declarations for the DigitalPersona DPFPDD native SDK.
    /// Requires dpfpdd.dll (device access) and dpfp.dll (feature processing)
    /// from the DigitalPersona DPFPDD SDK installed on the host machine.
    /// </summary>
    internal static class DpfpddNative
    {
        private const string DeviceDll  = "dpfpdd.dll";
        private const string ProcessDll = "dpfp.dll";

        // ── Return codes ─────────────────────────────────────────────────────────
        internal const int  DPFPDD_SUCCESS     = 0;
        internal const int  DPFPDD_E_MORE_DATA = 0x1B;                        // buffer too small; retry with reported size
        internal const int  DPFPDD_E_TIMED_OUT = unchecked((int)0x80000008);  // no finger placed in timeout window

        // ── Image format ──────────────────────────────────────────────────────────
        internal const uint IMG_FMT_ANSI381      = 0;
        internal const uint IMG_FMT_PIXEL_BUFFER = 2;  // 8-bit gray raw

        // ── Image processing ──────────────────────────────────────────────────────
        internal const uint IMG_PROC_DEFAULT = 0;

        // ── Open priority ─────────────────────────────────────────────────────────
        internal const int PRIORITY_COOPERATIVE = 0;

        // ── Capture quality ───────────────────────────────────────────────────────
        internal const uint QUALITY_GOOD      = 0;
        internal const uint QUALITY_TIMED_OUT = 4;

        // ── FMD types (dpfp.dll) ──────────────────────────────────────────────────
        internal const uint FMD_DP_PRE_REG      = 0;  // enrollment sample
        internal const uint FMD_DP_REGISTRATION = 1;  // enrolled template
        internal const uint FMD_DP_VERIFICATION = 2;  // verification sample

        // ─────────────────────────────────────────────────────────────────────────
        // Structures – must match the native SDK headers byte-for-byte.
        // ─────────────────────────────────────────────────────────────────────────

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct DPFPDD_DEV_INFO
        {
            public uint size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]  public string descr;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)] public string id;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DPFPDD_DEV_CAPS
        {
            public uint size;
            public uint can_capture_image;
            public uint can_stream_image;
            public uint can_extract_features;
            public uint can_match;
            public uint can_identify;
            public uint has_fp_storage;
            public uint indicator_type;
            public uint has_power_mgmt;
            public uint has_calibration;
            public uint piv_compliant;
            public uint num_resolutions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public uint[] resolutions;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DPFPDD_CAPTURE_PARAM
        {
            public uint size;
            public uint image_fmt;
            public uint image_proc;
            public uint image_res;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DPFPDD_IMAGE_INFO
        {
            public uint size;
            public uint width;
            public uint height;
            public uint res;
            public uint bpp;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DPFPDD_CAPTURE_RESULT
        {
            public uint             size;
            public int              success;
            public uint             quality;
            public DPFPDD_IMAGE_INFO info;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // dpfpdd.dll – device access
        // ─────────────────────────────────────────────────────────────────────────

        [DllImport(DeviceDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfpdd_init();

        [DllImport(DeviceDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfpdd_exit();

        /// <param name="count">In: array capacity. Out: number of devices found.</param>
        /// <param name="devInfo">Unmanaged array of DPFPDD_DEV_INFO, or IntPtr.Zero to query count only.</param>
        [DllImport(DeviceDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfpdd_query_devices(ref uint count, IntPtr devInfo);

        [DllImport(DeviceDll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int dpfpdd_open_ext(string devName, int priority, out IntPtr dev);

        [DllImport(DeviceDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfpdd_close(IntPtr dev);

        [DllImport(DeviceDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfpdd_get_device_capabilities(IntPtr dev, ref DPFPDD_DEV_CAPS caps);

        /// <summary>
        /// Two-step capture: call with <paramref name="imageData"/>=null first to get the required
        /// buffer size (returns DPFPDD_E_MORE_DATA), then call again with an allocated buffer.
        /// Both calls are blocking up to <paramref name="timeout"/> ms.
        /// </summary>
        [DllImport(DeviceDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfpdd_capture(
            IntPtr dev,
            ref DPFPDD_CAPTURE_PARAM  captureParam,
            uint                      timeout,
            ref DPFPDD_CAPTURE_RESULT captureResult,
            ref uint                  imageSize,
            [In, Out] byte[]?         imageData);

        [DllImport(DeviceDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfpdd_capture_cancel(IntPtr dev);

        // ─────────────────────────────────────────────────────────────────────────
        // dpfp.dll – feature extraction & matching
        // ─────────────────────────────────────────────────────────────────────────

        [DllImport(ProcessDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfp_init();

        [DllImport(ProcessDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfp_exit();

        /// <summary>
        /// Extracts an FMD from a captured FID image buffer.
        /// On success <paramref name="fmd"/> points to SDK-allocated memory;
        /// the caller MUST free it with <see cref="dpfp_free"/>.
        /// </summary>
        [DllImport(ProcessDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfp_create_fmd_from_fid(
            uint        fmdType,
            [In] byte[] fid,
            uint        fidSize,
            out IntPtr  fmd,
            out uint    fmdSize);

        /// <summary>
        /// Combines pre-registration FMD samples into a single enrollment template.
        /// <paramref name="fmds"/> is an array of pointers to pinned pre-registration FMD buffers.
        /// On success <paramref name="enrollmentFmd"/> points to SDK-allocated memory;
        /// the caller MUST free it with <see cref="dpfp_free"/>.
        /// </summary>
        [DllImport(ProcessDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfp_create_enrollment_fmd(
            uint          fmdType,
            [In] IntPtr[] fmds,
            uint          count,
            out IntPtr    enrollmentFmd,
            out uint      enrollmentFmdSize);

        /// <summary>
        /// Identifies a sample FMD against an array of enrolled templates.
        /// <paramref name="resultIndices"/> must be pre-allocated with at least
        /// <paramref name="count"/> elements.
        /// </summary>
        [DllImport(ProcessDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern int dpfp_identify(
            [In] byte[]      sampleFmd,
            uint             sampleFmdSize,
            uint             begin,
            [In] IntPtr[]    fmds,
            [In] uint[]      fmdSizes,
            uint             count,
            uint             threshold,
            out uint         resultCount,
            [In, Out] uint[] resultIndices);

        /// <summary>Frees memory allocated by the dpfp library.</summary>
        [DllImport(ProcessDll, CallingConvention = CallingConvention.StdCall)]
        internal static extern void dpfp_free(IntPtr ptr);
    }
}