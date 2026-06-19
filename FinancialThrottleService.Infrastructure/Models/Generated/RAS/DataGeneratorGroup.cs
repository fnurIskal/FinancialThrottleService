using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS;

public partial class DataGeneratorGroup
{
    public int Id { get; set; }

    public string GroupName { get; set; } = null!;

    public string DataGeneratorName { get; set; } = null!;

    public string Database { get; set; } = null!;

    public string? Parameters { get; set; }

    public bool? Enabled { get; set; }
}
