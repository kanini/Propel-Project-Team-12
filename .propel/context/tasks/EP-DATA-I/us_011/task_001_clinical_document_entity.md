# Task - task_001_clinical_document_entity

## Requirement Reference
- User Story: US_011
- Story Location: .propel/context/tasks/EP-DATA-I/us_011/us_011.md
- Acceptance Criteria:
    - AC-1: ClinicalDocument entity contains ID, patient reference (FK), file name, file size, MIME type, storage path, processing status (enum: Uploaded/Processing/Completed/Failed), upload timestamp, processing timestamp
- Edge Cases:
    - Document deletion cascades to extracted data (orphan prevention)
    - File size validation prevents oversized document metadata persistence

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
Implement ClinicalDocument entity for tracking patient-uploaded PDF documents through AI processing pipeline. Entity includes file metadata, storage path reference, and processing status tracking implementing DR-003 requirements for clinical document management.

## Dependent Tasks
- us_009/task_001_user_entity_implementation — Requires User entity for patient FK

## Impacted Components
- **NEW**: PatientAccess.Data/Models/ProcessingStatus.cs — ProcessingStatus enum
- **NEW**: PatientAccess.Data/Models/ClinicalDocument.cs — ClinicalDocument entity
- **NEW**: PatientAccess.Data/Configurations/ClinicalDocumentConfiguration.cs — Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register ClinicalDocuments DbSet
- **UPDATE**: PatientAccess.Data/Models/User.cs — Add ClinicalDocuments navigation property

## Implementation Plan
1. **Create ProcessingStatus enum** for document processing lifecycle
2. **Define ClinicalDocument entity** with file metadata and storage path
3. **Implement ClinicalDocumentConfiguration** with FK to User (CASCADE delete)
4. **Add indexes** for query performance (PatientId, ProcessingStatus, UploadedAt)
5. **Add navigation property** to User entity
6. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Provider.cs
│   │   ├── TimeSlot.cs
│   │   ├── Appointment.cs
│   │   └── enums...
│   ├── Configurations/
│   │   └── [existing configurations]
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/ProcessingStatus.cs | Enum for document processing states |
| CREATE | src/backend/PatientAccess.Data/Models/ClinicalDocument.cs | Clinical document entity |
| CREATE | src/backend/PatientAccess.Data/Configurations/ClinicalDocumentConfiguration.cs | Fluent API configuration |
| MODIFY | src/backend/PatientAccess.Data/Models/User.cs | Add ClinicalDocuments navigation property |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add ClinicalDocuments DbSet |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddClinicalDocumentEntity.cs | Migration file |

## External References
- [File Storage Best Practices](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads)
- [PostgreSQL Large Object Storage](https://www.postgresql.org/docs/16/largeobjects.html)
- [EF Core File Path Storage](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)

## Build Commands
- Generate migration: `dotnet ef migrations add AddClinicalDocumentEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] ClinicalDocuments table created with file metadata columns
- [ ] FK to User (patient) with CASCADE delete verified
- [ ] Index on ProcessingStatus for status filtering queries
- [ ] StoragePath column type supports full file path strings

## Implementation Checklist
- [ ] Define ProcessingStatus enum (Uploaded=0, Processing=1, Completed=2, Failed=3)
- [ ] Create ClinicalDocument entity with ID, PatientId (FK), FileName, FileSize, MimeType, StoragePath
- [ ] Add ProcessingStatus, UploadedAt, ProcessedAt timestamp fields
- [ ] Implement ClinicalDocumentConfiguration with FK to User (CASCADE delete)
- [ ] Configure column types (varchar for file fields, bigint for FileSize, timestamptz for timestamps)
- [ ] Add indexes on PatientId and ProcessingStatus
- [ ] Update User entity with ClinicalDocuments navigation property
- [ ] Register ClinicalDocuments DbSet in PatientAccessDbContext
- [ ] Generate migration and verify schema
- [ ] Apply migration to database
