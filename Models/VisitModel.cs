using System.ComponentModel.DataAnnotations;

namespace RVMSService.Models
{
    public class VisitModel
    {
        [Key]
        public Guid VisitId { get; set; }
        public Guid VisitorId { get; set; }
        public Guid TypeId { get; set; }
        public Guid GateId { get; set; }
        public Guid QrId { get; set; }
        public Guid DestinationId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        public byte[] CurrentPhoto { get; set; }
        public byte[] VehiclePhoto { get; set; }
        public byte[] AdditionalPhoto { get; set; }
        public bool Status { get; set; }

    }
}
