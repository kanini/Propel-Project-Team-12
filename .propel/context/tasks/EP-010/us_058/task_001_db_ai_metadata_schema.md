# Task - task_001_db_ai_metadata_schema

## Requirement Reference
- User Story: US_058
- Story Location: .propel/context/tasks/EP-010/us_058/us_058.md
- Acceptance Criteria:
    - **AC1**: Given the human-in-the-loop requirement (AIR-S01), When AI generates any clinical suggestion, Then the output is flagged as "AI-Suggested" and cannot be committed to the patient record without explicit staff verification.
    - **AC2**: Given confidence thresholds (AIR-S02), When an AI extraction has a confidence score below the defined threshold, Then it is auto-flagged for mandatory manual review and highlighted with a warning indicator.
    - **AC4**: Given token and cost management (AIR-O05), When AI requests are made, Then per-request token usage and estimated cost are logged.
    - **AC5**: Given monitoring requirements (NFR-015), When AI operations run, Then model version is captured per-extraction for traceability.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | N/A | N/A |
| Backend | N/A | N/A |
| Database | PostgreSQL | 16.x |
| Database | Entity Framework Core | 8.0 |
| Caching | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-S01, AIR-S02, AIR-O05, NFR-015 |
| **AI Pattern** | Human-in-the-loop verification, Confidence thresholds, Cost tracking |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | Azure OpenAI (GPT-4o), Azure AI Document Intelligence |

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Add database schema fields for AI metadata tracking to support human-in-the-loop verification, confidence thresholds, token/cost logging, and model version traceability. This task extends ExtractedClinicalData and ClinicalDocument tables with AI-specific columns: IsAISuggested flag (AC1), ConfidenceScore decimal (AC2), RequiresManualReview boolean (AC2), ModelVersion string (AC5, Edge Case), PromptTokens/CompletionTokens/TotalTokens integers (AC4), EstimatedCost decimal (AC4), and VerifiedBy/VerifiedAt fields for staff verification workflow (AC1). Adds database migration script with rollback support, Entity Framework Core entity updates, and indexes for query performance on confidence score and verification status.

**Key Capabilities:**
- IsAISuggested flag for human-in-the-loop (AC1)
- ConfidenceScore field for threshold checking (AC2)  
- RequiresManualReview auto-flag for low-confidence extractions (AC2)
- ModelVersion field for traceability (AC5, Edge Case)
- Token usage fields: PromptTokens, CompletionTokens, TotalTokens (AC4)
- EstimatedCost field for per-request cost tracking (AC4)
- VerifiedBy/VerifiedAt fields for staff verification (AC1)
- VerificationStatus enum: Pending, Verified, ManuallyEdited
- Database indexes for performance
- Migration script with rollback support

## Dependent Tasks
- None (foundational schema task)

## Impacted Components
- **NEW**: `src/backend/scripts/migrations/20260323_add_ai_metadata_columns.sql` - Add AI metadata columns
- **NEW**: `src/backend/PatientAccess.Business/Enums/VerificationStatus.cs` - Verification status enum
- **MODIFY**: `src/backend/PatientAccess.Data/Entities/ExtractedClinicalData.cs` - Add AI metadata properties
- **MODIFY**: `src/backend/PatientAccess.Data/Entities/ClinicalDocument.cs` - Add AI process metadata
- **MODIFY**: `src/backend/PatientAccess.Data/Configurations/ExtractedClinicalDataConfiguration.cs` - EF Core configuration

## Implementation Plan

1. **Create VerificationStatus Enum**
   - File: `src/backend/PatientAccess.Business/Enums/VerificationStatus.cs`
   - Verification lifecycle enum:
     ```csharp
     namespace PatientAccess.Business.Enums
     {
         /// <summary>
         /// Verification status for AI-extracted clinical data (AC1 - US_058).
         /// </summary>
         public enum VerificationStatus
         {
             /// <summary>
             /// AI extraction pending staff verification (default state).
             /// </summary>
             Pending = 0,
             
             /// <summary>
             /// Staff verified AI extraction as accurate (AC1: explicit verification).
             /// </summary>
             Verified = 1,
             
             /// <summary>
             /// Staff manually edited AI extraction (AC1: corrections made).
             /// </summary>
             ManuallyEdited = 2,
             
             /// <summary>
             /// AI extraction rejected, data entered manually.
             /// </summary>
             Rejected = 3
         }
     }
     ```

2. **Update ExtractedClinicalData Entity**
   - File: `src/backend/PatientAccess.Data/Entities/ExtractedClinicalData.cs`
   - Add AI metadata properties:
     ```csharp
     using PatientAccess.Business.Enums;
     using System.ComponentModel.DataAnnotations;
     using System.ComponentModel.DataAnnotations.Schema;
     
     namespace PatientAccess.Data.Entities
     {
         public class ExtractedClinicalData
         {
             public int Id { get; set; }
             
             // Existing properties...
             public int ClinicalDocumentId { get; set; }
             public required string DataType { get; set; } // Vital, Medication, Allergy, etc.
             public required string Value { get; set; }
             
             /// <summary>
             /// AI-suggested flag for human-in-the-loop (AC1 - US_058).
             /// True = AI-generated, requires staff verification before commit.
             /// </summary>
             [Column("IsAISuggested")]
             public bool IsAISuggested { get; set; } = true;
             
             /// <summary>
             /// AI extraction confidence score (0.0 - 1.0) (AC2 - US_058).
             /// Below threshold (e.g., 0.7) triggers mandatory manual review.
             /// </summary>
             [Column("ConfidenceScore")]
             [Column(TypeName = "decimal(5,4)")]
             public decimal? ConfidenceScore { get; set; }
             
             /// <summary>
             /// Auto-flagged for mandatory manual review (AC2 - US_058).
             /// Set to true when ConfidenceScore < threshold.
             /// </summary>
             [Column("RequiresManualReview")]
             public bool RequiresManualReview { get; set; } = false;
             
             /// <summary>
             /// Verification status lifecycle (AC1 - US_058).
             /// Default: Pending (requires staff verification).
             /// </summary>
             [Column("VerificationStatus")]
             public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
             
             /// <summary>
             /// Staff member who verified the AI extraction (AC1 - US_058).
             /// Null = not yet verified.
             /// </summary>
             [Column("VerifiedBy")]
             public int? VerifiedBy { get; set; }
             
             /// <summary>
             /// Timestamp when staff verified the extraction (AC1 - US_058).
             /// </summary>
             [Column("VerifiedAt")]
             public DateTime? VerifiedAt { get; set; }
             
             /// <summary>
             /// AI model version used for extraction (AC5 - US_058, Edge Case).
             /// Format: "gpt-4o-2024-05-13" or "prebuilt-layout-2024-02-29".
             /// Captured per-extraction for traceability.
             /// </summary>
             [Column("ModelVersion")]
             [MaxLength(100)]
             public string? ModelVersion { get; set; }
             
             /// <summary>
             /// Prompt tokens consumed (AC4 - US_058).
             /// Used for cost tracking and monitoring.
             /// </summary>
             [Column("PromptTokens")]
             public int? PromptTokens { get; set; }
             
             /// <summary>
             /// Completion tokens generated (AC4 - US_058).
             /// Used for cost tracking and monitoring.
             /// </summary>
             [Column("CompletionTokens")]
             public int? CompletionTokens { get; set; }
             
             /// <summary>
             /// Total tokens (prompt + completion) (AC4 - US_058).
             /// </summary>
             [Column("TotalTokens")]
             public int? TotalTokens { get; set; }
             
             /// <summary>
             /// Estimated cost for this AI operation in USD (AC4 - US_058).
             /// Calculated based on token usage and model pricing.
             /// </summary>
             [Column("EstimatedCost")]
             [Column(TypeName = "decimal(10,6)")]
             public decimal? EstimatedCost { get; set; }
             
             // Navigation properties
             public ClinicalDocument ClinicalDocument { get; set; }
             public User? Verifier { get; set; } // Staff who verified
         }
     }
     ```

3. **Update ClinicalDocument Entity**
   - File: `src/backend/PatientAccess.Data/Entities/ClinicalDocument.cs`
   - Add AI processing metadata:
     ```csharp
     namespace PatientAccess.Data.Entities
     {
         public class ClinicalDocument
         {
             public int Id { get; set; }
             
             // Existing properties...
             public int PatientId { get; set; }
             public required string FileName { get; set; }
             public required string FileType { get; set; }
             public required string Status { get; set; }
             
             /// <summary>
             /// True if document was processed by AI (AC5 - US_058).
             /// False for manually uploaded documents without AI processing.
             /// </summary>
             [Column("IsAIProcessed")]
             public bool IsAIProcessed { get; set; } = false;
             
             /// <summary>
             /// AI model version used for document processing (AC5 - US_058).
             /// Captured at document level for traceability.
             /// </summary>
             [Column("AIModelVersion")]
             [MaxLength(100)]
             public string? AIModelVersion { get; set; }
             
             /// <summary>
             /// Total tokens consumed for document processing (AC4 - US_058).
             /// Aggregate of all extraction operations.
             /// </summary>
             [Column("TotalTokensConsumed")]
             public int? TotalTokensConsumed { get; set; }
             
             /// <summary>
             /// Total estimated cost for document processing (AC4 - US_058).
             /// Sum of all extraction costs.
             /// </summary>
             [Column("TotalEstimatedCost")]
             [Column(TypeName = "decimal(10,6)")]
             public decimal? TotalEstimatedCost { get; set; }
             
             // Navigation properties
             public Patient Patient { get; set; }
             public ICollection<ExtractedClinicalData> ExtractedData { get; set; } = new List<ExtractedClinicalData>();
         }
     }
     ```

4. **Create Database Migration Script**
   - File: `src/backend/scripts/migrations/20260323_add_ai_metadata_columns.sql`
   - Migration SQL:
     ```sql
     -- Migration: Add AI metadata columns for guardrails and tracking
     -- Date: 2026-03-23
     -- Epic: EP-010, User Story: US_058, Task: task_001
     -- Requirements: AIR-S01, AIR-S02, AIR-O05, NFR-015
     
     -- ExtractedClinicalData: Add AI metadata columns
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "IsAISuggested" BOOLEAN DEFAULT TRUE;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "ConfidenceScore" DECIMAL(5,4) NULL;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "RequiresManualReview" BOOLEAN DEFAULT FALSE;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "VerificationStatus" INTEGER DEFAULT 0;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "VerifiedBy" INTEGER NULL;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "VerifiedAt" TIMESTAMPTZ NULL;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "ModelVersion" VARCHAR(100) NULL;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "PromptTokens" INTEGER NULL;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "CompletionTokens" INTEGER NULL;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "TotalTokens" INTEGER NULL;
     
     ALTER TABLE "ExtractedClinicalData"
     ADD COLUMN IF NOT EXISTS "EstimatedCost" DECIMAL(10,6) NULL;
     
     -- ClinicalDocument: Add AI processing metadata
     ALTER TABLE "ClinicalDocuments"
     ADD COLUMN IF NOT EXISTS "IsAIProcessed" BOOLEAN DEFAULT FALSE;
     
     ALTER TABLE "ClinicalDocuments"
     ADD COLUMN IF NOT EXISTS "AIModelVersion" VARCHAR(100) NULL;
     
     ALTER TABLE "ClinicalDocuments"
     ADD COLUMN IF NOT EXISTS "TotalTokensConsumed" INTEGER NULL;
     
     ALTER TABLE "ClinicalDocuments"
     ADD COLUMN IF NOT EXISTS "TotalEstimatedCost" DECIMAL(10,6) NULL;
     
     -- Create indexes for query performance
     CREATE INDEX IF NOT EXISTS "IX_ExtractedClinicalData_ConfidenceScore"
     ON "ExtractedClinicalData" ("ConfidenceScore");
     
     CREATE INDEX IF NOT EXISTS "IX_ExtractedClinicalData_RequiresManualReview"
     ON "ExtractedClinicalData" ("RequiresManualReview")
     WHERE "RequiresManualReview" = TRUE;
     
     CREATE INDEX IF NOT EXISTS "IX_ExtractedClinicalData_VerificationStatus"
     ON "ExtractedClinicalData" ("VerificationStatus");
     
     CREATE INDEX IF NOT EXISTS "IX_ExtractedClinicalData_VerifiedBy_VerifiedAt"
     ON "ExtractedClinicalData" ("VerifiedBy", "VerifiedAt");
     
     -- Add foreign key constraint for VerifiedBy
     ALTER TABLE "ExtractedClinicalData"
     ADD CONSTRAINT "FK_ExtractedClinicalData_Users_VerifiedBy"
     FOREIGN KEY ("VerifiedBy") REFERENCES "Users" ("Id")
     ON DELETE SET NULL;
     
     -- Add column comments for documentation
     COMMENT ON COLUMN "ExtractedClinicalData"."IsAISuggested" IS 'AC1 (US_058): True if AI-generated, requires staff verification';
     COMMENT ON COLUMN "ExtractedClinicalData"."ConfidenceScore" IS 'AC2 (US_058): AI extraction confidence (0.0-1.0)';
     COMMENT ON COLUMN "ExtractedClinicalData"."RequiresManualReview" IS 'AC2 (US_058): Auto-flagged when confidence < threshold';
     COMMENT ON COLUMN "ExtractedClinicalData"."ModelVersion" IS 'AC5 (US_058): AI model version for traceability';
     COMMENT ON COLUMN "ExtractedClinicalData"."TotalTokens" IS 'AC4 (US_058): Total tokens for cost tracking';
     COMMENT ON COLUMN "ExtractedClinicalData"."EstimatedCost" IS 'AC4 (US_058): Per-request cost in USD';
     
     -- Rollback script:
     -- ALTER TABLE "ExtractedClinicalData" DROP CONSTRAINT IF EXISTS "FK_ExtractedClinicalData_Users_VerifiedBy";
     -- DROP INDEX IF EXISTS "IX_ExtractedClinicalData_ConfidenceScore";
     -- DROP INDEX IF EXISTS "IX_ExtractedClinicalData_RequiresManualReview";
     -- DROP INDEX IF EXISTS "IX_ExtractedClinicalData_VerificationStatus";
     -- DROP INDEX IF EXISTS "IX_ExtractedClinicalData_VerifiedBy_VerifiedAt";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "IsAISuggested";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "ConfidenceScore";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "RequiresManualReview";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "VerificationStatus";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "VerifiedBy";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "VerifiedAt";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "ModelVersion";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "PromptTokens";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "CompletionTokens";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "TotalTokens";
     -- ALTER TABLE "ExtractedClinicalData" DROP COLUMN IF EXISTS "EstimatedCost";
     -- ALTER TABLE "ClinicalDocuments" DROP COLUMN IF EXISTS "IsAIProcessed";
     -- ALTER TABLE "ClinicalDocuments" DROP COLUMN IF EXISTS "AIModelVersion";
     -- ALTER TABLE "ClinicalDocuments" DROP COLUMN IF EXISTS "TotalTokensConsumed";
     -- ALTER TABLE "ClinicalDocuments" DROP COLUMN IF EXISTS "TotalEstimatedCost";
     ```

5. **Update Entity Framework Core Configuration**
   - File: `src/backend/PatientAccess.Data/Configurations/ExtractedClinicalDataConfiguration.cs`
   - Configure AI metadata fields:
     ```csharp
     using Microsoft.EntityFrameworkCore;
     using Microsoft.EntityFrameworkCore.Metadata.Builders;
     using PatientAccess.Data.Entities;
     using PatientAccess.Business.Enums;
     
     namespace PatientAccess.Data.Configurations
     {
         public class ExtractedClinicalDataConfiguration : IEntityTypeConfiguration<ExtractedClinicalData>
         {
             public void Configure(EntityTypeBuilder<ExtractedClinicalData> builder)
             {
                 // Table name
                 builder.ToTable("ExtractedClinicalData");
                 
                 // Primary key
                 builder.HasKey(e => e.Id);
                 
                 // Required fields
                 builder.Property(e => e.DataType).IsRequired().HasMaxLength(50);
                 builder.Property(e => e.Value).IsRequired();
                 
                 // AI metadata fields (AC1, AC2, AC4, AC5 - US_058)
                 builder.Property(e => e.IsAISuggested).HasDefaultValue(true);
                 builder.Property(e => e.ConfidenceScore).HasColumnType("decimal(5,4)");
                 builder.Property(e => e.RequiresManualReview).HasDefaultValue(false);
                 builder.Property(e => e.VerificationStatus)
                     .HasConversion<int>()
                     .HasDefaultValue(VerificationStatus.Pending);
                 builder.Property(e => e.ModelVersion).HasMaxLength(100);
                 builder.Property(e => e.EstimatedCost).HasColumnType("decimal(10,6)");
                 
                 // Indexes for query performance
                 builder.HasIndex(e => e.ConfidenceScore)
                     .HasDatabaseName("IX_ExtractedClinicalData_ConfidenceScore");
                 
                 builder.HasIndex(e => e.RequiresManualReview)
                     .HasDatabaseName("IX_ExtractedClinicalData_RequiresManualReview")
                     .HasFilter("\"RequiresManualReview\" = TRUE");
                 
                 builder.HasIndex(e => e.VerificationStatus)
                     .HasDatabaseName("IX_ExtractedClinicalData_VerificationStatus");
                 
                 builder.HasIndex(e => new { e.VerifiedBy, e.VerifiedAt })
                     .HasDatabaseName("IX_ExtractedClinicalData_VerifiedBy_VerifiedAt");
                 
                 // Foreign key relationships
                 builder.HasOne(e => e.ClinicalDocument)
                     .WithMany(d => d.ExtractedData)
                     .HasForeignKey(e => e.ClinicalDocumentId)
                     .OnDelete(DeleteBehavior.Cascade);
                 
                 builder.HasOne(e => e.Verifier)
                     .WithMany()
                     .HasForeignKey(e => e.VerifiedBy)
                     .OnDelete(DeleteBehavior.SetNull);
             }
         }
     }
     ```

## Current Project State

```
src/backend/
├── scripts/
│   └── migrations/
├── PatientAccess.Business/
│   └── Enums/
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── ExtractedClinicalData.cs (existing)
│   │   └── ClinicalDocument.cs (existing)
│   └── Configurations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/scripts/migrations/20260323_add_ai_metadata_columns.sql | Add AI metadata columns migration |
| CREATE | src/backend/PatientAccess.Business/Enums/VerificationStatus.cs | Verification status enum |
| CREATE | src/backend/PatientAccess.Data/Configurations/ExtractedClinicalDataConfiguration.cs | EF Core configuration |
| MODIFY | src/backend/PatientAccess.Data/Entities/ExtractedClinicalData.cs | Add AI metadata properties |
| MODIFY | src/backend/PatientAccess.Data/Entities/ClinicalDocument.cs | Add AI process metadata |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### PostgreSQL
- **Decimal Data Type**: https://www.postgresql.org/docs/16/datatype-numeric.html
- **Partial Indexes**: https://www.postgresql.org/docs/16/indexes-partial.html
- **Column Comments**: https://www.postgresql.org/docs/16/sql-comment.html

### Entity Framework Core
- **Value Conversions**: https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions
- **Entity Configuration**: https://learn.microsoft.com/en-us/ef/core/modeling/entity-types
- **Indexes**: https://learn.microsoft.com/en-us/ef/core/modeling/indexes

### Design Requirements
- **AIR-S01**: Human-in-the-loop for clinical AI (design.md)
- **AIR-S02**: Confidence threshold flagging (design.md)
- **AIR-O05**: Token and cost guardrails (design.md)
- **NFR-015**: AI operational monitoring (design.md)

## Build Commands
```powershell
# Run migration
cd src/backend/scripts/migrations
psql $env:DATABASE_URL -f 20260323_add_ai_metadata_columns.sql

# Verify migration
psql $env:DATABASE_URL -c "\d+ ExtractedClinicalData"
psql $env:DATABASE_URL -c "\d+ ClinicalDocuments"

# Build solution
cd ../../
dotnet build PatientAccess.sln
```

## Validation Strategy

### Database Tests
- File: `src/backend/PatientAccess.Tests/Database/AIMetadataSchemaTests.cs`
- Test cases:
  1. **Test_ExtractedClinicalData_HasAIMetadataColumns**
     - Query: `SELECT column_name FROM information_schema.columns WHERE table_name = 'ExtractedClinicalData' AND column_name IN ('IsAISuggested', 'ConfidenceScore', 'ModelVersion', 'TotalTokens', 'EstimatedCost');`
     - Assert: All AI metadata columns exist
  2. **Test_ExtractedClinicalData_ConfidenceScoreDecimalPrecision**
     - Query: Check data_type and numeric_precision for ConfidenceScore
     - Assert: decimal(5,4) - supports values 0.0000 to 9.9999
  3. **Test_ExtractedClinicalData_DefaultValues**
     - Insert: New ExtractedClinicalData record without AI metadata
     - Assert: IsAISuggested = TRUE, RequiresManualReview = FALSE, VerificationStatus = 0 (Pending)
  4. **Test_ExtractedClinicalData_VerifierForeignKey**
     - Query: Check foreign key constraint FK_ExtractedClinicalData_Users_VerifiedBy
     - Assert: Constraint exists with ON DELETE SET NULL
  5. **Test_ExtractedClinicalData_IndexesExist**
     - Query: `SELECT indexname FROM pg_indexes WHERE tablename = 'ExtractedClinicalData';`
     - Assert: Indexes exist for ConfidenceScore, RequiresManualReview, VerificationStatus

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/AIMetadataIntegrationTests.cs`
- Test cases:
  1. **Test_SaveExtractedData_WithAIMetadata**
     - Setup: Create ExtractedClinicalData with AI metadata (confidence=0.85, tokens=150)
     - Save to database
     - Assert: All AI metadata fields persisted correctly
  2. **Test_QueryByConfidenceScore**
     - Setup: Create 10 ExtractedClinicalData records with varying confidence (0.5 to 0.95)
     - Query: Select records with ConfidenceScore < 0.7
     - Assert: Returns only low-confidence records, uses index

### Acceptance Criteria Validation
- **AC1**: ✅ IsAISuggested flag + VerificationStatus + VerifiedBy/VerifiedAt fields for human-in-the-loop
- **AC2**: ✅ ConfidenceScore + RequiresManualReview fields for threshold flagging
- **AC4**: ✅ Token usage (PromptTokens, CompletionTokens, TotalTokens) + EstimatedCost fields
- **AC5**: ✅ ModelVersion field for model version traceability (Edge Case)

## Success Criteria Checklist
- [MANDATORY] VerificationStatus enum created (Pending, Verified, ManuallyEdited, Rejected)
- [MANDATORY] ExtractedClinicalData.IsAISuggested column added (AC1)
- [MANDATORY] ExtractedClinicalData.ConfidenceScore column added (decimal 5,4) (AC2)
- [MANDATORY] ExtractedClinicalData.RequiresManualReview column added (AC2)
- [MANDATORY] ExtractedClinicalData.VerificationStatus column added (AC1)
- [MANDATORY] ExtractedClinicalData.VerifiedBy + VerifiedAt columns added (AC1)
- [MANDATORY] ExtractedClinicalData.ModelVersion column added (AC5, Edge Case)
- [MANDATORY] ExtractedClinicalData.PromptTokens/CompletionTokens/TotalTokens columns added (AC4)
- [MANDATORY] ExtractedClinicalData.EstimatedCost column added (AC4)
- [MANDATORY] ClinicalDocument.IsAIProcessed column added
- [MANDATORY] ClinicalDocument.AIModelVersion column added (AC5)
- [MANDATORY] ClinicalDocument.TotalTokensConsumed + TotalEstimatedCost columns added (AC4)
- [MANDATORY] Index on ConfidenceScore for query performance
- [MANDATORY] Partial index on RequiresManualReview (WHERE TRUE)
- [MANDATORY] Index on VerificationStatus for filtering
- [MANDATORY] Foreign key constraint VerifiedBy -> Users with ON DELETE SET NULL
- [MANDATORY] Migration script with rollback support
- [MANDATORY] Entity Framework Core configuration for AI metadata fields
- [RECOMMENDED] Database test: Verify all AI metadata columns exist
- [RECOMMENDED] Integration test: Save and query by confidence score

## Estimated Effort
**2 hours** (Database migration + entity updates + EF Core configuration + indexes + tests)
