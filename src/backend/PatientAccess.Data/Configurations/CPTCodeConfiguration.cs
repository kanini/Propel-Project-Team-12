using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// Entity configuration for CPTCode with pgvector support (AIR-R04, DR-010).
/// </summary>
public class CPTCodeConfiguration : IEntityTypeConfiguration<CPTCode>
{
    public void Configure(EntityTypeBuilder<CPTCode> builder)
    {
        builder.ToTable("CPTCodes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.Category)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Modifier)
            .HasMaxLength(50);

        // Configure Vector embedding for pgvector - CRITICAL FIX
        builder.Property(c => c.Embedding)
            .HasColumnType("vector(1536)");

        builder.Property(c => c.ChunkText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.Metadata)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(c => c.Version)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt);

        // Indexes
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.Category);
        builder.HasIndex(c => c.Version);
    }
}
