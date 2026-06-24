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
                return Ok(new
                {
                    isRunning = response.IsRunning,
                    queuedCount = response.QueuedCount,
                    suspendedCount = response.SuspendedCount,
                    processedThisCycle = response.ProcessedThisCycle,
                    lastHeartbeatUtc = response.LastHeartbeatUtc,
                    workerStartedUtc = response.WorkerStartedUtc
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve status");
                return StatusCode(500, new { error = "Failed to retrieve status" });
            }
        }
    }
}
