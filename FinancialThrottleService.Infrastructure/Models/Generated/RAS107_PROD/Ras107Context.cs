using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FinancialThrottleService.Infrastructure.Models.Generated.RAS107_PROD;

public partial class Ras107Context : DbContext
{
    public Ras107Context()
    {
    }

    public Ras107Context(DbContextOptions<Ras107Context> options)
        : base(options)
    {
    }

    public virtual DbSet<FinancialsAnnouncementDate> FinancialsAnnouncementDates { get; set; }

    public virtual DbSet<FinancialsAnnouncementFutureDate> FinancialsAnnouncementFutureDates { get; set; }

    public virtual DbSet<Quarterly> Quarterlies { get; set; }

    public virtual DbSet<QuarterlyNote> QuarterlyNotes { get; set; }

    public virtual DbSet<QuarterlyOriginal> QuarterlyOriginals { get; set; }

    public virtual DbSet<QuarterlyOriginalNote> QuarterlyOriginalNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinancialsAnnouncementDate>(entity =>
        {
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TemplateId, e.IsOriginal }).HasName("PK__Financia__0451149F6D1C2073");

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
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TableTemplateId, e.ItemQuarterlyNoteCode }).HasName("PK__Quarterl__606E2B043DC1AC74");

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
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TableTypeId, e.TemplateId, e.Order }).HasName("PK__Quarterl__2F21AE0E948837B6");

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
            entity.HasKey(e => new { e.SecurityId, e.Quarter, e.TableTemplateId, e.ItemQuarterlyNoteCode }).HasName("PK__Quarterl__606E2B0441BA86A5");

            entity.ToTable("QuarterlyOriginalNote");

            entity.Property(e => e.SecurityId).HasColumnName("securityId");
            entity.Property(e => e.Quarter).HasColumnName("quarter");
            entity.Property(e => e.TableTemplateId).HasColumnName("tableTemplateId");
            entity.Property(e => e.ItemQuarterlyNoteCode).HasColumnName("itemQuarterlyNoteCode");
            entity.Property(e => e.ItemValue)
                .HasMaxLength(1000)
                .HasColumnName("itemValue");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
