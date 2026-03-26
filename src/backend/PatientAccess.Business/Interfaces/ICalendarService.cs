using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Provider-agnostic calendar integration service (US_039 - FR-024).
/// Supports OAuth2 authorization, event CRUD, and connection management.
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// Generates OAuth2 authorization URL for user consent (AC-4).
    /// Returns Google/Outlook OAuth consent page URL.
    /// </summary>
    /// <param name="userId">User identifier for state parameter (CSRF protection)</param>
    /// <param name="redirectUri">OAuth2 callback endpoint URL</param>
    /// <returns>Authorization URL to redirect user to</returns>
    Task<string> GetAuthorizationUrlAsync(Guid userId, string redirectUri);

    /// <summary>
    /// Handles OAuth2 callback to exchange authorization code for access tokens (AC-4).
    /// Stores encrypted access token and refresh token in CalendarIntegration entity.
    /// </summary>
    /// <param name="userId">User identifier from state parameter</param>
    /// <param name="authorizationCode">Authorization code from OAuth provider</param>
    /// <param name="redirectUri">OAuth2 callback endpoint URL (must match authorization request)</param>
    Task HandleCallbackAsync(Guid userId, string authorizationCode, string redirectUri);

    /// <summary>
    /// Creates calendar event via Google Calendar API (AC-1).
    /// Returns event ID for storage in Appointment.GoogleCalendarEventId.
    /// </summary>
    /// <param name="userId">User identifier for token lookup</param>
    /// <param name="eventData">Event details (title, start/end time, description, location)</param>
    /// <returns>Calendar event ID (Google event ID)</returns>
    /// <exception cref="CalendarTokenExpiredException">Thrown when refresh token is invalid or expired</exception>
    Task<string?> CreateEventAsync(Guid userId, CalendarEventDto eventData);

    /// <summary>
    /// Updates existing calendar event with new date and time (AC-2).
    /// </summary>
    /// <param name="userId">User identifier for token lookup</param>
    /// <param name="eventId">Calendar event ID (Google event ID)</param>
    /// <param name="eventData">Updated event details</param>
    /// <exception cref="CalendarTokenExpiredException">Thrown when refresh token is invalid or expired</exception>
    Task UpdateEventAsync(Guid userId, string eventId, CalendarEventDto eventData);

    /// <summary>
    /// Deletes calendar event from user's calendar (AC-3).
    /// </summary>
    /// <param name="userId">User identifier for token lookup</param>
    /// <param name="eventId">Calendar event ID (Google event ID)</param>
    /// <exception cref="CalendarTokenExpiredException">Thrown when refresh token is invalid or expired</exception>
    Task DeleteEventAsync(Guid userId, string eventId);

    /// <summary>
    /// Checks if user has an active calendar connection.
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>True if user has connected calendar with valid tokens</returns>
    Task<bool> IsConnectedAsync(Guid userId);

    /// <summary>
    /// Disconnects user's calendar integration.
    /// Revokes OAuth token and marks connection inactive.
    /// </summary>
    /// <param name="userId">User identifier</param>
    Task DisconnectAsync(Guid userId);
}
