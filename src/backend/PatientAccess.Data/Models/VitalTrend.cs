namespace PatientAccess.Data.Models;

/// <summary>
/// Time-series vital signs data preserving historical measurements (FR-030).
/// No de-duplication - preserves all vital measurements over time to prevent data loss.
/// </summary>
public class VitalTrend
{
    public Guid Id { get; set; }

    public int PatientProfileId { get; set; }

    /// <summary>
    /// Vital type: BloodPressure, HeartRate, Temperature, Weight, Height, BMI, O2Saturation
    /// </summary>
    public string VitalType { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public DateTime RecordedAt { get; set; }

    public Guid SourceDocumentId { get; set; }

    public Guid SourceDataId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
}
