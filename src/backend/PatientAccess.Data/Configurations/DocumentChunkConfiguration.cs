using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core configuration for DocumentChunk entity.
/// Implements staging table for pre-embedding chunks (AIR-R01: 512-token chunks with 64-token overlap).
/// </summary>
public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CodeSystem)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.SourceText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.TokenCount)
            .IsRequired();

        builder.Property(c => c.ChunkIndex)
            .IsRequired();

        builder.Property(c => c.StartToken)
            .IsRequired();

        builder.Property(c => c.EndToken)
            .IsRequired();

        builder.Property(c => c.OverlapWithPrevious)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.TargetEntityId)
            .IsRequired(false);

        builder.Property(c => c.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.ProcessedAt)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Indices for query performance
        builder.HasIndex(c => c.CodeSystem)
            .HasDatabaseName("IX_DocumentChunks_CodeSystem");

        builder.HasIndex(c => c.IsProcessed)
            .HasDatabaseName("IX_DocumentChunks_IsProcessed");

        builder.HasIndex(c => c.TargetEntityId)
            .HasDatabaseName("IX_DocumentChunks_TargetEntityId");

        // Composite index for common query pattern (CodeSystem + IsProcessed)
        builder.HasIndex(c => new { c.CodeSystem, c.IsProcessed })
            .HasDatabaseName("IX_DocumentChunks_CodeSystem_IsProcessed");
    }
}
