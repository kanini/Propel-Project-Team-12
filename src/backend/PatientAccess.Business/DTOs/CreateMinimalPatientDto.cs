using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for minimal patient creation during walk-in registration (US_029, AC-2).
/// Staff can quickly register walk-in patients without full onboarding process.
/// Patient status is Active, allowing immediate appointment booking.
/// </summary>
public class CreateMinimalPatientDto
{
    /// <summary>
    /// Patient first name (2-50 characters).
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Patient last name (2-50 characters).
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Patient date of birth in ISO 8601 format (YYYY-MM-DD).
    /// </summary>
    [Required(ErrorMessage = "Date of birth is required")]
    public string DateOfBirth { get; set; } = string.Empty;

    /// <summary>
    /// Patient phone number (10-15 digits with optional formatting).
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Patient email address (optional).
    /// If provided and already exists in system, returns existing patient record.
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string? Email { get; set; }
}
