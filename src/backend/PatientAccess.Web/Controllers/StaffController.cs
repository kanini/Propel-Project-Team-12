using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Staff controller for healthcare provider operations (US_020).
/// Endpoints accessible by Staff and Admin roles.
/// </summary>
[ApiController]
[Route("api/staff")]
[Authorize(Policy = "StaffOnly")] // Accessible by Staff and Admin
public class StaffController : ControllerBase
{
    private readonly ILogger<StaffController> _logger;

    public StaffController(ILogger<StaffController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for staff panel.
    /// </summary>
    /// <returns>Staff panel status</returns>
    /// <response code="200">Staff access confirmed</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetStaffHealth()
    {
        _logger.LogInformation("Staff health check accessed");

        return Ok(new
        {
            status = "healthy",
            message = "Staff access confirmed",
            role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Placeholder for appointment management endpoint.
    /// </summary>
    /// <returns>List of appointments (placeholder)</returns>
    /// <response code="200">Appointments retrieved successfully</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    [HttpGet("appointments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAppointments()
    {
        _logger.LogInformation("Staff appointment list accessed");

        // Placeholder - future implementation
        return Ok(new
        {
            message = "Appointment management endpoint - placeholder",
            appointments = new object[] { }
        });
    }
}
