using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Interfaces
{
    public interface ISecurityPriorityClient
    {
        Task<Dictionary<string, double>> GetPrioritiesAsync(string[] securityCodes);
    }
}
