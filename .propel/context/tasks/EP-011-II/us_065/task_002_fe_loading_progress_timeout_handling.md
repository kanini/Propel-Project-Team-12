# Task - task_002_fe_loading_progress_timeout_handling

## Requirement Reference
- User Story: US_065 - Interaction Feedback & Loading States
- Story Location: .propel/context/tasks/EP-011-II/us_065/us_065.md
- Acceptance Criteria:
    - AC-2: Skeleton loaders display in content area matching expected layout shape with screen reader text label ("Loading appointments...") when data is being fetched
    - AC-3: Progress bar or step indicator shows percentage or stage completion with estimated time remaining for long-running operations (document upload, AI processing)
- Edge Case:
    - Loading state exceeds 10 seconds: "Still working..." message replaces initial loading text, after 30 seconds show "Something may have gone wrong" with retry button
    - Multiple concurrent loading states: Each content section manages its own skeleton loader independently; global loading bar only for full-page navigations

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A (cross-cutting pattern affecting all screens) |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-014-document-upload.html (shows progress bar), .propel/context/wireframes/Hi-Fi/wireframe-SCR-006-provider-browser.html (shows skeleton loading) |
| **Screen Spec** | .propel/context/docs/figma_spec.md#UXR-502 (skeleton states), #UXR-504 (progress indicators) |
| **UXR Requirements** | UXR-502 (Skeleton loading states), UXR-504 (Progress indicators for long operations) |
| **Design Tokens** | .propel/context/docs/designsystem.md#skeleton (animation, colors), designsystem.md#progress-bar (track, fill, height) |

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
Enhance loading states and progress indicators across the application to meet UXR-502 (skeleton loading) and UXR-504 (progress indicators) requirements. This task consolidates skeleton loading patterns with timeout handling (10s/30s thresholds), creates reusable progress bar components for long-running operations, and implements ARIA live regions for screen reader announcements. It also establishes global loading state management for full-page navigations while maintaining independent section-level loaders.

## Dependent Tasks
- None (enhances existing SkeletonLoader and UploadProgressBar components)

## Impacted Components
- **MODIFY** `src/frontend/src/components/common/SkeletonLoader.tsx` - Add timeout handling (10s "Still working...", 30s retry), ARIA live region with status text
- **MODIFY** `src/frontend/src/components/documents/UploadProgressBar.tsx` - Add estimated time remaining, enhance ARIA announcements
- **CREATE** `src/frontend/src/components/common/ProgressBar.tsx` - Generic progress bar component for any long-running operation
- **CREATE** `src/frontend/src/components/common/GlobalLoadingBar.tsx` - Top-of-page loading bar for full-page navigations
- **CREATE** `src/frontend/src/hooks/useLoadingTimeout.ts` - Custom hook for 10s/30s timeout state management
- **CREATE** `src/frontend/src/utils/estimateTimeRemaining.ts` - Utility to calculate ETA based on progress velocity
- **MODIFY** `src/frontend/src/pages/ProviderBrowser.tsx` - Use enhanced SkeletonLoader with timeout handling
- **MODIFY** `src/frontend/src/pages/MyAppointments.tsx` - Use enhanced SkeletonLoader with "Loading appointments..." label

## Implementation Plan

1. **Create useLoadingTimeout Hook**
   - Accept startTime and thresholds (default: 10s, 30s)
   - Return: `{ timeoutStage: 'initial' | 'still-working' | 'error' | null, elapsedTime, reset }`
   - Use useEffect with setInterval to track elapsed time
   - Transition stages: 0-10s (initial) → 10-30s (still-working) → 30s+ (error)
   - Clear interval on unmount or when loading completes
   - Reset function to restart timer for retry actions

2. **Enhance SkeletonLoader Component**
   - Add `loadingText` prop for screen reader label (e.g., "Loading appointments...")
   - Integrate useLoadingTimeout hook
   - Show different messages based on timeout stage:
     - 0-10s: Show `loadingText` ("Loading appointments...")
     - 10-30s: Show "Still working..." with spinner
     - 30s+: Show "Something may have gone wrong" with Retry button
   - Implement ARIA live region: `<div role="status" aria-live="polite" aria-atomic="true">`
   - Add `onRetry` callback prop for retry button action
   - Preserve existing 300ms delay from UXR-502
   - Reference designsystem.md: neutral-200 color, pulse animation (1.5s cycle)

3. **Create Generic ProgressBar Component**
   - Props: `progress` (0-100), `label`, `showPercentage`, `size` (compact: 4px, default: 8px)
   - Implement horizontal bar with track (neutral-200) and fill (primary-500)
   - Add smooth width transition (transition-all duration-300)
   - Display percentage text aligned right (optional)
   - Support indeterminate state (animated gradient slide for unknown progress)
   - ARIA attributes: role="progressbar", aria-valuenow, aria-valuemin, aria-valuemax, aria-label
   - Reference wireframe-SCR-014-document-upload.html for visual styling

4. **Enhance UploadProgressBar Component**
   - Add estimated time remaining calculation using estimateTimeRemaining utility
   - Display format: "Uploading... 2 minutes remaining" or "Uploading... 30 seconds remaining"
   - Show chunks received vs total chunks if available: "42/50 chunks"
   - Add indeterminate state for "Processing..." phase (no chunk data available)
   - Enhance ARIA announcements: announce percentage milestones (25%, 50%, 75%, 100%)
   - Use ARIA live region with politeness level: `aria-live="polite"`

5. **Create estimateTimeRemaining Utility**
   - Function signature: `estimateTimeRemaining(progress: number, startTime: number): string | null`
   - Calculate progress velocity: (currentProgress - initialProgress) / elapsed time
   - Estimate remaining time: (100 - currentProgress) / velocity
   - Format output: "X minutes remaining" or "X seconds remaining"
   - Return null if insufficient data (progress < 5%) or velocity is zero
   - Handle edge cases: stalled progress (velocity drops to 0 for 5s), complete progress

6. **Create GlobalLoadingBar Component**
   - Fixed position at top of viewport (z-index: 1000)
   - Thin bar (height: 3px) with primary-500 color
   - Animated width: 0% → 30% (fast) → 60% (slower) → 90% (very slow) → 100% (on complete)
   - Use CSS transitions: fast (200ms), slow (800ms), very slow (2s)
   - Integrate with React Router navigation events: start on route change, complete on render
   - Auto-hide after route completes (fade out 300ms)
   - Reference NProgress.js behavior for UX pattern

7. **Integrate GlobalLoadingBar with React Router**
   - Wrap App component with GlobalLoadingBar
   - Listen to router events: `useNavigation()` from react-router-dom
   - Show bar on navigation.state === 'loading'
   - Hide bar on navigation.state === 'idle'
   - Ensure bar doesn't show for instant navigations (<100ms)

8. **Refactor ProviderBrowser.tsx**
   - Replace basic SkeletonLoader with enhanced version
   - Pass loadingText="Loading providers..."
   - Add onRetry callback to refetch providers on timeout
   - Ensure skeleton matches final provider card layout (avatar + name + specialty + availability)

9. **Refactor MyAppointments.tsx**
   - Use enhanced SkeletonLoader with loadingText="Loading appointments..."
   - Add timeout handling with retry button
   - Ensure skeleton matches appointment card layout

10. **Add Screen Reader Testing**
    - Test: ARIA live region announces "Loading appointments..." on initial load
    - Test: Announce "Still working..." after 10 seconds
    - Test: Announce "Something may have gone wrong. Retry available." after 30 seconds
    - Test: Progress bar announces percentage milestones (25%, 50%, 75%, 100%)
    - Use screen reader testing tools: NVDA, JAWS, or VoiceOver

## Current Project State
```
src/frontend/
├── src/
│   ├── components/
│   │   ├── common/
│   │   │   ├── SkeletonLoader.tsx (EXISTS - NEEDS TIMEOUT ENHANCEMENT)
│   │   │   └── [ProgressBar.tsx TO BE CREATED]
│   │   │   └── [GlobalLoadingBar.tsx TO BE CREATED]
│   │   ├── documents/
│   │   │   └── UploadProgressBar.tsx (EXISTS - NEEDS ETA FEATURE)
│   │   └── appointments/
│   │       └── ProgressIndicator.tsx (EXISTS - wizard steps, not for loading)
│   ├── hooks/
│   │   └── [useLoadingTimeout.ts TO BE CREATED]
│   ├── utils/
│   │   └── [estimateTimeRemaining.ts TO BE CREATED]
│   ├── pages/
│   │   ├── ProviderBrowser.tsx (USES SkeletonLoader - TO REFACTOR)
│   │   └── MyAppointments.tsx (HAS LOADING STATE - TO REFACTOR)
│   └── App.tsx (TO WRAP WITH GlobalLoadingBar)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/hooks/useLoadingTimeout.ts | Custom hook for tracking 10s/30s timeout stages with elapsed time state |
| CREATE | src/frontend/src/components/common/ProgressBar.tsx | Generic progress bar component with percentage display and indeterminate state |
| CREATE | src/frontend/src/components/common/GlobalLoadingBar.tsx | Top-of-page loading bar for full-page navigations (React Router integration) |
| CREATE | src/frontend/src/utils/estimateTimeRemaining.ts | Utility function to calculate ETA based on progress velocity |
| MODIFY | src/frontend/src/components/common/SkeletonLoader.tsx | Add timeout handling, ARIA live region, retry button, loadingText prop (all lines) |
| MODIFY | src/frontend/src/components/documents/UploadProgressBar.tsx | Add estimated time remaining display, enhance ARIA announcements (lines 47-80) |
| MODIFY | src/frontend/src/pages/ProviderBrowser.tsx | Use enhanced SkeletonLoader with loadingText and onRetry (lines 159-161) |
| MODIFY | src/frontend/src/pages/MyAppointments.tsx | Use enhanced SkeletonLoader with timeout handling |
| MODIFY | src/frontend/src/App.tsx | Wrap app with GlobalLoadingBar component for route transitions |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [ARIA Live Regions](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions) - Screen reader announcements
- [ARIA Progressbar Role](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Roles/progressbar_role) - Accessible progress indicators
- [NProgress.js](https://ricostacruz.com/nprogress/) - Reference implementation for global loading bar UX
- [Skeleton Screens Best Practices](https://www.nngroup.com/articles/skeleton-screens/) - Nielsen Norman Group research
- [Web Content Accessibility Guidelines (WCAG) 2.2: Status Messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html) - ARIA live region guidance
- [React Router v6: useNavigation](https://reactrouter.com/en/main/hooks/use-navigation) - Navigation state tracking

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
- [ ] Skeleton loader shows initial message for 0-10 seconds
- [ ] "Still working..." message appears after 10 seconds
- [ ] "Something may have gone wrong" with retry button appears after 30 seconds
- [ ] ARIA live region announces loading state changes to screen readers (test with NVDA/JAWS/VoiceOver)
- [ ] Progress bar shows accurate percentage for document upload operations
- [ ] Estimated time remaining updates dynamically based on progress velocity
- [ ] GlobalLoadingBar appears only for full-page navigations (not for component-level data fetching)
- [ ] Multiple concurrent loading sections maintain independent states

## Technical Context

### Architecture Patterns
- **Timeout State Machine**: useLoadingTimeout implements 3-stage state machine (initial → still-working → error)
- **ARIA Live Regions**: Polite announcements for non-critical status updates (loading progress)
- **Progress Velocity Calculation**: Linear regression over sliding window (last 5 progress updates) for ETA accuracy
- **Global Loading Coordination**: GlobalLoadingBar only for route-level transitions; section-level loaders managed independently

### Related Requirements
- UXR-502: Skeleton loading states for data-fetching screens when load exceeds 300ms
- UXR-504: Animated transitions between flow steps (150-300ms ease-out) respecting prefers-reduced-motion
- UXR-207: ARIA live regions for status announcements (accessibility requirement)
- NFR-001: API response time within 500ms for 95th percentile (most requests won't hit 10s timeout)

### Edge Case Handling
- **Stalled progress**: If progress velocity drops to 0 for 5 consecutive seconds, switch to indeterminate mode
- **Instant completion**: If progress jumps from <90% to 100% in single update, show completion immediately without ETA
- **Concurrent sections**: Each SkeletonLoader instance maintains independent timeout state (no shared global timer)

## Implementation Checklist
- [ ] Create useLoadingTimeout hook with 10s/30s threshold stages (initial, still-working, error)
- [ ] Enhance SkeletonLoader with loadingText prop, ARIA live region, timeout messages, and retry button
- [ ] Create generic ProgressBar component with horizontal bar, percentage display, and indeterminate state
- [ ] Create GlobalLoadingBar component with fixed top position, animated width, React Router integration
- [ ] Implement estimateTimeRemaining utility with progress velocity calculation
- [ ] Enhance UploadProgressBar with ETA display and ARIA milestone announcements (25%, 50%, 75%, 100%)
- [ ] Refactor ProviderBrowser.tsx to use enhanced SkeletonLoader with loadingText="Loading providers..."
- [ ] Refactor MyAppointments.tsx to use enhanced SkeletonLoader with timeout handling
- [ ] Integrate GlobalLoadingBar in App.tsx with React Router navigation events
- [ ] Add unit tests for useLoadingTimeout stage transitions (0s → 10s → 30s)
- [ ] Add unit tests for estimateTimeRemaining calculation with various progress velocities
- [ ] Test ARIA live region announcements with screen reader (NVDA/JAWS/VoiceOver)
- [ ] Validate skeleton layout matches final content shape (provider cards, appointment cards)
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
