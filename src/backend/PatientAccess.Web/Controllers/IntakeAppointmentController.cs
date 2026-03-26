using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Controller for intake appointment selection operations (US_037).
/// Provides REST endpoints for fetching appointments requiring intake.
/// </summary>
[ApiController]
[Route("api/intake")]
[Authorize]
public class IntakeAppointmentController : ControllerBase
{
    private readonly IIntakeAppointmentService _intakeAppointmentService;
    private readonly ILogger<IntakeAppointmentController> _logger;

    public IntakeAppointmentController(
        IIntakeAppointmentService intakeAppointmentService,
        ILogger<IntakeAppointmentController> logger)
    {
        _intakeAppointmentService = intakeAppointmentService ?? throw new ArgumentNullException(nameof(intakeAppointmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves appointments requiring intake for the authenticated patient (US_037, AC-1).
    /// GET /api/intake/appointments
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of appointments with intake status</returns>
    [HttpGet("appointments")]
    [ProducesResponseType(typeof(List<IntakeAppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIntakeAppointments(CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                _logger.LogWarning("Unauthorized access attempt - invalid authentication token");
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            _logger.LogInformation(
                "Fetching intake appointments for PatientId: {PatientId}",
                patientId);

            var appointments = await _intakeAppointmentService.GetPatientIntakeAppointmentsAsync(
                patientId.Value,
                cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} intake appointments for PatientId: {PatientId}",
                appointments.Count, patientId);

            return Ok(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching intake appointments");
            return StatusCode(500, new { message = "An error occurred while fetching intake appointments" });
        }
    }

    /// <summary>
    /// Helper method to extract patient GUID from JWT claims.
    /// </summary>
    private Guid? GetPatientIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var patientGuid))
        {
            return null;
        }

        return patientGuid;
    }
}
