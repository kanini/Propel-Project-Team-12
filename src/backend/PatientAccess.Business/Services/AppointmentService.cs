using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Appointment service implementation for US_024 - Appointment Booking API (FR-007, FR-008).
/// Implements availability calendar and appointment creation with pessimistic locking.
/// Targets 500ms P95 response time (NFR-001).
/// Extended with cancellation and rescheduling support (US_027 - FR-011).
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<AppointmentService> _logger;
    private readonly IConfiguration _configuration;
    private const int MaxRetries = 3;
    private const int InitialRetryDelayMs = 100;
    private const int DefaultCancellationNoticeHours = 24;

    public AppointmentService(
        PatientAccessDbContext context,
        ILogger<AppointmentService> logger,
        IConfiguration configuration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Retrieves monthly availability for a provider (FR-007, AC2).
    /// Returns dates with at least one available slot.
    /// </summary>
    public async Task<List<AvailabilityResponseDto>> GetMonthlyAvailabilityAsync(Guid providerId, int year, int month)
    {
        try
        {
            _logger.LogInformation(
                "Fetching monthly availability for Provider {ProviderId}, {Year}-{Month}",
                providerId, year, month);

            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            // Query time slots within month range
            var timeSlots = await _context.TimeSlots
                .AsNoTracking() // Read-only performance optimization
                .Where(ts => ts.ProviderId == providerId &&
                            ts.StartTime >= startDate &&
                            ts.StartTime < endDate)
                .OrderBy(ts => ts.StartTime)
                .Select(ts => new
                {
                    ts.TimeSlotId,
                    ts.StartTime,
                    ts.EndTime,
                    ts.IsBooked
                })
                .ToListAsync();

            // Group by date and build response
            var availabilityByDate = timeSlots
                .GroupBy(ts => ts.StartTime.Date)
                .Select(group => new AvailabilityResponseDto
                {
                    Date = group.Key,
                    TimeSlots = group.Select(ts => new TimeSlotDto
                    {
                        Id = ts.TimeSlotId,
                        StartTime = ts.StartTime,
                        EndTime = ts.EndTime,
                        IsBooked = ts.IsBooked
                    }).ToList()
                })
                .OrderBy(a => a.Date)
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} dates with availability for Provider {ProviderId}",
                availabilityByDate.Count, providerId);

            return availabilityByDate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching monthly availability for Provider {ProviderId}, {Year}-{Month}",
                providerId, year, month);
            throw;
        }
    }

    /// <summary>
    /// Retrieves daily availability for a provider (FR-007, AC2).
    /// Returns all time slots for the specified date.
    /// Must respond within 500ms at P95 (NFR-001).
    /// </summary>
    public async Task<AvailabilityResponseDto> GetDailyAvailabilityAsync(Guid providerId, DateTime date)
    {
        try
        {
            _logger.LogInformation(
                "Fetching daily availability for Provider {ProviderId}, Date {Date}",
                providerId, date.Date);

            var startDate = date.Date.ToUniversalTime();
            var endDate = startDate.AddDays(1);

            // Query time slots for specific date
            var timeSlots = await _context.TimeSlots
                .AsNoTracking() // Read-only performance optimization
                .Where(ts => ts.ProviderId == providerId &&
                            ts.StartTime >= startDate &&
                            ts.StartTime < endDate)
                .OrderBy(ts => ts.StartTime)
                .Select(ts => new TimeSlotDto
                {
                    Id = ts.TimeSlotId,
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    IsBooked = ts.IsBooked
                })
                .ToListAsync();

            _logger.LogInformation(
                "Retrieved {Count} time slots for Provider {ProviderId}, Date {Date}",
                timeSlots.Count, providerId, date.Date);

            return new AvailabilityResponseDto
            {
                Date = date.Date,
                TimeSlots = timeSlots
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching daily availability for Provider {ProviderId}, Date {Date}",
                providerId, date.Date);
            throw;
        }
    }

    /// <summary>
    /// Creates a new appointment with pessimistic locking (FR-008, AC3, AC4).
    /// Uses database-level concurrency check to prevent double-booking.
    /// Uses EF Core execution strategy for retry handling.
    /// </summary>
    public async Task<AppointmentResponseDto> CreateAppointmentAsync(Guid patientId, CreateAppointmentRequestDto request)
    {
        _logger.LogInformation(
            "Creating appointment for Patient {PatientId}, Provider {ProviderId}, TimeSlot {TimeSlotId}",
            patientId, request.ProviderId, request.TimeSlotId);

        // Validate request before transaction
        await ValidateAppointmentRequestAsync(request);

        // Create execution strategy to handle retries with transactions
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Use serializable transaction for strong isolation
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // Fetch and lock the time slot
                var timeSlot = await _context.TimeSlots
                    .Where(ts => ts.TimeSlotId == request.TimeSlotId)
                    .FirstOrDefaultAsync();

                if (timeSlot == null)
                {
                    throw new ArgumentException($"Time slot {request.TimeSlotId} does not exist", nameof(request.TimeSlotId));
                }

                // Check if slot is already booked (AC4 - conflict detection)
                if (timeSlot.IsBooked)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(
                        "Time slot {TimeSlotId} is already booked. Conflict detected.",
                        request.TimeSlotId);
                    throw new ConflictException($"Time slot {request.TimeSlotId} is no longer available");
                }

                // Mark time slot as booked
                timeSlot.IsBooked = true;
                timeSlot.UpdatedAt = DateTime.UtcNow;

                // Create appointment record
                var appointment = new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    PatientId = patientId,
                    ProviderId = request.ProviderId,
                    TimeSlotId = request.TimeSlotId,
                    ScheduledDateTime = timeSlot.StartTime,
                    Status = AppointmentStatus.Scheduled,
                    VisitReason = request.VisitReason,
                    PreferredSlotId = request.PreferredSlotId,
                    ConfirmationNumber = GenerateConfirmationNumber(),
                    IsWalkIn = false,
                    ConfirmationReceived = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Appointment {AppointmentId} created successfully with confirmation {ConfirmationNumber}",
                    appointment.AppointmentId, appointment.ConfirmationNumber);

                // Load provider name and specialty for response
                var provider = await _context.Providers
                    .AsNoTracking()
                    .Where(p => p.ProviderId == request.ProviderId)
                    .Select(p => new { p.Name, p.Specialty })
                    .FirstOrDefaultAsync();

                // Load preferred slot start time if preference is set
                DateTime? preferredSlotStartTime = null;
                if (appointment.PreferredSlotId.HasValue)
                {
                    preferredSlotStartTime = await _context.TimeSlots
                        .AsNoTracking()
                        .Where(ts => ts.TimeSlotId == appointment.PreferredSlotId.Value)
                        .Select(ts => ts.StartTime)
                        .FirstOrDefaultAsync();
                }

                return new AppointmentResponseDto
                {
                    Id = appointment.AppointmentId,
                    ProviderId = appointment.ProviderId,
                    ProviderName = provider?.Name ?? "Unknown",
                    ProviderSpecialty = provider?.Specialty ?? "General Practice",
                    ScheduledDateTime = appointment.ScheduledDateTime,
                    VisitReason = appointment.VisitReason,
                    Status = appointment.Status.ToString(),
                    ConfirmationNumber = appointment.ConfirmationNumber,
                    PreferredSlotId = appointment.PreferredSlotId,
                    PreferredSlotStartTime = preferredSlotStartTime
                };
            }
            catch (ConflictException)
            {
                await transaction.RollbackAsync();
                throw; // Don't retry conflicts
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Error creating appointment for Patient {PatientId}, TimeSlot {TimeSlotId}",
                    patientId, request.TimeSlotId);
                throw;
            }
        });
    }

    /// <summary>
    /// Validates appointment request data (AC3 - validation).
    /// </summary>
    private async Task ValidateAppointmentRequestAsync(CreateAppointmentRequestDto request)
    {
        // Validate provider exists
        var providerExists = await _context.Providers
            .AsNoTracking()
            .AnyAsync(p => p.ProviderId == request.ProviderId && p.IsActive);

        if (!providerExists)
        {
            throw new ArgumentException($"Provider {request.ProviderId} does not exist or is inactive", nameof(request.ProviderId));
        }

        // Validate time slot exists
        var timeSlotExists = await _context.TimeSlots
            .AsNoTracking()
            .AnyAsync(ts => ts.TimeSlotId == request.TimeSlotId && ts.ProviderId == request.ProviderId);

        if (!timeSlotExists)
        {
            throw new ArgumentException($"Time slot {request.TimeSlotId} does not exist for provider {request.ProviderId}", nameof(request.TimeSlotId));
        }

        // Validate preferred slot if specified
        if (request.PreferredSlotId.HasValue)
        {
            var preferredSlotExists = await _context.TimeSlots
                .AsNoTracking()
                .AnyAsync(ts => ts.TimeSlotId == request.PreferredSlotId.Value && ts.ProviderId == request.ProviderId);

            if (!preferredSlotExists)
            {
                throw new ArgumentException($"Preferred time slot {request.PreferredSlotId} does not exist for provider {request.ProviderId}", nameof(request.PreferredSlotId));
            }
        }

        // Visit reason length validation (already handled by data annotations, but double-check)
        if (string.IsNullOrWhiteSpace(request.VisitReason) || request.VisitReason.Length > 500)
        {
            throw new ArgumentException("VisitReason must be between 1 and 500 characters", nameof(request.VisitReason));
        }
    }

    /// <summary>
    /// Generates a unique 8-character alphanumeric confirmation number.
    /// Format: Uppercase letters and digits only (e.g., "A1B2C3D4").
    /// </summary>
    private string GenerateConfirmationNumber()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    /// <summary>
    /// Checks if exception is a deadlock exception.
    /// PostgreSQL deadlock_detected error code: 40P01
    /// </summary>
    private bool IsDeadlockException(Exception ex)
    {
        // Check for Npgsql.PostgresException with deadlock_detected SqlState
        var postgresException = ex as Npgsql.PostgresException;
        if (postgresException != null && postgresException.SqlState == "40P01")
        {
            return true;
        }

        // Check inner exceptions
        if (ex.InnerException != null)
        {
            return IsDeadlockException(ex.InnerException);
        }

        return false;
    }

    /// <summary>
    /// Cancels an existing appointment with cancellation policy enforcement (US_027 - FR-011, AC-1, AC-4).
    /// Validates cancellation window based on configurable advance notice hours.
    /// Releases time slot and updates appointment status to Cancelled.
    /// </summary>
    public async Task<bool> CancelAsync(Guid appointmentId, Guid patientId)
    {
        try
        {
            _logger.LogInformation(
                "Cancelling appointment {AppointmentId} for Patient {PatientId}",
                appointmentId, patientId);

            // Fetch appointment with related entities
            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .Where(a => a.AppointmentId == appointmentId)
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                throw new ArgumentException($"Appointment {appointmentId} not found", nameof(appointmentId));
            }

            // Verify ownership
            if (appointment.PatientId != patientId)
            {
                _logger.LogWarning(
                    "Unauthorized cancellation attempt: Appointment {AppointmentId} belongs to Patient {OwnerId}, not {RequesterId}",
                    appointmentId, appointment.PatientId, patientId);
                throw new UnauthorizedAccessException(
                    $"Appointment {appointmentId} does not belong to patient {patientId}");
            }

            // Check if already cancelled
            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                _logger.LogWarning("Appointment {AppointmentId} is already cancelled", appointmentId);
                throw new ArgumentException($"Appointment {appointmentId} is already cancelled", nameof(appointmentId));
            }

            // Enforce cancellation policy (AC-4)
            var cancellationNoticeHours = _configuration.GetSection("AppointmentSettings")?["CancellationNoticeHours"] != null
                ? int.Parse(_configuration.GetSection("AppointmentSettings")["CancellationNoticeHours"]!)
                : DefaultCancellationNoticeHours;

            var hoursUntilAppointment = (appointment.ScheduledDateTime - DateTime.UtcNow).TotalHours;

            if (hoursUntilAppointment < cancellationNoticeHours)
            {
                _logger.LogWarning(
                    "Cancellation policy violation: Appointment {AppointmentId} is in {Hours} hours, requires {Required} hours notice",
                    appointmentId, hoursUntilAppointment, cancellationNoticeHours);
                throw new PolicyViolationException(
                    $"Appointments must be cancelled at least {cancellationNoticeHours} hours in advance. " +
                    $"This appointment is in {hoursUntilAppointment:F1} hours.");
            }

            // Update appointment status
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;

            // Release time slot (AC-1)
            var timeSlot = appointment.TimeSlot;
            if (timeSlot != null)
            {
                timeSlot.IsBooked = false;
                timeSlot.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully cancelled appointment {AppointmentId}. Time slot {TimeSlotId} released.",
                appointmentId, timeSlot?.TimeSlotId);

            // TODO: Send cancellation confirmation notification
            // await _notificationService.SendCancellationConfirmationAsync(appointment);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error cancelling appointment {AppointmentId} for Patient {PatientId}",
                appointmentId, patientId);
            throw;
        }
    }

    /// <summary>
    /// Reschedules an existing appointment to a new time slot (US_027 - FR-011, AC-2, AC-3).
    /// Uses atomic transaction to release original slot and book new slot.
    /// Implements pessimistic locking to prevent double-booking.
    /// </summary>
    public async Task<AppointmentResponseDto> RescheduleAsync(Guid appointmentId, Guid patientId, Guid newTimeSlotId)
    {
        var retryCount = 0;
        var retryDelay = InitialRetryDelayMs;

        while (retryCount < MaxRetries)
        {
            try
            {
                _logger.LogInformation(
                    "Rescheduling appointment {AppointmentId} for Patient {PatientId} to TimeSlot {NewTimeSlotId} (Attempt {Attempt})",
                    appointmentId, patientId, newTimeSlotId, retryCount + 1);

                var strategy = _context.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                // Start transaction with serializable isolation for atomic operations (AC-3)
                using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                try
                {
                    // Fetch appointment with provider
                    var appointment = await _context.Appointments
                        .Include(a => a.Provider)
                        .Where(a => a.AppointmentId == appointmentId)
                        .FirstOrDefaultAsync();

                    if (appointment == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                        throw new ArgumentException($"Appointment {appointmentId} not found", nameof(appointmentId));
                    }

                    // Verify ownership
                    if (appointment.PatientId != patientId)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(
                            "Unauthorized reschedule attempt: Appointment {AppointmentId} belongs to Patient {OwnerId}, not {RequesterId}",
                            appointmentId, appointment.PatientId, patientId);
                        throw new UnauthorizedAccessException(
                            $"Appointment {appointmentId} does not belong to patient {patientId}");
                    }

                    // Check if appointment is in a state that allows rescheduling
                    if (appointment.Status == AppointmentStatus.Cancelled ||
                        appointment.Status == AppointmentStatus.Completed)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(
                            "Appointment {AppointmentId} cannot be rescheduled. Current status: {Status}",
                            appointmentId, appointment.Status);
                        throw new ArgumentException(
                            $"Appointment cannot be rescheduled. Current status: {appointment.Status}",
                            nameof(appointmentId));
                    }

                    // Fetch original time slot
                    var originalSlot = await _context.TimeSlots
                        .Where(ts => ts.TimeSlotId == appointment.TimeSlotId)
                        .FirstOrDefaultAsync();

                    // Fetch new time slot
                    var newSlot = await _context.TimeSlots
                        .Where(ts => ts.TimeSlotId == newTimeSlotId)
                        .FirstOrDefaultAsync();

                    if (newSlot == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("New time slot {TimeSlotId} not found", newTimeSlotId);
                        throw new ArgumentException($"New time slot {newTimeSlotId} not found", nameof(newTimeSlotId));
                    }

                    // Verify new slot belongs to same provider
                    if (newSlot.ProviderId != appointment.ProviderId)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(
                            "New time slot {TimeSlotId} belongs to different provider {NewProviderId}, expected {OriginalProviderId}",
                            newTimeSlotId, newSlot.ProviderId, appointment.ProviderId);
                        throw new ArgumentException(
                            $"New time slot must be with the same provider ({appointment.Provider.Name})",
                            nameof(newTimeSlotId));
                    }

                    // Check if new slot is available (AC-3 - conflict handling)
                    if (newSlot.IsBooked)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(
                            "New time slot {TimeSlotId} is already booked. Conflict detected.",
                            newTimeSlotId);
                        throw new ConflictException($"Time slot {newTimeSlotId} is no longer available");
                    }

                    // Execute atomic reschedule (AC-3)
                    // 1. Release original slot
                    if (originalSlot != null)
                    {
                        originalSlot.IsBooked = false;
                        originalSlot.UpdatedAt = DateTime.UtcNow;
                    }

                    // 2. Book new slot
                    newSlot.IsBooked = true;
                    newSlot.UpdatedAt = DateTime.UtcNow;

                    // 3. Update appointment
                    appointment.TimeSlotId = newTimeSlotId;
                    appointment.ScheduledDateTime = newSlot.StartTime;
                    appointment.UpdatedAt = DateTime.UtcNow;
                    // Clear preferred slot if it was set
                    appointment.PreferredSlotId = null;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Successfully rescheduled appointment {AppointmentId} from slot {OriginalSlotId} to {NewSlotId}",
                        appointmentId, originalSlot?.TimeSlotId, newTimeSlotId);

                    // TODO: Send reschedule confirmation notification
                    // await _notificationService.SendRescheduleConfirmationAsync(appointment);

                    // Return updated appointment details
                    return new AppointmentResponseDto
                    {
                        Id = appointment.AppointmentId,
                        ProviderId = appointment.ProviderId,
                        ProviderName = appointment.Provider.Name,
                        ScheduledDateTime = appointment.ScheduledDateTime,
                        VisitReason = appointment.VisitReason,
                        Status = appointment.Status.ToString(),
                        ConfirmationNumber = appointment.ConfirmationNumber,
                        PreferredSlotId = appointment.PreferredSlotId
                    };
                }
                catch (ConflictException)
                {
                    await transaction.RollbackAsync();
                    throw; // Rethrow conflict exceptions immediately (no retry)
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                }); // end strategy.ExecuteAsync
            }
            catch (DbUpdateException ex) when (IsDeadlockException(ex))
            {
                retryCount++;
                if (retryCount >= MaxRetries)
                {
                    _logger.LogError(ex,
                        "Deadlock retry limit reached for rescheduling Appointment {AppointmentId} to TimeSlot {NewTimeSlotId}",
                        appointmentId, newTimeSlotId);
                    throw new InvalidOperationException(
                        $"Unable to complete rescheduling after {MaxRetries} attempts due to high concurrency. Please try again.",
                        ex);
                }

                _logger.LogWarning(
                    "Deadlock detected during reschedule. Retrying in {Delay}ms (Attempt {Attempt}/{MaxRetries})",
                    retryDelay, retryCount + 1, MaxRetries);

                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff
            }
        }

        throw new InvalidOperationException("Unexpected retry loop exit");
    }

    /// <summary>
    /// Retrieves all appointments for a patient (FR-011, US_027).
    /// Used by My Appointments page to display upcoming and past appointments.
    /// </summary>
    public async Task<List<AppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientId)
    {
        try
        {
            _logger.LogInformation("Fetching all appointments for Patient {PatientId}", patientId);

            // Fetch all appointments for the patient with provider details
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Provider)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.ScheduledDateTime)
                .Select(a => new AppointmentResponseDto
                {
                    Id = a.AppointmentId,
                    ProviderId = a.ProviderId,
                    ProviderName = a.Provider.Name,
                    ProviderSpecialty = a.Provider.Specialty,
                    ScheduledDateTime = a.ScheduledDateTime,
                    VisitReason = a.VisitReason,
                    Status = a.Status.ToString(),
                    ConfirmationNumber = a.ConfirmationNumber,
                    PreferredSlotId = a.PreferredSlotId
                })
                .ToListAsync();

            _logger.LogInformation(
                "Retrieved {Count} appointments for Patient {PatientId}",
                appointments.Count, patientId);

            return appointments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for Patient {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves appointment entity by ID for internal use (US_028).
    /// Includes navigation properties for PDF generation and verification.
    /// </summary>
    public async Task<Appointment?> GetAppointmentByIdInternalAsync(Guid appointmentId)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    /// <summary>
    /// Creates a walk-in appointment with IsWalkin flag and Arrived status (US_029, AC-3).
    /// Staff-only operation for immediate appointment booking.
    /// Uses pessimistic locking to prevent double-booking.
    /// </summary>
    public async Task<AppointmentResponseDto> CreateWalkinAppointmentAsync(CreateWalkinAppointmentDto request)
    {
        var retryCount = 0;
        var retryDelay = InitialRetryDelayMs;

        while (retryCount < MaxRetries)
        {
            try
            {
                _logger.LogInformation(
                    "Creating walk-in appointment for Patient {PatientId}, Provider {ProviderId}, TimeSlot {TimeSlotId} (Attempt {Attempt})",
                    request.PatientId, request.ProviderId, request.TimeSlotId, retryCount + 1);

                // Validate request
                await ValidateWalkinAppointmentRequestAsync(request);

                var strategy = _context.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                // Start transaction with serializable isolation for concurrency control
                using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                try
                {
                    // Fetch and lock the time slot
                    var timeSlot = await _context.TimeSlots
                        .Where(ts => ts.TimeSlotId == request.TimeSlotId)
                        .FirstOrDefaultAsync();

                    if (timeSlot == null)
                    {
                        throw new ArgumentException($"Time slot {request.TimeSlotId} does not exist", nameof(request.TimeSlotId));
                    }

                    // Check if slot is already booked (AC4 - conflict detection)
                    if (timeSlot.IsBooked)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(
                            "Time slot {TimeSlotId} is already booked. Conflict detected.",
                            request.TimeSlotId);
                        throw new ConflictException($"Time slot {request.TimeSlotId} is no longer available");
                    }

                    // Mark time slot as booked
                    timeSlot.IsBooked = true;
                    timeSlot.UpdatedAt = DateTime.UtcNow;

                    // Create walk-in appointment record
                    var appointment = new Appointment
                    {
                        AppointmentId = Guid.NewGuid(),
                        PatientId = request.PatientId,
                        ProviderId = request.ProviderId,
                        TimeSlotId = request.TimeSlotId,
                        ScheduledDateTime = timeSlot.StartTime,
                        Status = AppointmentStatus.Arrived, // Walk-ins default to Arrived status
                        VisitReason = request.VisitReason,
                        IsWalkIn = true, // Flag for walk-in appointments
                        ConfirmationReceived = true, // Walk-ins are considered confirmed (staff-initiated)
                        NoShowRiskScore = 0, // Zero risk for walk-ins (already arrived)
                        ConfirmationNumber = GenerateConfirmationNumber(),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Walk-in appointment {AppointmentId} created successfully with confirmation {ConfirmationNumber}",
                        appointment.AppointmentId, appointment.ConfirmationNumber);

                    // Load provider name for response
                    var provider = await _context.Providers
                        .AsNoTracking()
                        .Where(p => p.ProviderId == request.ProviderId)
                        .Select(p => p.Name)
                        .FirstOrDefaultAsync();

                    return new AppointmentResponseDto
                    {
                        Id = appointment.AppointmentId,
                        ProviderId = appointment.ProviderId,
                        ProviderName = provider ?? "Unknown",
                        ScheduledDateTime = appointment.ScheduledDateTime,
                        VisitReason = appointment.VisitReason,
                        Status = appointment.Status.ToString(),
                        ConfirmationNumber = appointment.ConfirmationNumber,
                        PreferredSlotId = null,
                        PreferredSlotStartTime = null
                    };
                }
                catch (ConflictException)
                {
                    await transaction.RollbackAsync();
                    throw; // Rethrow conflict exceptions immediately (no retry)
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw; // Rethrow all exceptions; deadlock retry handled by outer loop
                }
                }); // end strategy.ExecuteAsync
            }
            catch (ConflictException)
            {
                throw; // Don't retry conflicts
            }
            catch (ArgumentException)
            {
                throw; // Don't retry validation errors
            }
            catch (DbUpdateException ex) when (IsDeadlockException(ex))
            {
                retryCount++;
                if (retryCount >= MaxRetries)
                {
                    _logger.LogError(ex,
                        "Deadlock detected after {RetryCount} retries for Patient {PatientId}, TimeSlot {TimeSlotId}",
                        retryCount, request.PatientId, request.TimeSlotId);
                    throw new InvalidOperationException(
                        $"Unable to complete walk-in appointment booking after {MaxRetries} attempts due to high concurrency. Please try again.",
                        ex);
                }

                _logger.LogWarning(
                    "Deadlock detected on attempt {Attempt} for Patient {PatientId}, TimeSlot {TimeSlotId}. Retrying in {Delay}ms...",
                    retryCount, request.PatientId, request.TimeSlotId, retryDelay);

                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating walk-in appointment for Patient {PatientId}, TimeSlot {TimeSlotId}",
                    request.PatientId, request.TimeSlotId);
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected retry loop exit");
    }

    /// <summary>
    /// Validates walk-in appointment request data (US_029, AC-3).
    /// </summary>
    private async Task ValidateWalkinAppointmentRequestAsync(CreateWalkinAppointmentDto request)
    {
        // Validate patient exists and is active
        var patientExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserId == request.PatientId &&
                          u.Role == UserRole.Patient &&
                          u.Status == UserStatus.Active);

        if (!patientExists)
        {
            throw new ArgumentException($"Patient {request.PatientId} does not exist or is inactive", nameof(request.PatientId));
        }

        // Validate provider exists
        var providerExists = await _context.Providers
            .AsNoTracking()
            .AnyAsync(p => p.ProviderId == request.ProviderId && p.IsActive);

        if (!providerExists)
        {
            throw new ArgumentException($"Provider {request.ProviderId} does not exist or is inactive", nameof(request.ProviderId));
        }

        // Validate time slot exists for provider
        var timeSlotExists = await _context.TimeSlots
            .AsNoTracking()
            .AnyAsync(ts => ts.TimeSlotId == request.TimeSlotId && ts.ProviderId == request.ProviderId);

        if (!timeSlotExists)
        {
            throw new ArgumentException($"Time slot {request.TimeSlotId} does not exist for provider {request.ProviderId}", nameof(request.TimeSlotId));
        }

        // Visit reason length validation (already handled by data annotations, but double-check)
        if (string.IsNullOrWhiteSpace(request.VisitReason) || request.VisitReason.Length > 500)
        {
            throw new ArgumentException("VisitReason must be between 1 and 500 characters", nameof(request.VisitReason));
        }
    }
}

