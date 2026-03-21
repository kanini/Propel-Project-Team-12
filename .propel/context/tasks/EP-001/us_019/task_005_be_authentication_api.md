# Task - TASK_005

## Requirement Reference
- User Story: US_019
- Story Location: .propel/context/tasks/EP-001/us_019/us_019.md
- Acceptance Criteria:
    - AC1: System authenticates user, generates JWT, stores in Redis with 15-minute TTL
    - AC2: Session expires after 15 minutes, user redirected to login
    - AC4: Account locked after 5 failed login attempts
    - AC5: JWT claims include user role for RBAC
- Edge Case:
    - Concurrent sessions from different devices coexist independently

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
| Library | StackExchange.Redis | Latest |
| Library | BCrypt.Net-Next | Latest |
| Library | System.IdentityModel.Tokens.Jwt | Latest |
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
Implement backend authentication API endpoint that validates user credentials using BCrypt, generates JWT tokens with RS256 signing including user ID and role claims, stores session token in Upstash Redis with 15-minute TTL, and implements account lockout after 5 failed login attempts. The service integrates with existing JwtTokenService and SessionCacheService.

## Dependent Tasks
- TASK_003 (User table must exist)
- TASK_006 (JwtTokenService must be implemented)

## Impacted Components
- NEW: src/backend/PatientAccess.Web/Controllers/AuthController.cs (Login endpoint)
- MODIFY: src/backend/PatientAccess.Business/Services/AuthService.cs (add Login method)
- NEW: src/backend/PatientAccess.Business/Services/SessionCacheService.cs
- NEW: src/backend/PatientAccess.Business/Interfaces/ISessionCacheService.cs
- NEW: src/backend/PatientAccess.Business/DTOs/LoginRequestDto.cs
- NEW: src/backend/PatientAccess.Business/DTOs/LoginResponseDto.cs

## Implementation Plan
1. Create LoginRequestDto with Email and Password properties
2. Create LoginResponseDto with Token, UserId, Role, ExpiresAt properties
3. Create ISessionCacheService interface for Redis session management
4. Implement SessionCacheService using StackExchange.Redis with 15-minute TTL
5. Implement AuthService.Login method: validate credentials, check lockout, generate token, cache session
6. Create AuthController.Login POST endpoint with rate limiting
7. Implement failed login counter in Redis with 5-attempt lockout threshold
8. Add account lockout flag in User entity or separate cache entry

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Web/Controllers/AuthController.cs | Add POST /api/auth/login endpoint |
| MODIFY | src/backend/PatientAccess.Business/Services/AuthService.cs | Add Login method with credential validation |
| CREATE | src/backend/PatientAccess.Business/Services/SessionCacheService.cs | Redis session management with 15-min TTL |
| CREATE | src/backend/PatientAccess.Business/Interfaces/ISessionCacheService.cs | Session cache interface |
| CREATE | src/backend/PatientAccess.Business/DTOs/LoginRequestDto.cs | Login request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/LoginResponseDto.cs | Login response DTO |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register SessionCacheService and configure Redis |

## External References
- **StackExchange.Redis**: https://stackexchange.github.io/StackExchange.Redis/
- **Upstash Redis**: https://docs.upstash.com/redis
- **ASP.NET Core Authentication**: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/
- **Account Lockout Pattern**: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#account-lockout

## Implementation Checklist
- [x] Create LoginRequestDto and LoginResponseDto
- [x] Implement ISessionCacheService with StoreSession, GetSession, DeleteSession methods - *Already existed*
- [x] Create SessionCacheService using StackExchange.Redis (Upstash connection string) - *Already existed*
- [x] Implement AuthService.Login: retrieve user by email, verify password with BCrypt, check lockout
- [x] Generate JWT token with userId, role claims using JwtTokenService
- [x] Store session token in Redis with 15-minute sliding expiration
- [x] Implement failed login counter: increment on failure, reset on success, lock after 5 attempts
- [x] Create AuthController.Login endpoint with [AllowAnonymous] attribute
- [x] Return generic error message for invalid credentials or locked accounts
