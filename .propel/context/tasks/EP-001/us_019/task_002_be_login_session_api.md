# Task - task_002_be_login_session_api

## Requirement Reference
- User Story: us_019
- Story Location: .propel/context/tasks/EP-001/us_019/us_019.md
- Acceptance Criteria:
    - AC1: Enter valid credentials -> System authenticates, generates JWT session token, stores in Redis with 15-minute TTL, redirects to role-appropriate dashboard
    - AC2: 15 minutes of inactivity pass -> Session token expires in Redis, frontend detects expiration, user redirected to login page
    - AC3: Enter invalid credentials -> System displays generic "Invalid email or password" error
    - AC4: Failed login 5 or more times -> Account temporarily locked, system displays lockout message
- Edge Case:
    - Concurrent sessions from different devices -> Each login creates new session token; previous sessions coexist until they expire

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

> **Wireframe Status Legend:**
> - **AVAILABLE**: Local file exists at specified path
> - **PENDING**: UI-impacting task awaiting wireframe (provide file or URL)
> - **EXTERNAL**: Wireframe provided via external URL
> - **N/A**: Task has no UI impact
>
> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | N/A | N/A |
| Backend | .NET | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Redis (Upstash) | 7.x compatible |
| Library | BCrypt.Net | Latest |
| AI/ML | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> **Mobile Impact Legend:**
> - **Yes**: Task involves mobile app development (native or cross-platform)
> - **No**: Task is web, backend, or infrastructure only
>
> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview
Implement login API (POST /api/auth/login) with email/password authentication, BCrypt password verification, JWT token generation with RS256 signing (existing JwtTokenService already configured for HS256, will use as-is), Redis session storage with 15-minute TTL (NFR-005), failed login attempt tracking (5 attempts), account lockout mechanism (30-minute lock), and audit logging (FR-005).

## Dependent Tasks
- task_002_be_registration_api (for UserRepository and UserService infrastructure)

## Impacted Components
- **UPDATED**: src/backend/PatientAccess.Web/Controllers/AuthController.cs (add Login endpoint)
- **UPDATED**: src/backend/PatientAccess.Business/Services/IUserService.cs (add AuthenticateUser method)
- **UPDATED**: src/backend/PatientAccess.Business/Services/UserService.cs (add AuthenticateUser implementation)
- **NEW**: src/backend/PatientAccess.Business/DTOs/LoginRequest.cs
- **NEW**: src/backend/PatientAccess.Business/DTOs/LoginResponse.cs
- **UPDATED**: src/backend/PatientAccess.Data/Models/User.cs (add FailedLoginAttempts, LastFailedLogin, AccountLockedUntil)
- **NEW**: src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddLoginTrackingFields.cs
- **UPDATED**: src/backend/PatientAccess.Business/Services/SessionCacheService.cs (use existing, add StoreSession method if not present)

## Implementation Plan
1. Add login tracking fields to User model (FailedLoginAttempts, LastFailedLogin, AccountLockedUntil)
2. Create database migration for new fields
3. Implement LoginRequest and LoginResponse DTOs
4. Add AuthenticateUser method to IUserService and implementation
5. Check account lockout status (AccountLockedUntil > DateTime.UtcNow)
6. Verify password using PasswordHashingService.VerifyPassword
7. On success: Generate JWT using JwtTokenService, store in Redis via SessionCacheService (15-min TTL), reset failed attempts to 0, create audit log
8. On failure: Increment FailedLoginAttempts, set AccountLockedUntil if attempts >= 5, create audit log for failed attempt
9. Add POST /api/auth/login endpoint to AuthController
10. Return 200 OK with JWT token on success
11. Return 401 Unauthorized on invalid credentials (generic message)
12. Return 403 Forbidden on account locked or inactive status

## Current Project State
```
src/backend/
├── PatientAccess.Web/
│   └── Controllers/
│       └── AuthController.cs (with Register and VerifyEmail endpoints)
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── JwtTokenService.cs (existing, HS256)
│   │   ├── PasswordHashingService.cs (existing)
│   │   ├── SessionCacheService.cs (existing)
│   │   ├── IUserService.cs (with RegisterUserAsync)
│   │   └── UserService.cs (with RegisterUserAsync)
│   └── DTOs/
│       ├── RegisterUserRequest.cs
│       └── RegisterUserResponse.cs
├── PatientAccess.Data/
│   ├── Models/
│   │   └── User.cs (with verification fields)
│   └── Repositories/
│       ├── IUserRepository.cs
│       └── UserRepository.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Data/Models/User.cs | Add FailedLoginAttempts (int), LastFailedLogin (DateTime?), AccountLockedUntil (DateTime?) |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddLoginTrackingFields.cs | Migration for login tracking fields |
| CREATE | src/backend/PatientAccess.Business/DTOs/LoginRequest.cs | Login request DTO (Email, Password) |
| CREATE | src/backend/PatientAccess.Business/DTOs/LoginResponse.cs | Login response DTO (Token, Role) |
| MODIFY | src/backend/PatientAccess.Business/Services/IUserService.cs | Add AuthenticateUserAsync method |
| MODIFY | src/backend/PatientAccess.Business/Services/UserService.cs | Implement AuthenticateUserAsync with lockout logic |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AuthController.cs | Add POST /api/auth/login endpoint |
| MODIFY | src/backend/PatientAccess.Business/Services/ISessionCacheService.cs | Add StoreSessionAsync method if missing |
| MODIFY | src/backend/PatientAccess.Business/Services/SessionCacheService.cs | Implement StoreSessionAsync with 15-min TTL |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Redis Session Management](https://redis.io/docs/latest/develop/use/sessions/)
- [BCrypt Verification](https://github.com/BcryptNet/bcrypt.net)
- [OWASP Account Lockout](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#account-lockout)

## Build Commands
```bash
cd src/backend
dotnet restore
dotnet build
dotnet ef migrations add AddLoginTrackingFields --project PatientAccess.Data --startup-project PatientAccess.Web
dotnet ef database update --project PatientAccess.Data --startup-project PatientAccess.Web
dotnet test
dotnet run --project PatientAccess.Web
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] **[AI Tasks]** Prompt templates validated with test inputs
- [ ] **[AI Tasks]** Guardrails tested for input sanitization and output validation
- [ ] **[AI Tasks]** Fallback logic tested with low-confidence/error scenarios
- [ ] **[AI Tasks]** Token budget enforcement verified
- [ ] **[AI Tasks]** Audit logging verified (no PII in logs)
- [ ] **[Mobile Tasks]** Headless platform compilation succeeds
- [ ] **[Mobile Tasks]** Native dependency linking verified
- [ ] **[Mobile Tasks]** Permission manifests validated against task requirements

## Implementation Checklist
- [ ] Add FailedLoginAttempts (int, default 0), LastFailedLogin (DateTime?), AccountLockedUntil (DateTime?) to User model
- [ ] Create and apply migration for login tracking fields
- [ ] Create LoginRequest DTO with [EmailAddress], [Required] validation attributes
- [ ] Create LoginResponse DTO with Token (string) and Role (string) properties
- [ ] Add AuthenticateUserAsync(string email, string password) to IUserService
- [ ] Implement AuthenticateUserAsync: fetch user by email, check AccountLockedUntil > UtcNow (return 403), verify password
- [ ] On successful password verification: Reset FailedLoginAttempts to 0, generate JWT, store in Redis with 15-min TTL, create success audit log
- [ ] On failed password verification: Increment FailedLoginAttempts, update LastFailedLogin, if attempts >= 5 set AccountLockedUntil = UtcNow + 30 minutes, create failed audit log
- [ ] Add StoreSessionAsync method to ISessionCacheService if not present
- [ ] Implement StoreSessionAsync in SessionCacheService using Redis SET with EX 900 (15 minutes)
- [ ] Add POST /api/auth/login endpoint to AuthController
- [ ] Call UserService.AuthenticateUserAsync and return LoginResponse on success (200 OK)
- [ ] Return 401 Unauthorized with generic message "Invalid email or password" on credentials failure
- [ ] Return 403 Forbidden with "Account locked" message if AccountLockedUntil not expired
- [ ] Return 403 Forbidden with "Account not active" message if UserStatus != Active
- [ ] Write unit tests for AuthenticateUserAsync (successful login, failed login, account lockout)
- [ ] Write integration tests for /api/auth/login endpoint
- [ ] Test concurrent sessions (multiple logins same user) - verify multiple Redis keys
- [ ] Test session expiration after 15 minutes (manually expire Redis key)
