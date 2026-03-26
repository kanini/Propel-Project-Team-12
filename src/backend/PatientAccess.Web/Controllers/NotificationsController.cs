using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Notifications controller for patient notification management (US_067).
/// Provides endpoints for retrieving notifications, unread counts, and marking as read.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Patient")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        IAuditLogService auditService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get recent notifications for authenticated patient (US_067, AC5).
    /// Returns most recent notifications for dashboard display.
    /// </summary>
    /// <param name="limit">Maximum number of notifications to return (default: 5, max: 20)</param>
    /// <returns>List of recent notifications</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<NotificationDto>>> GetRecentAsync(
        [FromQuery] int limit = 5)
    {
        try
        {
            if (limit < 1 || limit > 20)
            {
                return BadRequest(new { error = "Limit must be between 1 and 20" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            _logger.LogInformation("Retrieving {Limit} recent notifications for user {UserId}", limit, userId);

            // TODO: Implement PHI access logging when audit service supports it (NFR-007)
            // await _auditService.LogAccessAsync(userId, "Notification", "ViewRecent");

            var notifications = await _notificationService.GetRecentNotificationsAsync(userId, limit);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent notifications");
            return StatusCode(500, new { error = "Unable to retrieve notifications" });
        }
    }

    /// <summary>
    /// Get count of unread notifications for badge display (US_067, AC5).
    /// Used to display notification badge count in header.
    /// </summary>
    /// <returns>Unread notification count</returns>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCountAsync()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new UnreadCountDto { Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unread count");
            return StatusCode(500, new { error = "Unable to retrieve unread count" });
        }
    }

    /// <summary>
    /// Mark notification as read (US_067, AC7).
    /// Updates notification read status and invalidates cache.
    /// </summary>
    /// <param name="id">Notification GUID</param>
    /// <returns>200 OK if successful</returns>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> MarkAsReadAsync([FromRoute] Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", id, userId);

            var success = await _notificationService.MarkNotificationAsReadAsync(id, userId);

            if (!success)
            {
                _logger.LogWarning("Notification {NotificationId} not found for user {UserId}", id, userId);
                return NotFound(new { error = "Notification not found" });
            }

            // TODO: Implement PHI access logging when audit service supports it (NFR-007)
            // await _auditService.LogAccessAsync(userId, "Notification", "MarkRead", id.ToString());

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, new { error = "Unable to update notification" });
        }
    }
}
