# Information Architecture — Patient Access & Clinical Intelligence Platform

## 1. Wireframe Specification

| Attribute | Value |
|-----------|-------|
| Platform | Web (Responsive) |
| Fidelity | High-Fidelity |
| Viewport | 1440px (desktop primary) |
| Breakpoints | Desktop 1024+, Tablet 768–1023, Mobile 320–767 |
| Total Screens | 26 |
| Total Flows | 11 |

---

## 2. System Overview

The Patient Access & Clinical Intelligence Platform serves three user personas (Patient, Staff, Admin) through role-gated portals. The architecture follows a shell layout pattern (Header 64px + Sidebar 240px + Main content) with role-based navigation. Public screens (Registration, Login) use centered card layouts without the shell.

---

## 3. Personas & Flows

### Personas

| Persona | Role | Primary Screens |
|---------|------|-----------------|
| Patient | Healthcare consumer | SCR-001, SCR-002, SCR-003, SCR-006–SCR-016 |
| Staff | Front desk / Clinical | SCR-002, SCR-004, SCR-017–SCR-020, SCR-023–SCR-024 |
| Admin | System administrator | SCR-002, SCR-005, SCR-021–SCR-022, SCR-025–SCR-026 |

### Prototype Flows

| Flow ID | Name | Screens | Start → End |
|---------|------|---------|-------------|
| FL-001 | Patient Registration | SCR-001 → SCR-002 | Registration → Login |
| FL-002 | Patient Login → Dashboard | SCR-002 → SCR-003 | Login → Patient Dashboard |
| FL-003 | Appointment Booking | SCR-003 → SCR-006 → SCR-007 → SCR-008 | Dashboard → Provider → Book → Confirm |
| FL-004 | Waitlist Enrollment | SCR-006 → SCR-009 → SCR-010 | Provider → Waitlist → My Appointments |
| FL-005 | Appointment Management | SCR-010 → SCR-011 | My Appointments → Reschedule |
| FL-006 | AI Intake | SCR-003 → SCR-012 → SCR-013 | Dashboard → AI Intake ↔ Manual Intake |
| FL-007 | Document Upload | SCR-003 → SCR-014 → SCR-015 | Dashboard → Upload → Status |
| FL-008 | Staff Walk-in | SCR-004 → SCR-018 → SCR-019 | Staff Dashboard → Walk-in → Queue |
| FL-009 | Clinical Verification | SCR-004 → SCR-023 → SCR-024 | Staff Dashboard → Verify → Resolve |
| FL-010 | User Management | SCR-005 → SCR-021 → SCR-022 | Admin Dashboard → Users → Form |
| FL-011 | Admin Ops | SCR-005 → SCR-025, SCR-026 | Admin Dashboard → Audit / Settings |

---

## 4. Screen Hierarchy

### Level 0 — Public

| Screen | File | Priority |
|--------|------|----------|
| SCR-001 Registration | wireframe-SCR-001-registration.html | P0 |
| SCR-002 Login | wireframe-SCR-002-login.html | P0 |

### Level 1 — Dashboards (Landing)

| Screen | File | Role | Priority |
|--------|------|------|----------|
| SCR-003 Patient Dashboard | wireframe-SCR-003-patient-dashboard.html | Patient | P0 |
| SCR-004 Staff Dashboard | wireframe-SCR-004-staff-dashboard.html | Staff | P0 |
| SCR-005 Admin Dashboard | wireframe-SCR-005-admin-dashboard.html | Admin | P0 |

### Level 2 — Feature Screens

| Screen | File | Role | Priority |
|--------|------|------|----------|
| SCR-006 Provider Browser | wireframe-SCR-006-provider-browser.html | Patient | P0 |
| SCR-007 Appointment Booking | wireframe-SCR-007-appointment-booking.html | Patient | P0 |
| SCR-008 Confirmation | wireframe-SCR-008-appointment-confirmation.html | Patient | P0 |
| SCR-009 Waitlist | wireframe-SCR-009-waitlist-enrollment.html | Patient | P1 |
| SCR-010 My Appointments | wireframe-SCR-010-my-appointments.html | Patient | P0 |
| SCR-011 Reschedule | wireframe-SCR-011-reschedule.html | Patient | P1 |
| SCR-012 AI Intake | wireframe-SCR-012-ai-intake.html | Patient | P0 |
| SCR-013 Manual Intake | wireframe-SCR-013-manual-intake.html | Patient | P0 |
| SCR-014 Document Upload | wireframe-SCR-014-document-upload.html | Patient | P1 |
| SCR-015 Document Status | wireframe-SCR-015-document-status.html | Patient | P1 |
| SCR-016 Health Dashboard | wireframe-SCR-016-patient-health-dashboard.html | Patient | P0 |
| SCR-017 Staff Patient View | wireframe-SCR-017-staff-patient-view.html | Staff | P0 |
| SCR-018 Walk-in Booking | wireframe-SCR-018-walkin-booking.html | Staff | P0 |
| SCR-019 Queue Management | wireframe-SCR-019-queue-management.html | Staff | P0 |
| SCR-020 Arrival Management | wireframe-SCR-020-arrival-management.html | Staff | P1 |
| SCR-021 User Management | wireframe-SCR-021-user-management.html | Admin | P0 |
| SCR-022 User Form | wireframe-SCR-022-user-form.html | Admin | P0 |
| SCR-023 Clinical Verification | wireframe-SCR-023-clinical-verification.html | Staff | P0 |
| SCR-024 Conflict Resolution | wireframe-SCR-024-conflict-resolution.html | Staff | P1 |
| SCR-025 Audit Log | wireframe-SCR-025-audit-log.html | Admin | P1 |
| SCR-026 System Settings | wireframe-SCR-026-system-settings.html | Admin | P2 |

---

## 5. Navigation Structure

### Patient Sidebar (6 items)

1. Dashboard → SCR-003
2. My Appointments → SCR-010
3. Find Providers → SCR-006
4. Health Dashboard → SCR-016
5. Documents → SCR-014
6. Intake → SCR-012

### Staff Sidebar (6 items)

1. Dashboard → SCR-004
2. Queue → SCR-019
3. Arrivals → SCR-020
4. Patient View → SCR-017
5. Verification → SCR-023
6. Walk-in Booking → SCR-018

### Admin Sidebar (4 items)

1. Dashboard → SCR-005
2. Users → SCR-021
3. Audit Log → SCR-025
4. Settings → SCR-026

---

## 6. Interaction Patterns

| Pattern | Screens | Description |
|---------|---------|-------------|
| Multi-step Stepper | SCR-007, SCR-013 | 4-step progress indicator with active/completed/pending states |
| Modal Confirmation | SCR-010 | Cancel appointment modal with overlay dismiss |
| Accordion | SCR-016, SCR-017 | Collapsible sections for clinical data categories |
| Side Panel | SCR-017, SCR-023 | Source document viewer alongside data table |
| Side-by-Side Compare | SCR-024 | Dual-card conflict comparison layout |
| Drag-and-Drop | SCR-014 | File upload zone with progress indicators |
| Chat Interface | SCR-012 | AI/user chat bubbles with typing indicator |
| Toggle Switch | SCR-022, SCR-026 | Boolean settings with immediate visual feedback |
| Tab Navigation | SCR-010 | Upcoming/Past/Waitlist appointment views |
| Inline Search | SCR-018, SCR-021, SCR-025 | Search with real-time result filtering |

---

## 7. Responsive Strategy

| Breakpoint | Layout Changes |
|------------|----------------|
| Desktop (1024+) | Full shell: Sidebar 240px + Main content area |
| Tablet (768–1023) | Sidebar collapses/hidden, single-column main |
| Mobile (320–767) | Stacked layouts, reduced padding, bottom nav |

---

## 8. Accessibility Summary

- Skip-to-content link on every screen
- Semantic HTML landmarks: `<header>`, `<nav>`, `<main>`, `<aside>`
- ARIA labels on all interactive elements
- `aria-expanded` on accordion headers
- `aria-current="page"` on active navigation items
- Role attributes on custom widgets (radio groups, switches)
- Focus-visible outlines with 3:1 contrast ratio
- Keyboard navigation support (Tab, Enter, Escape)
