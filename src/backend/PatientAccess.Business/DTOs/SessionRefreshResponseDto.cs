namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for session refresh endpoint (US_022, AC5).
/// </summary>
public class SessionRefreshResponseDto
{
    public DateTime ExpiresAt { get; set; }
    public string Message { get; set; } = "Session refreshed successfully";
}
