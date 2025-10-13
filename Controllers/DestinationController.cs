using Microsoft.AspNetCore.Mvc;
using RVMSService.Models;
using RVMSService.Services;

namespace RVMSService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DestinationController : ControllerBase
    {

        private readonly ILogger<DestinationController> _logger;
        private readonly IDestinationService _destination;

        public DestinationController(IDestinationService destination, ILogger<DestinationController> logger)
        {
            _destination = destination;
            _logger = logger;
        }


        // GET: api/User/addGate
        [HttpPost("addDestination")]

        public async Task<IActionResult> AddDestination([FromBody] DestinationModel destination)
        {
            try
            {

                _logger.LogInformation("Add destination with address : {Address}", destination.Address);
                var generatedId = await _destination.AddDestination(destination);
                _logger.LogInformation("Destination added with ID: {destinationId}", generatedId);

                return Ok(new { message = "Add destination Success", destinationId = generatedId });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while adding destination");
                return StatusCode(500, new { message = "An error occurred while adding the destination." });
            }

        }


        //get all destination list

        [HttpGet("getDestinations")]
        public async Task<List<DestinationModel>> GetDestinations()
        {
            try
            {
                _logger.LogInformation("GetDestinations called");
                var destinations = await _destination.GetAllDestinations();
                _logger.LogInformation("Retrieved {Count} destinations", destinations.Count());
                return destinations;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving destinations");
                return new List<DestinationModel>();
            }
        }


        [HttpDelete("deleteDestination/{destinationId}")]
        public async Task<IActionResult> DeleteDestination(Guid destinationId)
        {
            try
            {
                _logger.LogInformation("DeleteDestination called with ID: {DestinationId}", destinationId);
                var result = await _destination.DeleteDestination(destinationId);
                if (result)
                {
                    _logger.LogInformation("Destination with ID: {DestinationId} deleted successfully", destinationId);
                    return Ok(new { message = "Delete Destination Success" });
                }
                else
                {
                    _logger.LogWarning("Destination with ID: {DestinationId} not found", destinationId);
                    return NotFound(new { message = "Destination not found" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while deleting destination");
                return StatusCode(500, new { message = "An error occurred while deleting the destination." });
            }
        }

        [HttpGet("GetGestinationbyId")]
        public async Task<List<DestinationModel?>> GetDestinationsById(Guid gateId)
        {
            try
            {
                _logger.LogInformation("GetDestinationsById called with Gate ID: {GateId}", gateId);
                var destinations = await _destination.GetDestinationsByGateId(gateId);
                _logger.LogInformation("Retrieved {Count} destinations for Gate ID: {GateId}", destinations.Count(), gateId);
                return destinations;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving destinations by Gate ID");
                return new List<DestinationModel?>();
            }
        }

        [HttpPost("UpdateDestination")]
        public async Task<bool> UpdateDestination(DestinationModel destination)
        {
            try
            {
               _logger.LogInformation("UpdateDestination called for ID: {DestinationId}", destination.DestinationId);
                await _destination.UpdateDestination(destination);
                _logger.LogInformation("Destination with ID: {DestinationId} updated successfully", destination.DestinationId);
                return true;

            }
            catch
            {
                _logger.LogError("Error occurred while updating destination with ID: {DestinationId}", destination.DestinationId);
                return false;
            }

        }
    }
}
