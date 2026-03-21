namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for successful user registration (FR-001).
/// Returns user ID and confirmation message.
/// </summary>
public class RegisterUserResponse
{
    public Guid UserId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
