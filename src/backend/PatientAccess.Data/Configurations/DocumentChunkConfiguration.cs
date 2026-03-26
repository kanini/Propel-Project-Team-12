using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the DocumentChunk entity (AIR-R01, AIR-R04).
/// Configures staging table for pre-embedding chunks with indexes for efficient querying.
/// </summary>
public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.CodeSystem)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.SourceText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.TokenCount)
            .IsRequired();

        builder.Property(e => e.ChunkIndex)
            .IsRequired();

        builder.Property(e => e.StartToken)
            .IsRequired();

        builder.Property(e => e.EndToken)
            .IsRequired();

        builder.Property(e => e.OverlapWithPrevious)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.TargetEntityId)
            .IsRequired(false);

        builder.Property(e => e.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.ProcessedAt)
            .HasColumnType("timestamptz");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        // B-tree index on CodeSystem for filtering by code system type
        builder.HasIndex(e => e.CodeSystem)
            .HasDatabaseName("IX_DocumentChunks_CodeSystem");

        // B-tree index on IsProcessed for querying pending chunks
        builder.HasIndex(e => e.IsProcessed)
            .HasDatabaseName("IX_DocumentChunks_IsProcessed");

        // B-tree index on TargetEntityId for FK lookup after embedding
        builder.HasIndex(e => e.TargetEntityId)
            .HasDatabaseName("IX_DocumentChunks_TargetEntityId");

        // Composite index for efficient pending chunk queries by code system
        builder.HasIndex(e => new { e.CodeSystem, e.IsProcessed })
            .HasDatabaseName("IX_DocumentChunks_CodeSystem_IsProcessed");
    }
}
