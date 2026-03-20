# Task - task_002_health_check_endpoints

## Requirement Reference
- User Story: us_005
- Story Location: .propel/context/tasks/EP-TECH/us_005/us_005.md
- Acceptance Criteria:
    - AC-3: Health check endpoint at `GET /health` returns 200 OK with JSON payload showing database, Redis connectivity, and overall status
    - AC-4: When database is unreachable, health endpoint returns 503 Service Unavailable identifying the unhealthy dependency
- Edge Case:
    - Each dependency check should have 5-second timeout to prevent hanging

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
| Library | Microsoft.Extensions.Diagnostics.HealthChecks | 8.x |
| Library | AspNetCore.HealthChecks.Npgsql | 8.x |
| Library | AspNetCore.HealthChecks.Redis | 8.x |

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
Implement comprehensive health check endpoints for monitoring service availability and dependency status. Configure health checks for PostgreSQL database connectivity, Redis cache connectivity, and overall application status. Health checks enable load balancers and monitoring systems to detect unhealthy instances and route traffic accordingly, supporting the 99.9% uptime requirement (NFR-008).

## Dependent Tasks
- task_002_efcore_dbcontext_configuration (US_003) - Requires database connectivity
- (Optional) Future: Session caching Redis setup (US_006)

## Impacted Components
- **MODIFY** src/backend/PatientAccess.Web/PatientAccess.Web.csproj - Add health check NuGet packages
- **MODIFY** src/backend/PatientAccess.Web/Program.cs - Register health check services and endpoints
- **NEW** src/backend/PatientAccess.Web/HealthChecks/DatabaseHealthCheck.cs - Custom database health check with timeout
- **NEW** src/backend/PatientAccess.Web/HealthChecks/RedisHealthCheck.cs - Custom Redis health check with timeout (if Redis configured)

## Implementation Plan
1. **Install Health Check Packages**: Add `AspNetCore.HealthChecks.Npgsql` and `AspNetCore.HealthChecks.Redis` NuGet packages
2. **Register Health Check Services**: Configure health checks in Program.cs for database and Redis dependencies
3. **Configure Timeout Policies**: Set 5-second timeout for each dependency check to prevent hanging
4. **Create Database Health Check**: Implement custom health check executing simple query (`SELECT 1`) with timeout
5. **Create Redis Health Check**: Implement health check with PING command to verify Redis connectivity (conditional on Redis setup)
6. **Map Health Check Endpoints**: Add `/health` endpoint returning JSON with detailed status
7. **Configure Response Writer**: Implement custom response writer formatting health check results as structured JSON
8. **Test Failure Scenarios**: Verify 503 status when dependencies are unreachable

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
│       └── PatientAccessDbContext.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Web/PatientAccess.Web.csproj | Add health check NuGet packages (Npgsql, Redis) |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register health checks and map /health endpoint |
| CREATE | src/backend/PatientAccess.Web/HealthChecks/DatabaseHealthCheck.cs | Custom database connectivity check with 5s timeout |
| CREATE | src/backend/PatientAccess.Web/HealthChecks/CustomHealthCheckResponseWriter.cs | JSON response formatter for health check results |
| CREATE | docs/MONITORING.md | Health check endpoints documentation and integration guide |

## External References
- ASP.NET Core Health Checks: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0
- AspNetCore.HealthChecks.Npgsql: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
- Health Check Response Format: https://tools.ietf.org/html/rfc7807
- Load Balancer Health Checks: https://learn.microsoft.com/en-us/azure/architecture/patterns/health-endpoint-monitoring

## Build Commands
```bash
# Install health check packages
cd src/backend/PatientAccess.Web
dotnet add package AspNetCore.HealthChecks.Npgsql
dotnet add package AspNetCore.HealthChecks.Redis

# Run application and test health check
dotnet run
curl https://localhost:5001/health
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for health check configuration)
- [ ] Integration tests pass (create integration test for health endpoint)
- [ ] `GET /health` returns 200 OK when all dependencies healthy
- [ ] Response JSON contains database status with `Healthy` state
- [ ] `GET /health` returns 503 when database connection string is invalid
- [ ] Response JSON identifies specific unhealthy dependency
- [ ] Health check completes within 6 seconds (5s timeout + overhead)
- [ ] Load balancer test: unhealthy instances removed from rotation

## Implementation Checklist
- [ ] Install AspNetCore.HealthChecks.Npgsql and AspNetCore.HealthChecks.Redis packages
- [ ] Register health check services in Program.cs with AddHealthChecks()
- [ ] Add database health check with 5-second timeout configuration
- [ ] Add Redis health check with 5-second timeout (conditional on US_006 completion)
- [ ] Map `/health` endpoint with MapHealthChecks()
- [ ] Create `CustomHealthCheckResponseWriter` formatting results as JSON
- [ ] Configure response writer to include dependency details and status codes
- [ ] Test health endpoint with healthy and unhealthy database scenarios
