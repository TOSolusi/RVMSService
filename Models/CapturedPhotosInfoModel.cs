namespace RVMSService.Models
{
    public class CapturedPhotosInfoModel
    {
        public string CameraName { get; set; }
        public DateTime CapturedAt { get; set; }
        public string CameraType { get; set; }
        public int ImageSize { get; set; } // Size in bytes (optional)

    }
}
