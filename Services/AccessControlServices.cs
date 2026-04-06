using RVMSService.Data;
using System.Linq.Expressions;

namespace RVMSService.Services
{
    public class AccessControlServices : IAccessControlServices
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AccessControlServices> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private string accessControlGateway;
        private string accessControlGatewayPort;



        public AccessControlServices(AppDBContext dBContext, ILogger<AccessControlServices> logger, IConfiguration configuration)
        {
            _context = dBContext;
            _logger = logger;
            _configuration = configuration;

            accessControlGateway = _configuration["AccessControlGatewayServer"] ?? String.Empty;
            accessControlGatewayPort = _configuration["AccessControlGatewayPort"] ?? String.Empty;
            _httpClient = new HttpClient();
            //var baseAddress = $"http://{accessControlGateway}:{accessControlGatewayPort}/";
        }

        public async Task<bool?> AllowAccess(int globalDoorNumber)
        {
            try
            {
                var url = $"http://{accessControlGateway}:{accessControlGatewayPort}/api/access/allowaccess";

                //initialize httpclient
                //_httpClient = new HttpClient();
                var response = await _httpClient.PostAsync($"{url}?globalDoorNumber={globalDoorNumber}", null);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Access allowed for GlobalDoorNumber: {GlobalDoorNumber}", globalDoorNumber);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to allow access for GlobalDoorNumber: {GlobalDoorNumber}. Status Code: {StatusCode}", globalDoorNumber, response.StatusCode);
                    throw new Exception($"Failed to allow access. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)

            {
                _logger.LogError(ex, "Error occurred while allowing access for GlobalDoorNumber: {GlobalDoorNumber}", globalDoorNumber);
                throw;
            }

            //var url = $"http://{accessControlGateway}:{accessControlGatewayPort}/api/access/allowaccess";

            ////initialize httpclient
            ////_httpClient = new HttpClient();
            //_ = await _httpClient.PostAsync($"{url}?globalDoorNumber={globalDoorNumber}", null);
            //return true;
        }

    }
}
