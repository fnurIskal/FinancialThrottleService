using FinancialThrottleService.Application.Logic;
using FinancialThrottle.Grpc;
using Grpc.Core;

namespace FinancialThrottle.Worker.Grpc;

public class ThrottleStatusService : ThrottleService.ThrottleServiceBase
{
    private readonly GroupRetryTracker _retryTracker;
    private readonly ILogger<ThrottleStatusService> _logger;

    public ThrottleStatusService(
        GroupRetryTracker retryTracker,
        ILogger<ThrottleStatusService> logger)
    {
        _retryTracker = retryTracker;
        _logger = logger;
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

    public override Task<QueueResponse> GetQueue(
        QueueRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new QueueResponse());
    }

    public override Task<SuspendedResponse> GetSuspended(
        SuspendedRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new SuspendedResponse());
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