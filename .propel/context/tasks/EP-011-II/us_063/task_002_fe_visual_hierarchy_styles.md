# Task - task_002_fe_visual_hierarchy_styles

## Requirement Reference
- User Story: US_063 - Navigation & Visual Hierarchy
- Story Location: .propel/context/tasks/EP-011-II/us_063/us_063.md
- Acceptance Criteria:
    - AC-2: **Given** visual hierarchy (UXR-002), **When** content is displayed, **Then** headings (h1–h3) follow a clear size/weight progression, primary actions use filled buttons, secondary actions use outlined buttons, and destructive actions use red coloring.
- Edge Case:
    - None specific to this task

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all wireframes show visual hierarchy) |
| **Screen Spec** | figma_spec.md (cross-cutting visual hierarchy) |
| **UXR Requirements** | UXR-002 (Clear visual hierarchy distinguishing actions), UXR-401 (Design system tokens consistency) |
| **Design Tokens** | designsystem.md#typography, designsystem.md#button, designsystem.md#colors |

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

Implement consistent visual hierarchy styles across all pages using design system tokens. This task creates reusable heading components (h1, h2, h3) with clear size and weight progression, standardizes button variants (filled primary, outlined secondary, destructive red), and ensures all interactive elements follow the established visual hierarchy. The implementation extends Tailwind CSS configuration with custom design tokens and creates React components that enforce consistent styling patterns throughout the application.

## Dependent Tasks
- None (can be implemented in parallel with task_001 and task_003)

## Impacted Components
- **New Components**: Button (with variants), Heading components (H1, H2, H3)
- **Existing Components**: All pages and components using buttons and headings
- **Global Styles**: tailwind.config.js, index.css
- **Typography**: All text elements across the application

## Implementation Plan

### 1. Extend Tailwind Configuration with Design Tokens
- Update `tailwind.config.js` to include custom design tokens from designsystem.md
- Add typography scale: h1 (36px/700), h2 (28px/600), h3 (22px/600)
- Add color palette: primary-500, neutral-600, neutral-900, danger-500, danger-600
- Add custom font families: Inter for headings and body
- Configure Tailwind typography plugin for consistent text styles
- Add custom utility classes for button variants

### 2. Create Heading Components (H1, H2, H3)
- Create `src/components/common/Heading.tsx` with variants: h1, h2, h3
- H1 component: `text-4xl font-bold leading-tight text-neutral-900` (36px, 700 weight, -0.02em letter-spacing)
- H2 component: `text-3xl font-semibold leading-snug text-neutral-900` (28px, 600 weight, -0.01em letter-spacing)
- H3 component: `text-2xl font-semibold leading-7 text-neutral-900` (22px, 600 weight)
- Accept props: `as` (polymorphic component), `className` (for overrides), `children`
- Use semantic HTML tags (h1, h2, h3) with appropriate ARIA if needed
- Add TypeScript types for all props

### 3. Create Button Component with Variants
- Create `src/components/common/Button.tsx` with three variants: primary, secondary, destructive
- **Primary Variant (Filled)**:
  - Base: `bg-primary-500 text-neutral-0 hover:bg-primary-600 active:bg-primary-700`
  - Padding: `px-5 py-2` (20px horizontal, 8px vertical)
  - Border radius: `rounded-md` (8px)
  - Font: `font-medium text-sm` (14px, 500 weight)
  - Transition: `transition-colors duration-200`
  - Focus: `focus:outline-2 focus:outline-primary-500 focus:outline-offset-2`
- **Secondary Variant (Outlined)**:
  - Base: `bg-transparent border-2 border-primary-500 text-primary-500 hover:bg-primary-50 active:bg-primary-100`
  - Same padding, radius, font, transition as primary
  - Focus: Same as primary
- **Destructive Variant (Red)**:
  - Base: `bg-danger-500 text-neutral-0 hover:bg-danger-600 active:bg-danger-700`
  - Same padding, radius, font, transition as primary
  - Focus: `focus:outline-2 focus:outline-danger-500 focus:outline-offset-2`

### 4. Add Button Props and States
- Accept props: `variant` (primary | secondary | destructive), `size` (sm | md | lg), `disabled`, `loading`, `onClick`, `type`, `className`, `children`
- Size variants:
  - `sm`: `px-3 py-1.5 text-xs` (12px horizontal, 6px vertical, 12px font)
  - `md`: `px-5 py-2 text-sm` (default, 20px horizontal, 8px vertical, 14px font)
  - `lg`: `px-6 py-3 text-base` (24px horizontal, 12px vertical, 16px font)
- Disabled state: `opacity-50 cursor-not-allowed pointer-events-none`
- Loading state: Show spinner icon, disable button, text: "Loading..."
- Add ARIA attributes: `aria-disabled`, `aria-busy` (for loading)

### 5. Create Global Typography Styles
- Update `src/index.css` with base typography styles
- Set base font family: `font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif`
- Set base font size: `font-size: 14px` (body text default)
- Set base line height: `line-height: 1.5` (20px for 14px font)
- Set base text color: `color: var(--color-neutral-900)`
- Add utility classes for common text styles: `.text-body`, `.text-body-sm`, `.text-caption`

### 6. Audit and Update Existing Components
- Search codebase for inline button styles and replace with Button component
- Search for inline heading styles (className="text-2xl font-bold") and replace with Heading components
- Update all hard-coded colors to use design tokens (primary-500, neutral-900, danger-500)
- Ensure all buttons use correct variant (primary for main actions, secondary for cancel/back, destructive for delete/remove)
- Verify visual hierarchy: Primary actions more prominent than secondary actions

### 7. Create Visual Hierarchy Documentation
- Document button usage guidelines:
  - Primary: Main call-to-action (Submit, Save, Book, Confirm)
  - Secondary: Supportive actions (Cancel, Back, Edit)
  - Destructive: Irreversible actions (Delete, Remove, Reject)
- Document heading hierarchy:
  - H1: Page titles (one per page)
  - H2: Major sections
  - H3: Subsections and card titles
- Provide code examples for each variant
- Include accessibility notes (color contrast, focus indicators)

### 8. Test Visual Hierarchy Consistency
- Verify all headings follow size progression (h1 > h2 > h3)
- Verify primary buttons stand out from secondary buttons
- Verify destructive buttons are clearly red-colored
- Test button states: default, hover, focus, active, disabled, loading
- Test heading components across all pages
- Verify color contrast meets WCAG 2.2 AA standards (4.5:1 for text, 3:1 for UI)

## Current Project State

```
src/frontend/src/
├── components/
│   └── common/
│       ├── Button.tsx (to be created)
│       └── Heading.tsx (to be created)
├── tailwind.config.js (needs design token extension)
├── index.css (needs global typography styles)
└── pages/
    └── (all pages will use Button and Heading components)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/common/Button.tsx | Button component with primary/secondary/destructive variants |
| CREATE | src/components/common/Heading.tsx | Heading components (H1, H2, H3) with design tokens |
| MODIFY | tailwind.config.js | Add custom design tokens (typography, colors) |
| MODIFY | src/index.css | Add global typography base styles |
| MODIFY | src/components/**/*.tsx | Replace inline button/heading styles with components |
| CREATE | src/__tests__/components/common/Button.test.tsx | Button component tests |
| CREATE | src/__tests__/components/common/Heading.test.tsx | Heading component tests |
| CREATE | docs/VISUAL_HIERARCHY_GUIDE.md | Visual hierarchy usage guidelines |

## External References

### Design System Best Practices
- [Material Design: Typography](https://m3.material.io/styles/typography/overview)
- [Apple HIG: Typography](https://developer.apple.com/design/human-interface-guidelines/typography)
- [Atlassian Design System: Typography](https://atlassian.design/foundations/typography)

### Button Design Patterns
- [WAI-ARIA: Button Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/button/)
- [Material Design: Buttons](https://m3.material.io/components/buttons/overview)
- [Smashing Magazine: Button Design](https://www.smashingmagazine.com/2016/11/a-quick-guide-for-designing-better-buttons/)

### Visual Hierarchy Principles
- [Nielsen Norman Group: Visual Hierarchy](https://www.nngroup.com/articles/visual-hierarchy/)
- [UX Planet: Visual Hierarchy in UX](https://uxplanet.org/visual-hierarchy-in-ux-design-58a0e1c97a67)

### Tailwind CSS
- [Tailwind CSS: Configuration](https://tailwindcss.com/docs/configuration)
- [Tailwind CSS: Adding Custom Styles](https://tailwindcss.com/docs/adding-custom-styles)
- [Tailwind CSS: Typography Plugin](https://tailwindcss.com/docs/typography-plugin)

### Color Contrast
- [WebAIM: Color Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [WCAG 2.2: Contrast (Minimum)](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html)

### Testing
- [Testing Library: Button Testing](https://testing-library.com/docs/example-input-event/)
- [Vitest: Component Testing](https://vitest.dev/guide/)

### Project Documentation
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [UI/UX Design Standards](.propel/rules/ui-ux-design-standards.md)
- [Design System](../../../.propel/context/docs/designsystem.md#typography)

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

# Run Button and Heading component tests
npm test -- Button Heading

# Build for production
npm run build

# Development server
npm run dev
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Headings follow clear size progression (h1 36px > h2 28px > h3 22px)
- [ ] Primary buttons use filled style with primary-500 background
- [ ] Secondary buttons use outlined style with primary-500 border
- [ ] Destructive buttons use filled style with danger-500 (red) background
- [ ] Button hover states work correctly with color transitions
- [ ] Button focus indicators visible with 2px outline
- [ ] All colors meet WCAG 2.2 AA contrast requirements
- [ ] Typography design tokens applied consistently
- [ ] No hard-coded colors or font sizes in components

## Implementation Checklist
- [ ] Update `tailwind.config.js` with custom design tokens (typography scale, color palette)
- [ ] Add custom font families to Tailwind config: Inter for headings and body
- [ ] Create `Heading` component with h1, h2, h3 variants using design tokens
- [ ] H1 styles: `text-4xl font-bold leading-tight text-neutral-900` (36px, 700)
- [ ] H2 styles: `text-3xl font-semibold leading-snug text-neutral-900` (28px, 600)
- [ ] H3 styles: `text-2xl font-semibold leading-7 text-neutral-900` (22px, 600)
- [ ] Create `Button` component with primary, secondary, destructive variants
- [ ] Primary button: `bg-primary-500 text-neutral-0 hover:bg-primary-600`
- [ ] Secondary button: `bg-transparent border-2 border-primary-500 text-primary-500 hover:bg-primary-50`
- [ ] Destructive button: `bg-danger-500 text-neutral-0 hover:bg-danger-600`
- [ ] Add button size variants: sm (px-3 py-1.5), md (px-5 py-2), lg (px-6 py-3)
- [ ] Add button states: disabled (`opacity-50 cursor-not-allowed`), loading (spinner icon)
- [ ] Add focus indicators to buttons: `focus:outline-2 focus:outline-primary-500 focus:outline-offset-2`
- [ ] Update `src/index.css` with base typography styles (font-family, font-size, line-height, color)
- [ ] Audit all existing components for inline button styles and replace with Button component
- [ ] Audit all existing components for inline heading styles and replace with Heading components
- [ ] Verify all hard-coded colors replaced with design tokens
- [ ] Test color contrast ratios using WebAIM checker (≥4.5:1 for text, ≥3:1 for UI)
- [ ] Test button variants on multiple pages (forms, dashboards, modals)
- [ ] Document button usage guidelines in `docs/VISUAL_HIERARCHY_GUIDE.md`
- [ ] Document heading hierarchy best practices
