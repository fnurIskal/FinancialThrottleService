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

        public async Task<int> GetDisclosureQuarterAsync(
    int securityId, int disclosureId, string databaseName, int templateId)
        {
            try
            {
                var disclosure = await _context.FinancialTraces
                    .FirstOrDefaultAsync(f =>
                        f.SecurityId == securityId &&
                        f.DisclosureId == disclosureId);

                if (disclosure == null)
                {
                    Console.WriteLine($"[WARN] No FinancialTrace found");
                    return 1;
                }

                return disclosure.Quarter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return 1;
            }
        }

        public async Task ExecuteSendAsync(
        string databaseName, int securityId, int quarter, int templateId,
        int disclosureId, bool isOriginal, List<int> tableTypeIds, bool isInflation)
        {
            try
            {
                Console.WriteLine($"[INFO] ExecuteSendAsync: Preparing to send data");
                Console.WriteLine($"  DatabaseName: {databaseName}");
                Console.WriteLine($"  SecurityId: {securityId}");
                Console.WriteLine($"  Quarter: {quarter}");
                Console.WriteLine($"  TemplateId: {templateId}");
                Console.WriteLine($"  DisclosureId: {disclosureId}");
                Console.WriteLine($"  IsOriginal: {isOriginal}");
                Console.WriteLine($"  TableTypeIds: {string.Join(",", tableTypeIds)}");
                Console.WriteLine($"  IsInflation: {isInflation}");

                // TODO: Step 1: FinancialTransactionApi'ye veri gönder
                // var apiResult = await _financialTransactionApi.SendDataAsync(...);

                // TODO: Step 2: Email queue'ya ekle (sendEmail = true ise)
                // await _emailQueueRepository.EnqueueAsync(...);

                // TODO: Step 3: Başarılı olduktan sonra sadece sil
                // var items = await _context.WaitingFinancialTables
                //     .Where(w => w.DatabaseName == databaseName && ...)
                //     .ToListAsync();
                // _context.WaitingFinancialTables.RemoveRange(items);
                // await _context.SaveChangesAsync();

                Console.WriteLine($"[INFO] ExecuteSendAsync: Data prepared (not sent yet - testing phase)");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ExecuteSendAsync failed: {ex.Message}");
                throw;
            }
        }

        public async Task WriteHeartbeatAsync()
        {
            try
            {
                Console.WriteLine($"[DEBUG] WriteHeartbeatAsync called at {DateTime.UtcNow}");

                // TODO: Heartbeat tablosuna kayıt yaz veya log dosyasına yaz
                // Örnek: worker'ın hala çalıştığını göstermek için

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] WriteHeartbeatAsync failed: {ex.Message}");
            }
        }

        public async Task<Dictionary<int, string>> GetSecurityCodesAsync(int[] securityIds)
        {
            try
            {
                var codes = await _context.WaitingFinancialTables
                    .Where(w => securityIds.Contains(w.SecurityId))
                    .Select(w => new { w.SecurityId, w.Username })
                    .Distinct()
                    .ToListAsync();

                var result = codes.ToDictionary(x => x.SecurityId, x => x.Username ?? "UNKNOWN");

                Console.WriteLine($"[DEBUG] GetSecurityCodesAsync returning {result.Count} codes");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetSecurityCodesAsync failed: {ex.Message}");
                return new Dictionary<int, string>();
            }
        }
        public async Task<Dictionary<(int securityId, int templateId), int>> GetDuplicateItemCodeMapAsync()
        {
            try
            {
                var map = await _context.DuplicateTableTemplateMaps
                    .Where(d => d.FromTemplateId.HasValue && d.ToTemplateId.HasValue)
                    .Select(d => new
                    {
                        d.SecurityId,
                        d.FromTemplateId,
                        d.ToTemplateId
                    })
                    .ToListAsync();

                var result = map.ToDictionary(
                    x => (x.SecurityId, x.FromTemplateId.Value),
                    x => x.ToTemplateId.Value);

                Console.WriteLine($"[DEBUG] GetDuplicateItemCodeMapAsync returning {result.Count} mappings");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetDuplicateItemCodeMapAsync failed: {ex.Message}");
                return new Dictionary<(int, int), int>();
            }
        }

        public async Task<int[]> GetMsSourceIdsAsync()
        {
            try
            {
                // TODO: Gerçek implementasyon: ConfigurationTable'dan oku
                var msSourceIds = new[] { 32501 };

                Console.WriteLine($"[DEBUG] GetMsSourceIdsAsync returning: {string.Join(",", msSourceIds)}");
                return await Task.FromResult(msSourceIds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMsSourceIdsAsync failed: {ex.Message}");
                return new[] { 32501 }; // Default fallback
            }
        }
        public async Task<bool> HasQuarterlyDataAsync(
                   string databaseName, int securityId, int quarter, int templateId, bool isOriginal)
        {
            try
            {
                var exists = await _context.WaitingFinancialTables
                    .AnyAsync(w => w.DatabaseName == databaseName &&
                                   w.SecurityId == securityId &&
                                   w.Quarter == quarter &&
                                   w.TemplateId == templateId &&
                                   w.IsOriginal == isOriginal);

                Console.WriteLine($"[DEBUG] HasQuarterlyDataAsync: {databaseName}/{securityId}/Q{quarter}/T{templateId} = {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] HasQuarterlyDataAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task WriteAliveSqlAsync()
        {
            try
            {
                Console.WriteLine($"[DEBUG] WriteAliveSqlAsync called at {DateTime.UtcNow}");

                // TODO: Alive tablosuna kayıt yaz veya güncelle
                // Örnek: LastAliveTime = DateTime.UtcNow

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] WriteAliveSqlAsync failed: {ex.Message}");
            }
        }
    }
}
