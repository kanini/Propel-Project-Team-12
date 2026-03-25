# Task - task_001_fe_appointment_selection_ui

## Requirement Reference

- User Story: us_037
- Story Location: .propel/context/tasks/EP-004/us_037/us_037.md
- Acceptance Criteria:
  - AC-1: When patient has multiple upcoming appointments requiring intake, display list with provider name, date, time, and intake status (pending/completed/in-progress)
  - AC-2: When patient selects a specific appointment, navigate to intake mode selection screen (AI or Manual) with appointment ID
  - AC-3: Completed appointments display green "Completed" badge and "Edit Intake" button; pending appointments show "Start Intake" button
  - AC-4: In-progress intake displays "In Progress" badge and "Continue Intake" button
  - AC-5: When patient has only one upcoming appointment, bypass appointment list and go directly to intake mode selection
  - AC-6: When patient has no upcoming appointments requiring intake, display empty state with "No upcoming appointments requiring intake" and "Book Appointment" link
- Edge Cases:
  - Past appointments with incomplete intake: Removed from intake-required list
  - Appointments that don't require intake: Not shown in selection list
  - Switching between appointments: Current intake session auto-saved; new session created for different appointment

## Design References (Frontend Tasks Only)

| Reference Type         | Value                                                                                                      |
| ---------------------- | ---------------------------------------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                                                        |
| **Figma URL**          | N/A                                                                                                        |
| **Wireframe Status**   | PENDING                                                                                                    |
| **Wireframe Type**     | N/A                                                                                                        |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-014-appointment-selection.html                              |
| **Screen Spec**        | figma_spec.md#SCR-014                                                                                      |
| **UXR Requirements**   | UXR-104, UXR-105, UXR-203, UXR-601                                                                         |
| **Design Tokens**      | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing, designsystem.md#status-badges |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**

**IF Wireframe Status = AVAILABLE:**

- **MUST** open and reference the wireframe file during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, loading, error, empty state)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px
- Run `/analyze-ux` after implementation to verify pixel-perfect alignment

**IF Wireframe Status = PENDING:**

- Follow design system tokens and UXR requirements for layout, spacing, typography
- Use existing component patterns from similar screens (e.g., appointment list patterns from booking flow)
- Ensure WCAG 2.1 AA compliance (color contrast, focus indicators, ARIA labels)

## Applicable Technology Stack

| Layer    | Technology                                        | Version                                       |
| -------- | ------------------------------------------------- | --------------------------------------------- |
| Frontend | React + TypeScript + Redux Toolkit + Tailwind CSS | React 18.x, TypeScript 5.x, Redux Toolkit 2.x |
| Backend  | .NET 8 ASP.NET Core Web API                       | .NET 8.0                                      |
| Library  | React Router                                      | v7                                            |
| AI/ML    | N/A                                               | N/A                                           |

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

## Mobile References (Mobile Tasks Only)

| Reference Type       | Value |
| -------------------- | ----- |
| **Mobile Impact**    | No    |
| **Platform Target**  | N/A   |
| **Min OS Version**   | N/A   |
| **Mobile Framework** | N/A   |

## Task Overview

Implement the Appointment Selection UI for SCR-014. This task builds the intake entry point where patients with multiple upcoming appointments can select which appointment they want to complete intake for. The component displays a list of appointment cards showing provider name, specialty, appointment date/time, and intake status badge (Pending/In Progress/Completed). Each card has a primary action button ("Start Intake" / "Continue Intake" / "Edit Intake") based on the intake status. When only one appointment exists, the user is automatically redirected to the intake mode selection. When no appointments exist, an empty state with a "Book Appointment" link is displayed. The UI consumes the backend API endpoint GET `/api/intake/appointments` to fetch the list of appointments requiring intake.

## Dependent Tasks

- None (this is the entry point for the intake flow; backend API can be mocked during development)

## Impacted Components

- **NEW** `src/frontend/src/features/intake/pages/AppointmentSelectionPage.tsx` — Main appointment selection page
- **NEW** `src/frontend/src/features/intake/components/AppointmentCard.tsx` — Reusable appointment card with status badge and action button
- **NEW** `src/frontend/src/features/intake/components/IntakeStatusBadge.tsx` — Status badge component (Pending/In Progress/Completed)
- **NEW** `src/frontend/src/features/intake/components/EmptyStateIntake.tsx` — Empty state component for no appointments
- **NEW** `src/frontend/src/store/slices/intakeAppointmentSlice.ts` — Redux slice for intake appointment list state
- **NEW** `src/frontend/src/api/intakeAppointmentApi.ts` — API client for appointment selection endpoints
- **NEW** `src/frontend/src/types/intakeAppointment.ts` — TypeScript types for appointment selection data
- **MODIFY** `src/frontend/src/App.tsx` — Update routes to add `/intake` for AppointmentSelectionPage and `/intake/:appointmentId` for IntakePage
- **MODIFY** `src/frontend/src/store/rootReducer.ts` — Register intakeAppointment slice
- **MODIFY** `src/frontend/src/features/appointments/pages/AppointmentListPage.tsx` — Add "Start Intake" button to upcoming appointment cards

## Implementation Plan

1. **Define TypeScript types** (`types/intakeAppointment.ts`): Create interfaces for `IntakeAppointment` (id, appointmentId, providerId, providerName, specialty, appointmentDate, appointmentTime, intakeStatus: 'pending' | 'inProgress' | 'completed', intakeSessionId?), `IntakeAppointmentListResponse`, and API response shapes matching the backend contract for GET `/api/intake/appointments`.

2. **Build API client** (`api/intakeAppointmentApi.ts`): Implement `fetchIntakeAppointments()` function using fetch with `VITE_API_BASE_URL`. Include JWT bearer token from auth state. Follow existing pattern in `appointmentApi.ts` and `providerApi.ts`.

3. **Create Redux slice** (`store/slices/intakeAppointmentSlice.ts`): Define state shape holding `appointments: IntakeAppointment[]`, `status: 'idle' | 'loading' | 'error'`, `error: string | null`. Add `fetchIntakeAppointments` async thunk. Follow existing pattern from `appointmentSlice.ts`.

4. **Build IntakeStatusBadge component**: Render status badges with appropriate colors — Pending: `bg-yellow-100 text-yellow-800`, In Progress: `bg-blue-100 text-blue-800`, Completed: `bg-green-100 text-green-800`. Use pill shape with icon. Include `aria-label` for accessibility (UXR-203).

5. **Build AppointmentCard component**: Render appointment card with provider avatar (or initials), provider name in bold, specialty in text-sm, date/time with calendar icon, IntakeStatusBadge, and action button. Button text changes based on status: "Start Intake" (pending), "Continue Intake" (inProgress), "Edit Intake" (completed). Use Tailwind card classes (`bg-white shadow-md rounded-lg p-4`). Include hover state (`hover:shadow-lg transition-shadow`). Add `onClick` handler that navigates to `/intake/{appointmentId}?mode={status}`. Ensure keyboard accessibility with `tabIndex={0}` and `onKeyDown` handler for Enter/Space keys (UXR-203).

6. **Build EmptyStateIntake component**: Display centered empty state with icon (calendar-x), heading "No upcoming appointments requiring intake", subtext "You don't have any appointments that need intake forms right now.", and a primary button "Book Appointment" that routes to `/book-appointment`. Match empty state pattern from existing components (UXR-105).

7. **Build AppointmentSelectionPage**: Fetch appointment list on mount using `useEffect` and `dispatch(fetchIntakeAppointments())`. Use loading skeleton while fetching. Implement routing logic — if `appointments.length === 0`, render EmptyStateIntake; if `appointments.length === 1`, use `useEffect` to automatically navigate to `/intake/${appointments[0].appointmentId}`; otherwise, render the appointment list with a header "Select an appointment for intake" and map over `appointments` to render AppointmentCard components. Include error state if fetch fails. Add `role="main"` and `aria-label="Select appointment for intake"` to page container (UXR-203).

8. **Update App.tsx routes**: Add route `<Route path="/intake" element={<AppointmentSelectionPage />} />` and update existing `/intake/:appointmentId` route to `<Route path="/intake/:appointmentId" element={<IntakePage />} />`. Wrap with `<ProtectedRoute />` to require authentication.

9. **Modify AppointmentListPage**: Add "Start Intake" button to upcoming appointment cards that routes to `/intake` (which will then show the appointment selection or go directly to intake if only one appointment). Only show button for appointments with `requiresIntake: true` flag and if appointment date is within the next 7 days (configurable intake window).

10. **Add loading skeletons**: Create skeleton loader for appointment cards matching the card dimensions. Display 3 skeleton cards during loading state.

11. **Write unit tests**: Test AppointmentSelectionPage with 0, 1, and multiple appointments. Test automatic navigation with 1 appointment. Test AppointmentCard button text based on status. Test EmptyStateIntake rendering and navigation. Use React Testing Library and Vitest. Follow existing patterns in `src/frontend/src/__tests__/`.

## Acceptance Verification

- [ ] Appointment list displays all upcoming appointments requiring intake with correct provider info, date/time, and status badge
- [ ] Clicking appointment card navigates to `/intake/{appointmentId}` with appropriate mode
- [ ] Single appointment automatically redirects to intake page without showing selection screen
- [ ] Empty state displays when no appointments require intake with working "Book Appointment" link
- [ ] Status badges display correct colors and text for Pending/In Progress/Completed states
- [ ] "Edit Intake" button appears for completed appointments and allows re-entry
- [ ] Loading state shows skeleton loaders, error state displays error message
- [ ] Component is keyboard accessible (Tab navigation, Enter/Space activation)
- [ ] Screen reader announces appointment details and status correctly
- [ ] Component is responsive at 375px, 768px, and 1440px breakpoints
- [ ] Unit tests pass with >80% coverage
- [ ] No linting errors or TypeScript errors

## Additional Notes

- **Performance**: Appointment list should render within 300ms on typical networks. Use React.memo for AppointmentCard to prevent unnecessary re-renders.
- **Accessibility**: Each appointment card must be keyboard navigable with clear focus indicators. Status badges should be announced by screen readers.
- **Edge Case Handling**: If an appointment's date has passed but intake is incomplete, the backend should exclude it from the response. Frontend should not filter dates client-side.
- **Auto-save**: When navigating away from an in-progress intake session, ensure data is auto-saved via the intake API before leaving the page (implement `beforeunload` handler).
