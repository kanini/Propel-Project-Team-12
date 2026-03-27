using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core configuration for DataConflict entity (FR-031).
/// Configures conflict resolution workflow and JSONB columns for conflicting data IDs.
/// </summary>
public class DataConflictConfiguration : IEntityTypeConfiguration<DataConflict>
{
    public void Configure(EntityTypeBuilder<DataConflict> builder)
    {
        builder.ToTable("DataConflicts");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(d => d.ConflictType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.EntityId)
            .IsRequired();

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(2000);

        // JSONB column for conflicting source data IDs
        builder.Property(d => d.SourceDataIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(d => d.ResolutionStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.ResolvedAt)
            .HasColumnType("timestamptz");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // FK: DataConflict -> PatientProfile — CASCADE on delete
        builder.HasOne(d => d.PatientProfile)
            .WithMany(p => p.Conflicts)
            .HasForeignKey(d => d.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DataConflicts_PatientProfiles");

        // FK: DataConflict -> User (resolver) — SET NULL on delete
        builder.HasOne(d => d.Resolver)
            .WithMany()
            .HasForeignKey(d => d.ResolvedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_DataConflicts_ResolvedBy");

        // Indexes for conflict filtering and resolution workflow
        builder.HasIndex(d => d.PatientProfileId)
            .HasDatabaseName("IX_DataConflicts_PatientProfileId");

        builder.HasIndex(d => d.ResolutionStatus)
            .HasDatabaseName("IX_DataConflicts_ResolutionStatus");

        builder.HasIndex(d => d.EntityType)
            .HasDatabaseName("IX_DataConflicts_EntityType");
    }
}
