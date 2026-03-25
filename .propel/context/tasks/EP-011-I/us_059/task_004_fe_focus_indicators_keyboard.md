# Task - task_004_fe_focus_indicators_keyboard

## Requirement Reference
- User Story: US_059 - WCAG 2.2 AA & Semantic HTML
- Story Location: .propel/context/tasks/EP-011-I/us_059/us_059.md
- Acceptance Criteria:
    - AC-4: **Given** focus visibility, **When** a user navigates via keyboard, **Then** all focusable elements display a visible focus indicator that meets the 2px minimum outline requirement.
- Edge Case:
    - None specific to focus indicators

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all 26 wireframes SCR-001 through SCR-026) |
| **Screen Spec** | figma_spec.md (All screens) |
| **UXR Requirements** | UXR-202 (Focus indicators with 3:1 contrast), UXR-205 (Keyboard-only navigation) |
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
|-------|----------||---------|
| Frontend | React + TypeScript | React 18.x, TypeScript 5.x |
| Frontend | Tailwind CSS | Latest |
| Library | React Router | v6 |
| Testing | Vitest + React Testing Library | Latest |
| Tool | Playwright | Latest (for E2E keyboard testing) |

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

Implement comprehensive keyboard navigation and visible focus indicators across all interactive elements to meet WCAG 2.2 Level AA standards. This task ensures all focusable elements (buttons, links, form controls, custom widgets) display a 2px minimum outline with ≥3:1 contrast ratio, keyboard-only users can complete all workflows without using a mouse, tab order follows logical reading order, focus is not trapped except in modals, and Escape key closes dismissible overlays.

## Dependent Tasks
- task_001_fe_semantic_html_landmarks (provides semantic structure)
- task_002_fe_color_contrast_validation (ensures focus indicator contrast)

## Impacted Components
- **Global CSS**: Focus styles in index.css or Tailwind config
- **All Interactive Components**: Buttons, links, form controls, cards, modals
- **Custom Widgets**: Time slot grids, calendar widgets, dropdowns, accordions
- **Modal/Dialog Components**: Focus trap implementation
- **Navigation Components**: Sidebar, breadcrumbs, bottom nav

## Implementation Plan

### 1. Implement Global Focus Styles
- Add `:focus-visible` styles to index.css or Tailwind config
- Use 2px solid outline with offset for visibility
- Set outline color to `primary-500` (#0F62FE) or equivalent
- Ensure 3:1 contrast against all background colors
- Remove default browser outline and replace with custom style
- Implement `focus-visible` polyfill if needed (modern browsers support natively)

### 2. Audit and Fix Tab Order
- Review tab order on all 26 screens using keyboard navigation
- Ensure tab order follows visual/reading order (top-to-bottom, left-to-right)
- Remove `tabindex` from static content (headings, paragraphs, divs)
- Use `tabindex="0"` for custom interactive elements not naturally focusable
- Use `tabindex="-1"` for programmatically focusable elements (skip links, error messages)
- Fix any unexpected tab order jumps

### 3. Implement Keyboard Event Handlers
- Ensure all clickable divs/spans are converted to `<button>` or have proper role and keyboard handlers
- Add `onKeyDown` handlers for Enter and Space keys on custom controls
- Implement Escape key to close modals, dropdowns, and tooltips
- Add Arrow key navigation to custom widgets (date pickers, time slot grids)
- Implement Home/End keys for lists and grids

### 4. Implement Modal Focus Management
- Trap focus within modal using `focus-trap-react` library or custom solution
- Set initial focus to first focusable element (close button or primary action)
- Restore focus to trigger element when modal closes
- Prevent background content from being focusable (use `aria-hidden` and `inert`)
- Allow Escape key to close modal

### 5. Enhance Custom Widget Accessibility
- Time slot grids: Add Arrow key navigation (↑↓←→)
- Dropdowns/Comboboxes: Implement Up/Down arrows, Enter to select, Escape to close
- Accordions: Space/Enter to toggle, optionally arrow keys to navigate between headers
- Tabs: Left/Right arrows to navigate between tabs, Home/End for first/last
- Calendar widgets: Arrow keys for date navigation, Page Up/Down for months

### 6. Add Skip Links
- Ensure "Skip to main content" link is present on all pages
- Add additional skip links if page has complex structure ("Skip to search results")
- Style skip links to be visible on focus (position: absolute → static on focus)
- Test skip links navigate to correct targets

### 7. Remove Focus Traps
- Audit all pages for unintended focus traps (focus stuck in region)
- Ensure tab navigation cycles through all content
- Fix overflow containers that prevent focus visibility
- Ensure modals allow Escape to exit

### 8. Test Keyboard Navigation
- Complete all user workflows using keyboard only (no mouse)
- Test appointment booking flow (select provider, date, time, submit)
- Test form submissions (registration, login, manual intake)
- Test document upload flow
- Test navigation between screens
- Document any workflows that cannot be completed via keyboard

## Current Project State

```
src/frontend/
├── src/
│   ├── index.css (global styles, needs focus-visible)
│   ├── components/
│   │   ├── layout/
│   │   │   └── Sidebar.tsx (navigation links)
│   │   ├── common/
│   │   │   └── Pagination.tsx (needs arrow key support)
│   │   ├── forms/
│   │   │   └── (form controls need focus styles)
│   │   ├── appointments/
│   │   │   ├── TimeSlotGrid.tsx (needs arrow navigation)
│   │   │   └── ProgressIndicator.tsx (has nav role)
│   │   └── modals/
│   │       └── SessionTimeoutModal.tsx (needs focus trap)
│   └── pages/
│       └── (all pages need keyboard testing)
├── tailwind.config.js
└── __tests__/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/index.css | Add global :focus-visible styles with 2px outline |
| MODIFY | tailwind.config.js | Configure focus ring utilities for accessibility |
| CREATE | src/hooks/useFocusTrap.ts | Custom hook for modal focus management |
| MODIFY | src/components/modals/SessionTimeoutModal.tsx | Implement focus trap |
| MODIFY | src/components/appointments/TimeSlotGrid.tsx | Add arrow key navigation |
| MODIFY | src/components/common/Pagination.tsx | Add arrow key navigation |
| MODIFY | src/components/**/*.tsx | Convert clickable divs to buttons with keyboard handlers |
| CREATE | src/__tests__/accessibility/keyboardNavigation.test.tsx | Keyboard accessibility test suite |
| CREATE | test-automation/tests/keyboard-navigation.spec.ts | E2E keyboard tests (Playwright) |
| CREATE | docs/KEYBOARD_ACCESSIBILITY.md | Keyboard navigation patterns guide |

## External References

### WCAG 2.2 Standards
- [Focus Visible (2.4.7) - Enhanced (AAA)](https://www.w3.org/WAI/WCAG22/Understanding/focus-visible.html)
- [Focus Appearance (2.4.13) - Level AAA](https://www.w3.org/WAI/WCAG22/Understanding/focus-appearance.html)
- [Keyboard (2.1.1)](https://www.w3.org/WAI/WCAG22/Understanding/keyboard.html)
- [No Keyboard Trap (2.1.2)](https://www.w3.org/WAI/WCAG22/Understanding/no-keyboard-trap.html)

### WAI-ARIA Authoring Practices
- [Keyboard Navigation Inside Components](https://www.w3.org/WAI/ARIA/apg/practices/keyboard-interface/)
- [Managing Focus](https://www.w3.org/WAI/ARIA/apg/practices/keyboard-interface/#kbd_focus_management)
- [Grid Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/grid/)
- [Accordion Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/accordion/)

### Focus Management Libraries
- [focus-trap-react](https://github.com/focus-trap/focus-trap-react)
- [react-focus-lock](https://github.com/theKashey/react-focus-lock)
- [Radix UI Primitives (Dialog with focus management)](https://www.radix-ui.com/primitives/docs/components/dialog)

### CSS Focus Styles
- [MDN: :focus-visible](https://developer.mozilla.org/en-US/docs/Web/CSS/:focus-visible)
- [What's New in WCAG 2.1: Focus Visible](https://www.w3.org/WAI/standards-guidelines/wcag/new-in-21/#focus-visible-enhanced-level-aaa)

### Testing
- [Playwright Keyboard API](https://playwright.dev/docs/api/class-keyboard)
- [Testing Library: User Event](https://testing-library.com/docs/user-event/intro/)

### Project Documentation
- [Web Accessibility Standards Rule](.propel/rules/web-accessibility-standards.md)
- [Playwright Testing Guide](../../../docs/E2E_TESTING.md)

## Build Commands

```bash
# Navigate to frontend directory
cd src/frontend

# Install dependencies
npm install

# Add focus-trap-react dependency
npm install focus-trap-react

# Run type checking
npm run typecheck

# Run linting
npm run lint

# Run unit tests
npm test

# Run accessibility tests only
npm test -- --grep="keyboard"

# Build for production
npm run build

# Development server (manual keyboard testing)
npm run dev

# Run E2E keyboard navigation tests
cd ../../test-automation
npm test -- keyboard-navigation.spec.ts
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] All focusable elements display visible 2px outline on keyboard focus
- [ ] Focus indicators meet 3:1 contrast on all backgrounds
- [ ] All workflows completable using keyboard only (no mouse)
- [ ] Tab order follows logical reading order on all pages
- [ ] Modal focus trap works correctly (Enter/Escape)
- [ ] Custom widgets support arrow key navigation
- [ ] E2E keyboard tests pass in Playwright

## Implementation Checklist
- [ ] Add global `:focus-visible` styles to `src/index.css` with 2px outline, offset, primary color
- [ ] Configure Tailwind focus ring utilities for consistent focus styles
- [ ] Audit tab order on all 26 screens using keyboard (Tab, Shift+Tab)
- [ ] Remove `tabindex` from non-interactive elements (headings, paragraphs, static divs)
- [ ] Add `tabindex="0"` to custom interactive elements not naturally focusable
- [ ] Convert all clickable divs/spans to `<button>` elements
- [ ] Add `onKeyDown` handlers for Enter/Space on remaining custom controls
- [ ] Implement Escape key handler to close modals, dropdowns, tooltips
- [ ] Create `useFocusTrap` custom hook for modal focus management
- [ ] Implement focus trap in SessionTimeoutModal using focus-trap-react or custom hook
- [ ] Restore focus to trigger element when modal closes
- [ ] Add `aria-hidden="true"` to background content when modal is open
- [ ] Implement arrow key navigation in TimeSlotGrid component (↑↓←→)
- [ ] Add arrow key support to Pagination component
- [ ] Implement arrow key navigation in any dropdown/combobox components
- [ ] Test "Skip to main content" link on all pages
- [ ] Complete appointment booking workflow using keyboard only (document steps)
- [ ] Complete form submissions using keyboard only (registration, login)
- [ ] Test navigation between all screens using keyboard
- [ ] Create E2E Playwright tests for critical keyboard workflows
- [ ] Create keyboard accessibility guide for developers (docs/KEYBOARD_ACCESSIBILITY.md)
