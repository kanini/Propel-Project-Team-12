using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the IntakeRecord entity (DR-012).
/// JSONB columns for structured health data.
/// </summary>
public class IntakeRecordConfiguration : IEntityTypeConfiguration<IntakeRecord>
{
    public void Configure(EntityTypeBuilder<IntakeRecord> builder)
    {
        builder.ToTable("IntakeRecords");

        builder.HasKey(i => i.IntakeRecordId);
        builder.Property(i => i.IntakeRecordId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.IntakeMode)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.ChiefComplaint)
            .HasColumnType("text");

        builder.Property(i => i.SymptomHistory)
            .HasColumnType("jsonb");

        builder.Property(i => i.CurrentMedications)
            .HasColumnType("jsonb");

        builder.Property(i => i.KnownAllergies)
            .HasColumnType("jsonb");

        builder.Property(i => i.MedicalHistory)
            .HasColumnType("jsonb");

        builder.Property(i => i.InsuranceValidationStatus)
            .HasConversion<int?>();

        builder.Property(i => i.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.CompletedAt)
            .HasColumnType("timestamptz");

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(i => i.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: IntakeRecord -> Appointment (CASCADE)
        builder.HasOne(i => i.Appointment)
            .WithMany()
            .HasForeignKey(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_IntakeRecords_Appointments");

        // FK: IntakeRecord -> User (patient, CASCADE)
        builder.HasOne(i => i.Patient)
            .WithMany()
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_IntakeRecords_Patients");

        // FK: IntakeRecord -> InsuranceRecord (nullable, SET NULL)
        builder.HasOne(i => i.ValidatedInsuranceRecord)
            .WithMany()
            .HasForeignKey(i => i.ValidatedInsuranceRecordId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_IntakeRecords_InsuranceRecords");

        // Unique: one intake per appointment
        builder.HasIndex(i => i.AppointmentId)
            .IsUnique()
            .HasDatabaseName("IX_IntakeRecords_AppointmentId");

        builder.HasIndex(i => i.PatientId)
            .HasDatabaseName("IX_IntakeRecords_PatientId");

        builder.HasIndex(i => i.IsCompleted)
            .HasDatabaseName("IX_IntakeRecords_IsCompleted");
    }
}
