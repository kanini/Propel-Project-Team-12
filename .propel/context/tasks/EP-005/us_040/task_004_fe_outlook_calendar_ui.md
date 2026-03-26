# Task - task_004_fe_outlook_calendar_ui

## Requirement Reference

- User Story: us_040
- Story Location: .propel/context/tasks/EP-005/us_040/us_040.md
- Acceptance Criteria:
  - AC-4: Patients can connect/disconnect Outlook Calendar from the application UI
- Edge Cases:
  - Both Google and Outlook connected: Events are created in both calendars independently (EC-2)

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | Hi-Fi HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-008-appointment-confirmation.html |
| **Screen Spec** | SCR-008 |
| **UXR Requirements** | UXR-501: Action feedback within 200ms |
| **Design Tokens** | --color-primary-500: #0F62FE, --color-success: #16A34A, --color-success-light: #DCFCE7, --color-neutral-300: #CBD5E1, --color-neutral-500: #64748B, --color-neutral-700: #334155, --radius-md: 8px |

## Applicable Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Language | TypeScript | 5.x |
| State | Redux Toolkit | 2.x |
| Styling | Tailwind CSS | 3.x |
| Build | Vite | 5.x |

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

Replace the static Outlook Calendar URL link in `ConfirmationDialog.tsx` with an OAuth-aware connection flow that uses the multi-provider `CalendarController` API (from US_040/task_003). Extends the `calendarApi.ts` module (from US_039/task_004) to support a provider parameter for both Google and Outlook. Shows independent connection status badges for each provider (EC-2). Handles Microsoft OAuth return via URL parameter detection. Adds `outlookCalendarSynced` optional field to the frontend Appointment TypeScript interface.

## Dependent Tasks

- EP-005/us_039/task_004_fe_google_calendar_ui — Provides calendarApi.ts (getStatus/getConnectUrl/disconnect), ConfirmationDialog OAuth-aware Google flow, OAuth return URL parameter detection
- EP-005/us_040/task_003_be_multi_provider_sync — Provides multi-provider CalendarController with {provider} route parameter and multi-provider status endpoint

## Impacted Components

- **MODIFY** `src/frontend/src/api/calendarApi.ts` — Update to accept provider parameter, add Outlook-specific endpoints
- **MODIFY** `src/frontend/src/components/appointments/ConfirmationDialog.tsx` — Replace static Outlook URL with OAuth-aware connect/disconnect flow; show independent Google and Outlook badges
- **MODIFY** `src/frontend/src/types/appointment.ts` — Add outlookCalendarSynced optional field to Appointment interface

## Implementation Plan

1. **Extend calendarApi.ts with provider parameter**:
   ```typescript
   // Update existing functions from US_039/task_004 to accept provider
   export const calendarApi = {
     getStatus: async (): Promise<CalendarStatusResponse> => {
       // GET /api/calendar/status — returns { google: { isConnected }, outlook: { isConnected } }
       const response = await api.get('/api/calendar/status');
       return response.data;
     },

     getConnectUrl: async (provider: 'google' | 'outlook'): Promise<string> => {
       // GET /api/calendar/{provider}/connect
       const response = await api.get(`/api/calendar/${provider}/connect`);
       return response.data.url;
     },

     disconnect: async (provider: 'google' | 'outlook'): Promise<void> => {
       // POST /api/calendar/{provider}/disconnect
       await api.post(`/api/calendar/${provider}/disconnect`);
     },
   };

   export interface CalendarStatusResponse {
     google: { isConnected: boolean };
     outlook: { isConnected: boolean };
   }
   ```
   - Refactor existing Google-only functions from US_039/task_004 to multi-provider
   - Status endpoint returns all providers at once (single API call for both)

2. **Update Appointment TypeScript interface**:
   ```typescript
   // In src/frontend/src/types/appointment.ts
   export interface Appointment {
     // ... existing fields
     googleCalendarSynced?: boolean;   // Added by US_039/task_004
     outlookCalendarSynced?: boolean;  // NEW
   }
   ```

3. **Replace static Outlook link with OAuth-aware button in ConfirmationDialog.tsx**:
   - Remove `getOutlookCalendarUrl()` static URL function
   - Add Outlook connection state management:
     ```typescript
     const [outlookConnected, setOutlookConnected] = useState<boolean>(false);
     const [outlookConnecting, setOutlookConnecting] = useState<boolean>(false);
     ```
   - Fetch connection status on mount (single API call returns both providers):
     ```typescript
     useEffect(() => {
       const fetchStatus = async () => {
         const status = await calendarApi.getStatus();
         setGoogleConnected(status.google.isConnected);
         setOutlookConnected(status.outlook.isConnected);
       };
       fetchStatus();
     }, []);
     ```
   - Handle OAuth return (check URL for `?provider=outlook&calendar_connected=true`):
     ```typescript
     useEffect(() => {
       const params = new URLSearchParams(window.location.search);
       if (params.get('provider') === 'outlook' && params.get('calendar_connected') === 'true') {
         setOutlookConnected(true);
       }
     }, []);
     ```

4. **Render Outlook calendar button with three states**:
   ```tsx
   {/* State: Connected — show synced badge */}
   {outlookConnected ? (
     <div className="inline-flex items-center justify-center gap-2 h-11 px-4
                     bg-success-light border border-success rounded-lg text-sm
                     font-medium text-success-dark">
       <svg className="w-5 h-5" ...>✓</svg>
       Synced to Outlook
     </div>
   ) : outlookConnecting ? (
     /* State: Connecting — show loading spinner */
     <button disabled className="... opacity-50 cursor-not-allowed">
       <span className="animate-spin ...">⟳</span>
       Connecting...
     </button>
   ) : (
     /* State: Not connected — show connect button */
     <button
       onClick={handleConnectOutlook}
       className="inline-flex items-center justify-center gap-2 h-11 px-4
                  border border-neutral-300 rounded-lg text-sm font-medium
                  text-neutral-700 bg-neutral-0 hover:bg-neutral-50
                  focus:outline-none focus:ring-2 focus:ring-primary-500
                  focus:ring-offset-2 transition-colors"
     >
       <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
         {/* Outlook icon SVG */}
       </svg>
       Connect Outlook
     </button>
   )}
   ```
   - Uses design tokens: `--color-success-light` (#DCFCE7), `--color-success` (#16A34A)
   - Maintains the same button height (h-11) and styling as the existing Google button
   - UXR-501: Loading state shown within 200ms via `outlookConnecting` state

5. **Handle Connect Outlook action**:
   ```typescript
   const handleConnectOutlook = async () => {
     setOutlookConnecting(true);
     try {
       const url = await calendarApi.getConnectUrl('outlook');
       window.location.href = url; // Redirect to Microsoft OAuth
     } catch {
       setOutlookConnecting(false);
     }
   };
   ```
   - Redirects to Microsoft OAuth consent screen via backend connect URL
   - On OAuth completion, user returns to ConfirmationDialog with `?provider=outlook&calendar_connected=true`

6. **Handle Disconnect Outlook action (optional disconnect button)**:
   ```typescript
   const handleDisconnectOutlook = async () => {
     await calendarApi.disconnect('outlook');
     setOutlookConnected(false);
   };
   ```

7. **Independent Google and Outlook badges (EC-2)**:
   - Both calendar buttons render independently in the grid layout
   - Google button shows "Synced to Google" or "Connect Google" (from US_039/task_004)
   - Outlook button shows "Synced to Outlook" or "Connect Outlook" (this task)
   - Both can be connected simultaneously — each operates independently
   - Layout: `grid grid-cols-2 gap-3` (unchanged from current wireframe)

## Current Project State

```
src/frontend/src/
├── api/
│   └── calendarApi.ts                # FROM US_039/task_004 — Google-only, needs provider param
├── components/
│   └── appointments/
│       ├── ConfirmationDialog.tsx     # Current: static getOutlookCalendarUrl()
│       └── BookingSteps.tsx           # Imports ConfirmationDialog
├── types/
│   └── appointment.ts                # No outlookCalendarSynced field yet
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/frontend/src/api/calendarApi.ts | Add provider parameter to getConnectUrl and disconnect; update CalendarStatusResponse |
| MODIFY | src/frontend/src/components/appointments/ConfirmationDialog.tsx | Replace static Outlook URL with OAuth-aware connect/disconnect flow and synced badge |
| MODIFY | src/frontend/src/types/appointment.ts | Add outlookCalendarSynced optional field to Appointment interface |

## External References

- Microsoft OAuth 2.0 Consent Flow: https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow
- SCR-008 Wireframe: .propel/context/wireframes/Hi-Fi/wireframe-SCR-008-appointment-confirmation.html

## Build Commands

```bash
cd src/frontend
npm run build
npm run test
```

## Implementation Validation Strategy

- [ ] Static `getOutlookCalendarUrl()` function is removed from ConfirmationDialog
- [ ] "Connect Outlook" button triggers Microsoft OAuth redirect via `/api/calendar/outlook/connect`
- [ ] "Synced to Outlook" badge shown when Outlook is connected (green success styling)
- [ ] Loading spinner shown during OAuth redirect (UXR-501: within 200ms)
- [ ] OAuth return detected via URL parameter (`?provider=outlook&calendar_connected=true`)
- [ ] Both Google and Outlook can be connected simultaneously and shown independently (EC-2)
- [ ] `outlookCalendarSynced` field added to Appointment TypeScript interface

## Implementation Checklist

- [x] Update `calendarApi.ts` to accept provider parameter on `getConnectUrl` and `disconnect`
- [x] Update `CalendarStatusResponse` to include both `google` and `outlook` status
- [x] Add `outlookCalendarSynced` optional field to the Appointment TypeScript interface
- [x] Replace `getOutlookCalendarUrl()` static function with OAuth-aware `handleConnectOutlookCalendar`
- [x] Add `outlookConnected` and `connectingProvider` state to ConfirmationDialog
- [x] Render Outlook button with three states: not-connected, connecting (loading), synced
- [x] Handle Microsoft OAuth return via URL parameter detection on component mount
