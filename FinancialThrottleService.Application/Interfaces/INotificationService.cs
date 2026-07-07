namespace FinancialThrottleService.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendSuspendedNotificationAsync(string groupKey, string errorMessage);
    }
}
