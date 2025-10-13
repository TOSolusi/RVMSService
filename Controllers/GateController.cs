using Microsoft.AspNetCore.Mvc;
using RVMSService.Models;
using RVMSService.Services;

namespace RVMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class GateController : ControllerBase
    {

        private readonly ILogger<GateController> _logger;
        private readonly IGateService _gateService;

        public GateController(IGateService gateService, ILogger<GateController> logger)
        {
            _gateService = gateService;
            _logger = logger;
        }

        // GET: api/User/addGate
        [HttpPost("addGate")]
        
        public async Task<IActionResult> AddGate([FromBody] GateModel gate)
        {
            try
            {

                _logger.LogInformation("AddGate called with GateName: {GateName}", gate.GateName);
                var generatedId = await _gateService.AddGate(gate);
                _logger.LogInformation("Gate added with ID: {GateId}", generatedId);

                return Ok(new { message = "Add Gate Success", gateId = generatedId });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while adding gate");
                return StatusCode(500, new { message = "An error occurred while adding the gate." });
            }

        }

        [HttpGet("getGates")]
        public async Task<IActionResult> GetGates()
        {
            try
            {
                _logger.LogInformation("GetGates called");
                var gates = await _gateService.GetAllGates();
                _logger.LogInformation("Retrieved {Count} gates", gates.Count());
                return Ok(gates);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving gates");
                return StatusCode(500, new { message = "An error occurred while retrieving the gates." });
            }
        }

        [HttpPost("updateGate")]
        public async Task<IActionResult> UpdateGate([FromBody] GateModel gate)
        {
            try
            {
                _logger.LogInformation("UpdateGate called for GateId: {GateId}", gate.GateId);
                await _gateService.UpdateGate(gate);
                _logger.LogInformation("Gate updated successfully for GateId: {GateId}", gate.GateId);
                return Ok(new { message = "Update Gate Success" });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while updating gate with GateId: {GateId}", gate.GateId);
                return StatusCode(500, new { message = "An error occurred while updating the gate." });
            }
        }

        [HttpPost("deleteGate")]
        public async Task<IActionResult> DeleteGate([FromBody] GateModel gate)
        {
            try
            {
                _logger.LogInformation("DeleteGate called for GateId: {GateId}", gate.GateId);
                await _gateService.DeleteGate(gate);
                _logger.LogInformation("Gate deleted successfully for GateId: {GateId}", gate.GateId);
                return Ok(new { message = "Delete Gate Success" });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while deleting gate with GateId: {GateId}", gate.GateId);
                return StatusCode(500, new { message = "An error occurred while deleting the gate." });
            }
        }

        [HttpGet("getGateById/{gateId}")]
        public async Task<IActionResult> GetGateById(Guid gateId)
        {
            try
            {
                _logger.LogInformation("GetGateById called for GateId: {GateId}", gateId);
                var gate = await _gateService.GetGateById(gateId);
                if (gate == null)
                {
                    _logger.LogWarning("Gate not found for GateId: {GateId}", gateId);
                    return NotFound(new { message = "Gate not found." });
                }
                _logger.LogInformation("Gate retrieved successfully for GateId: {GateId}", gateId);
                return Ok(gate);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving gate with GateId: {GateId}", gateId);
                return StatusCode(500, new { message = ex });
            }
        }



        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
