using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the ExtractedClinicalData entity (DR-004, DR-010).
/// Includes indexes for patient reference and data type for query performance.
/// </summary>
public class ExtractedClinicalDataConfiguration : IEntityTypeConfiguration<ExtractedClinicalData>
{
    public void Configure(EntityTypeBuilder<ExtractedClinicalData> builder)
    {
        builder.ToTable("ExtractedClinicalData");

        builder.HasKey(e => e.ExtractedDataId);
        builder.Property(e => e.ExtractedDataId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.DataType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.DataKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.DataValue)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(e => e.ConfidenceScore)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.VerificationStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.SourceTextExcerpt)
            .HasColumnType("text");

        builder.Property(e => e.VerifiedAt)
            .HasColumnType("timestamptz");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: ExtractedClinicalData -> ClinicalDocument — CASCADE on delete (data meaningless without source)
        builder.HasOne(e => e.Document)
            .WithMany(d => d.ExtractedData)
            .HasForeignKey(e => e.DocumentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ExtractedData_Documents");

        // FK: ExtractedClinicalData -> User (patient) — CASCADE on delete
        builder.HasOne(e => e.Patient)
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ExtractedData_Patients");

        // FK: ExtractedClinicalData -> User (verifier) — SET NULL on delete
        builder.HasOne(e => e.Verifier)
            .WithMany()
            .HasForeignKey(e => e.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_ExtractedData_VerifiedBy");

        builder.HasIndex(e => e.DocumentId)
            .HasDatabaseName("IX_ExtractedData_DocumentId");

        builder.HasIndex(e => e.PatientId)
            .HasDatabaseName("IX_ExtractedData_PatientId");

        builder.HasIndex(e => e.DataType)
            .HasDatabaseName("IX_ExtractedData_DataType");

        builder.HasIndex(e => e.VerificationStatus)
            .HasDatabaseName("IX_ExtractedData_VerificationStatus");
    }
}
