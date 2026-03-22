namespace PatientAccess.Business.DTOs;

/// <summary>
/// Provider data transfer object for US_023 - Provider Browser (FR-006, AC1).
/// Represents a single provider with availability information.
/// </summary>
public class ProviderDto
{
    /// <summary>
    /// Unique provider identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Provider full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Medical specialty (e.g., "Family Medicine", "Cardiology").
    /// </summary>
    public string Specialty { get; set; } = string.Empty;

    /// <summary>
    /// Provider rating (0.0 to 5.0).
    /// </summary>
    public decimal Rating { get; set; }

    /// <summary>
    /// Number of reviews contributing to rating.
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Next available appointment slot (null if no availability).
    /// Edge Case: Providers with no available slots return null (task requirement).
    /// </summary>
    public DateTime? NextAvailableSlot { get; set; }

    /// <summary>
    /// Provider avatar URL (optional).
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Provider gender (optional filter criterion).
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Provider location/address (optional).
    /// </summary>
    public string? Location { get; set; }
}
