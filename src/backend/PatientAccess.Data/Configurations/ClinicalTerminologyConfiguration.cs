using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// Entity configuration for ClinicalTerminology with pgvector support (AIR-R04, DR-010).
/// </summary>
public class ClinicalTerminologyConfiguration : IEntityTypeConfiguration<ClinicalTerminology>
{
    public void Configure(EntityTypeBuilder<ClinicalTerminology> builder)
    {
        builder.ToTable("ClinicalTerminology");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Term)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.Term);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(200);

        // Configure Vector embedding for pgvector
        builder.Property(t => t.Embedding)
            .HasColumnType("vector(1536)");

        builder.Property(t => t.ChunkText)
            .IsRequired()
            .HasMaxLength(3000);

        builder.Property(t => t.Metadata)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.UpdatedAt);

        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => t.Category);
    }
}
