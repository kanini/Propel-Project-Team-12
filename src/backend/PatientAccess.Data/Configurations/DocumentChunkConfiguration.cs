using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;
using Pgvector.EntityFrameworkCore;

namespace PatientAccess.Data.Configurations;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(c => c.ChunkId);
        builder.Property(c => c.ChunkId).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.ChunkIndex).IsRequired();

        builder.Property(c => c.ChunkText).IsRequired().HasColumnType("text");

        builder.Property(c => c.TokenCount).IsRequired();

        builder.Property(c => c.Embedding);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(c => c.Document)
            .WithMany()
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DocumentChunks_Documents");

        builder.HasIndex(c => c.DocumentId).HasDatabaseName("IX_DocumentChunks_DocumentId");
    }
}
