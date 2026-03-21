# Task - TASK_008

## Requirement Reference
- User Story: US_020
- Story Location: .propel/context/tasks/EP-001/us_020/us_020.md
- Acceptance Criteria:
    - AC1: API returns 403 for unauthorized role access
    - AC2: API rejects Admin-only requests from Staff with 403
    - AC3: API returns 403 for cross-patient data access attempts
- Edge Case:
    - Token with tampered role claims rejected with 401

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
| Library | Microsoft.AspNetCore.Authorization | 8.x |
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
Implement backend RBAC middleware that validates user roles from JWT claims and enforces minimum necessary access by rejecting unauthorized endpoint access with 403 Forbidden. Includes authorization attributes, policy-based authorization, and cross-patient data access prevention.

## Dependent Tasks
- TASK_006 (JWT service with role claims)

## Impacted Components
- NEW: src/backend/PatientAccess.Web/Middleware/RbacAuthorizationMiddleware.cs
- NEW: src/backend/PatientAccess.Web/Attributes/RequireRoleAttribute.cs
- MODIFY: src/backend/PatientAccess.Web/Program.cs
- MODIFY: src/backend/PatientAccess.Web/Controllers/* (add [RequireRole] attributes)

## Implementation Plan
1. Create RequireRoleAttribute with allowed roles parameter
2. Implement RbacAuthorizationMiddleware extracting role from JWT claims
3. Validate role against endpoint required roles
4. Return 403 Forbidden for unauthorized access
5. Implement patient data access validation: ensure patient can only access own data
6. Add authorization policies in Program.cs (Patient, Staff, Admin policies)
7. Apply [RequireRole] or [Authorize(Policy = "...")] to controller actions
8. Log unauthorized access attempts for audit

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Middleware/RbacAuthorizationMiddleware.cs | RBAC middleware validating roles |
| CREATE | src/backend/PatientAccess.Web/Attributes/RequireRoleAttribute.cs | Custom authorization attribute for roles |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register RBAC policies and middleware |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AdminController.cs | Add [RequireRole("Admin")] |
| MODIFY | src/backend/PatientAccess.Web/Controllers/StaffController.cs | Add [RequireRole("Staff", "Admin")] |

## External References
- **ASP.NET Core Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction
- **Policy-Based Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies
- **Claims-Based Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims

## Implementation Checklist
- [x] Create RequireRoleAttribute inheriting AuthorizeAttribute
- [x] Implement RbacAuthorizationMiddleware: extract role claim, compare with endpoint requirements
- [x] Configure authorization policies in Program.cs: AddPolicy("AdminOnly"), AddPolicy("StaffOnly"), AddPolicy("PatientOnly")
- [x] Apply [Authorize(Policy = "AdminOnly")] to admin endpoints
- [x] Apply [Authorize(Policy = "StaffOnly")] to staff endpoints  
- [x] Implement patient data filtering: ensure userId from token matches resource ownerId
- [x] Return 403 Forbidden with message "Insufficient permissions"
- [x] Log unauthorized access to audit log service
