# Task - task_002_fe_color_contrast_validation

## Requirement Reference
- User Story: US_059 - WCAG 2.2 AA & Semantic HTML
- Story Location: .propel/context/tasks/EP-011-I/us_059/us_059.md
- Acceptance Criteria:
    - AC-2: **Given** color contrast (UXR-204), **When** text is displayed, **Then** normal text achieves ≥ 4.5:1 contrast ratio and large text achieves ≥ 3:1 against its background, validated by automated axe-core tests.
    - AC-4 (partial): Focus indicators meet 2px minimum outline requirement with sufficient contrast
- Edge Case:
    - None specific to color contrast

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all 26 wireframes SCR-001 through SCR-026) |
| **Screen Spec** | figma_spec.md (All screens) |
| **UXR Requirements** | UXR-204 (Color contrast ≥ 4.5:1 / 3:1), UXR-202 (Focus indicators with 3:1 contrast) |
| **Design Tokens** | designsystem.md#color-palette, designsystem.md#semantic-colors, designsystem.md#accessibility-requirements |

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
| Tool | axe-core | Latest |

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

Validate and fix color contrast compliance across all UI elements to meet WCAG 2.2 Level AA standards. This task ensures normal text achieves ≥4.5:1 contrast ratio, large text (18pt+/14pt+ bold) achieves ≥3:1, and UI components (buttons, form controls, focus indicators) meet ≥3:1 contrast against adjacent colors. All color combinations will be tested with automated axe-core tests and manual validation tools.

## Dependent Tasks
- None

## Impacted Components
- **Tailwind Configuration**: tailwind.config.js (color palette)
- **Design System**: designsystem.md (color tokens requiring updates)
- **All Components**: Any component using text, buttons, form controls, badges, or borders
- **Theme Files**: CSS variables for dark mode (if implemented)

## Implementation Plan

### 1. Audit Current Color Usage
- Extract all color combinations from Tailwind CSS classes used in components
- Use browser developer tools to inspect computed colors on live pages
- Document all text-on-background and component color pairs
- Reference `designsystem.md` color palette for primary/secondary/neutral colors
- Identify potential violations (text-neutral-400 on white, primary-300 on primary-100, etc.)

### 2. Test Color Contrast Ratios
- Use WebAIM Contrast Checker or similar tool to test all documented color pairs
- Categorize text as "normal" (<18pt regular, <14pt bold) or "large" (≥18pt, ≥14pt bold)
- Verify normal text meets ≥4.5:1 contrast
- Verify large text meets ≥3:1 contrast
- Verify UI component colors meet ≥3:1 contrast (borders, backgrounds, icons)
- Create spreadsheet/report of failing combinations

### 3. Fix Failing Color Combinations
- Update Tailwind color values in `tailwind.config.js` if palette is non-compliant
- Replace failing text color classes (e.g., `text-neutral-400` → `text-neutral-600`)
- Adjust button and badge background colors if contrast insufficient
- Ensure placeholder text meets minimum 4.5:1 contrast
- Fix link colors (default and visited states)
- Update error, warning, success message colors if needed

### 4. Validate Focus Indicators
- Ensure all focus outlines meet 2px minimum width and 3:1 contrast
- Test focus indicators against all background colors (white, primary, secondary, neutral)
- Update Tailwind focus ring utilities if necessary (`focus:ring-2 focus:ring-primary-500`)
- Add focus-visible styles for keyboard navigation (remove for mouse clicks)

### 5. Update Design Tokens Documentation
- Document approved color combinations in `designsystem.md`
- Provide "Do/Don't" examples for common patterns
- Add color contrast ratios to design token definitions
- Create color matrix showing all approved text-on-background pairs

### 6. Implement Automated Testing
- Add axe-core color-contrast rule to all component tests
- Create dedicated color contrast test suite running on all pages
- Add CI/CD check to fail builds with accessibility violations
- Document expected axe-core configuration

### 7. Manual Validation
- Test all 26 screens with browser extensions (Accessibility Insights, axe DevTools)
- Validate in both light and dark modes (if dark mode exists)
- Check printed output (ensure sufficient contrast for print media)
- Test on different displays/color profiles for edge cases

### 8. Create Color Contrast Guidelines
- Update `src/components/README.md` with color usage guidelines
- Document Tailwind utility classes that meet accessibility standards
- Provide code examples for common patterns (button variants, badges, alerts)
- Add ESLint configuration to warn about potentially problematic color classes

## Current Project State

```
src/frontend/
├── tailwind.config.js (defines color palette)
├── src/
│   ├── index.css (global styles, Tailwind imports)
│   ├── components/
│   │   ├── common/ (buttons, badges, alerts use color tokens)
│   │   ├── forms/ (form controls, validation messages)
│   │   └── providers/ (cards with text/background combinations)
│   └── pages/
│       └── (all pages use design tokens)
├── __tests__/
└── .propel/context/docs/designsystem.md (color palette reference)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | tailwind.config.js | Update color palette values for accessibility compliance |
| MODIFY | src/index.css | Add global focus-visible styles |
| MODIFY | src/components/**/*.tsx | Replace non-compliant color classes with accessible alternatives |
| CREATE | src/__tests__/accessibility/colorContrast.test.tsx | Automated color contrast test suite |
| MODIFY | .propel/context/docs/designsystem.md | Add color contrast ratios and approved combinations |
| CREATE | docs/COLOR_ACCESSIBILITY.md | Color usage guidelines for developers |
| MODIFY | .eslintrc.cjs | Add accessibility linting rules (optional) |

## External References

### WCAG 2.2 Color Contrast Standards
- [Contrast (Minimum) 1.4.3 Level AA](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html)
- [Contrast (Enhanced) 1.4.6 Level AAA](https://www.w3.org/WAI/WCAG22/Understanding/contrast-enhanced.html)
- [Non-text Contrast 1.4.11 Level AA](https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast.html)

### Color Contrast Tools
- [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [Accessible Colors](https://accessible-colors.com/)
- [Color Contrast Analyzer (CCA)](https://www.tpgi.com/color-contrast-checker/)
- [Colorable](https://colorable.jxnblk.com/)

### Tailwind CSS
- [Tailwind CSS Color Palette](https://tailwindcss.com/docs/customizing-colors)
- [Tailwind CSS Focus Ring](https://tailwindcss.com/docs/ring-width)
- [Tailwind CSS Dark Mode](https://tailwindcss.com/docs/dark-mode)

### Testing Libraries
- [axe-core Rules: color-contrast](https://github.com/dequelabs/axe-core/blob/develop/doc/rule-descriptions.md#color-contrast)
- [jest-axe Usage](https://github.com/nickcolley/jest-axe#usage)
- [Accessibility Insights for Web](https://accessibilityinsights.io/docs/web/overview/)

### Project Documentation
- [Web Accessibility Standards Rule](.propel/rules/web-accessibility-standards.md)
- [Design System](../.propel/context/docs/designsystem.md#color-palette)

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

# Run accessibility tests only
npm test -- --grep="color contrast"

# Run unit tests with coverage
npm run test:coverage

# Build for production
npm run build

# Development server (manual testing)
npm run dev
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] All axe-core color-contrast tests pass
- [ ] WebAIM Contrast Checker validates all documented color pairs
- [ ] Manual browser extension testing shows zero violations
- [ ] Focus indicators meet 3:1 contrast on all backgrounds
- [ ] All 26 screens validated for color compliance
- [ ] Design system documentation updated with contrast ratios

## Implementation Checklist
- [ ] Extract and document all current color combinations across components
- [ ] Test all color pairs with WebAIM Contrast Checker (create spreadsheet)
- [ ] Identify failing combinations (normal text <4.5:1, large text <3:1, UI <3:1)
- [ ] Update Tailwind color palette in `tailwind.config.js` if base colors fail
- [ ] Replace non-compliant text color classes throughout codebase
- [ ] Fix button, badge, and alert background/foreground combinations
- [ ] Ensure placeholder text meets 4.5:1 contrast minimum
- [ ] Validate and fix link colors (default, hover, visited, focus states)
- [ ] Update error/warning/success message colors if necessary
- [ ] Verify focus indicators meet 2px width and 3:1 contrast
- [ ] Add global `focus-visible` styles for keyboard navigation
- [ ] Update `designsystem.md` with approved color combinations and ratios
- [ ] Create color contrast test suite with axe-core in Jest/Vitest
- [ ] Add CI/CD check for color-contrast axe rule
- [ ] Test all 26 screens with Accessibility Insights browser extension
- [ ] Create developer guidelines document for color usage (docs/COLOR_ACCESSIBILITY.md)
