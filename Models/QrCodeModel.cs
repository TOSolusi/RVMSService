using System.ComponentModel.DataAnnotations;

namespace RVMSService.Models
{
    public class QrCodeModel
    {
        [Key]
        public Guid QrId { get; set; }
        public string QrString { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsed { get; set; }
        public bool Status { get; set; } //active/inactive
        public bool Used { get; set; } //used/not used

    }
}
