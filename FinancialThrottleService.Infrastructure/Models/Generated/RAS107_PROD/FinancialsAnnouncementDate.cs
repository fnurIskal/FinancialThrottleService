using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS107_PROD;

public partial class FinancialsAnnouncementDate
{
    public int SecurityId { get; set; }

    public int Quarter { get; set; }

    public int TemplateId { get; set; }

    public DateTime? Date { get; set; }

    public bool IsOriginal { get; set; }
}
