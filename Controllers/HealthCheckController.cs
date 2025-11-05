using Microsoft.AspNetCore.Mvc;
using RVMSService.Services;

namespace RVMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class HealthCheckController : Controller
    {

       

        private readonly ILogger<HealthCheckService> _logger;
        private readonly IHealthCheckService _healthCheck;

        public HealthCheckController(ILogger<HealthCheckService> logger, IHealthCheckService healthCheck)
        {
            _logger = logger;
            _healthCheck = healthCheck;

        }

        // GET: api/HealthCheck/isServerOK
        [HttpGet("isServerOK")]

        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Health check requested");
                var isServerOK = _healthCheck.IsServerOK().Result;
                if (isServerOK)
                {
                    _logger.LogInformation("Server is OK");
                    return Ok(new { status = "OK", message = "Server is operational." });
                }
                else
                {
                    _logger.LogWarning("Server is not OK");
                    return StatusCode(503, new { status = "Service Unavailable", message = "Server is experiencing issues." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during health check");
                return StatusCode(500, new { status = "Error", message = "An error occurred while checking server health." });
            }
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
