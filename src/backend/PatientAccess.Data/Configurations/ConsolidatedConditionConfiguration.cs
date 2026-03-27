using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core configuration for ConsolidatedCondition entity (FR-030).
/// Configures indexes for patient queries and JSONB columns for source traceability.
/// </summary>
public class ConsolidatedConditionConfiguration : IEntityTypeConfiguration<ConsolidatedCondition>
{
    public void Configure(EntityTypeBuilder<ConsolidatedCondition> builder)
    {
        builder.ToTable("ConsolidatedConditions");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.ConditionName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.ICD10Code)
            .HasMaxLength(20);

        builder.Property(c => c.DiagnosisDate)
            .HasColumnType("timestamptz");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Severity)
            .HasMaxLength(50);

        // JSONB columns for source traceability
        builder.Property(c => c.SourceDocumentIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(c => c.SourceDataIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(c => c.IsDuplicate)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DuplicateCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.FirstRecordedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(c => c.LastUpdatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // FK: ConsolidatedCondition -> PatientProfile — CASCADE on delete
        builder.HasOne(c => c.PatientProfile)
            .WithMany(p => p.Conditions)
            .HasForeignKey(c => c.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConsolidatedConditions_PatientProfiles");

        // Composite index for patient profile queries
        builder.HasIndex(c => c.PatientProfileId)
            .HasDatabaseName("IX_ConsolidatedConditions_PatientProfileId");

        builder.HasIndex(c => c.ConditionName)
            .HasDatabaseName("IX_ConsolidatedConditions_ConditionName");

        builder.HasIndex(c => c.ICD10Code)
            .HasDatabaseName("IX_ConsolidatedConditions_ICD10Code");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_ConsolidatedConditions_Status");
    }
}
