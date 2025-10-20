using Microsoft.AspNetCore.Mvc;
using RVMSService.Models;
using RVMSService.Services;

namespace RVMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorController : Controller
    {

      

        private readonly ILogger<VisitorController> _logger;
        private readonly IVisitorService _visitor;

        public VisitorController(ILogger<VisitorController> logger, IVisitorService visitor)
        {
            _logger = logger;
            _visitor = visitor;
        }

        //Add Visitor   
        [HttpPost("addVisitor")]

        public async Task<IActionResult> AddVisitor(VisitorModel visitor)
        {
            try
            {
                _logger.LogInformation("Add visitor with name : {Name}", visitor.VisitorName);
                var isAdded = await _visitor.AddVisitorAsync(visitor);
                if (isAdded)
                {
                    _logger.LogInformation("Visitor added with ID: {visitorId}", visitor.VisitorId);
                    return Ok(new { message = "Add visitor Success", visitorId = visitor.VisitorId });
                }
                else
                {
                    _logger.LogWarning("Failed to add visitor with name: {Name}", visitor.VisitorName);
                    return StatusCode(500, new { message = "Failed to add visitor." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while adding visitor");
                return StatusCode(500, new { message = "An error occurred while adding the visitor." });
            }
        }

        //Delete Visitor
        [HttpDelete("deleteVisitor/{visitorId}")]
        public async Task<IActionResult> DeleteVisitor(Guid visitorId)
        {
            try
            {
                _logger.LogInformation("DeleteVisitor called for ID: {visitorId}", visitorId);
                var isDeleted = await _visitor.DeleteVisitorAsync(visitorId);
                if (isDeleted)
                {
                    _logger.LogInformation("Visitor deleted with ID: {visitorId}", visitorId);
                    return Ok(new { message = "Delete visitor Success" });
                }
                else
                {
                    _logger.LogWarning("Failed to delete visitor with ID: {visitorId}", visitorId);
                    return StatusCode(500, new { message = "Failed to delete visitor." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting visitor with ID: {visitorId}", visitorId);
                return StatusCode(500, new { message = "An error occurred while deleting the visitor." });
            }
        }

        //Update Visitor
        [HttpPost("updateVisitor")]
        public async Task<IActionResult> UpdateVisitor(VisitorModel visitor)
        {
            try
            {
                _logger.LogInformation("UpdateVisitor called for ID: {visitorId}", visitor.VisitorId);
                var isUpdated = await _visitor.UpdateVisitor(visitor);
                if (isUpdated)
                {
                    _logger.LogInformation("Visitor updated with ID: {visitorId}", visitor.VisitorId);
                    return Ok(new { message = "Update visitor Success" });
                }
                else
                {
                    _logger.LogWarning("Failed to update visitor with ID: {visitorId}", visitor.VisitorId);
                    return StatusCode(500, new { message = "Failed to update visitor." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating visitor with ID: {visitorId}", visitor.VisitorId);
                return StatusCode(500, new { message = "An error occurred while updating the visitor." });
            }
        }

        //Get visitor info only
        [HttpGet("getVisitorsInfoOnly")]    
        public async Task<List<VisitorModel>> GetVisitorsInfoOnly()
        {
            try
            {
                _logger.LogInformation("GetVisitorsInfoOnly called");
                var visitors = await _visitor.GetAllVisitorsInfoOnlyAsync();
                _logger.LogInformation("Retrieved {Count} visitors", visitors.Count());
                return visitors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving visitors info only");
                return new List<VisitorModel>();
            }
        }

        //Get Visitor photo by Id
        [HttpGet("getVisitorPictureById/{visitorId}")]
        public async Task<VisitorModel?> GetVisitorPictureById(Guid visitorId)
        {
            try
            {
                _logger.LogInformation("GetVisitorPictureById called for ID: {visitorId}", visitorId);
                var visitor = await _visitor.GetVisitorPictureByIdAsync(visitorId);
                if (visitor != null)
                {
                    _logger.LogInformation("Retrieved picture for visitor ID: {visitorId}", visitorId);
                }
                else
                {
                    _logger.LogWarning("No picture found for visitor ID: {visitorId}", visitorId);
                }
                return visitor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving visitor picture for ID: {visitorId}", visitorId);
                return null;
            }
        }

        //get blacklisted visitor  
        [HttpGet("getBlackListedVisitor")]
        public async Task<List<VisitorModel>?> getBlacklisted()
        {
            try
            {
                _logger.LogInformation("Get Blacklisted Visitors.");
                var blacklistVisitors = await _visitor.GetAllBlacklistedVisitorsAsync();
                return blacklistVisitors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting blacklist");
                return null;
            }
        }


        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
