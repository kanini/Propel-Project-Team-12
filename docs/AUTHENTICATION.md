# Authentication Setup Guide

## Overview

This application uses JWT (JSON Web Tokens) with HS256 symmetric signing for secure authentication. HS256 uses HMAC-SHA256 with a shared secret key for both signing and validating tokens.

## Requirements

- Secret key string (minimum 32 characters / 256 bits)
- Key stored securely in configuration or environment variables
- **IMPORTANT**: Never commit secret keys to version control

## Quick Setup

### Development Environment

The application comes with a default secret key in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenSigningMustBeAtLeast32CharactersLong!"
  }
}
```

**WARNING**: This default key is for development only. Change it immediately for production.

### Generate a Secure Secret Key

**Option 1: PowerShell (Windows)**

```powershell
# Generate a 64-character random key (512 bits)
-join ((65..90) + (97..122) + (48..57) + @(33,35,36,37,38,42,43,45,61,63,64) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

**Option 2: OpenSSL (Cross-Platform)**

```bash
# Generate a base64-encoded 256-bit key
openssl rand -base64 32
```

**Option 3: .NET CLI**

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_SECRET_KEY" --project src/backend/PatientAccess.Web
```

**Option 4: Online Generator**

Visit a secure random string generator:
- Use "CodeIgniter Encryption Keys" or "Fort Knox Passwords"
- Minimum 32 characters required

### Update Configuration

#### Development

Update `appsettings.Development.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YOUR_GENERATED_SECRET_KEY_HERE"
  }
}
```

#### Production (Recommended Methods)

**1. Environment Variables**

```bash
export JwtSettings__SecretKey="YOUR_GENERATED_SECRET_KEY_HERE"
```

**2. Azure App Service Configuration**

```bash
az webapp config appsettings set --name <app-name> \
  --resource-group <rg-name> \
  --settings JwtSettings__SecretKey="YOUR_SECRET_KEY"
```

**3. Azure Key Vault (Most Secure)**

```bash
# Store in Key Vault
az keyvault secret set --vault-name <vault-name> \
  --name "JwtSettings--SecretKey" \
  --value "YOUR_SECRET_KEY"

# Configure App Service to use Key Vault
az webapp config appsettings set --name <app-name> \
  --settings JwtSettings__SecretKey="@Microsoft.KeyVault(SecretUri=https://<vault-name>.vault.azure.net/secrets/JwtSettings--SecretKey/)"
```

## Security Best Practices

### DO ✅

- **Generate strong keys**: Use cryptographically secure random generators
- **Minimum 256 bits**: Use at least 32 characters for the secret key
- **Environment-specific keys**: Use different keys for dev, staging, and production
- **Rotate keys regularly**: Update keys every 90-180 days in production
- **Use secure storage**: Azure Key Vault, AWS Secrets Manager, or environment variables
- **Restrict access**: Limit who can view/modify the secret key

### DON'T ❌

- **Never commit keys**: Add `appsettings.*.json` overrides to `.gitignore`
- **Don't share keys**: Each environment should have unique keys
- **Don't use weak keys**: Avoid predictable or short keys
- **Don't hardcode keys**: Keep keys in configuration, not in source code
- **Don't log keys**: Ensure keys are never written to logs

## Configuration

JWT settings are configured in `appsettings.json`:

```json
{
  "JwtSettings": {
    "Issuer": "PatientAccessAPI",
    "Audience": "PatientAccessClient",
    "ExpirationMinutes": 15,
    "SecretKey": "YourSuperSecretKeyForJWTTokenSigningMustBeAtLeast32CharactersLong!",
    "ClockSkewMinutes": 5
  }
}
```

| Setting | Description | Default | Required |
|---------|-------------|---------|----------|
| `Issuer` | Token issuer identifier | PatientAccessAPI | Yes |
| `Audience` | Token audience identifier | PatientAccessClient | Yes |
| `ExpirationMinutes` | Token lifetime in minutes | 15 | Yes |
| `SecretKey` | HMAC-SHA256 signing key | - | Yes |
| `ClockSkewMinutes` | Allowed time drift for validation | 5 | Yes |

## JWT Token Structure

Generated tokens include:

| Claim | Description | Standard |
|-------|-------------|----------|
| `sub` | User ID (unique identifier) | RFC 7519 |
| `email` | User email address | RFC 7519 |
| `role` | User role (Patient, Clinician, Admin) | Custom |
| `jti` | Token ID (prevents replay attacks) | RFC 7519 |
| `iat` | Issued at timestamp | RFC 7519 |
| `exp` | Expiration timestamp | RFC 7519 |
| `iss` | Issuer (API identifier) | RFC 7519 |
| `aud` | Audience (client identifier) | RFC 7519 |

## Token Lifecycle

1. **Login**: User submits credentials
2. **Validation**: Server verifies credentials against database
3. **Token Generation**: Server signs JWT with secret key
4. **Token Response**: Client receives token (valid 15 minutes)
5. **Request Authorization**: Client sends token in `Authorization: Bearer <token>` header
6. **Token Validation**: Server validates signature using same secret key
7. **Session Timeout**: Token expires after 15 minutes (NFR-005)
8. **Refresh/Re-login**: User must authenticate again

## Troubleshooting

### Error: "JWT SecretKey is not configured"

**Cause**: `SecretKey` missing from `JwtSettings` in appsettings.json

**Solution**: Add `SecretKey` to configuration (see Update Configuration above)

### Error: "JWT SecretKey must be at least 32 characters"

**Cause**: Secret key too short (less than 256 bits)

**Solution**: Generate a longer key using one of the methods above

### Error: "JWT signature validation failed"

**Cause**: Signature mismatch or key changed

**Solution**: 
1. Ensure same secret key is used for signing and validation
2. Check if key was recently changed (invalidates existing tokens)
3. Verify configuration is loaded correctly

### Error: "Token expired"

**Cause**: Token lifetime exceeded (15 minutes by default)

**Solution**: User needs to re-authenticate

## Algorithm Comparison

| Feature | HS256 (Current) | RS256 (Alternative) |
|---------|-----------------|---------------------|
| **Algorithm** | HMAC-SHA256 | RSA-SHA256 |
| **Key Type** | Symmetric (shared secret) | Asymmetric (public/private) |
| **Complexity** | Simple | Complex |
| **Key Management** | One secret key | Two keys (public + private) |
| **Performance** | Faster | Slower |
| **Use Case** | Single backend | Distributed systems |
| **Security** | High (if key protected) | High |

**Why HS256?**
- Simpler setup and maintenance
- Better performance for monolithic applications
- Sufficient security when key is properly protected
- No key file management required

## Production Deployment

### Checklist

- [ ] Generate unique secret key for production
- [ ] Store key in Azure Key Vault or secure storage
- [ ] Configure App Service to use Key Vault reference
- [ ] Remove default key from appsettings.json
- [ ] Test authentication endpoints with new key
- [ ] Configure monitoring/alerts for auth failures
- [ ] Document key rotation procedure
- [ ] Set up key rotation schedule (90-180 days)

### Key Rotation Process

1. **Generate new key**: Create a new secret key
2. **Deploy with dual support**: Temporarily accept both old and new keys
3. **Notify users**: Inform users they may need to re-login
4. **Switch to new key**: Update token generation to use new key
5. **Grace period**: Keep old key validation for 24 hours
6. **Remove old key**: Disable old key, all users re-authenticate
7. **Document change**: Update runbook with rotation date

## References

- **Technical Requirement**: TR-012 (JWT Bearer Authentication)
- **Non-Functional Requirement**: NFR-005 (15-minute session timeout)
- **Security Standard**: OWASP Top 10 - Broken Authentication
- **RFC 7519**: JSON Web Token (JWT) Standard
- **Algorithm**: HS256 (HMAC with SHA-256)

## Related Documentation

- [DATABASE_SETUP.md](./DATABASE_SETUP.md) - Database configuration
- [MIGRATIONS.md](./MIGRATIONS.md) - Database migrations
- [../README.md](../README.md) - Project setup guide

---

**Last Updated**: March 21, 2026  
**Version**: 2.0.0 (HS256)  
**Maintained By**: Development Team
