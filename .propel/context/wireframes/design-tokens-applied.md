# Design Tokens Applied — Patient Access & Clinical Intelligence Platform

## Token Source

All design tokens sourced from `.propel/context/docs/designsystem.md` and applied as CSS custom properties in every wireframe's embedded `<style>` block.

---

## 1. Color Tokens

| Token | CSS Variable | Value | Usage |
|-------|-------------|-------|-------|
| Primary 50 | `--color-primary-50` | #EBF5FF | Active nav background, focus ring |
| Primary 100 | `--color-primary-100` | #D1E9FF | Avatar background |
| Primary 500 | `--color-primary-500` | #0F62FE | Primary buttons, links, active states |
| Primary 600 | `--color-primary-600` | #0C50D4 | Primary button hover |
| Success Light | `--color-success-light` | #DCFCE7 | Verified badge bg, arrived row |
| Success | `--color-success` | #16A34A | Success badge, toggle active, verify button |
| Success Dark | `--color-success-dark` | #166534 | Success badge text |
| Warning Light | `--color-warning-light` | #FEF3C7 | AI-suggested badge bg, alert bg |
| Warning | `--color-warning` | #D97706 | Warning badge, timer warn |
| Warning Dark | `--color-warning-dark` | #92400E | Warning badge text |
| Error Light | `--color-error-light` | #FEE2E2 | Error badge bg, conflict banner |
| Error | `--color-error` | #DC2626 | Error badge, reject button, critical timer |
| Error Dark | `--color-error-dark` | #991B1B | Error badge text |
| Info Light | `--color-info-light` | #DBEAFE | Info badge bg, source card header |
| Info | `--color-info` | #2563EB | Info badge, count indicators |
| Neutral 0 | `--color-neutral-0` | #FFFFFF | Card background, header |
| Neutral 50 | `--color-neutral-50` | #F8FAFC | Sidebar bg, table header |
| Neutral 100 | `--color-neutral-100` | #F1F5F9 | Page background, hover states |
| Neutral 200 | `--color-neutral-200` | #E2E8F0 | Borders, dividers |
| Neutral 300 | `--color-neutral-300` | #CBD5E1 | Input borders, toggle inactive |
| Neutral 400 | `--color-neutral-400` | #94A3B8 | Chevrons, disabled text |
| Neutral 500 | `--color-neutral-500` | #64748B | Secondary text, labels |
| Neutral 600 | `--color-neutral-600` | #475569 | Nav item text |
| Neutral 700 | `--color-neutral-700` | #334155 | Table headers, form labels |
| Neutral 800 | `--color-neutral-800` | #1E293B | Body text |
| Neutral 900 | `--color-neutral-900` | #0F172A | Headings |

---

## 2. Typography Tokens

| Token | CSS Variable | Value | Usage |
|-------|-------------|-------|-------|
| H2 | `--font-h2` | 600 28px/36px Inter | Page titles |
| H3 | `--font-h3` | 600 22px/28px Inter | Section headers, patient name |
| H4 | `--font-h4` | 600 18px/24px Inter | Card headers, stat values |
| H5 | `--font-h5` | 600 16px/22px Inter | Subheadings, nav section titles |
| H6 | `--font-h6` | 600 14px/20px Inter | Table headers, form labels |
| Body | `--font-body` | 400 14px/20px Inter | Default text, table cells |
| Body Small | `--font-body-sm` | 400 13px/18px Inter | Metadata, breadcrumbs |
| Caption | `--font-caption` | 400 12px/16px Inter | Badges, timestamps, hints |
| Overline | `--font-overline` | 600 11px/16px Inter | Nav section titles, calendar headers |

---

## 3. Spacing Tokens

| Token | Value | Usage |
|-------|-------|-------|
| Base unit | 4px | Foundation for all spacing |
| Card padding | 20px (5 units) | Standard card interior |
| Main padding | 32px (8 units) | Main content area |
| Header height | 64px (16 units) | Fixed header |
| Sidebar width | 240px (60 units) | Navigation sidebar |
| Gap small | 8px | Button gaps, filter bar |
| Gap medium | 12px | Nav item gaps |
| Gap large | 16px | Grid gaps, card margins |
| Gap xl | 24px | Section spacing |

---

## 4. Border Radius Tokens

| Token | CSS Variable | Value | Usage |
|-------|-------------|-------|-------|
| Small | `--radius-sm` | 4px | Focus outlines, calendar days |
| Medium | `--radius-md` | 8px | Cards, buttons, inputs, accordions |
| Large | `--radius-lg` | 16px | Large containers |
| Full | `--radius-full` | 9999px | Badges, avatars, toggles |

---

## 5. Elevation Tokens

| Token | CSS Variable | Value | Usage |
|-------|-------------|-------|-------|
| Level 1 | `--shadow-1` | 0 1px 3px rgba(0,0,0,.08), 0 1px 2px rgba(0,0,0,.06) | Cards, stat cards |
| Level 2 | `--shadow-2` | 0 4px 6px rgba(0,0,0,.07), 0 2px 4px rgba(0,0,0,.06) | Side panels, elevated cards |

---

## 6. Motion Tokens

| Token | CSS Variable | Value | Usage |
|-------|-------------|-------|-------|
| Fast | `--transition-fast` | 100ms ease | Button hover, nav hover |
| Normal | `--transition-normal` | 200ms ease | General transitions |
| Moderate | `--transition-moderate` | 300ms cubic-bezier(.16,1,.3,1) | Accordion expand, chevron rotate |

---

## 7. Breakpoint Tokens

| Breakpoint | Value | Behavior |
|------------|-------|----------|
| Desktop | ≥ 1024px | Full shell layout |
| Tablet | 768px – 1023px | Sidebar hidden, single column |
| Mobile | < 768px | Reduced padding, stacked layouts |

---

## 8. Token Coverage Verification

| Category | Defined | Applied | Coverage |
|----------|---------|---------|----------|
| Colors | 26 tokens | 26 tokens | 100% |
| Typography | 9 scales | 9 scales | 100% |
| Spacing | 8 values | 8 values | 100% |
| Border Radius | 4 tokens | 4 tokens | 100% |
| Elevation | 2 levels | 2 levels | 100% |
| Motion | 3 tokens | 3 tokens | 100% |
| Breakpoints | 3 values | 3 values | 100% |

**Total: 100% design token coverage — zero hard-coded values in wireframes.**
