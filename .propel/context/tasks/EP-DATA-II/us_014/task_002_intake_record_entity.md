# Task - task_002_intake_record_entity

## Requirement Reference
- User Story: US_014
- Story Location: .propel/context/tasks/EP-DATA-II/us_014/us_014.md
- Acceptance Criteria:
    - AC-2: IntakeRecord entity contains ID, appointment reference (FK), patient reference (FK), intake mode (enum: AI/Manual), medical history (JSONB), current medications (JSONB), allergies (JSONB), visit concerns, insurance validation status, insurance record reference (FK nullable), completion status
    - AC-3: Foreign keys from IntakeRecord to Appointment and User with indexes on patient reference
    - AC-4: JSONB columns enable efficient querying and indexing of nested intake data
- Edge Cases:
    - Intake record remains when appointment cancelled (historical reference with soft delete flag)
    - Structured health data stored as JSONB for flexible schema evolution

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
Implement IntakeRecord entity for storing dual-mode patient intake data (AI conversational or manual form) with JSONB columns for flexible structured health information (DR-012). Entity supports FR-017 (AI intake), FR-018 (manual intake), and insurance pre-check validation (FR-021).

## Dependent Tasks
- us_010/task_002_appointment_entity — Requires Appointment entity for appointment FK
- us_009/task_001_user_entity_implementation — Requires User entity for patient FK

## Impacted Components
- **NEW**: PatientAccess.Data/Models/IntakeMode.cs — IntakeMode enum
- **NEW**: PatientAccess.Data/Models/IntakeStatus.cs — IntakeStatus enum
- **NEW**: PatientAccess.Data/Models/IntakeRecord.cs — IntakeRecord entity with JSONB fields
- **NEW**: PatientAccess.Data/Configurations/IntakeRecordConfiguration.cs — Fluent API with JSONB columns
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register IntakeRecords DbSet

## Implementation Plan
1. **Create IntakeMode enum** for tracking intake method (AI vs Manual)
2. **Create IntakeStatus enum** for completion tracking
3. **Define IntakeRecord entity** with multiple JSONB columns for structured health data
4. **Implement IntakeRecordConfiguration** with FKs to Appointment, User, and optional InsuranceRecord
5. **Configure JSONB columns** for MedicalHistory, CurrentMedications, Allergies
6. **Add indexes** for query performance (PatientId, AppointmentId, CompletionStatus)
7. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── Appointment.cs
│   │   ├── User.cs
│   │   ├── WaitlistEntry.cs (from task_001)
│   │   └── [other entities]
│   ├── Configurations/
│   │   └── [existing configurations]
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/IntakeMode.cs | Enum for intake method tracking |
| CREATE | src/backend/PatientAccess.Data/Models/IntakeStatus.cs | Enum for completion status |
| CREATE | src/backend/PatientAccess.Data/Models/IntakeRecord.cs | Intake record entity with JSONB fields |
| CREATE | src/backend/PatientAccess.Data/Configurations/IntakeRecordConfiguration.cs | Fluent API with JSONB configuration |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add IntakeRecords DbSet |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddIntakeRecordEntity.cs | Migration file |

## External References
- [EF Core JSONB Support](https://www.npgsql.org/efcore/mapping/json.html)
- [PostgreSQL JSONB Indexing](https://www.postgresql.org/docs/16/datatype-json.html#JSON-INDEXING)
- [JSONB Query Patterns](https://www.postgresql.org/docs/16/functions-json.html)

## Build Commands
- Generate migration: `dotnet ef migrations add AddIntakeRecordEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] IntakeRecords table created with JSONB columns
- [ ] FK to Appointment with SET NULL delete (preserve historical intake records)
- [ ] FK to User (patient) with CASCADE delete
- [ ] JSONB columns store nested JSON structures correctly
- [ ] Indexes on PatientId and CompletionStatus verified

## Implementation Checklist
- [ ] Define IntakeMode enum (AI=0, Manual=1, Hybrid=2)
- [ ] Define IntakeStatus enum (InProgress=0, Completed=1, Abandoned=2)
- [ ] Create IntakeRecord entity with ID, AppointmentId (FK nullable), PatientId (FK), IntakeMode, CompletionStatus
- [ ] Add JSONB properties: MedicalHistory (Dictionary), CurrentMedications (Dictionary), Allergies (Dictionary)
- [ ] Add InsuranceValidationStatus (bool), InsuranceRecordId (FK nullable), VisitConcerns (string), CompletedAt fields
- [ ] Implement IntakeRecordConfiguration with FK to Appointment (SET NULL), User (CASCADE)
- [ ] Configure JSONB columns using HasColumnType("jsonb") for health data dictionaries
- [ ] Add index on PatientId for patient-specific intake history queries
- [ ] Add index on CompletionStatus for filtering incomplete intakes
- [ ] Register IntakeRecords DbSet in PatientAccessDbContext
- [ ] Generate migration and verify JSONB column types
- [ ] Apply migration to database
