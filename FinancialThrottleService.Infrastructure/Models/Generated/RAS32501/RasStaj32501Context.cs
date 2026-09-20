using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS32501;

public partial class RasStaj32501Context : DbContext
{
    public RasStaj32501Context()
    {
    }

    public RasStaj32501Context(DbContextOptions<RasStaj32501Context> options)
        : base(options)
    {
    }

    public virtual DbSet<FinancialsAnnouncementDate> FinancialsAnnouncementDates { get; set; }

    public virtual DbSet<FinancialsAnnouncementFutureDate> FinancialsAnnouncementFutureDates { get; set; }

    public virtual DbSet<Quarterly> Quarterlies { get; set; }

    public virtual DbSet<QuarterlyNote> QuarterlyNotes { get; set; }

    public virtual DbSet<QuarterlyOriginal> QuarterlyOriginals { get; set; }

    public virtual DbSet<QuarterlyOriginalNote> QuarterlyOriginalNotes { get; set; }

    public virtual DbSet<Security> Securities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinancialsAnnouncementDate>(entity =>
        {
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TemplateId, e.IsOriginal }).HasName("PK__Financia__0451149FA1F3476D");

            entity.ToTable("FinancialsAnnouncementDate");

            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.TemplateId).HasColumnName("templateId");
            entity.Property(e => e.IsOriginal).HasColumnName("isOriginal");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
        });

        modelBuilder.Entity<FinancialsAnnouncementFutureDate>(entity =>
        {
            entity.HasKey(e => new { e.SecurityId, e.Quarter });

            entity.ToTable("FinancialsAnnouncementFutureDate");

            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.KapDate)
                .HasColumnType("datetime")
                .HasColumnName("kapDate");
        });

        modelBuilder.Entity<Quarterly>(entity =>
        {
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TableTypeId, e.TemplateId, e.Order }).HasName("PK_sg_st_tr_ise_rs_Quarterly");

            entity.ToTable("Quarterly");

            entity.HasIndex(e => e.ItemQuarterlyCode, "nxQuarterly_itemQuarterlyCode");

            entity.HasIndex(e => new { e.Quarter, e.SecurityId }, "nxQuarterly_quarter_securityId");

            entity.HasIndex(e => new { e.SecurityId, e.Quarter, e.TemplateId, e.TableTypeId }, "nxQuarterly_securityId_quarter_templateId_tableTypeId");

            entity.HasIndex(e => new { e.TableTypeId, e.TemplateId }, "nxQuarterly_tableTypeId_templateId");

            entity.HasIndex(e => new { e.TemplateId, e.ItemQuarterlyCode }, "nxQuarterly_templateId_itemQuarterlyCode");

            entity.HasIndex(e => new { e.TemplateId, e.TableTypeId, e.Quarter }, "nxQuarterly_templateId_tableTypeId_quarter");

            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.TableTypeId).HasColumnName("tableTypeId");
            entity.Property(e => e.TemplateId).HasColumnName("templateId");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.DisabledRules)
                .HasMaxLength(200)
                .HasColumnName("disabledRules");
            entity.Property(e => e.IndentLevel).HasColumnName("indentLevel");
            entity.Property(e => e.IsAutoCreated)
                .HasDefaultValue(false)
                .HasColumnName("isAutoCreated");
            entity.Property(e => e.ItemQuarterlyCode).HasColumnName("itemQuarterlyCode");
            entity.Property(e => e.ItemValue).HasColumnName("itemValue");
            entity.Property(e => e.OriginalDefinition)
                .HasMaxLength(200)
                .HasColumnName("originalDefinition");
        });

        modelBuilder.Entity<QuarterlyNote>(entity =>
        {
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TableTemplateId, e.ItemQuarterlyNoteCode }).HasName("PK__Quarterl__606E2B04675E04BD");

            entity.ToTable("QuarterlyNote");

            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.TableTemplateId).HasColumnName("tableTemplateId");
            entity.Property(e => e.ItemQuarterlyNoteCode).HasColumnName("itemQuarterlyNoteCode");
            entity.Property(e => e.ItemValue)
                .HasMaxLength(1000)
                .HasColumnName("itemValue");
        });

        modelBuilder.Entity<QuarterlyOriginal>(entity =>
        {
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TableTypeId, e.TemplateId, e.Order }).HasName("PK__Quarterl__2F21AE0E65184458");

            entity.ToTable("QuarterlyOriginal");

            entity.HasIndex(e => e.ItemQuarterlyCode, "nxQuarterlyOriginal_itemQuarterlyCode");

            entity.HasIndex(e => new { e.Quarter, e.SecurityId }, "nxQuarterlyOriginal_quarter_securityId");

            entity.HasIndex(e => new { e.SecurityId, e.Quarter, e.TemplateId, e.TableTypeId }, "nxQuarterlyOriginal_securityId_quarter_templateId_tableTypeId");

            entity.HasIndex(e => new { e.TableTypeId, e.TemplateId }, "nxQuarterlyOriginal_tableTypeId_templateId");

            entity.HasIndex(e => new { e.TemplateId, e.ItemQuarterlyCode }, "nxQuarterlyOriginal_templateId_itemQuarterlyCode");

            entity.HasIndex(e => new { e.TemplateId, e.TableTypeId, e.Quarter }, "nxQuarterlyOriginal_templateId_tableTypeId_quarter");

            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.TableTypeId).HasColumnName("tableTypeId");
            entity.Property(e => e.TemplateId).HasColumnName("templateId");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.DisabledRules)
                .HasMaxLength(200)
                .HasColumnName("disabledRules");
            entity.Property(e => e.IndentLevel).HasColumnName("indentLevel");
            entity.Property(e => e.IsAutoCreated)
                .HasDefaultValue(false)
                .HasColumnName("isAutoCreated");
            entity.Property(e => e.ItemQuarterlyCode).HasColumnName("itemQuarterlyCode");
            entity.Property(e => e.ItemValue).HasColumnName("itemValue");
            entity.Property(e => e.OriginalDefinition)
                .HasMaxLength(200)
                .HasColumnName("originalDefinition");
        });

        modelBuilder.Entity<QuarterlyOriginalNote>(entity =>
        {
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TableTemplateId, e.ItemQuarterlyNoteCode }).HasName("PK__Quarterl__606E2B0489378B4D");

            entity.ToTable("QuarterlyOriginalNote");

            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.TableTemplateId).HasColumnName("tableTemplateId");
            entity.Property(e => e.ItemQuarterlyNoteCode).HasColumnName("itemQuarterlyNoteCode");
            entity.Property(e => e.ItemValue)
                .HasMaxLength(1000)
                .HasColumnName("itemValue");
        });

        modelBuilder.Entity<Security>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_sgtrrsstise_ticker");

            entity.ToTable("Security");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BusinessCountryId).HasColumnName("businessCountryId");
            entity.Property(e => e.CapId).HasColumnName("capId");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CoorporationId).HasColumnName("coorporationId");
            entity.Property(e => e.Elite).HasColumnName("elite");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.IsListed)
                .HasDefaultValue(true)
                .HasColumnName("isListed");
            entity.Property(e => e.Popularity).HasColumnName("popularity");
            entity.Property(e => e.RatioSectorId)
                .HasComment("1:Sanayi,2:Banka,3:Sigorta,4:OFK , 5:BDMK")
                .HasDefaultValue(1, "DF_Security_ratioSectorId")
                .HasColumnName("ratioSectorId");
            entity.Property(e => e.Screenable).HasColumnName("screenable");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.VisibleOnSymbolSelect)
                .HasDefaultValue(true, "DF_Security_visibleOnSymbolSelect")
                .HasColumnName("visibleOnSymbolSelect");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
