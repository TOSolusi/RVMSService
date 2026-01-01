namespace RVMSService.Models
{
    public class DOTVisitModel
    {
        public VisitModel? visit { get; set; }
        public VisitorModel? visitor { get; set; }
        public QrCodeModel? qrCode { get; set; }
        public AuditTrailModel? auditTrail { get; set; }
    }
}
