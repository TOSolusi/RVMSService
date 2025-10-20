using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IVisitorService
    {
        Task<bool> AddVisitorAsync(VisitorModel visitor);
        Task<bool> DeleteVisitorAsync(Guid visitorId);
        Task<List<VisitorModel>> GetAllBlacklistedVisitorsAsync();
        Task<List<VisitorModel>> GetAllVisitorsInfoOnlyAsync();
        Task<VisitorModel?> GetVisitorPictureByIdAsync(Guid visitorId);
        Task<bool> UpdateVisitor(VisitorModel visitor);
    }
}