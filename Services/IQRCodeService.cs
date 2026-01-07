using RVMSService.Models;
using System.Threading.Tasks;

namespace RVMSService.Services
{
    public interface IQRCodeService
    {
        Task<Guid?> AddQRCode(QrCodeModel qrCode, AuditTrailModel auditTrail);
        Task<List<QrCodeModel>> GetAllQRCodes();
        Task<List<QrCodeModel>> GetQRCodesbyGateId(Guid gateId);
        Task<bool> UpdateQrCode(DOTQRModel dotQR);
        Task<bool> deleteQrCode(DOTQRModel dotQR);
        Task<QrCodeModel?> GetQRCodeById(Guid qrId);


    }
}