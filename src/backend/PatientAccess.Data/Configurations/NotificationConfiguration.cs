using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Notification entity (DR-014).
/// Indexes on recipient_id, scheduled_time, and status for efficient pending notification queries.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.NotificationId);
        builder.Property(n => n.NotificationId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(n => n.ChannelType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.TemplateName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.ScheduledTime)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(n => n.SentTime)
            .HasColumnType("timestamptz");

        builder.Property(n => n.DeliveryConfirmation)
            .HasColumnType("timestamptz");

        builder.Property(n => n.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.LastErrorMessage)
            .HasColumnType("text");

        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(n => n.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: Notification -> User (recipient)
        builder.HasOne(n => n.Recipient)
            .WithMany()
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Notifications_Recipients");

        // FK: Notification -> Appointment (nullable, SET NULL)
        builder.HasOne(n => n.Appointment)
            .WithMany()
            .HasForeignKey(n => n.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Notifications_Appointments");

        builder.HasIndex(n => n.RecipientId)
            .HasDatabaseName("IX_Notifications_RecipientId");

        builder.HasIndex(n => n.AppointmentId)
            .HasDatabaseName("IX_Notifications_AppointmentId");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("IX_Notifications_Status");

        builder.HasIndex(n => n.ScheduledTime)
            .HasDatabaseName("IX_Notifications_ScheduledTime");

        // US_037: Composite index for efficient pending reminder queries
        builder.HasIndex(n => new { n.Status, n.ScheduledTime })
            .HasDatabaseName("IX_Notifications_Status_ScheduledTime");
    }
}
