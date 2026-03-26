# Task - task_001_db_patient_profile_schema

## Requirement Reference
- User Story: US_047
- Story Location: .propel/context/tasks/EP-007/us_047/us_047.md
- Acceptance Criteria:
    - **AC3**: Given aggregation produces a de-duplicated profile, When the PatientProfile entity is updated, Then it contains consolidated conditions, medications, allergies, vital trends, and encounters from all processed documents.
    - **AC4**: Given a new document is processed for an existing patient, When extraction completes, Then the aggregation service incrementally updates the patient profile without reprocessing previously aggregated data.
- Edge Case:
    - How does the system handle data from documents uploaded years apart? Temporal context is preserved — older vitals do not overwrite newer ones.

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
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Entity Framework Core | 8.0 |
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

Create database schema for PatientProfile entity to store de-duplicated, consolidated clinical data aggregated from multiple documents. This task implements the 360-Degree Patient View data model with separate tables for consolidated conditions, medications, allergies, vital trends, and encounters. The schema supports temporal tracking (preserving older data without overwriting), source document traceability, conflict flagging, and incremental updates. The design enables efficient querying for patient health summaries while maintaining data lineage and enabling staff verification workflows.

**Key Capabilities:**
- PatientProfile table (one-to-one with Patient)
- ConsolidatedCondition table (de-duplicated diagnoses with temporal tracking)
- ConsolidatedMedication table (de-duplicated medications with source references)
- ConsolidatedAllergy table (de-duplicated allergies with severity)
- VitalTrend table (time-series vital signs preserving historical data)
- ConsolidatedEncounter table (de-duplicated encounters from documents)
- DataConflict table (flagged conflicts requiring staff review)
- Foreign keys to ExtractedClinicalData for source traceability
- Indexes for patient queries and aggregation performance
- Timestamp tracking (CreatedAt, UpdatedAt, LastAggregatedAt)

## Dependent Tasks
- EP-006-II: US_045: task_001_db_extracted_data_schema (ExtractedClinicalData table for source data)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Data/Entities/PatientProfile.cs` - Aggregated patient profile entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/ConsolidatedCondition.cs` - De-duplicated conditions entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/ConsolidatedMedication.cs` - De-duplicated medications entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/ConsolidatedAllergy.cs` - De-duplicated allergies entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/VitalTrend.cs` - Time-series vital signs entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/ConsolidatedEncounter.cs` - De-duplicated encounters entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/DataConflict.cs` - Conflict tracking entity
- **NEW**: `src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddPatientProfileAggregation.cs` - EF Core migration
- **MODIFY**: `src/backend/PatientAccess.Data/ApplicationDbContext.cs` - Add DbSet properties

## Implementation Plan

1. **Create PatientProfile Entity**
   - Properties:
     - Id (int, PK, auto-increment)
     - PatientId (int, FK to Patients, unique)
     - LastAggregatedAt (DateTime, UTC timestamp of last aggregation)
     - TotalDocumentsProcessed (int, count of documents contributing to profile)
     - HasUnresolvedConflicts (bool, flag for conflicts requiring review)
     - ProfileCompleteness (decimal, percentage 0-100 based on data coverage)
     - CreatedAt (DateTime, UTC)
     - UpdatedAt (DateTime, UTC)
   - Navigation properties:
     - Patient (Patient entity)
     - Conditions (List<ConsolidatedCondition>)
     - Medications (List<ConsolidatedMedication>)
     - Allergies (List<ConsolidatedAllergy>)
     - VitalTrends (List<VitalTrend>)
     - Encounters (List<ConsolidatedEncounter>)
     - Conflicts (List<DataConflict>)

2. **Create ConsolidatedCondition Entity**
   - Properties:
     - Id (Guid, PK)
     - PatientProfileId (int, FK to PatientProfiles)
     - ConditionName (string, 500 chars, indexed)
     - ICD10Code (string, 20 chars, indexed, nullable)
     - DiagnosisDate (DateTime, nullable)
     - Status (string, 50 chars: "Active", "Resolved", "Historical")
     - Severity (string, 50 chars, nullable)
     - SourceDocumentIds (List<Guid>, JSONB array, FK refs to ClinicalDocuments)
     - SourceDataIds (List<Guid>, JSONB array, FK refs to ExtractedClinicalData)
     - IsDuplicate (bool, marks if merged from duplicates)
     - DuplicateCount (int, count of merged duplicates)
     - FirstRecordedAt (DateTime, earliest mention across documents)
     - LastUpdatedAt (DateTime, most recent mention)
     - CreatedAt (DateTime, UTC)
   - Index: PatientProfileId, ConditionName, ICD10Code, Status

3. **Create ConsolidatedMedication Entity**
   - Properties:
     - Id (Guid, PK)
     - PatientProfileId (int, FK to PatientProfiles)
     - DrugName (string, 500 chars, indexed)
     - Dosage (string, 200 chars)
     - Frequency (string, 200 chars)
     - RouteOfAdministration (string, 100 chars, nullable)
     - StartDate (DateTime, nullable)
     - EndDate (DateTime, nullable)
     - Status (string, 50 chars: "Active", "Discontinued", "Historical")
     - SourceDocumentIds (List<Guid>, JSONB array)
     - SourceDataIds (List<Guid>, JSONB array)
     - IsDuplicate (bool)
     - DuplicateCount (int)
     - HasConflict (bool, flag for different doses)
     - FirstRecordedAt (DateTime)
     - LastUpdatedAt (DateTime)
     - CreatedAt (DateTime, UTC)
   - Index: PatientProfileId, DrugName, Status

4. **Create ConsolidatedAllergy Entity**
   - Properties:
     - Id (Guid, PK)
     - PatientProfileId (int, FK to PatientProfiles)
     - AllergenName (string, 500 chars, indexed)
     - Reaction (string, 1000 chars, nullable)
     - Severity (string, 50 chars: "Mild", "Moderate", "Severe", "Critical")
     - OnsetDate (DateTime, nullable)
     - Status (string, 50 chars: "Active", "Inactive")
     - SourceDocumentIds (List<Guid>, JSONB array)
     - SourceDataIds (List<Guid>, JSONB array)
     - IsDuplicate (bool)
     - DuplicateCount (int)
     - FirstRecordedAt (DateTime)
     - LastUpdatedAt (DateTime)
     - CreatedAt (DateTime, UTC)
   - Index: PatientProfileId, AllergenName, Severity

5. **Create VitalTrend Entity**
   - Properties:
     - Id (Guid, PK)
     - PatientProfileId (int, FK to PatientProfiles)
     - VitalType (string, 100 chars: "BloodPressure", "HeartRate", "Temperature", "Weight", "Height", "BMI", "O2Saturation")
     - Value (string, 200 chars)
     - Unit (string, 50 chars)
     - RecordedAt (DateTime, indexed for time-series queries)
     - SourceDocumentId (Guid, FK to ClinicalDocuments)
     - SourceDataId (Guid, FK to ExtractedClinicalData)
     - CreatedAt (DateTime, UTC)
   - Index: PatientProfileId, VitalType, RecordedAt (composite for time-series queries)
   - **No de-duplication**: Preserves all vital measurements over time

6. **Create ConsolidatedEncounter Entity**
   - Properties:
     - Id (Guid, PK)
     - PatientProfileId (int, FK to PatientProfiles)
     - EncounterDate (DateTime, indexed)
     - EncounterType (string, 100 chars: "Inpatient", "Outpatient", "Emergency", "Telehealth")
     - Provider (string, 200 chars, nullable)
     - Facility (string, 200 chars, nullable)
     - ChiefComplaint (string, 1000 chars, nullable)
     - SourceDocumentIds (List<Guid>, JSONB array)
     - SourceDataIds (List<Guid>, JSONB array)
     - IsDuplicate (bool)
     - DuplicateCount (int)
     - CreatedAt (DateTime, UTC)
   - Index: PatientProfileId, EncounterDate

7. **Create DataConflict Entity**
   - Properties:
     - Id (Guid, PK)
     - PatientProfileId (int, FK to PatientProfiles)
     - ConflictType (string, 100 chars: "MedicationDosageMismatch", "DiagnosisMismatch", "AllergyMismatch")
     - EntityType (string, 50 chars: "Medication", "Condition", "Allergy")
     - EntityId (Guid, polymorphic FK to consolidated entities)
     - Description (string, 2000 chars)
     - SourceDataIds (List<Guid>, JSONB array, conflicting ExtractedClinicalData IDs)
     - ResolutionStatus (string, 50 chars: "Unresolved", "Resolved", "Dismissed")
     - ResolvedBy (int, FK to Users, nullable)
     - ResolvedAt (DateTime, nullable)
     - CreatedAt (DateTime, UTC)
   - Index: PatientProfileId, ResolutionStatus, EntityType

8. **Create EF Core Migration**
   - Generate migration: `Add-Migration AddPatientProfileAggregation`
   - Configure entity relationships:
     - One-to-One: Patient → PatientProfile
     - One-to-Many: PatientProfile → ConsolidatedCondition/Medication/Allergy/VitalTrend/Encounter/DataConflict
     - Many-to-Many (implicit via JSONB): ConsolidatedEntities → ExtractedClinicalData (source traceability)
   - Add indexes for performance:
     - PatientProfile: PatientId (unique)
     - ConsolidatedCondition: PatientProfileId, ConditionName, ICD10Code
     - ConsolidatedMedication: PatientProfileId, DrugName, Status
     - ConsolidatedAllergy: PatientProfileId, AllergenName, Severity
     - VitalTrend: PatientProfileId, VitalType, RecordedAt (composite)
     - ConsolidatedEncounter: PatientProfileId, EncounterDate
     - DataConflict: PatientProfileId, ResolutionStatus
   - Add foreign key constraints with ON DELETE CASCADE for PatientProfile dependencies
   - Add CHECK constraints: ProfileCompleteness BETWEEN 0 AND 100

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── Patient.cs (from EP-001)
│   │   ├── ClinicalDocument.cs (from EP-006-I)
│   │   └── ExtractedClinicalData.cs (from EP-006-II)
│   ├── ApplicationDbContext.cs
│   └── Migrations/
└── PatientAccess.Web/
    └── Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Entities/PatientProfile.cs | Aggregated patient profile entity |
| CREATE | src/backend/PatientAccess.Data/Entities/ConsolidatedCondition.cs | De-duplicated conditions |
| CREATE | src/backend/PatientAccess.Data/Entities/ConsolidatedMedication.cs | De-duplicated medications |
| CREATE | src/backend/PatientAccess.Data/Entities/ConsolidatedAllergy.cs | De-duplicated allergies |
| CREATE | src/backend/PatientAccess.Data/Entities/VitalTrend.cs | Time-series vital signs |
| CREATE | src/backend/PatientAccess.Data/Entities/ConsolidatedEncounter.cs | De-duplicated encounters |
| CREATE | src/backend/PatientAccess.Data/Entities/DataConflict.cs | Conflict tracking |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddPatientProfileAggregation.cs | EF migration |
| MODIFY | src/backend/PatientAccess.Data/ApplicationDbContext.cs | Add DbSet properties |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Entity Framework Core Documentation
- **Migrations**: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- **Relationships**: https://learn.microsoft.com/en-us/ef/core/modeling/relationships
- **Indexes**: https://learn.microsoft.com/en-us/ef/core/modeling/indexes
- **Table Splitting**: https://learn.microsoft.com/en-us/ef/core/modeling/table-splitting
- **JSONB Columns**: https://www.npgsql.org/efcore/mapping/json.html

### PostgreSQL JSONB
- **JSONB Type**: https://www.postgresql.org/docs/16/datatype-json.html
- **JSONB Performance**: https://www.postgresql.org/docs/16/functions-json.html

### Design Requirements
- **FR-030**: System MUST aggregate extracted data from multiple clinical documents into a de-duplicated, consolidated patient profile (spec.md)
- **FR-031**: System MUST explicitly highlight critical data conflicts requiring staff verification (spec.md)
- **FR-032**: System MUST generate 360-Degree Patient View displaying unified patient health summary (spec.md)
- **AIR-005**: System MUST aggregate extracted data using entity resolution pattern (design.md)
- **DR-001**: Database schema MUST support referential integrity with foreign keys (design.md)

### Existing Codebase Patterns
- **Entity Pattern**: `src/backend/PatientAccess.Data/Entities/Patient.cs`
- **Migration Pattern**: Previous EF Core migrations in `Migrations/` folder

## Build Commands
```powershell
# Create migration
cd src/backend/PatientAccess.Data
dotnet ef migrations add AddPatientProfileAggregation --startup-project ../PatientAccess.Web

# Update database
dotnet ef database update --startup-project ../PatientAccess.Web

# Build solution
cd ..
dotnet build

# Run application
cd PatientAccess.Web
dotnet run
```

## Implementation Validation Strategy
- [ ] Entity classes follow EF Core conventions
- [ ] All navigation properties configured correctly
- [ ] Migration generates expected tables and columns
- [ ] Foreign key constraints created successfully
- [ ] Indexes created on PatientId, ConditionName, DrugName, AllergenName, VitalType, EncounterDate
- [ ] JSONB columns mapped correctly for SourceDocumentIds and SourceDataIds
- [ ] CHECK constraint enforces ProfileCompleteness 0-100 range
- [ ] One-to-one relationship between Patient and PatientProfile enforced
- [ ] Temporal tracking preserved (older vitals not overwritten)
- [ ] Database schema matches design specification
- [ ] Rollback migration works without errors

## Implementation Checklist
- [ ] Create PatientProfile entity with LastAggregatedAt and ProfileCompleteness properties
- [ ] Create ConsolidatedCondition entity with temporal tracking (FirstRecordedAt, LastUpdatedAt)
- [ ] Create ConsolidatedMedication entity with conflict flag (HasConflict)
- [ ] Create ConsolidatedAllergy entity with severity classification
- [ ] Create VitalTrend entity with time-series indexing (no de-duplication)
- [ ] Create ConsolidatedEncounter entity with EncounterType enumeration
- [ ] Create DataConflict entity with ResolutionStatus workflow
- [ ] Configure entity relationships in ApplicationDbContext
- [ ] Add DbSet properties for all new entities
- [ ] Create EF Core migration with indexes and foreign keys
- [ ] Add CHECK constraint for ProfileCompleteness (0-100)
- [ ] Validate migration Up/Down methods generate correct SQL
