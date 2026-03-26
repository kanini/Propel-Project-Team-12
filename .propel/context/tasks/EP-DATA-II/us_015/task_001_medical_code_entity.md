# Task - task_001_medical_code_entity

## Requirement Reference
- User Story: US_015
- Story Location: .propel/context/tasks/EP-DATA-II/us_015/us_015.md
- Acceptance Criteria:
    - AC-1: MedicalCode entity contains ID, extracted data reference (FK), code system (enum: ICD10/CPT), code value, description, confidence score (0-100), verification status (enum: AISuggested/Accepted/Modified/Rejected), verifier reference (FK nullable)
    - AC-3: Index on MedicalCode (extracted_data_id) for retrieval by source data
- Edge Cases:
    - Composite uniqueness on (extracted_data_id, code_system, code_value) prevents duplicates per extraction
    - Medical codes with identical values but different systems handled via composite constraint

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
Implement MedicalCode entity for AI-suggested ICD-10 diagnosis and CPT procedure code mappings with Trust-First verification workflow (DR-013). Entity supports FR-034 (ICD-10 mapping), FR-035 (CPT mapping), and FR-036 (staff verification).

## Dependent Tasks
- us_011/task_002_extracted_clinical_data_entity — Requires ExtractedClinicalData entity for source reference
- us_009/task_001_user_entity_implementation — Requires User entity for verifier FK

## Impacted Components
- **NEW**: PatientAccess.Data/Models/CodeSystem.cs — CodeSystem enum
- **NEW**: PatientAccess.Data/Models/CodeVerificationStatus.cs — CodeVerificationStatus enum
- **NEW**: PatientAccess.Data/Models/MedicalCode.cs — MedicalCode entity
- **NEW**: PatientAccess.Data/Configurations/MedicalCodeConfiguration.cs — Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register MedicalCodes DbSet

## Implementation Plan
1. **Create CodeSystem enum** for ICD-10 vs CPT classification
2. **Create CodeVerificationStatus enum** for verification workflow tracking
3. **Define MedicalCode entity** with confidence scoring and source traceability
4. **Implement MedicalCodeConfiguration** with FK to ExtractedClinicalData and User (verifier)
5. **Add composite unique constraint** on (ExtractedDataId, CodeSystem, CodeValue)
6. **Add indexes** for query performance and duplicate prevention
7. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── ExtractedClinicalData.cs
│   │   ├── User.cs
│   │   └── [other entities]
│   ├── Configurations/
│   │   └── [existing configurations]
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/CodeSystem.cs | Enum for medical coding systems |
| CREATE | src/backend/PatientAccess.Data/Models/CodeVerificationStatus.cs | Enum for verification workflow |
| CREATE | src/backend/PatientAccess.Data/Models/MedicalCode.cs | Medical code entity |
| CREATE | src/backend/PatientAccess.Data/Configurations/MedicalCodeConfiguration.cs | Fluent API with composite constraint |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add MedicalCodes DbSet |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddMedicalCodeEntity.cs | Migration file |

## External References
- [ICD-10 Code System](https://www.cdc.gov/nchs/icd/icd-10-cm.htm)
- [CPT Code System](https://www.ama-assn.org/practice-management/cpt)
- [EF Core Composite Keys](https://learn.microsoft.com/en-us/ef/core/modeling/keys?tabs=fluent-api#composite-keys)

## Build Commands
- Generate migration: `dotnet ef migrations add AddMedicalCodeEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] MedicalCodes table created with composite unique constraint
- [ ] Duplicate code insertion with same (ExtractedDataId, CodeSystem, CodeValue) raises constraint violation
- [ ] FK to ExtractedClinicalData and User (verifier) verified
- [ ] Index on ExtractedDataId for source data lookup

## Implementation Checklist
- [ ] Define CodeSystem enum (ICD10=0, CPT=1)
- [ ] Define CodeVerificationStatus enum (AISuggested=0, Accepted=1, Modified=2, Rejected=3)
- [ ] Create MedicalCode entity with ID, ExtractedDataId (FK), CodeSystem, CodeValue, Description
- [ ] Add ConfidenceScore (decimal 0-100), VerificationStatus, VerifierId (FK nullable), VerifiedAt fields
- [ ] Implement MedicalCodeConfiguration with FK to ExtractedClinicalData (CASCADE) and User (SET NULL)
- [ ] Add composite unique index on (ExtractedDataId, CodeSystem, CodeValue)
- [ ] Add index on ExtractedDataId for source lookup queries
- [ ] Add index on VerificationStatus for filtering unverified codes
- [ ] Register MedicalCodes DbSet in PatientAccessDbContext
- [ ] Generate migration and verify composite constraint
- [ ] Apply migration to database
