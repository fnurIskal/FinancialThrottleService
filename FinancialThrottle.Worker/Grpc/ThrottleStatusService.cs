using FinancialThrottle.Grpc;
using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Application.Logic;
using Grpc.Core;
using Microsoft.Extensions.Configuration;

namespace FinancialThrottle.Worker.Grpc;

public class ThrottleStatusService : ThrottleService.ThrottleServiceBase
{
    private readonly GroupRetryTracker _retryTracker;
    private readonly ILogger<ThrottleStatusService> _logger;
    private readonly IFinancialRepository _repository;
    private readonly ISecurityPriorityClient _priorityClient;
    private readonly string _turkeyDb;

    public ThrottleStatusService(
        GroupRetryTracker retryTracker,
        ILogger<ThrottleStatusService> logger,
        IFinancialRepository repository,
        ISecurityPriorityClient priorityClient,
        IConfiguration configuration)
    {
        _retryTracker = retryTracker;
        _logger = logger;
        _repository = repository;
        _priorityClient = priorityClient;
        _turkeyDb = configuration["DatabaseNames:Turkey"] ?? "RAS_STAJ107";
    }

    public override Task<StatusResponse> GetStatus(
        StatusRequest request,
        ServerCallContext context)
    {
        var heartbeatAge = (DateTime.UtcNow - Worker.LastHeartbeat).TotalSeconds;
        return Task.FromResult(new StatusResponse
        {
            IsRunning = heartbeatAge < 90,
            LastHeartbeatUtc = Worker.LastHeartbeat.ToString("O"),
            QueuedCount = Worker.LastQueuedCount,
            SuspendedCount = _retryTracker.GetSuspendedKeys().Count,
            ProcessedThisCycle = Worker.ProcessedThisCycle,
            WorkerStartedUtc = Worker.StartedAt.ToString("O")
        });
    }

    public override async Task<QueueResponse> GetQueue(
      QueueRequest request,
      ServerCallContext context)
    {
        List<FinancialThrottleService.Domain.Models.WaitingGroup> groups;
        try
        {
            groups = await _repository.GetAllWaitingGroupsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetQueue repository call failed");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

        var turkeySecurityCodes = groups
            .Where(g => g.DatabaseName == _turkeyDb && !string.IsNullOrEmpty(g.SecurityCode))
            .Select(g => g.SecurityCode)
            .Distinct()
            .ToArray();

        Dictionary<string, double> priorityScores = new();
        if (turkeySecurityCodes.Length > 0)
        {
            try { priorityScores = await _priorityClient.GetPrioritiesAsync(turkeySecurityCodes); }
            catch (Exception ex) { _logger.LogWarning(ex, "Priority fetch failed, defaulting to 0"); }
        }

        var response = new QueueResponse();

        foreach (var group in groups
            .Where(g => !_retryTracker.IsSuspended(g.GroupKey))
            .OrderByDescending(g => g.OrderType))
        {
            var groupMsg = new WaitingGroupMessage
            {
                DatabaseName = group.DatabaseName,
                SecurityId = group.SecurityId,
                TemplateId = group.TemplateId,
                SecurityCode = group.SecurityCode,
                ItemCount = group.Items.Count,
                PriorityScore = priorityScores.GetValueOrDefault(group.SecurityCode, 0.0),
                OrderType = group.OrderType,
                Status = _retryTracker.IsSuspended(group.GroupKey) ? "suspended"
                       : _retryTracker.IsWaiting(group.GroupKey) ? "waiting"
                       : "queued"
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

    public override async Task<SuspendedResponse> GetSuspended(
           SuspendedRequest request,
           ServerCallContext context)
    {
        var suspendedGroups = _retryTracker.GetSuspendedGroups();
        var response = new SuspendedResponse();

        var securityIds = suspendedGroups.Select(g => g.SecurityId).Distinct().ToArray();
        Dictionary<int, string> securityCodes = new();
        if (securityIds.Length > 0)
        {
            try
            {
                securityCodes = await _repository.GetSecurityCodesAsync(securityIds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not resolve security codes for suspended groups");
            }
        }

        foreach (var group in suspendedGroups)
        {
            var resolvedCode = securityCodes.TryGetValue(group.SecurityId, out var code)
                ? code
                : $"SEC_{group.SecurityId}";

            response.Groups.Add(new SuspendedGroupMessage
            {
                DatabaseName = group.DatabaseName,
                SecurityId = group.SecurityId,
                TemplateId = group.TemplateId,
                SecurityCode = resolvedCode,
                FailureCount = group.FailureCount,
                FirstFailedUtc = group.FirstFailedAt.ToString("O"),
                NextRetryUtc = group.NextRetryAt.ToString("O"),
                RetryInProgress = group.RetryInProgress
            });
        }

        return response;
    }

    public override Task<ForceSendResponse> ForceSendGroup(
        ForceSendRequest request,
        ServerCallContext context)
    {
        var groupKey = $"{request.DatabaseName}|{request.SecurityId}|{request.TemplateId}";
        _retryTracker.MarkForceSend(groupKey);
        _logger.LogInformation("ForceSend marked for {GroupKey}", groupKey);
        return Task.FromResult(new ForceSendResponse
        {
            Success = true,
            Message = $"{groupKey} will be force-sent on next cycle"
        });
    }

    public override Task<RetryGroupResponse> RetryGroup(
        RetryGroupRequest request,
        ServerCallContext context)
    {
        var groupKey = $"{request.DatabaseName}|{request.SecurityId}|{request.TemplateId}";

        if (!_retryTracker.IsSuspended(groupKey))
        {
            return Task.FromResult(new RetryGroupResponse
            {
                Success = false,
                Message = $"{groupKey} not in the suspended list"
            });
        }

        _retryTracker.ForceRetry(groupKey);

        return Task.FromResult(new RetryGroupResponse
        {
            Success = true,
            Message = $"{groupKey} it will be retried in the next cycle."
        });
    }
}