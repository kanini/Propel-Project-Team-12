# Task - task_004_fe_admin_system_settings

## Requirement Reference

- User Story: us_037
- Story Location: .propel/context/tasks/EP-005/us_037/us_037.md
- Acceptance Criteria:
  - AC-4: Admin changes reminder timing in system settings; future appointments use new intervals while already-scheduled reminders remain unchanged
- Edge Cases:
  - None directly applicable to this task (UI configuration only; backend handles scheduling semantics)

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-026-system-settings.html |
| **Screen Spec** | figma_spec.md#SCR-026 |
| **UXR Requirements** | UXR-201, UXR-301, UXR-601 |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing |

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
| Frontend | React + TypeScript + Redux Toolkit + Tailwind CSS | React 18.x, TypeScript 5.x, Redux Toolkit 2.x |

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

## Mobile References (Mobile Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview

Build the Admin System Settings page (SCR-026) that allows administrators to configure reminder intervals and notification channel toggles. This replaces the current "Coming soon" placeholder at `/admin/settings` with a functional form. The page fetches current settings from `GET /api/admin/settings`, displays editable fields for reminder intervals (hours before appointment), SMS enable/disable toggle, and Email enable/disable toggle. On save, it sends `PUT /api/admin/settings` with the updated values. The page implements all 4 required states: Default, Loading, Error, and Validation per SCR-026 specification. A Redux store slice (`settingsSlice`) manages async API calls and form state.

## Dependent Tasks

- EP-005/us_037/task_003_be_reminder_scheduling_delivery — Provides `GET/PUT /api/admin/settings` API endpoints

## Impacted Components

- **NEW** `src/frontend/src/features/admin/pages/SystemSettingsPage.tsx` — Admin settings page component with reminder configuration form
- **NEW** `src/frontend/src/store/settingsSlice.ts` — Redux slice with async thunks for settings CRUD
- **NEW** `src/frontend/src/types/settings.ts` — TypeScript interfaces for settings API
- **MODIFY** `src/frontend/src/App.tsx` — Replace placeholder with SystemSettingsPage import and route

## Implementation Plan

1. **Define TypeScript types**:
   ```typescript
   // src/frontend/src/types/settings.ts
   export interface SystemSetting {
       key: string;
       value: string;
       description?: string;
   }

   export interface ReminderSettings {
       intervals: number[];      // parsed from "Reminder.Intervals" JSON string
       smsEnabled: boolean;      // parsed from "Reminder.SmsEnabled"
       emailEnabled: boolean;    // parsed from "Reminder.EmailEnabled"
   }
   ```

2. **Create settingsSlice**:
   - State: `{ settings: SystemSetting[], loading: boolean, error: string | null, saving: boolean }`
   - `fetchSettings` async thunk: `GET /api/admin/settings` with JWT Authorization header
   - `updateSettings` async thunk: `PUT /api/admin/settings` with body `{ settings: SystemSettingDto[] }`
   - Reducers: standard fulfilled/pending/rejected handlers
   - Selector: `selectReminderSettings` — parses SystemSetting[] into typed `ReminderSettings` object
   - Use `createAsyncThunk` with `rejectWithValue` for error handling

3. **Create SystemSettingsPage component**:
   - Wrapped in `MainLayout` and guarded by `ProtectedRoute allowedRoles={["Admin"]}` (already done at route level)
   - On mount: dispatch `fetchSettings()`
   - **Default state**: Display form with:
     - "Reminder Intervals" section: list of interval inputs (hours), add/remove buttons
     - SMS channel toggle (Switch/Checkbox)
     - Email channel toggle (Switch/Checkbox)
     - Save button
   - **Loading state**: Skeleton or spinner overlay while fetching
   - **Error state**: Error banner with retry button when API call fails
   - **Validation state**: Inline validation errors (intervals must be positive integers, at least one channel enabled)
   - Form validation before save:
     - Each interval > 0 and is a whole number
     - At least one reminder interval configured
     - At least one channel (SMS or Email) enabled
   - On save: dispatch `updateSettings()`, show success toast/alert on completion
   - Tailwind CSS styling matching existing admin pages

4. **Reminder intervals editor**:
   - Display each interval as an input with "hours before" label
   - "Add interval" button appends a new empty input
   - Remove button (X icon) on each interval (minimum 1 required)
   - Sort intervals descending on save (48, 24, 2)
   - Serialize as JSON array string for the `Reminder.Intervals` setting value

5. **Integrate into App.tsx routes**:
   Replace the current placeholder:
   ```tsx
   // Current (remove):
   <div className="text-center">
       <h1 className="text-2xl font-bold mb-2">Settings</h1>
       <p className="text-neutral-500">Coming soon</p>
   </div>

   // New:
   <SystemSettingsPage />
   ```

6. **Wire up Redux store**:
   - Import and add `settingsReducer` to root store configuration (if combineReducers pattern is used)
   - Or add to existing store setup

7. **Accessibility requirements (UXR-201, UXR-301, UXR-601)**:
   - All form fields have associated labels (UXR-201: form accessibility)
   - Toggle switches have aria-checked and aria-label attributes (UXR-301: interactive element accessibility)
   - Validation errors linked to fields via aria-describedby (UXR-601: error message accessibility)
   - Focus management: auto-focus first field on load, move focus to error on validation failure

## Current Project State

```
src/frontend/src/
├── App.tsx                              # EXISTS — /admin/settings route with "Coming soon" placeholder
├── features/
│   └── admin/
│       └── pages/
│           └── UserManagementPage.tsx   # EXISTS — pattern reference for admin pages
├── store/
│   ├── index.ts                         # EXISTS — Redux store configuration
│   └── usersSlice.ts                    # EXISTS — pattern reference for async thunks
├── types/
│   └── index.ts                         # EXISTS
└── components/
    └── ...
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/features/admin/pages/SystemSettingsPage.tsx | Admin settings page with reminder interval configuration form |
| CREATE | src/frontend/src/store/settingsSlice.ts | Redux slice with fetchSettings/updateSettings async thunks |
| CREATE | src/frontend/src/types/settings.ts | TypeScript interfaces for SystemSetting and ReminderSettings |
| MODIFY | src/frontend/src/App.tsx | Replace "Coming soon" placeholder with SystemSettingsPage component |

## External References

- Redux Toolkit createAsyncThunk: https://redux-toolkit.js.org/api/createAsyncThunk
- React 18 Forms: https://react.dev/reference/react-dom/components/form
- Tailwind CSS Forms: https://tailwindcss.com/docs/plugins#forms
- WCAG 2.2 AA Form Accessibility: https://www.w3.org/WAI/tutorials/forms/

## Build Commands

```bash
cd src/frontend
npm run build
npm run lint
```

## Implementation Validation Strategy

- [x] Unit tests pass
- [x] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [x] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [x] SystemSettingsPage renders Default state with form fields
- [x] Loading state shows spinner/skeleton during API fetch
- [x] Error state shows error banner with retry on API failure
- [x] Validation state shows inline errors for invalid input
- [x] Settings save successfully and show confirmation feedback
- [x] Admin-only route protection verified (non-admin redirected)

## Implementation Checklist

- [x] Create TypeScript interfaces in `types/settings.ts` for SystemSetting and ReminderSettings
- [x] Create `settingsSlice.ts` with `fetchSettings` and `updateSettings` async thunks
- [x] Build `SystemSettingsPage` component with reminder intervals editor and channel toggles
- [x] Implement all 4 SCR-026 states: Default, Loading, Error, Validation
- [x] Add form validation (positive integers, at least one interval, at least one channel)
- [x] Replace "Coming soon" placeholder in `App.tsx` with `SystemSettingsPage`
- [x] Ensure accessibility: labels, aria attributes, focus management per UXR-201, UXR-301, UXR-601
- [x] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [x] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
