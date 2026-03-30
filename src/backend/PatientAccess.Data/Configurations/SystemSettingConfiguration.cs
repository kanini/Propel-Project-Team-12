using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the SystemSetting entity (US_037).
/// Implements key-value store for admin-configurable system settings.
/// </summary>
public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(s => s.SystemSettingId);
        builder.Property(s => s.SystemSettingId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Description)
            .HasColumnType("text");

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(s => s.UpdatedAt)
            .HasColumnType("timestamptz");

        // Unique index on Key for fast lookups
        builder.HasIndex(s => s.Key)
            .IsUnique()
            .HasDatabaseName("IX_SystemSettings_Key");
    }
}
