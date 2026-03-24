namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for unread notification count (US_067).
/// Used for notification badge display in header.
/// </summary>
public class UnreadCountDto
{
    /// <summary>
    /// Count of unread notifications for the user.
    /// </summary>
    public int Count { get; set; }
}
