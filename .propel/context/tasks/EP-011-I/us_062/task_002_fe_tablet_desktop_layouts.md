# Task - task_002_fe_tablet_desktop_layouts

## Requirement Reference
- User Story: US_062 - Responsive Layout & Mobile Adaptation
- Story Location: .propel/context/tasks/EP-011-I/us_062/us_062.md
- Acceptance Criteria:
    - AC-2: **Given** tablet viewport ≥ 768px (UXR-302), **When** I access the app on a tablet, **Then** the layout uses a two-column grid where appropriate (e.g., sidebar navigation + content), and spacing increases for comfortable touch interaction.
    - AC-3: **Given** desktop viewport ≥ 1024px (UXR-303), **When** I access the app on a desktop, **Then** the layout uses the full multi-column design with sidebar, content area, and optional context panels.
- Edge Case:
    - None specific to this task

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all 26 wireframes with desktop/tablet views) |
| **Screen Spec** | figma_spec.md#responsive-strategy |
| **UXR Requirements** | UXR-302 (Tablet layout ≥ 768px), UXR-303 (Desktop layout ≥ 1024px), UXR-304 (Tailwind responsive breakpoints) |
| **Design Tokens** | designsystem.md#spacing, designsystem.md#grid-system |

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

Implement tablet and desktop responsive layouts for viewports ≥ 768px across all 26 screens. This task implements two-column layouts for tablet (sidebar + content) with increased spacing, and full multi-column designs for desktop (sidebar + content + optional context panels). The implementation uses Tailwind CSS grid and flexbox utilities with breakpoint prefixes to progressively enhance from tablet to desktop viewports.

## Dependent Tasks
- task_001_fe_mobile_layout_navigation (provides mobile-first foundation)

## Impacted Components
- **Layout Components**: MainLayout, Sidebar, ContentArea
- **Grid Layouts**: Dashboard stats, appointment lists, provider cards
- **Page Layouts**: All pages requiring multi-column layouts
- **Context Panels**: Optional side panels for additional information

## Implementation Plan

### 1. Implement Tablet Two-Column Layout
- Update MainLayout to show Sidebar on tablet (hidden md:block)
- Set Sidebar width for tablet: `md:w-64` (256px) or as per designsystem.md
- Position Sidebar: fixed or sticky on left side
- Hide BottomNav on tablet and above (block md:hidden)
- Increase content padding on tablet: `px-4 md:px-6 lg:px-8`
- Increase spacing between elements on tablet: `space-y-4 md:space-y-6`
- Use two-column grids where appropriate: `grid grid-cols-1 md:grid-cols-2`

### 2. Implement Desktop Multi-Column Layout
- Increase Sidebar width on desktop if needed: `md:w-64 lg:w-72`
- Implement three-column grids: `grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3`
- Implement four-column grids for dense content: `grid-cols-1 md:grid-cols-2 lg:grid-cols-4`
- Add optional context panels on desktop (e.g., recent activity, quick actions)
- Increase max content width on desktop: `max-w-7xl` or similar
- Increase content padding on desktop: `px-6 lg:px-8`
- Increase spacing on desktop: `space-y-6 lg:space-y-8`

### 3. Update Dashboard Page Layouts
- Patient Dashboard: Stat cards in 2-column (tablet) / 3-column (desktop) grid
- Upcoming appointments: List on mobile/tablet, cards on desktop
- Add optional sidebar widget (recent notifications, quick links) on desktop
- Ensure consistent spacing using designsystem.md tokens

### 4. Update List and Card Layouts
- Provider cards: 1 col (mobile), 2 col (tablet), 3 col (desktop)
- Appointment cards: 1 col (mobile), 2 col (tablet), 2 col (desktop)
- Document cards: 1 col (mobile), 2 col (tablet), 3 col (desktop)
- Use Tailwind grid utilities: `grid gap-4 md:gap-6 lg:gap-8`

### 5. Update Form Layouts
- Multi-column forms on tablet/desktop: `grid grid-cols-1 md:grid-cols-2`
- Keep single-column for complex forms (intake forms)
- Group related fields using fieldsets in side-by-side columns
- Increase form spacing on larger viewports

### 6. Implement Context Panels (Optional)
- Create `ContextPanel` component for desktop-only content
- Position on right side of main content area
- Hide on mobile/tablet: `hidden lg:block`
- Width: `lg:w-80` or `lg:w-96`
- Examples: Recent activity feed, quick stats, help tips
- Use sparingly to avoid cluttering interface

### 7. Test Tablet and Desktop Layouts
- Test at 768px (tablet minimum)
- Test at 1024px (desktop minimum)
- Test at 1280px, 1440px, 1920px (common desktop sizes)
- Verify Sidebar visible and properly sized
- Verify grid columns display correctly
- Verify spacing increases appropriately
- Verify no layout shifts when resizing

### 8. Document Grid and Layout Patterns
- Create examples of common grid patterns
- Document when to use 2-col, 3-col, 4-col grids
- Document context panel usage guidelines
- Provide code examples for responsive layouts

## Current Project State

```
src/frontend/src/
├── components/
│   └── layout/
│       ├── MainLayout.tsx (needs tablet/desktop grids)
│       ├── Sidebar.tsx (needs tablet/desktop sizing)
│       └── ContextPanel.tsx (to be created)
├── pages/
│   ├── PatientDashboard.tsx (needs multi-column stats)
│   ├── AppointmentBooking.tsx (needs layout updates)
│   └── (other pages need grid updates)
└── __tests__/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/components/layout/MainLayout.tsx | Implement tablet/desktop multi-column layouts |
| MODIFY | src/components/layout/Sidebar.tsx | Adjust sizing for tablet/desktop viewports |
| CREATE | src/components/layout/ContextPanel.tsx | Optional desktop-only context panel component |
| MODIFY | src/pages/PatientDashboard.tsx | Update stat cards to responsive grid |
| MODIFY | src/pages/ProviderBrowser.tsx | Update provider cards to responsive grid |
| MODIFY | src/pages/AppointmentBooking.tsx | Update form layout for tablet/desktop |
| MODIFY | src/components/**/*.tsx | Update grid layouts across components |
| CREATE | src/__tests__/components/layout/ResponsiveLayout.test.tsx | Responsive layout tests |
| CREATE | docs/RESPONSIVE_GRID_PATTERNS.md | Grid layout guidelines |

## External References

### Responsive Design Principles
- [MDN: Responsive Design](https://developer.mozilla.org/en-US/docs/Learn/CSS/CSS_layout/Responsive_Design)
- [Smashing Magazine: Responsive Web Design](https://www.smashingmagazine.com/2011/01/guidelines-for-responsive-web-design/)

### CSS Grid and Flexbox
- [MDN: CSS Grid Layout](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_grid_layout)
- [CSS-Tricks: A Complete Guide to Grid](https://css-tricks.com/snippets/css/complete-guide-grid/)
- [CSS-Tricks: A Complete Guide to Flexbox](https://css-tricks.com/snippets/css/a-guide-to-flexbox/)

### Tailwind CSS Grid System
- [Tailwind CSS: Grid Template Columns](https://tailwindcss.com/docs/grid-template-columns)
- [Tailwind CSS: Gap](https://tailwindcss.com/docs/gap)
- [Tailwind CSS: Flexbox](https://tailwindcss.com/docs/flex)
- [Tailwind CSS: Container](https://tailwindcss.com/docs/container)

### Responsive Layout Patterns
- [Responsive Layout Patterns](https://web.dev/patterns/layout/)
- [Every Layout](https://every-layout.dev/) (responsive layout primitives)

### Testing
- [Playwright Viewport Emulation](https://playwright.dev/docs/emulation#viewport)
- [Testing Responsive Components](https://kentcdodds.com/blog/how-to-test-responsive-components)

### Project Documentation
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [UI/UX Design Standards](.propel/rules/ui-ux-design-standards.md)
- [Design System](../../../.propel/context/docs/designsystem.md#grid-system)

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

# Run responsive layout tests
npm test -- --grep="responsive"

# Build for production
npm run build

# Development server (test with browser responsive mode)
npm run dev

# Run E2E tests at different viewports
cd ../../test-automation
npx playwright test --project="Desktop Chrome"
npx playwright test --project="Tablet Chrome"
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Sidebar visible on tablet (768px) and desktop (1024px+)
- [ ] BottomNav hidden on tablet and desktop
- [ ] Two-column grids display correctly on tablet
- [ ] Three-column grids display correctly on desktop
- [ ] Spacing increases appropriately across breakpoints
- [ ] Context panels display on desktop only (if implemented)
- [ ] No layout shifts when resizing between breakpoints
- [ ] Test at 768px, 1024px, 1280px, 1440px, 1920px

## Implementation Checklist
- [ ] Update MainLayout to show Sidebar on tablet and desktop (hidden md:block)
- [ ] Set Sidebar width for tablet and desktop (md:w-64 lg:w-72 or per designsystem.md)
- [ ] Hide BottomNav on tablet and desktop (block md:hidden)
- [ ] Increase content padding across breakpoints (px-4 md:px-6 lg:px-8)
- [ ] Increase spacing between elements (space-y-4 md:space-y-6 lg:space-y-8)
- [ ] Update Patient Dashboard stat cards to responsive grid (grid-cols-1 md:grid-cols-2 lg:grid-cols-3)
- [ ] Update provider cards to responsive grid (grid-cols-1 md:grid-cols-2 lg:grid-cols-3)
- [ ] Update appointment cards to responsive grid (grid-cols-1 md:grid-cols-2)
- [ ] Update document cards to responsive grid (grid-cols-1 md:grid-cols-2 lg:grid-cols-3)
- [ ] Update form layouts for multi-column on tablet/desktop (grid-cols-1 md:grid-cols-2)
- [ ] Create ContextPanel component for desktop-only content (hidden lg:block)
- [ ] Add optional context panels to appropriate screens (dashboard, profiles)
- [ ] Test layout at 768px (tablet) - verify two-column layouts
- [ ] Test layout at 1024px (desktop) - verify multi-column layouts
- [ ] Test layout at 1280px, 1440px, 1920px - verify scaling
- [ ] Verify no layout shifts when resizing browser
- [ ] Verify consistent spacing using gap utilities (gap-4 md:gap-6 lg:gap-8)
- [ ] Create developer documentation for responsive grid patterns
- [ ] Document context panel usage guidelines
