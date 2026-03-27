using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core configuration for ConsolidatedMedication entity (FR-030, FR-031).
/// Includes conflict detection flag and JSONB columns for source traceability.
/// </summary>
public class ConsolidatedMedicationConfiguration : IEntityTypeConfiguration<ConsolidatedMedication>
{
    public void Configure(EntityTypeBuilder<ConsolidatedMedication> builder)
    {
        builder.ToTable("ConsolidatedMedications");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(m => m.DrugName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Dosage)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Frequency)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.RouteOfAdministration)
            .HasMaxLength(100);

        builder.Property(m => m.StartDate)
            .HasColumnType("timestamptz");

        builder.Property(m => m.EndDate)
            .HasColumnType("timestamptz");

        builder.Property(m => m.Status)
            .IsRequired()
            .HasMaxLength(50);

        // JSONB columns for source traceability
        builder.Property(m => m.SourceDocumentIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(m => m.SourceDataIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(m => m.IsDuplicate)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.DuplicateCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.HasConflict)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.FirstRecordedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(m => m.LastUpdatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // FK: ConsolidatedMedication -> PatientProfile — CASCADE on delete
        builder.HasOne(m => m.PatientProfile)
            .WithMany(p => p.Medications)
            .HasForeignKey(m => m.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConsolidatedMedications_PatientProfiles");

        // Indexes for patient queries and conflict detection
        builder.HasIndex(m => m.PatientProfileId)
            .HasDatabaseName("IX_ConsolidatedMedications_PatientProfileId");

        builder.HasIndex(m => m.DrugName)
            .HasDatabaseName("IX_ConsolidatedMedications_DrugName");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("IX_ConsolidatedMedications_Status");

        builder.HasIndex(m => m.HasConflict)
            .HasDatabaseName("IX_ConsolidatedMedications_HasConflict");
    }
}
