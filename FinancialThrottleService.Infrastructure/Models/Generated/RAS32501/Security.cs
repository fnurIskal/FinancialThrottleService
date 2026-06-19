using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS32501;

public partial class Security
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// 1:Sanayi,2:Banka,3:Sigorta,4:OFK , 5:BDMK
    /// </summary>
    public int RatioSectorId { get; set; }

    public int? State { get; set; }

    public bool? IsListed { get; set; }

    public int? Popularity { get; set; }

    public int? CapId { get; set; }

    public bool Screenable { get; set; }

    public bool VisibleOnSymbolSelect { get; set; }

    public int? BusinessCountryId { get; set; }

    public bool? Elite { get; set; }

    public int? CoorporationId { get; set; }
}
