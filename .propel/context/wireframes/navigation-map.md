# Navigation Map — Patient Access & Clinical Intelligence Platform

## Cross-Screen Link Map

All `href` attributes used in the 26 Hi-Fi wireframes. Every link target is a valid wireframe file.

### Public Screens

| From | Link Text | To |
|------|-----------|-----|
| SCR-001 | "Already have an account? Login" | SCR-002 |
| SCR-002 | "Create Account" | SCR-001 |
| SCR-002 | Login (Patient) | SCR-003 |
| SCR-002 | Login (Staff) | SCR-004 |
| SCR-002 | Login (Admin) | SCR-005 |

### Patient Portal Navigation

| From | Link Text / Action | To |
|------|-------------------|-----|
| SCR-003 | Sidebar: Dashboard | SCR-003 |
| SCR-003 | Sidebar: My Appointments | SCR-010 |
| SCR-003 | Sidebar: Find Providers | SCR-006 |
| SCR-003 | Sidebar: Health Dashboard | SCR-016 |
| SCR-003 | Sidebar: Documents | SCR-014 |
| SCR-003 | Sidebar: Intake | SCR-012 |
| SCR-003 | Quick Action: Book Appointment | SCR-006 |
| SCR-003 | Quick Action: Complete Intake | SCR-012 |
| SCR-003 | Quick Action: Upload Documents | SCR-014 |
| SCR-006 | "Book Appointment" per provider | SCR-007 |
| SCR-007 | Confirm Booking | SCR-008 |
| SCR-008 | "View My Appointments" | SCR-010 |
| SCR-008 | Back to Dashboard | SCR-003 |
| SCR-009 | Back link | SCR-006 |
| SCR-010 | Reschedule | SCR-011 |
| SCR-010 | Cancel (modal confirm) | SCR-010 |
| SCR-011 | Confirm Reschedule | SCR-010 |
| SCR-012 | Toggle to Manual | SCR-013 |
| SCR-013 | Toggle to AI | SCR-012 |
| SCR-014 | View Status | SCR-015 |

### Staff Portal Navigation

| From | Link Text / Action | To |
|------|-------------------|-----|
| SCR-004 | Sidebar: Dashboard | SCR-004 |
| SCR-004 | Sidebar: Queue | SCR-019 |
| SCR-004 | Sidebar: Arrivals | SCR-020 |
| SCR-004 | Sidebar: Patient View | SCR-017 |
| SCR-004 | Sidebar: Verification | SCR-023 |
| SCR-004 | Sidebar: Walk-in Booking | SCR-018 |
| SCR-017 | Back to Dashboard | SCR-004 |
| SCR-018 | "Register new patient" | SCR-001 |
| SCR-018 | Cancel | SCR-004 |
| SCR-019 | "+ Add Walk-in" | SCR-018 |
| SCR-023 | Resolve (conflict row) | SCR-024 |
| SCR-024 | Back to Verification | SCR-023 |
| SCR-024 | Cancel | SCR-023 |

### Admin Portal Navigation

| From | Link Text / Action | To |
|------|-------------------|-----|
| SCR-005 | Sidebar: Dashboard | SCR-005 |
| SCR-005 | Sidebar: Users | SCR-021 |
| SCR-005 | Sidebar: Audit Log | SCR-025 |
| SCR-005 | Sidebar: Settings | SCR-026 |
| SCR-021 | "+ Create User" | SCR-022 |
| SCR-021 | "Edit" per row | SCR-022 |
| SCR-022 | Cancel | SCR-021 |
| SCR-022 | Breadcrumb: Users | SCR-021 |

---

## Flow Connectivity Verification

| Flow | Screens | All Links Wired | Status |
|------|---------|-----------------|--------|
| FL-001 Registration | SCR-001 → SCR-002 | Yes | PASS |
| FL-002 Login → Dashboard | SCR-002 → SCR-003/004/005 | Yes | PASS |
| FL-003 Booking | SCR-003 → SCR-006 → SCR-007 → SCR-008 | Yes | PASS |
| FL-004 Waitlist | SCR-006 → SCR-009 → SCR-010 | Yes | PASS |
| FL-005 Appt Management | SCR-010 → SCR-011 | Yes | PASS |
| FL-006 Intake | SCR-003 → SCR-012 ↔ SCR-013 | Yes | PASS |
| FL-007 Documents | SCR-003 → SCR-014 → SCR-015 | Yes | PASS |
| FL-008 Walk-in | SCR-004 → SCR-018 → SCR-019 | Yes | PASS |
| FL-009 Verification | SCR-004 → SCR-023 → SCR-024 | Yes | PASS |
| FL-010 User Mgmt | SCR-005 → SCR-021 → SCR-022 | Yes | PASS |
| FL-011 Admin Ops | SCR-005 → SCR-025, SCR-026 | Yes | PASS |

**Result: 11/11 flows fully wired (100%)**
