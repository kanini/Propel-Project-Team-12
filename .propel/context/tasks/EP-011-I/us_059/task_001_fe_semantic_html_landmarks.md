# Task - task_001_fe_semantic_html_landmarks

## Requirement Reference
- User Story: US_059 - WCAG 2.2 AA & Semantic HTML
- Story Location: .propel/context/tasks/EP-011-I/us_059/us_059.md
- Acceptance Criteria:
    - AC-1: **Given** semantic HTML (UXR-201), **When** any page renders, **Then** content uses proper landmark roles (header, nav, main, footer), heading hierarchy (h1-h6 without skipping levels), and ARIA labels on interactive elements.
- Edge Case:
    - Complex data tables: Tables include proper `<th>` scope attributes, `<caption>` elements, and `aria-describedby` for supplementary notes.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all 26 wireframes SCR-001 through SCR-026) |
| **Screen Spec** | figma_spec.md (All screens) |
| **UXR Requirements** | UXR-201 (Semantic HTML and landmark roles), UXR-206 (Screen reader compatibility) |
| **Design Tokens** | designsystem.md#typography, designsystem.md#accessibility-requirements |

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

Audit and implement proper semantic HTML structure and ARIA landmark roles across all 26 screens to meet WCAG 2.2 Level AA standards (UXR-201). This task ensures all pages use proper HTML5 semantic elements (`<header>`, `<nav>`, `<main>`, `<footer>`, `<article>`, `<section>`, `<aside>`), maintain correct heading hierarchy (h1-h6 without skipping levels), and provide appropriate ARIA labels on all interactive elements. This enables screen reader users to efficiently navigate the application using landmark shortcuts and understand page structure.

## Dependent Tasks
- None (foundational accessibility task)

## Impacted Components
- **Layout Components**: MainLayout.tsx, Sidebar.tsx, BottomNav.tsx, Header (create if missing)
- **Page Components**: All page components in src/pages/ and src/features/*/pages/
- **Common Components**: All components in src/components/common/
- **Form Components**: All components in src/components/forms/
- **Feature Components**: All components in src/components/

## Implementation Plan

### 1. Audit Current Semantic HTML Usage
- Review all wireframes (SCR-001 through SCR-026) to identify required semantic structure
- Scan all React components for current usage of semantic HTML vs. generic div/span
- Identify missing landmark regions (header, nav, main, footer, aside)
- Document heading hierarchy issues (skipped levels, multiple h1s, missing headings)
- Create audit report with priority issues

### 2. Implement Global Layout Landmarks
- Create/update Header component with `<header role="banner">` for all pages
- Update MainLayout.tsx to use `<main role="main" id="main-content">` wrapper
- Ensure Sidebar.tsx uses `<nav role="navigation" aria-label="Primary navigation">`
- Add Footer component with `<footer role="contentinfo">` if required by design
- Implement skip-to-content link pattern (already exists in Login/Register, extend to all pages)

### 3. Fix Heading Hierarchy Across All Pages
- Ensure each page has exactly one h1 element (page title)
- Verify heading levels increment by 1 (h1 → h2 → h3, never h1 → h3)
- Replace styled divs with appropriate heading elements
- Use `designsystem.md#typography` scale for heading styles
- Add `aria-labelledby` references where headings serve as section labels

### 4. Add Semantic Structure to Complex Components
- Update table components to use `<table>`, `<thead>`, `<tbody>`, `<th scope="col|row">`, and `<caption>` elements
- Add `<article>` for self-contained content (appointment cards, provider cards)
- Use `<section>` with `aria-labelledby` for distinct content regions
- Replace generic list divs with `<ul>`/`<ol>` where appropriate
- Add `<time datetime>` elements for date/time displays

### 5. Enhance Interactive Element Accessibility
- Add descriptive ARIA labels to all buttons without visible text (icon buttons)
- Ensure form labels use `<label for="inputId">` or `aria-labelledby`
- Add `aria-describedby` for supplementary help text on form fields
- Use `role="button"` and proper keyboard handlers for clickable divs (convert to `<button>`)
- Add `aria-current="page"` to active navigation links

### 6. Validate with Automated Tools
- Run axe-core accessibility tests via React Testing Library `axe` matcher
- Use browser extensions (Accessibility Insights, axe DevTools) for manual validation
- Test with screen reader (NVDA/JAWS on Windows, VoiceOver on macOS) to verify landmark navigation
- Verify heading structure using browser developer tools (H1-H6 outline)

### 7. Update Component Tests
- Add accessibility tests to all component test files using `@testing-library/jest-dom/extend-expect`
- Use `toHaveAccessibleName()`, `toHaveAccessibleDescription()` assertions
- Verify semantic structure in tests (check for presence of `<nav>`, `<main>`, proper headings)
- Add regression tests for heading hierarchy

### 8. Document Semantic HTML Patterns
- Create/update `src/components/README.md` with semantic HTML usage guidelines
- Add code examples for common patterns (skip links, landmark regions, heading hierarchy)
- Document ARIA label conventions for the project

## Current Project State

```
src/frontend/src/
├── components/
│   ├── layout/
│   │   ├── MainLayout.tsx (needs main landmark)
│   │   ├── Sidebar.tsx (has nav role)
│   │   └── BottomNav.tsx
│   ├── common/
│   │   ├── EmptyState.tsx
│   │   ├── Pagination.tsx
│   │   └── SkeletonLoader.tsx
│   ├── forms/
│   │   └── PasswordStrengthIndicator.tsx
│   └── appointments/
│       ├── ProgressIndicator.tsx (has nav role)
│       └── AppointmentCard.tsx
├── pages/
│   ├── LoginPage.tsx (has skip link)
│   ├── RegisterPage.tsx (has skip link)
│   ├── AppointmentBooking.tsx
│   └── DocumentStatusPage.tsx (uses main landmark)
├── features/
│   └── auth/pages/
│       ├── LoginPage.tsx
│       └── RegisterPage.tsx
└── __tests__/
    └── components/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/layout/Header.tsx | Create global header with banner role and skip link |
| MODIFY | src/components/layout/MainLayout.tsx | Add main landmark with id="main-content" |
| MODIFY | src/components/layout/Sidebar.tsx | Ensure nav landmark with aria-label |
| MODIFY | src/pages/*.tsx | Fix heading hierarchy, add semantic sections |
| MODIFY | src/features/*/pages/*.tsx | Fix heading hierarchy, add semantic sections |
| MODIFY | src/components/common/*.tsx | Replace divs with semantic elements where appropriate |
| MODIFY | src/components/forms/*.tsx | Ensure proper label associations |
| MODIFY | src/components/providers/ProviderCard.tsx | Wrap in article element |
| MODIFY | src/components/appointments/AppointmentCard.tsx | Wrap in article element |
| CREATE | src/__tests__/accessibility/semanticHtml.test.tsx | Comprehensive semantic HTML tests |
| MODIFY | src/components/README.md | Add semantic HTML guidelines |

## External References

### WCAG 2.2 Standards
- [WCAG 2.2 Level AA Success Criteria](https://www.w3.org/WAI/WCAG22/quickref/?currentsidebar=%23col_customize&levels=aaa)
- [Info and Relationships (1.3.1)](https://www.w3.org/WAI/WCAG22/Understanding/info-and-relationships.html)
- [Headings and Labels (2.4.6)](https://www.w3.org/WAI/WCAG22/Understanding/headings-and-labels.html)
- [Section Headings (2.4.10)](https://www.w3.org/WAI/WCAG22/Understanding/section-headings.html)

### HTML5 Semantic Elements
- [MDN: HTML5 Semantic Elements](https://developer.mozilla.org/en-US/docs/Glossary/Semantics#semantic_elements)
- [HTML Living Standard: Sections](https://html.spec.whatwg.org/multipage/sections.html)
- [Using ARIA: Landmark Roles](https://www.w3.org/WAI/ARIA/apg/practices/landmark-regions/)

### React & Testing
- [React 18 Documentation](https://react.dev/)
- [Testing Library: Accessibility Queries](https://testing-library.com/docs/queries/byrole/)
- [jest-axe: Automated Accessibility Testing](https://github.com/nickcolley/jest-axe)

### Project Documentation
- [Web Accessibility Standards Rule](.propel/rules/web-accessibility-standards.md)
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [Design System](../.propel/context/docs/designsystem.md#accessibility-requirements)

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

# Run unit tests with coverage
npm run test:coverage

# Run accessibility tests
npm test -- --grep="accessibility"

# Build for production
npm run build
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Axe-core automated tests pass with zero violations
- [ ] Manual screen reader testing confirms landmark navigation works
- [ ] Heading hierarchy validated with browser developer tools
- [ ] All interactive elements have accessible names
- [ ] Skip-to-content link present and functional on all pages

## Implementation Checklist
- [ ] Audit all 26 wireframes and document semantic structure requirements
- [ ] Create audit report with current state and priority issues
- [ ] Create/update Header component with `<header role="banner">` and skip link
- [ ] Update MainLayout.tsx to wrap content in `<main role="main" id="main-content">`
- [ ] Verify Sidebar.tsx uses `<nav role="navigation" aria-label="[Portal] navigation">`
- [ ] Fix heading hierarchy on all pages (ensure single h1, no skipped levels)
- [ ] Convert appointment/provider cards to use `<article>` elements
- [ ] Add `<table>`, `<thead>`, `<tbody>`, `<th scope>`, `<caption>` to table components
- [ ] Add descriptive ARIA labels to all icon-only buttons
- [ ] Update all form components to use proper `<label for>` associations
- [ ] Install and configure jest-axe for automated accessibility testing
- [ ] Create comprehensive semantic HTML test suite
- [ ] Test with NVDA/JAWS/VoiceOver to verify landmark navigation
- [ ] Validate heading structure with browser developer tools on all pages
- [ ] Update component documentation with semantic HTML guidelines
