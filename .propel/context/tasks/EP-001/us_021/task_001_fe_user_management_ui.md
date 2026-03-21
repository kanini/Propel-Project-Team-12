# Task - task_001_fe_user_management_ui

## Requirement Reference
- User Story: us_021
- Story Location: .propel/context/tasks/EP-001/us_021/us_021.md
- Acceptance Criteria:
    - AC1: Click "Create User" -> Form displays for entering user name, email, role (Staff/Admin), system sends activation email upon save
    - AC2: Edit user account, modify details (name, role), save -> Changes persisted, audit log entry created, update confirmed
    - AC3: Click "Deactivate" and confirm -> Account status changes to inactive, all active sessions terminated, future login attempts blocked
    - AC4: User list page loads -> All Staff and Admin users displayed in table with name, email, role, status, last login columns with sorting and filtering
    - AC5: Admin attempts to deactivate their own account -> System prevents self-deactivation with error message
- Edge Cases:
    - Create user with email already in use -> System displays "Email already registered" error, prevents duplicate creation
    - Deactivate last Admin account -> System prevents deactivation if would leave zero active Admin accounts

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-021-user-management.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-021 |
| **UXR Requirements** | UXR-004, UXR-201, UXR-601, UXR-605 |
| **Design Tokens** | designsystem.md#table-components, designsystem.md#forms |

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
Build Admin user management interface (SCR-021, SCR-022) with user list table (sortable, filterable, searchable), create/edit user modal form, deactivate confirmation dialog, inline search (UXR-004), empty state illustration (UXR-605), validation preventing self-deactivation and last Admin deletion, and WCAG 2.2 Level AA compliance.

## Dependent Tasks
- task_001_fe_rbac_navigation (for Admin portal access)
- task_002_be_rbac_middleware (for [Authorize(Roles = "Admin")] enforcement)

## Impacted Components
- **NEW**: src/frontend/src/pages/AdminPortal/UserManagementPage.tsx
- **NEW**: src/frontend/src/components/users/UserListTable.tsx
- **NEW**: src/frontend/src/components/users/UserFormModal.tsx
- **NEW**: src/frontend/src/components/users/DeactivateUserDialog.tsx
- **NEW**: src/frontend/src/store/slices/userManagementSlice.ts
- **NEW**: src/frontend/src/types/user.types.ts

## Implementation Plan
1. Create user.types.ts with User, CreateUserRequest, UpdateUserRequest interfaces
2. Build userManagementSlice with async thunks: fetchUsers, createUser, updateUser, deactivateUser
3. Create UserListTable component with sortable columns, inline search filter (UXR-004), pagination
4. Build UserFormModal for create/edit with validation (name, email, role selection)
5. Implement DeactivateUserDialog with confirmation message and edge case checks
6. Add self-deactivation prevention logic (check if userId equals current user)
7. Add last Admin check logic (call API to verify active Admin count > 1)
8. Display empty state with "No users found" illustration and "Create User" CTA (UXR-605)
9. Apply Tailwind styling per wireframe with accessible table structure
10. Handle API errors: duplicate email, insufficient permissions, validation failures

## Current Project State
```
src/frontend/src/
├── pages/
│   ├── AdminDashboard/
│   │   └── AdminDashboardPage.tsx
│   ├── Login/
│   └── Registration/
├── components/
│   ├── layout/
│   │   ├── Sidebar.tsx
│   │   └── NavigationMenu.tsx
│   └── forms/
├── store/
│   └── slices/
│       └── authSlice.ts
├── types/
│   └── auth.types.ts
└── App.tsx
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/pages/AdminPortal/UserManagementPage.tsx | User management page with table and actions |
| CREATE | src/frontend/src/components/users/UserListTable.tsx | Sortable, filterable user table component |
| CREATE | src/frontend/src/components/users/UserFormModal.tsx | Create/edit user modal with form validation |
| CREATE | src/frontend/src/components/users/DeactivateUserDialog.tsx | Deactivate confirmation dialog |
| CREATE | src/frontend/src/components/common/EmptyState.tsx | Reusable empty state component with illustration |
| CREATE | src/frontend/src/store/slices/userManagementSlice.ts | Redux slice for user management state |
| CREATE | src/frontend/src/types/user.types.ts | User-related TypeScript interfaces |
| MODIFY | src/frontend/src/App.tsx | Add /admin/users route |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [React Table Sorting](https://tanstack.com/table/latest)
- [Headless UI Dialogs](https://headlessui.com/react/dialog)
- [Tailwind CSS Tables](https://tailwindcss.com/docs/table-layout)

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
- [ ] Create user.types.ts with User (userId, name, email, role, status, lastLogin), CreateUserRequest, UpdateUserRequest
- [ ] Create userManagementSlice with fetchUsers, createUser, updateUser, deactivateUser async thunks
- [ ] Build UserListTable with column headers: Name, Email, Role, Status, Last Login, Actions
- [ ] Implement table sorting (client-side) for all columns
- [ ] Add inline search input filtering users by name or email (debounced, UXR-004)
- [ ] Implement pagination (10 users per page)
- [ ] Create UserFormModal with fields: name (required), email (required, email format), role (dropdown: Staff/Admin)
- [ ] Add form validation: name 2-100 chars, valid email format, role selection required
- [ ] Implement createUser mode vs updateUser mode (same modal, different submit logic)
- [ ] Build DeactivateUserDialog with warning message and Confirm/Cancel buttons
- [ ] In DeactivateUserDialog: Check if userId === currentUserId, if true show error "Cannot deactivate your own account"
- [ ] Before deactivate: Call GET /api/users/admin-count, if count <= 1 show error "Cannot deactivate last Admin account"
- [ ] Create EmptyState component with illustration, heading, message, and CTA button
- [ ] Display EmptyState when users array is empty
- [ ] Handle API errors: 409 Conflict (duplicate email), 403 Forbidden (self-deactivate), 400 Bad Request (last Admin)
- [ ] Style UserListTable with Tailwind CSS per wireframe (zebra striping, hover states)
- [ ] Add ARIA labels to table (role="table", columnheader, row, cell)
- [ ] Ensure keyboard navigation for all interactive elements (UXR-205)
- [ ] Add loading skeleton while fetching users (UXR-502)
- [ ] Test create user flow: open modal, fill form, submit, verify user appears in table
- [ ] Test update user flow: click edit, modify fields, save, verify changes reflected
- [ ] Test deactivate flow: click deactivate, confirm, verify status changes to Inactive
- [ ] Test self-deactivate prevention: attempt to deactivate current user, verify error message
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
