using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS;

public partial class TableTemplate2
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public int? Priority { get; set; }

    public bool IsVirtual { get; set; }

    public int? Order { get; set; }

    public string? SourceCode { get; set; }

    public bool? ReadOnly { get; set; }

    public int TemplateGroupId { get; set; }

    public int TemplateFamilyId { get; set; }

    public int? CurrencyId { get; set; }

    public bool? IsVisible { get; set; }

    public bool? IsOriginalEnabled { get; set; }

    public bool? Hvenabled { get; set; }

    public bool? IsSolo { get; set; }

    public int? TemplateTypeId { get; set; }

    public bool? IsOriginalFormulatedEnabled { get; set; }

    public bool? ShowInFunctionTree { get; set; }

    public string? SourceCodes { get; set; }

    public int? AccountingStandardsId { get; set; }

    public bool? IsAdjustedEps { get; set; }
}
