using FinancialThrottle.Grpc;
using Microsoft.AspNetCore.Mvc;

namespace FinancialThrottle.Apii.Controllers
{
    [Route("api/register-token")]
    [ApiController]
    public class RegisterTokenController : ControllerBase
    {
        private readonly ThrottleService.ThrottleServiceClient _grpcClient;
        private readonly ILogger<RegisterTokenController> _logger;

        public RegisterTokenController(
            ThrottleService.ThrottleServiceClient grpcClient,
            ILogger<RegisterTokenController> logger)
        {
            _grpcClient = grpcClient;
            _logger = logger;
        }

        public class RegisterTokenRequestBody
        {
            public string Token { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenRequestBody body)
        {
            if (string.IsNullOrWhiteSpace(body?.Token))
                return BadRequest(new { error = "Token is required" });

            try
            {
                var response = await _grpcClient.RegisterPushTokenAsync(
                    new RegisterPushTokenRequest { Token = body.Token });

                if (response.Success)
                    return Ok(new { message = response.Message });
                else
                {

                    _logger.LogWarning("gRPC Servisinden gelen hata mesajı: {Message}", response.Message);
                return BadRequest(new { error = response.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register push token");
                return StatusCode(500, new { error = "Failed to register push token" });
            }
        }
    }
}
