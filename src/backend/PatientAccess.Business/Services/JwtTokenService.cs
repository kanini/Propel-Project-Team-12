using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PatientAccess.Business.Services;

/// <summary>
/// JWT token generation and validation service using RS256 asymmetric signing algorithm.
/// Implements TR-012 requirement for JWT Bearer tokens with RS256.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly int _clockSkewMinutes;
    private readonly RSA _rsaPrivate;
    private readonly RSA _rsaPublic;

    public JwtTokenService(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        _issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        _audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");
        _expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "15");
        _clockSkewMinutes = int.Parse(jwtSettings["ClockSkewMinutes"] ?? "5");

        // Load RSA keys from configured paths
        var privateKeyPath = jwtSettings["PrivateKeyPath"] ?? throw new InvalidOperationException("JWT PrivateKeyPath is not configured.");
        var publicKeyPath = jwtSettings["PublicKeyPath"] ?? throw new InvalidOperationException("JWT PublicKeyPath is not configured.");

        // Validate key files exist
        if (!File.Exists(privateKeyPath))
        {
            throw new FileNotFoundException(
                "RSA private key file not found. Please ensure RS256 key pair is generated. " +
                $"Run: openssl genrsa -out {privateKeyPath} 2048",
                privateKeyPath);
        }

        if (!File.Exists(publicKeyPath))
        {
            throw new FileNotFoundException(
               $"RSA public key file not found at {publicKeyPath}.",
                publicKeyPath);
        }

        // Load RSA keys (XML format for Windows compatibility)
        _rsaPrivate = RSA.Create();
        _rsaPublic = RSA.Create();

        try
        {
            var privateKeyXml = File.ReadAllText(privateKeyPath);
            var publicKeyXml = File.ReadAllText(publicKeyPath);

            _rsaPrivate.FromXmlString(privateKeyXml);
            _rsaPublic.FromXmlString(publicKeyXml);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to load RSA keys. Ensure keys are in valid XML format. " +
                "Generate using: New RSA key pair via PowerShell or openssl.",
                ex);
        }
    }

    /// <summary>
    /// Generates JWT token with user claims (userId, email, role) signed using RS256 private key.
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
            new RsaSecurityKey(_rsaPrivate),
            SecurityAlgorithms.RsaSha256); // RS256 algorithm

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
    /// Validates JWT token signature and expiration using RS256 public key.
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
            IssuerSigningKey = new RsaSecurityKey(_rsaPublic),
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
