namespace PatientAccess.Business.Services;

/// <summary>
/// Service interface for secure password hashing and verification using BCrypt algorithm.
/// Implements TR-013 requirement for BCrypt with cost factor 12.
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a plaintext password using BCrypt with work factor 12.
    /// </summary>
    /// <param name="plaintext">Plaintext password to hash</param>
    /// <returns>BCrypt password hash string (includes salt)</returns>
    string HashPassword(string plaintext);

    /// <summary>
    /// Verifies a plaintext password against a BCrypt hash.
    /// </summary>
    /// <param name="plaintext">Plaintext password to verify</param>
    /// <param name="hash">BCrypt hash to verify against</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    bool VerifyPassword(string plaintext, string hash);
}
