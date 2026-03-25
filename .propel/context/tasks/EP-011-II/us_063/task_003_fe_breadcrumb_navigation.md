# Task - task_003_fe_breadcrumb_navigation

## Requirement Reference
- User Story: US_063 - Navigation & Visual Hierarchy
- Story Location: .propel/context/tasks/EP-011-II/us_063/us_063.md
- Acceptance Criteria:
    - AC-3: **Given** breadcrumb navigation (UXR-003), **When** I navigate to a nested page (e.g., Appointment > Provider > Time Slot), **Then** a breadcrumb trail displays the path with clickable ancestors allowing quick navigation back.
- Edge Case:
    - None specific to this task

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-006-provider-browser.html (shows breadcrumb example) |
| **Screen Spec** | figma_spec.md#navigation-patterns |
| **UXR Requirements** | UXR-003 (Breadcrumb navigation for nested pages), UXR-001 (Max 3 clicks navigation) |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors |

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

Implement a breadcrumb navigation component that displays the hierarchical path for nested pages, allowing users to quickly navigate back to ancestor pages. The breadcrumb appears in the header below the logo (on desktop) or as a back button with parent label (on mobile). The component automatically generates breadcrumb trails based on the current route using React Router's location context, supports clickable ancestor links, and integrates with the Header component created in task_001. Breadcrumbs help users understand their location within the application hierarchy and provide a quick way to navigate back without using browser controls.

## Dependent Tasks
- task_001_fe_primary_navigation_header (breadcrumb integrates into Header component)

## Impacted Components
- **New Components**: Breadcrumb, BreadcrumbItem, useBreadcrumbs (custom hook)
- **Existing Components**: Header (needs breadcrumb integration)
- **All Nested Pages**: Appointment flow, Provider browsing, Document management, User management, Settings

## Implementation Plan

### 1. Create Breadcrumb Component Structure
- Create `src/components/common/Breadcrumb.tsx` component
- Accept props: `items` (array of breadcrumb items with label and path)
- Render horizontal list with separator between items
- Separator: `/` character or `›` symbol in `text-neutral-400`
- Container: `flex items-center gap-2 text-sm text-neutral-500`
- Add ARIA navigation landmark: `<nav aria-label="Breadcrumb">`
- Use semantic HTML: `<ol>` with `<li>` for each breadcrumb item

### 2. Implement BreadcrumbItem Component
- Create `BreadcrumbItem` sub-component
- Two types: Link (clickable ancestor) and Text (current page)
- **Link item**: React Router `<Link>` with hover effect
  - Base: `text-neutral-600 hover:text-primary-500 hover:underline`
  - Transition: `transition-colors duration-200`
  - Clickable: Navigate to ancestor path
- **Text item** (current page): Non-clickable text
  - Base: `text-neutral-800 font-medium`
  - ARIA: `aria-current="page"`
- Add keyboard support: Tab to focus, Enter/Space to navigate

### 3. Create useBreadcrumbs Custom Hook
- Create `src/hooks/useBreadcrumbs.ts` hook
- Use React Router's `useLocation` to get current path
- Parse path segments: `/appointments/book/provider` → ["appointments", "book", "provider"]
- Map segments to breadcrumb items with labels and paths:
  - Example: `/appointments` → {label: "Appointments", path: "/appointments"}
  - Example: `/appointments/book` → {label: "Book Appointment", path: "/appointments/book"}
- Maintain route-to-label mapping config:
  ```typescript
  const breadcrumbLabels: Record<string, string> = {
    dashboard: "Dashboard",
    appointments: "Appointments",
    book: "Book Appointment",
    providers: "Find Providers",
    documents: "Documents",
    profile: "Profile",
    // ... more mappings
  };
  ```
- Always include "Home" as first breadcrumb item
- Filter out dynamic segments (e.g., `:id`) and replace with actual resource name if available

### 4. Integrate Breadcrumb into Header Component
- Update `Header` component from task_001 to include Breadcrumb
- Position: Between logo and user menu (center section on desktop)
- Desktop: Display full breadcrumb trail horizontally
- Tablet: Display breadcrumb, truncate long labels with ellipsis
- Mobile: Show back arrow + parent label only (e.g., "← Appointments")
- Responsive classes:
  - Desktop: `hidden md:flex` for full breadcrumb
  - Mobile: `md:hidden flex` for back button
- Ensure breadcrumb doesn't overlap with navigation items

### 5. Create Mobile Back Button Variant
- Create `MobileBackButton` component for mobile breadcrumb
- Display: Left arrow icon + parent page label
- Example: "← Dashboard" when on Appointments page
- Styling: `flex items-center gap-2 text-primary-500`
- Click behavior: Navigate to parent route using React Router's `useNavigate`
- Show only when current route has a parent (not on dashboard/home)
- Hide on pages without parent context (login, registration)

### 6. Handle Dynamic Route Segments
- For routes with dynamic parameters (e.g., `/appointments/:id`), fetch resource name
- Examples:
  - `/appointments/123` → "Appointments / Appointment #123"
  - `/providers/456` → "Find Providers / Dr. Smith"
- Use React Query or state management to fetch resource labels
- Display skeleton/loading state while fetching resource name
- Fallback to generic label if fetch fails (e.g., "Appointment Details")

### 7. Configure Breadcrumb Visibility Rules
- Show breadcrumbs on all nested pages (depth > 1)
- Hide breadcrumbs on:
  - Dashboard/Home (root level)
  - Login/Registration pages (unauthenticated)
  - Full-screen modals or wizards (optional based on UX)
- Show breadcrumbs on:
  - Appointment booking flow (multi-step)
  - Provider browser and detail pages
  - Document upload and status pages
  - User management pages (Admin)
  - Settings pages
- Example paths requiring breadcrumbs:
  - `/appointments/book` → "Home / Appointments / Book Appointment"
  - `/providers/search/Dr-Smith` → "Home / Find Providers / Dr. Smith"
  - `/documents/upload` → "Home / Documents / Upload"

### 8. Add Accessibility Features
- Semantic HTML: `<nav aria-label="Breadcrumb">` with `<ol>` list
- Add `aria-current="page"` to last breadcrumb item (current page)
- Keyboard navigation: Tab through clickable breadcrumb links
- Focus indicators on breadcrumb links: `:focus-visible`
- Screen reader announcements: "Breadcrumb navigation" landmark
- Skip breadcrumb if using keyboard shortcuts (skip-to-content link already implemented)

### 9. Style Breadcrumb Component
- Typography: `text-sm` (14px) from design system body text
- Colors:
  - Links: `text-neutral-600 hover:text-primary-500`
  - Current page: `text-neutral-800 font-medium`
  - Separator: `text-neutral-400`
- Spacing: `gap-2` (8px) between items and separators
- Max width: Truncate long breadcrumb trails with ellipsis if > 4 items
- Mobile back button: Use design system icon or Unicode arrow `←`

### 10. Test Breadcrumb Navigation
- Verify breadcrumbs display on nested pages
- Test clickable ancestor links navigate correctly
- Test mobile back button navigates to parent page
- Test dynamic route segments fetch resource names
- Test breadcrumb truncation for long paths (> 4 levels)
- Test screen reader announces breadcrumb navigation
- Test keyboard navigation through breadcrumb links
- Verify breadcrumbs hidden on dashboard, login, registration pages

## Current Project State

```
src/frontend/src/
├── components/
│   ├── common/
│   │   ├── Breadcrumb.tsx (to be created)
│   │   └── BreadcrumbItem.tsx (to be created)
│   └── layout/
│       └── Header.tsx (needs breadcrumb integration)
├── hooks/
│   └── useBreadcrumbs.ts (to be created)
├── config/
│   └── breadcrumbLabels.ts (to be created)
└── pages/
    └── (all nested pages will show breadcrumbs)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/common/Breadcrumb.tsx | Breadcrumb navigation component |
| CREATE | src/components/common/BreadcrumbItem.tsx | Individual breadcrumb item component |
| CREATE | src/components/common/MobileBackButton.tsx | Mobile back button variant |
| CREATE | src/hooks/useBreadcrumbs.ts | Custom hook to generate breadcrumb items from route |
| CREATE | src/config/breadcrumbLabels.ts | Route-to-label mapping configuration |
| MODIFY | src/components/layout/Header.tsx | Integrate breadcrumb between logo and user menu |
| CREATE | src/__tests__/components/common/Breadcrumb.test.tsx | Breadcrumb component tests |
| CREATE | src/__tests__/hooks/useBreadcrumbs.test.ts | useBreadcrumbs hook tests |
| CREATE | docs/BREADCRUMB_USAGE.md | Breadcrumb usage guidelines |

## External References

### Breadcrumb Design Patterns
- [WAI-ARIA: Breadcrumb Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/breadcrumb/)
- [Nielsen Norman Group: Breadcrumbs](https://www.nngroup.com/articles/breadcrumbs/)
- [Smashing Magazine: Breadcrumb Navigation](https://www.smashingmagazine.com/2009/03/breadcrumbs-in-web-design-examples-and-best-practices/)

### React Router
- [React Router: useLocation](https://reactrouter.com/web/api/Hooks/uselocation)
- [React Router: Link Component](https://reactrouter.com/web/api/Link)
- [React Router: useNavigate](https://reactrouter.com/en/main/hooks/use-navigate)

### Accessibility
- [MDN: Breadcrumb Navigation](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/nav)
- [WebAIM: Breadcrumb Navigation](https://webaim.org/techniques/breadcrumbs/)
- [A11y: aria-current](https://www.w3.org/TR/wai-aria-1.2/#aria-current)

### Tailwind CSS
- [Tailwind CSS: Flexbox](https://tailwindcss.com/docs/flex)
- [Tailwind CSS: Typography](https://tailwindcss.com/docs/font-size)
- [Tailwind CSS: Gap](https://tailwindcss.com/docs/gap)

### Testing
- [Testing Library: Navigation Testing](https://testing-library.com/docs/example-react-router/)
- [Vitest: Hook Testing](https://vitest.dev/guide/)

### Project Documentation
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [UI/UX Design Standards](.propel/rules/ui-ux-design-standards.md)
- [Design System](../../../.propel/context/docs/designsystem.md#navigation)

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

# Run breadcrumb tests
npm test -- Breadcrumb useBreadcrumbs

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
- [ ] Breadcrumbs display on nested pages (appointments, providers, documents, settings)
- [ ] Breadcrumb links navigate to ancestor pages correctly
- [ ] Current page (last breadcrumb item) is non-clickable and has `aria-current="page"`
- [ ] Mobile back button displays parent page label and navigates correctly
- [ ] Dynamic route segments fetch and display resource names
- [ ] Breadcrumbs hidden on dashboard, login, registration pages
- [ ] Breadcrumb separators display correctly between items
- [ ] Screen reader announces breadcrumb navigation landmark
- [ ] Keyboard navigation works through breadcrumb links

## Implementation Checklist
- [ ] Create `Breadcrumb` component at `src/components/common/Breadcrumb.tsx` with `<nav aria-label="Breadcrumb">`
- [ ] Use semantic HTML: `<ol>` with `<li>` for breadcrumb items
- [ ] Create `BreadcrumbItem` component with link (clickable) and text (current page) variants
- [ ] Link item styles: `text-neutral-600 hover:text-primary-500 hover:underline transition-colors`
- [ ] Current page styles: `text-neutral-800 font-medium` with `aria-current="page"`
- [ ] Add breadcrumb separator: `/` or `›` in `text-neutral-400` between items
- [ ] Create `useBreadcrumbs` hook using `useLocation` to parse current path
- [ ] Map path segments to breadcrumb items with labels and paths
- [ ] Create `breadcrumbLabels` config mapping routes to display labels
- [ ] Always include "Home" as first breadcrumb item
- [ ] Create `MobileBackButton` component with left arrow icon + parent label
- [ ] Mobile back button styles: `flex items-center gap-2 text-primary-500`
- [ ] Integrate breadcrumb into `Header` component between logo and user menu
- [ ] Desktop breadcrumb: `hidden md:flex` full breadcrumb trail
- [ ] Mobile breadcrumb: `md:hidden flex` back button only
- [ ] Handle dynamic route segments: fetch resource names for `:id` parameters
- [ ] Hide breadcrumbs on dashboard, login, registration pages
- [ ] Add keyboard support: Tab to focus breadcrumb links, Enter/Space to navigate
- [ ] Add focus indicators: `:focus-visible` outline on breadcrumb links
- [ ] Test breadcrumbs on appointment booking flow, provider browser, document pages
- [ ] Document breadcrumb usage guidelines in `docs/BREADCRUMB_USAGE.md`
