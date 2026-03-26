# Task - task_001_fe_tailwind_design_token_config

## Requirement Reference
- User Story: US_064 - Design Token & Iconography Consistency
- Story Location: .propel/context/tasks/EP-011-II/us_064/us_064.md
- Acceptance Criteria:
    - AC-1: **Given** design token requirements (UXR-401), **When** Tailwind CSS is configured, **Then** the `tailwind.config.ts` defines color tokens (primary, secondary, success, warning, error, neutral), spacing scale (4px base), font families (system + Inter), and border-radius tokens matching the design system.
    - AC-4: **Given** dark mode readiness, **When** design tokens are defined, **Then** CSS custom properties support future dark mode toggling without component-level changes.
- Edge Case:
    - What happens when a designer introduces a color not in the token system? The PR review process flags non-token colors via a Tailwind CSS linting rule that rejects arbitrary color values.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all wireframes use CSS custom properties) |
| **Screen Spec** | figma_spec.md (cross-cutting design system) |
| **UXR Requirements** | UXR-401 (Unified design token system), UXR-403 (see task_002 for icon sizing) |
| **Design Tokens** | designsystem.md (complete token specification) |

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
| Frontend | Tailwind CSS | Latest |
| Frontend | TypeScript | TypeScript 5.x |
| Tool | PostCSS | Latest |
| Testing | Visual Validation | Manual (token coverage audit) |

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

Configure Tailwind CSS with a complete design token system matching the design system specification in designsystem.md. This task replaces the current partial token configuration with a comprehensive system including color scales (primary, secondary, success, warning, error, neutral with 50-900 shades), spacing scale (4px base unit), typography (font families, sizes, weights, line heights), border-radius tokens, elevation/shadow tokens, motion/transition tokens, and breakpoint tokens. The configuration uses CSS custom properties to enable future dark mode support without component-level changes. This foundational configuration ensures all UI components use consistent design tokens instead of hardcoded values.

## Dependent Tasks
- None (foundational configuration task)

## Impacted Components
- **Configuration Files**: tailwind.config.js (convert to .ts), postcss.config.js
- **Global Styles**: src/index.css (add CSS custom properties)
- **All Components**: Will benefit from token system once refactored in task_002

## Implementation Plan

### 1. Convert tailwind.config.js to TypeScript
- Rename `tailwind.config.js` to `tailwind.config.ts`
- Add TypeScript type imports: `import type { Config } from 'tailwindcss'`
- Export config with full type safety: `export default { ... } satisfies Config`
- Update package.json scripts if needed to recognize .ts config
- Verify Tailwind still builds correctly with TypeScript config

### 2. Configure Complete Color Token System
- Read designsystem.md color palette section (lines 19-115)
- Define primary color scale (50-900) using exact hex values from design system:
  - Primary: #EBF5FF (50) to #072F80 (800) matching `--color-primary-*` tokens
- Define neutral color scale (0, 50-900) using Slate values from design system:
  - Neutral-0: #FFFFFF, Neutral-50: #F8FAFC to Neutral-900: #0F172A
- Define semantic color tokens matching design system:
  - Success: #16A34A (default), success-light: #DCFCE7, success-dark: #166534
  - Warning: #D97706 (default), warning-light: #FEF3C7, warning-dark: #92400E
  - Error: #DC2626 (default), error-light: #FEE2E2, error-dark: #991B1B
  - Info: #2563EB (default), info-light: #DBEAFE, info-dark: #1E40AF
- Replace current blue/gray color definitions with correct hex values

### 3. Configure Spacing Scale (4px Base Unit)
- Define spacing scale from designsystem.md (4px base):
  - 0: "0px", 0.5: "2px", 1: "4px", 1.5: "6px", 2: "8px", 3: "12px"
  - 4: "16px", 5: "20px", 6: "24px", 8: "32px", 10: "40px", 12: "48px"
  - 16: "64px", 20: "80px"
- Remove custom "128" and "144" spacing values not in design system
- Document component padding guidelines: 12px-16px standard, 16px-24px cards

### 4. Configure Typography Tokens
- Define font families from designsystem.md:
  - Heading: `['Inter', '-apple-system', 'BlinkMacSystemFont', 'sans-serif']`
  - Body: `['Inter', '-apple-system', 'BlinkMacSystemFont', 'sans-serif']`
  - Mono: `['JetBrains Mono', 'Fira Code', 'monospace']`
- Replace current sans/serif/mono definitions
- Define font sizes matching design system scale:
  - h1: 36px, h2: 28px, h3: 22px, h4: 18px, h5: 16px, h6: 14px
  - body-lg: 16px, body: 14px, body-sm: 13px, caption: 12px, overline: 11px
- Define font weights: 400 (normal), 500 (medium), 600 (semibold), 700 (bold)
- Define line heights matching design system (tight, snug, normal, relaxed)
- Define letter spacing for h1 (-0.02em), h2 (-0.01em), overline (0.05em)

### 5. Configure Border Radius Tokens
- Define border-radius scale from designsystem.md:
  - none: "0px", sm: "4px", md: "8px", lg: "16px", xl: "24px", full: "9999px"
- Replace current "4xl: 2rem" with correct design system values
- Document usage: sm (inputs, checkboxes), md (buttons, cards), lg (large cards), full (avatars, badges, pills)

### 6. Configure Elevation/Shadow Tokens
- Define box-shadow tokens from designsystem.md elevation levels (0-5):
  - level-1: "0 1px 3px rgba(0,0,0,0.08), 0 1px 2px rgba(0,0,0,0.06)"
  - level-2: "0 4px 6px rgba(0,0,0,0.07), 0 2px 4px rgba(0,0,0,0.06)"
  - level-3: "0 10px 15px rgba(0,0,0,0.1), 0 4px 6px rgba(0,0,0,0.05)"
  - level-4: "0 20px 25px rgba(0,0,0,0.1), 0 8px 10px rgba(0,0,0,0.04)"
  - level-5: "0 25px 50px rgba(0,0,0,0.15)"
- Map to Tailwind shadow utilities: shadow-sm, shadow, shadow-md, shadow-lg, shadow-xl
- Document usage: level-1 (cards, dropdowns), level-3 (modals), level-5 (toasts)

### 7. Configure Motion/Transition Tokens
- Define transition duration tokens from designsystem.md:
  - fast: "100ms", normal: "200ms", moderate: "300ms", slow: "500ms"
- Define easing functions:
  - ease-out: "cubic-bezier(0.16, 1, 0.3, 1)" (entry animations)
  - ease-in-out: "cubic-bezier(0.45, 0, 0.55, 1)" (toggle/reposition)
  - ease-in: "cubic-bezier(0.55, 0.055, 0.675, 0.19)" (exit animations)
- Configure Tailwind transition utilities with design system values
- Add prefers-reduced-motion support

### 8. Configure Breakpoint Tokens
- Define screen breakpoints from designsystem.md:
  - sm: "640px" (mobile-large, Tailwind default kept for intermediate breakpoint)
  - md: "768px" (tablet)
  - lg: "1024px" (desktop)
  - xl: "1280px" (large desktop)
  - 2xl: "1920px" (wide screens)
- Document grid system: 12 columns, gutter (16px mobile, 24px tablet, 32px desktop), max-width 1280px

### 9. Add CSS Custom Properties for Dark Mode
- Create `src/index.css` with CSS custom properties using `:root` selector
- Define all color tokens as CSS variables:
  ```css
  :root {
    --color-primary-50: #EBF5FF;
    --color-primary-500: #0F62FE;
    /* ... all color tokens ... */
  }
  ```
- Add dark mode variant using `[data-theme="dark"]` selector:
  ```css
  [data-theme="dark"] {
    --color-neutral-0: #0F172A;
    --color-neutral-900: #F8FAFC;
    /* ... inverted neutral tokens ... */
  }
  ```
- Reference CSS variables in Tailwind config: `colors: { primary: { 500: 'var(--color-primary-500)' } }`
- This enables future dark mode support without component changes

### 10. Validate Token Configuration
- Build Tailwind CSS to verify no configuration errors
- Inspect generated CSS output for correct token values
- Test token usage in sample component (e.g., `className="bg-primary-500 text-neutral-0"`)
- Verify all designsystem.md tokens are available as Tailwind utilities
- Document any missing tokens or deviations from design system

## Current Project State

```
src/frontend/
├── tailwind.config.js (needs complete rewrite with design tokens)
├── postcss.config.js (verify compatibility)
├── src/
│   ├── index.css (needs CSS custom properties)
│   └── components/ (will use tokens after refactoring in task_002)
└── package.json (verify Tailwind v3.x)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| RENAME | src/frontend/tailwind.config.js | Rename to tailwind.config.ts |
| MODIFY | src/frontend/tailwind.config.ts | Complete rewrite with design system tokens |
| MODIFY | src/frontend/src/index.css | Add CSS custom properties for dark mode |
| CREATE | src/frontend/src/styles/tokens.css | Optional: Separate CSS custom properties file |
| CREATE | docs/DESIGN_TOKEN_USAGE.md | Token usage guidelines for developers |

## External References

### Tailwind CSS Configuration
- [Tailwind CSS: Configuration](https://tailwindcss.com/docs/configuration)
- [Tailwind CSS: Theme Configuration](https://tailwindcss.com/docs/theme)
- [Tailwind CSS: Customizing Colors](https://tailwindcss.com/docs/customizing-colors)
- [Tailwind CSS: TypeScript](https://tailwindcss.com/docs/configuration#typescript)

### CSS Custom Properties
- [MDN: Using CSS custom properties](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
- [CSS-Tricks: A Complete Guide to Custom Properties](https://css-tricks.com/a-complete-guide-to-custom-properties/)

### Dark Mode Implementation
- [Tailwind CSS: Dark Mode](https://tailwindcss.com/docs/dark-mode)
- [Web.dev: prefers-color-scheme](https://web.dev/prefers-color-scheme/)

### Design Token Systems
- [Design Tokens: What Are They & How Will They Help You?](https://www.youtube.com/watch?v=wtTstdiBuUk)
- [Nathan Curtis: Tokens in Design Systems](https://medium.com/eightshapes-llc/tokens-in-design-systems-25dd82d58421)

### Project Documentation
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [UI/UX Design Standards](.propel/rules/ui-ux-design-standards.md)
- [Design System](../../../.propel/context/docs/designsystem.md)

## Build Commands

```bash
# Navigate to frontend directory
cd src/frontend

# Install dependencies (if needed)
npm install

# Build Tailwind CSS to verify configuration
npm run build:css

# Run development server to test tokens
npm run dev

# Type check TypeScript configuration
npx tsc --noEmit tailwind.config.ts

# View generated Tailwind CSS output
npx tailwindcss -o dist/output.css --watch
```

## Implementation Validation Strategy
- [ ] Tailwind config builds without errors
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] All color tokens from designsystem.md available as Tailwind utilities
- [ ] Spacing scale uses 4px base unit (0, 0.5, 1, 1.5, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20)
- [ ] Typography tokens (Inter font family, h1-h6 sizes, body sizes) configured
- [ ] Border-radius tokens (none, sm, md, lg, xl, full) match design system
- [ ] Shadow tokens (level-1 through level-5) configured
- [ ] Transition tokens (fast, normal, moderate, slow) with easing functions
- [ ] Breakpoint tokens (sm, md, lg, xl, 2xl) match design system
- [ ] CSS custom properties defined in index.css for dark mode support
- [ ] Sample component renders correctly using tokens (bg-primary-500, text-neutral-0)

## Implementation Checklist
- [ ] Rename `tailwind.config.js` to `tailwind.config.ts` with TypeScript types
- [ ] Define primary color scale (50-900) using exact hex values from designsystem.md
- [ ] Define neutral color scale (0, 50-900) using Slate values
- [ ] Define semantic colors: success, warning, error, info with light/dark variants
- [ ] Replace current color definitions with design system tokens
- [ ] Configure spacing scale with 4px base unit (0, 0.5, 1, 1.5, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20)
- [ ] Define font families: Inter for heading/body, JetBrains Mono for monospace
- [ ] Configure font sizes: h1-h6, body-lg, body, body-sm, caption, overline
- [ ] Configure font weights: 400, 500, 600, 700
- [ ] Configure line heights and letter spacing from design system
- [ ] Define border-radius tokens: none, sm, md, lg, xl, full
- [ ] Configure box-shadow tokens: level-1 through level-5 elevation
- [ ] Configure transition durations: fast (100ms), normal (200ms), moderate (300ms), slow (500ms)
- [ ] Configure easing functions: ease-out, ease-in-out, ease-in
- [ ] Define breakpoint tokens: sm (640px), md (768px), lg (1024px), xl (1280px), 2xl (1920px)
- [ ] Add CSS custom properties to `src/index.css` using `:root` selector
- [ ] Define dark mode CSS custom properties using `[data-theme="dark"]` selector
- [ ] Reference CSS variables in Tailwind config for dark mode support
- [ ] Build Tailwind CSS and verify no configuration errors
- [ ] Test token usage in sample component (bg-primary-500, text-neutral-0, p-4, rounded-md)
- [ ] Create `docs/DESIGN_TOKEN_USAGE.md` with token usage guidelines
