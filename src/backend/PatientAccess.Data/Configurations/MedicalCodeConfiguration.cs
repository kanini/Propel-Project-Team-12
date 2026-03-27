using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the MedicalCode entity (DR-013).
/// Composite uniqueness on (extracted_data_id, code_system, code_value).
/// </summary>
public class MedicalCodeConfiguration : IEntityTypeConfiguration<MedicalCode>
{
    public void Configure(EntityTypeBuilder<MedicalCode> builder)
    {
        builder.ToTable("MedicalCodes");

        builder.HasKey(m => m.MedicalCodeId);
        builder.Property(m => m.MedicalCodeId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(m => m.CodeSystem)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.CodeValue)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.CodeDescription)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(m => m.ConfidenceScore)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // US_051 Task 1: New properties for RAG-based code mapping
        builder.Property(m => m.Rationale)
            .HasColumnType("text");

        builder.Property(m => m.Rank)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(m => m.IsTopSuggestion)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.RetrievedContext)
            .HasColumnType("text");

        builder.Property(m => m.VerificationStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.VerifiedAt)
            .HasColumnType("timestamptz");

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()");

        builder.Property(m => m.UpdatedAt)
            .HasColumnType("timestamptz");

        // FK: MedicalCode -> ExtractedClinicalData (CASCADE)
        builder.HasOne(m => m.ExtractedData)
            .WithMany(e => e.MedicalCodes)
            .HasForeignKey(m => m.ExtractedDataId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MedicalCodes_ExtractedData");

        // FK: MedicalCode -> User (verifier, SET NULL)
        builder.HasOne(m => m.Verifier)
            .WithMany()
            .HasForeignKey(m => m.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_MedicalCodes_VerifiedBy");

        builder.HasIndex(m => m.ExtractedDataId)
            .HasDatabaseName("IX_MedicalCodes_ExtractedDataId");

        builder.HasIndex(m => m.CodeSystem)
            .HasDatabaseName("IX_MedicalCodes_CodeSystem");

        builder.HasIndex(m => m.CodeValue)
            .HasDatabaseName("IX_MedicalCodes_CodeValue");

        builder.HasIndex(m => m.VerificationStatus)
            .HasDatabaseName("IX_MedicalCodes_VerificationStatus");

        // US_051 Task 1: Index for filtering top suggestions in quality metrics
        builder.HasIndex(m => m.IsTopSuggestion)
            .HasDatabaseName("IX_MedicalCodes_IsTopSuggestion");

        // Composite unique: prevent duplicate codes per extraction
        builder.HasIndex(m => new { m.ExtractedDataId, m.CodeSystem, m.CodeValue })
            .IsUnique()
            .HasDatabaseName("IX_MedicalCodes_ExtractedData_System_Value");
    }
}
