using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Staff controller for healthcare provider operations (US_020, US_029).
/// Endpoints accessible by Staff and Admin roles.
/// </summary>
[ApiController]
[Route("api/staff")]
[Authorize(Policy = "StaffOnly")] // Accessible by Staff and Admin
public class StaffController : ControllerBase
{
    private readonly ILogger<StaffController> _logger;
    private readonly IPatientService _patientService;
    private readonly IQueueManagementService _queueManagementService;
    private readonly IArrivalManagementService _arrivalManagementService;
    private readonly IStaffDashboardService _staffDashboardService;

    public StaffController(
        ILogger<StaffController> logger,
        IPatientService patientService,
        IQueueManagementService queueManagementService,
        IArrivalManagementService arrivalManagementService,
        IStaffDashboardService staffDashboardService)
    {
        _logger = logger;
        _patientService = patientService;
        _queueManagementService = queueManagementService;
        _arrivalManagementService = arrivalManagementService;
        _staffDashboardService = staffDashboardService;
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
    /// Searches for patients by name, email, or phone (US_029, AC-1 / US_032, AC-1,2).
    /// Supports special characters (accents, hyphens, apostrophes) in search queries.
    /// Results sorted by relevance: exact email match > name starts-with > last appointment date.
    /// </summary>
    /// <param name="query">Search term (2-100 characters, letters/numbers/spaces/hyphens/accents/@/.)</param>
    /// <returns>List of matching patient records (max 20 results)</returns>
    /// <response code="200">Search completed successfully</response>
    /// <response code="400">Invalid search query (too short, too long, or contains invalid characters)</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service temporarily unavailable (database timeout or connection error)</response>
    [HttpGet("patients/search")]
    [ProducesResponseType(typeof(List<PatientSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SearchPatients([FromQuery] string query)
    {
        try
        {
            // Validate query length (minimum 2 characters)
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            {
                return BadRequest(new
                {
                    error = "Search query too short",
                    message = "Search query must be at least 2 characters",
                    query
                });
            }

            // Validate query length (maximum 100 characters to prevent abuse)
            if (query.Length > 100)
            {
                return BadRequest(new
                {
                    error = "Search query too long",
                    message = "Search query must not exceed 100 characters",
                    query = query.Substring(0, 50) + "..."
                });
            }

            // Validate allowed characters: letters (including accents), numbers, spaces, hyphens, apostrophes, @ and .
            // Regex: allows Unicode letters (À-ÿ for accents), digits, spaces, hyphens, apostrophes, @ and period
            var allowedCharactersPattern = @"^[a-zA-Z0-9\s\-'À-ÿ@.]+$";
            if (!Regex.IsMatch(query, allowedCharactersPattern))
            {
                return BadRequest(new
                {
                    error = "Invalid characters in search query",
                    message = "Only letters, numbers, spaces, hyphens, apostrophes, accents, @ and . are allowed",
                    query
                });
            }

            var results = await _patientService.SearchPatientsAsync(query);

            _logger.LogInformation(
                "Patient search completed: Query={Query}, ResultCount={Count}",
                query, results.Count);

            return Ok(results);
        }
        catch (DbException dbEx)
        {
            // Database connection or timeout errors
            _logger.LogError(dbEx, "Database error while searching patients: {Query}", query);
            return StatusCode(503, new
            {
                error = "Service temporarily unavailable",
                message = "Database connection error. Please try again in a moment."
            });
        }
        catch (TimeoutException timeoutEx)
        {
            // Query timeout errors
            _logger.LogError(timeoutEx, "Timeout error while searching patients: {Query}", query);
            return StatusCode(503, new
            {
                error = "Search timeout",
                message = "Search took too long. Please try a more specific query."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients: {Query}", query);
            return StatusCode(500, new
            {
                error = "An error occurred while searching patients",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Creates a minimal patient record for walk-in registration (US_029, AC-2).
    /// If email is provided and already exists, returns existing patient record.
    /// </summary>
    /// <param name="dto">Minimal patient creation data</param>
    /// <returns>Created or existing patient record</returns>
    /// <response code="201">Patient created successfully</response>
    /// <response code="200">Patient with email already exists (returns existing)</response>
    /// <response code="400">Validation error</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("patients")]
    [ProducesResponseType(typeof(PatientSearchResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PatientSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateMinimalPatient([FromBody] CreateMinimalPatientDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _patientService.CreateMinimalPatientAsync(dto);

            // Check if patient already existed (has email and matches input email)
            var isExisting = !string.IsNullOrWhiteSpace(dto.Email) &&
                            result.Email?.Equals(dto.Email, StringComparison.OrdinalIgnoreCase) == true;

            if (isExisting)
            {
                _logger.LogInformation(
                    "Returned existing patient: {PatientId}, Email={Email}",
                    result.Id, result.Email);
                return Ok(result);
            }

            _logger.LogInformation(
                "Created new minimal patient: {PatientId}, Name={Name}",
                result.Id, result.FullName);

            return CreatedAtAction(
                nameof(SearchPatients),
                new { query = result.FullName },
                result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating patient");
            return BadRequest(new
            {
                error = "Validation error",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating minimal patient");
            return StatusCode(500, new
            {
                error = "An error occurred while creating patient",
                message = ex.Message
            });
        }
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

    /// <summary>
    /// Get same-day patient queue with "Arrived" status (US_030, AC-1).
    /// </summary>
    /// <param name="providerId">Optional provider ID filter (null for all providers)</param>
    /// <returns>List of queue patients ordered by priority and arrival time</returns>
    /// <response code="200">Queue retrieved successfully</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("queue")]
    [ProducesResponseType(typeof(List<QueuePatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSameDayQueue([FromQuery] Guid? providerId = null)
    {
        try
        {
            _logger.LogInformation("Fetching same-day queue. ProviderId={ProviderId}", providerId);

            var queue = await _queueManagementService.GetSameDayQueueAsync(providerId);

            _logger.LogInformation("Queue retrieved successfully. Count={Count}", queue.Count);

            return Ok(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching same-day queue. ProviderId={ProviderId}", providerId);
            return StatusCode(500, new
            {
                error = "An error occurred while fetching the queue",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Update patient priority flag in the queue (US_030, AC-3).
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="dto">Priority update data</param>
    /// <returns>Updated queue patient data</returns>
    /// <response code="200">Priority updated successfully</response>
    /// <response code="400">Invalid request - appointment not in "Arrived" status</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="404">Appointment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("queue/{id}/priority")]
    [ProducesResponseType(typeof(QueuePatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePatientPriority(Guid id, [FromBody] UpdatePriorityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Updating priority for appointment {AppointmentId}. IsPriority={IsPriority}",
                id, dto.IsPriority);

            var updatedPatient = await _queueManagementService.UpdatePatientPriorityAsync(id, dto.IsPriority);

            _logger.LogInformation("Priority updated successfully for appointment {AppointmentId}", id);

            return Ok(updatedPatient);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Appointment {AppointmentId} not found", id);
            return NotFound(new
            {
                error = "Appointment not found",
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for appointment {AppointmentId}", id);
            return BadRequest(new
            {
                error = "Invalid request",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating priority for appointment {AppointmentId}", id);
            return StatusCode(500, new
            {
                error = "An error occurred while updating priority",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Search for appointments scheduled for today matching the query (US_031, AC-1).
    /// <summary>
    /// Search for today's appointments or retrieve all appointments for specified date (US_031, AC-1).
    /// If query is omitted or empty, returns all appointments for the date.
    /// </summary>
    /// <param name="query">Optional search term (patient name, email, or phone). Returns all if empty.</param>
    /// <param name="date">Optional date to search (defaults to today)</param>
    /// <returns>List of matching appointments</returns>
    /// <response code="200">Appointments found</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("arrivals/search")]
    [ProducesResponseType(typeof(List<ArrivalSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchTodayAppointments([FromQuery] string? query = null, [FromQuery] DateTime? date = null)
    {
        try
        {
            _logger.LogInformation("Searching appointments. Query={Query}, Date={Date}", query ?? "(all)", date);

            var results = await _arrivalManagementService.SearchTodayAppointmentsAsync(query, date);

            _logger.LogInformation("Search completed. Found {Count} appointments", results.Count);

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching appointments. Query={Query}", query);
            return StatusCode(500, new
            {
                error = "An error occurred while searching appointments",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Mark an appointment as arrived and add patient to queue (US_031, AC-2).
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Updated appointment data</returns>
    /// <response code="200">Appointment marked as arrived successfully</response>
    /// <response code="400">Invalid request - appointment cannot be marked (e.g., cancelled)</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="404">Appointment not found</response>
    /// <response code="409">Appointment already marked as arrived</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("arrivals/{id}/mark-arrived")]
    [ProducesResponseType(typeof(ArrivalSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAppointmentArrived(Guid id)
    {
        try
        {
            _logger.LogInformation("Marking appointment {AppointmentId} as arrived", id);

            var result = await _arrivalManagementService.MarkAppointmentArrivedAsync(id);

            _logger.LogInformation("Appointment {AppointmentId} marked as arrived successfully", id);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Appointment {AppointmentId} not found", id);
            return NotFound(new
            {
                error = "Appointment not found",
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for appointment {AppointmentId}", id);

            // Check if it's an "already arrived" case (return 409) or other invalid operation (return 400)
            if (ex.Message.Contains("already marked as arrived"))
            {
                return Conflict(new
                {
                    error = "Appointment already arrived",
                    message = ex.Message
                });
            }

            return BadRequest(new
            {
                error = "Invalid request",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking appointment {AppointmentId} as arrived", id);
            return StatusCode(500, new
            {
                error = "An error occurred while marking appointment as arrived",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get staff dashboard metrics (US_068, AC2).
    /// Returns stat cards data: today's appointments, queue size, pending verifications.
    /// </summary>
    /// <returns>Dashboard metrics</returns>
    /// <response code="200">Metrics retrieved successfully</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("dashboard/metrics")]
    [ProducesResponseType(typeof(StaffDashboardMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        try
        {
            _logger.LogInformation("Fetching staff dashboard metrics");

            var metrics = await _staffDashboardService.GetDashboardMetricsAsync();

            _logger.LogInformation("Dashboard metrics retrieved successfully");

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch staff dashboard metrics");
            return StatusCode(500, new
            {
                error = "Failed to fetch dashboard metrics",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get queue preview for dashboard (US_068, AC4).
    /// Returns next N patients in the queue with appointment details.
    /// </summary>
    /// <param name="count">Number of patients to return (default: 5)</param>
    /// <returns>Queue preview list</returns>
    /// <response code="200">Queue preview retrieved successfully</response>
    /// <response code="403">Insufficient permissions - Staff or Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("dashboard/queue-preview")]
    [ProducesResponseType(typeof(List<QueuePreviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQueuePreview([FromQuery] int count = 5)
    {
        try
        {
            _logger.LogInformation("Fetching queue preview. Count={Count}", count);

            var queue = await _staffDashboardService.GetQueuePreviewAsync(count);

            _logger.LogInformation("Queue preview retrieved successfully. Count={Count}", queue.Count);

            return Ok(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch queue preview");
            return StatusCode(500, new
            {
                error = "Failed to fetch queue preview",
                message = ex.Message
            });
        }
    }
}

