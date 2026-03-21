namespace PatientAccess.Data.Models;

/// <summary>
/// Pre-visit patient intake data with AI or manual mode (DR-012).
/// </summary>
public class IntakeRecord
{
    public Guid IntakeRecordId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid PatientId { get; set; }

    public IntakeMode IntakeMode { get; set; }

    public string? ChiefComplaint { get; set; }

    /// <summary>JSONB — structured symptom history.</summary>
    public string? SymptomHistory { get; set; }

    /// <summary>JSONB — current medications list.</summary>
    public string? CurrentMedications { get; set; }

    /// <summary>JSONB — known allergies list.</summary>
    public string? KnownAllergies { get; set; }

    /// <summary>JSONB — medical history data.</summary>
    public string? MedicalHistory { get; set; }

    public InsuranceValidationStatus? InsuranceValidationStatus { get; set; }

    public Guid? ValidatedInsuranceRecordId { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
    public User Patient { get; set; } = null!;
    public InsuranceRecord? ValidatedInsuranceRecord { get; set; }
}
