# Navigation Map - Unified Patient Access & Clinical Intelligence Platform

## Overview

This document provides the cross-screen navigation index for all wireframes, documenting the user flows (FL-XXX) from figma_spec.md and the navigation links implemented in each wireframe.

## Navigation Index by Portal

### Public Screens Navigation

| Source Screen | Element | Action | Target Screen | Notes |
|--------------|---------|--------|---------------|-------|
| SCR-001 Landing | Sign In button | click | SCR-002 Sign In | Primary CTA |
| SCR-001 Landing | Sign Up link | click | SCR-002 Sign In | With signup mode |
| SCR-002 Sign In | Submit button | click | SCR-003/SCR-011/SCR-018 | Based on role |
| SCR-002 Sign In | Forgot Password | click | Password Reset flow | External |

### Patient Portal Navigation

| Source Screen | Element | Action | Target Screen | Notes |
|--------------|---------|--------|---------------|-------|
| SCR-003 Dashboard | Book Appointment | click | SCR-004 Calendar | Primary CTA |
| SCR-003 Dashboard | View All Appointments | click | SCR-010 History | Secondary |
| SCR-003 Dashboard | Complete Intake | click | SCR-006 AI Intake | Intake prompt |
| SCR-003 Dashboard | Upload Documents | click | SCR-008 Doc Upload | Quick action |
| SCR-003 Dashboard | View Profile | click | SCR-009 360-View | Profile link |
| SCR-004 Calendar | Select Slot | click | SCR-005 Booking Confirm | Slot selected |
| SCR-004 Calendar | Back to Dashboard | click | SCR-003 Dashboard | Cancel/back |
| SCR-005 Booking | Confirm Booking | click | SCR-003 Dashboard | Success → toast |
| SCR-005 Booking | Cancel | click | SCR-004 Calendar | Return to calendar |
| SCR-006 AI Intake | Switch to Manual | click | SCR-007 Manual Intake | Mode toggle |
| SCR-006 AI Intake | Submit | click | SCR-003 Dashboard | Success → toast |
| SCR-007 Manual Intake | Switch to AI | click | SCR-006 AI Intake | Mode toggle |
| SCR-007 Manual Intake | Submit | click | SCR-003 Dashboard | Success → toast |
| SCR-008 Doc Upload | Back | click | SCR-003 Dashboard | Return |
| SCR-009 360-View | Back | click | SCR-003 Dashboard | Return |
| SCR-010 History | View Details | click | SCR-009 360-View | Appointment detail |
| SCR-010 History | Back | click | SCR-003 Dashboard | Return |

### Staff Portal Navigation

| Source Screen | Element | Action | Target Screen | Notes |
|--------------|---------|--------|---------------|-------|
| SCR-011 Dashboard | Walk-in | click | SCR-014 Walk-in | Quick action |
| SCR-011 Dashboard | View Queue | click | SCR-013 Queue | Queue management |
| SCR-011 Dashboard | Search Patients | click | SCR-012 Patient Search | Lookup |
| SCR-011 Dashboard | Pending Conflicts | click | SCR-015 Conflict Review | Badge count |
| SCR-011 Dashboard | Pending Codes | click | SCR-016 Code Verify | Badge count |
| SCR-011 Dashboard | Validate Insurance | click | SCR-017 Insurance | Quick action |
| SCR-012 Patient Search | Select Patient | click | SCR-009 360-View | Patient detail |
| SCR-012 Patient Search | Back | click | SCR-011 Dashboard | Return |
| SCR-013 Queue | Mark Arrived | click | SCR-013 Queue | Status update |
| SCR-013 Queue | View Patient | click | SCR-009 360-View | Patient detail |
| SCR-013 Queue | Back | click | SCR-011 Dashboard | Return |
| SCR-014 Walk-in | Submit | click | SCR-013 Queue | Success → queue |
| SCR-014 Walk-in | Cancel | click | SCR-011 Dashboard | Return |
| SCR-015 Conflict | Resolve | click | SCR-015 Conflict | Update in place |
| SCR-015 Conflict | View Evidence | click | Document Drawer | Drawer opens |
| SCR-015 Conflict | Back | click | SCR-011 Dashboard | Return |
| SCR-016 Code Verify | Confirm Code | click | SCR-016 Code Verify | Update in place |
| SCR-016 Code Verify | View Evidence | click | Evidence Drawer | Drawer opens |
| SCR-016 Code Verify | Back | click | SCR-011 Dashboard | Return |
| SCR-017 Insurance | Validate | click | SCR-017 Insurance | Inline result |
| SCR-017 Insurance | Back | click | SCR-011 Dashboard | Return |

### Admin Portal Navigation

| Source Screen | Element | Action | Target Screen | Notes |
|--------------|---------|--------|---------------|-------|
| SCR-018 Dashboard | Manage Users | click | SCR-019 User Mgmt | Primary action |
| SCR-018 Dashboard | Configure Roles | click | SCR-020 Roles | Configuration |
| SCR-018 Dashboard | View Audit Logs | click | SCR-021 Audit Logs | Compliance |
| SCR-019 User Mgmt | Add User | click | Create User Modal | Modal opens |
| SCR-019 User Mgmt | Edit User | click | Edit User Modal | Modal opens |
| SCR-019 User Mgmt | Back | click | SCR-018 Dashboard | Return |
| SCR-020 Roles | Save | click | SCR-020 Roles | Update in place |
| SCR-020 Roles | Back | click | SCR-018 Dashboard | Return |
| SCR-021 Audit Logs | Export | click | Download | CSV export |
| SCR-021 Audit Logs | Back | click | SCR-018 Dashboard | Return |

---

## User Flow Sequences

### FL-001: Patient Appointment Booking
**Derived From**: UC-001, UC-002
**Personas Covered**: Patient

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FL-001: PATIENT BOOKING FLOW                      │
└─────────────────────────────────────────────────────────────────────┘

[SCR-003 Dashboard]
       │
       │ Click "Book Appointment"
       ▼
[SCR-004 Calendar] ─────────────────────────────────┐
       │                                             │
       │ Select available slot                       │ No slots available
       ▼                                             ▼
[SCR-005 Booking Confirm]              [Swap Preference Modal]
       │                                     │
       │ Confirm booking                     │ Join waitlist
       ▼                                     ▼
   [Loading]                          [SCR-003 + Notification]
       │
       ├── Success ────────────────────────────┐
       │                                        ▼
       └── Error ──────> [SCR-005 Error] ─> [SCR-003 + Success Toast]
```

**Navigation Implementation**:
- SCR-003 → SCR-004: `#book-appointment-btn`
- SCR-004 → SCR-005: `#slot-{id}` (any available slot)
- SCR-005 → SCR-003: `#confirm-booking-btn` (success)
- SCR-005 → SCR-004: `#cancel-btn` (back)

---

### FL-002: Patient Intake (AI Mode)
**Derived From**: UC-003
**Personas Covered**: Patient

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FL-002: AI INTAKE FLOW                            │
└─────────────────────────────────────────────────────────────────────┘

[SCR-003 Dashboard]
       │
       │ Click "Complete Intake"
       ▼
[SCR-006 AI Intake] ◄───────────────────────────────┐
       │                                             │
       │ AI conversation                             │
       ▼                                             │
   [AI Prompts] ─── Switch to Manual ───► [SCR-007 Manual Intake]
       │                                             │
       │ Complete conversation                       │ Complete form
       ▼                                             │
   [Summary Review]                                  │
       │                                             │
       │ Confirm ◄───────────────────────────────────┘
       ▼
[SCR-003 + Success Toast]
```

**Navigation Implementation**:
- SCR-003 → SCR-006: `#complete-intake-btn`
- SCR-006 ↔ SCR-007: `#mode-toggle` (bidirectional switch)
- SCR-006/SCR-007 → SCR-003: `#submit-intake-btn` (success)

---

### FL-003: Staff Walk-in Booking
**Derived From**: UC-007, UC-009
**Personas Covered**: Staff

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FL-003: WALK-IN BOOKING FLOW                      │
└─────────────────────────────────────────────────────────────────────┘

[SCR-011 Staff Dashboard]
       │
       │ Click "Walk-in"
       ▼
[SCR-014 Walk-in Booking]
       │
       │ Search patient
       ▼
   [Patient Search]
       │
       ├── Found ──────────────────┐
       │                           │
       └── Not Found               │
              │                    │
              ▼                    ▼
       [Enter New Info]    [Select Patient]
              │                    │
              └────────┬───────────┘
                       │
                       │ Select slot, submit
                       ▼
                   [Loading]
                       │
                       ├── Success ─────────────────┐
                       │                             ▼
                       └── Error ──> Retry    [SCR-013 Queue]
                                                     │
                                                     │ Mark Arrived
                                                     ▼
                                              [SCR-013 + Toast]
```

**Navigation Implementation**:
- SCR-011 → SCR-014: `#walkin-btn`
- SCR-014 → SCR-013: `#submit-walkin-btn` (success)
- SCR-013: `#mark-arrived-{id}` (inline status update)

---

### FL-004: Clinical Data Conflict Resolution
**Derived From**: UC-011
**Personas Covered**: Staff

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FL-004: CONFLICT RESOLUTION FLOW                  │
└─────────────────────────────────────────────────────────────────────┘

[SCR-011 Staff Dashboard]
       │
       │ Click conflict badge
       ▼
[SCR-015 Conflict Review]
       │
       │ Select conflict
       ▼
[Conflict Detail Drawer] ◄──────────────────────────┐
       │                                             │
       │ View source documents                       │
       ▼                                             │
[Document Preview]                                   │
       │                                             │
       │ Compare values                              │
       ▼                                             │
   [Decision Point]                                  │
       │                                             │
       ├── Select Value A ──────────────────────────┤
       ├── Select Value B ──────────────────────────┤
       ├── Escalate ────────────────────────────────┤
       └── Cannot Determine ────────────────────────┘
                       │
                       │ Resolution logged
                       ▼
              [SCR-015 + Toast]
                       │
                       │ Next conflict?
                       ▼
              [Loop or Exit to SCR-011]
```

**Navigation Implementation**:
- SCR-011 → SCR-015: `#conflicts-badge` or `#nav-conflicts`
- SCR-015 → Drawer: `#conflict-{id}` (click row)
- Drawer actions: `#select-value-a`, `#select-value-b`, `#escalate-btn`

---

### FL-005: Medical Code Verification
**Derived From**: UC-012
**Personas Covered**: Staff

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FL-005: CODE VERIFICATION FLOW                    │
└─────────────────────────────────────────────────────────────────────┘

[SCR-011 Staff Dashboard]
       │
       │ Click "Pending Codes"
       ▼
[SCR-016 Code Verification]
       │
       │ Select code to review
       ▼
[Code Evidence Drawer] ◄────────────────────────────┐
       │                                             │
       │ Review AI suggestion                        │
       │ View supporting evidence                    │
       ▼                                             │
   [Decision Point]                                  │
       │                                             │
       ├── Confirm Code ────────────────────────────┤
       ├── Modify Code ─────────────────────────────┤
       ├── Reject Code ─────────────────────────────┤
       └── Add Missing Code ────────────────────────┘
                       │
                       │ Verification logged
                       ▼
              [SCR-016 + Toast]
                       │
                       │ Next code?
                       ▼
              [Loop or Exit to SCR-011]
```

**Navigation Implementation**:
- SCR-011 → SCR-016: `#pending-codes-badge` or `#nav-codes`
- SCR-016 → Drawer: `#code-{id}` (click row)
- Drawer actions: `#confirm-code`, `#modify-code`, `#reject-code`
- Keyboard shortcuts: `Enter` = confirm, `X` = reject

---

### FL-006: Admin User Management
**Derived From**: UC-013
**Personas Covered**: Admin

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FL-006: USER MANAGEMENT FLOW                      │
└─────────────────────────────────────────────────────────────────────┘

[SCR-018 Admin Dashboard]
       │
       │ Click "User Management"
       ▼
[SCR-019 User Management]
       │
       ├── Add User ─────────────────┐
       │                              ▼
       │                     [Create User Modal]
       │                              │
       │                              │ Submit
       │                              ▼
       │                         [Loading]
       │                              │
       │                              ├── Success ──────────────┐
       │                              │                          │
       │                              └── Error ──> Fix/Retry    │
       │                                                         │
       ├── Edit User ────────────────────────────────────────────┤
       │        │                                                 │
       │        ▼                                                 │
       │   [Edit User Modal]                                     │
       │        │                                                 │
       │        └── Submit ──> [Same as above]                   │
       │                                                         │
       └── Deactivate User                                       │
                │                                                 │
                ▼                                                 │
       [Confirmation Dialog]                                     │
                │                                                 │
                │ Confirm                                        │
                ▼                                                 │
       [SCR-019 + Success Toast] ◄───────────────────────────────┘
```

**Navigation Implementation**:
- SCR-018 → SCR-019: `#nav-users` or `#manage-users-btn`
- SCR-019 → Modal: `#add-user-btn`, `#edit-user-{id}`, `#deactivate-{id}`
- Modal → SCR-019: Form submit or close

---

## Global Navigation Components

### Header Navigation
All authenticated screens include consistent header navigation:

| Element | Target | Condition |
|---------|--------|-----------|
| Logo | Portal Dashboard | SCR-003/SCR-011/SCR-018 |
| Profile Avatar | Profile Dropdown | Always |
| Notifications | Notifications Panel | If notifications exist |
| Settings | Settings Page | Admin only |
| Logout | SCR-001 Landing | Always |

### Sidebar Navigation (Staff/Admin)

**Staff Sidebar Items**:
| Item | Target | Badge Trigger |
|------|--------|---------------|
| Dashboard | SCR-011 | - |
| Queue | SCR-013 | Active count |
| Patients | SCR-012 | - |
| Walk-in | SCR-014 | - |
| Conflicts | SCR-015 | Pending count |
| Codes | SCR-016 | Pending count |
| Insurance | SCR-017 | - |

**Admin Sidebar Items**:
| Item | Target | Badge Trigger |
|------|--------|---------------|
| Dashboard | SCR-018 | - |
| Users | SCR-019 | - |
| Roles | SCR-020 | - |
| Audit Logs | SCR-021 | - |

### Patient Top Navigation

| Item | Target | Active Condition |
|------|--------|-----------------|
| Dashboard | SCR-003 | Default |
| Appointments | SCR-010 | Viewing history |
| Documents | SCR-008 | Viewing documents |
| Profile | SCR-009 | Viewing 360-view |

---

## Dead-End Screen Analysis

| Screen | Outbound Links | Status |
|--------|----------------|--------|
| SCR-001 Landing | SCR-002 | ✓ Connected |
| SCR-002 Sign In | SCR-003/011/018 | ✓ Connected |
| All other screens | Multiple | ✓ Connected |

**Dead-End Count**: 0 (all screens have navigation paths)

---

## Modal Navigation Flows

### Session Timeout Modal
- **Trigger**: 13 minutes of inactivity
- **Actions**: 
  - Extend Session → Dismiss modal, reset timer
  - Logout → SCR-001 Landing

### Confirm Booking Modal (SCR-005)
- **Trigger**: Submit booking button
- **Actions**:
  - Confirm → Submit, show loading, navigate to SCR-003 on success
  - Cancel → Dismiss modal, stay on SCR-005

### Create/Edit User Modal (SCR-019)
- **Trigger**: Add User / Edit buttons
- **Actions**:
  - Save → Submit, show loading, refresh table on success
  - Cancel → Dismiss modal, stay on SCR-019

---

## Keyboard Navigation Summary

| Shortcut | Screen | Action |
|----------|--------|--------|
| `Tab` | All | Navigate forward through interactive elements |
| `Shift+Tab` | All | Navigate backward |
| `Enter` | SCR-016 | Confirm selected code |
| `X` | SCR-016 | Reject selected code |
| `Esc` | Modals/Drawers | Close overlay |
| `Arrow Keys` | SCR-004 Calendar | Navigate dates |
| `Arrow Keys` | DataTables | Navigate cells |
