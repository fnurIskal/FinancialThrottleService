using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Domain.Models;
using FinancialThrottleService.Infrastructure.Models.Generated;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS;
using Microsoft.EntityFrameworkCore;

namespace FinancialThrottleService.Infrastructure.Persistence.Sql
{
    public class SqlFinancialRepository : IFinancialRepository
    {

        private readonly RasStajContext _context;

        public SqlFinancialRepository(RasStajContext context)
        {
            _context = context;
        }

        public async Task<List<WaitingGroup>> GetAllWaitingGroupsAsync()
        {
            Console.WriteLine($"[DEBUG] Context type: {_context.GetType().Name}");
            Console.WriteLine($"[DEBUG] Database: {_context.Database.GetDbConnection().Database}");

            var count = await _context.WaitingFinancialTables.CountAsync();
            Console.WriteLine($"[DEBUG] WaitingFinancialTables count: {count}");

            var items = await _context.WaitingFinancialTables
                .GroupBy(w => new { w.DatabaseName, w.SecurityId, w.TemplateId })
                .Select(g => new WaitingGroup
                {
                    DatabaseName = g.Key.DatabaseName,
                    SecurityId = g.Key.SecurityId,
                    TemplateId = g.Key.TemplateId,
                    SecurityCode = g.First().Username ?? "UNKNOWN",
                    Items = g.Select(w => new WaitingItem
                    {
                        Quarter = w.Quarter,
                        IsOriginal = w.IsOriginal,
                        Username = w.Username ?? "",
                        DisclosureId = w.DisclosureId
                    }).ToList()
                })
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Returning {items.Count} groups");
            return items;
        }

        public async Task<int[]> GetMsSourceIdsAsync()
        {
            return new[] { 32501 };
        }

        public async Task<Dictionary<(int, int), int>> GetDuplicateItemCodeMapAsync()
        {
            var map = await _context.DuplicateTableTemplateMaps
                .Where(d => d.FromTemplateId.HasValue && d.ToTemplateId.HasValue)
                .ToDictionaryAsync(
                    d => (d.SecurityId, d.FromTemplateId.Value),
                    d => d.ToTemplateId.Value);
            return map;
        }
        public async Task<List<int>> GetTableTypeIdsAsync(
            string databaseName, int securityId, int quarter, int templateId, bool isOriginal)
        {
            var typeIds = await _context.WaitingFinancialTables
                .Where(w => w.DatabaseName == databaseName &&
                            w.SecurityId == securityId &&
                            w.Quarter == quarter &&
                            w.TemplateId == templateId &&
                            w.IsOriginal == isOriginal)
                .Select(w => w.TableTypeId)
                .Distinct()
                .ToListAsync();

            return typeIds;
        }

        public async Task<bool> HasQuarterlyDataAsync(
            string databaseName, int securityId, int quarter, int templateId, bool isOriginal)
        {
            return await _context.WaitingFinancialTables
                .AnyAsync(w => w.DatabaseName == databaseName &&
                               w.SecurityId == securityId &&
                               w.Quarter == quarter &&
                               w.TemplateId == templateId &&
                               w.IsOriginal == isOriginal);
        }

        public async Task WriteAliveSqlAsync()
        {
            // Boş kalabilir şimdilik
            await Task.CompletedTask;
        }

        public async Task WriteHeartbeatAsync()
        {
            // Boş kalabilir şimdilik
            await Task.CompletedTask;
        }

        public async Task ExecuteSendAsync(
            string databaseName, int securityId, int quarter, int templateId,
            int disclosureId, bool isOriginal, List<int> tableTypeIds, bool isInflation)
        {
            // İtem'i sil
            var items = await _context.WaitingFinancialTables
                .Where(w => w.DatabaseName == databaseName &&
                            w.SecurityId == securityId &&
                            w.Quarter == quarter &&
                            w.TemplateId == templateId &&
                            w.IsOriginal == isOriginal)
                .ToListAsync();

            _context.WaitingFinancialTables.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetDisclosureQuarterAsync(
            int securityId, int disclosureId, string databaseName, int templateId)
        {
            return 1; // Dummy
        }

        public async Task<Dictionary<int, string>> GetSecurityCodesAsync(int[] securityIds)
        {
            var codes = await _context.WaitingFinancialTables
                .Where(w => securityIds.Contains(w.SecurityId))
                .Select(w => new { w.SecurityId, w.Username })
                .Distinct()
                .ToDictionaryAsync(x => x.SecurityId, x => x.Username ?? "UNKNOWN");

            return codes;
        }
    }
}
