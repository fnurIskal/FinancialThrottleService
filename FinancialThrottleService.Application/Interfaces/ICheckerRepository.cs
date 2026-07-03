using FinancialThrottleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Interfaces
{
    public interface ICheckerRepository
    {
        Task<List<CheckerItemResult>> CheckAsync(
            string databaseName,
            int securityId,
            int templateId,
            int quarter,
            int? itemQuarterlyCode = null);
    }
}
