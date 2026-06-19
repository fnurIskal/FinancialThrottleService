using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS107;

public partial class Quarterly
{
    public int SecurityId { get; set; }

    public int Quarter { get; set; }

    public int TableTypeId { get; set; }

    public int TemplateId { get; set; }

    public int Order { get; set; }

    public string? OriginalDefinition { get; set; }

    public int? ItemQuarterlyCode { get; set; }

    public double? ItemValue { get; set; }

    public int? IndentLevel { get; set; }

    public bool? IsAutoCreated { get; set; }

    public string? DisabledRules { get; set; }
}
