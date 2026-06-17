using FinancialThrottleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Interfaces
{
    public interface ILogRepository
    {
        Task WriteAsync(LogEntry entry);
        Task<List<LogEntry>> QueryAsync(
       string date,       
       string? category, 
       string? level,  
       int skip = 0,
       int take = 100);
        IAsyncEnumerable<LogEntry> StreamAsync(
        string date,
        string? category,
        CancellationToken ct);
    }
}
