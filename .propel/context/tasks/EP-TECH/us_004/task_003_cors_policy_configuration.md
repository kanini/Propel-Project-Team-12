# Task - task_003_cors_policy_configuration

## Requirement Reference
- User Story: us_004
- Story Location: .propel/context/tasks/EP-TECH/us_004/us_004.md
- Acceptance Criteria:
    - AC-3: CORS policy configured to reject requests from non-allowed origins with 403 status; requests from configured frontend domains are allowed
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
Configure Cross-Origin Resource Sharing (CORS) policy to restrict API access to approved frontend domains only, preventing unauthorized cross-origin requests from malicious websites. Implement strict CORS policy with allowed origins from configuration, permitted headers (Authorization, Content-Type), and allowed methods (GET, POST, PUT, DELETE). Ensure credentials (cookies, authorization headers) are enabled for authenticated requests.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project structure

## Impacted Components
- **MODIFY** src/backend/PatientAccess.Web/Program.cs - Add CORS policy configuration and middleware
- **MODIFY** src/backend/PatientAccess.Web/appsettings.json - Add AllowedOrigins configuration
- **MODIFY** src/backend/PatientAccess.Web/appsettings.Development.json - Add localhost origins for development

## Implementation Plan
1. **Add CORS Configuration Section**: Update `appsettings.json` with `CorsSettings` section containing `AllowedOrigins` array
2. **Configure Development Origins**: Add localhost URLs (http://localhost:5173 for Vite) to `appsettings.Development.json`
3. **Register CORS Policy**: Add CORS services to DI container with named policy "DefaultCorsPolicy"
4. **Define Allowed Origins**: Use `WithOrigins()` to restrict access to configured frontend domains
5. **Configure Allowed Headers**: Use `WithHeaders()` to permit Authorization and Content-Type headers
6. **Configure Allowed Methods**: Use `WithMethods()` to permit GET, POST, PUT, DELETE, OPTIONS
7. **Enable Credentials**: Use `AllowCredentials()` to support cookies and authorization headers
8. **Apply CORS Middleware**: Add `app.UseCors()` to middleware pipeline before authentication middleware

## Current Project State
```
Propel-Project-Team-12/
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   ├── PatientAccess.Business/
│   └── PatientAccess.Data/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add CorsSettings section with AllowedOrigins (production URLs) |
| MODIFY | src/backend/PatientAccess.Web/appsettings.Development.json | Add AllowedOrigins for localhost (http://localhost:5173) |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register CORS services and apply middleware with origin/header/method restrictions |
| CREATE | docs/CORS.md | CORS configuration documentation and troubleshooting guide |

## External References
- ASP.NET Core CORS Documentation: https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0
- CORS MDN Web Docs: https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS
- OWASP CORS Security Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Cross-Origin_Resource_Sharing_Cheat_Sheet.html
- Preflight Requests Explained: https://developer.mozilla.org/en-US/docs/Glossary/Preflight_request

## Build Commands
```bash
# No additional build commands required (uses built-in ASP.NET Core CORS middleware)

# Test CORS from command line (replace URLs accordingly)
curl -H "Origin: http://localhost:5173" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: Content-Type" \
     -X OPTIONS \
     --verbose \
     http://localhost:5000/api/test
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for middleware configuration)
- [ ] Integration tests pass (N/A for middleware configuration)
- [ ] Request from allowed origin (localhost:5173) succeeds with 200 status
- [ ] Request from disallowed origin returns CORS error (browser blocks response)
- [ ] Preflight OPTIONS request returns correct Access-Control-Allow-* headers
- [ ] Authorization header allowed in CORS policy (Access-Control-Allow-Headers includes Authorization)
- [ ] Browser DevTools shows no CORS errors for requests from allowed origins
- [ ] Production configuration restricts origins to actual frontend domain (Vercel URL)

## Implementation Checklist
- [X] Add `CorsSettings:AllowedOrigins` array to `appsettings.json` (production URL placeholder)
- [X] Add `CorsSettings:AllowedOrigins` to `appsettings.Development.json` with http://localhost:5173
- [X] Register CORS services in Program.cs with named policy "DefaultCorsPolicy"
- [X] Configure `WithOrigins()` reading from appsettings.CorsSettings.AllowedOrigins
- [X] Configure `WithHeaders("Authorization", "Content-Type")` for allowed headers
- [X] Configure `WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")` for allowed methods
- [X] Add `AllowCredentials()` to enable cookies and authorization headers
- [X] Add `app.UseCors("DefaultCorsPolicy")` before `app.UseAuthentication()` in middleware pipeline
