using System.ComponentModel.DataAnnotations.Schema;

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

    /// <summary>
    /// Encrypted Social Security Number (stored as BYTEA, encrypted with pgcrypto).
    /// NEVER store plaintext SSN in database. Use database functions encrypt_ssn/decrypt_ssn
    /// for encryption/decryption with column encryption key.
    /// Encrypted using AES-256 cipher (US_056, AC1, FR-041).
    /// </summary>
    [Column("SSNEncrypted")]
    public byte[]? SSNEncrypted { get; set; }

    /// <summary>
    /// Encrypted Insurance ID Number (stored as BYTEA, encrypted with pgcrypto).
    /// Use database functions encrypt_insurance_id/decrypt_insurance_id for encryption/decryption.
    /// Encrypted using AES-256 cipher (US_056, AC1, FR-041).
    /// </summary>
    [Column("InsuranceIDEncrypted")]
    public byte[]? InsuranceIDEncrypted { get; set; }

    /// <summary>
    /// Encryption key version identifier for key rotation support.
    /// Default: "v1". Updated during key rotation migrations.
    /// Used to identify which encryption key was used to encrypt SSN/InsuranceID (US_056).
    /// </summary>
    [Column("EncryptionKeyVersion")]
    public string EncryptionKeyVersion { get; set; } = "v1";

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ClinicalDocument> ClinicalDocuments { get; set; } = new List<ClinicalDocument>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
