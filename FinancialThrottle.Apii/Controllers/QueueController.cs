using FinancialThrottle.Grpc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinancialThrottle.Apii.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly ThrottleService.ThrottleServiceClient _grpcClient;
        private readonly ILogger<QueueController> _logger;

        public QueueController(
            ThrottleService.ThrottleServiceClient grpcClient,
            ILogger<QueueController> logger)
        {
            _grpcClient = grpcClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetQueue()
        {
            try
            {
                var response = await _grpcClient.GetQueueAsync(new QueueRequest());
                return Ok(new
                {
                    groups = response.Groups.Select(g => new
                    {
                        databaseName = g.DatabaseName,
                        securityId = g.SecurityId,
                        templateId = g.TemplateId,
                        securityCode = g.SecurityCode,
                        priorityScore = g.PriorityScore,
                        itemCount = g.ItemCount,
                        orderType = g.OrderType,
                        status = g.Status,
                        items = g.Items.Select(i => new
                        {
                            quarter = i.Quarter,
                            isOriginal = i.IsOriginal,
                            username = i.Username,
                            disclosureId = i.DisclosureId
                        }).ToList()
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve queue");
                return StatusCode(500, new { error = "Failed to retrieve queue information", detail = ex.Message, type = ex.GetType().Name });
            }
        }

        [HttpGet("{db}/{securityId}/{templateId}")]
        public async Task<IActionResult> GetQueueDetail(
            string db, int securityId, int templateId)
        {
            try
            {
                var response = await _grpcClient.GetQueueAsync(new QueueRequest());

                var group = response.Groups.FirstOrDefault(g =>
                    g.DatabaseName == db &&
                    g.SecurityId == securityId &&
                    g.TemplateId == templateId);

                if (group is null)
                    return NotFound(new { error = $"Group not found: {db}|{securityId}|{templateId}" });

                return Ok(new
                {
                    databaseName = group.DatabaseName,
                    securityId = group.SecurityId,
                    templateId = group.TemplateId,
                    securityCode = group.SecurityCode,
                    priorityScore = group.PriorityScore,
                    itemCount = group.ItemCount, 
                    items = group.Items.Select(i => new
                    {
                        quarter = i.Quarter,
                        isOriginal = i.IsOriginal,
                        username = i.Username,
                        disclosureId = i.DisclosureId
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve queue detail");
                return StatusCode(500, new { error = "Failed to retrieve group details" });
            }
        }
    }
}