# Task - task_001_fe_button_states_optimistic_ui

## Requirement Reference
- User Story: US_065 - Interaction Feedback & Loading States
- Story Location: .propel/context/tasks/EP-011-II/us_065/us_065.md
- Acceptance Criteria:
    - AC-1: Visual feedback (button state change, spinner, or checkmark) appears within 200ms of button click or form submit
    - AC-4: UI updates immediately for quick actions (toggle, mark complete) and rolls back with error toast if server request fails
- Edge Case:
    - Multiple concurrent loading states: Each content section manages its own state independently

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A (cross-cutting pattern affecting all screens) |
| **Wireframe Status** | EXTERNAL |
| **Wireframe Type** | URL |
| **Wireframe Path/URL** | Multiple screens: SCR-007 (Booking), SCR-014 (Document Upload), SCR-011 (Reschedule) all demonstrate button loading states |
| **Screen Spec** | .propel/context/docs/figma_spec.md (all screens) |
| **UXR Requirements** | UXR-501 (200ms action feedback) |
| **Design Tokens** | .propel/context/docs/designsystem.md#motion (transition durations), designsystem.md#button (states) |

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
Standardize button loading states and implement optimistic UI pattern across all interactive components to meet UXR-501 (200ms action feedback) requirement. This task creates reusable button state components with loading spinners, disabled states, and success/error indicators. It also implements optimistic UI updates for quick actions (toggles, checkmarks) with automatic rollback on server failure.

## Dependent Tasks
- None (foundational UI pattern enhancement)

## Impacted Components
- **MODIFY** `src/frontend/src/components/appointments/BookingSteps.tsx` - Refactor to use standardized Button component with loading states
- **MODIFY** `src/frontend/src/components/appointments/ConfirmationDialog.tsx` - Replace custom button states with Button component
- **MODIFY** `src/frontend/src/components/waitlist/WaitlistEnrollmentModal.tsx` - Standardize submit button loading state
- **MODIFY** `src/frontend/src/components/documents/DocumentUpload.tsx` - Use Button component for upload actions
- **CREATE** `src/frontend/src/components/common/Button.tsx` - Standardized button component with loading/success/error states
- **CREATE** `src/frontend/src/hooks/useOptimisticUpdate.ts` - Custom hook for optimistic UI pattern with rollback
- **CREATE** `src/frontend/src/utils/actionFeedback.ts` - Utility functions for 200ms feedback timing

## Implementation Plan

1. **Create Button Component with State Management**
   - Design Button component API with variants (primary, secondary, tertiary, danger)
   - Implement state props: `isLoading`, `isSuccess`, `isError`, `disabled`
   - Add loading spinner with 200ms transition (UXR-501)
   - Add success checkmark animation (scale-in 150ms)
   - Add error shake animation (300ms)
   - Reference designsystem.md for button sizes (small: 32px, medium: 40px, large: 48px)
   - Use Tailwind motion tokens: `transition-fast` (100ms), `transition-normal` (200ms)
   - Implement proper ARIA states: `aria-busy`, `aria-live="polite"`

2. **Implement Loading Spinner SVG**
   - Create reusable Spinner component (16px, 20px, 24px sizes)
   - Use CSS animation: `animate-spin` with 0.6s duration
   - Reference existing spinner patterns in MyAppointments.tsx and BookingSteps.tsx
   - Ensure currentColor inheritance for theming
   - Add `aria-hidden="true"` for decorative spinner

3. **Create useOptimisticUpdate Hook**
   - Signature: `useOptimisticUpdate<T>(mutationFn, options)`
   - Implement immediate local state update on user action
   - Show success feedback (checkmark) for 1.5 seconds
   - Rollback state on mutation failure
   - Trigger error toast on rollback: `showErrorToast(error.message)`
   - Return: `{ execute, isLoading, isSuccess, isError, rollback }`
   - Handle concurrent updates: queue mutations or cancel pending

4. **Implement Action Feedback Timing Utility**
   - Create `ensureMinimumFeedback(promise, minDuration = 200)` function
   - Guarantees visual feedback displays for at least minDuration ms
   - Use case: fast API responses (<200ms) should still show loading state
   - Return: promise that resolves after max(apiTime, minDuration)

5. **Refactor BookingSteps.tsx**
   - Replace inline button with `<Button isLoading={isBooking} />`
   - Remove custom spinner SVG
   - Add success checkmark after successful booking
   - Implement 200ms minimum feedback on "Confirm Booking" button

6. **Refactor WaitlistEnrollmentModal.tsx**
   - Replace submit button with `<Button isLoading={isSubmitting} />`
   - Use useOptimisticUpdate for "Update Preferences" action
   - Show immediate success state, rollback on error

7. **Refactor ConfirmationDialog.tsx**
   - Replace download button with `<Button isLoading={isDownloadingPDF} />`
   - Add download success checkmark (showing for 1.5s)
   - Implement error state for PDF generation failures

8. **Add Visual Feedback Tests**
   - Test: Button transition to loading state occurs within 200ms
   - Test: Success checkmark displays for 1.5 seconds
   - Test: Error state triggers shake animation
   - Test: Optimistic update rolls back on server error
   - Test: Multiple concurrent button actions maintain independent states

## Current Project State
```
src/frontend/
├── src/
│   ├── components/
│   │   ├── common/
│   │   │   └── SkeletonLoader.tsx (EXISTS)
│   │   ├── appointments/
│   │   │   ├── BookingSteps.tsx (HAS CUSTOM SPINNER - TO REFACTOR)
│   │   │   ├── ConfirmationDialog.tsx (HAS CUSTOM BUTTON STATE - TO REFACTOR)
│   │   │   └── ProgressIndicator.tsx (EXISTS - wizard progress)
│   │   ├── waitlist/
│   │   │   └── WaitlistEnrollmentModal.tsx (HAS isSubmitting FLAG - TO REFACTOR)
│   │   └── documents/
│   │       ├── DocumentUpload.tsx (USES INLINE BUTTONS - TO REFACTOR)
│   │       └── UploadProgressBar.tsx (EXISTS - progress bar)
│   ├── hooks/
│   │   └── [useOptimisticUpdate.ts TO BE CREATED]
│   ├── utils/
│   │   └── [actionFeedback.ts TO BE CREATED]
│   └── store/
│       └── slices/ (RTK Query mutations for optimistic updates)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/common/Button.tsx | Standardized button component with loading, success, error states and 200ms feedback guarantee |
| CREATE | src/frontend/src/components/common/Spinner.tsx | Reusable loading spinner component (16px, 20px, 24px sizes) |
| CREATE | src/frontend/src/hooks/useOptimisticUpdate.ts | Custom hook for optimistic UI pattern with automatic rollback on error |
| CREATE | src/frontend/src/utils/actionFeedback.ts | Utility functions for ensuring minimum 200ms visual feedback duration |
| MODIFY | src/frontend/src/components/appointments/BookingSteps.tsx | Replace custom spinner with Button component (lines 275-305) |
| MODIFY | src/frontend/src/components/appointments/ConfirmationDialog.tsx | Replace download button with Button component (lines 240-260) |
| MODIFY | src/frontend/src/components/waitlist/WaitlistEnrollmentModal.tsx | Standardize submit button and use useOptimisticUpdate (lines 363-376) |
| MODIFY | src/frontend/src/components/documents/DocumentUpload.tsx | Replace inline upload buttons with Button component |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [React Testing Library: Testing Button States](https://testing-library.com/docs/react-testing-library/example-intro)
- [ARIA Button Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/button/)
- [Web Vitals: Interaction to Next Paint (INP)](https://web.dev/articles/inp) - Target: <200ms for good user experience
- [Optimistic UI Pattern](https://www.apollographql.com/docs/react/performance/optimistic-ui/)
- [Tailwind CSS: Transition & Animation](https://tailwindcss.com/docs/transition-property)

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
- [ ] Button loading state appears within 200ms of click (verified via React Testing Library + user-event timing)
- [ ] Success checkmark animation completes in 1.5 seconds
- [ ] Optimistic UI updates roll back correctly on server error (test with network failure simulation)
- [ ] Multiple concurrent button actions maintain independent loading states
- [ ] Spinner size matches button size variant (small: 16px, medium: 20px, large: 24px)
- [ ] ARIA attributes correctly announce loading/success/error states to screen readers

## Implementation Checklist
- [ ] Create Button component with variants (primary, secondary, tertiary, danger) and sizes (small, medium, large)
- [ ] Implement button states: isLoading (spinner), isSuccess (checkmark), isError (shake animation)
- [ ] Add 200ms minimum feedback timing using ensureMinimumFeedback utility
- [ ] Create Spinner component with size variants (16px, 20px, 24px) and currentColor inheritance
- [ ] Implement useOptimisticUpdate hook with immediate UI update and automatic rollback
- [ ] Add actionFeedback.ts utility with ensureMinimumFeedback function
- [ ] Refactor BookingSteps.tsx to use Button component (replace lines 275-305)
- [ ] Refactor ConfirmationDialog.tsx to use Button component for PDF download (lines 240-260)
- [ ] Refactor WaitlistEnrollmentModal.tsx to use Button and useOptimisticUpdate (lines 363-376)
- [ ] Add unit tests for Button component states (loading → success → reset)
- [ ] Add unit tests for useOptimisticUpdate rollback behavior
- [ ] Test 200ms minimum feedback with fast API responses (<100ms)
- [ ] Validate ARIA announcements for button state changes (aria-busy, aria-live)
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
