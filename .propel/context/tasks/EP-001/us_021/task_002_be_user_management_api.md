# Task - task_002_be_user_management_api

## Requirement Reference
- User Story: us_021
- Story Location: .propel/context/tasks/EP-001/us_021/us_021.md
- Acceptance Criteria:
    - AC1: Click "Create User" -> System creates Staff/Admin user account and sends activation email upon save
    - AC2: Edit user account -> Changes persisted, audit log entry created
    - AC3: Click "Deactivate" -> Account status changes to inactive, all active sessions terminated, future login blocked
    - AC4: User list page loads -> All Staff and Admin users returned with sorting and filtering
    - AC5: Admin attempts self-deactivation -> System prevents with error
- Edge Cases:
    - Create user with duplicate email -> Return 409 Conflict
    - Deactivate last Admin -> Prevent if would leave zero active Admins

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
Implement Admin-only user management API endpoints: GET /api/users (list with filtering/sorting), POST /api/users (create Staff/Admin), PUT /api/users/{id} (update), DELETE /api/users/{id} (deactivate), GET /api/users/admin-count (check active Admins), with [Authorize(Roles = "Admin")], audit logging, self-deactivation prevention, last Admin check, session termination on deactivation, and activation email sending.

## Dependent Tasks
- task_002_be_registration_api (for UserService and EmailService)
- task_002_be_rbac_middleware (for [Authorize(Roles = "Admin")] enforcement)

## Impacted Components
- **NEW**: src/backend/PatientAccess.Web/Controllers/UsersController.cs
- **UPDATED**: src/backend/PatientAccess.Business/Services/IUserService.cs (add CRUD methods)
- **UPDATED**: src/backend/PatientAccess.Business/Services/UserService.cs (implement CRUD methods)
- **NEW**: src/backend/PatientAccess.Business/DTOs/UserDto.cs
- **NEW**: src/backend/PatientAccess.Business/DTOs/CreateUserRequest.cs
- **NEW**: src/backend/PatientAccess.Business/DTOs/UpdateUserRequest.cs
- **UPDATED**: src/backend/PatientAccess.Data/Repositories/IUserRepository.cs (add GetAllAsync, GetActiveAdminCountAsync)
- **UPDATED**: src/backend/PatientAccess.Data/Repositories/UserRepository.cs (implement new methods)
- **UPDATED**: src/backend/PatientAccess.Business/Services/ISessionCacheService.cs (add InvalidateUserSessionsAsync)
- **UPDATED**: src/backend/PatientAccess.Business/Services/SessionCacheService.cs (implement session invalidation)

## Implementation Plan
1. Add GetAllAsync (with filtering), GetActiveAdminCountAsync to IUserRepository
2. Implement new repository methods in UserRepository
3. Create UserDto, CreateUserRequest, UpdateUserRequest DTOs
4. Add CreateUserAsync, UpdateUserAsync, DeactivateUserAsync, GetAllUsersAsync, GetActiveAdminCountAsync to IUserService
5. Implement UserService methods with business logic: duplicate check, self-deactivate check, last Admin check
6. Add InvalidateUserSessionsAsync to ISessionCacheService to delete Redis keys for user
7. Implement session invalidation using Redis SCAN + DEL pattern
8. Create UsersController with [Authorize(Roles = "Admin")] and CRUD endpoints
9. Send activation email on user creation (reuse EmailService)
10. Create audit logs for create, update, deactivate actions
11. Write unit and integration tests

## Current Project State
```
src/backend/
├── PatientAccess.Web/
│   └── Controllers/
│       └── AuthController.cs
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── UserService.cs (RegisterUserAsync, VerifyEmailAsync, AuthenticateUserAsync)
│   │   ├── EmailService.cs
│   │   ├── SessionCacheService.cs
│   │   └── AuditService.cs
│   └── DTOs/
│       ├── RegisterUserRequest.cs
│       └── LoginRequest.cs
├── PatientAccess.Data/
│   ├── Repositories/
│   │   ├── UserRepository.cs (GetByEmailAsync, CreateAsync, UpdateAsync)
│   │   └── AuditLogRepository.cs
│   └── Models/
│       └── User.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Controllers/UsersController.cs | Admin user management controller |
| CREATE | src/backend/PatientAccess.Business/DTOs/UserDto.cs | User data transfer object (exclude PasswordHash) |
| CREATE | src/backend/PatientAccess.Business/DTOs/CreateUserRequest.cs | Create user request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/UpdateUserRequest.cs | Update user request DTO |
| MODIFY | src/backend/PatientAccess.Business/Services/IUserService.cs | Add CreateUserAsync, UpdateUserAsync, DeactivateUserAsync, GetAllUsersAsync, GetActiveAdminCountAsync |
| MODIFY | src/backend/PatientAccess.Business/Services/UserService.cs | Implement new service methods |
| MODIFY | src/backend/PatientAccess.Data/Repositories/IUserRepository.cs | Add GetAllAsync, GetActiveAdminCountAsync, UpdateStatusAsync |
| MODIFY | src/backend/PatientAccess.Data/Repositories/UserRepository.cs | Implement new repository methods |
| MODIFY | src/backend/PatientAccess.Business/Services/ISessionCacheService.cs | Add InvalidateUserSessionsAsync |
| MODIFY | src/backend/PatientAccess.Business/Services/SessionCacheService.cs | Implement session invalidation |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [EF Core Filtering and Sorting](https://learn.microsoft.com/en-us/ef/core/querying/)
- [Redis SCAN Command](https://redis.io/commands/scan/)
- [ASP.NET Core Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)

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
- [ ] Create UserDto with UserId, Name, Email, Role, Status, CreatedAt, LastLogin (exclude PasswordHash)
- [ ] Create CreateUserRequest DTO with Name, Email, Role validation attributes
- [ ] Create UpdateUserRequest DTO with Name, Role (email immutable after creation)
- [ ] Add GetAllAsync(string? search, string? sortBy, bool ascending) to IUserRepository
- [ ] Implement GetAllAsync with EF Core Where (search filter) and OrderBy (sorting)
- [ ] Add GetActiveAdminCountAsync() to IUserRepository returning count of Active Admin users
- [ ] Add UpdateStatusAsync(Guid userId, UserStatus status) to IUserRepository
- [ ] Implement repository methods in UserRepository
- [ ] Add CreateUserAsync, UpdateUserAsync, DeactivateUserAsync, GetAllUsersAsync, GetActiveAdminCountAsync to IUserService
- [ ] Implement CreateUserAsync: Check email exists (return 409), generate random password, hash password, create user with "Active" status, send activation email
- [ ] Implement UpdateUserAsync: Fetch user, update name/role, save, create audit log
- [ ] Implement DeactivateUserAsync: Check if userId == currentUserId (return 400), check GetActiveAdminCountAsync <= 1 and user is Admin (return 400), update status to "Inactive", invalidate sessions, create audit log
- [ ] Add InvalidateUserSessionsAsync(Guid userId) to ISessionCacheService
- [ ] Implement InvalidateUserSessionsAsync: Use Redis SCAN to find keys matching pattern "session:{userId}:*", delete all matching keys
- [ ] Create UsersController with [ApiController], [Route("api/users")], [Authorize(Roles = "Admin")]
- [ ] Implement GET /api/users?search={search}&sortBy={sortBy}&ascending={bool} endpoint
- [ ] Implement POST /api/users endpoint creating Staff/Admin users
- [ ] Implement PUT /api/users/{id} endpoint updating user details
- [ ] Implement DELETE /api/users/{id} endpoint deactivating user (soft delete)
- [ ] Implement GET /api/users/admin-count endpoint returning active Admin count
- [ ] Write unit tests for UserService methods (mock repository)
- [ ] Write integration tests for UsersController endpoints
- [ ] Test self-deactivation prevention: Admin attempts to deactivate themselves, verify 400 error
- [ ] Test last Admin prevention: Attempt to deactivate last Admin, verify 400 error
- [ ] Test session invalidation: Deactivate user with active session, verify session no longer valid
