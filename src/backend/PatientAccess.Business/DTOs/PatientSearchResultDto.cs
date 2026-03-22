namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for patient search results (US_029, AC-1).
/// Returns minimal patient information for walk-in booking staff interface.
/// </summary>
public class PatientSearchResultDto
{
    /// <summary>
    /// Patient unique identifier (User ID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Patient full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Patient date of birth in ISO 8601 format (YYYY-MM-DD).
    /// </summary>
    public string DateOfBirth { get; set; } = string.Empty;

    /// <summary>
    /// Patient email address (optional).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Patient phone number (optional).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Date of last appointment in ISO 8601 format (YYYY-MM-DD), if any.
    /// </summary>
    public string? LastAppointmentDate { get; set; }
}
