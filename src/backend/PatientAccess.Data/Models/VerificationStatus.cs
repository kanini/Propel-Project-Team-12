namespace PatientAccess.Data.Models;

/// <summary>
/// Verification status for AI-extracted clinical data (AC1 - US_058, DR-004).
/// Tracks human-in-the-loop verification lifecycle per AIR-S01.
/// </summary>
public enum VerificationStatus
{
    /// <summary>
    /// AI extraction pending staff verification (default state).
    /// Data cannot be committed to patient record without explicit verification (AC1).
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Staff verified AI extraction as accurate (AC1: explicit verification).
    /// </summary>
    Verified = 1,
    
    /// <summary>
    /// Staff manually edited AI extraction (AC1: corrections made).
    /// </summary>
    ManuallyEdited = 2,
    
    /// <summary>
    /// AI extraction rejected, data entered manually.
    /// </summary>
    Rejected = 3
}
