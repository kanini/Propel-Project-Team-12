# Task - task_001_fe_arrival_management_ui

## Requirement Reference

- User Story: US_031 - Patient Arrival Status Marking
- Story Location: .propel/context/tasks/EP-003/us_031/us_031.md
- Acceptance Criteria:
  - AC-1: Given I am on the Arrival Management page, When I search for a patient with a today's appointment, Then the patient's appointment details are displayed with a "Mark Arrived" button.
  - AC-2: Given I click "Mark Arrived", When the action is confirmed, Then the appointment status updates to "Arrived", the patient is added to the queue, and an audit log entry is created.
  - AC-3: Given a patient has no appointment today, When I search for them on the arrival page, Then the system indicates no appointment found and offers to create a walk-in booking.
  - AC-4: Given RBAC is enforced, When a Patient user attempts to self-check-in, Then the arrival marking functionality is not available — only Staff can mark arrivals.
- Edge Case:
  - What happens when Staff marks a patient arrived who was already marked? System displays "Patient already marked as arrived" and prevents duplicate status change.
  - How does the system handle marking arrival for a cancelled appointment? System prevents arrival marking for cancelled appointments with "Appointment was cancelled" message.

## Design References (Frontend Tasks Only)

| Reference Type         | Value                                                                                        |
| ---------------------- | -------------------------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                                          |
| **Figma URL**          | N/A                                                                                          |
| **Wireframe Status**   | AVAILABLE                                                                                    |
| **Wireframe Type**     | HTML                                                                                         |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-020-arrival-management.html                   |
| **Screen Spec**        | .propel/context/docs/figma_spec.md#SCR-020                                                   |
| **UXR Requirements**   | UXR-004 (Inline search), UXR-501 (200ms action feedback)                                     |
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

> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)

| Reference Type       | Value |
| -------------------- | ----- |
| **Mobile Impact**    | No    |
| **Platform Target**  | N/A   |
| **Min OS Version**   | N/A   |
| **Mobile Framework** | N/A   |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement the Staff Arrival Management UI (SCR-020) allowing Staff users to search for patients with appointments today and mark them as "Arrived". The interface includes patient search with real-time filtering, appointment details display with "Mark Arrived" CTA, edge case handling (no appointment found, already arrived, cancelled appointment), and a link to walk-in booking for patients without appointments. This page is exclusively accessible to Staff role users and implements RBAC enforcement at the routing level.

## Dependent Tasks

- US_029 Task 001 (Walk-in Booking UI) - for navigation link when no appointment found
- US_030 Task 001 (Queue Management UI) - patient is added to queue after marking arrived

## Impacted Components

- **NEW**: `src/frontend/src/pages/staff/ArrivalManagement.tsx` - Main arrival management page component
- **NEW**: `src/frontend/src/features/staff/components/ArrivalSearchInput.tsx` - Patient search input with today's appointments filter
- **NEW**: `src/frontend/src/features/staff/components/AppointmentCard.tsx` - Display appointment details with "Mark Arrived" button
- **MODIFY**: `src/frontend/src/App.tsx` - Add Staff-only route for /staff/arrivals
- **MODIFY**: `src/frontend/src/api/client.ts` - Add API methods for appointment search and arrival marking

## Implementation Plan

1. **Set Up Page Structure and Routing**
   - Create `ArrivalManagement.tsx` page component in `src/pages/staff/`
   - Add Staff-only route `/staff/arrivals` in App.tsx with role guard
   - Implement page layout matching wireframe SCR-020

2. **Implement Patient Search Component**
   - Create `ArrivalSearchInput.tsx` with controlled input and 300ms debounce
   - Integrate API call to `/api/staff/arrivals/search?query={term}&date={today}` endpoint
   - Display search results in list format with patient name, DOB, and appointment time
   - Handle empty results state:
     - Display "No appointment found for today" message
     - Show "Create Walk-in Booking" CTA button linking to `/staff/walkin-booking?patientQuery={term}`
   - Implement keyboard navigation (arrow keys, Enter to select)

3. **Implement Appointment Details Card**
   - Create `AppointmentCard.tsx` displaying:
     - Patient name and DOB
     - Appointment date and time
     - Provider name
     - Visit reason
     - Appointment status
   - Add "Mark Arrived" primary button (large, prominent)
   - Display current status badge (Scheduled, Confirmed, Arrived, Cancelled)
   - Apply conditional rendering based on status:
     - Show "Mark Arrived" button only if status is "Scheduled" or "Confirmed"
     - Display "Already Arrived" message if status is "Arrived"
     - Display "Appointment Cancelled" message with disabled state if status is "Cancelled"

4. **Implement Mark Arrival Action**
   - Add click handler for "Mark Arrived" button
   - Show confirmation modal: "Mark [Patient Name] as arrived for [Time] appointment?"
   - On confirm, POST to `/api/staff/arrivals/{appointmentId}/mark-arrived` endpoint
   - Display loading spinner during API call (< 200ms per UXR-501)
   - On success:
     - Display success toast: "[Patient Name] marked as arrived and added to queue"
     - Update appointment card status to "Arrived"
     - Disable "Mark Arrived" button
     - Clear search input for next patient
   - On error, display error toast with message from API

5. **Implement Edge Case Handling**
   - Handle "already arrived" scenario:
     - API returns 409 Conflict with message "Patient already marked as arrived"
     - Display info toast instead of error
     - Update card to show "Arrived" status
   - Handle "cancelled appointment" scenario:
     - Disable "Mark Arrived" button if status is "Cancelled"
     - Display message: "This appointment was cancelled. Cannot mark as arrived."
   - Handle no appointment found:
     - Display empty state with illustration
     - Message: "No appointment found for today. Would you like to create a walk-in booking?"
     - CTA button: "Create Walk-in Booking" (navigate to walk-in page with patient query)

6. **Implement State Management and Error Handling**
   - Use local component state for search query, selected appointment, and loading states
   - Display loading spinner during API calls
   - Show error toast notifications for API failures
   - Clear form after successful arrival marking

7. **Implement Accessibility**
   - Add ARIA labels for all form inputs and buttons
   - Ensure keyboard navigation for all interactive elements
   - Implement focus management (focus on success toast, return to search input)
   - Use semantic HTML (form, button, input elements)

8. **Apply Design Tokens and Responsive Styling**
   - Reference design tokens from designsystem.md for colors, spacing, typography
   - Implement responsive layout with breakpoints: 375px (mobile), 768px (tablet), 1440px (desktop)
   - Match wireframe layout, spacing, and component positioning
   - Use status badge colors: green for "Arrived", blue for "Confirmed", gray for "Scheduled", red for "Cancelled"

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
│   └── staff/
│       └── components/
│           ├── PatientSearchInput.tsx        # From US_029 (can reuse pattern)
│           └── WalkinBookingForm.tsx         # From US_029
├── pages/
│   └── staff/
│       ├── WalkinBooking.tsx                 # From US_029
│       └── QueueManagement.tsx               # From US_030
└── components/
    └── common/                               # Shared components (existing)
```

## Expected Changes

| Action | File Path                                                         | Description                                                            |
| ------ | ----------------------------------------------------------------- | ---------------------------------------------------------------------- |
| CREATE | src/frontend/src/pages/staff/ArrivalManagement.tsx                | Main arrival management page with search and appointment display       |
| CREATE | src/frontend/src/features/staff/components/ArrivalSearchInput.tsx | Patient search input filtered for today's appointments                 |
| CREATE | src/frontend/src/features/staff/components/AppointmentCard.tsx    | Appointment details card with "Mark Arrived" button and status display |
| MODIFY | src/frontend/src/App.tsx                                          | Add Staff-only route `/staff/arrivals` with ProtectedRoute wrapper     |
| MODIFY | src/frontend/src/api/client.ts                                    | Add methods: searchArrivals(query, date), markArrived(appointmentId)   |

## External References

- React Debounce Hook: https://usehooks-ts.com/react-hook/use-debounce
- React Router Navigation: https://reactrouter.com/en/main/hooks/use-navigate
- Tailwind CSS Badge Component: https://tailwindui.com/components/application-ui/elements/badges
- WCAG 2.2 AA Guidelines: https://www.w3.org/WAI/WCAG22/quickref/?currentsidebar=%23col_customize&levels=aa

## Build Commands

- `npm run dev` - Start development server on http://localhost:5173
- `npm run build` - Build for production
- `npm run test` - Run Vitest unit tests
- `npm run lint` - Run ESLint
- `npm run format` - Run Prettier

## Implementation Validation Strategy

- [ ] Unit tests pass for ArrivalSearchInput, AppointmentCard components
- [ ] Integration tests pass for search API call with today's date filter
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] API calls correctly invoke backend endpoints with proper authentication headers
- [ ] RBAC enforcement verified - non-Staff users cannot access /staff/arrivals route
- [ ] Accessibility audit passes with axe DevTools (zero critical violations)
- [ ] Keyboard navigation works for all interactive elements
- [ ] Action feedback displays within 200ms per UXR-501
- [ ] Edge cases handled correctly (already arrived, cancelled, no appointment)
- [ ] Success toast displays patient name and queue confirmation

## Implementation Checklist

- [ ] Create ArrivalManagement.tsx page component with staff layout structure
- [ ] Add Staff-only route `/staff/arrivals` in App.tsx with role-based access guard
- [ ] Create ArrivalSearchInput.tsx with 300ms debounce
- [ ] Implement search API call to `/api/staff/arrivals/search?query={term}&date={today}`
- [ ] Display search results in list format with patient name, DOB, appointment time
- [ ] Add keyboard navigation (arrow keys, Enter) for search results
- [ ] Create AppointmentCard.tsx component displaying appointment details
- [ ] Display patient name, DOB, appointment date/time, provider, visit reason, status
- [ ] Add status badge with conditional colors (green=Arrived, blue=Confirmed, gray=Scheduled, red=Cancelled)
- [ ] Implement "Mark Arrived" button (visible only for Scheduled/Confirmed status)
- [ ] Add confirmation modal: "Mark [Patient Name] as arrived for [Time] appointment?"
- [ ] Implement mark arrival action - POST to `/api/staff/arrivals/{appointmentId}/mark-arrived`
- [ ] Display loading spinner during API call
- [ ] On success, display toast: "[Patient Name] marked as arrived and added to queue"
- [ ] Update appointment card status to "Arrived" after successful marking
- [ ] Disable "Mark Arrived" button after successful marking
- [ ] Clear search input after successful marking
- [ ] Handle "already arrived" edge case (409 Conflict):
  - [ ] Display info toast: "Patient already marked as arrived"
  - [ ] Update card to show "Arrived" status
- [ ] Handle "cancelled appointment" edge case:
  - [ ] Disable "Mark Arrived" button if status is "Cancelled"
  - [ ] Display message: "This appointment was cancelled"
- [ ] Handle "no appointment found" scenario:
  - [ ] Display empty state with message
  - [ ] Add "Create Walk-in Booking" CTA button
  - [ ] Navigate to `/staff/walkin-booking?patientQuery={term}` on click
- [ ] Add error toast notifications for API failures with descriptive messages
- [ ] Apply design tokens from designsystem.md (colors, spacing, typography)
- [ ] Implement responsive layout matching wireframe at 375px, 768px, 1440px breakpoints
- [ ] Add ARIA labels for all form inputs, buttons, and interactive elements
- [ ] Ensure keyboard-only navigation through entire arrival flow
- [ ] Test action feedback timing (< 200ms per UXR-501)
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
