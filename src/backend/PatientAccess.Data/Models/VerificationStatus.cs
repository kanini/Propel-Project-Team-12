namespace PatientAccess.Data.Models;

/// <summary>
/// Verification status for AI-extracted clinical data (DR-004).
/// Maps to: 1=AISuggested, 2=StaffVerified, 3=Rejected
/// </summary>
public enum VerificationStatus
{
    AISuggested = 1,
    StaffVerified = 2,
    Rejected = 3
}
