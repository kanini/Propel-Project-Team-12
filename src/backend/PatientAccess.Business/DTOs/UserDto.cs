using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for user information (US_021).
/// Contains user details without sensitive information (e.g., password hash).
/// </summary>
public class UserDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
