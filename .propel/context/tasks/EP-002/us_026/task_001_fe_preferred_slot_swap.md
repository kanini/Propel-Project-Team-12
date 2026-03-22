# Task - task_001_fe_preferred_slot_swap

## Requirement Reference
- User Story: US_026
- Story Location: .propel/context/tasks/EP-002/us_026/us_026.md
- Acceptance Criteria:
    - AC-1: During booking, option to mark preferred unavailable slot as swap preference
    - AC-2: Automatic swap to preferred slot when available, with confirmation notification
    - AC-3: Race condition handling - maintain original booking if preferred slot booked by another patient
    - AC-4: Cancel swap preference option, maintaining original booking
    - AC-5: My Appointments reflects new time after swap, calendar integration updated, original slot released
- Edge Case:
    - Original and preferred slots on same day - handle atomic swap transition

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-007-appointment-booking.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-007 |
| **UXR Requirements** | UXR-501 (200ms action feedback) |
| **Design Tokens** | .propel/context/docs/designsystem.md#colors, #typography, #spacing |

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
| AI/ML | N/A | N/A |

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
|Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview
Extend appointment booking flow with optional preferred slot swap feature. Add checkbox "Enable Preferred Slot Swap" during booking that reveals secondary calendar to select a preferred unavailable time slot. Store swap preference in backend (PreferredSlotId). Display swap status in My Appointments with "Cancel Swap Preference" option. Handle automatic swap notifications when preferred slot becomes available. Update UI to reflect swapped appointment time and sync with calendar integrations.

## Dependent Tasks
- task_001_fe_appointment_booking_calendar.md - Base booking flow
- task_002_be_preferred_slot_swap_engine.md - Backend swap detection and execution

## Impacted Components
- Frontend (React):
  - `src/frontend/src/components/appointments/VisitReasonForm.tsx` (UPDATE - add swap preference checkbox)
  - `src/frontend/src/components/appointments/PreferredSlotSelector.tsx` (NEW)
  - `src/frontend/src/pages/MyAppointments.tsx` (UPDATE - show swap status, cancel button)
  - `src/frontend/src/components/appointments/AppointmentCard.tsx` (NEW or UPDATE - display swap indicator)

## Implementation Plan  
1. Update VisitReasonForm: Add "Enable Preferred Slot Swap" checkbox
2. Create PreferredSlotSelector: Secondary calendar showing unavailable slots only
3. Update appointmentSlice Redux state: Add preferredSlotId field
4. Modify booking submission: Include preferredSlotId in POST /api/appointments
5. Update AppointmentCard: Show swap icon and "Preferred: [new time]" label when swap active
6. Add "Cancel Swap Preference" button: PATCH /api/appointments/{id}/swap preference with preferredSlotId = null
7. Handle swap notification: Listen for real-time updates (WebSocket/polling) showing swap success modal

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| UPDATE | src/frontend/src/components/appointments/VisitReasonForm.tsx | Add "Enable Preferred Slot Swap" checkbox and toggle logic |
| CREATE | src/frontend/src/components/appointments/PreferredSlotSelector.tsx | Secondary calendar for selecting unavailable preferred slot |
| UPDATE | src/frontend/src/pages/MyAppointments.tsx | Display swap status and "Cancel Swap Preference" button |
| CREATE | src/frontend/src/components/appointments/AppointmentCard.tsx | Card component with swap indicator icon and preferred time label |
| UPDATE | src/frontend/src/store/slices/appointmentSlice.ts | Add preferredSlotId to state and update booking action |

## Implementation Checklist
- [ ] Update VisitReasonForm: Add checkbox "Enable Preferred Slot Swap"
- [ ] Show/hide PreferredSlotSelector based on checkbox state
- [ ] Create PreferredSlotSelector component displaying unavailable slots only
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before complete
- [ ] Update appointmentSlice: Add preferredSlotId to booking request payload
- [ ] Modify POST /api/appointments to include preferredSlotId
- [ ] Create AppointmentCard with swap status indicator (icon + "Preferred: [time]" label)
- [ ] Add "Cancel Swap Preference" button calling PATCH /api/appointments/{id}/swap
- [ ] Implement swap notification handler (modal: "Your appointment was moved to [new time]")
- [ ] Update My Appointments after swap: Refresh appointment list, update calendar sync

Task truncated for brevity. Estimated effort: 6 hours.

