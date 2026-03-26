# Task - task_001_fe_mobile_layout_navigation

## Requirement Reference
- User Story: US_062 - Responsive Layout & Mobile Adaptation
- Story Location: .propel/context/tasks/EP-011-I/us_062/us_062.md
- Acceptance Criteria:
    - AC-1: **Given** mobile viewport ≥ 320px (UXR-301), **When** I access the app on a smartphone, **Then** the layout uses a single-column stack, navigation collapses to a hamburger menu, and all touch targets are ≥ 44×44px.
- Edge Case:
    - Viewport widths between 320px and 768px: Layout progressively enhances using Tailwind's `sm:` breakpoint (640px) as intermediate step.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all 26 wireframes with mobile views) |
| **Screen Spec** | figma_spec.md#responsive-strategy |
| **UXR Requirements** | UXR-301 (Mobile layout ≥ 320px), UXR-304 (Tailwind responsive breakpoints) |
| **Design Tokens** | designsystem.md#spacing, designsystem.md#breakpoints |

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
| Frontend | Tailwind CSS | Latest |
| Library | React Router | v6 |
| Testing | Vitest + React Testing Library | Latest |
| Tool | Playwright | Latest (for responsive testing) |

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

Implement mobile-first responsive layout for viewports ≥ 320px across all 26 screens. This task converts the desktop sidebar navigation to a hamburger menu, implements single-column stacking for content, ensures all touch targets meet the 44×44px minimum size requirement, and creates the BottomNav component for mobile navigation. The implementation uses Tailwind CSS breakpoints to progressively enhance from mobile to larger viewports.

## Dependent Tasks
- None (foundational responsive task)

## Impacted Components
- **Layout Components**: MainLayout, Sidebar, BottomNav (already exists), Header
- **Navigation**: All navigation elements requiring hamburger menu
- **Interactive Elements**: All buttons, links, form controls (touch target sizing)
- **All Pages**: Content stacking on mobile

## Implementation Plan

### 1. Configure Tailwind Breakpoints
- Review tailwind.config.js for breakpoint configuration
- Ensure standard breakpoints match design system:
  - `sm:` 640px (intermediate mobile-to-tablet)
  - `md:` 768px (tablet)
  - `lg:` 1024px (desktop)
  - `xl:` 1280px (large desktop)
- Reference designsystem.md for confirmed breakpoint values
- Document mobile-first approach: base styles = mobile, then enhance with breakpoint prefixes

### 2. Implement Hamburger Menu Component
- Create `HamburgerButton` component for menu toggle
- Use standard hamburger icon (three horizontal lines) SVG
- Ensure button meets 44×44px minimum size (or larger)
- Add accessible label: `aria-label="Menu"` or `aria-label="Close menu"`
- Add `aria-expanded` state (true when menu open, false when closed)
- Style with focus indicator for keyboard accessibility
- Position in top-left or top-right of header on mobile

### 3. Update Sidebar Component for Mobile
- Hide Sidebar on mobile by default (hidden md:block)
- When hamburger clicked, show Sidebar as fullscreen overlay
- Use fixed positioning with z-index above content
- Add backdrop/overlay behind Sidebar (semi-transparent)
- Close Sidebar when backdrop clicked
- Close Sidebar when navigation item selected
- Maintain existing desktop Sidebar behavior for md+ viewports
- Test smooth open/close animations (slide-in from left/right)

### 4. Update BottomNav Component (Already Exists)
- Review existing BottomNav implementation
- Ensure visible only on mobile (block md:hidden)
- Verify all nav items have 44×44px minimum touch targets
- Add active state indicator for current route
- Limit to 4-5 primary navigation items
- Style with appropriate spacing from designsystem.md
- Fixed position at bottom of viewport

### 5. Implement Single-Column Mobile Stacking
- Update MainLayout to stack content in single column on mobile
- Remove side padding on mobile for full-width content (px-4 instead of px-8)
- Use Tailwind classes: `flex flex-col` for mobile, `flex flex-row` for desktop
- Stack grid layouts: `grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3`
- Stack card layouts vertically on mobile
- Hide or collapse secondary content panels on mobile (show on tablet+)

### 6. Ensure Touch Target Sizing
- Audit all interactive elements (buttons, links, icons, form controls)
- Apply minimum 44×44px sizing via Tailwind: `min-h-[44px] min-w-[44px]`
- Alternative: Use padding to expand touch area: `p-3` (12px = 44px total for 20px icon)
- Update icon-only buttons with appropriate padding
- Test touch targets on actual mobile devices or browser dev tools
- Document touch target pattern for future components

### 7. Test Mobile Navigation Patterns
- Test hamburger menu open/close on mobile viewport (375px)
- Test navigation item selection closes menu
- Test backdrop click closes menu
- Test BottomNav navigation on mobile
- Test scroll behavior (content scrolls, navigation stays fixed)
- Test on multiple mobile viewport sizes (320px, 375px, 414px, 768px)

### 8. Document Mobile-First Approach
- Create developer guidelines for mobile-first responsive design
- Document Tailwind breakpoint usage patterns
- Document touch target sizing requirements
- Provide code examples for common responsive patterns

## Current Project State

```
src/frontend/src/
├── components/
│   └── layout/
│       ├── MainLayout.tsx (needs mobile stacking)
│       ├── Sidebar.tsx (needs hamburger integration)
│       ├── BottomNav.tsx (already exists, needs review)
│       └── Header.tsx (needs hamburger button)
├── pages/
│   └── (all pages need mobile stacking verification)
├── tailwind.config.js
└── __tests__/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/common/HamburgerButton.tsx | Hamburger menu toggle button |
| CREATE | src/hooks/useMobileMenu.ts | Mobile menu state management hook |
| MODIFY | src/components/layout/MainLayout.tsx | Add mobile hamburger menu and single-column stacking |
| MODIFY | src/components/layout/Sidebar.tsx | Add mobile overlay mode |
| MODIFY | src/components/layout/BottomNav.tsx | Verify touch targets and mobile-only visibility |
| MODIFY | src/components/layout/Header.tsx | Add hamburger button on mobile |
| MODIFY | tailwind.config.js | Verify breakpoint configuration |
| MODIFY | src/components/**/*.tsx | Update touch target sizing on interactive elements |
| CREATE | src/__tests__/components/layout/HamburgerButton.test.tsx | Hamburger button tests |
| CREATE | docs/MOBILE_FIRST_RESPONSIVE_DESIGN.md | Mobile-first design guidelines |

## External References

### WCAG Touch Target Standards
- [WCAG 2.2: Target Size (2.5.5)](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum.html)
- [WCAG 2.2: Target Size (Enhanced) (2.5.8)](https://www.w3.org/WAI/WCAG22/Understanding/target-size-enhanced.html)

### Mobile UX Patterns
- [MDN: Mobile UX Best Practices](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/App_design/UX_basics)
- [Android: Touch Target Guidelines](https://developer.android.com/develop/ui/views/touch-and-input/gestures/scale#thetouchtargetsize)
- [iOS Human Interface Guidelines: Touch Targets](https://developer.apple.com/design/human-interface-guidelines/inputs/touchscreen-gestures)

### Tailwind CSS Responsive Design
- [Tailwind CSS: Responsive Design](https://tailwindcss.com/docs/responsive-design)
- [Tailwind CSS: Breakpoint Configuration](https://tailwindcss.com/docs/breakpoints)
- [Tailwind CSS: Mobile-First](https://tailwindcss.com/docs/responsive-design#mobile-first)

### React & Menu Patterns
- [React useState for menu state](https://react.dev/reference/react/useState)
- [Hamburger Menu Accessibility](https://www.a11yproject.com/posts/hamburger-menu-alternative/)
- [ARIA: Expanded State](https://www.w3.org/TR/wai-aria-1.2/#aria-expanded)

### Testing
- [Playwright Viewport Emulation](https://playwright.dev/docs/emulation#viewport)
- [Testing Library: Responsive Testing](https://testing-library.com/docs/react-testing-library/intro/)

### Project Documentation
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [UI/UX Design Standards](.propel/rules/ui-ux-design-standards.md)
- [Design System](../../../.propel/context/docs/designsystem.md#breakpoints)

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

# Run responsive tests
npm test -- --grep="responsive"

# Build for production
npm run build

# Development server (test with browser responsive mode)
npm run dev

# Run E2E tests with mobile viewport
cd ../../test-automation
npx playwright test --project="Mobile Chrome"
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Hamburger menu toggles Sidebar on mobile viewports
- [ ] Sidebar displays as overlay with backdrop on mobile
- [ ] BottomNav visible only on mobile (hidden on tablet/desktop)
- [ ] All touch targets meet 44×44px minimum size
- [ ] Single-column stacking works on all pages at 320px viewport
- [ ] No horizontal scrolling at any viewport size
- [ ] Test on actual mobile devices or Chrome DevTools device emulation

## Implementation Checklist
- [ ] Review and document Tailwind breakpoint configuration in tailwind.config.js
- [ ] Create `HamburgerButton` component with 44×44px minimum size
- [ ] Add `aria-label` and `aria-expanded` to HamburgerButton
- [ ] Create `useMobileMenu` hook for menu state management
- [ ] Update MainLayout to show HamburgerButton on mobile (block md:hidden)
- [ ] Update Sidebar to hide by default on mobile (hidden md:block)
- [ ] Implement Sidebar overlay mode with fullscreen fixed positioning
- [ ] Add semi-transparent backdrop behind mobile Sidebar
- [ ] Close Sidebar on backdrop click
- [ ] Close Sidebar when navigation item selected
- [ ] Add slide-in animation for Sidebar open/close
- [ ] Review BottomNav component for mobile-only visibility (block md:hidden)
- [ ] Verify BottomNav items have 44×44px touch targets
- [ ] Update MainLayout for single-column stacking on mobile (flex flex-col md:flex-row)
- [ ] Update grid layouts across pages (grid-cols-1 md:grid-cols-2 lg:grid-cols-3)
- [ ] Audit all buttons and links for 44×44px minimum touch target size
- [ ] Update icon-only buttons with padding to expand touch area
- [ ] Test hamburger menu on 320px, 375px, 414px viewports
- [ ] Test BottomNav navigation on mobile
- [ ] Test no horizontal scrolling at 320px viewport on all pages
- [ ] Create developer documentation for mobile-first responsive patterns
