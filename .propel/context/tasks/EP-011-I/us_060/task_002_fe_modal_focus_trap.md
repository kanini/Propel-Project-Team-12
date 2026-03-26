# Task - task_002_fe_modal_focus_trap

## Requirement Reference
- User Story: US_060 - Keyboard Navigation & Focus Management
- Story Location: .propel/context/tasks/EP-011-I/us_060/us_060.md
- Acceptance Criteria:
    - AC-2: **Given** modal/dialog focus management (UXR-205), **When** a modal opens, **Then** focus is trapped within the modal, Escape closes it, and focus returns to the trigger element on close.
- Edge Case:
    - What happens when focus is on a disabled element? Disabled elements within the modal are removed from the tab order and skipped during focus cycling.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (modals present in multiple wireframes) |
| **Screen Spec** | figma_spec.md#modal-overlay-inventory |
| **UXR Requirements** | UXR-205 (Focus management for modals and dynamic content) |
| **Design Tokens** | designsystem.md#accessibility-requirements |

> **Wireframe Status Legend:**
> - **AVAILABLE**: Local file exists at specified path
> - **PENDING**: UI-impacting task awaiting wireframe (provide file or URL)
> - **EXTERNAL**: Wireframe provided via external URL
> - **N/A**: Task has no UI impact

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
| Frontend | React + TypeScript | React 18.x, TypeScript 5.x |
| Frontend | Redux Toolkit | 2.x |
| Frontend | Tailwind CSS | Latest |
| Library | focus-trap-react | Latest (for modal focus management) |
| Testing | Vitest + React Testing Library | Latest |

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

Implement comprehensive focus trap pattern for all modal and dialog components to ensure keyboard-only users cannot accidentally tab out of modals into background content. This task implements WAI-ARIA dialog pattern with focus trapping, Escape key dismissal, and proper focus restoration when modals close. The implementation ensures modals are fully keyboard accessible and prevent focus leakage to underlying page content.

## Dependent Tasks
- task_001_fe_tab_order_skip_links (establishes baseline tab order)

## Impacted Components
- **Modal Components**: SessionTimeoutModal, RescheduleModal, WaitlistEnrollmentModal
- **Dialog Components**: Confirmation dialogs, alert dialogs
- **Overlay Components**: Drawers, sidesheets (if applicable)
- **Custom Hooks**: useFocusTrap, useModalState
- **Background Content**: Inert attribute implementation

## Implementation Plan

### 1. Install and Configure focus-trap-react
- Install `focus-trap-react` npm package
- Review library API and configuration options
- Test basic focus trap functionality in isolation
- Determine if additional polyfills needed for older browsers

### 2. Create Reusable Modal Base Component
- Create `BaseModal` component wrapping FocusTrap from focus-trap-react
- Accept props: isOpen, onClose, initialFocus (optional), children
- Add `role="dialog"`, `aria-modal="true"` attributes
- Implement proper ARIA labeling (`aria-labelledby`, `aria-describedby`)
- Add backdrop click handler to close (configurable)
- Style overlay with proper z-index and backdrop styles

### 3. Implement Focus Trap Logic
- Configure FocusTrap with appropriate options:
  - `initialFocus`: Focus first interactive element or specified element
  - `fallbackFocus`: Modal container if no focusable elements
  - `escapeDeactivates`: true (Escape key closes modal)
  - `returnFocusOnDeactivate`: true (return focus to trigger)
  - `clickOutsideDeactivates`: configurable (default true for modals)
- Store reference to trigger element when modal opens
- Restore focus to trigger element when modal closes
- Handle edge case where trigger element no longer exists (focus fallback element)

### 4. Implement Escape Key Handler
- Add global keydown listener when modal is open
- Check for Escape key press
- Call onClose callback to dismiss modal
- Ensure event doesn't propagate to other handlers
- Handle nested modals (Escape closes only the topmost modal)

### 5. Update Existing Modal Components
- Refactor SessionTimeoutModal to use BaseModal
- Refactor RescheduleModal to use BaseModal
- Refactor WaitlistEnrollmentModal to use BaseModal
- Add any project-specific confirmation/alert modals
- Ensure all modals have visible close button with accessible label

### 6. Implement Background Content Inert State
- Add `aria-hidden="true"` to main content when modal opens
- Add `inert` attribute to background content (supported in modern browsers)
- Use polyfill for `inert` if needed (wicg-inert)
- Prevent scrolling of background content when modal is open
- Remove attributes when modal closes

### 7. Handle Special Cases
- Disabled elements within modal: Ensure they're skipped in tab cycle
- Modals with forms: Ensure validation errors within modal are accessible
- Nested modals: Stack multiple modals if needed (rare case)
- Modal with no focusable elements: Focus container itself with `tabindex="-1"`

### 8. Create Automated Tests
- Test focus moves to modal when opened
- Test Tab cycles through only modal content (not background)
- Test Shift+Tab cycles backwards within modal
- Test Escape key closes modal
- Test focus returns to trigger element on close
- Test backdrop click closes modal
- Test keyboard navigation through modal form fields

## Current Project State

```
src/frontend/src/
├── components/
│   ├── modals/
│   │   ├── SessionTimeoutModal.tsx (needs focus trap)
│   │   ├── RescheduleModal.tsx (needs focus trap)
│   │   └── WaitlistEnrollmentModal.tsx (needs focus trap)
│   └── common/
│       └── (potential location for BaseModal)
├── hooks/
│   └── (potential location for useFocusTrap, useModalState)
└── __tests__/
    └── components/modals/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/common/BaseModal.tsx | Reusable modal with focus trap |
| CREATE | src/hooks/useFocusTrap.ts | Custom hook wrapping focus-trap-react |
| CREATE | src/hooks/useModalState.ts | Modal state management hook |
| MODIFY | src/components/modals/SessionTimeoutModal.tsx | Refactor to use BaseModal with focus trap |
| MODIFY | src/components/modals/RescheduleModal.tsx | Refactor to use BaseModal with focus trap |
| MODIFY | src/components/modals/WaitlistEnrollmentModal.tsx | Refactor to use BaseModal with focus trap |
| MODIFY | src/components/layout/MainLayout.tsx | Add aria-hidden when modal is open |
| CREATE | src/__tests__/components/common/BaseModal.test.tsx | Focus trap test suite |
| CREATE | docs/MODAL_FOCUS_PATTERNS.md | Modal implementation guide |

## External References

### WCAG 2.2 Standards
- [No Keyboard Trap (2.1.2)](https://www.w3.org/WAI/WCAG22/Understanding/no-keyboard-trap.html)
- [Focus Order (2.4.3)](https://www.w3.org/WAI/WCAG22/Understanding/focus-order.html)

### WAI-ARIA Dialog Pattern
- [Dialog (Modal) Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/)
- [Dialog (Modal) Examples](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/examples/dialog/)
- [Alert Dialog Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/alertdialog/)

### Focus Management Libraries
- [focus-trap-react](https://github.com/focus-trap/focus-trap-react)
- [focus-trap](https://github.com/focus-trap/focus-trap) (core library)
- [react-focus-lock](https://github.com/theKashey/react-focus-lock) (alternative)

### HTML Inert Attribute
- [MDN: inert](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/inert)
- [wicg-inert polyfill](https://github.com/WICG/inert)

### React & Testing
- [React Portals](https://react.dev/reference/react-dom/createPortal) (for modal rendering)
- [Testing Library: Modal Testing](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library#using-query-variants-for-anything-except-checking-for-non-existence)

### Project Documentation
- [Web Accessibility Standards Rule](.propel/rules/web-accessibility-standards.md)
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)

## Build Commands

```bash
# Navigate to frontend directory
cd src/frontend

# Install focus-trap-react dependency
npm install focus-trap-react

# Optional: Install inert polyfill if supporting older browsers
npm install wicg-inert

# Run type checking
npm run typecheck

# Run linting
npm run lint

# Run unit tests
npm test

# Run modal tests specifically
npm test -- --grep="modal"

# Build for production
npm run build

# Development server (manual modal testing)
npm run dev
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Focus trapped within modal when open (Tab doesn't reach background)
- [ ] Escape key closes modal
- [ ] Focus returns to trigger element on close
- [ ] Background content has aria-hidden and inert when modal is open
- [ ] Modal has proper ARIA role and labels
- [ ] All existing modals refactored to use BaseModal
- [ ] Keyboard-only navigation through modals works correctly

## Implementation Checklist
- [ ] Install `focus-trap-react` npm package (verify compatibility with React 18.x)
- [ ] Create `BaseModal` component wrapping FocusTrap from focus-trap-react
- [ ] Add `role="dialog"` and `aria-modal="true"` to BaseModal
- [ ] Implement ARIA labeling with `aria-labelledby` and `aria-describedby`
- [ ] Configure FocusTrap options (initialFocus, returnFocusOnDeactivate, escapeDeactivates)
- [ ] Store trigger element reference when modal opens (using useRef)
- [ ] Implement Escape key handler to call onClose callback
- [ ] Add backdrop click handler (configurable via prop)
- [ ] Apply overlay styling with proper z-index (z-50 or higher)
- [ ] Refactor SessionTimeoutModal to use BaseModal
- [ ] Refactor RescheduleModal to use BaseModal
- [ ] Refactor WaitlistEnrollmentModal to use BaseModal
- [ ] Add `aria-hidden="true"` to main content when any modal is open
- [ ] Implement `inert` attribute on background content (with polyfill fallback)
- [ ] Prevent body scrolling when modal is open (add overflow-hidden)
- [ ] Create `useFocusTrap` custom hook for non-modal focus trap scenarios (optional)
- [ ] Create `useModalState` hook for modal state management (optional)
- [ ] Write tests for focus trap functionality (focus in, Tab cycling, Escape, focus out)
- [ ] Test focus restoration when trigger element no longer exists
- [ ] Test modal accessibility with screen reader (verify ARIA attributes announce correctly)
- [ ] Create developer documentation for modal focus patterns
