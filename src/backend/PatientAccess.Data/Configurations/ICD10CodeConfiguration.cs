using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// Entity configuration for ICD10Code with pgvector support (AIR-R04, DR-010).
/// </summary>
public class ICD10CodeConfiguration : IEntityTypeConfiguration<ICD10Code>
{
    public void Configure(EntityTypeBuilder<ICD10Code> builder)
    {
        builder.ToTable("ICD10Codes");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(i => i.Code)
            .IsUnique();

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(i => i.Category)
            .IsRequired()
            .HasMaxLength(200);

        // Configure Vector embedding for pgvector
        builder.Property(i => i.Embedding)
            .HasColumnType("vector(1536)");

        builder.Property(i => i.ChunkText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.Metadata)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(i => i.Version)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(i => i.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(i => i.UpdatedAt);

        builder.HasIndex(i => i.IsActive);
        builder.HasIndex(i => i.Category);
    }
}
