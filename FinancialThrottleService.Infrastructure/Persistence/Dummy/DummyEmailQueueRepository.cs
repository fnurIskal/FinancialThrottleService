using System;
using System.Collections.Generic;
using System.Text;
using FinancialThrottleService.Application.Interfaces;


namespace FinancialThrottleService.Infrastructure.Persistence.Dummy
{
    public class DummyEmailQueueRepository : IEmailQueueRepository
    {      public Task EnqueueAsync(string subject, string body, string[] recipients)
        {
            Console.WriteLine($"[DUMMY] Email → {string.Join(",", recipients)}");
            Console.WriteLine($"[DUMMY] Subject: {subject}");
            Console.WriteLine($"[DUMMY] Body: {body}");
            return Task.CompletedTask;
        }
    }
}
