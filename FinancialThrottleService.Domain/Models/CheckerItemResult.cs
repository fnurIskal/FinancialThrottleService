using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Domain.Models
{
    public class CheckerItemResult
    {
        public int? ItemQuarterlyCode { get; set; }
        public string OriginalDefinition { get; set; } = string.Empty;
        public bool InQuarterly { get; set; }
        public bool InQuarterlyOriginal {  get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
