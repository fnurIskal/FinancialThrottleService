using FinancialThrottle.Grpc;
using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Application.Logic;
using Grpc.Core;

namespace FinancialThrottle.Worker.Grpc;

public class ThrottleStatusService : ThrottleService.ThrottleServiceBase
{
    private readonly GroupRetryTracker _retryTracker;
    private readonly ILogger<ThrottleStatusService> _logger;
    private readonly IFinancialRepository _repository;

    public ThrottleStatusService(
        GroupRetryTracker retryTracker,
        ILogger<ThrottleStatusService> logger,
        IFinancialRepository repository)
    {
        _retryTracker = retryTracker;
        _logger = logger;
        _repository = repository;
    }

    public override Task<StatusResponse> GetStatus(
        StatusRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new StatusResponse
        {
            IsRunning = true,
            LastHeartbeatUtc = Worker.LastHeartbeat.ToString("O"),
            QueuedCount = 0,
            SuspendedCount = _retryTracker.GetSuspendedKeys().Count,
            ProcessedThisCycle = Worker.ProcessedThisCycle,
            WorkerStartedUtc = Worker.StartedAt.ToString("O")
        });
    }

    public override async Task<QueueResponse> GetQueue(
      QueueRequest request,
      ServerCallContext context)
    {
        var groups = await _repository.GetAllWaitingGroupsAsync();
        var response = new QueueResponse();

        foreach (var group in groups)
        {
            var groupMsg = new WaitingGroupMessage
            {
                DatabaseName = group.DatabaseName,
                SecurityId = group.SecurityId,
                TemplateId = group.TemplateId,
                SecurityCode = group.SecurityCode,
                ItemCount = group.Items.Count,
                PriorityScore = 0.0
            };

            foreach (var item in group.Items)
            {
                groupMsg.Items.Add(new WaitingItemMessage
                {
                    Quarter = item.Quarter,
                    IsOriginal = item.IsOriginal,
                    Username = item.Username,
                    DisclosureId = item.DisclosureId
                });
            }

            response.Groups.Add(groupMsg);
        }

        return response;
    }

    public override Task<SuspendedResponse> GetSuspended(
        SuspendedRequest request,
        ServerCallContext context)
    {
        var suspendedKeys = _retryTracker.GetSuspendedKeys();
        var response = new SuspendedResponse();

        foreach (var key in suspendedKeys)
        {
            response.Groups.Add(new SuspendedGroupMessage
            {
                DatabaseName = key.Split('|')[0],
                SecurityId = int.Parse(key.Split('|')[1]),
                TemplateId = int.Parse(key.Split('|')[2]),
                SecurityCode = key,
                FailureCount = 0,
                FirstFailedUtc = DateTime.UtcNow.ToString("O"),
                NextRetryUtc = DateTime.UtcNow.ToString("O"),
                RetryInProgress = false
            });
        }

        return Task.FromResult(response);
    }

    public override Task<RetryGroupResponse> RetryGroup(
        RetryGroupRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new RetryGroupResponse
        {
            Success = false,
            Message = "Retry henüz implement edilmedi"
        });
    }
}