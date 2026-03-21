using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Provider entity (DR-002).
/// </summary>
public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");

        builder.HasKey(p => p.ProviderId);
        builder.Property(p => p.ProviderId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Specialty)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Email)
            .HasMaxLength(255);

        builder.Property(p => p.Phone)
            .HasMaxLength(20);

        builder.Property(p => p.LicenseNumber)
            .HasMaxLength(50);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("timestamptz");

        builder.HasIndex(p => p.Specialty)
            .HasDatabaseName("IX_Providers_Specialty");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Providers_IsActive");
    }
}
