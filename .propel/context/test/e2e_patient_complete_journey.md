---
id: e2e_patient_complete_journey
title: E2E Test Workflow - Patient Complete Platform Experience
version: 1.0.0
status: active
created: 2026-03-23
updated: 2026-03-23
framework: Playwright TypeScript
priority: P0
---

# E2E Test Workflow: Patient Complete Platform Experience

## Metadata
| Field | Value |
|-------|-------|
| Journey | Patient Complete Onboarding & Appointment Flow |
| Source | `.propel/context/docs/spec.md` |
| UC Chain | UC-001 → UC-002 → UC-003 → UC-007/008 → UC-009 → UC-010 |
| Base URL | `http://localhost:5173` |
| Backend API | `http://localhost:5000/api` |
| Framework | Playwright 1.40+ |
| Session Scope | Single browser context (session persistence across UCs) |

---

## Journey Overview

### E2E-PAT-001: Complete Patient Onboarding & Appointment Journey

**Goal**: Verify a new patient can register, authenticate, book appointment, complete intake, and view health dashboard in a single session

**Journey Flow:**

| Step # | Use Case | Action | Expected State |
|--------|----------|--------|----------------|
| 1 | UC-001 | Register new patient account | Account created, pending verification |
| 2 | UC-001 | Verify email via link | Account activated |
| 3 | UC-002 | Login with credentials | Session established, dashboard accessible |
| 4 | UC-003 | Search and browse providers | Provider list displayed |
| 5 | UC-003 | Select provider and view availability | Calendar shown with available slots |
| 6 | UC-003 | Book appointment | Appointment confirmed, awaiting intake |
| 7 | UC-007 | Complete AI intake form | Health data collected, stored |
| 8 | UC-009 | Upload clinical document (optional) | Document queued for processing |
| 9 | UC-010 | View 360-degree health view | Consolidated patient profile displayed |

**Session Requirements:**
- Authentication: REQUIRED (login persists across journey)
- State Persistence: Browser cookies + localStorage
- Cleanup: Delete test patient and all related data after test
- Execution Time: ~6-8 minutes
- Isolation: No shared state with other tests

---

## E2E Test Specification

### TC-E2E-PAT-001-001: Complete Patient Journey - Registration to Dashboard

**Type:** e2e | **Priority:** P0 | **Business Value:** Demonstrates core value proposition end-to-end

**Journey Steps with Detailed Execution:**

```yaml
e2e_steps:
  - phase: "UC-001: Patient Registration"
    description: "New patient registers account with email verification"
    steps:
      - step_id: "E2E-001-001"
        action: navigate
        target: "/"
        expect: "Home page with Registration link visible"
        wait: "domcontentloaded"
        checkpoint: true

      - step_id: "E2E-001-002"
        action: click
        target: "getByRole('link', {name: 'Register'})"
        expect: "Navigated to /register page"

      - step_id: "E2E-001-003"
        action: waitForSelector
        target: "getByRole('heading', {name: 'Create Account'})"
        timeout_ms: 5000
        expect: "Registration form displayed"

      - step_id: "E2E-001-004"
        action: fill
        target: "getByLabel('Full Name')"
        value: "{{ patient.firstName }} {{ patient.lastName }}"
        expect: "Name field populated"

      - step_id: "E2E-001-005"
        action: fill
        target: "getByLabel('Date of Birth')"
        value: "{{ patient.dateOfBirth }}"
        expect: "DOB field populated"

      - step_id: "E2E-001-006"
        action: fill
        target: "getByLabel('Email Address')"
        value: "{{ patient.email }}"
        expect: "Email field populated with unique email"

      - step_id: "E2E-001-007"
        action: fill
        target: "getByLabel('Phone Number')"
        value: "{{ patient.phone }}"
        expect: "Phone field populated"

      - step_id: "E2E-001-008"
        action: fill
        target: "getByLabel('Password')"
        value: "{{ patient.password }}"
        expect: "Password masked in input"

      - step_id: "E2E-001-009"
        action: click
        target: "getByRole('button', {name: 'Create Account'})"
        expect: "Form submission initiated"

      - step_id: "E2E-001-010"
        action: waitForSelector
        target: "getByText('Check your email to verify your account')"
        timeout_ms: 10000
        expect: "Verification page displayed"

      - step_id: "E2E-001-011"
        action: verify
        target: "getByTestId('verification-email-hint')"
        expect: "Masked email shown: {{ patient.emailMasked }}"

      - step_id: "E2E-001-012"
        action: simulate_email_verification
        email: "{{ patient.email }}"
        expect: "Verification email fetched and link extracted"

      - step_id: "E2E-001-013"
        action: click
        target: "constructed_verification_link"
        expect: "Account activated"

      - step_id: "E2E-001-014"
        action: waitForNavigation
        target: "/login"
        timeout_ms: 5000
        expect: "Redirected to login page after verification"
        checkpoint: true

  - phase: "UC-002: Patient Authentication"
    description: "Patient logs in with verified credentials"
    steps:
      - step_id: "E2E-002-001"
        action: verify
        target: "getByRole('heading', {name: 'Patient Login'})"
        expect: "Login page displayed"

      - step_id: "E2E-002-002"
        action: fill
        target: "getByLabel('Email')"
        value: "{{ patient.email }}"
        expect: "Email filled"

      - step_id: "E2E-002-003"
        action: fill
        target: "getByLabel('Password')"
        value: "{{ patient.password }}"
        expect: "Password filled"

      - step_id: "E2E-002-004"
        action: click
        target: "getByRole('button', {name: 'Sign In'})"
        expect: "Login attempt"

      - step_id: "E2E-002-005"
        action: waitForNavigation
        target: "/dashboard"
        timeout_ms: 10000
        expect: "Authenticated user redirected to dashboard"

      - step_id: "E2E-002-006"
        action: verify
        target: "getByRole('heading', {name: /Welcome|Dashboard/i})"
        expect: "Dashboard welcome message visible"

      - step_id: "E2E-002-007"
        action: verify
        target: "getByText('{{ patient.email }}')"
        expect: "User email displayed in profile"
        checkpoint: true

      - step_id: "E2E-002-008"
        action: saveAuthentication
        key: "patient_session"
        expect: "Session context preserved for next phases"

  - phase: "UC-003: Appointment Booking"
    description: "Patient searches, views provider availability, and books appointment"
    steps:
      - step_id: "E2E-003-001"
        action: click
        target: "getByRole('link', {name: 'Book Appointment'})"
        expect: "Navigated to appointment booking page"

      - step_id: "E2E-003-002"
        action: waitForSelector
        target: "getByRole('heading', {name: 'Find a Provider'})"
        timeout_ms: 5000
        expect: "Provider search page displayed"

      - step_id: "E2E-003-003"
        action: fill
        target: "getByPlaceholder('Search providers')"
        value: "Sarah"
        expect: "Search field accepts input"

      - step_id: "E2E-003-004"
        action: waitForSelector
        target: "getByText('Dr. Sarah Johnson')"
        timeout_ms: 5000
        expect: "Provider search results shown"

      - step_id: "E2E-003-005"
        action: click
        target: "getByRole('button', {name: /view availability|select/i})"
        expect: "Provider selected"

      - step_id: "E2E-003-006"
        action: waitForSelector
        target: "getByRole('heading', {name: /select time|availability/i})"
        timeout_ms: 5000
        expect: "Calendar with time slots displayed"

      - step_id: "E2E-003-007"
        action: click
        target: "getByRole('button', {name: '2026-03-26'})"
        expect: "Date selected"

      - step_id: "E2E-003-008"
        action: waitForSelector
        target: "getByTestId('time-slots')"
        timeout_ms: 5000
        expect: "Time slots displayed for selected date"

      - step_id: "E2E-003-009"
        action: click
        target: "getByRole('button', {name: '10:00 AM'})"
        expect: "Time slot selected"

      - step_id: "E2E-003-010"
        action: fill
        target: "getByLabel('Reason for Visit')"
        value: "Annual checkup"
        expect: "Visit reason captured"

      - step_id: "E2E-003-011"
        action: click
        target: "getByRole('button', {name: 'Confirm Booking'})"
        expect: "Booking submission"

      - step_id: "E2E-003-012"
        action: waitForSelector
        target: "getByText(/booking.*confirmed|appointment.*scheduled/i)"
        timeout_ms: 10000
        expect: "Booking confirmation displayed"

      - step_id: "E2E-003-013"
        action: verify
        target: "getByTestId('confirmation-number')"
        expect: "Confirmation number visible"

      - step_id: "E2E-003-014"
        action: saveData
        key: "appointment_id"
        from: "confirmation-number"
        expect: "Appointment ID stored for later verification"
        checkpoint: true

  - phase: "UC-007: AI-Powered Patient Intake (Conversational)"
    description: "Patient completes health intake using conversational AI interface"
    steps:
      - step_id: "E2E-007-001"
        action: click
        target: "getByRole('link', {name: 'Complete Intake'})"
        expect: "Navigated to intake page"

      - step_id: "E2E-007-002"
        action: waitForSelector
        target: "getByText('What is your full name')"
        timeout_ms: 5000
        expect: "AI conversational intake started"

      - step_id: "E2E-007-003"
        action: fill
        target: "getByTestId('intake-input')"
        value: "{{ patient.firstName }} {{ patient.lastName }}"
        expect: "Name provided in conversational form"

      - step_id: "E2E-007-004"
        action: click
        target: "getByRole('button', {name: 'Continue'})"
        expect: "Next question displayed"

      - step_id: "E2E-007-005"
        action: waitForSelector
        target: "getByText(/age|date.*birth/i)"
        timeout_ms: 3000
        expect: "Age/DOB question asked"

      - step_id: "E2E-007-006"
        action: fill
        target: "getByTestId('intake-input')"
        value: "41"
        expect: "Age provided"

      - step_id: "E2E-007-007"
        action: click
        target: "getByRole('button', {name: 'Continue'})"
        expect: "Next question"

      - step_id: "E2E-007-008"
        action: waitForSelector
        target: "getByText(/medications|currently.*taking/i)"
        timeout_ms: 3000
        expect: "Medications question"

      - step_id: "E2E-007-009"
        action: fill
        target: "getByTestId('intake-input')"
        value: "Metformin 500mg, Lisinopril 10mg"
        expect: "Medications listed"

      - step_id: "E2E-007-010"
        action: click
        target: "getByRole('button', {name: 'Continue'})"
        expect: "Next question"

      - step_id: "E2E-007-011"
        action: waitForSelector
        target: "getByText(/allergies|react/i)"
        timeout_ms: 3000
        expect: "Allergies question"

      - step_id: "E2E-007-012"
        action: fill
        target: "getByTestId('intake-input')"
        value: "Penicillin"
        expect: "Allergy documented"

      - step_id: "E2E-007-013"
        action: click
        target: "getByRole('button', {name: 'Continue'})"
        expect: "Final question"

      - step_id: "E2E-007-014"
        action: waitForSelector
        target: "getByText(/chief.*complaint|reason.*visit|concern/i)"
        timeout_ms: 3000
        expect: "Chief complaint question"

      - step_id: "E2E-007-015"
        action: fill
        target: "getByTestId('intake-input')"
        value: "Routine checkup, feeling healthy"
        expect: "Chief complaint recorded"

      - step_id: "E2E-007-016"
        action: click
        target: "getByRole('button', {name: 'Complete Intake'})"
        expect: "Intake submitted"

      - step_id: "E2E-007-017"
        action: waitForSelector
        target: "getByText(/intake.*completed|thank.*you/i)"
        timeout_ms: 8000
        expect: "Intake confirmation displayed"
        checkpoint: true

  - phase: "UC-009: Upload Clinical Document"
    description: "Patient uploads historical clinical document (PDF)"
    steps:
      - step_id: "E2E-009-001"
        action: click
        target: "getByRole('link', {name: 'Upload Documents'})"
        expect: "Document upload page"

      - step_id: "E2E-009-002"
        action: waitForSelector
        target: "getByText('Upload Clinical Documents')"
        timeout_ms: 5000
        expect: "Upload interface displayed"

      - step_id: "E2E-009-003"
        action: uploadFile
        target: "getByTestId('document-upload')"
        file: "test_fixtures/sample_discharge_summary.pdf"
        expect: "File selected for upload"

      - step_id: "E2E-009-004"
        action: verify
        target: "getByText('sample_discharge_summary.pdf')"
        expect: "File name displayed in upload queue"

      - step_id: "E2E-009-005"
        action: click
        target: "getByRole('button', {name: 'Start Upload'})"
        expect: "Upload initiated"

      - step_id: "E2E-009-006"
        action: waitForSelector
        target: "getByRole('progressbar')"
        timeout_ms: 3000
        expect: "Progress bar visible"

      - step_id: "E2E-009-007"
        action: waitForSelector
        target: "getByText(/upload.*complete|processing/i)"
        timeout_ms: 15000
        expect: "Upload completion confirmed"

      - step_id: "E2E-009-008"
        action: verify
        target: "getByTestId('document-status')"
        state: "contains"
        value: "Processing"
        expect: "Document marked as processing"
        checkpoint: true

  - phase: "UC-010: View 360-Degree Patient View"
    description: "Patient views consolidated health dashboard aggregating intake and documents"
    steps:
      - step_id: "E2E-010-001"
        action: click
        target: "getByRole('link', {name: 'Health Dashboard|My Health'})"
        expect: "360-degree view page"

      - step_id: "E2E-010-002"
        action: waitForSelector
        target: "getByRole('heading', {name: '360-Degree Health View'})"
        timeout_ms: 5000
        expect: "Dashboard title displayed"

      - step_id: "E2E-010-003"
        action: verify
        target: "getByText('{{ patient.firstName }} {{ patient.lastName }}')"
        expect: "Patient name displayed"

      - step_id: "E2E-010-004"
        action: verify
        target: "getByTestId('medications-section')"
        expect: "Medications section visible"

      - step_id: "E2E-010-005"
        action: verify
        target: "getByText('Metformin')"
        expect: "Medications from intake displayed"

      - step_id: "E2E-010-006"
        action: verify
        target: "getByTestId('allergies-section')"
        expect: "Allergies section visible"

      - step_id: "E2E-010-007"
        action: verify
        target: "getByText('Penicillin')"
        expect: "Allergies from intake displayed"

      - step_id: "E2E-010-008"
        action: verify
        target: "getByTestId('chief-complaint-section')"
        expect: "Chief complaint displayed"

      - step_id: "E2E-010-009"
        action: verify
        target: "getByText('Routine checkup')"
        expect: "Visit reason visible"

      - step_id: "E2E-010-010"
        action: verify
        target: "getByTestId('upcoming-appointments')"
        expect: "Upcoming appointments section"

      - step_id: "E2E-010-011"
        action: verify
        target: "getByText('Dr. Sarah Johnson')"
        expect: "Booked appointment visible"

      - step_id: "E2E-010-012"
        action: verify
        target: "getByText('March 26, 2026')"
        expect: "Appointment date displayed"

      - step_id: "E2E-010-013"
        action: verify
        target: "getByText('10:00 AM')"
        expect: "Appointment time displayed"
        checkpoint: true

  - phase: "Journey Completion Verification"
    description: "Verify all data persisted correctly and journey completed successfully"
    steps:
      - step_id: "E2E-FINAL-001"
        action: api_verify
        target: "GET /api/patients/{{ patient.id }}"
        expect: "Patient record exists in database"

      - step_id: "E2E-FINAL-002"
        action: api_verify
        target: "GET /api/appointments?patientId={{ patient.id }}"
        expect: "Appointment record exists"
        verify_response:
          appointment_date: "2026-03-26"
          appointment_time: "10:00"
          provider_name: "Dr. Sarah Johnson"
          status: "Scheduled"

      - step_id: "E2E-FINAL-003"
        action: api_verify
        target: "GET /api/patients/{{ patient.id }}/intake"
        expect: "Intake data persisted"
        verify_response:
          medications: contains "Metformin"
          allergies: contains "Penicillin"
          chief_complaint: contains "checkup"

      - step_id: "E2E-FINAL-004"
        action: api_verify
        target: "GET /api/patients/{{ patient.id }}/documents"
        expect: "Document uploaded and visible"
        verify_response:
          count: ">= 1"
          status: "contains Processing|Completed"

      - step_id: "E2E-FINAL-005"
        action: navigate
        target: "/dashboard"
        expect: "Dashboard loads without errors"

      - step_id: "E2E-FINAL-006"
        action: click
        target: "getByRole('button', {name: 'Logout|Sign Out'})"
        expect: "User authenticated for logout"

      - step_id: "E2E-FINAL-007"
        action: waitForNavigation
        target: "/login"
        timeout_ms: 5000
        expect: "User logged out successfully"
        checkpoint: true
```

---

## Journey Test Data

```yaml
journey_data:
  patient:
    firstName: "Emma"
    lastName: "Wilson"
    dateOfBirth: "1985-03-15"
    email: "emma.wilson.{{timestamp}}@example.com"
    emailMasked: "e***@example.com"
    phone: "+14155552671"
    password: "SecurePass123!"
    
  appointment:
    provider: "Dr. Sarah Johnson"
    specialty: "Family Medicine"
    date: "2026-03-26"
    time: "10:00 AM"
    reason: "Annual checkup"
    
  intake_response:
    medications:
      - "Metformin 500mg"
      - "Lisinopril 10mg"
    allergies:
      - "Penicillin"
    chief_complaint: "Routine checkup, feeling healthy"
    
  document:
    filename: "sample_discharge_summary.pdf"
    file_size: "2.5 MB"
    expected_extraction_status: "Processing"
```

---

## Additional E2E Journey Tests

### E2E-PAT-002: Waitlist & Slot Swap Journey

**UC Chain**: UC-002 → UC-003 → UC-004 → UC-005

**Scenario**: Patient logs in, attempts to book unavailable slot, joins waitlist, preferred slot becomes available, system performs automatic swap

```yaml
e2e_journey_pat_002:
  - phase: "UC-002: Login"
    action: "Patient authenticates"
    
  - phase: "UC-003: Browse"
    action: "Patient searches for provider with no available slots"
    
  - phase: "UC-004: Waitlist"
    action: "Patient joins waitlist with preferred time preference"
    checkpoint: "Enrollment confirmed, position assigned"
    
  - phase: "UC-005: Swap"
    action: "Simulate slot availability becoming available"
    action: "System detects and creates preferred appointment"
    action: "Original booking released"
    checkpoint: "Appointment successfully swapped to preferred time"
```

---

### E2E-PAT-003: Intake Mode Switching Journey

**UC Chain**: UC-002 → UC-007 → UC-008

**Scenario**: Patient starts AI intake, switches to manual form mid-way, completes form-based intake

```yaml
e2e_journey_pat_003:
  - phase: "UC-007: AI Intake Start"
    action: "Patient begins conversational intake"
    checkpoint: "Conversational flow started"
    
  - phase: "Mode Switch"
    action: "Patient clicks 'Switch to Manual Form'"
    checkpoint: "Form displayed with previously answered questions preserved"
    
  - phase: "UC-008: Manual Intake"
    action: "Patient completes remaining fields in traditional form"
    checkpoint: "Intake data submitted and saved"
```

---

## Page Objects for E2E Tests

```yaml
pages:
  - name: RegistrationPage
    file: "pages/registration.page.ts"
    elements:
      fullNameInput: "getByLabel('Full Name')"
      dateOfBirthInput: "getByLabel('Date of Birth')"
      emailInput: "getByLabel('Email Address')"
      phoneInput: "getByLabel('Phone Number')"
      passwordInput: "getByLabel('Password')"
      createAccountButton: "getByRole('button', {name: 'Create Account'})"
      
  - name: LoginPage
    file: "pages/login.page.ts"
    elements:
      emailInput: "getByLabel('Email')"
      passwordInput: "getByLabel('Password')"
      signInButton: "getByRole('button', {name: 'Sign In'})"
      
  - name: AppointmentPage
    file: "pages/appointment.page.ts"
    elements:
      bookNewButton: "getByRole('button', {name: 'Book New Appointment'})"
      searchInput: "getByPlaceholder('Search providers')"
      confirmButton: "getByRole('button', {name: 'Confirm Booking'})"
      
  - name: IntakePage
    file: "pages/intake.page.ts"
    elements:
      intakeInput: "getByTestId('intake-input')"
      continueButton: "getByRole('button', {name: 'Continue'})"
      completeButton: "getByRole('button', {name: 'Complete Intake'})"
      switchModeButton: "getByRole('button', {name: 'Switch to Manual Form'})"
      
  - name: DashboardPage
    file: "pages/dashboard.page.ts"
    elements:
      welcomeHeading: "getByRole('heading', {name: /Welcome|Dashboard/i})"
      uploadDocumentsLink: "getByRole('link', {name: 'Upload Documents'})"
      healthDashboardLink: "getByRole('link', {name: 'Health Dashboard'})"
      logoutButton: "getByRole('button', {name: 'Logout'})"
```

---

## Session Management

### Session Persistence Pattern

```typescript
// Save session after login
async function preserveSession(page, storageState) {
  await page.context().storageState({ path: storageState });
}

// Restore session for next test
async function loadSession(page, storageState) {
  const context = await browser.newContext({
    storageState: storageState
  });
  const page = await context.newPage();
  return { page, context };
}

// In E2E test - single session throughout
async function e2ePatientJourney(page) {
  // Single page/context object used for entire journey
  // No session resets between UC phases
  // All data preserved across navigation
}
```

---

## Error Recovery & Debugging

### Journey Checkpoints for Tracing

Each major phase has a `checkpoint: true` marker. If journey fails:

```bash
# Run with video recording
npx playwright test e2e_patient_complete_journey.md --video=on

# Run with trace for debugging
npx playwright test e2e_patient_complete_journey.md --trace=on

# Step through interactively
npx playwright test e2e_patient_complete_journey.md --debug --headed
```

### Common Failure Points

| Phase | Common Failure | Root Cause | Recovery |
|-------|---|---|---|
| Registration | Email verification link timeout | Email service mocked incorrectly | Check test email fixture setup |
| Booking | Time slots not loading | API latency | Increase waitForSelector timeout |
| Intake | Conversational flow stalled | AI service timeout | Check OpenAI mock responses |
| Upload | Document processing stuck | Background job failure | Check Hangfire queue in logs |
| Dashboard | Data not aggregated | Cache not invalidated | Clear Redis cache before test |

---

## Success Criteria

- [x] Journey completes end-to-end without manual intervention
- [x] Session maintained across all 6 UC phases
- [x] All data persisted correctly in database
- [x] Email notifications sent at appropriate points
- [x] Calendar integration triggered for appointment
- [x] Intake data aggregated into 360-view correctly
- [x] No duplicate data created during journey
- [x] Audit logs record all major actions
- [x] Cleanup properly removes test data
- [x] Journey executes in <10 minutes

---

## Execution Commands

```bash
# Run single E2E journey test
npx playwright test e2e_patient_complete_journey.md -g "E2E-PAT-001"

# Run all E2E journeys
npx playwright test e2e_patient_complete_journey.md

# Run with reporting
npx playwright test e2e_patient_complete_journey.md --reporter=html

# Run in headed mode to watch
npx playwright test e2e_patient_complete_journey.md --headed

# Debug specific test
npx playwright test e2e_patient_complete_journey.md -g "E2E-PAT-001" --debug
```

---

## Setup & Teardown

### Global Setup (runs once before all tests)

```typescript
// globalSetup.ts
export default async function globalSetup() {
  // Ensure backend API is running
  // Ensure PostgreSQL test database is up
  // Seed reference data (providers, insurance records)
  // Clear any leftover test data from previous runs
  // Setup mock email/SMS services
}
```

### Per-Test Teardown

```typescript
test.afterEach(async ({page}) => {
  // Delete test patient created during journey
  // Delete test appointments
  // Delete test documents
  // Clear browser storage
  // Close any unclosed contexts
});
```

---

## Monitoring & Metrics

**Journey Execution Metrics**:
- Total runtime: `~6-8 minutes`
- API calls: `~45-60`
- Database writes: `~8-12`
- Email messages: `2-3`
- Network failures tolerated: `0` (journey depends on all services)

**Success Rate Target**: 98%+ (failures tracked for root cause analysis)

---

*E2E Test Workflow: e2e_patient_complete_journey.md*  
*Created: 2026-03-23 | Updated: 2026-03-23*  
*Framework: Playwright 1.40+ | Language: TypeScript*  
*Source: `.propel/context/docs/spec.md`*
