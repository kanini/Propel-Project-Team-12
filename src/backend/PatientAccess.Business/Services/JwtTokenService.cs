using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PatientAccess.Business.Services;

/// <summary>
/// JWT token generation and validation service using HS256 symmetric signing algorithm.
/// Implements TR-012 requirement for JWT Bearer tokens with HMAC-SHA256.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly int _clockSkewMinutes;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        _issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        _audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");
        _expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "15");
        _clockSkewMinutes = int.Parse(jwtSettings["ClockSkewMinutes"] ?? "5");

        // Load secret key from configuration
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        // Validate key length (minimum 256 bits / 32 characters for HS256)
        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT SecretKey must be at least 32 characters (256 bits) for HS256 algorithm. " +
                "Current length: " + secretKey.Length + " characters.");
        }

        // Create symmetric security key from secret
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    /// <summary>
    /// Generates JWT token with user claims (userId, email, role) signed using HS256 symmetric key.
    /// Token expires after configured timeout (default 15 minutes per NFR-005).
    /// </summary>
    public string GenerateToken(string userId, string email, string role)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentNullException(nameof(role), "Role cannot be null or empty.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var signingCredentials = new SigningCredentials(
            _signingKey,
            SecurityAlgorithms.HmacSha256); // HS256 algorithm

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates JWT token signature and expiration using HS256 symmetric key.
    /// Returns ClaimsPrincipal if valid, null if invalid or expired.
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.FromMinutes(_clockSkewMinutes) // Allow slight time drift
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            // Token expired - handle separately in middleware
            return null;
        }
        catch (SecurityTokenException)
        {
            // Invalid token (signature, claims, etc.)
            return null;
        }
    }
}
