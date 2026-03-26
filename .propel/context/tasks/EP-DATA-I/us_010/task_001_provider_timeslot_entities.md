# Task - task_001_provider_timeslot_entities

## Requirement Reference
- User Story: US_010
- Story Location: .propel/context/tasks/EP-DATA-I/us_010/us_010.md
- Acceptance Criteria:
    - AC-2: Provider entity contains ID, name, specialty, availability schedule with no login capability
    - AC-3: TimeSlot entity contains ID, provider reference (FK), start time, end time, booking status with proper indexing
- Edge Cases:
    - Provider deletion prevented when appointments exist (RESTRICT cascade rule)
    - Concurrent time slot bookings handled via optimistic concurrency tokens

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
Implement Provider and TimeSlot reference entities required for appointment scheduling. Provider represents healthcare professionals with specialty and availability metadata (reference-only, no authentication). TimeSlot represents bookable time ranges linked to providers with optimistic concurrency support for preventing double-booking race conditions.

## Dependent Tasks
- None — These are reference entities with no external dependencies

## Impacted Components
- **NEW**: PatientAccess.Data/Models/Provider.cs — Provider reference entity
- **NEW**: PatientAccess.Data/Models/TimeSlot.cs — TimeSlot entity
- **NEW**: PatientAccess.Data/Models/BookingStatus.cs — BookingStatus enum
- **NEW**: PatientAccess.Data/Configurations/ProviderConfiguration.cs — Provider Fluent API configuration
- **NEW**: PatientAccess.Data/Configurations/TimeSlotConfiguration.cs — TimeSlot Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register Provider and TimeSlot DbSets

## Implementation Plan
1. **Create BookingStatus enum** for time slot states (Available, Booked, Blocked)
2. **Define Provider entity** as reference-only entity (no password/authentication fields)
3. **Define TimeSlot entity** with provider FK, time range, booking status, and concurrency token
4. **Implement ProviderConfiguration** with indexes on specialty and active status
5. **Implement TimeSlotConfiguration** with indexes on provider and start time for availability queries
6. **Add optimistic concurrency** to TimeSlot using RowVersion property
7. **Register DbSets** in PatientAccessDbContext
8. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── User.cs (completed in previous task)
│   │   ├── UserRole.cs
│   │   └── UserStatus.cs
│   ├── Configurations/
│   │   └── UserConfiguration.cs
│   └── Migrations/
│       └── *_AddUserEntity.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/BookingStatus.cs | Enum for time slot states |
| CREATE | src/backend/PatientAccess.Data/Models/Provider.cs | Provider reference entity |
| CREATE | src/backend/PatientAccess.Data/Models/TimeSlot.cs | TimeSlot entity with concurrency token |
| CREATE | src/backend/PatientAccess.Data/Configurations/ProviderConfiguration.cs | Fluent API for Provider |
| CREATE | src/backend/PatientAccess.Data/Configurations/TimeSlotConfiguration.cs | Fluent API for TimeSlot |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add Providers and TimeSlots DbSets |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddProviderTimeSlotEntities.cs | Migration file |

## External References
- [EF Core Concurrency Tokens](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [Optimistic Concurrency Pattern](https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations)
- [PostgreSQL Timestamp Types](https://www.postgresql.org/docs/16/datatype-datetime.html)

## Build Commands
- Generate migration: `dotnet ef migrations add AddProviderTimeSlotEntities --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] Providers and TimeSlots tables created with correct schema
- [ ] Optimistic concurrency test: Concurrent time slot booking attempt raises DbUpdateConcurrencyException
- [ ] Index on TimeSlot (ProviderId, StartTime) verified for query performance

## Implementation Checklist
- [ ] Define BookingStatus enum (Available = 0, Booked = 1, Blocked = 2)
- [ ] Create Provider entity with ID, Name, Specialty, Email, Phone, IsActive, timestamps
- [ ] Create TimeSlot entity with ID, ProviderId (FK), StartTime, EndTime, BookingStatus, RowVersion
- [ ] Implement ProviderConfiguration with column types, max lengths, indexes
- [ ] Implement TimeSlotConfiguration with FK to Provider (RESTRICT delete), indexes, concurrency token
- [ ] Register Providers and TimeSlots DbSets in PatientAccessDbContext
- [ ] Generate migration and verify Up/Down methods
- [ ] Apply migration to database and verify schema
