# Task - task_001_fe_primary_navigation_header

## Requirement Reference
- User Story: US_063 - Navigation & Visual Hierarchy
- Story Location: .propel/context/tasks/EP-011-II/us_063/us_063.md
- Acceptance Criteria:
    - AC-1: **Given** primary navigation (UXR-001), **When** I view any page, **Then** the top navigation bar displays the app logo, main sections (Dashboard, Appointments, Documents, Profile), active state highlighting, and user avatar/menu — consistent across all pages.
- Edge Case:
    - What happens when the navigation has more items than fit the viewport? Overflow items collapse into a "More" dropdown menu on desktop and are included in the hamburger menu on mobile.
    - How is the active navigation state maintained during multi-step wizards? The parent section remains highlighted; a sub-navigation step indicator shows progress within the wizard.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-001-registration.html (header reference) |
| **Screen Spec** | figma_spec.md#navigation-patterns |
| **UXR Requirements** | UXR-001 (Consistent primary navigation), UXR-003 (Persistent role-based navigation) |
| **Design Tokens** | designsystem.md#header, designsystem.md#colors, designsystem.md#typography |

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
| Frontend | Redux Toolkit | Redux Toolkit 2.x |
| Library | React Router | v6 |
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

Implement a consistent primary navigation header component displayed across all authenticated pages. The header includes the app logo, main navigation sections (Dashboard, Appointments, Documents, Profile), active state highlighting based on current route, and a user avatar with dropdown menu. The component must support role-based navigation (Patient, Staff, Admin), handle overflow items with a "More" dropdown on desktop, and integrate with the existing Sidebar and BottomNav components. On mobile, the header displays the hamburger menu button (already implemented in US_062) and collapses main navigation into the mobile menu.

## Dependent Tasks
- None (foundational navigation task)

## Impacted Components
- **New Components**: Header, UserMenu, NavOverflowMenu
- **Existing Components**: MainLayout (needs header integration), Sidebar (already has active state logic), BottomNav (mobile navigation)
- **All Pages**: All authenticated pages will use the Header component

## Implementation Plan

### 1. Create Header Component Structure
- Create `src/components/layout/Header.tsx` component
- Fixed position at top of viewport: `fixed top-0 left-0 right-0 z-50`
- Height: 64px (from designsystem.md)
- Background: `bg-neutral-0` (white) with `border-b border-neutral-200`
- Three-section layout: Left (logo), Center (navigation - desktop only), Right (user avatar/menu)
- Use Flexbox for horizontal layout: `flex items-center justify-between`
- Add proper ARIA role: `role="banner"`

### 2. Implement Logo Section (Left)
- Logo icon: Colored square with "+" symbol (28px × 28px)
- Logo text: "PatientAccess" in h5 typography (16px, 600 weight)
- Link logo to dashboard route (role-specific: /dashboard for Patient, /staff/dashboard for Staff, /admin/dashboard for Admin)
- Responsive: Show logo icon + text on desktop, icon only on small screens
- Use design tokens: `text-primary-500` for icon, `text-neutral-900` for text

### 3. Implement Main Navigation Section (Center - Desktop Only)
- Horizontal navigation menu visible only on desktop: `hidden md:flex`
- Navigation items based on user role:
  - **Patient**: Dashboard, Appointments, Documents, Profile
  - **Staff**: Dashboard, Queue, Walk-in, Patients, Verification
  - **Admin**: Dashboard, Users, Audit, Settings
- Each nav item as link with hover and active states
- Active state: `text-primary-500 font-medium` with optional bottom border indicator
- Hover state: `text-primary-600` transition
- Use NavLink from react-router-dom for automatic active state handling
- Spacing between items: `gap-6` (24px)

### 4. Implement Overflow "More" Dropdown (Desktop)
- Detect viewport width and available space for navigation items
- When items exceed available width, show "More..." dropdown
- Dropdown triggered by button: `text-neutral-600 hover:text-neutral-900`
- Dropdown menu: Absolute positioned, `bg-neutral-0 border border-neutral-200 rounded-md shadow-lg`
- List remaining navigation items in dropdown
- Close dropdown on click outside (use useClickOutside hook)
- Dropdown z-index: `z-50`

### 5. Implement User Avatar and Menu (Right)
- User avatar displaying initials (e.g., "JD" for John Doe)
- Avatar size: 36px × 36px, `rounded-full`
- Avatar background: `bg-primary-100`, text: `text-primary-500`
- Click avatar to open dropdown menu
- Dropdown menu items:
  - User name and email (read-only)
  - Role badge (Patient/Staff/Admin)
  - Profile link
  - Settings link (if applicable)
  - Logout button (destructive style)
- Dropdown position: `absolute top-full right-0 mt-2`
- Dropdown styling: `bg-neutral-0 border border-neutral-200 rounded-md shadow-lg min-w-[200px]`

### 6. Integrate with Role-Based Access Control
- Read user role from AuthContext (useAuth hook)
- Filter navigation items based on role
- Dynamically generate navigation config per role
- Update MainLayout to pass role-specific navigation items to Header
- Ensure navigation items link to role-specific routes (e.g., /dashboard vs /staff/dashboard)

### 7. Handle Mobile Responsiveness
- On mobile (<768px): Hide center navigation section
- Show hamburger button on left (if not already present from US_062)
- User avatar menu remains on right
- Hamburger toggles Sidebar overlay (already implemented)
- Test header height remains consistent: 64px on all viewports

### 8. Implement Active State Highlighting
- Use React Router's `useLocation` hook to determine current route
- Highlight active navigation item with `text-primary-500 font-medium`
- For nested routes (e.g., /appointments/book), highlight parent "Appointments" item
- During multi-step wizards, keep parent section highlighted
- Add `aria-current="page"` attribute to active navigation item

### 9. Add Accessibility Features
- Semantic HTML: `<header role="banner">`, `<nav role="navigation">`
- Skip-to-content link at top (already implemented in US_059)
- Keyboard navigation: Tab through navigation items
- Focus indicators on all interactive elements (`:focus-visible`)
- Screen reader labels: `aria-label` for avatar button, dropdown menus
- Announce active page: `aria-current="page"` on active nav item

### 10. Test Header Consistency
- Verify header appears on all authenticated pages
- Test role-based navigation filtering (Patient, Staff, Admin)
- Test active state highlighting on route changes
- Test overflow "More" dropdown on narrow desktop viewports
- Test user avatar menu open/close behavior
- Test mobile hamburger integration (if applicable)
- Test logout functionality from user menu

## Current Project State

```
src/frontend/src/
├── components/
│   └── layout/
│       ├── MainLayout.tsx (needs header integration)
│       ├── Sidebar.tsx (has role-based navigation logic)
│       ├── BottomNav.tsx (mobile navigation)
│       └── Header.tsx (to be created)
├── hooks/
│   ├── useAuth.ts (provides role context)
│   └── useClickOutside.ts (to be created or verified)
└── pages/
    └── (all pages will use Header via MainLayout)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/layout/Header.tsx | Primary navigation header component |
| CREATE | src/components/layout/UserMenu.tsx | User avatar dropdown menu component |
| CREATE | src/components/layout/NavOverflowMenu.tsx | Overflow "More" dropdown for desktop |
| MODIFY | src/components/layout/MainLayout.tsx | Integrate Header component into layout |
| CREATE | src/hooks/useClickOutside.ts | Hook for detecting clicks outside dropdown (if not exists) |
| CREATE | src/types/navigation.types.ts | TypeScript types for navigation configuration |
| CREATE | src/config/navigationConfig.ts | Role-based navigation configuration |
| CREATE | src/__tests__/components/layout/Header.test.tsx | Header component tests |
| CREATE | docs/NAVIGATION_PATTERNS.md | Navigation implementation guidelines |

## External References

### React Router Documentation
- [NavLink Component](https://reactrouter.com/web/api/NavLink) - Active state handling
- [useLocation Hook](https://reactrouter.com/web/api/Hooks/uselocation) - Current route detection

### Navigation Patterns
- [WAI-ARIA: Navigation Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/navigation/)
- [MDN: Navigation Semantics](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Roles/navigation_role)

### Dropdown Menu Patterns
- [WAI-ARIA: Menu Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/menu/)
- [React: Handling Click Outside](https://stackoverflow.com/questions/32553158/detect-click-outside-react-component)

### Header Design Patterns
- [Smashing Magazine: Navigation Patterns](https://www.smashingmagazine.com/2017/11/comprehensive-guide-web-design/)
- [UX Planet: Header Best Practices](https://uxplanet.org/navigation-bar-best-practices-and-latest-trends-74d7dd81efb5)

### Tailwind CSS
- [Tailwind CSS: Fixed Positioning](https://tailwindcss.com/docs/position#fixed)
- [Tailwind CSS: Z-Index](https://tailwindcss.com/docs/z-index)
- [Tailwind CSS: Flexbox](https://tailwindcss.com/docs/flex)

### Testing
- [Testing Library: User Interactions](https://testing-library.com/docs/user-event/intro/)
- [Vitest: Testing React Components](https://vitest.dev/guide/)

### Project Documentation
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [UI/UX Design Standards](.propel/rules/ui-ux-design-standards.md)
- [Design System](../../../.propel/context/docs/designsystem.md#header)

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

# Run header component tests
npm test -- Header

# Build for production
npm run build

# Development server
npm run dev

# Run E2E tests
cd ../../test-automation
npx playwright test
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Header displays consistently across all authenticated pages
- [ ] Logo links to correct dashboard based on user role
- [ ] Main navigation items filtered by user role (Patient/Staff/Admin)
- [ ] Active state highlighting works on route changes
- [ ] Overflow "More" dropdown appears when navigation items exceed width
- [ ] User avatar menu opens/closes correctly
- [ ] Logout functionality works from user menu
- [ ] Mobile: Hamburger button visible, center navigation hidden
- [ ] Keyboard navigation works through all interactive elements
- [ ] Screen reader announces navigation structure correctly

## Implementation Checklist
- [ ] Create `Header` component at `src/components/layout/Header.tsx` with 64px height and fixed positioning
- [ ] Implement logo section (left) with icon + text, linking to role-specific dashboard
- [ ] Create role-based navigation configuration in `src/config/navigationConfig.ts` (Patient, Staff, Admin)
- [ ] Implement main navigation section (center) with horizontal nav items, visible only on desktop (hidden md:flex)
- [ ] Add active state logic using `useLocation` hook to highlight current route
- [ ] Add hover states for navigation items with `transition-colors` effect
- [ ] Create `NavOverflowMenu` component for overflow "More" dropdown on desktop
- [ ] Implement overflow detection logic to show "More" dropdown when items exceed available width
- [ ] Create `UserMenu` component with avatar (initials), dropdown menu (Profile, Settings, Logout)
- [ ] Position user menu dropdown: `absolute top-full right-0 mt-2` with `shadow-lg`
- [ ] Integrate Header into `MainLayout` component below skip-to-content link
- [ ] Add `role="banner"` to header element for accessibility
- [ ] Add `aria-current="page"` attribute to active navigation item
- [ ] Add keyboard support: Tab navigation, Enter/Space to activate, Escape to close dropdowns
- [ ] Create `useClickOutside` hook for dropdown close-on-outside-click behavior
- [ ] Test header on all authenticated pages (Dashboard, Appointments, Documents, Profile, etc.)
- [ ] Test role-based navigation filtering for Patient, Staff, Admin roles
- [ ] Test overflow "More" dropdown appears correctly on narrow desktop viewports
- [ ] Test mobile responsiveness: center nav hidden, hamburger visible (if applicable)
- [ ] Create navigation types in `src/types/navigation.types.ts` (NavigationItem, NavigationConfig)
- [ ] Document navigation patterns in `docs/NAVIGATION_PATTERNS.md`
