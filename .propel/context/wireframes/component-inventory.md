# Component Inventory - Unified Patient Access & Clinical Intelligence Platform

## Component Specification

**Fidelity Level**: High
**Screen Type**: Web (Responsive)
**Viewport**: 1440px × 900px (Primary Desktop)

## Component Summary

| Component Name | Type | Screens Used | Priority | Implementation Status |
|---------------|------|-------------|----------|---------------------|
| Header | Layout | All screens | High | Pending |
| Sidebar | Layout | Staff, Admin portals | High | Pending |
| Button | Interactive | All screens | High | Pending |
| TextField | Interactive | SCR-002, SCR-005, SCR-007, SCR-014, SCR-017, SCR-019 | High | Pending |
| Card | Content | SCR-003, SCR-006, SCR-009, SCR-015, SCR-018, SCR-020 | High | Pending |
| StatCard | Content | SCR-003, SCR-011, SCR-018 | High | Pending |
| DataTable | Content | SCR-009, SCR-010, SCR-012, SCR-013, SCR-016, SCR-019, SCR-021 | High | Pending |
| Modal | Feedback | SCR-004, SCR-005, SCR-019 | High | Pending |
| Calendar | DataDisplay | SCR-004 | High | Pending |
| Badge | Content | Multiple screens | High | Pending |
| Toast | Feedback | All screens | High | Pending |
| Alert | Feedback | SCR-002, SCR-005, SCR-017 | High | Pending |
| ChatBubble | DataDisplay | SCR-006 | High | Pending |
| FileUpload | Interactive | SCR-008 | High | Pending |
| Tabs | Navigation | SCR-009 | High | Pending |
| QueueList | DataDisplay | SCR-011, SCR-013 | High | Pending |
| ComparePanel | DataDisplay | SCR-015 | Medium | Pending |
| CodeCard | Content | SCR-016 | Medium | Pending |
| VitalChart | DataDisplay | SCR-009 | Medium | Pending |

## Detailed Component Specifications

### Layout Components

#### Header
- **Type**: Layout
- **Used In Screens**: All screens
- **Wireframe References**:
  - All wireframe files
- **Description**: Primary navigation header with logo, navigation items, and user menu
- **Variants**: Public (minimal), Patient (with nav), Staff (with role indicator), Admin (with role indicator)
- **Interactive States**: Default, Hover on nav items
- **Responsive Behavior**:
  - Desktop (1440px): Full horizontal nav, user dropdown on right
  - Tablet (768px): Collapsed, hamburger menu trigger
  - Mobile (375px): Logo + hamburger menu, bottom nav supplements
- **Design Tokens Applied**:
  - Background: `color.surface.primary` (#FFFFFF)
  - Border: `color.border.default` (#E5E7EB)
  - Height: 64px
  - Z-index: 10
- **Implementation Notes**: Sticky positioning, accounts for portal-specific accents

#### Sidebar
- **Type**: Layout
- **Used In Screens**: Staff Portal (SCR-011 to SCR-017), Admin Portal (SCR-018 to SCR-021)
- **Wireframe References**:
  - [wireframe-SCR-011-staff-dashboard.html](./Hi-Fi/wireframe-SCR-011-staff-dashboard.html)
  - [wireframe-SCR-018-admin-dashboard.html](./Hi-Fi/wireframe-SCR-018-admin-dashboard.html)
- **Description**: Vertical navigation sidebar with collapsible menu items
- **Variants**: Expanded (280px), Collapsed (64px icons only)
- **Interactive States**: Default, Hover, Active item highlighted
- **Responsive Behavior**:
  - Desktop (1440px): Expanded 280px with labels
  - Tablet (768px): Collapsed 64px icons only
  - Mobile (375px): Hidden, replaced by bottom nav
- **Design Tokens Applied**:
  - Background: `color.surface.secondary` (#F9FAFB)
  - Active item: `color.role.staff` or `color.role.admin` accent
  - Width: 280px (expanded) / 64px (collapsed)
- **Implementation Notes**: Smooth collapse animation 250ms

### Navigation Components

#### Tabs
- **Type**: Navigation
- **Used In Screens**: SCR-009 (360-Degree View)
- **Wireframe References**: [wireframe-SCR-009-360-view.html](./Hi-Fi/wireframe-SCR-009-360-view.html)
- **Description**: Horizontal tab navigation for content sections
- **Variants**: Default, With badge count
- **Interactive States**: Default, Hover, Active (selected), Focus
- **Responsive Behavior**:
  - Desktop (1440px): Horizontal tabs inline
  - Tablet (768px): Horizontal with scroll
  - Mobile (375px): Dropdown selector
- **Design Tokens Applied**:
  - Active border: `color.primary` (#2E8FE5)
  - Tab padding: `spacing.3` (12px) vertical, `spacing.4` (16px) horizontal
- **Implementation Notes**: Active tab indicator animates 150ms

#### Breadcrumb
- **Type**: Navigation
- **Used In Screens**: All authenticated screens (desktop only)
- **Description**: Secondary navigation showing page hierarchy
- **Variants**: Default
- **Interactive States**: Hover on links
- **Responsive Behavior**:
  - Desktop (1440px): Visible below header
  - Tablet (768px): Hidden
  - Mobile (375px): Hidden
- **Design Tokens Applied**:
  - Link color: `color.text.link` (#2573C4)
  - Separator: `color.text.tertiary` (#9CA3AF)

#### Pagination
- **Type**: Navigation
- **Used In Screens**: SCR-010, SCR-019, SCR-021
- **Description**: Page navigation for data tables
- **Variants**: Default, With page size selector
- **Interactive States**: Default, Hover, Active page, Disabled
- **Responsive Behavior**:
  - Desktop (1440px): Full pagination with page numbers
  - Mobile (375px): Previous/Next only
- **Design Tokens Applied**:
  - Button size: 32px × 32px
  - Active bg: `color.primary` (#2E8FE5)

### Content Components

#### Card
- **Type**: Content
- **Used In Screens**: SCR-003, SCR-006, SCR-009, SCR-015, SCR-018, SCR-020
- **Wireframe References**: Multiple screens
- **Description**: Container for grouped content with optional header and actions
- **Variants**: Default, Elevated, Interactive (clickable)
- **Interactive States**: Default, Hover (elevated shadow), Focus
- **Responsive Behavior**:
  - Desktop (1440px): Fixed width in grid
  - Mobile (375px): Full width stacked
- **Design Tokens Applied**:
  - Background: `color.surface.primary` (#FFFFFF)
  - Border: `color.border.default` (#E5E7EB)
  - Radius: `radius.lg` (12px)
  - Padding: `spacing.4` (16px)
  - Shadow: `shadow.sm` default, `shadow.md` on hover
- **Implementation Notes**: shadcn/ui Card component

#### StatCard
- **Type**: Content
- **Used In Screens**: SCR-003, SCR-011, SCR-018
- **Wireframe References**:
  - [wireframe-SCR-003-patient-dashboard.html](./Hi-Fi/wireframe-SCR-003-patient-dashboard.html)
  - [wireframe-SCR-011-staff-dashboard.html](./Hi-Fi/wireframe-SCR-011-staff-dashboard.html)
- **Description**: Dashboard statistics card with icon, value, label, and trend
- **Variants**: Default, With trend indicator (up/down)
- **Interactive States**: Default, Hover
- **Responsive Behavior**:
  - Desktop (1440px): 4-column grid
  - Tablet (768px): 2-column grid
  - Mobile (375px): 2-column grid (compact)
- **Design Tokens Applied**:
  - Value font: `typography.display.sm` (30px/600)
  - Label font: `typography.caption` (12px/400)
  - Icon size: 24px

#### AppointmentCard
- **Type**: Content
- **Used In Screens**: SCR-003
- **Description**: Appointment summary card with date, provider, status
- **Variants**: Upcoming, Past, Cancelled
- **Interactive States**: Default, Hover (shows actions)
- **Responsive Behavior**:
  - Desktop (1440px): Horizontal layout
  - Mobile (375px): Stacked layout
- **Design Tokens Applied**:
  - Border-left accent by status: success/warning/neutral

#### DocumentCard
- **Type**: Content
- **Used In Screens**: SCR-008, SCR-009
- **Description**: Document thumbnail with metadata and actions
- **Variants**: Default, Processing, Error
- **Interactive States**: Default, Hover (shows preview action)
- **Responsive Behavior**:
  - Desktop (1440px): Grid of cards
  - Mobile (375px): List view
- **Design Tokens Applied**:
  - Thumbnail size: 64px × 64px
  - Border: Processing shows animated border

#### CodeCard
- **Type**: Content
- **Used In Screens**: SCR-016
- **Description**: Medical code display with confidence indicator
- **Variants**: ICD-10, CPT
- **Interactive States**: Default, Selected, Verified, Rejected
- **Responsive Behavior**:
  - Desktop (1440px): Card grid with evidence drawer
  - Mobile (375px): Stacked list
- **Design Tokens Applied**:
  - Confidence badge: `color.ai.confidence.*`
  - AI indicator: sparkles icon + `color.ai.suggestion`

#### Badge
- **Type**: Content
- **Used In Screens**: Multiple screens
- **Description**: Status indicator for categorization and counts
- **Variants**: Default (gray), Primary (blue), Success (green), Warning (amber), Error (red), AI (blue dotted), Verified (green)
- **Interactive States**: Static (no interaction)
- **Responsive Behavior**: Same across all breakpoints
- **Design Tokens Applied**:
  - Height: 20px
  - Padding: 2px 8px
  - Radius: `radius.full` (9999px)
  - Font: `typography.overline` (11px/500)
- **Implementation Notes**: Per designsystem.md badge specs

#### DataTable
- **Type**: Content
- **Used In Screens**: SCR-009, SCR-010, SCR-012, SCR-013, SCR-016, SCR-019, SCR-021
- **Description**: Tabular data with sorting, filtering, and row actions
- **Variants**: Default, Selectable (with checkboxes), Expandable rows
- **Interactive States**: Row hover, Row selected, Column sort active
- **Responsive Behavior**:
  - Desktop (1440px): Full columns, fixed header
  - Tablet (768px): Horizontal scroll, priority columns
  - Mobile (375px): Card-based list view
- **Design Tokens Applied**:
  - Header bg: `color.surface.secondary` (#F9FAFB)
  - Row hover: `color.neutral.50` (#F9FAFB)
  - Cell padding: `spacing.3` × `spacing.4`
- **Implementation Notes**: Keyboard navigation support

### Interactive Components

#### Button
- **Type**: Interactive
- **Used In Screens**: All screens
- **Description**: Primary action trigger
- **Variants**: Primary, Secondary, Tertiary, Ghost, Destructive
- **Interactive States**: Default, Hover, Active, Focus, Disabled, Loading
- **Responsive Behavior**:
  - Desktop (1440px): Inline with content
  - Mobile (375px): Full width for primary actions
- **Design Tokens Applied**:
  - Height: 32px (sm), 40px (md), 48px (lg)
  - Radius: `radius.md` (8px)
  - Transition: `duration.fast` (150ms)
  - Focus ring: `shadow.focus`
- **Implementation Notes**: Icons optional (leading/trailing), loading shows spinner

#### TextField
- **Type**: Interactive
- **Used In Screens**: SCR-002, SCR-005, SCR-007, SCR-014, SCR-017, SCR-019
- **Description**: Single-line text input
- **Variants**: Default, With icon, With validation, Password
- **Interactive States**: Default, Hover, Focus, Error, Disabled
- **Responsive Behavior**:
  - All breakpoints: Full width within container
- **Design Tokens Applied**:
  - Height: 40px
  - Border: `color.border.default` (#E5E7EB)
  - Focus border: `color.border.focus` (#2E8FE5)
  - Error border: `color.border.error` (#EF4444)
  - Radius: `radius.md` (8px)
- **Implementation Notes**: Label above, helper text below, error replaces helper

#### Select
- **Type**: Interactive
- **Used In Screens**: SCR-005, SCR-007, SCR-014, SCR-021
- **Description**: Dropdown selection
- **Variants**: Default, Multi-select, Searchable
- **Interactive States**: Default, Hover, Focus, Open, Disabled
- **Responsive Behavior**:
  - Desktop (1440px): Dropdown popover
  - Mobile (375px): Native select or full-screen picker
- **Design Tokens Applied**: Same as TextField
- **Implementation Notes**: shadcn/ui Select component

#### Checkbox
- **Type**: Interactive
- **Used In Screens**: SCR-007, SCR-013, SCR-020
- **Description**: Boolean toggle for multiple selections
- **Variants**: Default, Indeterminate
- **Interactive States**: Unchecked, Checked, Indeterminate, Focus, Disabled
- **Responsive Behavior**: Same across breakpoints
- **Design Tokens Applied**:
  - Size: 16px × 16px
  - Checked bg: `color.primary` (#2E8FE5)
  - Radius: `radius.sm` (4px)

#### Radio
- **Type**: Interactive
- **Used In Screens**: SCR-007, SCR-015
- **Description**: Single selection from options
- **Variants**: Default, With description
- **Interactive States**: Unselected, Selected, Focus, Disabled
- **Responsive Behavior**: Same across breakpoints
- **Design Tokens Applied**: Similar to Checkbox

#### Toggle
- **Type**: Interactive
- **Used In Screens**: SCR-006 (AI/Manual switch), SCR-020
- **Description**: On/off switch
- **Variants**: Default, With label
- **Interactive States**: Off, On, Focus, Disabled
- **Design Tokens Applied**:
  - Track: 44px × 24px
  - On color: `color.primary` (#2E8FE5)

#### FileUpload
- **Type**: Interactive
- **Used In Screens**: SCR-008
- **Description**: File drop zone with preview
- **Variants**: Default, Drag active, Uploading
- **Interactive States**: Default, Drag over, Uploading, Complete, Error
- **Responsive Behavior**:
  - Desktop (1440px): Large drop zone
  - Mobile (375px): Compact with button trigger
- **Design Tokens Applied**:
  - Border: Dashed `color.border.default`
  - Drag active border: `color.primary`

#### DatePicker
- **Type**: Interactive
- **Used In Screens**: SCR-021
- **Description**: Date selection calendar picker
- **Variants**: Single date, Date range
- **Interactive States**: Default, Open, Date hover, Date selected
- **Design Tokens Applied**: Similar to Select + Calendar

### Feedback Components

#### Modal
- **Type**: Feedback
- **Used In Screens**: SCR-004, SCR-005, SCR-019
- **Description**: Overlay dialog for focused tasks
- **Variants**: Default (md), Small (sm), Large (lg)
- **Interactive States**: Opening animation, Open, Closing animation
- **Responsive Behavior**:
  - Desktop (1440px): Centered, max-width 560px
  - Mobile (375px): Full screen slide up
- **Design Tokens Applied**:
  - Background: `color.surface.primary`
  - Overlay: rgba(0, 0, 0, 0.5)
  - Radius: `radius.lg` (12px)
  - Shadow: `shadow.xl`
  - Animation: `duration.normal` (250ms) with `easing.out`
- **Implementation Notes**: Focus trap, ESC to close, overlay click to close

#### Drawer
- **Type**: Feedback
- **Used In Screens**: SCR-008, SCR-009, SCR-015, SCR-016
- **Description**: Side panel for supplementary content
- **Variants**: Right (default), Left, Bottom (mobile filters)
- **Interactive States**: Closed, Open, Closing
- **Responsive Behavior**:
  - Desktop (1440px): 400px width from right
  - Mobile (375px): Full width bottom sheet
- **Design Tokens Applied**:
  - Width: 400px (desktop)
  - Shadow: `shadow.xl`
- **Implementation Notes**: Content scrolls independently

#### Toast
- **Type**: Feedback
- **Used In Screens**: All screens
- **Description**: Temporary notification messages
- **Variants**: Success, Warning, Error, Info
- **Interactive States**: Entering, Visible, Exiting
- **Responsive Behavior**: Same position (top-right), responsive width
- **Design Tokens Applied**:
  - Position: Top-right, 16px from edges
  - Duration: 5000ms auto-dismiss
  - Shadow: `shadow.lg`
  - Border-left: 4px colored by type
- **Implementation Notes**: Queue management for multiple toasts

#### Alert
- **Type**: Feedback
- **Used In Screens**: SCR-002, SCR-005, SCR-017
- **Description**: Inline contextual messages
- **Variants**: Success, Warning, Error, Info
- **Interactive States**: Static, Dismissible
- **Responsive Behavior**: Full width in container
- **Design Tokens Applied**:
  - Background: `color.{type}.bg`
  - Border-left: 4px `color.{type}`
  - Icon color: `color.{type}`
  - Radius: `radius.md` (8px)

#### Tooltip
- **Type**: Feedback
- **Used In Screens**: Multiple (on icons and truncated text)
- **Description**: Contextual help on hover
- **Variants**: Default, Rich (with formatting)
- **Interactive States**: Hidden, Visible
- **Responsive Behavior**:
  - Desktop: Hover-triggered
  - Mobile: Touch-hold or tap
- **Design Tokens Applied**:
  - Background: `color.neutral.800`
  - Text: `color.text.inverse`
  - Radius: `radius.sm` (4px)
  - Shadow: `shadow.md`

#### Skeleton
- **Type**: Feedback
- **Used In Screens**: All screens (loading states)
- **Description**: Placeholder for loading content
- **Variants**: Text line, Card, Avatar, Table row
- **Interactive States**: Animated shimmer
- **Design Tokens Applied**:
  - Background: `color.neutral.200`
  - Highlight: `color.neutral.300`
  - Animation: shimmer 1.5s infinite

### Data Display Components

#### Calendar
- **Type**: DataDisplay
- **Used In Screens**: SCR-004
- **Description**: Interactive date and time slot picker
- **Variants**: Month view, Week view, Day view
- **Interactive States**: Date hover, Date selected, Slot available, Slot unavailable
- **Responsive Behavior**:
  - Desktop (1440px): Month view default
  - Tablet (768px): Week view default
  - Mobile (375px): Day view with swipe
- **Design Tokens Applied**:
  - Selected date: `color.primary` bg
  - Available slot: `color.success` indicator
  - Unavailable: `color.neutral.300` text
- **Implementation Notes**: Touch gestures for mobile navigation

#### QueueList
- **Type**: DataDisplay
- **Used In Screens**: SCR-011, SCR-013
- **Description**: Real-time patient queue display
- **Variants**: Summary (dashboard), Full (management)
- **Interactive States**: Row hover, Row selected for action
- **Responsive Behavior**:
  - Same principles as DataTable
- **Design Tokens Applied**:
  - Status colors: Waiting (amber), In Progress (blue), Complete (green)
  - Timer: countdown with red when overdue

#### ComparePanel
- **Type**: DataDisplay
- **Used In Screens**: SCR-015
- **Description**: Side-by-side comparison of conflicting data
- **Variants**: Two sources, Multiple sources
- **Interactive States**: Value hover, Value selected
- **Responsive Behavior**:
  - Desktop (1440px): Side-by-side columns
  - Mobile (375px): Stacked comparison
- **Design Tokens Applied**:
  - Highlight differences: `color.warning.bg`
  - Selected value: `color.success` border

#### VitalChart
- **Type**: DataDisplay
- **Used In Screens**: SCR-009
- **Description**: Line chart for patient vitals over time
- **Variants**: Blood pressure, Heart rate, Weight
- **Interactive States**: Point hover shows tooltip
- **Responsive Behavior**:
  - Desktop: Full chart
  - Mobile: Simplified sparkline
- **Design Tokens Applied**:
  - Line colors: `color.primary` and variants
  - Grid: `color.border.default`

#### ActivityFeed
- **Type**: DataDisplay
- **Used In Screens**: SCR-018
- **Description**: Timeline of recent system activities
- **Variants**: Default, Compact
- **Interactive States**: Item hover
- **Responsive Behavior**: Same across breakpoints
- **Design Tokens Applied**:
  - Timeline line: `color.border.default`
  - Icon bg: varies by activity type

#### ChatBubble
- **Type**: DataDisplay
- **Used In Screens**: SCR-006
- **Description**: Conversational message display
- **Variants**: User message (right), AI message (left), System message (center)
- **Interactive States**: Default, Typing indicator (AI)
- **Responsive Behavior**: Full width with max-width
- **Design Tokens Applied**:
  - User bubble: `color.primary.subtle` (#E8F4FD)
  - AI bubble: `color.surface.secondary` (#F9FAFB)
  - AI indicator: sparkles icon
- **Implementation Notes**: Typing animation for AI responses

#### Timer
- **Type**: DataDisplay
- **Used In Screens**: SCR-013
- **Description**: Wait time countdown display
- **Variants**: Normal, Warning (>15min), Critical (>30min)
- **Interactive States**: Counting, Paused, Overdue
- **Design Tokens Applied**:
  - Normal: `color.text.primary`
  - Warning: `color.warning`
  - Critical: `color.error`

## Component Relationships

```
Layout Hierarchy
├── Header
│   ├── Logo
│   ├── Navigation Items
│   └── User Menu (Avatar + Dropdown)
│
├── Sidebar (Staff/Admin)
│   ├── Navigation Items
│   │   └── Badge (notification count)
│   └── Collapse Toggle
│
├── Main Content
│   ├── Breadcrumb
│   ├── Page Header (Title + Actions)
│   └── Content Area
│       ├── Cards / StatCards
│       ├── DataTables
│       ├── Forms (TextField, Select, etc.)
│       └── Feedback (Alert, Toast)
│
└── Modals / Drawers (overlay layer)
    ├── Modal Header + Close
    ├── Modal Content
    └── Modal Footer (Buttons)
```

## Component States Matrix

| Component | Default | Hover | Active | Focus | Disabled | Error | Loading | Empty |
|-----------|---------|-------|--------|-------|----------|-------|---------|-------|
| Button | ✓ | ✓ | ✓ | ✓ | ✓ | - | ✓ | - |
| TextField | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | - | - |
| Select | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | - | ✓ |
| Checkbox | ✓ | ✓ | - | ✓ | ✓ | - | - | - |
| Card | ✓ | ✓ | - | - | - | - | ✓ | ✓ |
| DataTable | ✓ | ✓ | - | ✓ | - | - | ✓ | ✓ |
| Modal | ✓ | - | - | ✓ | - | - | ✓ | - |
| Badge | ✓ | - | - | - | - | - | - | - |
| Toast | ✓ | - | - | - | - | - | - | - |
| Calendar | ✓ | ✓ | ✓ | ✓ | ✓ | - | ✓ | ✓ |
| FileUpload | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | - |

## Reusability Analysis

| Component | Reuse Count | Screens | Recommendation |
|-----------|-------------|---------|----------------|
| Button | 21 screens | All | Core shared component |
| Card | 12 screens | Multiple | Core shared component |
| TextField | 8 screens | Forms | Core shared component |
| Badge | 15 screens | Multiple | Core shared component |
| DataTable | 7 screens | Data views | Core shared component |
| StatCard | 3 screens | Dashboards | Dashboard-specific variant |
| ChatBubble | 1 screen | SCR-006 | AI Intake specific |
| ComparePanel | 1 screen | SCR-015 | Conflict Review specific |
| CodeCard | 1 screen | SCR-016 | Code Verification specific |

## Responsive Breakpoints Summary

| Breakpoint | Width | Components Affected | Key Adaptations |
|-----------|-------|-------------------|-----------------|
| Mobile | 375px | All | Single column, bottom nav, touch targets 44px+, full-width inputs |
| Tablet | 768px | Sidebar, DataTable, Calendar | 2-column grid, collapsed sidebar, week view calendar |
| Desktop | 1440px | All | Multi-column, 12-col grid, expanded sidebar, full features |

## Implementation Priority Matrix

### High Priority (Core Components - Week 1)
- [x] Button - Primary user interaction
- [x] TextField - Form foundation
- [x] Card - Content container
- [x] Header - Global navigation
- [x] Badge - Status indicators
- [x] Toast - User feedback
- [x] Modal - Focused tasks

### Medium Priority (Feature Components - Week 2-3)
- [ ] DataTable - Data display foundation
- [ ] Sidebar - Portal navigation
- [ ] StatCard - Dashboard metrics
- [ ] Calendar - Appointment booking
- [ ] Select - Form dropdowns
- [ ] Alert - Inline feedback
- [ ] Tabs - Content organization

### Lower Priority (Specialized Components - Week 4+)
- [ ] ChatBubble - AI intake
- [ ] ComparePanel - Conflict resolution
- [ ] CodeCard - Code verification
- [ ] VitalChart - Health data
- [ ] FileUpload - Document management
- [ ] QueueList - Queue management

## Framework-Specific Notes
**Detected Framework**: React with Tailwind CSS
**Component Library**: shadcn/ui (Radix primitives)

### Framework Patterns Applied
- **Composition**: Compound components (Card.Header, Card.Content)
- **Variants**: CVA (Class Variance Authority) for variant styling
- **Animation**: Framer Motion for transitions
- **Forms**: React Hook Form + Zod validation

### Component Library Mappings
| Wireframe Component | shadcn/ui Component | Customization Required |
|-------------------|-------------------|----------------------|
| Button | Button | Color variants per portal |
| TextField | Input | Healthcare validation patterns |
| Card | Card | AI indicator variant |
| Modal | Dialog | Full-screen mobile adaptation |
| Select | Select | Native fallback on mobile |
| DataTable | Table + tanstack/react-table | Keyboard navigation |
| Toast | Toast (Sonner) | Healthcare-appropriate messaging |
| Badge | Badge | AI and Verified variants |
| Tabs | Tabs | Mobile dropdown adaptation |
| Calendar | Calendar | Custom slot selection |

## Accessibility Considerations

| Component | ARIA Attributes | Keyboard Navigation | Screen Reader Notes |
|-----------|----------------|-------------------|-------------------|
| Button | role="button", aria-disabled | Enter/Space to activate | Label announced |
| TextField | aria-invalid, aria-describedby | Tab to focus, type to input | Error messages linked |
| Modal | role="dialog", aria-modal="true" | Tab trap, Esc to close | Focus returned on close |
| DataTable | role="grid", aria-rowcount | Arrow keys for cells | Row selection announced |
| Calendar | role="grid", aria-selected | Arrow keys for dates | Date announced on focus |
| Toast | role="alert", aria-live="polite" | N/A (auto-dismiss) | Message announced |
| Tabs | role="tablist", aria-selected | Arrow keys between tabs | Tab label announced |

## Design System Integration

**Design System Reference**: [designsystem.md](../docs/designsystem.md)

### Components Matching Design System
- [x] All components use design tokens from designsystem.md
- [x] Color palette applied consistently
- [x] Typography scale followed
- [x] Spacing and radius tokens used
- [x] Shadow/elevation system applied
- [x] Motion/animation tokens used

### New Components to Add to Design System
- [ ] ChatBubble - AI intake pattern
- [ ] ComparePanel - Conflict resolution pattern
- [ ] CodeCard - Medical code display pattern
- [ ] QueueList - Real-time queue pattern
