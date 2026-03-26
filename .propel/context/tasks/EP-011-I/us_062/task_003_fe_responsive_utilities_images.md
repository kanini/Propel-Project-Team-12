# Task - task_003_fe_responsive_utilities_images

## Requirement Reference
- User Story: US_062 - Responsive Layout & Mobile Adaptation
- Story Location: .propel/context/tasks/EP-011-I/us_062/us_062.md
- Acceptance Criteria:
    - AC-4: **Given** Tailwind responsive breakpoints (UXR-304), **When** the viewport transitions between breakpoints, **Then** transitions are smooth via CSS media queries (no JavaScript-driven layout shifts), images use responsive `srcset`, and no horizontal scrolling occurs.
- Edge Case:
    - Data-heavy tables on mobile: Tables convert to stacked card layouts on mobile with key data visible and "View Details" expansion for secondary fields.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all 26 wireframes) |
| **Screen Spec** | figma_spec.md#responsive-strategy |
| **UXR Requirements** | UXR-304 (Tailwind responsive breakpoints), UXR-301 (Mobile adaptations) |
| **Design Tokens** | designsystem.md#images, designsystem.md#tables |

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

Implement responsive utilities, optimized images with srcset, and adaptive table layouts for mobile viewports. This task ensures smooth CSS-driven transitions between breakpoints without JavaScript layout shifts, implements responsive image loading with appropriate sizes for each viewport, prevents horizontal scrolling across all viewport sizes, and converts data-heavy tables to stacked card layouts on mobile devices.

## Dependent Tasks
- task_001_fe_mobile_layout_navigation (provides mobile layout foundation)
- task_002_fe_tablet_desktop_layouts (provides grid systems)

## Impacted Components
- **Image Components**: All image elements across the application
- **Table Components**: Appointment tables, document tables, audit logs
- **Utility Components**: Container, MaxWidth wrappers
- **All Pages**: Horizontal scroll prevention

## Implementation Plan

### 1. Implement Responsive Image Component
- Create `ResponsiveImage` component using HTML `<img>` with `srcset` and `sizes` attributes
- Accept props: `src`, `alt`, `srcset`, `sizes`, `className`
- Example srcset: `image-320w.jpg 320w, image-640w.jpg 640w, image-1024w.jpg 1024w`
- Example sizes: `(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw`
- Add lazy loading: `loading="lazy"` for images below the fold
- Ensure proper aspect ratio maintained (use aspect-ratio CSS or wrapper)
- Provide TypeScript types for all props

### 2. Optimize Existing Images
- Audit all image usages across the application
- Replace `<img>` with `ResponsiveImage` component where appropriate
- Generate multiple image sizes for srcset (320w, 640w, 1024w, 1536w)
- Use WebP format with fallback to JPG/PNG
- Compress images for web (aim for <100KB per image)
- Test images load correctly at different viewport sizes

### 3. Prevent Horizontal Scrolling
- Add `overflow-x-hidden` to body or root container
- Ensure all content respects container max-width: `max-w-7xl` or similar
- Set `box-sizing: border-box` globally (Tailwind default)
- Audit all fixed-width elements (tables, images, code blocks)
- Use `w-full` or `max-w-full` on potentially overflowing elements
- Test at 320px viewport (narrowest supported width)

### 4. Implement Responsive Table Component
- Create `ResponsiveTable` component with mobile card view
- Desktop/tablet: Display as standard HTML table
- Mobile: Convert to stacked cards using `display: block` on table elements
- Mobile card structure:
  - Each row becomes a card
  - Key data displayed prominently (e.g., patient name, appointment date)
  - Secondary data hidden or collapsed
  - "View Details" button expands full row data
- Use Tailwind classes: `hidden md:table` for desktop table, `block md:hidden` for mobile cards
- Add ARIA attributes for accessibility

### 5. Update Existing Table Usages
- Identify tables across the application:
  - Appointment list tables
  - Document status tables
  - Audit log tables
  - User management tables (admin)
- Wrap tables with ResponsiveTable component or apply mobile card pattern
- Ensure key information visible on mobile cards
- Test table overflow and scrolling on mobile

### 6. Ensure Smooth CSS Transitions
- Avoid JavaScript-driven layout changes on resize
- Use CSS media queries (Tailwind breakpoints) for all responsive changes
- Test smooth transitions when resizing browser window
- Avoid sudden layout jumps or content shifts
- Use CSS transitions for smooth animations: `transition-all duration-300`

### 7. Test Responsive Utilities
- Test no horizontal scrolling at 320px, 375px, 414px, 768px, 1024px
- Test images load correct sizes at different viewports
- Test tables display as cards on mobile, tables on desktop
- Test smooth transitions when resizing browser
- Test with browser DevTools device emulation
- Test on actual mobile devices if available

### 8. Document Responsive Utilities
- Document ResponsiveImage component usage
- Document ResponsiveTable pattern for mobile cards
- Document horizontal scroll prevention techniques
- Provide code examples for common scenarios

## Current Project State

```
src/frontend/src/
├── components/
│   ├── common/
│   │   ├── ResponsiveImage.tsx (to be created)
│   │   └── ResponsiveTable.tsx (to be created)
│   ├── appointments/
│   │   └── AppointmentList.tsx (has table needing responsive treatment)
│   └── documents/
│       └── DocumentList.tsx (has table needing responsive treatment)
├── pages/
│   └── (various pages with images and tables)
└── public/
    └── images/ (static images)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/common/ResponsiveImage.tsx | Responsive image component with srcset |
| CREATE | src/components/common/ResponsiveTable.tsx | Table component with mobile card view |
| MODIFY | src/components/appointments/AppointmentList.tsx | Apply responsive table pattern |
| MODIFY | src/components/documents/DocumentList.tsx | Apply responsive table pattern |
| MODIFY | src/index.css | Add global overflow-x prevention and smooth transitions |
| MODIFY | src/components/**/*.tsx | Replace img elements with ResponsiveImage |
| CREATE | src/__tests__/components/common/ResponsiveImage.test.tsx | Responsive image tests |
| CREATE | src/__tests__/components/common/ResponsiveTable.test.tsx | Responsive table tests |
| CREATE | docs/RESPONSIVE_IMAGES_TABLES.md | Responsive utilities guidelines |

## External References

### Responsive Images
- [MDN: Responsive Images](https://developer.mozilla.org/en-US/docs/Learn/HTML/Multimedia_and_embedding/Responsive_images)
- [HTML srcset and sizes](https://html.spec.whatwg.org/multipage/images.html#attr-img-srcset)
- [Web.dev: Serve Responsive Images](https://web.dev/serve-responsive-images/)

### Image Optimization
- [Web.dev: Image Optimization](https://web.dev/fast/#optimize-your-images)
- [WebP Image Format](https://developers.google.com/speed/webp)
- [Lazy Loading](https://web.dev/lazy-loading-images/)

### Responsive Tables
- [CSS-Tricks: Responsive Data Tables](https://css-tricks.com/responsive-data-tables/)
- [A11y: Responsive Accessible Table](https://adrianroselli.com/2017/11/a-responsive-accessible-table.html)
- [MDN: Table Accessibility](https://developer.mozilla.org/en-US/docs/Learn/HTML/Tables/Advanced#Tables_for_visually_impaired_users)

### CSS Transitions
- [MDN: CSS Transitions](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_transitions)
- [Tailwind CSS: Transition](https://tailwindcss.com/docs/transition-property)

### Overflow Prevention
- [CSS-Tricks: Overflow](https://css-tricks.com/almanac/properties/o/overflow/)
- [Preventing Horizontal Scroll](https://css-tricks.com/findingfixing-unintended-body-overflow/)

### Testing
- [Playwright Viewport Emulation](https://playwright.dev/docs/emulation#viewport)
- [Testing Responsive Images](https://web.dev/browser-level-image-lazy-loading/)

### Project Documentation
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [Performance Best Practices](.propel/rules/performance-best-practices.md)
- [Design System](../../../.propel/context/docs/designsystem.md#images)

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

# Run responsive utilities tests
npm test -- --grep="responsive"

# Build for production
npm run build

# Development server (test with browser responsive mode)
npm run dev

# Run E2E tests at different viewports
cd ../../test-automation
npx playwright test --project="Mobile Chrome"
npx playwright test --project="Desktop Chrome"
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Images use srcset and sizes attributes
- [ ] Images load correct size at each viewport
- [ ] No horizontal scrolling at 320px viewport
- [ ] Tables display as cards on mobile (<768px)
- [ ] Tables display as standard tables on desktop (≥768px)
- [ ] Smooth CSS transitions when resizing browser
- [ ] No JavaScript-driven layout shifts
- [ ] Test with DevTools Network throttling for image loading

## Implementation Checklist
- [ ] Create `ResponsiveImage` component with srcset and sizes props
- [ ] Add TypeScript types for ResponsiveImage props (src, alt, srcset, sizes, className)
- [ ] Implement lazy loading with loading="lazy" attribute
- [ ] Ensure aspect ratio maintained with aspect-ratio CSS or wrapper
- [ ] Create `ResponsiveTable` component with mobile card view pattern
- [ ] Desktop table: Use standard HTML table with `hidden md:table` classes
- [ ] Mobile cards: Convert rows to cards with `block md:hidden` classes
- [ ] Add "View Details" expansion for secondary data on mobile cards
- [ ] Add global `overflow-x-hidden` to prevent horizontal scrolling
- [ ] Ensure all containers respect max-width constraints (max-w-7xl)
- [ ] Audit and replace all `<img>` elements with ResponsiveImage component
- [ ] Generate multiple image sizes for srcset (320w, 640w, 1024w, 1536w)
- [ ] Optimize images for web (compress, use WebP with fallback)
- [ ] Update AppointmentList table with responsive pattern
- [ ] Update DocumentList table with responsive pattern
- [ ] Add smooth CSS transitions globally: `transition-all duration-300`
- [ ] Test no horizontal scrolling at 320px viewport on all pages
- [ ] Test images load correct sizes at 320px, 768px, 1440px viewports
- [ ] Test tables display correctly on mobile and desktop
- [ ] Test smooth transitions when resizing browser window
- [ ] Create developer documentation for responsive images and tables
