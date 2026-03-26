# Task - task_002_extracted_clinical_data_entity

## Requirement Reference
- User Story: US_011
- Story Location: .propel/context/tasks/EP-DATA-I/us_011/us_011.md
- Acceptance Criteria:
    - AC-2: ExtractedClinicalData entity contains ID, source document reference (FK), patient reference (FK), data type (enum: Vital/Medication/Allergy/Diagnosis/LabResult), key, value, confidence score (0-100), verification status (enum: AISuggested/StaffVerified/Rejected), source page number, source text excerpt, verifier reference
    - AC-3: Foreign keys from ExtractedClinicalData to ClinicalDocument and User with indexes on patient reference and data type
    - AC-4: pgvector support configured with optional vector column (embedding vector(1536)) for semantic similarity search
- Edge Cases:
    - Extracted data cascades delete when source document is deleted
    - Confidence score validation at application level (0-100 range)

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

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | N/A | N/A |
| Backend | .NET 8 ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL with pgvector | 16 |
| Library | Entity Framework Core | 8.0.x |
| Library | Pgvector.EntityFrameworkCore | 0.2.x+ |

**Note**: All code and libraries MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview
Implement ExtractedClinicalData entity with pgvector support for AI-extracted clinical information from documents. Entity includes confidence scoring, verification workflow tracking, source traceability, and optional 1536-dimensional embedding vector for semantic similarity search (DR-004, DR-010).

## Dependent Tasks
- task_001_clinical_document_entity — Requires ClinicalDocument entity for source document FK
- us_009/task_001_user_entity_implementation — Requires User entity for patient and verifier FKs

## Impacted Components
- **NEW**: PatientAccess.Data/Models/ClinicalDataType.cs — ClinicalDataType enum
- **NEW**: PatientAccess.Data/Models/VerificationStatus.cs — VerificationStatus enum
- **NEW**: PatientAccess.Data/Models/ExtractedClinicalData.cs — ExtractedClinicalData entity with vector field
- **NEW**: PatientAccess.Data/Configurations/ExtractedClinicalDataConfiguration.cs — Fluent API with pgvector configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register ExtractedClinicalData DbSet, enable pgvector extension

## Implementation Plan
1. **Create ClinicalDataType enum** for data classification (Vital, Medication, Allergy, Diagnosis, LabResult)
2. **Create VerificationStatus enum** for Trust-First workflow tracking
3. **Define ExtractedClinicalData entity** with confidence score, source traceability, and pgvector embedding field
4. **Install Pgvector.EntityFrameworkCore NuGet package** for vector support
5. **Implement ExtractedClinicalDataConfiguration** with dual FKs (document and patient) and vector column mapping
6. **Enable pgvector extension** in PatientAccessDbContext
7. **Add indexes** for query performance (PatientId, DataType, DocumentId, VerificationStatus)
8. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── ClinicalDocument.cs (from task_001)
│   │   └── [other entities]
│   ├── Configurations/
│   │   ├── ClinicalDocumentConfiguration.cs (from task_001)
│   │   └── [other configurations]
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/ClinicalDataType.cs | Enum for extracted data classification |
| CREATE | src/backend/PatientAccess.Data/Models/VerificationStatus.cs | Enum for verification workflow |
| CREATE | src/backend/PatientAccess.Data/Models/ExtractedClinicalData.cs | Entity with vector embedding field |
| CREATE | src/backend/PatientAccess.Data/Configurations/ExtractedClinicalDataConfiguration.cs | Fluent API with pgvector column |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Enable pgvector extension, add DbSet |
| MODIFY | src/backend/PatientAccess.Data.csproj | Add Pgvector.EntityFrameworkCore NuGet package |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddExtractedClinicalDataEntity.cs | Migration with vector extension |

## External References
- [pgvector for PostgreSQL](https://github.com/pgvector/pgvector)
- [Pgvector.EntityFrameworkCore](https://github.com/pgvector/pgvector-dotnet)
- [Vector Similarity Search Patterns](https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/how-to-use-pgvector)
- [OpenAI Embedding Dimensions](https://platform.openai.com/docs/guides/embeddings)

## Build Commands
- Add NuGet package: `dotnet add src/backend/PatientAccess.Data package Pgvector.EntityFrameworkCore`
- Generate migration: `dotnet ef migrations add AddExtractedClinicalDataEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Pgvector.EntityFrameworkCore NuGet package added to project
- [ ] Migration applies successfully with vector extension enabled
- [ ] ExtractedClinicalData table created with vector(1536) column
- [ ] FK constraints to ClinicalDocument and User verified
- [ ] Indexes on PatientId, DataType, VerificationStatus created
- [ ] Vector column allows NULL values (optional embedding)

## Implementation Checklist
- [ ] Define ClinicalDataType enum (Vital=0, Medication=1, Allergy=2, Diagnosis=3, LabResult=4)
- [ ] Define VerificationStatus enum (AISuggested=0, StaffVerified=1, Rejected=2)
- [ ] Add Pgvector.EntityFrameworkCore NuGet package to PatientAccess.Data project
- [ ] Create ExtractedClinicalData entity with all required fields
- [ ] Add Embedding property as Vector (nullable) for pgvector storage
- [ ] Implement ExtractedClinicalDataConfiguration with FK to ClinicalDocument (CASCADE) and User (patient, verifier)
- [ ] Configure vector column as vector(1536) using HasColumnType
- [ ] Enable pgvector extension in PatientAccessDbContext via modelBuilder.HasPostgresExtension("vector")
- [ ] Add indexes on composite (PatientId, DataType) and VerificationStatus
- [ ] Register ExtractedClinicalData DbSet in PatientAccessDbContext
- [ ] Generate migration and verify pgvector extension creation
- [ ] Apply migration and validate vector column in database schema
