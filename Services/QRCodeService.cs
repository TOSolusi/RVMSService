using Microsoft.EntityFrameworkCore;
using RVMSService.Data;
using RVMSService.Models;
using System.Runtime.CompilerServices;

namespace RVMSService.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<QRCodeService> _logger;
        private readonly IAuditTrailService _auditTrail;

        public QRCodeService(AppDBContext context, ILogger<QRCodeService> logger, IAuditTrailService auditTrail)
        {
            _context = context;
            _logger = logger;
            _auditTrail = auditTrail;

        }

        public async Task<Guid?> AddQRCode(QrCodeModel qrCode, AuditTrailModel auditTrail)
        {
            try
            {

                _logger.LogInformation("Adding new QR Code");


                qrCode.CreatedAt = DateTime.Now;
                qrCode.Status = true;
                qrCode.Used = false;
                // Implementation for adding destination
                await _context.QrCodes.AddAsync(qrCode);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"QR Codes added with ID : {qrCode.QrId}");

                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Add QR Code {qrCode.QrString}",
                    Timestamp = DateTime.Now,
                    Status = "Success",
                    Category = "QR Code",
                    UserName = auditTrail.UserName,
                    Location = auditTrail.Location

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
                    Timestamp = DateTime.Now,
                    Status = "Failure",
                    Category = "QR Code",
                    UserName = auditTrail.UserName,
                    Location = auditTrail.Location
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

        public async Task<bool> UpdateQrCode(DOTQRModel dotQR)
        {
            try
            {
                _logger.LogInformation($"Updating QR Code with ID: {dotQR.QrCode.QrId}");

                // Retrieve the existing QR code from the database
                var existingQrCode = await _context.QrCodes.FindAsync(dotQR.QrCode.QrId);
                if (existingQrCode == null)
                {
                    _logger.LogWarning($"QR Code with ID {dotQR.QrCode.QrId} not found");
                    return false;
                }

                // Update the properties
                existingQrCode.QrString = dotQR.QrCode.QrString ?? existingQrCode.QrString;
                existingQrCode.Notes = dotQR.QrCode.Notes ?? existingQrCode.Notes;
                existingQrCode.LastUsed = dotQR.QrCode.LastUsed ?? existingQrCode.LastUsed;
                existingQrCode.Status = dotQR.QrCode.Status ?? existingQrCode.Status;
                existingQrCode.Used = dotQR.QrCode.Used ?? existingQrCode.Used;
                existingQrCode.GateId = dotQR.QrCode.GateId ?? existingQrCode.GateId;
                existingQrCode.VisitId = dotQR.QrCode.VisitId ?? existingQrCode.VisitId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("QR Code updated successfully");
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Update QR Code {dotQR.QrCode.QrString}",
                    Timestamp = DateTime.Now,
                    Status = "Success",
                    Category = "QR Code",
                    Location = dotQR.AuditTrail.Location,
                    UserName = dotQR.AuditTrail.UserName
                };
                await _auditTrail.RecordAsync(audit);
                return true;
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Update QR Code {dotQR.QrCode.QrString} fail",
                    Timestamp = DateTime.Now,
                    Status = "Failure",
                    Category = "QR Code"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while updating QR Code in database");
                throw new Exception("An error occurred while updating the QR Code.", ex);
            }
        }


        public async Task<bool> deleteQrCode(DOTQRModel dotQR)
        {
            try
            {
                _logger.LogInformation($"Deleting QR Code ID: {dotQR.QrCode.QrId}");
                var existingQrCode = await _context.QrCodes.FindAsync(dotQR.QrCode.QrId);
                if (existingQrCode == null)
                {
                    _logger.LogWarning($"Qr Code with ID: {dotQR.QrCode.QrId} not found");
                    throw new Exception("QRCode not found");
                }
                _context.QrCodes.Remove(existingQrCode);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"QR Codes with ID: {dotQR.QrCode.QrId} deleted successfully");
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    UserName = dotQR.AuditTrail.UserName,
                    UserId = dotQR.AuditTrail.UserId,
                    Location = dotQR.AuditTrail.Location,
                    Description = $"Delete Qr Code {existingQrCode.QrString}.",
                    Timestamp = DateTime.Now,
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
                    Description = $"Delete QR Codes {dotQR.QrCode.QrId} fail.",
                    Timestamp = DateTime.Now,
                    Status = "Failure",
                    Category = "QR Code",
                    UserName = dotQR.AuditTrail.UserName,
                    Location = dotQR.AuditTrail.Location,
                    UserId = dotQR.AuditTrail.UserId

                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, $"Error occurred while deleting QR Code with ID: {dotQR.QrCode.QrId}");
                throw new Exception("An error occurred while deleting the QR Code.", ex);
            }
        }

        public async Task<List<QrCodeModel>> GetQRCodesbyGateId(Guid gateId)
        {
            try
            {
                _logger.LogInformation($"Retrieving QR Codes for gate {gateId}");
                var qrCodes = await _context.QrCodes.Where(q => ((q.Status == true) && (q.GateId == gateId))).ToListAsync();
                _logger.LogInformation("Retrieved {Count} QR Codes", qrCodes.Count);
                return qrCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving used QR Codes from database");
                throw new Exception("An error occurred while retrieving the used QR Codes.", ex);
            }
        }

        public async Task<QrCodeModel?> GetQRCodeById(Guid qrId)
        {
            try
            {
                _logger.LogInformation("GetQRCodeById called for QrId: {QrId}", qrId);
                var qrCode = await _context.QrCodes.FindAsync(qrId);
                if (qrCode == null)
                {
                    _logger.LogWarning("QR Code not found for QrId: {QrId}", qrId);
                }
                return qrCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving QR Code with QrId: {QrId}", qrId);
                throw new Exception("An error occurred while retrieving the QR Code.", ex);
            }
        }
    }
}
