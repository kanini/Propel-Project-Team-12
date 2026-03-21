# Task - task_001_fe_login_ui

## Requirement Reference
- User Story: us_019
- Story Location: .propel/context/tasks/EP-001/us_019/us_019.md
- Acceptance Criteria:
    - AC1: Enter valid credentials -> System authenticates, generates JWT session token, stores in Redis with 15-minute TTL, redirects to role-appropriate dashboard
    - AC2: Enter invalid credentials -> System displays generic "Invalid email or password" error without revealing which field is incorrect
    - AC3: Failed login 5 or more times -> Account temporarily locked, system displays lockout message with contact information
    - AC4: System validates session -> User role determined from JWT claims, appropriate dashboard displayed
- Edge Case:
    - Deactivated account attempts login -> System displays "Account not active. Please contact support." without specifying deactivation

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-002-login.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-002 |
| **UXR Requirements** | UXR-201, UXR-203, UXR-601 |
| **Design Tokens** | designsystem.md#form-components, designsystem.md#typography |

> **Wireframe Status Legend:**
> - **AVAILABLE**: Local file exists at specified path
> - **PENDING**: UI-impacting task awaiting wireframe (provide file or URL)
> - **EXTERNAL**: Wireframe provided via external URL
> - **N/A**: Task has no UI impact
>
> If UI Impact = No, all design references should be N/A

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**
**IF Wireframe Status = AVAILABLE or EXTERNAL:**
- **MUST** open and reference the wireframe file/URL during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, hover, focus, error, loading)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px
- Run `/analyze-ux` after implementation to verify pixel-perfect alignment

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Frontend | TypeScript | 5.x |
| Frontend | Redux Toolkit | 2.x |
| Frontend | Tailwind CSS | Latest |
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | React Router | v7 |
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
Build login page (SCR-002) with email/password form, JWT token storage in sessionStorage, role-based redirection (Patient -> /patient/dashboard, Staff -> /staff/dashboard, Admin -> /admin/dashboard), generic error handling, account lockout display, and WCAG 2.2 Level AA compliance.

## Dependent Tasks
- task_001_fe_registration_ui (for shared authSlice structure)

## Impacted Components
- **NEW**: src/frontend/src/pages/Login/LoginPage.tsx
- **NEW**: src/frontend/src/components/forms/LoginForm.tsx
- **UPDATED**: src/frontend/src/store/slices/authSlice.ts (add loginUser async thunk)
- **NEW**: src/frontend/src/utils/tokenStorage.ts
- **NEW**: src/frontend/src/utils/roleBasedRedirect.ts

## Implementation Plan
1. Create LoginPage and LoginForm components
2. Implement loginUser async thunk in authSlice calling POST /api/auth/login
3. Store JWT token in sessionStorage on successful login
4. Decode JWT to extract role claim (Patient/Staff/Admin)
5. Implement roleBasedRedirect utility redirecting based on role
6. Handle error responses: invalid credentials (401), account locked (403), account inactive (403)
7. Display generic error messages per AC2
8. Apply Tailwind styling per wireframe with ARIA labels
9. Add "Forgot Password?" and "Create Account" links
10. Ensure keyboard navigation and screen reader support

## Current Project State
```
src/frontend/src/
├── pages/
│   └── Registration/
│       └── RegistrationPage.tsx
├── components/
│   └── forms/
│       └── RegistrationForm.tsx
├── store/
│   └── slices/
│       └── authSlice.ts (with registerUser thunk)
├── utils/
│   └── validators/
└── types/
    └── auth.types.ts
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/pages/Login/LoginPage.tsx | Login page component |
| CREATE | src/frontend/src/components/forms/LoginForm.tsx | Login form with email/password inputs |
| MODIFY | src/frontend/src/store/slices/authSlice.ts | Add loginUser async thunk |
| CREATE | src/frontend/src/utils/tokenStorage.ts | JWT token storage utilities (get, set, remove) |
| CREATE | src/frontend/src/utils/roleBasedRedirect.ts | Role-based routing logic |
| MODIFY | src/frontend/src/types/auth.types.ts | Add LoginRequest, LoginResponse interfaces |
| MODIFY | src/frontend/src/App.tsx | Add /login route |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [JWT Decoding in JS](https://github.com/auth0/jwt-decode)
- [React Router v7 Navigation](https://reactrouter.com/en/main/hooks/use-navigate)
- [Session Storage Best Practices](https://developer.mozilla.org/en-US/docs/Web/API/Window/sessionStorage)

## Build Commands
```bash
cd src/frontend
npm install
npm run dev
npm test
npm run build
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [x] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [x] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] **[AI Tasks]** Prompt templates validated with test inputs
- [ ] **[AI Tasks]** Guardrails tested for input sanitization and output validation
- [ ] **[AI Tasks]** Fallback logic tested with low-confidence/error scenarios
- [ ] **[AI Tasks]** Token budget enforcement verified
- [ ] **[AI Tasks]** Audit logging verified (no PII in logs)
- [ ] **[Mobile Tasks]** Headless platform compilation succeeds
- [ ] **[Mobile Tasks]** Native dependency linking verified
- [ ] **[Mobile Tasks]** Permission manifests validated against task requirements

## Implementation Checklist
- [ ] Add LoginRequest (email, password) and LoginResponse (token, role) interfaces to auth.types.ts
- [ ] Create tokenStorage.ts with setToken, getToken, removeToken functions using sessionStorage
- [ ] Create roleBasedRedirect.ts with getRedirectPath(role: string) function
- [ ] Implement loginUser async thunk in authSlice calling POST /api/auth/login
- [ ] Handle async thunk states: pending (loading spinner), fulfilled (redirect), rejected (error message)
- [ ] Decode JWT token to extract role claim using jwt-decode library
- [ ] Build LoginForm component with email and password inputs
- [ ] Implement form validation (email format, password not empty)
- [ ] Display generic error message on 401 (AC2)
- [ ] Display account locked message on 403 with lockout reason
- [ ] Display account inactive message on 403 with inactive status
- [ ] Add "Forgot Password?" link (placeholder for future US)
- [ ] Add "Create Account" link navigating to /register
- [ ] Style with Tailwind CSS per wireframe
- [ ] Add ARIA labels and error announcements (UXR-203, UXR-207)
- [ ] Test keyboard navigation and focus indicators (UXR-202, UXR-205)
- [ ] Add LoginPage route to App.tsx
- [ ] Test role-based redirection for all three roles
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
