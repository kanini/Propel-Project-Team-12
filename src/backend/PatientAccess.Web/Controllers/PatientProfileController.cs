using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// REST API controller for 360-Degree Patient View (FR-032, FR-033).
/// Implements role-based authorization (NFR-003) with sub-2-second retrieval (NFR-002).
/// </summary>
[ApiController]
[Route("api/patients")]
[Authorize] // Require authentication for all endpoints
public class PatientProfileController : ControllerBase
{
    private readonly IPatientProfileService _profileService;
    private readonly ILogger<PatientProfileController> _logger;

    public PatientProfileController(
        IPatientProfileService profileService,
        ILogger<PatientProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves comprehensive 360° patient profile view (FR-032, AIR-007).
    /// Patient role: Can only access own profile (FR-033).
    /// Staff/Admin role: Can access any patient profile.
    /// </summary>
    /// <param name="patientId">Patient's GUID identifier</param>
    /// <param name="vitalRangeStart">Start date for vital trends (optional, default: 12 months ago)</param>
    /// <param name="vitalRangeEnd">End date for vital trends (optional, default: today)</param>
    /// <returns>Complete 360° patient profile with verification badges (UXR-402)</returns>
    /// <response code="200">Returns the patient's 360° health profile</response>
    /// <response code="403">Forbidden - Patient attempting to access another patient's profile</response>
    /// <response code="404">Patient profile not found - no clinical documents uploaded</response>
    [HttpGet("{patientId:guid}/profile/360")]
    [ProducesResponseType(typeof(PatientProfile360Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientProfile360Dto>> Get360Profile(
        Guid patientId,
        [FromQuery] DateTime? vitalRangeStart = null,
        [FromQuery] DateTime? vitalRangeEnd = null)
    {
        // Get current user ID and role from JWT claims
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
        {
            _logger.LogWarning("Unable to extract user ID from JWT claims");
            return Unauthorized("Invalid authentication token");
        }

        // Role-based access control (NFR-003)
        // Patient role: Can only access own profile (FR-033)
        if (currentUserRole == "Patient" && currentUserId != patientId)
        {
            _logger.LogWarning(
                "Patient {CurrentUserId} attempted to access profile for {TargetPatientId}",
                currentUserId, patientId);

            return Forbid(); // 403 Forbidden
        }

        // Staff and Admin roles can access any patient profile
        _logger.LogInformation(
            "User {UserId} (Role: {Role}) requesting 360° profile for patient {PatientId}",
            currentUserId, currentUserRole, patientId);

        try
        {
            var profile = await _profileService.Get360ProfileAsync(
                patientId, vitalRangeStart, vitalRangeEnd);

            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogInformation(ex, "Patient profile not found for {PatientId}", patientId);
            return NotFound(new
            {
                message = "Patient profile not found. Upload your clinical documents to build your health profile.",
                patientId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving 360° profile for patient {PatientId}", patientId);
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving the patient profile",
                patientId
            });
        }
    }

    /// <summary>
    /// Invalidates cached profile data for a patient (Staff/Admin only).
    /// Used after data aggregation or conflict resolution updates.
    /// </summary>
    /// <param name="patientId">Patient's GUID identifier</param>
    /// <returns>204 No Content on success</returns>
    [HttpDelete("{patientId:guid}/profile/360/cache")]
    [Authorize(Roles = "Staff,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InvalidateProfileCache(Guid patientId)
    {
        _logger.LogInformation(
            "User {UserId} invalidating cache for patient {PatientId}",
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value, patientId);

        await _profileService.InvalidateCacheAsync(patientId);

        return NoContent();
    }
}
