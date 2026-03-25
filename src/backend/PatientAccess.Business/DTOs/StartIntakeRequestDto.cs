using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for starting an intake session (US_033, AC-1).
/// POST /api/intake/start
/// </summary>
public class StartIntakeRequestDto
{
    /// <summary>
    /// Appointment ID for which intake is being started.
    /// </summary>
    [Required(ErrorMessage = "AppointmentId is required")]
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Intake mode: "ai" for AI conversational, "manual" for form-based.
    /// </summary>
    [Required(ErrorMessage = "Mode is required")]
    [RegularExpression("^(ai|manual)$", ErrorMessage = "Mode must be 'ai' or 'manual'")]
    public string Mode { get; set; } = "ai";
}

/// <summary>
/// Response DTO for starting an intake session.
/// </summary>
public class StartIntakeResponseDto
{
    /// <summary>
    /// Unique session identifier for subsequent API calls.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// AI welcome message to display to the patient.
    /// </summary>
    public string WelcomeMessage { get; set; } = string.Empty;

    /// <summary>
    /// Current session status.
    /// </summary>
    public string Status { get; set; } = "active";
}
