# Task - TASK_012

## Requirement Reference
- User Story: US_022
- Story Location: .propel/context/tasks/EP-001/us_022/us_022.md
- Acceptance Criteria:
    - AC1: Successful login creates audit log with userId, timestamp, action type, IP, user agent
    - AC2: Failed login logs hashed email, timestamp, IP, failure reason
    - AC3: Session timeout logs userId and last activity timestamp
- Edge Case:
    - Audit log service failure should not block auth events (non-blocking)

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
Implement audit logging service that captures all authentication events (login, logout, failed attempts, session timeouts) with immutable records including userId, timestamp, action type, IP address, user agent, and additional metadata. Service runs asynchronously to avoid blocking auth operations and logs failures to application error logs for retry.

## Dependent Tasks
- TASK_013 (Audit log database schema)

## Impacted Components
- NEW: src/backend/PatientAccess.Business/Services/AuditLogService.cs
- NEW: src/backend/PatientAccess.Business/Interfaces/IAuditLogService.cs
- MODIFY: src/backend/PatientAccess.Business/Services/AuthService.cs (integrate audit logging)
- MODIFY: src/backend/PatientAccess.Web/Middleware/AuditLoggingMiddleware.cs

## Implementation Plan
1. Create IAuditLogService interface with LogAuthEvent method
2. Implement AuditLogService with async logging to prevent blocking
3. Create AuditLogEntry entity with userId, timestamp, actionType, ipAddress, userAgent, metadata fields
4. Integrate audit logging in AuthService: log successful login, failed login, registration, logout
5. Create middleware to extract IP address and user agent from HttpContext
6. Implement session timeout logging triggered by session expiration event
7. Add error handling: log audit service failures to application logs, continue auth flow
8. Hash sensitive data (email) in failed login logs per GDPR/privacy requirements

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/AuditLogService.cs | Async audit logging service |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAuditLogService.cs | Audit service interface |
| MODIFY | src/backend/PatientAccess.Business/Services/AuthService.cs | Integrate LogAuthEvent calls |
| CREATE | src/backend/PatientAccess.Web/Middleware/AuditLoggingMiddleware.cs | Middleware for audit context |

## External References
- **ASP.NET Core Middleware**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/
- **OWASP Logging Cheat Sheet**: https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html
- **HIPAA Audit Requirements**: https://www.hhs.gov/hipaa/for-professionals/security/laws-regulations/index.html

## Implementation Checklist
- [x] Create IAuditLogService with LogAuthEvent (userId, actionType, ipAddress, userAgent, metadata)
- [x] Implement AuditLogService.LogAuthEvent: create AuditLog entity, save async (fire-and-forget)
- [x] Integrate in AuthService.Login: call LogAuthEvent on success ("Login") and failure ("FailedLogin")
- [x] Integrate in AuthService.Logout: call LogAuthEvent with action type "Logout"
- [x] Create AuditLoggingMiddleware: extract IP from HttpContext.Connection.RemoteIpAddress, UserAgent from headers
- [x] Hash email addresses in failed login logs using SHA256
- [x] Implement error handling: catch audit log exceptions, log to app logger, continue auth flow
- [ ] Log session timeout events triggered by Redis expiration callback (if supported) or periodic cleanup job
- [x] Ensure audit logs are immutable: no UPDATE/DELETE operations allowed on AuditLog table
