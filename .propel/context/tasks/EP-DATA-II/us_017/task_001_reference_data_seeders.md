# Task - task_001_reference_data_seeders

## Requirement Reference
- User Story: US_017
- Story Location: .propel/context/tasks/EP-DATA-II/us_017/us_017.md
- Acceptance Criteria:
    - AC-1: Insurance data seeder creates at least 10 dummy insurance provider records with varied provider names, ID patterns, coverage types
    - AC-2: Provider data seeder creates at least 5 providers with different specialties and weekly availability schedules generating time slots for next 30 days
    - AC-3: Seeders are idempotent - existing records not duplicated when run multiple times
    - AC-4: Development environment initialization populates both insurance and provider data after migration
- Edge Cases:
    - Seeders environment-guarded to execute only in Development/Staging
    - Seeder failures mid-execution roll back via transaction

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
Implement idempotent data seeders for InsuranceRecord and Provider/TimeSlot entities to populate development/staging environments with realistic test data. Insurance seeder provides dummy providers for FR-021 pre-check validation; Provider seeder creates bookable availability for appointment scheduling testing.

## Dependent Tasks
- us_016/task_001_insurance_noshow_entities — Requires InsuranceRecord entity
- us_010/task_001_provider_timeslot_entities — Requires Provider and TimeSlot entities

## Impacted Components
- **NEW**: src/backend/PatientAccess.Data/Seeders/InsuranceSeeder.cs — Insurance reference data seeder
- **NEW**: src/backend/PatientAccess.Data/Seeders/ProviderSeeder.cs — Provider and TimeSlot seeder
- **NEW**: src/backend/PatientAccess.Data/Seeders/DataSeederExtensions.cs — Seeder registration and orchestration
- **UPDATE**: src/backend/PatientAccess.Web/Program.cs — Call seeders on startup in Development environment

## Implementation Plan
1. **Create InsuranceSeeder class** with 10+ dummy insurance providers (varied names, regex patterns, coverage types)
2. **Implement idempotency check** for InsuranceSeeder (check existing by ProviderName before insert)
3. **Create ProviderSeeder class** with 5+ providers across different specialties
4. **Generate TimeSlots for each provider** covering next 30 days with realistic availability patterns
5. **Implement idempotency check** for ProviderSeeder (check existing by Name before insert)
6. **Create DataSeederExtensions** for DI registration and orchestration
7. **Add environment guard** in Program.cs to run seeders only in Development/Staging
8. **Wrap seeder execution in transaction** for all-or-nothing data population

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── InsuranceRecord.cs (from US_016)
│   │   ├── Provider.cs (from US_010)
│   │   ├── TimeSlot.cs (from US_010)
│   │   └── [all other entities]
│   └── PatientAccessDbContext.cs
├── PatientAccess.Web/
│   └── Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Seeders/InsuranceSeeder.cs | Insurance reference data seeder |
| CREATE | src/backend/PatientAccess.Data/Seeders/ProviderSeeder.cs | Provider and TimeSlot seeder |
| CREATE | src/backend/PatientAccess.Data/Seeders/DataSeederExtensions.cs | Seeder orchestration |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Call seeders after EF Core migration in Development |

## External References
- [EF Core Data Seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)
- [Seeding Best Practices](https://andrewlock.net/deploying-asp-net-core-applications-to-kubernetes-part-8-running-database-migrations/)
- [Idempotent Data Operations](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/)

## Build Commands
- Run with seeders: `dotnet run --project src/backend/PatientAccess.Web --environment Development`
- Verify seeded data: `psql -d <database> -c "SELECT COUNT(*) FROM \\\"InsuranceRecords\\\"; SELECT COUNT(*) FROM \\"Providers\\\""`

## Implementation Validation Strategy
- [ ] InsuranceSeeder creates 10+ records on first run
- [ ] InsuranceSeeder is idempotent (no duplicates on re-run)
- [ ] ProviderSeeder creates 5+ providers with distinct specialties
- [ ] ProviderSeeder generates 30 days of time slots per provider
- [ ] TimeSlots have realistic availability patterns (weekdays, business hours)
- [ ] Seeders execute only in Development environment
- [ ] Seeder failure rolls back transaction (no partial data)

## Implementation Checklist
- [ ] Create InsuranceSeeder class with SeedAsync(PatientAccessDbContext context) method
- [ ] Add 10 dummy InsuranceRecord objects with varied ProviderName, AcceptedIdPatterns (regex), CoverageType
- [ ] Implement idempotency: Check if InsuranceRecords.Any() before inserting
- [ ] Create ProviderSeeder class with SeedAsync method
- [ ] Add 5 Provider objects (Cardiology, Pediatrics, General Practice, Orthopedics, Dermatology)
- [ ] For each Provider, generate TimeSlots for next 30 days (weekdays, 9 AM - 5 PM, 30-minute slots)
- [ ] Implement idempotency: Check if Providers.Any() before inserting
- [ ] Create DataSeederExtensions with SeedDatabaseAsync extension method on IServiceProvider
- [ ] Wrap seeder calls in transaction using context.Database.BeginTransactionAsync()
- [ ] Update Program.cs: After app.Build(), check if (app.Environment.IsDevelopment()) then call await SeedDatabaseAsync(serviceProvider)
- [ ] Test seeder execution in Development environment
- [ ] Verify Production environment does not execute seeders
