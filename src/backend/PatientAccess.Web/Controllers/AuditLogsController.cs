using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Controller for querying audit log entries (US_022, US_055).
/// Admin-only access for viewing immutable audit trail.
/// </summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = "AdminOnly")] // All endpoints require Admin role
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditLogService auditLogService,
        ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated audit log entries with optional filtering.
    /// </summary>
    /// <param name="userId">Optional: filter by user ID.</param>
    /// <param name="actionType">Optional: filter by action type (Login, Logout, FailedLogin, etc.).</param>
    /// <param name="startDate">Optional: filter by start date (inclusive).</param>
    /// <param name="endDate">Optional: filter by end date (inclusive).</param>
    /// <param name="page">Page number (1-based, default: 1).</param>
    /// <param name="pageSize">Results per page (default: 25, max: 100).</param>
    /// <response code="200">Paginated audit log entries</response>
    /// <response code="403">User is not an admin</response>
    [HttpGet]
    [ProducesResponseType(typeof(AuditLogQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? actionType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        // Clamp pageSize to prevent excessive queries
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        _logger.LogInformation(
            "Admin querying audit logs: UserId={UserId}, ActionType={ActionType}, Page={Page}",
            userId, actionType, page);

        try
        {
            var result = await _auditLogService.GetAuditLogsAsync(
                userId, actionType, startDate, endDate, page, pageSize);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying audit logs");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve audit logs" });
        }
    }
}
