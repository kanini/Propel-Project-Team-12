# Task - task_001_fe_appointment_booking_calendar

## Requirement Reference
- User Story: US_024
- Story Location: .propel/context/tasks/EP-002/us_024/us_024.md
- Acceptance Criteria:
    - AC-1: Calendar view displays with available time slots highlighted and unavailable slots grayed out
    - AC-2: Date selection displays available time slots with API response within 500ms at P95 (NFR-001)
    - AC-3: Booking validates slot availability, creates appointment, displays confirmation
    - AC-4: Concurrent booking conflict shows "Slot no longer available" and refreshes calendar
    - AC-5: Multi-step progress indicator shows current step (Provider → Date/Time → Details → Confirm)
- Edge Case:
    - No slots in future months: Show "No availability" with waitlist option
    - Slow API responses: Skeleton loading after 300ms, error message after 10-second timeout

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-007-appointment-booking.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-007 |
| **UXR Requirements** | UXR-101 (Progress indicators), UXR-201 (WCAG 2.2 AA), UXR-501 (200ms action feedback), UXR-601 (Inline validation) |
| **Design Tokens** | .propel/context/docs/designsystem.md#colors, #typography, #spacing |

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
| Frontend | Tailwind CSS | 3.x |
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | React Router | 6.x |
| Library | date-fns | 2.x |
| Library | react-calendar | 4.x |
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
Implement the Appointment Booking Calendar and multi-step booking flow for the Patient Portal. Create an interactive calendar component displaying provider availability with visual distinction between available (highlighted) and unavailable (grayed) time slots. Implement a 4-step booking wizard (Provider → Date/Time → Details → Confirm) with progress indicator per UXR-101. Handle concurrent booking conflicts with 409 error handling showing "Slot no longer available" message and automatic calendar refresh. Integrate with backend APIs meeting 500ms P95 response time target (NFR-001). Support optional preferred slot swap selection and visit reason entry with inline validation (UXR-601).

## Dependent Tasks
- task_002_be_appointment_booking_api.md - Backend APIs for availability and booking

## Impacted Components
- Frontend (React): New components to be created
  - `src/frontend/src/pages/AppointmentBooking.tsx` (NEW)
  - `src/frontend/src/components/appointments/CalendarView.tsx` (NEW)
  - `src/frontend/src/components/appointments/TimeSlotGrid.tsx` (NEW)
  - `src/frontend/src/components/appointments/BookingSteps.tsx` (NEW)
  - `src/frontend/src/components/appointments/ProgressIndicator.tsx` (NEW)
  - `src/frontend/src/components/appointments/VisitReasonForm.tsx` (NEW)
  - `src/frontend/src/components/appointments/ConfirmationDialog.tsx` (NEW)
  - `src/frontend/src/store/slices/appointmentSlice.ts` (NEW)

## Implementation Plan
1. **Create multi-step wizard state management**:
   - Redux appointmentSlice: currentStep (1-4), selectedProvider, selectedDate, selectedTimeSlot, visitReason, preferredSlotSwap
   - Step navigation: nextStep(), previousStep(), resetBooking()
   - Validation per step before allowing navigation forward

2. **Build ProgressIndicator component**:
   - Display 4 steps: "Provider" → "Date/Time" → "Details" → "Confirm"
   - Highlight current step, gray out future steps, checkmark completed steps
   - Responsive: horizontal stepper on desktop, vertical on mobile

3. **Implement CalendarView component**:
   - Use react-calendar or build custom calendar with date-fns
   - Fetch availability for selected provider: GET /api/providers/{providerId}/availability?month={month}
   - Visual states: Available (primary color), Unavailable (gray), Selected (highlighted)
   - Month navigation with "No availability" message when no slots exist
   - Edge case: If no slots for 30+ days, show "Join Waitlist" CTA button

4. **Create TimeSlotGrid component**:
   - Display time slots for selected date (30-minute intervals: 9:00 AM - 5:00 PM)
   - Fetch slots: GET /api/providers/{providerId}/availability?date={date}
   - Visual states: Available (clickable), Booked (disabled), Selected (highlighted)
   - Loading state: Skeleton loading after 300ms (UXR-502)
   - Timeout handling: Error message after 10 seconds with retry button

5. **Build VisitReasonForm component**:
   - TextField for visit reason (required, max 200 chars)
   - Checkbox: "Enable preferred slot swap" (optional)
   - If checked, show secondary calendar to select preferred unavailable slot
   - Inline validation: Visit reason required (UXR-601)

6. **Implement booking submission logic**:
   - POST /api/appointments with { providerId, timeSlotId, visitReason, preferredSlotId? }
   - Optimistic UI update, then handle response
   - Success (201): Navigate to confirmation screen
   - Conflict (409): Show error toast "Slot no longer available", refresh calendar automatically
   - Validation error (400): Display inline errors below form fields
   - Server error (500): Show generic error modal with retry option

7. **Create ConfirmationDialog component**:
   - Display appointment details: Date, Time, Provider, Visit Reason
   - "Add to Calendar" buttons (Google, Outlook)
   - "Download PDF" button
   - "Book Another Appointment" CTA
   - Navigate to "My Appointments" button

8. **Integrate progress indicator (UXR-101)**:
   - Step 1: Provider selection (from provider browser)
   - Step 2: Date/Time selection (calendar + time slots)
   - Step 3: Details (visit reason + optional preferred swap)
   - Step 4: Confirmation (success screen)
   - "Back" button available on steps 2-3, disabled on step 1, hidden on step 4

## Current Project State
```
src/frontend/
├── src/
│   ├── pages/
│   │   └── (AppointmentBooking.tsx to be created)
│   ├── components/
│   │   └── appointments/
│   │       └── (CalendarView.tsx, TimeSlotGrid.tsx, BookingSteps.tsx, etc. to be created)
│   └── store/
│       └── slices/
│           └── (appointmentSlice.ts to be created)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/pages/AppointmentBooking.tsx | Main booking page with 4-step wizard orchestration |
| CREATE | src/frontend/src/components/appointments/CalendarView.tsx | Calendar component with availability highlighting |
| CREATE | src/frontend/src/components/appointments/TimeSlotGrid.tsx | Time slot selection grid for selected date |
| CREATE | src/frontend/src/components/appointments/BookingSteps.tsx | Step container component with navigation |
| CREATE | src/frontend/src/components/appointments/ProgressIndicator.tsx | 4-step progress bar (Provider → Date/Time → Details → Confirm) |
| CREATE | src/frontend/src/components/appointments/VisitReasonForm.tsx | Visit reason input and optional preferred swap selection |
| CREATE | src/frontend/src/components/appointments/ConfirmationDialog.tsx | Booking success confirmation screen |
| CREATE | src/frontend/src/store/slices/appointmentSlice.ts | Redux slice for booking wizard state |
| MODIFY | src/frontend/src/App.tsx | Add route for /appointments/book/:providerId |
| MODIFY | src/frontend/src/store/store.ts | Register appointmentSlice reducer |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [React Calendar Library](https://www.npmjs.com/package/react-calendar)
- [date-fns Documentation](https://date-fns.org/docs/Getting-Started)
- [Multi-Step Form Pattern](https://www.smashingmagazine.com/2023/03/good-better-best-designing-effective-multi-step-forms/)
- [Optimistic UI Updates](https://redux-toolkit.js.org/rtk-query/usage/optimistic-updates)
- [Concurrent Request Handling](https://axios-http.com/docs/cancellation)
- [WCAG 2.2 Progress Indicators](https://www.w3.org/WAI/WCAG22/Understanding/status-messages)

## Build Commands
- `npm run dev` - Start Vite development server
- `npm run build` - Build production bundle
- `npm run test` - Run Vitest unit tests for booking components
- `npm run lint` - Run ESLint checks

## Implementation Validation Strategy
- [ ] Unit tests pass for CalendarView, TimeSlotGrid, ProgressIndicator components
- [ ] Redux slice tests verify step navigation and state transitions
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Progress indicator updates correctly at each step (UXR-101)
- [ ] Available slots highlighted, unavailable slots grayed out (AC-1)
- [ ] Date selection loads time slots within 500ms P95 (AC-2, NFR-001)
- [ ] Skeleton loading appears after 300ms for slow responses
- [ ] Timeout error shown after 10 seconds with retry button
- [ ] Booking submission handles 409 conflict with auto-refresh (AC-4)
- [ ] Visit reason validation works (required field)
- [ ] Preferred slot swap checkbox toggles secondary calendar
- [ ] Confirmation screen displays appointment details correctly
- [ ] "Back" button navigation works (steps 2-3 only)
- [ ] Accessibility: WCAG 2.2 AA compliance verified

## Implementation Checklist
- [x] Create Redux appointmentSlice with state: currentStep, selectedProvider, selectedDate, selectedTimeSlot, visitReason, preferredSlotId
- [x] Implement step navigation actions: setStep(), nextStep(), previousStep(), resetBooking()
- [x] Build ProgressIndicator with 4 steps and visual states (current, completed, future)
- [x] Create CalendarView component using react-calendar or date-fns
- [x] Fetch monthly availability: GET /api/providers/{providerId}/availability?month={month}
- [x] Highlight available dates in calendar, gray out unavailable dates
- [x] Implement month navigation with "No availability" message
- [x] Create TimeSlotGrid displaying 30-minute intervals (9 AM - 5 PM)
- [x] Fetch daily slots: GET /api/providers/{providerId}/availability?date={date}
- [x] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
- [x] Add skeleton loading with 300ms delay (UXR-502)
- [x] Implement 10-second timeout with error message and retry button
- [x] Build VisitReasonForm with visit reason TextField (max 200 chars, required)
- [x] Add "Enable preferred slot swap" checkbox
- [x] Show secondary calendar when swap enabled (select unavailable slot)
- [x] Implement inline validation for visit reason (UXR-601)
- [x] Create booking submission handler: POST /api/appointments
- [x] Handle 201 Success: Navigate to confirmation
- [x] Handle 409 Conflict: Show "Slot no longer available" toast, refresh calendar
- [x] Handle 400 Validation: Display inline errors
- [x] Handle 500 Server error: Show error modal with retry
- [x] Build ConfirmationDialog with appointment details display
- [x] Add "Add to Calendar" buttons (Google, Outlook)
- [x] Add "Download PDF" button
- [x] Add navigation buttons: "Book Another", "My Appointments"
- [x] Integrate "Back" button on steps 2-3 (disabled on step 1, hidden on step 4)
- [x] Add route /appointments/book/:providerId in App.tsx
- [x] Register appointmentSlice in Redux store
- [x] Write unit tests for ProgressIndicator step transitions
- [ ] Write unit tests for CalendarView date selection
- [ ] Write unit tests for TimeSlotGrid slot selection
- [x] Write unit tests for VisitReasonForm validation
- [x] Write Redux slice tests for step navigation
- [ ] Test responsive breakpoints (375px, 768px, 1440px)
- [ ] Verify WCAG 2.2 AA compliance for progress indicator
- [ ] Manual test concurrent booking conflict (409 handling)
- [ ] Verify API response time <500ms P95 with network throttling
