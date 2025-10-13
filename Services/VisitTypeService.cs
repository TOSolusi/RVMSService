using Microsoft.EntityFrameworkCore;
using RVMSService.Data;
using RVMSService.Models;

namespace RVMSService.Services
{

    public class VisitTypeService : IVisitTypeService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<VisitTypeService> _logger;
        private readonly IAuditTrailService _auditTrail;

        public VisitTypeService(AppDBContext context, ILogger<VisitTypeService> logger, IAuditTrailService auditTrail)
        {
            _context = context;
            _logger = logger;
            _auditTrail = auditTrail;

        }

        public async Task<Guid?> AddVisitType(VisitTypeModel visitType)
        {
            try
            {
                _logger.LogInformation("Adding new visit type");

                // Implementation for adding destination
                await _context.VisitTypes.AddAsync(visitType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Visit Types added with ID : {TypeId}", visitType.TypeId);

                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Add Visit Type {visitType.TypeVisit}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visit Type"
                };
                await _auditTrail.RecordAsync(audit);

                // After SaveChangesAsync, destination.DestinationId will contain the generated GUID
                return visitType.TypeId;
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Add Visit Type {visitType.TypeVisit} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visit Type"
                };
                await _auditTrail.RecordAsync(audit);

                _logger.LogError(ex, "Error occurred while adding Visit Type to database");
                throw new Exception("An error occurred while adding the Visit Type.", ex);
            }
        }

        public async Task<bool> DeleteVisitType(Guid visitTypeId)
        {
            try
            {
                _logger.LogInformation($"Deleting Visit Type with ID: {visitTypeId}");
                var existingVisitType = await _context.VisitTypes.FindAsync(visitTypeId);
                if (existingVisitType == null)
                {
                    _logger.LogWarning($"Visit Type with ID: {visitTypeId} not found");
                    throw new Exception("Visit type not found");
                }
                _context.VisitTypes.Remove(existingVisitType);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Visit Types with ID: {visitTypeId} deleted successfully");
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Delete Visit Types {existingVisitType}.",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visit Type"
                };
                await _auditTrail.RecordAsync(audit);
                return true;
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Delete Visit Types {visitTypeId} fail.",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visit Type"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, $"Error occurred while deleting Visit Type with ID: {visitTypeId}");
                throw new Exception("An error occurred while deleting the Visit Type.", ex);
            }
        }

        public async Task<List<VisitTypeModel>> GetAllVisitTypes()
        {
            try
            {
                _logger.LogInformation("Retrieving all visit types");
                var visitTypes = await _context.VisitTypes.ToListAsync();
                _logger.LogInformation("Retrieved {Count} visit types", visitTypes.Count);
                return visitTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving visit types from database");
                throw new Exception("An error occurred while retrieving visit types.", ex);
            }
        }


        public async Task UpdateVisitType(VisitTypeModel visitType)
        {
            try
            {
                _logger.LogInformation($"Updating Visit Type with ID: {visitType.TypeId}");
                var existingVisitType = await _context.VisitTypes.FindAsync(visitType.TypeId);
                if (existingVisitType == null)
                {
                    _logger.LogWarning($"Visit Type with ID: {visitType.TypeId} not found");
                    throw new Exception("Visit type not found");
                }
                // Update fields
                existingVisitType.TypeVisit = visitType.TypeVisit;
                existingVisitType.TypeColorBadge = visitType.TypeColorBadge;
                existingVisitType.UpdatedAt = DateTime.UtcNow;
                existingVisitType.Status = visitType.Status;
                existingVisitType.Default = visitType.Default;

                // Save changes
                _context.VisitTypes.Update(existingVisitType);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Visit Type with ID: {visitType.TypeId} updated successfully");
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Update Visit Type {visitType.TypeVisit}.",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visit Type"
                };
                await _auditTrail.RecordAsync(audit);
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Update Visit Type {visitType.TypeId} fail.",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visit Type"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, $"Error occurred while updating Visit Type with ID: {visitType.TypeId}");
                throw new Exception("An error occurred while updating the Visit Type.", ex);
            }
        }

        public async Task<List<VisitTypeModel>> GetActiveVisitTypes()
        {
            try
            {
                _logger.LogInformation("Retrieving active visit types");
                var visitTypes = await _context.VisitTypes.Where(vt => vt.Status).ToListAsync();
                _logger.LogInformation("Retrieved {Count} active visit types", visitTypes.Count);
                return visitTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving active visit types from database");
                throw new Exception("An error occurred while retrieving active visit types.", ex);
            }
        }
    }
}
