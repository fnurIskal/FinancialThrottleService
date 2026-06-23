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
        [HttpGet("{date}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLogsByDate([FromRoute] string date)
        {
            try
            {
                _logger.LogInformation("GetLogsByDate called with date: {Date}", date);

                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    _logger.LogWarning("Invalid date format provided: {Date}", date);
                    return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd" });
                }

                var response = await _logRepository.GetLogsByDateAsync(parsedDate);

                _logger.LogInformation(
                    "Successfully retrieved {Count} logs for date {Date}",
                    response.TotalCount, date);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLogsByDate with date: {Date}", date);
                return StatusCode(500, new { error = "Failed to retrieve logs", details = ex.Message });
            }
        }

        [HttpGet("{date}/{category}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLogsByDateAndCategory(
             [FromRoute] string date,
             [FromRoute] string category)
        {
            try
            {
                _logger.LogInformation(
                    "GetLogsByDateAndCategory called with date: {Date}, category: {Category}",
                    date, category);

                // Validate date format
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    _logger.LogWarning("Invalid date format provided: {Date}", date);
                    return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd" });
                }

                // Validate category
                var validCategories = new[] { "financial", "system", "api", "consistency" };
                if (!validCategories.Contains(category.ToLower()))
                {
                    _logger.LogWarning("Invalid category provided: {Category}", category);
                    return BadRequest(new
                    {
                        error = "Invalid category",
                        validCategories = validCategories
                    });
                }

                // Get logs from repository
                var response = await _logRepository.GetLogsByDateAndCategoryAsync(parsedDate, category);

                _logger.LogInformation(
                    "Successfully retrieved {Count} logs for date {Date} and category {Category}",
                    response.TotalCount, date, category);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error in GetLogsByDateAndCategory with date: {Date}, category: {Category}",
                    date, category);
                return StatusCode(500, new { error = "Failed to retrieve logs", details = ex.Message });
            }
        }

        [HttpGet("{date}/level/{level}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLogsByDateAndLevel(
            [FromRoute] string date,
            [FromRoute] string level)
        {
            try
            {
                _logger.LogInformation(
                    "GetLogsByDateAndLevel called with date: {Date}, level: {Level}",
                    date, level);

                // Validate date format
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    _logger.LogWarning("Invalid date format provided: {Date}", date);
                    return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd" });
                }

                // Validate level (allow any level string, but warn if unusual)
                if (string.IsNullOrWhiteSpace(level))
                {
                    _logger.LogWarning("Empty level provided");
                    return BadRequest(new { error = "Level cannot be empty" });
                }

                var validLevels = new[] { "Information", "Warning", "Error", "Debug" };
                if (!validLevels.Contains(level))
                {
                    _logger.LogWarning("Unusual log level provided: {Level}", level);
                }

                // Get logs from repository
                var response = await _logRepository.GetLogsByDateAndLevelAsync(parsedDate, level);

                _logger.LogInformation(
                    "Successfully retrieved {Count} logs for date {Date} and level {Level}",
                    response.TotalCount, date, level);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error in GetLogsByDateAndLevel with date: {Date}, level: {Level}",
                    date, level);
                return StatusCode(500, new { error = "Failed to retrieve logs", details = ex.Message });
            }
        }
    } }