using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Application.Logic;
using FinancialThrottleService.Domain.Models;
using Microsoft.Extensions.Options;


namespace FinancialThrottle.Worker
{
    public class Worker : BackgroundService
    {

        private readonly ILogger<Worker> _logger = null!;
        private readonly IServiceScopeFactory _scopeFactory = null!;
        private readonly GroupRetryTracker _retryTracker = null!;
        private readonly ThrottleOptions _options = null!;
        private readonly string _turkeyDb = null!;
        private readonly ILogRepository _logRepository;
        public static DateTime StartedAt { get; private set; }
        public static DateTime LastHeartbeat { get; private set; }


        public Worker(
      ILogger<Worker> logger,
      IServiceScopeFactory scopeFactory,
      GroupRetryTracker retryTracker,
      IOptions<ThrottleOptions> options,
      IConfiguration configuration, ILogRepository logRepository)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _retryTracker = retryTracker;
            _options = options.Value;
            _turkeyDb = configuration["DatabaseNames:Turkey"] ?? "RAS_STAJ107";
            _logRepository = logRepository;

            StartedAt = DateTime.UtcNow;
            LastHeartbeat = DateTime.UtcNow;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartedAt = DateTime.UtcNow;
            _logger.LogInformation("FinancialThrottleWorker başladı → {Time}", StartedAt);

            await _logRepository.WriteAsync(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = "Information",
                Category = "system",
                Message = $"FinancialThrottleWorker started. Interval={_options.WorkerIntervalSeconds}s MaxParallel={_options.MaxParallelGroups}"
            });

            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(_options.WorkerIntervalSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunCycleAsync(stoppingToken);
            }

            _logger.LogInformation("FinancialThrottleWorker durdu → {Time}", DateTime.UtcNow);
            await _logRepository.WriteAsync(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = "Information",
                Category = "system",
                Message = "FinancialThrottleWorker stopped (cancellation requested)"
            });
        }

        private async Task RunCycleAsync(CancellationToken ct)
        {
            var cycleStart = DateTime.UtcNow;

            try
            {
                _logger.LogDebug("Cycle başladı → {Time}", cycleStart);
                _processedThisCycle = 0;

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = cycleStart,
                    Level = "Debug",
                    Category = "system",
                    Message = $"Cycle started at {cycleStart:HH:mm:ss}"
                });

                await using var scope = _scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IFinancialRepository>();
                var evaluator = scope.ServiceProvider.GetRequiredService<SendConditionEvaluator>();
                var msSourceIds = await repository.GetMsSourceIdsAsync();

                await repository.WriteAliveSqlAsync();

                _logger.LogDebug("WriteAliveSql tamamlandı");
                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Debug",
                    Category = "system",
                    Message = "WriteAliveSql completed"
                });

                await repository.WriteHeartbeatAsync();
                LastHeartbeat = DateTime.UtcNow;

                _logger.LogDebug("Heartbeat yazıldı → {Time}", LastHeartbeat);
                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = LastHeartbeat,
                    Level = "Information",
                    Category = "system",
                    Message = $"Heartbeat written at {LastHeartbeat:HH:mm:ss}"
                });

                var allGroups = await repository.GetAllWaitingGroupsAsync();
                Interlocked.Exchange(ref _lastQueuedCount, allGroups.Count);
                _logger.LogInformation("Kuyrukta {Count} grup bulundu", allGroups.Count);

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Information",
                    Category = "financial",
                    Message = $"Queue poll: {allGroups.Count} waiting group(s) found"
                });

                var duplicateMap = await repository.GetDuplicateItemCodeMapAsync();

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Debug",
                    Category = "consistency",
                    Message = $"DuplicateItemCodeMap loaded: {duplicateMap.Count} mapping(s)"
                });

                var priorityScores = await GetPriorityScoresAsync(scope, allGroups);

                for (int i = 0; i <= 1; i++)
                {
                    bool isOriginal = i == 1;

                    var groups = allGroups
                        .Where(g => g.Items.Any(item => item.IsOriginal == isOriginal))
                        .Where(g => !_retryTracker.IsSuspended(g.GroupKey))
                        .OrderByDescending(g => priorityScores.GetValueOrDefault(g.SecurityCode, 0.0))
                        .Take(_options.MaxParallelGroups)
                        .ToList();

                    if (groups.Count > 0)
                    {
                        _logger.LogDebug(
                            "IsOriginal={IsOriginal}: {Count} grup işlenecek",
                            isOriginal, groups.Count);

                        await _logRepository.WriteAsync(new LogEntry
                        {
                            Timestamp = DateTime.UtcNow,
                            Level = "Debug",
                            Category = "financial",
                            Message = $"Processing {groups.Count} group(s) with IsOriginal={isOriginal}"
                        });
                    }

                    var tasks = groups.Select(group =>
                        ProcessGroupAsync(group, isOriginal, _scopeFactory, evaluator,
                            msSourceIds, duplicateMap, ct));

                    await Task.WhenAll(tasks);
                }

                await TryProcessSuspendedGroupsAsync(scope, ct);

                var cycleMs = (DateTime.UtcNow - cycleStart).TotalMilliseconds;
                _logger.LogDebug("Cycle bitti → İşlenen: {Count}", _processedThisCycle);

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Debug",
                    Category = "system",
                    Message = $"Cycle completed in {cycleMs:F0}ms. Processed={_processedThisCycle}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RunCycle fatal error");

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Error",
                    Category = "system",
                    Message = $"RunCycle fatal error: {ex.Message}",
                    Exception = ex.ToString()
                });

                throw;
            }
        }

        private async Task ProcessGroupAsync(
       WaitingGroup group,
       bool isOriginal,
       IServiceScopeFactory scopeFactory,
       SendConditionEvaluator evaluator,
       int[] msSourceIds,
       Dictionary<(int, int), int> duplicateMap,
       CancellationToken ct)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IFinancialRepository>();

            var items = group.Items
                .Where(i => i.IsOriginal == isOriginal)
                .ToList();

            foreach (var item in items)
            {
                if (ct.IsCancellationRequested) return;

                try
                {
                    var tableTypeIds = await repository.GetTableTypeIdsAsync(
                        group.DatabaseName, group.SecurityId,
                        item.Quarter, group.TemplateId, item.IsOriginal);

                    _logger.LogDebug(
                        "{GroupKey} Quarter={Quarter} → TableTypeIds=[{Ids}]",
                        group.GroupKey, item.Quarter, string.Join(",", tableTypeIds));

                    await _logRepository.WriteAsync(new LogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Level = "Debug",
                        Category = "consistency",
                        Message = $"GetTableTypeIds: [{string.Join(",", tableTypeIds)}] for {group.GroupKey} Quarter={item.Quarter}",
                        GroupKey = group.GroupKey,
                        SecurityCode = group.SecurityCode,
                        DatabaseName = group.DatabaseName,
                        SecurityId = group.SecurityId,
                        TemplateId = group.TemplateId,
                        Quarter = item.Quarter
                    });

                    var requiredTypeIds = TemplateTableTypeConfig.Resolve(
                        group.DatabaseName, group.TemplateId, msSourceIds);

                    _logger.LogDebug(
                        "{GroupKey} Quarter={Quarter} → Required=[{Ids}]",
                        group.GroupKey, item.Quarter, string.Join(",", requiredTypeIds));

                    await _logRepository.WriteAsync(new LogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Level = "Debug",
                        Category = "consistency",
                        Message = $"TemplateTableTypeConfig.Resolve: Required=[{string.Join(",", requiredTypeIds)}] for {group.GroupKey} TemplateId={group.TemplateId}",
                        GroupKey = group.GroupKey,
                        SecurityCode = group.SecurityCode,
                        DatabaseName = group.DatabaseName,
                        SecurityId = group.SecurityId,
                        TemplateId = group.TemplateId,
                        Quarter = item.Quarter
                    });

                    var condition = await evaluator.EvaluateAsync(
                        group, item, tableTypeIds, requiredTypeIds);

                    switch (condition)
                    {
                        case SendCondition.Send:
                            await SendGroupAsync(group, item, tableTypeIds,
                                repository, duplicateMap);
                            _retryTracker.RecordSuccess(group.GroupKey);
                            Interlocked.Increment(ref _processedThisCycle);

                            await _logRepository.WriteAsync(new LogEntry
                            {
                                Timestamp = DateTime.UtcNow,
                                Level = "Information",
                                Category = "financial",
                                Message = $"SENT: {group.GroupKey} Quarter={item.Quarter} IsOriginal={item.IsOriginal}",
                                GroupKey = group.GroupKey,
                                SecurityCode = group.SecurityCode,
                                DatabaseName = group.DatabaseName,
                                SecurityId = group.SecurityId,
                                TemplateId = group.TemplateId,
                                Quarter = item.Quarter
                            });
                            break;

                        case SendCondition.Wait:
                            _logger.LogDebug(
                                "{GroupKey} Quarter={Quarter} henüz hazır değil",
                                group.GroupKey, item.Quarter);

                            await _logRepository.WriteAsync(new LogEntry
                            {
                                Timestamp = DateTime.UtcNow,
                                Level = "Debug",
                                Category = "consistency",
                                Message = $"WAIT: {group.GroupKey} Quarter={item.Quarter} — not all required TableTypeIds present",
                                GroupKey = group.GroupKey,
                                SecurityCode = group.SecurityCode,
                                DatabaseName = group.DatabaseName,
                                SecurityId = group.SecurityId,
                                TemplateId = group.TemplateId,
                                Quarter = item.Quarter
                            });
                            break;

                        case SendCondition.Skip:
                            _logger.LogDebug(
                                "{GroupKey} Quarter={Quarter} atlandı (boş kural)",
                                group.GroupKey, item.Quarter);

                            await _logRepository.WriteAsync(new LogEntry
                            {
                                Timestamp = DateTime.UtcNow,
                                Level = "Debug",
                                Category = "consistency",
                                Message = $"SKIP: {group.GroupKey} Quarter={item.Quarter} — no config rule matched",
                                GroupKey = group.GroupKey,
                                SecurityCode = group.SecurityCode,
                                DatabaseName = group.DatabaseName,
                                SecurityId = group.SecurityId,
                                TemplateId = group.TemplateId,
                                Quarter = item.Quarter
                            });
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "{GroupKey} Quarter={Quarter} işlenirken hata",
                        group.GroupKey, item.Quarter);

                    await _logRepository.WriteAsync(new LogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Level = "Error",
                        Category = "system",
                        Message = $"ProcessGroup error: {group.GroupKey} Quarter={item.Quarter} — {ex.Message}",
                        GroupKey = group.GroupKey,
                        SecurityCode = group.SecurityCode,
                        DatabaseName = group.DatabaseName,
                        SecurityId = group.SecurityId,
                        TemplateId = group.TemplateId,
                        Quarter = item.Quarter,
                        Exception = ex.ToString()
                    });

                    bool suspended = _retryTracker.RecordFailure(group.GroupKey);
                    if (suspended)
                    {
                        _logger.LogWarning("{GroupKey} SUSPEND edildi", group.GroupKey);

                        await _logRepository.WriteAsync(new LogEntry
                        {
                            Timestamp = DateTime.UtcNow,
                            Level = "Warning",
                            Category = "system",
                            Message = $"SUSPENDED: {group.GroupKey} after 10 consecutive failures",
                            GroupKey = group.GroupKey,
                            SecurityCode = group.SecurityCode,
                            DatabaseName = group.DatabaseName,
                            SecurityId = group.SecurityId,
                            TemplateId = group.TemplateId
                        });

                        await NotifySuspendAsync(scope, group);
                    }
                }
            }
        }

        private async Task SendGroupAsync(
       WaitingGroup group,
       WaitingItem item,
       List<int> tableTypeIds,
       IFinancialRepository repository,
       Dictionary<(int, int), int> duplicateMap)
        {
            bool isInflation = TemplateTableTypeConfig.IsInflationTemplate(group.TemplateId);

            _logger.LogInformation(
                "GÖNDER → {GroupKey} Quarter={Quarter} IsOriginal={IsOriginal} Inflation={Inflation}",
                group.GroupKey, item.Quarter, item.IsOriginal, isInflation);

            await repository.ExecuteSendAsync(
                group.DatabaseName, group.SecurityId, item.Quarter,
                group.TemplateId, item.DisclosureId, item.IsOriginal,
                tableTypeIds, isInflation, item.SendEmail);

            // DuplicateMap kontrolü — ek templateId varsa onu da gönder
            if (duplicateMap.TryGetValue(
                (group.SecurityId, group.TemplateId), out int toTemplateId))
            {
                _logger.LogInformation(
                    "DuplicateMap → {GroupKey} toTemplateId={ToTemplateId}",
                    group.GroupKey, toTemplateId);

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Information",
                    Category = "financial",
                    Message = $"DuplicateMap send: {group.GroupKey} → TemplateId={toTemplateId} Quarter={item.Quarter}",
                    GroupKey = group.GroupKey,
                    SecurityCode = group.SecurityCode,
                    DatabaseName = group.DatabaseName,
                    SecurityId = group.SecurityId,
                    TemplateId = toTemplateId,
                    Quarter = item.Quarter
                });

                await repository.ExecuteSendAsync(
                    group.DatabaseName, group.SecurityId, item.Quarter,
                    toTemplateId, item.DisclosureId, item.IsOriginal,
                    tableTypeIds, isInflation, item.SendEmail);
            }
        }

        private async Task TryProcessSuspendedGroupsAsync(
        AsyncServiceScope scope, CancellationToken ct)
        {
            var dueKeys = _retryTracker.GetSuspendedKeys()
                .Where(_retryTracker.IsRetryDue)
                .ToList();

            if (dueKeys.Count == 0) return;

            _logger.LogInformation(
                "{Count} suspend grup retry zamanı geldi", dueKeys.Count);

            await _logRepository.WriteAsync(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = "Information",
                Category = "system",
                Message = $"{dueKeys.Count} suspended group(s) due for retry: [{string.Join(", ", dueKeys)}]"
            });

            var repository = scope.ServiceProvider.GetRequiredService<IFinancialRepository>();
            var evaluator = scope.ServiceProvider.GetRequiredService<SendConditionEvaluator>();
            var msSourceIds = await repository.GetMsSourceIdsAsync();
            var duplicateMap = await repository.GetDuplicateItemCodeMapAsync();
            var allGroups = await repository.GetAllWaitingGroupsAsync();

            foreach (var key in dueKeys)
            {
                if (ct.IsCancellationRequested) return;

                var group = allGroups.FirstOrDefault(g => g.GroupKey == key);
                if (group is null) continue;

                _retryTracker.BeginSuspendRetry(key);

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Information",
                    Category = "system",
                    Message = $"Suspend retry attempt: {key}",
                    GroupKey = key,
                    SecurityCode = group.SecurityCode,
                    DatabaseName = group.DatabaseName,
                    SecurityId = group.SecurityId,
                    TemplateId = group.TemplateId
                });

                try
                {
                    await ProcessGroupAsync(group, false, _scopeFactory, evaluator,
                        msSourceIds, duplicateMap, ct);

                    _retryTracker.RecordSuspendRetrySuccess(key);
                    _logger.LogInformation("{GroupKey} suspend retry başarılı", key);

                    await _logRepository.WriteAsync(new LogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Level = "Information",
                        Category = "system",
                        Message = $"Suspend retry succeeded: {key}",
                        GroupKey = key,
                        SecurityCode = group.SecurityCode,
                        DatabaseName = group.DatabaseName,
                        SecurityId = group.SecurityId,
                        TemplateId = group.TemplateId
                    });
                }
                catch (Exception ex)
                {
                    _retryTracker.RecordSuspendRetryFailure(key);
                    _logger.LogError(ex, "{GroupKey} suspend retry başarısız", key);

                    await _logRepository.WriteAsync(new LogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Level = "Error",
                        Category = "system",
                        Message = $"Suspend retry failed: {key} — {ex.Message}",
                        GroupKey = key,
                        SecurityCode = group.SecurityCode,
                        DatabaseName = group.DatabaseName,
                        SecurityId = group.SecurityId,
                        TemplateId = group.TemplateId,
                        Exception = ex.ToString()
                    });
                }
            }
        }

        private async Task<Dictionary<string, double>> GetPriorityScoresAsync(
       AsyncServiceScope scope, List<WaitingGroup> groups)
        {
            var turkeyGroups = groups
                .Where(g => g.DatabaseName == _turkeyDb && !string.IsNullOrEmpty(g.SecurityCode))
                .Select(g => g.SecurityCode)
                .Distinct()
                .ToArray();

            if (turkeyGroups.Length == 0)
            {
                _logger.LogDebug("Priority API atlandı — Turkey grubu yok");
                return new Dictionary<string, double>();
            }

            _logger.LogDebug(
                "Priority API çağrılıyor → {Count} security code: [{Codes}]",
                turkeyGroups.Length, string.Join(",", turkeyGroups));

            await _logRepository.WriteAsync(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = "Information",
                Category = "api",
                Message = $"GetPriorityScores: calling API for {turkeyGroups.Length} code(s): [{string.Join(", ", turkeyGroups)}]"
            });

            try
            {
                var client = scope.ServiceProvider.GetRequiredService<ISecurityPriorityClient>();
                var scores = await client.GetPrioritiesAsync(turkeyGroups);

                _logger.LogDebug(
                    "Priority API yanıtı → {Count} skor alındı",
                    scores.Count);

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Information",
                    Category = "api",
                    Message = $"GetPriorityScores: received {scores.Count} score(s) from API"
                });

                return scores;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Priority API çağrısı başarısız — sıfır skor ile devam edildi");

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Warning",
                    Category = "api",
                    Message = $"GetPriorityScores API call failed: {ex.Message} — falling back to zero scores",
                    Exception = ex.ToString()
                });

                return new Dictionary<string, double>();
            }
        }

        private async Task NotifySuspendAsync(AsyncServiceScope? scope, WaitingGroup group)
        {
            try
            {
                if (scope is null) return;
                var emailQueue = scope.Value.ServiceProvider
                    .GetRequiredService<IEmailQueueRepository>();

                await emailQueue.EnqueueAsync(
                    subject: $"[SUSPEND] {group.GroupKey}",
                    body: $"{group.GroupKey} 10 başarısız denemeden sonra askıya alındı.",
                    recipients: new[] { "admin@example.com" });

                _logger.LogInformation(
                    "Suspend e-posta kuyruğa eklendi → {GroupKey}", group.GroupKey);

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Information",
                    Category = "api",
                    Message = $"Suspend notification email enqueued for {group.GroupKey}",
                    GroupKey = group.GroupKey,
                    SecurityCode = group.SecurityCode,
                    DatabaseName = group.DatabaseName,
                    SecurityId = group.SecurityId,
                    TemplateId = group.TemplateId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Suspend bildirimi gönderilemedi");

                await _logRepository.WriteAsync(new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Error",
                    Category = "api",
                    Message = $"Failed to enqueue suspend notification email for {group.GroupKey}: {ex.Message}",
                    GroupKey = group.GroupKey,
                    SecurityCode = group.SecurityCode,
                    DatabaseName = group.DatabaseName,
                    SecurityId = group.SecurityId,
                    TemplateId = group.TemplateId,
                    Exception = ex.ToString()
                });
            }
        }


        private static int _processedThisCycle;
        public static int ProcessedThisCycle => _processedThisCycle;

        private static int _lastQueuedCount;
        public static int LastQueuedCount => _lastQueuedCount;
    }

}

public class ThrottleOptions
{
    public int MaxParallelGroups { get; set; } = 5;
    public int WorkerIntervalSeconds { get; set; } = 30;
    public bool UseDummyData { get; set; } = true;
}
