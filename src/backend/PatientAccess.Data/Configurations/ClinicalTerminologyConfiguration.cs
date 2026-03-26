using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the ClinicalTerminology entity (AIR-R04, DR-010).
/// Configures vector embeddings with HNSW indexing for semantic similarity search,
/// JSONB metadata with GIN indexing, and indexes for term and category lookups.
/// </summary>
public class ClinicalTerminologyConfiguration : IEntityTypeConfiguration<ClinicalTerminology>
{
    public void Configure(EntityTypeBuilder<ClinicalTerminology> builder)
    {
        builder.ToTable("ClinicalTerminology");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Term)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(100);

        // Configure Synonyms as JSONB array
        builder.Property(e => e.Synonyms)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        // Configure vector embedding column (1536 dimensions for text-embedding-3-small)
        builder.Property(e => e.Embedding)
            .HasColumnType("vector(1536)");

        builder.Property(e => e.ChunkText)
            .IsRequired()
            .HasMaxLength(2000);

        // Configure JSONB metadata column
        builder.Property(e => e.Metadata)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(e => e.Source)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamptz");

        // B-tree index on Term for text search
        builder.HasIndex(e => e.Term)
            .HasDatabaseName("IX_ClinicalTerminology_Term");

        // B-tree index on IsActive for filtering active terms
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_ClinicalTerminology_IsActive");

        // B-tree index on Category for categorization queries
        builder.HasIndex(e => e.Category)
            .HasDatabaseName("IX_ClinicalTerminology_Category");

        // B-tree index on Source for data lineage tracking
        builder.HasIndex(e => e.Source)
            .HasDatabaseName("IX_ClinicalTerminology_Source");

        // HNSW index on Embedding for fast cosine similarity search
        // Note: HNSW index creation is deferred to raw SQL in migration due to EF Core limitations
        // Migration will include: CREATE INDEX ... USING hnsw (Embedding vector_cosine_ops);
        builder.HasIndex(e => e.Embedding)
            .HasDatabaseName("IX_ClinicalTerminology_Embedding_HNSW");

        // GIN index on Metadata JSONB for efficient keyword search
        // Note: GIN index creation is deferred to raw SQL in migration
        // Migration will include: CREATE INDEX ... USING gin (Metadata);
        builder.HasIndex(e => e.Metadata)
            .HasDatabaseName("IX_ClinicalTerminology_Metadata_GIN");

        // GIN index on Synonyms JSONB array for synonym search
        builder.HasIndex(e => e.Synonyms)
            .HasDatabaseName("IX_ClinicalTerminology_Synonyms_GIN");
    }
}
