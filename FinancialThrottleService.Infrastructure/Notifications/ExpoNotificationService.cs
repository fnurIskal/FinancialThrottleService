using FinancialThrottleService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace FinancialThrottleService.Infrastructure.Notifications
{
    public class ExpoNotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IPushTokenStore _tokenStore;
        private readonly string _configToken;

        public ExpoNotificationService(
            IHttpClientFactory factory,
            IConfiguration config,
            IPushTokenStore tokenStore)
        {
            _httpClient = factory.CreateClient("expo");
            _tokenStore = tokenStore;
            _configToken = config["ExpoPushToken"] ?? string.Empty;
        }

        public async Task SendSuspendedNotificationAsync(string groupKey, string errorMessage)
        {
            var token = _tokenStore.GetToken();
            if (string.IsNullOrEmpty(token)) token = _configToken;

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[PUSH] No Expo push token available (store and config both empty) — skipping notification");
                return;
            }

            try
            {
                var parts = groupKey.Split('|');
                var title = "Group Suspended ⚠️";
                var body = parts.Length == 3
                    ? $"{parts[0]} | SEC {parts[1]} | T{parts[2]}"
                    : groupKey;

                if (!string.IsNullOrEmpty(errorMessage))
                    body += $"\n{(errorMessage.Length > 80 ? errorMessage[..80] + "..." : errorMessage)}";

                var payload = new
                {
                    to = token,
                    title,
                    body,
                    channelId = "suspended",
                    priority = "high",
                    sound = "default"
                };


                var response = await _httpClient.PostAsJsonAsync("https://exp.host/--/api/v2/push/send", payload);
                var responseBody = await response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Push notification failed: {ex.Message}");
            }
        }
    }
}
