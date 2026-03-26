# Task - task_004_fe_google_calendar_ui

## Requirement Reference

- User Story: us_039
- Story Location: .propel/context/tasks/EP-005/us_039/us_039.md
- Acceptance Criteria:
  - AC-4: "Add to Google Calendar" button available on appointment confirmation that initiates the OAuth2 flow when not connected
- Edge Cases:
  - OAuth token expiry: If backend returns disconnected status, show "Reconnect Google Calendar" button prompting re-authorization (EC-2)

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-008-appointment-confirmation.html |
| **Screen Spec** | figma_spec.md#SCR-008 |
| **UXR Requirements** | UXR-001, UXR-301, UXR-501 |
| **Design Tokens** | designsystem.md#colors, designsystem.md#typography |

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

Enhance the appointment confirmation screen (SCR-008) with Google Calendar OAuth2 integration. The existing `ConfirmationDialog` component currently generates a static Google Calendar URL that opens a new tab. This task replaces that static link with a dynamic flow: if the user has already connected Google Calendar (checked via `/api/calendar/status`), the booking automatically triggers a backend sync and the button shows "Synced to Google Calendar". If the user has not connected, the "Add to Google Calendar" button initiates the OAuth2 flow by redirecting to the backend `/api/calendar/connect` endpoint, which redirects to Google's consent screen. After successful authorization, the callback redirects back to the confirmation page and the calendar event is created automatically. A new `calendarApi.ts` module provides API functions, and calendar connection state is managed in the existing `appointmentSlice` or a dedicated state.

## Dependent Tasks

- EP-005/us_039/task_002_be_google_calendar_service — Provides /api/calendar/* endpoints (connect, callback, status, disconnect)
- EP-005/us_039/task_003_be_calendar_sync_integration — Ensures calendar events are created on booking

## Impacted Components

- **NEW** `src/frontend/src/api/calendarApi.ts` — API functions for calendar status, connect, disconnect
- **MODIFY** `src/frontend/src/components/appointments/ConfirmationDialog.tsx` — Replace static Google Calendar URL with OAuth-aware flow
- **MODIFY** `src/frontend/src/types/appointment.ts` — Add `googleCalendarSynced` optional field to Appointment interface

## Implementation Plan

1. **Create calendarApi.ts module**:
   ```typescript
   // src/frontend/src/api/calendarApi.ts
   import axios from 'axios'; // or use existing API client pattern

   const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5173/api';

   export const calendarApi = {
       getStatus: async (): Promise<{ isConnected: boolean; provider: string }> => {
           const response = await axios.get(`${API_BASE}/calendar/status`);
           return response.data;
       },

       getConnectUrl: async (): Promise<{ authorizationUrl: string }> => {
           const response = await axios.get(`${API_BASE}/calendar/connect`);
           return response.data;
       },

       disconnect: async (): Promise<void> => {
           await axios.post(`${API_BASE}/calendar/disconnect`);
       }
   };
   ```
   - Follows existing API module pattern (see `providerApi.ts`, `staffApi.ts`)
   - Uses the same base URL and authentication token patterns as existing API modules

2. **Add googleCalendarSynced to Appointment interface**:
   ```typescript
   // In types/appointment.ts Appointment interface
   googleCalendarSynced?: boolean; // True when event is synced to Google Calendar
   ```
   - Optional field — existing appointments won't have this

3. **Update ConfirmationDialog — check connection status on mount**:
   ```typescript
   const [calendarConnected, setCalendarConnected] = useState<boolean | null>(null);
   const [calendarLoading, setCalendarLoading] = useState(true);

   useEffect(() => {
       const checkCalendarStatus = async () => {
           try {
               const status = await calendarApi.getStatus();
               setCalendarConnected(status.isConnected);
           } catch {
               setCalendarConnected(false); // Default to disconnected on error
           } finally {
               setCalendarLoading(false);
           }
       };
       checkCalendarStatus();
   }, []);
   ```
   - Checks connection status on component mount to determine button behavior
   - Loading state prevents premature button rendering (UXR-501)

4. **Update ConfirmationDialog — conditional button rendering**:
   - **Connected state**: Show "Synced to Google Calendar" with green checkmark icon (event auto-created by backend job)
   - **Disconnected state**: Show "Connect Google Calendar" button that triggers OAuth flow
   - **Loading state**: Show skeleton/spinner placeholder for button area
   ```tsx
   {calendarLoading ? (
       <div className="h-11 bg-neutral-100 animate-pulse rounded-lg" />
   ) : calendarConnected ? (
       <div className="inline-flex items-center justify-center gap-2 h-11 px-4
                       text-sm font-medium text-success bg-success/10 rounded-lg">
           <svg className="w-5 h-5" ... /> {/* Checkmark icon */}
           Synced to Google Calendar
       </div>
   ) : (
       <button
           onClick={handleConnectGoogleCalendar}
           className="inline-flex items-center justify-center gap-2 h-11 px-4
                      border border-neutral-300 rounded-lg text-sm font-medium
                      text-neutral-700 bg-neutral-0 hover:bg-neutral-50
                      focus:outline-none focus:ring-2 focus:ring-primary-500
                      focus:ring-offset-2 transition-colors"
       >
           <svg className="w-5 h-5" ... /> {/* Google Calendar icon */}
           Connect Google Calendar
       </button>
   )}
   ```
   - Matches existing button styles from ConfirmationDialog (border, height, font)

5. **Implement OAuth flow trigger**:
   ```typescript
   const handleConnectGoogleCalendar = async () => {
       try {
           const { authorizationUrl } = await calendarApi.getConnectUrl();
           // Redirect to Google OAuth consent in same window
           // After consent, Google redirects to /api/calendar/callback
           // which redirects back to the confirmation page
           window.location.href = authorizationUrl;
       } catch (error) {
           // Show error toast/message
           console.error('Failed to initiate Google Calendar connection', error);
       }
   };
   ```
   - Uses window redirect (not popup) for OAuth — more reliable across browsers and popup blockers
   - Backend callback redirects back to the frontend confirmation page after token exchange

6. **Preserve static Google Calendar fallback for Outlook**:
   - Keep the existing `getOutlookCalendarUrl()` function and Outlook button unchanged
   - The Outlook button remains static URL-based until US_040 implements its own OAuth flow
   - Google button is now OAuth-aware; Outlook button stays as-is

7. **Handle OAuth return — check URL params**:
   ```typescript
   useEffect(() => {
       const urlParams = new URLSearchParams(window.location.search);
       if (urlParams.get('calendar_connected') === 'true') {
           setCalendarConnected(true);
           // Clean up URL params
           window.history.replaceState({}, '', window.location.pathname);
       }
   }, []);
   ```
   - Backend callback appends `?calendar_connected=true` to redirect URL after successful token exchange
   - Frontend detects this parameter and updates state without additional API call

## Current Project State

```
src/frontend/src/
├── api/
│   ├── providerApi.ts                   # EXISTS — API module pattern reference
│   ├── staffApi.ts                      # EXISTS — API module pattern reference
│   └── (no calendarApi.ts)
├── components/
│   └── appointments/
│       └── ConfirmationDialog.tsx        # EXISTS — has static getGoogleCalendarUrl() and getOutlookCalendarUrl()
├── types/
│   └── appointment.ts                   # EXISTS — Appointment interface without googleCalendarSynced
└── store/
    └── slices/
        └── appointmentSlice.ts          # EXISTS — appointment state management
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/api/calendarApi.ts | API functions for calendar status check, connect URL, disconnect |
| MODIFY | src/frontend/src/components/appointments/ConfirmationDialog.tsx | Replace static Google Calendar link with OAuth-aware conditional rendering |
| MODIFY | src/frontend/src/types/appointment.ts | Add optional googleCalendarSynced field to Appointment interface |

## External References

- Google OAuth2 Web Flow UX: https://developers.google.com/identity/protocols/oauth2/web-server#redirecting
- React useEffect for side effects: https://react.dev/reference/react/useEffect
- Tailwind CSS Transitions: https://tailwindcss.com/docs/transition-property

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
- [x] calendarApi.ts exports getStatus, getConnectUrl, disconnect functions
- [x] ConfirmationDialog checks calendar connection status on mount
- [x] Connected users see "Synced to Google Calendar" with checkmark
- [x] Disconnected users see "Connect Google Calendar" button that triggers OAuth redirect
- [x] OAuth return handled via URL parameter detection
- [x] Outlook button remains unchanged (static URL)
- [x] Loading state shown during calendar status check (UXR-501)

## Implementation Checklist

- [x] Create `calendarApi.ts` with getStatus, getConnectUrl, disconnect API functions
- [x] Add `googleCalendarSynced` optional field to `Appointment` TypeScript interface
- [x] Add calendar connection status check on `ConfirmationDialog` mount with loading state
- [x] Implement conditional rendering: connected (synced badge), disconnected (connect button), loading (skeleton)
- [x] Implement OAuth flow trigger via `handleConnectGoogleCalendar` using window redirect
- [x] Handle OAuth return by detecting `calendar_connected` URL parameter
- [x] Preserve existing Outlook Calendar static URL button unchanged
- [x] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [x] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
