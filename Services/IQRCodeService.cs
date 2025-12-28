using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IQRCodeService
    {
        Task<Guid?> AddQRCode(QrCodeModel qrCode, AuditTrailModel auditTrail);
        Task<List<QrCodeModel>> GetAllQRCodes();
        Task<List<QrCodeModel>> GetActiveQRCodes(Guid gateId);
        Task UpdateQrCode(DOTQRModel dotQR);
        Task<bool> deleteQrCode(DOTQRModel dotQR);


    }
}