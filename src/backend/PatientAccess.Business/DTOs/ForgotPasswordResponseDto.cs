namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for forgot password request.
/// </summary>
public class ForgotPasswordResponseDto
{
    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Email address where reset link was sent.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
