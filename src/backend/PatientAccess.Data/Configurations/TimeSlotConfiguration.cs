using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the TimeSlot entity (DR-002).
/// Includes optimistic concurrency token for concurrent booking protection.
/// Updated for US_024 - relationship with Appointment handled in AppointmentConfiguration.
/// </summary>
public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.ToTable("TimeSlots");

        builder.HasKey(t => t.TimeSlotId);
        builder.Property(t => t.TimeSlotId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.StartTime)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(t => t.EndTime)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(t => t.IsBooked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("timestamptz");

        // Optimistic concurrency token for concurrent booking protection
        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        // FK: TimeSlot -> Provider (CASCADE on delete)
        builder.HasOne(t => t.Provider)
            .WithMany(p => p.TimeSlots)
            .HasForeignKey(t => t.ProviderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_TimeSlots_Providers");

        // Note: TimeSlot -> Appointment relationship is now defined in AppointmentConfiguration
        // AppointmentId column still exists for legacy compatibility but relationship is inverse

        builder.HasIndex(t => t.ProviderId)
            .HasDatabaseName("IX_TimeSlots_ProviderId");

        builder.HasIndex(t => t.StartTime)
            .HasDatabaseName("IX_TimeSlots_StartTime");

        builder.HasIndex(t => t.IsBooked)
            .HasDatabaseName("IX_TimeSlots_IsBooked");
    }
}
