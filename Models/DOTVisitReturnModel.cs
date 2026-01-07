namespace RVMSService.Models
{
    public class DOTVisitReturnModel
    {
        public VisitModel? visit { get; set; }
        public VisitorModel? visitor { get; set; }
        public QrCodeModel? qrCode { get; set; }
        public DestinationModel? destination { get; set; }
        public VisitTypeModel? visitType { get; set; }
        public AuditTrailModel? auditTrail { get; set; }
    }
}
