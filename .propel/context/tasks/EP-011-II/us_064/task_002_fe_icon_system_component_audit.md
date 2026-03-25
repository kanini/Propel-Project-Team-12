# Task - task_002_fe_icon_system_component_audit

## Requirement Reference
- User Story: US_064 - Design Token & Iconography Consistency
- Story Location: .propel/context/tasks/EP-011-II/us_064/us_064.md
- Acceptance Criteria:
    - AC-2: **Given** iconography consistency (UXR-403), **When** icons are used in the UI, **Then** all icons come from a single library (Lucide React), use consistent sizing (16px, 20px, 24px), match the text color of their context, and have `aria-hidden="true"` when decorative.
    - AC-3: **Given** component theming, **When** a UI component is rendered, **Then** it exclusively uses design tokens via Tailwind utility classes (no hardcoded hex colors, no inline pixel values).
- Edge Case:
    - How are third-party component styles handled? Third-party components are wrapped with token-based style overrides applied via Tailwind's `@layer components`.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all wireframes show icon usage) |
| **Screen Spec** | figma_spec.md#branding-visual-direction |
| **UXR Requirements** | UXR-403 (Consistent iconography), UXR-401 (Design token adherence) |
| **Design Tokens** | designsystem.md#iconography |

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
| Library | Lucide React | Latest (^0.263.0) |
| Tool | ESLint | Latest |
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

Implement a consistent icon system using Lucide React library and audit all components to replace hardcoded colors/styles with design tokens. This task installs Lucide React, creates an Icon wrapper component supporting multiple sizes (16px, 20px, 24px) and automatic color inheritance, replaces emoji/inline SVG icons with Lucide icons, audits all components for hardcoded colors (bg-blue-100, text-gray-500) and replaces them with design tokens (bg-primary-100, text-neutral-500), and adds ESLint rules to prevent future hardcoded values. The refactoring ensures 100% design token adherence across the application.

## Dependent Tasks
- task_001_fe_tailwind_design_token_config (provides design token system)

## Impacted Components
- **New Components**: Icon wrapper component
- **All Existing Components**: Sidebar, BottomNav, Header, Button, StatusBadge, EmptyState (need icon and color refactoring)
- **ESLint Config**: Add custom rules to prevent hardcoded values

## Implementation Plan

### 1. Install Lucide React Icon Library
- Run: `npm install lucide-react`
- Verify installation in package.json
- Browse available icons: https://lucide.dev/icons/
- Document icon naming convention (PascalCase: Home, Calendar, Search)
- Create icon mapping guide for common use cases

### 2. Create Icon Wrapper Component
- Create `src/components/common/Icon.tsx` component
- Accept props: `name` (Lucide icon name), `size` (16 | 20 | 24), `className`, `ariaLabel` (optional)
- Default size: 24px (from designsystem.md)
- Supported sizes: 16px (inline), 20px (compact), 24px (default), 32px (feature)
- Inherit text color from parent context: `currentColor`
- Add `aria-hidden="true"` automatically for decorative icons
- If `ariaLabel` provided, use `aria-label` instead of `aria-hidden`
- TypeScript types: Define IconProps interface with strict size literal type

### 3. Map Common Icons to Lucide Equivalents
- Create icon mapping in Icon component or config file:
  - Home/Dashboard → Home icon
  - Calendar/Appointments → Calendar icon
  - Search/Find → Search icon
  - Documents → FileText icon
  - Profile/User → User icon
  - Settings → Settings icon
  - Logout → LogOut icon
  - Upload → Upload icon
  - Download → Download icon
  - Check/Success → Check icon
  - X/Close → X icon
  - Alert/Warning → AlertTriangle icon
  - Info → Info icon
  - Menu/Hamburger → Menu icon
- Document full icon mapping for developers

### 4. Replace Emoji Icons with Lucide Icons
- Audit codebase for emoji usage: Sidebar.tsx, BottomNav.tsx use emoji icons
- Replace emoji icons with Icon component using Lucide icons:
  - 🏠 → `<Icon name="Home" size={20} />`
  - 📅 → `<Icon name="Calendar" size={20} />`
  - 🔍 → `<Icon name="Search" size={20} />`
  - 👤 → `<Icon name="User" size={20} />`
  - ⚙️ → `<Icon name="Settings" size={20} />`
  - 🚪 → `<Icon name="LogOut" size={20} />`
- Update all navigation components (Sidebar, BottomNav, Header)

### 5. Replace Inline SVG Icons with Lucide Icons
- Audit codebase for inline `<svg>` elements: StatusBadge.tsx uses inline SVG
- Replace inline SVG with Icon component:
  - Check icon → `<Icon name="Check" size={16} />`
  - X icon → `<Icon name="X" size={16} />`
  - AlertTriangle → `<Icon name="AlertTriangle" size={16} />`
- EmptyState component: Replace inline SVG with Lucide icons
- Ensure icon color inherits from parent badge/component color

### 6. Audit and Refactor Hardcoded Colors
- Search codebase for hardcoded color patterns:
  - `bg-blue-*` → `bg-primary-*`
  - `text-blue-*` → `text-primary-*`
  - `bg-gray-*` → `bg-neutral-*`
  - `text-gray-*` → `text-neutral-*`
  - `bg-green-*` → `bg-success-*`
  - `bg-red-*` → `bg-error-*`
  - `bg-amber-*` → `bg-warning-*`
- Target components for refactoring:
  - Sidebar.tsx: Replace gray/blue with neutral/primary tokens
  - BottomNav.tsx: Replace gray/blue with neutral/primary tokens
  - Header.tsx (from US_063): Ensure uses tokens
  - Button.tsx (from US_063): Ensure uses tokens
  - StatusBadge.tsx: Replace green/red/amber/blue with success/error/warning/info tokens
  - EmptyState.tsx: Replace neutral-800 with neutral-900 or verify token usage

### 7. Audit and Remove Inline Pixel Values
- Search codebase for inline pixel values: className="text-2xl", className="w-3 h-3"
- Replace with Tailwind utilities using design tokens:
  - Font sizes: Use text-h1, text-h2, text-body, text-caption (from Tailwind config)
  - Spacing: Ensure uses 4px base scale (p-2, m-4, gap-3)
  - Widths/Heights: Use token-based values (w-6, h-6) or icon size prop
- Verify all spacing values align with 4px base unit

### 8. Add ESLint Rules for Token Enforcement
- Install ESLint plugin if needed: `eslint-plugin-tailwindcss`
- Add ESLint rule to `.eslintrc.js` or `.eslintrc.json`:
  ```json
  {
    "rules": {
      "tailwindcss/no-custom-classname": "warn",
      "tailwindcss/no-arbitrary-value": "error"
    }
  }
  ```
- Configure rule to reject arbitrary color values: `bg-[#3b82f6]` should error
- Configure rule to reject arbitrary pixel values: `p-[17px]` should error
- Document ESLint rules in developer guidelines

### 9. Wrap Third-Party Component Styles (If Any)
- Identify third-party components (React Router, date pickers, modals)
- For components needing custom styles, use Tailwind's `@layer components`:
  ```css
  @layer components {
    .react-datepicker {
      @apply bg-neutral-0 border border-neutral-200 rounded-md shadow-md;
    }
  }
  ```
- Wrap third-party components with token-based overrides
- Document third-party styling approach for future components

### 10. Validate Token Adherence
- Run grep search for hardcoded hex colors: `grep -r "#[0-9a-fA-F]\{6\}" src/`
- Run grep search for arbitrary Tailwind values: `grep -r "\[.*px\]" src/`
- Run ESLint to catch violations: `npm run lint`
- Manually review all components for token usage
- Create token adherence report: X components refactored, Y violations fixed
- Test visual consistency across all screens

## Current Project State

```
src/frontend/src/
├── components/
│   ├── common/
│   │   ├── Icon.tsx (to be created)
│   │   ├── Button.tsx (needs color token verification)
│   │   └── EmptyState.tsx (needs icon refactoring)
│   ├── layout/
│   │   ├── Sidebar.tsx (needs emoji → Lucide + color token refactoring)
│   │   ├── BottomNav.tsx (needs emoji → Lucide + color token refactoring)
│   │   └── Header.tsx (needs icon + color token verification)
│   └── documents/
│       └── StatusBadge.tsx (needs inline SVG → Lucide + color token refactoring)
├── .eslintrc.json (needs token enforcement rules)
└── package.json (needs lucide-react dependency)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/frontend/package.json | Add lucide-react dependency |
| CREATE | src/components/common/Icon.tsx | Icon wrapper component with size variants |
| MODIFY | src/components/layout/Sidebar.tsx | Replace emoji icons with Lucide, refactor colors to tokens |
| MODIFY | src/components/layout/BottomNav.tsx | Replace emoji icons with Lucide, refactor colors to tokens |
| MODIFY | src/components/layout/Header.tsx | Verify icon + color token usage |
| MODIFY | src/components/common/Button.tsx | Verify color token usage |
| MODIFY | src/components/documents/StatusBadge.tsx | Replace inline SVG with Lucide, refactor colors to tokens |
| MODIFY | src/components/common/EmptyState.tsx | Replace inline SVG with Lucide icons |
| MODIFY | src/frontend/.eslintrc.json | Add tailwindcss/no-arbitrary-value rule |
| CREATE | docs/ICON_USAGE_GUIDE.md | Icon library usage guidelines |
| CREATE | docs/TOKEN_REFACTORING_REPORT.md | Component token adherence report |

## External References

### Lucide React Documentation
- [Lucide React: Getting Started](https://lucide.dev/guide/packages/lucide-react)
- [Lucide Icons: Icon List](https://lucide.dev/icons/)
- [Lucide React: Props](https://lucide.dev/guide/packages/lucide-react#props)

### Accessibility
- [WAI-ARIA: aria-hidden](https://www.w3.org/TR/wai-aria-1.2/#aria-hidden)
- [MDN: aria-label](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Attributes/aria-label)

### ESLint + Tailwind CSS
- [eslint-plugin-tailwindcss](https://github.com/francoismassart/eslint-plugin-tailwindcss)
- [Tailwind CSS: Arbitrary Values](https://tailwindcss.com/docs/adding-custom-styles#using-arbitrary-values)

### Design Token Systems
- [Nathan Curtis: Tokens in Design Systems](https://medium.com/eightshapes-llc/tokens-in-design-systems-25dd82d58421)
- [Design Tokens: W3C Community Group](https://www.w3.org/community/design-tokens/)

### Project Documentation
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)
- [UI/UX Design Standards](.propel/rules/ui-ux-design-standards.md)
- [Design System](../../../.propel/context/docs/designsystem.md#iconography)

## Build Commands

```bash
# Navigate to frontend directory
cd src/frontend

# Install lucide-react
npm install lucide-react

# Run type checking
npm run typecheck

# Run linting with new rules
npm run lint

# Fix auto-fixable lint issues
npm run lint -- --fix

# Run unit tests
npm test

# Build for production
npm run build

# Development server
npm run dev

# Search for hardcoded colors
grep -r "bg-blue-\|text-blue-\|bg-gray-\|text-gray-" src/ --include="*.tsx"

# Search for hardcoded hex colors
grep -r "#[0-9a-fA-F]\{6\}" src/ --include="*.tsx" --include="*.css"

# Search for arbitrary Tailwind values
grep -r "\[.*px\]" src/ --include="*.tsx"
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Lucide React installed and Icon component created
- [ ] All emoji icons replaced with Lucide icons (Sidebar, BottomNav)
- [ ] All inline SVG icons replaced with Lucide icons (StatusBadge, EmptyState)
- [ ] Icon sizes consistent: 16px (inline), 20px (compact), 24px (default)
- [ ] Icons inherit text color from parent context (currentColor)
- [ ] Decorative icons have `aria-hidden="true"`
- [ ] All hardcoded colors replaced with design tokens (bg-blue → bg-primary, text-gray → text-neutral)
- [ ] No hardcoded hex colors remain in codebase (grep search passes)
- [ ] No arbitrary Tailwind values remain (grep search passes)
- [ ] ESLint rules enforce token usage (no-arbitrary-value error)
- [ ] Visual consistency maintained across all screens after refactoring

## Implementation Checklist
- [ ] Install lucide-react: `npm install lucide-react`
- [ ] Create `Icon` component at `src/components/common/Icon.tsx` with size variants (16, 20, 24, 32)
- [ ] Define IconProps interface with TypeScript types (name, size, className, ariaLabel)
- [ ] Icon component inherits parent text color using `currentColor`
- [ ] Icon component adds `aria-hidden="true"` for decorative icons
- [ ] Create icon mapping documentation (Home, Calendar, Search, FileText, User, Settings, LogOut, etc.)
- [ ] Replace emoji icons in Sidebar.tsx with Lucide icons (Home, Calendar, Search, User, Settings, LogOut)
- [ ] Replace emoji icons in BottomNav.tsx with Lucide icons
- [ ] Replace inline SVG in StatusBadge.tsx with Lucide icons (Check, X, AlertTriangle)
- [ ] Replace inline SVG in EmptyState.tsx with Lucide icons
- [ ] Refactor Sidebar.tsx colors: bg-blue → bg-primary, text-gray → text-neutral
- [ ] Refactor BottomNav.tsx colors: bg-blue → bg-primary, text-gray → text-neutral
- [ ] Refactor StatusBadge.tsx colors: bg-green → bg-success, bg-red → bg-error, bg-amber → bg-warning, bg-blue → bg-info
- [ ] Verify Button.tsx uses design tokens exclusively
- [ ] Verify Header.tsx uses design tokens exclusively
- [ ] Search codebase for hardcoded hex colors and remove
- [ ] Search codebase for arbitrary Tailwind pixel values and remove
- [ ] Add ESLint rules to `.eslintrc.json`: tailwindcss/no-arbitrary-value (error)
- [ ] Run ESLint and fix all violations: `npm run lint -- --fix`
- [ ] Test visual consistency on Dashboard, Appointments, Documents, Profile pages
- [ ] Create `docs/ICON_USAGE_GUIDE.md` with Lucide icon usage examples
- [ ] Create `docs/TOKEN_REFACTORING_REPORT.md` documenting components refactored
