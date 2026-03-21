namespace PatientAccess.Data.Models;

/// <summary>
/// Clinical data type classification for extracted data (DR-004).
/// Maps to: 1=Vital, 2=Medication, 3=Allergy, 4=Diagnosis, 5=LabResult
/// </summary>
public enum ClinicalDataType
{
    Vital = 1,
    Medication = 2,
    Allergy = 3,
    Diagnosis = 4,
    LabResult = 5
}
