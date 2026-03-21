# Task - TASK_011

## Requirement Reference
- User Story: US_022
- Story Location: .propel/context/tasks/EP-001/us_022/us_022.md
- Acceptance Criteria:
    - AC4: Warning modal appears at 13-minute mark with countdown and "Extend Session" button
    - AC5: "Extend Session" resets TTL to 15 minutes; if ignored, session expires at 15 minutes
- Edge Case:
    - Multiple browser tabs share session state for consistent timeout warnings

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | PENDING |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | TODO: Upload to `.propel/context/wireframes/Hi-Fi/wireframe-timeout-warning.html` or provide external URL |
| **Screen Spec** | figma_spec.md#UXR-604 |
| **UXR Requirements** | UXR-604 (Session timeout warning at 13-minute mark) |
| **Design Tokens** | designsystem.md#modals |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**
**IF Wireframe Status = AVAILABLE or EXTERNAL:**
- **MUST** implement session timeout warning modal per UXR-604 specification

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | TypeScript | 5.x |
| Library | Tailwind CSS | Latest |
| AI/ML | N/A | N/A |

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
Implement session timeout warning modal that displays at the 13-minute mark of user inactivity, showing a countdown timer and "Extend Session" button. Modal communicates with backend to refresh session token TTL and handles automatic logout if user ignores the warning.

## Dependent Tasks
- TASK_005 (Session management with Redis and 15-minute TTL)

## Impacted Components
- NEW: src/frontend/src/components/modals/SessionTimeoutModal.tsx
- NEW: src/frontend/src/hooks/useSessionTimeout.ts
- MODIFY: src/frontend/src/App.tsx (add SessionTimeoutModal)
- MODIFY: src/frontend/src/features/auth/authSlice.ts (add refreshSession thunk)

## Implementation Plan
1. Create useSessionTimeout hook tracking last activity timestamp
2. Implement 13-minute timer triggering warning modal display
3. Build SessionTimeoutModal with countdown timer (2 minutes remaining)
4. Add "Extend Session" button calling refreshSession API endpoint
5. Implement automatic logout when countdown reaches zero
6. Store session state in localStorage for cross-tab communication
7. Listen to storage events for synchronized timeout warnings across tabs
8. Reset activity timer on user interactions (mouse move, keypress, scroll)

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/modals/SessionTimeoutModal.tsx | Timeout warning modal with countdown |
| CREATE | src/frontend/src/hooks/useSessionTimeout.ts | Session timeout tracking hook |
| MODIFY | src/frontend/src/App.tsx | Include SessionTimeoutModal component |
| MODIFY | src/frontend/src/features/auth/authSlice.ts | Add refreshSession async thunk |

## External References
- **React Timer Hooks**: https://www.npmjs.com/package/react-timer-hook
- **LocalStorage Cross-Tab Communication**: https://developer.mozilla.org/en-US/docs/Web/API/Window/storage_event

## Implementation Checklist
- [x] Create useSessionTimeout hook: track lastActivity timestamp, calculate timeUntilWarning
- [x] Implement 13-minute timer (780 seconds) triggering modal display
- [x] Build SessionTimeoutModal: display countdown (2 min = 120 sec), "Extend Session" and "Logout" buttons
- [x] Implement refreshSession thunk calling POST /api/auth/refresh-session
- [x] Reset lastActivity timestamp on user interactions (mousemove, keydown, scroll, click)  
- [x] Store lastActivity in localStorage for cross-tab sync
- [x] Listen to storage events: sync timeout state across multiple browser tabs
- [x] Auto-logout when countdown reaches 0: clear auth state, redirect to /login
- [x] **[UI Tasks - MANDATORY]** Reference UXR-604 specification during implementation
