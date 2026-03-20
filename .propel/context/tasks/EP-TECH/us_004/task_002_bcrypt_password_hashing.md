# Task - task_002_bcrypt_password_hashing

## Requirement Reference
- User Story: us_004
- Story Location: .propel/context/tasks/EP-TECH/us_004/us_004.md
- Acceptance Criteria:
    - AC-2: Password hashed using BCrypt with cost factor 12 and can be verified against original password
- Edge Case:
    - None specified

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
| Library | BCrypt.Net-Next | 4.x |

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
Implement secure password hashing using BCrypt algorithm with cost factor 12 to balance security and performance. Create password hashing service with methods for hashing plaintext passwords during registration and verifying passwords during login. BCrypt's adaptive hash function with configurable work factor provides resistance against brute-force attacks and future-proofs password security as computing power increases.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project structure

## Impacted Components
- **NEW** src/backend/PatientAccess.Business/Services/IPasswordHashingService.cs - Password hashing service interface
- **NEW** src/backend/PatientAccess.Business/Services/PasswordHashingService.cs - BCrypt implementation with cost factor 12
- **MODIFY** src/backend/PatientAccess.Business/PatientAccess.Business.csproj - Add BCrypt.Net-Next package

## Implementation Plan
1. **Install BCrypt NuGet Package**: Add `BCrypt.Net-Next` (version 4.x) to Business project for password hashing
2. **Create IPasswordHashingService Interface**: Define contract with `HashPassword(string plaintext)` and `VerifyPassword(string plaintext, string hash)` methods
3. **Implement PasswordHashingService**: Use `BCrypt.HashPassword` with work factor 12 and `BCrypt.Verify` for validation
4. **Configure Cost Factor**: Set BCrypt work factor to 12 (TR-013 requirement) balancing security and performance
5. **Register Service in DI**: Add password hashing service to dependency injection container with Singleton lifetime
6. **Document Security Rationale**: Explain work factor selection and BCrypt advantages over alternatives (PBKDF2, Argon2)
7. **Create Usage Examples**: Document service usage patterns for registration and login flows

## Current Project State
```
Propel-Project-Team-12/
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   │   └── Program.cs
│   ├── PatientAccess.Business/
│   │   ├── Services/
│   │   │   ├── IJwtTokenService.cs
│   │   │   └── JwtTokenService.cs
│   │   └── PatientAccess.Business.csproj
│   └── PatientAccess.Data/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add BCrypt.Net-Next NuGet package reference |
| CREATE | src/backend/PatientAccess.Business/Services/IPasswordHashingService.cs | Password hashing service interface |
| CREATE | src/backend/PatientAccess.Business/Services/PasswordHashingService.cs | BCrypt implementation with cost factor 12 |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register IPasswordHashingService as Singleton in DI container |
| CREATE | docs/SECURITY.md | Security documentation including password hashing strategy |

## External References
- BCrypt.Net-Next Documentation: https://github.com/BcryptNet/bcrypt.net
- BCrypt Work Factor Recommendations: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#bcrypt
- OWASP Password Storage Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
- BCrypt vs Argon2 vs PBKDF2: https://security.stackexchange.com/questions/193351/in-2018-what-is-the-recommended-hash-to-store-passwords-bcrypt-scrypt-argon2

## Build Commands
```bash
# Install BCrypt.Net-Next package
cd src/backend/PatientAccess.Business
dotnet add package BCrypt.Net-Next

# Restore and build
cd ..
dotnet restore
dotnet build
```

## Implementation Validation Strategy
- [ ] Unit tests pass (create tests for HashPassword and VerifyPassword methods)
- [ ] Integration tests pass (N/A for utility service)
- [ ] `HashPassword` generates unique hash for same input (salt is random)
- [ ] `VerifyPassword` returns true for correct password against its hash
- [ ] `VerifyPassword` returns false for incorrect password
- [ ] Work factor 12 confirmed by examining generated hash string (starts with `$2a$12$`)
- [ ] Hash generation time is acceptable (< 500ms on development machine)
- [ ] Service registered in DI container with Singleton lifetime

## Implementation Checklist
- [X] Add BCrypt.Net-Next NuGet package to PatientAccess.Business project
- [X] Create `IPasswordHashingService` interface with HashPassword and VerifyPassword methods
- [X] Implement `PasswordHashingService` using `BCrypt.HashPassword` with work factor 12
- [X] Implement `VerifyPassword` using `BCrypt.Verify` for password validation
- [X] Register `IPasswordHashingService` in DI container as Singleton
- [ ] Create unit tests verifying hash uniqueness and password verification
- [ ] Document security rationale for BCrypt and work factor 12 in SECURITY.md
- [X] Verify hash format starts with `$2a$12$` confirming correct work factor
