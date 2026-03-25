# Task - task_007_be_notifications_documents_api

## Task ID

* ID: task_007_be_notifications_documents_api

## Task Title

* Implement Notifications and Recent Documents API Endpoints (Backend)

## Parent User Story

* US_067 - Patient Dashboard - Post-Login Landing Page

## Description

Create RESTful API endpoints for retrieving recent notifications (5 most recent unread), unread notification count, mark notification as read functionality, and recent clinical documents (3 most recent with processing status) with proper authentication, authorization, and caching.

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

1. **Given** an authenticated patient requests recent notifications, **When** GET `/api/notifications/recent?limit=5` is called, **Then** the system returns 5 most recent unread notifications with title, message, timestamp, and action links within 500ms.

2. **Given** an authenticated patient requests unread count, **When** GET `/api/notifications/unread-count` is called, **Then** the system returns total unread notification count for badge display.

3. **Given** a patient marks a notification as read, **When** PATCH `/api/notifications/{id}/read` is called, **Then** the system updates the notification status and returns 200 OK.

4. **Given** an authenticated patient requests recent documents, **When** GET `/api/documents/recent?limit=3` is called, **Then** the system returns 3 most recent clinical documents with file name, upload date, and processing status.

5. **Given** a request is made without authentication token, **When** any endpoint is called, **Then** the system returns 401 Unauthorized.

6. **Given** a patient requests unread count, **When** the query completes, **Then** the system caches the result for 2 minutes using Redis to reduce database load.

7. **Given** a patient marks notification as read, **When** the update completes, **Then** the system invalidates the cached unread count and recent notifications list.

8. **Given** audit logging is enabled, **When** a patient accesses notifications or documents (PHI), **Then** the system logs the access with user ID, timestamp, and resource type per NFR-007.

## Implementation Checklist

- [ ] Create NotificationsController at `PatientAccess.Web/Controllers/NotificationsController.cs` with GET /recent, GET /unread-count, and PATCH /{id}/read endpoints
- [ ] Implement INotificationService interface and NotificationService at `PatientAccess.Business/Services/NotificationService.cs` with business logic
- [ ] Add GetRecentNotificationsAsync method querying notifications filtered by PatientId and IsRead = false, ordered by CreatedAt descending, limited by parameter
- [ ] Implement GetUnreadCountAsync method returning count of unread notifications for patient with Redis caching (2-minute TTL)
- [ ] Add MarkNotificationAsReadAsync mutation method updating IsRead flag and invalidating cache
- [ ] Update DocumentsController adding GET /api/documents/recent endpoint with limit parameter (default: 3, max: 10)
- [ ] Implement GetRecentDocumentsAsync in DocumentService querying documents by PatientId ordered by UploadedAt descending
- [ ] Add audit logging interceptor for PHI access tracking using IAuditService for notifications and documents endpoints

## Estimated Effort

* 6 hours

## Dependencies

- Existing Notification and ClinicalDocument entities and repositories
- Redis/Upstash cache configuration
- JWT authentication middleware
- AuditService for PHI access logging

## Technical Context

### Architecture Patterns

* **Pattern**: Three-layer architecture (Controller → Service → Repository)
* **Caching Strategy**: Redis distributed cache with 2-minute TTL for counts
* **Cache Invalidation**: Explicit cache invalidation on mutation operations
* **Audit Logging**: Centralized audit service logging all PHI access

### Related Requirements

* FR-002: Secure session management with authentication
* NFR-001: API response time within 500ms at 95th percentile
* NFR-006: Role-based access control restricting to Patient role
* NFR-007: Immutable audit logs capturing PHI access patterns

### Implementation References

**NotificationsController:**
```csharp
// PatientAccess.Web/Controllers/NotificationsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.DTOs;

namespace PatientAccess.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            IAuditService auditService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Get recent unread notifications for authenticated patient
        /// </summary>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NotificationDto>>> GetRecentAsync(
            [FromQuery] int limit = 5)
        {
            try
            {
                if (limit < 1 || limit > 20)
                {
                    return BadRequest(new { error = "Limit must be between 1 and 20" });
                }

                var userId = User.FindFirst("sub")?.Value 
                    ?? throw new UnauthorizedAccessException("User ID not found");

                await _auditService.LogAccessAsync(userId, "Notification", "ViewRecent");

                var notifications = await _notificationService.GetRecentNotificationsAsync(userId, limit);
                return Ok(notifications);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to notifications");
                return Unauthorized(new { error = "Authentication required" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent notifications");
                return StatusCode(500, new { error = "Unable to retrieve notifications" });
            }
        }

        /// <summary>
        /// Get count of unread notifications
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UnreadCountDto>> GetUnreadCountAsync()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value 
                    ?? throw new UnauthorizedAccessException("User ID not found");

                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(new UnreadCountDto { Count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread count");
                return StatusCode(500, new { error = "Unable to retrieve count" });
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPatch("{id}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> MarkAsReadAsync(string id)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value 
                    ?? throw new UnauthorizedAccessException("User ID not found");

                var success = await _notificationService.MarkNotificationAsReadAsync(id, userId);
                
                if (!success)
                {
                    return NotFound(new { error = "Notification not found" });
                }

                await _auditService.LogAccessAsync(userId, "Notification", "MarkRead", id);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized mark as read attempt");
                return Unauthorized(new { error = "Authentication required" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new { error = "Unable to update notification" });
            }
        }
    }
}
```

**NotificationService Implementation:**
```csharp
// PatientAccess.Business/Services/NotificationService.cs
using Microsoft.Extensions.Caching.Distributed;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.DTOs;
using System.Text.Json;

namespace PatientAccess.Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepo,
            IDistributedCache cache,
            ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<NotificationDto>> GetRecentNotificationsAsync(string userId, int limit)
        {
            var cacheKey = $"recent_notifications_{userId}_{limit}";
            
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<NotificationDto>>(cachedData)!;
            }

            var notifications = await _notificationRepo.GetRecentUnreadAsync(userId, limit);
            
            var dtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                ActionLink = n.ActionLink,
                ActionLabel = n.ActionLabel,
                IsRead = n.IsRead
            }).ToList();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dtos), cacheOptions);

            return dtos;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var cacheKey = $"unread_count_{userId}";
            
            var cachedCount = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedCount) && int.TryParse(cachedCount, out var count))
            {
                return count;
            }

            var unreadCount = await _notificationRepo.GetUnreadCountAsync(userId);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };
            await _cache.SetStringAsync(cacheKey, unreadCount.ToString(), cacheOptions);

            return unreadCount;
        }

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            
            if (notification == null || notification.PatientId != userId)
            {
                return false;
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            
            await _notificationRepo.UpdateAsync(notification);

            // Invalidate caches
            await _cache.RemoveAsync($"unread_count_{userId}");
            await _cache.RemoveAsync($"recent_notifications_{userId}_5");

            return true;
        }
    }
}
```

**DocumentsController Recent Endpoint:**
```csharp
// PatientAccess.Web/Controllers/DocumentsController.cs (add to existing)
[HttpGet("recent")]
[Authorize(Roles = "Patient")]
[ProducesResponseType(typeof(List<ClinicalDocumentDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<List<ClinicalDocumentDto>>> GetRecentDocumentsAsync(
    [FromQuery] int limit = 3)
{
    try
    {
        if (limit < 1 || limit > 10)
        {
            return BadRequest(new { error = "Limit must be between 1 and 10" });
        }

        var userId = User.FindFirst("sub")?.Value 
            ?? throw new UnauthorizedAccessException("User ID not found");

        await _auditService.LogAccessAsync(userId, "ClinicalDocument", "ViewRecent");

        var documents = await _documentService.GetRecentDocumentsAsync(userId, limit);
        return Ok(documents);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Unauthorized access to documents");
        return Unauthorized(new { error = "Authentication required" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving recent documents");
        return StatusCode(500, new { error = "Unable to retrieve documents" });
    }
}
```

**DocumentService Method:**
```csharp
// PatientAccess.Business/Services/DocumentService.cs (add to existing)
public async Task<List<ClinicalDocumentDto>> GetRecentDocumentsAsync(string userId, int limit)
{
    var documents = await _documentRepo.GetRecentByPatientAsync(userId, limit);
    
    return documents.Select(d => new ClinicalDocumentDto
    {
        Id = d.Id,
        FileName = d.FileName,
        UploadedAt = d.UploadedAt,
        ProcessingStatus = d.ProcessingStatus.ToString()
    }).ToList();
}
```

**Repository Methods:**
```csharp
// Add to INotificationRepository and implementation
Task<List<Notification>> GetRecentUnreadAsync(string userId, int limit);
Task<int> GetUnreadCountAsync(string userId);

// Implementation in NotificationRepository
public async Task<List<Notification>> GetRecentUnreadAsync(string userId, int limit)
{
    return await _context.Notifications
        .Where(n => n.PatientId == userId && !n.IsRead)
        .OrderByDescending(n => n.CreatedAt)
        .Take(limit)
        .ToListAsync();
}

public async Task<int> GetUnreadCountAsync(string userId)
{
    return await _context.Notifications
        .CountAsync(n => n.PatientId == userId && !n.IsRead);
}

// Add to IClinicalDocumentRepository and implementation
Task<List<ClinicalDocument>> GetRecentByPatientAsync(string userId, int limit);

// Implementation in ClinicalDocumentRepository
public async Task<List<ClinicalDocument>> GetRecentByPatientAsync(string userId, int limit)
{
    return await _context.ClinicalDocuments
        .Where(d => d.PatientId == userId)
        .OrderByDescending(d => d.UploadedAt)
        .Take(limit)
        .ToListAsync();
}
```

**DTOs:**
```csharp
// PatientAccess.Business/DTOs/NotificationDto.cs
namespace PatientAccess.Business.DTOs
{
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ActionLink { get; set; }
        public string? ActionLabel { get; set; }
        public bool IsRead { get; set; }
    }

    public class UnreadCountDto
    {
        public int Count { get; set; }
    }

    public class ClinicalDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string ProcessingStatus { get; set; } = string.Empty;
    }
}
```

### Documentation References

* **ASP.NET Core PATCH Operations**: https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch
* **.NET Distributed Cache**: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed
* **EF Core Queries**: https://learn.microsoft.com/en-us/ef/core/querying/
* **ASP.NET Core Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/

### Edge Cases

* **What happens if notification belongs to different patient?** Validate notification.PatientId matches authenticated userId; return 404 if mismatch to prevent information disclosure.
* **How does the system handle marking already-read notification as read?** Idempotent operation; return 200 OK without error; cache invalidation still occurs to maintain consistency.
* **What happens if unread count grows very large (>999)?** Frontend caps display at "9+" but backend returns accurate count for metrics; no server-side capping.
* **How does cache invalidation handle multiple notification reads in quick succession?** Each mutation invalidates cache; subsequent reads repopulate cache; trade-off accepted for consistency.

## Traceability

### Parent Epic

* EP-001

### Requirement Tags

* FR-002, NFR-001, NFR-006, NFR-007

### Related Tasks

* task_004_fe_notifications_documents.md - Frontend consuming notifications and documents APIs

## Story Points

* 3

## Status

* not-started
