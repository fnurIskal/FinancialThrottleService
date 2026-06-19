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

            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(_options.WorkerIntervalSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunCycleAsync(stoppingToken);
            }
        }

        private async Task RunCycleAsync(CancellationToken ct)
        {
            try
            {
                _logger.LogDebug("Cycle başladı → {Time}", DateTime.UtcNow);
                _processedThisCycle = 0;

                await using var scope = _scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IFinancialRepository>();
                var evaluator = scope.ServiceProvider.GetRequiredService<SendConditionEvaluator>();
                var msSourceIds = await repository.GetMsSourceIdsAsync();

                await repository.WriteAliveSqlAsync();

                await repository.WriteHeartbeatAsync();
                LastHeartbeat = DateTime.UtcNow;

                var allGroups = await repository.GetAllWaitingGroupsAsync();
                _logger.LogInformation("Kuyrukta {Count} grup bulundu", allGroups.Count);

                var duplicateMap = await repository.GetDuplicateItemCodeMapAsync();

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
                    var tasks = groups.Select(group =>
                        ProcessGroupAsync(group, isOriginal, _scopeFactory, evaluator,
                            msSourceIds, duplicateMap, ct));

                    await Task.WhenAll(tasks);
                }

                await TryProcessSuspendedGroupsAsync(scope, ct);

                _logger.LogDebug("Cycle bitti → İşlenen: {Count}", _processedThisCycle);
            }
            catch(Exception ex) {
                {
                    _logger.LogError(ex, "RunCycle fatal error");
                    throw;
                }
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

                    var requiredTypeIds = TemplateTableTypeConfig.Resolve(
                        group.DatabaseName, group.TemplateId, msSourceIds);

                    var condition = await evaluator.EvaluateAsync(
                        group, item, tableTypeIds, requiredTypeIds);

                    switch (condition)
                    {
                        case SendCondition.Send:
                            await SendGroupAsync(group, item, tableTypeIds,
                                repository, duplicateMap);
                            _retryTracker.RecordSuccess(group.GroupKey);
                            Interlocked.Increment(ref _processedThisCycle);

                        
                            var logEntry = new LogEntry
                            {
                                Timestamp = DateTime.UtcNow,
                                Level = "Information",
                                Category = "financial",
                                Message = $"GÖNDER: {group.GroupKey} Quarter={item.Quarter}",
                                GroupKey = group.GroupKey,
                                SecurityCode = group.SecurityCode,
                                DatabaseName = group.DatabaseName,
                                SecurityId = group.SecurityId,
                                TemplateId = group.TemplateId,
                                Quarter = item.Quarter
                            };
                            await _logRepository.WriteAsync(logEntry);
                            break;

                        case SendCondition.Wait:
                            _logger.LogDebug(
                                "{GroupKey} Quarter={Quarter} henüz hazır değil",
                                group.GroupKey, item.Quarter);
                            break;

                        case SendCondition.Skip:
                            _logger.LogDebug(
                                "{GroupKey} Quarter={Quarter} atlandı (boş kural)",
                                group.GroupKey, item.Quarter);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "{GroupKey} Quarter={Quarter} işlenirken hata",
                        group.GroupKey, item.Quarter);

                    bool suspended = _retryTracker.RecordFailure(group.GroupKey);
                    if (suspended)
                    {
                        _logger.LogWarning(
                            "{GroupKey} SUSPEND edildi", group.GroupKey);

                        await NotifySuspendAsync(scope: null, group);
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
                tableTypeIds, isInflation);

            // DuplicateMap kontrolü — ek templateId varsa onu da gönder
            if (duplicateMap.TryGetValue(
                (group.SecurityId, group.TemplateId), out int toTemplateId))
            {
                _logger.LogInformation(
                    "DuplicateMap → {GroupKey} toTemplateId={ToTemplateId}",
                    group.GroupKey, toTemplateId);

                await repository.ExecuteSendAsync(
                    group.DatabaseName, group.SecurityId, item.Quarter,
                    toTemplateId, item.DisclosureId, item.IsOriginal,
                    tableTypeIds, isInflation);
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

                
                    try
                    {
                        await ProcessGroupAsync(group, false, _scopeFactory, evaluator,
                            msSourceIds, duplicateMap, ct);

                        _retryTracker.RecordSuspendRetrySuccess(key);
                        _logger.LogInformation("{GroupKey} suspend retry başarılı", key);
                    }
                    catch (Exception ex)
                    {
                        _retryTracker.RecordSuspendRetryFailure(key);
                        _logger.LogError(ex, "{GroupKey} suspend retry başarısız", key);
                    }
                
        } }
        private async Task<Dictionary<string, double>> GetPriorityScoresAsync(
       AsyncServiceScope scope, List<WaitingGroup> groups)
        {
            var turkeyGroups = groups
                .Where(g => g.DatabaseName == _turkeyDb && !string.IsNullOrEmpty(g.SecurityCode))
                .Select(g => g.SecurityCode)
                .Distinct()
                .ToArray();

            if (turkeyGroups.Length == 0)
                return new Dictionary<string, double>();

            var client = scope.ServiceProvider.GetRequiredService<ISecurityPriorityClient>();
            return await client.GetPrioritiesAsync(turkeyGroups);
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Suspend bildirimi gönderilemedi");
            }
        }

    
        private static int _processedThisCycle;
        public static int ProcessedThisCycle => _processedThisCycle;
    }

}

public class ThrottleOptions
{
    public int MaxParallelGroups { get; set; } = 5;
    public int WorkerIntervalSeconds { get; set; } = 30;
    public bool UseDummyData { get; set; } = true;
}