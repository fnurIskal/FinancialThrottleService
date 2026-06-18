using Microsoft.AspNetCore.Mvc;
using FinancialThrottleService.Application.Interfaces;

namespace FinancialThrottle.Apii.Controllers
{
    //TEST EDİLEMEDİ
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
    
    private readonly ILogRepository _logRepository;
        private readonly ILogger<LogsController> _logger;

        public LogsController(
            ILogRepository logRepository,
            ILogger<LogsController> logger)
        {
            _logRepository = logRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogDates()
        {
            try
            {
                var dates = Enumerable.Range(0, 30)
                    .Select(i => DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd"))
                    .ToList();

                return Ok(new { totalCount = dates.Count, dates });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve log dates");
                return StatusCode(500, new { error = "Failed to retrieve log dates" });
            }
        }

        [HttpGet("{date}/{category}")]
        public async Task<IActionResult> GetLogsByDateAndCategory(string date, string category)
        {
            try
            {
                var logs = await _logRepository.QueryAsync(date, category: category, level: null, take: 100);
                return Ok(new { totalCount = logs.Count, logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve logs");
                return StatusCode(500, new { error = "Failed to retrieve logs" });
            }
        }
    } }