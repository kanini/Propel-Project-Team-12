# Task - task_003_fe_keyboard_patterns_workflows

## Requirement Reference
- User Story: US_060 - Keyboard Navigation & Focus Management
- Story Location: .propel/context/tasks/EP-011-I/us_060/us_060.md
- Acceptance Criteria:
    - AC-3: **Given** dropdown menus and custom components, **When** I interact via keyboard, **Then** Arrow keys navigate options, Enter/Space activates, and Escape dismisses, following WAI-ARIA combobox/listbox patterns.
    - AC-4: **Given** multi-step workflows (booking, intake), **When** I advance to the next step, **Then** focus moves to the first interactive element of the new step with an ARIA live announcement of the step title.
- Edge Case:
    - Dynamic validation errors: Focus shifts to the first error message and an ARIA alert announces the error count.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-007-appointment-booking.html (multi-step), wireframe-SCR-012-ai-intake.html (workflow), wireframe-SCR-006-provider-browser.html (filters) |
| **Screen Spec** | figma_spec.md#SCR-007, figma_spec.md#SCR-012 |
| **UXR Requirements** | UXR-202 (Full keyboard operability), UXR-205 (Focus management), UXR-207 (ARIA live regions) |
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
| Library | React Router | v6 |
| Testing | Vitest + React Testing Library | Latest |
| Tool | Playwright | Latest (for E2E workflow testing) |

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

Implement comprehensive keyboard interaction patterns for custom components (dropdowns, filters, selects) and multi-step workflows (appointment booking, patient intake). This task ensures all interactive widgets follow WAI-ARIA patterns with Arrow key navigation, Enter/Space activation, and Escape dismissal. Additionally, it implements focus management for workflow transitions with ARIA live announcements and dynamic error handling that shifts focus to validation messages.

## Dependent Tasks
- task_001_fe_tab_order_skip_links (establishes baseline tab order)
- task_002_fe_modal_focus_trap (provides modal focus patterns)

## Impacted Components
- **Dropdown Components**: Provider filters, specialty filters, date selectors
- **Custom Select Components**: Time slot selection, form selects
- **Multi-step Workflows**: AppointmentBooking, AI/Manual Intake pages
- **Progress Indicators**: ProgressIndicator component
- **Form Components**: Validation error handling
- **ARIA Live Regions**: LiveRegion component for announcements

## Implementation Plan

### 1. Implement Dropdown Keyboard Pattern (Combobox/Listbox)
- Identify dropdown components needing keyboard support (provider filters, date pickers)
- Implement WAI-ARIA Combobox pattern:
  - Add `role="combobox"`, `aria-expanded`, `aria-controls`, `aria-activedescendant`
  - Add `role="listbox"` to dropdown menu
  - Add `role="option"` to each menu item
- Add keyboard event handlers:
  - Enter/Space: Open dropdown or select focused option
  - Arrow Down: Move focus to next option (wrap at end)
  - Arrow Up: Move focus to previous option (wrap at start)
  - Home: Focus first option
  - End: Focus last option
  - Escape: Close dropdown and return focus to trigger
  - Type-ahead: Jump to option starting with typed character (optional)
- Use `aria-activedescendant` to indicate focused option
- Update visual focus styles to match keyboard focus

### 2. Implement Time Slot Grid Keyboard Navigation
- Update TimeSlotGrid component with Arrow key navigation
- Implement grid navigation pattern:
  - Arrow Right: Move to next time slot
  - Arrow Left: Move to previous time slot
  - Arrow Down: Move to slot in same column, next row
  - Arrow Up: Move to slot in same column, previous row
  - Home: Focus first slot in current row
  - End: Focus last slot in current row
  - Page Down: Jump one week forward (if applicable)
  - Page Up: Jump one week backward (if applicable)
- Add `role="grid"` to container, `role="row"` to rows, `role="gridcell"` to slots
- Use `tabindex="-1"` on all slots except first, manage focus via JavaScript
- Update selected slot state and announce via ARIA live region

### 3. Implement Multi-Step Workflow Focus Management
- Update AppointmentBooking page (4 steps: Provider → Date/Time → Details → Confirm)
- Update AI/Manual Intake pages (multi-step forms)
- On step transition:
  - Store current step in state
  - Render next step content
  - Focus first interactive element of new step using `useEffect` + ref
  - Announce step change via ARIA live region (`role="status"`)
- Create reusable `useWorkflowFocus` hook:
  - Accepts: currentStep, stepRefs
  - Effect: Focus appropriate element when step changes
  - Returns: ref to attach to first focusable element
- Add ARIA live announcement: "Step [N] of [Total]: [Step Title]"

### 4. Create ARIA Live Region Component
- Create `LiveRegion` component for announcements
- Support `role="status"` (polite) and `role="alert"` (assertive)
- Accept `message` prop, announce when message changes
- Use `aria-live="polite"` for step changes, search results, success messages
- Use `aria-live="assertive"` for errors, critical warnings
- Visually hide component (sr-only class) but keep in DOM
- Implement debounce to avoid rapid successive announcements

### 5. Implement Dynamic Error Focus Management
- On form validation failure:
  - Collect all validation errors
  - Focus first field with error using ref
  - Announce error count via ARIA alert: "[N] errors found. First error: [message]"
  - Add `aria-invalid="true"` to fields with errors
  - Link error message to field via `aria-describedby`
- Create reusable `useFormErrorFocus` hook:
  - Accepts: errors object, fieldRefs map
  - Effect: Focus first error field when errors change
  - Announces error summary
- Update all major forms: Login, Register, Appointment booking, Manual intake

### 6. Implement Pagination Keyboard Support
- Update Pagination component (used in appointment lists, document lists)
- Add keyboard support:
  - Arrow Left: Previous page
  - Arrow Right: Next page
  - Home: First page
  - End: Last page
- Add `aria-label` to pagination: "Pagination navigation"
- Add `aria-current="page"` to current page button
- Announce page change via live region: "Page [N] of [Total]"

### 7. Test Custom Component Keyboard Patterns
- Test dropdown navigation with Arrow keys, Enter, Escape
- Test time slot grid navigation with Arrow keys in all directions
- Test pagination Arrow key navigation
- Test workflow step transitions with focus and announcements
- Test form error focus management
- Create E2E tests for appointment booking keyboard flow

### 8. Document Keyboard Patterns
- Create component-specific keyboard documentation
- Document patterns for each custom widget type
- Provide code examples for future components
- Add keyboard shortcut reference for users (optional)

## Current Project State

```
src/frontend/src/
├── components/
│   ├── appointments/
│   │   ├── TimeSlotGrid.tsx (needs arrow key navigation)
│   │   └── ProgressIndicator.tsx (workflow step indicator)
│   ├── providers/
│   │   └── ProviderFilters.tsx (needs dropdown keyboard support)
│   ├── common/
│   │   ├── Pagination.tsx (needs arrow key support)
│   │   └── LiveRegion.tsx (to be created)
│   └── forms/
│       └── (form validation needs error focus)
├── pages/
│   ├── AppointmentBooking.tsx (4-step workflow needs focus management)
│   └── (AI/Manual intake pages need focus management)
├── hooks/
│   ├── useWorkflowFocus.ts (to be created)
│   └── useFormErrorFocus.ts (to be created)
└── __tests__/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/common/LiveRegion.tsx | ARIA live region component for announcements |
| CREATE | src/hooks/useWorkflowFocus.ts | Focus management hook for multi-step workflows |
| CREATE | src/hooks/useFormErrorFocus.ts | Error focus management hook for forms |
| MODIFY | src/components/appointments/TimeSlotGrid.tsx | Add arrow key grid navigation |
| MODIFY | src/components/providers/ProviderFilters.tsx | Add dropdown keyboard support |
| MODIFY | src/components/common/Pagination.tsx | Add arrow key pagination navigation |
| MODIFY | src/pages/AppointmentBooking.tsx | Add workflow focus management with announcements |
| MODIFY | src/features/auth/pages/LoginPage.tsx | Add error focus management |
| MODIFY | src/features/auth/pages/RegisterPage.tsx | Add error focus management |
| CREATE | src/__tests__/components/common/LiveRegion.test.tsx | Live region test suite |
| CREATE | src/__tests__/hooks/useWorkflowFocus.test.ts | Workflow focus hook tests |
| CREATE | test-automation/tests/appointment-booking-keyboard.spec.ts | E2E keyboard workflow test |
| CREATE | docs/CUSTOM_COMPONENT_KEYBOARD_PATTERNS.md | Component keyboard patterns guide |

## External References

### WAI-ARIA Widget Patterns
- [Combobox Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/combobox/)
- [Listbox Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/listbox/)
- [Grid Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/grid/)
- [Menu Button Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/menu-button/)

### ARIA Live Regions
- [ARIA Live Regions](https://www.w3.org/WAI/ARIA/apg/practices/names-and-descriptions/)
- [Status Role](https://www.w3.org/TR/wai-aria-1.2/#status)
- [Alert Role](https://www.w3.org/TR/wai-aria-1.2/#alert)
- [Live Region Best Practices](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions)

### WCAG Standards
- [Keyboard (2.1.1)](https://www.w3.org/WAI/WCAG22/Understanding/keyboard.html)
- [Focus Order (2.4.3)](https://www.w3.org/WAI/WCAG22/Understanding/focus-order.html)
- [Status Messages (4.1.3)](https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html)

### React Focus Management
- [React useRef](https://react.dev/reference/react/useRef)
- [React useEffect](https://react.dev/reference/react/useEffect)
- [Managing Focus in React](https://react.dev/learn/manipulating-the-dom-with-refs#managing-focus-with-react)

### Testing
- [Testing Library: User Event Keyboard](https://testing-library.com/docs/user-event/keyboard/)
- [Playwright Keyboard Navigation](https://playwright.dev/docs/api/class-keyboard)

### Project Documentation
- [Web Accessibility Standards Rule](.propel/rules/web-accessibility-standards.md)
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)

## Build Commands

```bash
# Navigate to frontend directory
cd src/frontend

# Install dependencies
npm install

# Run type checking
npm run typecheck

# Run linting
npm run lint

# Run unit tests
npm test

# Run keyboard pattern tests
npm test -- --grep="keyboard"

# Build for production
npm run build

# Development server (manual testing)
npm run dev

# Run E2E keyboard workflow tests
cd ../../test-automation
npm test -- appointment-booking-keyboard.spec.ts
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Dropdown navigation works with Arrow keys, Enter, Escape
- [ ] Time slot grid navigation works with Arrow keys in all directions
- [ ] Pagination navigation works with Arrow Left/Right, Home/End
- [ ] Workflow step transitions focus first element with ARIA announcement
- [ ] Form validation errors focus first error field with announcement
- [ ] ARIA live regions announce changes appropriately (tested with screen reader)
- [ ] E2E appointment booking workflow completable via keyboard only

## Implementation Checklist
- [ ] Create `LiveRegion` component with role="status" and role="alert" support
- [ ] Implement visually hidden styling for LiveRegion (sr-only class)
- [ ] Add debounce logic to LiveRegion to prevent rapid announcements
- [ ] Identify all dropdown components requiring keyboard support (list them)
- [ ] Implement combobox keyboard pattern with role attributes and event handlers
- [ ] Add Arrow Down, Arrow Up, Home, End, Escape handlers to dropdowns
- [ ] Update TimeSlotGrid with role="grid" and gridcell structure
- [ ] Implement Arrow key navigation for TimeSlotGrid (all 4 directions, Home, End)
- [ ] Use tabindex="-1" on grid cells and manage focus programmatically
- [ ] Create `useWorkflowFocus` hook accepting currentStep and stepRefs
- [ ] Update AppointmentBooking to use useWorkflowFocus and announce step changes
- [ ] Update AI/Manual Intake pages with workflow focus management
- [ ] Add ARIA live announcement for step transitions: "Step [N] of [Total]: [Title]"
- [ ] Create `useFormErrorFocus` hook for form validation error handling
- [ ] Update LoginPage with error focus management
- [ ] Update RegisterPage with error focus management
- [ ] Announce error count via ARIA alert: "[N] errors found. First error: [message]"
- [ ] Update Pagination component with Arrow key navigation
- [ ] Add aria-label="Pagination navigation" and aria-current="page"
- [ ] Test all custom component keyboard patterns manually
- [ ] Create E2E test for appointment booking keyboard workflow
- [ ] Test with screen reader to verify ARIA announcements (NVDA/VoiceOver)
- [ ] Create developer documentation for custom component keyboard patterns
