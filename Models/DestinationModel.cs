using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVMSService.Models
{
    public class DestinationModel
    {
        [Key]
        public Guid DestinationId { get; set; }
        //public List<GateModel>? Gates { get; set; }
        public string? Gates { get; set; }
        [NotMapped]
        public List<Guid> GateIds
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Gates))
                    return new List<Guid>();

                return Gates.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(g => Guid.Parse(g.Trim()))
                           .ToList();
            }
            set
            {
                Gates = value != null && value.Any()
                    ? string.Join(",", value)
                    : null;
            }
        }
        public string Address { get; set; }
        public string Owner_Name { get; set; }
        public string? Owner_Email { get; set; }
        public string? Owner_Phone { get; set; }
        public string? Owner_TelegramChatId { get; set; }
        public string? Notes { get; set; } = null;
        public DateTime Updated_At { get; set; }
        public bool Status { get; set; }
        public bool IsLinked
        {
            get
            {
                return  !string.IsNullOrWhiteSpace(Owner_TelegramChatId);
            }
        }
        
         
    }

    public class DotDestinationModel
    {
       public DestinationModel Destination { get; set; }
        public AuditTrailModel AuditTrail { get; set; }
    }
}
