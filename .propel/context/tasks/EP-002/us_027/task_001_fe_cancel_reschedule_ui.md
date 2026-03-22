# Task - task_001_fe_cancel_reschedule_ui

## Requirement Reference
- User Story: US_027
- Story Location: .propel/context/tasks/EP-002/us_027/us_027.md
- Acceptance Criteria:
    - AC-1: Cancel from My Appointments with confirmation, updates status, releases slot, sends notification, removes calendar event
    - AC-2: Reschedule displays alternative slots for same provider
    - AC-3: Confirm reschedule releases original, books new, sends confirmation, updates calendar
    - AC-4: Cancellation policy enforcement based on advance notice configuration

## Design References
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-010-my-appointments.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-010 |
| **UXR Requirements** | UXR-501 (200ms feedback), UXR-601 (Inline validation), UXR-605 (Empty states) |

### **CRITICAL: Wireframe Reference MANDATORY**
- MUST reference wireframe during implementation
- MUST validate UI matches wireframe before complete

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Frontend | TypeScript | 5.x |
| Frontend | Redux Toolkit | 2.x |
| Frontend | Tailwind CSS | 3.x |

## Task Overview
Add cancel and reschedule actions to My Appointments page. Create confirmation modal for cancellation showing policy restrictions (e.g., "Must cancel 24 hours before appointment"). Implement reschedule flow showing available alternative slots via calendar picker. Handle policy violations (403 Forbidden) with error message. Update appointment list after successful operations.

## Dependent Tasks
- task_002_be_cancel_reschedule_api.md - Backend cancellation and rescheduling logic

## Impacted Components
- `src/frontend/src/pages/MyAppointments.tsx` (UPDATE - add Cancel/Reschedule buttons)
- `src/frontend/src/components/appointments/CancelConfirmationModal.tsx` (NEW)
- `src/frontend/src/components/appointments/RescheduleModal.tsx` (NEW)
- `src/frontend/src/store/slices/appointmentSlice.ts` (UPDATE - cancel/reschedule actions)

## Implementation Plan
1. Add "Cancel" and "Reschedule" buttons to AppointmentCard in My Appointments
2. Create CancelConfirmationModal: Show cancellation policy message, Confirm/Cancel buttons
3. Implement cancelAppointment action: DELETE /api/appointments/{id}, handle 403 Forbidden (policy violation)
4. Create RescheduleModal: Display calendar with available slots for appointment's provider
5. Implement rescheduleAppointment action: PATCH /api/appointments/{id}/reschedule with newTimeSlotId
6. Handle success: Refresh appointment list, show success toast, trigger calendar sync update
7. Display policy error: "Cancellation not allowed within 24 hours of appointment time"

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| UPDATE | src/frontend/src/pages/MyAppointments.tsx | Add Cancel/Reschedule action buttons |
| CREATE | src/frontend/src/components/appointments/CancelConfirmationModal.tsx | Modal with policy notice and confirmation |
| CREATE | src/frontend/src/components/appointments/RescheduleModal.tsx | Calendar picker for alternative slots |
| UPDATE | src/frontend/src/store/slices/appointmentSlice.ts | Add cancelAppointment, rescheduleAppointment async thunks |

## Implementation Checklist
- [x] Add "Cancel" and "Reschedule" buttons to AppointmentCard
- [x] Create CancelConfirmationModal with policy message display
- [x] **[UI Tasks - MANDATORY]** Reference wireframe during implementation
- [x] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before complete
- [x] Implement DELETE /api/appointments/{id} call in cancelAppointment thunk
- [x] Handle 403 Forbidden: Display error toast "Cancellation not allowed..."
- [x] Create RescheduleModal with calendar showing alternative slots
- [x] Fetch alternative slots: GET /api/providers/{providerId}/availability
- [x] Implement PATCH /api/appointments/{id}/reschedule with newTimeSlotId
- [x] Handle conflict (slot no longer available): Show error, refresh calendar
- [x] Refresh appointment list after successful cancel/reschedule
- [x] Show success toast: "Appointment cancelled" or "Appointment rescheduled"

Estimated effort: 5 hours.

