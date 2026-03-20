using System.Security.Claims;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service interface for generating and validating JWT tokens using HS256 symmetric signing.
/// Tokens include user claims (ID, email, role) and expire after configured timeout (default 15 minutes per NFR-005).
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT token containing user identity claims.
    /// </summary>
    /// <param name="userId">Unique user identifier</param>
    /// <param name="email">User email address</param>
    /// <param name="role">User role (Patient, Staff, Admin)</param>
    /// <returns>Signed JWT token string</returns>
    string GenerateToken(string userId, string email, string role);

    /// <summary>
    /// Validates a JWT token and extracts claims if valid.
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>ClaimsPrincipal if valid, null if invalid or expired</returns>
    ClaimsPrincipal? ValidateToken(string token);
}
