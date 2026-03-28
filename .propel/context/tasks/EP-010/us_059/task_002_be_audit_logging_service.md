# Task - task_002_be_audit_logging_service

## Requirement Reference
- User Story: US_059
- Story Location: .propel/context/tasks/EP-010/US_059/US_059.md
- Acceptance Criteria:
    - **AC1**: Given any user performs an auditable action (FR-040), When the action completes, Then an audit record is created within 200ms containing actor ID, action type, target resource, timestamp (UTC), IP address, and result (success/failure).
    - **AC4**: Given high-volume operations, When the system processes bulk actions, Then audit writes are batched asynchronously via Hangfire without dropping any records, and a health check confirms zero audit loss.
- Edge Case:
    - What happens when the audit service is temporarily unavailable? Actions are queued in Upstash Redis and replayed in order once the service recovers; no user action is blocked.

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
| Frontend | N/A | N/A |
| Backend | ASP.NET Core | 8.0 |
| Backend | C# | 12.0 |
| Database | PostgreSQL | 16.x |
| Database | Entity Framework Core | 8.0 |
| Library | Hangfire | 1.8.x |
| Caching | Upstash Redis | Redis 7.x compatible |
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

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
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

Create Audit Logging Service with asynchronous batching via Hangfire for high-volume operations (AC4) and Redis queue for service unavailability resilience (edge case). This task implements IAuditLoggingService interface capturing actor ID, action type, target resource, timestamp (UTC), IP address, and result (AC1), AuditLoggingMiddleware for automatic HTTP request/response logging, asynchronous batch writing via Hangfire background jobs (AC4), Redis queue for audit entries when database unavailable (edge case), health check endpoint confirming zero audit loss (AC4), and Application Insights integration for audit activity monitoring. Features non-blocking audit writes (<200ms per AC1), zero-data-loss guarantee via Redis persistence, automatic replay on service recovery, and comprehensive audit coverage for all HIPAA-critical actions.

**Key Capabilities:**
- IAuditLoggingService.LogActionAsync(userId, actionType, resourceType, resourceId, ipAddress, result, actionDetails)
- AuditLoggingService with EF Core persistence
- AuditLoggingMiddleware for automatic HTTP logging
- Hangfire background job: ProcessAuditBatchJob (AC4)
- Redis queue for resilience (StackExchange.Redis, edge case)
- Health check: AuditLogHealthCheck confirming zero loss (AC4)
- Batch size: 100 audit entries per job
- IP address extraction from HttpContext
- User agent capture from request headers
- Session ID from JWT claims
- Application Insights custom metrics for audit volume

## Dependent Tasks
- EP-010: US_059: task_001_db_audit_log_schema (AuditLog entity, DbContext)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/AuditLoggingService.cs` - Audit service
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IAuditLoggingService.cs` - Service interface
- **NEW**: `src/backend/PatientAccess.Web/Middleware/AuditLoggingMiddleware.cs` - HTTP logging middleware
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/ProcessAuditBatchJob.cs` - Hangfire job
- **NEW**: `src/backend/PatientAccess.Web/HealthChecks/AuditLogHealthCheck.cs` - Health check
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register services, middleware, health checks

## Implementation Plan

1. **Create IAuditLoggingService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IAuditLoggingService.cs`
   - Service interface:
     ```csharp
     namespace PatientAccess.Business.Interfaces
     {
         public interface IAuditLoggingService
         {
             /// <summary>
             /// Logs an auditable action (AC1).
             /// </summary>
             /// <param name="userId">User ID performing the action (nullable for system actions).</param>
             /// <param name="actionType">Login, Logout, Create, Read, Update, Delete, Export, Print.</param>
             /// <param name="resourceType">User, Patient, Appointment, ClinicalDocument, ExtractedClinicalData.</param>
             /// <param name="resourceId">Identifier of the resource.</param>
             /// <param name="ipAddress">Client IP address.</param>
             /// <param name="result">Success, Failure, PartialSuccess.</param>
             /// <param name="actionDetails">Optional JSONB details.</param>
             /// <param name="userAgent">Optional user agent string.</param>
             /// <param name="sessionId">Optional session ID.</param>
             /// <param name="cancellationToken">Cancellation token.</param>
             /// <returns>Task completing within 200ms (AC1).</returns>
             Task LogActionAsync(
                 int? userId,
                 string actionType,
                 string resourceType,
                 string? resourceId,
                 string ipAddress,
                 string result,
                 string? actionDetails = null,
                 string? userAgent = null,
                 string? sessionId = null,
                 CancellationToken cancellationToken = default);
             
             /// <summary>
             /// Logs a batch of audit entries asynchronously (AC4).
             /// </summary>
             Task LogBatchAsync(
                 IEnumerable<AuditLog> auditLogs,
                 CancellationToken cancellationToken = default);
         }
     }
     ```

2. **Create AuditLoggingService**
   - File: `src/backend/PatientAccess.Business/Services/AuditLoggingService.cs`
   - Service implementation with Redis queue:
     ```csharp
     using Microsoft.EntityFrameworkCore;
     using PatientAccess.Business.Interfaces;
     using PatientAccess.Data;
     using PatientAccess.Data.Entities;
     using StackExchange.Redis;
     using System.Text.Json;
     
     namespace PatientAccess.Business.Services
     {
         public sealed class AuditLoggingService : IAuditLoggingService
         {
             private readonly AppDbContext _context;
             private readonly IDatabase _redisCache;
             private readonly ILogger<AuditLoggingService> _logger;
             private const string REDIS_AUDIT_QUEUE = "audit:queue";
             
             public AuditLoggingService(
                 AppDbContext context,
                 IConnectionMultiplexer redis,
                 ILogger<AuditLoggingService> logger)
             {
                 _context = context;
                 _redisCache = redis.GetDatabase();
                 _logger = logger;
             }
             
             public async Task LogActionAsync(
                 int? userId,
                 string actionType,
                 string resourceType,
                 string? resourceId,
                 string ipAddress,
                 string result,
                 string? actionDetails = null,
                 string? userAgent = null,
                 string? sessionId = null,
                 CancellationToken cancellationToken = default)
             {
                 var auditLog = new AuditLog
                 {
                     UserId = userId,
                     ActionType = actionType,
                     ResourceType = resourceType,
                     ResourceId = resourceId,
                     Timestamp = DateTime.UtcNow,
                     IpAddress = ipAddress,
                     Result = result,
                     ActionDetails = actionDetails,
                     UserAgent = userAgent,
                     SessionId = sessionId,
                     IsArchived = false
                 };
                 
                 try
                 {
                     // Attempt direct database write (AC1: <200ms)
                     _context.AuditLogs.Add(auditLog);
                     await _context.SaveChangesAsync(cancellationToken);
                     
                     _logger.LogInformation(
                         "Audit log created: {ActionType} on {ResourceType}/{ResourceId} by user {UserId}",
                         actionType, resourceType, resourceId, userId);
                 }
                 catch (Exception ex)
                 {
                     // Edge case: Database unavailable, queue in Redis
                     _logger.LogWarning(ex, 
                         "Database unavailable for audit logging. Queuing in Redis.");
                     
                     await QueueAuditLogAsync(auditLog);
                 }
             }
             
             public async Task LogBatchAsync(
                 IEnumerable<AuditLog> auditLogs,
                 CancellationToken cancellationToken = default)
             {
                 try
                 {
                     await _context.AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
                     await _context.SaveChangesAsync(cancellationToken);
                     
                     _logger.LogInformation(
                         "Batch of {Count} audit logs written to database.",
                         auditLogs.Count());
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, "Failed to write audit batch to database.");
                     
                     // Fallback: Queue individual entries in Redis
                     foreach (var auditLog in auditLogs)
                     {
                         await QueueAuditLogAsync(auditLog);
                     }
                 }
             }
             
             /// <summary>
             /// Queues audit log in Redis when database unavailable (edge case).
             /// </summary>
             private async Task QueueAuditLogAsync(AuditLog auditLog)
             {
                 var serialized = JsonSerializer.Serialize(auditLog);
                 await _redisCache.ListRightPushAsync(REDIS_AUDIT_QUEUE, serialized);
                 
                 _logger.LogInformation(
                     "Audit log queued in Redis: {ActionType} on {ResourceType}/{ResourceId}",
                     auditLog.ActionType, auditLog.ResourceType, auditLog.ResourceId);
             }
             
             /// <summary>
             /// Replays queued audit logs from Redis (edge case recovery).
             /// </summary>
             public async Task ReplayQueuedAuditLogsAsync(CancellationToken cancellationToken = default)
             {
                 var queueLength = await _redisCache.ListLengthAsync(REDIS_AUDIT_QUEUE);
                 
                 if (queueLength == 0)
                 {
                     _logger.LogInformation("No queued audit logs in Redis.");
                     return;
                 }
                 
                 _logger.LogInformation("Replaying {Count} queued audit logs from Redis.", queueLength);
                 
                 var auditLogs = new List<AuditLog>();
                 
                 for (long i = 0; i < queueLength; i++)
                 {
                     var serialized = await _redisCache.ListLeftPopAsync(REDIS_AUDIT_QUEUE);
                     if (serialized.HasValue)
                     {
                         var auditLog = JsonSerializer.Deserialize<AuditLog>(serialized!);
                         if (auditLog != null)
                         {
                             auditLogs.Add(auditLog);
                         }
                     }
                 }
                 
                 if (auditLogs.Count > 0)
                 {
                     await LogBatchAsync(auditLogs, cancellationToken);
                     _logger.LogInformation("Replayed {Count} audit logs from Redis.", auditLogs.Count);
                 }
             }
         }
     }
     ```

3. **Create AuditLoggingMiddleware**
   - File: `src/backend/PatientAccess.Web/Middleware/AuditLoggingMiddleware.cs`
   - HTTP request/response logging:
     ```csharp
     using PatientAccess.Business.Interfaces;
     using System.Security.Claims;
     
     namespace PatientAccess.Web.Middleware
     {
         public sealed class AuditLoggingMiddleware
         {
             private readonly RequestDelegate _next;
             private readonly ILogger<AuditLoggingMiddleware> _logger;
             
             public AuditLoggingMiddleware(
                 RequestDelegate next,
                 ILogger<AuditLoggingMiddleware> logger)
             {
                 _next = next;
                 _logger = logger;
             }
             
             public async Task InvokeAsync(
                 HttpContext context,
                 IAuditLoggingService auditService)
             {
                 var startTime = DateTime.UtcNow;
                 
                 // Capture request details
                 var userId = GetUserIdFromContext(context);
                 var ipAddress = GetIpAddress(context);
                 var userAgent = context.Request.Headers["User-Agent"].ToString();
                 var sessionId = context.User?.FindFirst("sid")?.Value;
                 
                 Exception? exception = null;
                 
                 try
                 {
                     await _next(context);
                 }
                 catch (Exception ex)
                 {
                     exception = ex;
                     throw;
                 }
                 finally
                 {
                     // Log after response
                     var actionType = DetermineActionType(context.Request.Method);
                     var resourceType = DetermineResourceType(context.Request.Path);
                     var resourceId = ExtractResourceId(context.Request.Path);
                     var result = exception != null ? "Failure" : 
                                  context.Response.StatusCode >= 400 ? "Failure" : "Success";
                     
                     var actionDetails = JsonSerializer.Serialize(new
                     {
                         Method = context.Request.Method,
                         Path = context.Request.Path.ToString(),
                         StatusCode = context.Response.StatusCode,
                         Duration = (DateTime.UtcNow - startTime).TotalMilliseconds,
                         ErrorMessage = exception?.Message
                     });
                     
                     // Non-blocking audit write
                     _ = auditService.LogActionAsync(
                         userId,
                         actionType,
                         resourceType,
                         resourceId,
                         ipAddress,
                         result,
                         actionDetails,
                         userAgent,
                         sessionId);
                 }
             }
             
             private static int? GetUserIdFromContext(HttpContext context)
             {
                 var userIdClaim = context.User?.FindFirst("sub")?.Value 
                                ?? context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                 
                 return int.TryParse(userIdClaim, out var userId) ? userId : null;
             }
             
             private static string GetIpAddress(HttpContext context)
             {
                 // Check for X-Forwarded-For header (behind proxy)
                 var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                 if (!string.IsNullOrEmpty(forwardedFor))
                 {
                     return forwardedFor.Split(',')[0].Trim();
                 }
                 
                 return context.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
             }
             
             private static string DetermineActionType(string httpMethod)
             {
                 return httpMethod switch
                 {
                     "GET" => "Read",
                     "POST" => "Create",
                     "PUT" => "Update",
                     "PATCH" => "Update",
                     "DELETE" => "Delete",
                     _ => "Unknown"
                 };
             }
             
             private static string DetermineResourceType(PathString path)
             {
                 // Extract resource type from path (e.g., /api/patients/123 -> Patient)
                 var segments = path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);
                 if (segments.Length >= 2 && segments[0] == "api")
                 {
                     return segments[1].TrimEnd('s'); // Remove plural 's'
                 }
                 return "Unknown";
             }
             
             private static string? ExtractResourceId(PathString path)
             {
                 var segments = path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);
                 // Assuming /api/resource/{id} pattern
                 if (segments.Length >= 3 && int.TryParse(segments[2], out _))
                 {
                     return segments[2];
                 }
                 return null;
             }
         }
     }
     ```

4. **Create ProcessAuditBatchJob**
   - File: `src/backend/PatientAccess.Business/BackgroundJobs/ProcessAuditBatchJob.cs`
   - Hangfire background job for batching (AC4):
     ```csharp
     using Hangfire;
     using PatientAccess.Business.Interfaces;
     
     namespace PatientAccess.Business.BackgroundJobs
     {
         public sealed class ProcessAuditBatchJob
         {
             private readonly IAuditLoggingService _auditService;
             private readonly ILogger<ProcessAuditBatchJob> _logger;
             
             public ProcessAuditBatchJob(
                 IAuditLoggingService auditService,
                 ILogger<ProcessAuditBatchJob> logger)
             {
                 _auditService = auditService;
                 _logger = logger;
             }
             
             /// <summary>
             /// Replays queued audit logs from Redis (AC4, edge case).
             /// Runs every 5 minutes to ensure zero audit loss.
             /// </summary>
             [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
             public async Task ReplayQueuedAudits()
             {
                 _logger.LogInformation("Starting audit log replay job...");
                 
                 try
                 {
                     await ((AuditLoggingService)_auditService).ReplayQueuedAuditLogsAsync();
                     _logger.LogInformation("Audit log replay job completed successfully.");
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, "Audit log replay job failed.");
                     throw;
                 }
             }
         }
     }
     ```

5. **Create AuditLogHealthCheck**
   - File: `src/backend/PatientAccess.Web/HealthChecks/AuditLogHealthCheck.cs`
   - Health check confirming zero audit loss (AC4):
     ```csharp
     using Microsoft.Extensions.Diagnostics.HealthChecks;
     using StackExchange.Redis;
     
     namespace PatientAccess.Web.HealthChecks
     {
         public sealed class AuditLogHealthCheck : IHealthCheck
         {
             private readonly IConnectionMultiplexer _redis;
             private const string REDIS_AUDIT_QUEUE = "audit:queue";
             
             public AuditLogHealthCheck(IConnectionMultiplexer redis)
             {
                 _redis = redis;
             }
             
             public async Task<HealthCheckResult> CheckHealthAsync(
                 HealthCheckContext context,
                 CancellationToken cancellationToken = default)
             {
                 try
                 {
                     var redisDb = _redis.GetDatabase();
                     var queueLength = await redisDb.ListLengthAsync(REDIS_AUDIT_QUEUE);
                     
                     // Healthy: Queue length < 100 (indicates timely processing)
                     // Degraded: Queue length 100-1000 (backlog building)
                     // Unhealthy: Queue length > 1000 (significant audit loss risk)
                     
                     if (queueLength == 0)
                     {
                         return HealthCheckResult.Healthy("No queued audit logs. All audits processed.");
                     }
                     else if (queueLength < 100)
                     {
                         return HealthCheckResult.Healthy($"Queue length: {queueLength}. Processing normally.");
                     }
                     else if (queueLength < 1000)
                     {
                         return HealthCheckResult.Degraded($"Queue length: {queueLength}. Backlog building.");
                     }
                     else
                     {
                         return HealthCheckResult.Unhealthy($"Queue length: {queueLength}. Significant audit loss risk!");
                     }
                 }
                 catch (Exception ex)
                 {
                     return HealthCheckResult.Unhealthy("Unable to check audit queue health.", ex);
                 }
             }
         }
     }
     ```

6. **Register Services in Program.cs**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Service registration and middleware setup:
     ```csharp
     // Audit Logging Service
     builder.Services.AddScoped<IAuditLoggingService, AuditLoggingService>();
     
     // Hangfire for background jobs
     builder.Services.AddHangfire(config => config
         .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
         .UseSimpleAssemblyNameTypeSerializer()
         .UseRecommendedSerializerSettings()
         .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
     
     builder.Services.AddHangfireServer();
     
     // Health Checks
     builder.Services.AddHealthChecks()
         .AddCheck<AuditLogHealthCheck>("audit_log_health");
     
     // Configure recurring jobs
     RecurringJob.AddOrUpdate<ProcessAuditBatchJob>(
         "replay-queued-audits",
         job => job.ReplayQueuedAudits(),
         "*/5 * * * *"); // Every 5 minutes
     
     // Middleware pipeline
     app.UseMiddleware<AuditLoggingMiddleware>();
     ```

7. **Create Helper Extension Methods**
   - File: `src/backend/PatientAccess.Business/Extensions/AuditLoggingExtensions.cs`
   - Convenience methods for common audit scenarios:
     ```csharp
     namespace PatientAccess.Business.Extensions
     {
         public static class AuditLoggingExtensions
         {
             public static async Task LogLoginAsync(
                 this IAuditLoggingService auditService,
                 int userId,
                 string ipAddress,
                 bool success,
                 string? userAgent = null,
                 string? sessionId = null)
             {
                 await auditService.LogActionAsync(
                     userId,
                     "Login",
                     "User",
                     userId.ToString(),
                     ipAddress,
                     success ? "Success" : "Failure",
                     userAgent: userAgent,
                     sessionId: sessionId);
             }
             
             public static async Task LogDataAccessAsync(
                 this IAuditLoggingService auditService,
                 int userId,
                 string resourceType,
                 string resourceId,
                 string ipAddress,
                 string? sessionId = null)
             {
                 await auditService.LogActionAsync(
                     userId,
                     "Read",
                     resourceType,
                     resourceId,
                     ipAddress,
                     "Success",
                     sessionId: sessionId);
             }
         }
     }
     ```

## Current Project State

```
src/backend/
â”œâ”€â”€ PatientAccess.Business/
â”‚   â”œâ”€â”€ Services/
â”‚   â”œâ”€â”€ Interfaces/
â”‚   â””â”€â”€ BackgroundJobs/
â”œâ”€â”€ PatientAccess.Web/
â”‚   â”œâ”€â”€ Middleware/
â”‚   â””â”€â”€ HealthChecks/
â””â”€â”€ PatientAccess.Data/
    â””â”€â”€ Entities/
        â””â”€â”€ AuditLog.cs (from task_001)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/AuditLoggingService.cs | Audit service |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAuditLoggingService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Web/Middleware/AuditLoggingMiddleware.cs | HTTP logging middleware |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/ProcessAuditBatchJob.cs | Hangfire job |
| CREATE | src/backend/PatientAccess.Web/HealthChecks/AuditLogHealthCheck.cs | Health check |
| CREATE | src/backend/PatientAccess.Business/Extensions/AuditLoggingExtensions.cs | Helper methods |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register services |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Middleware
- **Middleware Documentation**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/

### Hangfire
- **Background Jobs**: https://docs.hangfire.io/en/latest/background-methods/index.html
- **Recurring Jobs**: https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html

### StackExchange.Redis
- **Redis Lists**: https://stackexchange.github.io/StackExchange.Redis/Basics

### Health Checks
- **ASP.NET Core Health Checks**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks

### Design Requirements
- **FR-040**: Comprehensive audit logging (spec.md)
- **NFR-007**: Immutable audit logs (design.md)
- **AD-007**: Immutable audit trail architecture (design.md)

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build PatientAccess.sln

# Run tests
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj

# Run API locally
cd PatientAccess.Web
dotnet run
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/AuditLoggingServiceTests.cs`
- Test cases:
  1. **Test_LogAction_WritesToDatabase**
     - Call: LogActionAsync with valid parameters
     - Assert: AuditLog entry created in database
  2. **Test_LogAction_CompletesWithin200ms**
     - Measure: Execution time for LogActionAsync
     - Assert: Time < 200ms (AC1)
  3. **Test_LogAction_QueuesInRedisWhenDatabaseUnavailable**
     - Setup: Mock database failure
     - Call: LogActionAsync
     - Assert: Entry added to Redis queue (edge case)
  4. **Test_ReplayQueuedAudits_WritesAllEntries**
     - Setup: Add 50 audit logs to Redis queue
     - Call: ReplayQueuedAuditLogsAsync
     - Assert: All 50 entries written to database, queue empty
  5. **Test_LogBatch_WritesMultipleEntries**
     - Call: LogBatchAsync with 100 audit logs
     - Assert: All 100 entries written to database (AC4)

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/AuditMiddlewareTests.cs`
- Test cases:
  1. **Test_Middleware_LogsHttpRequest**
     - Request: GET /api/patients/123
     - Assert: AuditLog created with ActionType=Read, ResourceType=Patient, ResourceId=123
  2. **Test_HealthCheck_ReturnsHealthyWhenQueueEmpty**
     - Setup: Empty Redis queue
     - Request: GET /health
     - Assert: HealthCheckResult.Status = Healthy

### Acceptance Criteria Validation
- **AC1**: âœ… Audit record created within 200ms with all required fields
- **AC4**: âœ… Batch writes via Hangfire, health check confirms zero audit loss
- **Edge Case**: âœ… Redis queue used when database unavailable, replay on recovery

## Success Criteria Checklist
- [MANDATORY] IAuditLoggingService interface with LogActionAsync method (AC1)
- [MANDATORY] AuditLoggingService writes audit logs to database
- [MANDATORY] LogActionAsync completes within 200ms (AC1)
- [MANDATORY] AuditLoggingMiddleware captures HTTP requests automatically
- [MANDATORY] ProcessAuditBatchJob replays queued audits via Hangfire (AC4)
- [MANDATORY] Redis queue for audit entries when database unavailable (edge case)
- [MANDATORY] ReplayQueuedAuditLogsAsync writes all queued entries
- [MANDATORY] AuditLogHealthCheck confirms zero audit loss (AC4)
- [MANDATORY] IP address extraction from HttpContext
- [MANDATORY] User agent and session ID capture
- [MANDATORY] Non-blocking audit writes (fire-and-forget pattern)
- [MANDATORY] Application Insights custom metrics for audit volume
- [MANDATORY] Unit test: LogAction completes within 200ms
- [MANDATORY] Unit test: Redis queue used when database unavailable
- [MANDATORY] Integration test: Middleware logs HTTP requests
- [RECOMMENDED] Circuit breaker pattern for database resilience
- [RECOMMENDED] Audit log compression for JSONB ActionDetails

## Estimated Effort
**5 hours** (Service + middleware + Hangfire job + Redis queue + health check + tests)
