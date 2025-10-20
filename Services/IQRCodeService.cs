using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IQRCodeService
    {
        Task<Guid?> AddQRCode(QrCodeModel qrCode);
        Task<List<QrCodeModel>> GetAllQRCodes();
        Task<List<QrCodeModel>> GetActiveQRCodes(Guid gateId);
        Task UpdateQrCode(QrCodeModel qrCode);
        Task<bool> deleteQrCode(Guid qrCodeId);


    }
}