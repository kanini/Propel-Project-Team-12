# Task - TASK_004

## Requirement Reference
- User Story: US_019
- Story Location: .propel/context/tasks/EP-001/us_019/us_019.md
- Acceptance Criteria:
    - AC1: System redirects to role-appropriate dashboard after valid login
    - AC3: System displays generic "Invalid email or password" error for invalid credentials
- Edge Case:
    - Deactivated account displays "Account not active" message

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-002-login.html |
| **Screen Spec** | figma_spec.md#SCR-002 |
| **UXR Requirements** | UXR-201 (WCAG 2.2 AA), UXR-203 (Form labels), UXR-601 (Inline validation) |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**
**IF Wireframe Status = AVAILABLE or EXTERNAL:**
- **MUST** open and reference the wireframe file/URL during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, hover, focus, error, loading)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | TypeScript | 5.x |
| Library | Tailwind CSS | Latest |
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
Implement the login form UI component with email/password fields, remember me option, and links to registration and password recovery. Handle form submission, display generic error messages for failed authentication, and redirect authenticated users to role-appropriate dashboards (Patient/Staff/Admin).

## Dependent Tasks
- TASK_005 (Backend Login API must be implemented)

## Impacted Components
- NEW: src/frontend/src/features/auth/pages/LoginPage.tsx
- NEW: src/frontend/src/features/auth/components/LoginForm.tsx
- MODIFY: src/frontend/src/features/auth/authSlice.ts (add loginUser thunk)
- MODIFY: src/frontend/src/router.tsx (add role-based redirects)

## Implementation Plan
1. Create LoginPage.tsx as main login route component
2. Build LoginForm component with email and password TextField components
3. Add "Forgot Password" and "Register" links
4. Create loginUser async thunk in authSlice.ts
5. Implement error display for failed login (generic message)
6. Add role-based redirect logic after successful authentication
7. Style using Tailwind CSS per design tokens
8. Ensure WCAG 2.2 AA compliance with proper labels and focus states

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/features/auth/pages/LoginPage.tsx | Login page component |
| CREATE | src/frontend/src/features/auth/components/LoginForm.tsx | Login form with email/password fields |
| MODIFY | src/frontend/src/features/auth/authSlice.ts | Add loginUser async thunk |
| MODIFY | src/frontend/src/router.tsx | Add role-based redirect after login |

## External References
- **React Router v6**: https://reactrouter.com/en/main/start/tutorial
- **Redux Toolkit Auth Pattern**: https://redux-toolkit.js.org/tutorials/quick-start

## Implementation Checklist
- [x] Create LoginPage.tsx with route "/login"
- [x] Implement LoginForm with email and password TextFields
- [x] Add "Forgot Password" link (navigate to /forgot-password)
- [x] Add "Don't have an account? Register" link (navigate to /register)
- [x] Create loginUser async thunk calling POST /api/auth/login - *Already existed from Task 005*
- [x] Implement generic error message display ("Invalid email or password")
- [x] Add role-based redirect: Patient → /dashboard, Staff → /staff/dashboard, Admin → /admin/dashboard
- [x] Style form fields matching wireframe design
- [x] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
