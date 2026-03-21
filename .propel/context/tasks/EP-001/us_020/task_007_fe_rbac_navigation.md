# Task - TASK_007

## Requirement Reference
- User Story: US_020
- Story Location: .propel/context/tasks/EP-001/us_020/us_020.md
- Acceptance Criteria:
    - AC1: Frontend does not render Staff navigation for Patient users
    - AC2: Frontend redirects non-Admin users attempting to access Admin pages
    - AC4: Role-based navigation displays menu items per role
- Edge Case:
    - None specific to frontend RBAC

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-003-patient-dashboard.html |
| **Screen Spec** | figma_spec.md#SCR-003 |
| **UXR Requirements** | UXR-003 (Persistent role-based navigation) |
| **Design Tokens** | designsystem.md#navigation |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**
**IF Wireframe Status = AVAILABLE or EXTERNAL:**
- **MUST** implement role-based navigation matching wireframe specifications
- **MUST** validate navigation visibility per role at all breakpoints

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | React Router | 6.x |
| Library | Redux Toolkit | 2.x |
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
Implement frontend role-based navigation and route guards that conditionally render menu items based on user role (Patient/Staff/Admin) and protect routes from unauthorized access by redirecting non-authorized users to appropriate dashboards.

## Dependent Tasks
- TASK_004 (Login must return role information)

## Impacted Components
- NEW: src/frontend/src/components/layout/Sidebar.tsx
- NEW: src/frontend/src/components/layout/BottomNav.tsx
- NEW: src/frontend/src/hooks/useAuth.ts
- NEW: src/frontend/src/components/ProtectedRoute.tsx
- MODIFY: src/frontend/src/router.tsx

## Implementation Plan
1. Create useAuth hook extracting role from Redux auth state
2. Build ProtectedRoute component with role-based access checks
3. Create Sidebar component with conditional navigation items per role
4. Create BottomNav component for mobile with role filtering
5. Implement route protection in router.tsx using ProtectedRoute
6. Define navigationConfig mapping routes to required roles
7. Redirect unauthorized access attempts to role-appropriate dashboard
8. Hide navigation items for inaccessible routes based on current user role

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/layout/Sidebar.tsx | Desktop sidebar with role-based menu |
| CREATE | src/frontend/src/components/layout/BottomNav.tsx | Mobile bottom navigation with role filtering |
| CREATE | src/frontend/src/hooks/useAuth.ts | Custom hook for auth state and role checking |
| CREATE | src/frontend/src/components/ProtectedRoute.tsx | Route wrapper with role authorization |
| MODIFY | src/frontend/src/router.tsx | Wrap protected routes with ProtectedRoute component |

## External References
- **React Router Protected Routes**: https://reactrouter.com/en/main/start/overview#protected-routes
- **Conditional Rendering React**: https://react.dev/learn/conditional-rendering

## Implementation Checklist
- [x] Create useAuth hook: returns { user, role, isAuthenticated }
- [x] Implement ProtectedRoute: check role, redirect if unauthorized
- [x] Create Sidebar with navigationConfig: Patient → [Dashboard, Appointments, Intake, Documents], Staff → [Dashboard, Queue, Walk-in, Verification], Admin → [Dashboard, Users, Audit, Settings]
- [x] Build BottomNav for mobile (displays at <768px breakpoint)
- [x] Wrap protected routes in router.tsx with ProtectedRoute and allowedRoles prop
- [x] Implement redirect logic: unauthorized → role-specific dashboard or /login
- [x] Test navigation visibility for each role
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
