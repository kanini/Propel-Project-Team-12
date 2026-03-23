# Figma Design Specification - Unified Patient Access & Clinical Intelligence Platform

## 1. Figma Specification
**Platform**: Responsive Web (Desktop + Tablet + Mobile)

---

## 2. Source References

### Primary Source
| Document | Path | Purpose |
|----------|------|---------|
| Requirements | `.propel/context/docs/spec.md` | Personas, use cases (UC-001 to UC-015), FR-XXX |
| Architecture | `.propel/context/docs/design.md` | NFR, TR, DR, AI requirements |

### Related Documents
| Document | Path | Purpose |
|----------|------|---------|
| Design System | `.propel/context/docs/designsystem.md` | Tokens, branding, component specifications |

---

## 3. UX Requirements

### UXR Requirements Table

| UXR-ID | Category | Requirement | Acceptance Criteria | Screens Affected |
|--------|----------|-------------|---------------------|------------------|
| **Project-Wide (UXR-001 to UXR-099)** |||||
| UXR-001 | Global | System MUST provide consistent navigation across all authenticated views | Navigation component appears in same position on all screens | All authenticated screens |
| UXR-002 | Global | System MUST provide clear visual distinction between Patient, Staff, and Admin portals | Role-specific color accents and navigation items | All screens |
| UXR-003 | Global | System MUST preserve user context during 15-minute session timeout with auto-save | Form data recovered after re-authentication | SCR-005, SCR-006, SCR-007 |
| **Usability (UXR-1XX)** |||||
| UXR-101 | Usability | System MUST enable appointment booking completion in max 3 clicks from dashboard | Click count audit passes ≤3 clicks | SCR-003, SCR-004, SCR-005 |
| UXR-102 | Usability | System MUST provide real-time availability updates within 500ms | Visual indicator updates without page refresh | SCR-004 |
| UXR-103 | Usability | System MUST allow seamless switching between AI and manual intake modes | Toggle visible, preserves data on switch | SCR-006, SCR-007 |
| UXR-104 | Usability | System MUST provide clear visual hierarchy for 360-degree patient view | Primary info scannable in <5 seconds | SCR-009 |
| UXR-105 | Usability | System MUST enable bulk actions on queue management dashboard | Multi-select with batch operations | SCR-013 |
| UXR-106 | Usability | System MUST provide keyboard shortcuts for high-frequency staff actions | Shortcuts documented and functional | SCR-013, SCR-015, SCR-016 |
| **Accessibility (UXR-2XX)** |||||
| UXR-201 | Accessibility | System MUST comply with WCAG 2.2 AA standards for all screens | WAVE/axe audit passes with 0 critical errors | All screens |
| UXR-202 | Accessibility | System MUST provide visible focus indicators with ≥3:1 contrast | Focus outline visible on all interactive elements | All screens |
| UXR-203 | Accessibility | System MUST support screen reader navigation with semantic HTML and ARIA labels | VoiceOver/NVDA testing passes | All screens |
| UXR-204 | Accessibility | System MUST provide text alternatives for all informative images and icons | All images have alt text or aria-label | All screens |
| UXR-205 | Accessibility | System MUST ensure minimum touch targets of 44x44px on mobile | Touch target audit passes | All screens (mobile) |
| UXR-206 | Accessibility | System MUST support keyboard navigation for all workflows | Tab order follows logical reading order | All screens |
| **Responsiveness (UXR-3XX)** |||||
| UXR-301 | Responsiveness | System MUST adapt layouts for mobile (320px), tablet (768px), desktop (1024px+) | Responsive audit passes at all breakpoints | All screens |
| UXR-302 | Responsiveness | System MUST provide mobile-optimized navigation (bottom nav or hamburger menu) | Navigation accessible on viewport <768px | All screens |
| UXR-303 | Responsiveness | System MUST ensure calendar view is usable on mobile with touch gestures | Date selection functional on touch devices | SCR-004 |
| UXR-304 | Responsiveness | System MUST stack form fields vertically on mobile viewports | Forms readable without horizontal scroll | SCR-005, SCR-006, SCR-007 |
| **Visual Design (UXR-4XX)** |||||
| UXR-401 | Visual Design | System MUST use design tokens for all colors, typography, and spacing | No hard-coded values in implementation | All screens |
| UXR-402 | Visual Design | System MUST provide light and dark mode support | Theme toggle functional; both modes meet contrast | All screens |
| UXR-403 | Visual Design | System MUST use healthcare-appropriate color palette (calming, professional) | Brand colors applied consistently | All screens |
| UXR-404 | Visual Design | System MUST distinguish AI-generated vs human-verified data visually | Visual badge or indicator on AI suggestions | SCR-009, SCR-015, SCR-016 |
| **Interaction (UXR-5XX)** |||||
| UXR-501 | Interaction | System MUST provide loading feedback within 200ms for user actions | Skeleton or spinner appears immediately | All screens |
| UXR-502 | Interaction | System MUST confirm successful actions with toast notifications | Success toast appears and auto-dismisses | All screens |
| UXR-503 | Interaction | System MUST animate transitions between screens (150-300ms easing) | Smooth page transitions, no jarring jumps | All screens |
| UXR-504 | Interaction | System MUST provide inline validation feedback on form fields | Errors appear as user types with clear messages | SCR-002, SCR-005, SCR-006, SCR-007 |
| UXR-505 | Interaction | System MUST honor prefers-reduced-motion settings | Animations disabled when preference set | All screens |
| **Error Handling (UXR-6XX)** |||||
| UXR-601 | Error Handling | System MUST display user-friendly error messages with recovery actions | Error messages actionable (retry, contact support) | All screens |
| UXR-602 | Error Handling | System MUST provide graceful degradation when AI services unavailable | Manual fallback offered; no blocking errors | SCR-006, SCR-008, SCR-015 |
| UXR-603 | Error Handling | System MUST persist form data on validation errors (no data loss) | User input preserved after error submission | SCR-002, SCR-005, SCR-006, SCR-007 |
| UXR-604 | Error Handling | System MUST display session timeout warning 2 minutes before expiration | Modal appears with extend/logout options | All authenticated screens |
| UXR-605 | Error Handling | System MUST handle offline state with cached data when available | Offline indicator; read-only mode functional | SCR-003, SCR-009 |

---

## 4. Personas Summary

| Persona | Role | Primary Goals | Key Screens |
|---------|------|---------------|-------------|
| Patient | Healthcare Consumer | Book appointments easily, complete intake forms, view health dashboard, upload documents | SCR-001 to SCR-010 |
| Staff | Front Desk / Clinical | Manage walk-ins, monitor queues, mark arrivals, review conflicts, verify codes | SCR-011 to SCR-017 |
| Admin | System Administrator | Manage users, configure roles, monitor audit compliance | SCR-018 to SCR-021 |

---

## 5. Information Architecture

### Site Map
```
Unified Patient Access Platform
├── Public
│   ├── Landing Page
│   ├── Sign In
│   └── Sign Up
├── Patient Portal
│   ├── Dashboard (360-View)
│   ├── Book Appointment
│   │   ├── Calendar View
│   │   ├── Slot Selection
│   │   └── Confirmation
│   ├── Intake
│   │   ├── AI Conversational
│   │   └── Manual Form
│   ├── Documents
│   │   ├── Upload
│   │   └── View/Manage
│   ├── Profile
│   └── Appointments
├── Staff Portal
│   ├── Queue Dashboard
│   ├── Patient Search
│   ├── Walk-in Booking
│   ├── Arrival Management
│   ├── Insurance Validation
│   ├── Conflict Review
│   └── Code Verification
└── Admin Portal
    ├── User Management
    ├── Role Configuration
    └── Audit Logs
```

### Navigation Patterns
| Pattern | Type | Platform Behavior |
|---------|------|-------------------|
| Primary Nav | Sidebar | Desktop: Collapsible sidebar / Mobile: Bottom nav |
| Role Switcher | Header | Top-right user menu with role context |
| Breadcrumb | Secondary | Desktop: Below header / Mobile: Hidden |
| Utility Nav | Header | Notifications, settings, logout |

---

## 6. Screen Inventory

### Screen List

| Screen ID | Screen Name | Derived From | UXR-XXX Mapped | Personas Covered | Priority | States Required |
|-----------|-------------|--------------|----------------|------------------|----------|-----------------|
| SCR-001 | Landing Page | - | UXR-201, UXR-301 | Public | P0 | Default, Loading |
| SCR-002 | Sign In | UC-001 (precondition) | UXR-201, UXR-504, UXR-601 | All | P0 | Default, Loading, Error, Validation |
| SCR-003 | Patient Dashboard | UC-006 | UXR-104, UXR-201, UXR-301 | Patient | P0 | Default, Loading, Empty, Error |
| SCR-004 | Appointment Calendar | UC-001, UC-002 | UXR-101, UXR-102, UXR-303 | Patient | P0 | Default, Loading, Empty, Error |
| SCR-005 | Booking Confirmation | UC-001, UC-002 | UXR-101, UXR-502 | Patient | P0 | Default, Loading, Error, Validation |
| SCR-006 | AI Intake | UC-003 | UXR-103, UXR-203, UXR-602 | Patient | P0 | Default, Loading, Error |
| SCR-007 | Manual Intake Form | UC-004 | UXR-103, UXR-304, UXR-504 | Patient | P0 | Default, Loading, Error, Validation |
| SCR-008 | Document Upload | UC-005 | UXR-501, UXR-602 | Patient | P0 | Default, Loading, Empty, Error |
| SCR-009 | 360-Degree Patient View | UC-006 | UXR-104, UXR-404 | Patient, Staff | P0 | Default, Loading, Empty, Error |
| SCR-010 | Appointment History | UC-001 | UXR-201, UXR-301 | Patient | P1 | Default, Loading, Empty |
| SCR-011 | Staff Dashboard | UC-008 | UXR-105, UXR-106 | Staff | P0 | Default, Loading, Empty, Error |
| SCR-012 | Patient Search | UC-007, UC-009 | UXR-105, UXR-206 | Staff | P0 | Default, Loading, Empty |
| SCR-013 | Queue Management | UC-008 | UXR-105, UXR-106, UXR-502 | Staff | P0 | Default, Loading, Empty, Error |
| SCR-014 | Walk-in Booking | UC-007 | UXR-101, UXR-504 | Staff | P0 | Default, Loading, Error, Validation |
| SCR-015 | Conflict Review | UC-011 | UXR-404, UXR-601 | Staff | P0 | Default, Loading, Empty, Error |
| SCR-016 | Code Verification | UC-012 | UXR-106, UXR-404 | Staff | P0 | Default, Loading, Empty, Error |
| SCR-017 | Insurance Validation | UC-010 | UXR-502, UXR-601 | Staff | P1 | Default, Loading, Error, Validation |
| SCR-018 | Admin Dashboard | UC-013 | UXR-105 | Admin | P0 | Default, Loading, Error |
| SCR-019 | User Management | UC-013 | UXR-105, UXR-504 | Admin | P0 | Default, Loading, Empty, Error, Validation |
| SCR-020 | Role Configuration | UC-013 | UXR-504 | Admin | P1 | Default, Loading, Error, Validation |
| SCR-021 | Audit Log Viewer | FR-037 | UXR-105, UXR-301 | Admin | P0 | Default, Loading, Empty, Error |

### Priority Legend
- **P0**: Critical path (MVP must-have)
- **P1**: Core functionality (high priority, post-MVP)
- **P2**: Important features (medium priority)
- **P3**: Nice-to-have (low priority)

### Screen-to-Persona Coverage Matrix
| Screen | Patient | Staff | Admin | Notes |
|--------|---------|-------|-------|-------|
| SCR-001 Landing | ✓ | ✓ | ✓ | Public entry point |
| SCR-002 Sign In | ✓ | ✓ | ✓ | Universal authentication |
| SCR-003 Patient Dashboard | Primary | - | - | Patient home |
| SCR-004 Calendar | Primary | - | - | Booking flow |
| SCR-005 Booking Confirm | Primary | - | - | Booking flow |
| SCR-006 AI Intake | Primary | - | - | Intake option |
| SCR-007 Manual Intake | Primary | - | - | Intake option |
| SCR-008 Doc Upload | Primary | - | - | Document management |
| SCR-009 360-View | Primary | Secondary | - | Shared patient view |
| SCR-010 History | Primary | - | - | Patient records |
| SCR-011 Staff Dashboard | - | Primary | - | Staff home |
| SCR-012 Patient Search | - | Primary | - | Patient lookup |
| SCR-013 Queue Mgmt | - | Primary | - | Same-day operations |
| SCR-014 Walk-in | - | Primary | - | Walk-in flow |
| SCR-015 Conflict Review | - | Primary | - | Data review |
| SCR-016 Code Verify | - | Primary | - | Medical coding |
| SCR-017 Insurance | - | Primary | - | Validation |
| SCR-018 Admin Dashboard | - | - | Primary | Admin home |
| SCR-019 User Mgmt | - | - | Primary | User admin |
| SCR-020 Roles | - | - | Primary | Role config |
| SCR-021 Audit Logs | - | - | Primary | Compliance |

### Modal/Overlay Inventory
| Name | Type | Trigger | Parent Screen(s) | Priority |
|------|------|---------|------------------|----------|
| Session Timeout Warning | Modal | 13 min inactivity | All authenticated | P0 |
| Confirm Booking | Modal | Submit booking | SCR-005 | P0 |
| Cancel Appointment | Dialog | Cancel action | SCR-003, SCR-010 | P0 |
| Document Preview | Drawer | Click document | SCR-008, SCR-009 | P0 |
| Conflict Detail | Drawer | Click conflict | SCR-015 | P0 |
| Code Evidence | Drawer | Click code | SCR-016 | P0 |
| Create User | Modal | Add user action | SCR-019 | P0 |
| Swap Preference | Modal | Indicate preferred slot | SCR-004 | P1 |
| Filter Queue | Drawer (mobile) | Filter action | SCR-013 | P1 |

---

## 7. Content & Tone

### Voice & Tone
- **Overall Tone**: Professional, reassuring, healthcare-appropriate
- **Error Messages**: Helpful, non-blaming, actionable ("We couldn't verify your insurance. Please check the details or contact support.")
- **Empty States**: Encouraging, guiding with clear CTAs ("No appointments yet. Book your first visit.")
- **Success Messages**: Brief, celebratory, next-action oriented ("Appointment confirmed! Check your email for details.")

### Content Guidelines
- **Headings**: Sentence case (e.g., "Your appointments")
- **CTAs**: Action-oriented, specific verbs ("Book appointment", "Verify code", "Upload document")
- **Labels**: Concise, descriptive (avoid abbreviations in patient-facing UI)
- **Placeholder Text**: Helpful examples ("e.g., john.doe@email.com")
- **Medical Terminology**: Use plain language with tooltips for technical terms

---

## 8. Data & Edge Cases

### Data Scenarios
| Scenario | Description | Handling |
|----------|-------------|----------|
| No Data | New patient with no records | Empty state with onboarding guidance and CTAs |
| First Use | New user, no appointments | Welcome tour, quick booking CTA |
| Large Data | Patient with 10,000+ documents | Pagination (20/page), search, date filters |
| Slow Connection | >3s load time | Skeleton screens, progressive loading |
| Offline | No network | Offline indicator, cached data read-only |
| Session Timeout | 15-min inactivity | Warning modal at 13 min, auto-save then logout |

### Edge Cases
| Case | Screen(s) Affected | Solution |
|------|-------------------|----------|
| Long patient name | All with names | Truncation with tooltip (max 30 chars visible) |
| No available slots | SCR-004 | Empty state with waitlist CTA |
| Document extraction failure | SCR-008, SCR-009 | Manual review flag, staff notification |
| AI confidence <80% | SCR-006, SCR-015, SCR-016 | Automatic fallback indicator, manual mode |
| Concurrent slot booking | SCR-004 | Real-time update, conflict notification |
| Multiple data conflicts | SCR-015 | Severity sorting, batch resolution option |
| Insurance validation fail | SCR-017 | Clear fail reason, retry or override option |

---

## 9. Branding & Visual Direction

*See `designsystem.md` for complete design tokens*

### Branding Assets
- **Logo**: Healthcare wordmark, horizontal and icon variants
- **Icon Style**: Outlined, consistent stroke width (1.5-2px)
- **Illustration Style**: Flat, minimal, healthcare-themed (diverse patients, friendly staff)
- **Photography Style**: Not applicable (illustrations only for Phase 1)

### Visual Direction
- **Primary Colors**: Calming blue-green palette (trust, healthcare)
- **Accent**: Warm orange for CTAs and highlights
- **Semantic**: Red for errors/critical, Green for success, Amber for warnings
- **Theme**: Professional healthcare with modern SaaS aesthetics

---

## 10. Component Specifications

### Component Library Reference
**Source**: `.propel/context/docs/designsystem.md` (Component Specifications section)

### Required Components per Screen
| Screen ID | Components Required | Notes |
|-----------|---------------------|-------|
| SCR-001 | Header, Button (2), Link (3), Hero section | Landing page |
| SCR-002 | TextField (2), PasswordField (1), Button (2), Link (1), Alert | Login form |
| SCR-003 | Card (N), StatCard (4), AppointmentCard (N), Header, Sidebar | Dashboard |
| SCR-004 | Calendar, SlotButton (N), Button (2), Modal | Slot selection |
| SCR-005 | Form sections, TextField (5), Select (2), Button (2), Alert | Booking form |
| SCR-006 | ChatBubble (N), TextField (1), Button (3), Card | AI conversation |
| SCR-007 | TextField (10+), Select (5), Checkbox (3), Radio (2), Button (2) | Manual form |
| SCR-008 | FileUpload, DocumentCard (N), Button (2), ProgressBar, Badge | Upload UI |
| SCR-009 | Tabs, Card (N), DataTable, Badge, Tag, VitalChart | 360 view |
| SCR-010 | DataTable, Badge, Button (1), Pagination | Appointment list |
| SCR-011 | StatCard (4), QueueList, Button (3), Badge | Staff dashboard |
| SCR-012 | SearchField, DataTable, Button (2), Badge | Patient search |
| SCR-013 | DataTable, Badge (status), Button (4), Checkbox (N), Timer | Queue table |
| SCR-014 | SearchField, TextField (6), Select (2), Button (3) | Walk-in form |
| SCR-015 | Card (N), ComparePanel, Button (3), Badge (severity) | Conflict review |
| SCR-016 | DataTable, CodeCard (N), Badge (confidence), Button (4) | Code verification |
| SCR-017 | TextField (3), Button (2), Alert (result) | Insurance check |
| SCR-018 | StatCard (4), ActivityFeed, Card (N) | Admin dashboard |
| SCR-019 | DataTable, Modal (form), Button (4), Badge (status), Pagination | User CRUD |
| SCR-020 | Card (N), Toggle (N), Checkbox (N), Button (2) | Role config |
| SCR-021 | DataTable, SearchField, DatePicker (2), Select (3), Button (2), Pagination | Audit logs |

### Component Summary
| Category | Components | Variants |
|----------|------------|----------|
| Actions | Button, IconButton, Link, FAB | Primary/Secondary/Tertiary/Ghost × S/M/L × States |
| Inputs | TextField, PasswordField, Select, Checkbox, Radio, Toggle, DatePicker, FileUpload | States + Sizes + Validation |
| Navigation | Header, Sidebar, Tabs, Breadcrumb, BottomNav, Pagination | Platform variants |
| Content | Card, StatCard, AppointmentCard, DocumentCard, CodeCard, ListItem, DataTable, Avatar, Badge, Tag | Content variants |
| Feedback | Modal, Drawer, Toast, Alert, Tooltip, Skeleton, ProgressBar | Types + States |
| Data Display | Calendar, QueueList, VitalChart, ActivityFeed, ComparePanel, Timer | Display variants |

### Component Constraints
- Use only components from designsystem.md
- All components must support: Default, Hover, Focus, Active, Disabled, Loading states
- Follow naming convention: `C/<Category>/<Name>`
- No custom components without design review approval

---

## 11. Prototype Flows

### Flow: FL-001 - Patient Appointment Booking
**Flow ID**: FL-001
**Derived From**: UC-001, UC-002
**Personas Covered**: Patient
**Description**: Complete flow from dashboard to confirmed appointment

#### Flow Sequence
```
1. Entry: SCR-003 (Patient Dashboard) / Default
   - Trigger: User clicks "Book Appointment" CTA
   |
   v
2. Step: SCR-004 (Appointment Calendar) / Default
   - Action: User views available slots
   |
   v
3. Step: SCR-004 (Appointment Calendar) / Default
   - Action: User selects preferred date/time
   |
   v
4. Decision Point:
   +-- Slot Available -> SCR-005 (Booking Confirmation) / Default
   +-- Slot Unavailable -> Show Swap Preference Modal
       +-- Accept swap -> SCR-005 with swap noted
       +-- Join waitlist -> SCR-003 with waitlist notification
   |
   v
5. Step: SCR-005 (Booking Confirmation) / Default
   - Action: User reviews and confirms booking details
   |
   v
6. Step: SCR-005 (Booking Confirmation) / Loading
   - Action: System processes booking
   |
   v
7. Decision Point:
   +-- Success -> SCR-003 / Default + Success Toast
   +-- Error -> SCR-005 / Error state with retry
```

#### Required Interactions
- Calendar navigation (month swipe/click)
- Slot selection (tap/click)
- Swap preference modal dismiss
- Form field validation
- Confirmation button loading state

---

### Flow: FL-002 - Patient Intake (AI Mode)
**Flow ID**: FL-002
**Derived From**: UC-003
**Personas Covered**: Patient
**Description**: AI-driven conversational intake with manual fallback

#### Flow Sequence
```
1. Entry: SCR-003 (Patient Dashboard) / Default
   - Trigger: "Complete Intake" prompt or manual navigation
   |
   v
2. Step: SCR-006 (AI Intake) / Default
   - Action: AI initiates conversation greeting
   |
   v
3. Step: SCR-006 (AI Intake) / Default
   - Action: Patient responds to AI prompts
   |
   v
4. Decision Point:
   +-- Continue AI -> Loop to Step 3
   +-- Switch to Manual -> SCR-007 (Manual Intake) / Default (data preserved)
   +-- AI Error -> SCR-006 / Error with fallback to SCR-007
   |
   v
5. Step: SCR-006 (AI Intake) / Default
   - Action: AI displays summary for review
   |
   v
6. Step: SCR-006 (AI Intake) / Default
   - Action: Patient confirms or edits data
   |
   v
7. Exit: SCR-003 (Patient Dashboard) / Default + Success Toast
```

#### Required Interactions
- Chat message typing and sending
- Mode toggle switch (AI ↔ Manual)
- Edit inline data
- Summary review and confirm

---

### Flow: FL-003 - Staff Walk-in Booking
**Flow ID**: FL-003
**Derived From**: UC-007, UC-009
**Personas Covered**: Staff
**Description**: Staff creates walk-in appointment and marks arrival

#### Flow Sequence
```
1. Entry: SCR-011 (Staff Dashboard) / Default
   - Trigger: Click "Walk-in" button
   |
   v
2. Step: SCR-014 (Walk-in Booking) / Default
   - Action: Staff searches for existing patient
   |
   v
3. Decision Point:
   +-- Patient Found -> Select patient, continue
   +-- Patient Not Found -> Enter minimal info, continue
   |
   v
4. Step: SCR-014 (Walk-in Booking) / Default
   - Action: Staff selects available same-day slot
   |
   v
5. Step: SCR-014 (Walk-in Booking) / Loading
   - Action: System creates appointment
   |
   v
6. Decision Point:
   +-- Success -> SCR-013 / Default (patient added to queue)
   +-- Error -> SCR-014 / Error with retry
   |
   v
7. Optional: SCR-013 (Queue Management) / Default
   - Action: Staff marks patient as "Arrived"
   |
   v
8. Exit: SCR-013 (Queue Management) / Default + Arrival confirmed toast
```

#### Required Interactions
- Patient search field with autocomplete
- Same-day slot quick select
- Arrival status toggle
- Queue position update

---

### Flow: FL-004 - Clinical Data Conflict Resolution
**Flow ID**: FL-004
**Derived From**: UC-011
**Personas Covered**: Staff
**Description**: Staff reviews and resolves data conflicts from multiple documents

#### Flow Sequence
```
1. Entry: SCR-011 (Staff Dashboard) / Default
   - Trigger: Click conflict notification badge
   |
   v
2. Step: SCR-015 (Conflict Review) / Default
   - Action: System displays patients with unresolved conflicts
   |
   v
3. Step: SCR-015 (Conflict Review) / Default
   - Action: Staff selects patient to review
   |
   v
4. Step: Conflict Detail Drawer / Default
   - Action: System shows conflicting values with source documents
   |
   v
5. Step: Document Preview Overlay / Default
   - Action: Staff reviews source documents side-by-side
   |
   v
6. Decision Point:
   +-- Select Value A -> Conflict resolved
   +-- Select Value B -> Conflict resolved  
   +-- Escalate -> Mark for clinical review
   +-- Cannot determine -> Leave unresolved
   |
   v
7. Step: SCR-015 (Conflict Review) / Default
   - Action: Conflict status updated, audit logged
   |
   v
8. Exit: Loop to Step 3 for next conflict or return to SCR-011
```

#### Required Interactions
- Conflict card selection
- Side-by-side document comparison
- Value selection radio buttons
- Escalation action with notes
- Batch navigation (next/previous)

---

### Flow: FL-005 - Medical Code Verification
**Flow ID**: FL-005
**Derived From**: UC-012
**Personas Covered**: Staff
**Description**: Staff verifies AI-suggested ICD-10/CPT codes

#### Flow Sequence
```
1. Entry: SCR-011 (Staff Dashboard) / Default
   - Trigger: Click "Pending Codes" or navigate to Code Verification
   |
   v
2. Step: SCR-016 (Code Verification) / Default
   - Action: System displays AI-suggested codes with confidence scores
   |
   v
3. Step: SCR-016 (Code Verification) / Default
   - Action: Staff selects code to review
   |
   v
4. Step: Code Evidence Drawer / Default
   - Action: System shows supporting evidence from clinical documents
   |
   v
5. Decision Point:
   +-- Confirm Code -> Code verified status
   +-- Modify Code -> Open code editor, save change
   +-- Reject Code -> Remove from suggestions
   +-- Add Missing Code -> Open code search, add manually
   |
   v
6. Step: SCR-016 (Code Verification) / Default
   - Action: Code status updated, agreement metrics recorded
   |
   v
7. Exit: Loop to Step 3 for next code or return to SCR-011
```

#### Required Interactions
- Code card expansion
- Evidence drawer scroll
- Confidence badge display
- Confirm/Reject button actions
- Code search autocomplete
- Keyboard shortcuts (Enter=confirm, X=reject)

---

### Flow: FL-006 - Admin User Management
**Flow ID**: FL-006
**Derived From**: UC-013
**Personas Covered**: Admin
**Description**: Admin creates, edits, or deactivates user accounts

#### Flow Sequence
```
1. Entry: SCR-018 (Admin Dashboard) / Default
   - Trigger: Click "User Management" navigation
   |
   v
2. Step: SCR-019 (User Management) / Default
   - Action: System displays user table
   |
   v
3. Decision Point (Action Selection):
   +-- Create User -> Open Create User Modal
   +-- Edit User -> Open Edit User Modal
   +-- Deactivate User -> Show Confirmation Dialog
   |
   v
4a. Step: Create User Modal / Default
    - Action: Admin fills user details, assigns role
    - Validation: Email format, required fields
    |
    v
4b. Step: Edit User Modal / Default
    - Action: Admin modifies user details
    - Validation: Change validation
    |
    v
4c. Step: Deactivate Confirmation Dialog / Default
    - Action: Admin confirms deactivation
    |
    v
5. Step: Modal / Loading
   - Action: System processes change
   |
   v
6. Decision Point:
   +-- Success -> SCR-019 / Default + Success Toast
   +-- Validation Error -> Modal / Validation state
   +-- Server Error -> Modal / Error state with retry
   |
   v
7. Exit: SCR-019 (User Management) / Default with updated table
```

#### Required Interactions
- Table row actions (kebab menu)
- Modal form fields
- Role dropdown selection
- Confirmation dialog
- Table refresh/pagination

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
`UPACIP__<Platform>__<ScreenName>__<State>__v<Version>.jpg`

### Export Manifest
| Screen | State | Platform | Filename |
|--------|-------|----------|----------|
| SCR-001 Landing | Default | Web | UPACIP__Web__Landing__Default__v1.jpg |
| SCR-002 Sign In | Default | Web | UPACIP__Web__SignIn__Default__v1.jpg |
| SCR-002 Sign In | Error | Web | UPACIP__Web__SignIn__Error__v1.jpg |
| SCR-002 Sign In | Validation | Web | UPACIP__Web__SignIn__Validation__v1.jpg |
| SCR-003 Patient Dashboard | Default | Web | UPACIP__Web__PatientDashboard__Default__v1.jpg |
| SCR-003 Patient Dashboard | Loading | Web | UPACIP__Web__PatientDashboard__Loading__v1.jpg |
| SCR-003 Patient Dashboard | Empty | Web | UPACIP__Web__PatientDashboard__Empty__v1.jpg |
| SCR-004 Calendar | Default | Web | UPACIP__Web__Calendar__Default__v1.jpg |
| SCR-004 Calendar | Default | Mobile | UPACIP__Mobile__Calendar__Default__v1.jpg |
| ... | ... | ... | ... |

### Total Export Count
- **Screens**: 21
- **States per screen**: 4 average (Default, Loading, Empty, Error)
- **Platforms**: 2 (Web, Mobile for key screens)
- **Total JPGs**: ~100 exports

---

## 13. Figma File Structure

### Page Organization
```
UPACIP Figma File
├── 00_Cover
│   ├── Project info: Unified Patient Access & Clinical Intelligence Platform
│   ├── Version: 1.0.0
│   ├── Last Updated: [Date]
│   └── Stakeholders: Product, Design, Engineering
├── 01_Foundations
│   ├── Color tokens (Light + Dark)
│   ├── Typography scale
│   ├── Spacing scale
│   ├── Radius tokens
│   ├── Elevation/shadows
│   └── Grid definitions (12-col responsive)
├── 02_Components
│   ├── C/Actions/[Button, IconButton, Link, FAB]
│   ├── C/Inputs/[TextField, PasswordField, Select, Checkbox, Radio, Toggle, DatePicker, FileUpload]
│   ├── C/Navigation/[Header, Sidebar, Tabs, Breadcrumb, BottomNav, Pagination]
│   ├── C/Content/[Card, StatCard, AppointmentCard, DocumentCard, CodeCard, ListItem, DataTable, Avatar, Badge, Tag]
│   ├── C/Feedback/[Modal, Drawer, Toast, Alert, Tooltip, Skeleton, ProgressBar]
│   └── C/DataDisplay/[Calendar, QueueList, VitalChart, ActivityFeed, ComparePanel, Timer, ChatBubble]
├── 03_Patterns
│   ├── Auth form pattern
│   ├── Search + filter pattern
│   ├── Dashboard layout pattern
│   ├── Form layout pattern
│   ├── Data table pattern
│   ├── Chat interface pattern
│   └── Error/Empty/Loading patterns
├── 04_Screens
│   ├── 01_Public/
│   │   ├── Landing/[Default]
│   │   └── SignIn/[Default, Loading, Error, Validation]
│   ├── 02_Patient/
│   │   ├── Dashboard/[Default, Loading, Empty, Error]
│   │   ├── Calendar/[Default, Loading, Empty, Error]
│   │   ├── BookingConfirm/[Default, Loading, Error, Validation]
│   │   ├── AIIntake/[Default, Loading, Error]
│   │   ├── ManualIntake/[Default, Loading, Error, Validation]
│   │   ├── DocUpload/[Default, Loading, Empty, Error]
│   │   ├── 360View/[Default, Loading, Empty, Error]
│   │   └── History/[Default, Loading, Empty]
│   ├── 03_Staff/
│   │   ├── Dashboard/[Default, Loading, Empty, Error]
│   │   ├── PatientSearch/[Default, Loading, Empty]
│   │   ├── Queue/[Default, Loading, Empty, Error]
│   │   ├── WalkIn/[Default, Loading, Error, Validation]
│   │   ├── ConflictReview/[Default, Loading, Empty, Error]
│   │   ├── CodeVerify/[Default, Loading, Empty, Error]
│   │   └── Insurance/[Default, Loading, Error, Validation]
│   └── 04_Admin/
│       ├── Dashboard/[Default, Loading, Error]
│       ├── UserMgmt/[Default, Loading, Empty, Error, Validation]
│       ├── Roles/[Default, Loading, Error, Validation]
│       └── AuditLog/[Default, Loading, Empty, Error]
├── 05_Prototype
│   ├── FL-001: Patient Booking Flow
│   ├── FL-002: Patient Intake (AI)
│   ├── FL-003: Staff Walk-in
│   ├── FL-004: Conflict Resolution
│   ├── FL-005: Code Verification
│   └── FL-006: Admin User Management
└── 06_Handoff
    ├── Token usage rules
    ├── Component guidelines
    ├── Responsive specs
    ├── Edge cases
    └── Accessibility notes
```

---

## 14. Quality Checklist

### Pre-Export Validation
- [ ] All 21 screens have required states (Default/Loading/Empty/Error/Validation)
- [ ] All components use design tokens (no hard-coded values)
- [ ] Color contrast meets WCAG AA (≥4.5:1 text, ≥3:1 UI)
- [ ] Focus states defined for all interactive elements
- [ ] Touch targets ≥44x44px (mobile)
- [ ] 6 prototype flows wired and functional
- [ ] Naming conventions followed
- [ ] Export manifest complete

### Post-Generation
- [ ] designsystem.md created with complete tokens
- [ ] Export manifest generated
- [ ] JPG files named correctly
- [ ] Handoff documentation complete
