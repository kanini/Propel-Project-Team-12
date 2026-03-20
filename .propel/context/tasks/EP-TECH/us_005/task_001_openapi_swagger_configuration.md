# Task - task_001_openapi_swagger_configuration

## Requirement Reference
- User Story: us_005
- Story Location: .propel/context/tasks/EP-TECH/us_005/us_005.md
- Acceptance Criteria:
    - AC-1: Swagger UI at `/swagger` displays all API endpoints with request/response schemas, authentication requirements, and example payloads
    - AC-2: OpenAPI 3.0 specification available at `/swagger/v1/swagger.json` suitable for client code generation
- Edge Case:
    - Swagger UI should be disabled in production environments via configuration

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
| Library | Swashbuckle.AspNetCore | 6.x |

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
Configure OpenAPI 3.0 specification generation using Swashbuckle.AspNetCore with Swagger UI for interactive API documentation. Enable automatic schema generation from C# models, document authentication requirements with Bearer token support, provide example request/response payloads, and organize endpoints by controller tags. Implement environment-aware Swagger enablement to disable in production while allowing in development/staging.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project structure
- task_001_jwt_authentication_middleware (US_004) - For Bearer authentication documentation

## Impacted Components
- **MODIFY** src/backend/PatientAccess.Web/PatientAccess.Web.csproj - Add Swashbuckle.AspNetCore package
- **MODIFY** src/backend/PatientAccess.Web/Program.cs - Register Swagger services and middleware
- **NEW** src/backend/PatientAccess.Web/Configuration/SwaggerConfiguration.cs - Swagger options configuration
- **NEW** src/backend/PatientAccess.Web/Filters/SwaggerAuthorizationOperationFilter.cs - Filter to document authorization requirements

## Implementation Plan
1. **Install Swashbuckle.AspNetCore**: Add NuGet package for OpenAPI/Swagger support
2. **Configure Swagger Services**: Register Swagger generator with OpenAPI 3.0 specification in Program.cs
3. **Add API Metadata**: Configure Swagger document with title, version, description, and contact information
4. **Configure Bearer Authentication**: Add security definition for JWT Bearer tokens in Swagger UI
5. **Create Operation Filter**: Implement filter to automatically add `[Authorize]` attribute documentation to protected endpoints
6. **Enable Swagger UI**: Add Swagger middleware conditionally based on environment (disable in production)
7. **Customize Swagger UI**: Configure UI options (deep linking, try-it-out enabled, display operation IDs)
8. **Test API Documentation**: Verify all controllers/endpoints appear in Swagger UI with correct schemas

## Current Project State
```
Propel-Project-Team-12/
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   │   ├── Program.cs
│   │   ├── Controllers/
│   │   └── appsettings.json
│   ├── PatientAccess.Business/
│   └── PatientAccess.Data/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Web/PatientAccess.Web.csproj | Add Swashbuckle.AspNetCore NuGet package |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register Swagger services and middleware with environment check |
| CREATE | src/backend/PatientAccess.Web/Configuration/SwaggerConfiguration.cs | Static class with Swagger setup extensions |
| CREATE | src/backend/PatientAccess.Web/Filters/SwaggerAuthorizationOperationFilter.cs | Operation filter adding 401/403 responses to protected endpoints |
| CREATE | docs/API_DOCUMENTATION.md | Guide for API documentation standards and Swagger usage |

## External References
- Swashbuckle Documentation: https://github.com/domaindrivendev/Swashbuckle.AspNetCore
- OpenAPI Specification 3.0: https://swagger.io/specification/
- ASP.NET Core Web API Documentation: https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0
- JWT Bearer in Swagger: https://dev.to/eduardstefanescu/jwt-authorization-in-swagger-4e2h

## Build Commands
```bash
# Install Swashbuckle.AspNetCore
cd src/backend/PatientAccess.Web
dotnet add package Swashbuckle.AspNetCore

# Run API and access Swagger UI
dotnet run --project PatientAccess.Web
# Navigate to: https://localhost:5001/swagger
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for Swagger configuration)
- [ ] Integration tests pass (N/A for Swagger configuration)
- [ ] Swagger UI accessible at `/swagger` in development environment
- [ ] OpenAPI JSON specification accessible at `/swagger/v1/swagger.json`
- [ ] All controllers and endpoints appear in Swagger UI organized by tags
- [ ] Bearer authentication scheme visible in Swagger UI with "Authorize" button
- [ ] Protected endpoints show lock icon indicating authentication requirement
- [ ] Request/response schemas auto-generated from C# DTOs
- [ ] Swagger UI disabled in production environment (returns 404)

## Implementation Checklist
- [X] Install Swashbuckle.AspNetCore NuGet package (version 6.x)
- [X] Register Swagger generator in Program.cs with OpenAPI 3.0 configuration
- [X] Configure API metadata (title, version, description, contact)
- [X] Add JWT Bearer security definition to Swagger configuration
- [X] Create `SwaggerAuthorizationOperationFilter` to document protected endpoints
- [X] Enable Swagger UI middleware conditionally (only if not production)
- [X] Customize Swagger UI options (deep linking, try-it-out, display operation ID)
- [X] Test Swagger UI functionality and verify all endpoints documented correctly
