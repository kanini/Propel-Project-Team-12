# Task - task_002_appointment_entity

## Requirement Reference
- User Story: US_010
- Story Location: .propel/context/tasks/EP-DATA-I/us_010/us_010.md
- Acceptance Criteria:
    - AC-1: Appointment entity contains ID, patient reference (FK), provider reference (FK), scheduled datetime, status (enum: Scheduled/Confirmed/Arrived/Completed/Cancelled/NoShow), visit reason, walk-in flag, preferred swap reference, no-show risk score, cancellation notice hours
    - AC-4: Foreign keys link Appointment to User (patient) and Provider with proper cascade rules
- Edge Cases:
    - Appointment referencing deleted provider prevented by RESTRICT cascade
    - Concurrent bookings prevented at application level with transaction isolation

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
Implement Appointment entity with full lifecycle status tracking, foreign key relationships to User (patient) and Provider, optional preferred slot swap reference for dynamic scheduling (FR-010), and no-show risk scoring fields. This task establishes the core scheduling entity implementing DR-002 requirements.

## Dependent Tasks
- task_001_provider_timeslot_entities — Requires Provider and TimeSlot entities
- us_009/task_001_user_entity_implementation — Requires User entity for patient FK

## Impacted Components
- **NEW**: PatientAccess.Data/Models/AppointmentStatus.cs — AppointmentStatus enum
- **NEW**: PatientAccess.Data/Models/Appointment.cs — Appointment entity
- **NEW**: PatientAccess.Data/Configurations/AppointmentConfiguration.cs — Appointment Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register Appointments DbSet
- **UPDATE**: PatientAccess.Data/Models/User.cs — Add Appointments navigation property
- **UPDATE**: PatientAccess.Data/Models/Provider.cs — Add Appointments navigation property

## Implementation Plan
1. **Create AppointmentStatus enum** with complete lifecycle states
2. **Define Appointment entity** with all required fields per DR-002 and FR-010 (preferred slot swap)
3. **Implement AppointmentConfiguration** with complex FK relationships:
   - Patient FK with CASCADE delete (patient-owned data)
   - Provider FK with RESTRICT delete (prevent orphaned appointments)
   - TimeSlot FK with RESTRICT delete
   - PreferredSlot FK with SET NULL delete (optional swap target)
4. **Add navigation properties** to User and Provider entities
5. **Configure indexes** for query performance (PatientId, ProviderId, ScheduledDateTime, Status)
6. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Provider.cs (from task_001)
│   │   ├── TimeSlot.cs (from task_001)
│   │   └── enums...
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── ProviderConfiguration.cs (from task_001)
│   │   └── TimeSlotConfiguration.cs (from task_001)
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/AppointmentStatus.cs | Enum for appointment lifecycle states |
| CREATE | src/backend/PatientAccess.Data/Models/Appointment.cs | Core appointment entity |
| CREATE | src/backend/PatientAccess.Data/Configurations/AppointmentConfiguration.cs | Fluent API with FK cascade rules |
| MODIFY | src/backend/PatientAccess.Data/Models/User.cs | Add Appointments navigation property |
| MODIFY | src/backend/PatientAccess.Data/Models/Provider.cs | Add Appointments navigation property |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add Appointments DbSet |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddAppointmentEntity.cs | Migration file |

## External References
- [EF Core Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)
- [Delete Behavior Options](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete)
- [EF Core Self-Referencing Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#self-referencing-relationship)

## Build Commands
- Generate migration: `dotnet ef migrations add AddAppointmentEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] Appointments table created with all FK constraints
- [ ] Provider delete blocked when appointments exist (RESTRICT test)
- [ ] Patient delete cascades to appointments (CASCADE test)
- [ ] PreferredSlot FK allows NULL and sets to NULL on referenced slot delete
- [ ] Indexes on PatientId, ProviderId, ScheduledDateTime verified

## Implementation Checklist
- [ ] Define AppointmentStatus enum (Scheduled=0, Confirmed=1, Arrived=2, Completed=3, Cancelled=4, NoShow=5)
- [ ] Create Appointment entity with all properties per DR-002
- [ ] Add TimeSlotId, PreferredSlotId, PatientId, ProviderId FK properties
- [ ] Add NoShowRiskScore (decimal), ConfirmationReceived (bool), CancellationNoticeHours (int) fields
- [ ] Implement AppointmentConfiguration with four FK relationships and correct delete behaviors
- [ ] Configure indexes for query optimization
- [ ] Update User entity with Appointments navigation property
- [ ] Update Provider entity with Appointments navigation property
- [ ] Register Appointments DbSet in PatientAccessDbContext
- [ ] Generate migration and verify FK constraint names and cascade rules
- [ ] Apply migration and test FK behaviors via SQL
