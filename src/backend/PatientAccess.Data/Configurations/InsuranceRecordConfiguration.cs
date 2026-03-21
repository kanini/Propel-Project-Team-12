using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the InsuranceRecord entity (DR-015).
/// Reference data for FR-021 insurance pre-check validation.
/// </summary>
public class InsuranceRecordConfiguration : IEntityTypeConfiguration<InsuranceRecord>
{
    public void Configure(EntityTypeBuilder<InsuranceRecord> builder)
    {
        builder.ToTable("InsuranceRecords");

        builder.HasKey(i => i.InsuranceRecordId);
        builder.Property(i => i.InsuranceRecordId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.ProviderName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.AcceptedIdPattern)
            .HasMaxLength(500);

        builder.Property(i => i.CoverageType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(i => i.UpdatedAt)
            .HasColumnType("timestamptz");

        // Composite index on (provider_name, active) per US_016 AC-4
        builder.HasIndex(i => new { i.ProviderName, i.IsActive })
            .HasDatabaseName("IX_InsuranceRecords_ProviderName_IsActive");

        builder.HasIndex(i => i.IsActive)
            .HasDatabaseName("IX_InsuranceRecords_IsActive");
    }
}
