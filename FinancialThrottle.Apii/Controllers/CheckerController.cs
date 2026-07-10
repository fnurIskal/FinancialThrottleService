using FinancialThrottleService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinancialThrottle.Apii.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckerController : ControllerBase
    {
        private readonly ICheckerRepository _checkerRepository;
        private readonly ILogger<CheckerController> _logger;

        public CheckerController(
            ICheckerRepository checkerRepository,
            ILogger<CheckerController> logger)
        {
            _checkerRepository = checkerRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Check(
            [FromQuery] string databaseName,
            [FromQuery] int securityId,
            [FromQuery] int templateId,
            [FromQuery] int quarter,
            [FromQuery] int? itemQuarterlyCode = null)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                return BadRequest(new { error = "databaseName is required" });

            try
            {
                var results = await _checkerRepository.CheckAsync(
                    databaseName, securityId, templateId, quarter, itemQuarterlyCode);

                return Ok(new
                {
                    databaseName,
                    securityId,
                    templateId,
                    quarter,
                    itemQuarterlyCode,
                    totalCount = results.Count,
                    processedCount = results.Count(r => r.Status == "processed"),
                    notFoundCount = results.Count(r => r.Status == "not_found"),
                    items = results.Select(r => new
                    {
                        itemQuarterlyCode = r.ItemQuarterlyCode,
                        originalDefinition = r.OriginalDefinition,
                        inQuarterly = r.InQuarterly,
                        inQuarterlyOriginal = r.InQuarterlyOriginal,
                        status = r.Status,
                        quarterlyValue = r.QuarterlyValue,
                        originalValue = r.OriginalValue,
                        valuesMatch = r.ValuesMatch
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checker failed for {Db}/{SecurityId}/{TemplateId}/{Quarter}",
                    databaseName, securityId, templateId, quarter);
                return StatusCode(500, new { error = "Checker query failed", detail = ex.Message });
            }

        }
    }
}
