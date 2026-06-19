using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS;

public partial class DuplicateTableTemplateMap
{
    public int SourceId { get; set; }

    public int SecurityId { get; set; }

    public int? FromTemplateId { get; set; }

    public int? ToTemplateId { get; set; }
}
