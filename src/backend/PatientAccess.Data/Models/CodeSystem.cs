namespace PatientAccess.Data.Models;

/// <summary>
/// Medical code system identifier (DR-013).
/// Maps to: 1=ICD10, 2=CPT
/// </summary>
public enum CodeSystem
{
    ICD10 = 1,
    CPT = 2
}
