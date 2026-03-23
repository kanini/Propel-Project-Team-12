# Design System - Unified Patient Access & Clinical Intelligence Platform

> **Purpose**: Design tokens, branding guidelines, and component specifications for the UPACIP healthcare platform.

## 1. Brand Overview

### Brand Identity
- **Platform Name**: Unified Patient Access & Clinical Intelligence Platform (UPACIP)
- **Brand Personality**: Professional, trustworthy, calming, modern
- **Target Aesthetic**: Clean healthcare SaaS with accessibility focus
- **Visual Direction**: Trust-first design with clear visual hierarchy, calming colors, and reassuring feedback

### Design Philosophy
- **Users First**: Optimize task completion for healthcare workflows
- **Trust Through Transparency**: AI suggestions clearly distinguished from verified data
- **Accessibility by Design**: WCAG 2.2 AA compliance from inception
- **Consistent Experience**: Unified visual grammar across Patient, Staff, and Admin portals

---

## 2. Color Palette

### Primitive Tokens

```yaml
# Blue (Primary Brand)
color.blue.50: "#E8F4FD"
color.blue.100: "#C5E3F9"
color.blue.200: "#9FCEF4"
color.blue.300: "#79B9EF"
color.blue.400: "#54A4EA"
color.blue.500: "#2E8FE5"   # Primary Blue
color.blue.600: "#2573C4"
color.blue.700: "#1C57A3"
color.blue.800: "#133B82"
color.blue.900: "#0A1F61"

# Teal (Secondary)
color.teal.50: "#E6F7F5"
color.teal.100: "#C0ECE7"
color.teal.200: "#99E0D9"
color.teal.300: "#73D4CB"
color.teal.400: "#4CC8BD"
color.teal.500: "#26BCAF"   # Secondary Teal
color.teal.600: "#1F9A8F"
color.teal.700: "#18786F"
color.teal.800: "#11564F"
color.teal.900: "#0A342F"

# Orange (Accent/CTA)
color.orange.50: "#FFF3E8"
color.orange.100: "#FFDFC5"
color.orange.200: "#FFCB9F"
color.orange.300: "#FFB779"
color.orange.400: "#FFA353"
color.orange.500: "#FF8F2D"   # Accent Orange
color.orange.600: "#D97524"
color.orange.700: "#B35B1B"
color.orange.800: "#8C4112"
color.orange.900: "#662709"

# Neutral (Grayscale)
color.neutral.0: "#FFFFFF"
color.neutral.50: "#F9FAFB"
color.neutral.100: "#F3F4F6"
color.neutral.200: "#E5E7EB"
color.neutral.300: "#D1D5DB"
color.neutral.400: "#9CA3AF"
color.neutral.500: "#6B7280"
color.neutral.600: "#4B5563"
color.neutral.700: "#374151"
color.neutral.800: "#1F2937"
color.neutral.900: "#111827"
color.neutral.950: "#030712"
```

### Semantic Tokens

```yaml
# Light Mode
color.primary: "{color.blue.500}"
color.primary.hover: "{color.blue.600}"
color.primary.active: "{color.blue.700}"
color.primary.subtle: "{color.blue.50}"

color.secondary: "{color.teal.500}"
color.secondary.hover: "{color.teal.600}"
color.secondary.active: "{color.teal.700}"
color.secondary.subtle: "{color.teal.50}"

color.accent: "{color.orange.500}"
color.accent.hover: "{color.orange.600}"
color.accent.active: "{color.orange.700}"
color.accent.subtle: "{color.orange.50}"

# Semantic States
color.success: "#10B981"       # Emerald 500
color.success.bg: "#D1FAE5"    # Emerald 100
color.success.text: "#065F46"  # Emerald 800

color.warning: "#F59E0B"       # Amber 500
color.warning.bg: "#FEF3C7"    # Amber 100
color.warning.text: "#92400E"  # Amber 800

color.error: "#EF4444"         # Red 500
color.error.bg: "#FEE2E2"      # Red 100
color.error.text: "#991B1B"    # Red 800

color.info: "#3B82F6"          # Blue 500
color.info.bg: "#DBEAFE"       # Blue 100
color.info.text: "#1E40AF"     # Blue 800

# Surface Colors
color.surface.primary: "{color.neutral.0}"
color.surface.secondary: "{color.neutral.50}"
color.surface.tertiary: "{color.neutral.100}"

# Text Colors
color.text.primary: "{color.neutral.900}"
color.text.secondary: "{color.neutral.600}"
color.text.tertiary: "{color.neutral.400}"
color.text.inverse: "{color.neutral.0}"
color.text.link: "{color.blue.600}"
color.text.link.hover: "{color.blue.700}"

# Border Colors
color.border.default: "{color.neutral.200}"
color.border.strong: "{color.neutral.300}"
color.border.focus: "{color.blue.500}"
color.border.error: "{color.error}"
```

### Dark Mode Tokens

```yaml
# Dark Mode Overrides
color.surface.primary: "{color.neutral.900}"
color.surface.secondary: "{color.neutral.800}"
color.surface.tertiary: "{color.neutral.700}"

color.text.primary: "{color.neutral.50}"
color.text.secondary: "{color.neutral.300}"
color.text.tertiary: "{color.neutral.500}"
color.text.inverse: "{color.neutral.900}"

color.border.default: "{color.neutral.700}"
color.border.strong: "{color.neutral.600}"

# Semantic States (Dark Mode)
color.success: "#34D399"       # Emerald 400
color.success.bg: "#064E3B"    # Emerald 900
color.warning: "#FBBF24"       # Amber 400
color.warning.bg: "#78350F"    # Amber 900
color.error: "#F87171"         # Red 400
color.error.bg: "#7F1D1D"      # Red 900
color.info: "#60A5FA"          # Blue 400
color.info.bg: "#1E3A8A"       # Blue 900
```

### Role-Specific Accent Colors

```yaml
# Patient Portal Accent
color.role.patient: "{color.blue.500}"
color.role.patient.bg: "{color.blue.50}"

# Staff Portal Accent
color.role.staff: "{color.teal.500}"
color.role.staff.bg: "{color.teal.50}"

# Admin Portal Accent
color.role.admin: "{color.orange.500}"
color.role.admin.bg: "{color.orange.50}"
```

### AI/Verification Visual Tokens

```yaml
# AI Suggestion Indicator
color.ai.suggestion: "{color.blue.400}"
color.ai.suggestion.bg: "#E8F4FD"
color.ai.confidence.high: "{color.success}"
color.ai.confidence.medium: "{color.warning}"
color.ai.confidence.low: "{color.error}"

# Human Verified Indicator
color.verified: "{color.success}"
color.verified.bg: "#D1FAE5"
```

---

## 3. Typography

### Font Families

```yaml
font.family.heading: "'Inter', 'SF Pro Display', sans-serif"
font.family.body: "'Inter', 'SF Pro Text', sans-serif"
font.family.mono: "'JetBrains Mono', 'SF Mono', monospace"
```

### Type Scale

| Token | Size | Weight | Line Height | Letter Spacing | Use Case |
|-------|------|--------|-------------|----------------|----------|
| typography.display.lg | 48px | 700 | 1.2 | -0.02em | Landing hero |
| typography.display.md | 36px | 700 | 1.2 | -0.02em | Page titles |
| typography.display.sm | 30px | 600 | 1.3 | -0.01em | Section headers |
| typography.heading.h1 | 24px | 600 | 1.3 | -0.01em | Main headings |
| typography.heading.h2 | 20px | 600 | 1.4 | 0 | Subheadings |
| typography.heading.h3 | 18px | 600 | 1.4 | 0 | Card headers |
| typography.heading.h4 | 16px | 600 | 1.5 | 0 | Small headers |
| typography.body.lg | 18px | 400 | 1.6 | 0 | Lead paragraphs |
| typography.body.md | 16px | 400 | 1.6 | 0 | Body text |
| typography.body.sm | 14px | 400 | 1.5 | 0 | Secondary text |
| typography.caption | 12px | 400 | 1.5 | 0.01em | Captions, labels |
| typography.overline | 11px | 500 | 1.5 | 0.1em | Overlines, badges |
| typography.code | 14px | 400 | 1.5 | 0 | Code, ICD codes |

### Font Weights

```yaml
font.weight.regular: 400
font.weight.medium: 500
font.weight.semibold: 600
font.weight.bold: 700
```

---

## 4. Spacing

### Base Unit
**Base**: 4px (multiplied for consistent spacing)

### Spacing Scale

```yaml
spacing.0: "0px"
spacing.1: "4px"
spacing.2: "8px"
spacing.3: "12px"
spacing.4: "16px"
spacing.5: "20px"
spacing.6: "24px"
spacing.8: "32px"
spacing.10: "40px"
spacing.12: "48px"
spacing.16: "64px"
spacing.20: "80px"
spacing.24: "96px"
```

### Component Spacing

```yaml
# Padding
padding.button.sm: "{spacing.2} {spacing.3}"   # 8px 12px
padding.button.md: "{spacing.3} {spacing.4}"   # 12px 16px
padding.button.lg: "{spacing.4} {spacing.6}"   # 16px 24px

padding.input.sm: "{spacing.2} {spacing.3}"    # 8px 12px
padding.input.md: "{spacing.3} {spacing.4}"    # 12px 16px

padding.card: "{spacing.4}"                    # 16px
padding.card.lg: "{spacing.6}"                 # 24px

padding.modal: "{spacing.6}"                   # 24px
padding.section: "{spacing.8}"                 # 32px
padding.page: "{spacing.6} {spacing.8}"        # 24px 32px

# Gap
gap.inline.sm: "{spacing.2}"                   # 8px
gap.inline.md: "{spacing.3}"                   # 12px
gap.inline.lg: "{spacing.4}"                   # 16px

gap.stack.sm: "{spacing.3}"                    # 12px
gap.stack.md: "{spacing.4}"                    # 16px
gap.stack.lg: "{spacing.6}"                    # 24px
```

---

## 5. Border Radius

```yaml
radius.none: "0px"
radius.sm: "4px"         # Inputs, badges
radius.md: "8px"         # Buttons, cards
radius.lg: "12px"        # Modals, large cards
radius.xl: "16px"        # Feature sections
radius.2xl: "24px"       # Hero elements
radius.full: "9999px"    # Pills, avatars
```

---

## 6. Elevation & Shadows

### Shadow Scale

```yaml
shadow.none: "none"
shadow.sm: "0 1px 2px 0 rgba(0, 0, 0, 0.05)"
shadow.md: "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1)"
shadow.lg: "0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1)"
shadow.xl: "0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1)"
shadow.2xl: "0 25px 50px -12px rgba(0, 0, 0, 0.25)"
```

### Focus Ring

```yaml
shadow.focus: "0 0 0 3px {color.blue.200}"
shadow.focus.error: "0 0 0 3px {color.error.bg}"
```

### Elevation Levels

| Level | Use Case | Shadow Token |
|-------|----------|--------------|
| 0 | Default surfaces | none |
| 1 | Cards, input focus | shadow.sm |
| 2 | Dropdown menus, hovering cards | shadow.md |
| 3 | Popovers, tooltips | shadow.lg |
| 4 | Modals, drawers | shadow.xl |
| 5 | Floating overlays | shadow.2xl |

---

## 7. Motion & Animation

### Duration

```yaml
duration.instant: "0ms"
duration.fast: "150ms"
duration.normal: "250ms"
duration.slow: "400ms"
duration.slower: "600ms"
```

### Easing

```yaml
easing.default: "cubic-bezier(0.4, 0, 0.2, 1)"     # Standard
easing.in: "cubic-bezier(0.4, 0, 1, 1)"            # Accelerate
easing.out: "cubic-bezier(0, 0, 0.2, 1)"           # Decelerate
easing.inOut: "cubic-bezier(0.4, 0, 0.2, 1)"       # Standard
easing.spring: "cubic-bezier(0.34, 1.56, 0.64, 1)" # Bouncy
```

### Transition Presets

```yaml
transition.default: "{duration.fast} {easing.default}"
transition.fade: "opacity {duration.fast} {easing.default}"
transition.scale: "transform {duration.fast} {easing.default}"
transition.slide: "transform {duration.normal} {easing.out}"
transition.modal: "all {duration.normal} {easing.out}"
```

### Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## 8. Grid & Layout

### Breakpoints

```yaml
breakpoint.sm: "640px"
breakpoint.md: "768px"
breakpoint.lg: "1024px"
breakpoint.xl: "1280px"
breakpoint.2xl: "1440px"
```

### Grid System

```yaml
grid.columns: 12
grid.gutter: "{spacing.4}"       # 16px
grid.gutter.lg: "{spacing.6}"    # 24px
grid.margin: "{spacing.4}"       # 16px mobile
grid.margin.lg: "{spacing.8}"    # 32px desktop
```

### Container Widths

```yaml
container.sm: "640px"
container.md: "768px"
container.lg: "1024px"
container.xl: "1280px"
container.full: "100%"
```

### Layout Patterns

| Pattern | Left | Main | Right | Usage |
|---------|------|------|-------|-------|
| Sidebar + Content | 280px | Flex | - | Staff/Admin portals |
| Content Only | - | Max 1280px centered | - | Patient portal |
| Split View | 50% | - | 50% | Conflict comparison |

---

## 9. Iconography

### Icon Specifications

```yaml
icon.size.xs: "12px"
icon.size.sm: "16px"
icon.size.md: "20px"
icon.size.lg: "24px"
icon.size.xl: "32px"

icon.strokeWidth: "1.5px"
icon.style: "outlined"           # Consistent outlined style
```

### Icon Library
**Recommended**: Heroicons (MIT License) - https://heroicons.com/
- Consistent 24px/20px variants
- Outlined style matches brand
- Healthcare-friendly icons available

### Semantic Icons

| Context | Icon | Usage |
|---------|------|-------|
| Add/Create | plus | Add record, create appointment |
| Edit | pencil | Edit profile, modify data |
| Delete | trash | Remove, delete |
| Success | check-circle | Confirmations, verified |
| Warning | exclamation-triangle | Warnings, conflicts |
| Error | x-circle | Errors, failures |
| Info | information-circle | Tooltips, help |
| AI | sparkles | AI-generated content |
| Verified | shield-check | Human-verified data |
| Calendar | calendar | Appointments, dates |
| Document | document-text | Clinical documents |
| User | user | Patient, profile |
| Settings | cog | Configuration |

---

## 10. Component Specifications

### Button

| Variant | Background | Text | Border | Hover | Active |
|---------|------------|------|--------|-------|--------|
| Primary | {color.primary} | {color.text.inverse} | none | {color.primary.hover} | {color.primary.active} |
| Secondary | transparent | {color.primary} | {color.primary} | {color.primary.subtle} | {color.blue.100} |
| Tertiary | transparent | {color.primary} | none | {color.neutral.100} | {color.neutral.200} |
| Ghost | transparent | {color.text.secondary} | none | {color.neutral.100} | {color.neutral.200} |
| Destructive | {color.error} | {color.text.inverse} | none | {color.error}/90% | {color.error}/80% |

| Size | Height | Padding | Font Size | Radius |
|------|--------|---------|-----------|--------|
| Small | 32px | 8px 12px | 14px | {radius.md} |
| Medium | 40px | 12px 16px | 14px | {radius.md} |
| Large | 48px | 16px 24px | 16px | {radius.md} |

### States

```yaml
button.state.default: # Base styling
button.state.hover: # Subtle background shift
button.state.focus: # Focus ring visible (shadow.focus)
button.state.active: # Pressed visual (darker)
button.state.disabled: # 40% opacity, cursor: not-allowed
button.state.loading: # Spinner replaces text/icon
```

### Input Field

| Property | Value |
|----------|-------|
| Height | 40px (md), 32px (sm), 48px (lg) |
| Border | 1px solid {color.border.default} |
| Border Radius | {radius.md} |
| Background | {color.surface.primary} |
| Padding | 12px 16px |
| Font Size | 14px |

| State | Border | Background | Shadow |
|-------|--------|------------|--------|
| Default | {color.border.default} | {color.surface.primary} | none |
| Hover | {color.border.strong} | {color.surface.primary} | none |
| Focus | {color.border.focus} | {color.surface.primary} | {shadow.focus} |
| Error | {color.border.error} | {color.error.bg}/10% | {shadow.focus.error} |
| Disabled | {color.border.default} | {color.neutral.100} | none |

### Card

```yaml
card.background: "{color.surface.primary}"
card.border: "1px solid {color.border.default}"
card.radius: "{radius.lg}"
card.padding: "{spacing.4}"
card.shadow.default: "{shadow.sm}"
card.shadow.hover: "{shadow.md}"
```

### Modal

```yaml
modal.background: "{color.surface.primary}"
modal.border: "none"
modal.radius: "{radius.lg}"
modal.shadow: "{shadow.xl}"
modal.padding: "{spacing.6}"
modal.width.sm: "400px"
modal.width.md: "560px"
modal.width.lg: "720px"
modal.overlay: "rgba(0, 0, 0, 0.5)"
```

### Toast

```yaml
toast.background: "{color.surface.primary}"
toast.border: "1px solid {color.border.default}"
toast.radius: "{radius.md}"
toast.shadow: "{shadow.lg}"
toast.padding: "{spacing.3} {spacing.4}"
toast.duration: "5000ms"  # Auto-dismiss
toast.position: "top-right"
```

| Type | Icon Color | Background Accent |
|------|------------|-------------------|
| Success | {color.success} | {color.success.bg} |
| Warning | {color.warning} | {color.warning.bg} |
| Error | {color.error} | {color.error.bg} |
| Info | {color.info} | {color.info.bg} |

### Badge

```yaml
badge.height: "20px"
badge.padding: "2px 8px"
badge.radius: "{radius.full}"
badge.fontSize: "11px"
badge.fontWeight: "500"
```

| Variant | Background | Text |
|---------|------------|------|
| Default | {color.neutral.100} | {color.text.secondary} |
| Primary | {color.primary.subtle} | {color.primary} |
| Success | {color.success.bg} | {color.success.text} |
| Warning | {color.warning.bg} | {color.warning.text} |
| Error | {color.error.bg} | {color.error.text} |
| AI | {color.ai.suggestion.bg} | {color.ai.suggestion} |
| Verified | {color.verified.bg} | {color.verified} |

### Data Table

```yaml
table.header.background: "{color.surface.secondary}"
table.header.fontWeight: "600"
table.row.borderBottom: "1px solid {color.border.default}"
table.row.hover: "{color.neutral.50}"
table.cell.padding: "{spacing.3} {spacing.4}"
table.cell.fontSize: "14px"
```

### Skeleton

```yaml
skeleton.background: "{color.neutral.200}"
skeleton.highlight: "{color.neutral.300}"
skeleton.radius: "{radius.md}"
skeleton.animation: "shimmer 1.5s infinite"
```

---

## 11. Accessibility Guidelines

### Contrast Requirements

| Element | Minimum Ratio | WCAG Level |
|---------|---------------|------------|
| Normal text (≤18px) | 4.5:1 | AA |
| Large text (≥18px bold, ≥24px) | 3:1 | AA |
| UI components | 3:1 | AA |
| Focus indicators | 3:1 | AA |
| Icons (informative) | 3:1 | AA |

### Focus Indicators

```yaml
focus.outline.width: "3px"
focus.outline.offset: "2px"
focus.outline.color: "{color.blue.500}"
focus.ring: "{shadow.focus}"
```

### Touch Targets

```yaml
touchTarget.minimum: "44px"
touchTarget.comfortable: "48px"
```

### Screen Reader Support

- All images require alt text (informative) or aria-hidden (decorative)
- Form fields require associated labels via `<label for>` or `aria-labelledby`
- Dynamic content updates use `aria-live` regions
- Interactive elements have descriptive accessible names
- Modals trap focus and return focus on close

---

## 12. Technical Implementation Notes

### CSS Custom Properties

```css
:root {
  /* Color Tokens */
  --color-primary: #2E8FE5;
  --color-secondary: #26BCAF;
  --color-accent: #FF8F2D;
  --color-success: #10B981;
  --color-warning: #F59E0B;
  --color-error: #EF4444;
  
  /* Typography */
  --font-family-heading: 'Inter', sans-serif;
  --font-family-body: 'Inter', sans-serif;
  --font-family-mono: 'JetBrains Mono', monospace;
  
  /* Spacing */
  --spacing-1: 4px;
  --spacing-2: 8px;
  --spacing-4: 16px;
  --spacing-6: 24px;
  --spacing-8: 32px;
  
  /* Radius */
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-full: 9999px;
  
  /* Motion */
  --duration-fast: 150ms;
  --duration-normal: 250ms;
  --easing-default: cubic-bezier(0.4, 0, 0.2, 1);
}

[data-theme="dark"] {
  --color-surface-primary: #1F2937;
  --color-surface-secondary: #111827;
  --color-text-primary: #F9FAFB;
  --color-border-default: #374151;
}
```

### React/Component Library

**Recommended Stack**:
- **Base**: shadcn/ui (Radix primitives + Tailwind)
- **Icons**: @heroicons/react
- **Animation**: Framer Motion
- **Forms**: React Hook Form + Zod

### Tailwind Configuration

```javascript
// tailwind.config.js
module.exports = {
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#E8F4FD',
          500: '#2E8FE5',
          600: '#2573C4',
          700: '#1C57A3',
        },
        // ... other colors
      },
      fontFamily: {
        heading: ['Inter', 'SF Pro Display', 'sans-serif'],
        body: ['Inter', 'SF Pro Text', 'sans-serif'],
        mono: ['JetBrains Mono', 'SF Mono', 'monospace'],
      },
      borderRadius: {
        sm: '4px',
        md: '8px',
        lg: '12px',
      },
    },
  },
}
```

---

## 13. Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-03-16 | Initial design system creation |
