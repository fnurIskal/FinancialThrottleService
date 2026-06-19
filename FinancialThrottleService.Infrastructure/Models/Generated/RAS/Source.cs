using System;
using System.Collections.Generic;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS;

public partial class Source
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public int? MarketId { get; set; }

    public int? CountryId { get; set; }

    public int? ExchangeId { get; set; }

    public int? VendorId { get; set; }

    public int CurrencyId { get; set; }

    public int DelegateSecurityId { get; set; }

    public int? DefaultInflationId { get; set; }

    public string? DirectoryAlias { get; set; }

    public int? Order { get; set; }

    public string? Description { get; set; }

    public bool? IsCoveragePureSecurity { get; set; }

    public int? IsAdjustable { get; set; }

    public int? DefaultTemplateFamilyId { get; set; }

    public double? DefaultDailyCoefficient { get; set; }

    public int DefaultGroupItem { get; set; }

    public int? DefaultSecurityId { get; set; }

    public string? DefaultCurrencySourceCode { get; set; }

    public bool? BackofficeDailyTest { get; set; }

    public bool? HideInLayoutTree { get; set; }

    public bool? ShowInLayoutTree { get; set; }

    public int? MinimumEquityCount { get; set; }

    public int? MinimumIndexCount { get; set; }

    public int? BenchmarkGroupItemId { get; set; }

    public int? FunctionTreeVersion { get; set; }

    public bool? HasCapitalAction { get; set; }

    public bool? HasCompanyEstimate { get; set; }

    public bool? HasDaily { get; set; }

    public bool? HasDividend { get; set; }

    public bool? HasNews { get; set; }

    public bool? HasPeriodically { get; set; }

    public bool? HasQuarterly { get; set; }

    public bool? HasReport { get; set; }

    public bool? HasSessionly { get; set; }

    public bool? HasWeekly { get; set; }

    public bool? HasConsensusEstimate { get; set; }

    public bool Screenable { get; set; }

    public string? BenchmarkGroupItemSourceCode { get; set; }

    public bool? IncludedtoGlobalPackage { get; set; }

    public string? PrivateCompanyGroupItem { get; set; }

    public bool? HasConstituents { get; set; }

    public bool? HasHoldingStructure { get; set; }

    public bool? HasRepresentative { get; set; }

    public bool? IsIndexOnly { get; set; }

    public int? RollConvention { get; set; }
}
