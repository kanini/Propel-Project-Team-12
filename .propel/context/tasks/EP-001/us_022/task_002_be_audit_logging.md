# Task - task_002_be_audit_logging

## Requirement Reference
- User Story: us_022
- Story Location: .propel/context/tasks/EP-001/us_022/us_022.md
- Acceptance Criteria:
    - AC1: User logs in successfully -> Immutable audit log entry created with user ID, timestamp, action type (login), IP address, user agent
    - AC2: Login attempt fails -> Audit log entry records failed attempt with email (hashed), timestamp, IP, failure reason
    - AC3: Session timeout occurs -> Audit log entry records session timeout with user ID and last activity timestamp
- Edge Cases:
    - Audit log service temporarily unavailable -> Auth events succeed (audit is non-blocking) but failures logged to application error logs for retry

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
| Library | EF Core | 8.x |
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
Implement comprehensive audit logging for authentication events (FR-005, NFR-007) creating immutable audit log entries for successful logins, failed login attempts, session timeouts, with IP address and user agent capture, email hashing for privacy, non-blocking audit calls (try-catch wrapper), and session refresh endpoint (POST /api/auth/refresh-session) resetting Redis TTL to 15 minutes.

## Dependent Tasks
- task_002_be_login_session_api (for authentication infrastructure)
- task_002_be_rbac_middleware (for AuditService)

## Impacted Components
- **UPDATED**: src/backend/PatientAccess.Web/Controllers/AuthController.cs (add audit calls to Login, add RefreshSession endpoint)
- **UPDATED**: src/backend/PatientAccess.Business/Services/IUserService.cs (add RefreshSessionAsync)
- **UPDATED**: src/backend/PatientAccess.Business/Services/UserService.cs (implement RefreshSessionAsync)
- **UPDATED**: src/backend/PatientAccess.Business/Services/IAuditService.cs (add LogAuthenticationEvent methods)
- **UPDATED**: src/backend/PatientAccess.Business/Services/AuditService.cs (implement auth logging methods)
- **NEW**: src/backend/PatientAccess.Web/Middleware/AuditLoggingMiddleware.cs (capture IP and user agent)

## Implementation Plan
1. Create AuditLoggingMiddleware extracting IP address (X-Forwarded-For or RemoteIpAddress) and User-Agent from HttpContext
2. Add LogSuccessfulLogin, LogFailedLogin, LogSessionTimeout to IAuditService
3. Implement audit methods in AuditService with try-catch (non-blocking), log failures to ILogger
4. Update AuthController.Login: On success call LogSuccessfulLogin, on failure call LogFailedLogin (hash email for failed attempts)
5. Add RefreshSessionAsync to IUserService extending Redis session TTL to 15 minutes, creating audit log for session extension
6. Implement POST /api/auth/refresh-session endpoint in AuthController calling RefreshSessionAsync
7. Add background job detecting expired sessions (via Redis key expiration events or periodic scan) and creating session timeout audit logs
8. Ensure AuditLog table has database trigger preventing UPDATE/DELETE (immutability per AD-007)
9. Hash email in failed login audit logs using SHA256
10. Write unit tests for audit logging (mock AuditService to verify calls)

## Current Project State
```
src/backend/
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── AuthController.cs (Register, Login, VerifyEmail)
│   ├── Middleware/
│   │   ├── JwtValidationMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── AuditService.cs (LogUnauthorizedAccessAsync)
│   │   ├── UserService.cs
│   │   └── SessionCacheService.cs
│   └── DTOs/
├── PatientAccess.Data/
│   ├── Models/
│   │   └── AuditLog.cs
│   └── Repositories/
│       └── AuditLogRepository.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Middleware/AuditLoggingMiddleware.cs | Middleware capturing IP and user agent for audit context |
| MODIFY | src/backend/PatientAccess.Business/Services/IAuditService.cs | Add LogSuccessfulLoginAsync, LogFailedLoginAsync, LogSessionTimeoutAsync |
| MODIFY | src/backend/PatientAccess.Business/Services/AuditService.cs | Implement authentication audit logging methods |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AuthController.cs | Add audit calls to Login method, add RefreshSession endpoint |
| MODIFY | src/backend/PatientAccess.Business/Services/IUserService.cs | Add RefreshSessionAsync method |
| MODIFY | src/backend/PatientAccess.Business/Services/UserService.cs | Implement RefreshSessionAsync |
| MODIFY | src/backend/PatientAccess.Business/Services/ISessionCacheService.cs | Add ExtendSessionAsync method |
| MODIFY | src/backend/PatientAccess.Business/Services/SessionCacheService.cs | Implement ExtendSessionAsync with Redis EXPIRE command |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register AuditLoggingMiddleware |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [HashAlgorithm Class (SHA256)](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256)
- [Redis EXPIRE Command](https://redis.io/commands/expire/)
- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Immutable Audit Logs with Triggers](https://www.postgresql.org/docs/current/trigger-definition.html)

## Build Commands
```bash
cd src/backend
dotnet restore
dotnet build
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
- [ ] Create AuditLoggingMiddleware extracting IpAddress from HttpContext.Connection.RemoteIpAddress or X-Forwarded-For header
- [ ] Extract UserAgent from HttpContext.Request.Headers["User-Agent"]
- [ ] Store IpAddress and UserAgent in HttpContext.Items for access in controllers
- [ ] Add LogSuccessfulLoginAsync(Guid userId, string ipAddress, string userAgent) to IAuditService
- [ ] Add LogFailedLoginAsync(string emailHash, string ipAddress, string userAgent, string failureReason) to IAuditService
- [ ] Add LogSessionTimeoutAsync(Guid userId, DateTime lastActivityTime) to IAuditService
- [ ] Add LogSessionExtensionAsync(Guid userId) to IAuditService
- [ ] Implement audit methods in AuditService creating AuditLog entries with ActionType = "Login", "LoginFailed", "SessionTimeout", "SessionExtended"
- [ ] Wrap audit calls in try-catch, log exceptions to ILogger<AuditService> without throwing (non-blocking)
- [ ] Hash email using SHA256.HashData(Encoding.UTF8.GetBytes(email)) for failed login audit logs
- [ ] Update AuthController.Login: After successful authentication, call LogSuccessfulLoginAsync
- [ ] Update AuthController.Login: On authentication failure, call LogFailedLoginAsync with hashed email
- [ ] Add ExtendSessionAsync(string sessionKey) to ISessionCacheService
- [ ] Implement ExtendSessionAsync: Call Redis EXPIRE command setting TTL to 900 seconds (15 minutes)
- [ ] Add RefreshSessionAsync(Guid userId) to IUserService
- [ ] Implement RefreshSessionAsync: Construct session key, call ExtendSessionAsync, create audit log via LogSessionExtensionAsync
- [ ] Add POST /api/auth/refresh-session endpoint to AuthController
- [ ] Extract userId from JWT claims in RefreshSession endpoint
- [ ] Call UserService.RefreshSessionAsync(userId) and return 200 OK
- [ ] Register AuditLoggingMiddleware in Program.cs before authentication middleware
- [ ] Verify AuditLog table has immutability trigger (check existing migration: AddAuditLogImmutabilityTriggers)
- [ ] Write unit test for LogSuccessfulLoginAsync verifying AuditLog created
- [ ] Write unit test for LogFailedLoginAsync verifying email is hashed
- [ ] Write integration test for Login endpoint verifying audit log entry created
- [ ] Test audit service failure: Mock AuditLogRepository to throw exception, verify login still succeeds
- [ ] Test RefreshSession endpoint: Call endpoint, verify Redis TTL extended, audit log created
