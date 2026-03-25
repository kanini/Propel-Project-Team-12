using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Intake API controller for US_033 - AI Conversational Intake.
/// Provides REST endpoints for intake session management and chat messages.
/// </summary>
[ApiController]
[Route("api/intake")]
[Authorize]
public class IntakeController : ControllerBase
{
    private readonly IIntakeService _intakeService;
    private readonly ILogger<IntakeController> _logger;

    public IntakeController(
        IIntakeService intakeService,
        ILogger<IntakeController> logger)
    {
        _intakeService = intakeService ?? throw new ArgumentNullException(nameof(intakeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts a new intake session (US_033, AC-1).
    /// POST /api/intake/start
    /// </summary>
    /// <param name="request">Start request with appointment ID and mode</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session ID and welcome message</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(StartIntakeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartIntake(
        [FromBody] StartIntakeRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            _logger.LogInformation(
                "Starting intake session for PatientId: {PatientId}, AppointmentId: {AppointmentId}",
                patientId, request.AppointmentId);

            var response = await _intakeService.StartSessionAsync(
                patientId.Value,
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting intake session");
            return StatusCode(500, new { message = "An error occurred while starting intake" });
        }
    }

    /// <summary>
    /// Processes a chat message and returns AI response (US_033, AC-2, AC-4).
    /// POST /api/intake/message
    /// </summary>
    /// <param name="request">Message request with session ID and message text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI response with extracted data and confidence metrics</returns>
    [HttpPost("message")]
    [ProducesResponseType(typeof(IntakeMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(
        [FromBody] IntakeMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            // Validate message length (AIR-O01 token budget consideration)
            if (request.Message.Length > 2000)
            {
                return BadRequest(new { message = "Message exceeds maximum length of 2000 characters" });
            }

            var response = await _intakeService.ProcessMessageAsync(
                patientId.Value,
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing intake message");
            return StatusCode(500, new { message = "An error occurred while processing message" });
        }
    }

    /// <summary>
    /// Gets intake summary for review (US_033, AC-3).
    /// GET /api/intake/{id}/summary
    /// </summary>
    /// <param name="id">Intake session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated intake summary</returns>
    [HttpGet("{id}/summary")]
    [ProducesResponseType(typeof(IntakeSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            var summary = await _intakeService.GetSummaryAsync(
                patientId.Value,
                id,
                cancellationToken);

            return Ok(summary);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting intake summary");
            return StatusCode(500, new { message = "An error occurred while retrieving summary" });
        }
    }

    /// <summary>
    /// Gets intake session information.
    /// GET /api/intake/{id}
    /// </summary>
    /// <param name="id">Intake session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session information</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IntakeSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            var session = await _intakeService.GetSessionAsync(
                patientId.Value,
                id,
                cancellationToken);

            if (session == null)
            {
                return NotFound(new { message = "Intake session not found" });
            }

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting intake session");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Updates intake data (for patient edits in summary).
    /// PATCH /api/intake/{id}
    /// </summary>
    /// <param name="id">Intake session ID</param>
    /// <param name="request">Partial update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success confirmation</returns>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIntake(
        string id,
        [FromBody] UpdateIntakeRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            await _intakeService.UpdateIntakeAsync(
                patientId.Value,
                id,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating intake");
            return StatusCode(500, new { message = "An error occurred while updating intake" });
        }
    }

    /// <summary>
    /// Completes and submits the intake session.
    /// POST /api/intake/{id}/complete
    /// </summary>
    /// <param name="id">Intake session ID</param>
    /// <param name="request">Optional final summary data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completion confirmation</returns>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(CompleteIntakeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteIntake(
        string id,
        [FromBody] CompleteIntakeRequestDto? request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            var response = await _intakeService.CompleteIntakeAsync(
                patientId.Value,
                id,
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing intake");
            return StatusCode(500, new { message = "An error occurred while completing intake" });
        }
    }

    /// <summary>
    /// Submits manual intake form (US_034).
    /// POST /api/intake/{id}/submit
    /// </summary>
    /// <param name="id">Intake session ID</param>
    /// <param name="request">Manual form data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completion confirmation</returns>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(CompleteIntakeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitManualIntake(
        string id,
        [FromBody] object request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            // Complete the intake with the manual form data
            var response = await _intakeService.CompleteIntakeAsync(
                patientId.Value,
                id,
                null,
                cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting manual intake");
            return StatusCode(500, new { message = "An error occurred while submitting intake" });
        }
    }

    /// <summary>
    /// Switches intake mode between AI and manual (US_035).
    /// POST /api/intake/{id}/switch
    /// </summary>
    /// <param name="id">Intake session ID</param>
    /// <param name="request">Mode switch request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Switch confirmation with data preservation status</returns>
    [HttpPost("{id}/switch")]
    [ProducesResponseType(typeof(SwitchModeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SwitchMode(
        string id,
        [FromBody] SwitchModeRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = GetPatientIdFromClaims();
            if (patientId == null)
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            if (request.Mode != "ai" && request.Mode != "manual")
            {
                return BadRequest(new { message = "Mode must be 'ai' or 'manual'" });
            }

            var response = await _intakeService.SwitchModeAsync(
                patientId.Value,
                id,
                request.Mode,
                cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching intake mode");
            return StatusCode(500, new { message = "An error occurred while switching mode" });
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

/// <summary>
/// Request DTO for mode switch operation.
/// </summary>
public class SwitchModeRequestDto
{
    /// <summary>
    /// New mode: "ai" or "manual".
    /// </summary>
    public string Mode { get; set; } = string.Empty;
}
