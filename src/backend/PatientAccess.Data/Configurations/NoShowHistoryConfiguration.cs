using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the NoShowHistory entity (DR-016).
/// Unique constraint on patient_id — one record per patient.
/// </summary>
public class NoShowHistoryConfiguration : IEntityTypeConfiguration<NoShowHistory>
{
    public void Configure(EntityTypeBuilder<NoShowHistory> builder)
    {
        builder.ToTable("NoShowHistory");

        builder.HasKey(n => n.NoShowHistoryId);
        builder.Property(n => n.NoShowHistoryId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(n => n.TotalAppointments)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.NoShowCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.ConfirmationResponseRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(n => n.AverageLeadTimeHours)
            .HasColumnType("decimal(10,2)");

        builder.Property(n => n.LastCalculatedRiskScore)
            .HasColumnType("decimal(5,2)");

        builder.Property(n => n.LastCalculatedAt)
            .HasColumnType("timestamptz");

        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(n => n.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: NoShowHistory -> User (patient, CASCADE)
        builder.HasOne(n => n.Patient)
            .WithMany()
            .HasForeignKey(n => n.PatientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_NoShowHistory_Patients");

        // Unique constraint: one NoShowHistory per patient
        builder.HasIndex(n => n.PatientId)
            .IsUnique()
            .HasDatabaseName("IX_NoShowHistory_PatientId");
    }
}
