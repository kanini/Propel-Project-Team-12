using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the ClinicalTerminology entity (DR-010, AIR-R04).
/// Configures pgvector embedding column, JSONB metadata and synonyms, and indexes for hybrid retrieval.
/// </summary>
public class ClinicalTerminologyConfiguration : IEntityTypeConfiguration<ClinicalTerminology>
{
    public void Configure(EntityTypeBuilder<ClinicalTerminology> builder)
    {
        builder.ToTable("ClinicalTerminology");

        // Primary Key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Term - B-tree index for exact matching
        builder.Property(t => t.Term)
            .IsRequired()
            .HasMaxLength(500);
        builder.HasIndex(t => t.Term)
            .HasDatabaseName("IX_ClinicalTerminology_Term");

        // Category - B-tree index for filtering
        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(t => t.Category)
            .HasDatabaseName("IX_ClinicalTerminology_Category");

        // Synonyms - JSONB array format: ["T2DM", "NIDDM"]
        builder.Property(t => t.Synonyms)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        // Vector Embedding - 1536 dimensions, HNSW index for cosine similarity (AIR-R04)
        builder.Property(t => t.Embedding)
            .HasColumnType("vector(1536)");
        
        // HNSW index for fast similarity search using cosine distance
        builder.HasIndex(t => t.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops")
            .HasDatabaseName("IX_ClinicalTerminology_Embedding_Cosine");

        // ChunkText
        builder.Property(t => t.ChunkText)
            .IsRequired()
            .HasMaxLength(2000);

        // Metadata - JSONB with GIN index for keyword search
        builder.Property(t => t.Metadata)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");
        builder.HasIndex(t => t.Metadata)
            .HasMethod("gin")
            .HasDatabaseName("IX_ClinicalTerminology_Metadata_Gin");

        // Source
        builder.Property(t => t.Source)
            .IsRequired()
            .HasMaxLength(100);

        // IsActive - B-tree index for filtering active terms
        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("IX_ClinicalTerminology_IsActive");

        // Timestamps
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("timestamptz");
    }
}
