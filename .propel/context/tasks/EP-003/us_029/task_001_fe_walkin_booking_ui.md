# Task - task_001_fe_walkin_booking_ui

## Requirement Reference

- User Story: US_029 - Staff Walk-in Booking
- Story Location: .propel/context/tasks/EP-003/us_029/us_029.md
- Acceptance Criteria:
  - AC-1: Given I am a Staff user on the Walk-in Booking page, When I search for an existing patient, Then the system returns matching records by name, email, or phone within 300ms.
  - AC-2: Given the walk-in patient is not registered, When I click "Create New Patient", Then a minimal registration form appears (name, DOB, phone) and the account is created with status "active" — patient completes full registration later.
  - AC-3: Given I have identified the patient, When I select a provider and available time slot and enter the visit reason, Then the appointment is created with a "Walk-in" flag and an optional confirmation is sent if contact info is available.
- Edge Case:
  - What happens when trying to create a patient with an existing email? System displays match and allows Staff to select the existing record instead.
  - How does the system handle walk-in booking for a patient-only-restricted feature? Walk-in booking is exclusively Staff-accessible; patient role users cannot access this page.

## Design References (Frontend Tasks Only)

| Reference Type         | Value                                                                                        |
| ---------------------- | -------------------------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                                          |
| **Figma URL**          | N/A                                                                                          |
| **Wireframe Status**   | AVAILABLE                                                                                    |
| **Wireframe Type**     | HTML                                                                                         |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-018-walkin-booking.html                       |
| **Screen Spec**        | .propel/context/docs/figma_spec.md#SCR-018                                                   |
| **UXR Requirements**   | UXR-004 (Inline search), UXR-201 (WCAG 2.2 AA), UXR-601 (Inline validation)                  |
| **Design Tokens**      | .propel/context/docs/designsystem.md#typography, .propel/context/docs/designsystem.md#colors |

> **Wireframe Status Legend:**
>
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

| Layer        | Technology    | Version |
| ------------ | ------------- | ------- |
| Frontend     | React         | 18.x    |
| Frontend     | TypeScript    | 5.x     |
| Frontend     | Redux Toolkit | 2.x     |
| Frontend     | Tailwind CSS  | Latest  |
| Frontend     | React Router  | 6.x     |
| Library      | Axios         | Latest  |
| Backend      | N/A           | N/A     |
| Database     | N/A           | N/A     |
| AI/ML        | N/A           | N/A     |
| Vector Store | N/A           | N/A     |
| AI Gateway   | N/A           | N/A     |
| Mobile       | N/A           | N/A     |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)

| Reference Type           | Value |
| ------------------------ | ----- |
| **AI Impact**            | No    |
| **AIR Requirements**     | N/A   |
| **AI Pattern**           | N/A   |
| **Prompt Template Path** | N/A   |
| **Guardrails Config**    | N/A   |
| **Model Provider**       | N/A   |

> **AI Impact Legend:**
>
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)

| Reference Type       | Value |
| -------------------- | ----- |
| **Mobile Impact**    | No    |
| **Platform Target**  | N/A   |
| **Min OS Version**   | N/A   |
| **Mobile Framework** | N/A   |

> **Mobile Impact Legend:**
>
> - **Yes**: Task involves mobile app development (native or cross-platform)
> - **No**: Task is web, backend, or infrastructure only
>
> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement the Staff Walk-in Booking UI (SCR-018) allowing Staff users to search for existing patients, optionally create new patient accounts with minimal information, and book walk-in appointments. The interface includes real-time patient search with 300ms response time, inline validation, conditional patient creation flow, provider/slot selection, and visit reason entry. This page is exclusively accessible to Staff role users and implements RBAC enforcement at the routing level.

## Dependent Tasks

- None (this is the first task for US_029)

## Impacted Components

- **NEW**: `src/frontend/src/pages/staff/WalkinBooking.tsx` - Main walk-in booking page component
- **NEW**: `src/frontend/src/features/staff/components/PatientSearchInput.tsx` - Real-time patient search component with 300ms debounce
- **NEW**: `src/frontend/src/features/staff/components/CreatePatientModal.tsx` - Minimal patient creation form (name, DOB, phone)
- **NEW**: `src/frontend/src/features/staff/components/WalkinBookingForm.tsx` - Provider selection, slot selection, visit reason form
- **MODIFY**: `src/frontend/src/App.tsx` - Add Staff-only route for /staff/walkin-booking
- **MODIFY**: `src/frontend/src/api/client.ts` - Add API methods for patient search, patient creation, and walk-in booking

## Implementation Plan

1. **Set Up Page Structure and Routing**
   - Create WalkinBooking.tsx page component in `src/pages/staff/`
   - Add Staff-only route `/staff/walkin-booking` in App.tsx with role guard
   - Implement page layout matching wireframe SCR-018

2. **Implement Real-Time Patient Search Component**
   - Create PatientSearchInput.tsx with controlled input and 300ms debounce
   - Integrate API call to `/api/staff/patients/search?query={term}` endpoint
   - Display search results in dropdown with patient name, DOB, email, phone
   - Handle empty results state with "Create New Patient" button
   - Implement keyboard navigation (arrow keys, Enter to select)

3. **Implement Patient Creation Modal**
   - Create CreatePatientModal.tsx with minimal fields: name (required), DOB (required), phone (required), email (optional)
   - Implement inline validation (name length, DOB age validation, phone format)
   - Handle duplicate email edge case - display matching patient and offer selection
   - POST to `/api/staff/patients` with status "active"
   - On success, auto-populate search result and close modal

4. **Implement Walk-in Booking Form**
   - Create WalkinBookingForm.tsx with provider dropdown, available slots calendar view, and visit reason textarea
   - Fetch providers from `/api/providers` and display with specialty
   - Fetch available slots from `/api/providers/{id}/slots?date={today}` on provider selection
   - Display slots in time-grid format (e.g., 8:00 AM, 9:00 AM blocks)
   - Add visit reason textarea with character counter (max 500 chars)
   - Implement form submission to `/api/appointments/walkin` with walk-in flag

5. **Implement State Management and Error Handling**
   - Use local component state for search, selected patient, and booking form
   - Display loading spinner during API calls
   - Show error toast notifications for API failures
   - Display success toast on successful booking with appointment details
   - Clear form after successful booking

6. **Implement WCAG 2.2 AA Accessibility**
   - Add ARIA labels for all form inputs and buttons
   - Ensure keyboard navigation for all interactive elements
   - Implement focus management (trap focus in modal, return focus on close)
   - Use semantic HTML (form, button, input elements)
   - Ensure color contrast ratio meets WCAG AA standards (4.5:1 for text)

7. **Apply Design Tokens and Responsive Styling**
   - Reference design tokens from designsystem.md for colors, spacing, typography
   - Implement responsive layout with breakpoints: 375px (mobile), 768px (tablet), 1440px (desktop)
   - Match wireframe layout, spacing, and component positioning
   - Implement Tailwind CSS utility classes for consistent styling

## Current Project State

```
src/frontend/src/
├── api/
│   └── client.ts                              # API client (existing)
├── App.tsx                                    # Main app with routing (existing)
├── features/
│   ├── auth/
│   │   └── components/
│   │       └── LoginForm.tsx                 # Authentication (existing)
│   └── appointments/
│       └── components/                       # Appointment components (existing)
├── pages/
│   ├── AppointmentBooking.tsx                # Patient booking (existing)
│   ├── MyAppointments.tsx                    # Patient appointments (existing)
│   └── ProviderBrowser.tsx                   # Provider browsing (existing)
└── components/
    └── common/                               # Shared components (existing)
```

## Expected Changes

| Action | File Path                                                         | Description                                                                                       |
| ------ | ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| CREATE | src/frontend/src/pages/staff/WalkinBooking.tsx                    | Main walk-in booking page with layout, patient search integration, and booking form orchestration |
| CREATE | src/frontend/src/features/staff/components/PatientSearchInput.tsx | Real-time patient search input with 300ms debounce, dropdown results, keyboard navigation         |
| CREATE | src/frontend/src/features/staff/components/CreatePatientModal.tsx | Modal with minimal patient creation form (name, DOB, phone, optional email)                       |
| CREATE | src/frontend/src/features/staff/components/WalkinBookingForm.tsx  | Provider selection dropdown, available slots calendar, visit reason textarea                      |
| MODIFY | src/frontend/src/App.tsx                                          | Add Staff-only route `/staff/walkin-booking` with ProtectedRoute wrapper checking for Staff role  |
| MODIFY | src/frontend/src/api/client.ts                                    | Add API methods: searchPatients(query), createPatient(data), createWalkinAppointment(data)        |

## External References

- React Debounce Hook: https://usehooks-ts.com/react-hook/use-debounce
- React Hook Form Documentation: https://react-hook-form.com/get-started
- Tailwind CSS Forms Plugin: https://github.com/tailwindlabs/tailwindcss-forms
- WCAG 2.2 AA Guidelines: https://www.w3.org/WAI/WCAG22/quickref/?currentsidebar=%23col_customize&levels=aa
- React ARIA - Focus Management: https://react-spectrum.adobe.com/react-aria/FocusScope.html

## Build Commands

- `npm run dev` - Start development server on http://localhost:5173
- `npm run build` - Build for production
- `npm run test` - Run Vitest unit tests
- `npm run lint` - Run ESLint
- `npm run format` - Run Prettier

## Implementation Validation Strategy

- [ ] Unit tests pass for PatientSearchInput, CreatePatientModal, WalkinBookingForm components
- [ ] Integration tests pass for patient search API call with 300ms debounce
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] API calls correctly invoke backend endpoints with proper authentication headers
- [ ] RBAC enforcement verified - non-Staff users cannot access /staff/walkin-booking route
- [ ] Accessibility audit passes with axe DevTools (zero critical violations)
- [ ] Keyboard navigation works for all interactive elements
- [ ] Form validation displays inline errors for required fields
- [ ] Error handling displays appropriate messages for API failures
- [ ] Success toast displays appointment details after successful booking

## Implementation Checklist

- [ ] Create WalkinBooking.tsx page component with staff layout structure
- [ ] Add Staff-only route in App.tsx with role-based access guard
- [ ] Implement PatientSearchInput.tsx with 300ms debounce and dropdown results
- [ ] Add keyboard navigation (arrow keys, Enter) for search results dropdown
- [ ] Create CreatePatientModal.tsx with minimal form fields (name, DOB, phone)
- [ ] Implement duplicate email detection and patient selection flow
- [ ] Add inline validation for patient creation form (name, DOB, phone format)
- [ ] Implement WalkinBookingForm.tsx with provider dropdown and slots calendar
- [ ] Fetch and display available same-day slots on provider selection
- [ ] Add visit reason textarea with character counter (max 500)
- [ ] Implement form submission to `/api/appointments/walkin` endpoint
- [ ] Add loading states for API calls (spinner overlay)
- [ ] Implement error toast notifications with actionable messages
- [ ] Add success toast with appointment details on booking completion
- [ ] Clear form state after successful booking
- [ ] Apply design tokens from designsystem.md (colors, spacing, typography)
- [ ] Implement responsive layout matching wireframe at 375px, 768px, 1440px breakpoints
- [ ] Add ARIA labels for all form inputs, buttons, and interactive elements
- [ ] Ensure color contrast meets WCAG 2.2 AA standards (4.5:1 for text)
- [ ] Test keyboard-only navigation through entire booking flow
- [ ] Implement focus trap in CreatePatientModal and restore focus on close
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
