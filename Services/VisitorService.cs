using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using RVMSService.Data;
using RVMSService.Models;

namespace RVMSService.Services
{
    public class VisitorService : IVisitorService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<VisitorService> _logger;
        private readonly IAuditTrailService _auditTrail;
        public VisitorService(AppDBContext context, ILogger<VisitorService> logger, IAuditTrailService auditTrail)
        {
            _context = context;
            _logger = logger;
            _auditTrail = auditTrail;
        }

        public async Task<bool> AddVisitorAsync(VisitorModel visitor)
        {
            try
            {
                await _context.Visitors.AddAsync(visitor);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Visitor added with ID: {visitor.VisitorId}. ");

                // Record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Add Visitor {visitor.VisitorId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visitor"
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
                    Description = $"Add visitor {visitor.VisitorName} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visitor"
                };
                await _auditTrail.RecordAsync(audit);

                _logger.LogError(ex, "Error occurred while adding visitor to database");
                throw new Exception("An error occurred while adding the visitor.", ex);
            }
        }


        public async Task<bool> DeleteVisitorAsync(Guid visitorId)
        {
            //VisitorModel existingVisitor = new();
            try
            {
                var existingVisitor = await _context.Visitors.FindAsync(visitorId);
                if (existingVisitor == null)
                {
                    return false;
                }
                _context.Visitors.Remove(existingVisitor);
                await _context.SaveChangesAsync();
                // Record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Delete Visitor {visitorId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visitor"
                };
                await _auditTrail.RecordAsync(audit);
                return true;
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Delete visitor fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visitor"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error deleting visitor");
                return false;
            }
        }

        public async Task<bool> UpdateVisitor(VisitorModel visitor)
        {
            VisitorModel? existingVisitor = new();
            try
            {
                existingVisitor = await _context.Visitors.FindAsync(visitor.VisitorId);
                if (existingVisitor == null)
                {
                    return false;
                }
                existingVisitor.VisitorName = visitor.VisitorName;
                existingVisitor.VisitorPhone = visitor.VisitorPhone;
                existingVisitor.LastVisit = visitor.LastVisit;
                existingVisitor.Blacklist = visitor.Blacklist;
                if (visitor.VisitorImage != null && visitor.VisitorImage.Length > 0)
                {
                    existingVisitor.VisitorImage = visitor.VisitorImage;
                }
                _context.Visitors.Update(existingVisitor);
                await _context.SaveChangesAsync();
                // Record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Update Visitor {visitor.VisitorId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Visitor"
                };
                await _auditTrail.RecordAsync(audit);
                return true;
            }
            catch (Exception ex)
            {
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"update visitor {existingVisitor.VisitorName} fail",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Visitor"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error updating visitor");
                return false;
            }
        }


        public async Task<List<VisitorModel>> GetAllVisitorsInfoOnlyAsync()
        {
            try
            {
                return await _context.Visitors
                    .Select(v => new VisitorModel
                    {
                        VisitorId = v.VisitorId,
                        VisitorIdNo = v.VisitorIdNo,
                        VisitorName = v.VisitorName,
                        VisitorPhone = v.VisitorPhone,
                        LastVisit = v.LastVisit,
                        Blacklist = v.Blacklist
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving visitors");
                throw new Exception("An error occurred while retrieving visitors.", ex);
            }
        }

        public async Task<VisitorModel?> GetVisitorPictureByIdAsync(Guid visitorId)
        {
            VisitorModel? visitor = new();
            try
            {
                visitor = await _context.Visitors
                .Where(v => v.VisitorId == visitorId)
                .Select(v => new VisitorModel
                {
                    VisitorId = v.VisitorId,
                    VisitorImage = v.VisitorImage
                })
                .FirstOrDefaultAsync();
                return visitor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving visitor picture by ID");
                throw new Exception("An error occurred while retrieving the visitor picture.", ex);
            }

        }

        public async Task<List<VisitorModel>> GetAllBlacklistedVisitorsAsync()
        {
            try
            {
                return await _context.Visitors
                    .Where(v => v.Blacklist == true)
                    .Select(v => new VisitorModel
                    {
                        VisitorId = v.VisitorId,
                        VisitorIdNo = v.VisitorIdNo,
                        VisitorName = v.VisitorName,
                        VisitorPhone = v.VisitorPhone,
                        LastVisit = v.LastVisit,
                        Blacklist = v.Blacklist
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blacklisted visitors");
                throw new Exception("An error occurred while retrieving blacklisted visitors.", ex);
            }
        }
    }
}
 