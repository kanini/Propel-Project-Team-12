namespace PatientAccess.Data.Models;

/// <summary>
/// Calendar integration entity for per-user OAuth2 token storage (US_039).
/// Supports multi-provider architecture: Google Calendar (US_039), Outlook (US_040).
/// Tokens stored encrypted to comply with OWASP cryptographic failure prevention.
/// </summary>
public class CalendarIntegration
{
    public Guid CalendarIntegrationId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Calendar provider identifier: "Google", "Outlook".
    /// Enables multi-provider support for future integrations.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 access token for calendar API access.
    /// Stored encrypted at rest for OWASP compliance.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 refresh token for token renewal.
    /// Stored encrypted at rest for OWASP compliance.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration timestamp.
    /// Used to trigger proactive token refresh before expiry.
    /// </summary>
    public DateTime TokenExpiry { get; set; }

    /// <summary>
    /// Calendar identifier for the target calendar.
    /// For Google: typically "primary". For Outlook: calendar ID.
    /// </summary>
    public string? CalendarId { get; set; }

    /// <summary>
    /// Active connection status flag.
    /// Set to false when token refresh fails (EC-2) to prompt user re-authorization.
    /// </summary>
    public bool IsConnected { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
