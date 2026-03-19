# Design System - PulseCare Platform

## Document Overview

**System Name**: PulseCare Design System  
**Version**: 1.0.0 (Phase 1 MVP)  
**Last Updated**: 2024  
**Technology Stack**: React 18+ TypeScript, Next.js 14+, Tailwind CSS 3.4+, shadcn/ui  
**Purpose**: Authoritative source for design tokens, component specifications, and brand guidelines for the Unified Patient Access & Clinical Intelligence Platform

**Related Documents**:
- [Figma Specification](./figma_spec.md) - Screen inventory, UX requirements, prototype flows
- [Requirements Specification](./spec.md) - Functional requirements and use cases
- [Architecture Design](./design.md) - Non-functional requirements and technical constraints

---

## 1. Design Principles

### Core Values
1. **Accessibility First**: WCAG 2.2 Level AA compliance is non-negotiable; design for assistive technologies from the start
2. **Performance Conscious**: Free-tier infrastructure constraint requires lightweight assets and optimized rendering
3. **Healthcare Trustworthy**: Professional, secure aesthetic appropriate for HIPAA-compliant PHI handling
4. **Progressive Disclosure**: Reveal complexity gradually; staff dashboards information-dense, patient interfaces streamlined
5. **Consistent by Default**: Design token usage enforced; no arbitrary values allowed in implementation

### Design System Goals
- **Consistency**: Ensure visual and interaction consistency across 18 screens (SCR-001 to SCR-018)
- **Efficiency**: Enable rapid prototyping and development with reusable components
- **Scalability**: Support future features without design system fragmentation
- **Maintainability**: Single source of truth for design decisions; update propagates automatically

---

## 2. Design Tokens

*All tokens mapped to Tailwind CSS configuration and CSS custom properties for React implementation*

### 2.1 Color Palette

#### Foundation Colors (Tailwind Default Extended)

**Primary (Blue) - Trust, Healthcare Standard**
```yaml
primary:
  50: "#eff6ff"   # Lightest background tints
  100: "#dbeafe"  # Hover states, light backgrounds
  200: "#bfdbfe"  # Disabled states
  300: "#93c5fd"  # Borders, dividers
  400: "#60a5fa"  # Secondary actions
  500: "#3b82f6"  # PRIMARY brand color (buttons, links)
  600: "#2563eb"  # Hover states for primary actions
  700: "#1d4ed8"  # Active/pressed states
  800: "#1e40af"  # Dark backgrounds
  900: "#1e3a8a"  # Darkest text on light backgrounds
```

**Success (Green) - Health, Completion**
```yaml
success:
  50: "#f0fdf4"
  100: "#dcfce7"
  500: "#22c55e"  # Success messages, checkmarks, badges
  600: "#16a34a"  # Hover states
  700: "#15803d"  # Active states
```

**Warning (Yellow) - Caution, Review Required**
```yaml
warning:
  50: "#fefce8"
  100: "#fef9c3"
  500: "#eab308"  # Warning badges, low-confidence AI extractions (70-90%)
  600: "#ca8a04"  # Hover states
  700: "#a16207"  # Active states
```

**Error (Red) - Critical Issues, Conflicts**
```yaml
error:
  50: "#fef2f2"
  100: "#fee2e2"
  500: "#ef4444"  # Error messages, conflict alerts, validation errors
  600: "#dc2626"  # Hover states for destructive actions
  700: "#b91c1c"  # Active states
```

**Neutral (Gray) - Text, Backgrounds, Borders**
```yaml
neutral:
  50: "#f9fafb"   # Page backgrounds (light mode)
  100: "#f3f4f6"  # Card backgrounds, secondary backgrounds
  200: "#e5e7eb"  # Borders, dividers
  300: "#d1d5db"  # Disabled text, placeholders
  400: "#9ca3af"  # Secondary text, icons
  500: "#6b7280"  # Body text (on white backgrounds)
  600: "#4b5563"  # Headings, emphasized text
  700: "#374151"  # High-emphasis text
  800: "#1f2937"  # Darkest text (near-black)
  900: "#111827"  # Pure black text (sparingly used)
```

#### Semantic Color Mapping

**Application to UI Elements**
```yaml
# Text Colors
text-primary: neutral-700       # Headings, high-emphasis text
text-secondary: neutral-500     # Body text, descriptions
text-tertiary: neutral-400      # Captions, metadata, timestamps
text-disabled: neutral-300      # Disabled form inputs, inactive states
text-inverse: neutral-50        # Text on dark backgrounds (future dark mode)

# Background Colors
bg-canvas: neutral-50           # Page background
bg-surface: "#ffffff"           # Card surfaces, modals, form containers
bg-elevated: "#ffffff"          # Elevated cards (with shadow)
bg-subtle: neutral-100          # Secondary backgrounds, table alternating rows
bg-overlay: "rgba(0,0,0,0.5)"   # Modal backdrop (50% black)

# Border Colors
border-default: neutral-200     # Standard borders (cards, inputs)
border-subtle: neutral-100      # Dividers, low-emphasis separators
border-emphasis: neutral-300    # Focused borders (before brand color)

# Interactive States
state-hover: primary-100        # Hover background for interactive elements
state-active: primary-200       # Active/pressed background
state-focus: primary-500        # Focus ring color (2px solid outline)
state-disabled: neutral-100     # Disabled background

# Status Colors (Applied to Badges, Alerts, Icons)
status-success: success-500     # "Intake Complete", "Arrived", "Verified"
status-warning: warning-500     # "Review Required", "Late Arrival", "Low Confidence"
status-error: error-500         # "Conflict Detected", "Validation Error", "Failed"
status-info: primary-500        # "Scheduled", "In Progress", "Info"
```

### 2.2 Typography

#### Font Families
```yaml
font-primary: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Helvetica Neue', Arial, sans-serif"
font-code: "'JetBrains Mono', 'Fira Code', Consolas, Monaco, 'Courier New', monospace"
```

**Rationale**: Inter (Google Fonts) provides excellent readability at small sizes, professional appearance for healthcare context, and extensive OpenType features. JetBrains Mono for code snippets in error messages or audit logs (monospaced for alignment).

#### Type Scale (Tailwind CSS Defaults + Extensions)

**Size Scale**
```yaml
text-xs: "0.75rem"      # 12px - Captions, badges, metadata
text-sm: "0.875rem"     # 14px - Secondary text, form labels
text-base: "1rem"       # 16px - Body text (DEFAULT)
text-lg: "1.125rem"     # 18px - Emphasized body text, large buttons
text-xl: "1.25rem"      # 20px - Section headings
text-2xl: "1.5rem"      # 24px - Page headings (h3)
text-3xl: "1.875rem"    # 30px - Major headings (h2)
text-4xl: "2.25rem"     # 36px - Hero headings (h1)
text-5xl: "3rem"        # 48px - Landing page titles (future)
```

**Font Weights**
```yaml
font-normal: 400        # Body text, descriptions
font-medium: 500        # Emphasized text, form labels, button text
font-semibold: 600      # Headings (h3, h4), active nav items
font-bold: 700          # Major headings (h1, h2), critical alerts
```

**Line Heights**
```yaml
leading-tight: 1.25     # Headings, large text (h1, h2)
leading-snug: 1.375     # Subheadings (h3, h4)
leading-normal: 1.5     # Body text (DEFAULT for paragraphs)
leading-relaxed: 1.625  # Long-form content (future blog/help docs)
```

**Letter Spacing**
```yaml
tracking-tight: "-0.025em"  # Large headings (h1, h2) to reduce visual weight
tracking-normal: "0"        # Body text (DEFAULT)
tracking-wide: "0.025em"    # All-caps labels, buttons
```

#### Typography Usage Rules

**Heading Hierarchy**
```yaml
h1:
  size: text-4xl (36px desktop) / text-3xl (30px mobile)
  weight: font-bold (700)
  color: text-primary (neutral-700)
  line-height: leading-tight (1.25)
  usage: "Page titles (e.g., 'Patient Dashboard', 'Arrival Management')"

h2:
  size: text-3xl (30px desktop) / text-2xl (24px mobile)
  weight: font-bold (700)
  color: text-primary (neutral-700)
  line-height: leading-tight (1.25)
  usage: "Major section headings (e.g., 'Upcoming Appointments', '360-Degree Patient View')"

h3:
  size: text-2xl (24px desktop) / text-xl (20px mobile)
  weight: font-semibold (600)
  color: text-primary (neutral-700)
  line-height: leading-snug (1.375)
  usage: "Subsection headings (e.g., 'Patient Information', 'Documents')"

h4:
  size: text-xl (20px)
  weight: font-semibold (600)
  color: text-secondary (neutral-500)
  line-height: leading-snug (1.375)
  usage: "Card titles, form section labels"

body:
  size: text-base (16px)
  weight: font-normal (400)
  color: text-secondary (neutral-500)
  line-height: leading-normal (1.5)
  usage: "Paragraphs, form help text, descriptions"

caption:
  size: text-sm (14px) / text-xs (12px for timestamps)
  weight: font-normal (400)
  color: text-tertiary (neutral-400)
  line-height: leading-normal (1.5)
  usage: "Metadata, timestamps, secondary information"
```

### 2.3 Spacing Scale

**Base Unit: 4px** (Tailwind default scale)

```yaml
spacing:
  0: "0"          # No space
  1: "0.25rem"    # 4px - Tight spacing (icon-text gap)
  2: "0.5rem"     # 8px - Minimum tap target padding
  3: "0.75rem"    # 12px - Form label-input gap
  4: "1rem"       # 16px - Default padding (buttons, cards)
  5: "1.25rem"    # 20px - Section spacing
  6: "1.5rem"     # 24px - Card internal spacing
  8: "2rem"       # 32px - Component separation
  10: "2.5rem"    # 40px - Large component gaps
  12: "3rem"      # 48px - Section separation
  16: "4rem"      # 64px - Major section breaks
  20: "5rem"      # 80px - Page-level spacing
  24: "6rem"      # 96px - Hero spacing (landing pages)
```

**Usage Guidelines**
- **Tight spacing (1-2)**: Icon-text gaps, badge padding
- **Default spacing (4)**: Button padding, form input padding, card padding
- **Component spacing (6-8)**: Gaps between form fields, list items, table cells
- **Section spacing (12-16)**: Separation between major page sections
- **Page-level spacing (20-24)**: Hero sections, landing pages

### 2.4 Border Radius

```yaml
radius:
  none: "0"           # Square corners (tables, strict data views)
  sm: "0.25rem"       # 4px - Badges, small buttons
  DEFAULT: "0.5rem"   # 8px - Buttons, cards, inputs (MOST COMMON)
  md: "0.5rem"        # 8px - Alias for DEFAULT
  lg: "0.75rem"       # 12px - Large cards, modals
  xl: "1rem"          # 16px - Hero sections, feature cards
  2xl: "1.5rem"       # 24px - Prominent CTAs (future)
  full: "9999px"      # Fully rounded (avatars, pills, badges)
```

**Application**
- **Buttons**: `radius-DEFAULT (8px)` for standard, `radius-full` for pill-shaped
- **Cards**: `radius-DEFAULT (8px)` for standard, `radius-lg (12px)` for elevated
- **Inputs**: `radius-DEFAULT (8px)` for consistency with buttons
- **Avatars**: `radius-full` for circular profile images
- **Badges**: `radius-sm (4px)` for compact look or `radius-full` for pill badges

### 2.5 Shadows (Elevation)

```yaml
shadow:
  none: "0 0 #0000"                                          # No shadow (flat design)
  sm: "0 1px 2px 0 rgb(0 0 0 / 0.05)"                       # Subtle elevation (form inputs, borders)
  DEFAULT: "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)"  # Cards, buttons
  md: "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)"    # Elevated cards, dropdowns
  lg: "0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)"  # Modals, popovers
  xl: "0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)" # Drawers, overlays
  2xl: "0 25px 50px -12px rgb(0 0 0 / 0.25)"                # Prominent modals (rarely used)
```

**Elevation Hierarchy**
- **Level 0** (`shadow-none`): Page background, flat components
- **Level 1** (`shadow-sm`): Form inputs, subtle borders
- **Level 2** (`shadow-DEFAULT`): Cards, buttons, default elevation
- **Level 3** (`shadow-md`): Dropdowns, tooltips, elevated cards
- **Level 4** (`shadow-lg`): Modals, dialogs requiring attention
- **Level 5** (`shadow-xl`): Drawers, side panels

### 2.6 Transitions & Animations

**Duration**
```yaml
duration:
  75: "75ms"      # Instant feedback (button press)
  100: "100ms"    # Fast transitions (hover effects)
  150: "150ms"    # Standard transitions (most interactions)
  200: "200ms"    # Moderate transitions (modals opening)
  300: "300ms"    # Slow transitions (page transitions)
  500: "500ms"    # Very slow (drawer slide, rarely used)
```

**Easing Functions**
```yaml
ease-linear: "linear"                          # Constant speed (loading spinners)
ease-in: "cubic-bezier(0.4, 0, 1, 1)"          # Slow start, fast end (elements exiting)
ease-out: "cubic-bezier(0, 0, 0.2, 1)"         # Fast start, slow end (elements entering) - DEFAULT
ease-in-out: "cubic-bezier(0.4, 0, 0.2, 1)"    # Slow start and end (dialog open/close)
```

**Animation Constraints**
- **Performance**: Only animate `transform` and `opacity` properties (hardware-accelerated)
- **Accessibility**: Respect `prefers-reduced-motion` media query; disable animations for users with vestibular disorders
- **Max Duration**: 300ms for interactive elements (UXR-503 60fps requirement)
- **Interaction Feedback**: Must occur within 200ms (UXR-501)

---

## 3. Component Library

*All components based on shadcn/ui with customizations via CSS variables and Tailwind utilities*

### 3.1 Actions Category

#### Button Component

**Variants**
- **Primary**: Brand-colored, highest emphasis (booking, saving, submitting)
- **Secondary**: Neutral-colored, medium emphasis (canceling, navigating)
- **Destructive**: Red-colored, for delete/destructive actions
- **Ghost**: Transparent background, low emphasis (tertiary actions, icons)
- **Link**: Styled as hyperlink, minimal emphasis

**Sizes**
- **Small (S)**: `h-8 px-3 text-sm` (28px height) - Compact tables, inline actions
- **Medium (M)**: `h-10 px-4 text-base` (40px height) - DEFAULT, forms, general use
- **Large (L)**: `h-12 px-6 text-lg` (48px height) - CTAs, hero sections

**States**
- **Default**: Base appearance
- **Hover**: Background darkens (primary-600), cursor pointer
- **Focus**: 2px solid outline (primary-500), visible keyboard focus
- **Active**: Background darker (primary-700), pressed appearance
- **Disabled**: Opacity 50%, cursor not-allowed, non-interactive
- **Loading**: Spinner icon replaces text or prepends, button disabled

**Specifications**
```typescript
// React TypeScript Component API
<Button
  variant="primary" | "secondary" | "destructive" | "ghost" | "link"
  size="sm" | "md" | "lg"
  disabled={boolean}
  loading={boolean}
  icon={ReactNode}          // Optional icon (prepended to text)
  fullWidth={boolean}       // Stretch to container width (mobile forms)
  onClick={Function}
>
  Button Text
</Button>
```

**Visual Tokens**
```yaml
Button.Primary:
  background: primary-500
  color: white
  borderRadius: radius-DEFAULT (8px)
  padding: "h-10 px-4" (medium default)
  fontSize: text-base (16px)
  fontWeight: font-medium (500)
  shadow: shadow-sm
  hover:
    background: primary-600
  focus:
    outline: "2px solid" primary-500
    outlineOffset: 2px
  active:
    background: primary-700
  disabled:
    opacity: 0.5
    cursor: "not-allowed"

Button.Secondary:
  background: neutral-200
  color: neutral-700
  [other properties same as Primary]

Button.Destructive:
  background: error-500
  color: white
  hover:
    background: error-600
  [other properties same as Primary]

Button.Ghost:
  background: transparent
  color: neutral-600
  hover:
    background: neutral-100
  [no shadow]

Button.Link:
  background: transparent
  color: primary-600
  textDecoration: underline
  hover:
    color: primary-700
  [no padding, no shadow]
```

**Accessibility**
- ARIA attribute: `role="button"` (if not using `<button>` element)
- Keyboard: Activates on Enter or Space
- Screen reader: Button text must be descriptive (avoid "Click here")
- Loading state: `aria-busy="true"` when loading
- Disabled state: `aria-disabled="true"` + `disabled` attribute

**Usage Examples**
- Primary: "Book Appointment", "Confirm Booking", "Submit Intake"
- Secondary: "Cancel", "Back", "Skip"
- Destructive: "Delete User", "Deactivate Account", "Remove Document"
- Ghost: Icon-only actions in tables (edit, delete, view)
- Link: "Learn more", "View details", navigation within text

---

#### IconButton Component

**Purpose**: Compact button for icon-only actions (no text label)

**Variants**: Same as Button (Primary, Secondary, Destructive, Ghost)

**Sizes**
- **Small**: `w-8 h-8` (32x32px)
- **Medium**: `w-10 h-10` (40x40px) - DEFAULT
- **Large**: `w-12 h-12` (48x48px)

**Specifications**
```typescript
<IconButton
  variant="primary" | "secondary" | "destructive" | "ghost"
  size="sm" | "md" | "lg"
  icon={<LucideIcon />}        // Lucide React icon component
  ariaLabel={string}           // REQUIRED for screen readers
  onClick={Function}
/>
```

**Accessibility**
- `aria-label` REQUIRED (e.g., "Delete document", "Edit user")
- Minimum touch target: 44x44px on mobile (UXR-205) - use medium or large size
- Tooltip appears on hover with descriptive text

**Usage Examples**
- Table row actions: Edit (pencil icon), Delete (trash icon), View (eye icon)
- Navigation: Menu toggle (hamburger), Close modal (X icon)
- Media controls: Play/Pause, Mute/Unmute (future video features)

---

### 3.2 Inputs Category

#### TextField Component

**Purpose**: Single-line text input for forms

**Variants**
- **Default**: Standard input with border
- **Error**: Red border, error message below
- **Disabled**: Grayed out, non-interactive

**Sizes**
- **Small**: `h-8 px-3 text-sm` (32px height)
- **Medium**: `h-10 px-4 text-base` (40px height) - DEFAULT
- **Large**: `h-12 px-6 text-lg` (48px height)

**Specifications**
```typescript
<TextField
  label={string}                    // Form label above input
  placeholder={string}              // Placeholder text (e.g., "john.doe@example.com")
  value={string}
  onChange={Function}
  type="text" | "email" | "password" | "tel" | "number" | "date"
  error={string}                    // Error message (displays below input)
  helperText={string}               // Helper text (displays below input, gray color)
  required={boolean}
  disabled={boolean}
  icon={ReactNode}                  // Optional icon (prepended inside input)
  fullWidth={boolean}
/>
```

**Visual Tokens**
```yaml
TextField.Default:
  background: white
  border: "1px solid" border-default (neutral-200)
  borderRadius: radius-DEFAULT (8px)
  padding: "h-10 px-4"
  fontSize: text-base (16px)
  color: text-primary (neutral-700)
  placeholder:
    color: text-tertiary (neutral-400)
  focus:
    border: "2px solid" primary-500
    outline: none
  hover:
    border: border-emphasis (neutral-300)

TextField.Error:
  border: "2px solid" error-500
  errorMessage:
    color: error-500
    fontSize: text-sm (14px)
    marginTop: spacing-2 (8px)

TextField.Disabled:
  background: neutral-100
  color: text-disabled (neutral-300)
  cursor: "not-allowed"
```

**Accessibility**
- `<label>` element associated with `<input>` via `htmlFor` and `id`
- `aria-describedby` links to error message or helper text
- `aria-invalid="true"` when error present
- `aria-required="true"` when required
- Keyboard: Tab to focus, Enter submits form (if inside form)

**Validation States**
- **Default**: No validation feedback
- **Valid (optional)**: Green checkmark icon on right side (after successful validation)
- **Invalid**: Red border, error icon, error message below

**Usage Examples**
- Email input: `type="email"`, placeholder `"john.doe@example.com"`
- Password input: `type="password"`, show/hide password toggle icon
- Phone input: `type="tel"`, placeholder `"(555) 123-4567"`

---

#### TextArea Component

**Purpose**: Multi-line text input for long-form content

**Specifications**: Similar to TextField but with `rows` prop

```typescript
<TextArea
  label={string}
  placeholder={string}
  value={string}
  onChange={Function}
  rows={number}                     // Default: 4
  maxLength={number}                // Character count (e.g., 500)
  error={string}
  helperText={string}
  required={boolean}
  disabled={boolean}
/>
```

**Visual Differences from TextField**
- Height: Minimum 4 rows (80px at 20px line-height)
- Resize: `resize-y` (vertical only) or `resize-none` (fixed height)
- Character counter: Optional, displayed as `"250 / 500 characters"` below input

**Usage Examples**
- Reason for visit: 500 character limit
- Clinical notes: 2000 character limit
- Medication instructions: 250 character limit

---

#### Select Component (Dropdown)

**Purpose**: Single or multi-select dropdown for constrained choices

**Specifications**
```typescript
<Select
  label={string}
  placeholder={string}              // "Select an option"
  value={string | string[]}         // Single or multi-select
  onChange={Function}
  options={Array<{value, label}>}   // Dropdown options
  error={string}
  required={boolean}
  disabled={boolean}
  searchable={boolean}              // Enable search/filter (for long lists)
  multiple={boolean}                // Allow multi-select
/>
```

**Visual Tokens**
```yaml
Select.Trigger:
  appearance: TextField (same border, padding, radius)
  icon: "chevron-down" on right side
  placeholder:
    color: text-tertiary (neutral-400)

Select.Dropdown:
  background: white
  border: "1px solid" border-default (neutral-200)
  borderRadius: radius-md (8px)
  shadow: shadow-md
  maxHeight: "300px"                # Scroll if more options
  zIndex: 50                        # Above other content

Select.Option:
  padding: spacing-3 spacing-4 (12px 16px)
  hover:
    background: state-hover (primary-100)
  selected:
    background: primary-50
    color: primary-600
    icon: "check" on right side
```

**Accessibility**
- `role="combobox"` on trigger, `role="listbox"` on dropdown
- `aria-expanded` indicates dropdown open/closed state
- Arrow keys navigate options, Enter selects, Escape closes
- Type-ahead: Typing filters options (if searchable)

**Usage Examples**
- Provider selection: Dropdown with provider names
- Appointment type: Dropdown with "Initial Consultation", "Follow-Up", "Procedure"
- User role selection (admin): Dropdown with "Patient", "Staff", "Admin"

---

#### Checkbox Component

**Purpose**: Binary or multi-select option (checked/unchecked)

**States**
- **Unchecked**: Empty box with border
- **Checked**: Blue box with white checkmark icon
- **Indeterminate**: Blue box with white dash (for "select all" parent checkboxes)
- **Disabled**: Grayed out, non-interactive

**Specifications**
```typescript
<Checkbox
  label={string}                    // Text label to right of checkbox
  checked={boolean}
  onChange={Function}
  indeterminate={boolean}           // For parent "select all" checkboxes
  disabled={boolean}
  error={string}
/>
```

**Visual Tokens**
```yaml
Checkbox.Unchecked:
  size: "w-5 h-5" (20x20px)
  border: "2px solid" border-default (neutral-200)
  borderRadius: radius-sm (4px)
  background: white

Checkbox.Checked:
  background: primary-500
  border: primary-500
  icon: "check" (white, centered)

Checkbox.Indeterminate:
  background: primary-500
  border: primary-500
  icon: "minus" (white, centered)

Checkbox.Disabled:
  background: neutral-100
  border: border-default (neutral-200)
  opacity: 0.5
```

**Accessibility**
- Native `<input type="checkbox">` with custom styling
- `aria-checked="true" | "false" | "mixed"` (mixed for indeterminate)
- Keyboard: Space toggles checked state
- Label clickable area extends beyond just the box (easier targeting)

**Usage Examples**
- Medical history: "Do you have diabetes?" (single checkbox)
- Permissions: "Manage Users", "Review Clinical Data", "Access Audit Logs" (multi-select)
- Terms acceptance: "I agree to the Terms of Service" (required checkbox)

---

#### Radio Component

**Purpose**: Single-select from mutually exclusive options

**Specifications**
```typescript
<RadioGroup
  label={string}                    // Group label
  value={string}                    // Selected value
  onChange={Function}
  options={Array<{value, label}>}
  error={string}
  required={boolean}
>
  <Radio value="option1" label="Option 1" />
  <Radio value="option2" label="Option 2" />
  <Radio value="option3" label="Option 3" />
</RadioGroup>
```

**Visual Tokens**
```yaml
Radio.Unselected:
  size: "w-5 h-5" (20x20px)
  border: "2px solid" border-default (neutral-200)
  borderRadius: radius-full (circular)
  background: white

Radio.Selected:
  border: primary-500
  innerCircle:
    size: "w-2.5 h-2.5" (10x10px)
    background: primary-500
    borderRadius: radius-full
    centered: true

Radio.Disabled:
  opacity: 0.5
  cursor: "not-allowed"
```

**Accessibility**
- Native `<input type="radio">` with `name` attribute linking group
- `role="radiogroup"` on container, `role="radio"` on inputs
- Arrow keys navigate between options (within same group)
- Only one option selectable per group

**Usage Examples**
- Intake mode selection: "AI Intake" vs "Manual Form" (radio group)
- Gender selection: "Male", "Female", "Other", "Prefer not to say" (radio group)

---

#### Toggle Component (Switch)

**Purpose**: Binary on/off state (alternative to checkbox for settings)

**Specifications**
```typescript
<Toggle
  label={string}
  checked={boolean}
  onChange={Function}
  disabled={boolean}
/>
```

**Visual Tokens**
```yaml
Toggle.Off:
  track:
    width: "w-11" (44px)
    height: "h-6" (24px)
    background: neutral-200
    borderRadius: radius-full
  thumb:
    size: "w-5 h-5" (20px)
    background: white
    borderRadius: radius-full
    position: left (2px margin)
    transition: "transform 200ms"

Toggle.On:
  track:
    background: primary-500
  thumb:
    position: right (2px margin from right edge)
    transform: "translateX(20px)"

Toggle.Disabled:
  opacity: 0.5
  cursor: "not-allowed"
```

**Accessibility**
- `role="switch"`, `aria-checked="true|false"`
- Keyboard: Space toggles state
- Visual label clearly indicates current state ("On" / "Off" text optional but helpful)

**Usage Examples**
- Intake mode toggle: "AI Intake" (left) ↔ "Manual Form" (right) in header
- Email notifications: "Enable email reminders" (toggle on/off)
- System settings: "Enable audit logging" (admin panel)

---

#### FileUpload Component

**Purpose**: Drag-and-drop file upload zone

**States**
- **Default**: Idle, shows upload icon and instructions
- **Hover** (drag-over): Highlighted border, "Drop files here" text
- **Uploading**: Progress bar with percentage
- **Complete**: Green checkmark, file name displayed
- **Error**: Red border, error message

**Specifications**
```typescript
<FileUpload
  accept={string}                   // "application/pdf" for PDFs only
  maxSize={number}                  // Bytes (e.g., 10MB = 10485760)
  multiple={boolean}                // Allow multiple files
  onUpload={Function}               // Callback with File objects
  error={string}
/>
```

**Visual Tokens**
```yaml
FileUpload.Default:
  border: "2px dashed" border-default (neutral-200)
  borderRadius: radius-lg (12px)
  padding: spacing-12 (48px vertical)
  background: neutral-50
  textAlign: center
  icon: "upload-cloud" (32x32px, neutral-400)
  text: "Drag PDF files here or click to browse"

FileUpload.Hover:
  border: "2px dashed" primary-500
  background: primary-50

FileUpload.Uploading:
  progressBar:
    height: "4px"
    background: neutral-200
    fill: primary-500
    borderRadius: radius-full
  text: "Uploading [Filename]... 42%"

FileUpload.Complete:
  icon: "check-circle" (success-500)
  text: "[Filename] uploaded successfully"

FileUpload.Error:
  border: "2px dashed" error-500
  background: error-50
  text: "Upload failed: [Error message]"
```

**Accessibility**
- Native `<input type="file">` with custom styling (visually hidden)
- Keyboard: Tab to focus, Enter/Space opens file picker
- Screen reader: Announces "Upload files button" and file selection results

**Usage Examples**
- Clinical document upload (SCR-003): Accept PDFs only, max 10MB
- Profile photo upload (future): Accept images (JPG, PNG), max 2MB

---

### 3.3 Navigation Category

#### Header Component

**Purpose**: Top navigation bar with logo, user menu, session timer

**Variants**
- **Desktop**: Full logo, horizontal nav links, user avatar on right
- **Mobile**: Compact logo, hamburger menu, user avatar

**Specifications**
```typescript
<Header
  logo={ReactNode}
  navItems={Array<{label, href, icon?}>}
  userMenu={ReactNode}              // Dropdown with profile, settings, logout
  sessionTimer={boolean}            // Show countdown timer (15 min logout)
/>
```

**Visual Tokens**
```yaml
Header:
  height: "h-16" (64px)
  background: white
  borderBottom: "1px solid" border-default (neutral-200)
  padding: "px-6" (24px horizontal)
  display: flex
  justifyContent: space-between
  alignItems: center
  position: sticky (top: 0)
  zIndex: 40

Header.Logo:
  height: "h-10" (40px)
  fontWeight: font-bold (700)
  fontSize: text-xl (20px)
  color: primary-600

Header.NavLinks: (Desktop only)
  gap: spacing-6 (24px)
  fontSize: text-base (16px)
  color: text-secondary (neutral-500)
  hover:
    color: primary-600
  active:
    color: primary-600
    borderBottom: "2px solid" primary-600

Header.UserMenu:
  avatar:
    size: "w-10 h-10" (40x40px)
    borderRadius: radius-full
    background: primary-100
    color: primary-600
    fontSize: text-sm (14px)
  dropdown:
    shadow: shadow-md
    borderRadius: radius-md (8px)
    minWidth: "200px"
```

**Accessibility**
- `<nav>` element with `aria-label="Main navigation"`
- Skip to main content link (visually hidden, keyboard-accessible)
- User menu: `aria-haspopup="true"`, `aria-expanded` state

**Usage Examples**
- Patient portal header: Logo, "Dashboard", "Book", "Documents", User Menu
- Staff portal header: Logo, "Arrivals", "Walk-Ins", "Clinical Review", User Menu
- Admin panel header: Logo, "Users", "Audit Logs", "Config", User Menu

---

#### Sidebar Component

**Purpose**: Vertical navigation for desktop layouts

**Variants**
- **Expanded**: Full labels visible (240px width)
- **Collapsed**: Icons only (64px width)

**Specifications**
```typescript
<Sidebar
  items={Array<{label, href, icon, badge?}>}
  collapsed={boolean}
  onToggle={Function}
  userName={string}
  userRole={string}
/>
```

**Visual Tokens**
```yaml
Sidebar.Expanded:
  width: "w-60" (240px)
  background: neutral-50
  borderRight: "1px solid" border-default (neutral-200)
  padding: spacing-4 (16px)
  display: flex
  flexDirection: column

Sidebar.Collapsed:
  width: "w-16" (64px)
  [labels hidden, only icons visible]

Sidebar.Item:
  padding: spacing-3 spacing-4 (12px 16px)
  borderRadius: radius-DEFAULT (8px)
  fontSize: text-base (16px)
  color: text-secondary (neutral-500)
  icon:
    size: "w-5 h-5" (20x20px)
    marginRight: spacing-3 (12px)
  hover:
    background: primary-50
    color: primary-600
  active:
    background: primary-100
    color: primary-600
    fontWeight: font-semibold (600)

Sidebar.Badge:
  background: error-500
  color: white
  fontSize: text-xs (12px)
  borderRadius: radius-full
  padding: "2px 6px"
  position: absolute (top-right of icon)
```

**Accessibility**
- `<nav>` with `aria-label="Sidebar navigation"`
- `aria-current="page"` on active nav item
- Keyboard: Tab through items, Enter activates

**Usage Examples**
- Staff portal sidebar: "Arrivals" (2 badge count), "Walk-Ins", "Clinical Review", "Insurance"
- Admin panel sidebar: "Users", "Audit Logs", "Configuration"

---

#### Tabs Component

**Purpose**: Horizontal tab navigation within a screen

**Specifications**
```typescript
<Tabs
  value={string}                    // Active tab ID
  onChange={Function}
  tabs={Array<{id, label, badge?}>}
>
  <TabPanel value="tab1">Content 1</TabPanel>
  <TabPanel value="tab2">Content 2</TabPanel>
  <TabPanel value="tab3">Content 3</TabPanel>
</Tabs>
```

**Visual Tokens**
```yaml
Tabs.Container:
  borderBottom: "1px solid" border-default (neutral-200)

Tabs.Tab:
  padding: spacing-4 spacing-6 (16px 24px)
  fontSize: text-base (16px)
  color: text-secondary (neutral-500)
  borderBottom: "2px solid transparent"
  hover:
    color: primary-600
  active:
    color: primary-600
    borderBottom: "2px solid" primary-600
    fontWeight: font-semibold (600)

Tabs.Badge:
  background: error-500
  color: white
  fontSize: text-xs (12px)
  borderRadius: radius-full
  padding: "2px 6px"
  marginLeft: spacing-2 (8px)
```

**Accessibility**
- `role="tablist"` on container, `role="tab"` on each tab, `role="tabpanel"` on content
- `aria-selected="true"` on active tab
- Arrow keys navigate tabs, Enter/Space activates
- Tab content receives focus when activated

**Usage Examples**
- 360° View (SCR-009): "Timeline", "Documents", "Medications", "Conflicts (2)"
- System Config (SCR-018): "General", "Notifications", "Security"

---

#### BottomNav Component (Mobile)

**Purpose**: Fixed bottom navigation bar for mobile devices

**Specifications**
```typescript
<BottomNav
  items={Array<{label, href, icon, badge?}>}
  activeIndex={number}
/>
```

**Visual Tokens**
```yaml
BottomNav:
  height: "h-16" (64px)
  background: white
  borderTop: "1px solid" border-default (neutral-200)
  position: fixed (bottom: 0)
  display: flex
  justifyContent: space-around
  zIndex: 40

BottomNav.Item:
  display: flex
  flexDirection: column
  alignItems: center
  fontSize: text-xs (12px)
  color: text-tertiary (neutral-400)
  icon:
    size: "w-6 h-6" (24x24px)
    marginBottom: spacing-1 (4px)
  active:
    color: primary-600
```

**Accessibility**
- `<nav>` with `aria-label="Mobile navigation"`
- `aria-current="page"` on active item
- Minimum 44x44px tap targets (UXR-205)

**Usage Examples**
- Patient portal mobile nav: "Home" (house icon), "Book" (calendar icon), "Documents" (file icon), "Profile" (user icon)

---

### 3.4 Content Category

#### Card Component

**Purpose**: Container for grouped content with optional header, body, footer

**Variants**
- **Simple**: Plain card with padding, no header/footer
- **WithHeader**: Title at top, body below
- **WithActions**: Footer with buttons (e.g., "Edit", "Delete")
- **Elevated**: Raised shadow for emphasis

**Specifications**
```typescript
<Card
  variant="simple" | "elevated"
  header={ReactNode}                // Optional header content
  footer={ReactNode}                // Optional footer content
  onClick={Function}                // Optional (makes card clickable)
>
  Card body content
</Card>
```

**Visual Tokens**
```yaml
Card.Simple:
  background: white
  border: "1px solid" border-default (neutral-200)
  borderRadius: radius-DEFAULT (8px)
  padding: spacing-6 (24px)
  shadow: shadow-sm

Card.Elevated:
  border: none
  shadow: shadow-DEFAULT

Card.Header:
  fontSize: text-xl (20px)
  fontWeight: font-semibold (600)
  marginBottom: spacing-4 (16px)

Card.Footer:
  borderTop: "1px solid" border-subtle (neutral-100)
  padding-top: spacing-4 (16px)
  display: flex
  gap: spacing-3 (12px)
  justifyContent: flex-end

Card.Clickable:
  cursor: pointer
  hover:
    shadow: shadow-md
    borderColor: primary-300
```

**Accessibility**
- If clickable, `role="button"` or wrap in `<a>` tag
- Header uses semantic heading (`<h3>` or `<h4>`)

**Usage Examples**
- Dashboard cards (SCR-001): "Upcoming Appointments", "Intake Status", "Your Documents"
- Clinical data entries (SCR-009): Individual lab results, medications, allergies

---

#### Table Component

**Purpose**: Data table with sortable columns, row actions

**Specifications**
```typescript
<Table
  columns={Array<{id, label, sortable?}>}
  data={Array<Object>}
  onSort={Function}
  onRowClick={Function}
  actions={Array<{label, icon, onClick}>}
  emptyState={ReactNode}
/>
```

**Visual Tokens**
```yaml
Table.Container:
  width: full
  border: "1px solid" border-default (neutral-200)
  borderRadius: radius-DEFAULT (8px)
  overflow: hidden

Table.Header:
  background: neutral-50
  borderBottom: "1px solid" border-default (neutral-200)
  fontWeight: font-semibold (600)
  fontSize: text-sm (14px)
  color: text-primary (neutral-700)
  padding: spacing-3 spacing-4 (12px 16px)

Table.Row:
  borderBottom: "1px solid" border-subtle (neutral-100)
  hover:
    background: neutral-50
  padding: spacing-3 spacing-4 (12px 16px)

Table.Cell:
  fontSize: text-base (16px)
  color: text-secondary (neutral-500)
  verticalAlign: middle

Table.EmptyState:
  padding: spacing-12 (48px)
  textAlign: center
  color: text-tertiary (neutral-400)
```

**Accessibility**
- Semantic `<table>`, `<thead>`, `<tbody>`, `<tr>`, `<th>`, `<td>` elements
- `scope="col"` on header cells
- Sortable columns: `aria-sort="ascending|descending|none"`
- Row actions: Accessible via Tab key, Enter/Space activates

**Usage Examples**
- User management (SCR-010): Columns: Name, Email, Role, Status, Actions (Edit, Deactivate)
- Appointment history (SCR-016): Columns: Date, Provider, Status, Actions (Reschedule, Cancel)
- Audit logs (SCR-012): Columns: Timestamp, User, Action, Entity, IP Address

---

#### Badge Component

**Purpose**: Small status indicator or label

**Variants**
- **Neutral**: Gray background (default status)
- **Success**: Green background (completed, verified)
- **Warning**: Yellow background (review required, low confidence)
- **Error**: Red background (failed, conflict)
- **Info**: Blue background (scheduled, in progress)

**Specifications**
```typescript
<Badge
  variant="neutral" | "success" | "warning" | "error" | "info"
  size="sm" | "md"
>
  Badge Text
</Badge>
```

**Visual Tokens**
```yaml
Badge.Success:
  background: success-100
  color: success-700
  fontSize: text-xs (12px)
  fontWeight: font-medium (500)
  padding: "2px 8px"
  borderRadius: radius-full
  display: inline-flex
  alignItems: center

Badge.Warning:
  background: warning-100
  color: warning-700

Badge.Error:
  background: error-100
  color: error-700

Badge.Info:
  background: primary-100
  color: primary-700

Badge.Neutral:
  background: neutral-100
  color: neutral-700
```

**Accessibility**
- `role="status"` for dynamic badges (e.g., "Uploading...")
- Screen reader: Announce badge text with context (e.g., "Status: Verified")

**Usage Examples**
- Appointment status: "Scheduled" (info), "Arrived" (success), "Late" (warning), "Cancelled" (error)
- AI extraction confidence: "High Confidence 95%" (success), "Review Required 68%" (warning)
- User status: "Active" (success), "Pending Activation" (warning), "Inactive" (error)

---

### 3.5 Feedback Category

#### Modal Component

**Purpose**: Dialog overlay for focused tasks, confirmations, forms

**Sizes**
- **Small**: `max-w-sm` (384px) - Simple confirmations
- **Medium**: `max-w-lg` (512px) - DEFAULT, forms
- **Large**: `max-w-2xl` (672px) - Complex content, multi-step forms

**Specifications**
```typescript
<Modal
  isOpen={boolean}
  onClose={Function}
  title={string}
  size="sm" | "md" | "lg"
  footer={ReactNode}                // Optional footer with action buttons
>
  Modal body content
</Modal>
```

**Visual Tokens**
```yaml
Modal.Backdrop:
  background: "rgba(0,0,0,0.5)"     # 50% black overlay
  position: fixed
  inset: 0
  zIndex: 50                        # Above all content
  display: flex
  alignItems: center
  justifyContent: center

Modal.Container:
  background: white
  borderRadius: radius-lg (12px)
  shadow: shadow-2xl
  padding: spacing-6 (24px)
  maxWidth: [size-dependent]
  maxHeight: "90vh"                 # Avoid full-screen modals
  overflow: auto

Modal.Header:
  fontSize: text-2xl (24px)
  fontWeight: font-bold (700)
  marginBottom: spacing-4 (16px)
  display: flex
  justifyContent: space-between
  CloseButton:
    icon: "x" (close icon)
    size: "w-6 h-6" (24x24px)

Modal.Footer:
  borderTop: "1px solid" border-subtle (neutral-100)
  padding-top: spacing-4 (16px)
  display: flex
  gap: spacing-3 (12px)
  justifyContent: flex-end
```

**Accessibility**
- `role="dialog"`, `aria-modal="true"`, `aria-labelledby` points to title
- Focus trap: Tab cycles within modal, Shift+Tab reverses
- Escape key closes modal (unless critical action in progress)
- Close button has `aria-label="Close dialog"`
- Initial focus on first interactive element or close button

**Usage Examples**
- Booking confirmation (SCR-002): "Confirm your appointment on [Date] at [Time]?" with "Confirm" and "Cancel" buttons
- Session timeout warning (UXR-603): "You'll be logged out in 2 minutes. Continue session?" with "Stay Logged In" button
- Delete user confirmation (SCR-010): "Deactivate Jane Smith? User will be logged out immediately." with reason field

---

#### Toast Component

**Purpose**: Non-blocking notification, auto-dismisses after timeout

**Variants**
- **Info**: Blue icon, neutral message
- **Success**: Green checkmark icon, success message
- **Warning**: Yellow alert icon, caution message
- **Error**: Red X icon, error message

**Specifications**
```typescript
<Toast
  variant="info" | "success" | "warning" | "error"
  message={string}
  duration={number}                 // Milliseconds (default: 5000ms)
  onClose={Function}
  action={ReactNode}                // Optional action button (e.g., "Retry", "Undo")
/>
```

**Visual Tokens**
```yaml
Toast.Container:
  position: fixed
  bottom: spacing-8 (32px)
  right: spacing-8 (32px)
  zIndex: 60                        # Above modals
  maxWidth: "400px"

Toast.Success:
  background: white
  border: "1px solid" success-200
  borderRadius: radius-DEFAULT (8px)
  shadow: shadow-lg
  padding: spacing-4 (16px)
  display: flex
  gap: spacing-3 (12px)
  icon:
    color: success-500
    size: "w-5 h-5" (20x20px)
  message:
    fontSize: text-base (16px)
    color: text-primary (neutral-700)

Toast.Error:
  border: "1px solid" error-200
  icon:
    color: error-500

Toast.Warning:
  border: "1px solid" warning-200
  icon:
    color: warning-500

Toast.Info:
  border: "1px solid" primary-200
  icon:
    color: primary-500
```

**Accessibility**
- `role="status"` for non-critical toasts, `role="alert"` for errors
- Screen reader announces message immediately (live region)
- Dismissible via close button (X icon) or click outside
- Auto-dismiss respects `prefers-reduced-motion` (longer duration)

**Usage Examples**
- Booking confirmed (FL-002 step 6): Success toast "Appointment booked! Confirmation email sent."
- Slot conflict (UXR-605): Error toast "This slot was just booked. Please select another."
- PDF generation (NFR-004): Info toast "Generating PDF confirmation..."

---

#### Alert Component

**Purpose**: Inline, persistent message (requires explicit dismissal)

**Variants**: Same as Toast (Info, Success, Warning, Error)

**Specifications**
```typescript
<Alert
  variant="info" | "success" | "warning" | "error"
  title={string}                    // Optional bold title
  message={string | ReactNode}
  dismissible={boolean}
  onDismiss={Function}
>
  Alert content (can include buttons, links)
</Alert>
```

**Visual Tokens**
```yaml
Alert.Warning:
  background: warning-50
  border: "1px solid" warning-500
  borderRadius: radius-DEFAULT (8px)
  padding: spacing-4 (16px)
  display: flex
  gap: spacing-3 (12px)
  icon:
    color: warning-500
    size: "w-5 h-5" (20x20px)
  title:
    fontSize: text-base (16px)
    fontWeight: font-semibold (600)
    color: warning-700
  message:
    fontSize: text-sm (14px)
    color: warning-700

Alert.Error:
  background: error-50
  border: "1px solid" error-500
  icon/title/message: error color variants

Alert.Success:
  background: success-50
  border: "1px solid" success-500

Alert.Info:
  background: primary-50
  border: "1px solid" primary-500
```

**Accessibility**
- `role="alert"` for critical alerts requiring immediate attention
- `role="status"` for informational, non-urgent alerts
- Screen reader announces alert content

**Usage Examples**
- Conflict resolution banner (UXR-604, SCR-009): Red error alert "2 critical conflicts detected. Resolve before proceeding."
- Insurance validation result (SCR-008): Green success alert "Validation successful. Insurance verified."
- Session timeout countdown: Warning alert "You'll be logged out in 2 minutes."

---

#### Skeleton Component

**Purpose**: Loading placeholder mimicking content shape

**Variants**
- **Text**: Single line or paragraph (multiple lines)
- **Card**: Rectangle with rounded corners
- **Table Row**: Multiple cells with spacing

**Specifications**
```typescript
<Skeleton
  variant="text" | "card" | "circle" | "rect"
  width={string | number}
  height={string | number}
  lines={number}                    // For text variant
  animation="pulse" | "wave"
/>
```

**Visual Tokens**
```yaml
Skeleton:
  background: neutral-200
  borderRadius: radius-DEFAULT (8px for card, 4px for text)
  animation: "pulse" (opacity oscillates 50%-100%)
  duration: "1.5s"

Skeleton.Text:
  height: "16px"
  width: "100%" (or custom)
  marginBottom: spacing-2 (8px between lines)

Skeleton.Card:
  height: "200px"
  width: "full"

Skeleton.Circle: (Avatar placeholder)
  borderRadius: radius-full
  size: "w-10 h-10" (or custom)
```

**Accessibility**
- `aria-busy="true"` on container while loading
- `aria-label="Loading content"` on skeleton
- Announce completion when real content loads ("Content loaded")

**Usage Examples**
- Dashboard loading (SCR-001): Three skeleton cards while appointments load
- 360° View loading (SCR-009): Skeleton timeline entries, skeleton table rows
- Slot selection loading (SCR-002): Skeleton calendar grid

---

## 4. Layout Patterns

### 4.1 Page Layout

**Structure**: Header → Main Content → Footer (optional)

```yaml
PageLayout:
  minHeight: "100vh"
  display: flex
  flexDirection: column

  Header:
    position: sticky (top: 0)
    zIndex: 40

  MainContent:
    flex: 1                         # Fills remaining space
    padding: spacing-8 (32px desktop), spacing-4 (16px mobile)
    maxWidth: "1280px" (desktop), full (mobile)
    margin: "0 auto"

  Footer: (optional, future)
    background: neutral-50
    padding: spacing-6 (24px)
    borderTop: "1px solid" border-default (neutral-200)
```

### 4.2 Grid System

**Desktop (1024px+)**
```yaml
Grid:
  columns: 12
  gap: spacing-6 (24px)
  margin: spacing-10 (40px)

Common Layouts:
  - Two-column: 8/4 split (main content / sidebar)
  - Three-column: 4/4/4 split (dashboard cards)
  - Four-column: 3/3/3/3 split (thumbnails, icons)
```

**Tablet (768-1023px)**
```yaml
Grid:
  columns: 8
  gap: spacing-4 (16px)
  margin: spacing-6 (24px)
```

**Mobile (320-767px)**
```yaml
Grid:
  columns: 1 (single-column stack)
  gap: spacing-4 (16px)
  margin: spacing-4 (16px)
```

### 4.3 Responsive Breakpoints

```yaml
breakpoints:
  sm: "640px"                       # Large phones (landscape)
  md: "768px"                       # Tablets
  lg: "1024px"                      # Desktop
  xl: "1280px"                      # Large desktop
  2xl: "1536px"                     # Extra large desktop (future)
```

**Usage**: Tailwind CSS responsive modifiers (`sm:`, `md:`, `lg:`)

Example:
```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  <!-- Mobile: 1 column, Tablet: 2 columns, Desktop: 3 columns -->
</div>
```

---

## 5. Brand Guidelines

### 5.1 Logo & Identity

**Logo**: "PulseCare" wordmark with medical cross icon (to be designed)

**Logo Usage**
- **Minimum Size**: 120px width (ensures readability)
- **Clear Space**: Minimum spacing equal to height of "P" character on all sides
- **Color Variations**: 
  - Primary: primary-600 (#2563eb) on white backgrounds
  - White: #FFFFFF on dark backgrounds (future dark mode)
  - Black: neutral-900 (#111827) for print/grayscale

**Logo Placement**
- Header: Top-left corner, 40px height
- Footer: Centered, 32px height (future)
- Login screen: Centered, 64px height

### 5.2 Iconography

**Icon Library**: Lucide React (https://lucide.dev/)

**Icon Style**: Outlined (stroked, not filled)

**Icon Sizes**
```yaml
icon-sm: "w-4 h-4" (16x16px)        # Inline with text, badges
icon-md: "w-5 h-5" (20x20px)        # Buttons, nav items (DEFAULT)
icon-lg: "w-6 h-6" (24x24px)        # Page headings, prominent actions
icon-xl: "w-8 h-8" (32x32px)        # Empty states, hero sections
```

**Icon Color**: Inherits text color or explicit color class (e.g., `text-primary-600`)

**Common Icons**
- Navigation: `Home`, `Calendar`, `FileText`, `User`, `Settings`, `LogOut`
- Actions: `Plus`, `Edit3`, `Trash2`, `Check`, `X`, `ChevronDown`, `ChevronRight`
- Status: `CheckCircle` (success), `AlertCircle` (warning), `XCircle` (error), `Info` (info)
- Content: `File`, `Upload`, `Download`, `Search`, `Filter`, `Clock`, `Users`

### 5.3 Illustration Style

**Approach**: Minimal spot illustrations for empty states (healthcare-themed)

**Style Guidelines**
- **Flat Design**: No gradients or 3D effects (aligns with Material Design principles)
- **Color Palette**: Limited to brand colors (primary, success, neutral)
- **Line Weight**: 2-3px strokes, consistent with Lucide icons
- **Complexity**: Simple, recognizable shapes (avoid intricate details)

**Usage**
- Empty states: "No appointments yet" illustration (calendar with checkmark)
- Error states: "Connection lost" illustration (broken wifi icon)
- Success states: "Booking confirmed" illustration (checkmark with confetti)

**Avoid**: Stock photos, realistic medical imagery (HIPAA privacy concerns)

### 5.4 Voice & Tone

**Brand Personality**: Professional yet approachable, empathetic, trustworthy

**Patient-Facing Content**
- **Tone**: Warm, conversational, reassuring
- **Language**: Simple, jargon-free ("reason for visit" vs "chief complaint")
- **Examples**: 
  - "Welcome back! You have 1 upcoming appointment."
  - "We couldn't find that email address. Double-check and try again?"

**Staff-Facing Content**
- **Tone**: Efficient, direct, respectful
- **Language**: Clinical terminology acceptable, concise instructions
- **Examples**:
  - "Patient checked in at 2:15 PM. Expected wait time: 10 minutes."
  - "Low-confidence extraction detected. Review required before approval."

**Error Messages**
- **Tone**: Helpful, non-blaming, actionable
- **Language**: Explain what went wrong + what to do next
- **Examples**:
  - "This slot was just booked by another patient. Please select another time."
  - "We couldn't save your intake form. Check your connection and try again."

---

## 6. Implementation Guidelines

### 6.1 Technology Integration

**Tailwind CSS Configuration** (`tailwind.config.js`)
```javascript
module.exports = {
  theme: {
    extend: {
      colors: {
        primary: { /* Blue palette from 2.1 */ },
        success: { /* Green palette */ },
        warning: { /* Yellow palette */ },
        error: { /* Red palette */ },
        neutral: { /* Gray palette */ },
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui'],
        mono: ['JetBrains Mono', 'ui-monospace'],
      },
      spacing: { /* 4px base grid */ },
      borderRadius: { /* sm, DEFAULT, md, lg, xl, full */ },
      boxShadow: { /* sm, DEFAULT, md, lg, xl, 2xl */ },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),      // Form reset styles
    require('@tailwindcss/typography'), // Prose styles (future blog/help docs)
  ],
}
```

**CSS Custom Properties** (`globals.css`)
```css
:root {
  --color-primary-500: #3b82f6;
  --color-success-500: #22c55e;
  --color-error-500: #ef4444;
  --color-warning-500: #eab308;
  
  --radius-default: 0.5rem;
  --shadow-default: 0 1px 3px 0 rgb(0 0 0 / 0.1);
  
  --transition-fast: 150ms cubic-bezier(0, 0, 0.2, 1);
}

@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

**shadcn/ui Component Installation**
```bash
# Install with Next.js 14+ (automatic setup)
npx shadcn-ui@latest init

# Add specific components as needed
npx shadcn-ui@latest add button
npx shadcn-ui@latest add card
npx shadcn-ui@latest add input
npx shadcn-ui@latest add select
npx shadcn-ui@latest add dialog
npx shadcn-ui@latest add toast
```

### 6.2 Component Organization

**Folder Structure**
```
src/
├── components/
│   ├── ui/                         # shadcn/ui base components
│   │   ├── Button.tsx
│   │   ├── Card.tsx
│   │   ├── Input.tsx
│   │   ├── Select.tsx
│   │   ├── Dialog.tsx
│   │   ├── Toast.tsx
│   │   └── ...
│   ├── layout/                     # Layout components
│   │   ├── Header.tsx
│   │   ├── Sidebar.tsx
│   │   ├── BottomNav.tsx
│   │   └── PageLayout.tsx
│   ├── features/                   # Feature-specific components
│   │   ├── booking/
│   │   │   ├── SlotCalendar.tsx
│   │   │   ├── BookingModal.tsx
│   │   ├── intake/
│   │   │   ├── AIIntakeChat.tsx
│   │   │   ├── ManualIntakeForm.tsx
│   │   │   └── IntakeToggle.tsx
│   │   ├── clinical/
│   │   │   ├── PatientTimeline.tsx
│   │   │   ├── ConflictAlert.tsx
│   │   │   └── DocumentViewer.tsx
│   │   └── admin/
│   │       ├── UserTable.tsx
│   │       ├── AuditLogTable.tsx
│   │       └── UserForm.tsx
│   └── shared/                     # Shared utilities
│       ├── EmptyState.tsx
│       ├── ErrorBoundary.tsx
│       └── SessionTimeoutModal.tsx
├── styles/
│   ├── globals.css                 # Tailwind imports, CSS resets
│   └── design-tokens.css           # CSS custom properties
└── config/
    └── tailwind.config.js          # Tailwind configuration
```

### 6.3 Accessibility Implementation

**Focus Management**
```typescript
// Example: Trap focus within modal
import { useEffect, useRef } from 'react';

const Modal = ({ isOpen, onClose, children }) => {
  const modalRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'Tab') {
        // Trap focus within modal (cycle through focusable elements)
        const focusableElements = modalRef.current?.querySelectorAll(
          'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        // ... focus trap logic
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  return isOpen ? (
    <div ref={modalRef} role="dialog" aria-modal="true">
      {children}
    </div>
  ) : null;
};
```

**ARIA Live Regions**
```typescript
// Example: Announce toast messages to screen readers
const Toast = ({ message, variant }) => {
  return (
    <div
      role={variant === 'error' ? 'alert' : 'status'}
      aria-live={variant === 'error' ? 'assertive' : 'polite'}
      aria-atomic="true"
    >
      {message}
    </div>
  );
};
```

**Color Contrast Validation** (CI/CD integration)
```bash
# Install axe-core for automated testing
npm install --save-dev @axe-core/react jest-axe

# Run accessibility tests in CI pipeline
npm run test:a11y
```

### 6.4 Performance Optimization

**Image Optimization** (Next.js Image component)
```typescript
import Image from 'next/image';

<Image
  src="/logo.svg"
  alt="PulseCare logo"
  width={120}
  height={40}
  priority={true}                   // Load above the fold
/>
```

**Code Splitting** (lazy load components)
```typescript
import { lazy, Suspense } from 'react';

const DocumentViewer = lazy(() => import('@/components/features/clinical/DocumentViewer'));

<Suspense fallback={<Skeleton variant="card" />}>
  <DocumentViewer />
</Suspense>
```

**Tailwind CSS JIT Mode** (production optimization)
```javascript
// tailwind.config.js
module.exports = {
  mode: 'jit',                      // Just-In-Time compilation
  purge: ['./src/**/*.{js,ts,jsx,tsx}'], // Remove unused styles
}
```

---

## 7. Version History

### Version 1.0.0 (Phase 1 MVP)
- Initial design system creation
- 25 UXR requirements mapped to system
- Component library based on shadcn/ui + Tailwind CSS
- Light mode only (dark mode deferred to Phase 2)
- Responsive breakpoints: Mobile (320px+), Tablet (768px+), Desktop (1024px+)
- WCAG 2.2 AA compliance enforced
- Free-tier infrastructure constraints applied

### Planned Updates (Phase 2)
- Dark mode color tokens and component variants
- Advanced animations (page transitions, micro-interactions)
- Extended component library (Charts, DataGrid, RichTextEditor)
- Provider-facing design patterns
- Multi-language support (i18n tokens)

---

## 8. References & Resources

**External Documentation**
- Tailwind CSS: https://tailwindcss.com/docs
- shadcn/ui: https://ui.shadcn.com/
- Lucide Icons: https://lucide.dev/
- WCAG 2.2 Guidelines: https://www.w3.org/WAI/WCAG22/quickref/
- Material Design 3: https://m3.material.io/ (reference for interaction patterns)

**Internal Documents**
- [Figma Specification](./figma_spec.md) - Screen designs, UX requirements, flows
- [Requirements Specification](./spec.md) - Functional requirements, use cases
- [Architecture Design](./design.md) - Non-functional requirements, tech stack

**Design Tools**
- Figma: https://figma.com/ (design prototyping)
- Stark Plugin: https://www.getstark.co/ (accessibility testing)
- axe DevTools: https://www.deque.com/axe/devtools/ (browser accessibility testing)

---

*Document End*
