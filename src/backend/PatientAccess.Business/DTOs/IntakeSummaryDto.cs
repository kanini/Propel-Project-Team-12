namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for intake summary (US_033, AC-3).
/// GET /api/intake/{id}/summary
/// </summary>
public class IntakeSummaryDto
{
    /// <summary>
    /// Patient's primary reason for visit.
    /// </summary>
    public string ChiefComplaint { get; set; } = string.Empty;

    /// <summary>
    /// List of current symptoms.
    /// </summary>
    public List<string> Symptoms { get; set; } = new();

    /// <summary>
    /// Current medications list.
    /// </summary>
    public List<MedicationDto> Medications { get; set; } = new();

    /// <summary>
    /// Known allergies list.
    /// </summary>
    public List<AllergyDto> Allergies { get; set; } = new();

    /// <summary>
    /// Medical history items.
    /// </summary>
    public List<MedicalHistoryItemDto> MedicalHistory { get; set; } = new();

    /// <summary>
    /// Family medical history conditions.
    /// </summary>
    public List<string> FamilyHistory { get; set; } = new();

    /// <summary>
    /// Lifestyle information.
    /// </summary>
    public LifestyleDto? Lifestyle { get; set; }

    /// <summary>
    /// Additional concerns or notes.
    /// </summary>
    public string? AdditionalConcerns { get; set; }

    /// <summary>
    /// Insurance information if provided.
    /// </summary>
    public InsuranceInfoDto? InsuranceInfo { get; set; }
}

/// <summary>
/// Medication information.
/// </summary>
public class MedicationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
}

/// <summary>
/// Allergy information.
/// </summary>
public class AllergyDto
{
    public string Allergen { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public string? Severity { get; set; }
}

/// <summary>
/// Medical history item.
/// </summary>
public class MedicalHistoryItemDto
{
    public string Condition { get; set; } = string.Empty;
    public int? DiagnosedYear { get; set; }
    public string Status { get; set; } = "active";
}

/// <summary>
/// Lifestyle information.
/// </summary>
public class LifestyleDto
{
    public string? SmokingStatus { get; set; }
    public string? AlcoholUse { get; set; }
    public string? ExerciseFrequency { get; set; }
}

/// <summary>
/// Insurance information.
/// </summary>
public class InsuranceInfoDto
{
    public string? ProviderName { get; set; }
    public string? MemberId { get; set; }
    public string? GroupNumber { get; set; }
}
