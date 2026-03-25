# Task - task_001_db_extracted_data_schema

## Requirement Reference
- User Story: US_045
- Story Location: .propel/context/tasks/EP-006-II/us_045/us_045.md
- Acceptance Criteria:
    - **AC2**: Given data is extracted, When each data point is stored, Then it includes a confidence score (0-100%) and a source document reference (page number, text excerpt) enabling traceability to the original document.
    - **AC5**: Given multiple data types exist in a document, When extraction completes, Then each element is classified by type (Vital, Medication, Allergy, Diagnosis, LabResult) with appropriate structured fields.

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
| Database | PostgreSQL | 16.x |
| Backend | Entity Framework Core | 8.0 |
| Backend | .NET | 8.0 |
| AI/ML | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

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

Create the database schema and EF Core migration for the `ExtractedClinicalData` table to store AI-extracted clinical data points from uploaded documents. This table maintains traceability from extracted data to source documents, stores confidence scores for quality assessment, supports data classification by type (Vital, Medication, Allergy, Diagnosis, LabResult), and enables verification workflow tracking (AI-suggested → Staff-verified). The schema extends the domain model defined in design.md (DR-004: ExtractedClinicalData entity).

**Key Capabilities:**
- Store extracted clinical data with type classification
- Maintain confidence scores (0-100%) for quality assessment
- Reference source document and page number for traceability
- Store text excerpt from source document
- Track verification status (Suggested, Verified, Rejected)
- Support structured fields per data type (JSON or typed columns)
- Enable efficient querying by patient, document, and verification status

## Dependent Tasks
- EP-006-I: US_042: task_003_db_document_schema_migration (ClinicalDocument table must exist for FK)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Data/Entities/ExtractedClinicalData.cs` - EF Core entity model
- **NEW**: `src/backend/PatientAccess.Data/Migrations/{timestamp}_AddExtractedClinicalDataTable.cs` - EF migration
- **MODIFY**: `src/backend/PatientAccess.Data/ApplicationDbContext.cs` - Add DbSet<ExtractedClinicalData>
- **CREATE**: `src/backend/scripts/migrations/V002__Create_ExtractedClinicalData_Table.sql` - Raw SQL migration

## Implementation Plan

1. **Define ExtractedClinicalData Entity**
   - Create `ExtractedClinicalData.cs` in `PatientAccess.Data/Entities/`
   - Define properties: Id (Guid, PK), DocumentId (Guid, FK), PatientId (Guid, FK), DataType (enum: Vital, Medication, Allergy, Diagnosis, LabResult), Value (string, 500), StructuredData (JSON, nullable), ConfidenceScore (decimal 0-100), SourcePageNumber (int), SourceTextExcerpt (string, 1000), VerificationStatus (enum: Suggested, Verified, Rejected), VerifiedBy (Guid, nullable FK to Users), VerifiedAt (DateTime, nullable), ExtractedAt (DateTime), CreatedAt (DateTime)
   - Add navigation properties: ClinicalDocument, Patient, VerifiedByUser
   - Follow existing entity patterns from `ClinicalDocument.cs`

2. **Configure Entity Relationships in DbContext**
   - Add `DbSet<ExtractedClinicalData> ExtractedClinicalData` to `ApplicationDbContext.cs`
   - Configure fluent API in `OnModelCreating`:
     - Set required fields (DocumentId, PatientId, DataType, Value, ConfidenceScore, ExtractedAt)
     - Set max length constraints (Value: 500, SourceTextExcerpt: 1000)
     - Configure foreign key to ClinicalDocument with cascading behavior (DeleteBehavior.Cascade)
     - Configure foreign key to Patient (DeleteBehavior.Restrict)
     - Configure foreign key to Users for VerifiedBy (DeleteBehavior.SetNull)
     - Add index on PatientId for efficient querying
     - Add index on DocumentId for document-based queries
     - Add index on VerificationStatus for filtering unverified data
     - Add index on DataType for type-based queries
     - Configure StructuredData as JSON column (PostgreSQL JSONB)

3. **Create DataType and VerificationStatus Enums**
   - Create `DataType` enum: Vital, Medication, Allergy, Diagnosis, LabResult
   - Create `VerificationStatus` enum: Suggested, Verified, Rejected
   - Store as strings in database for readability
   - Follow existing enum patterns in codebase

4. **Create EF Core Migration**
   - Run `dotnet ef migrations add AddExtractedClinicalDataTable` from PatientAccess.Data project
   - Review generated migration file in `Migrations/` folder
   - Verify Up() method creates extracted_clinical_data table with correct schema
   - Verify Down() method drops table and indexes (for rollback)
   - Test migration on local PostgreSQL instance

5. **Create Raw SQL Migration Script**
   - Create `V002__Create_ExtractedClinicalData_Table.sql` in `src/backend/scripts/migrations/`
   - Write raw PostgreSQL DDL for table creation (for manual deployment scenarios)
   - Include primary key, foreign keys, indexes, and constraints
   - Add CHECK constraint: ConfidenceScore BETWEEN 0 AND 100
   - Include comments on table and columns for documentation
   - Add rollback script: `V002__Create_ExtractedClinicalData_Table_Rollback.sql`

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Appointment.cs
│   │   ├── Provider.cs
│   │   └── ClinicalDocument.cs (from EP-006-I)
│   ├── Migrations/
│   └── ApplicationDbContext.cs
├── scripts/
│   └── migrations/
│       └── V001__Create_ClinicalDocument_Table.sql (from EP-006-I)
└── PatientAccess.Web/
    └── appsettings.json
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Entities/ExtractedClinicalData.cs | Entity model for extracted clinical data |
| CREATE | src/backend/PatientAccess.Data/Entities/DataType.cs | Enum for data type classification |
| CREATE | src/backend/PatientAccess.Data/Entities/VerificationStatus.cs | Enum for verification workflow |
| MODIFY | src/backend/PatientAccess.Data/ApplicationDbContext.cs | Add DbSet and configure relationships |
| CREATE | src/backend/PatientAccess.Data/Migrations/{timestamp}_AddExtractedClinicalDataTable.cs | EF Core migration (auto-generated) |
| CREATE | src/backend/scripts/migrations/V002__Create_ExtractedClinicalData_Table.sql | Raw SQL migration script |
| CREATE | src/backend/scripts/migrations/V002__Create_ExtractedClinicalData_Table_Rollback.sql | Rollback script |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Entity Framework Core Documentation
- **EF Core Migrations**: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- **Entity Configuration**: https://learn.microsoft.com/en-us/ef/core/modeling/entity-types
- **Relationships**: https://learn.microsoft.com/en-us/ef/core/modeling/relationships
- **Indexes**: https://learn.microsoft.com/en-us/ef/core/modeling/indexes
- **JSON Columns**: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns

### PostgreSQL Documentation
- **Table Creation**: https://www.postgresql.org/docs/16/sql-createtable.html
- **JSONB Type**: https://www.postgresql.org/docs/16/datatype-json.html
- **CHECK Constraints**: https://www.postgresql.org/docs/16/ddl-constraints.html#DDL-CONSTRAINTS-CHECK-CONSTRAINTS

### Design Requirements
- **DR-004**: System MUST store extracted clinical data with unique identifier, source document reference, data type, value, confidence score, and verification status (design.md)
- **DR-006**: System MUST enforce referential integrity between patient records and all related entities (design.md)
- **AIR-008**: System MUST provide confidence scores (0-100%) for all AI-suggested clinical data and medical codes (design.md)
- **AIR-009**: System MUST provide source document references (page number, text excerpt) for all AI-extracted data points (design.md)

### Existing Codebase Patterns
- **Entity Model Pattern**: `src/backend/PatientAccess.Data/Entities/ClinicalDocument.cs`
- **DbContext Pattern**: ApplicationDbContext.cs configuration
- **Migration Pattern**: Existing migrations in PatientAccess.Data/Migrations/

## Build Commands
```powershell
# Navigate to PatientAccess.Data
cd src/backend/PatientAccess.Data

# Add migration
dotnet ef migrations add AddExtractedClinicalDataTable

# Apply migration to database
dotnet ef database update

# Generate SQL script (for review/manual deployment)
dotnet ef migrations script --output scripts/migrations/AddExtractedClinicalDataTable.sql

# Rollback migration (if needed)
dotnet ef migrations remove
```

## Implementation Validation Strategy
- [ ] EF Core migration runs successfully on local PostgreSQL instance
- [ ] extracted_clinical_data table created with correct schema (columns, types, constraints)
- [ ] Foreign key constraints enforced (DocumentId → clinical_documents, PatientId → patients, VerifiedBy → users)
- [ ] Indexes created successfully (PatientId, DocumentId, VerificationStatus, DataType)
- [ ] CHECK constraint validates ConfidenceScore between 0 and 100
- [ ] StructuredData JSONB column supports JSON storage and querying
- [ ] Insert test record via DbContext (validates entity configuration)
- [ ] Query test records by PatientId, DocumentId, and VerificationStatus (validates indexes)
- [ ] Migration rollback successful (Down() method works)
- [ ] Raw SQL migration script matches EF-generated schema

## Implementation Checklist
- [ ] Create ExtractedClinicalData.cs entity with all required properties and navigation properties
- [ ] Create DataType and VerificationStatus enums with string storage
- [ ] Add DbSet<ExtractedClinicalData> to ApplicationDbContext
- [ ] Configure fluent API relationships (FK to ClinicalDocument, Patient, Users, indexes, CHECK constraint)
- [ ] Configure StructuredData as JSONB column using EF Core 7+ JSON mapping
- [ ] Generate EF Core migration using `dotnet ef migrations add AddExtractedClinicalDataTable`
- [ ] Review generated migration for correctness (Up and Down methods, CHECK constraint)
- [ ] Test migration on local PostgreSQL instance
- [ ] Create raw SQL migration script (V002__Create_ExtractedClinicalData_Table.sql)
- [ ] Create rollback SQL script (V002__Create_ExtractedClinicalData_Table_Rollback.sql)
