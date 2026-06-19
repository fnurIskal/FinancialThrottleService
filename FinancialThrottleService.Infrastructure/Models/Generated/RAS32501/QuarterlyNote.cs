using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS32501;

public partial class QuarterlyNote
{
    public int SecurityId { get; set; }

    public int Quarter { get; set; }

    public int TableTemplateId { get; set; }

    public int ItemQuarterlyNoteCode { get; set; }

    public string? ItemValue { get; set; }
}
