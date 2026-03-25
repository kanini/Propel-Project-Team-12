# Task - task_003_be_audit_query_api

## Requirement Reference
- User Story: US_055
- Story Location: .propel/context/tasks/EP-010/us_055/us_055.md
- Acceptance Criteria:
    - **AC3**: Given a compliance review is needed, When I query the audit log, Then I can filter by date range, user, action type, and resource with paginated results returning within 2 seconds (NFR-007).

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

Create Audit Query API with filtering, pagination, and performance optimization returning results within 2 seconds (AC3, NFR-007). This task implements GET /api/audit-logs endpoint with query parameters for filtering (date range, user ID, action type, resource type, resource ID), pagination support (page number, page size with default 50, max 200), query performance optimization using composite indexes from task_001, Redis caching for repeated queries (15-minute TTL), role-based authorization (Admin-only access), and CSV export functionality for compliance reports. Features query builder pattern for dynamic filtering, LINQ optimization with AsNoTracking(), response compression, and Application Insights tracking for query performance monitoring (NFR-007).

**Key Capabilities:**
- GET /api/audit-logs endpoint with filtering (AC3)
- Query parameters: fromDate, toDate, userId, actionType, resourceType, resourceId, page, pageSize
- AuditLogFilterDto for request validation
- Pagination wrapper: PagedResult<AuditLogDto>
- Query performance < 2 seconds (NFR-007)
- Redis caching for repeated queries (15-min TTL)
- GET /api/audit-logs/export endpoint for CSV download
- Admin-only authorization (NFR-014: minimum necessary access)
- Application Insights custom metrics for query performance
- LINQ optimization with AsNoTracking()

## Dependent Tasks
- EP-010: US_055: task_001_db_audit_log_schema (AuditLog entity, composite indexes)
- EP-010: US_055: task_002_be_audit_logging_service (IAuditLoggingService)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Web/Controllers/AuditLogsController.cs` - Audit query API
- **NEW**: `src/backend/PatientAccess.Business/DTOs/AuditLogDto.cs` - Response DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/AuditLogFilterDto.cs` - Filter DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/PagedResult.cs` - Pagination wrapper
- **NEW**: `src/backend/PatientAccess.Business/Services/AuditQueryService.cs` - Query service
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IAuditQueryService.cs` - Service interface
- **NEW**: `src/backend/PatientAccess.Business/Services/CsvExportService.cs` - CSV export service
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register AuditQueryService

## Implementation Plan

1. **Create DTOs**
   - File: `src/backend/PatientAccess.Business/DTOs/AuditLogDto.cs`
   - Response DTO:
     ```csharp
     namespace PatientAccess.Business.DTOs
     {
         public sealed class AuditLogDto
         {
             public long Id { get; init; }
             public int? UserId { get; init; }
             public string? UserEmail { get; init; }
             public required string ActionType { get; init; }
             public required string ResourceType { get; init; }
             public string? ResourceId { get; init; }
             public DateTime Timestamp { get; init; }
             public required string IpAddress { get; init; }
             public required string Result { get; init; }
             public string? ActionDetails { get; init; }
             public string? UserAgent { get; init; }
             public string? SessionId { get; init; }
         }
     }
     ```

2. **Create Filter DTO**
   - File: `src/backend/PatientAccess.Business/DTOs/AuditLogFilterDto.cs`
   - Filter parameters (AC3):
     ```csharp
     namespace PatientAccess.Business.DTOs
     {
         public sealed class AuditLogFilterDto
         {
             /// <summary>
             /// Filter by start date (UTC, AC3).
             /// </summary>
             public DateTime? FromDate { get; init; }
             
             /// <summary>
             /// Filter by end date (UTC, AC3).
             /// </summary>
             public DateTime? ToDate { get; init; }
             
             /// <summary>
             /// Filter by user ID (AC3).
             /// </summary>
             public int? UserId { get; init; }
             
             /// <summary>
             /// Filter by action type: Login, Logout, Create, Read, Update, Delete, Export, Print (AC3).
             /// </summary>
             public string? ActionType { get; init; }
             
             /// <summary>
             /// Filter by resource type: User, Patient, Appointment, ClinicalDocument, ExtractedClinicalData (AC3).
             /// </summary>
             public string? ResourceType { get; init; }
             
             /// <summary>
             /// Filter by resource ID (AC3).
             /// </summary>
             public string? ResourceId { get; init; }
             
             /// <summary>
             /// Page number (1-indexed, default: 1).
             /// </summary>
             public int Page { get; init; } = 1;
             
             /// <summary>
             /// Page size (default: 50, max: 200).
             /// </summary>
             public int PageSize { get; init; } = 50;
         }
     }
     ```

3. **Create Pagination Wrapper**
   - File: `src/backend/PatientAccess.Business/DTOs/PagedResult.cs`
   - Generic pagination wrapper:
     ```csharp
     namespace PatientAccess.Business.DTOs
     {
         public sealed class PagedResult<T>
         {
             public required IEnumerable<T> Items { get; init; }
             public int TotalCount { get; init; }
             public int Page { get; init; }
             public int PageSize { get; init; }
             public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
             public bool HasPreviousPage => Page > 1;
             public bool HasNextPage => Page < TotalPages;
         }
     }
     ```

4. **Create IAuditQueryService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IAuditQueryService.cs`
   - Service interface:
     ```csharp
     namespace PatientAccess.Business.Interfaces
     {
         public interface IAuditQueryService
         {
             /// <summary>
             /// Queries audit logs with filtering and pagination (AC3, NFR-007: <2s).
             /// </summary>
             Task<PagedResult<AuditLogDto>> QueryAuditLogsAsync(
                 AuditLogFilterDto filter,
                 CancellationToken cancellationToken = default);
             
             /// <summary>
             /// Exports audit logs to CSV for compliance reports.
             /// </summary>
             Task<byte[]> ExportAuditLogsToCsvAsync(
                 AuditLogFilterDto filter,
                 CancellationToken cancellationToken = default);
         }
     }
     ```

5. **Create AuditQueryService**
   - File: `src/backend/PatientAccess.Business/Services/AuditQueryService.cs`
   - Query service with performance optimization:
     ```csharp
     using Microsoft.EntityFrameworkCore;
     using PatientAccess.Business.DTOs;
     using PatientAccess.Business.Interfaces;
     using PatientAccess.Data;
     using StackExchange.Redis;
     using System.Text.Json;
     
     namespace PatientAccess.Business.Services
     {
         public sealed class AuditQueryService : IAuditQueryService
         {
             private readonly AppDbContext _context;
             private readonly IDatabase _redisCache;
             private readonly ILogger<AuditQueryService> _logger;
             
             public AuditQueryService(
                 AppDbContext context,
                 IConnectionMultiplexer redis,
                 ILogger<AuditQueryService> logger)
             {
                 _context = context;
                 _redisCache = redis.GetDatabase();
                 _logger = logger;
             }
             
             public async Task<PagedResult<AuditLogDto>> QueryAuditLogsAsync(
                 AuditLogFilterDto filter,
                 CancellationToken cancellationToken = default)
             {
                 var startTime = DateTime.UtcNow;
                 
                 // Check Redis cache first
                 var cacheKey = GenerateCacheKey(filter);
                 var cachedData = await _redisCache.StringGetAsync(cacheKey);
                 
                 if (cachedData.HasValue)
                 {
                     _logger.LogInformation("Cache hit for audit query: {CacheKey}", cacheKey);
                     return JsonSerializer.Deserialize<PagedResult<AuditLogDto>>(cachedData!)!;
                 }
                 
                 // Validate page size
                 var pageSize = Math.Min(filter.PageSize, 200);
                 
                 // Build query with filters (uses composite indexes from task_001)
                 var query = _context.AuditLogs
                     .AsNoTracking() // Optimization: read-only query
                     .Where(a => !a.IsArchived); // Exclude archived logs
                 
                 // Apply filters (AC3)
                 if (filter.FromDate.HasValue)
                 {
                     query = query.Where(a => a.Timestamp >= filter.FromDate.Value);
                 }
                 
                 if (filter.ToDate.HasValue)
                 {
                     query = query.Where(a => a.Timestamp <= filter.ToDate.Value);
                 }
                 
                 if (filter.UserId.HasValue)
                 {
                     query = query.Where(a => a.UserId == filter.UserId.Value);
                 }
                 
                 if (!string.IsNullOrEmpty(filter.ActionType))
                 {
                     query = query.Where(a => a.ActionType == filter.ActionType);
                 }
                 
                 if (!string.IsNullOrEmpty(filter.ResourceType))
                 {
                     query = query.Where(a => a.ResourceType == filter.ResourceType);
                 }
                 
                 if (!string.IsNullOrEmpty(filter.ResourceId))
                 {
                     query = query.Where(a => a.ResourceId == filter.ResourceId);
                 }
                 
                 // Get total count
                 var totalCount = await query.CountAsync(cancellationToken);
                 
                 // Apply pagination and ordering (uses IX_AuditLogs_Timestamp_DESC index)
                 var auditLogs = await query
                     .OrderByDescending(a => a.Timestamp)
                     .Skip((filter.Page - 1) * pageSize)
                     .Take(pageSize)
                     .Select(a => new AuditLogDto
                     {
                         Id = a.Id,
                         UserId = a.UserId,
                         UserEmail = null, // Populate separately if needed
                         ActionType = a.ActionType,
                         ResourceType = a.ResourceType,
                         ResourceId = a.ResourceId,
                         Timestamp = a.Timestamp,
                         IpAddress = a.IpAddress,
                         Result = a.Result,
                         ActionDetails = a.ActionDetails,
                         UserAgent = a.UserAgent,
                         SessionId = a.SessionId
                     })
                     .ToListAsync(cancellationToken);
                 
                 var result = new PagedResult<AuditLogDto>
                 {
                     Items = auditLogs,
                     TotalCount = totalCount,
                     Page = filter.Page,
                     PageSize = pageSize
                 };
                 
                 // Cache for 15 minutes
                 await _redisCache.StringSetAsync(
                     cacheKey,
                     JsonSerializer.Serialize(result),
                     TimeSpan.FromMinutes(15));
                 
                 var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                 _logger.LogInformation(
                     "Audit query completed in {Duration}ms. Returned {Count} results.",
                     duration, auditLogs.Count);
                 
                 // Warn if query exceeds 2s threshold (NFR-007)
                 if (duration > 2000)
                 {
                     _logger.LogWarning(
                         "Audit query exceeded 2s threshold: {Duration}ms. Filters: {@Filter}",
                         duration, filter);
                 }
                 
                 return result;
             }
             
             public async Task<byte[]> ExportAuditLogsToCsvAsync(
                 AuditLogFilterDto filter,
                 CancellationToken cancellationToken = default)
             {
                 // Fetch all matching records (no pagination for export)
                 var query = _context.AuditLogs
                     .AsNoTracking()
                     .Where(a => !a.IsArchived);
                 
                 // Apply same filters as QueryAuditLogsAsync
                 if (filter.FromDate.HasValue)
                     query = query.Where(a => a.Timestamp >= filter.FromDate.Value);
                 if (filter.ToDate.HasValue)
                     query = query.Where(a => a.Timestamp <= filter.ToDate.Value);
                 if (filter.UserId.HasValue)
                     query = query.Where(a => a.UserId == filter.UserId.Value);
                 if (!string.IsNullOrEmpty(filter.ActionType))
                     query = query.Where(a => a.ActionType == filter.ActionType);
                 if (!string.IsNullOrEmpty(filter.ResourceType))
                     query = query.Where(a => a.ResourceType == filter.ResourceType);
                 if (!string.IsNullOrEmpty(filter.ResourceId))
                     query = query.Where(a => a.ResourceId == filter.ResourceId);
                 
                 var auditLogs = await query
                     .OrderByDescending(a => a.Timestamp)
                     .Take(10000) // Limit export to 10k records
                     .ToListAsync(cancellationToken);
                 
                 // Generate CSV
                 using var memoryStream = new MemoryStream();
                 using var writer = new StreamWriter(memoryStream);
                 
                 // Write CSV header
                 await writer.WriteLineAsync("Id,UserId,ActionType,ResourceType,ResourceId,Timestamp,IpAddress,Result,ActionDetails");
                 
                 // Write CSV rows
                 foreach (var log in auditLogs)
                 {
                     var line = $"{log.Id},{log.UserId}," +
                                $"\"{log.ActionType}\",\"{log.ResourceType}\",\"{log.ResourceId}\"," +
                                $"{log.Timestamp:yyyy-MM-dd HH:mm:ss},\"{log.IpAddress}\"," +
                                $"\"{log.Result}\",\"{log.ActionDetails?.Replace("\"", "\"\"")}\"";
                     await writer.WriteLineAsync(line);
                 }
                 
                 await writer.FlushAsync();
                 
                 _logger.LogInformation("Exported {Count} audit logs to CSV.", auditLogs.Count);
                 
                 return memoryStream.ToArray();
             }
             
             private static string GenerateCacheKey(AuditLogFilterDto filter)
             {
                 return $"audit:query:{filter.FromDate?.ToString("yyyyMMdd")}:" +
                        $"{filter.ToDate?.ToString("yyyyMMdd")}:" +
                        $"{filter.UserId}:{filter.ActionType}:{filter.ResourceType}:" +
                        $"{filter.ResourceId}:{filter.Page}:{filter.PageSize}";
             }
         }
     }
     ```

6. **Create AuditLogsController**
   - File: `src/backend/PatientAccess.Web/Controllers/AuditLogsController.cs`
   - REST API controller (Admin-only):
     ```csharp
     using Microsoft.AspNetCore.Authorization;
     using Microsoft.AspNetCore.Mvc;
     using PatientAccess.Business.DTOs;
     using PatientAccess.Business.Interfaces;
     
     namespace PatientAccess.Web.Controllers
     {
         [ApiController]
         [Route("api/[controller]")]
         [Authorize(Roles = "Admin")] // Admin-only access (NFR-014)
         public sealed class AuditLogsController : ControllerBase
         {
             private readonly IAuditQueryService _auditQueryService;
             private readonly ILogger<AuditLogsController> _logger;
             
             public AuditLogsController(
                 IAuditQueryService auditQueryService,
                 ILogger<AuditLogsController> logger)
             {
                 _auditQueryService = auditQueryService;
                 _logger = logger;
             }
             
             /// <summary>
             /// Queries audit logs with filtering and pagination (AC3, NFR-007).
             /// </summary>
             /// <param name="filter">Filter parameters.</param>
             /// <param name="cancellationToken">Cancellation token.</param>
             /// <returns>Paginated audit log results within 2 seconds (NFR-007).</returns>
             [HttpGet]
             [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AuditLogDto>))]
             [ProducesResponseType(StatusCodes.Status403Forbidden)]
             public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
                 [FromQuery] AuditLogFilterDto filter,
                 CancellationToken cancellationToken)
             {
                 var result = await _auditQueryService.QueryAuditLogsAsync(filter, cancellationToken);
                 
                 return Ok(result);
             }
             
             /// <summary>
             /// Exports audit logs to CSV for compliance reports.
             /// </summary>
             [HttpGet("export")]
             [ProducesResponseType(StatusCodes.Status200OK)]
             [ProducesResponseType(StatusCodes.Status403Forbidden)]
             public async Task<IActionResult> ExportAuditLogs(
                 [FromQuery] AuditLogFilterDto filter,
                 CancellationToken cancellationToken)
             {
                 _logger.LogInformation("Admin {UserId} requested audit log export.", User.Identity?.Name);
                 
                 var csvBytes = await _auditQueryService.ExportAuditLogsToCsvAsync(filter, cancellationToken);
                 
                 var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                 return File(csvBytes, "text/csv", fileName);
             }
             
             /// <summary>
             /// Gets audit log statistics (summary view).
             /// </summary>
             [HttpGet("stats")]
             [ProducesResponseType(StatusCodes.Status200OK)]
             public async Task<ActionResult<object>> GetAuditStats(
                 [FromQuery] DateTime? fromDate,
                 [FromQuery] DateTime? toDate,
                 CancellationToken cancellationToken)
             {
                 // Future enhancement: aggregate statistics
                 return Ok(new
                 {
                     Message = "Audit statistics endpoint placeholder."
                 });
             }
         }
     }
     ```

7. **Register AuditQueryService in Program.cs**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Service registration:
     ```csharp
     // Audit Query Service
     builder.Services.AddScoped<IAuditQueryService, AuditQueryService>();
     
     // Response compression for /api/audit-logs endpoint
     builder.Services.AddResponseCompression(options =>
     {
         options.EnableForHttps = true;
     });
     ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   └── AuditLoggingService.cs (from task_002)
│   ├── Interfaces/
│   │   └── IAuditLoggingService.cs (from task_002)
│   └── DTOs/
├── PatientAccess.Web/
│   └── Controllers/
└── PatientAccess.Data/
    └── Entities/
        └── AuditLog.cs (from task_001)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Controllers/AuditLogsController.cs | Audit query API |
| CREATE | src/backend/PatientAccess.Business/DTOs/AuditLogDto.cs | Response DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/AuditLogFilterDto.cs | Filter DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/PagedResult.cs | Pagination wrapper |
| CREATE | src/backend/PatientAccess.Business/Services/AuditQueryService.cs | Query service |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAuditQueryService.cs | Service interface |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register service |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Web API
- **Query Parameters**: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding#query-strings
- **Response Compression**: https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression

### Entity Framework Core
- **Query Performance**: https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying
- **AsNoTracking**: https://learn.microsoft.com/en-us/ef/core/querying/tracking#no-tracking-queries

### Design Requirements
- **FR-040**: Comprehensive audit logging (spec.md)
- **NFR-007**: Immutable audit logs with query performance < 2s (design.md)
- **NFR-014**: Minimum necessary access (design.md)

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

# Test endpoint
curl -X GET "https://localhost:5001/api/audit-logs?fromDate=2026-01-01&toDate=2026-03-23&page=1&pageSize=50" \
  -H "Authorization: Bearer <admin_token>"
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/AuditQueryServiceTests.cs`
- Test cases:
  1. **Test_QueryAuditLogs_ReturnsFilteredResults**
     - Setup: Create 100 audit logs with varying filters
     - Call: QueryAuditLogsAsync with filter (userId=1, actionType=Login)
     - Assert: Returns only matching audit logs
  2. **Test_QueryAuditLogs_ReturnsPaginatedResults**
     - Setup: Create 150 audit logs
     - Call: QueryAuditLogsAsync with page=2, pageSize=50
     - Assert: Returns items 51-100, TotalCount=150, TotalPages=3
  3. **Test_QueryAuditLogs_CompletesWithin2Seconds**
     - Setup: Database with 1 million audit logs
     - Call: QueryAuditLogsAsync with date range filter
     - Assert: Execution time < 2s (NFR-007)
  4. **Test_QueryAuditLogs_UsesCacheOnSecondCall**
     - Call: QueryAuditLogsAsync twice with same filter
     - Assert: Second call returns cached result
  5. **Test_ExportToCsv_GeneratesValidCsv**
     - Setup: Create 50 audit logs
     - Call: ExportAuditLogsToCsvAsync
     - Assert: CSV contains 51 lines (header + 50 rows)

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/AuditLogsControllerTests.cs`
- Test cases:
  1. **Test_GetAuditLogs_Returns200ForAdmin**
     - Request: GET /api/audit-logs (with Admin JWT)
     - Assert: StatusCode = 200, returns PagedResult
  2. **Test_GetAuditLogs_Returns403ForNonAdmin**
     - Request: GET /api/audit-logs (with Staff JWT)
     - Assert: StatusCode = 403 Forbidden
  3. **Test_ExportAuditLogs_ReturnsCSV**
     - Request: GET /api/audit-logs/export (with Admin JWT)
     - Assert: StatusCode = 200, Content-Type = text/csv

### Performance Tests
- File: `src/backend/PatientAccess.Tests/Performance/AuditQueryPerformanceTests.cs`
- Test cases:
  1. **Test_QueryWithDateRange_Under2Seconds**
     - Setup: 1 million audit logs
     - Query: Filter by date range (last 30 days)
     - Assert: Query time < 2s (NFR-007)
  2. **Test_QueryWithMultipleFilters_Under2Seconds**
     - Setup: 1 million audit logs
     - Query: Filter by date range + userId + actionType
     - Assert: Query time < 2s (uses composite indexes)

### Acceptance Criteria Validation
- **AC3**: ✅ Query with filters (date range, user, action type, resource) returns paginated results within 2 seconds

## Success Criteria Checklist
- [MANDATORY] GET /api/audit-logs endpoint with filtering (AC3)
- [MANDATORY] Query parameters: fromDate, toDate, userId, actionType, resourceType, resourceId
- [MANDATORY] Pagination support (page, pageSize with max 200)
- [MANDATORY] Query performance < 2 seconds (NFR-007)
- [MANDATORY] Redis caching for repeated queries (15-min TTL)
- [MANDATORY] PagedResult wrapper with metadata (TotalCount, TotalPages, HasNextPage)
- [MANDATORY] GET /api/audit-logs/export endpoint for CSV download
- [MANDATORY] Admin-only authorization (NFR-014)
- [MANDATORY] AsNoTracking optimization for read-only queries
- [MANDATORY] Composite indexes utilized (from task_001)
- [MANDATORY] Response compression enabled
- [MANDATORY] Application Insights tracking for query performance
- [MANDATORY] Unit test: Query returns filtered results
- [MANDATORY] Unit test: Pagination works correctly
- [MANDATORY] Performance test: Query completes within 2 seconds
- [RECOMMENDED] Real-time audit log streaming via SignalR
- [RECOMMENDED] Aggregate statistics endpoint (/api/audit-logs/stats)

## Estimated Effort
**4 hours** (Query service + API controller + filtering + pagination + caching + CSV export + tests)
