namespace FinancialThrottleService.Application.Interfaces
{
    public interface IPushTokenStore
    {
        void SetToken(string token);
        string? GetToken();
    }
}
