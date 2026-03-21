namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for successful user registration (FR-001).
/// Returns user ID and account status after registration.
/// </summary>
public class RegisterUserResponseDto
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
