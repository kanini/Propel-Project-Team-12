# Task - TASK_002

## Requirement Reference
- User Story: US_018
- Story Location: .propel/context/tasks/EP-001/us_018/us_018.md
- Acceptance Criteria:
    - AC1: System creates account with status "pending" and sends verification email within 2 minutes
    - AC2: Verification link activates account and redirects to login with success message
    - AC3: System displays error for duplicate email and offers password recovery
    - AC5: Expired verification link displays "Link expired" and offers resend
- Edge Case:
    - Rate limiting allows max 3 verification email requests per 5 minutes

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
| Library | Entity Framework Core | 8.x |
| Library | BCrypt.Net-Next | Latest |
| AI/ML | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

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
Implement backend API endpoints and business logic for patient registration, including email validation, secure password hashing using BCrypt (cost factor 12), verification email generation, email uniqueness validation, and rate limiting for verification email requests. The service handles account creation, email verification link generation with 24-hour expiration, and integration with email service for sending verification emails.

## Dependent Tasks
- TASK_003 (Database user schema must exist before this task)

## Impacted Components
- NEW: src/backend/PatientAccess.Web/Controllers/AuthController.cs (RegisterUser endpoint)
- NEW: src/backend/PatientAccess.Business/Services/AuthService.cs
- NEW: src/backend/PatientAccess.Business/Services/PasswordHashingService.cs
- NEW: src/backend/PatientAccess.Business/Services/EmailService.cs
- NEW: src/backend/PatientAccess.Business/Interfaces/IAuthService.cs
- NEW: src/backend/PatientAccess.Business/Interfaces/IPasswordHashingService.cs
- NEW: src/backend/PatientAccess.Business/Interfaces/IEmailService.cs
- NEW: src/backend/PatientAccess.Business/DTOs/RegisterUserRequestDto.cs
- NEW: src/backend/PatientAccess.Business/DTOs/RegisterUserResponseDto.cs

## Implementation Plan
1. Create RegisterUserRequestDto with validation attributes for name, DOB, email, phone, password
2. Create RegisterUserResponseDto with userId and status fields
3. Implement IPasswordHashingService with BCrypt.Net-Next using cost factor 12
4. Create IEmailService interface with SendVerificationEmail method
5. Implement EmailService using SendGrid or SMTP with email template for verification
6. Create IAuthService interface with RegisterUser method signature
7. Implement AuthService business logic: check email uniqueness, hash password, create User entity, generate verification token, send email
8. Create AuthController.RegisterUser POST endpoint with validation filter and error handling
9. Implement rate limiting middleware for verification email requests (max 3 per 5 minutes per email)
10. Add VerifyEmail GET endpoint to handle verification link clicks with token validation and account activation

## Current Project State
```
src/backend/
├── PatientAccess.Web/
│   ├── Controllers/ (to be extended)
│   └── Middleware/ (to be extended)
├── PatientAccess.Business/
│   ├── Services/ (to be created)
│   ├── Interfaces/ (to be created)
│   └── DTOs/ (to be created)
└── PatientAccess.Data/
    └── Repositories/ (existing)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Controllers/AuthController.cs | POST /api/auth/register and GET /api/auth/verify endpoints |
| CREATE | src/backend/PatientAccess.Web/Middleware/RateLimitingMiddleware.cs | Rate limiting for email verification requests |
| CREATE | src/backend/PatientAccess.Business/Services/AuthService.cs | Registration business logic with email validation |
| CREATE | src/backend/PatientAccess.Business/Services/PasswordHashingService.cs | BCrypt password hashing with cost factor 12 |
| CREATE | src/backend/PatientAccess.Business/Services/EmailService.cs | Email sending via SendGrid/SMTP |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAuthService.cs | Auth service interface |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IPasswordHashingService.cs | Password hashing interface |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IEmailService.cs | Email service interface |
| CREATE | src/backend/PatientAccess.Business/DTOs/RegisterUserRequestDto.cs | Registration request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/RegisterUserResponseDto.cs | Registration response DTO |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register services in DI container |

## External References
- **BCrypt.Net-Next Documentation**: https://github.com/BcryptNet/bcrypt.net
- **ASP.NET Core Rate Limiting**: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit
- **SendGrid .NET SDK**: https://github.com/sendgrid/sendgrid-csharp
- **Email Verification Best Practices**: https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html
- **ASP.NET Core Security**: https://learn.microsoft.com/en-us/aspnet/core/security/

## Build Commands
```powershell
# Navigate to backend solution directory
cd src/backend

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run PatientAccess.Web project
dotnet run --project PatientAccess.Web

# Run unit tests
dotnet test PatientAccess.Tests
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [ ] Rate limiting verified (max 3 emails / 5 min)
- [ ] BCrypt cost factor 12 confirmed
- [ ] Email delivery tested (verification link received)
- [ ] Token expiration tested (24-hour TTL)
- [ ] Duplicate email validation tested

## Implementation Checklist
- [x] Create RegisterUserRequestDto with System.ComponentModel.DataAnnotations
- [x] Implement PasswordHashingService using BCrypt.Net-Next (cost factor 12) - *Already existed*
- [x] Create EmailService with SendGrid integration or SMTP fallback
- [x] Implement AuthService.RegisterUser method with email uniqueness check
- [x] Generate verification token (cryptographically secure random string, 32 bytes)
- [x] Store verification token hash with 24-hour expiration in database
- [x] Create AuthController.RegisterUser POST endpoint with [ValidateModel] filter
- [x] Implement rate limiting middleware (3 requests / 5 min per email address)
- [x] Create AuthController.VerifyEmail GET endpoint with token validation
