# Task - task_006_be_dashboard_api

## Task ID

* ID: task_006_be_dashboard_api

## Task Title

* Implement Dashboard Statistics and Appointments API Endpoints (Backend)

## Parent User Story

* US_067 - Patient Dashboard - Post-Login Landing Page

## Description

Create RESTful API endpoints for retrieving dashboard statistics (total appointments past 6 months, upcoming appointments next 30 days, waitlist entries count with trend calculations) and upcoming appointments list (next 5 chronologically) with proper authentication, authorization, caching, and error handling.

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

## Technology Layer

* Backend (.NET 8 ASP.NET Core Web API)

## Acceptance Criteria

1. **Given** an authenticated patient requests dashboard stats, **When** GET `/api/dashboard/stats` is called, **Then** the system returns statistics showing total appointments (past 6 months), upcoming appointments (next 30 days), and waitlist entries (current count) within 500ms (NFR-001).

2. **Given** the stats endpoint calculates trends, **When** data is retrieved, **Then** the system returns trend percentages comparing current period to previous period (e.g., current 6 months vs. previous 6 months).

3. **Given** an authenticated patient requests upcoming appointments, **When** GET `/api/appointments/upcoming?limit=5` is called, **Then** the system returns next 5 appointments in chronological order with provider name, date/time, specialty, and status.

4. **Given** a request is made without authentication token, **When** the endpoint is called, **Then** the system returns 401 Unauthorized with error message.

5. **Given** a patient requests dashboard stats, **When** the calculation completes, **Then** the system caches the result for 5 minutes using Redis to reduce database load.

6. **Given** database query fails, **When** error occurs, **Then** the system logs the error and returns 500 Internal Server Error with generic message (not exposing internal details).

7. **Given** a patient has no appointments, **When** requesting upcoming appointments, **Then** the system returns empty array with 200 OK status (not 404).

8. **Given** the endpoint executes database queries, **When** fetching data, **Then** the system uses optimized queries with proper indexes to ensure sub-500ms response time.

## Implementation Checklist

- [ ] Create DashboardController at `PatientAccess.Web/Controllers/DashboardController.cs` with GET /api/dashboard/stats endpoint requiring [Authorize(Roles = "Patient")]
- [ ] Implement IDashboardService interface and DashboardService at `PatientAccess.Business/Services/DashboardService.cs` with stats calculation logic
- [ ] Add GetDashboardStatsAsync method calculating: total appointments (past 6 months), upcoming appointments (next 30 days), waitlist entries (current)
- [ ] Implement trend calculation logic comparing current period counts to previous period counts returning percentage change
- [ ] Update AppointmentsController adding GET /api/appointments/upcoming endpoint with query parameter `limit` (default: 5, max: 20)
- [ ] Implement GetUpcomingAppointmentsAsync in AppointmentService with LINQ query filtering by PatientId, status IN ('Confirmed', 'Pending'), and dateTime >= now, ordered by dateTime ascending
- [ ] Add Redis caching decorator for dashboard stats with 5-minute TTL using IDistributedCache
- [ ] Implement comprehensive error handling with structured logging using ILogger and return appropriate HTTP status codes

## Estimated Effort

* 7 hours

## Dependencies

- Existing AppointmentService and database entities
- Redis/Upstash cache configuration in appsettings.json
- JWT authentication middleware

## Technical Context

### Architecture Patterns

* **Pattern**: Three-layer architecture (Controller → Service → Repository)
* **Caching Strategy**: Redis distributed cache with 5-minute TTL for stats
* **Authentication**: JWT Bearer token with role-based authorization
* **Error Handling**: Centralized exception handling middleware with structured logging

### Related Requirements

* FR-002: Secure session management with authentication
* NFR-001: API response time within 500ms at 95th percentile
* NFR-006: Role-based access control (RBAC) restricting to Patient role
* NFR-007: Immutable audit logs capturing PHI access

### Implementation References

**DashboardController:**
```csharp
// PatientAccess.Web/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.DTOs;

namespace PatientAccess.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics for authenticated patient
        /// </summary>
        /// <returns>Dashboard statistics with trends</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DashboardStatsDto>> GetStatsAsync()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value 
                    ?? throw new UnauthorizedAccessException("User ID not found in token");

                var stats = await _dashboardService.GetDashboardStatsAsync(userId);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to dashboard stats");
                return Unauthorized(new { error = "Authentication required" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats for user");
                return StatusCode(500, new { error = "Unable to retrieve dashboard statistics" });
            }
        }
    }
}
```

**DashboardService Implementation:**
```csharp
// PatientAccess.Business/Services/DashboardService.cs
using Microsoft.Extensions.Caching.Distributed;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.DTOs;
using System.Text.Json;

namespace PatientAccess.Business.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IWaitlistRepository _waitlistRepo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IAppointmentRepository appointmentRepo,
            IWaitlistRepository waitlistRepo,
            IDistributedCache cache,
            ILogger<DashboardService> logger)
        {
            _appointmentRepo = appointmentRepo;
            _waitlistRepo = waitlistRepo;
            _cache = cache;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(string userId)
        {
            var cacheKey = $"dashboard_stats_{userId}";
            
            // Try to get from cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Dashboard stats cache hit for user {UserId}", userId);
                return JsonSerializer.Deserialize<DashboardStatsDto>(cachedData)!;
            }

            // Calculate stats
            var now = DateTime.UtcNow;
            var sixMonthsAgo = now.AddMonths(-6);
            var twelveMonthsAgo = now.AddMonths(-12);
            var thirtyDaysFromNow = now.AddDays(30);

            // Current period counts
            var totalAppointmentsCurrent = await _appointmentRepo.CountByPatientAndDateRangeAsync(
                userId, sixMonthsAgo, now);
            var upcomingAppointmentsCurrent = await _appointmentRepo.CountByPatientAndDateRangeAsync(
                userId, now, thirtyDaysFromNow);
            var waitlistEntriesCurrent = await _waitlistRepo.CountByPatientAsync(userId);

            // Previous period counts for trend calculation
            var totalAppointmentsPrevious = await _appointmentRepo.CountByPatientAndDateRangeAsync(
                userId, twelveMonthsAgo, sixMonthsAgo);

            var stats = new DashboardStatsDto
            {
                TotalAppointments = totalAppointmentsCurrent,
                UpcomingAppointments = upcomingAppointmentsCurrent,
                WaitlistEntries = waitlistEntriesCurrent,
                Trends = new TrendsDto
                {
                    TotalAppointmentsTrend = CalculateTrendPercentage(totalAppointmentsCurrent, totalAppointmentsPrevious),
                    UpcomingAppointmentsTrend = 0, // Trend not applicable for future counts
                    WaitlistEntriesTrend = 0 // Trend requires historical tracking
                }
            };

            // Cache for 5 minutes
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stats), cacheOptions);

            return stats;
        }

        private double CalculateTrendPercentage(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100.0 : 0.0;
            return ((current - previous) / (double)previous) * 100.0;
        }
    }
}
```

**DTOs:**
```csharp
// PatientAccess.Business/DTOs/DashboardStatsDto.cs
namespace PatientAccess.Business.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int WaitlistEntries { get; set; }
        public TrendsDto Trends { get; set; } = new();
    }

    public class TrendsDto
    {
        public double TotalAppointmentsTrend { get; set; }
        public double UpcomingAppointmentsTrend { get; set; }
        public double WaitlistEntriesTrend { get; set; }
    }
}
```

**Appointments Upcoming Endpoint:**
```csharp
// PatientAccess.Web/Controllers/AppointmentsController.cs (add to existing)
[HttpGet("upcoming")]
[Authorize(Roles = "Patient")]
[ProducesResponseType(typeof(List<AppointmentResponseDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<List<AppointmentResponseDto>>> GetUpcomingAppointmentsAsync(
    [FromQuery] int limit = 5)
{
    try
    {
        if (limit < 1 || limit > 20)
        {
            return BadRequest(new { error = "Limit must be between 1 and 20" });
        }

        var userId = User.FindFirst("sub")?.Value 
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(userId, limit);
        return Ok(appointments);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Unauthorized access to upcoming appointments");
        return Unauthorized(new { error = "Authentication required" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving upcoming appointments");
        return StatusCode(500, new { error = "Unable to retrieve appointments" });
    }
}
```

**AppointmentService Method:**
```csharp
// PatientAccess.Business/Services/AppointmentService.cs (add to existing)
public async Task<List<AppointmentResponseDto>> GetUpcomingAppointmentsAsync(string userId, int limit)
{
    var appointments = await _appointmentRepo.GetUpcomingByPatientAsync(userId, limit);
    
    return appointments
        .Select(a => new AppointmentResponseDto
        {
            Id = a.Id,
            ProviderName = a.Provider.Name,
            Specialty = a.Provider.Specialty,
            DateTime = a.ScheduledDateTime,
            Status = a.Status.ToString(),
            VisitReason = a.VisitReason
        })
        .ToList();
}
```

**Repository Method:**
```csharp
// Add to IAppointmentRepository and implementation
Task<List<Appointment>> GetUpcomingByPatientAsync(string userId, int limit);
Task<int> CountByPatientAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);

// Implementation in AppointmentRepository
public async Task<List<Appointment>> GetUpcomingByPatientAsync(string userId, int limit)
{
    return await _context.Appointments
        .Include(a => a.Provider)
        .Where(a => a.PatientId == userId 
            && a.ScheduledDateTime >= DateTime.UtcNow
            && (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending))
        .OrderBy(a => a.ScheduledDateTime)
        .Take(limit)
        .ToListAsync();
}

public async Task<int> CountByPatientAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
{
    return await _context.Appointments
        .Where(a => a.PatientId == userId 
            && a.ScheduledDateTime >= startDate 
            && a.ScheduledDateTime <= endDate)
        .CountAsync();
}
```

### Documentation References

* **ASP.NET Core Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
* **.NET Distributed Caching**: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed
* **EF Core Performance**: https://learn.microsoft.com/en-us/ef/core/performance/
* **ASP.NET Core Logging**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/

### Edge Cases

* **What happens if patient has appointments spanning multiple years?** Query uses explicit date ranges (6 months, 30 days) preventing unbounded queries; indexes on ScheduledDateTime ensure performance.
* **How does the system handle timezone differences?** Store all datetimes in UTC in database; convert to patient's timezone on frontend based on user profile settings.
* **What happens if cache connection fails?** Catch Redis exceptions, log warning, proceed with direct database query; system remains functional without cache.
* **How does the system handle concurrent requests from same patient?** Redis cache prevents duplicate calculations; first request populates cache, subsequent requests hit cache within 5-minute window.

## Traceability

### Parent Epic

* EP-001

### Requirement Tags

* FR-002, NFR-001, NFR-006, NFR-007

### Related Tasks

* task_002_fe_stat_cards.md - Frontend consuming stats API
* task_003_fe_quick_actions_appointments.md - Frontend consuming appointments API

## Story Points

* 3

## Status

* not-started
