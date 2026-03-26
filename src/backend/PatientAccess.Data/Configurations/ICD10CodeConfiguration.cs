using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the ICD10Code entity (AIR-R04, DR-010).
/// Configures vector embeddings with HNSW indexing for semantic similarity search,
/// JSONB metadata with GIN indexing, and unique constraints on code values.
/// </summary>
public class ICD10CodeConfiguration : IEntityTypeConfiguration<ICD10Code>
{
    public void Configure(EntityTypeBuilder<ICD10Code> builder)
    {
        builder.ToTable("ICD10Codes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ChapterCode)
            .IsRequired()
            .HasMaxLength(10);

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

        builder.Property(e => e.Version)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamptz");

        // Unique index on Code for fast exact lookups and uniqueness enforcement
        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_ICD10Codes_Code_Unique");

        // B-tree index on IsActive for filtering active codes
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_ICD10Codes_IsActive");

        // B-tree index on Category for categorization queries
        builder.HasIndex(e => e.Category)
            .HasDatabaseName("IX_ICD10Codes_Category");

        // HNSW index on Embedding for fast cosine similarity search
        // Note: HNSW index creation is deferred to raw SQL in migration due to EF Core limitations
        // Migration will include: CREATE INDEX ... USING hnsw (Embedding vector_cosine_ops);
        builder.HasIndex(e => e.Embedding)
            .HasDatabaseName("IX_ICD10Codes_Embedding_HNSW");

        // GIN index on Metadata JSONB for efficient keyword search
        // Note: GIN index creation is deferred to raw SQL in migration
        // Migration will include: CREATE INDEX ... USING gin (Metadata);
        builder.HasIndex(e => e.Metadata)
            .HasDatabaseName("IX_ICD10Codes_Metadata_GIN");
    }
}
