using Microsoft.EntityFrameworkCore;
using RVMSService.Data;
using RVMSService.Models;

namespace RVMSService.Services
{
    public class AuditTrailService : IAuditTrailService
    {
        private readonly AppDBContext _dbContext;
        private readonly ILogger<AuditTrailService> _logger;

        public AuditTrailService(AppDBContext dbContext, ILogger<AuditTrailService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task RecordAsync(AuditTrailModel audit)
        {
            await _dbContext.AuditTrails.AddAsync(audit);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Audit trail recorded: {Description} by {UserId}", audit.Description, audit.UserId);
        }

    }
}
