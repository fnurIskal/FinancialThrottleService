using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Domain.Models;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS107_PROD;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS32501_PROD;
using Microsoft.EntityFrameworkCore;

namespace FinancialThrottleService.Infrastructure.Persistence.Sql
{
    public class SqlCheckerRepository : ICheckerRepository
    {
        private readonly Ras107Context _context107;
        private readonly Ras32501Context _context32501;

        public SqlCheckerRepository(
            Ras107Context context107,
            Ras32501Context context32501)
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
            int quarter,
            int? itemQuarterlyCode = null)
        {
            List<(int? Code, string Definition, int Order, double? Value)> quarterlyItems = new();
            List<(int? Code, string Definition, int Order, double? Value)> quarterlyOriginalItems = new();

            if (IsTurkeyDb(databaseName))
            {
                quarterlyItems = await _context107.Quarterlies
                    .Where(q => q.SecurityId == securityId &&
                                q.TemplateId == templateId &&
                                q.Quarter == quarter &&
                                (itemQuarterlyCode == null || q.ItemQuarterlyCode == itemQuarterlyCode))
                    .OrderBy(q => q.Order)
                    .Select(q => ValueTuple.Create(q.ItemQuarterlyCode, q.OriginalDefinition ?? string.Empty, q.Order, q.ItemValue))
                    .Distinct()
                    .ToListAsync();

                quarterlyOriginalItems = await _context107.QuarterlyOriginals
                    .Where(q => q.SecurityId == securityId &&
                                q.TemplateId == templateId &&
                                q.Quarter == quarter &&
                                (itemQuarterlyCode == null || q.ItemQuarterlyCode == itemQuarterlyCode))
                    .OrderBy(q => q.Order)
                    .Select(q => ValueTuple.Create(q.ItemQuarterlyCode, q.OriginalDefinition ?? string.Empty, q.Order, q.ItemValue))
                    .Distinct()
                    .ToListAsync();
            }
            else if (IsMsSourceDb(databaseName))
            {
                quarterlyItems = await _context32501.Quarterlies
                    .Where(q => q.SecurityId == securityId &&
                                q.TemplateId == templateId &&
                                q.Quarter == quarter &&
                                (itemQuarterlyCode == null || q.ItemQuarterlyCode == itemQuarterlyCode))
                    .OrderBy(q => q.Order)
                    .Select(q => ValueTuple.Create(q.ItemQuarterlyCode, q.OriginalDefinition ?? string.Empty, q.Order, q.ItemValue))
                    .Distinct()
                    .ToListAsync();

                quarterlyOriginalItems = await _context32501.QuarterlyOriginals
                    .Where(q => q.SecurityId == securityId &&
                                q.TemplateId == templateId &&
                                q.Quarter == quarter &&
                                (itemQuarterlyCode == null || q.ItemQuarterlyCode == itemQuarterlyCode))
                    .OrderBy(q => q.Order)
                    .Select(q => ValueTuple.Create(q.ItemQuarterlyCode, q.OriginalDefinition ?? string.Empty, q.Order, q.ItemValue))
                    .Distinct()
                    .ToListAsync();
            }

            var allCodes = quarterlyItems
                .Select(q => q.Code)
                .Concat(quarterlyOriginalItems.Select(q => q.Code))
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
                        OriginalDefinition = string.Empty,
                        InQuarterly = false,
                        InQuarterlyOriginal = false,
                        Status = "not_found"
                    }
                };
            }

            return allCodes.Select(code =>
            {
                var qItem = quarterlyItems.FirstOrDefault(q => q.Code == code);
                var qoItem = quarterlyOriginalItems.FirstOrDefault(q => q.Code == code);

                bool inQ = qItem.Code != null;
                bool inQO = qoItem.Code != null;
                string definition = !string.IsNullOrEmpty(qItem.Definition) ? qItem.Definition : qoItem.Definition ?? string.Empty;
                // Rapor sırasını (order) korumak için: önce Quarterly'deki, yoksa QuarterlyOriginal'daki order kullanılır.
                int sortOrder = inQ ? qItem.Order : qoItem.Order;

                double? quarterlyValue = inQ ? qItem.Value : null;
                double? originalValue = inQO ? qoItem.Value : null;
                bool? valuesMatch = (quarterlyValue.HasValue && originalValue.HasValue)
                    ? quarterlyValue.Value == originalValue.Value
                    : null;

                return (
                    Result: new CheckerItemResult
                    {
                        ItemQuarterlyCode = code,
                        OriginalDefinition = definition,
                        InQuarterly = inQ,
                        InQuarterlyOriginal = inQO,
                        Status = inQ || inQO ? "processed" : "not_found",
                        QuarterlyValue = quarterlyValue,
                        OriginalValue = originalValue,
                        ValuesMatch = valuesMatch
                    },
                    SortOrder: sortOrder
                );
            })
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Result)
            .ToList();
        }
    }
}