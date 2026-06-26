using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Domain.Models;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS107;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS32501;
using Microsoft.EntityFrameworkCore;

namespace FinancialThrottleService.Infrastructure.Persistence.Sql
{
    public class SqlFinancialRepository : IFinancialRepository
    {
        private readonly RasStajContext _context;
        private readonly RasStaj107Context _context107;
        private readonly RasStaj32501Context _context32501;
        private readonly IEmailQueueRepository _emailQueueRepository;
        private readonly IFinancialTransactionApi _financialTransactionApi;
        private readonly ILogRepository _logRepository;

        public SqlFinancialRepository(
            RasStajContext context,
            RasStaj107Context context107,
            RasStaj32501Context context32501,
            IEmailQueueRepository emailQueueRepository,
            IFinancialTransactionApi financialTransactionApi,
            ILogRepository logRepository)
        {
            _context = context;
            _context107 = context107;
            _context32501 = context32501;
            _emailQueueRepository = emailQueueRepository;
            _financialTransactionApi = financialTransactionApi;
            _logRepository = logRepository;
        }

        private static bool IsTurkeyDb(string dbName) =>
            dbName.Contains("107", StringComparison.OrdinalIgnoreCase) ||
            dbName.Equals("RAS_101", StringComparison.OrdinalIgnoreCase) ||
            dbName.Equals("RAS_1", StringComparison.OrdinalIgnoreCase);

        private static bool IsMsSourceDb(string dbName) =>
            dbName.Contains("32501", StringComparison.OrdinalIgnoreCase);

        public async Task<List<WaitingGroup>> GetAllWaitingGroupsAsync()
        {
            // Step 1: Load all queue rows then group in memory
            var allRows = await _context.WaitingFinancialTables.ToListAsync();

            var rawGroups = allRows
                .GroupBy(w => new { w.DatabaseName, w.SecurityId, w.TemplateId })
                .Select(g => new
                {
                    g.Key.DatabaseName,
                    g.Key.SecurityId,
                    g.Key.TemplateId,
                    Items = g.Select(w => new WaitingItem
                    {
                        Quarter = w.Quarter,
                        IsOriginal = w.IsOriginal,
                        Username = w.Username ?? "",
                        DisclosureId = w.DisclosureId,
                        SendEmail = w.SendEmail ?? false
                    }).ToList()
                })
                .ToList();

            Console.WriteLine($"[DEBUG] WaitingFinancialTables: {allRows.Count} rows → {rawGroups.Count} groups");

            // Step 2a: Resolve OrderType from TableType table
            var templateIds = rawGroups.Select(r => r.TemplateId).Distinct().ToArray();
            var orderTypeMap = await _context.TableTypes
                .Where(t => templateIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Order ?? 0);

            // Step 2: Resolve SecurityCode from the correct Security table
            var turkey107Ids = rawGroups
                .Where(r => IsTurkeyDb(r.DatabaseName))
                .Select(r => r.SecurityId).Distinct().ToArray();

            var ms32501Ids = rawGroups
                .Where(r => IsMsSourceDb(r.DatabaseName))
                .Select(r => r.SecurityId).Distinct().ToArray();

            Dictionary<int, string> turkey107Codes = new();
            if (turkey107Ids.Length > 0)
                turkey107Codes = await _context107.Securities
                    .Where(s => turkey107Ids.Contains(s.Id) && s.Code != null)
                    .ToDictionaryAsync(s => s.Id, s => s.Code!);

            Dictionary<int, string> ms32501Codes = new();
            if (ms32501Ids.Length > 0)
                ms32501Codes = await _context32501.Securities
                    .Where(s => ms32501Ids.Contains(s.Id) && s.Code != null)
                    .ToDictionaryAsync(s => s.Id, s => s.Code!);

            // Step 3: Build final WaitingGroup list with resolved SecurityCode
            return rawGroups.Select(r =>
            {
                var codeLookup =
                    IsTurkeyDb(r.DatabaseName) ? turkey107Codes :
                    IsMsSourceDb(r.DatabaseName) ? ms32501Codes :
                    new Dictionary<int, string>();

                return new WaitingGroup
                {
                    DatabaseName = r.DatabaseName,
                    SecurityId = r.SecurityId,
                    TemplateId = r.TemplateId,
                    SecurityCode = codeLookup.GetValueOrDefault(r.SecurityId, $"SEC_{r.SecurityId}"),
                    Items = r.Items,
                    OrderType = orderTypeMap.GetValueOrDefault(r.TemplateId, 0)
                };
            }).ToList();
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
                    Console.WriteLine($"[WARN] No FinancialTrace found for securityId={securityId} disclosureId={disclosureId}");
                    return 1;
                }

                return disclosure.Quarter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetDisclosureQuarterAsync failed: {ex.Message}");
                return 1;
            }
        }

        public async Task ExecuteSendAsync(
            string databaseName, int securityId, int quarter, int templateId,
            int disclosureId, bool isOriginal, List<int> tableTypeIds, bool isInflation,
            bool sendEmail)
        {
            Console.WriteLine($"[INFO] ExecuteSendAsync: {databaseName}/{securityId}/Q{quarter}/T{templateId} " +
                              $"TypeIds=[{string.Join(",", tableTypeIds)}] Inflation={isInflation} SendEmail={sendEmail}");
            try
            {
                // Step 1: Fetch rows (and their SQL commands) from the queue
                var rows = await _context.WaitingFinancialTables
                    .Where(w => w.DatabaseName == databaseName &&
                                w.SecurityId == securityId &&
                                w.Quarter == quarter &&
                                w.TemplateId == templateId &&
                                w.IsOriginal == isOriginal &&
                                w.DisclosureId == disclosureId &&
                                tableTypeIds.Contains(w.TableTypeId))
                    .ToListAsync();

                var commands = string.Join("\n", rows
                    .Where(r => !string.IsNullOrWhiteSpace(r.Sql))
                    .Select(r => r.Sql!));

                Console.WriteLine($"[INFO] ExecuteSendAsync: {rows.Count} row(s) found, SQL length={commands.Length}");

                // Step 2: Call FinancialTransactionApi (Inflation or Restated)
                string apiResult = isInflation
                    ? await _financialTransactionApi.GenerateInflationAsync(
                        databaseName, quarter, securityId, templateId, commands)
                    : await _financialTransactionApi.GenerateRestatedAsync(
                        databaseName, quarter, securityId, templateId, commands);

                Console.WriteLine($"[INFO] ExecuteSendAsync: API result → {apiResult}");

                // Step 3: Notify via email
                if (sendEmail)
                {
                    var subject = $"Financial Data Sent: {databaseName} / SecurityId={securityId} / Q{quarter}";
                    var body = $"DatabaseName: {databaseName}\nSecurityId: {securityId}\nQuarter: {quarter}\n" +
                               $"TemplateId: {templateId}\nDisclosureId: {disclosureId}\nIsOriginal: {isOriginal}\n" +
                               $"TableTypeIds: [{string.Join(",", tableTypeIds)}]\nAPI Result: {apiResult}";

                    await _emailQueueRepository.EnqueueAsync(subject, body, new[] { "admin@example.com" });
                    Console.WriteLine($"[INFO] EmailQueue: enqueued for {databaseName}/{securityId}/Q{quarter}");
                }

                // Step 4: Remove processed rows from queue
                _context.WaitingFinancialTables.RemoveRange(rows);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[INFO] ExecuteSendAsync: removed {rows.Count} row(s) from WaitingFinancialTables");

                // Step 5: Write audit log to MongoDB
                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Information",
                    Category = "financial",
                    Message = $"Send completed: {databaseName}/{securityId}/Q{quarter}/T{templateId} " +
                              $"IsOriginal={isOriginal} Inflation={isInflation} Rows={rows.Count}",
                    DatabaseName = databaseName,
                    SecurityId = securityId,
                    TemplateId = templateId,
                    Quarter = quarter,
                    Metadata = new
                    {
                        DisclosureId = disclosureId,
                        IsOriginal = isOriginal,
                        IsInflation = isInflation,
                        TableTypeIds = tableTypeIds,
                        RowsRemoved = rows.Count,
                        ApiResult = apiResult
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ExecuteSendAsync failed: {ex.Message}");

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Error",
                    Category = "financial",
                    Message = $"Send failed: {databaseName}/{securityId}/Q{quarter}/T{templateId} — {ex.Message}",
                    DatabaseName = databaseName,
                    SecurityId = securityId,
                    TemplateId = templateId,
                    Quarter = quarter,
                    Exception = ex.ToString()
                });

                throw;
            }
        }

        public async Task WriteHeartbeatAsync()
        {
            try
            {
                const string paramName = "FinancialThrottle_Heartbeat";
                var param = await _context.UtTrcTracerParams
                    .FirstOrDefaultAsync(p => p.ParamName == paramName);

                if (param == null)
                {
                    _context.UtTrcTracerParams.Add(new UtTrcTracerParam
                    {
                        ParamName = paramName,
                        StrValue = DateTime.UtcNow.ToString("o")
                    });
                }
                else
                {
                    param.StrValue = DateTime.UtcNow.ToString("o");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[DEBUG] WriteHeartbeatAsync: written at {DateTime.UtcNow:HH:mm:ss}");
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
                var codes107 = await _context107.Securities
                    .Where(s => securityIds.Contains(s.Id) && s.Code != null)
                    .ToDictionaryAsync(s => s.Id, s => s.Code!);

                var codes32501 = await _context32501.Securities
                    .Where(s => securityIds.Contains(s.Id) && s.Code != null && !codes107.ContainsKey(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Code!);

                var result = codes107
                    .Concat(codes32501)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

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
                    .Select(d => new { d.SecurityId, d.FromTemplateId, d.ToTemplateId })
                    .ToListAsync();

                var result = map.ToDictionary(
                    x => (x.SecurityId, x.FromTemplateId!.Value),
                    x => x.ToTemplateId!.Value);

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
                var param = await _context.UtTrcTracerParams
                    .FirstOrDefaultAsync(p => p.ParamName == "ms_source_ids");

                if (param?.StrValue != null)
                {
                    var ids = param.StrValue
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var id) ? id : -1)
                        .Where(id => id > 0)
                        .ToArray();

                    if (ids.Length > 0)
                    {
                        Console.WriteLine($"[DEBUG] GetMsSourceIdsAsync: {string.Join(",", ids)}");
                        return ids;
                    }
                }

                Console.WriteLine("[WARN] GetMsSourceIdsAsync: 'ms_source_ids' not found, using fallback {32501}");
                return new[] { 32501 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMsSourceIdsAsync failed: {ex.Message}");
                return new[] { 32501 };
            }
        }

        public async Task<bool> HasQuarterlyDataAsync(
            string databaseName, int securityId, int quarter, int templateId, bool isOriginal)
        {
            try
            {
                bool exists;

                if (IsTurkeyDb(databaseName))
                {
                    exists = isOriginal
                        ? await _context107.QuarterlyOriginals.AnyAsync(q =>
                            q.SecurityId == securityId && q.Quarter == quarter && q.TemplateId == templateId)
                        : await _context107.Quarterlies.AnyAsync(q =>
                            q.SecurityId == securityId && q.Quarter == quarter && q.TemplateId == templateId);
                }
                else if (IsMsSourceDb(databaseName))
                {
                    exists = isOriginal
                        ? await _context32501.QuarterlyOriginals.AnyAsync(q =>
                            q.SecurityId == securityId && q.Quarter == quarter && q.TemplateId == templateId)
                        : await _context32501.Quarterlies.AnyAsync(q =>
                            q.SecurityId == securityId && q.Quarter == quarter && q.TemplateId == templateId);
                }
                else
                {
                    Console.WriteLine($"[WARN] HasQuarterlyDataAsync: no Quarterly context for db={databaseName}");
                    return false;
                }

                Console.WriteLine($"[DEBUG] HasQuarterlyDataAsync: {databaseName}/{securityId}/Q{quarter}/T{templateId} isOriginal={isOriginal} → {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] HasQuarterlyDataAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task DeleteFromQueueAsync(
            string databaseName, int securityId, int quarter, int templateId,
            int disclosureId, bool isOriginal, List<int> tableTypeIds)
        {
            var rows = await _context.WaitingFinancialTables
                .Where(w => w.DatabaseName == databaseName &&
                            w.SecurityId == securityId &&
                            w.Quarter == quarter &&
                            w.TemplateId == templateId &&
                            w.IsOriginal == isOriginal &&
                            w.DisclosureId == disclosureId &&
                            tableTypeIds.Contains(w.TableTypeId))
                .ToListAsync();

            _context.WaitingFinancialTables.RemoveRange(rows);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[INFO] DeleteFromQueueAsync: removed {rows.Count} row(s) for {databaseName}/{securityId}/Q{quarter}/T{templateId}");
        }

        public async Task WriteAliveSqlAsync()
        {
            try
            {
                Console.WriteLine($"[DEBUG] WriteAliveSqlAsync called at {DateTime.UtcNow:HH:mm:ss}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] WriteAliveSqlAsync failed: {ex.Message}");
            }
        }
    }
}
