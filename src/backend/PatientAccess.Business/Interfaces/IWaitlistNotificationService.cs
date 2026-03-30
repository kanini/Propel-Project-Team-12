using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Waitlist notification service interface for US_041 - Waitlist Slot Availability Notifications.
/// Manages the lifecycle of slot availability notifications: detect, notify, confirm, decline, timeout.
/// </summary>
public interface IWaitlistNotificationService
{
    /// <summary>
    /// Detects unbooked time slots matching active waitlist entries (AC-1).
    /// Returns list of (WaitlistEntryId, TimeSlotId) pairs to notify.
    /// Deduplicates by TimeSlotId — one notification per slot to highest-priority patient.
    /// </summary>
    /// <returns>List of waitlist entry and slot pairs eligible for notification</returns>
    Task<List<(Guid WaitlistEntryId, Guid TimeSlotId)>> DetectAvailableSlotsAsync();

    /// <summary>
    /// Notifies the highest-priority active waitlist patient for a specific slot (AC-1, EC-1).
    /// Generates ResponseToken, sets NotifiedAt/ResponseDeadline, sends SMS/Email.
    /// Priority selection: Order by Priority (ascending), then CreatedAt (FIFO).
    /// </summary>
    /// <param name="timeSlotId">The available time slot to offer</param>
    /// <returns>True if notification sent, false if no eligible patient or slot already booked</returns>
    Task<bool> NotifyNextPatientAsync(Guid timeSlotId);

    /// <summary>
    /// Processes a confirm response from the patient (AC-2, EC-2).
    /// Validates token, checks slot availability (EC-2: slot may have been re-booked),
    /// books appointment, sets Fulfilled status.
    /// </summary>
    /// <param name="responseToken">Unique response token from notification URL</param>
    /// <returns>Confirm response DTO with success status and message</returns>
    /// <exception cref="KeyNotFoundException">Token not found or invalid</exception>
    /// <exception cref="InvalidOperationException">Token already responded to or expired</exception>
    Task<ConfirmWaitlistResponseDto> ProcessConfirmAsync(string responseToken);

    /// <summary>
    /// Processes a decline response from the patient (AC-3, EC-1).
    /// Resets entry to Active, notifies next eligible patient sequentially.
    /// </summary>
    /// <param name="responseToken">Unique response token from notification URL</param>
    /// <returns>True if processed successfully</returns>
    /// <exception cref="KeyNotFoundException">Token not found or invalid</exception>
    Task<bool> ProcessDeclineAsync(string responseToken);

    /// <summary>
    /// Finds and processes expired notifications (AC-4).
    /// Treats timed-out entries as declines, cascades to next patient.
    /// Scans for WaitlistStatus.Notified entries past ResponseDeadline.
    /// </summary>
    /// <returns>Count of expired notifications processed</returns>
    Task<int> ProcessTimeoutsAsync();
}
