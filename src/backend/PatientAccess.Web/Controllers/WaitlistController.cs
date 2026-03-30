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
    /// Confirms waitlist slot offer via anonymous token link (US_041 - AC-2).
    /// Books appointment if slot is still available, returns 410 Gone if slot unavailable.
    /// </summary>
    /// <param name="token">Response token from notification email/SMS</param>
    /// <returns>200 OK with appointment details, 410 Gone if slot taken, 404 if invalid token</returns>
    /// <response code="200">Appointment booked successfully</response>
    /// <response code="404">Invalid or expired token</response>
    /// <response code="410">Slot no longer available (re-booked by another patient)</response>
    [HttpPost("confirm/{token}")]
    [AllowAnonymous] // US_041 - AC-2: Allow token-based access without authentication
    [ProducesResponseType(typeof(ConfirmWaitlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ConfirmWaitlistResponseDto), StatusCodes.Status410Gone)]
    public async Task<IActionResult> ConfirmWaitlistOffer([FromRoute] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            _logger.LogInformation("Processing waitlist confirmation for token {Token}", token);

            var result = await _notificationService.ProcessConfirmAsync(token);

            if (!result.Success)
            {
                // EC-2: Slot no longer available
                _logger.LogWarning("Waitlist confirmation failed - slot unavailable for token {Token}", token);
                return StatusCode(StatusCodes.Status410Gone, result);
            }

            _logger.LogInformation(
                "Waitlist confirmation successful. Appointment {AppointmentId} booked for token {Token}",
                result.AppointmentId, token);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Invalid or expired token: {Token}", token);
            return NotFound(new { message = "Invalid or expired confirmation link" });
        }
        catch (InvalidOperationException ex)
        {
            // Response deadline expired
            _logger.LogWarning(ex, "Expired token: {Token}", token);
            return StatusCode(StatusCodes.Status410Gone, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing waitlist confirmation for token {Token}", token);
            return StatusCode(500, new { message = "An error occurred while processing confirmation" });
        }
    }

    /// <summary>
    /// Declines waitlist slot offer via anonymous token link (US_041 - AC-3).
    /// Resets waitlist entry to Active status and cascades offer to next patient.
    /// </summary>
    /// <param name="token">Response token from notification email/SMS</param>
    /// <returns>200 OK, 404 if invalid token</returns>
    /// <response code="200">Declined successfully, slot offered to next patient</response>
    /// <response code="404">Invalid or expired token</response>
    [HttpPost("decline/{token}")]
    [AllowAnonymous] // US_041 - AC-3: Allow token-based access without authentication
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeclineWaitlistOffer([FromRoute] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            _logger.LogInformation("Processing waitlist decline for token {Token}", token);

            var success = await _notificationService.ProcessDeclineAsync(token);

            if (!success)
            {
                _logger.LogWarning("Waitlist decline failed for token {Token}", token);
                return NotFound(new { message = "Invalid or expired decline link" });
            }

            _logger.LogInformation("Waitlist decline successful for token {Token}. Slot cascaded to next patient.", token);

            return Ok(new
            {
                success = true,
                message = "You have successfully declined the appointment offer. The slot will be offered to the next patient on the waitlist."
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Invalid token: {Token}", token);
            return NotFound(new { message = "Invalid or expired decline link" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing waitlist decline for token {Token}", token);
            return StatusCode(500, new { message = "An error occurred while processing decline" });
        }
    }
}
