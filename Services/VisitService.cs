using Microsoft.EntityFrameworkCore;
using RVMSService.Data;
using RVMSService.Models;

namespace RVMSService.Services
{
    public class VisitService : IVisitService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<VisitService> _logger;
        private readonly IAuditTrailService _auditTrail;

        public VisitService(AppDBContext context, ILogger<VisitService> logger, IAuditTrailService auditTrail)
        {
            _context = context;
            _logger = logger;
            _auditTrail = auditTrail;

        }

        public async Task<bool> AddVisitAsync(VisitModel visit)
        {
            try
            {
                await _context.Visits.AddAsync(visit);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Visit added with ID: {visit.VisitId}. ");
                // Record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Add Visit {visit.VisitId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visit"
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
                    Description = $"Add visit {visit.VisitId} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visit"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while adding visit to database");
                throw new Exception("An error occurred while adding the visit.", ex);
            }
        }

        public async Task<bool> UpdateVisitAsync(VisitModel visit)
        {
            try
            {
                _context.Visits.Update(visit);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Visit updated with ID: {visit.VisitId}. ");
                // Record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Update Visit {visit.VisitId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visit"
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
                    Description = $"Update visit {visit.VisitId} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visit"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while updating visit in database");
                throw new Exception("An error occurred while updating the visit.", ex);
            }
        }

        public async Task<VisitModel?> GetVisitByIdAsync(Guid visitId)
        {
            return await _context.Visits.FindAsync(visitId);
        }

        public async Task<bool> updateVisitSignOut(Guid visitId)
        {
            try
            {
                var visit = await _context.Visits.FindAsync(visitId);
                if (visit == null)
                {
                    return false;
                }
                visit.CheckOut = DateTime.UtcNow;
                _context.Visits.Update(visit);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Visit signed out with ID: {visit.VisitId}. ");
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Sign out Visit {visit.VisitId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visit"
                };
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while signing out visit in database");
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Sign out visit {visitId} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visit"
                };
                throw new Exception("An error occurred while signing out the visit.", ex);
            }
        }

        public async Task<List<VisitModel>> GetLastSignedInVisitsWithoutPhotosToday(int count)
        {
            return await _context.Visits
                .Where(v => (v.CheckOut == null) && (v.CheckIn == DateTime.Today))
                .OrderByDescending(v => v.CheckIn)
                .Select(v => new VisitModel
                {
                    VisitId = v.VisitId,
                    VisitorId = v.VisitorId,
                    TypeId = v.TypeId,
                    GateId = v.GateId,
                    QrId = v.QrId,
                    DestinationId = v.DestinationId,
                    UserId = v.UserId,
                    CheckIn = v.CheckIn,
                    //CheckOut = v.CheckOut,
                    Status = v.Status
                    // Exclude photo fields
                })
                .Take(count)
                .ToListAsync();
        }

        public async Task<VisitModel?> GetVisitPhotos(Guid visitId)
        {
            return await _context.Visits
                .Where(v => v.VisitId == visitId)
                .Select(v => new VisitModel
                {
                    VisitId = v.VisitId,
                    CurrentPhoto = v.CurrentPhoto,
                    VehiclePhoto = v.VehiclePhoto,
                    AdditionalPhoto = v.AdditionalPhoto
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<VisitModel>> GetVisitswithoutPhotosByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Visits
                .Where(v => v.CheckIn.Date >= startDate.Date && v.CheckIn.Date <= endDate.Date)
                .Select(v => new VisitModel
                {
                    VisitId = v.VisitId,
                    VisitorId = v.VisitorId,
                    TypeId = v.TypeId,
                    GateId = v.GateId,
                    QrId = v.QrId,
                    DestinationId = v.DestinationId,
                    UserId = v.UserId,
                    CheckIn = v.CheckIn,
                    CheckOut = v.CheckOut,
                    Status = v.Status
                    // Exclude photo fields
                })
                .ToListAsync();
        }

    }
}
