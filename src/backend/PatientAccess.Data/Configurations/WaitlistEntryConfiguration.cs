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

        // US_041 - Waitlist notification lifecycle fields
        builder.Property(w => w.NotifiedAt)
            .HasColumnType("timestamptz");

        builder.Property(w => w.ResponseToken)
            .HasMaxLength(64);

        builder.Property(w => w.ResponseDeadline)
            .HasColumnType("timestamptz");

        builder.Property(w => w.NotifiedSlotId);

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

        // US_041 - FK: WaitlistEntry -> TimeSlot (notified slot)
        builder.HasOne(w => w.NotifiedSlot)
            .WithMany()
            .HasForeignKey(w => w.NotifiedSlotId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_WaitlistEntries_NotifiedSlot");

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

        // US_041 - Unique filtered index on ResponseToken for O(1) lookup from confirm/decline URLs
        builder.HasIndex(w => w.ResponseToken)
            .IsUnique()
            .HasFilter("\"ResponseToken\" IS NOT NULL")
            .HasDatabaseName("IX_WaitlistEntries_ResponseToken");

        // US_041 - Partial index for timeout detection job (find Notified entries past deadline)
        builder.HasIndex(w => new { w.Status, w.ResponseDeadline })
            .HasFilter("\"Status\" = 2") // WaitlistStatus.Notified = 2
            .HasDatabaseName("IX_WaitlistEntries_Status_ResponseDeadline");
    }
}
