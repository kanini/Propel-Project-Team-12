# Task - task_002_fe_toast_notification_system_error_handling

## Requirement Reference
- User Story: US_066 - Error Handling Patterns
- Story Location: .propel/context/tasks/EP-011-II/us_066/us_066.md
- Acceptance Criteria:
    - AC-2: API/system errors display toast notification with user-friendly message, error code for support reference, and retry action when applicable — no raw error codes or stack traces shown
    - AC-3: 404 and empty states show friendly illustration with clear message and actionable link (e.g., "Go back to dashboard" or "Create your first appointment")
    - AC-4: Network errors display persistent banner announcing "You're offline — changes will sync when you reconnect" and app queues actions for retry
- Edge Case:
    - Multiple validation errors simultaneously (handled by task_001)
    - Errors in modals (handled by task_001)

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A (cross-cutting pattern affecting all screens) |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all wireframes show error states), reference existing toast in WalkinBooking.tsx |
| **Screen Spec** | .propel/context/docs/figma_spec.md#UXR-603 (API error banner with retry) |
| **UXR Requirements** | UXR-603 (Global error banner for API/system failures with retry action) |
| **Design Tokens** | .propel/context/docs/designsystem.md#toast (position, duration, variants, animation), designsystem.md#empty-state (layout, illustration) |

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
| Frontend | Tailwind CSS | Latest |
| Library | React Router | v6 |

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
Create centralized toast notification system, API error handling utilities, 404/empty state components, and network offline detection to meet UXR-603 (user-friendly error messaging). This task standardizes error presentation across the application by creating a global toast manager with Redux state, API error interceptor utilities, 404 page component, and online/offline event detection with action queue for retry.

## Dependent Tasks
- None (establishes foundational error handling infrastructure)

## Impacted Components
- **CREATE** `src/frontend/src/components/common/Toast.tsx` - Reusable toast component with success/error/warning/info variants
- **CREATE** `src/frontend/src/components/common/ToastContainer.tsx` - Global toast manager rendering up to 3 toasts with stacking
- **CREATE** `src/frontend/src/components/common/OfflineBanner.tsx` - Persistent banner for network offline state
- **CREATE** `src/frontend/src/components/common/NotFoundPage.tsx` - 404 error page with illustration and navigation links
- **CREATE** `src/frontend/src/store/slices/toastSlice.ts` - Redux slice for global toast state management
- **CREATE** `src/frontend/src/hooks/useToast.ts` - Custom hook for showing toasts from any component
- **CREATE** `src/frontend/src/hooks/useOnlineStatus.ts` - Custom hook for detecting online/offline network status
- **CREATE** `src/frontend/src/utils/apiErrorHandler.ts` - Utility to transform API errors to user-friendly messages with error codes
- **CREATE** `src/frontend/src/utils/offlineQueue.ts` - Action queue for retrying failed requests when back online
- **MODIFY** `src/frontend/src/App.tsx` - Add ToastContainer and OfflineBanner at root level
- **MODIFY** `src/frontend/src/api/documentsApi.ts` - Use apiErrorHandler for error transformation (example)
- **MODIFY** `src/frontend/src/api/dashboardApi.ts` - Use apiErrorHandler for error transformation (example)
- **MODIFY** `src/frontend/src/features/staff/pages/WalkinBooking.tsx` - Replace local toast state with useToast hook

## Implementation Plan

1. **Create Toast Redux Slice**
   - State: `toasts: Array<{ id: string, type: 'success' | 'error' | 'warning' | 'info', message: string, duration: number, retryAction?: Function }>`
   - Actions: `addToast(toast)`, `removeToast(id)`, `clearAllToasts()`
   - addToast generates unique ID (Date.now() + Math.random())
   - Auto-removal: Dispatch removeToast after `duration` ms using setTimeout in component
   - Max 3 visible toasts (trim oldest when adding 4th)

2. **Create Toast Component**
   - Props: `id`, `type`, `message`, `onClose`, `retryAction?`, `duration`
   - Visual variants based on type:
     - Success: border-success, bg-success-50, CheckCircle icon
     - Error: border-error, bg-error-50, XCircle icon
     - Warning: border-warning, bg-warning-50, AlertTriangle icon
     - Info: border-info, bg-info-50, Info icon
   - Layout: Flex with icon (left), message (center), close button (right)
   - Retry button: If `retryAction` provided, show "Retry" button between message and close
   - Animation: Slide-in from right (animate-slide-in), fade-out on close (300ms)
   - ARIA: role="alert", aria-live="polite"
   - Reference designsystem.md: toast position (top-right desktop, top-center mobile), 360px max width, 5s duration

3. **Create ToastContainer Component**
   - Connect to Redux: `useSelector(selectToasts)` from toastSlice
   - Render: Fixed position top-right (desktop) or top-center (mobile), z-index 1000
   - Stack toasts with 12px gap (designsystem.md)
   - Auto-dismiss: useEffect per toast to dispatch removeToast after duration
   - Max 3 visible: Slice toasts array to show only last 3

4. **Create useToast Hook**
   - Signature: `useToast()`
   - Return: `{ showSuccess, showError, showWarning, showInfo }`
   - Each function: `(message: string, duration?: number, retryAction?: Function) => void`
   - Internally dispatches `addToast` action to Redux store
   - Default duration: 5000ms (per designsystem.md)
   - Example usage: `const { showError } = useToast(); showError("Failed to load data", 5000, retryFunction);`

5. **Create apiErrorHandler Utility**
   - Function signature: `handleApiError(error: unknown, context: string): { userMessage: string, errorCode: string }`
   - Transform API errors:
     - 400 Bad Request → "Invalid request. Please check your input." + ERR_BAD_REQUEST
     - 401 Unauthorized → "Your session has expired. Please log in again." + ERR_UNAUTHORIZED
     - 403 Forbidden → "You don't have permission to perform this action." + ERR_FORBIDDEN
     - 404 Not Found → "The requested resource was not found." + ERR_NOT_FOUND
     - 500 Server Error → "Something went wrong on our end. Please try again later." + ERR_SERVER_ERROR
     - Network Error → "Unable to connect. Please check your internet connection." + ERR_NETWORK
   - Error code format: `ERR_<STATUS>_<CONTEXT>` (e.g., ERR_500_APPOINTMENT_FETCH)
   - Hide raw error messages and stack traces
   - Return: `{ userMessage, errorCode }` for toast display

6. **Create OfflineBanner Component**
   - Fixed position top of viewport (below header if present), full width, z-index 999
   - Background: warning-light (amber), border-bottom: warning
   - Content: WiFi-off icon + "You're offline — changes will sync when you reconnect"
   - Only render when `isOnline === false`
   - ARIA: role="alert", aria-live="assertive"

7. **Create useOnlineStatus Hook**
   - Listen to `online` and `offline` events on window
   - State: `isOnline: boolean` (initialize with `navigator.onLine`)
   - useEffect: Add event listeners for online/offline, cleanup on unmount
   - Return: `{ isOnline }`
   - Integration with OfflineBanner: `const { isOnline } = useOnlineStatus();`

8. **Create offlineQueue Utility**
   - Queue: Array of pending actions `{ id: string, action: Function, timestamp: number }`
   - Functions:
     - `enqueueAction(action: Function): void` - Add action to queue
     - `processQueue(): Promise<void>` - Execute all queued actions sequentially
     - `clearQueue(): void` - Clear all queued actions
   - Storage: Use localStorage for persistence across page reloads
   - Integration: When `isOnline` transitions from false to true, call processQueue()

9. **Enhance EmptyState Component (EXTEND EXISTING)**
   - Current: `src/frontend/src/components/common/EmptyState.tsx` (already exists for AC-3)
   - Add additional variants for different empty states:
     - No appointments: "No appointments yet. Browse providers to schedule your first visit."
     - No documents: "No documents uploaded. Upload your first clinical document."
     - No notifications: "All caught up! No new notifications."
   - Props already support: `title`, `message`, `showClearButton`, `onClearFilters`, `icon`
   - Keep existing implementation, just add more usage examples in documentation

10. **Create NotFoundPage Component (404)**
    - Route: `/404` or catch-all route `*`
    - Layout: Centered EmptyState with 404 illustration
    - Message: "Page Not Found. The page you're looking for doesn't exist or has been moved."
    - CTA buttons: "Go to Dashboard" (primary), "Go Back" (secondary)
    - Reference designsystem.md: empty_state layout (centered, max 200px illustration, h3 heading, body description, CTA button)

11. **Integrate ToastContainer in App.tsx**
    - Import ToastContainer and OfflineBanner
    - Render at root level (after Router, before Routes):
      ```tsx
      <Router>
        <ToastContainer />
        <OfflineBanner />
        <Routes>...</Routes>
      </Router>
      ```

12. **Refactor WalkinBooking.tsx to Use useToast**
    - Remove local toast state: `const [toast, setToast] = useState(...)`
    - Replace with: `const { showSuccess, showError } = useToast();`
    - Replace `setToast({ type: 'success', message: '...' })` with `showSuccess('...')`
    - Remove local Toast rendering JSX (now handled by global ToastContainer)

13. **Refactor documentsApi.ts and dashboardApi.ts**
    - Wrap all catch blocks with `apiErrorHandler`
    - Example:
      ```typescript
      catch (error) {
        const { userMessage, errorCode } = handleApiError(error, 'DOCUMENT_UPLOAD');
        throw new Error(`${userMessage} (${errorCode})`);
      }
      ```
    - Component will receive formatted error message to display in toast

14. **Add Offline Event Handling**
    - In components with form submissions (DocumentUpload, BookingWizard):
    - Check `isOnline` before API calls
    - If offline: Show toast "You're offline. Your changes will be saved and synced when you reconnect."
    - Enqueue action: `enqueueAction(() => submitForm(data))`
    - When back online: Process queue and show success/error toasts

15. **Add Unit Tests**
    - Test: Toast displays with correct type (success/error/warning/info) and auto-dismisses after duration
    - Test: Max 3 toasts visible, oldest removed when 4th added
    - Test: useToast hook dispatches addToast action correctly
    - Test: apiErrorHandler transforms error codes to user-friendly messages (no stack traces)
    - Test: OfflineBanner appears when offline, hides when online
    - Test: offlineQueue enqueues actions and processes them on reconnect
    - Test: 404 page renders with "Page Not Found" message and navigation links

## Current Project State
```
src/frontend/
├── src/
│   ├── components/
│   │   └── common/
│   │       ├── EmptyState.tsx (EXISTS - for AC-3)
│   │       └── [Toast.tsx, ToastContainer.tsx, OfflineBanner.tsx, NotFoundPage.tsx TO BE CREATED]
│   ├── features/
│   │   └── staff/
│   │       └── pages/
│   │           └── WalkinBooking.tsx (HAS LOCAL TOAST - TO REFACTOR)
│   ├── hooks/
│   │   └── [useToast.ts, useOnlineStatus.ts TO BE CREATED]
│   ├── store/
│   │   └── slices/
│   │       └── [toastSlice.ts TO BE CREATED]
│   ├── utils/
│   │   └── [apiErrorHandler.ts, offlineQueue.ts TO BE CREATED]
│   ├── api/
│   │   ├── documentsApi.ts (TO ENHANCE WITH ERROR HANDLING)
│   │   └── dashboardApi.ts (TO ENHANCE WITH ERROR HANDLING)
│   └── App.tsx (TO ADD ToastContainer + OfflineBanner)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/common/Toast.tsx | Reusable toast component with 4 variants (success/error/warning/info), icons, retry button |
| CREATE | src/frontend/src/components/common/ToastContainer.tsx | Global toast manager with Redux connection, max 3 visible, auto-dismiss |
| CREATE | src/frontend/src/components/common/OfflineBanner.tsx | Persistent offline banner with "You're offline" message |
| CREATE | src/frontend/src/components/common/NotFoundPage.tsx | 404 error page with EmptyState, illustration, and navigation links |
| CREATE | src/frontend/src/store/slices/toastSlice.ts | Redux slice for toast state with addToast, removeToast, clearAllToasts actions |
| CREATE | src/frontend/src/hooks/useToast.ts | Custom hook providing showSuccess, showError, showWarning, showInfo functions |
| CREATE | src/frontend/src/hooks/useOnlineStatus.ts | Custom hook detecting online/offline network status with window event listeners |
| CREATE | src/frontend/src/utils/apiErrorHandler.ts | API error transformer mapping status codes to user-friendly messages with error codes |
| CREATE | src/frontend/src/utils/offlineQueue.ts | Action queue for retrying failed requests when reconnected (localStorage persistence) |
| MODIFY | src/frontend/src/App.tsx | Add ToastContainer and OfflineBanner at root level (after Router) |
| MODIFY | src/frontend/src/features/staff/pages/WalkinBooking.tsx | Replace local toast state with useToast hook (lines 23-75, 213-268) |
| MODIFY | src/frontend/src/api/documentsApi.ts | Wrap errors with apiErrorHandler utility (catch blocks) |
| MODIFY | src/frontend/src/api/dashboardApi.ts | Wrap errors with apiErrorHandler utility (catch blocks) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [ARIA Alert Role](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Roles/alert_role) - Toast and offline banner accessibility
- [Navigator.onLine API](https://developer.mozilla.org/en-US/docs/Web/API/Navigator/onLine) - Network status detection
- [Online and Offline Events](https://developer.mozilla.org/en-US/docs/Web/API/Navigator/onLine#example) - Window event listeners
- [React Portal for Toasts](https://react.dev/reference/react-dom/createPortal) - Rendering toasts outside React root
- [HTTP Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) - Error code reference for apiErrorHandler
- [NN Group: Error Codes](https://www.nngroup.com/articles/error-message-guidelines/) - User-friendly error messaging

## Build Commands
```bash
# Development
cd src/frontend
npm run dev

# Type checking
npm run type-check

# Linting
npm run lint

# Unit tests
npm run test

# Build production
npm run build
```

## Implementation Validation Strategy
- [x] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [x] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] **[AI Tasks]** Prompt templates validated with test inputs
- [ ] **[AI Tasks]** Guardrails tested for input sanitization and output validation
- [ ] **[AI Tasks]** Fallback logic tested with low-confidence/error scenarios
- [ ] **[AI Tasks]** Token budget enforcement verified
- [ ] **[AI Tasks]** Audit logging verified (no PII in logs)
- [ ] **[Mobile Tasks]** Headless platform compilation succeeds
- [ ] **[Mobile Tasks]** Native dependency linking verified
- [ ] **[Mobile Tasks]** Permission manifests validated against task requirements

### Custom Validation Criteria
- [ ] Toast auto-dismisses after 5 seconds (configurable duration)
- [ ] Max 3 toasts visible simultaneously (oldest removed when 4th added)
- [ ] Toast displays user-friendly error message with error code (e.g., "Unable to connect. (ERR_NETWORK_DOCUMENT_UPLOAD)")
- [ ] No raw error codes or stack traces shown in toasts
- [ ] OfflineBanner appears immediately when network disconnects
- [ ] OfflineBanner hides immediately when network reconnects
- [ ] Queued actions retry automatically when back online
- [ ] 404 page displays with illustration and navigation links
- [ ] EmptyState components display with friendly illustrations and CTAs across all empty scenarios
- [ ] Screen reader announces toast messages via role="alert" and aria-live
- [ ] Toast retry button (if present) executes provided retry action

## Technical Context

### Architecture Patterns
- **Global Toast Management**: Redux-based toast state ensures toasts persist across component unmounts
- **Error Transformation Layer**: apiErrorHandler abstracts error code logic from components
- **Offline-First Pattern**: Action queue with localStorage persistence enables retry after reconnect
- **Portal Rendering**: ToastContainer uses React Portal for rendering outside component hierarchy

### Related Requirements
- UXR-603: Global error banner for API/system failures with retry action
- UXR-605: Empty states with illustrations and CTAs (EmptyState component already exists)
- NFR-011: API error rate below 0.1% (error handling improves error visibility for debugging)

### Edge Case Handling
- **Multiple concurrent toasts**: Limit to 3 visible, queue overflow toasts in Redux state
- **Toast during navigation**: Toasts persist across route changes (global state)
- **Offline during form submission**: Queue action, show offline toast, retry on reconnect
- **Error during offline queue processing**: Show individual error toasts, don't clear queue item

## Implementation Checklist
- [ ] Create toastSlice with addToast, removeToast, clearAllToasts actions and max 3 toast state
- [ ] Create Toast component with 4 variants (success/error/warning/info), icons, retry button, auto-dismiss
- [ ] Create ToastContainer with Redux connection, fixed position (top-right/top-center), z-index 1000
- [ ] Create useToast hook with showSuccess, showError, showWarning, showInfo functions
- [ ] Create apiErrorHandler utility mapping status codes to user-friendly messages with error codes
- [ ] Create OfflineBanner component with "You're offline" message and warning styling
- [ ] Create useOnlineStatus hook with window online/offline event listeners
- [ ] Create offlineQueue utility with enqueue, processQueue, clearQueue, localStorage persistence
- [ ] Create NotFoundPage component with EmptyState, 404 illustration, and navigation links
- [ ] Add ToastContainer and OfflineBanner to App.tsx root level
- [ ] Refactor WalkinBooking.tsx to use useToast hook (remove local toast state)
- [ ] Wrap documentsApi.ts catch blocks with apiErrorHandler utility
- [ ] Wrap dashboardApi.ts catch blocks with apiErrorHandler utility
- [ ] Add unit tests for toastSlice actions and state management
- [ ] Add unit tests for apiErrorHandler error transformation (verify no stack traces exposed)
- [ ] Test offline scenario: Disconnect network → see offline banner → submit form → reconnect → action retries
- [ ] Validate toast accessibility with screen reader (NVDA/JAWS/VoiceOver)
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
