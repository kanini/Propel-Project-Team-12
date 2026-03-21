using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Patient controller for patient-specific operations (US_020).
/// Endpoints require Patient role and enforce same-patient data access (AC3).
/// </summary>
[ApiController]
[Route("api/patients")]
[Authorize(Policy = "PatientOnly")] // Accessible by patients only
public class PatientController : ControllerBase
{
    private readonly ILogger<PatientController> _logger;

    public PatientController(ILogger<PatientController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets patient profile for the authenticated patient.
    /// Enforces same-patient access (AC3) - patients can only access their own data.
    /// Staff and Admin can access any patient profile.
    /// </summary>
    /// <param name="patientId">Patient ID from route</param>
    /// <returns>Patient profile information</returns>
    /// <response code="200">Patient profile retrieved successfully</response>
    /// <response code="403">Cross-patient access denied</response>
    /// <response code="404">Patient not found</response>
    [HttpGet("{patientId}/profile")]
    [Authorize(Policy = "SamePatient")] // Enforces same-patient access
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPatientProfile([FromRoute] Guid patientId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation("Patient profile accessed: PatientId={PatientId}, UserId={UserId}", 
            patientId, userId);

        // Placeholder - will be implemented with actual patient data
        return Ok(new
        {
            patientId,
            message = "Patient profile endpoint - placeholder",
            accessedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets appointments for the authenticated patient.
    /// Enforces same-patient access (AC3).
    /// </summary>
    /// <param name="patientId">Patient ID from route</param>
    /// <returns>List of patient appointments</returns>
    /// <response code="200">Appointments retrieved successfully</response>
    /// <response code="403">Cross-patient access denied</response>
    [HttpGet("{patientId}/appointments")]
    [Authorize(Policy = "SamePatient")] // Enforces same-patient access
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetPatientAppointments([FromRoute] Guid patientId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation("Patient appointments accessed: PatientId={PatientId}, UserId={UserId}", 
            patientId, userId);

        // Placeholder - future implementation
        return Ok(new
        {
            patientId,
            message = "Patient appointments endpoint - placeholder",
            appointments = new object[] { }
        });
    }
}
