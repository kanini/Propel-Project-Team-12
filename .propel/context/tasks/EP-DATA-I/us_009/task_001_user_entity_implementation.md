# Task - task_001_user_entity_implementation

## Requirement Reference
- User Story: US_009
- Story Location: .propel/context/tasks/EP-DATA-I/us_009/us_009.md
- Acceptance Criteria:
    - AC-1: User entity contains fields for ID (GUID), email (unique), name, date of birth, phone, password hash, role (enum), status (enum), and timestamps (created, updated)
    - AC-2: Email uniqueness enforced with unique index at database level  
    - AC-3: EF Core Fluent API configuration defines column types, max lengths, required fields, and indexes
    - AC-4: Users table created in PostgreSQL with all constraints after migration
- Edge Cases:
    - Duplicate email insert attempts raise database unique constraint violation
    - Null or empty email values rejected by NOT NULL constraint and application validation

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
Implement the User entity model with enum types (UserRole, UserStatus), EF Core Fluent API configuration enforcing email uniqueness, and generate the initial migration to create the Users table in PostgreSQL. This task establishes the foundational entity for authentication, implementing DR-001 requirements for user data persistence with secure credential storage and role-based access control.

## Dependent Tasks
- None — This is a foundational task establishing core user entity

## Impacted Components
- **NEW**: PatientAccess.Data/Models/User.cs — User entity model
- **NEW**: PatientAccess.Data/Models/UserRole.cs — UserRole enum (Patient, Staff, Admin)
- **NEW**: PatientAccess.Data/Models/UserStatus.cs — UserStatus enum (PendingVerification, Active, Suspended, Deactivated)
- **UPDATE**: PatientAccess.Data/Configurations/UserConfiguration.cs — EF Core Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — DbSet registration
- **NEW**: PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddUserEntity.cs — EF Core migration

## Implementation Plan
1. **Create enum types** for UserRole and UserStatus to support role-based access control and account lifecycle management
2. **Define User entity model** with all required properties per DR-001
3. **Implement UserConfiguration** using Fluent API to configure:
   - Column types and max lengths for data integrity
   - Required field constraints
   - Unique index on Email column
   - Default values for timestamps and GUID generation
4. **Register User DbSet** in PatientAccessDbContext
5. **Generate EF Core migration** using `dotnet ef migrations add AddUserEntity`
6. **Apply migration** to development database using `dotnet ef database update`
7. **Verify schema** by querying PostgreSQL to confirm Users table structure and constraints

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   ├── Configurations/
│   └── Migrations/
└── PatientAccess.sln
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/UserRole.cs | Enum defining Patient, Staff, Admin roles |
| CREATE | src/backend/PatientAccess.Data/Models/UserStatus.cs | Enum defining user account lifecycle states |
| CREATE | src/backend/PatientAccess.Data/Models/User.cs | Core user entity with authentication and profile fields |
| CREATE | src/backend/PatientAccess.Data/Configurations/UserConfiguration.cs | Fluent API configuration for User entity |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add Users DbSet and apply UserConfiguration |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddUserEntity.cs | EF Core migration for Users table |

## External References
- [EF Core Fluent API Documentation](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL UUID Generation](https://www.postgresql.org/docs/16/functions-uuid.html)
- [EF Core Enum Mapping](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)

## Build Commands
- Generate migration: `dotnet ef migrations add AddUserEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`
- Run tests: `dotnet test src/backend/PatientAccess.Tests`

## Implementation Validation Strategy
- [ ] Unit tests pass for User entity property validation
- [ ] Migration file generated successfully
- [ ] Migration applies to database without errors
- [ ] Users table exists with correct column types and constraints
- [ ] Unique index IX_Users_Email exists and enforces email uniqueness
- [ ] Duplicate email insert test raises PostgreSQL unique constraint exception
- [ ] Default values for GUID and timestamps are generated automatically
- [ ] UserRole and UserStatus enums map correctly to integer storage

## Implementation Checklist
- [ ] Define UserRole enum with Patient = 1, Staff = 2, Admin = 3
- [ ] Define UserStatus enum with PendingVerification = 0, Active = 1, Suspended = 2, Deactivated = 3
- [ ] Create User entity class with all properties per DR-001
- [ ] Add navigation properties for Appointments, ClinicalDocuments, AuditLogs (prepare for future relationships)
- [ ] Implement UserConfiguration with Fluent API defining table name, primary key, column types
- [ ] Configure unique index on Email column using HasIndex().IsUnique()
- [ ] Configure indexes on Role and Status columns for query performance
- [ ] Set default values: UserId (gen_random_uuid()), CreatedAt (NOW())
- [ ] Register Users DbSet in PatientAccessDbContext
- [ ] Apply UserConfiguration in OnModelCreating using modelBuilder.ApplyConfiguration
- [ ] Generate migration using dotnet ef migrations add command
- [ ] Review migration Up/Down methods for correctness
- [ ] Apply migration to development database
- [ ] Query database schema to verify Users table structure
- [ ] Test unique email constraint by attempting duplicate insert via SQL
