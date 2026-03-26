# Task - task_001_waitlist_entry_entity

## Requirement Reference
- User Story: US_014
- Story Location: .propel/context/tasks/EP-DATA-II/us_014/us_014.md
- Acceptance Criteria:
    - AC-1: WaitlistEntry entity contains ID, patient reference (FK), provider reference (FK), preferred start date, preferred end date, preferred time range, notification preference (enum: SMS/Email/Both), priority timestamp, status
    - AC-3: Foreign keys from WaitlistEntry to User and Provider with indexes on patient reference
- Edge Cases:
    - Duplicate waitlist entries prevented via unique constraint on (patient_id, provider_id, preferred_start_date)
    - Application-level validation prevents duplicate entries

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
Implement WaitlistEntry entity for patients to enroll when preferred appointment slots are unavailable (FR-009). Entity captures preferred time ranges, notification preferences, and enforces duplicate prevention via unique constraint on patient-provider-date combination (DR-011).

## Dependent Tasks
- us_009/task_001_user_entity_implementation — Requires User entity for patient FK
- us_010/task_001_provider_timeslot_entities — Requires Provider entity for provider FK

## Impacted Components
- **NEW**: PatientAccess.Data/Models/NotificationPreference.cs — NotificationPreference enum
- **NEW**: PatientAccess.Data/Models/WaitlistStatus.cs — WaitlistStatus enum
- **NEW**: PatientAccess.Data/Models/WaitlistEntry.cs — WaitlistEntry entity
- **NEW**: PatientAccess.Data/Configurations/WaitlistEntryConfiguration.cs — Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register WaitlistEntries DbSet

## Implementation Plan
1. **Create NotificationPreference enum** for contact method selection
2. **Create WaitlistStatus enum** for waitlist lifecycle tracking
3. **Define WaitlistEntry entity** with preferred time range and notification preferences
4. **Implement WaitlistEntryConfiguration** with FKs to User and Provider
5. **Add unique constraint** on (PatientId, ProviderId, PreferredStartDate) to prevent duplicates
6. **Add indexes** for query performance and waitlist matching
7. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Provider.cs
│   │   └── [EP-DATA-I entities]
│   ├── Configurations/
│   │   └── [EP-DATA-I configurations]
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/NotificationPreference.cs | Enum for notification channel selection |
| CREATE | src/backend/PatientAccess.Data/Models/Waitlist Status.cs | Enum for waitlist entry states |
| CREATE | src/backend/PatientAccess.Data/Models/WaitlistEntry.cs | Waitlist entry entity |
| CREATE | src/backend/PatientAccess.Data/Configurations/WaitlistEntryConfiguration.cs | Fluent API with unique constraint |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add WaitlistEntries DbSet |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddWaitlistEntryEntity.cs | Migration file |

## External References
- [EF Core Unique Constraints](https://learn.microsoft.com/en-us/ef/core/modeling/indexes?tabs=data-annotations#index-uniqueness)
- [Composite Indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes?tabs=fluent-api#composite-index)
- [PostgreSQL Date Range Types](https://www.postgresql.org/docs/16/rangetypes.html)

## Build Commands
- Generate migration: `dotnet ef migrations add AddWaitlistEntryEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] WaitlistEntries table created with unique constraint
- [ ] Duplicate entry test raises unique constraint violation
- [ ] FK constraints to User and Provider verified
- [ ] Indexes on PatientId and (ProviderId, Status) created

## Implementation Checklist
- [ ] Define NotificationPreference enum (SMS=0, Email=1, Both=2)
- [ ] Define WaitlistStatus enum (Active=0, Matched=1, Expired=2, Cancelled=3)
- [ ] Create WaitlistEntry entity with ID, PatientId (FK), ProviderId (FK), PreferredStartDate, PreferredEndDate
- [ ] Add PreferredTimeRange (TimeSpan or string), NotificationPreference, PriorityTimestamp, Status fields
- [ ] Implement WaitlistEntryConfiguration with FKs to User (CASCADE) and Provider (RESTRICT)
- [ ] Add unique constraint on (PatientId, ProviderId, PreferredStartDate)
- [ ] Add index on (ProviderId, Status) for waitlist matching queries
- [ ] Add index on PriorityTimestamp for FIFO queue processing
- [ ] Register WaitlistEntries DbSet in PatientAccessDbContext
- [ ] Generate migration and verify unique constraint
- [ ] Apply migration to database
