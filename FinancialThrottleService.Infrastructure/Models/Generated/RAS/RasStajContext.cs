using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS;

public partial class RasStajContext : DbContext
{
    public RasStajContext()
    {
    }

    public RasStajContext(DbContextOptions<RasStajContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ConsistencyCheckGroup> ConsistencyCheckGroups { get; set; }

    public virtual DbSet<DataGeneratorGroup> DataGeneratorGroups { get; set; }

    public virtual DbSet<DuplicateTableTemplateMap> DuplicateTableTemplateMaps { get; set; }

    public virtual DbSet<EmailQueue> EmailQueues { get; set; }

    public virtual DbSet<FinancialTrace> FinancialTraces { get; set; }

    public virtual DbSet<FormulaTestGroup> FormulaTestGroups { get; set; }

    public virtual DbSet<Source> Sources { get; set; }

    public virtual DbSet<TableTemplate2> TableTemplate2s { get; set; }

    public virtual DbSet<TableType> TableTypes { get; set; }

    public virtual DbSet<UtTrcTracerParam> UtTrcTracerParams { get; set; }

    public virtual DbSet<WaitingFinancialTable> WaitingFinancialTables { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=RAS_STAJ;User Id=sqlserver;Password=REMOVED_SECRET;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConsistencyCheckGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Consiste__3213E83F6754599E");

            entity.ToTable("ConsistencyCheckGroup");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ConsistencyCheckName)
                .HasMaxLength(50)
                .HasColumnName("consistencyCheckName");
            entity.Property(e => e.Database)
                .HasMaxLength(50)
                .HasColumnName("database");
            entity.Property(e => e.EmailSubject)
                .HasMaxLength(250)
                .HasColumnName("emailSubject");
            entity.Property(e => e.Emails)
                .HasMaxLength(250)
                .HasColumnName("emails");
            entity.Property(e => e.EmailsBcc)
                .HasMaxLength(250)
                .HasColumnName("emailsBcc");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.Property(e => e.GroupName)
                .HasMaxLength(50)
                .HasColumnName("groupName");
            entity.Property(e => e.HasError).HasColumnName("hasError");
            entity.Property(e => e.IsPublic).HasColumnName("isPublic");
            entity.Property(e => e.NotificationChannel)
                .HasMaxLength(100)
                .HasColumnName("notificationChannel");
            entity.Property(e => e.Parameters).HasColumnName("parameters");
            entity.Property(e => e.SendEmail).HasColumnName("sendEmail");
            entity.Property(e => e.SendNotification).HasColumnName("sendNotification");
        });

        modelBuilder.Entity<DataGeneratorGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DataGene__3213E83F08A0334B");

            entity.ToTable("DataGeneratorGroup");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DataGeneratorName)
                .HasMaxLength(50)
                .HasColumnName("dataGeneratorName");
            entity.Property(e => e.Database)
                .HasMaxLength(50)
                .HasColumnName("database");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.Property(e => e.GroupName)
                .HasMaxLength(50)
                .HasColumnName("groupName");
            entity.Property(e => e.Parameters)
                .HasMaxLength(4000)
                .HasColumnName("parameters");
        });

        modelBuilder.Entity<DuplicateTableTemplateMap>(entity =>
        {
            entity.HasKey(e => new { e.SourceId, e.SecurityId });

            entity.ToTable("DuplicateTableTemplateMap");

            entity.Property(e => e.SourceId).HasColumnName("sourceId");
            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.FromTemplateId).HasColumnName("fromTemplateId");
            entity.Property(e => e.ToTemplateId).HasColumnName("toTemplateId");
        });

        modelBuilder.Entity<EmailQueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EmailQue__3213E83F16B953F2D");

            entity.ToTable("EmailQueue", tb => tb.HasTrigger("trg_EmailQueue_DmsTestServer"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Bcc)
                .HasMaxLength(400)
                .HasColumnName("bcc");
            entity.Property(e => e.Body)
                .HasColumnType("image")
                .HasColumnName("body");
            entity.Property(e => e.IsBodyHtml).HasColumnName("isBodyHtml");
            entity.Property(e => e.LastRetryDate)
                .HasColumnType("datetime")
                .HasColumnName("lastRetryDate");
            entity.Property(e => e.RequestDate)
                .HasColumnType("datetime")
                .HasColumnName("requestDate");
            entity.Property(e => e.RetryCount).HasColumnName("retryCount");
            entity.Property(e => e.SentDate)
                .HasColumnType("datetime")
                .HasColumnName("sentDate");
            entity.Property(e => e.Subject)
                .HasMaxLength(1000)
                .HasColumnName("subject");
            entity.Property(e => e.To)
                .HasMaxLength(400)
                .HasColumnName("to");
        });

        modelBuilder.Entity<FinancialTrace>(entity =>
        {
            entity.HasKey(e => new { e.DatabaseName, e.SecurityId, e.Quarter, e.TemplateId, e.DisclosureId }).HasName("PK_FinancialTraces");

            entity.ToTable("FinancialTrace");

            entity.Property(e => e.DatabaseName)
                .HasMaxLength(50)
                .HasColumnName("databaseName");
            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.TemplateId).HasColumnName("templateId");
            entity.Property(e => e.DisclosureId)
                .HasDefaultValue(1)
                .HasColumnName("disclosureId");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.Type1).HasColumnName("type1");
            entity.Property(e => e.Type10).HasColumnName("type10");
            entity.Property(e => e.Type11).HasColumnName("type11");
            entity.Property(e => e.Type12).HasColumnName("type12");
            entity.Property(e => e.Type13).HasColumnName("type13");
            entity.Property(e => e.Type14).HasColumnName("type14");
            entity.Property(e => e.Type15).HasColumnName("type15");
            entity.Property(e => e.Type16).HasColumnName("type16");
            entity.Property(e => e.Type17).HasColumnName("type17");
            entity.Property(e => e.Type18).HasColumnName("type18");
            entity.Property(e => e.Type19).HasColumnName("type19");
            entity.Property(e => e.Type2).HasColumnName("type2");
            entity.Property(e => e.Type3).HasColumnName("type3");
            entity.Property(e => e.Type4).HasColumnName("type4");
            entity.Property(e => e.Type5).HasColumnName("type5");
            entity.Property(e => e.Type6).HasColumnName("type6");
            entity.Property(e => e.Type7).HasColumnName("type7");
            entity.Property(e => e.Type8).HasColumnName("type8");
            entity.Property(e => e.Type9).HasColumnName("type9");
            entity.Property(e => e.Type99).HasColumnName("type99");
        });

        modelBuilder.Entity<FormulaTestGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FormulaTestGroup2");

            entity.ToTable("FormulaTestGroup");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_sgglobal_source20");

            entity.ToTable("Source");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BackofficeDailyTest).HasColumnName("backofficeDailyTest");
            entity.Property(e => e.BenchmarkGroupItemId).HasColumnName("benchmarkGroupItemId");
            entity.Property(e => e.BenchmarkGroupItemSourceCode)
                .HasMaxLength(10)
                .HasColumnName("benchmarkGroupItemSourceCode");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.CountryId).HasColumnName("countryId");
            entity.Property(e => e.CurrencyId).HasColumnName("currencyId");
            entity.Property(e => e.DefaultCurrencySourceCode)
                .HasMaxLength(10)
                .HasColumnName("defaultCurrencySourceCode");
            entity.Property(e => e.DefaultDailyCoefficient).HasColumnName("defaultDailyCoefficient");
            entity.Property(e => e.DefaultGroupItem).HasColumnName("defaultGroupItem");
            entity.Property(e => e.DefaultInflationId).HasColumnName("defaultInflationId");
            entity.Property(e => e.DefaultSecurityId).HasColumnName("defaultSecurityId");
            entity.Property(e => e.DefaultTemplateFamilyId).HasColumnName("defaultTemplateFamilyId");
            entity.Property(e => e.DelegateSecurityId).HasColumnName("delegateSecurityId");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.DirectoryAlias)
                .HasMaxLength(100)
                .HasColumnName("directoryAlias");
            entity.Property(e => e.ExchangeId).HasColumnName("exchangeId");
            entity.Property(e => e.FunctionTreeVersion).HasColumnName("functionTreeVersion");
            entity.Property(e => e.HasCapitalAction).HasColumnName("hasCapitalAction");
            entity.Property(e => e.HasCompanyEstimate).HasColumnName("hasCompanyEstimate");
            entity.Property(e => e.HasConsensusEstimate).HasColumnName("hasConsensusEstimate");
            entity.Property(e => e.HasConstituents).HasColumnName("hasConstituents");
            entity.Property(e => e.HasDaily).HasColumnName("hasDaily");
            entity.Property(e => e.HasDividend).HasColumnName("hasDividend");
            entity.Property(e => e.HasHoldingStructure).HasColumnName("hasHoldingStructure");
            entity.Property(e => e.HasNews).HasColumnName("hasNews");
            entity.Property(e => e.HasPeriodically).HasColumnName("hasPeriodically");
            entity.Property(e => e.HasQuarterly).HasColumnName("hasQuarterly");
            entity.Property(e => e.HasReport).HasColumnName("hasReport");
            entity.Property(e => e.HasRepresentative).HasColumnName("hasRepresentative");
            entity.Property(e => e.HasSessionly).HasColumnName("hasSessionly");
            entity.Property(e => e.HasWeekly).HasColumnName("hasWeekly");
            entity.Property(e => e.HideInLayoutTree).HasColumnName("hideInLayoutTree");
            entity.Property(e => e.IncludedtoGlobalPackage).HasColumnName("includedtoGlobalPackage");
            entity.Property(e => e.IsAdjustable).HasColumnName("isAdjustable");
            entity.Property(e => e.IsCoveragePureSecurity).HasColumnName("isCoveragePureSecurity");
            entity.Property(e => e.IsIndexOnly).HasColumnName("isIndexOnly");
            entity.Property(e => e.MarketId).HasColumnName("marketId");
            entity.Property(e => e.MinimumEquityCount).HasColumnName("minimumEquityCount");
            entity.Property(e => e.MinimumIndexCount).HasColumnName("minimumIndexCount");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.PrivateCompanyGroupItem)
                .HasMaxLength(50)
                .HasColumnName("privateCompanyGroupItem");
            entity.Property(e => e.RollConvention).HasDefaultValue(0);
            entity.Property(e => e.Screenable).HasColumnName("screenable");
            entity.Property(e => e.ShowInLayoutTree).HasColumnName("showInLayoutTree");
            entity.Property(e => e.VendorId).HasColumnName("vendorId");
        });

        modelBuilder.Entity<TableTemplate2>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TableTem__3213E83F6DC23433");

            entity.ToTable("TableTemplate2");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountingStandardsId).HasColumnName("accountingStandardsId");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CurrencyId).HasColumnName("currencyId");
            entity.Property(e => e.Hvenabled).HasColumnName("hvenabled");
            entity.Property(e => e.IsAdjustedEps).HasColumnName("isAdjustedEps");
            entity.Property(e => e.IsOriginalEnabled).HasColumnName("isOriginalEnabled");
            entity.Property(e => e.IsOriginalFormulatedEnabled).HasColumnName("isOriginalFormulatedEnabled");
            entity.Property(e => e.IsSolo).HasColumnName("isSolo");
            entity.Property(e => e.IsVirtual).HasColumnName("isVirtual");
            entity.Property(e => e.IsVisible).HasColumnName("isVisible");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.ReadOnly).HasColumnName("readOnly");
            entity.Property(e => e.ShowInFunctionTree).HasColumnName("showInFunctionTree");
            entity.Property(e => e.SourceCode)
                .HasMaxLength(10)
                .HasColumnName("sourceCode");
            entity.Property(e => e.SourceCodes)
                .HasMaxLength(4000)
                .HasColumnName("sourceCodes");
            entity.Property(e => e.TemplateFamilyId).HasColumnName("templateFamilyId");
            entity.Property(e => e.TemplateGroupId).HasColumnName("templateGroupId");
            entity.Property(e => e.TemplateTypeId).HasColumnName("templateTypeId");
        });

        modelBuilder.Entity<TableType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TableTyp__3213E83F7EA1BC21");

            entity.ToTable("TableType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.DataType)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("dataType");
            entity.Property(e => e.InheritedId).HasColumnName("inheritedId");
            entity.Property(e => e.IsVirtual).HasColumnName("isVirtual");
            entity.Property(e => e.IsVisible).HasColumnName("isVisible");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.ParentId).HasColumnName("parentId");
            entity.Property(e => e.ShowInFunctionTree).HasColumnName("showInFunctionTree");
            entity.Property(e => e.SourceCode)
                .HasMaxLength(10)
                .HasColumnName("sourceCode");
            entity.Property(e => e.SourceId).HasColumnName("sourceId");
        });

        modelBuilder.Entity<UtTrcTracerParam>(entity =>
        {
            entity.HasKey(e => e.ParamName);

            entity.ToTable("ut_trc_tracer_param");

            entity.Property(e => e.ParamName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("param_name");
            entity.Property(e => e.NValue).HasColumnName("n_value");
            entity.Property(e => e.StrValue)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("str_value");
        });

        modelBuilder.Entity<WaitingFinancialTable>(entity =>
        {
            entity.HasKey(e => new { e.DatabaseName, e.SecurityId, e.Quarter, e.TemplateId, e.TableTypeId, e.IsOriginal, e.DisclosureId });

            entity.Property(e => e.DatabaseName)
                .HasMaxLength(50)
                .HasColumnName("databaseName");
            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.TemplateId).HasColumnName("templateId");
            entity.Property(e => e.TableTypeId).HasColumnName("tableTypeId");
            entity.Property(e => e.IsOriginal).HasColumnName("isOriginal");
            entity.Property(e => e.DisclosureId)
                .HasDefaultValue(1)
                .HasColumnName("disclosureId");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.DisabledRules).HasColumnName("disabledRules");
            entity.Property(e => e.SendEmail).HasColumnName("sendEmail");
            entity.Property(e => e.Sql).HasColumnName("sql");
            entity.Property(e => e.Username)
                .HasMaxLength(150)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
