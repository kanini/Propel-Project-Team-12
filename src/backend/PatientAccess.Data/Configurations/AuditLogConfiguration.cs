using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the AuditLog entity (DR-005, DR-006).
/// Immutable: database triggers prevent UPDATE and DELETE.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.AuditLogId);
        builder.Property(a => a.AuditLogId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.ActionType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ActionDetails)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.Timestamp)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // FK: AuditLog -> User — RESTRICT on delete (preserve audit trail)
        // Nullable to support logging failed login attempts for non-existent users
        builder.HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false)
            .HasConstraintName("FK_AuditLogs_Users");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        builder.HasIndex(a => a.ResourceType)
            .HasDatabaseName("IX_AuditLogs_ResourceType");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        builder.HasIndex(a => a.ActionType)
            .HasDatabaseName("IX_AuditLogs_ActionType");
    }
}
