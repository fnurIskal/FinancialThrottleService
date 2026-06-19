using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated;

public partial class ConsistencyCheckGroup
{
    public int Id { get; set; }

    public string GroupName { get; set; } = null!;

    public string ConsistencyCheckName { get; set; } = null!;

    public string Database { get; set; } = null!;

    public string? Parameters { get; set; }

    public string? Emails { get; set; }

    public bool? Enabled { get; set; }

    public bool? IsPublic { get; set; }

    public bool? HasError { get; set; }

    public string? EmailSubject { get; set; }

    public string? EmailsBcc { get; set; }

    public bool? SendEmail { get; set; }

    public bool? SendNotification { get; set; }

    public string? NotificationChannel { get; set; }
}
