using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for CalendarIntegration entity (US_039).
/// Enforces unique composite index on (UserId, Provider) for one connection per provider per user.
/// Token columns sized for encrypted JWT-like tokens (2048 max) for OWASP compliance.
/// </summary>
public class CalendarIntegrationConfiguration : IEntityTypeConfiguration<CalendarIntegration>
{
    public void Configure(EntityTypeBuilder<CalendarIntegration> builder)
    {
        builder.ToTable("CalendarIntegrations");

        builder.HasKey(c => c.CalendarIntegrationId);
        builder.Property(c => c.CalendarIntegrationId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.Provider)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(c => c.AccessToken)
            .IsRequired()
            .HasMaxLength(2048)
            .HasColumnType("varchar(2048)");

        builder.Property(c => c.RefreshToken)
            .IsRequired()
            .HasMaxLength(2048)
            .HasColumnType("varchar(2048)");

        builder.Property(c => c.TokenExpiry)
            .IsRequired()
            .HasColumnType("timestamptz");

        builder.Property(c => c.CalendarId)
            .HasMaxLength(256)
            .HasColumnType("varchar(256)");

        builder.Property(c => c.IsConnected)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: CalendarIntegration -> User — CASCADE on delete
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_CalendarIntegrations_Users");

        // Unique composite index on (UserId, Provider) to enforce one connection per provider per user
        builder.HasIndex(c => new { c.UserId, c.Provider })
            .IsUnique()
            .HasDatabaseName("IX_CalendarIntegrations_UserId_Provider");

        builder.HasIndex(c => c.IsConnected)
            .HasDatabaseName("IX_CalendarIntegrations_IsConnected");
    }
}
