using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Web.Filters;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Controller demonstrating Role-Based Access Control (RBAC) with minimum necessary access enforcement (NFR-006, NFR-014).
/// Shows role-specific endpoint protection and resource-level authorization.
/// </summary>
[ApiController]
[Route("api/patients")]
[Produces("application/json")]
[ServiceFilter(typeof(MinimumNecessaryAccessFilter))]
public class PatientsController : ControllerBase
{
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(ILogger<PatientsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current patient's own profile (Patient role only).
    /// Demonstrates minimum necessary access - patients can only access their own data.
    /// </summary>
    /// <returns>Patient profile data</returns>
    /// <response code="200">Patient profile retrieved</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not a patient user</response>
    [HttpGet("me")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetMyProfile()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("Patient {UserId} accessed their own profile", userId);
        
        return Ok(new
        {
            UserId = userId,
            Message = "Patient profile data (minimum necessary access enforced)",
            Role = "Patient"
        });
    }

    /// <summary>
    /// Gets a specific patient by ID (Staff and Admin roles only).
    /// MinimumNecessaryAccessFilter logs data access for audit trail.
    /// </summary>
    /// <param name="id">Patient ID</param>
    /// <returns>Patient profile data</returns>
    /// <response code="200">Patient profile retrieved</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not authorized (Patient users blocked by role check or minimum access filter)</response>
    /// <response code="404">Patient not found</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Staff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPatientById(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("User {UserId} accessed patient {PatientId} profile", userId, id);
        
        return Ok(new
        {
            PatientId = id,
            AccessedBy = userId,
            Message = "Patient profile data (Staff/Admin access, logged for audit)"
        });
    }

    /// <summary>
    /// Gets all patients (Staff and Admin roles only).
    /// </summary>
    /// <returns>List of patient profiles</returns>
    /// <response code="200">Patient list retrieved</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not authorized</response>
    [HttpGet]
    [Authorize(Roles = "Staff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAllPatients()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("User {UserId} accessed all patients list", userId);
        
        return Ok(new
        {
            Message = "Patient list (Staff/Admin access only)",
            Count = 0,
            Patients = Array.Empty<object>()
        });
    }

    /// <summary>
    /// Creates a new patient (Admin role only).
    /// </summary>
    /// <param name="patientData">Patient data</param>
    /// <returns>Created patient</returns>
    /// <response code="201">Patient created</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not authorized (only Admin can create patients)</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult CreatePatient([FromBody] object patientData)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("Admin {UserId} created new patient", userId);
        
        return CreatedAtAction(nameof(GetPatientById), new { id = Guid.NewGuid() }, new
        {
            Message = "Patient created (Admin-only operation)",
            CreatedBy = userId
        });
    }
}
