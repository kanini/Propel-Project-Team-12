using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service for managing waitlist slot notification lifecycle (US_041/FR-026).
/// Detects available slots, notifies patients via preferred channel, processes confirm/decline/timeout responses.
/// </summary>
public interface IWaitlistNotificationService
{
    /// <summary>
    /// Detects unbooked time slots matching active waitlist entries (AC-1).
    /// Returns list of (WaitlistEntryId, TimeSlotId) pairs to notify.
    /// </summary>
    /// <returns>List of waitlist  entry and slot ID pairs</returns>
    Task<List<(Guid WaitlistEntryId, Guid TimeSlotId)>> DetectAvailableSlotsAsync();

    /// <summary>
    /// Notifies the highest-priority active waitlist patient for a specific slot (AC-1, EC-1).
    /// Generates ResponseToken, sets NotifiedAt/ResponseDeadline, sends SMS/Email.
    /// </summary>
    /// <param name="timeSlotId">Available time slot ID</param>
    /// <returns>True if notification sent successfully</returns>
    Task<bool> NotifyNextPatientAsync(Guid timeSlotId);

    /// <summary>
    /// Processes a confirm response from the patient (AC-2).
    /// Validates token, checks slot availability (EC-2), books appointment, sets Fulfilled.
    /// </summary>
    /// <param name="responseToken">Secure token from notification URL</param>
    /// <returns>Response with booking status and message</returns>
    Task<ConfirmWaitlistResponseDto> ProcessConfirmAsync(string responseToken);

    /// <summary>
    /// Processes a decline response from the patient (AC-3).
    /// Resets entry to Active, notifies next eligible patient (EC-1).
    /// </summary>
    /// <param name="responseToken">Secure token from notification URL</param>
    /// <returns>True if processed successfully</returns>
    Task<bool> ProcessDeclineAsync(string responseToken);

    /// <summary>
    /// Finds and processes expired notifications (AC-4).
    /// Treats timed-out entries as declines, cascades to next patient.
    /// </summary>
    /// <returns>Number of expired entries processed</returns>
    Task<int> ProcessTimeoutsAsync();
}
