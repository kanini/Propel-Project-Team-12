using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Waitlist API controller for US_025 - Waitlist Enrollment (FR-009).
/// Provides REST endpoints for joining, viewing, updating, and leaving waitlist.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all waitlist endpoints
public class WaitlistController : ControllerBase
{
    private readonly IWaitlistService _waitlistService;
    private readonly IWaitlistNotificationService _notificationService;
    private readonly ILogger<WaitlistController> _logger;

    public WaitlistController(
        IWaitlistService waitlistService,
        IWaitlistNotificationService notificationService,
        ILogger<WaitlistController> logger)
    {
        _waitlistService = waitlistService ?? throw new ArgumentNullException(nameof(waitlistService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Joins waitlist for a provider (FR-009, AC-2, AC-3).
    /// Returns 409 Conflict if patient already on waitlist with existing entry data.
    /// </summary>
    /// <param name="request">Waitlist enrollment request</param>
    /// <returns>201 Created with waitlist entry and queue position</returns>
    /// <response code="201">Successfully joined waitlist</response>
    /// <response code="400">Validation error (invalid dates, provider)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="409">Patient already on waitlist for this provider (AC-3)</response>
    [HttpPost]
    [ProducesResponseType(typeof(WaitlistEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> JoinWaitlist([FromBody] JoinWaitlistRequestDto request)
    {
        try
        {
            // Extract patient ID from authenticated user claims
            var patientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(patientIdClaim) || !Guid.TryParse(patientIdClaim, out var patientId))
            {
                _logger.LogWarning("Unable to extract patient ID from claims");
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            // Validate request model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Invalid waitlist join request: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Join waitlist
            var entry = await _waitlistService.JoinWaitlistAsync(patientId, request);

            // Return 201 Created with Location header
            return CreatedAtAction(
                nameof(GetWaitlistEntries),
                new { },
                entry);
        }
        catch (ConflictException ex)
        {
            // AC-3: Return 409 Conflict with existing entry data
            _logger.LogWarning(ex, "Patient already on waitlist for Provider {ProviderId}", request.ProviderId);

            var existingEntry = ex.Data["ExistingEntry"] as WaitlistEntryDto;
            return Conflict(new
            {
                message = ex.Message,
                existingEntry
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Waitlist join validation error");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining waitlist for Provider {ProviderId}", request.ProviderId);
            return StatusCode(500, new { message = "An error occurred while joining waitlist" });
        }
    }

    /// <summary>
    /// Retrieves patient's active waitlist entries with queue positions (FR-009, AC-4).
    /// </summary>
    /// <returns>200 OK with list of waitlist entries</returns>
    /// <response code="200">Waitlist entries retrieved successfully (may be empty array)</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<WaitlistEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWaitlistEntries()
    {
        try
        {
            var patientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(patientIdClaim) || !Guid.TryParse(patientIdClaim, out var patientId))
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            var entries = await _waitlistService.GetPatientWaitlistAsync(patientId);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving waitlist entries");
            return StatusCode(500, new { message = "An error occurred while retrieving waitlist entries" });
        }
    }

    /// <summary>
    /// Updates waitlist preferences (FR-009).
    /// Maintains queue position while updating date range and notification preferences.
    /// </summary>
    /// <param name="id">Waitlist entry ID</param>
    /// <param name="request">Update request</param>
    /// <returns>200 OK with updated entry</returns>
    /// <response code="200">Waitlist entry updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Entry not found or unauthorized</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WaitlistEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWaitlist([FromRoute] Guid id, [FromBody] UpdateWaitlistRequestDto request)
    {
        try
        {
            var patientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(patientIdClaim) || !Guid.TryParse(patientIdClaim, out var patientId))
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var updatedEntry = await _waitlistService.UpdateWaitlistAsync(id, patientId, request);
            return Ok(updatedEntry);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Waitlist entry {EntryId} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Waitlist update validation error");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating waitlist entry {EntryId}", id);
            return StatusCode(500, new { message = "An error occurred while updating waitlist entry" });
        }
    }

    /// <summary>
    /// Removes patient from waitlist (FR-009).
    /// Soft deletes entry by setting status to Expired.
    /// </summary>
    /// <param name="id">Waitlist entry ID</param>
    /// <returns>204 No Content</returns>
    /// <response code="204">Waitlist entry deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Entry not found or unauthorized</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWaitlist([FromRoute] Guid id)
    {
        try
        {
            var patientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(patientIdClaim) || !Guid.TryParse(patientIdClaim, out var patientId))
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            await _waitlistService.DeleteWaitlistAsync(id, patientId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Waitlist entry {EntryId} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting waitlist entry {EntryId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting waitlist entry" });
        }
    }

    /// <summary>
    /// Confirms waitlist slot offer — books appointment (US_041 - FR-026, AC-2).
    /// AllowAnonymous because patient clicks link from email/SMS.
    /// Token-based authentication via ResponseToken.
    /// </summary>
    /// <param name="token">Unique response token from notification</param>
    /// <returns>Appointment booking result</returns>
    /// <response code="200">Appointment booked successfully</response>
    /// <response code="404">Invalid or expired token</response>
    /// <response code="409">Slot no longer available (EC-2)</response>
    /// <response code="410">Notification already responded to or expired</response>
    [HttpPost("{token}/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ConfirmWaitlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<IActionResult> ConfirmSlot([FromRoute] string token)
    {
        try
        {
            _logger.LogInformation("Processing waitlist slot confirmation for token {Token}", token);

            var result = await _notificationService.ProcessConfirmAsync(token);

            if (!result.Success)
            {
                // EC-2: Slot no longer available
                _logger.LogWarning("Slot no longer available for token {Token}", token);
                return Conflict(new { message = result.Message });
            }

            _logger.LogInformation("Appointment {AppointmentId} booked via waitlist confirmation", result.AppointmentId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Invalid or expired token: {Token}", token);
            return NotFound(new { message = "Invalid or expired notification token" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Notification already responded to or expired: {Token}", token);
            return StatusCode(StatusCodes.Status410Gone, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing waitlist confirmation for token {Token}", token);
            return StatusCode(500, new { message = "An error occurred while confirming the appointment" });
        }
    }

    /// <summary>
    /// Declines waitlist slot offer — stays on waitlist, next patient notified (US_041 - FR-026, AC-3).
    /// AllowAnonymous for same reason as confirm.
    /// </summary>
    /// <param name="token">Unique response token from notification</param>
    /// <returns>Decline confirmation</returns>
    /// <response code="200">Declined successfully, remains on waitlist</response>
    /// <response code="404">Invalid or expired token</response>
    /// <response code="410">Notification already responded to</response>
    [HttpPost("{token}/decline")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<IActionResult> DeclineSlot([FromRoute] string token)
    {
        try
        {
            _logger.LogInformation("Processing waitlist slot decline for token {Token}", token);

            await _notificationService.ProcessDeclineAsync(token);

            _logger.LogInformation("Slot declined for token {Token}, patient remains on waitlist", token);
            return Ok(new { message = "You remain on the waitlist. We'll notify you when another slot opens." });
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Invalid or expired token: {Token}", token);
            return NotFound(new { message = "Invalid or expired notification token" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Notification already responded to: {Token}", token);
            return StatusCode(StatusCodes.Status410Gone, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing waitlist decline for token {Token}", token);
            return StatusCode(500, new { message = "An error occurred while processing your decline" });
        }
    }
}
