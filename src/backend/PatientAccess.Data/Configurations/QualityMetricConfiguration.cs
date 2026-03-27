using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the QualityMetric entity.
/// US_051 Task 1 - Quality metrics tracking for AIR-Q01 (AI-Human Agreement >98%) and AIR-Q03 (Schema Validity >99%).
/// </summary>
public class QualityMetricConfiguration : IEntityTypeConfiguration<QualityMetric>
{
    public void Configure(EntityTypeBuilder<QualityMetric> builder)
    {
        builder.ToTable("QualityMetrics");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(q => q.MetricType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(q => q.MetricValue)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(q => q.SampleSize)
            .IsRequired();

        builder.Property(q => q.MeasurementPeriod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(q => q.PeriodStart)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(q => q.PeriodEnd)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(q => q.Target)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(q => q.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(q => q.Notes)
            .HasColumnType("text");

        builder.Property(q => q.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // Indices for querying and time-series analysis
        builder.HasIndex(q => q.MetricType)
            .HasDatabaseName("IX_QualityMetrics_MetricType");

        builder.HasIndex(q => new { q.PeriodStart, q.PeriodEnd })
            .HasDatabaseName("IX_QualityMetrics_PeriodStartEnd");

        builder.HasIndex(q => q.Status)
            .HasDatabaseName("IX_QualityMetrics_Status");
    }
}
