# Task - task_001_insurance_noshow_entities

## Requirement Reference
- User Story: US_016
- Story Location: .propel/context/tasks/EP-DATA-II/us_016/us_016.md
- Acceptance Criteria:
    - AC-1: InsuranceRecord entity contains ID, provider name, accepted ID patterns (regex), coverage type (enum), active status, validation rules
    - AC-2: NoShowHistory entity contains ID, patient reference (FK), total appointment count, no-show count, confirmation response rate, average lead time for no-shows, last calculated risk score, last updated timestamp
    - AC-4: InsuranceRecord has index on (provider_name, active) and NoShowHistory has unique constraint on patient_id
- Edge Cases:
    - Invalid regex patterns rejected by application-level validation before persistence
    - Audit logs retention beyond 7 years requires archival strategy

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
Implement InsuranceRecord reference entity for dummy insurance provider data validation (DR-015, FR-021) and NoShowHistory entity for patient no-show risk scoring aggregation (DR-016, FR-023). InsuranceRecord supports pre-check validation; NoShowHistory enables predictive no-show risk assessment.

## Dependent Tasks
- us_009/task_001_user_entity_implementation — Requires User entity for patient FK in NoShowHistory

## Impacted Components
- **NEW**: PatientAccess.Data/Models/CoverageType.cs — CoverageType enum
- **NEW**: PatientAccess.Data/Models/InsuranceRecord.cs — Insurance reference entity
- **NEW**: PatientAccess.Data/Models/NoShowHistory.cs — No-show history aggregation entity
- **NEW**: PatientAccess.Data/Configurations/InsuranceRecordConfiguration.cs — Fluent API configuration
- **NEW**: PatientAccess.Data/Configurations/NoShowHistoryConfiguration.cs — Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register InsuranceRecords and NoShowHistory DbSets

## Implementation Plan
1. **Create CoverageType enum** for insurance classification
2. **Define InsuranceRecord entity** as reference data (no FK dependencies)
3. **Define NoShowHistory entity** with aggregated risk metrics per patient
4. **Implement InsuranceRecordConfiguration** with index on (ProviderName, IsActive)
5. **Implement NoShowHistoryConfiguration** with unique constraint on PatientId
6. **Add FK from NoShowHistory to User** (patient) with CASCADE delete
7. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── User.cs
│   │   └── [other entities]
│   ├── Configurations/
│   │   └── [existing configurations]
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/CoverageType.cs | Enum for insurance coverage types |
| CREATE | src/backend/PatientAccess.Data/Models/InsuranceRecord.cs | Insurance reference entity |
| CREATE | src/backend/PatientAccess.Data/Models/NoShowHistory.cs | No-show history aggregation entity |
| CREATE | src/backend/PatientAccess.Data/Configurations/InsuranceRecordConfiguration.cs | Fluent API for InsuranceRecord |
| CREATE | src/backend/PatientAccess.Data/Configurations/NoShowHistoryConfiguration.cs | Fluent API for NoShowHistory |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add InsuranceRecords and NoShowHistory DbSets |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddInsuranceNoShowEntities.cs | Migration file |

## External References
- [Regex Pattern Validation](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference)
- [No-Show Prediction Models](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC3116346/)
- [Insurance ID Validation Patterns](https://www.cms.gov/medicare/regulations-guidance)

## Build Commands
- Generate migration: `dotnet ef migrations add AddInsuranceNoShowEntities --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] InsuranceRecords and NoShowHistory tables created
- [ ] Index on InsuranceRecord (ProviderName, IsActive) verified
- [ ] Unique constraint on NoShowHistory (PatientId) enforced
- [ ] FK from NoShowHistory to User with CASCADE delete

## Implementation Checklist
- [ ] Define CoverageType enum (Commercial=0, Medicare=1, Medicaid=2, SelfPay=3)
- [ ] Create InsuranceRecord entity with ID, ProviderName, AcceptedIdPatterns (string for regex), CoverageType, IsActive
- [ ] Add ValidationRules (JSONB) for flexible validation logic storage
- [ ] Create NoShowHistory entity with ID, PatientId (FK), TotalAppointmentCount, NoShowCount
- [ ] Add ConfirmationResponseRate (decimal), AverageNoShowLeadTimeHours (int), LastCalculatedRiskScore (decimal 0-100), LastUpdatedAt fields
- [ ] Implement InsuranceRecordConfiguration with composite index on (ProviderName, IsActive)
- [ ] Implement NoShowHistoryConfiguration with unique constraint on PatientId
- [ ] Configure FK from NoShowHistory to User (CASCADE delete)
- [ ] Register InsuranceRecords and NoShowHistory DbSets in PatientAccessDbContext
- [ ] Generate migration and verify constraints
- [ ] Apply migration to database
