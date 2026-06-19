using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS;

public partial class EmailQueue
{
    public Guid Id { get; set; }

    public string? To { get; set; }

    public string? Bcc { get; set; }

    public bool? IsBodyHtml { get; set; }

    public string? Subject { get; set; }

    public byte[]? Body { get; set; }

    public DateTime? RequestDate { get; set; }

    public DateTime? SentDate { get; set; }

    public DateTime? LastRetryDate { get; set; }

    public int? RetryCount { get; set; }
}
