# Design Reference — Patient Access & Clinical Intelligence Platform

## UI Impact Assessment

**Has UI Changes**: [x] Yes [ ] No

## Design System Context

**Platform**: Web (Responsive — Desktop, Tablet, Mobile)
**Frontend Stack**: React 18 + TypeScript 5.x + Redux Toolkit 2.x + Tailwind CSS
**Design Source**: `.propel/context/docs/figma_spec.md`

---

## 1. Design Tokens

### 1.1 Color Palette

#### Primary Colors

```yaml
colors:
  primary:
    50: "#EBF5FF"
    100: "#D1E9FF"
    200: "#A8D4FF"
    300: "#6FB6FF"
    400: "#3D9BFF"
    500: "#0F62FE"    # Primary brand blue
    600: "#0C50D4"
    700: "#0A3FAA"
    800: "#072F80"
    900: "#051F56"
    usage: "Primary CTAs, active states, links, navigation highlights"

  secondary:
    50: "#F0FDF4"
    100: "#DCFCE7"
    200: "#BBF7D0"
    300: "#86EFAC"
    400: "#4ADE80"
    500: "#16A34A"    # Secondary green — verification, success
    600: "#15803D"
    700: "#166534"
    800: "#14532D"
    900: "#052E16"
    usage: "Staff-verified badges, success states, positive indicators"
```

#### Semantic Colors

```yaml
semantic:
  success:
    light: "#DCFCE7"
    default: "#16A34A"
    dark: "#166534"
    usage: "Verified data, successful operations, available slots"

  warning:
    light: "#FEF3C7"
    default: "#D97706"
    dark: "#92400E"
    usage: "AI-suggested data badges, pending verification, waitlist status"

  error:
    light: "#FEE2E2"
    default: "#DC2626"
    dark: "#991B1B"
    usage: "Validation errors, failed operations, critical conflicts"

  info:
    light: "#DBEAFE"
    default: "#2563EB"
    dark: "#1E40AF"
    usage: "Informational tooltips, help text, system notices"
```

#### Neutral Colors

```yaml
neutral:
  0: "#FFFFFF"
  50: "#F8FAFC"
  100: "#F1F5F9"
  200: "#E2E8F0"
  300: "#CBD5E1"
  400: "#94A3B8"
  500: "#64748B"
  600: "#475569"
  700: "#334155"
  800: "#1E293B"
  900: "#0F172A"
  usage: "Backgrounds, borders, text, disabled states"
```

#### Dark Mode Variants

```yaml
dark_mode:
  background:
    primary: "#0F172A"     # neutral-900
    secondary: "#1E293B"   # neutral-800
    elevated: "#334155"    # neutral-700
  text:
    primary: "#F8FAFC"     # neutral-50
    secondary: "#CBD5E1"   # neutral-300
    muted: "#94A3B8"       # neutral-400
  border:
    default: "#475569"     # neutral-600
    subtle: "#334155"      # neutral-700
  surface:
    card: "#1E293B"
    modal: "#334155"
```

### 1.2 Typography

```yaml
typography:
  font_family:
    heading: "'Inter', -apple-system, BlinkMacSystemFont, sans-serif"
    body: "'Inter', -apple-system, BlinkMacSystemFont, sans-serif"
    mono: "'JetBrains Mono', 'Fira Code', monospace"

  scale:
    h1:
      size: "36px"
      weight: "700"
      line_height: "44px"
      letter_spacing: "-0.02em"
      usage: "Page titles (Dashboard, Health Dashboard)"

    h2:
      size: "28px"
      weight: "600"
      line_height: "36px"
      letter_spacing: "-0.01em"
      usage: "Section headers (Appointments, Clinical Data)"

    h3:
      size: "22px"
      weight: "600"
      line_height: "28px"
      usage: "Card titles, subsection headers"

    h4:
      size: "18px"
      weight: "600"
      line_height: "24px"
      usage: "Sub-headers, modal titles"

    h5:
      size: "16px"
      weight: "600"
      line_height: "22px"
      usage: "Compact headers, sidebar sections"

    h6:
      size: "14px"
      weight: "600"
      line_height: "20px"
      usage: "Table headers, badge labels"

    body_lg:
      size: "16px"
      weight: "400"
      line_height: "24px"
      usage: "Primary body text, form labels"

    body:
      size: "14px"
      weight: "400"
      line_height: "20px"
      usage: "Default body text, table cells"

    body_sm:
      size: "13px"
      weight: "400"
      line_height: "18px"
      usage: "Helper text, secondary info"

    caption:
      size: "12px"
      weight: "400"
      line_height: "16px"
      usage: "Timestamps, badges, footnotes"

    overline:
      size: "11px"
      weight: "600"
      line_height: "16px"
      letter_spacing: "0.05em"
      text_transform: "uppercase"
      usage: "Labels, category tags"
```

### 1.3 Spacing

```yaml
spacing:
  base_unit: "4px"
  scale:
    0: "0px"
    0.5: "2px"
    1: "4px"
    1.5: "6px"
    2: "8px"
    3: "12px"
    4: "16px"
    5: "20px"
    6: "24px"
    8: "32px"
    10: "40px"
    12: "48px"
    16: "64px"
    20: "80px"
  usage:
    component_padding: "12px–16px"
    card_padding: "16px–24px"
    section_gap: "24px–32px"
    page_margin: "24px (mobile) / 32px (tablet) / 48px (desktop)"
```

### 1.4 Border Radius

```yaml
radius:
  none: "0px"
  sm: "4px"          # Inputs, checkboxes
  md: "8px"          # Buttons, cards, modals
  lg: "16px"         # Large cards, containers
  xl: "24px"         # Feature cards, hero sections
  full: "9999px"     # Avatars, badges, pills, toggles
```

### 1.5 Elevation / Shadows

```yaml
elevation:
  level_0:
    value: "none"
    usage: "Flat elements, inline content"

  level_1:
    value: "0 1px 3px rgba(0,0,0,0.08), 0 1px 2px rgba(0,0,0,0.06)"
    usage: "Cards, dropdown menus"

  level_2:
    value: "0 4px 6px rgba(0,0,0,0.07), 0 2px 4px rgba(0,0,0,0.06)"
    usage: "Elevated cards, popovers"

  level_3:
    value: "0 10px 15px rgba(0,0,0,0.1), 0 4px 6px rgba(0,0,0,0.05)"
    usage: "Modals, drawers"

  level_4:
    value: "0 20px 25px rgba(0,0,0,0.1), 0 8px 10px rgba(0,0,0,0.04)"
    usage: "Overlays floating above content"

  level_5:
    value: "0 25px 50px rgba(0,0,0,0.15)"
    usage: "Toasts, sticky elements"

  dark_mode_note: "Reduce opacity by 50% in dark mode; use border-based distinction instead of shadow"
```

### 1.6 Motion / Transitions

```yaml
motion:
  duration:
    fast: "100ms"        # Hover states, toggles
    normal: "200ms"      # Button presses, dropdown open
    moderate: "300ms"     # Modal open/close, drawer slide
    slow: "500ms"        # Page transitions, skeleton fade

  easing:
    ease_out: "cubic-bezier(0.16, 1, 0.3, 1)"       # Entry animations
    ease_in_out: "cubic-bezier(0.45, 0, 0.55, 1)"   # Toggle/reposition
    ease_in: "cubic-bezier(0.55, 0.055, 0.675, 0.19)" # Exit animations

  reduced_motion: "Honor prefers-reduced-motion: disable transforms, crossfade only"
```

### 1.7 Breakpoints

```yaml
breakpoints:
  mobile: "320px"        # Min mobile width
  mobile_lg: "425px"     # Large mobile
  tablet: "768px"        # Tablet portrait
  desktop: "1024px"      # Desktop baseline
  desktop_lg: "1440px"   # Large desktop / design baseline
  desktop_xl: "1920px"   # Wide screens

grid:
  columns: 12
  gutter:
    mobile: "16px"
    tablet: "24px"
    desktop: "32px"
  max_width: "1280px"
  margin: "auto (centered)"
```

---

## 2. Component Library Reference

### 2.1 Actions

#### Button

```yaml
button:
  variants:
    - type: [Primary, Secondary, Tertiary, Ghost, Danger]
    - size: [Small (32px), Medium (40px), Large (48px)]
    - state: [Default, Hover, Focus, Active, Disabled, Loading]
    - icon: [None, Leading, Trailing, IconOnly]
  specs:
    primary:
      background: "primary-500"
      text: "neutral-0 (white)"
      hover: "primary-600"
      focus: "primary-500 + 2px ring primary-200"
      disabled: "neutral-200, text neutral-400"
      loading: "Spinner replacing text, preserve width"
    border_radius: "md (8px)"
    min_width: "120px (text), 40px (icon-only)"
    font: "body, weight 500"
```

#### Link

```yaml
link:
  variants:
    - type: [Default, Subtle]
    - size: [Small, Medium]
    - state: [Default, Hover, Focus, Visited]
  specs:
    color: "primary-500"
    hover: "primary-700, underline"
    focus: "2px ring primary-200"
```

### 2.2 Inputs

#### TextField

```yaml
text_field:
  variants:
    - size: [Small (32px), Medium (40px), Large (48px)]
    - state: [Default, Focus, Error, Disabled, Filled]
    - adornment: [None, Leading icon, Trailing icon, Prefix, Suffix]
  specs:
    border: "1px solid neutral-300"
    focus_border: "2px solid primary-500"
    error_border: "2px solid error-default"
    border_radius: "md (8px)"
    padding: "8px 12px"
    label: "body_sm, neutral-700, above field"
    helper_text: "caption, neutral-500, below field"
    error_text: "caption, error-default, below field (replaces helper)"
    placeholder: "neutral-400"
```

#### Select

```yaml
select:
  variants:
    - size: [Small, Medium, Large]
    - state: [Default, Open, Focus, Error, Disabled]
  specs:
    extends: "TextField base styles"
    dropdown: "Elevation level-2, border-radius md, max-height 240px"
    option_height: "40px"
    active_option: "primary-50 background"
```

#### Checkbox / Radio

```yaml
checkbox_radio:
  size: "20px (touch target 44px)"
  checked_color: "primary-500"
  unchecked_border: "neutral-300"
  focus: "2px ring primary-200"
  disabled: "neutral-200"
  label_spacing: "8px gap"
```

#### Toggle

```yaml
toggle:
  size:
    track: "44px x 24px"
    thumb: "20px"
  colors:
    off: "neutral-300"
    on: "primary-500"
  transition: "200ms ease-in-out"
```

#### FileUpload

```yaml
file_upload:
  specs:
    area: "Dashed border, neutral-200, 16px padding"
    hover: "primary-50 background, primary-300 dashed border"
    active_drag: "primary-100 background, primary-500 dashed border"
    accepted_formats: "PDF only (per spec)"
    max_size: "10MB (per NFR-010)"
    progress: "ProgressBar per file, percentage + file name"
```

### 2.3 Navigation

#### Header

```yaml
header:
  height: "64px"
  background: "neutral-0 (light) / neutral-900 (dark)"
  border_bottom: "1px solid neutral-200"
  content: "Logo (left), Breadcrumb (center, desktop), UserMenu (right)"
  z_index: "50"
```

#### Sidebar

```yaml
sidebar:
  width:
    expanded: "240px"
    collapsed: "64px"
  background: "neutral-50 (light) / neutral-800 (dark)"
  item_height: "44px"
  active_item: "primary-50 background, primary-500 text, left-border 3px primary-500"
  hover: "neutral-100 background"
  behavior:
    desktop: "Persistent, collapsible"
    mobile: "Hidden, overlay drawer on hamburger tap"
```

#### BottomNav

```yaml
bottom_nav:
  height: "64px"
  max_items: 5
  visibility: "Mobile only (< 768px)"
  active_item: "primary-500 icon + label"
  inactive: "neutral-500 icon + label"
```

#### Tabs

```yaml
tabs:
  height: "48px"
  active: "primary-500 text, 2px bottom border primary-500"
  inactive: "neutral-600 text"
  hover: "neutral-100 background"
```

### 2.4 Content

#### Card

```yaml
card:
  padding: "16px (compact) / 24px (standard)"
  border_radius: "md (8px)"
  border: "1px solid neutral-200"
  elevation: "level-1"
  hover: "level-2 (when clickable)"
```

#### StatCard

```yaml
stat_card:
  extends: "Card"
  layout: "Icon (left or top), Value (h3), Label (caption), Trend indicator"
  trend_positive: "success-default, arrow-up"
  trend_negative: "error-default, arrow-down"
```

#### Table

```yaml
table:
  header:
    background: "neutral-50"
    font: "h6 (14px semibold)"
    height: "44px"
    sortable: "Arrow icon suffix"
  row:
    height: "52px"
    hover: "neutral-50 background"
    border_bottom: "1px solid neutral-100"
    selected: "primary-50 background"
  alignment: "Text left, numbers right, status center"
  pagination: "Below table, 20 items/page default"
```

#### Badge

```yaml
badge:
  variants:
    - type: [Status, Count, Label]
    - semantic: [Success, Warning, Error, Info, Neutral]
  specs:
    padding: "2px 8px"
    border_radius: "full (9999px)"
    font: "caption (12px) weight 500"
    ai_suggested: "warning-light background, warning-dark text"
    staff_verified: "success-light background, success-dark text"
    unverified: "neutral-100 background, neutral-600 text"
    conflict: "error-light background, error-dark text"
```

#### Accordion

```yaml
accordion:
  header_height: "52px"
  icon: "Chevron right (collapsed) / Chevron down (expanded)"
  transition: "300ms ease-out"
  divider: "1px solid neutral-200 between items"
```

#### ChatBubble

```yaml
chat_bubble:
  user:
    background: "primary-500"
    text: "neutral-0 (white)"
    alignment: "Right"
    border_radius: "16px 16px 4px 16px"
  ai:
    background: "neutral-100"
    text: "neutral-800"
    alignment: "Left"
    border_radius: "16px 16px 16px 4px"
  max_width: "75%"
  typing_indicator: "3 animated dots, neutral-400"
```

### 2.5 Feedback

#### Modal

```yaml
modal:
  width:
    small: "400px"
    medium: "560px"
    large: "720px"
  max_height: "85vh"
  border_radius: "lg (16px)"
  elevation: "level-3"
  overlay: "neutral-900 at 50% opacity"
  padding: "24px"
  header: "h4, close icon top-right"
  footer: "Right-aligned buttons, 8px gap"
  animation: "Fade-in 200ms + scale from 95%"
  trap_focus: "Yes — cycle within modal"
  close: "Escape key, overlay click, X button"
```

#### Dialog

```yaml
dialog:
  extends: "Modal (small)"
  content: "Title, description, action buttons"
  destructive: "Danger button right, Cancel left"
```

#### Drawer

```yaml
drawer:
  width: "480px (desktop) / 100% (mobile)"
  slide_from: "Right"
  overlay: "neutral-900 at 30% opacity"
  elevation: "level-4"
  animation: "Slide 300ms ease-out"
```

#### Toast

```yaml
toast:
  position: "Top-right (desktop) / Top-center (mobile)"
  width: "360px max"
  duration: "5s auto-dismiss (configurable)"
  variants: [Success, Error, Warning, Info]
  icon: "Left, semantic color"
  dismiss: "X button right"
  animation: "Slide-in from right, fade-out"
  max_visible: 3
  stacking: "12px gap"
```

#### Skeleton

```yaml
skeleton:
  animation: "Pulse (opacity 0.5 to 1), 1.5s cycle"
  shapes: "Rectangle, circle, text lines"
  color: "neutral-200 (light) / neutral-700 (dark)"
  usage: "Replace content areas 1:1 in loading state"
```

#### EmptyState

```yaml
empty_state:
  layout: "Centered — illustration (max 200px), heading (h3), description (body), CTA button"
  illustration_style: "Flat, minimal, healthcare-themed, neutral tones with primary accent"
```

#### ProgressBar

```yaml
progress_bar:
  height: "8px (default) / 4px (compact)"
  track: "neutral-200"
  fill: "primary-500"
  border_radius: "full"
  label: "Percentage right-aligned (optional)"
  indeterminate: "Animated gradient slide"
```

### 2.6 Data Visualization

#### LineChart

```yaml
line_chart:
  usage: "Vital trend display on SCR-016, SCR-017"
  colors: "primary-500 (primary line), secondary-500 (comparison)"
  grid: "Horizontal lines, neutral-100"
  tooltip: "On hover, show data point value + date"
  axes: "X-axis dates, Y-axis values, neutral-500 labels"
```

#### Calendar

```yaml
calendar:
  cell_size: "44px min (touch target)"
  today: "Primary-500 ring"
  selected: "Primary-500 background, white text"
  available: "Neutral-0 background, neutral-800 text"
  unavailable: "Neutral-100 background, neutral-400 text, strikethrough"
  navigation: "Month/Year header with prev/next arrows"
```

#### TimeSlotGrid

```yaml
time_slot:
  height: "44px"
  available: "Primary-50 background, primary-700 text, primary-200 border"
  selected: "Primary-500 background, white text"
  unavailable: "Neutral-100 background, neutral-400 text"
  preferred_swap: "Warning-light background, warning-dark border (dashed)"
  hover: "Primary-100 background"
  layout: "3-column grid (desktop), 2-column (tablet), 1-column (mobile)"
```

---

## 3. Brand Guidelines

### Logo

- **Primary**: "PatientAccess" wordmark with healthcare cross icon — primary-500 (#0F62FE) on white
- **Inverse**: White wordmark on dark backgrounds (neutral-800+)
- **Minimum size**: 120px wide
- **Clear space**: 1x height of cross icon on all sides

### Iconography

- **Style**: Outlined, 24px default, 1.5px stroke weight
- **Library**: Lucide Icons or Heroicons Outline (consistent set)
- **Color**: Inherits text color of parent context
- **Sizes**: 16px (inline), 20px (compact), 24px (default), 32px (feature)

### Illustration

- **Style**: Flat, minimal, healthcare-appropriate
- **Usage**: Empty states, onboarding prompts, error pages
- **Palette**: Neutral tones (#E2E8F0, #94A3B8) with primary-500 accent
- **Max dimensions**: 200px × 200px for inline; 400px × 300px for full-page

---

## 4. Accessibility Requirements

- **WCAG Level**: AA (all screens)
- **Color Contrast**: ≥4.5:1 normal text, ≥3:1 large text and UI components
- **Focus States**: 2px outline, offset 2px, primary-500, contrast ≥3:1 against background
- **Touch Targets**: ≥44x44px for all interactive elements on mobile
- **Screen Reader**: ARIA labels on all interactive elements; live regions for dynamic updates
- **Keyboard Navigation**: All flows completable via keyboard alone; no focus traps; logical tab order
- **Reduced Motion**: Respect `prefers-reduced-motion`; crossfade only when enabled
- **Semantic HTML**: `<nav>`, `<main>`, `<aside>`, `<header>`, `<footer>` landmarks on all pages

---

## 5. Design Review Checklist

- [x] UI Impact confirmed — all 26 screens have design specifications
- [x] Design tokens extracted for all components (colors, typography, spacing, radius, elevation, motion)
- [x] Component specifications documented with variants and states
- [x] Visual validation criteria defined (AI-suggested vs. verified badges)
- [x] Responsive behavior specified (320px–1440px breakpoints)
- [x] Accessibility requirements noted (WCAG 2.2 AA)
- [x] Light and dark mode token parity defined
