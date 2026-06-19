using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated;

public partial class TableType
{
    public int Id { get; set; }

    public int SourceId { get; set; }

    public string? Code { get; set; }

    public string DataType { get; set; } = null!;

    public string? SourceCode { get; set; }

    public int? Order { get; set; }

    public int? ParentId { get; set; }

    public int? InheritedId { get; set; }

    public bool IsVirtual { get; set; }

    public bool? IsVisible { get; set; }

    public bool? ShowInFunctionTree { get; set; }
}
