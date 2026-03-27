namespace PatientAccess.Data.Models.Enums;

/// <summary>
/// Severity classification for data conflicts (US_048, AC3).
/// Used to prioritize conflict resolution workflow.
/// </summary>
public enum ConflictSeverity
{
    /// <summary>
    /// Critical severity - patient safety implications (medications, severe allergies).
    /// Requires immediate staff attention.
    /// </summary>
    Critical = 0,

    /// <summary>
    /// Warning severity - clinical significance (diagnoses, moderate allergies).
    /// Requires staff review within reasonable timeframe.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Info severity - minor discrepancies (vitals, administrative data).
    /// Review at convenience.
    /// </summary>
    Info = 2
}
