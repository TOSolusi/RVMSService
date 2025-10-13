using Microsoft.EntityFrameworkCore;
using RVMSService.Data;
using RVMSService.Models;

namespace RVMSService.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DestinationService> _logger;
        private readonly IAuditTrailService _auditTrail;
        public DestinationService(AppDBContext context, IAuditTrailService auditTrail, ILogger<DestinationService> logger)
        {
            _context = context;
            _auditTrail = auditTrail;
            _logger = logger;

        }

        public async Task<Guid?> AddDestination(DestinationModel destination)
        {
            try
            {
                _logger.LogInformation("Adding destination to database");

                // Implementation for adding destination
                await _context.Destinations.AddAsync(destination);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Destination added with ID: {DestinationId} and address {Address}", destination.DestinationId, destination.Address);

                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Add Destination {destination.Address} with gate {destination.GateId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Destination"
                };
                await _auditTrail.RecordAsync(audit);

                // After SaveChangesAsync, destination.DestinationId will contain the generated GUID
                return destination.DestinationId;

            }


            catch (Exception ex)
            {
                // Log the exception (ex) as needed

                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Add Destination {destination.Address} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Destination"
                };
                await _auditTrail.RecordAsync(audit);

                _logger.LogError(ex, "Error occurred while adding destination to database");
                throw new Exception("An error occurred while adding the destination.", ex);
            }
        }


        public async Task<List<DestinationModel>> GetAllDestinations()
        {
            try
            {
                _logger.LogInformation("Retrieving all destinations from database");
                var destinations = await _context.Destinations.ToListAsync();
                _logger.LogInformation("Retrieved {Count} destinations", destinations.Count);
                return destinations;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving destinations from database");
                throw new Exception("An error occurred while retrieving the destinations.", ex);
            }

        }

        public async Task<DestinationModel?> GetDestinationById(Guid destinationId)
        {
            try
            {
                _logger.LogInformation("Retrieving destination with ID: {DestinationId}", destinationId);
                var destination = await _context.Destinations.FindAsync(destinationId);
                if (destination == null)
                {
                    _logger.LogWarning("Destination with ID: {DestinationId} not found", destinationId);
                }
                else
                {
                    _logger.LogInformation("Destination with ID: {DestinationId} retrieved successfully", destinationId);
                }
                return destination;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving destination with ID: {DestinationId}", destinationId);
                throw new Exception("An error occurred while retrieving the destination.", ex);
            }
        }

        public async Task UpdateDestination(DestinationModel destination)
        {
            try
            {
                _logger.LogInformation("Updating destination with ID: {DestinationId}", destination.DestinationId);
                var existingDestination = await _context.Destinations.FindAsync(destination.DestinationId);
                if (existingDestination == null)
                {
                    _logger.LogWarning("Destination with ID: {DestinationId} not found", destination.DestinationId);
                    throw new Exception("Destination not found");
                }
                // Update fields
                existingDestination.GateId = destination.GateId;
                existingDestination.Address = destination.Address;
                existingDestination.Owner_Name = destination.Owner_Name;
                existingDestination.Owner_Email = destination.Owner_Email;
                existingDestination.Owner_Phone = destination.Owner_Phone;
                existingDestination.Notes = destination.Notes;
                existingDestination.Updated_At = DateTime.UtcNow; // Update timestamp
                existingDestination.Status = destination.Status;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Destination with ID: {DestinationId} updated successfully", destination.DestinationId);
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Update Destination {destination.Address} with gate {destination.GateId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Destination"
                };
                await _auditTrail.RecordAsync(audit);

            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Update Destination {destination.Address} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Destination"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while updating destination with ID: {DestinationId}", destination.DestinationId);
                throw new Exception("An error occurred while updating the destination.", ex);
            }
        }

        public async Task<bool> DeleteDestination(Guid destinationId)
        {
            try
            {
                _logger.LogInformation("Deleting destination with ID: {DestinationId}", destinationId);
                var existingDestination = await _context.Destinations.FindAsync(destinationId);
                if (existingDestination == null)
                {
                    _logger.LogWarning("Destination with ID: {DestinationId} not found", destinationId);
                    throw new Exception("Destination not found");
                }
                _context.Destinations.Remove(existingDestination);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Destination with ID: {DestinationId} deleted successfully", destinationId);
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Delete Destination {existingDestination.Address} with gate {existingDestination.GateId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Destination"
                };
                await _auditTrail.RecordAsync(audit);
                return true;

            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Delete Destination with ID {destinationId} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Destination"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while deleting destination with ID: {DestinationId}", destinationId);
                throw new Exception("An error occurred while deleting the destination.", ex);
                //return false;
            }
        }


        public async Task<List<DestinationModel>> GetDestinationsByGateId(Guid gateId)
        {
            try
            {
                _logger.LogInformation("Retrieving destinations for Gate ID: {GateId}", gateId);
                var destinations = await _context.Destinations
                    .Where(d => (d.GateId == gateId) && (d.Status == true) )
                    .ToListAsync();
                _logger.LogInformation("Retrieved {Count} destinations for Gate ID: {GateId}", destinations.Count, gateId);
                return destinations;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                _logger.LogError(ex, "Error occurred while retrieving destinations for Gate ID: {GateId}", gateId);
                throw new Exception("An error occurred while retrieving the destinations.", ex);
            }
        }

        
        
    }
}
