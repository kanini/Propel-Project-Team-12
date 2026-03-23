using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Slot swap service implementation for US_026 - Dynamic Preferred Slot Swap (FR-010).
/// Handles automatic appointment swapping when preferred slots become available.
/// Uses pessimistic locking to prevent race conditions during concurrent swap attempts.
/// </summary>
public class SlotSwapService : ISlotSwapService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<SlotSwapService> _logger;
    private const int MaxRetries = 3;
    private const int InitialRetryDelayMs = 100;

    public SlotSwapService(PatientAccessDbContext context, ILogger<SlotSwapService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes atomic swap of appointment from current time slot to preferred time slot (AC2).
    /// Uses pessimistic locking (SELECT FOR UPDATE) to prevent race conditions (AC3).
    /// Implements retry logic for deadlock scenarios.
    /// </summary>
    public async Task<bool> ExecuteSwapAsync(Guid appointmentId, Guid preferredSlotId)
    {
        var retryCount = 0;
        var retryDelay = InitialRetryDelayMs;

        while (retryCount < MaxRetries)
        {
            try
            {
                _logger.LogInformation(
                    "Executing swap for Appointment {AppointmentId} to PreferredSlot {PreferredSlotId} (Attempt {Attempt})",
                    appointmentId, preferredSlotId, retryCount + 1);

                var strategy = _context.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                // Start transaction for atomic operations
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Lock and fetch appointment (pessimistic locking)
                    var appointment = await _context.Appointments
                        .FromSqlRaw(@"
                            SELECT * FROM ""Appointments"" 
                            WHERE ""AppointmentId"" = {0} 
                            FOR UPDATE", appointmentId)
                        .Include(a => a.TimeSlot)
                        .Include(a => a.PreferredSlot)
                        .FirstOrDefaultAsync();

                    if (appointment == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                        throw new ArgumentException($"Appointment {appointmentId} not found", nameof(appointmentId));
                    }

                    // Validate appointment is in swappable state
                    if (appointment.Status != AppointmentStatus.Scheduled &&
                        appointment.Status != AppointmentStatus.Confirmed)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(
                            "Appointment {AppointmentId} is in non-swappable state: {Status}",
                            appointmentId, appointment.Status);
                        throw new InvalidOperationException(
                            $"Appointment {appointmentId} cannot be swapped in status {appointment.Status}");
                    }

                    // Lock and fetch preferred slot (pessimistic locking)
                    var preferredSlot = await _context.TimeSlots
                        .FromSqlRaw(@"
                            SELECT * FROM ""TimeSlots"" 
                            WHERE ""TimeSlotId"" = {0} 
                            FOR UPDATE", preferredSlotId)
                        .FirstOrDefaultAsync();

                    if (preferredSlot == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("Preferred slot {PreferredSlotId} not found", preferredSlotId);
                        throw new ArgumentException($"Preferred slot {preferredSlotId} not found", nameof(preferredSlotId));
                    }

                    // Check if preferred slot is still available (race condition check - AC3)
                    if (preferredSlot.IsBooked)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogInformation(
                            "Preferred slot {PreferredSlotId} is already booked. Swap failed gracefully.",
                            preferredSlotId);
                        // Swap failure is not an error - just means someone else booked it first
                        return false;
                    }

                    // Lock and fetch original slot
                    var originalSlot = await _context.TimeSlots
                        .FromSqlRaw(@"
                            SELECT * FROM ""TimeSlots"" 
                            WHERE ""TimeSlotId"" = {0} 
                            FOR UPDATE", appointment.TimeSlotId)
                        .FirstOrDefaultAsync();

                    if (originalSlot == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Original slot {TimeSlotId} not found", appointment.TimeSlotId);
                        throw new InvalidOperationException($"Original slot {appointment.TimeSlotId} not found");
                    }

                    // Execute atomic swap (AC2, AC5)
                    // 1. Release original slot
                    originalSlot.IsBooked = false;
                    originalSlot.UpdatedAt = DateTime.UtcNow;

                    // 2. Book preferred slot
                    preferredSlot.IsBooked = true;
                    preferredSlot.UpdatedAt = DateTime.UtcNow;

                    // 3. Update appointment to point to new slot
                    appointment.TimeSlotId = preferredSlotId;
                    appointment.ScheduledDateTime = preferredSlot.StartTime;
                    appointment.PreferredSlotId = null; // Clear preference after successful swap
                    appointment.UpdatedAt = DateTime.UtcNow;

                    // Save all changes atomically
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Successfully swapped Appointment {AppointmentId} from slot {OriginalSlotId} to {PreferredSlotId}",
                        appointmentId, originalSlot.TimeSlotId, preferredSlotId);

                    // TODO: Send swap confirmation notification to patient
                    // await _notificationService.SendSwapConfirmationAsync(appointment.PatientId, appointment);

                    return true;
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
                        "Deadlock retry limit reached for swap Appointment {AppointmentId} to PreferredSlot {PreferredSlotId}",
                        appointmentId, preferredSlotId);
                    throw;
                }

                _logger.LogWarning(
                    "Deadlock detected during swap. Retrying in {Delay}ms (Attempt {Attempt}/{MaxRetries})",
                    retryDelay, retryCount + 1, MaxRetries);

                await Task.Delay(retryDelay);
                retryDelay *= 2; // Exponential backoff
            }
        }

        return false;
    }

    /// <summary>
    /// Detects available slots with pending swap requests and processes them (AC2).
    /// Called by background job (SlotAvailabilityMonitor) every 5 minutes.
    /// Processes swaps in FIFO order based on CreatedAt timestamp.
    /// </summary>
    public async Task<int> ProcessPendingSwapsAsync()
    {
        try
        {
            _logger.LogInformation("Starting pending swaps processing");

            // Find all available slots that have pending swap preferences
            // Uses partial index: IX_Appointments_PreferredSlotId_Status_CreatedAt
            var availableSlotsWithSwaps = await _context.TimeSlots
                .AsNoTracking()
                .Where(ts => !ts.IsBooked)
                .Where(ts => _context.Appointments.Any(a =>
                    a.PreferredSlotId == ts.TimeSlotId &&
                    (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)))
                .Select(ts => ts.TimeSlotId)
                .ToListAsync();

            _logger.LogInformation(
                "Found {Count} available slots with pending swap preferences",
                availableSlotsWithSwaps.Count);

            int successfulSwaps = 0;

            foreach (var slotId in availableSlotsWithSwaps)
            {
                // Get first appointment with swap preference for this slot (FIFO - AC2)
                var appointmentToSwap = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.PreferredSlotId == slotId &&
                               (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed))
                    .OrderBy(a => a.CreatedAt) // FIFO - first booked gets priority
                    .Select(a => a.AppointmentId)
                    .FirstOrDefaultAsync();

                if (appointmentToSwap != Guid.Empty)
                {
                    try
                    {
                        var swapSucceeded = await ExecuteSwapAsync(appointmentToSwap, slotId);
                        if (swapSucceeded)
                        {
                            successfulSwaps++;
                            _logger.LogInformation(
                                "Successfully processed swap for Appointment {AppointmentId} to Slot {SlotId}",
                                appointmentToSwap, slotId);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing other swaps
                        _logger.LogError(ex,
                            "Failed to process swap for Appointment {AppointmentId} to Slot {SlotId}",
                            appointmentToSwap, slotId);
                    }
                }
            }

            _logger.LogInformation(
                "Completed pending swaps processing. Successful swaps: {Count}",
                successfulSwaps);

            return successfulSwaps;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during pending swaps processing");
            throw;
        }
    }

    /// <summary>
    /// Cancels swap preference for an appointment (AC4).
    /// Maintains original booking unchanged.
    /// </summary>
    public async Task<bool> CancelSwapPreferenceAsync(Guid appointmentId, Guid patientId)
    {
        try
        {
            _logger.LogInformation(
                "Cancelling swap preference for Appointment {AppointmentId}, Patient {PatientId}",
                appointmentId, patientId);

            var appointment = await _context.Appointments
                .Where(a => a.AppointmentId == appointmentId && a.PatientId == patientId)
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                _logger.LogWarning(
                    "Appointment {AppointmentId} not found or not owned by Patient {PatientId}",
                    appointmentId, patientId);
                throw new UnauthorizedAccessException(
                    $"Appointment {appointmentId} not found or not owned by patient {patientId}");
            }

            if (appointment.PreferredSlotId == null)
            {
                _logger.LogInformation(
                    "Appointment {AppointmentId} has no swap preference to cancel",
                    appointmentId);
                return true; // Already no preference set
            }

            // Clear swap preference (AC4)
            appointment.PreferredSlotId = null;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully cancelled swap preference for Appointment {AppointmentId}",
                appointmentId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error cancelling swap preference for Appointment {AppointmentId}, Patient {PatientId}",
                appointmentId, patientId);
            throw;
        }
    }

    /// <summary>
    /// Checks if exception is a database deadlock exception.
    /// </summary>
    private bool IsDeadlockException(Exception ex)
    {
        // PostgreSQL deadlock error code: 40P01
        return ex.Message.Contains("40P01") || ex.Message.Contains("deadlock");
    }
}
