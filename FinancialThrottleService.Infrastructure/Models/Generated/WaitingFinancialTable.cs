using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated;

public partial class WaitingFinancialTable
{
    public string DatabaseName { get; set; } = null!;

    public int SecurityId { get; set; }

    public int Quarter { get; set; }

    public int TemplateId { get; set; }

    public int TableTypeId { get; set; }

    public bool IsOriginal { get; set; }

    public DateTime? Date { get; set; }

    public bool? SendEmail { get; set; }

    public string? Username { get; set; }

    public string? DisabledRules { get; set; }

    public string? Sql { get; set; }

    public int DisclosureId { get; set; }
}
