# Task - task_001_fe_tab_order_skip_links

## Requirement Reference
- User Story: US_060 - Keyboard Navigation & Focus Management
- Story Location: .propel/context/tasks/EP-011-I/us_060/us_060.md
- Acceptance Criteria:
    - AC-1: **Given** keyboard navigation (UXR-202), **When** I press Tab, **Then** focus moves in a logical reading order through all interactive elements (links, buttons, inputs, selects) and skip-to-content link is the first focusable element.
- Edge Case:
    - Disabled elements: Disabled elements are removed from the tab order; visual indication and `aria-disabled="true"` explain why the element is unavailable.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all 26 wireframes SCR-001 through SCR-026) |
| **Screen Spec** | figma_spec.md (All screens) |
| **UXR Requirements** | UXR-202 (Full keyboard operability) |
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

Implement logical tab order and skip navigation across all 26 screens to ensure keyboard-only users can navigate efficiently without unnecessary tab stops. This task establishes the foundation for keyboard accessibility by ensuring focus moves through interactive elements in reading order (top-to-bottom, left-to-right), skip-to-content links appear as the first focusable element on every page, and disabled elements are properly excluded from tab order with appropriate ARIA attributes.

## Dependent Tasks
- None (foundational keyboard navigation task)

## Impacted Components
- **Layout Components**: All page layouts requiring skip link
- **All Pages**: Tab order validation across all 26 screens
- **Form Components**: Disabled element handling
- **Navigation Components**: Tab order in sidebars, menus
- **Interactive Widgets**: Custom controls needing tabindex management

## Implementation Plan

### 1. Implement Skip-to-Content Links
- Create reusable `SkipLink` component styled to be visually hidden until focused
- Position skip link as first element in document (before header)
- Style to appear at top-left on keyboard focus with high z-index
- Target `id="main-content"` on main content region
- Add skip links to all 26 pages (already exists in Login/Register, extend to authenticated pages)
- Test skip link navigates focus directly to main content

### 2. Audit Tab Order Across All Screens
- Use keyboard to navigate all 26 screens (Tab, Shift+Tab)
- Document current tab order for each page
- Identify tab order issues:
  - Non-interactive elements in tab order (static divs, headings)
  - Unexpected tab order jumps (CSS layout not matching DOM order)
  - Missing interactive elements (should be focusable but aren't)
  - Redundant tab stops (multiple stops for single logical element)
- Create prioritized fix list

### 3. Fix Tab Order Issues
- Remove `tabindex` from non-interactive elements (headings, paragraphs, static divs)
- Fix DOM order to match visual order (avoid CSS tricks that reorder visually)
- Add `tabindex="0"` to custom interactive elements not naturally focusable (only if semantic HTML not possible)
- Convert clickable divs/spans to `<button>` elements (naturally focusable)
- Ensure form fields appear in logical order

### 4. Implement Disabled Element Handling
- Audit all form controls and buttons for disabled states
- Ensure disabled elements use `disabled` attribute (automatically removes from tab order)
- Add `aria-disabled="true"` to custom disabled controls (if using role="button")
- Provide visual styling for disabled state (reduced opacity, cursor: not-allowed)
- Add tooltip or helper text explaining why element is disabled (if contextually needed)
- Test disabled elements are not reachable via Tab key

### 5. Handle Special Tab Order Cases
- Cards/tiles: Ensure single tab stop per card (using button wrapper or link)
- Tables: Add `tabindex="-1"` to row wrappers for programmatic focus, individual cell actions are tabbable
- Accordion headers: Ensure tab order flows through all headers before content
- Tabs: Arrow key navigation (not Tab) between tabs (implement in task_003)
- Search results: Logical order through filters, results, pagination

### 6. Validate Tab Order with Automated Tests
- Create tab order test utility to simulate Tab keypresses
- Write tests for critical workflows:
  - Login form: email → password → login button → register link
  - Appointment booking: provider select → date → time → reason → submit
  - Document upload: file input → upload button → cancel
- Assert focus moves to expected next element
- Use `document.activeElement` to verify focus position

### 7. Test with Real Keyboard Navigation
- Complete all user workflows using keyboard only (no mouse)
- Verify skip link appears and works on all pages
- Ensure no unexpected tab stops or confusing tab order
- Test Shift+Tab to navigate backwards
- Document any remaining issues

### 8. Create Developer Documentation
- Document skip link usage pattern for future pages
- Provide guidelines for tab order best practices
- Add examples of proper tabindex usage (when -1, 0, positive values appropriate)
- Document disabled element patterns

## Current Project State

```
src/frontend/src/
├── components/
│   ├── layout/
│   │   ├── MainLayout.tsx (needs skip link integration)
│   │   └── Sidebar.tsx (tab order validation)
│   ├── common/
│   │   └── (various components needing tab order validation)
│   └── forms/
│       └── (disabled element handling)
├── pages/
│   ├── LoginPage.tsx (already has skip link)
│   ├── RegisterPage.tsx (already has skip link)
│   └── (all other pages need skip links)
└── __tests__/
    └── (tab order tests to be created)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/common/SkipLink.tsx | Reusable skip-to-content link component |
| MODIFY | src/components/layout/MainLayout.tsx | Add SkipLink as first child element |
| MODIFY | src/pages/**/*.tsx | Ensure main content has id="main-content" |
| MODIFY | src/components/**/*.tsx | Fix tab order issues (remove unnecessary tabindex, convert divs to buttons) |
| MODIFY | src/components/forms/**/*.tsx | Add proper disabled attribute handling |
| CREATE | src/__tests__/accessibility/tabOrder.test.tsx | Tab order test suite |
| CREATE | src/__tests__/utils/tabOrderHelper.ts | Tab simulation utility for tests |
| CREATE | docs/TAB_ORDER_GUIDELINES.md | Developer guidelines for tab order |

## External References

### WCAG 2.2 Standards
- [Keyboard (2.1.1)](https://www.w3.org/WAI/WCAG22/Understanding/keyboard.html)
- [Focus Order (2.4.3)](https://www.w3.org/WAI/WCAG22/Understanding/focus-order.html)
- [Bypass Blocks (2.4.1)](https://www.w3.org/WAI/WCAG22/Understanding/bypass-blocks.html)

### WAI-ARIA Practices
- [Developing a Keyboard Interface](https://www.w3.org/WAI/ARIA/apg/practices/keyboard-interface/)
- [Managing Focus](https://www.w3.org/WAI/ARIA/apg/practices/keyboard-interface/#kbd_focus_management)
- [Skip Navigation Links](https://webaim.org/techniques/skipnav/)

### HTML & Accessibility
- [MDN: tabindex](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/tabindex)
- [Using the tabindex attribute](https://www.a11yproject.com/posts/how-to-use-the-tabindex-attribute/)
- [Disabled Buttons](https://www.w3.org/WAI/ARIA/apg/practices/keyboard-interface/#kbd_disabled_controls)

### React & Testing
- [React Accessibility: Keyboard](https://react.dev/learn/accessibility#keyboard)
- [Testing Library: User Event](https://testing-library.com/docs/user-event/keyboard/)
- [Playwright Keyboard API](https://playwright.dev/docs/api/class-keyboard)

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

# Run tab order tests
npm test -- --grep="tab order"

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
- [ ] Skip-to-content link appears as first focusable element on all pages
- [ ] Tab order follows logical reading order on all 26 screens
- [ ] No non-interactive elements in tab order
- [ ] All interactive elements reachable via Tab key
- [ ] Disabled elements excluded from tab order with aria-disabled
- [ ] Keyboard-only navigation completes all critical workflows
- [ ] Tab order tests pass for all critical paths

## Implementation Checklist
- [ ] Create reusable `SkipLink` component with proper styling (visually hidden unless focused)
- [ ] Add SkipLink to MainLayout as first child element (before all other content)
- [ ] Ensure all pages have `<main id="main-content">` target for skip link
- [ ] Audit tab order on all 26 screens using keyboard (document findings)
- [ ] Remove `tabindex` from all non-interactive elements (headings, static divs, paragraphs)
- [ ] Fix DOM order to match visual order (avoid CSS reordering issues)
- [ ] Convert all clickable divs/spans to `<button>` elements
- [ ] Add `tabindex="0"` to custom interactive elements only when semantic HTML not possible
- [ ] Apply `disabled` attribute to all disabled form controls and buttons
- [ ] Add `aria-disabled="true"` to custom disabled controls with appropriate role
- [ ] Provide visual styling for disabled state (opacity, cursor)
- [ ] Test disabled elements are not in tab order
- [ ] Create tab order simulation utility for automated testing
- [ ] Write tab order tests for critical workflows (login, booking, upload)
- [ ] Test complete workflows using keyboard only (document any issues)
- [ ] Test skip link functionality on all pages
- [ ] Create developer documentation for tab order best practices
