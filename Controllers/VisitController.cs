using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVMSService.Models;
using RVMSService.Services;
using System.Runtime.CompilerServices;

namespace RVMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class VisitController : Controller
    {

        private readonly ILogger<VisitController> _logger;
        private readonly IVisitService _visit;
        private readonly IVisitorService _visitor;
        private readonly IQRCodeService _qrCodeService;
        private readonly IDestinationService _destinationService;
        private readonly IVisitTypeService _visitTypeService;
        private readonly IAuditTrailService _auditTrail;
        private readonly ITelegramService _telegramService;

        public VisitController(ILogger<VisitController> logger, IVisitService visit,
            IVisitorService visitorService, IQRCodeService qRCodeService,
            IDestinationService destinationService, IVisitTypeService visitTypeService,
            IAuditTrailService auditTrail, ITelegramService telegramService)
        {
            _logger = logger;
            _visit = visit;
            _visitor = visitorService;
            _qrCodeService = qRCodeService;
            _destinationService = destinationService;
            _visitTypeService = visitTypeService;
            _auditTrail = auditTrail;
            _telegramService = telegramService;
        }

        //Add Visit
        
        [HttpPost("addVisit")]
        public async Task<IActionResult> AddVisit(DOTVisitModel dotVisit)
        {
            try
            {
                var visit = dotVisit.visit;
                var visitor = dotVisit.visitor;
                var qrCode = dotVisit.qrCode;
                var auditTrail = dotVisit.auditTrail;

                DOTVisitorModel dotVisitor = new DOTVisitorModel
                {
                    Visitor = visitor,
                    AuditTrail = auditTrail
                };

                DOTQRModel dotQR = new DOTQRModel
                {
                    QrCode  = qrCode,
                    AuditTrail = auditTrail
                };


                _logger.LogInformation($"Add Visitor {visitor.VisitorId}, name : {visitor.VisitorName}");
                var visitorAdded = await _visitor.AddVisitorAsync(dotVisitor);
                if (!visitorAdded)
                {
                    _logger.LogWarning("Failed to add visitor with ID: {VisitorId}", visitor.VisitorId);
                    return StatusCode(500, new { message = "Failed to add visitor." });
                }
               
                _logger.LogInformation("Add visit for visitor ID : {VisitorId}", visit.VisitorId);
                var isAdded = await _visit.AddVisitAsync(dotVisit);
                if (!isAdded)
                {
                    _logger.LogWarning("Failed to add visit for visitor ID: {VisitorId}", visit.VisitorId);
                    return StatusCode(500, new { message = "Failed to add visit." });
                    
                }
              
                _logger.LogInformation("Visit added with ID: {visitId}", visit.VisitId);
                //return Ok(new { message = "Add visit Success", visitId = visit.VisitId });
                _logger.LogInformation("Set QR Code for visit ID : {VisitId}", visit.VisitId);
                dotQR.QrCode.Used = true;
                dotQR.QrCode.VisitId = visit.VisitId;
                dotQR.QrCode.LastUsed = DateTime.Now;

                var qrCodeSet = await _qrCodeService.UpdateQrCode(dotQR);
                if (!qrCodeSet)
                {
                    _logger.LogWarning("Failed to set QR code for visit ID: {VisitId}", visit.VisitId);
                    return StatusCode(500, new { message = "Failed to set QR code." });
                }

                // Send Telegram notification to destination owner
                if (visit.DestinationId.HasValue)
                {
                    try
                    {
                        var destination = await _destinationService.GetDestinationById(visit.DestinationId.Value);
                        if (destination != null && !string.IsNullOrWhiteSpace(destination.Owner_TelegramChatId))
                        {
                            var photos = new List<byte[]>();
                            byte[][] cameraImages = {
                                visitor.VisitorImage, visit.Camera1Image, visit.Camera2Image, visit.Camera3Image,
                                visit.Camera4Image, visit.Camera5Image, visit.Camera6Image,
                                visit.Camera7Image, visit.Camera8Image, visit.Camera9Image,
                                visit.Camera10Image
                            };
                            foreach (var img in cameraImages)
                            {
                                if (img != null && img.Length > 0)
                                    photos.Add(img);
                            }

                            var visitorName = visitor?.VisitorName ?? "Unknown";
                            _ = Task.Run(() => _telegramService.SendVisitNotificationAsync(
                                destination.Owner_TelegramChatId,
                                visitorName,
                                destination.Address,
                                photos));
                        }
                    }
                    catch (Exception texEx)
                    {
                        _logger.LogWarning(texEx, "Failed to send Telegram notification for visit {VisitId}", visit.VisitId);
                    }
                }

                return Ok(new { message = "Add visit Success", visitId = visit.VisitId });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while adding visit");
                return StatusCode(500, new { message = "An error occurred while adding the visit." });
            }
        }

        //Update Visit
        [HttpPost("updateVisit")]
        public async Task<IActionResult> UpdateVisit(VisitModel visit)
        {
            try
            {
                _logger.LogInformation("Update visit with ID : {VisitId}", visit.VisitId);
                var isUpdated = await _visit.UpdateVisitAsync(visit);
                if (isUpdated)
                {
                    _logger.LogInformation("Visit updated with ID: {visitId}", visit.VisitId);
                    return Ok(new { message = "Update visit Success", visitId = visit.VisitId });
                }
                else
                {
                    _logger.LogWarning("Failed to update visit with ID: {VisitId}", visit.VisitId);
                    return StatusCode(500, new { message = "Failed to update visit." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while updating visit");
                return StatusCode(500, new { message = "An error occurred while updating the visit." });
            }
        }

        //get visit by id
        [HttpGet("getVisitById/{visitId}")]
        public async Task<IActionResult> GetVisitById(Guid visitId)
        {
            try
            {
                _logger.LogInformation("Get visit by ID : {VisitId}", visitId);
                var visit = await _visit.GetVisitByIdAsync(visitId);
                if (visit != null)
                {
                    _logger.LogInformation("Visit retrieved with ID: {visitId}", visitId);
                    return Ok(visit);
                }
                else
                {
                    _logger.LogWarning("Visit not found with ID: {VisitId}", visitId);
                    return NotFound(new { message = "Visit not found." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving visit");
                return StatusCode(500, new { message = "An error occurred while retrieving the visit." });
            }
        }

        //Sign Out Visit
        [HttpPost("signOutVisit/{visitId}")]
        public async Task<IActionResult> SignOutVisit(Guid visitId)
        {
            try
            {
                _logger.LogInformation("Sign out visit with ID : {VisitId}", visitId);
                var isSignedOut = await _visit.updateVisitSignOut(visitId);
                if (isSignedOut)
                {
                    _logger.LogInformation("Visit signed out with ID: {visitId}", visitId);
                    return Ok(new { message = "Sign out visit Success", visitId = visitId });
                }
                else
                {
                    _logger.LogWarning("Failed to sign out visit with ID: {VisitId}", visitId);
                    return StatusCode(500, new { message = "Failed to sign out visit." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while signing out visit");
                return StatusCode(500, new { message = "An error occurred while signing out the visit." });
            }
        }

        //Get last signed-in visits without photos today 
        [HttpGet("getLastSignedInVisitsWithoutPhotosToday/{count}")]
        public async Task<IActionResult> GetLastSignedInVisitsWithoutPhotosToday(int count)
        {
            try
            {
                _logger.LogInformation("Get last signed-in visits without photos today, count: {Count}", count);
                var visits = await _visit.GetLastSignedInVisitsWithoutPhotosToday(count);
                _logger.LogInformation("Retrieved {VisitCount} visits", visits.Count);
                return Ok(visits);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving visits");
                return StatusCode(500, new { message = "An error occurred while retrieving the visits." });
            }
        }

        //Get visit photos by id
        [HttpGet("getVisitPhotos/{visitId}")]
        public async Task<IActionResult> GetVisitPhotos(Guid visitId)
        {
            try
            {
                _logger.LogInformation("Get visit photos by ID : {VisitId}", visitId);
                var visit = await _visit.GetVisitPhotos(visitId);
                if (visit != null)
                {
                    _logger.LogInformation("Visit photos retrieved with ID: {visitId}", visitId);
                    return Ok(visit);
                }
                else
                {
                    _logger.LogWarning("Visit not found with ID: {VisitId}", visitId);
                    return NotFound(new { message = "Visit not found." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving visit photos");
                return StatusCode(500, new { message = "An error occurred while retrieving the visit photos." });
            }
        }

        [HttpGet("GetVisitorPhotos/{visitorId}")]
        public async Task<IActionResult> GetVisitorPhotos(Guid visitorId)
        {
            try
            {
                _logger.LogInformation("Get visitor photos by ID : {VisitorId}", visitorId);
                var visitor = await _visit.GetVisitorPhotos(visitorId);
                if (visitor != null)
                {
                    _logger.LogInformation("Visitor photos retrieved with ID: {visitorId}", visitorId);
                    return Ok(visitor);
                }
                else
                {
                    _logger.LogWarning("Visitor not found with ID: {VisitorId}", visitorId);
                    return NotFound(new { message = "Visitor not found." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving visitor photos");
                return StatusCode(500, new { message = "An error occurred while retrieving the visitor photos." });
            }
        }

        //get visits without photos by date range
        [HttpGet("getVisitswithoutPhotosByDateRange")]
        public async Task<IActionResult> GetVisitswithoutPhotosByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Get visits without photos by date range : {StartDate} - {EndDate}", startDate, endDate);
                var visits = await _visit.GetVisitswithoutPhotosByDateRange(startDate, endDate);
                _logger.LogInformation("Retrieved {VisitCount} visits", visits.Count);
                return Ok(visits);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving visits by date range");
                return StatusCode(500, new { message = "An error occurred while retrieving the visits." });
            }
        }

        //get visits without photos by date range
        [HttpGet("getVisitswithoutPhotosByDateRangeByGateId")]
        public async Task<IActionResult> GetVisitswithoutPhotosByDateRangeByGateId([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid gateId)
        {
            try
            {
                _logger.LogInformation("Get visits without photos by date range : {StartDate} - {EndDate} for Gate ID: {GateId}", startDate, endDate, gateId);
                var visits = await _visit.GetVisitswithoutPhotosByDateRangeByGate(startDate, endDate, gateId);
                _logger.LogInformation("Retrieved {VisitCount} visits", visits.Count);
                return Ok(visits);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving visits by date range");
                return StatusCode(500, new { message = "An error occurred while retrieving the visits." });
            }
        }

        //get visits which return DOTVisitModel
        [Authorize(Roles = "Admin, Operator, Security")]
        [HttpGet("getDOTVisitByDateRangeByGateId")]
        public async Task<List<DOTVisitReturnModel>> GetDOTVisitByDateRangeByGateId([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid gateId)
        {
            
            _logger.LogInformation("Get DOT visits by date range : {StartDate} - {EndDate} for Gate ID: {GateId}", startDate, endDate, gateId);
            var visits = await _visit.GetVisitswithoutPhotosByDateRangeByGate(startDate, endDate, gateId);
            List<DOTVisitReturnModel> dotVisits = new List<DOTVisitReturnModel>();
            
            foreach (var visit in visits)
            {
                var dotVisit = new DOTVisitReturnModel();
                //visit.AdditionalPhoto = null;
                //visit.CurrentPhoto = null;
                //visit.VehiclePhoto = null;
                
                dotVisit.visit = visit;

                VisitorModel visitor = await _visitor.GetVisitorByIdAsync((Guid)visit.VisitorId);
                visitor.VisitorImage = null;

                dotVisit.visitor = visitor;

                QrCodeModel qrCode = await _qrCodeService.GetQRCodeById((Guid)visit.QrId);
                dotVisit.qrCode = qrCode;

                DestinationModel destination = await _destinationService.GetDestinationById((Guid)visit.DestinationId);
                dotVisit.destination = destination;

                VisitTypeModel visitType = await _visitTypeService.GetVisitTypebyID((Guid)visit.TypeId);
                dotVisit.visitType = visitType;

                dotVisits.Add(dotVisit);


            }
            _logger.LogInformation("Retrieved {VisitCount} DOT visits", dotVisits.Count);
            return dotVisits;
        }

        [Authorize(Roles = "Admin, Operator, Security")]
        [HttpPost("SignOutVisit")]
        public async Task<bool> SignOutVisit(DOTVisitModel dotVisit)
        {
            try
            {
                _logger.LogInformation("Sign out visit with ID : {VisitId}", dotVisit.visit.VisitId);

                //dotVisit.visit.CheckOut = DateTime.Now;

                VisitModel visitResult = await _visit.GetVisitByIdAsync((Guid)dotVisit.visit.VisitId);
                visitResult.CheckOut = DateTime.Now;
                

                var isSignedOut = await _visit.UpdateVisitAsync(visitResult);
                if (isSignedOut)
                {
                    _logger.LogInformation("Visit signed out with ID: {visitId}", dotVisit.visit.VisitId);

                    //return true;
                }
                else
                {
                    _logger.LogWarning("Failed to sign out visit with ID: {VisitId}", dotVisit.visit.VisitId);
                    throw new Exception("Failed to sign out visit.");
                }

                var qrCode = await _qrCodeService.GetQRCodeById((Guid)dotVisit.visit.QrId);
                qrCode.Used = false;
                qrCode.LastUsed = DateTime.Now;
                //qrCode.VisitId = null;
                var isQRsignout = await _qrCodeService.UpdateQrCode(new DOTQRModel
                {
                    QrCode = qrCode,
                    AuditTrail = dotVisit.auditTrail
                });
                if (isQRsignout)
                {
                    _logger.LogInformation("QR Code released for visit ID: {visitId}", (Guid)dotVisit.visit.VisitId);
                    //return true;
                }
                else
                {
                    _logger.LogWarning("Failed to release QR code for visit ID: {VisitId}", dotVisit.visit.VisitId);
                    throw new Exception("Failed to release QR code.");
                }

                var audit = dotVisit.auditTrail;
                audit.Description = $"Sign out Visit {dotVisit.visit.VisitId}";
                audit.Timestamp = DateTime.Now;
                audit.Status = "Success";
                audit.Category = "Visit";

                await _auditTrail.RecordAsync(audit);
                return true;



            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while signing out visit");

                throw new Exception("An error occurred while signing out the visit.", ex);
            }
        }

        //get visits which return DOTVisitModel for admin
        [Authorize(Roles = "Admin")]
        [HttpGet("getDOTVisitsByDateRange")]
        public async Task<List<DOTVisitReturnModel>> GetDOTVisitByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {

            _logger.LogInformation("Get DOT visits by date range : {StartDate} - {EndDate}", startDate, endDate);
            var visits = await _visit.GetVisitswithoutPhotosByDateRange(startDate, endDate);
            List<DOTVisitReturnModel> dotVisits = new List<DOTVisitReturnModel>();

            foreach (var visit in visits)
            {
                var dotVisit = new DOTVisitReturnModel();
                //visit.AdditionalPhoto = null;
                //visit.CurrentPhoto = null;
                //visit.VehiclePhoto = null;

                //if (visit.CapturedPhotos != null && visit.CapturedPhotos.Any())
                //{
                //    visit.CapturedPhotosInfo = visit.CapturedPhotos.Select(p => new CapturedPhotosInfoModel
                //    {
                //        CameraName = p.CameraName,
                //        CapturedAt = p.CapturedAt,
                //        CameraType = p.CameraType,
                //        ImageSize = p.ImageData?.Length ?? 0
                //    }).ToList();

                //    visit.CapturedPhotos = null; // Clear to prevent serialization
                //}

                dotVisit.visit = visit;

                VisitorModel visitor = await _visitor.GetVisitorByIdAsync((Guid)visit.VisitorId);
                visitor.VisitorImage = null;

                dotVisit.visitor = visitor;

                QrCodeModel qrCode = await _qrCodeService.GetQRCodeById((Guid)visit.QrId);
                dotVisit.qrCode = qrCode;

                DestinationModel destination = await _destinationService.GetDestinationById((Guid)visit.DestinationId);
                dotVisit.destination = destination;

                VisitTypeModel visitType = await _visitTypeService.GetVisitTypebyID((Guid)visit.TypeId);
                dotVisit.visitType = visitType;



                dotVisits.Add(dotVisit);


            }
            _logger.LogInformation("Retrieved {VisitCount} DOT visits", dotVisits.Count);
            return dotVisits;
        }

       

        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
