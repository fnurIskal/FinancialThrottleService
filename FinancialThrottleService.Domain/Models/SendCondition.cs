using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Domain.Models
{
    public enum SendCondition
    {
        Send,
        Wait,
        Skip
    }
}
