using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the ClinicalDocument entity (DR-003).
/// </summary>
public class ClinicalDocumentConfiguration : IEntityTypeConfiguration<ClinicalDocument>
{
    public void Configure(EntityTypeBuilder<ClinicalDocument> builder)
    {
        builder.ToTable("ClinicalDocuments");

        builder.HasKey(d => d.DocumentId);
        builder.Property(d => d.DocumentId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.FileSize)
            .IsRequired();

        builder.Property(d => d.FileType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.ProcessingStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.UploadedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(d => d.ProcessedAt)
            .HasColumnType("timestamptz");

        builder.Property(d => d.ErrorMessage)
            .HasColumnType("text");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(d => d.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: ClinicalDocument -> User (patient) — CASCADE on delete
        builder.HasOne(d => d.Patient)
            .WithMany(u => u.ClinicalDocuments)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ClinicalDocuments_Patients");

        builder.HasIndex(d => d.PatientId)
            .HasDatabaseName("IX_ClinicalDocuments_PatientId");

        builder.HasIndex(d => d.ProcessingStatus)
            .HasDatabaseName("IX_ClinicalDocuments_ProcessingStatus");

        builder.HasIndex(d => d.UploadedAt)
            .HasDatabaseName("IX_ClinicalDocuments_UploadedAt");
    }
}
