using Microsoft.AspNetCore.Authorization;
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
        private readonly ITelegramService _telegramService;
        private readonly IEmailService _emailService;

        public DestinationController(IDestinationService destination, ILogger<DestinationController> logger,
            ITelegramService telegramService, IEmailService emailService)
        {
            _destination = destination;
            _logger = logger;
            _telegramService = telegramService;
            _emailService = emailService;
        }


        // GET: api/User/addGate
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [HttpGet("getDestinationsByGateID/{gateId}")]
        public async Task<List<DestinationModel>> GetDestinationByGateID(Guid gateId)
        {
            try
            {
                _logger.LogInformation("GetDestinationByGateID called with Gate ID: {GateId}", gateId);
                var destinations = await _destination.GetDestinationsByGateId(gateId);
                _logger.LogInformation("Retrieved {Count} destinations for Gate ID: {GateId}", destinations.Count(), gateId);
                return destinations;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving destinations by Gate ID");
                return new List<DestinationModel>();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("UpdateDotDestination")]
        public async Task<bool> UpdateDotDestination(DotDestinationModel dotDestination)
        {
            try
            {
                _logger.LogInformation("UpdateDotDestination called for ID: {DestinationId}", dotDestination.Destination.DestinationId);
                await _destination.UpdateDOTDestination(dotDestination);
                _logger.LogInformation("Destination with ID: {DestinationId} updated successfully", dotDestination.Destination.DestinationId);
                return true;

            }
            catch
            {
                return false;
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AddDotDestination")]
        public async Task<bool> AddDotDestination(DotDestinationModel dotDestination)
        {
            try
            {
                _logger.LogInformation("AddDotDestination called for ID: {DestinationId}", dotDestination.Destination.DestinationId);
                await _destination.AddDOTDestination(dotDestination);
                _logger.LogInformation("Destination with ID: {DestinationId} added successfully", dotDestination.Destination.DestinationId);
                return true;

            }
            catch
            {
                return false;
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getTelegramLink/{destinationId}")]
        public async Task<IActionResult> GetTelegramLink(Guid destinationId)
        {
            try
            {
                var destination = await _destination.GetDestinationById(destinationId);
                if (destination == null)
                    return NotFound(new { message = "Destination not found." });

                var link = await _telegramService.GetBotLinkAsync(destinationId);

                bool emailSent = false;
                if (!string.IsNullOrWhiteSpace(destination.Owner_Email))
                {
                    try
                    {
                        var subject = "RVMS - Link Your Telegram for Visitor Notifications";
                        var htmlBody = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif; color: #333;'>
                                <h2>Hello {destination.Owner_Name},</h2>
                                <p>You have been set up to receive <strong>visitor notifications</strong> for:</p>
                                <p style='font-size: 16px;'>📍 <strong>{destination.Address}</strong></p>
                                <p>To start receiving notifications on Telegram, click the button below:</p>
                                <p style='margin: 24px 0;'>
                                    <a href='{link}'
                                       style='background-color: #0088cc; color: white; padding: 12px 24px;
                                              text-decoration: none; border-radius: 6px; font-size: 16px;'>
                                        🔗 Connect Telegram
                                    </a>
                                </p>
                                <p style='color: #888; font-size: 13px;'>
                                    Or copy this link into your browser:<br/>
                                    <a href='{link}'>{link}</a>
                                </p>
                                <hr style='border: none; border-top: 1px solid #eee; margin: 24px 0;'/>
                                <p style='color: #999; font-size: 12px;'>RVMS Visitor Management System</p>
                            </body>
                            </html>";

                        await _emailService.SendEmailAsync(destination.Owner_Email, subject, htmlBody);
                        emailSent = true;
                        _logger.LogInformation("Telegram link email sent to {Email} for destination {DestinationId}",
                            destination.Owner_Email, destinationId);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Failed to send Telegram link email to {Email}", destination.Owner_Email);
                    }
                }

                return Ok(new
                {
                    telegramLink = link,
                    emailSent,
                    ownerEmail = destination.Owner_Email,
                    message = emailSent
                        ? $"Telegram link emailed to {destination.Owner_Email}"
                        : "Telegram link generated (no owner email configured)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Telegram link for destination {DestinationId}", destinationId);
                return StatusCode(500, new { message = "Failed to generate Telegram link." });
            }
        }
    }
}
