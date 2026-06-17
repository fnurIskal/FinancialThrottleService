using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Interfaces
{
    public interface IFinancialTransactionApi
    {
        Task<string> GenerateInflationAsync(
        string database,
        int quarter,
        int securityId,
        int templateId,
        string commands);
        Task<string> GenerateRestatedAsync(
        string database,
        int quarter,
        int securityId,
        int templateId,
        string commands);
    }
}
