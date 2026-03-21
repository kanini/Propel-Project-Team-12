namespace PatientAccess.Business.DTOs;

/// <summary>
/// Login response DTO (FR-002 - User Authentication)
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT authentication token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// User role (Patient/Staff/Admin)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User display name
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
