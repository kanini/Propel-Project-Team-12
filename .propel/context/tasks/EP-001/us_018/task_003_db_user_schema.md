# Task - TASK_003

## Requirement Reference
- User Story: US_018
- Story Location: .propel/context/tasks/EP-001/us_018/us_018.md
- Acceptance Criteria:
    - AC1: System creates account with status "pending" (requires User table)
- Edge Case:
    - None specific to database

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
| Backend | N/A | N/A |
| Database | PostgreSQL | 16.x |
| Library | Entity Framework Core | 8.x |
| AI/ML | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

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
Create the database schema for the Users table with email as unique identifier, including fields for name, date of birth, contact information, role (Patient/Staff/Admin), credential hash, account status, and verification token with expiration. Implement Entity Framework Core entity configuration and create a versioned migration script.

## Dependent Tasks
- None (Foundational database task)

## Impacted Components
- NEW: src/backend/PatientAccess.Business/Models/Entities/User.cs
- NEW: src/backend/PatientAccess.Business/Models/Enums/UserRole.cs
- NEW: src/backend/PatientAccess.Business/Models/Enums/AccountStatus.cs
- NEW: src/backend/PatientAccess.Data/Configurations/UserConfiguration.cs
- NEW: src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_CreateUsersTable.cs
- MODIFY: src/backend/PatientAccess.Data/Context/PatientAccessDbContext.cs

## Implementation Plan
1. Define User entity class with properties: Id (Guid), Email (unique index), Name, DateOfBirth, Phone, PasswordHash, Role, Status, VerificationToken, VerificationTokenExpiry, CreatedAt, UpdatedAt
2. Create UserRole enum (Patient = 1, Staff = 2, Admin = 3)
3. Create AccountStatus enum (Pending = 0, Active = 1, Inactive = 2, Locked = 3)
4. Implement UserConfiguration using IEntityTypeConfiguration for Fluent API mappings
5. Configure Email as unique index, PasswordHash as required, VerificationToken as nullable
6. Add User DbSet to PatientAccessDbContext
7. Generate EF Core migration using dotnet ef migrations add CreateUsersTable
8. Review generated migration SQL for correctness and zero-downtime compatibility

## Current Project State
```
src/backend/PatientAccess.Data/
├── Context/
│   └── PatientAccessDbContext.cs (to be modified)
├── Configurations/ (to be created)
└── Migrations/ (to be created)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Models/Entities/User.cs | User entity with all required fields per DR-001 |
| CREATE | src/backend/PatientAccess.Business/Models/Enums/UserRole.cs | Enum for Patient, Staff, Admin roles |
| CREATE | src/backend/PatientAccess.Business/Models/Enums/AccountStatus.cs | Enum for account status lifecycle |
| CREATE | src/backend/PatientAccess.Data/Configurations/UserConfiguration.cs | EF Core Fluent API configuration for User entity |
| MODIFY | src/backend/PatientAccess.Data/Context/PatientAccessDbContext.cs | Add DbSet<User> Users property |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_CreateUsersTable.cs | EF migration for Users table creation |

## External References
- **Entity Framework Core Migrations**: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- **EF Core Fluent API**: https://learn.microsoft.com/en-us/ef/core/modeling/
- **PostgreSQL Naming Conventions**: https://www.postgresql.org/docs/16/sql-syntax-lexical.html
- **DR-001 Requirement**: .propel/context/docs/design.md#DR-001

## Build Commands
```powershell
# Navigate to Data layer project
cd src/backend/PatientAccess.Data

# Add migration
dotnet ef migrations add CreateUsersTable --startup-project ../PatientAccess.Web

# Review migration SQL
dotnet ef migrations script --startup-project ../PatientAccess.Web

# Apply migration to database
dotnet ef database update --startup-project ../PatientAccess.Web
```

## Implementation Validation Strategy
- [ ] Migration script generated successfully
- [ ] Migration SQL reviewed for correctness
- [ ] Database schema matches DR-001 specification
- [ ] Unique index on Email column confirmed
- [ ] Enum values stored as integers in database
- [ ] Rollback migration tested

## Implementation Checklist
- [x] Create User.cs entity with Id, Email, Name, DateOfBirth, Phone, PasswordHash, Role, Status fields
- [x] Add VerificationToken, VerificationTokenExpiry nullable fields for email verification
- [x] Add CreatedAt, UpdatedAt timestamp fields
- [x] Create UserRole enum (Patient, Staff, Admin)
- [x] Create AccountStatus enum (Pending, Active, Inactive, Locked)
- [x] Implement UserConfiguration with Fluent API: Email unique index, required fields, max lengths
- [x] Add DbSet<User> Users to PatientAccessDbContext
- [x] Generate migration using dotnet ef migrations add CreateUsersTable
- [x] Review and test migration SQL before applying to development database
