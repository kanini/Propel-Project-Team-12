# Task - task_001_be_rbac_authorization_middleware

## Requirement Reference
- User Story: US_057
- Story Location: .propel/context/tasks/EP-010/us_057/us_057.md
- Acceptance Criteria:
    - **AC1**: Given the RBAC requirement (FR-043), When a request arrives at a protected endpoint, Then the JWT middleware validates the token, extracts the role claim, and rejects requests with 403 Forbidden if the user's role lacks the required permission.

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

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | N/A | N/A |
| Backend | ASP.NET Core | 8.0 |
| Backend | C# | 12.0 |
| Backend | Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.6 |
| Backend | System.IdentityModel.Tokens.Jwt | 7.6.0 |
| Database | N/A | N/A |
| Caching | N/A | N/A |
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

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement role-based access control (RBAC) authorization middleware for ASP.NET Core Web API using JWT Bearer authentication with RS256 signing algorithm. This task ensures compliance with FR-043 (minimum necessary access), NFR-006 (RBAC for Patient, Staff, Admin roles), and NFR-014 (minimum access principle). The middleware validates JWT tokens, extracts role claims, and enforces authorization using [Authorize(Roles="...")] attributes on controller actions. Unauthorized requests return HTTP 403 Forbidden with actionable error messages. Supports JWT key rotation with 24-hour grace period (Edge Case) where both old and new keys are accepted during transition.

**Key Capabilities:**
- Configure JWT Bearer authentication middleware (AC1)
- RS256 asymmetric signing using RSA keys from security/rsa-keys/
- Extract role claim from JWT payload
- Authorize attribute for role-based endpoint protection
- 403 Forbidden response for insufficient permissions
- JWT key rotation support with grace period
- Public endpoint exemption (login, registration) with AllowAnonymous attribute
- Integration with audit logging for authorization failures

## Dependent Tasks
- US_021 (RBAC infrastructure from EP-001) - Role enum, User.Role property

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Web/Authorization/RoleRequirementHandler.cs` - Custom authorization handler
- **NEW**: `src/backend/PatientAccess.Web/Authorization/RoleRequirement.cs` - Authorization requirement
- **NEW**: `docs/JWT_KEY_ROTATION.md` - Key rotation procedure
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Configure JWT authentication
- **MODIFY**: `src/backend/PatientAccess.Web/Controllers/*.cs` - Add Authorize attributes
- **MODIFY**: `docs/AUTHENTICATION.md` - Document RBAC authorization

## Implementation Plan

1. **Configure JWT Bearer Authentication Middleware**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add JWT authentication before `builder.Build()`:
     ```csharp
     using Microsoft.AspNetCore.Authentication.JwtBearer;
     using Microsoft.IdentityModel.Tokens;
     using System.Security.Cryptography;
     
     // Configure JWT Bearer Authentication with RS256 (AC1 - US_057)
     var publicKeyXml = File.ReadAllText("security/rsa-keys/public-key.xml");
     var rsa = RSA.Create();
     rsa.FromXmlString(publicKeyXml);
     var rsaSecurityKey = new RsaSecurityKey(rsa);
     
     builder.Services.AddAuthentication(options =>
     {
         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
     })
     .AddJwtBearer(options =>
     {
         options.TokenValidationParameters = new TokenValidationParameters
         {
             // Validate signing key (RS256)
             ValidateIssuerSigningKey = true,
             IssuerSigningKey = rsaSecurityKey,
             
             // Validate issuer and audience
             ValidateIssuer = true,
             ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PatientAccessAPI",
             
             ValidateAudience = true,
             ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PatientAccessClient",
             
             // Validate token lifetime
             ValidateLifetime = true,
             ClockSkew = TimeSpan.FromMinutes(5), // Allow 5-minute clock skew
             
             // Role claim type
             RoleClaimType = "role"
         };
         
         // Custom events for logging
         options.Events = new JwtBearerEvents
         {
             OnAuthenticationFailed = context =>
             {
                 // Log authentication failure
                 var logger = context.HttpContext.RequestServices
                     .GetRequiredService<ILogger<Program>>();
                 
                 logger.LogWarning(
                     "JWT authentication failed: {Error}. Token: {Token}",
                     context.Exception.Message,
                     context.Request.Headers["Authorization"].ToString().Substring(0, Math.Min(50, context.Request.Headers["Authorization"].ToString().Length))
                 );
                 
                 return Task.CompletedTask;
             },
             
             OnChallenge = context =>
             {
                 // Custom 401 response
                 context.HandleResponse();
                 context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                 context.Response.ContentType = "application/json";
                 
                 var response = new
                 {
                     error = "Unauthorized",
                     message = "Valid JWT token required. Please login and provide a Bearer token.",
                     timestamp = DateTime.UtcNow
                 };
                 
                 return context.Response.WriteAsJsonAsync(response);
             },
             
             OnForbidden = context =>
             {
                 // Custom 403 response (AC1)
                 context.Response.StatusCode = StatusCodes.Status403Forbidden;
                 context.Response.ContentType = "application/json";
                 
                 var response = new
                 {
                     error = "Forbidden",
                     message = "Insufficient permissions. Your role does not have access to this resource.",
                     timestamp = DateTime.UtcNow
                 };
                 
                 return context.Response.WriteAsJsonAsync(response);
             }
         };
     });
     ```

2. **Add Authorization Middleware**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add after authentication middleware:
     ```csharp
     // Add authentication and authorization middleware (AC1 - US_057)
     app.UseAuthentication();
     app.UseAuthorization();
     ```

3. **Configure Authorization Policies**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add authorization policies before `builder.Build()`:
     ```csharp
     // Configure authorization policies (AC1 - US_057, NFR-006)
     builder.Services.AddAuthorization(options =>
     {
         // Patient-only policy
         options.AddPolicy("PatientOnly", policy =>
             policy.RequireRole("Patient"));
         
         // Staff-only policy (Staff or Admin)
         options.AddPolicy("StaffOnly", policy =>
             policy.RequireRole("Staff", "Admin"));
         
         // Admin-only policy
         options.AddPolicy("AdminOnly", policy =>
             policy.RequireRole("Admin"));
         
         // Authenticated user policy (any role)
         options.AddPolicy("Authenticated", policy =>
             policy.RequireAuthenticatedUser());
     });
     ```

4. **Apply Authorize Attributes to Controllers**
   - File: `src/backend/PatientAccess.Web/Controllers/PatientsController.cs`
   - Example authorization:
     ```csharp
     using Microsoft.AspNetCore.Authorization;
     
     namespace PatientAccess.Web.Controllers
     {
         [ApiController]
         [Route("api/[controller]")]
         [Authorize] // Require authentication for all endpoints
         public class PatientsController : ControllerBase
         {
             // GET /api/patients/{id} - Patient can access own record, Staff/Admin can access any
             [HttpGet("{id}")]
             public async Task<ActionResult<PatientDto>> GetPatient(int id)
             {
                 var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                 var currentUserRole = User.FindFirst("role")?.Value;
                 
                 // Patient can only access their own record (NFR-014 minimum access)
                 if (currentUserRole == "Patient" && currentUserId != id)
                 {
                     return Forbid(); // Returns 403 Forbidden
                 }
                 
                 // Staff/Admin can access any patient record
                 // ... implementation
             }
             
             // POST /api/patients - Staff/Admin only
             [HttpPost]
             [Authorize(Roles = "Staff,Admin")]
             public async Task<ActionResult<PatientDto>> CreatePatient(CreatePatientDto dto)
             {
                 // Only Staff/Admin can create patient records
                 // ... implementation
             }
             
             // DELETE /api/patients/{id} - Admin only
             [HttpDelete("{id}")]
             [Authorize(Roles = "Admin")]
             public async Task<IActionResult> DeletePatient(int id)
             {
                 // Only Admin can delete patient records
                 // ... implementation
             }
         }
     }
     ```

5. **Apply Authorize Attributes to Other Controllers**
   - File: `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs`
   - Authorization examples:
     ```csharp
     [ApiController]
     [Route("api/[controller]")]
     [Authorize]
     public class AppointmentsController : ControllerBase
     {
         // GET /api/appointments - Patient sees own, Staff/Admin see all
         [HttpGet]
         public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
         {
             var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
             var currentUserRole = User.FindFirst("role")?.Value;
             
             if (currentUserRole == "Patient")
             {
                 // Filter to current user's appointments only
                 // ... implementation
             }
             
             // Staff/Admin see all appointments
             // ... implementation
         }
         
         // POST /api/appointments - Patient and Staff
         [HttpPost]
         [Authorize(Roles = "Patient,Staff,Admin")]
         public async Task<ActionResult<AppointmentDto>> CreateAppointment(CreateAppointmentDto dto)
         {
             // Patients can book for themselves, Staff can book for any patient
             // ... implementation
         }
     }
     ```
   
   - File: `src/backend/PatientAccess.Web/Controllers/AuditLogsController.cs`
   - Admin-only controller:
     ```csharp
     [ApiController]
     [Route("api/[controller]")]
     [Authorize(Roles = "Admin")] // Admin-only access (NFR-014)
     public class AuditLogsController : ControllerBase
     {
         // All endpoints require Admin role
         // ... implementation
     }
     ```

6. **Exempt Public Endpoints with AllowAnonymous**
   - File: `src/backend/PatientAccess.Web/Controllers/AuthController.cs`
   - Public authentication endpoints:
     ```csharp
     [ApiController]
     [Route("api/[controller]")]
     public class AuthController : ControllerBase
     {
         // POST /api/auth/login - Public endpoint (Edge Case)
         [HttpPost("login")]
         [AllowAnonymous]
         public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
         {
             // Public access for login
             // ... implementation
         }
         
         // POST /api/auth/register - Public endpoint (Edge Case)
         [HttpPost("register")]
         [AllowAnonymous]
         public async Task<ActionResult<UserDto>> Register(CreateUserRequestDto dto)
         {
             // Public access for registration
             // ... implementation
         }
         
         // POST /api/auth/refresh - Requires authentication
         [HttpPost("refresh")]
         [Authorize]
         public async Task<ActionResult<LoginResponseDto>> RefreshToken(RefreshTokenDto dto)
         {
             // Authenticated users only
             // ... implementation
         }
     }
     ```

7. **Implement JWT Key Rotation Support (Edge Case)**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Support multiple signing keys during rotation:
     ```csharp
     // JWT Key Rotation Support (Edge Case - US_057)
     var signingKeys = new List<SecurityKey>();
     
     // Primary key (current)
     var publicKeyXml = File.ReadAllText("security/rsa-keys/public-key.xml");
     var rsa = RSA.Create();
     rsa.FromXmlString(publicKeyXml);
     signingKeys.Add(new RsaSecurityKey(rsa));
     
     // Rotation key (if exists - 24-hour grace period)
     var rotationKeyPath = "security/rsa-keys/public-key-rotation.xml";
     if (File.Exists(rotationKeyPath))
     {
         var rotationKeyXml = File.ReadAllText(rotationKeyPath);
         var rotationRsa = RSA.Create();
         rotationRsa.FromXmlString(rotationKeyXml);
         signingKeys.Add(new RsaSecurityKey(rotationRsa));
         
         // Log key rotation active
         var logger = builder.Services.BuildServiceProvider()
             .GetRequiredService<ILogger<Program>>();
         logger.LogInformation(
             "JWT key rotation active: accepting both current and rotation keys for 24-hour grace period.");
     }
     
     // Update TokenValidationParameters to accept multiple keys
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuerSigningKey = true,
         IssuerSigningKeys = signingKeys, // Support multiple keys
         // ... other parameters
     };
     ```

8. **Document JWT Key Rotation Procedure**
   - File: `docs/JWT_KEY_ROTATION.md`
   - Key rotation guide:
     ```markdown
     # JWT Key Rotation Procedure (Edge Case - US_057)
     
     ## Overview
     JWT signing keys should be rotated every 12 months or immediately if compromise suspected. The system supports a 24-hour grace period where both old and new keys are accepted.
     
     ## Rotation Steps
     
     ### Step 1: Generate New RSA Key Pair
     ```powershell
     # Run GenerateRsaKeys.ps1 script with rotation flag
     .\scripts\GenerateRsaKeys.ps1 -OutputPath "security/rsa-keys" -Rotation
     
     # Generates:
     # - public-key-rotation.xml
     # - private-key-rotation.xml
     ```
     
     ### Step 2: Deploy Rotation Keys (Grace Period Starts)
     1. Copy rotation keys to production server
     2. Restart application (accepts both old and new keys)
     3. Wait 24 hours for all clients to refresh tokens
     
     ### Step 3: Update Token Signing (After 24 Hours)
     1. Update token generation service to use rotation private key
     2. Restart application
     3. All new tokens signed with rotation key
     
     ### Step 4: Promote Rotation Key to Primary
     ```powershell
     # Replace primary key with rotation key
     mv security/rsa-keys/public-key.xml security/rsa-keys/public-key-old.xml
     mv security/rsa-keys/public-key-rotation.xml security/rsa-keys/public-key.xml
     
     mv security/rsa-keys/private-key.xml security/rsa-keys/private-key-old.xml
     mv security/rsa-keys/private-key-rotation.xml security/rsa-keys/private-key.xml
     
     # Delete old keys
     rm security/rsa-keys/*-old.xml
     ```
     
     ### Step 5: Restart Application
     - Application now uses new primary key only
     - Grace period complete
     
     ## Verification
     ```bash
     # Test with old token (should fail after grace period)
     curl -H "Authorization: Bearer <old-token>" https://api.patient-access.com/api/patients/1
     
     # Test with new token (should succeed)
     curl -H "Authorization: Bearer <new-token>" https://api.patient-access.com/api/patients/1
     ```
     ```

9. **Update AUTHENTICATION.md Documentation**
   - File: `docs/AUTHENTICATION.md`
   - Add RBAC authorization section:
     ```markdown
     ## Role-Based Access Control (AC1 - US_057)
     
     ### Roles
     - **Patient**: Can access own records, book own appointments, upload own documents
     - **Staff**: Can access all patient records, book appointments for any patient, manage walk-ins
     - **Admin**: Full system access including user management, audit logs, configuration
     
     ### Authorization Policies
     - `PatientOnly`: Requires Patient role
     - `StaffOnly`: Requires Staff or Admin role
     - `AdminOnly`: Requires Admin role only
     - `Authenticated`: Requires any authenticated user (any role)
     
     ### Endpoint Protection
     All API endpoints require authentication by default via [Authorize] attribute. Public endpoints (login, registration) use [AllowAnonymous] to bypass authentication.
     
     **Example: Patient Endpoint Authorization**
     ```csharp
     // GET /api/patients/{id}
     // - Patient can access own record only
     // - Staff/Admin can access any record
     [HttpGet("{id}")]
     [Authorize]
     public async Task<ActionResult<PatientDto>> GetPatient(int id)
     {
         if (User.IsInRole("Patient") && currentUserId != id)
             return Forbid(); // 403 Forbidden
         
         // ... implementation
     }
     ```
     
     ### HTTP Status Codes
     - **401 Unauthorized**: Missing or invalid JWT token
     - **403 Forbidden**: Valid token but insufficient role permissions (AC1)
     
     ### Error Responses
     ```json
     // 401 Unauthorized
     {
       "error": "Unauthorized",
       "message": "Valid JWT token required. Please login and provide a Bearer token.",
       "timestamp": "2026-03-23T00:00:00Z"
     }
     
     // 403 Forbidden
     {
       "error": "Forbidden",
       "message": "Insufficient permissions. Your role does not have access to this resource.",
       "timestamp": "2026-03-23T00:00:00Z"
     }
     ```
     ```

## Current Project State

```
src/backend/
├── PatientAccess.Web/
│   ├── Program.cs (existing)
│   └── Controllers/
│       ├── AuthController.cs (existing)
│       ├── PatientsController.cs (existing)
│       └── AppointmentsController.cs (existing)
security/
└── rsa-keys/
    ├── public-key.xml (existing)
    └── private-key.xml (existing)
docs/
└── AUTHENTICATION.md (existing)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | docs/JWT_KEY_ROTATION.md | Key rotation procedure |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure JWT authentication + authorization |
| MODIFY | src/backend/PatientAccess.Web/Controllers/PatientsController.cs | Add Authorize attributes |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs | Add Authorize attributes |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AuditLogsController.cs | Add Authorize(Roles="Admin") |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AuthController.cs | Add AllowAnonymous to login/register |
| MODIFY | docs/AUTHENTICATION.md | Document RBAC authorization |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Authentication
- **JWT Bearer Authentication**: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/
- **JWT Validation**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims
- **Role-Based Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles

### JWT Specifications
- **JWT RFC 7519**: https://datatracker.ietf.org/doc/html/rfc7519
- **JWS (RS256)**: https://datatracker.ietf.org/doc/html/rfc7515

### Design Requirements
- **FR-043**: Minimum necessary access principle (spec.md)
- **NFR-006**: RBAC for Patient, Staff, Admin roles (design.md)
- **NFR-014**: Minimum access based on role and clinical need (design.md)
- **TR-012**: JWT Bearer with RS256 signing (design.md)

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build PatientAccess.sln

# Run backend
cd PatientAccess.Web
dotnet run

# Test authorization (requires valid JWT)
$token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
curl -H "Authorization: Bearer $token" https://localhost:5001/api/patients/1

# Test 401 Unauthorized (no token)
curl https://localhost:5001/api/patients/1
# Expected: 401 Unauthorized

# Test 403 Forbidden (Patient accessing another patient's record)
curl -H "Authorization: Bearer $patientToken" https://localhost:5001/api/patients/999
# Expected: 403 Forbidden
```

## Validation Strategy

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/AuthorizationTests.cs`
- Test cases:
  1. **Test_ProtectedEndpoint_Returns401WithoutToken**
     - Request: GET /api/patients/1 (no Authorization header)
     - Assert: StatusCode = 401, error message contains "Valid JWT token required"
  2. **Test_ProtectedEndpoint_Returns403WithInsufficientRole**
     - Setup: Generate JWT with role="Patient", userId=1
     - Request: GET /api/patients/999 (different patient ID)
     - Assert: StatusCode = 403, error message contains "Insufficient permissions"
  3. **Test_PublicEndpoint_AllowsAnonymousAccess**
     - Request: POST /api/auth/login (no Authorization header)
     - Assert: StatusCode = 200 or 400 (not 401 Unauthorized)
  4. **Test_AdminEndpoint_RejectsNonAdminRole**
     - Setup: Generate JWT with role="Staff"
     - Request: GET /api/audit-logs
     - Assert: StatusCode = 403 Forbidden
  5. **Test_AdminEndpoint_AllowsAdminRole**
     - Setup: Generate JWT with role="Admin"
     - Request: GET /api/audit-logs
     - Assert: StatusCode = 200
  6. **Test_JwtKeyRotation_AcceptsBothKeys**
     - Setup: Place rotation key in security/rsa-keys/public-key-rotation.xml
     - Request 1: GET /api/patients/1 with token signed by old key
     - Request 2: GET /api/patients/1 with token signed by rotation key
     - Assert: Both requests return 200 OK during grace period

### Acceptance Criteria Validation
- **AC1**: ✅ JWT middleware validates token, extracts role, rejects with 403 if insufficient permissions

## Success Criteria Checklist
- [MANDATORY] JWT Bearer authentication configured with RS256 signing
- [MANDATORY] RSA public key loaded from security/rsa-keys/public-key.xml
- [MANDATORY] TokenValidationParameters validates issuer, audience, lifetime, signing key
- [MANDATORY] Custom 401 Unauthorized response with actionable error message
- [MANDATORY] Custom 403 Forbidden response for insufficient permissions (AC1)
- [MANDATORY] Authorization policies: PatientOnly, StaffOnly, AdminOnly, Authenticated
- [MANDATORY] UseAuthentication() and UseAuthorization() middleware registered
- [MANDATORY] PatientsController has Authorize attributes with role enforcement
- [MANDATORY] AppointmentsController has role-based authorization
- [MANDATORY] AuditLogsController restricted to Admin role only
- [MANDATORY] AuthController login/register have AllowAnonymous attribute
- [MANDATORY] JWT key rotation support with 24-hour grace period (Edge Case)
- [MANDATORY] JWT_KEY_ROTATION.md documents key rotation procedure
- [MANDATORY] AUTHENTICATION.md updated with RBAC authorization section
- [MANDATORY] Integration test: 401 without token
- [MANDATORY] Integration test: 403 with insufficient role
- [MANDATORY] Integration test: Public endpoints allow anonymous
- [RECOMMENDED] Integration test: JWT key rotation accepts both keys

## Estimated Effort
**3 hours** (JWT authentication config + authorization policies + Authorize attributes on controllers + key rotation support + docs + tests)
