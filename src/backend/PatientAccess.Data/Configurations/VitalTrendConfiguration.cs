using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core configuration for VitalTrend entity (FR-030).
/// Configures time-series indexing for historical vital signs data (no de-duplication).
/// </summary>
public class VitalTrendConfiguration : IEntityTypeConfiguration<VitalTrend>
{
    public void Configure(EntityTypeBuilder<VitalTrend> builder)
    {
        builder.ToTable("VitalTrends");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(v => v.VitalType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Value)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.RecordedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // FK: VitalTrend -> PatientProfile — CASCADE on delete
        builder.HasOne(v => v.PatientProfile)
            .WithMany(p => p.VitalTrends)
            .HasForeignKey(v => v.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_VitalTrends_PatientProfiles");

        // Composite index for time-series queries
        builder.HasIndex(v => new { v.PatientProfileId, v.VitalType, v.RecordedAt })
            .HasDatabaseName("IX_VitalTrends_PatientProfile_VitalType_RecordedAt");

        // Note: SourceDocumentId and SourceDataId are Guid foreign keys but not enforced
        // to avoid circular dependencies with ClinicalDocument/ExtractedClinicalData
    }
}
