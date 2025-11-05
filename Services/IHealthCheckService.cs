
namespace RVMSService.Services
{
    public interface IHealthCheckService
    {
        Task<bool> IsServerOK();
    }
}