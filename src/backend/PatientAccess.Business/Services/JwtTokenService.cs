using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace PatientAccess.Business.Services;

/// <summary>
/// JWT token generation and validation service using RS256 asymmetric signing algorithm (TR-012).
/// Uses RSA key pair from src/backend/rsa-keys directory for enhanced security.
/// Private key signs tokens, public key validates them.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly int _clockSkewMinutes;
    private readonly RsaSecurityKey _signingKey;
    private readonly RsaSecurityKey _validationKey;

    public JwtTokenService(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        _issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        _audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");
        _expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "15");
        _clockSkewMinutes = int.Parse(jwtSettings["ClockSkewMinutes"] ?? "5");

        // Load RSA keys from src/backend/rsa-keys directory
        var privateKeyPath = jwtSettings["PrivateKeyPath"];
        var publicKeyPath = jwtSettings["PublicKeyPath"];

        // Resolve relative paths relative to ContentRootPath (project directory)
        // This ensures keys are found in PatientAccess.Web\rsa-keys\ where GenerateRsaKeys.ps1 creates them
        if (string.IsNullOrEmpty(privateKeyPath) || !Path.IsPathRooted(privateKeyPath))
        {
            var relativePath = privateKeyPath ?? Path.Combine("rsa-keys", "private-key.xml");
            privateKeyPath = Path.Combine(hostEnvironment.ContentRootPath, relativePath);
        }

        if (string.IsNullOrEmpty(publicKeyPath) || !Path.IsPathRooted(publicKeyPath))
        {
            var relativePath = publicKeyPath ?? Path.Combine("rsa-keys", "public-key.xml");
            publicKeyPath = Path.Combine(hostEnvironment.ContentRootPath, relativePath);
        }

        // Load private key for signing
        if (!File.Exists(privateKeyPath))
        {
            throw new InvalidOperationException(
                $"JWT private key not found at {privateKeyPath} (absolute: {Path.GetFullPath(privateKeyPath)}). " +
                "Run 'powershell -ExecutionPolicy Bypass -File scripts/GenerateRsaKeys.ps1' to generate RSA key pair. " +
                "See docs/AUTHENTICATION.md for setup instructions.");
        }

        var privateKeyXml = File.ReadAllText(privateKeyPath);
        var privateRsa = RSA.Create();
        privateRsa.FromXmlString(privateKeyXml);
        _signingKey = new RsaSecurityKey(privateRsa) { KeyId = "patient-access-rsa-key-1" };

        // Load public key for validation
        if (!File.Exists(publicKeyPath))
        {
            throw new InvalidOperationException(
                $"JWT public key not found at {publicKeyPath} (absolute: {Path.GetFullPath(publicKeyPath)}). " +
                "Run 'powershell -ExecutionPolicy Bypass -File scripts/GenerateRsaKeys.ps1' to generate RSA key pair. " +
                "See docs/AUTHENTICATION.md for setup instructions.");
        }

        var publicKeyXml = File.ReadAllText(publicKeyPath);
        var publicRsa = RSA.Create();
        publicRsa.FromXmlString(publicKeyXml);
        _validationKey = new RsaSecurityKey(publicRsa) { KeyId = "patient-access-rsa-key-1" };
    }

    /// <summary>
    /// Generates JWT token with user claims (userId, email, role) signed using RS256 asymmetric algorithm (TR-012).
    /// Token expires after 15 minutes (NFR-005).
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
    /// Validates JWT token signature and expiration using RS256 asymmetric algorithm.
    /// Uses public key for validation. Returns ClaimsPrincipal if valid, null if invalid or expired.
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
            IssuerSigningKey = _validationKey, // Use public key for validation
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
