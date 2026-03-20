# Task - task_002_efcore_dbcontext_configuration

## Requirement Reference
- User Story: us_003
- Story Location: .propel/context/tasks/EP-TECH/us_003/us_003.md
- Acceptance Criteria:
    - AC-3: `PatientAccessDbContext` class exists inheriting from `DbContext` with proper connection string resolution from configuration
    - AC-4: Can generate and apply EF Core migrations with `dotnet ef migrations add` and `dotnet ef database update`
- Edge Case:
    - Application should log connection error and fail fast with descriptive message if database connection fails at startup

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
| Backend | .NET ASP.NET Core | 8.0 |
| Backend | Entity Framework Core | 8.x |
| Library | Npgsql.EntityFrameworkCore.PostgreSQL | 8.x |
| Library | Microsoft.EntityFrameworkCore.Tools | 8.x |
| Database | PostgreSQL | 16.x |

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
Configure Entity Framework Core as the ORM for PostgreSQL database access. Create `PatientAccessDbContext` class inheriting from `DbContext` with proper connection string resolution from `appsettings.json` and environment variables. Register DbContext in dependency injection container with Scoped lifetime. Install EF Core Tools for migration management. Implement database connection health check to ensure fail-fast behavior on startup if database is unreachable.

## Dependent Tasks
- task_001_database_postgresql_provisioning (US_003) - Requires database connection string

## Impacted Components
- **NEW** src/backend/PatientAccess.Data/PatientAccessDbContext.cs - EF Core DbContext class
- **MODIFY** src/backend/PatientAccess.Data/PatientAccess.Data.csproj - Add EF Core NuGet packages
- **MODIFY** src/backend/PatientAccess.Web/Program.cs - Register DbContext in DI container
- **NEW** src/backend/PatientAccess.Web/Extensions/ServiceCollectionExtensions.cs - Extension methods for DI registration

## Implementation Plan
1. **Install EF Core NuGet Packages**: Add `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design` to Data project
2. **Create PatientAccessDbContext Class**: Implement `DbContext` with constructor accepting `DbContextOptions<PatientAccessDbContext>`
3. **Configure Connection String Resolution**: Read connection string from configuration in `Program.cs` with fallback to environment variable
4. **Register DbContext in DI Container**: Add `AddDbContext<PatientAccessDbContext>` with Npgsql provider and connection string
5. **Implement OnModelCreating**: Override `OnModelCreating` method for future entity configurations
6. **Create Initial Migration**: Generate initial migration with `dotnet ef migrations add InitialCreate`
7. **Implement Database Health Check**: Add startup logic to verify database connectivity and log detailed error if connection fails
8. **Document Migration Commands**: Update README with migration workflow (add, apply, rollback)

## Current Project State
```
Propel-Project-Team-12/
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   │   ├── Program.cs
│   │   └── appsettings.json (with connection string placeholder)
│   ├── PatientAccess.Business/
│   └── PatientAccess.Data/
│       ├── PatientAccess.Data.csproj
│       ├── Models/
│       └── Repositories/
└── .env (with database connection string)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Data/PatientAccess.Data.csproj | Add EF Core and Npgsql NuGet packages |
| CREATE | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | EF Core DbContext class with constructor and OnModelCreating override |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register DbContext in DI with Npgsql provider and connection string |
| CREATE | src/backend/PatientAccess.Web/Extensions/ServiceCollectionExtensions.cs | Extension method AddDataAccessServices for DI registration |
| CREATE | docs/MIGRATIONS.md | Migration workflow documentation (add, apply, rollback, list) |

## External References
- EF Core DbContext Configuration: https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
- Npgsql EF Core Provider: https://www.npgsql.org/efcore/
- EF Core Migrations Overview: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
- Dependency Injection with DbContext: https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-dbcontext-with-dependency-injection

## Build Commands
```bash
# Navigate to backend directory
cd src/backend

# Install EF Core CLI tools globally (one-time)
dotnet tool install --global dotnet-ef

# Add initial migration
dotnet ef migrations add InitialCreate --project PatientAccess.Data --startup-project PatientAccess.Web

# Apply migrations to database
dotnet ef database update --project PatientAccess.Data --startup-project PatientAccess.Web

# List all migrations
dotnet ef migrations list --project PatientAccess.Data --startup-project PatientAccess.Web

# Remove last migration (if not applied)
dotnet ef migrations remove --project PatientAccess.Data --startup-project PatientAccess.Web
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for scaffolding task)
- [ ] Integration tests pass (N/A for scaffolding task)
- [ ] `dotnet build` compiles successfully after adding EF Core packages
- [ ] `dotnet ef migrations add InitialCreate` generates migration without errors
- [ ] `dotnet ef database update` applies migration to Supabase database successfully
- [ ] Application startup succeeds and DbContext resolves from DI container
- [ ] Application logs detailed error and fails to start when database connection string is invalid
- [ ] DbContext uses Scoped lifetime (verified via DI container inspection)

## Implementation Checklist
- [x] Install Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Tools, Microsoft.EntityFrameworkCore.Design
- [x] Create `PatientAccessDbContext.cs` inheriting from `DbContext` with constructor
- [x] Override `OnModelCreating` method for future entity configurations
- [x] Register DbContext in `Program.cs` with Npgsql provider and connection string from configuration
- [x] Create `ServiceCollectionExtensions.cs` with `AddDataAccessServices` method for clean DI registration
- [x] Generate initial migration with `dotnet ef migrations add InitialCreate`
- [ ] Apply migration to Supabase database with `dotnet ef database update` (Requires debugging connection string)
- [x] Implement database connectivity health check in Program.cs with fail-fast behavior
