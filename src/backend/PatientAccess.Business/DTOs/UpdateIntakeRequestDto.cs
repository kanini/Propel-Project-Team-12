namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for updating intake data (US_033).
/// PATCH /api/intake/{id}
/// </summary>
public class UpdateIntakeRequestDto
{
    /// <summary>
    /// Updated chief complaint.
    /// </summary>
    public string? ChiefComplaint { get; set; }

    /// <summary>
    /// Updated symptoms list (JSON string).
    /// </summary>
    public string? SymptomHistory { get; set; }

    /// <summary>
    /// Updated medications list (JSON string).
    /// </summary>
    public string? CurrentMedications { get; set; }

    /// <summary>
    /// Updated allergies list (JSON string).
    /// </summary>
    public string? KnownAllergies { get; set; }

    /// <summary>
    /// Updated medical history (JSON string).
    /// </summary>
    public string? MedicalHistory { get; set; }
}

/// <summary>
/// Request DTO for completing intake.
/// POST /api/intake/{id}/complete
/// </summary>
public class CompleteIntakeRequestDto
{
    /// <summary>
    /// Final summary data to save.
    /// </summary>
    public IntakeSummaryDto? Summary { get; set; }
}

/// <summary>
/// Response DTO for completing intake.
/// </summary>
public class CompleteIntakeResponseDto
{
    /// <summary>
    /// Whether the completion was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// ID of the created/updated intake record.
    /// </summary>
    public string IntakeRecordId { get; set; } = string.Empty;

    /// <summary>
    /// Completion message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
