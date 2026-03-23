# Task - task_003_db_document_schema_migration

## Requirement Reference
- User Story: US_042
- Story Location: .propel/context/tasks/EP-006-I/us_042/us_042.md
- Acceptance Criteria:
    - **AC3**: Given the upload completes successfully, When the file is stored, Then a confirmation message displays with the document name, size, and status "Uploaded — Processing pending".
- Edge Case:
    - Database must support storing document metadata for multiple simultaneous uploads from different patients.

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

Create the database schema and EF Core migration for the `ClinicalDocument` table to store uploaded clinical document metadata. This table tracks document lifecycle from upload through AI processing, maintaining referential integrity with the Patient entity, storing file metadata (name, size, MIME type, storage path), and supporting processing status tracking (Uploaded → Processing → Completed/Failed). The schema extends the domain model defined in design.md (DR-003: ClinicalDocument entity) with additional fields for chunked upload tracking and audit trail.

**Key Capabilities:**
- Store document metadata (name, size, MIME type, storage location)
- Track processing status lifecycle (Uploaded, Processing, Completed, Failed)
- Maintain foreign key relationship to Patients table
- Support audit fields (uploadedAt, uploadedBy, processedAt)
- Enable efficient querying by patient, status, and date range
- Support future extraction data linkage (extracted_clinical_data table)

## Dependent Tasks
- None (this is foundational database work)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Data/Entities/ClinicalDocument.cs` - EF Core entity model
- **NEW**: `src/backend/PatientAccess.Data/Migrations/{timestamp}_AddClinicalDocumentTable.cs` - EF migration
- **MODIFY**: `src/backend/PatientAccess.Data/ApplicationDbContext.cs` - Add DbSet<ClinicalDocument>
- **CREATE**: `src/backend/scripts/migrations/V001__Create_ClinicalDocument_Table.sql` - Raw SQL migration (for reference/rollback)

## Implementation Plan

1. **Define ClinicalDocument Entity**
   - Create `ClinicalDocument.cs` in `PatientAccess.Data/Entities/`
   - Define properties: Id (Guid, PK), PatientId (Guid, FK), FileName (string, 255), FileSize (long), MimeType (string, 100), FilePath (string, 500), Status (enum: Uploaded, Processing, Completed, Failed), ProcessingStatus (string, nullable), UploadedAt (DateTime), UploadedBy (Guid, FK to Users), ProcessedAt (DateTime, nullable), ErrorMessage (string, nullable, 1000)
   - Add navigation properties: Patient (Patient entity), UploadedByUser (User entity)
   - Follow existing entity patterns from `PatientAccess.Data/Entities/`

2. **Configure Entity Relationships in DbContext**
   - Add `DbSet<ClinicalDocument> ClinicalDocuments` to `ApplicationDbContext.cs`
   - Configure fluent API in `OnModelCreating`:
     - Set required fields (FileName, FileSize, MimeType, FilePath, Status, PatientId, UploadedBy, UploadedAt)
     - Set max length constraints (FileName: 255, MimeType: 100, FilePath: 500, ErrorMessage: 1000)
     - Configure foreign key to Patients table with cascading behavior (DeleteBehavior.Restrict)
     - Configure foreign key to Users table for UploadedBy
     - Add index on PatientId for efficient querying
     - Add index on Status for dashboard queries
     - Add index on UploadedAt for date-range filtering

3. **Create EF Core Migration**
   - Run `dotnet ef migrations add AddClinicalDocumentTable` from PatientAccess.Data project
   - Review generated migration file in `Migrations/` folder
   - Verify Up() method creates clinical_documents table with correct schema
   - Verify Down() method drops table and indexes (for rollback)
   - Test migration on local PostgreSQL instance

4. **Create Raw SQL Migration Script**
   - Create `V001__Create_ClinicalDocument_Table.sql` in `src/backend/scripts/migrations/`
   - Write raw PostgreSQL DDL for table creation (for manual deployment scenarios)
   - Include primary key, foreign keys, indexes, and constraints
   - Include comments on table and columns for documentation
   - Add rollback script as separate file: `V001__Create_ClinicalDocument_Table_Rollback.sql`

## Current Project State

```
src/backend/
├── PatientAccess.Data/ (if exists)
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Appointment.cs
│   │   ├── Provider.cs
│   │   └── TimeSlot.cs
│   ├── Migrations/ (EF Core migrations)
│   └── ApplicationDbContext.cs
├── scripts/
│   └── migrations/ (raw SQL scripts)
└── PatientAccess.Web/
    └── appsettings.json (connection string)
```

**Note**: If PatientAccess.Data project does not exist, entities may be in PatientAccess.Business or PatientAccess.Web. Confirm location before implementation.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Entities/ClinicalDocument.cs | Entity model for clinical documents |
| MODIFY | src/backend/PatientAccess.Data/ApplicationDbContext.cs | Add DbSet and configure relationships |
| CREATE | src/backend/PatientAccess.Data/Migrations/{timestamp}_AddClinicalDocumentTable.cs | EF Core migration (auto-generated) |
| CREATE | src/backend/scripts/migrations/V001__Create_ClinicalDocument_Table.sql | Raw SQL migration script |
| CREATE | src/backend/scripts/migrations/V001__Create_ClinicalDocument_Table_Rollback.sql | Rollback script |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Entity Framework Core Documentation
- **EF Core Migrations**: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- **Entity Configuration**: https://learn.microsoft.com/en-us/ef/core/modeling/entity-types
- **Relationships**: https://learn.microsoft.com/en-us/ef/core/modeling/relationships
- **Indexes**: https://learn.microsoft.com/en-us/ef/core/modeling/indexes

### PostgreSQL Documentation
- **Table Creation**: https://www.postgresql.org/docs/16/sql-createtable.html
- **Foreign Keys**: https://www.postgresql.org/docs/16/ddl-constraints.html#DDL-CONSTRAINTS-FK
- **Indexes**: https://www.postgresql.org/docs/16/indexes.html
- **Data Types**: https://www.postgresql.org/docs/16/datatype.html

### Design Requirements
- **DR-003**: System MUST store clinical documents with unique identifier, patient reference, file metadata, processing status, and extracted data reference (design.md)
- **DR-006**: System MUST enforce referential integrity between patient records and all related entities (design.md)

### Existing Codebase Patterns
- **Entity Model Pattern**: Existing entity models in PatientAccess.Data/Entities/
- **DbContext Pattern**: ApplicationDbContext.cs configuration
- **Migration Pattern**: Existing migrations in PatientAccess.Data/Migrations/

## Build Commands
```powershell
# Navigate to PatientAccess.Data (or where ApplicationDbContext exists)
cd src/backend/PatientAccess.Data

# Add migration
dotnet ef migrations add AddClinicalDocumentTable

# Apply migration to database
dotnet ef database update

# Generate SQL script (for review/manual deployment)
dotnet ef migrations script --output scripts/migrations/AddClinicalDocumentTable.sql

# Rollback migration (if needed)
dotnet ef migrations remove
```

## Implementation Validation Strategy
- [ ] EF Core migration runs successfully on local PostgreSQL instance
- [ ] clinical_documents table created with correct schema (columns, types, constraints)
- [ ] Foreign key constraints enforced (PatientId → patients, UploadedBy → users)
- [ ] Indexes created successfully (PatientId, Status, UploadedAt)
- [ ] Insert test record via DbContext (validates entity configuration)
- [ ] Query test records by PatientId and Status (validates indexes)
- [ ] Migration rollback successful (Down() method works)
- [ ] Raw SQL migration script matches EF-generated schema

## Implementation Checklist
- [ ] Create ClinicalDocument.cs entity with all required properties and navigation properties
- [ ] Add DbSet<ClinicalDocument> to ApplicationDbContext
- [ ] Configure fluent API relationships (FK to Patients, FK to Users, indexes)
- [ ] Generate EF Core migration using `dotnet ef migrations add AddClinicalDocumentTable`
- [ ] Review generated migration for correctness (Up and Down methods)
- [ ] Test migration on local PostgreSQL instance
- [ ] Create raw SQL migration script (V001__Create_ClinicalDocument_Table.sql)
- [ ] Create rollback SQL script (V001__Create_ClinicalDocument_Table_Rollback.sql)
