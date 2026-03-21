# Task - task_001_fe_session_timeout_warning

## Requirement Reference
- User Story: us_022
- Story Location: .propel/context/tasks/EP-001/us_022/us_022.md
- Acceptance Criteria:
    - AC4: User active for 13 minutes -> Warning modal appears with countdown timer and "Extend Session" button that refreshes TTL
    - AC5: Warning modal displayed, user clicks "Extend Session" -> Session TTL resets to 15 minutes, modal closes; if ignored, session expires at 15 minutes and user logged out
- Edge Cases:
    - User has multiple browser tabs open -> Session extension in any tab should dismiss warning in all tabs via shared session state

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | PENDING |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | TODO: Provide wireframe - upload to `.propel/context/wireframes/Hi-Fi/wireframe-timeout-warning.html` or add external URL |
| **Screen Spec** | N/A |
| **UXR Requirements** | UXR-604 |
| **Design Tokens** | designsystem.md#modals, designsystem.md#colors |

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
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | React Router | v7 |
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
Implement session timeout warning modal (UXR-604) appearing at 13-minute mark (2 minutes before 15-minute expiry) with countdown timer, "Extend Session" button calling POST /api/auth/refresh-session to reset Redis TTL, automatic logout on timeout (redirect to login), and cross-tab synchronization using localStorage events.

## Dependent Tasks
- task_001_fe_login_ui (for session storage infrastructure)
- task_002_be_login_session_api (for session TTL management)

## Impacted Components
- **NEW**: src/frontend/src/components/modals/SessionTimeoutModal.tsx
- **NEW**: src/frontend/src/hooks/useSessionTimeout.ts
- **NEW**: src/frontend/src/utils/sessionMonitor.ts
- **UPDATED**: src/frontend/src/store/slices/authSlice.ts (add refreshSession thunk)
- **UPDATED**: src/frontend/src/App.tsx (integrate SessionTimeoutModal)

## Implementation Plan
1. Create sessionMonitor.ts utility tracking last activity time and calculating remaining session time
2. Implement useSessionTimeout hook using setInterval checking session age every 10 seconds
3. Calculate when to show warning: currentTime - loginTime >= 13 minutes
4. Build SessionTimeoutModal with countdown timer (120 seconds), "Extend Session" and "Logout Now" buttons
5. Implement refreshSession async thunk calling POST /api/auth/refresh-session
6. On successful refresh: Update lastActivityTime, reset warning state, sync across tabs via localStorage event
7. Implement cross-tab synchronization: localStorage.setItem('session_extended', timestamp) triggers storage event in other tabs
8. On timeout expiry: Clear token, redirect to /login with message "Session expired"
9. Style modal with Tailwind CSS (warning colors, prominent CTA)
10. Add ARIA live region announcing countdown

## Current Project State
```
src/frontend/src/
├── components/
│   ├── layout/
│   ├── forms/
│   └── guards/
├── hooks/
├── store/
│   └── slices/
│       └── authSlice.ts
├── utils/
│   ├── tokenStorage.ts
│   └── roleBasedRedirect.ts
└── App.tsx
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/modals/SessionTimeoutModal.tsx | Session timeout warning modal component |
| CREATE | src/frontend/src/hooks/useSessionTimeout.ts | Custom hook monitoring session timeout |
| CREATE | src/frontend/src/utils/sessionMonitor.ts | Session activity tracking utility |
| MODIFY | src/frontend/src/store/slices/authSlice.ts | Add refreshSession async thunk |
| MODIFY | src/frontend/src/App.tsx | Add SessionTimeoutModal component |
| MODIFY | src/frontend/src/utils/tokenStorage.ts | Add getLoginTime, setLoginTime functions |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [localStorage Events](https://developer.mozilla.org/en-US/docs/Web/API/Window/storage_event)
- [ARIA Live Regions](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions)
- [React useEffect Timer Patterns](https://overreacted.io/making-setinterval-declarative-with-react-hooks/)

## Build Commands
```bash
cd src/frontend
npm install
npm run dev
npm test
npm run build
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [x] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [x] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] **[AI Tasks]** Prompt templates validated with test inputs
- [ ] **[AI Tasks]** Guardrails tested for input sanitization and output validation
- [ ] **[AI Tasks]** Fallback logic tested with low-confidence/error scenarios
- [ ] **[AI Tasks]** Token budget enforcement verified
- [ ] **[AI Tasks]** Audit logging verified (no PII in logs)
- [ ] **[Mobile Tasks]** Headless platform compilation succeeds
- [ ] **[Mobile Tasks]** Native dependency linking verified
- [ ] **[Mobile Tasks]** Permission manifests validated against task requirements

## Implementation Checklist
- [ ] Add setLoginTime(timestamp), getLoginTime() to tokenStorage.ts using sessionStorage
- [ ] Create sessionMonitor.ts with getSessionAge(), shouldShowWarning(), getTimeRemaining() functions
- [ ] Implement useSessionTimeout hook: setInterval every 10 seconds checking session age
- [ ] When session age >= 13 minutes: Set showWarning state to true
- [ ] When session age >= 15 minutes: Call logout(), redirect to /login with expiry message
- [ ] Build SessionTimeoutModal component with modal overlay, warning icon, countdown timer
- [ ] Display countdown in MM:SS format starting from 02:00 (2 minutes)
- [ ] Add "Extend Session" button calling refreshSession thunk
- [ ] Add "Logout Now" button calling logout action
- [ ] Implement refreshSession async thunk in authSlice: POST /api/auth/refresh-session, on success update loginTime
- [ ] On "Extend Session" click: Call refreshSession, sync via localStorage.setItem('session_extended', Date.now())
- [ ] Add storage event listener in useSessionTimeout: on 'session_extended' event, reset warning, update loginTime
- [ ] Close modal when session extended or user logs out
- [ ] Add ARIA live region with aria-live="polite" announcing countdown updates every 30 seconds
- [ ] Style modal with Tailwind CSS: warning amber colors, prominent buttons, centered layout
- [ ] Ensure modal cannot be dismissed by clicking outside (force user choice)
- [ ] Test warning appears at 13-minute mark (mock loginTime to 13 minutes ago)
- [ ] Test "Extend Session" resets timer and dismisses modal
- [ ] Test automatic logout at 15 minutes
- [ ] Test cross-tab synchronization: extend session in Tab A, verify modal dismissed in Tab B
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation (pending upload)
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
