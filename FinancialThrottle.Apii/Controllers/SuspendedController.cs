using FinancialThrottle.Grpc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinancialThrottle.Apii.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SuspendedController : ControllerBase
    {
        private readonly ThrottleService.ThrottleServiceClient _grpcClient;
        private readonly ILogger<SuspendedController> _logger;

        public SuspendedController(
            ThrottleService.ThrottleServiceClient grpcClient,
            ILogger<SuspendedController> logger)
        {
            _grpcClient = grpcClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuspended()
        {
            try
            {
                var response = await _grpcClient.GetSuspendedAsync(new SuspendedRequest());
                return Ok(new
                {
                    totalCount = response.Groups.Count,
                    groups = response.Groups.Select(g => new
                    {
                        databaseName = g.DatabaseName,
                        securityId = g.SecurityId,
                        templateId = g.TemplateId,
                        securityCode = g.SecurityCode,
                        failureCount = g.FailureCount,
                        firstFailedUtc = g.FirstFailedUtc,
                        nextRetryUtc = g.NextRetryUtc,
                        retryInProgress = g.RetryInProgress
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve suspended groups");
                return StatusCode(500, new { error = "Failed to retrieve suspended groups" });
            }
        }

        [HttpPost("{db}/{securityId}/{templateId}/retry")]
        public async Task<IActionResult> RetryGroup(
            string db, int securityId, int templateId)
        {
            try
            {
                var request = new RetryGroupRequest
                {
                    DatabaseName = db,
                    SecurityId = securityId,
                    TemplateId = templateId
                };

                var response = await _grpcClient.RetryGroupAsync(request);

                if (response.Success)
                    return Ok(new { message = response.Message });
                else
                    return BadRequest(new { error = response.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retry group");
                return StatusCode(500, new { error = "Failed to retry group" });
            }
        }
    }
}
