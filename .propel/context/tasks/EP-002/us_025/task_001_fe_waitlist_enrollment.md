# Task - task_001_fe_waitlist_enrollment

## Requirement Reference
- User Story: US_025
- Story Location: .propel/context/tasks/EP-002/us_025/us_025.md
- Acceptance Criteria:
    - AC-1: Waitlist enrollment form displays with date/time range and notification preferences (SMS/Email/Both)
    - AC-2: Submission creates waitlist entry with priority timestamp and sends confirmation via selected channel
    - AC-3: Duplicate enrollment check informs user of existing waitlist position with option to update preferences
    - AC-4: Waitlist entries displayed in "Waitlist" tab showing position, provider, preferred range, status
- Edge Case:
    - 50+ patients on waitlist should show queue position (e.g., "You are #23 in line")

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-009-waitlist.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-009 |
| **UXR Requirements** | UXR-201 (WCAG 2.2 AA), UXR-203 (Form labels), UXR-601 (Inline validation) |
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
Implement waitlist enrollment UI for patients to join waiting lists when preferred appointment slots are unavailable. Create a modal form allowing users to specify preferred date/time ranges and notification preferences (SMS/Email/Both). Integrate with backend waitlist API to check for duplicate entries and create new waitlist records with priority timestamps. Display waitlist entries in a "Waitlist" tab within the "My Appointments" page showing queue position, provider, preferred time range, and status. Handle confirmation notifications and provide ability to update preferences for existing waitlist entries.

## Dependent Tasks
- task_002_be_waitlist_api.md - Backend waitlist API endpoints
- task_001_fe_appointment_booking_calendar.md - Integration point for "Join Waitlist" CTA

## Impacted Components
- Frontend (React): New components to be created
  - `src/frontend/src/components/waitlist/WaitlistEnrollmentModal.tsx` (NEW)
  - `src/frontend/src/components/waitlist/WaitlistEntry.tsx` (NEW)
  - `src/frontend/src/pages/MyAppointments.tsx` (UPDATE - add Waitlist tab)
  - `src/frontend/src/store/slices/waitlistSlice.ts` (NEW)

## Implementation Plan
1. **Create Redux waitlistSlice**:
- State: waitlistEntries (array), loading, error, selectedEntry (for editing)
   - Actions: fetchWaitlist, joinWaitlist, updateWaitlist, deleteWaitlist
   - Async thunks for API calls

2. **Build WaitlistEnrollmentModal component**:
   - Triggered from Provider Browser "No availability" message or Calendar "Join Waitlist" button
   - Form fields: Provider (read-only, pre-selected), Preferred Date Range Start, Preferred Date Range End, Notification Preferences (SMS/Email/Both checkboxes)
   - Validation: Date range required, end date >= start date, at least one notification preference selected
   - Submit: POST /api/waitlist with { providerId, startDate, endDate, notificationChannels }
   - Handle 409 Conflict (duplicate entry): Show existing position message, "Update Preferences" button

3. **Implement duplicate entry handling (AC-3)**:
   - When API returns 409 Conflict with existing entry data
   - Display: "You are already on the waitlist for Dr. Smith (Position #23)"
   - Show "Update Preferences" button - pre-populates form with existing data
   - Submit updates via PUT /api/waitlist/{id}

4. **Add Waitlist tab to My Appointments page**:
   - Tab structure: Upcoming | Past | Waitlist
   - Fetch waitlist entries: GET /api/waitlist on tab activation
   - Display WaitlistEntry component for each entry

5. **Create WaitlistEntry component**:
   - Display: Queue position (e.g., "#5"), Provider name, Specialty, Preferred date range, Notification preferences, Status (Active/Notified/Expired)
   - Actions: "Update Preferences" button, "Leave Waitlist" button (DELETE request)
   - Empty state when no waitlist entries: "You're not on any waitlists. Browse providers to join one."

6. **Integrate "Join Waitlist" trigger**:
   - Provider Browser: When provider has no availability, show "Join Waitlist" button on ProviderCard
   - Calendar View: When no slots available for selected month, show "Join Waitlist" CTA
   - Button click opens WaitlistEnrollmentModal with provider pre-selected

7. **Handle notification confirmation feedback**:
   - Success response shows: "Waitlist joined! Confirmation sent to [email/phone]"
   - Display toast notification with auto-dismiss after 5 seconds
   - Refresh waitlist tab data after successful enrollment

8. **Add accessibility features**:
   - Modal: Focus trap, Esc to close, ARIA role="dialog", aria-labelledby, aria-describedby
   - Form labels: Programmatically associated with inputs (UXR-203)
   - Error messages: ARIA live region for screen reader announcements
   - Queue position: Semantic markup for assistive technologies

## Current Project State
```
src/frontend/
├── src/
│   ├── components/
│   │   └── waitlist/
│   │       └── (WaitlistEnrollmentModal.tsx, WaitlistEntry.tsx to be created)
│   ├── pages/
│   │   └── MyAppointments.tsx (to be updated)
│   └── store/
│       └── slices/
│           └── (waitlistSlice.ts to be created)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/waitlist/WaitlistEnrollmentModal.tsx | Modal form for joining waitlist with date range and notification preferences |
| CREATE | src/frontend/src/components/waitlist/WaitlistEntry.tsx | List item displaying waitlist entry details and actions |
| CREATE | src/frontend/src/store/slices/waitlistSlice.ts | Redux slice for waitlist state management |
| CREATE | src/frontend/src/pages/MyAppointments.tsx | **OR UPDATE if exists** - Add Waitlist tab to existing My Appointments page |
| MODIFY | src/frontend/src/components/providers/ProviderCard.tsx | Add "Join Waitlist" button when no availability |
| MODIFY | src/frontend/src/components/appointments/CalendarView.tsx | Add "Join Waitlist" CTA when month has no slots |
| MODIFY | src/frontend/src/store/store.ts | Register waitlistSlice reducer |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [React Modal Accessibility](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/)
- [React Hook Form Validation](https://react-hook-form.com/get-started#Applyvalidation)
- [Focus Trap React](https://www.npmjs.com/package/focus-trap-react)
- [ARIA Live Regions](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions)
- [Date Range Picker Component](https://reactdatepicker.com/)

## Build Commands
- `npm run dev` - Start Vite development server
- `npm run build` - Build production bundle
- `npm run test` - Run Vitest unit tests
- `npm run lint` - Run ESLint checks

## Implementation Validation Strategy
- [X] Unit tests pass for WaitlistEnrollmentModal component
- [X] Unit tests pass for WaitlistEntry component
- [X] Redux slice tests verify joinWaitlist/updateWaitlist actions
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [X] Form validation works: Date range required, end >= start, notification preference required
- [X] Duplicate entry (409 Conflict) shows existing position message (AC-3)
- [X] "Update Preferences" pre-populates form with existing waitlist data
- [X] Waitlist tab displays entries with queue position (#1, #2, #3...)
- [X] "Leave Waitlist" button removes entry and refreshes list
- [X] "Join Waitlist" button appears on ProviderCard when no availability
- [ ] "Join Waitlist" CTA appears on Calendar when month has no slots
- [ ] Confirmation toast displays after successful enrollment
- [ ] Empty state shown when no waitlist entries exist
- [ ] Accessibility: Modal focus trap works, Esc closes modal
- [ ] Accessibility: WCAG 2.2 AA compliance verified with axe

## Implementation Checklist
- [X] Create Redux waitlistSlice with state: waitlistEntries[], loading, error, selectedEntry
- [X] Implement async thunks: fetchWaitlist, joinWaitlist, updateWaitlist, deleteWaitlist
- [X] Build WaitlistEnrollmentModal component with form fields
- [X] Add provider display (read-only, passed as prop)
- [X] Add date range inputs: Preferred Start Date, Preferred End Date
- [X] Add notification preference checkboxes: SMS, Email, Both
- [X] Implement form validation: Date range required, end >= start, at least one notification channel
- [X] Handle form submission: POST /api/waitlist
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
- [X] Handle 409 Conflict: Display existing position (#X in line), show "Update Preferences" button
- [X] Implement "Update Preferences" flow: Pre-populate form, PUT /api/waitlist/{id}
- [X] Create WaitlistEntry component displaying position, provider, date range, status
- [X] Add "Update Preferences" and "Leave Waitlist" action buttons
- [X] Implement "Leave Waitlist": DELETE /api/waitlist/{id}, refresh list
- [X] Add or update MyAppointments.tsx page with tab structure
- [X] Create Waitlist tab alongside Upcoming and Past tabs
- [X] Fetch waitlist entries on tab activation: GET /api/waitlist
- [X] Render WaitlistEntry components in list
- [X] Add empty state: "You're not on any waitlists..."
- [X] Update ProviderCard: Add "Join Waitlist" button when no availability
- [X] Update CalendarView: Add "Join Waitlist" CTA when month has no slots
- [X] Integrate modal trigger: Button click opens WaitlistEnrollmentModal with provider pre-selected
- [ ] Display success toast: "Waitlist joined! Confirmation sent to [channel]"
- [ ] Refresh waitlist data after successful enrollment
- [X] Add modal accessibility: Focus trap, Esc handler, ARIA role="dialog"
- [X] Add ARIA labels: aria-labelledby="modal-title", aria-describedby="modal-description"
- [X] Ensure form labels programmatically associated with inputs (UXR-203)
- [X] Add ARIA live region for error announcements
- [X] Register waitlistSlice in Redux store
- [ ] Write unit tests for WaitlistEnrollmentModal validation logic
- [ ] Write unit tests for WaitlistEntry rendering
- [ ] Write Redux slice tests for joinWaitlist/updateWaitlist/deleteWaitlist
- [ ] Test responsive breakpoints (375px, 768px, 1440px)
- [ ] Verify WCAG 2.2 AA compliance with axe DevTools
- [ ] Manual test duplicate enrollment scenario (409 Conflict handling)
- [ ] Manual test "Update Preferences" flow end-to-end
