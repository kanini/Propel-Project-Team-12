# Task - task_001_jwt_authentication_middleware

## Requirement Reference
- User Story: us_004
- Story Location: .propel/context/tasks/EP-TECH/us_004/us_004.md
- Acceptance Criteria:
    - AC-1: JWT Bearer authentication registered with RS256 signing algorithm and token validation parameters
    - AC-4: Valid user authentication returns JWT token with user ID, email, and role claims with configurable expiration
    - AC-5: Unauthenticated request to protected endpoint returns 401 Unauthorized
- Edge Case:
    - RS256 key pair missing from configuration should prevent application startup with clear error
    - Expired tokens should return 401 with `token_expired` error code distinguishable from invalid token

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET ASP.NET Core | 8.0 |
| Library | Microsoft.AspNetCore.Authentication.JwtBearer | 8.x |
| Library | System.IdentityModel.Tokens.Jwt | 7.x |

**Note**: All code and libraries MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview
Implement JWT Bearer authentication using RS256 asymmetric signing algorithm for secure token generation and validation. Configure authentication middleware with token validation parameters including issuer, audience, lifetime validation, and clock skew tolerance. Create JWT token generation service with claims-based user identity (user ID, email, role). Implement proper 401 Unauthorized responses with distinguishable error codes for expired vs. invalid tokens.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project structure

## Impacted Components
- **NEW** src/backend/PatientAccess.Business/Services/JwtTokenService.cs - JWT token generation and validation service
- **NEW** src/backend/PatientAccess.Business/Services/IJ wtTokenService.cs - JWT service interface
- **MODIFY** src/backend/PatientAccess.Web/Program.cs - Register JWT authentication middleware
- **MODIFY** src/backend/PatientAccess.Web/appsettings.json - Add JWT configuration section
- **NEW** src/backend/Pat ientAccess.Web/Middleware/JwtValidationMiddleware.cs - Custom JWT validation with detailed error responses
- **NEW** security/rsa-keys/ - Directory for RS256 key pair storage (gitignored)

## Implementation Plan
1. **Generate RS256 Key Pair**: Create RSA public/private key pair using OpenSSL or .NET Crypto API for token signing
2. **Configure JWT Settings**: Add JWT section to `appsettings.json` with issuer, audience, expiration (15 minutes), and key configuration
3. **Install JWT NuGet Packages**: Add `Microsoft.AspNetCore.Authentication.JwtBearer` and `System.IdentityModel.Tokens.Jwt`
4. **Implement JwtTokenService**: Create service with `GenerateToken` method accepting user claims and returning signed JWT string
5. **Register Authentication Middleware**: Configure JWT Bearer authentication in `Program.cs` with RS256 algorithm and validation parameters
6. **Implement Token Validation Middleware**: Create middleware to catch token validation exceptions and return distinguishable error codes
7. **Add Authorization Attribute**: Document usage of `[Authorize]` attribute for protecting endpoints
8. **Test Authentication Flow**: Verify protected endpoints return 401 for missing/invalid/expired tokens

## Current Project State
```
Propel-Project-Team-12/
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── PatientAccess.Business/
│   │   └── Services/
│   └── PatientAccess.Data/
└── .env
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | security/rsa-keys/private-key.pem | RSA private key for token signing (gitignored) |
| CREATE | security/rsa-keys/public-key.pem | RSA public key for token validation |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add JwtSettings section (issuer, audience, expiration) |
| CREATE | src/backend/PatientAccess.Business/Services/IJwtTokenService.cs | JWT service interface with GenerateToken method |
| CREATE | src/backend/PatientAccess.Business/Services/JwtTokenService.cs | JWT token generation implementation |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register JWT authentication with RS256 and validation parameters |
| CREATE | src/backend/PatientAccess.Web/Middleware/JwtValidationMiddleware.cs | Custom middleware for detailed JWT error responses |
| CREATE | docs/AUTHENTICATION.md | JWT authentication flow and configuration documentation |

## External References
- JWT Bearer Authentication in ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-bearer?view=aspnetcore-8.0
- System.IdentityModel.Tokens.Jwt: https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt
- RS256 vs HS256: https://auth0.com/blog/rs256-vs-hs256-whats-the-difference/
- JWT Best Practices: https://datatracker.ietf.org/doc/html/rfc8725
- OpenSSL RSA Key Generation: https://www.openssl.org/docs/man1.1.1/man1/genrsa.html

## Build Commands
```bash
# Generate RSA private key (2048-bit)
openssl genrsa -out security/rsa-keys/private-key.pem 2048

# Extract public key from private key
openssl rsa -in security/rsa-keys/private-key.pem -pubout -out security/rsa-keys/public-key.pem

# View private key contents (for verification)
openssl rsa -in security/rsa-keys/private-key.pem -text -noout

# Add rsa-keys directory to .gitignore
echo "security/rsa-keys/*.pem" >> .gitignore
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for infrastructure task)
- [ ] Integration tests pass (N/A for infrastructure task)
- [ ] RS256 key pair generated and stored securely (gitignored)
- [ ] JWT token generated successfully with valid claims (user ID, email, role)
- [ ] Protected endpoint with `[Authorize]` returns 401 when no token provided
- [ ] Protected endpoint with valid token returns 200 and processes request
- [ ] Expired token returns 401 with `token_expired` error code
- [ ] Invalid token signature returns 401 with `invalid_token` error code
- [ ] Application fails to start when RS256 keys are missing with descriptive error

## Implementation Checklist
- [X] Generate RS256 key pair using OpenSSL (2048-bit minimum)
- [X] Add JwtSettings section to `appsettings.json` with issuer, audience, expiration
- [X] Install Microsoft.AspNetCore.Authentication.JwtBearer and System.IdentityModel.Tokens.Jwt
- [X] Create `IJwtTokenService` interface with `GenerateToken(userId, email, role)` method
- [X] Implement `JwtTokenService` with RS256 signing using private key
- [X] Register JWT authentication in `Program.cs` with token validation parameters
- [X] Create `JwtValidationMiddleware` to distinguish expired vs invalid token errors
- [X] Test authentication with `/api/test/protected` endpoint marked with `[Authorize]`
