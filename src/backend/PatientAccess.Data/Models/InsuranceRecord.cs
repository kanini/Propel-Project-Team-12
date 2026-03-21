namespace PatientAccess.Data.Models;

/// <summary>
/// Insurance provider reference data for FR-021 insurance pre-check (DR-015).
/// </summary>
public class InsuranceRecord
{
    public Guid InsuranceRecordId { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Regex pattern for validating insurance ID numbers.
    /// </summary>
    public string? AcceptedIdPattern { get; set; }

    public CoverageType CoverageType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
