namespace PatientAccess.Data.Models;

/// <summary>
/// Verification status for medical code suggestions (DR-013).
/// Maps to: 1=AISuggested, 2=Accepted, 3=Modified, 4=Rejected
/// </summary>
public enum MedicalCodeVerificationStatus
{
    AISuggested = 1,
    Accepted = 2,
    Modified = 3,
    Rejected = 4
}
