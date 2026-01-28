namespace RVMSService.Models
{
    public class CapturedImageDataModel
    {
        public string CameraName { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime CapturedAt { get; set; }
        public string CameraType { get; set; }
    }
}
