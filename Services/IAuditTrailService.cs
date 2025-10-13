using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IAuditTrailService
    {
        Task RecordAsync(AuditTrailModel audit);
    }
}