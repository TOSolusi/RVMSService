namespace RVMSService.Services
{
    public interface IAccessControlServices
    {
        Task<bool?> AllowAccess(int globalDoorNumber);
    }
}