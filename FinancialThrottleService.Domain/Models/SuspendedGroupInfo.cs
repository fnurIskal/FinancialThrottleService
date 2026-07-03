using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Domain.Models
{
    public class SuspendedGroupInfo
    {
        public string DatabaseName { get; set; } = string.Empty;
        public int SecurityId { get; set; }
        public int TemplateId { get; set; }
        public string SecurityCode { get; set; } = string.Empty;
        public int FailureCount { get; set; }
        public DateTime FirstFailedAt { get; set; }
        public DateTime NextRetryAt { get; set; }
        public bool RetryInProgress { get; set; }
        public string LastError { get; set; } = string.Empty;
        public string GroupKey => $"{DatabaseName}|{SecurityId}|{TemplateId}";

    }
}
