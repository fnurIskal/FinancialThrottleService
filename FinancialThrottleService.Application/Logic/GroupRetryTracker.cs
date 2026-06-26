using FinancialThrottleService.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FinancialThrottleService.Application.Logic
{
    public class GroupRetryTracker
    {
        private readonly ConcurrentDictionary<string, GroupRetryState> _states = new();
        private readonly ConcurrentDictionary<string, bool> _forceSendKeys = new();
        private readonly ConcurrentDictionary<string, bool> _waitingKeys = new();

        private const int MaxAttempts = 10;

        private static readonly TimeSpan[] RetrySchedule =
        {
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(60),
            TimeSpan.FromHours(3),
            TimeSpan.FromHours(6),
            TimeSpan.FromDays(1)
        };

        public bool RecordFailure(string groupKey)
        {
            var state = _states.GetOrAdd(groupKey, key =>
            {
                var parts = key.Split('|');
                return new GroupRetryState
                {
                    FirstFailedAt = DateTime.UtcNow,
                    DatabaseName = parts.Length > 0 ? parts[0] : string.Empty,
                    SecurityId = parts.Length > 1 && int.TryParse(parts[1], out var sid) ? sid : 0,
                    TemplateId = parts.Length > 2 && int.TryParse(parts[2], out var tid) ? tid : 0,
                    SecurityCode = string.Empty
                };
            });

            lock (state)
            {
                state.FailureCount++;

                if (state.FailureCount >= MaxAttempts && !state.IsSuspended)
                {
                    state.IsSuspended = true;
                    state.SuspendRetryIndex = 0;
                    state.NextRetryAt = DateTime.UtcNow.Add(RetrySchedule[0]);
                    return true;
                }
            }

            return false;
        }

        public void MarkForceSend(string groupKey)
        {
            _forceSendKeys[groupKey] = true;
            if (_states.TryGetValue(groupKey, out var state))
                lock (state) { state.IsSuspended = false; state.FailureCount = 0; state.RetryInProgress = false; }
        }

        public bool IsForceSend(string groupKey) => _forceSendKeys.ContainsKey(groupKey);

        public void ClearForceSend(string groupKey) => _forceSendKeys.TryRemove(groupKey, out _);

        public void RecordWait(string groupKey) => _waitingKeys[groupKey] = true;
        public void ClearWait(string groupKey) => _waitingKeys.TryRemove(groupKey, out _);
        public bool IsWaiting(string groupKey) => _waitingKeys.ContainsKey(groupKey);

        public void ForceRetry(string groupKey)
        {
            if (_states.TryGetValue(groupKey, out var state))
            {
                lock (state)
                {
                    state.NextRetryAt = DateTime.UtcNow;
                    state.RetryInProgress = false;
                }
            }
        }

        public void RecordSuccess(string groupKey)
        {
            _states.TryRemove(groupKey, out _);
        }

        public void BeginSuspendRetry(string groupKey)
        {
            if (_states.TryGetValue(groupKey, out var state))
                lock (state) { state.RetryInProgress = true; }
        }

        public void RecordSuspendRetryFailure(string groupKey)
        {
            if (!_states.TryGetValue(groupKey, out var state)) return;

            lock (state)
            {
                state.RetryInProgress = false;
                state.FailureCount++;

                int nextIndex = Math.Min(
                    state.SuspendRetryIndex + 1,
                    RetrySchedule.Length - 1);

                state.SuspendRetryIndex = nextIndex;
                state.NextRetryAt = DateTime.UtcNow.Add(RetrySchedule[nextIndex]);
            }
        }

        public void RecordSuspendRetrySuccess(string groupKey)
        {
            _states.TryRemove(groupKey, out _);
        }

        public bool IsSuspended(string groupKey) =>
            _states.TryGetValue(groupKey, out var s) && s.IsSuspended;

        public bool IsRetryDue(string groupKey) =>
            _states.TryGetValue(groupKey, out var s) &&
            s.IsSuspended &&
            !s.RetryInProgress &&
            DateTime.UtcNow >= s.NextRetryAt;

        public List<SuspendedGroupInfo> GetSuspendedGroups()
        {
            return _states.Values
                .Where(s => s.IsSuspended)
                .Select(s => new SuspendedGroupInfo
                {
                    DatabaseName = s.DatabaseName,
                    SecurityId = s.SecurityId,
                    TemplateId = s.TemplateId,
                    SecurityCode = s.SecurityCode,
                    FailureCount = s.FailureCount,
                    FirstFailedAt = s.FirstFailedAt,
                    NextRetryAt = s.NextRetryAt,
                    RetryInProgress = s.RetryInProgress
                })
                .ToList();
        }

        public List<string> GetSuspendedKeys() =>
            _states.Where(kv => kv.Value.IsSuspended)
                   .Select(kv => kv.Key)
                   .ToList();

        private class GroupRetryState
        {
            public string DatabaseName { get; set; } = string.Empty;
            public int SecurityId { get; set; }
            public int TemplateId { get; set; }
            public string SecurityCode { get; set; } = string.Empty;
            public int FailureCount { get; set; }
            public bool IsSuspended { get; set; }
            public bool RetryInProgress { get; set; }
            public DateTime FirstFailedAt { get; set; }
            public DateTime NextRetryAt { get; set; }
            public int SuspendRetryIndex { get; set; }
        }
    }
}