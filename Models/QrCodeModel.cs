using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVMSService.Models
{
    public class QrCodeModel
    {
        [Key]
        public Guid QrId { get; set; }
        public string? QrString { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool? Status { get; set; } //aActive/inactive
        public bool? Used { get; set; } //used/not used
        public Guid? GateId { get; set; } //nullable
        public Guid? VisitId { get; set; } //nullable
        

    }

   
    public class DOTQRModel
    {
        public QrCodeModel? QrCode { get; set; }
        public AuditTrailModel? AuditTrail { get; set; }
    }
}
