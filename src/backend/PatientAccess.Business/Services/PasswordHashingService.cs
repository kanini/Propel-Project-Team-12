using BCrypt.Net;

namespace PatientAccess.Business.Services;

/// <summary>
/// Password hashing service using BCrypt algorithm with cost factor 12.
/// Implements TR-013 requirement balancing security and performance.
/// BCrypt's adaptive hash function provides resistance against brute-force attacks.
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    private const int WorkFactor = 12; // TR-013: BCrypt cost factor 12

    /// <summary>
    /// Hashes a plaintext password using BCrypt with work factor 12.
    /// Each call generates a unique salt automatically.
    /// Hash format: $2a$12$[22-character salt][31-character hash]
    /// </summary>
    /// <param name="plaintext">Plaintext password to hash (cannot be null or empty)</param>
    /// <returns>BCrypt hash string including embedded salt</returns>
    /// <exception cref="ArgumentNullException">Thrown if plaintext is null or empty</exception>
    public string HashPassword(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
            throw new ArgumentNullException(nameof(plaintext), "Password cannot be null or empty.");

        // BCrypt.Net automatically generates salt and includes it in the hash
        // HashPassword with workFactor parameter ensures cost factor 12
        return BCrypt.Net.BCrypt.HashPassword(plaintext, WorkFactor);
    }

    /// <summary>
    /// Verifies a plaintext password against a BCrypt hash.
    /// Extracts salt from hash and recomputes hash of plaintext for constant-time comparison.
    /// </summary>
    /// <param name="plaintext">Plaintext password to verify</param>
    /// <param name="hash">BCrypt hash to verify against</param>
    /// <returns>True if password matches hash; false if mismatch or hash invalid</returns>
    public bool VerifyPassword(string plaintext, string hash)
    {
        if (string.IsNullOrWhiteSpace(plaintext) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            // BCrypt.Verify performs constant-time comparison to prevent timing attacks
            return BCrypt.Net.BCrypt.Verify(plaintext, hash);
        }
        catch
        {
            // Invalid hash format or verification error
            return false;
        }
    }
}
