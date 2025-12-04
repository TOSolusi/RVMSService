using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVMSService.Models
{
    public class GateModel
    {
        [Key]
        
        public Guid? GateId { get; set; }
        public string GateName { get; set; }
        public string Description { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Status { get; set; }
    }
}
