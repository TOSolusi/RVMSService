using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IGateService
    {
        Task<Guid?> AddGate(GateModel gate);
        Task<List<GateModel>> GetAllGates();
        Task UpdateGate(GateModel gate);
        Task DeleteGate(GateModel gate);
        Task<GateModel> GetGateById(Guid gateId);


    }
}