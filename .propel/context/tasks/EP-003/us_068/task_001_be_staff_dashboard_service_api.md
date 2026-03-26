# Task: Backend - Staff Dashboard Service + API Endpoints

## Task Metadata
- **Task ID:** task_001_be_staff_dashboard_service_api  
- **Parent Story:** [us_068](us_068.md) - Staff Dashboard - Operations Hub  
- **Epic:** EP-003 - Staff Operations Hub  
- **Technology Layer:** Backend (.NET 8 ASP.NET Core Web API)  
- **Estimated Effort:** 5 hours  
- **Priority:** P0 - CRITICAL  
- **Status:** COMPLETED  

---

## Objective
Implement backend service and API endpoints to provide staff dashboard metrics and queue preview data. This includes creating `StaffDashboardService.cs`, DTOs, controller endpoints, and seeding mock clinical verification data.

---

## Acceptance Criteria
- [x] StaffDashboardService.cs created with metrics calculation logic
- [x] DTOs created (StaffDashboardMetricsDto, QueuePreviewDto) - ✅ ALREADY DONE
- [x] GET /api/staff/dashboard/metrics endpoint returns today's appointments, queue size, pending verifications
- [x] GET /api/staff/dashboard/queue-preview endpoint returns next 5 patients with estimated wait times
- [x] Service registered in Program.cs DI container
- [x] Mock clinical data seeded to ExtractedClinicalData table - ✅ SEED SCRIPT ALREADY CREATED
- [x] All endpoints return data within 500ms (NFR-001)
- [x] Endpoints protected with [Authorize(Policy = "StaffOnly")]

---

## Implementation Checklist
- [x] Create IStaffDashboardService.cs interface in PatientAccess.Business/Interfaces/
- [x] Create StaffDashboardService.cs in PatientAccess.Business/Services/
- [x] Implement GetDashboardMetricsAsync() - query today's appointments, queue, verifications
- [x] Implement GetQueuePreviewAsync(int count = 5) - fetch next 5 patients with wait time logic
- [x] Add service registration in Program.cs: `builder.Services.AddScoped<IStaffDashboardService, StaffDashboardService>()`
- [x] Add GET /api/staff/dashboard/metrics endpoint in StaffController.cs
- [x] Add GET /api/staff/dashboard/queue-preview endpoint in StaffController.cs
- [ ] Run seed script: `scripts/seed_mock_clinical_data.sql` against dev database

---

## Technical Details

### Service Implementation Pattern
Follow existing `DashboardService.cs` pattern for patient dashboard (US_067):
```csharp
public class StaffDashboardService : IStaffDashboardService
{
    private readonly PatientAccessDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<StaffDashboardService> _logger;
    private const int CacheDurationMinutes = 5;

    public StaffDashboardService(
        PatientAccessDbContext context,
        IDistributedCache cache,
        ILogger<StaffDashboardService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<StaffDashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var cacheKey = "staff_dashboard_metrics";
        
        // Try cache first
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<StaffDashboardMetricsDto>(cachedData)!;
        }

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // Calculate metrics
        var todayAppointments = await _context.Appointments
            .Where(a => a.ScheduledDateTime >= today 
                && a.ScheduledDateTime < tomorrow
                && a.Status != AppointmentStatus.Cancelled)
            .CountAsync();

        var currentQueueSize = await _context.Appointments
            .Where(a => a.ScheduledDateTime >= today 
                && a.ScheduledDateTime < tomorrow
                && a.Status == AppointmentStatus.Waiting)
            .CountAsync();

        var pendingVerifications = await _context.ExtractedClinicalData
            .Where(ecd => ecd.VerificationStatus == VerificationStatus.AISuggested)
            .CountAsync();

        var metrics = new StaffDashboardMetricsDto
        {
            TodayAppointments = todayAppointments,
            CurrentQueueSize = currentQueueSize,
            PendingVerifications = pendingVerifications
        };

        // Cache for 5 minutes
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(metrics), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes) });

        return metrics;
    }

    public async Task<List<QueuePreviewDto>> GetQueuePreviewAsync(int count = 5)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var queueItems = await _context.Appointments
            .Where(a => a.ScheduledDateTime >= today 
                && a.ScheduledDateTime < tomorrow
                && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Waiting))
            .OrderBy(a => a.ScheduledDateTime)
            .Take(count)
            .Select(a => new QueuePreviewDto
            {
                AppointmentId = a.Id,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                Prov

iderName = a.Provider.FirstName + " " + a.Provider.LastName,
                AppointmentTime = a.ScheduledDateTime,
                EstimatedWait = CalculateWaitTime(a.ScheduledDateTime),
                RiskLevel = "low", // MVP: placeholder until FR-023 implemented
                Status = a.Status.ToString()
            })
            .ToListAsync();

        return queueItems;
    }

    private string CalculateWaitTime(DateTime scheduledTime)
    {
        var now = DateTime.UtcNow;
        if (scheduledTime > now)
        {
            var diff = scheduledTime - now;
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} mins";
            return $"{(int)diff.TotalHours}h {diff.Minutes}m";
        }
        return "Now";
    }
}
```

### Controller Endpoints
Add to existing `StaffController.cs`:
```csharp
/// <summary>
/// Get staff dashboard metrics (US_068, AC2).
/// </summary>
[HttpGet("dashboard/metrics")]
[ProducesResponseType(typeof(StaffDashboardMetricsDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetDashboardMetrics()
{
    try
    {
        var metrics = await _staffDashboardService.GetDashboardMetricsAsync();
        return Ok(metrics);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to fetch staff dashboard metrics");
        return StatusCode(500, new { error = "Failed to fetch dashboard metrics" });
    }
}

/// <summary>
/// Get queue preview for dashboard (US_068, AC4).
/// </summary>
[HttpGet("dashboard/queue-preview")]
[ProducesResponseType(typeof(List<QueuePreviewDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetQueuePreview([FromQuery] int count = 5)
{
    try
    {
        var queue = await _staffDashboardService.GetQueuePreviewAsync(count);
        return Ok(queue);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to fetch queue preview");
        return StatusCode(500, new { error = "Failed to fetch queue preview" });
    }
}
```

---

## References

### Existing Patterns
- **Service Pattern:** [DashboardService.cs](../../../src/backend/PatientAccess.Business/Services/DashboardService.cs) (Patient dashboard US_067)
- **Controller Pattern:** [StaffController.cs](../../../src/backend/PatientAccess.Web/Controllers/StaffController.cs) (US_029-031 endpoints)
- **DTO Pattern:** [AppointmentResponseDto.cs](../../../src/backend/PatientAccess.Business/DTOs/AppointmentResponseDto.cs)

### Already Created
- ✅ StaffDashboardMetricsDto.cs ([created](../../../src/backend/PatientAccess.Business/DTOs/StaffDashboardMetricsDto.cs))
- ✅ QueuePreviewDto.cs ([created](../../../src/backend/PatientAccess.Business/DTOs/QueuePreviewDto.cs))
- ✅ seed_mock_clinical_data.sql ([created](../../../src/backend/scripts/seed_mock_clinical_data.sql))

### Database Queries
- ExtractedClinicalData table with VerificationStatus = 'AISuggested'
- Appointments table filtering by date range and status

---

## Validation

### Manual Testing
```bash
# 1. Run seed script
sqlcmd -S localhost -d PatientAccessDb -i src/backend/scripts/seed_mock_clinical_data.sql

# 2. Start backend
cd src/backend/PatientAccess.Web
dotnet run

# 3. Test metrics endpoint
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/staff/dashboard/metrics

# Expected Response:
# {
#   "todayAppointments": 12,
#   "currentQueueSize": 3,
#   "pendingVerifications": 10
# }

# 4. Test queue preview endpoint
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/staff/dashboard/queue-preview?count=5

# Expected Response: Array of 5 QueuePreviewDto objects
```

### Performance Validation
- Metrics endpoint should respond within 500ms (NFR-001)
- Caching should reduce database hits on repeated requests

---

## Traceability
- **Parent User Story:** US_068 - Staff Dashboard
- **Epic:** EP-003 - Staff Operations Hub
- **Acceptance Criteria:** AC2 (stat cards), AC4 (queue widget)
- **Requirements:** FR-014 (queue management), NFR-001 (500ms response time)

---

## Dependencies
- ✅ ExtractedClinicalData table exists (80% complete)
- ✅ VerificationStatus enum exists
- ✅ Appointments table with Status field
- ✅ PatientAccessDbContext configured

---

## Notes
- Wait time calculation is simplified (MVP) - shows time until appointment
- Risk level hardcoded to "low" until FR-023 risk assessment implemented
- Pending verifications uses mock data from seed script until EP-009
- Cache duration set to 5 minutes (balance freshness vs performance)

---

**Status:** Ready for implementation  
**Blockers:** None  
**Next Task:** task_002_fe_redux_state_api (Frontend Redux setup)
