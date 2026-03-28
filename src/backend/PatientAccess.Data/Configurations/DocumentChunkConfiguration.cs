using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;
using Pgvector.EntityFrameworkCore;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// Entity configuration for DocumentChunk with pgvector support (AIR-R04, DR-010).
/// </summary>
public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentId)
            .IsRequired();

        builder.Property(d => d.ChunkIndex)
            .IsRequired();

        builder.Property(d => d.ChunkText)
            .IsRequired()
            .HasMaxLength(4000);

        // Configure Vector embedding for pgvector
        builder.Property(d => d.Embedding)
            .HasColumnType("vector(1536)")
            .HasDefaultValueSql("vector[]::real[]");

        builder.Property(d => d.Metadata)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(d => d.DocumentId);
        builder.HasIndex(d => new { d.DocumentId, d.ChunkIndex });
    }
}
