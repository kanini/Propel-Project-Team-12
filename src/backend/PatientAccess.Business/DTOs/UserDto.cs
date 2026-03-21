using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// User data transfer object for US_021.
/// Excludes sensitive PasswordHash field for secure data transmission.
/// </summary>
public class UserDto
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }
}
