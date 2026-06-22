using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Domain.Models
{
    public class WaitingItem
    {
        public int Quarter { get; set; }
        public bool IsOriginal { get; set; }
        public string Username { get; set; } = string.Empty;
        public int DisclosureId { get; set; }
        public bool SendEmail { get; set; }

    }
}
