using System.ComponentModel.DataAnnotations;

namespace RVMSService.Models
{
    public class VisitTypeModel
    {
        [Key]
        public Guid? TypeId { get; set; }
        public string? TypeVisit { get; set; }
        public string? TypeDescription { get; set; }
        public string? TypeColorBadge { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? Status { get; set; } //active or not active
        public bool? Default { get; set; }
    }
}
