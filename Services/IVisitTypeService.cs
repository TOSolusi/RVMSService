using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IVisitTypeService
    {
        Task<Guid?> AddVisitType(VisitTypeModel visitType);
        Task<bool> DeleteVisitType(Guid visitTypeId);
        Task<List<VisitTypeModel>> GetAllVisitTypes();
       
        Task UpdateVisitType(VisitTypeModel visitType);
        Task<List<VisitTypeModel>> GetActiveVisitTypes();


    }
}