namespace PatientAccess.Data.Models;

/// <summary>
/// Insurance validation status for intake records (DR-012).
/// Maps to: 1=NotValidated, 2=Valid, 3=Invalid
/// </summary>
public enum InsuranceValidationStatus
{
    NotValidated = 1,
    Valid = 2,
    Invalid = 3
}
