using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for sending a chat message in intake session (US_033, AC-2).
/// POST /api/intake/message
/// </summary>
public class IntakeMessageRequestDto
{
    /// <summary>
    /// Intake session ID.
    /// </summary>
    [Required(ErrorMessage = "SessionId is required")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Patient's message text.
    /// Maximum 2000 characters as per task specification.
    /// </summary>
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public string Message { get; set; } = string.Empty;
}
