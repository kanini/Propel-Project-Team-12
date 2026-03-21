# Task - TASK_006

## Requirement Reference
- User Story: US_019
- Story Location: .propel/context/tasks/EP-001/us_019/us_019.md
- Acceptance Criteria:
    - AC1: JWT token generation with RS256 signing
    - AC5: JWT claims include userId and role
- Edge Case:
    - None specific to JWT service

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
| Frontend | N/A | N/A |
| Backend | .NET 8 ASP.NET Core Web API | 8.0 |
| Database | N/A | N/A |
| Library | System.IdentityModel.Tokens.Jwt | Latest |
| Library | Microsoft.IdentityModel.Tokens | Latest |
| AI/ML | N/A | N/A |

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
Implement JWT token generation and validation service using RS256 asymmetric signing algorithm with RSA key pair from security/rsa-keys directory. Service generates JWT tokens with userId and role claims, 15-minute expiration, and validates incoming tokens for authentication middleware.

## Dependent Tasks
- None (Foundational service)

## Impacted Components
- MODIFY: src/backend/PatientAccess.Business/Services/JwtTokenService.cs
- MODIFY: src/backend/PatientAccess.Business/Services/IJwtTokenService.cs
- MODIFY: src/backend/PatientAccess.Web/Program.cs

## Implementation Plan
1. Load RSA private key from security/rsa-keys/private-key.xml for token signing
2. Load RSA public key from security/rsa-keys/public-key.xml for token validation
3. Implement IJwtTokenService.GenerateToken method with userId, role, email claims
4. Set token expiration to 15 minutes from issue time
5. Implement IJwtTokenService.ValidateToken method returning ClaimsPrincipal
6. Configure JWT Bearer authentication in Program.cs with RS256 algorithm
7. Add token issuer and audience configuration in appsettings.json

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Business/Services/JwtTokenService.cs | Implement RS256 JWT generation and validation |
| MODIFY | src/backend/PatientAccess.Business/Services/IJwtTokenService.cs | Define GenerateToken and ValidateToken methods |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure JWT Bearer authentication middleware |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add JWT issuer, audience configuration |

## External References
- **JWT RFC 7519**: https://datatracker.ietf.org/doc/html/rfc7519
- **RS256 Algorithm**: https://auth0.com/blog/rs256-vs-hs256-whats-the-difference/
- **ASP.NET Core JWT Bearer**: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn
- **System.IdentityModel.Tokens.Jwt**: https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/

## Implementation Checklist
- [x] Read RSA private key from security/rsa-keys/private-key.xml
- [x] Read RSA public key from security/rsa-keys/public-key.xml
- [x] Implement GenerateToken: create JwtSecurityToken with userId, role, email claims
- [x] Set token expiration to DateTime.UtcNow.AddMinutes(15)
- [x] Sign token using RsaSecurityKey with RS256 algorithm
- [x] Implement ValidateToken: validate signature, expiration, issuer, audience
- [x] Configure JWT Bearer authentication in Program.cs: TokenValidationParameters, RS256 algorithm
- [x] Add issuer ("PatientAccessPlatform") and audience ("PatientAccessAPI") to appsettings.json
