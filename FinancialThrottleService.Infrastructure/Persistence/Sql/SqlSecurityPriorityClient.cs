using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS107;
using Microsoft.EntityFrameworkCore;

namespace FinancialThrottleService.Infrastructure.Persistence.Sql
{
    public class SqlSecurityPriorityClient : ISecurityPriorityClient
    {
        private readonly RasStaj107Context _context;

        public SqlSecurityPriorityClient(RasStaj107Context context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, double>> GetPrioritiesAsync(string[] securityCodes)
        {
            var securities = await _context.Securities
                .Where(s => s.Code != null && securityCodes.Contains(s.Code))
                .Select(s => new { s.Code, s.Popularity })
                .ToListAsync();

            var result = securityCodes.ToDictionary(
                code => code,
                code =>
                {
                    var match = securities.FirstOrDefault(s => s.Code == code);
                    return match != null ? (double)(match.Popularity ?? 0) : 0.0;
                });

            Console.WriteLine($"[DEBUG] SqlSecurityPriorityClient: fetched {result.Count} priority scores");
            return result;
        }
    }
}
