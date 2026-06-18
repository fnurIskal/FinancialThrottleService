using FinancialThrottle.Grpc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinancialThrottle.Apii.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly ThrottleService.ThrottleServiceClient _grpcClient;
        private readonly ILogger <StatusController> _logger;

        public StatusController(
           ThrottleService.ThrottleServiceClient grpcClient,
           ILogger<StatusController> logger)
        {
            _grpcClient = grpcClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var response = await _grpcClient.GetStatusAsync(new StatusRequest());
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "gRPC GetStatus hatası");
                return StatusCode(500, "Worker'a bağlanılamadı");
            }
        }
    }
}
