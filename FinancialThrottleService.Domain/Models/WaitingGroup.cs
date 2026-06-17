using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Domain.Models
{
    public class WaitingGroup
    {
        public string DatabaseName { get; set; } = string.Empty;
        public int SecurityId { get; set; }
        public int TemplateId { get; set; }
        public string SecurityCode { get; set; } = string.Empty;
        public List<WaitingItem> Items { get; set; } = new();
        public string GroupKey => $"{DatabaseName}|{SecurityId}|{TemplateId}";
    }
}
