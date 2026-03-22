using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Appointments API controller for US_024 - Appointment Booking (FR-007, FR-008).
/// Provides REST endpoints for availability calendar and appointment booking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all appointment endpoints
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ISlotSwapService _slotSwapService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        ISlotSwapService slotSwapService,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
        _slotSwapService = slotSwapService ?? throw new ArgumentNullException(nameof(slotSwapService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves provider availability - supports both monthly and daily queries (FR-007, AC2).
    /// Query by month/year for monthly view, or by date for daily view.
    /// Must respond within 500ms at P95 (NFR-001).
    /// </summary>
    /// <param name="providerId">Provider GUID</param>
    /// <param name="year">Year for monthly availability (optional, mutually exclusive with date)</param>
    /// <param name="month">Month for monthly availability (1-12, optional, mutually exclusive with date)</param>
    /// <param name="date">Date for daily availability (optional, mutually exclusive with year/month)</param>
    /// <returns>200 OK with availability data</returns>
    /// <response code="200">Availability data retrieved successfully</response>
    /// <response code="400">Invalid query parameters (must provide either month/year OR date)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Provider not found</response>
    [HttpGet("/api/providers/{providerId:guid}/availability")]
    [ProducesResponseType(typeof(List<AvailabilityResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AvailabilityResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProviderAvailability(
        [FromRoute] Guid providerId,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] DateTime? date = null)
    {
        try
        {
            // Determine query type: monthly or daily
            var isMonthlyQuery = year.HasValue && month.HasValue;
            var isDailyQuery = date.HasValue;

            // Validate: must provide either monthly OR daily query parameters
            if (isMonthlyQuery && isDailyQuery)
            {
                return BadRequest(new { message = "Cannot specify both month/year and date parameters. Choose monthly or daily query." });
            }

            if (!isMonthlyQuery && !isDailyQuery)
            {
                return BadRequest(new { message = "Must provide either month/year parameters for monthly view or date parameter for daily view." });
            }

            // Handle monthly availability query
            if (isMonthlyQuery)
            {
                if (month!.Value < 1 || month.Value > 12)
                {
                    return BadRequest(new { message = "Month must be between 1 and 12" });
                }

                if (year!.Value < 2000 || year.Value > 2100)
                {
                    return BadRequest(new { message = "Year must be between 2000 and 2100" });
                }

                var monthlyAvailability = await _appointmentService.GetMonthlyAvailabilityAsync(providerId, year.Value, month.Value);
                return Ok(monthlyAvailability);
            }

            // Handle daily availability query
            if (isDailyQuery)
            {
                var dailyAvailability = await _appointmentService.GetDailyAvailabilityAsync(providerId, date!.Value);
                return Ok(dailyAvailability);
            }

            // Should never reach here due to validation above
            return BadRequest(new { message = "Invalid availability query parameters" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving availability for Provider {ProviderId}", providerId);
            return StatusCode(500, new { message = "An error occurred while retrieving availability" });
        }
    }

    /// <summary>
    /// Creates a new appointment with conflict detection (FR-008, AC3, AC4).
    /// Uses pessimistic locking (SELECT FOR UPDATE) to prevent double-booking.
    /// Returns 409 Conflict if slot is already booked.
    /// </summary>
    /// <param name="request">Appointment creation request</param>
    /// <returns>201 Created with appointment details and Location header</returns>
    /// <response code="201">Appointment created successfully with confirmation number</response>
    /// <response code="400">Validation error (invalid provider, time slot, or visit reason)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="409">Conflict - time slot already booked by another patient (AC4)</response>
    /// <response code="500">Internal server error or deadlock after retries</response>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDto request)
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

            // Validate request model (data annotations)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Invalid appointment request: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Create appointment
            var appointment = await _appointmentService.CreateAppointmentAsync(patientId, request);

            // Enqueue confirmation email job (US_028 - FR-012, AC-3)
            Hangfire.BackgroundJob.Enqueue<PatientAccess.Business.BackgroundJobs.ConfirmationEmailJob>(
                job => job.SendConfirmationAsync(appointment.Id));

            _logger.LogInformation(
                "Confirmation email job enqueued for appointment {AppointmentId}",
                appointment.Id);

            // Return 201 Created with Location header
            return CreatedAtAction(
                nameof(GetAppointmentById),
                new { id = appointment.Id },
                appointment);
        }
        catch (ConflictException ex)
        {
            // AC4: Return 409 Conflict when slot is already booked
            _logger.LogWarning(ex, "Appointment booking conflict for TimeSlot {TimeSlotId}", request.TimeSlotId);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Validation errors (invalid provider, time slot, etc.)
            _logger.LogWarning(ex, "Appointment booking validation error");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("high concurrency"))
        {
            // Deadlock after retries
            _logger.LogError(ex, "Deadlock detected after retries for TimeSlot {TimeSlotId}", request.TimeSlotId);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment for TimeSlot {TimeSlotId}", request.TimeSlotId);
            return StatusCode(500, new { message = "An error occurred while creating the appointment" });
        }
    }

    /// <summary>
    /// Retrieves a single appointment by ID (placeholder for future feature).
    /// </summary>
    /// <param name="id">Appointment GUID</param>
    /// <returns>200 OK with appointment details</returns>
    /// <response code="200">Appointment found</response>
    /// <response code="404">Appointment not found</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAppointmentById([FromRoute] Guid id)
    {
        // Placeholder for future implementation
        // For now, return 501 Not Implemented
        return StatusCode(501, new { message = "Get appointment by ID not yet implemented" });
    }

    /// <summary>
    /// Retrieves all appointments for the authenticated patient (US_027 - FR-011).
    /// Used by My Appointments page to display upcoming and past appointments.
    /// </summary>
    /// <returns>200 OK with upcoming and past appointments arrays</returns>
    /// <response code="200">Appointments retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("my-appointments")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyAppointments()
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

            // Fetch all appointments for the patient
            var appointments = await _appointmentService.GetPatientAppointmentsAsync(patientId);

            // Separate into upcoming and past appointments
            var now = DateTime.UtcNow;
            var upcoming = appointments
                .Where(a => a.ScheduledDateTime > now && a.Status != "Cancelled")
                .OrderBy(a => a.ScheduledDateTime)
                .ToList();

            var past = appointments
                .Where(a => a.ScheduledDateTime <= now || a.Status == "Cancelled" || a.Status == "Completed")
                .OrderByDescending(a => a.ScheduledDateTime)
                .ToList();

            _logger.LogInformation(
                "Retrieved {UpcomingCount} upcoming and {PastCount} past appointments for Patient {PatientId}",
                upcoming.Count, past.Count, patientId);

            return Ok(new { upcoming, past });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for patient");
            return StatusCode(500, new { message = "An error occurred while retrieving appointments" });
        }
    }

    /// <summary>
    /// Downloads appointment confirmation PDF (US_028 - FR-012, AC-4).
    /// Returns PDF file for downloading from My Appointments or confirmation screen.
    /// Verifies ownership before allowing download.
    /// </summary>
    /// <param name="id">Appointment GUID</param>
    /// <returns>PDF file stream</returns>
    /// <response code="200">PDF file downloaded successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not own the appointment</response>
    /// <response code="404">PDF not available (generation failed or not yet generated)</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/confirmation-pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetConfirmationPdf([FromRoute] Guid id)
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

            // Fetch appointment and verify ownership
            var appointment = await _appointmentService.GetAppointmentByIdInternalAsync(id);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found", id);
                return NotFound(new { message = $"Appointment {id} not found" });
            }

            if (appointment.PatientId != patientId)
            {
                _logger.LogWarning(
                    "Unauthorized PDF download attempt: Appointment {AppointmentId} belongs to Patient {OwnerId}, not {RequesterId}",
                    id, appointment.PatientId, patientId);
                return Forbid();
            }

            // Check if PDF exists
            if (string.IsNullOrEmpty(appointment.PdfFilePath) || !System.IO.File.Exists(appointment.PdfFilePath))
            {
                _logger.LogWarning("PDF not available for Appointment {AppointmentId}", id);
                return NotFound(new { message = "PDF not available. It may still be generating. Please try again in a moment." });
            }

            // Read PDF bytes
            var pdfBytes = await System.IO.File.ReadAllBytesAsync(appointment.PdfFilePath);
            var fileName = $"Appointment_Confirmation_{appointment.ConfirmationNumber}.pdf";

            _logger.LogInformation(
                "PDF downloaded for Appointment {AppointmentId} by Patient {PatientId}",
                id, patientId);

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt for appointment {AppointmentId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading PDF for appointment {AppointmentId}", id);
            return StatusCode(500, new { message = "An error occurred while downloading the PDF" });
        }
    }

    /// <summary>
    /// Cancels swap preference for an appointment (US_026 - FR-010, AC4).
    /// Patient can cancel their preferred slot swap preference, maintaining original booking.
    /// Sets PreferredSlotId to null without affecting the scheduled appointment.
    /// </summary>
    /// <param name="id">Appointment GUID</param>
    /// <returns>200 OK if swap preference cancelled successfully</returns>
    /// <response code="200">Swap preference cancelled successfully</response>
    /// <response code="400">Appointment not found or invalid ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not own the appointment</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("{id:guid}/swap")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelSwapPreference([FromRoute] Guid id)
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

            // Cancel swap preference (AC4)
            var result = await _slotSwapService.CancelSwapPreferenceAsync(id, patientId);

            _logger.LogInformation(
                "Swap preference cancelled for Appointment {AppointmentId} by Patient {PatientId}",
                id, patientId);

            return Ok(new { message = "Swap preference cancelled successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "Unauthorized attempt to cancel swap preference for Appointment {AppointmentId}",
                id);
            return StatusCode(403, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid appointment ID: {AppointmentId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error cancelling swap preference for Appointment {AppointmentId}",
                id);
            return StatusCode(500, new { message = "An error occurred while cancelling swap preference" });
        }
    }

    /// <summary>
    /// Cancels an existing appointment (US_027 - FR-011, AC-1, AC-4).
    /// Enforces cancellation policy based on advance notice hours configuration.
    /// Releases time slot and updates appointment status to Cancelled.
    /// </summary>
    /// <param name="id">Appointment GUID</param>
    /// <returns>200 OK if cancellation succeeded</returns>
    /// <response code="200">Appointment cancelled successfully</response>
    /// <response code="400">Appointment not found or invalid ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Policy violation - cancellation not allowed within restricted window</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelAppointment([FromRoute] Guid id)
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

            // Cancel appointment (AC-1, AC-4)
            var result = await _appointmentService.CancelAsync(id, patientId);

            _logger.LogInformation(
                "Appointment {AppointmentId} cancelled by Patient {PatientId}",
                id, patientId);

            return Ok(new { message = "Appointment cancelled successfully" });
        }
        catch (PolicyViolationException ex)
        {
            // AC-4: Return 403 Forbidden when cancellation policy violated
            _logger.LogWarning(ex,
                "Cancellation policy violation for Appointment {AppointmentId}",
                id);
            return StatusCode(403, new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "Unauthorized attempt to cancel Appointment {AppointmentId}",
                id);
            return StatusCode(403, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid appointment cancellation request: {AppointmentId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error cancelling Appointment {AppointmentId}",
                id);
            return StatusCode(500, new { message = "An error occurred while cancelling the appointment" });
        }
    }

    /// <summary>
    /// Reschedules an existing appointment to a new time slot (US_027 - FR-011, AC-2, AC-3).
    /// Uses atomic transaction to release original slot and book new slot.
    /// </summary>
    /// <param name="id">Appointment GUID</param>
    /// <param name="request">Reschedule request with new time slot ID</param>
    /// <returns>200 OK with updated appointment details</returns>
    /// <response code="200">Appointment rescheduled successfully</response>
    /// <response code="400">Invalid request or appointment not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not own the appointment</response>
    /// <response code="409">Conflict - new time slot already booked</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("{id:guid}/reschedule")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RescheduleAppointment(
        [FromRoute] Guid id,
        [FromBody] RescheduleAppointmentRequest request)
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

                _logger.LogWarning("Invalid reschedule request: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Reschedule appointment (AC-2, AC-3)
            var updatedAppointment = await _appointmentService.RescheduleAsync(
                id,
                patientId,
                request.NewTimeSlotId);

            _logger.LogInformation(
                "Appointment {AppointmentId} rescheduled by Patient {PatientId} to TimeSlot {NewTimeSlotId}",
                id, patientId, request.NewTimeSlotId);

            return Ok(updatedAppointment);
        }
        catch (ConflictException ex)
        {
            // AC-3: Return 409 Conflict when new slot is already booked
            _logger.LogWarning(ex,
                "Reschedule conflict for Appointment {AppointmentId} to TimeSlot {NewTimeSlotId}",
                id, request.NewTimeSlotId);
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "Unauthorized attempt to reschedule Appointment {AppointmentId}",
                id);
            return StatusCode(403, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid reschedule request for Appointment {AppointmentId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error rescheduling Appointment {AppointmentId} to TimeSlot {NewTimeSlotId}",
                id, request.NewTimeSlotId);
            return StatusCode(500, new { message = "An error occurred while rescheduling the appointment" });
        }
    }

    /// <summary>
    /// Creates a walk-in appointment (US_029, AC-3).
    /// Staff-only endpoint for immediate appointment booking.
    /// Walk-in appointments default to Arrived status with IsWalkin flag.
    /// </summary>
    /// <param name="request">Walk-in appointment creation request</param>
    /// <returns>201 Created with appointment details</returns>
    /// <response code="201">Walk-in appointment created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="404">Patient, provider, or time slot not found</response>
    /// <response code="409">Time slot is no longer available (concurrent booking conflict)</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("walkin")]
    [Authorize(Policy = "StaffOnly")] // Only Staff and Admin can create walk-in appointments
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateWalkinAppointment([FromBody] CreateWalkinAppointmentDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                "Creating walk-in appointment for Patient {PatientId}, Provider {ProviderId}, TimeSlot {TimeSlotId}",
                request.PatientId, request.ProviderId, request.TimeSlotId);

            var appointment = await _appointmentService.CreateWalkinAppointmentAsync(request);

            _logger.LogInformation(
                "Walk-in appointment {AppointmentId} created successfully",
                appointment.Id);

            return CreatedAtAction(
                nameof(GetAppointmentById),
                new { id = appointment.Id },
                appointment);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex,
                "Conflict creating walk-in appointment for Patient {PatientId}, TimeSlot {TimeSlotId}",
                request.PatientId, request.TimeSlotId);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid walk-in appointment request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating walk-in appointment for Patient {PatientId}, TimeSlot {TimeSlotId}",
                request.PatientId, request.TimeSlotId);
            return StatusCode(500, new { message = "An error occurred while creating the walk-in appointment" });
        }
    }
}

/// <summary>
/// Request DTO for reschedule endpoint (US_027)
/// </summary>
public class RescheduleAppointmentRequest
{
    /// <summary>
    /// New time slot ID to reschedule to
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "NewTimeSlotId is required")]
    public Guid NewTimeSlotId { get; set; }
}
