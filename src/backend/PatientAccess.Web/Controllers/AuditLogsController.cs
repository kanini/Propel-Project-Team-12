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

    /// <summary>
    /// Exports audit log entries to CSV format for compliance reporting (US_059 - AC3).
    /// </summary>
    /// <param name="userId">Optional: filter by user ID.</param>
    /// <param name="actionType">Optional: filter by action type.</param>
    /// <param name="startDate">Optional: filter by start date (inclusive).</param>
    /// <param name="endDate">Optional: filter by end date (inclusive).</param>
    /// <response code="200">CSV file download</response>
    /// <response code="403">User is not an admin</response>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAuditLogsToCsv(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? actionType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        _logger.LogInformation(
            "Admin exporting audit logs to CSV: UserId={UserId}, ActionType={ActionType}",
            userId, actionType);

        try
        {
            // Query all matching records (no pagination for export)
            var result = await _auditLogService.GetAuditLogsAsync(
                userId, actionType, startDate, endDate, 1, int.MaxValue);

            // Generate CSV content
            var csv = new System.Text.StringBuilder();
            
            // CSV Header
            csv.AppendLine("AuditLogId,UserId,UserName,UserEmail,Timestamp,ActionType,ResourceType,IpAddress,Result");

            // CSV Rows
            foreach (var log in result.Items)
            {
                csv.AppendLine($"\"{log.AuditLogId}\",\"{log.UserId}\",\"{EscapeCsv(log.UserName)}\",\"{EscapeCsv(log.UserEmail)}\",\"{log.Timestamp:O}\",\"{EscapeCsv(log.ActionType)}\",\"{EscapeCsv(log.ResourceType)}\",\"{EscapeCsv(log.IpAddress)}\",\"Success\"");
            }

            var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs to CSV");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Failed to export audit logs" });
        }
    }

    /// <summary>
    /// Escapes CSV field values to prevent injection and formatting issues.
    /// </summary>
    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape quotes and wrap in quotes if contains comma, quote, or newline
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return value.Replace("\"", "\"\"");
        }

        return value;
    }
}
