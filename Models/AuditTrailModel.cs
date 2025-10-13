using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVMSService.Models
{
    public class AuditTrailModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? AuditTrailId { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } //success or Failure
        public string Category { get; set; } //Login, Gate, Vehicle, User

    }
}
