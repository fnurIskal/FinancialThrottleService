using FinancialThrottleService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Infrastructure.Persistence.Dummy
{
    public class DummyFinancialTransactionApi: IFinancialTransactionApi
    {
        public Task<string> GenerateInflationAsync(
        string database, int quarter,
        int securityId, int templateId, string commands)
        {
            var sql = $"-- [DUMMY] GenerateInflation " +
                      $"DB={database} SecurityId={securityId} " +
                      $"Quarter={quarter} TemplateId={templateId}";

            Console.WriteLine(sql);
            return Task.FromResult(sql);
        }

        public Task<string> GenerateRestatedAsync(
            string database, int quarter,
            int securityId, int templateId, string commands)
        {
            var sql = $"-- [DUMMY] GenerateRestated " +
                      $"DB={database} SecurityId={securityId} " +
                      $"Quarter={quarter} TemplateId={templateId}";

            Console.WriteLine(sql);
            return Task.FromResult(sql);
        }
    }
}
