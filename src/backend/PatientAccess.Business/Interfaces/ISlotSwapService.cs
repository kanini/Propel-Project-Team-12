using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for dynamic preferred slot swap functionality (FR-010).
/// Handles automatic appointment swapping when preferred slots become available.
/// </summary>
public interface ISlotSwapService
{
    /// <summary>
    /// Executes atomic swap of appointment from current time slot to preferred time slot.
    /// Uses pessimistic locking (SELECT FOR UPDATE) to prevent race conditions.
    /// </summary>
    /// <param name="appointmentId">Appointment ID to swap</param>
    /// <param name="preferredSlotId">Target preferred slot ID</param>
    /// <returns>True if swap succeeded, false if swap failed (slot already booked or other race condition)</returns>
    /// <exception cref="ArgumentException">Thrown when appointment or slot not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when appointment is not in valid state for swap (e.g., cancelled)</exception>
    Task<bool> ExecuteSwapAsync(Guid appointmentId, Guid preferredSlotId);

    /// <summary>
    /// Detects available slots that have pending swap requests and processes them.
    /// Called by background job (SlotAvailabilityMonitor) every 5 minutes.
    /// Processes swaps in FIFO order (first booked appointment gets first priority).
    /// </summary>
    /// <returns>Number of successful swaps executed</returns>
    Task<int> ProcessPendingSwapsAsync();

    /// <summary>
    /// Cancels swap preference for an appointment, maintaining original booking.
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="patientId">Patient ID for ownership verification</param>
    /// <returns>True if preference cancelled successfully</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when patient doesn't own the appointment</exception>
    /// <exception cref="ArgumentException">Thrown when appointment not found</exception>
    Task<bool> CancelSwapPreferenceAsync(Guid appointmentId, Guid patientId);
}
