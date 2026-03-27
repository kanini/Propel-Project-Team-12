using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core configuration for PatientProfile entity (FR-030, FR-032).
/// Configures one-to-one relationship with User (patient) and CHECK constraint for ProfileCompleteness.
/// </summary>
public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> builder)
    {
        builder.ToTable("PatientProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.LastAggregatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(p => p.TotalDocumentsProcessed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.HasUnresolvedConflicts)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.ProfileCompleteness)
            .IsRequired()
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("timestamptz");

        // One-to-One: PatientProfile -> User (patient) — CASCADE on delete
        builder.HasOne(p => p.Patient)
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PatientProfiles_Patients");

        // Unique index on PatientId (one profile per patient)
        builder.HasIndex(p => p.PatientId)
            .IsUnique()
            .HasDatabaseName("IX_PatientProfiles_PatientId");

        // CHECK constraint: ProfileCompleteness BETWEEN 0 AND 100
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_PatientProfiles_ProfileCompleteness",
            "\"ProfileCompleteness\" >= 0 AND \"ProfileCompleteness\" <= 100"));
    }
}
