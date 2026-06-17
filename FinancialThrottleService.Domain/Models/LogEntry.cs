using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Domain.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Level { get; set; } = "Information";
        public string Category { get; set; } = "system";
        public string Message { get; set; } = string.Empty;
        public string? GroupKey { get; set; }
        public string? SecurityCode { get; set; }
        public string? DatabaseName { get; set; }
        public int? SecurityId { get; set; }
        public int? TemplateId { get; set; }
        public int? Quarter { get; set; }
        public string? Exception { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
