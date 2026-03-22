# Task - task_001_fe_queue_management_ui

## Requirement Reference

- User Story: US_030 - Same-Day Queue Management Interface
- Story Location: .propel/context/tasks/EP-003/us_030/us_030.md
- Acceptance Criteria:
  - AC-1: Given I am on the Queue Management page, When the page loads, Then same-day patients are displayed in chronological order (arrival time) with patient name, appointment type, provider, arrival time, and estimated wait time.
  - AC-2: Given new patients are added to the queue, When a walk-in or arrival is registered, Then the queue updates in real-time via Pusher Channels without requiring page refresh.
  - AC-3: Given I need to adjust priority, When I flag a patient for priority (e.g., emergency), Then the patient moves to the appropriate position in the queue and wait times recalculate.
  - AC-4: Given the queue is empty, When no patients are waiting, Then an empty state illustration displays with a guiding CTA (e.g., "No patients in queue. Book a walk-in?").
- Edge Case:
  - What happens when the Pusher connection drops? Queue should periodically poll (every 30 seconds) as fallback and display a "Live updates paused" indicator.
  - How does the system handle queue for multiple providers? Queue can be filtered by provider with a default "All Providers" view.

## Design References (Frontend Tasks Only)

| Reference Type         | Value                                                                                        |
| ---------------------- | -------------------------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                                          |
| **Figma URL**          | N/A                                                                                          |
| **Wireframe Status**   | AVAILABLE                                                                                    |
| **Wireframe Type**     | HTML                                                                                         |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-019-queue-management.html                     |
| **Screen Spec**        | .propel/context/docs/figma_spec.md#SCR-019                                                   |
| **UXR Requirements**   | UXR-207 (ARIA live regions), UXR-502 (Skeleton loading), UXR-605 (Empty states)              |
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
| Library      | Pusher JS     | 8.x     |
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

Implement the Staff Queue Management UI (SCR-019) displaying same-day patients in chronological order with real-time updates via Pusher Channels. The interface shows patient name, appointment type, provider, arrival time, and estimated wait time for all patients with status "Arrived". Staff can filter by provider, flag patients for priority (emergency), and see automatic wait time recalculation. The implementation includes WebSocket connection management with 30-second polling fallback when connection drops, ARIA live regions for screen reader announcements, and empty state handling with CTA.

## Dependent Tasks

- None (backend queue endpoints assumed to exist or will be created in parallel backend task)

## Impacted Components

- **NEW**: `src/frontend/src/pages/staff/QueueManagement.tsx` - Main queue management page component
- **NEW**: `src/frontend/src/features/staff/components/QueueList.tsx` - Queue display table with patient rows
- **NEW**: `src/frontend/src/features/staff/components/QueuePatientRow.tsx` - Individual patient row component with priority flag button
- **NEW**: `src/frontend/src/features/staff/components/ProviderFilter.tsx` - Provider dropdown filter (All Providers default)
- **NEW**: `src/frontend/src/hooks/usePusherQueue.ts` - Custom hook for Pusher connection and fallback polling
- **NEW**: `src/frontend/src/utils/queueHelpers.ts` - Utility functions for wait time calculation
- **MODIFY**: `src/frontend/src/App.tsx` - Add Staff-only route for /staff/queue
- **MODIFY**: `src/frontend/src/api/client.ts` - Add API methods for queue fetch, priority update

## Implementation Plan

1. **Set Up Pusher Connection and Fallback Polling**
   - Install `pusher-js` package: `npm install pusher-js`
   - Create `usePusherQueue.ts` custom hook:
     - Initialize Pusher client with app key from environment variables
     - Subscribe to "queue-updates" channel
     - Bind to "patient-added", "patient-removed", "priority-changed" events
     - Implement connection state tracking (connected, connecting, disconnected)
     - Implement 30-second polling fallback using setInterval when connection drops
     - Display "Live updates paused" indicator when using fallback
     - Clean up connection on unmount

2. **Implement Queue Management Page Structure**
   - Create `QueueManagement.tsx` page component in `src/pages/staff/`
   - Add Staff-only route `/staff/queue` in App.tsx with role guard
   - Fetch initial queue data from `/api/staff/queue` on mount
   - Display loading skeleton while fetching initial data
   - Implement page layout matching wireframe SCR-019

3. **Implement Provider Filter Component**
   - Create `ProviderFilter.tsx` with dropdown select
   - Fetch provider list from `/api/providers` on mount
   - Add "All Providers" option as default selection
   - Filter queue data by selected provider on change
   - Maintain filter selection in component state

4. **Implement Queue List and Patient Rows**
   - Create `QueueList.tsx` table component with headers: Position, Patient Name, Appointment Type, Provider, Arrival Time, Est. Wait Time, Priority, Actions
   - Create `QueuePatientRow.tsx` for individual patient rows
   - Display patients in chronological order by arrival time (earliest first)
   - Calculate wait time dynamically: current time - arrival time
   - Display priority badge (red "Emergency" badge if flagged)
   - Add "Flag Priority" button (icon button) for non-priority patients
   - Add "Remove Priority" button for priority patients
   - Implement priority flag action - call `/api/staff/queue/{patientId}/priority` PATCH endpoint

5. **Implement Real-Time Queue Updates**
   - Use `usePusherQueue` hook in QueueManagement component
   - Handle "patient-added" event: insert new patient into queue in chronological position
   - Handle "patient-removed" event: remove patient from queue
   - Handle "priority-changed" event: reorder queue (priority patients at top, then chronological)
   - Recalculate wait times for all patients after queue changes
   - Announce changes via ARIA live region (e.g., "New patient added to queue")

6. **Implement Wait Time Calculation Logic**
   - Create `queueHelpers.ts` with `calculateWaitTime(arrivalTime: string): string` function
   - Calculate difference between current time and arrival time in minutes
   - Format as "X min" for < 60 min, "X hr Y min" for >= 60 min
   - Create `recalculateQueueWaitTimes(queue: QueuePatient[]): QueuePatient[]` function
   - Update wait times for all patients when queue order changes

7. **Implement Empty State and Connection Indicator**
   - Display empty state when queue length is 0
   - Show illustration with message "No patients in queue. Book a walk-in?"
   - Add "Book Walk-in" CTA button linking to `/staff/walkin-booking`
   - Display "Live updates paused" warning banner when Pusher connection is disconnected
   - Show reconnection attempt count and retry button

8. **Implement Accessibility and Responsive Layout**
   - Add ARIA live region (`role="status" aria-live="polite"`) for queue updates
   - Announce queue changes to screen readers (e.g., "Queue updated. 3 patients waiting.")
   - Ensure keyboard navigation for priority flag buttons
   - Apply responsive layout matching wireframe at 375px, 768px, 1440px breakpoints
   - Use Tailwind CSS utility classes for styling

9. **Apply Design Tokens and Styling**
   - Reference design tokens from designsystem.md for colors, spacing, typography
   - Use semantic colors for priority badges (error red for emergency)
   - Match wireframe layout, spacing, and component positioning

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
│       └── components/                       # Staff components (walk-in booking components exist)
├── pages/
│   └── staff/
│       └── WalkinBooking.tsx                 # Walk-in booking page (from US_029)
├── hooks/                                    # Custom hooks directory
└── utils/                                    # Utility functions directory
```

## Expected Changes

| Action | File Path                                                      | Description                                                                           |
| ------ | -------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| CREATE | src/frontend/src/pages/staff/QueueManagement.tsx               | Main queue management page with Pusher integration, provider filter, and queue list   |
| CREATE | src/frontend/src/features/staff/components/QueueList.tsx       | Table component displaying queue with headers and patient rows                        |
| CREATE | src/frontend/src/features/staff/components/QueuePatientRow.tsx | Patient row component with priority flag button and wait time display                 |
| CREATE | src/frontend/src/features/staff/components/ProviderFilter.tsx  | Dropdown filter for provider selection (All Providers default)                        |
| CREATE | src/frontend/src/hooks/usePusherQueue.ts                       | Custom hook managing Pusher connection, event subscriptions, and 30s fallback polling |
| CREATE | src/frontend/src/utils/queueHelpers.ts                         | Utility functions for wait time calculation and queue reordering                      |
| MODIFY | src/frontend/src/App.tsx                                       | Add Staff-only route `/staff/queue` with ProtectedRoute wrapper                       |
| MODIFY | src/frontend/src/api/client.ts                                 | Add methods: fetchQueue(), updatePatientPriority(patientId, isPriority)               |

## External References

- Pusher JS Documentation: https://pusher.com/docs/channels/using_channels/client-api/
- Pusher Connection States: https://pusher.com/docs/channels/using_channels/connection/
- React Custom Hooks: https://react.dev/learn/reusing-logic-with-custom-hooks
- ARIA Live Regions: https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions
- Date Formatting (date-fns): https://date-fns.org/v3.0.0/docs/formatDistance

## Build Commands

- `npm install pusher-js` - Install Pusher client library
- `npm install date-fns` - Install date utility library
- `npm run dev` - Start development server on http://localhost:5173
- `npm run build` - Build for production
- `npm run test` - Run Vitest unit tests
- `npm run lint` - Run ESLint

## Implementation Validation Strategy

- [ ] Unit tests pass for QueueList, QueuePatientRow, queueHelpers utility functions
- [ ] Integration tests pass for Pusher event handling (patient-added, priority-changed)
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] RBAC enforcement verified - non-Staff users cannot access /staff/queue route
- [ ] Pusher connection established successfully and events received
- [ ] Fallback polling activates when Pusher connection drops (30s interval)
- [ ] "Live updates paused" indicator displays when using fallback
- [ ] Priority flag action updates queue order (priority patients at top)
- [ ] Wait times recalculate correctly after queue changes
- [ ] ARIA live region announces queue updates to screen readers
- [ ] Empty state displays with CTA when queue is empty
- [ ] Provider filter correctly filters queue by selected provider

## Implementation Checklist

- [ ] Install pusher-js and date-fns packages
- [ ] Create usePusherQueue.ts custom hook with Pusher client initialization
- [ ] Subscribe to "queue-updates" channel in usePusherQueue
- [ ] Bind to "patient-added", "patient-removed", "priority-changed" events
- [ ] Implement connection state tracking (connected, connecting, disconnected)
- [ ] Implement 30-second fallback polling using setInterval when disconnected
- [ ] Display "Live updates paused" indicator in UI when using fallback
- [ ] Create QueueManagement.tsx page component
- [ ] Add Staff-only route `/staff/queue` in App.tsx with role guard
- [ ] Fetch initial queue data from `/api/staff/queue` on mount
- [ ] Display loading skeleton while fetching data
- [ ] Create ProviderFilter.tsx dropdown component with "All Providers" default
- [ ] Fetch provider list from `/api/providers` and populate dropdown
- [ ] Implement filter logic to filter queue by selected provider
- [ ] Create QueueList.tsx table component with headers
- [ ] Create QueuePatientRow.tsx with patient data display (name, type, provider, arrival, wait time)
- [ ] Display priority badge (red "Emergency") if patient is flagged
- [ ] Add "Flag Priority" button (icon) for non-priority patients
- [ ] Add "Remove Priority" button for priority patients
- [ ] Implement priority flag action - PATCH `/api/staff/queue/{patientId}/priority`
- [ ] Handle "patient-added" Pusher event - insert patient in chronological order
- [ ] Handle "patient-removed" Pusher event - remove patient from queue
- [ ] Handle "priority-changed" Pusher event - reorder queue (priority first)
- [ ] Create calculateWaitTime utility function (format: "X min" or "X hr Y min")
- [ ] Create recalculateQueueWaitTimes utility function
- [ ] Update wait times for all patients after queue changes
- [ ] Add ARIA live region with role="status" aria-live="polite"
- [ ] Announce queue updates via ARIA live region (e.g., "Queue updated. 3 patients waiting.")
- [ ] Implement empty state with illustration and "No patients in queue" message
- [ ] Add "Book Walk-in" CTA button linking to /staff/walkin-booking
- [ ] Apply design tokens from designsystem.md (colors, spacing, typography)
- [ ] Implement responsive layout matching wireframe at 375px, 768px, 1440px
- [ ] Test Pusher connection and event handling in local dev environment
- [ ] Test fallback polling by disconnecting Pusher manually
- [ ] Verify ARIA live region announces queue changes to screen readers
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
