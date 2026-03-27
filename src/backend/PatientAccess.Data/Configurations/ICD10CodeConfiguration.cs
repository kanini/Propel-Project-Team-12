using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the ICD10Code entity (DR-010, AIR-R04).
/// Configures pgvector embedding column, JSONB metadata, and indexes for hybrid retrieval.
/// </summary>
public class ICD10CodeConfiguration : IEntityTypeConfiguration<ICD10Code>
{
    public void Configure(EntityTypeBuilder<ICD10Code> builder)
    {
        builder.ToTable("ICD10Codes");

        // Primary Key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Code - Unique B-tree index for exact matching
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(20);
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("IX_ICD10Codes_Code");

        // Description
        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(1000);

        // Category - B-tree index for filtering
        builder.Property(c => c.Category)
            .IsRequired()
            .HasMaxLength(200);
        builder.HasIndex(c => c.Category)
            .HasDatabaseName("IX_ICD10Codes_Category");

        // ChapterCode
        builder.Property(c => c.ChapterCode)
            .IsRequired()
            .HasMaxLength(10);

        // Vector Embedding - 1536 dimensions, HNSW index for cosine similarity (AIR-R04)
        builder.Property(c => c.Embedding)
            .HasColumnType("vector(1536)");
        
        // HNSW index for fast similarity search using cosine distance
        builder.HasIndex(c => c.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops")
            .HasDatabaseName("IX_ICD10Codes_Embedding_Cosine");

        // ChunkText
        builder.Property(c => c.ChunkText)
            .IsRequired()
            .HasMaxLength(2000);

        // Metadata - JSONB with GIN index for keyword search
        builder.Property(c => c.Metadata)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");
        builder.HasIndex(c => c.Metadata)
            .HasMethod("gin")
            .HasDatabaseName("IX_ICD10Codes_Metadata_Gin");

        // Version
        builder.Property(c => c.Version)
            .IsRequired()
            .HasMaxLength(20);

        // IsActive - B-tree index for filtering active codes
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_ICD10Codes_IsActive");

        // Timestamps
        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("timestamptz");
    }
}
