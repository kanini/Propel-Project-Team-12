namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for medical code audit trail entries returned by GET /api/medical-codes/{codeId}/audit-trail.
/// Displays modification history for medical codes (US_052 Task 2, Edge Case).
/// Distinct from the general AuditLogDto in Interfaces namespace - this is specific to medical code modifications.
/// </summary>
public class MedicalCodeAuditLogDto
{
    /// <summary>
    /// Type of action performed (e.g., "Modified", "Accepted", "Rejected").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Name of the user who performed the action.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// User ID who performed the action.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// JSON representation of the previous state before modification.
    /// For medical codes: { "Code": "E11.9", "Description": "...", "VerificationStatus": "Pending" }
    /// </summary>
    public string? PreviousValue { get; set; }

    /// <summary>
    /// JSON representation of the new state after modification.
    /// For medical codes: { "Code": "E11.65", "Description": "...", "VerificationStatus": "StaffVerified" }
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Staff rationale for the modification (null for non-modification actions).
    /// </summary>
    public string? Rationale { get; set; }

    /// <summary>
    /// Timestamp when the action was performed (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
