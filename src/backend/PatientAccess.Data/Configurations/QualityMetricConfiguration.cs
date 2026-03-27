using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the QualityMetric entity.
/// Tracks AI performance metrics for AIR-Q01 and AIR-Q03 requirements.
/// </summary>
public class QualityMetricConfiguration : IEntityTypeConfiguration<QualityMetric>
{
    public void Configure(EntityTypeBuilder<QualityMetric> builder)
    {
        builder.ToTable("QualityMetrics");

        builder.HasKey(q => q.QualityMetricId);
        builder.Property(q => q.QualityMetricId)
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

        // Indexes for time-series queries and metric filtering
        builder.HasIndex(q => q.MetricType)
            .HasDatabaseName("IX_QualityMetrics_MetricType");

        builder.HasIndex(q => new { q.PeriodStart, q.PeriodEnd })
            .HasDatabaseName("IX_QualityMetrics_Period");

        builder.HasIndex(q => q.Status)
            .HasDatabaseName("IX_QualityMetrics_Status");
    }
}
