# Figma Design Specification - Unified Patient Access & Clinical Intelligence Platform

## 1. Figma Specification
**Platform**: Responsive Web (Desktop 1024px+, Tablet 768-1023px, Mobile 320-767px)

---

## 2. Source References

### Primary Source
| Document | Path | Purpose |
|----------|------|---------|
| Requirements | `.propel/context/docs/spec.md` | 35 FR requirements, 10 use cases with UI impact flags |
| Architecture | `.propel/context/docs/design.md` | 23 NFR, 18 TR, 13 DR, 30 AIR requirements with tech stack |

### Optional Sources
| Document | Path | Purpose |
|----------|------|---------|
| Wireframes | `.propel/context/wireframes/` | (Not yet created) Entity understanding, content structure |
| Design Assets | `.propel/context/Design/` | (Not yet created) Visual references |

### Related Documents
| Document | Path | Purpose |
|----------|------|---------|
| Design System | `.propel/context/docs/designsystem.md` | Design tokens, branding, component specifications |

---

## 3. UX Requirements

*Generated from use cases UC-001 to UC-010 and functional requirements FR-001 to FR-035 with confirmed UI impact. Requirements derived from React 18+ TypeScript, Next.js 14+, Tailwind CSS tech stack with free-tier infrastructure constraints.*

### UXR Requirements Table
| UXR-ID | Category | Requirement | Acceptance Criteria | Screens Affected |
|--------|----------|-------------|---------------------|------------------|
| UXR-101 | Usability | System MUST enable appointment booking within 3 clicks from authenticated homepage | User logs in → Click "Book Appointment" (1) → Select slot (2) → Confirm booking (3) → Confirmation displayed | SCR-001, SCR-002 |
| UXR-102 | Usability | System MUST display clear navigation hierarchy per role (Patient/Staff/Admin) | Patient sees: Dashboard, Book, Intake, Documents; Staff sees: Arrivals, Walk-Ins, Clinical Review; Admin sees: Users, Audit Logs; Role-inappropriate nav items hidden | All screens |
| UXR-103 | Usability | System MUST display intake mode toggle prominently at all times during intake | Toggle visible in header/toolbar during intake; Single click switches AI ↔ Manual; Entered data preserved; Toggle labeled "AI Intake" / "Manual Form" | SCR-004, SCR-005 |
| UXR-104 | Usability | System MUST display staff arrival queue with visual hierarchy | Same-day appointments sorted by time; Arrived patients highlighted green; Overdue patients highlighted red; Drag-drop reorder enabled; Expected wait time visible | SCR-007 |
| UXR-105 | Usability | System MUST provide one-click access to 360-Degree Patient View from staff dashboard | "360° View" button/tab in patient record header; Click opens consolidated timeline; No nested navigation required | SCR-009 |
| UXR-201 | Accessibility | System MUST comply with WCAG 2.2 Level AA standards | All interactive elements keyboard-navigable; ARIA labels present; Heading hierarchy logical; Alt text for images; Form labels associated with inputs | All screens |
| UXR-202 | Accessibility | System MUST support full keyboard navigation without mouse | Tab order logical; Enter activates buttons; Escape closes modals; Arrow keys navigate lists/tables; Shortcuts documented | All screens |
| UXR-203 | Accessibility | System MUST be screen reader compatible (NVDA, JAWS, VoiceOver) | ARIA live regions announce status changes; Role attributes correct; Skip navigation links present; Table headers properly associated | All screens |
| UXR-204 | Accessibility | System MUST meet 4.5:1 color contrast ratio for normal text, 3:1 for large text/UI | Automated axe-core validation passes; Body text 16px minimum; Contrast failures cause build warning | All screens |
| UXR-205 | Accessibility | System MUST provide minimum 44x44px touch targets on mobile | Buttons, links, form inputs sized ≥44px height/width; Adequate spacing between targets; Tested on iPhone SE 320px viewport | All mobile screens |
| UXR-206 | Accessibility | System MUST display visible focus indicators for keyboard navigation | 2px solid outline on focused elements; Focus never hidden by other elements; High contrast focus color; Tested via keyboard-only navigation | All screens |
| UXR-301 | Responsiveness | System MUST render usable interface at 320px mobile viewport | No horizontal scroll; Text readable without zoom; Forms single-column; Navigation collapsed to hamburger menu | All screens |
| UXR-302 | Responsiveness | System MUST optimize layout for tablet 768-1023px viewport | Staff dashboard two-column layout; Touch-optimized controls; Forms span full width; Navigation visible in sidebar | SCR-006, SCR-007, SCR-009 |
| UXR-303 | Responsiveness | System MUST utilize desktop 1024px+ viewport efficiently | Multi-column dashboards; Sidebar navigation persistent; Data tables expanded; Admin panels use full width | All desktop screens |
| UXR-304 | Responsiveness | System MUST provide touch-friendly mobile interactions | Swipe gestures for navigation; Pull-to-refresh on lists; Tap targets ≥44px; No hover-dependent interactions | All mobile screens |
| UXR-305 | Responsiveness | System MUST prevent horizontal scrolling at all breakpoints | Max-width constraints on content; Tables scroll within containers; Images responsive; CSS overflow-x:hidden on body | All screens |
| UXR-401 | Visual Design | System MUST use design system tokens consistently | Colors from Tailwind CSS palette; Typography from designsystem.md scale; Spacing 4px/8px base grid; Shadows from elevation tokens | All screens |
| UXR-402 | Visual Design | System MUST adhere to Tailwind CSS utility-first approach | No custom CSS outside configuration; Utilities for layout, spacing, typography; Responsive modifiers (sm:, md:, lg:); JIT mode for production | All screens |
| UXR-403 | Visual Design | System MUST use shadcn/ui component library as base | Button, Card, Input, Select, Dialog, Toast from shadcn/ui; Customized via CSS variables; No duplicate component implementations | All screens |
| UXR-404 | Visual Design | System MUST convey healthcare-appropriate visual tone | Professional color scheme (blues, whites, grays); Sans-serif readable typography; Minimal decorative elements; HIPAA-compliant aesthetic (secure, trustworthy) | All screens |
| UXR-501 | Interaction | System MUST provide user feedback within 200ms of interaction | Button press shows active state immediately; Form input shows focus state; Click ripple effect; Optimistic UI for bookings | All screens |
| UXR-502 | Interaction | System MUST display loading states for async operations >500ms | PDF generation shows progress spinner (FR-007); AI extraction shows skeleton screens (FR-022); Calendar sync shows "Syncing..." indicator | SCR-002, SCR-009 |
| UXR-503 | Interaction | System MUST maintain 60fps animations | CSS transitions hardware-accelerated (transform, opacity only); Animations <300ms duration; Motion reduced for prefers-reduced-motion users | All screens |
| UXR-504 | Interaction | System MUST validate form inputs inline in real-time | Email format validated on blur; Required fields show error on blur; Valid inputs show green checkmark; Invalid inputs show red border + error text | SCR-004, SCR-005, SCR-011 |
| UXR-505 | Interaction | System MUST provide immediate slot selection feedback | Selected slot highlights immediately; Booked slots grayed out; Hover shows slot details; Double-booking prevented via optimistic locking (DR-002) | SCR-002 |
| UXR-601 | Error Handling | System MUST display clear, actionable error messages | Error text explains what went wrong + what to do; "Slot No Longer Available - Please select another time" vs "Error 409"; No technical jargon visible to patients | All screens |
| UXR-602 | Error Handling | System MUST provide recovery paths in error messages | "Retry" button for transient failures; "Contact Support" link for critical errors; "Back" button to previous screen; Error messages dismissible | All screens |
| UXR-603 | Error Handling | System MUST display session timeout warning at 13 minutes | Modal appears center-screen at 13:00 remaining; "You'll be logged out in 2 minutes. Continue session?" with "Stay Logged In" button; Countdown timer visible | All authenticated screens |
| UXR-604 | Error Handling | System MUST display critical clinical data conflict UI prominently | Red warning banner at top of 360° View; Conflicts listed with source documents; "Document A: No allergies" vs "Document B: Penicillin allergy"; Staff cannot proceed without acknowledging conflict | SCR-009 |
| UXR-605 | Error Handling | System MUST handle slot unavailability gracefully during booking | Race condition shows toast: "This slot was just booked by another patient"; Returns to slot selection; Previously selected slot highlighted but disabled; Alternative slots suggested | SCR-002 |

### UXR Category Derivation

**Usability Requirements (UXR-1XX):**
- Derived from use case success paths (UC-001 appointment booking, UC-008 360° view navigation)
- Navigation depth constraint (max 3 clicks) from FR-001 real-time availability requirement
- Role-based navigation (FR-028 RBAC) informing hierarchy clarity
- Intake toggle visibility (FR-011 anytime switching) requiring persistent UI element

**Accessibility Requirements (UXR-2XX):**
- WCAG 2.2 AA compliance mandated by NFR-020
- Keyboard navigation + screen reader support for ADA compliance
- Touch target sizing for mobile usability (NFR-021 responsive requirement)
- Color contrast ensuring readability for visual impairments

**Responsiveness Requirements (UXR-3XX):**
- Breakpoint definitions from NFR-021 (mobile 320px, tablet 768px, desktop 1024px+)
- Staff tablet usage scenarios (UC-005 walk-in, UC-006 arrivals)
- Patient mobile booking (UC-001) requiring touch-friendly interactions
- Multi-device access consistency

**Visual Design Requirements (UXR-4XX):**
- Design system adherence from TR-001 (Tailwind CSS 3.4+, shadcn/ui)
- Professional healthcare aesthetic supporting HIPAA-compliant branding (NFR-022)
- Token consistency supporting maintainability (NFR-019)

**Interaction Requirements (UXR-5XX):**
- 200ms feedback threshold from NFR-001 (p95 3s API response split into perceived chunks)
- Loading states for PDF generation (NFR-004 10s) and AI extraction (NFR-005 30s)
- Inline validation preventing errors before submission (UC-004 manual form)
- Optimistic UI for booking responsiveness (DR-002 optimistic locking)

**Error Handling Requirements (UXR-6XX):**
- Error recovery paths from UC-001 alternative flows (step 3a slot unavailable)
- Session timeout warning from FR-029 (15-minute timeout with 13-minute alert)
- Clinical conflict resolution from FR-024 + UC-008 (critical data discrepancies)
- User-friendly messaging avoiding technical jargon for patient-facing errors

---

## 4. Personas Summary

*Derived from spec.md Section "Actors & System Boundary" - Reference only*

| Persona | Role | Primary Goals | Key Screens |
|---------|------|---------------|-------------|
| Patient | End user seeking appointments | Book slots, complete intake, upload documents, view confirmations | SCR-001 Dashboard, SCR-002 Slot Selection, SCR-004/005 Intake, SCR-003 Documents |
| Staff | Front desk / Call center admin | Process walk-ins, mark arrivals, validate insurance, review clinical data | SCR-006 Walk-In, SCR-007 Arrivals, SCR-008 Insurance, SCR-009 360° View |
| Admin | System administrator | Manage user accounts, assign permissions, review audit logs, configure system | SCR-010 User List, SCR-011 Create/Edit User, SCR-012 Audit Logs |

---

## 5. Information Architecture

### Site Map
```text
Unified Patient Access & Clinical Intelligence Platform
+-- Authentication (Public)
|   +-- SCR-013 Login
|   +-- SCR-014 Password Reset
|   +-- SCR-015 Account Activation
+-- Patient Portal (Role: Patient)
|   +-- SCR-001 Patient Dashboard
|   +-- SCR-002 Appointment Booking / Slot Selection
|   +-- SCR-003 Document Upload
|   +-- SCR-004 AI Conversational Intake
|   +-- SCR-005 Manual Form Intake
|   +-- SCR-016 Appointment History
+-- Staff Portal (Role: Staff)
|   +-- SCR-006 Walk-In Management
|   +-- SCR-007 Arrival Management / Same-Day Queue
|   +-- SCR-008 Insurance Validation
|   +-- SCR-009 360-Degree Patient View
|   +-- SCR-017 Clinical Coding Review (ICD-10/CPT)
+-- Admin Panel (Role: Admin)
    +-- SCR-010 User Management (List)
    +-- SCR-011 Create/Edit User Form
    +-- SCR-012 Audit Log Viewer
    +-- SCR-018 System Configuration
```

### Navigation Patterns
| Pattern | Type | Platform Behavior |
|---------|------|-------------------|
| Primary Nav | Sidebar (Desktop) / Bottom Nav (Mobile) | Desktop: Persistent left sidebar with role-specific menu items; Mobile: Fixed bottom navigation bar with 4-5 core actions |
| Secondary Nav | Tabs (within screens) | 360° View (SCR-009) uses tabs: Timeline, Documents, Medications, Conflicts |
| Utility Nav | Header user menu | Top-right: User avatar → Dropdown (Profile, Settings, Logout); Session timeout countdown visible |
| Role Switching | (Future) Toggle in header | Admin can switch to Staff or Patient view for testing; Not in Phase 1 |

---

## 6. Screen Inventory

*All screens derived from use cases UC-001 to UC-010 and functional requirements FR-001 to FR-035*

### Screen List
| Screen ID | Screen Name | Derived From | Personas Covered | Priority | UXR Mapped |
|-----------|-------------|--------------|------------------|----------|------------|
| SCR-001 | Patient Dashboard | UC-001 entry | Patient | P0 | UXR-101, 102, 201-206, 301-305, 401-404, 501, 603 |
| SCR-002 | Appointment Slot Selection & Booking | UC-001, UC-002 | Patient | P0 | UXR-101, 105, 201-206, 301-305, 401-404, 501, 502, 505, 601, 602, 605 |
| SCR-003 | Clinical Document Upload | FR-022, UC-008 | Patient | P0 | UXR-201-206, 301-305, 401-404, 502, 601, 602 |
| SCR-004 | AI Conversational Intake | UC-003, FR-009 | Patient | P0 | UXR-103, 201-206, 301-305, 401-404, 501, 502, 601, 602 |
| SCR-005 | Manual Form Intake | UC-004, FR-010 | Patient | P0 | UXR-103, 201-206, 301-305, 401-404, 504, 601, 602 |
| SCR-006 | Staff Walk-In Management | UC-005, FR-016-017 | Staff | P0 | UXR-102, 201-206, 302-303, 401-404, 501, 504, 601, 602 |
| SCR-007 | Staff Arrival Management / Queue | UC-006, FR-018-019 | Staff | P0 | UXR-104, 102, 201-206, 302-303, 401-404, 501, 601, 602 |
| SCR-008 | Insurance Pre-Check Validation | UC-007, FR-021 | Staff | P1 | UXR-102, 201-206, 302-303, 401-404, 501, 601, 602 |
| SCR-009 | 360-Degree Patient View / Clinical Review | UC-008, FR-022-025 | Staff | P0 | UXR-105, 102, 201-206, 302-303, 401-404, 502, 601, 602, 604 |
| SCR-010 | Admin User Management List | UC-009, FR-030 | Admin | P0 | UXR-102, 201-206, 303, 401-404, 501, 601, 602 |
| SCR-011 | Admin Create/Edit User Form | UC-009, FR-030 | Admin | P0 | UXR-102, 201-206, 303, 401-404, 504, 601, 602 |
| SCR-012 | Audit Log Viewer | FR-031 | Admin | P1 | UXR-102, 201-206, 303, 401-404, 601, 602 |
| SCR-013 | Login Screen | FR-028 | All (unauthenticated) | P0 | UXR-201-206, 301-305, 401-404, 504, 601, 602 |
| SCR-014 | Password Reset | UC-009 (implicit) | All | P1 | UXR-201-206, 301-305, 401-404, 504, 601, 602 |
| SCR-015 | Account Activation | FR-017, UC-009 | All | P1 | UXR-201-206, 301-305, 401-404, 601, 602 |
| SCR-016 | Patient Appointment History | FR-002 (implicit) | Patient | P1 | UXR-102, 201-206, 301-305, 401-404, 501, 601, 602 |
| SCR-017 | Clinical Coding Review (ICD-10/CPT) | FR-026-027 | Staff | P1 | UXR-102, 201-206, 302-303, 401-404, 501, 502, 601, 602 |
| SCR-018 | System Configuration | FR-034 (health checks) | Admin | P2 | UXR-102, 201-206, 303, 401-404, 601, 602 |

### Priority Legend
- **P0**: Critical path (MVP launch blocker) - Authentication, core booking, intake, clinical review, arrivals
- **P1**: Core functionality (high priority) - Insurance validation, appointment history, coding review, audit logs
- **P2**: Important features (medium priority) - System config

### Screen-to-Persona Coverage Matrix
| Screen | Patient | Staff | Admin | Notes |
|--------|---------|-------|-------|-------|
| SCR-001 Dashboard | Primary | - | - | Patient entry point post-login |
| SCR-002 Slot Selection | Primary | - | - | Core booking workflow |
| SCR-003 Document Upload | Primary | Secondary | - | Patients upload; staff review in SCR-009 |
| SCR-004 AI Intake | Primary | - | - | Patient-selectable intake mode |
| SCR-005 Manual Intake | Primary | - | - | Alternative to AI intake |
| SCR-006 Walk-In | - | Primary | - | Staff-exclusive feature (FR-016) |
| SCR-007 Arrivals | - | Primary | - | Staff marks patient arrivals (FR-019) |
| SCR-008 Insurance | - | Primary | - | Staff-only validation (FR-021) |
| SCR-009 360° View | - | Primary | Secondary | Staff primary; admin can audit access |
| SCR-010 User List | - | - | Primary | Admin-only user management |
| SCR-011 User Form | - | - | Primary | Admin creates/edits accounts |
| SCR-012 Audit Logs | - | - | Primary | Admin-only compliance view |
| SCR-013 Login | All | All | All | Public authentication |
| SCR-014 Reset Password | All | All | All | Self-service password recovery |
| SCR-015 Activation | All | All | All | New account setup |
| SCR-016 History | Primary | - | - | Patient views past appointments |
| SCR-017 Coding Review | - | Primary | - | Staff verifies AI code suggestions |
| SCR-018 Config | - | - | Primary | Admin system settings |

### Modal/Overlay Inventory
| Name | Type | Trigger | Parent Screen(s) | Priority |
|------|------|---------|-----------------|----------|
| Session Timeout Warning | Modal | 13-minute inactivity | All authenticated screens | P0 |
| Booking Confirmation | Modal | "Confirm Booking" button | SCR-002 | P0 |
| Slot Conflict Error | Toast | Concurrent booking race condition | SCR-002 | P0 |
| Intake Toggle Confirmation | Dialog | Toggle with unsaved changes | SCR-004, SCR-005 | P0 |
| Clinical Data Conflict Alert | Banner | Conflicting data detected | SCR-009 | P0 |
| PDF Generation Progress | Toast | Booking confirmed | SCR-002 | P1 |
| AI Extraction Progress | Skeleton Overlay | Document upload processing | SCR-003, SCR-009 | P0 |
| Delete User Confirmation | Dialog | "Delete User" button | SCR-010 | P0 |
| Insurance Validation Result | Toast | Validation complete | SCR-008 | P1 |
| Calendar Sync Status | Toast | Calendar event creation | SCR-002 | P1 |

---

## 7. Content & Tone

### Voice & Tone
- **Overall Tone**: Professional yet approachable; empathetic for patient-facing, efficient for staff workflows
- **Error Messages**: Helpful, non-blaming, actionable ("This slot is no longer available. Please select another time." vs "Error 409 Conflict")
- **Empty States**: Encouraging with clear CTAs ("No appointments yet. Book your first visit to get started.")
- **Success Messages**: Brief, celebratory, next-action oriented ("Booking confirmed! Check your email for details. Add to Calendar →")
- **AI Conversational Intake**: Warm, conversational, respectful ("Thanks! Now, can you tell me about any medications you're currently taking?")

### Content Guidelines
- **Headings**: Sentence case ("Your appointments" not "Your Appointments")
- **CTAs**: Action-oriented verbs ("Book appointment", "Upload documents", "Mark arrived")
- **Labels**: Concise, descriptive ("Expected wait time" not "EWT")
- **Placeholder Text**: Helpful examples ("john.doe@example.com" in email field; "e.g., Metformin 500mg twice daily" in medication field)
- **Medical Terminology**: Use patient-friendly language in patient portal ("reason for visit" vs "chief complaint"); clinical terminology acceptable in staff/admin views

---

## 8. Data & Edge Cases

### Data Scenarios
| Scenario | Description | Handling |
|----------|-------------|----------|
| No Data - New Patient | Patient logs in first time, no appointments/documents | Empty state: "Welcome! Let's book your first appointment" with prominent "Book Now" CTA |
| No Data - Staff View | Staff opens patient with no uploaded documents | Empty state in 360° View: "No clinical documents uploaded yet" with intake form data visible |
| First Use - Walk-In | New walk-in patient, no existing record | Staff creates temporary record; prompt to create full account post-visit |
| Large Data - 360° View | Patient with 50+ documents, 10-year history | Virtualized timeline scroll; pagination on document list; lazy load document previews |
| Slow Connection | API response >3s for slot availability | Skeleton screens for slot list; "Loading..." indicator; disable booking button until loaded |
| Offline | No network during booking | Offline banner: "Connection lost. Your booking will be saved when you reconnect." (Future: service worker caching) |
| High-Confidence AI Extraction | PDF extraction with 95% confidence scores | Data displayed normally; green checkmark icon; no staff intervention required |
| Low-Confidence AI Extraction | PDF extraction with <70% confidence | Yellow/red highlight; "Review Required" badge; staff must verify before use |
| Concurrent Booking Conflict | Two users book same slot simultaneously | Optimistic locking (DR-002); second user receives toast: "Slot just booked - please choose another" |
| Preferred Slot Swap Execution | UC-010 automated swap triggered | Email + SMS notification; calendar auto-updated; old slot released |

### Edge Cases
| Case | Screen(s) Affected | Solution |
|------|-------------------|----------|
| Long patient name (>50 chars) | All screens with patient name | Truncate to 40 chars + "..." with tooltip on hover showing full name |
| Missing profile image | SCR-001, SCR-007, SCR-009 | Display initials in colored circle (algorithm: firstName[0] + lastName[0]) |
| Form validation failure | SCR-005, SCR-006, SCR-011 | Inline error message below invalid field; error summary at top of form; focus on first error |
| Session timeout during form entry | All forms | Auto-save draft to secure storage every 30s; restore on re-login if <1 hour old (FR-029) |
| Calendar sync failure | SCR-002 | Non-blocking error; booking still successful; fallback: email .ics file; toast: "Calendar sync failed - use attached .ics file" |
| PDF generation failure | SCR-002 | Non-blocking; appointment confirmed; email sent with plain text details; admin notified; retry background job |
| AI extraction API unavailable | SCR-003 | Graceful degradation: "AI extraction temporarily unavailable. Document saved - staff will review manually." |
| Insurance database down | SCR-008 | Display warning: "Validation service unavailable. Proceed with manual verification"; flag for future re-check |
| Extremely long text in clinical notes | SCR-009 | Expand/collapse sections; "Read more" link after 300 characters; full text in modal |
| Browser back button during booking | SCR-002 | Preserve form state; re-validate slot availability; warn if slot no longer available |

---

## 9. Branding & Visual Direction

*Full design tokens defined in `designsystem.md`. High-level branding overview below.*

### Branding Assets
- **Logo**: "PulseCare" logotype with medical cross icon (placeholder - to be designed)
- **Icon Style**: Outlined icons (Lucide React library) - modern, clean, professional
- **Illustration Style**: Minimal spot illustrations for empty states (healthcare-themed, no photos to avoid HIPAA concerns)
- **Photography Style**: None (avoid patient imagery due to privacy constraints)
- **Color Psychology**: Blue (trust, healthcare standard), Green (success, health), Red (errors, conflicts, urgency)

### Visual Tone
- **Professional**: Sans-serif typography, clean layouts, ample whitespace, structured data tables
- **Trustworthy**: HIPAA-compliant aesthetic (secure lock icons, encrypted data badges, audit trail transparency)
- **Approachable**: Rounded corners (8px radius), friendly microcopy, conversational AI tone
- **Efficient**: High information density for staff dashboards; streamlined patient flows; keyboard shortcuts documented

---

## 10. Component Specifications

*Component library defined in `designsystem.md`. Requirements per screen listed below.*

### Component Library Reference
**Source**: `.propel/context/docs/designsystem.md` (Component Specifications section)
**Base**: shadcn/ui components customized via CSS variables and Tailwind config

### Required Components per Screen
| Screen ID | Components Required | Notes |
|-----------|---------------------|-------|
| SCR-001 | Card (3+), Button (2), Badge (2), Header (1), Sidebar (1) | Dashboard cards for upcoming appointments, quick actions, recent documents |
| SCR-002 | Calendar (1), Button (3), Card (1), Modal (1), Toast (2) | Slot selection calendar, preferred slot toggle, booking confirmation modal |
| SCR-003 | FileUpload (1), Button (2), Table (1), Badge (N), Skeleton (1) | Document upload dropzone, file list table, extraction status badges |
| SCR-004 | ChatBubble (N), TextArea (1), Button (2), Toggle (1), Avatar (2) | AI conversational interface, user/AI message bubbles, mode toggle |
| SCR-005 | TextField (8+), Select (3), Checkbox (3), Button (3), Toggle (1) | Traditional form inputs, demographics, medical history, mode toggle |
| SCR-006 | TextField (5), Button (3), Card (1), Table (1) | Walk-in patient search, intake form, queue display |
| SCR-007 | Table (1), Button (N), Badge (N), Search (1), DragHandle (N) | Arrival queue table, drag-drop reorder, status badges (Scheduled/Arrived/Late) |
| SCR-008 | TextField (2), Button (2), Alert (1), Badge (1) | Insurance name/ID inputs, validation button, result alert (success/warning/error) |
| SCR-009 | Tabs (1), Timeline (1), Card (N), Badge (N), Alert (1), Button (N), Accordion (N) | 360° view tabs (Timeline/Documents/Medications), conflict alert banner, confidence badges |
| SCR-010 | Table (1), Button (3), Search (1), Badge (N), Pagination (1) | User list table, create/deactivate/reset buttons, role badges |
| SCR-011 | TextField (4), Select (2), Checkbox (N), Button (3) | User creation/edit form, role assignment, permission checkboxes |
| SCR-012 | Table (1), Search (1), Pagination (1), Select (1), DatePicker (2) | Audit log table, search/filter, date range picker |
| SCR-013 | TextField (2), Button (1), Link (2), Card (1) | Email/password inputs, login button, forgot password / create account links |
| SCR-014 | TextField (1), Button (1), Card (1) | Email input for reset request |
| SCR-015 | TextField (2), Button (1), Card (1) | New password + confirm password inputs |
| SCR-016 | Table (1), Badge (N), Button (N), Pagination (1) | Appointment history table, status badges, reschedule/cancel buttons |
| SCR-017 | Card (N), Badge (N), Button (N), Accordion (N), Select (1) | ICD-10/CPT code suggestions, confidence badges, accept/reject buttons |
| SCR-018 | TextField (N), Toggle (N), Button (2) | System configuration settings, feature flags |

### Component Summary by Category
| Category | Components | Variants | Usage |
|----------|------------|----------|-------|
| Actions | Button, IconButton, Link, FAB | Primary (blue), Secondary (gray), Destructive (red), Ghost (transparent) x S/M/L x States (Default/Hover/Focus/Active/Disabled/Loading) | CTAs, form submissions, navigation |
| Inputs | TextField, TextArea, Select, Checkbox, Radio, Toggle, FileUpload | States (Default/Focus/Error/Disabled) x Sizes (S/M/L) | Form data entry, search, filters |
| Navigation | Header, Sidebar, Tabs, BottomNav, Breadcrumb | Desktop (persistent sidebar), Mobile (bottom nav), Responsive breakpoints | App navigation, section switching |
| Content | Card, ListItem, Table, Timeline, Accordion, Badge | Content variants (simple, with image, with actions), Elevation (flat, raised) | Data display, grouping, status indicators |
| Feedback | Modal, Drawer, Toast, Alert, Skeleton, Spinner, ProgressBar | Types (Info/Success/Warning/Error) x Positions (Top/Center/Bottom) | User notifications, loading states, errors |
| Layout | Container, Grid, Stack, Divider | Responsive breakpoints, spacing variants | Page structure, whitespace management |

### Component Constraints
- **Single Source**: Use only shadcn/ui components; no ad-hoc custom components without design system update
- **State Coverage**: All interactive components must support all defined states (Default, Hover, Focus, Active, Disabled, Loading, Error)
- **Accessibility**: Components must pass axe-core validation; ARIA attributes required
- **Naming Convention**: `<ComponentName>` (e.g., `Button`, `TextField`) with variant props (e.g., `<Button variant="primary" size="lg" />`)
- **Dark Mode**: All components must support light/dark theme via CSS variables (future requirement - not Phase 1)

---

## 11. Prototype Flows

*Flows derived from use cases UC-001 to UC-010. Each flow notes which personas it covers.*

### Flow: FL-001 - Authentication & Onboarding
**Flow ID**: FL-001  
**Derived From**: FR-028 (RBAC), UC-009 (user creation)  
**Personas Covered**: All (Patient, Staff, Admin)  
**Description**: User authenticates and accesses role-appropriate dashboard

#### Flow Sequence
```text
1. Entry: SCR-013 Login / Default
   - User enters email + password
   - Clicks "Sign In" button
   |
   v
2. Loading: SCR-013 Login / Loading
   - Button shows spinner
   - "Signing in..." text
   |
   v
3. Decision Point:
   +-- Success → Route by role:
   |   +-- Patient → SCR-001 Patient Dashboard / Default
   |   +-- Staff → SCR-007 Arrival Management / Default
   |   +-- Admin → SCR-010 User Management / Default
   |
   +-- Error (invalid credentials) → SCR-013 Login / Error
       - "Invalid email or password. Try again or reset password." with inline error
       - Focus returns to email field

Alternative Path: First-Time User Activation (from UC-009 step 12)
1. User receives activation email with link
2. Click link → SCR-015 Account Activation / Default
3. Enter new password (2 fields: password + confirm)
4. Click "Activate Account" → Loading state
5. Success → SCR-013 Login / Default with success toast "Account activated! Please log in."
```

#### Required Interactions
- **Keyboard Navigation**: Tab through email → password → submit button; Enter submits form
- **Error Recovery**: "Forgot password?" link → SCR-014 Password Reset
- **Session Management**: Auto-logout after 15 min inactivity (UXR-603)

---

### Flow: FL-002 - Patient Appointment Booking (Simple Path)
**Flow ID**: FL-002  
**Derived From**: UC-001 (book available slot)  
**Personas Covered**: Patient  
**Description**: Patient navigates to booking interface, selects available slot, confirms booking, receives confirmation

#### Flow Sequence
```text
1. Entry: SCR-001 Patient Dashboard / Default
   - Patient clicks "Book Appointment" button
   |
   v
2. SCR-002 Slot Selection / Default
   - System displays calendar with available slots (green)
   - Booked slots grayed out
   - Patient selects desired slot
   |
   v
3. SCR-002 Slot Selection / Slot Selected
   - Selected slot highlights with blue border
   - "Confirm Booking" button enabled
   - Patient reviews slot details (time, provider, location)
   - Patient clicks "Confirm Booking"
   |
   v
4. Modal: Booking Confirmation / Default
   - "Confirm your appointment on [Date] at [Time] with [Provider]?"
   - "Confirm" and "Cancel" buttons
   - Patient clicks "Confirm"
   |
   v
5. Modal: Booking Confirmation / Loading
   - Spinner displayed
   - "Booking your appointment..." text
   - Background API call to create appointment record
   |
   v
6. Decision Point:
   +-- Success → SCR-001 Patient Dashboard / Default
   |   - Success toast: "Appointment booked! Confirmation email sent."
   |   - Booked appointment appears in "Upcoming Appointments" card
   |   - Second toast (5s later): "Generating PDF confirmation..." (FR-007)
   |   - Third toast (10s later): "Adding to your calendar..." (FR-005/006)
   |
   +-- Error (slot conflict) → SCR-002 Slot Selection / Error
       - Modal closes
       - Error toast: "This slot was just booked. Please select another time." (UXR-605)
       - Selected slot now grayed out
       - Return to step 2
```

#### Required Interactions
- **Real-Time Slot Updates**: WebSocket or polling every 5s to refresh availability (FR-001)
- **Optimistic UI**: Slot changes to "pending" immediately on click before API confirmation
- **Calendar Sync**: Non-blocking; toast notification on success/failure (FR-005/006)

---

### Flow: FL-003 - Patient Appointment Booking with Preferred Slot Swap
**Flow ID**: FL-003  
**Derived From**: UC-002 (preferred slot swap)  
**Personas Covered**: Patient  
**Description**: Patient books available slot while indicating preference for currently unavailable slot, system auto-swaps when preferred slot opens

#### Flow Sequence
```text
1-3. [Same as FL-002 steps 1-3: Navigate to booking, select available slot]
   |
   v
4. SCR-002 Slot Selection / Preferred Slot Prompt
   - After selecting available slot, system displays:
   - "Would you prefer a different time? We'll automatically move you if it becomes available."
   - Calendar remains visible with current selection highlighted
   - Patient clicks on preferred (currently grayed out) slot
   |
   v
5. SCR-002 Slot Selection / Dual Selection
   - Primary slot: [Date1 Time1] (green highlight)
   - Preferred slot: [Date2 Time2] (blue star icon overlay)
   - "Book with Preferred Swap" button enabled
   - Patient clicks "Book with Preferred Swap"
   |
   v
6. Modal: Swap Booking Confirmation / Default
   - "You'll be booked for [Primary Slot] but automatically moved to [Preferred Slot] if it opens."
   - "Sounds good" and "Cancel" buttons
   - Patient clicks "Sounds good"
   |
   v
7. [Same as FL-002 steps 5-6: Loading → Success/Error]
   - Success toast now says: "Booked for [Primary Slot]. We'll notify you if [Preferred Slot] becomes available!"

[Time passes - UC-010 automated swap executes]

8. Background Process (no screen change)
   - System detects preferred slot availability
   - System auto-swaps appointment
   - System sends email + SMS notification
   |
   v
9. Patient Next Login: SCR-001 Patient Dashboard / Default
   - "Upcoming Appointments" card now shows [Preferred Slot]
   - Info badge: "Moved from [Primary Slot]"
   - Patient received email: "Great news! Your appointment moved to your preferred time."
```

#### Required Interactions
- **Visual Differentiation**: Primary slot (green), Preferred slot (blue star), Unavailable slot (gray)
- **Confirmation Clarity**: Modal clearly explains auto-swap behavior
- **Notification**: Email + SMS when swap occurs (UC-010 step 8)

---

### Flow: FL-004 - Patient Intake (AI Conversational Mode)
**Flow ID**: FL-004  
**Derived From**: UC-003 (AI conversational intake)  
**Personas Covered**: Patient  
**Description**: Patient completes intake questionnaire via conversational AI interface with ability to toggle to manual form anytime

#### Flow Sequence
```text
1. Entry: SCR-001 Patient Dashboard / Default
   - Patient clicks "Complete Intake Form" card
   - System prompts: "How would you like to complete your intake?"
   - Patient clicks "AI Assisted (Conversational)"
   |
   v
2. SCR-004 AI Conversational Intake / Default
   - Chat interface with AI avatar on left
   - AI: "Hi! I'm here to help you with your intake. Let's start with some basic information. What's your date of birth?"
   - Text input field at bottom
   - Toggle switch in header: "AI Intake" (active, blue) | "Manual Form" (inactive, gray)
   |
   v
3. SCR-004 AI Conversational Intake / Conversation In Progress
   - Patient types: "March 15, 1985"
   - AI: "Thanks! And what's the main reason for your visit today?"
   - Patient types: "I've been having headaches for the past two weeks"
   - AI: "I'm sorry to hear that. Can you describe the headaches? Are they constant or do they come and go?"
   - [Conversation continues through medical history, medications, allergies]
   |
   v
4. SCR-004 AI Conversational Intake / Confirmation
   - AI: "Great! Let me confirm what I understood:"
   - Structured data summary displayed in card format:
     - DOB: 03/15/1985 ✓
     - Reason: Recurring headaches (2 weeks) ✓
     - Medications: Ibuprofen as needed ✓
     - Allergies: None ✓
   - AI: "Does this look correct? You can edit any item by clicking it."
   - Patient reviews and clicks "Looks good"
   |
   v
5. SCR-004 AI Conversational Intake / Submitting
   - "Saving your intake form..." spinner
   |
   v
6. Success → SCR-001 Patient Dashboard / Default
   - Success toast: "Intake complete! We'll see you soon."
   - "Complete Intake Form" card now shows green checkmark: "Intake Completed"

Alternative Path: Toggle to Manual Form (FR-011)
3a. During conversation, patient clicks toggle switch
   |
   v
3b. Modal: "Switch to manual form? Your progress will be saved."
    - Patient clicks "Switch"
   |
   v
3c. SCR-005 Manual Form Intake / Default
    - Form pre-populated with data extracted from conversation
    - DOB field: "03/15/1985" (already filled)
    - Reason for visit: "Recurring headaches (2 weeks)" (already filled)
    - Patient continues with remaining fields
```

#### Required Interactions
- **Toggle Visibility**: Persistent in header during entire intake flow (UXR-103)
- **Data Preservation**: All extracted data carries over when toggling (FR-011)
- **Inline Editing**: Click any confirmed data item to re-enter in chat
- **Save Draft**: Auto-save every 30s; "Save & Continue Later" button always visible

---

### Flow: FL-005 - Patient Intake (Manual Form Mode)
**Flow ID**: FL-005  
**Derived From**: UC-004 (manual form intake)  
**Personas Covered**: Patient  
**Description**: Patient completes intake via traditional form with real-time validation and ability to toggle to AI mode

#### Flow Sequence
```text
1. Entry: SCR-001 Patient Dashboard / Default
   - Patient clicks "Complete Intake Form" card
   - System prompts: "How would you like to complete your intake?"
   - Patient clicks "Manual Form"
   |
   v
2. SCR-005 Manual Form Intake / Default
   - Traditional form with sections:
     - Demographics (Name, DOB, Phone, Email)
     - Medical History (checkboxes for conditions)
     - Current Medications (text area + "Add Medication" button)
     - Allergies (text area)
     - Reason for Visit (text area)
   - Toggle switch in header: "AI Intake" (inactive) | "Manual Form" (active, blue)
   - Progress indicator: "Demographics: 4/5 fields complete"
   |
   v
3. SCR-005 Manual Form Intake / Validation
   - Patient enters invalid email: "john.doeexample.com"
   - On blur: Red border + error text below field: "Please enter valid email (e.g., john.doe@example.com)"
   - "Continue" button disabled until all errors resolved
   - Patient corrects to "john.doe@example.com"
   - Green checkmark appears next to field
   - "Continue" button enabled
   |
   v
4. SCR-005 Manual Form Intake / Review
   - Patient clicks "Continue" after completing all fields
   - System displays summary page with all entered data
   - "Edit" links next to each section
   - Patient reviews and clicks "Submit Intake"
   |
   v
5. SCR-005 Manual Form Intake / Submitting
   - "Saving your intake form..." spinner
   - Button disabled during submission
   |
   v
6. Success → SCR-001 Patient Dashboard / Default
   - Success toast: "Intake complete! You're all set."
   - "Complete Intake Form" card updated

Alternative Path: Toggle to AI Intake (FR-011)
3a. Patient clicks toggle switch during form entry
   |
   v
3b. [Same modal as FL-004 step 3b]
   |
   v
3c. SCR-004 AI Conversational Intake / Default
    - AI: "I see you've already filled out some information. Let's continue from where you left off."
    - AI resumes conversation from last incomplete section
```

#### Required Interactions
- **Inline Validation**: Real-time feedback on blur for each field (UXR-504)
- **Error Summary**: If patient tries to submit with errors, scroll to first error and display summary banner at top
- **Auto-Save**: Draft saved every 30s to prevent data loss (FR-029 session timeout requirement)

---

### Flow: FL-006 - Staff Walk-In Check-In
**Flow ID**: FL-006  
**Derived From**: UC-005 (walk-in appointment)  
**Personas Covered**: Staff  
**Description**: Staff member creates appointment for patient who arrives without prior booking, optionally creates account

#### Flow Sequence
```text
1. Entry: SCR-007 Arrival Management / Default
   - Staff clicks "New Walk-In" button in header
   |
   v
2. SCR-006 Walk-In Management / Search Existing Patient
   - Modal displays: "Search for existing patient"
   - Search fields: Name, DOB, Phone
   - Staff enters patient name: "Jane Smith"
   - Staff clicks "Search"
   |
   v
3. Decision Point:
   +-- Existing patient found → SCR-006 Walk-In Management / Patient Found
   |   - Patient record displayed with demographics
   |   - "Create Walk-In Appointment" button
   |   - Skip to step 5
   |
   +-- No match found → SCR-006 Walk-In Management / New Patient Form
       - "No existing patient found. Create new record?"
       - Continue to step 4

4. SCR-006 Walk-In Management / New Patient Form
   - Staff enters: Name, DOB, Phone, Reason for Visit
   - "Save & Create Appointment" button
   |
   v
5. SCR-006 Walk-In Management / Appointment Details
   - Staff selects provider from dropdown
   - Staff selects "Add to Queue" (no specific time) or "Schedule Time" (if slot available)
   - Staff clicks "Create Appointment"
   |
   v
6. SCR-006 Walk-In Management / Account Creation Prompt
   - Modal: "Would you like to create a portal account for this patient?"
   - Staff asks patient verbally
   - Decision:
     +-- Yes → Staff enters patient email → System creates account + sends activation email
     +-- No / Later → Skip account creation
   |
   v
7. Success → SCR-007 Arrival Management / Default
   - Walk-in appointment appears in queue with "Pending" status
   - Staff clicks patient row → "Check In" button
   - Status updates to "Arrived"
```

#### Required Interactions
- **Duplicate Prevention**: Search validates existing patients before creating new records
- **Queue Management**: Walk-in added to same-day queue with estimated wait time (UXR-104)
- **Optional Account**: Account creation non-blocking; can be done post-visit

---

### Flow: FL-007 - Staff Patient Arrival Management
**Flow ID**: FL-007  
**Derived From**: UC-006 (mark patient arrived)  
**Personas Covered**: Staff  
**Description**: Staff checks in patient upon arrival for scheduled appointment, updates queue, notifies provider

#### Flow Sequence
```text
1. Entry: SCR-007 Arrival Management / Default
   - System displays today's appointment list
   - Columns: Time, Patient Name, Provider, Status, Actions
   - Status badges: Scheduled (gray), Arrived (green), Late (red), In Progress (blue)
   |
   v
2. SCR-007 Arrival Management / Search
   - Patient arrives at front desk
   - Staff uses search field: types "Jane Smith"
   - Table filters to matching appointments
   |
   v
3. SCR-007 Arrival Management / Patient Row Selected
   - Staff clicks patient row to expand
   - Appointment details displayed: Time, Provider, Reason, Intake Status
   - "Check In" button visible
   |
   v
4. SCR-007 Arrival Management / Identity Verification
   - Staff verbally verifies: "Jane Smith, DOB 03/15/1985?"
   - Patient confirms
   - Staff clicks "Check In"
   |
   v
5. SCR-007 Arrival Management / Checking In (Loading)
   - Button shows spinner
   - "Checking in..." text
   |
   v
6. Success → SCR-007 Arrival Management / Updated Queue
   - Patient status updates from "Scheduled" to "Arrived" (green badge)
   - Timestamp displayed: "Checked in at 2:15 PM"
   - Patient moves to top of queue (if using drag-drop reordering)
   - Toast: "Jane Smith checked in. Provider notified."
   - Provider view updates with notification (not shown in this flow)

Edge Case: Late Arrival (>15 min after scheduled time)
6a. System detects late arrival
    - Status badge shows "Late" (red)
    - Toast warning: "Jane Smith is 20 min late. Confirm with provider before check-in?"
    - Staff clicks "Confirm" → Proceed with check-in
```

#### Required Interactions
- **Real-Time Updates**: Queue refreshes automatically when patients arrive (polling or WebSocket)
- **Drag-Drop Reorder**: Staff can manually reorder queue by dragging rows (UXR-104)
- **Provider Notification**: Background notification sent to provider dashboard (not shown)

---

### Flow: FL-008 - Staff 360-Degree Clinical Data Review
**Flow ID**: FL-008  
**Derived From**: UC-008 (360-degree patient view)  
**Personas Covered**: Staff  
**Description**: Staff reviews consolidated patient clinical data from multiple documents, verifies AI extractions, resolves conflicts

#### Flow Sequence
```text
1. Entry: SCR-007 Arrival Management / Default
   - Staff clicks patient name link in arrival queue
   |
   v
2. SCR-009 360-Degree Patient View / Timeline Tab (Default)
   - Patient demographics header: Name, DOB, Contact
   - Tab navigation: Timeline | Documents | Medications | Conflicts (Red badge: "2")
   - Timeline displays chronological entries:
     - Intake Form (Jan 15, 2024)
     - Lab Report Uploaded (Jan 20, 2024) - AI Extracted
     - Radiology Report Uploaded (Jan 25, 2024) - AI Extracted
     - Previous Visit Note (Jan 10, 2024)
   |
   v
3. SCR-009 360-Degree Patient View / Documents Tab
   - Table: Document Name, Type, Upload Date, Extraction Status
   - Row 1: Lab_Results_Jan2024.pdf, Lab Report, 01/20/24, [Badge: High Confidence 95%] (green)
   - Row 2: Radiology_Report.pdf, Imaging, 01/25/24, [Badge: Review Required 68%] (yellow)
   - Staff clicks Row 2 (low confidence)
   |
   v
4. SCR-009 360-Degree Patient View / Document Review Modal
   - Split view: PDF preview (left) | Extracted data (right)
   - Extracted data highlighted on PDF
   - Right panel shows:
     - Finding: "Mild degenerative changes L4-L5" [Badge: 68% confidence] (yellow)
     - Staff can edit extracted text inline
     - "Confirm" or "Reject" buttons
   - Staff verifies against PDF source
   - Staff clicks "Confirm"
   |
   v
5. SCR-009 360-Degree Patient View / Documents Tab (Updated)
   - Row 2 badge now shows: [Badge: Verified by Staff] (green checkmark)
   - Confidence score updated to 100%
   |
   v
6. SCR-009 360-Degree Patient View / Conflicts Tab
   - Red alert banner at top: "2 critical conflicts detected. Resolve before proceeding."
   - Conflict 1:
     - Type: Medication Discrepancy
     - Source A (Intake Form): "Metformin 500mg twice daily"
     - Source B (Lab Report): "Metformin 1000mg once daily"
     - Actions: [Select Source A] [Select Source B] [Flag for Provider]
   - Conflict 2:
     - Type: Allergy Mismatch
     - Source A (Previous Visit): "No known allergies"
     - Source B (Uploaded Document): "Penicillin allergy"
     - Actions: [Select Source A] [Select Source B] [Flag for Provider]
   |
   v
7. SCR-009 360-Degree Patient View / Conflict Resolution
   - Staff reviews conflict 1
   - Staff clicks "Select Source B" (Lab Report more recent)
   - Modal: "Confirm selection? This will update patient medication record."
   - Staff clicks "Confirm"
   - Conflict 1 removed from list
   |
   v
8. SCR-009 360-Degree Patient View / Conflict Resolution (Escalation)
   - Staff reviews conflict 2 (allergy is critical)
   - Staff clicks "Flag for Provider"
   - Modal: "Add note for provider?"
   - Staff types: "Patient verbally confirmed penicillin allergy. Update record."
   - Staff clicks "Submit"
   - Conflict 2 badge changes to "Pending Provider Review" (yellow)
   |
   v
9. SCR-009 360-Degree Patient View / Mark Ready
   - All conflicts resolved or escalated
   - Alert banner changes to green: "Patient ready for provider review"
   - "Mark as Ready" button enabled
   - Staff clicks "Mark as Ready"
   |
   v
10. Success → SCR-007 Arrival Management / Default
    - Patient status updates to "Ready for Provider" (blue badge)
    - Toast: "Jane Smith marked as ready. Data review complete."
```

#### Required Interactions
- **PDF Viewer**: Inline PDF rendering with extracted data highlighted (FR-022)
- **Conflict Alert**: Red banner cannot be dismissed until resolution (UXR-604)
- **Audit Trail**: All verifications and conflict resolutions logged (FR-031)

---

### Flow: FL-009 - Admin User Account Management
**Flow ID**: FL-009  
**Derived From**: UC-009 (admin manages user accounts)  
**Personas Covered**: Admin  
**Description**: Admin creates new user account, assigns role, sends activation email; also covers deactivation and password reset

#### Flow Sequence (Create User)
```text
1. Entry: SCR-010 User Management List / Default
   - Table displays: Name, Email, Role, Status, Last Login, Actions
   - Admin clicks "Create New User" button
   |
   v
2. SCR-011 Create User Form / Default
   - Fields: First Name, Last Name, Email, Role (dropdown: Patient/Staff/Admin), Permissions (checkboxes if Staff/Admin)
   - Admin enters:
     - Name: "John Doe"
     - Email: "john.doe@clinic.com"
     - Role: "Staff"
     - Permissions: [✓] Manage Arrivals, [✓] Review Clinical Data, [ ] Manage Users
   |
   v
3. SCR-011 Create User Form / Validation
   - Admin clicks "Create User"
   - System validates email uniqueness
   - Decision:
     +-- Email available → Continue to step 4
     +-- Email exists → Inline error: "User with this email already exists" → Return to step 2
   |
   v
4. SCR-011 Create User Form / Submitting
   - "Creating user..." spinner on button
   - System creates user account with temporary password
   - System sends activation email to user
   |
   v
5. Success → SCR-010 User Management List / Default
   - Success toast: "User created. Activation email sent to john.doe@clinic.com"
   - New user appears in table with Status: "Pending Activation" (yellow badge)
```

#### Flow Sequence (Deactivate User)
```text
1. SCR-010 User Management List / Default
   - Admin clicks "..." menu on user row
   - Dropdown shows: Edit, Deactivate, Reset Password
   - Admin clicks "Deactivate"
   |
   v
2. Modal: Confirm Deactivation
   - "Deactivate Jane Smith? User will be logged out immediately and cannot log in."
   - Text area: "Reason for deactivation (required)"
   - Admin types: "Employee resigned"
   - Admin clicks "Deactivate"
   |
   v
3. Success → SCR-010 User Management List / Default
   - User status changes to "Inactive" (red badge)
   - Toast: "Jane Smith deactivated"
```

#### Flow Sequence (Reset Password)
```text
1. SCR-010 User Management List / Default
   - Admin clicks "..." menu on user row
   - Admin clicks "Reset Password"
   |
   v
2. Modal: Confirm Password Reset
   - "Send password reset email to jane.smith@clinic.com?"
   - Admin clicks "Send Email"
   |
   v
3. Success → SCR-010 User Management List / Default
   - Toast: "Password reset email sent to jane.smith@clinic.com"
```

#### Required Interactions
- **Role-Based Permissions**: Admin can assign granular permissions based on role (FR-028)
- **Audit Logging**: All account actions logged with admin ID and reason (FR-031)
- **Email Notifications**: Activation and password reset emails sent automatically

---

## 12. Screen State Specifications

*Each screen MUST define 5 states: Default, Loading, Empty, Error, Validation. Additional states per screen requirements.*

### SCR-001: Patient Dashboard

**Default State:**
- Header: "Welcome back, [Patient Name]"
- Three main cards:
  1. "Upcoming Appointments" card with next appointment details or empty state
  2. "Intake Form Status" card with completion status or "Start Intake" CTA
  3. "Your Documents" card with count + "Upload New" button
- Primary CTA: "Book Appointment" button (prominent, top-right)
- Navigation sidebar visible (desktop) or bottom nav (mobile)

**Loading State:**
- Skeleton screens for three cards (shimmer effect)
- Header visible immediately (no skeleton)
- Loading indicators for appointment count, document count

**Empty State:**
- "Upcoming Appointments" card: "No appointments scheduled. Book your first visit!" with "Book Now" CTA
- "Your Documents" card: "No documents uploaded yet" with upload icon

**Error State:**
- If dashboard data fails to load: Error alert banner at top: "Unable to load dashboard. [Retry]"
- Cards show last cached data with warning badge "Data may be outdated"

**Validation State:** N/A (no form inputs)

---

### SCR-002: Appointment Slot Selection & Booking

**Default State:**
- Calendar view with month/week selector
- Available slots: Green background, clickable
- Booked slots: Gray background, disabled, "Unavailable" tooltip
- Selected slot: Blue border highlight
- "Confirm Booking" button: Disabled until slot selected
- "Preferred Slot?" link below calendar (optional, triggers UC-002 flow)

**Loading State:**
- Calendar skeleton on initial load
- "Loading available slots..." text
- Slot refresh: Spinner overlay on calendar, slots temporarily disabled

**Empty State:**
- "No appointments available for (Selected Date)" with calendar icon
- "Try a different date or provider" suggestion
- "Join Waitlist" button (FR-004)

**Error State:**
- API failure: Error banner: "Unable to load slots. [Retry]" with last cached slots displayed (warning badge)
- Booking failure (slot conflict): Toast: "This slot was just booked. Please select another." (UXR-605)

**Validation State:**
- Booking confirmation modal open (counts as validation state)
- Modal shows: Date, Time, Provider, Location, "Confirm" and "Cancel" buttons

**Additional States:**
- **Slot Selected State**: Slot highlighted, "Confirm Booking" button enabled, slot details card displayed below calendar
- **Booking in Progress State**: Modal overlay with spinner, "Booking your appointment..." text

---

### SCR-003: Clinical Document Upload

**Default State:**
- Drag-and-drop upload zone: "Drag PDF files here or click to browse"
- Accepted formats badge: "PDF only, max 10MB"
- Uploaded documents table below: Document Name, Upload Date, Extraction Status, Actions (View/Delete)

**Loading State:**
- File upload in progress: Progress bar with percentage (0-100%)
- "Uploading [Filename]..." text
- Upload zone disabled during upload

**Empty State:**
- "No documents uploaded yet" with upload icon
- "Upload your medical records for better care" subtitle

**Error State:**
- Upload failure: Error toast: "Upload failed. [Retry]" with reason (file too large, unsupported format, network error)
- Invalid file type: Inline error below upload zone: "Only PDF files are supported"
- Extraction failure: Document row shows red badge "Extraction Failed" with "Staff will review manually" tooltip

**Validation State:**
- File validation on drop: Checks file type and size before upload starts
- Invalid files rejected immediately with inline error message

**Additional States:**
- **AI Extraction in Progress State**: Document row shows spinner + "Extracting data..." (FR-022 30s processing)
- **Extraction Complete State**: Document row shows green badge "High Confidence 95%" or yellow badge "Review Required 68%"

---

### SCR-004: AI Conversational Intake

**Default State:**
- Chat interface with AI avatar (left) and user avatar (right)
- AI greeting message: "Hi! I'm here to help with your intake. Let's start - what's your date of birth?"
- Text input field at bottom with placeholder: "Type your response..."
- Send button (arrow icon)
- Toggle switch in header: "AI Intake" (blue, active) | "Manual Form" (gray, inactive)

**Loading State:**
- AI "typing" indicator: Three animated dots in AI message bubble
- User message sent: Message appears immediately with "Sending..." indicator until AI response loads

**Empty State:** N/A (conversation always starts with AI greeting)

**Error State:**
- AI API failure: Error message in chat: "I'm having trouble processing your response. Could you try again?" with "Retry" button
- Network error: Offline banner at top: "Connection lost. Your responses are saved and will send when reconnected."

**Validation State:**
- Data confirmation screen: AI displays structured data summary with "Does this look correct?" prompt
- Each data field has inline "Edit" button
- "Looks good" and "Start over" buttons

**Additional States:**
- **Conversation In Progress State**: Multiple message bubbles stacked, scroll-to-bottom behavior, auto-scroll on new message
- **Save Draft State**: "Auto-saved" indicator in header (appears after 30s of inactivity)

---

### SCR-005: Manual Form Intake

**Default State:**
- Traditional form with sections (collapsed/expanded accordion):
  1. Demographics (Name, DOB, Phone, Email)
  2. Medical History (checkboxes + "Other" text field)
  3. Current Medications (text area + "Add Medication" button for multiple entries)
  4. Allergies (text area)
  5. Reason for Visit (text area)
- Progress indicator at top: "3 of 5 sections complete"
- Toggle switch in header: "AI Intake" (gray, inactive) | "Manual Form" (blue, active)
- "Save Draft" and "Continue" buttons at bottom

**Loading State:**
- Form submission: "Saving your intake..." spinner on "Submit" button
- Button disabled during submission

**Empty State:**
- All fields empty with placeholder text (e.g., "john.doe@example.com" in email field)
- Sections collapsed by default; user expands to fill

**Error State:**
- Validation errors: Red border on invalid fields with inline error text below (e.g., "Please enter valid email")
- Error summary banner at top: "Please fix 3 errors before continuing" with links to scroll to errors
- Submission failure: Error toast: "Unable to save intake. [Retry]"

**Validation State:**
- Real-time validation on blur for each field (UXR-504)
- Valid fields: Green checkmark icon on right side of input
- Invalid fields: Red border + error message below

**Additional States:**
- **Review State**: After clicking "Continue", displays read-only summary with "Edit" links per section and "Submit Intake" button
- **Save Draft State**: "Auto-saved" indicator appears in header after 30s inactivity

---

### SCR-009: 360-Degree Patient View / Clinical Review

**Default State:**
- Patient header: Name, DOB, Contact, MRN
- Tab navigation: Timeline (active) | Documents | Medications | Conflicts
- **Timeline Tab**: Chronological entries with date, source, and summary (e.g., "Intake Form - Completed Jan 15, 2024")
- No conflict alert banner (only appears if conflicts detected)
- "Mark as Ready" button (disabled until all conflicts resolved)

**Loading State:**
- Tab content skeleton screens (shimmer effect)
- "Loading patient data..." text
- Patient header loads immediately (no skeleton)

**Empty State:**
- **Timeline Tab**: "No clinical data available yet" with upload icon
- **Documents Tab**: "No documents uploaded" with "Patient can upload from their portal" subtitle
- **Medications Tab**: "No medications recorded" with "Add manually" CTA (staff can add)
- **Conflicts Tab**: "No conflicts detected" with green checkmark

**Error State:**
- Data load failure: Error alert banner: "Unable to load patient data. [Retry]"
- AI extraction error: Document row shows red badge "Extraction Failed" with tooltip: "Staff review required"

**Validation State:** N/A (this is a review screen, not a form)

**Additional States:**
- **Conflict Detected State**: Red alert banner at top: "2 critical conflicts detected. Resolve before proceeding." Conflicts tab badge shows count (red)
- **Low-Confidence Extraction State**: Data items with <70% confidence highlighted yellow with "Review Required" badge and "Verify" button
- **Document Review Modal State**: Split-screen PDF viewer with extracted data overlay (triggered from Documents tab)

---

## 13. Export Requirements

### JPG Export Settings
| Setting | Value |
|---------|-------|
| Format | JPG |
| Quality | High (85%) |
| Scale - Mobile | 2x (640px viewport equivalent) |
| Scale - Tablet | 2x (1536px viewport equivalent) |
| Scale - Desktop | 2x (2560px viewport equivalent) |
| Color Profile | sRGB |
| Background | White (#FFFFFF) for screens, Transparent for components |

### Export Naming Convention
`PulseCare_Platform__<Platform>__<ScreenName>__<State>__v<Version>.jpg`

**Examples:**
- `PulseCare_Platform__Mobile__PatientDashboard__Default__v1.jpg`
- `PulseCare_Platform__Desktop__SlotSelection__Error__v1.jpg`
- `PulseCare_Platform__Tablet__ArrivalManagement__Loading__v1.jpg`

### Export Manifest
| Screen ID | Screen Name | States to Export | Platforms | Total JPGs |
|-----------|-------------|------------------|-----------|------------|
| SCR-001 | Patient Dashboard | Default, Loading, Empty | Mobile, Desktop | 6 |
| SCR-002 | Slot Selection | Default, Loading, Empty, Error, Validation, SlotSelected, BookingInProgress | Mobile, Desktop | 14 |
| SCR-003 | Document Upload | Default, Loading, Empty, Error, ExtractionInProgress | Mobile, Desktop | 10 |
| SCR-004 | AI Intake | Default, Loading, Error, Validation, ConversationInProgress | Mobile, Desktop | 10 |
| SCR-005 | Manual Intake | Default, Loading, Empty, Error, Validation, Review | Mobile, Desktop | 12 |
| SCR-006 | Walk-In Management | Default, Loading, Error, Validation | Tablet, Desktop | 8 |
| SCR-007 | Arrival Management | Default, Loading, Empty, Error, IdentityVerification | Tablet, Desktop | 10 |
| SCR-008 | Insurance Validation | Default, Loading, Error, Validation | Tablet, Desktop | 8 |
| SCR-009 | 360-Degree View | Default, Loading, Empty, Error, ConflictDetected, DocumentReviewModal | Tablet, Desktop | 12 |
| SCR-010 | User Management List | Default, Loading, Empty, Error | Desktop | 4 |
| SCR-011 | Create/Edit User | Default, Loading, Error, Validation | Desktop | 4 |
| SCR-012 | Audit Log Viewer | Default, Loading, Empty, Error | Desktop | 4 |
| SCR-013 | Login | Default, Loading, Error, Validation | Mobile, Desktop | 8 |
| SCR-014 | Password Reset | Default, Loading, Error, Validation | Mobile, Desktop | 8 |
| SCR-015 | Account Activation | Default, Loading, Error, Validation | Mobile, Desktop | 8 |
| SCR-016 | Appointment History | Default, Loading, Empty, Error | Mobile, Desktop | 8 |
| SCR-017 | Coding Review | Default, Loading, Empty, Error, Validation | Tablet, Desktop | 10 |
| SCR-018 | System Config | Default, Loading, Error, Validation | Desktop | 4 |

### Total Export Count
- **Screens**: 18
- **Average states per screen**: 5-7 (includes required 5 + additional states)
- **Platforms**: Mobile (320-767px), Tablet (768-1023px), Desktop (1024px+)
- **Total JPGs**: ~148

---

## 14. Figma File Structure

### Page Organization
```text
PulseCare Platform - Figma File
+-- 00_Cover
|   +-- Project overview: Unified Patient Access & Clinical Intelligence Platform
|   +-- Version: 1.0 (Phase 1 MVP)
|   +-- Stakeholders: Product, Design, Engineering, Compliance
|   +-- Last updated: [Date]
+-- 01_Foundations
|   +-- Color Tokens (Light Mode only - Phase 1)
|   |   +-- Primary: Blue scale (50-900)
|   |   +-- Semantic: Success (green), Warning (yellow), Error (red), Info (blue)
|   |   +-- Neutrals: Gray scale (50-900)
|   +-- Typography Scale
|   |   +-- Font Family: Inter (primary), JetBrains Mono (code)
|   |   +-- Sizes: 12px (caption) → 48px (h1)
|   |   +-- Weights: 400 (regular), 500 (medium), 600 (semibold), 700 (bold)
|   +-- Spacing Scale
|   |   +-- Base: 4px grid (4, 8, 12, 16, 20, 24, 32, 40, 48, 64, 80, 96)
|   +-- Border Radius Tokens
|   |   +-- sm: 4px, md: 8px, lg: 12px, xl: 16px, full: 9999px
|   +-- Elevation/Shadows
|   |   +-- sm, md, lg, xl (from Tailwind CSS)
|   +-- Grid Definitions
|       +-- Mobile: 16px margin, 8px gutter, fluid columns
|       +-- Tablet: 24px margin, 16px gutter, 8 columns
|       +-- Desktop: 40px margin, 24px gutter, 12 columns
+-- 02_Components
|   +-- C/Actions/
|   |   +-- Button (Primary, Secondary, Destructive, Ghost) x (S, M, L) x States
|   |   +-- IconButton (same variants)
|   |   +-- Link (Primary, Secondary) x States
|   |   +-- FAB (Floating Action Button for mobile)
|   +-- C/Inputs/
|   |   +-- TextField (Default, Error, Disabled) x (S, M, L)
|   |   +-- TextArea (same variants)
|   |   +-- Select (Dropdown)
|   |   +-- Checkbox (Unchecked, Checked, Indeterminate, Disabled)
|   |   +-- Radio (Unselected, Selected, Disabled)
|   |   +-- Toggle (Off, On, Disabled)
|   |   +-- FileUpload (Drag-Drop Zone, Uploading, Complete, Error)
|   +-- C/Navigation/
|   |   +-- Header (Desktop, Mobile variants)
|   |   +-- Sidebar (Expanded, Collapsed)
|   |   +-- Tabs (Horizontal, Selected/Unselected states)
|   |   +-- BottomNav (Mobile, 4-5 items, Selected/Unselected)
|   |   +-- Breadcrumb
|   +-- C/Content/
|   |   +-- Card (Simple, With Image, With Actions)
|   |   +-- ListItem (Single Line, Two Line, Three Line)
|   |   +-- Table (Header, Row, Empty State, Loading State)
|   |   +-- Timeline (Entry, Connector)
|   |   +-- Accordion (Collapsed, Expanded)
|   |   +-- Badge (Neutral, Success, Warning, Error, Info)
|   +-- C/Feedback/
|       +-- Modal (Small, Medium, Large)
|       +-- Drawer (Left, Right)
|       +-- Toast (Info, Success, Warning, Error)
|       +-- Alert (Info, Success, Warning, Error)
|       +-- Skeleton (Text, Card, Table Row)
|       +-- Spinner (Small, Medium, Large)
|       +-- ProgressBar (Determinate, Indeterminate)
+-- 03_Patterns
|   +-- Authentication Pattern (Login form layout)
|   +-- Search + Filter Pattern (Staff screens)
|   +-- Empty State Pattern (Template with icon, heading, body, CTA)
|   +-- Error State Pattern (Template with alert, retry button)
|   +-- Loading State Pattern (Skeleton screen template)
|   +-- Form Validation Pattern (Inline error display)
|   +-- Conflict Resolution Pattern (360° View conflict UI)
+-- 04_Screens
|   +-- Patient Portal/
|   |   +-- SCR-001_PatientDashboard/
|   |   |   +-- Mobile/Default, Mobile/Loading, Mobile/Empty
|   |   |   +-- Desktop/Default, Desktop/Loading, Desktop/Empty
|   |   +-- SCR-002_SlotSelection/
|   |   |   +-- Mobile/Default, Mobile/Loading, Mobile/Empty, Mobile/Error, Mobile/Validation, Mobile/SlotSelected, Mobile/BookingInProgress
|   |   |   +-- Desktop/[same states]
|   |   +-- SCR-003_DocumentUpload/
|   |   +-- SCR-004_AIIntake/
|   |   +-- SCR-005_ManualIntake/
|   |   +-- SCR-013_Login/
|   |   +-- SCR-014_PasswordReset/
|   |   +-- SCR-015_AccountActivation/
|   |   +-- SCR-016_AppointmentHistory/
|   +-- Staff Portal/
|   |   +-- SCR-006_WalkInManagement/
|   |   +-- SCR-007_ArrivalManagement/
|   |   +-- SCR-008_InsuranceValidation/
|   |   +-- SCR-009_360DegreeView/
|   |   +-- SCR-017_CodingReview/
|   +-- Admin Panel/
|       +-- SCR-010_UserManagementList/
|       +-- SCR-011_CreateEditUser/
|       +-- SCR-012_AuditLogViewer/
|       +-- SCR-018_SystemConfig/
+-- 05_Prototype
|   +-- FL-001_Authentication
|   |   +-- Wired flow: SCR-013 → SCR-001/007/010 (role-based routing)
|   +-- FL-002_SimpleBooking
|   |   +-- Wired flow: SCR-001 → SCR-002 → SCR-001 (success)
|   +-- FL-003_PreferredSlotSwap
|   |   +-- Wired flow: SCR-001 → SCR-002 (dual selection) → SCR-001
|   +-- FL-004_AIIntake
|   |   +-- Wired flow: SCR-001 → SCR-004 → SCR-001
|   +-- FL-005_ManualIntake
|   |   +-- Wired flow: SCR-001 → SCR-005 → SCR-001
|   +-- FL-006_WalkInCheckIn
|   |   +-- Wired flow: SCR-007 → SCR-006 → SCR-007
|   +-- FL-007_ArrivalManagement
|   |   +-- Wired flow: SCR-007 patient search → check-in → queue update
|   +-- FL-008_360DegreeReview
|   |   +-- Wired flow: SCR-007 → SCR-009 (tabs + conflict resolution) → SCR-007
|   +-- FL-009_AdminUserManagement
|       +-- Wired flow: SCR-010 → SCR-011 → SCR-010
+-- 06_Handoff
    +-- Design-to-Dev Handoff Guide
    |   +-- Token usage rules (reference designsystem.md)
    |   +-- Component naming conventions
    |   +-- Responsive breakpoint behavior
    |   +-- Animation timing and easing functions
    +-- Accessibility Specifications
    |   +-- WCAG 2.2 AA compliance checklist
    |   +-- Keyboard navigation map
    |   +-- Screen reader labels (ARIA)
    |   +-- Color contrast ratios
    +-- Edge Case Specifications
    |   +-- Long text handling (truncation rules)
    |   +-- Missing data fallbacks (placeholder images, empty states)
    |   +-- Error message copy (by error type)
    +-- State Transition Rules
        +-- Loading → Success (200ms fade-in)
        +-- Error → Retry (maintain user context)
        +-- Session timeout → Warning modal (13 min)
```

---

## 15. Quality Checklist

### Pre-Export Validation
- [ ] All 18 screens have required 5 states (Default/Loading/Empty/Error/Validation) + additional states where applicable
- [ ] All interactive components use design tokens from designsystem.md (no hard-coded hex colors)
- [ ] Color contrast meets WCAG AA: Text ≥4.5:1, Large Text/UI ≥3:1 (validated with axe-core or Stark plugin)
- [ ] Focus states defined for all interactive elements (2px solid outline, high contrast color)
- [ ] Touch targets ≥44x44px on mobile screens (UXR-205)
- [ ] Prototype flows wired and functional for FL-001 through FL-009
- [ ] Naming conventions followed: `PulseCare_Platform__<Platform>__<Screen>__<State>__v<Version>.jpg`
- [ ] Export manifest complete with all 148 JPGs listed

### Post-Generation (Implementation Phase)
- [ ] designsystem.md updated with Figma frame references (node-ids) or exported JPG paths
- [ ] Export manifest CSV generated with columns: Screen ID, State, Platform, Filename, Figma Frame Link
- [ ] JPG files organized in folder structure: `/exports/mobile/`, `/exports/tablet/`, `/exports/desktop/`
- [ ] Handoff documentation complete: Token guide, accessibility specs, edge cases, state transitions
- [ ] Stakeholder review conducted: Product (UX flows), Design (visual consistency), Engineering (feasibility), Compliance (HIPAA UI requirements)

### Quality Gates
- [ ] **UX Gate**: All 25 UXR requirements (UXR-101 to UXR-605) mapped to at least one screen
- [ ] **Consistency Gate**: All screens use components from Section 10 (no ad-hoc designs)
- [ ] **Traceability Gate**: All screens derived from UC-001 to UC-010 and FR-001 to FR-035
- [ ] **Accessibility Gate**: WCAG 2.2 AA compliance validated (automated + manual testing)
- [ ] **Completeness Gate**: All 11 prototype flows (FL-001 to FL-009 + variants) functional in Figma prototype mode

---

## 16. Validation & Testing

### Design System Compliance
- [ ] Typography: All text uses Inter font family (or JetBrains Mono for code)
- [ ] Spacing: All layouts use 4px/8px base grid (no arbitrary spacing values)
- [ ] Colors: All colors from Tailwind CSS palette (no custom hex codes)
- [ ] Components: All UI elements map to shadcn/ui components (Button, Card, Input, etc.)

### Responsive Behavior Testing
- [ ] Mobile 320px: No horizontal scroll, single-column layouts, bottom navigation visible
- [ ] Tablet 768px: Two-column layouts where appropriate, sidebar navigation on some screens
- [ ] Desktop 1024px+: Multi-column dashboards, persistent sidebar, full data tables

### Accessibility Testing  
- [ ] Keyboard navigation: Tab order logical, Enter activates, Escape closes modals
- [ ] Screen reader: ARIA labels present, live regions announce status changes, alt text on images
- [ ] Color contrast: Automated validation passed (axe-core, Stark, or similar tool)
- [ ] Touch targets: Mobile buttons ≥44x44px verified

### Content & Copy Review
- [ ] Error messages: Actionable, user-friendly, no technical jargon (UXR-601)
- [ ] Empty states: Encouraging with clear CTAs (Section 7 guidelines)
- [ ] Success messages: Brief, celebratory, next-action oriented
- [ ] Medical terminology: Patient-friendly in patient portal, clinical in staff screens

### Flow Completeness
- [ ] FL-001 (Authentication) tested: Role-based routing works
- [ ] FL-002 (Booking) tested: Success path flows to confirmation
- [ ] FL-003 (Swap) tested: Preferred slot selection and swap notification flows
- [ ] FL-004 (AI Intake) tested: Toggle to manual preserves data
- [ ] FL-005 (Manual Intake) tested: Validation errors display inline
- [ ] FL-006 (Walk-In) tested: Account creation optional
- [ ] FL-007 (Arrivals) tested: Queue updates after check-in
- [ ] FL-008 (360° View) tested: Conflict resolution flows work
- [ ] FL-009 (User Management) tested: Create/deactivate/reset flows

---

## 17. Implementation Notes

### Critical Path Screens (MVP Launch Blockers)
1. **SCR-013 Login** → Required for all personas to access system
2. **SCR-001 Patient Dashboard** → Entry point for patient workflows
3. **SCR-002 Slot Selection** → Core booking functionality (FR-001, FR-002)
4. **SCR-004 or SCR-005 Intake** → At least one intake mode required (FR-009 or FR-010)
5. **SCR-007 Arrival Management** → Staff check-in workflow (FR-019)
6. **SCR-009 360-Degree View** → Clinical data review (FR-022-FR-025)
7. **SCR-010 User Management** → Admin account creation (FR-030)

### Phase 1 Scope Exclusions (Future Enhancements)
- Dark mode support (all designs in light mode only)
- Real-time WebSocket updates (polling acceptable for MVP, limit: 5s intervals)
- Advanced animations (60fps constraint may require simplified transitions on low-end devices)
- Patient self-check-in (explicitly out of scope per FR-020)
- Provider-facing features (only Patient/Staff/Admin roles)

### Technical Constraints from design.md
- **Free-tier infrastructure** (FR-035): Design must account for occasional performance degradation (skeleton screens essential)
- **HIPAA compliance** (NFR-022): Avoid displaying PHI in browser console, screenshots, or error logs
- **Session timeout** (FR-029): 15-minute auto-logout requires persistent countdown timer in UI
- **AI processing time** (NFR-005): 30-second PDF extraction requires prominent loading states (UXR-502)

### Accessibility Priorities
- **WCAG 2.2 AA compliance** (NFR-020) is non-negotiable for legal reasons
- **Keyboard navigation** must support all workflows (no mouse-only interactions)
- **Screen reader compatibility** required for patient-facing screens (patient portal = public-facing)
- **Color contrast** violations cause build failures (automated testing in CI pipeline)

### Design Handoff Requirements
- **Component library** exported from Figma as React code (shadcn/ui base + customized variants)
- **Design tokens** exported as JSON for Tailwind CSS configuration
- **Prototype links** shared with engineering for flow reference
- **Accessibility annotations** documented in Figma (focus order, ARIA labels, keyboard shortcuts)

---

## 18. Appendix

### Glossary
- **360-Degree Patient View**: Consolidated display of patient clinical data from multiple sources (intake, documents, previous visits)
- **AI Extraction**: Automated data extraction from uploaded PDF documents using AI/ML (FR-022)
- **Confidence Score**: Percentage indicating AI extraction accuracy (0-100%, threshold: 70% for staff review)
- **Conflict**: Data discrepancy between multiple sources (e.g., medication dosage mismatch)
- **PHI**: Protected Health Information under HIPAA regulations
- **Preferred Slot Swap**: Feature allowing patients to book available slot while auto-swapping to preferred slot when it opens (FR-003, UC-002)
- **RBAC**: Role-Based Access Control (Patient, Staff, Admin roles with distinct permissions)
- **Walk-In**: Patient who arrives without prior appointment booking (staff-created appointment)

### Source Document References
- **spec.md**: 35 FR requirements, 10 use cases (UC-001 to UC-010) with PlantUML diagrams
- **design.md**: 23 NFR, 18 TR, 13 DR, 30 AIR requirements; technology stack table; architecture decisions
- **designsystem.md**: (To be created) Design tokens, component specifications, branding guidelines

### Related Standards
- **WCAG 2.2 Level AA**: Web Content Accessibility Guidelines (https://www.w3.org/WAI/WCAG22/quickref/)
- **HIPAA Security Rule (45 CFR Part 164)**: Healthcare data protection standards (https://www.hhs.gov/hipaa/for-professionals/security/index.html)
- **Tailwind CSS Documentation**: Utility-first CSS framework (https://tailwindcss.com/docs)
- **shadcn/ui Components**: React component library (https://ui.shadcn.com/)

---

*Document End*
