using RVMSService.Data;

namespace RVMSService.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<HealthCheckService> _logger;
        //private readonly IAuditTrailService _auditTrail;

        public HealthCheckService(AppDBContext context, ILogger<HealthCheckService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsServerOK()
        {
            try
            {
                //Simple check to see if the client can connect to server app
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }
}
