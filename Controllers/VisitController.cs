using Microsoft.AspNetCore.Mvc;
using RVMSService.Models;
using RVMSService.Services;

namespace RVMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class VisitController : Controller
    {

        private readonly ILogger<VisitController> _logger;
        private readonly IVisitService _visit;

        public VisitController(ILogger<VisitController> logger, IVisitService visit)
        {
            _logger = logger;
            _visit = visit;
        }

        //Add Visit
        [HttpPost("addVisit")]
        public async Task<IActionResult> AddVisit(VisitModel visit)
        {
            try
            {
                _logger.LogInformation("Add visit for visitor ID : {VisitorId}", visit.VisitorId);
                var isAdded = await _visit.AddVisitAsync(visit);
                if (isAdded)
                {
                    _logger.LogInformation("Visit added with ID: {visitId}", visit.VisitId);
                    return Ok(new { message = "Add visit Success", visitId = visit.VisitId });
                }
                else
                {
                    _logger.LogWarning("Failed to add visit for visitor ID: {VisitorId}", visit.VisitorId);
                    return StatusCode(500, new { message = "Failed to add visit." });
                }
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



        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
