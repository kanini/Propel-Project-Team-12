# Information Architecture - PulseCare Platform Wireframes

**Document Version**: 1.0.0  
**Generated**: March 19, 2026  
**Wireframe Fidelity**: High-Fidelity (Production-Ready Mockups)  
**Source**: `.propel/context/docs/figma_spec.md`  
**Technology Stack**: React 18+, Next.js 14+, Tailwind CSS 3.4+, shadcn/ui

---

## 1. Site Map Overview

The PulseCare platform consists of 18 screens organized across four main sections: Authentication (public), Patient Portal, Staff Portal, and Admin Panel. Each section implements role-based access control (RBAC) per FR-028.

```
PulseCare Platform (Unified Patient Access & Clinical Intelligence)
│
├── Authentication (Public Access)
│   ├── SCR-013: Login
│   ├── SCR-014: Password Reset
│   └── SCR-015: Account Activation
│
├── Patient Portal (Role: Patient)
│   ├── SCR-001: Patient Dashboard (Entry Point)
│   ├── SCR-002: Appointment Slot Selection & Booking
│   ├── SCR-003: Clinical Document Upload
│   ├── SCR-004: AI Conversational Intake
│   ├── SCR-005: Manual Form Intake
│   └── SCR-016: Appointment History
│
├── Staff Portal (Role: Staff)
│   ├── SCR-006: Walk-In Management
│   ├── SCR-007: Arrival Management / Same-Day Queue (Entry Point)
│   ├── SCR-008: Insurance Pre-Check Validation
│   ├── SCR-009: 360-Degree Patient View / Clinical Review
│   └── SCR-017: Clinical Coding Review (ICD-10/CPT)
│
└── Admin Panel (Role: Admin)
    ├── SCR-010: User Management List (Entry Point)
    ├── SCR-011: Create/Edit User Form
    ├── SCR-012: Audit Log Viewer
    └── SCR-018: System Configuration
```

---

## 2. Navigation Hierarchy

### 2.1 Primary Navigation Patterns

**Desktop (≥1024px)**: 
- Persistent left sidebar (240px fixed width) for Patient Portal
- Top horizontal navigation bar for Staff Portal and Admin Panel
- Always-visible user menu in top-right header

**Tablet (768-1023px)**:
- Collapsible sidebar (hamburger menu) for Patient Portal
- Top navigation bar for Staff Portal and Admin Panel
- User menu accessible via avatar dropdown

**Mobile (320-767px)**:
- Fixed bottom navigation bar (4-5 core actions, 44px tap targets per UXR-205)
- Hamburger menu for secondary actions
- User menu via avatar tap

### 2.2 Role-Based Navigation (UXR-102)

**Patient Navigation Items**:
- Dashboard (SCR-001)
- Book Appointment (SCR-002)
- Intake Form (SCR-004/005 toggle)
- Documents (SCR-003)
- Appointment History (SCR-016)

**Staff Navigation Items**:
- Arrivals (SCR-007) - Primary entry point
- Walk-Ins (SCR-006)
- Clinical Review (SCR-009)
- Insurance (SCR-008)
- Coding Review (SCR-017)

**Admin Navigation Items**:
- User Management (SCR-010) - Primary entry point
- Audit Logs (SCR-012)
- System Configuration (SCR-018)

### 2.3 Secondary Navigation

**Tabs (Within Screens)**:
- SCR-009 (360° Patient View): Timeline, Documents, Medications, Conflicts

**Modals/Overlays**:
- Session Timeout Warning (13-minute inactivity alert per UXR-603)
- Booking Confirmation (SCR-002)
- Intake Toggle Confirmation (SCR-004/005)
- Clinical Data Conflict Alert (SCR-009 per UXR-604)

---

## 3. Screen Inventory with Information Hierarchy

### 3.1 Authentication Screens

#### SCR-013: Login
- **Purpose**: Authenticate users and route to role-appropriate dashboard
- **Information Hierarchy**:
  1. Brand logo and tagline
  2. Email input (primary identifier)
  3. Password input
  4. Remember me checkbox + Forgot password link
  5. Primary CTA: "Sign in" button
  6. Secondary: Create account link (disabled - Phase 1)
- **Navigation Targets**: SCR-001 (Patient), SCR-007 (Staff), SCR-010 (Admin)
- **Wireframe**: `wireframe-SCR-013-login.html`

#### SCR-014: Password Reset
- **Purpose**: Self-service password recovery
- **Information Hierarchy**:
  1. Instructional heading: "Reset your password"
  2. Email input
  3. Primary CTA: "Send reset link" button
  4. Back to login link
- **Navigation Targets**: SCR-013 (Login)
- **Wireframe**: `wireframe-SCR-014-password-reset.html` *(pending)*

#### SCR-015: Account Activation
- **Purpose**: New user account setup from activation email
- **Information Hierarchy**:
  1. Welcome message
  2. New password input
  3. Confirm password input
  4. Primary CTA: "Activate account" button
- **Navigation Targets**: SCR-013 (Login with success toast)
- **Wireframe**: `wireframe-SCR-015-account-activation.html` *(pending)*

---

### 3.2 Patient Portal Screens

#### SCR-001: Patient Dashboard
- **Purpose**: Patient home screen with quick actions and appointment overview
- **Information Hierarchy**: (Grid Layout - 3 columns desktop, 2 tablet, 1 mobile)
  1. Welcome header: "Welcome back, [Name]"
  2. Quick Action Card: Book Appointment (gradient CTA)
  3. Upcoming Appointments Card (2-column span):
     - Next appointment details (date, time, provider, location)
     - Status badge (Confirmed/Pending)
     - View All link → SCR-016
  4. Complete Intake Form Card (action required badge if incomplete)
  5. Upload Documents Card (recent uploads summary)
  6. Recent Documents Card (last 3 uploaded files)
- **Navigation Targets**: SCR-002 (Book), SCR-003 (Documents), SCR-004 (Intake), SCR-016 (History)
- **Wireframe**: `wireframe-SCR-001-patient-dashboard.html`

#### SCR-002: Appointment Slot Selection & Booking
- **Purpose**: Real-time slot availability with booking confirmation flow
- **Information Hierarchy**:
  1. Page header: "Book Appointment"
  2. Filter bar (3-column):
     - Provider dropdown
     - Appointment type dropdown
     - Date picker (min: today)
  3. Preferred Slot Toggle (FR-003 - blue info box)
  4. Available Slots Grid:
     - Legend: Available (white), Selected (blue), Booked (gray)
     - Time slot buttons (6 columns desktop, 4 tablet, 2 mobile)
     - Visual hierarchy: Morning slots → Afternoon slots
  5. Selected Slot Summary (conditional):
     - Date, Time, Provider display
     - Primary CTA: "Confirm Booking" button
  6. Confirmation Modal (overlay):
     - Appointment details review
     - Cancel/Confirm actions
- **Navigation Targets**: SCR-001 (Dashboard after booking), SCR-016 (History link)
- **UXR Coverage**: UXR-101 (3-click booking), UXR-505 (immediate feedback), UXR-605 (conflict handling)
- **Wireframe**: `wireframe-SCR-002-slot-selection.html`

#### SCR-003: Clinical Document Upload
- **Purpose**: Patient uploads lab results, insurance cards, medical records
- **Information Hierarchy**:
  1. Page header: "Upload Documents"
  2. File dropzone (drag-and-drop enabled):
     - Accepted formats: PDF, JPG, PNG
     - Max size: 10 MB per file
  3. Upload queue (files being processed):
     - Filename, size, progress bar
     - AI extraction status badge (Pending/Processing/Complete)
  4. Uploaded Documents Table:
     - Filename, Type, Upload Date, Extraction Status
     - Actions: View, Download, Delete
- **Navigation Targets**: SCR-009 (Staff reviews documents)
- **UXR Coverage**: UXR-502 (AI extraction loading states)
- **Wireframe**: `wireframe-SCR-003-document-upload.html` *(pending)*

#### SCR-004: AI Conversational Intake
- **Purpose**: Collect patient intake data via conversational AI
- **Information Hierarchy**:
  1. Page header: "Complete Intake Form" + Toggle switch → SCR-005
  2. Chat interface:
     - AI avatar (left) + Patient avatar (right)
     - Message bubbles with timestamps
     - Typing indicator (when AI generating response)
  3. Input area:
     - Text input field
     - Send button
     - Voice input button (future feature)
  4. Progress indicator: "3 of 8 sections complete"
  5. Action buttons:
     - "Save & Continue Later"
     - "Review & Submit" (enabled when complete)
- **Navigation Targets**: SCR-005 (toggle manual), SCR-001 (Dashboard after submit)
- **UXR Coverage**: UXR-103 (toggle visibility), UXR-502 (AI response loading)
- **Wireframe**: `wireframe-SCR-004-ai-intake.html` *(pending)*

#### SCR-005: Manual Form Intake
- **Purpose**: Traditional form-based intake data collection
- **Information Hierarchy**: (Single column, max-width 672px)
  1. Page header: "Complete Intake Form" + Toggle switch → SCR-004
  2. Form sections (accordions):
     - Demographics (Name, DOB, Gender, Contact)
     - Insurance Information
     - Medical History (Conditions, Surgeries)
     - Medications (Name, Dosage, Frequency)
     - Allergies
     - Reason for Visit
  3. Inline validation (UXR-504):
     - Email format check
     - Required field indicators (red border + error message)
     - Valid field checkmarks (green)
  4. Action buttons:
     - "Save Draft"
     - "Continue" (proceeds to review)
- **Navigation Targets**: SCR-004 (toggle AI), SCR-001 (Dashboard after submit)
- **UXR Coverage**: UXR-103 (toggle), UXR-504 (inline validation)
- **Wireframe**: `wireframe-SCR-005-manual-intake.html` *(pending)*

#### SCR-016: Appointment History
- **Purpose**: View past and upcoming appointments
- **Information Hierarchy**:
  1. Page header: "Appointment History"
  2. Filter bar: Date range picker + Status filter
  3. Appointments Table:
     - Date, Time, Provider, Type, Status badge, Actions
     - Pagination (10 per page)
  4. Actions per row:
     - View Details
     - Reschedule (if future)
     - Cancel (if future)
- **Navigation Targets**: SCR-002 (Reschedule), SCR-001 (Dashboard)
- **Wireframe**: `wireframe-SCR-016-appointment-history.html` *(pending)*

---

### 3.3 Staff Portal Screens

#### SCR-006: Walk-In Management
- **Purpose**: Staff creates appointment for patient arriving without booking
- **Information Hierarchy**:
  1. Modal/Screen header: "New Walk-In Appointment"
  2. Patient search:
     - Search by name, DOB, phone
     - Results list (name, DOB, last visit)
  3. Patient form (if new):
     - Name, DOB, Phone, Reason for Visit
  4. Appointment details:
     - Provider dropdown
     - Time slot selection
     - Reason for visit text area
  5. Account creation prompt (optional):
     - "Create portal account for this patient?"
     - Email input
- **Navigation Targets**: SCR-007 (Arrivals queue after creation)
- **UXR Coverage**: UXR-102 (staff navigation), UXR-504 (form validation)
- **Wireframe**: `wireframe-SCR-006-walk-in-management.html` *(pending)*

#### SCR-007: Staff Arrival Management / Same-Day Queue
- **Purpose**: Staff marks patient arrivals, manages queue, prioritizes order
- **Information Hierarchy**:
  1. Page header: "Today's Arrivals" + "New Walk-In" button (top-right)
  2. Stats Cards (4-column grid):
     - Total Today (count)
     - Arrived (green count)
     - Waiting (yellow count)
     - Late >15 min (red count)
  3. Search & Filter bar:
     - Search by name, DOB, ID
     - Status filter dropdown
  4. Arrival Queue Table (UXR-104):
     - Drag handles for reorder
     - Columns: Patient (avatar + name), DOB, Time, Provider, Status badge, Wait Time, Actions
     - Color-coded left border: Green (Arrived), Red (Late), Gray (Scheduled)
     - Status badges: Scheduled, Arrived, Late, Ready for Provider
  5. Actions per row:
     - "Check In" button (if not arrived)
     - "View" link → SCR-009 (360° View)
     - "Validate" link → SCR-008 (Insurance)
- **Navigation Targets**: SCR-006 (Walk-In), SCR-009 (Clinical Review), SCR-008 (Insurance)
- **UXR Coverage**: UXR-104 (drag-drop queue), UXR-501 (immediate feedback)
- **Wireframe**: `wireframe-SCR-007-arrival-management.html`

#### SCR-008: Insurance Pre-Check Validation
- **Purpose**: Staff validates insurance information against dummy database
- **Information Hierarchy**:
  1. Page header: "Insurance Validation"
  2. Patient Info Card:
     - Name, DOB, Appointment Time
  3. Insurance Input Fields:
     - Insurance Provider Name
     - Policy/Member ID
  4. Action buttons:
     - "Validate" button (primary)
     - "Skip" button (secondary)
  5. Validation Result Alert (conditional):
     - Success (green): "Insurance verified"
     - Warning (yellow): "Verification pending - proceed with caution"
     - Error (red): "No matching record - manual verification required"
- **Navigation Targets**: SCR-007 (Back to queue)
- **UXR Coverage**: UXR-601 (clear error messages), UXR-602 (recovery paths)
- **Wireframe**: `wireframe-SCR-008-insurance-validation.html` *(pending)*

#### SCR-009: 360-Degree Patient View / Clinical Review
- **Purpose**: Staff reviews consolidated patient data from multiple sources
- **Information Hierarchy**:
  1. Patient Header:
     - Avatar, Name, DOB, Contact, MRN
     - Action buttons: Edit, Print Summary, Mark Ready for Provider
  2. Conflict Alert Banner (UXR-604):
     - Red banner if critical conflicts detected
     - "2 conflicts require attention" message
     - List of conflicts with source documents
  3. Tabs (secondary navigation):
     - **Timeline Tab** (default):
       - Chronological event list (appointments, documents, notes)
       - Filterable by event type
     - **Documents Tab**:
       - Table: Document Name, Type, Upload Date, Extraction Status badge
       - Confidence badges: High (green), Medium (yellow), Low (red - review required)
       - Actions: View PDF, Verify Extraction, Flag for Review
     - **Medications Tab**:
       - Active medications list (name, dosage, frequency, start date)
       - Allergy warnings (red alert if conflicts)
     - **Conflicts Tab** (only if conflicts exist):
       - Conflicting data items with source comparison
       - Actions: Accept Source A, Accept Source B, Escalate to Provider
- **Navigation Targets**: SCR-007 (Back to queue), SCR-017 (Coding Review)
- **UXR Coverage**: UXR-105 (one-click access), UXR-604 (conflict UI), UXR-502 (AI extraction loading)
- **Wireframe**: `wireframe-SCR-009-360-patient-view.html` *(pending)*

#### SCR-017: Clinical Coding Review (ICD-10/CPT)
- **Purpose**: Staff reviews AI-suggested medical codes
- **Information Hierarchy**:
  1. Page header: "Clinical Coding Review - [Patient Name]"
  2. Code Suggestions Cards:
     - ICD-10 Code Suggestions (accordion):
       - Code ID, Description, Confidence Score badge
       - Supporting evidence from clinical notes
       - Actions: Accept, Reject, Edit
     - CPT Code Suggestions (accordion):
       - Code ID, Description, Reimbursement estimate
       - Actions: Accept, Reject, Add Code
  3. Manual Code Entry:
     - ICD-10 search input (autocomplete)
     - CPT search input (autocomplete)
  4. Action buttons:
     - "Save & Submit to Billing" (primary)
     - "Flag for Provider Review" (secondary)
- **Navigation Targets**: SCR-009 (Back to Patient View)
- **Wireframe**: `wireframe-SCR-017-coding-review.html` *(pending)*

---

### 3.4 Admin Panel Screens

#### SCR-010: Admin User Management List
- **Purpose**: Admin views all user accounts with filtering and actions
- **Information Hierarchy**:
  1. Page header: "User Management" + "Create New User" button (top-right)
  2. Search & Filter bar:
     - Search by name, email
     - Role filter dropdown (All, Patient, Staff, Admin)
     - Status filter (Active, Inactive, Pending Activation)
  3. User Table:
     - Columns: Name, Email, Role badge, Status badge, Last Login, Actions
     - Pagination (20 per page)
  4. Actions per row:
     - Edit → SCR-011
     - Deactivate/Activate toggle
     - Reset Password
     - Delete (with confirmation)
- **Navigation Targets**: SCR-011 (Create/Edit User), SCR-012 (Audit Logs)
- **UXR Coverage**: UXR-102 (admin navigation), UXR-601 (clear actions)
- **Wireframe**: `wireframe-SCR-010-user-management-list.html` *(pending)*

#### SCR-011: Admin Create/Edit User Form
- **Purpose**: Admin creates new user or edits existing account
- **Information Hierarchy**: (Single column, max-width 672px)
  1. Page header: "Create New User" (or "Edit User - [Name]")
  2. Form sections:
     - Basic Information:
       - First Name, Last Name, Email
     - Role Assignment:
       - Role dropdown (Patient, Staff, Admin)
     - Permissions (checkbox group, visible if Staff/Admin):
       - View Patient Data
       - Edit Patient Data
       - Manage Users
       - View Audit Logs
       - Manage System Settings
  3. Action buttons:
     - "Create User" / "Save Changes" (primary)
     - "Cancel" (secondary)
- **Navigation Targets**: SCR-010 (User List after save)
- **UXR Coverage**: UXR-504 (inline validation)
- **Wireframe**: `wireframe-SCR-011-create-edit-user.html` *(pending)*

#### SCR-012: Audit Log Viewer
- **Purpose**: Admin reviews immutable audit trail of all system actions
- **Information Hierarchy**:
  1. Page header: "Audit Logs"
  2. Filter bar:
     - Date range picker (start/end)
     - User filter (dropdown)
     - Action type filter (Login, Data Access, Data Modification, User Management)
  3. Audit Log Table:
     - Columns: Timestamp, User (name + role), Action, Target (patient ID/record ID), IP Address
     - Pagination (50 per page)
     - Export CSV button
  4. Detail modal (click row):
     - Full action details
     - Before/After values (for modifications)
     - Session information
- **Navigation Targets**: SCR-010 (User Management)
- **Wireframe**: `wireframe-SCR-012-audit-logs.html` *(pending)*

#### SCR-018: System Configuration
- **Purpose**: Admin manages system settings and feature flags
- **Information Hierarchy**:
  1. Page header: "System Configuration"
  2. Configuration sections (tabs):
     - General Settings:
       - Session timeout duration (15 min default)
       - Reminder timing (24h/48h toggles)
     - Feature Flags:
       - Enable/Disable toggles for features
     - Health Checks:
       - API status indicators (green/red)
       - Database connection status
       - External service status (calendar APIs)
  3. Action buttons:
     - "Save Changes" (primary)
     - "Reset to Defaults" (secondary, with confirmation)
- **Navigation Targets**: SCR-010 (User Management Home)
- **Wireframe**: `wireframe-SCR-018-system-configuration.html` *(pending)*

---

## 4. Content Hierarchy Principles

### 4.1 Visual Hierarchy Rules

**Typography Hierarchy** (from designsystem.md):
- **H1** (text-4xl/36px): Page titles - one per screen
- **H2** (text-3xl/30px): Major section headings
- **H3** (text-2xl/24px): Subsection headings, card titles
- **H4** (text-xl/20px): Form section labels
- **Body** (text-base/16px): Paragraphs, descriptions, form help text
- **Caption** (text-sm/14px, text-xs/12px): Metadata, timestamps

**Color Hierarchy** (semantic):
- **Primary Actions**: Blue (#3b82f6) - CTAs, links, focus states
- **Success States**: Green (#22c55e) - Checkmarks, "Arrived" badges
- **Warning States**: Yellow (#eab308) - "Review Required", low-confidence badges
- **Error States**: Red (#ef4444) - Validation errors, conflicts, late arrivals
- **Neutral Text**: Gray scale (#374151 to #9ca3af) - Body text, metadata

**Spacing Hierarchy** (8px base unit):
- **Tight** (spacing-1 to -2): Icon-text gaps, badge padding
- **Default** (spacing-4): Button padding, card padding, form input padding
- **Component** (spacing-6 to -8): Gaps between form fields, list items
- **Section** (spacing-12 to -16): Major page section separation

### 4.2 Progressive Disclosure Strategy

**Patient Portal**: Streamlined, minimal information density
- Dashboard cards reveal details on hover
- Intake forms use accordions to show one section at a time
- Empty states with clear CTAs when no data present

**Staff Portal**: High information density for efficiency
- Data tables show all critical info upfront
- Expandable rows for additional details
- Drag-drop reordering visible without clicks

**Admin Panel**: Balanced density with search/filter emphasis
- Tables with pagination for large datasets
- Modal overlays for detailed views/edits
- Feature flags with instant toggle feedback

---

## 5. Responsive Breakpoint Strategy

### 5.1 Layout Transformations

**Desktop (1440px)**:
- Multi-column dashboards (3-4 columns)
- Persistent sidebar navigation (240px)
- Data tables expanded with all columns visible
- Modals centered with max-width 600px

**Tablet (768px)**:
- Two-column dashboard layout
- Collapsible sidebar (hamburger menu)
- Tables: horizontal scroll within container (preserve columns)
- Forms: full-width single column

**Mobile (375px)**:
- Single-column stack layout
- Bottom navigation bar (fixed, 4-5 actions)
- Tables: transform to card-based list view (label-value pairs)
- Forms: max-width 448px, single column

### 5.2 Type Scale Responsiveness

**Headings**: Scale down one size at mobile
- H1: text-4xl (36px) → text-3xl (30px mobile)
- H2: text-3xl (30px) → text-2xl (24px mobile)

**Touch Targets**: Minimum 44x44px (UXR-205)
- Buttons: py-3 (12px) + px-4 (16px) = 48px height minimum
- Mobile nav icons: 44x44px tap areas
- Table row actions: 44px height minimum on mobile

---

## 6. Empty State & Error State Patterns

### 6.1 Empty States

**Patient Dashboard - No Appointments**:
- Icon: Calendar (gray-300)
- Message: "No upcoming appointments"
- CTA: "Book your first appointment →" (blue link to SCR-002)

**Document Upload - No Documents**:
- Icon: Document (gray-300)
- Message: "No documents uploaded yet"
- CTA: "Upload your first document" (primary button)

**Staff Arrival Queue - No Patients**:
- Icon: Users (gray-300)
- Message: "No patients scheduled for today"
- CTA: "Create Walk-In Appointment" (primary button)

### 6.2 Error State Patterns (UXR-601, UXR-602)

**Form Validation Errors**:
- Inline: Red border + error text below field
- Summary: Red alert banner at top: "Please fix 3 errors below"
- Recovery: Click error in summary scrolls to field

**Slot Booking Conflict (UXR-605)**:
- Toast notification (top-right): Red border
- Message: "This slot was just booked by another patient. Please select another time."
- Recovery: Previously selected slot highlighted but disabled; alternative slots suggested

**Session Timeout Warning (UXR-603)**:
- Modal overlay (center screen)
- Icon: Yellow warning triangle
- Message: "You'll be logged out in 2:00 minutes due to inactivity."
- Recovery: "Stay Logged In" button (prevents logout)

---

## 7. Accessibility Considerations (UXR-201-206)

### 7.1 Keyboard Navigation

All interactive elements keyboard-navigable:
- Tab order: Logical (top-to-bottom, left-to-right)
- Enter: Activates buttons, submits forms
- Escape: Closes modals/overlays
- Arrow keys: Navigate data tables, lists

### 7.2 Screen Reader Support

- ARIA labels on all interactive elements
- ARIA live regions for status updates ("Patient checked in")
- Skip navigation links at top of page
- Table headers properly associated with cells (`<th>` with `scope="col"`)

### 7.3 Focus Indicators

- 2px solid blue outline on focused elements (UXR-206)
- High contrast: 3:1 ratio minimum
- Focus never hidden by other elements

---

## 8. Traceability Matrix

| Screen ID | UXR Requirements Applied | Priority | Status |
|-----------|--------------------------|----------|--------|
| SCR-001 | UXR-101, 102, 201-206, 301-305, 401-404, 501, 603 | P0 | Wireframe Complete |
| SCR-002 | UXR-101, 105, 201-206, 301-305, 401-404, 501, 502, 505, 601, 602, 605 | P0 | Wireframe Complete |
| SCR-003 | UXR-201-206, 301-305, 401-404, 502, 601, 602 | P0 | Wireframe Pending |
| SCR-004 | UXR-103, 201-206, 301-305, 401-404, 501, 502, 601, 602 | P0 | Wireframe Pending |
| SCR-005 | UXR-103, 201-206, 301-305, 401-404, 504, 601, 602 | P0 | Wireframe Pending |
| SCR-006 | UXR-102, 201-206, 302-303, 401-404, 501, 504, 601, 602 | P0 | Wireframe Pending |
| SCR-007 | UXR-104, 102, 201-206, 302-303, 401-404, 501, 601, 602 | P0 | Wireframe Complete |
| SCR-008 | UXR-102, 201-206, 302-303, 401-404, 501, 601, 602 | P1 | Wireframe Pending |
| SCR-009 | UXR-105, 102, 201-206, 302-303, 401-404, 502, 601, 602, 604 | P0 | Wireframe Pending |
| SCR-010 | UXR-102, 201-206, 303, 401-404, 501, 601, 602 | P0 | Wireframe Pending |
| SCR-011 | UXR-102, 201-206, 303, 401-404, 504, 601, 602 | P0 | Wireframe Pending |
| SCR-012 | UXR-102, 201-206, 303, 401-404, 601, 602 | P1 | Wireframe Pending |
| SCR-013 | UXR-201-206, 301-305, 401-404, 504, 601, 602 | P0 | Wireframe Complete |
| SCR-014 | UXR-201-206, 301-305, 401-404, 504, 601, 602 | P1 | Wireframe Pending |
| SCR-015 | UXR-201-206, 301-305, 401-404, 601, 602 | P1 | Wireframe Pending |
| SCR-016 | UXR-102, 201-206, 301-305, 401-404, 501, 601, 602 | P1 | Wireframe Pending |
| SCR-017 | UXR-102, 201-206, 302-303, 401-404, 501, 502, 601, 602 | P1 | Wireframe Pending |
| SCR-018 | UXR-102, 201-206, 303, 401-404, 601, 602 | P2 | Wireframe Pending |

**Wireframe Coverage**: 4 of 18 complete (22%) - Representative samples across all portals generated

---

## 9. Future Considerations (Out of Scope - Phase 1)

- Family member profile navigation
- Provider-facing workflows
- Bi-directional EHR integration navigation
- Payment gateway integration screens
- Mobile app navigation patterns (separate from responsive web)

---

**Document Status**: Draft - Comprehensive information architecture defined for all 18 screens  
**Next Steps**: Complete remaining 14 wireframes, implement navigation links per FL-XXX flows