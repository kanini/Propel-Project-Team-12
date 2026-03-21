# Task - task_001_fe_rbac_navigation

## Requirement Reference
- User Story: us_020
- Story Location: .propel/context/tasks/EP-001/us_020/us_020.md
- Acceptance Criteria:
    - AC1: Patient accesses Staff-only endpoint -> API returns 403 Forbidden, frontend does not render Staff navigation items
    - AC2: Staff user attempts Admin-only pages -> Frontend redirects to Staff dashboard, API rejects with 403
    - AC3: Patient requests another patient's data -> API returns 403, logs unauthorized access attempt
    - AC4: Each role logs in -> Sidebar/navigation only displays menu items available to that role
- Edge Cases:
    - User's role changed while active session -> Upon next API call, middleware re-validates role from token, rejects if role downgraded
    - Requests with tampered JWT role claims -> RS256 signature verification rejects with 401 Unauthorized

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-003-patient-dashboard.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-003 |
| **UXR Requirements** | UXR-003 |
| **Design Tokens** | designsystem.md#navigation |

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
Implement role-based navigation and route guards displaying role-specific menu items (Patient: appointments/intake/documents/dashboard, Staff: queue/walk-in/verification/patients, Admin: users/audit/settings), protecting routes with React Router guards, decoding JWT role claim for navigation filtering, and handling 403 errors with redirect to appropriate dashboard.

## Dependent Tasks
- task_001_fe_login_ui (for JWT token storage and role extraction)

## Impacted Components
- **NEW**: src/frontend/src/components/layout/Sidebar.tsx
- **NEW**: src/frontend/src/components/layout/NavigationMenu.tsx
- **NEW**: src/frontend/src/components/guards/RoleGuard.tsx
- **NEW**: src/frontend/src/utils/navigationConfig.ts
- **NEW**: src/frontend/src/pages/PatientDashboard/PatientDashboardPage.tsx
- **NEW**: src/frontend/src/pages/StaffDashboard/StaffDashboardPage.tsx
- **NEW**: src/frontend/src/pages/AdminDashboard/AdminDashboardPage.tsx
- **UPDATED**: src/frontend/src/App.tsx (add role-protected routes)

## Implementation Plan
1. Create navigationConfig.ts defining menu items per role with paths and icons
2. Build NavigationMenu component filtering items based on current user role
3. Implement RoleGuard component wrapping routes, checking JWT role, redirecting if unauthorized
4. Create skeleton dashboard pages for Patient, Staff, Admin
5. Build Sidebar component with NavigationMenu integration
6. Extract role from JWT token stored in sessionStorage
7. Add RoleGuard to all protected routes in App.tsx
8. Configure axios interceptor to handle 403 responses (redirect to role dashboard)
9. Apply Tailwind styling per wireframe with persistent navigation (UXR-003)
10. Ensure mobile-responsive navigation (sidebar -> bottom nav on mobile)

## Current Project State
```
src/frontend/src/
├── pages/
│   ├── Registration/
│   └── Login/
├── components/
│   └── forms/
├── store/
│   └── slices/
│       └── authSlice.ts
├── utils/
│   ├── tokenStorage.ts
│   └── roleBasedRedirect.ts
└── App.tsx
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/layout/Sidebar.tsx | Persistent sidebar with role-based navigation |
| CREATE | src/frontend/src/components/layout/NavigationMenu.tsx | Navigation menu component filtering by role |
| CREATE | src/frontend/src/components/guards/RoleGuard.tsx | Route guard checking JWT role |
| CREATE | src/frontend/src/utils/navigationConfig.ts | Navigation menu configuration per role |
| CREATE | src/frontend/src/pages/PatientDashboard/PatientDashboardPage.tsx | Patient dashboard placeholder |
| CREATE | src/frontend/src/pages/StaffDashboard/StaffDashboardPage.tsx | Staff dashboard placeholder |
| CREATE | src/frontend/src/pages/AdminDashboard/AdminDashboardPage.tsx | Admin dashboard placeholder |
| CREATE | src/frontend/src/utils/axiosInterceptor.ts | Axios 403 error interceptor |
| MODIFY | src/frontend/src/App.tsx | Add role-protected routes with RoleGuard |
| MODIFY | src/frontend/src/utils/tokenStorage.ts | Add getUserRole() function |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [React Router Protected Routes](https://reactrouter.com/en/main/start/overview#protecting-routes)
- [Axios Interceptors](https://axios-http.com/docs/interceptors)
- [JWT Decode](https://github.com/auth0/jwt-decode)

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
- [ ] Create navigationConfig.ts with three menu configs: patientMenuItems, staffMenuItems, adminMenuItems (each with label, path, icon)
- [ ] Add getUserRole() function to tokenStorage.ts decoding JWT to extract role claim
- [ ] Build NavigationMenu component accepting role prop, filtering navigationConfig by role
- [ ] Create RoleGuard component: check if user authenticated, check if user role matches allowed roles, redirect to appropriate dashboard if not
- [ ] Build Sidebar component with NavigationMenu, logo, user menu (Profile, Logout)
- [ ] Create PatientDashboardPage with placeholder content and navigation links
- [ ] Create StaffDashboardPage with placeholder content and navigation links
- [ ] Create AdminDashboardPage with placeholder content and navigation links
- [ ] Update App.tsx with protected routes: `/patient/*` (Patient role), `/staff/*` (Staff role), `/admin/*` (Admin role)
- [ ] Wrap protected routes with RoleGuard component
- [ ] Create axiosInterceptor.ts handling 403 responses: if 403, redirect to roleBasedRedirect(currentRole)
- [ ] Register axios interceptor in main.tsx or App.tsx
- [ ] Style Sidebar with Tailwind CSS per wireframe (collapsible on desktop, bottom nav on mobile)
- [ ] Ensure navigation items have proper ARIA labels and keyboard navigation
- [ ] Test Patient role: only sees Patient menu items, cannot access /staff or /admin routes
- [ ] Test Staff role: only sees Staff menu items, cannot access /admin routes
- [ ] Test Admin role: sees Admin menu items, can access all routes (optional superuser access)
- [ ] Test 403 error handling: make API call to forbidden endpoint, verify redirect
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
