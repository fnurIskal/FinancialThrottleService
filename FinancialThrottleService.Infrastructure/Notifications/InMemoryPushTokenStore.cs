using FinancialThrottleService.Application.Interfaces;

namespace FinancialThrottleService.Infrastructure.Notifications
{
    public class InMemoryPushTokenStore : IPushTokenStore
    {
        private volatile string? _token;

        public void SetToken(string token) => _token = token;

        public string? GetToken() => _token;
    }
}
