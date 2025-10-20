using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IVisitService
    {
        Task<bool> AddVisitAsync(VisitModel visit);
        Task<List<VisitModel>> GetLastSignedInVisitsWithoutPhotosToday(int count);
        Task<VisitModel?> GetVisitByIdAsync(Guid visitId);
        Task<VisitModel?> GetVisitPhotos(Guid visitId);
        Task<List<VisitModel>> GetVisitswithoutPhotosByDateRange(DateTime startDate, DateTime endDate);
        Task<bool> UpdateVisitAsync(VisitModel visit);
        Task<bool> updateVisitSignOut(Guid visitId);
    }
}