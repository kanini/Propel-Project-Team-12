using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the WaitlistEntry entity (DR-011).
/// </summary>
public class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.ToTable("WaitlistEntries");

        builder.HasKey(w => w.WaitlistEntryId);
        builder.Property(w => w.WaitlistEntryId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(w => w.PreferredDateStart)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(w => w.PreferredDateEnd)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(w => w.PreferredTimeOfDay)
            .HasConversion<int?>();

        builder.Property(w => w.NotificationPreference)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(w => w.Priority)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(w => w.Reason)
            .HasColumnType("text");

        builder.Property(w => w.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(w => w.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: WaitlistEntry -> User (patient)
        builder.HasOne(w => w.Patient)
            .WithMany()
            .HasForeignKey(w => w.PatientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WaitlistEntries_Patients");

        // FK: WaitlistEntry -> Provider
        builder.HasOne(w => w.Provider)
            .WithMany()
            .HasForeignKey(w => w.ProviderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WaitlistEntries_Providers");

        builder.HasIndex(w => w.PatientId)
            .HasDatabaseName("IX_WaitlistEntries_PatientId");

        builder.HasIndex(w => w.ProviderId)
            .HasDatabaseName("IX_WaitlistEntries_ProviderId");

        builder.HasIndex(w => w.Status)
            .HasDatabaseName("IX_WaitlistEntries_Status");

        // Composite index for priority-based ordering
        builder.HasIndex(w => new { w.Priority, w.CreatedAt })
            .IsDescending(true, false)
            .HasDatabaseName("IX_WaitlistEntries_Priority_CreatedAt");

        // Unique constraint to prevent duplicate waitlist entries per patient/provider/date
        builder.HasIndex(w => new { w.PatientId, w.ProviderId, w.PreferredDateStart })
            .IsUnique()
            .HasDatabaseName("IX_WaitlistEntries_Patient_Provider_Date");
    }
}
