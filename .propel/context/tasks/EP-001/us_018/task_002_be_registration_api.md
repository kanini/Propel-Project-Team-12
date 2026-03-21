# Task - task_002_be_registration_api

## Requirement Reference
- User Story: us_018
- Story Location: .propel/context/tasks/EP-001/us_018/us_018.md
- Acceptance Criteria:
    - AC1: Enter valid personal information -> System creates account with status "pending" and sends verification email within 2 minutes
    - AC2: Receive verification email -> Click verification link -> Account activated and redirected to login page with success message
    - AC3: Email already registered -> System displays error message without revealing whether email exists
    - AC4: Verification link clicked after 24 hours -> System displays "Link expired" and offers to resend verification email
- Edge Case:
    - Multiple verification emails within 5 minutes -> Rate limiting allows max 3 requests per 5 minutes to prevent abuse

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
| Library | BCrypt.Net | Latest |
| Library | SendGrid or SMTP | Latest |
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
Implement RESTful registration API endpoints (POST /api/auth/register, GET /api/auth/verify-email) with email validation, password hashing using BCrypt cost factor 12 (TR-013), user creation with "pending" status, verification token generation with 24-hour expiration, email delivery within 2 minutes, rate limiting (3 requests per 5 minutes per email), and audit logging (FR-005).

## Dependent Tasks
- None (foundational task, uses existing User model and PasswordHashingService)

## Impacted Components
- **NEW**: src/backend/PatientAccess.Web/Controllers/AuthController.cs
- **NEW**: src/backend/PatientAccess.Business/Services/IEmailService.cs
- **NEW**: src/backend/PatientAccess.Business/Services/EmailService.cs
- **NEW**: src/backend/PatientAccess.Business/Services/IUserService.cs
- **NEW**: src/backend/PatientAccess.Business/Services/UserService.cs
- **NEW**: src/backend/PatientAccess.Business/DTOs/RegisterUserRequest.cs
- **NEW**: src/backend/PatientAccess.Business/DTOs/RegisterUserResponse.cs
- **NEW**: src/backend/PatientAccess.Business/DTOs/VerifyEmailRequest.cs
- **NEW**: src/backend/PatientAccess.Data/Repositories/IUserRepository.cs
- **NEW**: src/backend/PatientAccess.Data/Repositories/UserRepository.cs
- **UPDATED**: src/backend/PatientAccess.Data/Models/User.cs (add VerificationToken, VerificationTokenExpiry, VerifiedAt)
- **UPDATED**: src/backend/PatientAccess.Web/Program.cs (register services, rate limiting middleware)

## Implementation Plan
1. Add verification token fields to User model (VerificationToken, VerificationTokenExpiry, VerifiedAt)
2. Create database migration for new User fields
3. Implement IUserRepository and UserRepository with methods: GetByEmail, Create, UpdateVerificationStatus
4. Implement IEmailService and EmailService with SendVerificationEmail method using SendGrid or SMTP
5. Implement IUserService and UserService with RegisterUser and VerifyEmail methods
6. Create DTOs: RegisterUserRequest, RegisterUserResponse, VerifyEmailRequest
7. Build AuthController with POST /api/auth/register and GET /api/auth/verify-email endpoints
8. Integrate PasswordHashingService for password hashing (existing, cost factor 12)
9. Generate secure verification token using GUID
10. Implement rate limiting using ASP.NET Core RateLimiter (3 requests per 5 minutes per email)
11. Add error handling for duplicate email (return 409 Conflict without revealing existence)
12. Create audit logs for registration attempts (success/failure)
13. Set verification token expiration to 24 hours
14. Send verification email via background task (non-blocking)

## Current Project State
```
src/backend/
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── README.md
│   ├── Middleware/
│   │   ├── JwtValidationMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── JwtTokenService.cs
│   │   ├── PasswordHashingService.cs
│   │   ├── SessionCacheService.cs
│   │   └── I*.cs (interfaces)
│   └── DTOs/
│       └── README.md
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── User.cs
│   │   ├── UserRole.cs
│   │   ├── UserStatus.cs
│   │   └── ...
│   ├── Repositories/
│   ├── Configurations/
│   └── PatientAccessDbContext.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Data/Models/User.cs | Add VerificationToken, VerificationTokenExpiry, VerifiedAt properties |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddUserVerificationFields.cs | Migration adding verification fields to Users table |
| CREATE | src/backend/PatientAccess.Data/Repositories/IUserRepository.cs | User repository interface |
| CREATE | src/backend/PatientAccess.Data/Repositories/UserRepository.cs | User repository implementation with EF Core |
| CREATE | src/backend/PatientAccess.Business/DTOs/RegisterUserRequest.cs | Registration request DTO with validation attributes |
| CREATE | src/backend/PatientAccess.Business/DTOs/RegisterUserResponse.cs | Registration response DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/VerifyEmailRequest.cs | Email verification request DTO |
| CREATE | src/backend/PatientAccess.Business/Services/IEmailService.cs | Email service interface |
| CREATE | src/backend/PatientAccess.Business/Services/EmailService.cs | Email service implementation with SendGrid/SMTP |
| CREATE | src/backend/PatientAccess.Business/Services/IUserService.cs | User service interface |
| CREATE | src/backend/PatientAccess.Business/Services/UserService.cs | User service with RegisterUser and VerifyEmail logic |
| CREATE | src/backend/PatientAccess.Web/Controllers/AuthController.cs | Authentication controller with register and verify endpoints |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register new services, configure rate limiting |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add email service configuration (SMTP/SendGrid settings) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [SendGrid .NET Client](https://github.com/sendgrid/sendgrid-csharp)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [BCrypt.Net Documentation](https://github.com/BcryptNet/bcrypt.net)
- [OWASP Email Enumeration Prevention](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#preventing-online-attacks)

## Build Commands
```bash
# Navigate to backend
cd src/backend

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Create migration
dotnet ef migrations add AddUserVerificationFields --project PatientAccess.Data --startup-project PatientAccess.Web

# Apply migration
dotnet ef database update --project PatientAccess.Data --startup-project PatientAccess.Web

# Run API
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
- [ ] Add VerificationToken (string, nullable), VerificationTokenExpiry (DateTime, nullable), VerifiedAt (DateTime, nullable) to User model
- [ ] Create and apply EF Core migration for verification fields
- [ ] Implement IUserRepository with GetByEmailAsync, CreateAsync, UpdateAsync methods
- [ ] Implement UserRepository using PatientAccessDbContext
- [ ] Create RegisterUserRequest DTO with validation: [EmailAddress], [Required], [StringLength] attributes
- [ ] Create RegisterUserResponse DTO with message and userId
- [ ] Create VerifyEmailRequest DTO with token parameter
- [ ] Implement IEmailService with SendVerificationEmailAsync method
- [ ] Implement EmailService using SendGrid SDK or SMTP client
- [ ] Configure email settings in appsettings.json (sender, API key, template)
- [ ] Implement IUserService interface (RegisterUserAsync, VerifyEmailAsync)
- [ ] Implement UserService.RegisterUserAsync: check email exists, hash password, generate GUID token, set 24h expiry, create user with "Pending" status, queue email
- [ ] Implement UserService.VerifyEmailAsync: validate token, check expiry, update user status to "Active", set VerifiedAt
- [ ] Create AuthController with [ApiController] and [Route("api/auth")]
- [ ] Implement POST /api/auth/register endpoint returning 201 Created on success, 409 Conflict on duplicate email
- [ ] Implement GET /api/auth/verify-email?token={token} endpoint returning 200 OK or 400 Bad Request
- [ ] Configure rate limiting in Program.cs: fixed window, 3 requests per 5 minutes, per email address partitioning
- [ ] Add audit logging for registration attempts (success/failure) using AuditLog model
- [ ] Handle expired tokens with helpful error message and resend option
- [ ] Write unit tests for UserService (mock repository)
- [ ] Write integration tests for AuthController (mock email service)
- [ ] Test rate limiting with multiple requests
- [ ] Test email delivery (use MailTrap or SendGrid sandbox for testing)
