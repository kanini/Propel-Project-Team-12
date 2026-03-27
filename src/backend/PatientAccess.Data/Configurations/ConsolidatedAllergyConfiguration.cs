using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core configuration for ConsolidatedAllergy entity (FR-030).
/// Configures severity classification and JSONB columns for source traceability.
/// </summary>
public class ConsolidatedAllergyConfiguration : IEntityTypeConfiguration<ConsolidatedAllergy>
{
    public void Configure(EntityTypeBuilder<ConsolidatedAllergy> builder)
    {
        builder.ToTable("ConsolidatedAllergies");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.AllergenName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Reaction)
            .HasMaxLength(1000);

        builder.Property(a => a.Severity)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.OnsetDate)
            .HasColumnType("timestamptz");

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(50);

        // JSONB columns for source traceability
        builder.Property(a => a.SourceDocumentIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.SourceDataIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.IsDuplicate)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.DuplicateCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(a => a.FirstRecordedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(a => a.LastUpdatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // FK: ConsolidatedAllergy -> PatientProfile — CASCADE on delete
        builder.HasOne(a => a.PatientProfile)
            .WithMany(p => p.Allergies)
            .HasForeignKey(a => a.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConsolidatedAllergies_PatientProfiles");

        // Indexes for patient queries and severity filtering
        builder.HasIndex(a => a.PatientProfileId)
            .HasDatabaseName("IX_ConsolidatedAllergies_PatientProfileId");

        builder.HasIndex(a => a.AllergenName)
            .HasDatabaseName("IX_ConsolidatedAllergies_AllergenName");

        builder.HasIndex(a => a.Severity)
            .HasDatabaseName("IX_ConsolidatedAllergies_Severity");
    }
}
