using Microsoft.AspNetCore.Mvc;
using RVMSService.Models;
using RVMSService.Services;

namespace RVMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : Controller
    {
        private readonly ILogger<QRCodeController> _logger;
        private readonly IQRCodeService _qRCodeService;


        public QRCodeController(ILogger<QRCodeController> logger, IQRCodeService qRCodeService)
        {
            _logger = logger;
            _qRCodeService = qRCodeService;
        }

        //Add QR Code
        [HttpPost("addQRCode")]
        public async Task<IActionResult> AddQRCode([FromBody] QrCodeModel qRCode)
        {
            try
            {
                _logger.LogInformation("Add QR Code with code : {Code}", qRCode.QrString);
                var generatedId = await _qRCodeService.AddQRCode(qRCode);
                _logger.LogInformation("QR Code added with ID: {qRCodeId}", generatedId);
                return Ok(new { message = "Add QR Code Success", qRCodeId = generatedId });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while adding QR Code");
                return StatusCode(500, new { message = "An error occurred while adding the QR Code." });
            }
        }

        //get all QR Code list
        [HttpGet("getQRCodes")]
        public async Task<List<QrCodeModel>> GetQRCodes()
        {
            try
            {
                _logger.LogInformation("GetQRCodes called");
                var qRCodes = await _qRCodeService.GetAllQRCodes();
                _logger.LogInformation("Retrieved {Count} QR Codes", qRCodes.Count());
                return qRCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving QR Codes");
                return new List<QrCodeModel>();
            }
        }

        [HttpDelete("deleteQRCode/{qrCodeId}")]
        public async Task<IActionResult> DeleteQRCode(Guid qrCodeId)
        {
            try
            {
                _logger.LogInformation("DeleteQRCode called for ID: {qrCodeId}", qrCodeId);
                var result = await _qRCodeService.deleteQrCode(qrCodeId);
                if (result)
                {
                    _logger.LogInformation("QR Code with ID: {qrCodeId} deleted successfully", qrCodeId);
                    return Ok(new { message = "Delete QR Code Success" });
                }
                else
                {
                    _logger.LogWarning("QR Code with ID: {qrCodeId} not found", qrCodeId);
                    return NotFound(new { message = "QR Code not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting QR Code with ID: {qrCodeId}", qrCodeId);
                return StatusCode(500, new { message = "An error occurred while deleting the QR Code." });
            }
        }

        //get active QR codes by gate ID
        [HttpGet("getActiveQRCodes/{gateId}")]
        public async Task<List<QrCodeModel>> GetActiveQRCodes(Guid gateId)
        {
            try
            {
                _logger.LogInformation("GetActiveQRCodes called for Gate ID: {gateId}", gateId);
                var qRCodes = await _qRCodeService.GetActiveQRCodes(gateId);
                _logger.LogInformation("Retrieved {Count} active QR Codes for Gate ID: {gateId}", qRCodes.Count(), gateId);
                return qRCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving active QR Codes for Gate ID: {gateId}", gateId);
                return new List<QrCodeModel>();
            }
        }

        //update QR code
        [HttpPost("updateQRCode")]
        public async Task<IActionResult> UpdateQRCode([FromBody] QrCodeModel qRCode)
        {
            try
            {
                _logger.LogInformation("UpdateQRCode called for ID: {qrCodeId}", qRCode.QrId);
                await _qRCodeService.UpdateQrCode(qRCode);
                _logger.LogInformation("QR Code with ID: {qrCodeId} updated successfully", qRCode.QrId);
                return Ok(new { message = "Update QR Code Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating QR Code with ID: {qrCodeId}", qRCode.QrId);
                return StatusCode(500, new { message = "An error occurred while updating the QR Code." });
            }
        }


        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
