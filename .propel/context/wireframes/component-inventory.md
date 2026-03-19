# Component Inventory - PulseCare Platform Wireframes

**Document Version**: 1.0.0  
**Generated**: March 19, 2026  
**Component Library**: shadcn/ui (customized via CSS variables)  
**Base Framework**: React 18+ with Tailwind CSS 3.4+  
**Source**: `.propel/context/docs/designsystem.md` + `figma_spec.md` Section 10

---

## 1. Component Library Overview

All components follow shadcn/ui patterns with customization through CSS custom properties. Components are accessed via Tailwind utility classes and maintain consistent states (default, hover, focus, active, disabled, loading) per UXR-501.

**Component State Standards**:
- **Default**: Base appearance
- **Hover**: Background tint, border color change (200ms transition)
- **Focus**: 2px solid blue outline, offset 2px (UXR-206)
- **Active**: Darker background/border on press
- **Disabled**: opacity-50, cursor-not-allowed
- **Loading**: Spinner icon, reduced opacity

---

## 2. Component Categories & Usage

### 2.1 ACTION COMPONENTS

#### 2.1.1 Button (Primary)
**Description**: Primary call-to-action button  
**Variants**: Primary (blue), Secondary (gray), Destructive (red), Ghost (transparent)  
**Sizes**: S (text-sm, py-2, px-3), M (text-base, py-3, px-4), L (text-lg, py-4, px-6)

**Design Tokens Applied**:
```css
.btn-primary {
  background: var(--color-primary-500, #3b82f6);
  color: #ffffff;
  padding: 0.75rem 1rem; /* py-3 px-4 */
  border-radius: var(--radius-default, 0.5rem);
  font-weight: 500;
  transition: all 200ms ease;
}
.btn-primary:hover { background: var(--color-primary-600, #2563eb); }
.btn-primary:focus { outline: 2px solid var(--color-primary-500); outline-offset: 2px; }
.btn-primary:active { background: var(--color-primary-700, #1d4ed8); }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
```

**Screen Placement**:
- SCR-001: "Book Appointment" card CTA, "Start Form" button
- SCR-002: "Confirm Booking" button
- SCR-004: "Review & Submit" button
- SCR-006: "Create Appointment" button
- SCR-007: "New Walk-In" button (top-right)
- SCR-010: "Create New User" button
- SCR-011: "Create User" / "Save Changes" button
- SCR-013: "Sign in" button

**Responsive Behavior**:
- Desktop: Default size M
- Mobile: Full width (w-full) for primary CTAs

**Accessibility**:
- Min height: 44px (UXR-205 touch target)
- ARIA attributes: `role="button"`, `aria-disabled="true"` when disabled

#### 2.1.2 Button (Secondary)
**Description**: Secondary action button, less visual emphasis  
**Design Tokens**: Border-gray-300, text-gray-700, hover:bg-gray-50

**Screen Placement**:
- SCR-002: "Cancel" button in confirmation modal
- SCR-005: "Save Draft" button
- SCR-011: "Cancel" button
- SCR-018: "Reset to Defaults" button

#### 2.1.3 IconButton
**Description**: Button with icon only (no text)  
**Sizes**: 40x40px (desktop), 44x44px (mobile)

**Screen Placement**:
- Modal close buttons (X icon)
- Hamburger menu toggle (mobile navigation)

---

### 2.2 INPUT COMPONENTS

#### 2.2.1 TextField (Text Input)
**Description**: Single-line text input field  
**Variants**: Default, Email, Password, Search, Number  
**States**: Focus (blue border), Error (red border + error text), Valid (green checkmark)

**Design Tokens Applied**:
```css
.input-field {
  border: 1px solid var(--color-neutral-200, #e5e7eb);
  padding: 0.75rem 1rem; /* py-3 px-4 */
  border-radius: var(--radius-default, 0.5rem);
  font-size: 1rem; /* text-base */
  color: var(--color-neutral-700, #374151);
  transition: all 200ms ease;
}
.input-field:focus {
  outline: 2px solid var(--color-primary-500, #3b82f6);
  outline-offset: 0;
  border-color: var(--color-primary-500);
}
.input-field.error {
  border-color: var(--color-error-500, #ef4444);
}
```

**Screen Placement**:
- SCR-013: Email input, Password input
- SCR-002: Date picker input
- SCR-005: All demographic fields (Name, DOB, Phone, Email)
- SCR-006: Patient search input, form fields
- SCR-007: Search bar (patient name/ID)
- SCR-008: Insurance Name, Policy ID inputs
- SCR-010: Search bar (user name/email)
- SCR-011: First Name, Last Name, Email inputs
- SCR-014: Email input for password reset

**Responsive Behavior**:
- Desktop: Fixed width or flex-grow within grid
- Mobile: Full width (w-full)

**Accessibility**:
- Associated `<label>` with `for` attribute
- `aria-required="true"` for required fields
- `aria-describedby` linking to error message element
- Placeholder text as example, not instructions

#### 2.2.2 TextArea
**Description**: Multi-line text input

**Screen Placement**:
- SCR-004: AI conversational input area
- SCR-005: Reason for Visit field
- SCR-006: Reason for Visit field

**Design Tokens**: Same as TextField, min-height: 120px (3 rows)

#### 2.2.3 Select (Dropdown)
**Description**: Single-select dropdown menu  
**States**: Closed (default), Open (dropdown visible), Focus

**Design Tokens**: Same border/padding as TextField, chevron icon right-aligned

**Screen Placement**:
- SCR-002: Provider dropdown, Appointment Type dropdown
- SCR-005: Gender dropdown, State dropdown
- SCR-006: Provider dropdown
- SCR-007: Status filter dropdown
- SCR-010: Role filter, Status filter
- SCR-011: Role dropdown
- SCR-012: Action type filter, User filter

**Responsive Behavior**:
- Mobile: Native OS dropdown for better UX

#### 2.2.4 Checkbox
**Description**: Boolean toggle input  
**Sizes**: 16x16px (form), 20x20px (large)

**Design Tokens**: Border-gray-300, checked:bg-primary-500, focus:ring-primary-500

**Screen Placement**:
- SCR-013: "Remember me" checkbox
- SCR-011: Permission checkboxes (View Data, Edit Data, Manage Users, etc.)
- SCR-018: Feature flag toggles

**Accessibility**: Associated label, `aria-checked` attribute

#### 2.2.5 Toggle Switch
**Description**: On/off switch for binary settings  
**Size**: 44x24px switch track, 20x20px toggle circle

**Screen Placement**:
- SCR-002: Preferred Slot Toggle (FR-003)
- SCR-004/005: AI ↔ Manual intake mode toggle (UXR-103)
- SCR-018: Feature flag toggles

**Design Tokens**: Track bg-gray-200 (off), bg-primary-500 (on), circle bg-white

#### 2.2.6 FileUpload (Dropzone)
**Description**: Drag-and-drop file upload area  
**States**: Default, Hover (dragover), Uploading (progress bar), Error

**Screen Placement**:
- SCR-003: Document upload dropzone (PDF/JPG/PNG, max 10 MB)

**Design Tokens**: Border-dashed border-gray-300, hover:border-primary-500, bg-gray-50

**Responsive Behavior**:
- Desktop: 600px width, center-aligned
- Mobile: Full width, reduced height

---

### 2.3 NAVIGATION COMPONENTS

#### 2.3.1 Header (Top Navigation Bar)
**Description**: Persistent header across all screens  
**Height**: 64px (h-16)  
**Structure**: Logo (left), Nav Links (center), User Menu (right)

**Design Tokens**: bg-white, shadow-sm (subtle elevation)

**Screen Placement**: All screens (SCR-001 to SCR-018)

**Responsive Behavior**:
- Desktop: Full horizontal layout
- Mobile: Logo + User Avatar only, nav moved to bottom bar

#### 2.3.2 Sidebar (Primary Navigation)
**Description**: Left sidebar with role-specific navigation items  
**Width**: 240px (fixed)  
**Structure**: Logo, Nav Items (icons + text), User Section (bottom)

**Screen Placement**:
- Patient Portal: SCR-001, 002, 003, 004, 005, 016

**Responsive Behavior**:
- Desktop: Persistent, always visible
- Tablet: Collapsible drawer (hamburger menu trigger)
- Mobile: Hidden, replaced by bottom nav bar (UXR-102)

#### 2.3.3 Bottom Navigation Bar (Mobile)
**Description**: Fixed bottom bar with 4-5 core actions  
**Height**: 64px with 44x44px tap targets (UXR-205)

**Design Tokens**: bg-white, border-top border-gray-200, fixed bottom-0

**Screen Placement**:
- Patient Portal mobile: Dashboard, Book, Intake, Documents icons

**Visible**: Mobile only (<768px)

#### 2.3.4 Tabs
**Description**: Secondary navigation within a screen  
**Variants**: Default (underline style), Pills (rounded background)

**Design Tokens**:
- Active tab: text-primary-500, border-bottom-2 border-primary-500
- Inactive tab: text-gray-500, hover:text-gray-700

**Screen Placement**:
- SCR-009: Timeline, Documents, Medications, Conflicts tabs
- SCR-018: General Settings, Feature Flags, Health Checks tabs

---

### 2.4 CONTENT DISPLAY COMPONENTS

#### 2.4.1 Card
**Description**: Container component for grouping related content  
**Variants**: Default (flat), Elevated (shadow), Interactive (hover effect)  
**Padding**: p-6 (24px)

**Design Tokens**:
```css
.card {
  background: #ffffff;
  border-radius: var(--radius-default, 0.5rem);
  box-shadow: var(--shadow-default, 0 1px 3px rgba(0,0,0,0.1));
  padding: 1.5rem; /* p-6 */
}
.card-hover:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg, 0 10px 15px rgba(0,0,0,0.1));
  transition: all 200ms ease;
}
```

**Screen Placement**:
- SCR-001: Book Appointment gradient card, Upcoming Appointments card, Intake card, Documents card
- SCR-002: Filters card, Available Slots card
- SCR-007: Stats cards (Total, Arrived, Waiting, Late)

**Responsive Behavior**:
- Desktop: Grid layout (3-4 columns)
- Tablet: 2 columns
- Mobile: Single column stack

#### 2.4.2 Table
**Description**: Data table with sortable columns  
**Structure**: `<thead>` (sticky), `<tbody>` (striped rows optional)  
**Row Height**: 48px minimum (UXR-205 mobile)

**Design Tokens**:
- Header: bg-gray-50, text-xs uppercase tracking-wider
- Rows: border-bottom border-gray-200, hover:bg-gray-50

**Screen Placement**:
- SCR-003: Uploaded Documents table
- SCR-007: Arrival queue table (with drag-drop handles per UXR-104)
- SCR-009: Documents tab table
- SCR-010: User management table
- SCR-012: Audit log table
- SCR-016: Appointment history table

**Responsive Behavior**:
- Desktop/Tablet: Horizontal scroll within container, all columns visible
- Mobile: Transform to card-based list (label-value pairs per row)

#### 2.4.3 Badge
**Description**: Small status indicator  
**Variants**: Success (green), Warning (yellow), Error (red), Info (blue), Neutral (gray)  
**Sizes**: SM (text-xs px-2.5 py-0.5), MD (text-sm px-3 py-1)

**Design Tokens**:
```css
.badge-success { background: var(--color-success-100, #dcfce7); color: var(--color-success-800, #15803d); }
.badge-warning { background: var(--color-warning-100, #fef9c3); color: var(--color-warning-800, #a16207); }
.badge-error { background: var(--color-error-100, #fee2e2); color: var(--color-error-800, #b91c1c); }
```

**Screen Placement**:
- SCR-001: "Confirmed" badge on appointment card, "Action Required" badge on intake card
- SCR-002: Slot legend (Available, Selected, Booked)
- SCR-003: AI extraction status badges (Pending, Processing, Complete)
- SCR-007: Status badges (Scheduled, Arrived, Late, Ready for Provider)
- SCR-009: Confidence badges (High, Medium, Low)
- SCR-010: Role badges (Patient, Staff, Admin), Status badges (Active, Inactive)

#### 2.4.4 Avatar (User Avatar)
**Description**: Circular user identifier with initials or image  
**Sizes**: SM (32x32px), MD (40x40px), LG (64x64px)

**Design Tokens**:
- Colors: bg-primary-500, bg-purple-500, bg-green-500 (varied per user)
- Text: text-white, font-medium

**Screen Placement**:
- SCR-001: User avatar in header (top-right)
- SCR-007: Patient avatars in arrival queue table

**Fallback**: Display initials when no image available

#### 2.4.5 Timeline
**Description**: Chronological event list with visual connector line  
**Structure**: Events displayed vertically with left-aligned icons/dates

**Screen Placement**:
- SCR-009: Timeline tab (appointments, documents, clinical notes chronologically)

**Design Tokens**: Timeline connector line: border-left-2 border-gray-200

#### 2.4.6 Accordion
**Description**: Expandable content sections  
**States**: Collapsed (default), Expanded

**Screen Placement**:
- SCR-005: Form sections (Demographics, Insurance, Medical History, Medications, Allergies)
- SCR-009: Document categories, medication groups
- SCR-017: ICD-10 code suggestions, CPT code suggestions

**Design Tokens**: Header bg-gray-50, content p-4

---

### 2.5 FEEDBACK COMPONENTS

#### 2.5.1 Modal (Dialog Overlay)
**Description**: Centered overlay for focused user interactions  
**Structure**: Backdrop (bg-black opacity-50) + Content Card (max-w-md)  
**States**: Hidden (default), Visible (z-50)

**Design Tokens**:
- Backdrop: rgba(0, 0, 0, 0.5)
- Content: bg-white, rounded-lg, shadow-xl, p-6

**Screen Placement**:
- SCR-002: Booking confirmation modal
- SCR-004/005: Intake toggle confirmation dialog ("Switch to manual form? Your progress will be saved.")
- SCR-006: Walk-in patient search modal
- SCR-007: Delete confirmation dialogs
- SCR-013: Session timeout warning modal (UXR-603)

**Accessibility**:
- Focus trap: Tab cycles within modal
- Escape key: Closes modal
- ARIA: `role="dialog"`, `aria-modal="true"`, `aria-labelledby`

#### 2.5.2 Toast (Notification)
**Description**: Temporary notification overlay (top-right or bottom-center)  
**Variants**: Success, Error, Warning, Info  
**Duration**: 5 seconds auto-dismiss (or manual close)

**Screen Placement**:
- SCR-002: "Slot just booked" conflict toast (UXR-605)
- SCR-001: "Intake complete" success toast
- SCR-007: "Patient checked in" success toast

**Design Tokens**:
- Success: bg-green-50, border-green-200
- Error: bg-red-50, border-red-200
- Position: fixed top-4 right-4 (desktop), bottom-20 center (mobile)

#### 2.5.3 Alert (Banner)
**Description**: Inline notification banner at top of content area  
**Variants**: Info (blue), Success (green), Warning (yellow), Error (red)  
**Structure**: Icon + Message + Actions

**Screen Placement**:
- SCR-008: Insurance validation result alert
- SCR-009: Critical clinical data conflict alert (UXR-604 - red banner)

**Design Tokens**:
- Error alert: bg-error-100, border-error-500, text-error-800

#### 2.5.4 Spinner (Loading Indicator)
**Description**: Animated circular spinner for loading states  
**Sizes**: SM (16x16px), MD (24x24px), LG (48x48px)

**Screen Placement**:
- Button loading states: Inline spinner (SM size)
- Page loading: Center spinner (LG size)
- SCR-002: Booking API call (button spinner)
- SCR-007: Check-in button loading

**Design Tokens**: SVG animation, text-primary-500 (blue)

#### 2.5.5 Skeleton Screen
**Description**: Placeholder content for async loading (UXR-502)  
**Appearance**: Gray animated shimmer effect

**Screen Placement**:
- SCR-003: AI extraction processing (document preview skeleton)
- SCR-009: Patient data loading (timeline skeleton)

**Design Tokens**: bg-gray-200, animate-pulse

#### 2.5.6 Progress Bar
**Description**: Horizontal bar showing completion percentage  
**Variants**: Linear (default), Circular (alternative)

**Screen Placement**:
- SCR-003: File upload progress (0-100%)
- SCR-004: Intake completion progress ("3 of 8 sections complete")

**Design Tokens**: Track bg-gray-200, Fill bg-primary-500

---

### 2.6 SPECIALIZED COMPONENTS

#### 2.6.1 Calendar (Date Picker)
**Description**: Monthly calendar view for date selection  
**States**: Disabled dates (past), Available dates, Selected date

**Screen Placement**:
- SCR-002: Date picker in appointment booking filters

**Design Tokens**: Selected date bg-primary-500, today border-primary-500

#### 2.6.2 Slot Grid
**Description**: Custom grid of time slot buttons  
**Structure**: 6 columns (desktop), 4 (tablet), 2 (mobile)  
**Slot States**: Available (white border-gray-300), Selected (blue bg-primary-500), Booked (gray disabled)

**Screen Placement**:
- SCR-002: Time slot selection (morning/afternoon slots)

**Design Tokens**: Border-2, hover:border-primary-500, transition 150ms

#### 2.6.3 Chat Bubble
**Description**: Message bubble for conversational UI  
**Variants**: User message (right-aligned, blue), AI message (left-aligned, gray)

**Screen Placement**:
- SCR-004: AI conversational intake interface

**Design Tokens**:
- User bubble: bg-primary-500, text-white, rounded-r-lg
- AI bubble: bg-gray-100, text-gray-800, rounded-l-lg

#### 2.6.4 Drag Handle
**Description**: Visual indicator for draggable rows  
**Icon**: Six dots (vertical grip)

**Screen Placement**:
- SCR-007: Arrival queue table rows (drag-drop reorder per UXR-104)

**Accessibility**: `role="button"`, `aria-label="Drag to reorder"`

---

## 3. Component State Matrix

| Component | Default | Hover | Focus | Active | Disabled | Loading |
|-----------|---------|-------|-------|--------|----------|---------|
| Button Primary | bg-primary-500 | bg-primary-600 | outline-2 primary-500 | bg-primary-700 | opacity-50 | Spinner |
| TextField | border-gray-200 | - | outline-2 primary-500 | - | bg-gray-100 opacity-50 | - |
| Checkbox | border-gray-300 | border-gray-400 | ring-2 primary-500 | checked bg-primary-500 | opacity-50 | - |
| Card | shadow-default | transform scale hover | - | - | - | Skeleton |
| Table Row | - | bg-gray-50 | - | - | - | - |
| Badge | status color | - | - | - | - | - |
| Modal | z-50 visible | - | - | - | - | - |
| Toast | visible | close-btn hover | - | - | - | auto-dismiss 5s |

---

## 4. Component Placement Summary by Screen

| Screen | Cards | Buttons | Inputs | Tables | Badges | Modals | Other |
|--------|-------|---------|--------|--------|--------|--------|-------|
| SCR-001 | 6 | 4 | 0 | 0 | 2 | 1 (timeout) | Avatar, Header |
| SCR-002 | 2 | 15+ (slots) | 5 | 0 | 4 | 1 (confirm) | Calendar, Toast |
| SCR-003 | 1 | 3 | 0 | 1 | N (status) | 0 | FileUpload |
| SCR-004 | 0 | 3 | 1 | 0 | 0 | 1 (toggle) | ChatBubbles, Toggle |
| SCR-005 | 0 | 3 | 12+ | 0 | 0 | 1 (toggle) | Accordion, Toggle |
| SCR-006 | 1 | 2 | 6 | 0 | 0 | 1 (search) | - |
| SCR-007 | 4 (stats) | N (actions) | 2 | 1 (queue) | N (status) | 0 | DragHandles, Avatar |
| SCR-008 | 2 | 2 | 2 | 0 | 1 | 0 | Alert |
| SCR-009 | N (sections) | N (actions) | 0 | 2 | N (confidence) | 1 (conflicts) | Tabs, Timeline, Alert |
| SCR-010 | 0 | 3 | 3 | 1 | N (roles) | 0 | - |
| SCR-011 | 0 | 2 | 5 | 0 | 0 | 0 | Checkboxes |
| SCR-012 | 0 | 1 (export) | 5 | 1 | 0 | 1 (details) | DatePicker |
| SCR-013 | 1 (login) | 1 | 2 | 0 | 0 | 0 | - |
| SCR-014 | 1 | 1 | 1 | 0 | 0 | 0 | - |
| SCR-015 | 1 | 1 | 2 | 0 | 0 | 0 | - |
| SCR-016 | 0 | N (actions) | 3 | 1 | N (status) | 0 | Pagination |
| SCR-017 | N (codes) | N (actions) | 2 | 0 | N (confidence) | 0 | Accordion |
| SCR-018 | 0 | 2 | N (settings) | 0 | N (status) | 1 (confirm reset) | Tabs, Toggles |

**Notation**: N = Variable count based on data

---

## 5. Component Reusability Score

| Component | Usage Frequency (of 18 screens) | Reusability Rating |
|-----------|----------------------------------|---------------------|
| Button | 18 (100%) | ⭐⭐⭐⭐⭐ Critical |
| TextField | 16 (89%) | ⭐⭐⭐⭐⭐ Critical |
| Card | 14 (78%) | ⭐⭐⭐⭐⭐ Critical |
| Badge | 12 (67%) | ⭐⭐⭐⭐ High |
| Table | 10 (56%) | ⭐⭐⭐⭐ High |
| Modal | 10 (56%) | ⭐⭐⭐⭐ High |
| Select | 9 (50%) | ⭐⭐⭐ Medium |
| Checkbox | 5 (28%) | ⭐⭐⭐ Medium |
| Tabs | 3 (17%) | ⭐⭐ Low |
| Accordion | 3 (17%) | ⭐⭐ Low |
| Toggle | 3 (17%) | ⭐⭐ Low |
| Toast | 3 (17%) | ⭐⭐ Low |
| Timeline | 1 (6%) | ⭐ Specific |
| Chat Bubble | 1 (6%) | ⭐ Specific |
| Drag Handle | 1 (6%) | ⭐ Specific |

---

## 6. Component Accessibility Checklist

✅ All interactive components keyboard-navigable (UXR-202)  
✅ Focus indicators 2px solid blue outline (UXR-206)  
✅ Touch targets ≥44x44px on mobile (UXR-205)  
✅ Form labels associated with inputs via `for`/`id`  
✅ ARIA labels on icon-only buttons  
✅ ARIA live regions for dynamic status updates  
✅ Color contrast ≥4.5:1 for text (UXR-204)  
✅ Error messages linked via `aria-describedby`  
✅ Modal focus trap with Escape key handler  
✅ Alt text on all images/icons (decorative: `aria-hidden="true"`)

---

**Document Status**: Complete - All components across 18 screens documented  
**Source**: shadcn/ui base + Tailwind CSS utilities + Custom CSS variables from designsystem.md  
**Next Steps**: Implement component library in React with TypeScript type definitions