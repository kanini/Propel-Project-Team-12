# Task - task_002_be_hangfire_health_monitoring

## Requirement Reference
- User Story: US_043
- Story Location: .propel/context/tasks/EP-006-I/us_043/us_043.md
- Acceptance Criteria:
    - **AC1**: Given a document is uploaded, When the upload completes, Then a background job is enqueued in Hangfire for document processing with the document ID and patient context.
- Edge Case:
    - What happens when Hangfire is unavailable? Document upload succeeds but processing is delayed; a health check detects the issue and alerts operations.

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

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Hangfire | 1.8.x |
| Library | Microsoft.Extensions.Diagnostics.HealthChecks | 8.0 |
| AI/ML | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement comprehensive health checks and monitoring for the Hangfire background job infrastructure. This task ensures operational visibility into the document processing pipeline by exposing health check endpoints that verify Hangfire server availability, job queue depth, failed job detection, and processing backlog monitoring. When Hangfire becomes unavailable or processing stalls, automated health checks alert operations teams while allowing document uploads to continue (graceful degradation).

**Key Capabilities:**
- ASP.NET Core health check endpoint for Hangfire availability
- Job queue depth monitoring (alert if >50 pending jobs)
- Failed job detection (alert if >10 failed jobs)
- Processing backlog monitoring (documents in "Uploaded" status >5 minutes)
- Hangfire dashboard access with role-based authorization
- Structured logging for health check results
- Integration with monitoring platforms (Application Insights, Seq)

## Dependent Tasks
- task_001_be_hangfire_processing_pipeline (Hangfire infrastructure must be configured first)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Web/HealthChecks/HangfireHealthCheck.cs` - Custom health check implementation
- **NEW**: `src/backend/PatientAccess.Web/HealthChecks/DocumentProcessingHealthCheck.cs` - Processing backlog health check
- **NEW**: `src/backend/PatientAccess.Web/Authorization/HangfireDashboardAuthorizationFilter.cs` - Dashboard security
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register health checks and dashboard authorization
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add health check thresholds configuration

## Implementation Plan

1. **Create HangfireHealthCheck**
   - Implement `IHealthCheck` interface
   - Check Hangfire server availability using `BackgroundJob.Enqueue(() => Console.WriteLine("test"))`
   - Query Hangfire storage for server status (active servers count)
   - Check job queue depth from monitoring API
   - Return `HealthCheckResult.Healthy()` if operational
   - Return `HealthCheckResult.Degraded()` if queue depth >50
   - Return `HealthCheckResult.Unhealthy()` if Hangfire unavailable
   - Follow existing health check patterns in codebase

2. **Create DocumentProcessingHealthCheck**
   - Implement `IHealthCheck` interface
   - Inject `ApplicationDbContext`
   - Query ClinicalDocument table for documents in "Uploaded" status older than 5 minutes
   - Query for documents in "Failed" status (count)
   - Return `HealthCheckResult.Healthy()` if backlog <10 and failed <10
   - Return `HealthCheckResult.Degraded()` if backlog <50 or failed <20
   - Return `HealthCheckResult.Unhealthy()` if backlog >=50 or failed >=20
   - Include metadata: backlogCount, failedCount, oldestBacklogAge

3. **Implement HangfireDashboardAuthorizationFilter**
   - Implement `IDashboardAuthorizationFilter` interface
   - Authorize only Admin role users to access Hangfire dashboard
   - Check user authentication status and role claims
   - Return `true` if user is authenticated and has "Admin" role
   - Return `false` otherwise (blocks access)
   - Follow existing authorization patterns from API controllers

4. **Register Health Checks in Program.cs**
   - Add health check services: `builder.Services.AddHealthChecks()`
   - Register `HangfireHealthCheck` with name "hangfire"
   - Register `DocumentProcessingHealthCheck` with name "document-processing"
   - Add health check endpoint: `app.MapHealthChecks("/health")`
   - Add detailed health check endpoint: `app.MapHealthChecks("/health/ready")` with JSON response
   - Configure health check UI (optional) for dashboard visualization

5. **Configure Hangfire Dashboard Authorization**
   - Apply `HangfireDashboardAuthorizationFilter` to Hangfire dashboard
   - Restrict dashboard URL to internal/admin access only
   - Configure dashboard options (polling interval, history retention)
   - Add logging for dashboard access attempts

6. **Add Health Check Threshold Configuration**
   - Add settings to `appsettings.json`:
     - HealthChecks:Hangfire:MaxQueueDepth (default: 50)
     - HealthChecks:DocumentProcessing:MaxBacklogMinutes (default: 5)
     - HealthChecks:DocumentProcessing:MaxFailedJobs (default: 10)
   - Inject `IConfiguration` into health checks to read thresholds
   - Allow environment-specific overrides in `appsettings.Development.json`

7. **Implement Structured Logging**
   - Log health check execution results with structured data
   - Include metrics: queue depth, backlog count, failed job count
   - Log at Warning level for Degraded status
   - Log at Error level for Unhealthy status
   - Use correlation IDs for health check traces

8. **Add Integration with Monitoring Platforms**
   - Ensure health check results are captured by Application Insights
   - Configure custom metrics for queue depth and backlog count
   - Set up alerts in monitoring platform (queue depth >50, failed jobs >10)
   - Document alert response procedures in operational runbook

## Current Project State

```
src/backend/
├── PatientAccess.Web/
│   ├── Controllers/
│   │   ├── AppointmentsController.cs
│   │   └── DocumentsController.cs
│   ├── Authorization/ (if exists)
│   ├── HealthChecks/ (to be created)
│   ├── Program.cs
│   └── appsettings.json
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── DocumentProcessingService.cs (from task_001)
│   │   └── DocumentUploadService.cs
│   └── BackgroundJobs/
│       └── DocumentProcessingJob.cs (from task_001)
└── PatientAccess.Data/
    └── Entities/
        └── ClinicalDocument.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/HealthChecks/HangfireHealthCheck.cs | Health check for Hangfire server availability |
| CREATE | src/backend/PatientAccess.Web/HealthChecks/DocumentProcessingHealthCheck.cs | Health check for processing backlog |
| CREATE | src/backend/PatientAccess.Web/Authorization/HangfireDashboardAuthorizationFilter.cs | Dashboard role-based authorization |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register health checks and configure dashboard security |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add health check threshold configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Health Checks
- **Health Checks**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
- **Custom Health Checks**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks#create-health-checks
- **Health Check UI**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks#health-check-ui

### Hangfire Monitoring
- **Monitoring API**: https://docs.hangfire.io/en/latest/background-processing/tracking-progress.html
- **Dashboard Authorization**: https://docs.hangfire.io/en/latest/configuration/using-dashboard.html#configuring-authorization
- **Job Storage API**: https://docs.hangfire.io/en/latest/extensibility/using-job-storage.html

### Application Insights Integration
- **Custom Metrics**: https://learn.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics
- **Availability Tests**: https://learn.microsoft.com/en-us/azure/azure-monitor/app/monitor-web-app-availability

### Design Requirements
- **TR-018**: System MUST implement health check endpoints for monitoring and load balancer integration (design.md)
- **NFR-008**: System MUST achieve 99.9% uptime measured monthly, excluding scheduled maintenance windows (design.md)

### Existing Codebase Patterns
- **Authorization Pattern**: Existing `[Authorize]` attributes in controllers
- **Logging Pattern**: Existing `ILogger` usage in services

## Build Commands
```powershell
# Navigate to backend directory
cd src/backend

# Add health check NuGet package (if not already included)
dotnet add PatientAccess.Web package Microsoft.Extensions.Diagnostics.HealthChecks

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run

# Test health check endpoint
curl http://localhost:5000/health
curl http://localhost:5000/health/ready
```

## Implementation Validation Strategy
- [ ] Unit tests pass (HangfireHealthCheck, DocumentProcessingHealthCheck)
- [ ] Health check endpoints return correct status codes (200 Healthy, 503 Unhealthy)
- [ ] HangfireHealthCheck detects server unavailability correctly
- [ ] DocumentProcessingHealthCheck detects processing backlog correctly
- [ ] Failed job count threshold triggers Unhealthy status
- [ ] Hangfire dashboard accessible only to Admin role users
- [ ] Health check thresholds configurable via appsettings.json
- [ ] Structured logging captures health check results
- [ ] Integration with Application Insights works (custom metrics visible)
- [ ] Load balancer can use /health endpoint for availability checks

## Implementation Checklist
- [ ] Create HangfireHealthCheck implementing IHealthCheck with server availability check
- [ ] Create DocumentProcessingHealthCheck with backlog and failed job detection
- [ ] Implement HangfireDashboardAuthorizationFilter for role-based dashboard access
- [ ] Register health checks in Program.cs with /health and /health/ready endpoints
- [ ] Configure Hangfire dashboard with authorization filter
- [ ] Add health check threshold configuration to appsettings.json
- [ ] Implement structured logging for health check results
- [ ] Configure Application Insights integration for custom metrics
