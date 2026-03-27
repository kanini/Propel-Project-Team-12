using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core configuration for ConsolidatedEncounter entity (FR-030).
/// Configures encounter type classification and JSONB columns for source traceability.
/// </summary>
public class ConsolidatedEncounterConfiguration : IEntityTypeConfiguration<ConsolidatedEncounter>
{
    public void Configure(EntityTypeBuilder<ConsolidatedEncounter> builder)
    {
        builder.ToTable("ConsolidatedEncounters");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.EncounterDate)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(e => e.EncounterType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Provider)
            .HasMaxLength(200);

        builder.Property(e => e.Facility)
            .HasMaxLength(200);

        builder.Property(e => e.ChiefComplaint)
            .HasMaxLength(1000);

        // JSONB columns for source traceability
        builder.Property(e => e.SourceDocumentIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.SourceDataIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.IsDuplicate)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DuplicateCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // FK: ConsolidatedEncounter -> PatientProfile — CASCADE on delete
        builder.HasOne(e => e.PatientProfile)
            .WithMany(p => p.Encounters)
            .HasForeignKey(e => e.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConsolidatedEncounters_PatientProfiles");

        // Indexes for patient queries and encounter date filtering
        builder.HasIndex(e => e.PatientProfileId)
            .HasDatabaseName("IX_ConsolidatedEncounters_PatientProfileId");

        builder.HasIndex(e => e.EncounterDate)
            .HasDatabaseName("IX_ConsolidatedEncounters_EncounterDate");
    }
}
