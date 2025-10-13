using RVMSService.Models;

namespace RVMSService.Services
{
    public interface IDestinationService
    {
        Task<Guid?> AddDestination(DestinationModel destination);
        Task<bool> DeleteDestination(Guid destinationId);
        Task<List<DestinationModel>> GetAllDestinations();
        Task<DestinationModel?> GetDestinationById(Guid destinationId);
        Task UpdateDestination(DestinationModel destination);
        Task<List<DestinationModel>> GetDestinationsByGateId(Guid gateId);

    }
}