using System.ComponentModel.DataAnnotations;

namespace RVMSService.Models
{
    public class VisitorModel
    {
        [Key]
        public Guid VisitorId { get; set; }
        public string? VisitorIdNo { get; set; } //ID No as per ID badge
        public string? VisitorName { get; set; }
        public string? VisitorPhone { get; set; }
        public DateTime LastVisit { get; set; }
        public byte[]? VisitorImage { get; set; }
        public bool? Blacklist { get; set; }
    }
}
