using FinancialThrottleService.Domain.Models;

namespace FinancialThrottleService.Application.Interfaces
{
    public interface ILogRepository
    {
        Task WriteAsync(LogEntry entry);
        Task<LogsResponse> GetLogsByDateAsync(DateTime date);
        Task<LogsResponse> GetLogsByDateAndCategoryAsync(DateTime date, string category);
        Task<LogsResponse> GetLogsByDateAndLevelAsync(DateTime date, string level);
    }

   
    public class LogsResponse
    {
        public List<LogEntry> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public string Date { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Level { get; set; }
    }
}