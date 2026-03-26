using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for notification management (US_067).
/// Handles notification retrieval, unread counts, and read status updates.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Retrieves recent unread notifications for authenticated patient.
    /// Results are cached for 2 minutes.
    /// </summary>
    /// <param name="userId">Patient user ID from JWT claims</param>
    /// <param name="limit">Maximum number of notifications to return (default: 5, max: 20)</param>
    /// <returns>List of recent notifications</returns>
    Task<List<NotificationDto>> GetRecentNotificationsAsync(Guid userId, int limit);

    /// <summary>
    /// Retrieves count of unread notifications for badge display.
    /// Results are cached for 2 minutes.
    /// </summary>
    /// <param name="userId">Patient user ID from JWT claims</param>
    /// <returns>Unread notification count</returns>
    Task<int> GetUnreadCountAsync(Guid userId);

    /// <summary>
    /// Marks notification as read for authenticated patient.
    /// Invalidates cached counts and notifications list.
    /// </summary>
    /// <param name="notificationId">Notification unique identifier</param>
    /// <param name="userId">Patient user ID for ownership verification</param>
    /// <returns>True if update succeeded, false if notification not found</returns>
    Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
}
