# Information Architecture - Unified Patient Access & Clinical Intelligence Platform

## 1. Wireframe Specification

**Fidelity Level**: High
**Screen Type**: Web (Responsive)
**Viewport**: 1440px × 900px (Primary Desktop)

## 2. System Overview

The Unified Patient Access & Clinical Intelligence Platform (UPACIP) is a comprehensive healthcare SaaS solution that provides:
- **Patient Portal**: Self-service appointment booking, AI-powered intake, document management, 360-degree health view
- **Staff Portal**: Queue management, walk-in booking, conflict resolution, medical code verification
- **Admin Portal**: User management, role configuration, audit compliance monitoring

## 3. Wireframe References

### Generated Wireframes
**HTML Wireframes**:
| Screen/Feature | File Path | Description | Fidelity | Date Created |
|---------------|-----------|-------------|----------|--------------|
| SCR-001 Landing | [Hi-Fi/wireframe-SCR-001-landing.html](./Hi-Fi/wireframe-SCR-001-landing.html) | Public landing page with CTA | High | 2026-03-16 |
| SCR-002 Sign In | [Hi-Fi/wireframe-SCR-002-signin.html](./Hi-Fi/wireframe-SCR-002-signin.html) | Authentication with validation states | High | 2026-03-16 |
| SCR-003 Patient Dashboard | [Hi-Fi/wireframe-SCR-003-patient-dashboard.html](./Hi-Fi/wireframe-SCR-003-patient-dashboard.html) | Patient home with 360-view summary | High | 2026-03-16 |
| SCR-004 Appointment Calendar | [Hi-Fi/wireframe-SCR-004-calendar.html](./Hi-Fi/wireframe-SCR-004-calendar.html) | Slot selection calendar | High | 2026-03-16 |
| SCR-005 Booking Confirmation | [Hi-Fi/wireframe-SCR-005-booking-confirm.html](./Hi-Fi/wireframe-SCR-005-booking-confirm.html) | Appointment booking form | High | 2026-03-16 |
| SCR-006 AI Intake | [Hi-Fi/wireframe-SCR-006-ai-intake.html](./Hi-Fi/wireframe-SCR-006-ai-intake.html) | Conversational AI intake | High | 2026-03-16 |
| SCR-007 Manual Intake | [Hi-Fi/wireframe-SCR-007-manual-intake.html](./Hi-Fi/wireframe-SCR-007-manual-intake.html) | Traditional form intake | High | 2026-03-16 |
| SCR-008 Document Upload | [Hi-Fi/wireframe-SCR-008-doc-upload.html](./Hi-Fi/wireframe-SCR-008-doc-upload.html) | Document management | High | 2026-03-16 |
| SCR-009 360° Patient View | [Hi-Fi/wireframe-SCR-009-360-view.html](./Hi-Fi/wireframe-SCR-009-360-view.html) | Comprehensive patient profile | High | 2026-03-16 |
| SCR-010 Appointment History | [Hi-Fi/wireframe-SCR-010-history.html](./Hi-Fi/wireframe-SCR-010-history.html) | Past appointments list | High | 2026-03-16 |
| SCR-011 Staff Dashboard | [Hi-Fi/wireframe-SCR-011-staff-dashboard.html](./Hi-Fi/wireframe-SCR-011-staff-dashboard.html) | Staff operations home | High | 2026-03-16 |
| SCR-012 Patient Search | [Hi-Fi/wireframe-SCR-012-patient-search.html](./Hi-Fi/wireframe-SCR-012-patient-search.html) | Patient lookup and selection | High | 2026-03-16 |
| SCR-013 Queue Management | [Hi-Fi/wireframe-SCR-013-queue.html](./Hi-Fi/wireframe-SCR-013-queue.html) | Real-time queue with actions | High | 2026-03-16 |
| SCR-014 Walk-in Booking | [Hi-Fi/wireframe-SCR-014-walkin.html](./Hi-Fi/wireframe-SCR-014-walkin.html) | Quick walk-in registration | High | 2026-03-16 |
| SCR-015 Conflict Review | [Hi-Fi/wireframe-SCR-015-conflict.html](./Hi-Fi/wireframe-SCR-015-conflict.html) | Data conflict resolution | High | 2026-03-16 |
| SCR-016 Code Verification | [Hi-Fi/wireframe-SCR-016-code-verify.html](./Hi-Fi/wireframe-SCR-016-code-verify.html) | ICD-10/CPT code verification | High | 2026-03-16 |
| SCR-017 Insurance Validation | [Hi-Fi/wireframe-SCR-017-insurance.html](./Hi-Fi/wireframe-SCR-017-insurance.html) | Insurance eligibility check | High | 2026-03-16 |
| SCR-018 Admin Dashboard | [Hi-Fi/wireframe-SCR-018-admin-dashboard.html](./Hi-Fi/wireframe-SCR-018-admin-dashboard.html) | Admin overview and stats | High | 2026-03-16 |
| SCR-019 User Management | [Hi-Fi/wireframe-SCR-019-user-mgmt.html](./Hi-Fi/wireframe-SCR-019-user-mgmt.html) | User CRUD with roles | High | 2026-03-16 |
| SCR-020 Role Configuration | [Hi-Fi/wireframe-SCR-020-roles.html](./Hi-Fi/wireframe-SCR-020-roles.html) | Permission management | High | 2026-03-16 |
| SCR-021 Audit Log Viewer | [Hi-Fi/wireframe-SCR-021-audit-logs.html](./Hi-Fi/wireframe-SCR-021-audit-logs.html) | Compliance audit trails | High | 2026-03-16 |

### Component Inventory
**Reference**: See [Component Inventory](./component-inventory.md) for detailed component documentation including:
- Complete component specifications
- Component states and variants
- Responsive behavior details
- Reusability analysis
- Implementation priorities

## 4. User Personas & Flows

### Persona 1: Patient
- **Role**: Healthcare Consumer
- **Goals**: Book appointments, complete intake forms, view health dashboard, upload documents
- **Key Screens**: SCR-001 through SCR-010
- **Primary Flow**: Landing → Sign In → Dashboard → Book Appointment → Confirm
- **Wireframe References**: SCR-003, SCR-004, SCR-005, SCR-006, SCR-007
- **Decision Points**: AI vs Manual intake mode selection

### Persona 2: Staff (Front Desk / Clinical)
- **Role**: Front Desk / Clinical Staff
- **Goals**: Manage walk-ins, monitor queues, mark arrivals, review conflicts, verify codes
- **Key Screens**: SCR-011 through SCR-017
- **Primary Flow**: Staff Dashboard → Queue → Patient Actions → Resolution
- **Wireframe References**: SCR-011, SCR-013, SCR-015, SCR-016
- **Decision Points**: Conflict resolution (accept/reject/escalate), code verification

### Persona 3: Admin
- **Role**: System Administrator
- **Goals**: Manage users, configure roles, monitor audit compliance
- **Key Screens**: SCR-018 through SCR-021
- **Primary Flow**: Admin Dashboard → User Management → Role Config → Audit Review
- **Wireframe References**: SCR-018, SCR-019, SCR-020, SCR-021
- **Decision Points**: User activation/deactivation, role assignments

### User Flow Diagrams
- **FL-001**: Patient Appointment Booking - [navigation-map.md](./navigation-map.md#fl-001)
- **FL-002**: Patient Intake (AI Mode) - [navigation-map.md](./navigation-map.md#fl-002)
- **FL-003**: Staff Walk-in Booking - [navigation-map.md](./navigation-map.md#fl-003)
- **FL-004**: Clinical Data Conflict Resolution - [navigation-map.md](./navigation-map.md#fl-004)
- **FL-005**: Medical Code Verification - [navigation-map.md](./navigation-map.md#fl-005)
- **FL-006**: Admin User Management - [navigation-map.md](./navigation-map.md#fl-006)

## 5. Screen Hierarchy

### Level 1: Public (Unauthenticated)
- **SCR-001 Landing Page** (P0 - Critical) - [wireframe-SCR-001-landing.html](./Hi-Fi/wireframe-SCR-001-landing.html)
  - Description: Public entry point with CTA to sign in/register
  - User Entry Point: Yes
  - Key Components: Header, Hero, Button, Link

- **SCR-002 Sign In** (P0 - Critical) - [wireframe-SCR-002-signin.html](./Hi-Fi/wireframe-SCR-002-signin.html)
  - Description: Authentication form with validation
  - User Entry Point: Yes
  - Key Components: TextField, PasswordField, Button, Alert

### Level 2: Patient Portal
- **SCR-003 Patient Dashboard** (P0 - Critical) - [wireframe-SCR-003-patient-dashboard.html](./Hi-Fi/wireframe-SCR-003-patient-dashboard.html)
  - Description: Patient home with 360-view summary, upcoming appointments
  - Key Components: StatCard, AppointmentCard, Header, Sidebar

- **SCR-004 Appointment Calendar** (P0 - Critical) - [wireframe-SCR-004-calendar.html](./Hi-Fi/wireframe-SCR-004-calendar.html)
  - Description: Interactive calendar for slot selection
  - Parent Screen: SCR-003
  - Key Components: Calendar, SlotButton, Button, Modal

- **SCR-005 Booking Confirmation** (P0 - Critical) - [wireframe-SCR-005-booking-confirm.html](./Hi-Fi/wireframe-SCR-005-booking-confirm.html)
  - Description: Appointment details and confirmation form
  - Parent Screen: SCR-004
  - Key Components: Form sections, TextField, Select, Button, Alert

- **SCR-006 AI Intake** (P0 - Critical) - [wireframe-SCR-006-ai-intake.html](./Hi-Fi/wireframe-SCR-006-ai-intake.html)
  - Description: Conversational AI-driven patient intake
  - Key Components: ChatBubble, TextField, Button, Card

- **SCR-007 Manual Intake Form** (P0 - Critical) - [wireframe-SCR-007-manual-intake.html](./Hi-Fi/wireframe-SCR-007-manual-intake.html)
  - Description: Traditional multi-step form intake
  - Key Components: TextField, Select, Checkbox, Radio, Button

- **SCR-008 Document Upload** (P0 - Critical) - [wireframe-SCR-008-doc-upload.html](./Hi-Fi/wireframe-SCR-008-doc-upload.html)
  - Description: Document upload and management interface
  - Key Components: FileUpload, DocumentCard, Button, ProgressBar, Badge

- **SCR-009 360-Degree Patient View** (P0 - Critical) - [wireframe-SCR-009-360-view.html](./Hi-Fi/wireframe-SCR-009-360-view.html)
  - Description: Comprehensive patient profile with medical history
  - Key Components: Tabs, Card, DataTable, Badge, Tag, VitalChart

- **SCR-010 Appointment History** (P1 - High Priority) - [wireframe-SCR-010-history.html](./Hi-Fi/wireframe-SCR-010-history.html)
  - Description: List of past appointments with details
  - Key Components: DataTable, Badge, Button, Pagination

### Level 3: Staff Portal
- **SCR-011 Staff Dashboard** (P0 - Critical) - [wireframe-SCR-011-staff-dashboard.html](./Hi-Fi/wireframe-SCR-011-staff-dashboard.html)
  - Description: Staff operations overview with queue stats
  - Key Components: StatCard, QueueList, Button, Badge

- **SCR-012 Patient Search** (P0 - Critical) - [wireframe-SCR-012-patient-search.html](./Hi-Fi/wireframe-SCR-012-patient-search.html)
  - Description: Patient lookup with filters
  - Key Components: SearchField, DataTable, Button, Badge

- **SCR-013 Queue Management** (P0 - Critical) - [wireframe-SCR-013-queue.html](./Hi-Fi/wireframe-SCR-013-queue.html)
  - Description: Real-time queue with bulk actions
  - Key Components: DataTable, Badge, Button, Checkbox, Timer

- **SCR-014 Walk-in Booking** (P0 - Critical) - [wireframe-SCR-014-walkin.html](./Hi-Fi/wireframe-SCR-014-walkin.html)
  - Description: Quick walk-in patient registration
  - Key Components: SearchField, TextField, Select, Button

- **SCR-015 Conflict Review** (P0 - Critical) - [wireframe-SCR-015-conflict.html](./Hi-Fi/wireframe-SCR-015-conflict.html)
  - Description: Clinical data conflict resolution interface
  - Key Components: Card, ComparePanel, Button, Badge

- **SCR-016 Code Verification** (P0 - Critical) - [wireframe-SCR-016-code-verify.html](./Hi-Fi/wireframe-SCR-016-code-verify.html)
  - Description: AI-suggested medical code verification
  - Key Components: DataTable, CodeCard, Badge, Button

- **SCR-017 Insurance Validation** (P1 - High Priority) - [wireframe-SCR-017-insurance.html](./Hi-Fi/wireframe-SCR-017-insurance.html)
  - Description: Insurance eligibility verification
  - Key Components: TextField, Button, Alert

### Level 4: Admin Portal
- **SCR-018 Admin Dashboard** (P0 - Critical) - [wireframe-SCR-018-admin-dashboard.html](./Hi-Fi/wireframe-SCR-018-admin-dashboard.html)
  - Description: Admin overview with system stats
  - Key Components: StatCard, ActivityFeed, Card

- **SCR-019 User Management** (P0 - Critical) - [wireframe-SCR-019-user-mgmt.html](./Hi-Fi/wireframe-SCR-019-user-mgmt.html)
  - Description: User CRUD operations with role assignment
  - Key Components: DataTable, Modal, Button, Badge, Pagination

- **SCR-020 Role Configuration** (P1 - High Priority) - [wireframe-SCR-020-roles.html](./Hi-Fi/wireframe-SCR-020-roles.html)
  - Description: Permission and role management
  - Key Components: Card, Toggle, Checkbox, Button

- **SCR-021 Audit Log Viewer** (P0 - Critical) - [wireframe-SCR-021-audit-logs.html](./Hi-Fi/wireframe-SCR-021-audit-logs.html)
  - Description: Compliance audit trail viewer
  - Key Components: DataTable, SearchField, DatePicker, Select, Button, Pagination

### Screen Priority Legend
- **P0**: Critical path screens (must-have) - 18 screens
- **P1**: High-priority screens (core functionality) - 3 screens
- **P2**: Medium-priority screens (important features) - 0 screens
- **P3**: Low-priority screens (nice-to-have) - 0 screens

### Modal/Dialog/Overlay Inventory

| Modal/Dialog Name | Type | Trigger Context | Parent Screen | Wireframe Reference | Priority |
|------------------|------|----------------|---------------|-------------------|----------|
| Session Timeout Warning | Modal | 13 min inactivity | All authenticated | Integrated state in all screens | P0 |
| Confirm Booking | Modal | Submit booking | SCR-005 | Integrated in SCR-005 | P0 |
| Cancel Appointment | Dialog | Cancel action | SCR-003, SCR-010 | Integrated in screens | P0 |
| Document Preview | Drawer | Click document | SCR-008, SCR-009 | Right-side drawer | P0 |
| Conflict Detail | Drawer | Click conflict | SCR-015 | Right-side drawer | P0 |
| Code Evidence | Drawer | Click code | SCR-016 | Right-side drawer | P0 |
| Create User | Modal | Add user action | SCR-019 | Integrated in SCR-019 | P0 |
| Swap Preference | Modal | Indicate preferred slot | SCR-004 | Integrated in SCR-004 | P1 |
| Filter Queue | Drawer (mobile) | Filter action | SCR-013 | Mobile-specific | P1 |

**Modal Behavior Notes:**
- **Responsive Behavior:** Desktop modal (centered) → Mobile full-screen transformation
- **Trigger Actions:** Buttons with aria-haspopup="dialog"
- **Dismissal Actions:** Close button, overlay click, ESC key
- **Focus Management:** Tab trap within modal, return focus on close
- **Accessibility:** role="dialog", aria-labelledby, aria-describedby

## 6. Navigation Architecture

```
UPACIP Navigation Structure
├── Public
│   ├── SCR-001 Landing Page (wireframe-SCR-001-landing.html)
│   └── SCR-002 Sign In (wireframe-SCR-002-signin.html)
│
├── Patient Portal (Blue Accent #2E8FE5)
│   ├── SCR-003 Dashboard (wireframe-SCR-003-patient-dashboard.html)
│   │   └── Quick Actions → SCR-004, SCR-006, SCR-008
│   ├── SCR-004 Appointment Calendar (wireframe-SCR-004-calendar.html)
│   │   └── SCR-005 Booking Confirmation (wireframe-SCR-005-booking-confirm.html)
│   ├── SCR-006 AI Intake (wireframe-SCR-006-ai-intake.html)
│   │   └── Switch → SCR-007 Manual Intake
│   ├── SCR-007 Manual Intake Form (wireframe-SCR-007-manual-intake.html)
│   │   └── Switch → SCR-006 AI Intake
│   ├── SCR-008 Document Upload (wireframe-SCR-008-doc-upload.html)
│   ├── SCR-009 360-Degree View (wireframe-SCR-009-360-view.html)
│   └── SCR-010 Appointment History (wireframe-SCR-010-history.html)
│
├── Staff Portal (Teal Accent #26BCAF)
│   ├── SCR-011 Staff Dashboard (wireframe-SCR-011-staff-dashboard.html)
│   │   └── Quick Actions → SCR-013, SCR-014, SCR-015, SCR-016
│   ├── SCR-012 Patient Search (wireframe-SCR-012-patient-search.html)
│   │   └── Select Patient → SCR-009
│   ├── SCR-013 Queue Management (wireframe-SCR-013-queue.html)
│   ├── SCR-014 Walk-in Booking (wireframe-SCR-014-walkin.html)
│   │   └── Success → SCR-013
│   ├── SCR-015 Conflict Review (wireframe-SCR-015-conflict.html)
│   ├── SCR-016 Code Verification (wireframe-SCR-016-code-verify.html)
│   └── SCR-017 Insurance Validation (wireframe-SCR-017-insurance.html)
│
└── Admin Portal (Orange Accent #FF8F2D)
    ├── SCR-018 Admin Dashboard (wireframe-SCR-018-admin-dashboard.html)
    ├── SCR-019 User Management (wireframe-SCR-019-user-mgmt.html)
    ├── SCR-020 Role Configuration (wireframe-SCR-020-roles.html)
    └── SCR-021 Audit Log Viewer (wireframe-SCR-021-audit-logs.html)
```

### Navigation Patterns
- **Primary Navigation**: Collapsible sidebar (Staff/Admin) or top nav (Patient)
- **Secondary Navigation**: Breadcrumbs on desktop below header
- **Mobile Navigation**: Bottom nav bar with 4 primary items + hamburger menu

## 7. Interaction Patterns

### Pattern 1: Appointment Booking (FL-001)
- **Trigger**: Click "Book Appointment" from SCR-003
- **Flow**: Calendar → Select Slot → Review → Confirm
- **Screens Involved**: SCR-003, SCR-004, SCR-005
- **Feedback**: Toast notification on success
- **Components Used**: Calendar, SlotButton, Modal, Button, Toast

### Pattern 2: AI Intake with Manual Fallback (FL-002)
- **Trigger**: Click "Complete Intake" from SCR-003
- **Flow**: AI Chat → Review → Confirm (or Switch → Manual Form)
- **Screens Involved**: SCR-006, SCR-007
- **Feedback**: Progress indicator, data preservation on mode switch
- **Components Used**: ChatBubble, TextField, Toggle, Form fields

### Pattern 3: Conflict Resolution (FL-004)
- **Trigger**: Click conflict notification from SCR-011
- **Flow**: Review conflicts → Compare values → Select/Escalate
- **Screens Involved**: SCR-011, SCR-015
- **Feedback**: Resolution confirmation, audit log entry
- **Components Used**: Card, ComparePanel, Radio, Button, Drawer

### Pattern 4: Code Verification (FL-005)
- **Trigger**: Click "Pending Codes" from SCR-011
- **Flow**: Review codes → View evidence → Confirm/Modify/Reject
- **Screens Involved**: SCR-011, SCR-016
- **Feedback**: Confidence badges update, verification status
- **Components Used**: DataTable, CodeCard, Badge, Drawer

## 8. Error Handling

### Error Scenario 1: Network Error
- **Trigger**: API request fails
- **Error Screen/State**: Inline error alert with retry action
- **User Action**: Retry or contact support
- **Recovery Flow**: Retry → Success → Continue

### Error Scenario 2: Session Timeout
- **Trigger**: 15-min inactivity
- **Error Screen/State**: Modal warning at 13 min, then logout
- **User Action**: Extend session or logout
- **Recovery Flow**: Re-authenticate → Data restored from auto-save

### Error Scenario 3: Validation Error
- **Trigger**: Invalid form input
- **Error Screen/State**: Inline field errors with red border
- **User Action**: Correct input values
- **Recovery Flow**: Fix errors → Re-submit

### Error Scenario 4: AI Service Unavailable
- **Trigger**: AI engine fails (SCR-006)
- **Error Screen/State**: Degraded banner with manual fallback
- **User Action**: Continue with manual intake (SCR-007)
- **Recovery Flow**: Mode switch preserves entered data

## 9. Responsive Strategy

| Breakpoint | Width | Layout Changes | Navigation Changes | Component Adaptations |
|-----------|-------|----------------|-------------------|---------------------|
| Mobile | 375px | Single column, stacked | Bottom nav bar | Touch targets 44px+, full-width inputs |
| Tablet | 768px | 2-column grid | Collapsed sidebar (icons) | Calendar week view |
| Desktop | 1440px | Multi-column, sidebar 280px | Expanded sidebar | Full data tables, month view |

### Responsive Wireframe Variants
- Primary viewport: Desktop (1440px) - all screens
- Key mobile adaptations documented in component-inventory.md

## 10. Accessibility

### WCAG Compliance
- **Target Level**: WCAG 2.2 AA
- **Color Contrast**: Minimum 4.5:1 for text, 3:1 for UI
- **Keyboard Navigation**: Full keyboard operability
- **Screen Reader Support**: Semantic HTML, ARIA labels

### Accessibility Considerations by Screen
| Screen | Key Accessibility Features | Notes |
|--------|---------------------------|-------|
| All screens | Focus indicators 3px with ≥3:1 contrast | UXR-202 |
| SCR-004 Calendar | Keyboard date navigation | UXR-303 |
| SCR-006 AI Intake | Live regions for AI responses | UXR-203 |
| SCR-013 Queue | Sort announcements, row selection | UXR-106 |

### Focus Order
- Header → Primary content → Sidebar (desktop)
- Header → Primary content → Bottom nav (mobile)
- Modals trap focus until dismissed

## 11. Content Strategy

### Content Hierarchy
- **H1**: One per page, page title (typography.heading.h1 - 24px)
- **H2**: Section headers (typography.heading.h2 - 20px)
- **Body Text**: 16px for primary content
- **Placeholder Content**: Lorem ipsum with realistic length

### Content Types by Screen
| Screen | Content Types | Notes |
|--------|--------------|-------|
| SCR-003 Dashboard | Stats, cards, lists | 4 StatCards + appointment list |
| SCR-006 AI Intake | Chat bubbles, suggestions | AI-generated content |
| SCR-009 360-View | Tabs, charts, tables | Dense data display |
| SCR-021 Audit Logs | Table rows, filters | Paginated data |
