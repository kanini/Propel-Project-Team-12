# Task - task_001_backend_dotnet_scaffolding

## Requirement Reference
- User Story: us_002
- Story Location: .propel/context/tasks/EP-TECH/us_002/us_002.md
- Acceptance Criteria:
    - AC-1: Solution file `PatientAccess.sln` exists with three projects: `PatientAccess.Web`, `PatientAccess.Business`, and `PatientAccess.Data`
    - AC-2: Project references follow three-layer architecture (Web -> Business -> Data, no circular dependencies)
    - AC-3: Solution compiles without errors targeting .NET 8.0
    - AC-4: Dependency injection configured in `Program.cs` with proper interface-to-implementation mappings
    - AC-5: Folder structure includes Controllers, Services, Repositories, Models, DTOs, and Middleware in respective layers
- Edge Case:
    - .NET SDK version not 8.x should fail with global.json-enforced version requirement
    - Missing NuGet packages should trigger automatic `dotnet restore`

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
| Library | Entity Framework Core | 8.x |
| Library | ASP.NET Core Identity | 8.x |

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
Create a well-structured .NET 8 ASP.NET Core Web API solution following three-layer architecture (Presentation, Business Logic, Data Access). The solution establishes clean separation of concerns with the Web layer handling HTTP requests/responses, Business layer containing all business logic and validation, and Data layer managing database operations via Entity Framework Core. This architecture enables independent testing, maintainability, and adherence to SOLID principles.

## Dependent Tasks
- None

## Impacted Components
- **NEW** src/backend/PatientAccess.sln - Solution file containing all projects
- **NEW** src/backend/PatientAccess.Web/Program.cs - Application entry point with DI configuration
- **NEW** src/backend/PatientAccess.Business/ - Business logic layer
- **NEW** src/backend/PatientAccess.Data/ - Data access layer

## Implementation Plan
1. **Create Solution and Projects**: Use `dotnet new sln` to create solution, then add three class library projects (Web as Web API, Business and Data as Class Libraries)
2. **Configure Project References**: Set up proper dependency chain (Web references Business, Business references Data)
3. **Create global.json**: Enforce .NET 8.0 SDK version requirement to prevent compatibility issues
4. **Setup Folder Structure**: Create organized folder hierarchy in each project (Controllers/Services/Repositories/Models/DTOs/Middleware/Extensions)
5. **Configure Dependency Injection**: Implement DI container registration in `Program.cs` with service lifetime management (Scoped for EF DbContext, Transient for services)
6. **Add Base NuGet Packages**: Install essential packages (EF Core, Npgsql provider, ASP.NET Core Identity, Swagger/OpenAPI)
7. **Create Base Middleware**: Implement exception handling middleware and request logging middleware
8. **Verify Clean Build**: Ensure solution builds without warnings or errors targeting .NET 8.0

## Current Project State
```
Propel-Project-Team-12/
├── .propel/
├── .github/
├── src/frontend/ (from US_001)
└── (No backend code exists yet)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.sln | Solution file orchestrating all projects |
| CREATE | src/backend/global.json | .NET SDK version lock file specifying 8.0.x |
| CREATE | src/backend/PatientAccess.Web/PatientAccess.Web.csproj | Web API project file with package references |
| CREATE | src/backend/PatientAccess.Web/Program.cs | Application entry point with DI, middleware pipeline, and Swagger setup |
| CREATE | src/backend/PatientAccess.Web/appsettings.json | Configuration for connection strings, JWT settings, CORS origins |
| CREATE | src/backend/PatientAccess.Web/appsettings.Development.json | Development-specific settings (verbose logging, dev database) |
| CREATE | src/backend/PatientAccess.Web/Controllers/.gitkeep | Placeholder for controller classes |
| CREATE | src/backend/PatientAccess.Web/Middleware/ExceptionHandlingMiddleware.cs | Global exception handler returning consistent error responses |
| CREATE | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Business logic layer project file |
| CREATE | src/backend/PatientAccess.Business/Services/.gitkeep | Placeholder for service classes |
| CREATE | src/backend/PatientAccess.Business/DTOs/.gitkeep | Placeholder for data transfer objects |
| CREATE | src/backend/PatientAccess.Data/PatientAccess.Data.csproj | Data access layer project file with EF Core dependencies |
| CREATE | src/backend/PatientAccess.Data/Repositories/.gitkeep | Placeholder for repository classes |
| CREATE | src/backend/PatientAccess.Data/Models/.gitkeep | Placeholder for entity models |
| CREATE | src/backend/.gitignore | Backend-specific gitignore (bin/, obj/, appsettings.*.json) |

## External References
- .NET 8 Web API Tutorial: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-8.0
- Three-Layer Architecture Pattern: https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures
- Dependency Injection in ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-8.0
- Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
- SOLID Principles in C#: https://www.pluralsight.com/guides/solid-principles-in-csharp

## Build Commands
```bash
# Navigate to backend directory
cd src/backend

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run Web API project
dotnet run --project PatientAccess.Web

# Clean build artifacts
dotnet clean
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for scaffolding task)
- [ ] Integration tests pass (N/A for scaffolding task)
- [ ] Solution builds successfully with `dotnet build` targeting .NET 8.0
- [ ] No circular project references exist
- [ ] `dotnet run` starts Web API successfully and listens on configured port
- [ ] Swagger UI accessible at `/swagger` endpoint
- [ ] Exception handling middleware catches and formats errors consistently
- [ ] Build fails with .NET SDK version below 8.0 due to global.json constraint

## Implementation Checklist
- [X] Create solution file with `dotnet new sln -n PatientAccess`
- [X] Create three projects (Web, Business, Data) with appropriate templates
- [X] Add project references following layered architecture pattern
- [X] Create `global.json` enforcing .NET 8.0 SDK requirement
- [X] Setup folder structure in each project (Controllers/Services/Repositories/Models/DTOs/Middleware)
- [X] Install NuGet packages (EF Core, Npgsql, Identity, Swagger)
- [X] Configure DI container in `Program.cs` with service registrations
- [X] Implement global exception handling middleware
