using Microsoft.AspNetCore.Mvc;
using RVMSService.Models;
using RVMSService.Services;

namespace RVMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitTypeController : Controller
    {

        private readonly ILogger<VisitTypeController> _logger;
        private readonly IVisitTypeService _visitType;
        public VisitTypeController(ILogger<VisitTypeController> logger, IVisitTypeService visitType)
        {
                _logger = logger;
                _visitType = visitType;
        }

        //add visit type
        [HttpPost("addVisitType")]
        public async Task<IActionResult> AddVisitType([FromBody] Models.VisitTypeModel visitType)
        {
            try
            {
                _logger.LogInformation("Add visit type with name : {Name}", visitType.TypeVisit);
                var generatedId = await _visitType.AddVisitType(visitType);
                _logger.LogInformation("Visit type added with ID: {visitTypeId}", generatedId);
                return Ok(new { message = "Add visit type Success", visitTypeId = generatedId });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while adding visit type");
                return StatusCode(500, new { message = "An error occurred while adding the visit type." });
            }
        }

        //get all visit type list
        [HttpGet("getVisitTypes")]
        public async Task<List<VisitTypeModel>> GetVisitTypes()
        {
            try
            {
                _logger.LogInformation("GetVisitTypes called");
                var visitTypes = await _visitType.GetAllVisitTypes();
                _logger.LogInformation("Retrieved {Count} visit types", visitTypes.Count());
                return visitTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving visit types");
                return new List<Models.VisitTypeModel>();
            }
        }

        [HttpDelete("deleteVisitType/{typeId}")]
        public async Task<IActionResult> DeleteVisitType(Guid typeId)
        {
            try
            {
                _logger.LogInformation("DeleteVisitType called for ID: {TypeId}", typeId);
                var result = await _visitType.DeleteVisitType(typeId);
                if (result)
                {
                    _logger.LogInformation("Visit type with ID: {TypeId} deleted successfully", typeId);
                    return Ok(new { message = "Delete visit type Success" });
                }
                else
                {
                    _logger.LogWarning("Visit type with ID: {TypeId} not found", typeId);
                    return NotFound(new { message = "Visit type not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting visit type with ID: {TypeId}", typeId);
                return StatusCode(500, new { message = "An error occurred while deleting the visit type." });
            }
        }

        [HttpPost]
        [Route("updateVisitType")]
        public async Task<IActionResult> UpdateVisitType([FromBody] Models.VisitTypeModel visitType)
        {
            try
            {
                _logger.LogInformation("UpdateVisitType called for ID: {TypeId}", visitType.TypeId);
                await _visitType.UpdateVisitType(visitType);
                _logger.LogInformation("Visit type with ID: {TypeId} updated successfully", visitType.TypeId);
                return Ok(new { message = "Update visit type Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating visit type with ID: {TypeId}", visitType.TypeId);
                return StatusCode(500, new { message = "An error occurred while updating the visit type." });
            }
        }

        [HttpGet]
        [Route("getActiveVisitTypes")]
        public async Task<List<VisitTypeModel>> GetActiveVisitTypes()
        {
            try
            {
                _logger.LogInformation("GetActiveVisitTypes called");
                var visitTypes = await _visitType.GetActiveVisitTypes();
                _logger.LogInformation("Retrieved {Count} active visit types", visitTypes.Count());
                return visitTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving active visit types");
                return new List<Models.VisitTypeModel>();
            }
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
