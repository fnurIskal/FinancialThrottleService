using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Interfaces
{
    internal interface ISecurityPriorityClient
    {
        Task<Dictionary<string, double>> GetPrioritiesAsync(string[] securityCodes);
    }
}
