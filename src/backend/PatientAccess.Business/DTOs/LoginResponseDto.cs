namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for successful user login (FR-002).
/// Returns JWT token, user information, and session expiration.
/// </summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string Message { get; set; } = string.Empty;
}
