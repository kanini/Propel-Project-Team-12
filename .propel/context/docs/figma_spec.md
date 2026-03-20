# Figma Design Specification - Patient Access & Clinical Intelligence Platform

## 1. Figma Specification

**Platform**: Web (Responsive — Desktop, Tablet, Mobile)

---

## 2. Source References

### Primary Source

| Document | Path | Purpose |
|----------|------|---------|
| Requirements | `.propel/context/docs/spec.md` | Personas, use cases (UC-001 to UC-016), FR-001 to FR-043 |

### Optional Sources

| Document | Path | Purpose |
|----------|------|---------|
| Architecture Design | `.propel/context/docs/design.md` | NFR, TR, DR, AI requirements, technology stack |

### Related Documents

| Document | Path | Purpose |
|----------|------|---------|
| Design System | `.propel/context/docs/designsystem.md` | Tokens, branding, component specifications |

---

## 3. UX Requirements

### UXR Requirements Table

| UXR-ID | Category | Requirement | Acceptance Criteria | Screens Affected |
|--------|----------|-------------|---------------------|------------------|
| UXR-001 | Usability | System MUST provide navigation to any feature in max 3 clicks from the portal dashboard | Click-count audit passes for all primary tasks | All screens |
| UXR-002 | Usability | System MUST display clear visual hierarchy distinguishing primary actions from secondary actions on every screen | Heuristic review confirms F-pattern/Z-pattern adherence | All screens |
| UXR-003 | Usability | System MUST provide persistent role-based navigation allowing users to access all portal sections without returning to dashboard | Navigation audit confirms all sections reachable from nav | SCR-003, SCR-004, SCR-005 |
| UXR-004 | Usability | System MUST provide inline search with real-time filtering for provider browsing and patient search | Search returns results within 300ms of keystroke debounce | SCR-006, SCR-018, SCR-021 |
| UXR-101 | Usability | System MUST clearly indicate current step within multi-step flows (booking, intake, registration) using progress indicators | Progress indicator visible and accurate at each step | SCR-001, SCR-007, SCR-012, SCR-013 |
| UXR-102 | Usability | System MUST preserve user-entered data when switching between AI and manual intake modes | Data persists after mode switch verified in both directions | SCR-012, SCR-013 |
| UXR-103 | Usability | System MUST provide contextual help tooltips for clinical terminology and complex form fields | Tooltips appear on hover/focus for marked fields | SCR-012, SCR-013, SCR-016, SCR-017 |
| UXR-201 | Accessibility | System MUST comply with WCAG 2.2 Level AA standards across all screens | WAVE/axe audit passes with zero critical violations | All screens |
| UXR-202 | Accessibility | System MUST provide visible focus indicators with minimum 3:1 contrast ratio on all interactive elements | Focus outline visible on keyboard navigation | All screens |
| UXR-203 | Accessibility | System MUST ensure all form inputs have programmatically associated labels and error descriptions | Screen reader audit passes for all forms | SCR-001, SCR-002, SCR-007, SCR-009, SCR-013, SCR-018, SCR-022 |
| UXR-204 | Accessibility | System MUST provide meaningful alt text for all informative images and decorative elements marked as hidden | Image audit confirms alt text coverage | All screens |
| UXR-205 | Accessibility | System MUST support keyboard-only navigation for all interactive workflows without focus traps | Complete flows navigable via keyboard alone | All screens |
| UXR-206 | Accessibility | System MUST use semantic HTML landmarks (nav, main, aside, header, footer) for all page regions | Landmark audit confirms proper usage | All screens |
| UXR-207 | Accessibility | System MUST announce dynamic content changes (toasts, status updates, live data) via ARIA live regions | Screen reader announces updates within 1 second | SCR-012, SCR-014, SCR-015, SCR-019 |
| UXR-301 | Responsiveness | System MUST adapt layout to mobile (320px–767px), tablet (768px–1023px), and desktop (1024px+) breakpoints | Responsive audit passes at all breakpoints | All screens |
| UXR-302 | Responsiveness | System MUST convert sidebar navigation to bottom navigation on mobile viewports | Navigation shifts correctly at 768px breakpoint | All screens |
| UXR-303 | Responsiveness | System MUST stack multi-column layouts into single column on mobile viewports while maintaining content hierarchy | Layout audit passes on 320px viewport | SCR-003, SCR-016, SCR-017, SCR-019 |
| UXR-304 | Responsiveness | System MUST ensure minimum touch target size of 44x44px on mobile viewports | Touch target audit passes | All screens (mobile) |
| UXR-401 | Visual Design | System MUST apply design system tokens consistently — no hard-coded color, spacing, or typography values | Token audit shows 100% adherence | All screens |
| UXR-402 | Visual Design | System MUST visually distinguish AI-suggested data (amber badge) from staff-verified data (green badge) | Badge differentiation verified by visual audit | SCR-016, SCR-017, SCR-023 |
| UXR-403 | Visual Design | System MUST use consistent iconography style (outlined, 24px default) across all screens | Icon audit confirms style consistency | All screens |
| UXR-501 | Interaction | System MUST provide visual feedback for all user actions within 200ms (button press, form submit, navigation) | Interaction latency audit passes | All screens |
| UXR-502 | Interaction | System MUST display skeleton loading states for data-fetching screens when load exceeds 300ms | Skeleton appears before content loads | SCR-003, SCR-004, SCR-005, SCR-006, SCR-010, SCR-016, SCR-017, SCR-019, SCR-021 |
| UXR-503 | Interaction | System MUST provide real-time upload progress indication during clinical document upload | Progress bar updates continuously during upload | SCR-014 |
| UXR-504 | Interaction | System MUST provide animated transitions between flow steps (150–300ms ease-out) respecting prefers-reduced-motion | Motion audit passes; reduced motion honored | All screens |
| UXR-601 | Error Handling | System MUST display inline field-level validation errors below the corresponding form field | Error placement audit passes | SCR-001, SCR-002, SCR-007, SCR-009, SCR-013, SCR-018, SCR-022 |
| UXR-602 | Error Handling | System MUST provide actionable error messages with clear recovery instructions (never generic "Something went wrong") | Error message audit confirms actionable text | All screens |
| UXR-603 | Error Handling | System MUST display a global error banner for API/system failures with retry action | Error banner visible on API failure | All authenticated screens |
| UXR-604 | Error Handling | System MUST display session timeout warning modal 2 minutes before expiry with extend option | Modal appears at 13-minute mark | All authenticated screens |
| UXR-605 | Error Handling | System MUST provide empty state illustrations with guiding CTA for screens with no data | Empty state audit confirms CTA presence | SCR-010, SCR-015, SCR-016, SCR-019, SCR-021, SCR-025 |

---

## 4. Personas Summary

| Persona | Role | Primary Goals | Key Screens |
|---------|------|---------------|-------------|
| Patient | Primary end-user / healthcare consumer | Self-service registration, appointment booking, intake completion, document upload, health dashboard access | SCR-001, SCR-002, SCR-003, SCR-006–SCR-016 |
| Staff | Front Desk / Call Center employee | Walk-in management, queue visibility, arrival marking, clinical data verification, conflict resolution | SCR-002, SCR-004, SCR-017–SCR-020, SCR-023, SCR-024 |
| Admin | System Administrator | User management, role assignment, audit log access, system configuration | SCR-002, SCR-005, SCR-021, SCR-022, SCR-025, SCR-026 |

---

## 5. Information Architecture

### Site Map

```text
Patient Access & Clinical Intelligence Platform
+-- Public
|   +-- SCR-001: Registration
|   +-- SCR-002: Login
|
+-- Patient Portal (Role: Patient)
|   +-- SCR-003: Patient Dashboard
|   +-- Appointments
|   |   +-- SCR-006: Provider/Service Browser
|   |   +-- SCR-007: Appointment Booking
|   |   +-- SCR-008: Appointment Confirmation
|   |   +-- SCR-009: Waitlist Enrollment
|   |   +-- SCR-010: My Appointments
|   |   +-- SCR-011: Reschedule Appointment
|   +-- Intake
|   |   +-- SCR-012: AI Conversational Intake
|   |   +-- SCR-013: Manual Intake Form
|   +-- Clinical
|   |   +-- SCR-014: Document Upload
|   |   +-- SCR-015: Document Status Tracker
|   |   +-- SCR-016: Patient Health Dashboard (360° View)
|
+-- Staff Portal (Role: Staff)
|   +-- SCR-004: Staff Dashboard
|   +-- SCR-017: Staff Patient View (360° + Verification)
|   +-- SCR-018: Walk-in Booking
|   +-- SCR-019: Queue Management
|   +-- SCR-020: Arrival Management
|   +-- SCR-023: Clinical Data Verification
|   +-- SCR-024: Conflict Resolution
|
+-- Admin Portal (Role: Admin)
    +-- SCR-005: Admin Dashboard
    +-- SCR-021: User Management
    +-- SCR-022: User Create/Edit Form
    +-- SCR-025: Audit Log Viewer
    +-- SCR-026: System Settings
```

### Navigation Patterns

| Pattern | Type | Platform Behavior |
|---------|------|-------------------|
| Primary Nav | Sidebar | Desktop: Collapsible sidebar (240px expanded / 64px collapsed). Mobile: Bottom navigation bar with 5 max items |
| Secondary Nav | Tabs | Horizontal tabs within content area (e.g., Appointments tabs: Upcoming / Past / Waitlist) |
| Breadcrumb | Breadcrumb | Desktop: Full path breadcrumb below header. Mobile: Back arrow with parent label |
| Utility Nav | User Menu | Top-right avatar dropdown: Profile, Settings, Logout |

---

## 6. Screen Inventory

### Screen List

| Screen ID | Screen Name | Derived From | UXR-XXX Mapped | Personas Covered | Priority | States Required |
|-----------|-------------|--------------|----------------|------------------|----------|-----------------|
| SCR-001 | Registration | UC-001 | UXR-001, UXR-101, UXR-201, UXR-203, UXR-301, UXR-601 | Patient | P0 | Default, Loading, Error, Validation |
| SCR-002 | Login | UC-002 | UXR-001, UXR-201, UXR-203, UXR-301, UXR-601 | All | P0 | Default, Loading, Error, Validation |
| SCR-003 | Patient Dashboard | UC-002, UC-003, UC-010 | UXR-001, UXR-002, UXR-003, UXR-301, UXR-303, UXR-502 | Patient | P0 | Default, Loading, Empty, Error |
| SCR-004 | Staff Dashboard | UC-002, UC-011, UC-012 | UXR-001, UXR-002, UXR-003, UXR-301, UXR-303, UXR-502 | Staff | P0 | Default, Loading, Empty, Error |
| SCR-005 | Admin Dashboard | UC-002, UC-014 | UXR-001, UXR-002, UXR-003, UXR-301, UXR-502 | Admin | P0 | Default, Loading, Empty, Error |
| SCR-006 | Provider/Service Browser | UC-003 | UXR-004, UXR-301, UXR-401, UXR-502 | Patient | P0 | Default, Loading, Empty, Error |
| SCR-007 | Appointment Booking | UC-003, UC-005 | UXR-101, UXR-201, UXR-203, UXR-301, UXR-501, UXR-601 | Patient | P0 | Default, Loading, Error, Validation |
| SCR-008 | Appointment Confirmation | UC-003 | UXR-001, UXR-301, UXR-501 | Patient | P0 | Default, Loading, Error |
| SCR-009 | Waitlist Enrollment | UC-004 | UXR-201, UXR-203, UXR-301, UXR-601 | Patient | P1 | Default, Loading, Error, Validation |
| SCR-010 | My Appointments | UC-003, UC-004, UC-006 | UXR-001, UXR-301, UXR-502, UXR-605 | Patient | P0 | Default, Loading, Empty, Error |
| SCR-011 | Reschedule Appointment | UC-006 | UXR-201, UXR-301, UXR-501, UXR-601 | Patient | P1 | Default, Loading, Error, Validation |
| SCR-012 | AI Conversational Intake | UC-007 | UXR-101, UXR-102, UXR-103, UXR-201, UXR-207, UXR-301, UXR-501, UXR-504 | Patient | P0 | Default, Loading, Error |
| SCR-013 | Manual Intake Form | UC-008 | UXR-101, UXR-102, UXR-103, UXR-201, UXR-203, UXR-301, UXR-601 | Patient | P0 | Default, Loading, Error, Validation |
| SCR-014 | Clinical Document Upload | UC-009 | UXR-201, UXR-207, UXR-301, UXR-503, UXR-601 | Patient | P1 | Default, Loading, Error, Validation |
| SCR-015 | Document Status Tracker | UC-009 | UXR-301, UXR-502, UXR-605 | Patient | P1 | Default, Loading, Empty, Error |
| SCR-016 | Patient Health Dashboard (360°) | UC-010 | UXR-001, UXR-103, UXR-201, UXR-301, UXR-303, UXR-402, UXR-502, UXR-605 | Patient | P0 | Default, Loading, Empty, Error |
| SCR-017 | Staff Patient View (360° + Verify) | UC-010, UC-015 | UXR-001, UXR-103, UXR-201, UXR-301, UXR-303, UXR-402, UXR-502 | Staff | P0 | Default, Loading, Empty, Error |
| SCR-018 | Walk-in Booking | UC-011 | UXR-004, UXR-201, UXR-203, UXR-301, UXR-601 | Staff | P0 | Default, Loading, Error, Validation |
| SCR-019 | Queue Management | UC-012 | UXR-201, UXR-207, UXR-301, UXR-303, UXR-502, UXR-605 | Staff | P0 | Default, Loading, Empty, Error |
| SCR-020 | Arrival Management | UC-013 | UXR-004, UXR-201, UXR-301, UXR-501 | Staff | P1 | Default, Loading, Empty, Error |
| SCR-021 | User Management | UC-014 | UXR-004, UXR-201, UXR-301, UXR-502, UXR-605 | Admin | P0 | Default, Loading, Empty, Error |
| SCR-022 | User Create/Edit Form | UC-014 | UXR-201, UXR-203, UXR-301, UXR-601 | Admin | P0 | Default, Loading, Error, Validation |
| SCR-023 | Clinical Data Verification | UC-015 | UXR-103, UXR-201, UXR-301, UXR-402, UXR-501 | Staff | P0 | Default, Loading, Empty, Error |
| SCR-024 | Conflict Resolution | UC-016 | UXR-201, UXR-301, UXR-402, UXR-501, UXR-602 | Staff | P1 | Default, Loading, Empty, Error |
| SCR-025 | Audit Log Viewer | FR-005, FR-040 | UXR-201, UXR-301, UXR-502, UXR-605 | Admin | P1 | Default, Loading, Empty, Error |
| SCR-026 | System Settings | FR-022 | UXR-201, UXR-301, UXR-601 | Admin | P2 | Default, Loading, Error, Validation |

### Priority Legend

- **P0**: Critical path — must-have for MVP (16 screens)
- **P1**: Core functionality — high priority (7 screens)
- **P2**: Important features — medium priority (3 screens)

### Screen-to-Persona Coverage Matrix

| Screen | Patient | Staff | Admin | Notes |
|--------|---------|-------|-------|-------|
| SCR-001 Registration | Primary | - | - | Public page |
| SCR-002 Login | Primary | Primary | Primary | Shared entry point |
| SCR-003 Patient Dashboard | Primary | - | - | Post-login hub |
| SCR-004 Staff Dashboard | - | Primary | - | Post-login hub |
| SCR-005 Admin Dashboard | - | - | Primary | Post-login hub |
| SCR-006 Provider Browser | Primary | - | - | Appointment flow entry |
| SCR-007 Appointment Booking | Primary | - | - | Core booking |
| SCR-008 Confirmation | Primary | - | - | Booking success |
| SCR-009 Waitlist | Primary | - | - | Alternative booking |
| SCR-010 My Appointments | Primary | - | - | Appointment management |
| SCR-011 Reschedule | Primary | - | - | Appointment modification |
| SCR-012 AI Intake | Primary | - | - | Pre-visit AI |
| SCR-013 Manual Intake | Primary | - | - | Pre-visit form |
| SCR-014 Document Upload | Primary | - | - | Clinical docs |
| SCR-015 Document Status | Primary | - | - | Processing tracker |
| SCR-016 Patient Health Dashboard | Primary | - | - | Read-only 360° |
| SCR-017 Staff Patient View | - | Primary | - | 360° with verify |
| SCR-018 Walk-in Booking | - | Primary | - | Walk-in flow |
| SCR-019 Queue Management | - | Primary | - | Real-time queue |
| SCR-020 Arrival Management | - | Primary | - | Check-in flow |
| SCR-021 User Management | - | - | Primary | CRUD users |
| SCR-022 User Create/Edit | - | - | Primary | User form |
| SCR-023 Clinical Verification | - | Primary | - | Trust-first workflow |
| SCR-024 Conflict Resolution | - | Primary | - | Data conflicts |
| SCR-025 Audit Log | - | - | Primary | Compliance |
| SCR-026 System Settings | - | - | Primary | Configuration |

### Modal/Overlay Inventory

| Name | Type | Trigger | Parent Screen(s) | Priority |
|------|------|---------|-----------------|----------|
| Session Timeout Warning | Modal | 13 minutes inactivity | All authenticated screens | P0 |
| Appointment Cancel Confirm | Dialog | Click "Cancel Appointment" | SCR-010 | P0 |
| Slot Swap Notification | Modal | System detects preferred slot | SCR-003, SCR-010 | P0 |
| Waitlist Slot Available | Modal | Preferred slot opens | SCR-003, SCR-010 | P1 |
| Booking Confirmation PDF | Drawer | Click "View Confirmation" | SCR-008, SCR-010 | P1 |
| Delete User Confirm | Dialog | Click "Deactivate User" | SCR-021 | P0 |
| Calendar Integration | Modal | Click "Add to Calendar" | SCR-008 | P1 |
| Document Upload Error | Dialog | Upload fails | SCR-014 | P0 |
| Conflict Detail | Drawer | Click conflict item | SCR-024 | P1 |
| Insurance Validation Result | Modal | Submit insurance pre-check | SCR-013 | P1 |

---

## 7. Content & Tone

### Voice & Tone

- **Overall Tone**: Professional, warm, and reassuring — healthcare context demands trust and clarity
- **Error Messages**: Helpful, non-blaming, actionable — "We couldn't verify your credentials. Please check your email and password, or reset your password."
- **Empty States**: Encouraging with clear CTA — "No appointments yet. Browse providers to schedule your first visit."
- **Success Messages**: Brief, confirming, next-action oriented — "Appointment booked! Check your email for confirmation."
- **AI Interactions**: Conversational, empathetic, and transparent about AI nature — "I'd like to understand your health history. Could you tell me about any current medications?"

### Content Guidelines

- **Headings**: Sentence case throughout (e.g., "Book an appointment")
- **CTAs**: Action-oriented, specific verbs (e.g., "Book appointment", "Upload document", "Verify data")
- **Labels**: Concise, descriptive, no jargon for patient-facing screens; clinical terminology acceptable for staff screens with tooltips
- **Placeholder Text**: Helpful examples (e.g., "john.doe@email.com", "e.g., Annual checkup")
- **Clinical Data**: Always pair technical terms with plain-language descriptions for patients

---

## 8. Data & Edge Cases

### Data Scenarios

| Scenario | Description | Handling |
|----------|-------------|----------|
| No Data | New patient with no appointments, documents, or clinical data | Empty state with illustration and guiding CTA |
| First Use | New user post-registration | Onboarding prompts on Patient Dashboard (book appointment, upload docs) |
| Large Data | Patient with 100+ clinical documents or 50+ appointments | Pagination (20 items/page) with scroll-to-top |
| Slow Connection | API response > 3 seconds | Skeleton loading states replacing content areas |
| Offline | No network connectivity | Offline banner at top; cached data where available |
| Concurrent Booking | Two patients book same slot simultaneously | "Slot no longer available" error with refreshed availability |
| AI Unavailable | AI services down (AIR-O02 circuit breaker) | Graceful fallback to manual form with notice banner |

### Edge Cases

| Case | Screen(s) Affected | Solution |
|------|-------------------|----------|
| Long patient name | All screens with patient names | Truncation with full name tooltip (max 40 chars visible) |
| Multiple conflicting medications | SCR-017, SCR-024 | Scrollable conflict list with severity badges |
| Low confidence AI extraction (<70%) | SCR-012 | Auto-suggest switch to manual form (FR-019, AIR-S03) |
| 50+ waitlist entries | SCR-010 | Paginated waitlist section with position indicator |
| Document processing failure | SCR-015 | Failed status badge with "Retry" and "Manual entry" options |
| Session timeout during form fill | SCR-007, SCR-013, SCR-022 | Warning modal at 13 min; auto-save draft before timeout |
| Insurance pre-check failure | SCR-013 | Inline error with "Insurance not found" and manual entry option |
| No providers available | SCR-006 | Empty state: "No providers available for this service. Try adjusting your filters." |

---

## 9. Branding & Visual Direction

*See `designsystem.md` for all design tokens (colors, typography, spacing, shadows, etc.)*

### Branding Assets

- **Logo**: Platform wordmark "PatientAccess" with healthcare cross icon — primary blue (#0F62FE) on white, inverse white on dark
- **Icon Style**: Outlined, 24px default size, 1.5px stroke weight — consistent with Lucide or Heroicons Outline
- **Illustration Style**: Flat, minimal healthcare-themed illustrations — used for empty states and onboarding
- **Photography Style**: Not applicable for Phase 1 — illustration-first approach

---

## 10. Component Specifications

*Component specifications defined in designsystem.md. Requirements per screen listed below.*

### Component Library Reference

**Source**: `.propel/context/docs/designsystem.md` (Component Specifications section)

### Required Components per Screen

| Screen ID | Components Required | Notes |
|-----------|---------------------|-------|
| SCR-001 | TextField (5), Button (2), Link (1), PasswordStrength (1), Alert (1) | Name, DOB, email, phone, password fields |
| SCR-002 | TextField (2), Button (2), Link (2), Alert (1) | Email, password + forgot password, register links |
| SCR-003 | Card (4), Badge (N), StatCard (3), Button (2), CalendarWidget (1) | Quick-stats, upcoming appointments, notifications |
| SCR-004 | Card (3), Table (1), StatCard (3), Badge (N) | Queue summary, today's appointments, alerts |
| SCR-005 | Card (3), StatCard (4), Table (1) | System stats, recent audit events |
| SCR-006 | SearchBar (1), FilterPanel (1), Card (N), Pagination (1), Badge (N) | Provider cards with specialty, rating, availability |
| SCR-007 | Calendar (1), TimeSlotGrid (1), TextField (1), Select (1), Checkbox (1), Button (2) | Date picker, time slots, reason, swap toggle |
| SCR-008 | Card (1), Button (3), Badge (1), Link (1) | Confirmation details, PDF download, calendar add |
| SCR-009 | TextField (2), Select (2), Radio (1), Button (2), Alert (1) | Date range, notification pref, confirmation |
| SCR-010 | Table (1), Badge (N), Button (N), Tabs (3), EmptyState (1) | Appointment list with Upcoming/Past/Waitlist tabs |
| SCR-011 | Calendar (1), TimeSlotGrid (1), Button (2), Alert (1) | Available alternative slots |
| SCR-012 | ChatBubble (N), TextField (1), Button (2), Toggle (1), ProgressBar (1) | AI chat interface with mode switch |
| SCR-013 | TextField (8), Select (4), Checkbox (N), Radio (N), Toggle (1), Button (3), ProgressBar (1) | Multi-section structured intake form |
| SCR-014 | FileUpload (1), ProgressBar (N), Button (2), Alert (1) | Drag-and-drop area with progress |
| SCR-015 | Table (1), Badge (N), EmptyState (1) | Document list with processing status |
| SCR-016 | Card (6), Accordion (4), Badge (N), LineChart (1), Table (2) | Demographics, conditions, meds, allergies, vitals, encounters |
| SCR-017 | Card (6), Accordion (4), Badge (N), LineChart (1), Table (2), Button (3) | Same as SCR-016 + verify/reject/correct actions |
| SCR-018 | SearchBar (1), TextField (5), Select (2), Calendar (1), TimeSlotGrid (1), Button (3) | Patient search/create + booking |
| SCR-019 | Table (1), Badge (N), Button (N), Timer (N), EmptyState (1) | Chronological queue with wait times |
| SCR-020 | SearchBar (1), Table (1), Button (N), Badge (N) | Today's appointments with "Mark Arrived" |
| SCR-021 | SearchBar (1), Table (1), Button (3), Badge (N), Pagination (1), EmptyState (1) | User list with create/edit/deactivate |
| SCR-022 | TextField (5), Select (2), Toggle (1), Button (3), Alert (1) | User form with role assignment |
| SCR-023 | DataGrid (1), Badge (N), Button (3), Tooltip (N), SidePanel (1) | Data points with source reference panel |
| SCR-024 | SideBySideCompare (1), Badge (N), Button (3), Tooltip (N), Select (1) | Conflicting data comparison |
| SCR-025 | Table (1), SearchBar (1), FilterPanel (1), Pagination (1), EmptyState (1) | Audit log entries with filtering |
| SCR-026 | TextField (4), Select (2), Toggle (3), Button (2) | Reminder intervals, cancellation policy |

### Component Summary

| Category | Components | Variants |
|----------|------------|----------|
| Actions | Button, IconButton, Link, FAB | Primary / Secondary / Tertiary / Ghost × S / M / L × Default / Hover / Focus / Active / Disabled / Loading |
| Inputs | TextField, TextArea, Select, Checkbox, Radio, Toggle, FileUpload, SearchBar | Default / Focus / Error / Disabled / Filled × S / M / L |
| Navigation | Header, Sidebar, BottomNav, Tabs, Breadcrumb, Pagination | Desktop / Mobile × Expanded / Collapsed |
| Content | Card, StatCard, ListItem, Table, DataGrid, Badge, Avatar, Accordion, ChatBubble, SideBySideCompare | Content variants per data type |
| Feedback | Modal, Dialog, Drawer, SidePanel, Toast, Alert, Tooltip, Skeleton, Spinner, ProgressBar, EmptyState | Info / Success / Warning / Error / Loading |
| Data Visualization | LineChart, Calendar, TimeSlotGrid, Timer, PasswordStrength | Interactive / Static × Light / Dark |

### Component Constraints

- Use only components from designsystem.md
- No custom components without design review approval
- All components MUST support all defined states (Default, Hover, Focus, Active, Disabled, Loading)
- Follow naming convention: `C/<Category>/<Name>`

---

## 11. Prototype Flows

### Flow: FL-001 — Patient Registration & Onboarding

**Flow ID**: FL-001
**Derived From**: UC-001
**Personas Covered**: Patient
**Description**: New patient creates account, verifies email, and lands on onboarded dashboard

#### Flow Sequence

```text
1. Entry: SCR-001 / Default
   - Trigger: Patient clicks "Register" from SCR-002
   |
   v
2. Step: SCR-001 / Validation
   - Action: Patient fills form; system validates inline
   |
   v
3. Step: SCR-001 / Loading
   - Action: Patient submits registration form
   |
   v
4. Decision Point:
   +-- Success -> Email verification sent (toast confirmation)
   +-- Error -> SCR-001 / Error (duplicate email, server error)
   |
   v
5. Exit: SCR-002 / Default
   - Trigger: Patient clicks email verification link, redirected to login
```

#### Required Interactions

- Inline password strength indicator during typing
- Email format validation on blur
- "Email already registered" error links to login

---

### Flow: FL-002 — Authentication

**Flow ID**: FL-002
**Derived From**: UC-002
**Personas Covered**: Patient, Staff, Admin
**Description**: User authenticates and is routed to role-appropriate dashboard

#### Flow Sequence

```text
1. Entry: SCR-002 / Default
   - Trigger: User navigates to platform or session expired
   |
   v
2. Step: SCR-002 / Loading
   - Action: User submits credentials
   |
   v
3. Decision Point:
   +-- Patient -> SCR-003 / Default (Patient Dashboard)
   +-- Staff -> SCR-004 / Default (Staff Dashboard)
   +-- Admin -> SCR-005 / Default (Admin Dashboard)
   +-- Invalid credentials -> SCR-002 / Error (generic error message)
   +-- Account locked -> SCR-002 / Error (lockout with contact info)
```

#### Required Interactions

- Generic error message (no credential-specific hints)
- Account lockout after 5 failed attempts

---

### Flow: FL-003 — Appointment Booking

**Flow ID**: FL-003
**Derived From**: UC-003, UC-004
**Personas Covered**: Patient
**Description**: Patient searches providers, selects time slot, books appointment with optional waitlist

#### Flow Sequence

```text
1. Entry: SCR-003 / Default
   - Trigger: Patient clicks "Book Appointment" from dashboard
   |
   v
2. Step: SCR-006 / Default
   - Action: Patient searches/filters providers
   |
   v
3. Step: SCR-006 / Loading
   - Action: Provider results loading
   |
   v
4. Step: SCR-007 / Default
   - Action: Patient selects provider, views calendar, picks time slot
   |
   v
5. Decision Point:
   +-- Slot available -> SCR-007 / Loading (booking in progress)
   |   +-- Success -> SCR-008 / Default (confirmation with PDF)
   |   +-- Concurrent conflict -> SCR-007 / Error (slot taken, refresh)
   +-- No slots -> SCR-009 / Default (waitlist enrollment)
   |   +-- Waitlist confirmed -> SCR-010 / Default (shows waitlist tab)
   |
   v
6. Exit: SCR-008 / Default or SCR-010 / Default
```

#### Required Interactions

- Calendar real-time availability update
- Preferred slot swap checkbox toggle
- "Add to Calendar" button (Google / Outlook)
- PDF download button

---

### Flow: FL-004 — Appointment Management

**Flow ID**: FL-004
**Derived From**: UC-006
**Personas Covered**: Patient
**Description**: Patient cancels or reschedules an existing appointment

#### Flow Sequence

```text
1. Entry: SCR-010 / Default
   - Trigger: Patient navigates to "My Appointments"
   |
   v
2. Decision Point:
   +-- Cancel -> Modal: Cancel Confirmation
   |   +-- Confirm -> SCR-010 / Default (appointment removed, toast shown)
   |   +-- Dismiss -> SCR-010 / Default (no change)
   +-- Reschedule -> SCR-011 / Default (alternative slots)
       +-- Select new slot -> SCR-011 / Loading
       +-- Success -> SCR-010 / Default (updated appointment, toast)
       +-- Error -> SCR-011 / Error (slot unavailable)
```

#### Required Interactions

- Cancellation policy notice in modal
- Slot availability refresh on reschedule

---

### Flow: FL-005 — Patient Intake (Dual Mode)

**Flow ID**: FL-005
**Derived From**: UC-007, UC-008
**Personas Covered**: Patient
**Description**: Patient completes pre-visit intake via AI conversation or manual form with seamless switching

#### Flow Sequence

```text
1. Entry: SCR-010 / Default
   - Trigger: Patient clicks "Complete Intake" on upcoming appointment
   |
   v
2. Decision Point: Mode Selection
   +-- AI Mode -> SCR-012 / Default
   |   - Action: AI guides conversation
   |   - Switch to Manual -> SCR-013 / Default (data preserved)
   +-- Manual Mode -> SCR-013 / Default
       - Action: Patient fills structured form
       - Switch to AI -> SCR-012 / Default (data preserved)
   |
   v
3. Step: Form/Chat completion
   - Action: Patient reviews summary
   |
   v
4. Step: Insurance pre-check (SCR-013 section or SCR-012 final step)
   - Decision:
     +-- Valid insurance -> Continue
     +-- Invalid -> Modal: Insurance not found, manual entry option
   |
   v
5. Exit: SCR-010 / Default (intake status: Complete, toast confirmation)
```

#### Required Interactions

- Mode toggle preserves all entered data
- AI typing indicator during response generation
- Progressive form sections with completion indicator
- Insurance pre-check inline validation

---

### Flow: FL-006 — Clinical Document Upload & Processing

**Flow ID**: FL-006
**Derived From**: UC-009
**Personas Covered**: Patient
**Description**: Patient uploads clinical PDFs, monitors processing, views extracted data

#### Flow Sequence

```text
1. Entry: SCR-003 / Default
   - Trigger: Patient clicks "Upload Documents" from dashboard
   |
   v
2. Step: SCR-014 / Default
   - Action: Patient drags/selects PDF files
   |
   v
3. Step: SCR-014 / Loading
   - Action: Upload progress bars for each file
   |
   v
4. Decision Point:
   +-- Upload success -> SCR-015 / Default (status: Processing)
   +-- Invalid file -> SCR-014 / Error (format/size error)
   +-- Upload failure -> Dialog: "Upload failed. Retry?"
   |
   v
5. Step: SCR-015 / Default
   - Action: Patient monitors processing status (Uploaded → Processing → Completed/Failed)
   |
   v
6. Exit: SCR-016 / Default (updated 360° view with extracted data)
```

#### Required Interactions

- Drag-and-drop file upload area
- Real-time progress bar per file (Pusher)
- Status badge auto-refresh on SCR-015

---

### Flow: FL-007 — Patient Health Dashboard

**Flow ID**: FL-007
**Derived From**: UC-010
**Personas Covered**: Patient
**Description**: Patient views read-only 360-Degree Patient View

#### Flow Sequence

```text
1. Entry: SCR-003 / Default
   - Trigger: Patient clicks "Health Dashboard" in navigation
   |
   v
2. Decision Point:
   +-- Has clinical data -> SCR-016 / Default (full 360° view)
   +-- No clinical data -> SCR-016 / Empty (upload prompt)
   |
   v
3. Step: SCR-016 / Default
   - Action: Patient reviews demographics, conditions, medications, allergies, vitals, encounters
   |
   v
4. Exit: SCR-016 / Default (read-only, no actions except navigation)
```

#### Required Interactions

- Accordion expand/collapse for data sections
- Vital trend line chart with hover tooltips
- Badge distinction: AI-suggested (amber) vs. verified (green)

---

### Flow: FL-008 — Staff Walk-in & Queue Management

**Flow ID**: FL-008
**Derived From**: UC-011, UC-012, UC-013
**Personas Covered**: Staff
**Description**: Staff books walk-in patient, manages queue, and marks arrivals

#### Flow Sequence

```text
1. Entry: SCR-004 / Default
   - Trigger: Staff selects "Walk-in Booking" or "Queue"
   |
   v
2. Step: SCR-018 / Default
   - Action: Staff searches existing patient or creates new
   |
   v
3. Decision Point:
   +-- Existing patient -> Auto-fill details
   +-- New patient -> Inline registration form (minimal)
   |
   v
4. Step: SCR-018 / Loading
   - Action: Staff selects provider and time, submits booking
   +-- Success -> Toast confirmation, redirect to SCR-019
   +-- Error -> SCR-018 / Error (no availability)
   |
   v
5. Step: SCR-019 / Default
   - Action: Staff views chronological queue with estimated wait times
   |
   v
6. Step: SCR-020 / Default
   - Action: Staff searches and marks patient as "Arrived"
   +-- Success -> Badge updates to "Arrived", added to queue
   |
   v
7. Exit: SCR-019 / Default (queue reflects updated status)
```

#### Required Interactions

- Patient search with autocomplete
- Queue auto-refresh (real-time via Pusher)
- "Mark Arrived" one-click action with confirmation toast

---

### Flow: FL-009 — Clinical Data Verification

**Flow ID**: FL-009
**Derived From**: UC-010, UC-015, UC-016
**Personas Covered**: Staff
**Description**: Staff reviews AI-extracted clinical data, verifies or corrects, resolves conflicts

#### Flow Sequence

```text
1. Entry: SCR-004 / Default
   - Trigger: Staff clicks patient with pending verifications
   |
   v
2. Step: SCR-017 / Default
   - Action: Staff views 360° Patient View with unverified items highlighted
   |
   v
3. Step: SCR-023 / Default
   - Action: Staff reviews individual data points with source document reference
   |
   v
4. Decision Loop (per data point):
   +-- Verify -> Badge updates to green "Verified"
   +-- Correct -> Inline edit, save correction
   +-- Reject -> Flag for re-extraction
   |
   v
5. Decision Point: Conflicts detected?
   +-- Yes -> SCR-024 / Default (side-by-side conflict comparison)
   |   +-- Resolve each -> Update profile, log resolution
   +-- No -> Continue to next data point
   |
   v
6. Exit: SCR-017 / Default (all items verified/resolved)
```

#### Required Interactions

- Source document viewer in side panel
- One-click verify/reject buttons per data point
- Audit trail logged for every action
- Conflict severity badges (Critical/High/Medium)

---

### Flow: FL-010 — Admin User Management

**Flow ID**: FL-010
**Derived From**: UC-014
**Personas Covered**: Admin
**Description**: Admin creates, edits, or deactivates Staff and Admin user accounts

#### Flow Sequence

```text
1. Entry: SCR-005 / Default
   - Trigger: Admin clicks "User Management" in navigation
   |
   v
2. Step: SCR-021 / Default
   - Action: Admin views user list with search and filters
   |
   v
3. Decision Point:
   +-- Create -> SCR-022 / Default (new user form)
   +-- Edit -> SCR-022 / Default (pre-filled user form)
   +-- Deactivate -> Dialog: Confirm deactivation
       +-- Confirm -> Toast: "User deactivated", sessions terminated
       +-- Cancel -> SCR-021 / Default
   |
   v
4. Step: SCR-022 / Validation
   - Action: Admin fills/edits form, system validates
   +-- Success -> Toast: "User created/updated", redirect to SCR-021
   +-- Error -> SCR-022 / Error (validation errors)
   |
   v
5. Exit: SCR-021 / Default (updated user list)
```

#### Required Interactions

- Role assignment dropdown with permission preview
- Deactivation confirmation includes session termination notice
- New user receives activation email

---

### Flow: FL-011 — Error Recovery (Authentication)

**Flow ID**: FL-011
**Derived From**: UC-002 extensions
**Personas Covered**: All
**Description**: Error recovery paths for authentication failures

#### Flow Sequence

```text
1. Entry: SCR-002 / Error
   - Trigger: Failed login attempt
   |
   v
2. Decision Point:
   +-- Forgot Password -> Password recovery flow
   |   - Enter email -> Confirmation message -> Email link -> Reset form
   +-- Account Locked -> Contact support message displayed
   +-- Account Deactivated -> Status message displayed
   |
   v
3. Exit: SCR-002 / Default (after recovery) or external support
```

#### Required Interactions

- Generic error (no credential-specific hints for security)
- Password reset email with timed token
- Clear lockout messaging with support contact

---

## 12. Export Requirements

### JPG Export Settings

| Setting | Value |
|---------|-------|
| Format | JPG |
| Quality | High (85%) |
| Scale - Mobile | 2x |
| Scale - Web | 2x |
| Color Profile | sRGB |

### Export Naming Convention

`PatientAccess__<Platform>__<ScreenName>__<State>__v<Version>.jpg`

### Export Manifest

| Screen | State | Platform | Filename |
|--------|-------|----------|----------|
| SCR-001 | Default | Web | PatientAccess__Web__Registration__Default__v1.jpg |
| SCR-001 | Loading | Web | PatientAccess__Web__Registration__Loading__v1.jpg |
| SCR-001 | Error | Web | PatientAccess__Web__Registration__Error__v1.jpg |
| SCR-001 | Validation | Web | PatientAccess__Web__Registration__Validation__v1.jpg |
| SCR-002 | Default | Web | PatientAccess__Web__Login__Default__v1.jpg |
| SCR-002 | Loading | Web | PatientAccess__Web__Login__Loading__v1.jpg |
| SCR-002 | Error | Web | PatientAccess__Web__Login__Error__v1.jpg |
| SCR-002 | Validation | Web | PatientAccess__Web__Login__Validation__v1.jpg |
| SCR-003 | Default | Web | PatientAccess__Web__PatientDashboard__Default__v1.jpg |
| SCR-003 | Loading | Web | PatientAccess__Web__PatientDashboard__Loading__v1.jpg |
| SCR-003 | Empty | Web | PatientAccess__Web__PatientDashboard__Empty__v1.jpg |
| SCR-003 | Error | Web | PatientAccess__Web__PatientDashboard__Error__v1.jpg |
| SCR-004 | Default | Web | PatientAccess__Web__StaffDashboard__Default__v1.jpg |
| SCR-004 | Loading | Web | PatientAccess__Web__StaffDashboard__Loading__v1.jpg |
| SCR-004 | Empty | Web | PatientAccess__Web__StaffDashboard__Empty__v1.jpg |
| SCR-004 | Error | Web | PatientAccess__Web__StaffDashboard__Error__v1.jpg |
| SCR-005 | Default | Web | PatientAccess__Web__AdminDashboard__Default__v1.jpg |
| SCR-005 | Loading | Web | PatientAccess__Web__AdminDashboard__Loading__v1.jpg |
| SCR-005 | Empty | Web | PatientAccess__Web__AdminDashboard__Empty__v1.jpg |
| SCR-005 | Error | Web | PatientAccess__Web__AdminDashboard__Error__v1.jpg |
| SCR-006 | Default | Web | PatientAccess__Web__ProviderBrowser__Default__v1.jpg |
| SCR-006 | Loading | Web | PatientAccess__Web__ProviderBrowser__Loading__v1.jpg |
| SCR-006 | Empty | Web | PatientAccess__Web__ProviderBrowser__Empty__v1.jpg |
| SCR-006 | Error | Web | PatientAccess__Web__ProviderBrowser__Error__v1.jpg |
| SCR-007 | Default | Web | PatientAccess__Web__AppointmentBooking__Default__v1.jpg |
| SCR-007 | Loading | Web | PatientAccess__Web__AppointmentBooking__Loading__v1.jpg |
| SCR-007 | Error | Web | PatientAccess__Web__AppointmentBooking__Error__v1.jpg |
| SCR-007 | Validation | Web | PatientAccess__Web__AppointmentBooking__Validation__v1.jpg |
| SCR-008 | Default | Web | PatientAccess__Web__AppointmentConfirmation__Default__v1.jpg |
| SCR-008 | Loading | Web | PatientAccess__Web__AppointmentConfirmation__Loading__v1.jpg |
| SCR-008 | Error | Web | PatientAccess__Web__AppointmentConfirmation__Error__v1.jpg |
| SCR-009 | Default | Web | PatientAccess__Web__WaitlistEnrollment__Default__v1.jpg |
| SCR-009 | Loading | Web | PatientAccess__Web__WaitlistEnrollment__Loading__v1.jpg |
| SCR-009 | Error | Web | PatientAccess__Web__WaitlistEnrollment__Error__v1.jpg |
| SCR-009 | Validation | Web | PatientAccess__Web__WaitlistEnrollment__Validation__v1.jpg |
| SCR-010 | Default | Web | PatientAccess__Web__MyAppointments__Default__v1.jpg |
| SCR-010 | Loading | Web | PatientAccess__Web__MyAppointments__Loading__v1.jpg |
| SCR-010 | Empty | Web | PatientAccess__Web__MyAppointments__Empty__v1.jpg |
| SCR-010 | Error | Web | PatientAccess__Web__MyAppointments__Error__v1.jpg |
| SCR-011 | Default | Web | PatientAccess__Web__RescheduleAppointment__Default__v1.jpg |
| SCR-011 | Loading | Web | PatientAccess__Web__RescheduleAppointment__Loading__v1.jpg |
| SCR-011 | Error | Web | PatientAccess__Web__RescheduleAppointment__Error__v1.jpg |
| SCR-011 | Validation | Web | PatientAccess__Web__RescheduleAppointment__Validation__v1.jpg |
| SCR-012 | Default | Web | PatientAccess__Web__AIIntake__Default__v1.jpg |
| SCR-012 | Loading | Web | PatientAccess__Web__AIIntake__Loading__v1.jpg |
| SCR-012 | Error | Web | PatientAccess__Web__AIIntake__Error__v1.jpg |
| SCR-013 | Default | Web | PatientAccess__Web__ManualIntake__Default__v1.jpg |
| SCR-013 | Loading | Web | PatientAccess__Web__ManualIntake__Loading__v1.jpg |
| SCR-013 | Error | Web | PatientAccess__Web__ManualIntake__Error__v1.jpg |
| SCR-013 | Validation | Web | PatientAccess__Web__ManualIntake__Validation__v1.jpg |
| SCR-014 | Default | Web | PatientAccess__Web__DocumentUpload__Default__v1.jpg |
| SCR-014 | Loading | Web | PatientAccess__Web__DocumentUpload__Loading__v1.jpg |
| SCR-014 | Error | Web | PatientAccess__Web__DocumentUpload__Error__v1.jpg |
| SCR-014 | Validation | Web | PatientAccess__Web__DocumentUpload__Validation__v1.jpg |
| SCR-015 | Default | Web | PatientAccess__Web__DocumentStatus__Default__v1.jpg |
| SCR-015 | Loading | Web | PatientAccess__Web__DocumentStatus__Loading__v1.jpg |
| SCR-015 | Empty | Web | PatientAccess__Web__DocumentStatus__Empty__v1.jpg |
| SCR-015 | Error | Web | PatientAccess__Web__DocumentStatus__Error__v1.jpg |
| SCR-016 | Default | Web | PatientAccess__Web__PatientHealthDashboard__Default__v1.jpg |
| SCR-016 | Loading | Web | PatientAccess__Web__PatientHealthDashboard__Loading__v1.jpg |
| SCR-016 | Empty | Web | PatientAccess__Web__PatientHealthDashboard__Empty__v1.jpg |
| SCR-016 | Error | Web | PatientAccess__Web__PatientHealthDashboard__Error__v1.jpg |
| SCR-017 | Default | Web | PatientAccess__Web__StaffPatientView__Default__v1.jpg |
| SCR-017 | Loading | Web | PatientAccess__Web__StaffPatientView__Loading__v1.jpg |
| SCR-017 | Empty | Web | PatientAccess__Web__StaffPatientView__Empty__v1.jpg |
| SCR-017 | Error | Web | PatientAccess__Web__StaffPatientView__Error__v1.jpg |
| SCR-018 | Default | Web | PatientAccess__Web__WalkinBooking__Default__v1.jpg |
| SCR-018 | Loading | Web | PatientAccess__Web__WalkinBooking__Loading__v1.jpg |
| SCR-018 | Error | Web | PatientAccess__Web__WalkinBooking__Error__v1.jpg |
| SCR-018 | Validation | Web | PatientAccess__Web__WalkinBooking__Validation__v1.jpg |
| SCR-019 | Default | Web | PatientAccess__Web__QueueManagement__Default__v1.jpg |
| SCR-019 | Loading | Web | PatientAccess__Web__QueueManagement__Loading__v1.jpg |
| SCR-019 | Empty | Web | PatientAccess__Web__QueueManagement__Empty__v1.jpg |
| SCR-019 | Error | Web | PatientAccess__Web__QueueManagement__Error__v1.jpg |
| SCR-020 | Default | Web | PatientAccess__Web__ArrivalManagement__Default__v1.jpg |
| SCR-020 | Loading | Web | PatientAccess__Web__ArrivalManagement__Loading__v1.jpg |
| SCR-020 | Empty | Web | PatientAccess__Web__ArrivalManagement__Empty__v1.jpg |
| SCR-020 | Error | Web | PatientAccess__Web__ArrivalManagement__Error__v1.jpg |
| SCR-021 | Default | Web | PatientAccess__Web__UserManagement__Default__v1.jpg |
| SCR-021 | Loading | Web | PatientAccess__Web__UserManagement__Loading__v1.jpg |
| SCR-021 | Empty | Web | PatientAccess__Web__UserManagement__Empty__v1.jpg |
| SCR-021 | Error | Web | PatientAccess__Web__UserManagement__Error__v1.jpg |
| SCR-022 | Default | Web | PatientAccess__Web__UserForm__Default__v1.jpg |
| SCR-022 | Loading | Web | PatientAccess__Web__UserForm__Loading__v1.jpg |
| SCR-022 | Error | Web | PatientAccess__Web__UserForm__Error__v1.jpg |
| SCR-022 | Validation | Web | PatientAccess__Web__UserForm__Validation__v1.jpg |
| SCR-023 | Default | Web | PatientAccess__Web__ClinicalVerification__Default__v1.jpg |
| SCR-023 | Loading | Web | PatientAccess__Web__ClinicalVerification__Loading__v1.jpg |
| SCR-023 | Empty | Web | PatientAccess__Web__ClinicalVerification__Empty__v1.jpg |
| SCR-023 | Error | Web | PatientAccess__Web__ClinicalVerification__Error__v1.jpg |
| SCR-024 | Default | Web | PatientAccess__Web__ConflictResolution__Default__v1.jpg |
| SCR-024 | Loading | Web | PatientAccess__Web__ConflictResolution__Loading__v1.jpg |
| SCR-024 | Empty | Web | PatientAccess__Web__ConflictResolution__Empty__v1.jpg |
| SCR-024 | Error | Web | PatientAccess__Web__ConflictResolution__Error__v1.jpg |
| SCR-025 | Default | Web | PatientAccess__Web__AuditLog__Default__v1.jpg |
| SCR-025 | Loading | Web | PatientAccess__Web__AuditLog__Loading__v1.jpg |
| SCR-025 | Empty | Web | PatientAccess__Web__AuditLog__Empty__v1.jpg |
| SCR-025 | Error | Web | PatientAccess__Web__AuditLog__Error__v1.jpg |
| SCR-026 | Default | Web | PatientAccess__Web__SystemSettings__Default__v1.jpg |
| SCR-026 | Loading | Web | PatientAccess__Web__SystemSettings__Loading__v1.jpg |
| SCR-026 | Error | Web | PatientAccess__Web__SystemSettings__Error__v1.jpg |
| SCR-026 | Validation | Web | PatientAccess__Web__SystemSettings__Validation__v1.jpg |

### Total Export Count

- **Screens**: 26
- **States per screen**: 4 average (mix of 3–5 states)
- **Total JPGs**: 102

---

## 13. Figma File Structure

### Page Organization

```text
PatientAccess Figma File
+-- 00_Cover
|   +-- Project: Patient Access & Clinical Intelligence Platform
|   +-- Version: 1.0
|   +-- Last Updated: 2026-03-20
|   +-- Stakeholders: Product, Engineering, Clinical
+-- 01_Foundations
|   +-- Color tokens (Light + Dark)
|   +-- Typography scale (H1–Caption)
|   +-- Spacing scale (4–64px)
|   +-- Radius tokens (4, 8, 16, 9999px)
|   +-- Elevation/shadows (Levels 1–5)
|   +-- Grid definitions (12-column responsive)
|   +-- Breakpoints (320, 768, 1024, 1440px)
+-- 02_Components
|   +-- C/Actions/[Button, IconButton, Link, FAB]
|   +-- C/Inputs/[TextField, TextArea, Select, Checkbox, Radio, Toggle, FileUpload, SearchBar]
|   +-- C/Navigation/[Header, Sidebar, BottomNav, Tabs, Breadcrumb, Pagination]
|   +-- C/Content/[Card, StatCard, ListItem, Table, DataGrid, Badge, Avatar, Accordion, ChatBubble, SideBySideCompare]
|   +-- C/Feedback/[Modal, Dialog, Drawer, SidePanel, Toast, Alert, Tooltip, Skeleton, Spinner, ProgressBar, EmptyState]
|   +-- C/DataViz/[LineChart, Calendar, TimeSlotGrid, Timer, PasswordStrength]
+-- 03_Patterns
|   +-- Auth form pattern (Login + Registration)
|   +-- Dashboard shell pattern (Sidebar + Header + Content)
|   +-- Search + filter pattern
|   +-- Detail page pattern (360° Patient View)
|   +-- Chat interface pattern (AI Intake)
|   +-- Data verification pattern (side panel with source)
|   +-- Error / Empty / Loading patterns
+-- 04_Screens
|   +-- Public/
|   |   +-- Registration/[Default, Loading, Error, Validation]
|   |   +-- Login/[Default, Loading, Error, Validation]
|   +-- PatientPortal/
|   |   +-- PatientDashboard/[Default, Loading, Empty, Error]
|   |   +-- ProviderBrowser/[Default, Loading, Empty, Error]
|   |   +-- AppointmentBooking/[Default, Loading, Error, Validation]
|   |   +-- AppointmentConfirmation/[Default, Loading, Error]
|   |   +-- WaitlistEnrollment/[Default, Loading, Error, Validation]
|   |   +-- MyAppointments/[Default, Loading, Empty, Error]
|   |   +-- RescheduleAppointment/[Default, Loading, Error, Validation]
|   |   +-- AIIntake/[Default, Loading, Error]
|   |   +-- ManualIntake/[Default, Loading, Error, Validation]
|   |   +-- DocumentUpload/[Default, Loading, Error, Validation]
|   |   +-- DocumentStatus/[Default, Loading, Empty, Error]
|   |   +-- PatientHealthDashboard/[Default, Loading, Empty, Error]
|   +-- StaffPortal/
|   |   +-- StaffDashboard/[Default, Loading, Empty, Error]
|   |   +-- StaffPatientView/[Default, Loading, Empty, Error]
|   |   +-- WalkinBooking/[Default, Loading, Error, Validation]
|   |   +-- QueueManagement/[Default, Loading, Empty, Error]
|   |   +-- ArrivalManagement/[Default, Loading, Empty, Error]
|   |   +-- ClinicalVerification/[Default, Loading, Empty, Error]
|   |   +-- ConflictResolution/[Default, Loading, Empty, Error]
|   +-- AdminPortal/
|       +-- AdminDashboard/[Default, Loading, Empty, Error]
|       +-- UserManagement/[Default, Loading, Empty, Error]
|       +-- UserForm/[Default, Loading, Error, Validation]
|       +-- AuditLog/[Default, Loading, Empty, Error]
|       +-- SystemSettings/[Default, Loading, Error, Validation]
+-- 05_Prototype
|   +-- FL-001: Patient Registration & Onboarding
|   +-- FL-002: Authentication (role-routed)
|   +-- FL-003: Appointment Booking
|   +-- FL-004: Appointment Management
|   +-- FL-005: Patient Intake (Dual Mode)
|   +-- FL-006: Document Upload & Processing
|   +-- FL-007: Patient Health Dashboard
|   +-- FL-008: Staff Walk-in & Queue
|   +-- FL-009: Clinical Data Verification
|   +-- FL-010: Admin User Management
|   +-- FL-011: Error Recovery (Auth)
+-- 06_Handoff
    +-- Token usage rules
    +-- Component usage guidelines
    +-- Responsive behavior specs (breakpoints)
    +-- Edge case handling
    +-- Accessibility notes (WCAG 2.2 AA)
    +-- AI/Staff badge distinction rules
```

---

## 14. Quality Checklist

### Pre-Export Validation

- [ ] All 26 screens have required states (Default/Loading/Empty or Validation/Error)
- [ ] All components use design tokens — no hard-coded values
- [ ] Color contrast meets WCAG AA (≥4.5:1 text, ≥3:1 UI)
- [ ] Focus states defined for all interactive elements (≥3:1 contrast outline)
- [ ] Touch targets ≥ 44x44px on mobile
- [ ] All 11 prototype flows wired and functional
- [ ] Naming conventions followed (`C/<Category>/<Name>`, `<Screen>/<State>`)
- [ ] Export manifest complete (102 JPGs)
- [ ] AI-suggested vs. staff-verified badge distinction applied (amber vs. green)
- [ ] Session timeout modal wired on all authenticated screens
- [ ] Empty states include illustrations and CTAs
- [ ] ARIA live region annotations for dynamic content screens

### Post-Generation

- [ ] designsystem.md updated with component references
- [ ] Export manifest generated
- [ ] JPG files named per convention
- [ ] Handoff documentation complete
