using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Domain.Models;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS107;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS32501;
using Microsoft.EntityFrameworkCore;

namespace FinancialThrottleService.Infrastructure.Persistence.Sql
{
    public class SqlCheckerRepository : ICheckerRepository
    {
        private readonly RasStaj107Context _context107;
        private readonly RasStaj32501Context _context32501;

        public SqlCheckerRepository(
            RasStaj107Context context107,
            RasStaj32501Context context32501)
        {
            _context107 = context107;
            _context32501 = context32501;
        }

        private static bool IsTurkeyDb(string dbName) =>
            dbName.Contains("107", StringComparison.OrdinalIgnoreCase) ||
            dbName.Equals("RAS_101", StringComparison.OrdinalIgnoreCase) ||
            dbName.Equals("RAS_1", StringComparison.OrdinalIgnoreCase);

        private static bool IsMsSourceDb(string dbName) =>
            dbName.Contains("32501", StringComparison.OrdinalIgnoreCase);

        public async Task<List<CheckerItemResult>> CheckAsync(
            string databaseName,
            int securityId,
            int templateId,
            int quarter)
        {
            List<int?> quarterlyItems = new();
            List<int?> quarterlyOriginalItems = new();

            if (IsTurkeyDb(databaseName))
            {
                quarterlyItems = await _context107.Quarterlies
                    .Where(q => q.SecurityId == securityId &&
                                q.TemplateId == templateId &&
                                q.Quarter == quarter)
                    .Select(q => q.ItemQuarterlyCode)
                    .Distinct()
                    .ToListAsync();

                quarterlyOriginalItems = await _context107.QuarterlyOriginals
                    .Where(q => q.SecurityId == securityId &&
                                q.TemplateId == templateId &&
                                q.Quarter == quarter)
                    .Select(q => q.ItemQuarterlyCode)
                    .Distinct()
                    .ToListAsync();
            }
            else if (IsMsSourceDb(databaseName))
            {
                quarterlyItems = await _context32501.Quarterlies
                    .Where(q => q.SecurityId == securityId &&
                                q.TemplateId == templateId &&
                                q.Quarter == quarter)
                    .Select(q => q.ItemQuarterlyCode)
                    .Distinct()
                    .ToListAsync();

                quarterlyOriginalItems = await _context32501.QuarterlyOriginals
                    .Where(q => q.SecurityId == securityId &&
                                q.TemplateId == templateId &&
                                q.Quarter == quarter)
                    .Select(q => q.ItemQuarterlyCode)
                    .Distinct()
                    .ToListAsync();
            }

            var allCodes = quarterlyItems
                .Concat(quarterlyOriginalItems)
                .Where(c => c != null)
                .Distinct()
                .ToList();

           
            if (allCodes.Count == 0)
            {
                return new List<CheckerItemResult>
                {
                    new CheckerItemResult
                    {
                        ItemQuarterlyCode = null,
                        InQuarterly = false,
                        InQuarterlyOriginal = false,
                        Status = "not_found"
                    }
                };
            }

     
            return allCodes.Select(code =>
            {
                bool inQ = quarterlyItems.Contains(code);
                bool inQO = quarterlyOriginalItems.Contains(code);

                return new CheckerItemResult
                {
                    ItemQuarterlyCode = code,
                    InQuarterly = inQ,
                    InQuarterlyOriginal = inQO,
                    Status = inQ || inQO ? "processed" : "not_found"
                };
            }).ToList();
        }
    }
}