using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS;

namespace FinancialThrottleService.Infrastructure.Persistence.Sql
{
    public class SqlEmailQueueRepository : IEmailQueueRepository
    {
        private readonly RasStajContext _context;

        public SqlEmailQueueRepository(RasStajContext context)
        {
            _context = context;
        }

        public async Task EnqueueAsync(string subject, string body, string[] recipients)
        {
            var entry = new EmailQueue
            {
                Id = Guid.NewGuid(),
                To = string.Join(";", recipients),
                Subject = subject,
                Body = System.Text.Encoding.UTF8.GetBytes(body),
                IsBodyHtml = false,
                RequestDate = DateTime.UtcNow,
                RetryCount = 0
            };

            _context.EmailQueues.Add(entry);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[INFO] EmailQueue: enqueued to={entry.To} subject={subject}");
        }
    }
}
