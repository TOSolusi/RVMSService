using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IVisitService
    {
        Task<bool> AddVisitAsync(DOTVisitModel dotVisit);
        Task<List<VisitModel>> GetLastSignedInVisitsWithoutPhotosToday(int count);
        Task<VisitModel?> GetVisitByIdAsync(Guid visitId);
       
        Task<List<VisitModel>> GetVisitswithoutPhotosByDateRange(DateTime startDate, DateTime endDate);
        Task<bool> UpdateVisitAsync(VisitModel visit);
        Task<bool> updateVisitSignOut(Guid visitId);
        Task<List<VisitModel>> GetVisitswithoutPhotosByDateRangeByGate(DateTime startDate, DateTime endDate, Guid gateId);
        Task<VisitModel?> GetVisitPhotos(Guid visitId);
        Task<VisitorModel?> GetVisitorPhotos(Guid visitorId);
    }
}