namespace PatientAccess.Data.Models;

/// <summary>
/// Core user entity for authentication and authorization (DR-001).
/// Represents any authenticated system user (Patient, Staff, Admin).
/// </summary>
public class User
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }

    public string? VerificationToken { get; set; }

    public DateTime? VerificationTokenExpiry { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ClinicalDocument> ClinicalDocuments { get; set; } = new List<ClinicalDocument>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
