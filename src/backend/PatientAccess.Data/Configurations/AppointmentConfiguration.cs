using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Appointment entity (DR-002).
/// Defines FK relationships with RESTRICT on Provider delete, CASCADE on Patient delete.
/// </summary>
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.AppointmentId);
        builder.Property(a => a.AppointmentId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.ScheduledDateTime)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.VisitReason)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(a => a.IsWalkIn)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.ConfirmationReceived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.NoShowRiskScore)
            .HasColumnType("decimal(5,2)");

        builder.Property(a => a.CancellationNoticeHours)
            .HasDefaultValue(24);

        builder.Property(a => a.Notes)
            .HasColumnType("text");

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: Appointment -> User (patient) — CASCADE on delete
        builder.HasOne(a => a.Patient)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Appointments_Patients");

        // FK: Appointment -> Provider — RESTRICT on delete (prevent orphaned appointments)
        builder.HasOne(a => a.Provider)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Appointments_Providers");

        builder.HasIndex(a => a.PatientId)
            .HasDatabaseName("IX_Appointments_PatientId");

        builder.HasIndex(a => a.ProviderId)
            .HasDatabaseName("IX_Appointments_ProviderId");

        builder.HasIndex(a => a.ScheduledDateTime)
            .HasDatabaseName("IX_Appointments_ScheduledDateTime");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_Appointments_Status");
    }
}
