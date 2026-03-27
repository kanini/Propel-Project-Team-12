namespace PatientAccess.Business.Enums;

/// <summary>
/// Verification badge for UI display per UXR-402 (AI-suggested vs staff-verified).
/// Maps from ExtractedClinicalData.VerificationStatus for 360° view display.
/// </summary>
public enum VerificationBadge
{
    /// <summary>
    /// AI-suggested data (amber badge) - not yet verified by staff.
    /// Maps from VerificationStatus.AISuggested.
    /// </summary>
    AISuggested = 0,

    /// <summary>
    /// Staff-verified data (green badge) - manually reviewed and confirmed.
    /// Maps from VerificationStatus.StaffVerified.
    /// </summary>
    StaffVerified = 1
}
