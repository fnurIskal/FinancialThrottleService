using System;
using System.Collections.Generic;
using System.Text;
using FinancialThrottleService.Application.Interfaces;

namespace FinancialThrottleService.Infrastructure.Persistence.Dummy
{
    public class DummySecurityPriorityClient : ISecurityPriorityClient
    {
        private static readonly Dictionary<string, double> _scores = new()
    {
        { "THYAO",   1.0 },  
        { "SASA",    0.5 },  
        { "AKBNK",   0.0 },  
        { "MSSTOCK", 0.0 }
    };

        public Task<Dictionary<string, double>> GetPrioritiesAsync(string[] securityCodes)
        {
            var result = securityCodes
                .ToDictionary(
                    code => code,
                    code => _scores.TryGetValue(code, out var score) ? score : 0.0);

            return Task.FromResult(result);
        }
    }
}