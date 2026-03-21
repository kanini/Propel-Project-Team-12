using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the User entity (DR-001).
/// Defines column types, max lengths, required fields, and indexes.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.DateOfBirth)
            .HasColumnType("date");

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.VerificationToken)
            .HasMaxLength(255);

        builder.Property(u => u.VerificationTokenExpiry)
            .HasColumnType("timestamptz");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("timestamptz");

        // Unique index on Email (DR-001)
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("IX_Users_Role");

        builder.HasIndex(u => u.Status)
            .HasDatabaseName("IX_Users_Status");
    }
}
