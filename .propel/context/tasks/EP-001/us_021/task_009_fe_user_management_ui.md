# Task - TASK_009

## Requirement Reference
- User Story: US_021
- Story Location: .propel/context/tasks/EP-001/us_021/us_021.md
- Acceptance Criteria:
    - AC1: Form displays for creating users with name, email, role fields
    - AC4: User list displays with sorting and filtering
    - AC5: Self-deactivation is prevented
- Edge Case:
    - Last Admin account cannot be deactivated

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-021-user-management.html |
| **Screen Spec** | figma_spec.md#SCR-021 |
| **UXR Requirements** | UXR-004 (Inline search), UXR-201 (WCAG 2.2 AA), UXR-601 (Inline validation), UXR-605 (Empty states) |
| **Design Tokens** | designsystem.md#tables, designsystem.md#forms |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**
**IF Wireframe Status = AVAILABLE or EXTERNAL:**
- **MUST** implement user management table and forms matching wireframe
- **MUST** validate UI matches wireframe at all breakpoints

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
Implement Admin user management UI screens including user list table with search/filter/sort, create/edit user form modal, and deactivate confirmation dialog. Display users with name, email, role, status, last login columns and provide CRUD actions restricted to Admin role.

## Dependent Tasks
- TASK_010 (Backend User Management API)

## Impacted Components
- NEW: src/frontend/src/features/admin/pages/UserManagementPage.tsx
- NEW: src/frontend/src/features/admin/components/UserTable.tsx
- NEW: src/frontend/src/features/admin/components/UserFormModal.tsx
- NEW: src/frontend/src/features/admin/components/DeactivateConfirmDialog.tsx
- NEW: src/frontend/src/store/usersSlice.ts

## Implementation Plan
1. Create UserManagementPage with search bar, "Create User" button, and UserTable
2. Build UserTable displaying all Staff/Admin users with columns: name, email, role, status, last login, actions
3. Implement inline search filtering users by name or email
4. Create UserFormModal for create/edit with TextField components for name, email, role dropdown
5. Build DeactivateConfirmDialog warning about account deactivation
6. Create usersSlice with fetchUsers, createUser, updateUser, deactivateUser thunks
7. Implement empty state when no users exist
8. Add loading skeleton for table data fetching

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/features/admin/pages/UserManagementPage.tsx | Main user management page |
| CREATE | src/frontend/src/features/admin/components/UserTable.tsx | User list table with actions |
| CREATE | src/frontend/src/features/admin/components/UserFormModal.tsx | Create/Edit user modal form |
| CREATE | src/frontend/src/features/admin/components/DeactivateConfirmDialog.tsx | Deactivation confirmation dialog |
| CREATE | src/frontend/src/store/usersSlice.ts | Redux slice for user management state |

## External References
- **React Table Libraries**: https://tanstack.com/table/v8/docs/guide/introduction
- **Modal Patterns**: https://www.radix-ui.com/primitives/docs/components/dialog

## Implementation Checklist
- [x] Create UserManagementPage with SearchBar and UserTable components
- [x] Implement UserTable with columns: name, email, role, status, lastLogin, actions (edit/deactivate)
- [x] Add inline search filtering by name/email with debounce
- [x] Build UserFormModal: TextField for name, email; Select for role (Staff/Admin)
- [x] Create DeactivateConfirmDialog with warning message
- [x] Implement usersSlice: fetchUsers, createUser, updateUser, deactivateUser async thunks
- [x] Add empty state component: "No users found"
- [x] Implement loading skeleton for table rows during data fetch
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
