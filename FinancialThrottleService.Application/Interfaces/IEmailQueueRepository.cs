using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialThrottleService.Application.Interfaces
{
    public interface IEmailQueueRepository
    {
        Task EnqueueAsync(string subject, string body, string[] recipients);
    }
}
