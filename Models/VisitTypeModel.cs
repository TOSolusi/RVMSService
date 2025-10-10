using System.ComponentModel.DataAnnotations;

namespace RVMSService.Models
{
    public class VisitTypeModel
    {
        [Key]
        public Guid TypeId { get; set; }
        public string TypeVisit { get; set; }
        public int TypeColorBadge { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Status { get; set; }
        public bool Default { get; set; }
    }
}
