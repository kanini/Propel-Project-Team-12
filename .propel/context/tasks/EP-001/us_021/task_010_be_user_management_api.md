# Task - TASK_010

## Requirement Reference
- User Story: US_021
- Story Location: .propel/context/tasks/EP-001/us_021/us_021.md
- Acceptance Criteria:
    - AC1: Create user with activation email
    - AC2: Update user details with audit logging
    - AC3: Deactivate account, terminate sessions, block future logins
    - AC5: Prevent self-deactivation
- Edge Case:
    - Prevent duplicate email on user creation
    - Prevent deactivation of last Admin account

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
Implement backend API endpoints for Admin user management including create, update, deactivate, and list users. Enforce business rules: prevent duplicate emails, prevent self-deactivation, prevent last Admin deactivation, create audit logs for all user changes, and send activation emails for new users.

## Dependent Tasks
- TASK_003 (User table must exist)
- TASK_012 (Audit logging service)

## Impacted Components
- NEW: src/backend/PatientAccess.Web/Controllers/AdminController.cs
- NEW: src/backend/PatientAccess.Business/Services/AdminService.cs
- NEW: src/backend/PatientAccess.Business/Interfaces/IAdminService.cs
- NEW: src/backend/PatientAccess.Business/DTOs/CreateUserRequestDto.cs
- NEW: src/backend/PatientAccess.Business/DTOs/UpdateUserRequestDto.cs
- NEW: src/backend/PatientAccess.Business/DTOs/UserDto.cs

## Implementation Plan
1. Create CreateUserRequestDto, UpdateUserRequestDto, UserDto
2. Create IAdminService interface with CreateUser, UpdateUser, DeactivateUser, GetAllUsers methods
3. Implement AdminService.CreateUser: validate email uniqueness, create user with Pending status, send activation email
4. Implement AdminService.UpdateUser: update name/role, create audit log entry
5. Implement AdminService.DeactivateUser: check not self, check not last Admin, set status Inactive, delete Redis sessions
6. Implement AdminService.GetAllUsers: return all Staff/Admin users with filtering
7. Create AdminController with [Authorize(Policy = "AdminOnly")] attribute
8. Add endpoints: POST /api/admin/users, PUT /api/admin/users/{id}, DELETE /api/admin/users/{id}, GET /api/admin/users

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Controllers/AdminController.cs | Admin user management endpoints |
| CREATE | src/backend/PatientAccess.Business/Services/AdminService.cs | User CRUD business logic |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAdminService.cs | Admin service interface |
| CREATE | src/backend/PatientAccess.Business/DTOs/CreateUserRequestDto.cs | Create user request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/UpdateUserRequestDto.cs | Update user request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/UserDto.cs | User response DTO |

## External References
- **RESTful API Design**: https://restfulapi.net/http-methods/
- **EF Core Concurrency**: https://learn.microsoft.com/en-us/ef/core/saving/concurrency

## Implementation Checklist
- [x] Create DTOs: CreateUserRequestDto, UpdateUserRequestDto, UserDto
- [x] Implement IAdminService with CreateUser, UpdateUser, DeactivateUser, GetAllUsers
- [x] CreateUser: check email uniqueness, hash password, create User entity (status Pending), send activation email
- [x] UpdateUser: update fields, create audit log, return updated user
- [x] DeactivateUser: validate userId != currentUserId, check not last Admin, set status Inactive, delete Redis sessions
- [x] GetAllUsers: query Staff/Admin users, filter by search term (name/email), sort by name
- [x] Create AdminController with [Authorize(Policy = "AdminOnly")]
- [x] Add POST /api/admin/users, PUT /api/admin/users/{id}, DELETE /api/admin/users/{id}, GET /api/admin/users endpoints
- [x] Return 400 Bad Request for duplicate email or self-deactivation attempts
