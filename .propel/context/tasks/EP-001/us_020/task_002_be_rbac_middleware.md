# Task - task_002_be_rbac_middleware

## Requirement Reference
- User Story: us_020
- Story Location: .propel/context/tasks/EP-001/us_020/us_020.md
- Acceptance Criteria:
    - AC1: Patient accesses Staff-only endpoint -> API returns 403 Forbidden
    - AC2: Staff user attempts Admin-only pages -> API rejects request with 403
    - AC3: Patient requests another patient's data -> API returns 403, logs unauthorized access attempt (NFR-014 minimum necessary access)
    - AC4: JWT role claims re-validated from token on every request (NFR-006 RBAC middleware)
- Edge Cases:
    - User's role changed while active session -> Upon next API call, middleware re-validates role from token, rejects if role downgraded
    - Requests with tampered JWT role claims -> Signature verification rejects any tampered tokens with 401 Unauthorized

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
| Library | ASP.NET Core Identity | Built-in .NET 8 |
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
Implement RBAC middleware enforcing role-based access control (NFR-006) with custom [Authorize(Roles = "...")] attributes on controller actions, minimum necessary access validation (NFR-014) preventing cross-patient data access, audit logging for unauthorized access attempts (FR-005, NFR-007), and JWT signature verification (existing JwtValidationMiddleware).

## Dependent Tasks
- task_002_be_login_session_api (for JWT authentication infrastructure)

## Impacted Components
- **UPDATED**: src/backend/PatientAccess.Web/Middleware/JwtValidationMiddleware.cs (ensure role claims properly extracted)
- **NEW**: src/backend/PatientAccess.Web/Filters/MinimumNecessaryAccessFilter.cs
- **NEW**: src/backend/PatientAccess.Business/Services/IAuditService.cs
- **NEW**: src/backend/PatientAccess.Business/Services/AuditService.cs
- **UPDATED**: src/backend/PatientAccess.Web/Controllers/*.cs (add [Authorize(Roles = "...")] attributes)
- **NEW**: src/backend/PatientAccess.Data/Repositories/IAuditLogRepository.cs
- **NEW**: src/backend/PatientAccess.Data/Repositories/AuditLogRepository.cs

## Implementation Plan
1. Verify JwtValidationMiddleware extracts role claims from JWT and adds to HttpContext.User
2. Create IAuditService and AuditService for creating audit log entries
3. Implement IAuditLogRepository and AuditLogRepository for persisting audit logs
4. Create MinimumNecessaryAccessFilter for cross-patient data access validation
5. Add [Authorize(Roles = "Patient,Staff,Admin")] attributes to controller actions per role requirements
6. Implement resource-level authorization checking userId from JWT matches resource owner
7. Create audit log entries for 403 Forbidden responses (unauthorized access attempts)
8. Configure authorization policies in Program.cs
9. Write unit tests for RBAC enforcement and minimum necessary access

## Current Project State
```
src/backend/
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── AuthController.cs (Register, Login, VerifyEmail)
│   ├── Middleware/
│   │   ├── JwtValidationMiddleware.cs (validates JWT signature)
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── JwtTokenService.cs
│   │   ├── UserService.cs
│   │   └── SessionCacheService.cs
│   └── DTOs/
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── User.cs
│   │   ├── AuditLog.cs
│   │   └── ...
│   └── Repositories/
│       └── UserRepository.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Web/Middleware/JwtValidationMiddleware.cs | Ensure role claim added to ClaimsPrincipal |
| CREATE | src/backend/PatientAccess.Web/Filters/MinimumNecessaryAccessFilter.cs | Action filter validating userId matches resource owner |
| CREATE | src/backend/PatientAccess.Business/Services/IAuditService.cs | Audit service interface |
| CREATE | src/backend/PatientAccess.Business/Services/AuditService.cs | Audit service implementation creating audit logs |
| CREATE | src/backend/PatientAccess.Data/Repositories/IAuditLogRepository.cs | Audit log repository interface |
| CREATE | src/backend/PatientAccess.Data/Repositories/AuditLogRepository.cs | Audit log repository implementation |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register audit services, configure authorization policies |
| CREATE | src/backend/PatientAccess.Web/Controllers/PatientsController.cs | Example protected controller with [Authorize(Roles = "Staff,Admin")] |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [ASP.NET Core Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
- [Role-Based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)
- [Resource-Based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased)
- [OWASP RBAC](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html)

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
- [ ] Verify JwtValidationMiddleware adds role claim to HttpContext.User.Claims
- [ ] Create IAuditLogRepository with CreateAsync(AuditLog log) method
- [ ] Implement AuditLogRepository using PatientAccessDbContext
- [ ] Create IAuditService with LogUnauthorizedAccessAsync(userId, resourceType, resourceId, action) method
- [ ] Implement AuditService calling AuditLogRepository
- [ ] Register IAuditService and AuditService in Program.cs DI container
- [ ] Create MinimumNecessaryAccessFilter implementing IAsyncActionFilter
- [ ] In MinimumNecessaryAccessFilter: Extract userId from HttpContext.User, compare with resource owner userId from route/query
- [ ] If userId mismatch: Return 403 Forbidden, log unauthorized access via AuditService
- [ ] Add [Authorize(Roles = "Patient")] to patient-specific endpoints (e.g., GET /api/patients/me)
- [ ] Add [Authorize(Roles = "Staff,Admin")] to staff/admin endpoints (e.g., GET /api/patients, POST /api/appointments/walk-in)
- [ ] Add [Authorize(Roles = "Admin")] to admin-only endpoints (e.g., GET /api/users, POST /api/users)
- [ ] Create PatientsController with GET /api/patients/{id} endpoint protected by [Authorize(Roles = "Staff,Admin")]
- [ ] Configure authorization policies in Program.cs: builder.Services.AddAuthorization()
- [ ] Write unit tests: Test Patient accessing Staff endpoint returns 403, Test Staff accessing Admin endpoint returns 403
- [ ] Write integration tests: Test Patient accessing another patient's data returns 403 and creates audit log
- [ ] Test tampered JWT token: Modify role claim in token, verify 401 Unauthorized (signature validation failure)
