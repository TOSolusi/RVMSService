using Microsoft.EntityFrameworkCore;
using RVMSService.Data;
using RVMSService.Models;

namespace RVMSService.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<QRCodeService> _logger;
        private readonly IAuditTrailService _auditTrail;

        public QRCodeService(AppDBContext context, ILogger<QRCodeService> logger, IAuditTrailService auditTrail)
        {
            context = _context;
            _logger = logger;
            _auditTrail = auditTrail;

        }

        public async Task<Guid?> AddQRCode(QrCodeModel qrCode)
        {
            try
            {

                _logger.LogInformation("Adding new QR Code");

                // Implementation for adding destination
                await _context.QrCodes.AddAsync(qrCode);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"QR Codes added with ID : {qrCode.QrId}");

                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Add QR Code {qrCode.QrString}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "QR Code"
                };
                await _auditTrail.RecordAsync(audit);

                // After SaveChangesAsync, destination.DestinationId will contain the generated GUID
                return qrCode.QrId;
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Add QR Code {qrCode.QrString} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "QR Code"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while adding QR Code to database");
                throw new Exception("An error occurred while adding the QR Code.", ex);
            }
        }

        public async Task<List<QrCodeModel>> GetAllQRCodes()
        {
            try
            {
                _logger.LogInformation("Retrieving all QR Codes");
                var qrCodes = await _context.QrCodes.ToListAsync();
                _logger.LogInformation("Retrieved {Count} QR Codes", qrCodes.Count);
                return qrCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving QR Codes from database");
                throw new Exception("An error occurred while retrieving the QR Codes.", ex);
            }
        }

        public async Task<List<QrCodeModel>> GetActiveQRCodes(Guid gateId)
        {
            try
            {
                _logger.LogInformation($"Retrieving active QR Codes for gate {gateId}");
                var qrCodes = await _context.QrCodes.Where(q => ((q.Status == true) && (q.Used == false) && (q.GateId == gateId))).ToListAsync();
                _logger.LogInformation("Retrieved {Count} active QR Codes", qrCodes.Count);
                return qrCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving active QR Codes from database");
                throw new Exception("An error occurred while retrieving the active QR Codes.", ex);
            }
        }

        public async Task UpdateQrCode(QrCodeModel qrCode)
        {
            try
            {
                _logger.LogInformation($"Updating QR Code with ID: {qrCode.QrId}");
                _context.QrCodes.Update(qrCode);
                await _context.SaveChangesAsync();
                _logger.LogInformation("QR Code updated successfully");
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Update QR Code {qrCode.QrString}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "QR Code"
                };
                await _auditTrail.RecordAsync(audit);
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Update QR Code {qrCode.QrString} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "QR Code"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while updating QR Code in database");
                throw new Exception("An error occurred while updating the QR Code.", ex);
            }
        }

        public async Task<bool> deleteQrCode(Guid qrCodeId)
        {
            try
            {
                _logger.LogInformation($"Deleting QR Code ID: {qrCodeId}");
                var existingQrCode = await _context.QrCodes.FindAsync(qrCodeId);
                if (existingQrCode == null)
                {
                    _logger.LogWarning($"Qr Code with ID: {qrCodeId} not found");
                    throw new Exception("QRCode not found");
                }
                _context.QrCodes.Remove(existingQrCode);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"QR Codes with ID: {qrCodeId} deleted successfully");
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Delete Qr Code {existingQrCode.QrString}.",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "QR Code"
                };
                await _auditTrail.RecordAsync(audit);
                return true;
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Delete QR Codes {qrCodeId} fail.",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "QR Code"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, $"Error occurred while deleting QR Code with ID: {qrCodeId}");
                throw new Exception("An error occurred while deleting the QR Code.", ex);
            }
        }
    }
}
