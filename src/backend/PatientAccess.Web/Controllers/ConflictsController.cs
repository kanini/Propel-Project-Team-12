using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data.Models.Enums;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Conflicts API for managing data conflicts requiring staff verification (US_048, FR-031).
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Roles = "Staff,Admin")] // Only staff can access conflict management
public class ConflictsController : ControllerBase
{
    private readonly IConflictDetectionService _conflictService;
    private readonly ILogger<ConflictsController> _logger;

    public ConflictsController(
        IConflictDetectionService conflictService,
        ILogger<ConflictsController> logger)
    {
        _conflictService = conflictService ?? throw new ArgumentNullException(nameof(conflictService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all conflicts for a patient with optional filtering.
    /// GET /api/patients/{patientId}/conflicts?severity=Critical&unresolvedOnly=true&page=1&pageSize=10
    /// </summary>
    [HttpGet("patients/{patientId}/conflicts")]
    [ProducesResponseType(typeof(List<DataConflictDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<DataConflictDto>>> GetPatientConflicts(
        int patientId,
        [FromQuery] ConflictSeverity? severity = null,
        [FromQuery] bool unresolvedOnly = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Fetching conflicts for patient {PatientId}, severity: {Severity}, page: {Page}",
            patientId, severity, page);

        var conflicts = await _conflictService.GetConflictsAsync(
            patientId,
            severity,
            unresolvedOnly,
            page,
            pageSize);

        return Ok(conflicts);
    }

    /// <summary>
    /// Get conflict summary statistics for a patient.
    /// GET /api/patients/{patientId}/conflicts/summary
    /// </summary>
    [HttpGet("patients/{patientId}/conflicts/summary")]
    [ProducesResponseType(typeof(ConflictSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ConflictSummaryDto>> GetConflictSummary(int patientId)
    {
        _logger.LogInformation("Fetching conflict summary for patient {PatientId}", patientId);

        var summary = await _conflictService.GetConflictSummaryAsync(patientId);
        return Ok(summary);
    }

    /// <summary>
    /// Resolve a specific conflict.
    /// POST /api/conflicts/{conflictId}/resolve
    /// </summary>
    [HttpPost("conflicts/{conflictId}/resolve")]
    [ProducesResponseType(typeof(DataConflictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataConflictDto>> ResolveConflict(
        Guid conflictId,
        [FromBody] ResolveConflictRequest request)
    {
        var staffUserId = GetCurrentUserId();
        if (staffUserId == Guid.Empty)
        {
            return Unauthorized("Staff user ID not found in token");
        }

        _logger.LogInformation("Staff user {UserId} resolving conflict {ConflictId}",
            staffUserId, conflictId);

        try
        {
            var resolvedConflict = await _conflictService.ResolveConflictAsync(
                conflictId,
                staffUserId,
                request.Resolution);

            return Ok(resolvedConflict);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Conflict {ConflictId} not found", conflictId);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get the current staff user ID from JWT token.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub")
                          ?? User.FindFirst("userId");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }
}
