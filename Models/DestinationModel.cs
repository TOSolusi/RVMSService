using System.ComponentModel.DataAnnotations;

namespace RVMSService.Models
{
    public class DestinationModel
    {
        [Key]
        public Guid DestinationId { get; set; }
        public Guid GateId { get; set; }
        public string Address { get; set; }
        public string Owner_Name { get; set; }
        public string Owner_Email { get; set; }
        public string Owner_Phone { get; set; }
        public string Notes { get; set; }
        public DateTime Updated_At { get; set; }
        public bool Status { get; set; }
    }
}
