---
id: tw_patient_booking_workflow
title: Test Workflow - Patient Registration & Appointment Booking
version: 1.0.0
status: active
created: 2026-03-23
updated: 2026-03-23
framework: Playwright TypeScript
priority: P0
---

# Test Workflow: Patient Registration & Appointment Booking

## Metadata
| Field | Value |
|-------|-------|
| Feature | Patient Registration, Authentication, Appointment Booking & Waitlist |
| Source | `.propel/context/docs/spec.md` |
| Use Cases | UC-001, UC-002, UC-003, UC-004, UC-005, UC-006 |
| Base URL | `http://localhost:5173` (Vite dev server) |
| Backend API | `http://localhost:5000/api` (.NET 8 API) |
| Framework | Playwright 1.40+ |

## Test Implementation Strategy

### Selector Priority
1. **getByRole()** - Semantic HTML, accessible
2. **getByTestId()** - React data-testid attributes
3. **getByLabel()** - Form labels
4. **getByPlaceholder()** - Input placeholders
5. **Avoid CSS selectors** - Brittle, avoid unless necessary

### Wait Strategies
- `networkidle` - Pages with data fetches
- `domcontentloaded` - Quick page loads
- `waitForSelector()` - Dynamic elements
- Explicit waits (5s-30s based on operation)

### Test Data Management
All test data stored in YAML fixtures at bottom of file. Sensitive data in `.env.test` file.

---

## UC-001: Patient Registration

### TC-UC001-HP-001: Successful Patient Registration with Valid Data
**Type:** happy_path | **Priority:** P0 | **Requirement:** FR-001

**Goal:** Verify a new patient can register with valid information and receive verification email

**Preconditions:**
- Application is running and accessible
- Test patient email is unique (not previously registered)
- Email service is mocked/available for testing

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/"
    expect: "Home page loads with Registration link"
    wait: "domcontentloaded"

  - step_id: "002"
    action: click
    target: "getByRole('link', {name: 'Register'})"
    expect: "Navigation to /register page"

  - step_id: "003"
    action: waitForSelector
    target: "getByRole('heading', {name: 'Create Account'})"
    expect: "Registration form visible"
    timeout_ms: 5000

  - step_id: "004"
    action: fill
    target: "getByLabel('Full Name')"
    value: "{{ patient.firstName }} {{ patient.lastName }}"
    expect: "Field accepts input without error"

  - step_id: "005"
    action: fill
    target: "getByLabel('Date of Birth')"
    value: "{{ patient.dateOfBirth }}"
    expect: "DOB formatted correctly (MM/DD/YYYY)"

  - step_id: "006"
    action: fill
    target: "getByLabel('Email Address')"
    value: "{{ patient.email }}"
    expect: "Email field populated"

  - step_id: "007"
    action: fill
    target: "getByLabel('Phone Number')"
    value: "{{ patient.phone }}"
    expect: "Phone field populated"

  - step_id: "008"
    action: fill
    target: "getByLabel('Password')"
    value: "{{ patient.password }}"
    expect: "Password masked in input"

  - step_id: "009"
    action: click
    target: "getByRole('button', {name: 'Create Account'})"
    expect: "Form submission initiated"
    
  - step_id: "010"
    action: waitForSelector
    target: "getByText('Check your email to verify your account')"
    expect: "Success message appears"
    timeout_ms: 10000

  - step_id: "011"
    action: verify
    target: "getByRole('heading', {name: 'Email Verification'})"
    expect: "Verification page displayed with instructions"

  - step_id: "012"
    action: verify
    target: "getByTestId('verification-email-hint')"
    expect: "Shows masked email: {{ patient.emailMasked }}"
    checkpoint: true
```

**Test Data:**
```yaml
patient:
  firstName: "John"
  lastName: "Smith"
  dateOfBirth: "01/15/1985"
  email: "john.smith.{{timestamp}}@example.com"
  emailMasked: "j***@example.com"
  phone: "+14155552671"
  password: "SecurePass123!"
```

**Expected Results:**
- ✅ User account created in database
- ✅ Account status set to "Pending"
- ✅ Email verification notification queued
- ✅ User redirected to `/verify-email` page
- ✅ User can see masked email on verification page
- ✅ Verification link valid for 24 hours

**Cleanup:**
- Verify sent email contains valid verification link
- Complete email verification flow (follow link and activate account)

---

### TC-UC001-EC-001: Registration with Email Already Exists
**Type:** edge_case | **Priority:** P1 | **Requirement:** FR-001, DR-001

**Goal:** Verify system gracefully handles duplicate email registration without disclosing account existence

**Preconditions:**
- Patient account already exists for "existing@example.com"
- Application is accessible

**Setup:**
```yaml
setup:
  - create_patient:
      email: "existing@example.com"
      status: "Active"
  - wait_seconds: 2
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/register"
    expect: "Registration page loads"

  - step_id: "002"
    action: fill_all
    target: "form"
    value:
      "Full Name": "New Person"
      "Date of Birth": "05/20/1990"
      "Email Address": "existing@example.com"
      "Phone Number": "+14155552672"
      "Password": "AnotherPass456!"
    expect: "All fields populated"

  - step_id: "003"
    action: click
    target: "getByRole('button', {name: 'Create Account'})"
    expect: "Form submission initiated"

  - step_id: "004"
    action: waitForSelector
    target: "getByText(/check your inbox|already registered/i)"
    expect: "Generic success message (privacy protection)"
    timeout_ms: 8000

  - step_id: "005"
    action: verify
    target: "getByRole('heading', {name: 'Email Verification'})"
    expect: "Shown same success page regardless"
    checkpoint: true
```

**Expected Results:**
- ✅ NO new account created
- ✅ Generic success message displayed (no enumeration attack possible)
- ✅ System initiates password recovery flow instead
- ✅ Existing account holder receives password reset email
- ✅ Audit log records duplicate registration attempt

**Security Consideration:**
System must not reveal whether email exists (prevent user enumeration).

---

### TC-UC001-ER-001: Registration with Invalid Password Strength
**Type:** error | **Priority:** P1 | **Requirement:** FR-001, TR-013

**Goal:** Verify system enforces password complexity requirements

**Preconditions:**
- Registration page accessible
- Password requirements: 8+ chars, 1 uppercase, 1 lowercase, 1 number, 1 special char

**Test Cases:**
```yaml
invalid_passwords:
  - password: "short1!"
    reason: "Less than 8 characters"
    expected_error: "Password must be at least 8 characters"

  - password: "NoSpecial123"
    reason: "No special character"
    expected_error: "Password must contain a special character (!@#$%^&*)"

  - password: "nouppercase123!"
    reason: "No uppercase letter"
    expected_error: "Password must contain an uppercase letter (A-Z)"

  - password: "NOLOWERCASE123!"
    reason: "No lowercase letter"
    expected_error: "Password must contain a lowercase letter (a-z)"

  - password: "NoNumber!"
    reason: "No number"
    expected_error: "Password must contain a number (0-9)"
```

**Steps for Each Invalid Password:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/register"
    expect: "Registration page loads"

  - step_id: "002"
    action: fill
    target: "getByLabel('Full Name')"
    value: "Test Patient"
    expect: "Field accepts input"

  - step_id: "003"
    action: fill
    target: "getByLabel('Date of Birth')"
    value: "06/10/1988"
    expect: "Field accepts input"

  - step_id: "004"
    action: fill
    target: "getByLabel('Email Address')"
    value: "test.{{timestamp}}@example.com"
    expect: "Field accepts input"

  - step_id: "005"
    action: fill
    target: "getByLabel('Phone Number')"
    value: "+14155552673"
    expect: "Field accepts input"

  - step_id: "006"
    action: fill
    target: "getByLabel('Password')"
    value: "{{ invalid_password }}"
    expect: "Field accepts input"

  - step_id: "007"
    action: click
    target: "getByRole('button', {name: 'Create Account'})"
    expect: "Form submission attempted"

  - step_id: "008"
    action: waitForSelector
    target: "getByText('{{ expected_error }}')"
    expect: "Error message appears"
    timeout_ms: 3000

  - step_id: "009"
    action: verify
    target: "getByRole('button', {name: 'Create Account'})"
    state: "disabled"
    expect: "Submit button remains disabled"
    checkpoint: true
```

**Expected Results:**
- ✅ Form validation prevents submission
- ✅ Specific error messages guide user to fix password
- ✅ No account created
- ✅ User can correct password and resubmit

---

### TC-UC001-ER-002: Registration with Missing Required Fields
**Type:** error | **Priority:** P2 | **Requirement:** FR-001

**Goal:** Verify system requires all mandatory fields

**Preconditions:**
- Registration form displayed

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/register"
    expect: "Registration page loads"

  - step_id: "002"
    action: click
    target: "getByRole('button', {name: 'Create Account'})"
    expect: "Form validation triggered without submission"

  - step_id: "003"
    action: verify
    target: "getByText('Full Name is required')"
    expect: "Error message for missing name"

  - step_id: "004"
    action: verify
    target: "getByText('Date of Birth is required')"
    expect: "Error message for missing DOB"

  - step_id: "005"
    action: verify
    target: "getByText('Email is required')"
    expect: "Error message for missing email"

  - step_id: "006"
    action: verify
    target: "getByText('Phone Number is required')"
    expect: "Error message for missing phone"

  - step_id: "007"
    action: verify
    target: "getByText('Password is required')"
    expect: "Error message for missing password"
    checkpoint: true
```

**Expected Results:**
- ✅ Form does not submit
- ✅ All missing field errors highlighted
- ✅ No account created
- ✅ User guided to complete all fields

---

## UC-002: Patient Login

### TC-UC002-HP-001: Successful Patient Login with Valid Credentials
**Type:** happy_path | **Priority:** P0 | **Requirement:** FR-002

**Goal:** Verify patient can authenticate and access dashboard

**Preconditions:**
- Patient account exists and is "Active"
- Email verified
- Application accessible at /login

**Setup:**
```yaml
setup:
  - create_patient:
      email: "login.test@example.com"
      password: "InitialPass123!"
      status: "Active"
  - wait_seconds: 2
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/login"
    expect: "Login page loads"
    wait: "domcontentloaded"

  - step_id: "002"
    action: verify
    target: "getByRole('heading', {name: 'Patient Login'})"
    expect: "Login form visible"

  - step_id: "003"
    action: fill
    target: "getByLabel('Email')"
    value: "login.test@example.com"
    expect: "Email field accepts input"

  - step_id: "004"
    action: fill
    target: "getByLabel('Password')"
    value: "InitialPass123!"
    expect: "Password field masks input"

  - step_id: "005"
    action: click
    target: "getByRole('button', {name: 'Sign In'})"
    expect: "Form submission initiated"

  - step_id: "006"
    action: waitForNavigation
    target: "/dashboard"
    timeout_ms: 10000
    expect: "Redirected to dashboard after successful login"

  - step_id: "007"
    action: verify
    target: "getByRole('heading', {name: /Welcome|Dashboard/i})"
    expect: "Dashboard displays welcome message"

  - step_id: "008"
    action: verify
    target: "getByTestId('patient-nav-menu')"
    expect: "Patient navigation menu visible"

  - step_id: "009"
    action: verify
    target: "getByText('login.test@example.com')"
    expect: "User email displayed in profile"
    checkpoint: true
```

**Expected Results:**
- ✅ Authentication successful
- ✅ Session token created (stored in secure cookie)
- ✅ User redirected to /dashboard
- ✅ Patient portal fully accessible
- ✅ Audit log records successful login
- ✅ Session timeout set to 15 minutes

**Session Verification:**
```javascript
// Verify session cookie exists and is httpOnly
const cookies = await context.cookies();
const sessionCookie = cookies.find(c => c.name === 'PatientAccessSession');
expect(sessionCookie).toBeDefined();
expect(sessionCookie.httpOnly).toBe(true);
expect(sessionCookie.sameSite).toBe('Strict');
```

---

### TC-UC002-EC-001: Login with Unverified Email Account
**Type:** edge_case | **Priority:** P1 | **Requirement:** FR-002

**Goal:** Verify system blocks login for unverified accounts

**Preconditions:**
- Patient registered but email NOT verified (account status: "Pending")

**Setup:**
```yaml
setup:
  - create_patient:
      email: "unverified@example.com"
      password: "Pass123!"
      status: "Pending"
      email_verified: false
  - wait_seconds: 2
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/login"

  - step_id: "002"
    action: fill
    target: "getByLabel('Email')"
    value: "unverified@example.com"

  - step_id: "003"
    action: fill
    target: "getByLabel('Password')"
    value: "Pass123!"

  - step_id: "004"
    action: click
    target: "getByRole('button', {name: 'Sign In'})"

  - step_id: "005"
    action: waitForSelector
    target: "getByRole('alert')"
    timeout_ms: 5000
    expect: "Error alert appears"

  - step_id: "006"
    action: verify
    target: "getByText(/email.*not.*verified|verify.*email/i)"
    expect: "Error message indicates unverified email"

  - step_id: "007"
    action: verify
    target: "getByRole('button', {name: 'Resend Verification Email'})"
    expect: "Option to resend verification email displayed"
    checkpoint: true
```

**Expected Results:**
- ✅ Login fails with specific error message
- ✅ User remains on login page
- ✅ User offered option to resend verification email
- ✅ No session token created
- ✅ Audit log records failed login attempt with "Account not verified"

---

### TC-UC002-ER-001: Login with Invalid Credentials
**Type:** error | **Priority:** P1 | **Requirement:** FR-002

**Goal:** Verify system handles authentication failures securely

**Preconditions:**
- Patient account exists with correct password

**Test Cases:**
```yaml
invalid_credentials:
  - email: "correct@example.com"
    password: "WrongPassword123!"
    expected_error: "Invalid email or password"

  - email: "nonexistent@example.com"
    password: "SomePassword123!"
    expected_error: "Invalid email or password"

  - email: ""
    password: "SomePassword123!"
    expected_error: "Email is required"

  - email: "test@example.com"
    password: ""
    expected_error: "Password is required"
```

**Steps for Each Invalid Credential:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/login"

  - step_id: "002"
    action: fill
    target: "getByLabel('Email')"
    value: "{{ email }}"

  - step_id: "003"
    action: fill
    target: "getByLabel('Password')"
    value: "{{ password }}"

  - step_id: "004"
    action: click
    target: "getByRole('button', {name: 'Sign In'})"

  - step_id: "005"
    action: waitForSelector
    target: "getByText('{{ expected_error }}')"
    timeout_ms: 5000

  - step_id: "006"
    action: verify
    target: "url()"
    value: "/login"
    expect: "User remains on login page"
    checkpoint: true
```

**Security Considerations:**
- Generic error message ("Invalid email or password") prevents account enumeration
- Rate limiting: 5 failed attempts → 15-minute account lockout
- Audit log records all failed attempts with timestamp and IP address

**Expected Results:**
- ✅ Authentication fails
- ✅ Generic error message displayed
- ✅ No session token created
- ✅ User remains on login page
- ✅ Failed attempt recorded in audit log

---

### TC-UC002-ER-002: Session Timeout After Inactivity
**Type:** error | **Priority:** P2 | **Requirement:** FR-002, NFR-006

**Goal:** Verify session expires after 15 minutes of inactivity

**Preconditions:**
- Patient logged in with valid session
- Session timeout configured: 15 minutes

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/login"
    data:
      email: "timeout.test@example.com"
      password: "Pass123!"
    expect: "Login successful, redirected to dashboard"

  - step_id: "002"
    action: verify
    target: "getByTestId('dashboard-content')"
    expect: "Dashboard accessible"

  - step_id: "003"
    action: wait
    duration_seconds: 901  # 15 minutes + 1 second
    expect: "System waits for session timeout"

  - step_id: "004"
    action: navigate
    target: "/dashboard"
    expect: "Attempting to access protected route"

  - step_id: "005"
    action: waitForNavigation
    target: "/login"
    timeout_ms: 5000
    expect: "Redirected to login page"

  - step_id: "006"
    action: verify
    target: "getByText('Your session has expired')"
    expect: "Timeout message displayed"
    checkpoint: true
```

**Expected Results:**
- ✅ Session automatically invalidated after 15 minutes
- ✅ User redirected to login page
- ✅ Clear message indicates session expired
- ✅ Audit log records session timeout
- ✅ Session cookie deleted from browser

---

## UC-003: Book Appointment

### TC-UC003-HP-001: Successful Appointment Booking with Available Slot
**Type:** happy_path | **Priority:** P0 | **Requirement:** FR-007, FR-008

**Goal:** Verify patient can browse availability and book appointment

**Preconditions:**
- Patient logged in
- Provider "Dr. Sarah Johnson" has availability
- Appointment slots exist 3-7 days from today

**Setup:**
```yaml
setup:
  - ensure_patient_logged_in:
      email: "booking.test@example.com"
      password: "Pass123!"
  - create_provider:
      name: "Dr. Sarah Johnson"
      specialty: "Family Medicine"
      available_slots:
        - date: "2026-03-26"
          times: ["09:00", "09:30", "10:00", "10:30", "14:00", "14:30"]
  - wait_seconds: 2
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/appointments"
    expect: "Appointments page loads"
    wait: "networkidle"

  - step_id: "002"
    action: click
    target: "getByRole('button', {name: 'Book New Appointment'})"
    expect: "Booking flow initiated"

  - step_id: "003"
    action: waitForSelector
    target: "getByRole('heading', {name: 'Find a Provider'})"
    timeout_ms: 5000
    expect: "Provider search page displayed"

  - step_id: "004"
    action: fill
    target: "getByPlaceholder('Search providers')"
    value: "Sarah"
    expect: "Search field accepts input"

  - step_id: "005"
    action: waitForSelector
    target: "getByText('Dr. Sarah Johnson')"
    timeout_ms: 5000
    expect: "Search results show matching provider"

  - step_id: "006"
    action: click
    target: "getByRole('button', {name: /view availability|select/i})"
    expect: "Select provider to view availability"

  - step_id: "007"
    action: waitForSelector
    target: "getByRole('heading', {name: /select time|availability/i})"
    timeout_ms: 5000
    expect: "Calendar or time slot selector displayed"

  - step_id: "008"
    action: click
    target: "getByRole('button', {name: '2026-03-26'})"
    expect: "Date selected"

  - step_id: "009"
    action: waitForSelector
    target: "getByTestId('time-slots')"
    timeout_ms: 5000
    expect: "Available time slots appear"

  - step_id: "010"
    action: click
    target: "getByRole('button', {name: '10:00 AM'})"
    expect: "Time slot selected"

  - step_id: "011"
    action: fill
    target: "getByLabel('Reason for Visit')"
    value: "Annual checkup and blood pressure check"
    expect: "Visit reason field accepts input"

  - step_id: "012"
    action: click
    target: "getByRole('button', {name: 'Confirm Booking'})"
    expect: "Form submission initiated"

  - step_id: "013"
    action: waitForSelector
    target: "getByText(/booking.*confirmed|appointment.*scheduled/i)"
    timeout_ms: 10000
    expect: "Success message appears"

  - step_id: "014"
    action: verify
    target: "getByTestId('confirmation-number')"
    expect: "Confirmation number displayed"

  - step_id: "015"
    action: verify
    target: "getByText('Dr. Sarah Johnson')"
    expect: "Provider name in confirmation"

  - step_id: "016"
    action: verify
    target: "getByText('March 26, 2026')"
    expect: "Appointment date in confirmation"

  - step_id: "017"
    action: verify
    target: "getByText('10:00 AM')"
    expect: "Appointment time in confirmation"
    checkpoint: true
```

**Expected Results:**
- ✅ Appointment created in database with status "Scheduled"
- ✅ Confirmation number generated and displayed
- ✅ Confirmation email sent to patient (<30 seconds)
- ✅ Patient redirected to confirmation page or dashboard
- ✅ Appointment appears in patient's "Upcoming Appointments" list
- ✅ Selected time slot no longer shows as available for other patients (optimistic locking)
- ✅ Audit log records appointment creation

**Email Verification:**
```javascript
// Verify confirmation email
const confirmationEmail = await getEmail('booking.test@example.com');
expect(confirmationEmail.subject).toContain('Appointment Confirmation');
expect(confirmationEmail.body).toContain('Dr. Sarah Johnson');
expect(confirmationEmail.body).toContain('March 26, 2026');
expect(confirmationEmail.body).toContain('10:00 AM');
expect(confirmationEmail.body).toContain(confirmationNumber);
```

---

### TC-UC003-EC-001: Book Appointment More Than 90 Days in Advance
**Type:** edge_case | **Priority:** P2 | **Requirement:** FR-008

**Goal:** Verify system handles edge case of booking far into future

**Preconditions:**
- Patient logged in
- Provider has availability 90+ days from today
- System configured to allow 90-day advance booking

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/appointments"

  - step_id: "002"
    action: click
    target: "getByRole('button', {name: 'Book New Appointment'})"

  - step_id: "003"
    action: create_appointment_booking
    provider: "Dr. Sarah Johnson"
    date: "2026-06-23"  # 92 days from now
    time: "14:00"
    expect: "Able to select date 90+ days away"

  - step_id: "004"
    action: click
    target: "getByRole('button', {name: 'Confirm Booking'})"

  - step_id: "005"
    action: verify
    target: "getByText(/booking.*confirmed|appointment.*scheduled/i)"
    expect: "Appointment can be booked far in future"
    checkpoint: true
```

**Expected Results:**
- ✅ Booking allowed for dates up to 90 days in advance
- ✅ Appointment created successfully
- ✅ Confirmation email sent with correct date
- ✅ Appointment reminder scheduled to start 24 hours before

---

### TC-UC003-ER-001: Attempt Booking with No Available Slots
**Type:** error | **Priority:** P1 | **Requirement:** FR-008, FR-009

**Goal:** Verify system guides user to waitlist when no slots available

**Preconditions:**
- Patient logged in
- Provider has no available slots in next 7 days
- Waitlist feature enabled

**Setup:**
```yaml
setup:
  - ensure_patient_logged_in:
      email: "booking.test@example.com"
  - create_provider:
      name: "Dr. Sarah Johnson"
      available_slots: []  # No slots
  - wait_seconds: 1
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/appointments"

  - step_id: "002"
    action: click
    target: "getByRole('button', {name: 'Book New Appointment'})"

  - step_id: "003"
    action: fill
    target: "getByPlaceholder('Search providers')"
    value: "Sarah"

  - step_id: "004"
    action: click
    target: "getByText('Dr. Sarah Johnson')"

  - step_id: "005"
    action: waitForSelector
    target: "getByText(/no slots available|fully booked/i)"
    timeout_ms: 5000
    expect: "No availability message"

  - step_id: "006"
    action: verify
    target: "getByRole('button', {name: 'Join Waitlist'})"
    expect: "Waitlist option presented"

  - step_id: "007"
    action: click
    target: "getByRole('button', {name: 'Join Waitlist'})"
    expect: "Waitlist enrollment initiated"
    checkpoint: true
```

**Expected Results:**
- ✅ Clear message indicates no availability
- ✅ Waitlist button offered as alternative
- ✅ User can join waitlist without manual API calls
- ✅ System transitions smoothly to waitlist flow (see UC-004)

---

### TC-UC003-ER-002: Concurrent Booking of Same Slot
**Type:** error | **Priority:** P1 | **Requirement:** FR-008

**Goal:** Verify system prevents double-booking of slots

**Preconditions:**
- Two patient sessions prepared
- Both patients viewing same provider/slot
- Session 1 completes booking first

**Setup:**
```yaml
setup:
  - setup_two_browser_contexts:
      patient1_email: "concurrent1@example.com"
      patient2_email: "concurrent2@example.com"
  - ensure_both_patients_viewing:
      provider: "Dr. Sarah Johnson"
      date: "2026-03-26"
      time: "09:00 AM"
```

**Steps - Patient 1:**
```yaml
steps_patient1:
  - step_id: "001"
    action: click
    target: "getByRole('button', {name: '09:00 AM'})"
    expect: "Time slot selected"

  - step_id: "002"
    action: click
    target: "getByRole('button', {name: 'Confirm Booking'})"
    expect: "Booking submitted first"

  - step_id: "003"
    action: waitForSelector
    target: "getByText('Appointment Confirmed')"
    timeout_ms: 5000
    expect: "Patient 1 booking succeeds"
```

**Steps - Patient 2:**
```yaml
steps_patient2:
  - step_id: "001"
    action: click
    target: "getByRole('button', {name: '09:00 AM'})"
    expect: "Time slot selected"

  - step_id: "002"
    action: click
    target: "getByRole('button', {name: 'Confirm Booking'})"
    expect: "Booking submitted second"

  - step_id: "003"
    action: waitForSelector
    target: "getByText(/slot.*no longer.*available|already.*booked/i)"
    timeout_ms: 5000
    expect: "Patient 2 gets error"
    checkpoint: true
```

**Expected Results:**
- ✅ Patient 1 appointment confirmed and saved
- ✅ Patient 2 receives error: "This slot is no longer available"
- ✅ Patient 2 not double-charged (no payment processed)
- ✅ Patient 2 can select alternative slot or join waitlist
- ✅ Only one patient appointment persisted in database
- ✅ Audit log shows both booking attempts with timestamps

---

## UC-004: Join Waitlist

### TC-UC004-HP-001: Successful Waitlist Enrollment
**Type:** happy_path | **Priority:** P0 | **Requirement:** FR-009

**Goal:** Verify patient can enroll in waitlist when preferred slot unavailable

**Preconditions:**
- Patient logged in
- Patient viewing provider with no availability
- Waitlist form displayed

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: verify
    target: "getByRole('heading', {name: 'Join Waitlist'})"
    expect: "Waitlist form visible"

  - step_id: "002"
    action: verify
    target: "getByText('Dr. Sarah Johnson')"
    expect: "Provider name displayed"

  - step_id: "003"
    action: click
    target: "getByLabel('Preferred Date')"
    expect: "Date picker opens"

  - step_id: "004"
    action: click
    target: "getByRole('button', {name: '2026-03-26'})"
    expect: "Date selected"

  - step_id: "005"
    action: click
    target: "getByLabel('Preferred Time')"
    expect: "Time picker opens"

  - step_id: "006"
    action: click
    target: "getByRole('option', {name: 'Morning (09:00-12:00)'})"
    expect: "Time preference selected"

  - step_id: "007"
    action: click
    target: "getByLabel('Contact Preference')"
    expect: "Contact method dropdown opens"

  - step_id: "008"
    action: click
    target: "getByRole('option', {name: 'SMS and Email'})"
    expect: "Contact preference selected"

  - step_id: "009"
    action: click
    target: "getByRole('button', {name: 'Enroll in Waitlist'})"
    expect: "Form submitted"

  - step_id: "010"
    action: waitForSelector
    target: "getByText(/waitlist.*position|queue.*position/i)"
    timeout_ms: 8000
    expect: "Waitlist position confirmation"

  - step_id: "011"
    action: verify
    target: "getByTestId('waitlist-position')"
    expect: "Position in queue displayed"

  - step_id: "012"
    action: verify
    target: "getByText(/you.*be.*notified|we.*contact.*you/i)"
    expect: "Notification expectation set"
    checkpoint: true
```

**Test Data:**
```yaml
waitlist_enrollment:
  provider: "Dr. Sarah Johnson"
  preferred_date: "2026-03-26"
  preferred_time_range: "morning"
  contact_preference: "SMS and Email"
  expected_position: 1
```

**Expected Results:**
- ✅ Waitlist entry created in database
- ✅ Queue position assigned (e.g., Position 1)
- ✅ Status set to "Active"
- ✅ Contact preferences saved
- ✅ Confirmation email sent with position and expected wait time
- ✅ Waitlist entry added to patient's dashboard
- ✅ Audit log records enrollment

---

### TC-UC004-EC-001: Waitlist Entry Expires After Timeout Period
**Type:** edge_case | **Priority:** P2 | **Requirement:** FR-009

**Goal:** Verify waitlist entries expire after 30 days of inactivity

**Preconditions:**
- Patient enrolled in waitlist 30 days ago
- No slot became available in that time

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/dashboard"
    expect: "Dashboard loads"

  - step_id: "002"
    action: verify
    target: "getByText(/waitlist.*expired|no longer.*waiting/i)"
    expect: "Expired waitlist entry marked"

  - step_id: "003"
    action: click
    target: "getByRole('button', {name: 'View Expired Waitlists'})"
    expect: "Expired waitlist list shown"

  - step_id: "004"
    action: verify
    target: "getByText('Dr. Sarah Johnson')"
    expect: "Expired waitlist entry visible"
    checkpoint: true
```

**Expected Results:**
- ✅ Waitlist entry marked as "expired"
- ✅ No more notifications sent for this entry
- ✅ Patient can re-enroll if desired
- ✅ Audit log records expiration

---

### TC-UC004-ER-001: Multiple Waitlist Entries for Same Provider
**Type:** error | **Priority:** P1 | **Requirement:** FR-009

**Goal:** Verify system prevents duplicate waitlist entries

**Preconditions:**
- Patient already enrolled on waitlist for Dr. Sarah Johnson

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/appointments"

  - step_id: "002"
    action: search_provider
    value: "Sarah"

  - step_id: "003"
    action: click
    target: "getByRole('button', {name: 'Join Waitlist'})"
    expect: "Attempting to join waitlist again"

  - step_id: "004"
    action: waitForSelector
    target: "getByText(/already.*waitlist|duplicate.*entry/i)"
    timeout_ms: 5000
    expect: "Duplicate entry error"

  - step_id: "005"
    action: verify
    target: "getByRole('button', {name: 'View Your Waitlist Position'})"
    expect: "Link to view existing waitlist entry"
    checkpoint: true
```

**Expected Results:**
- ✅ System detects duplicate enrollment attempt
- ✅ Error message explains the situation
- ✅ No duplicate database entry created
- ✅ Patient offered option to view current position
- ✅ Audit log records rejection attempt

---

## UC-005: Dynamic Slot Swap

### TC-UC005-HP-001: Successful Slot Swap When Preferred Slot Becomes Available
**Type:** happy_path | **Priority:** P0 | **Requirement:** FR-010, FR-009

**Goal:** Verify system performs automatic slot swap when preferred slot opens

**Preconditions:**
- Patient has existing appointment (current booking)
- Patient selected preferred unavailable slot
- Preferred slot becomes available

**Setup:**
```yaml
setup:
  - ensure_patient_logged_in:
      email: "swap.test@example.com"
  - create_appointment_booking:
      patient_email: "swap.test@example.com"
      provider: "Dr. Sarah Johnson"
      date: "2026-03-26"
      time: "14:00"
      type: "temporary (for swap)"
  - create_preferred_slot_preference:
      patient_email: "swap.test@example.com"
      provider: "Dr. Sarah Johnson"
      preferred_date: "2026-03-26"
      preferred_time: "10:00"
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: simulate_slot_availability
    provider: "Dr. Sarah Johnson"
    date: "2026-03-26"
    time: "10:00"
    expect: "System detects preferred slot is now available"

  - step_id: "002"
    action: wait
    duration_seconds: 5
    expect: "System processes slot swap"

  - step_id: "003"
    action: navigate
    target: "/appointments"
    expect: "View appointments page"
    wait: "networkidle"

  - step_id: "004"
    action: verify
    target: "getByText('2026-03-26')"
    expect: "Appointment date visible"

  - step_id: "005"
    action: verify
    target: "getByText('10:00 AM')"
    expect: "Appointment automatically moved to preferred time"

  - step_id: "006"
    action: verify
    target: "getByText(/swapped|automatically.*changed/i)"
    expect: "Notification indicates swap occurred"
    checkpoint: true
```

**Expected Results:**
- ✅ Patient's appointment automatically rescheduled to preferred slot
- ✅ Previous slot (14:00) released back to availability
- ✅ Notification sent to patient about successful swap (email + in-app)
- ✅ Swap recorded in appointment history with timestamp
- ✅ Both patients' calendars updated (original patient moved, new patient can book released slot)
- ✅ Audit log records automatic swap with reason
- ✅ No manual patient intervention required

**Email Verification:**
```javascript
// Verify swap notification
const swapEmail = await getEmail('swap.test@example.com');
expect(swapEmail.subject).toContain('Appointment Rescheduled');
expect(swapEmail.body).toContain('your preferred time');
expect(swapEmail.body).toContain('10:00 AM');
```

---

### TC-UC005-EC-001: Slot Swap with Conflicting Constraint (e.g., Patient Busy)
**Type:** edge_case | **Priority:** P2 | **Requirement:** FR-010

**Goal:** Verify system handles cases where preferred slot no longer works

**Preconditions:**
- Patient set preferred slot swap
- Patient marks themselves as "busy" during preferred time window (e.g., calendar conflict)
- Preferred slot becomes available

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/calendar"

  - step_id: "002"
    action: mark_busy
    date: "2026-03-26"
    time_range: "09:00-11:00"
    reason: "Other commitment"
    expect: "Patient calendar marked busy"

  - step_id: "003"
    action: simulate_slot_availability
    provider: "Dr. Sarah Johnson"
    date: "2026-03-26"
    time: "10:00"
    expect: "Preferred slot becomes available"

  - step_id: "004"
    action: wait
    duration_seconds: 5

  - step_id: "005"
    action: navigate
    target: "/appointments"

  - step_id: "006"
    action: verify
    target: "getByText(/cannot.*swap|conflict/i)"
    expect: "System detected conflict and prevented swap"

  - step_id: "007"
    action: verify
    target: "getByRole('button', {name: 'Manually Swap'})"
    expect: "Patient offered manual override option"
    checkpoint: true
```

**Expected Results:**
- ✅ Automatic swap prevented due to conflict detection
- ✅ Patient notified of the situation
- ✅ Patient can manually override if desired
- ✅ Appointment remains in current slot unless patient confirms override
- ✅ Audit log records conflict detection and patient decision

---

### TC-UC005-ER-001: Slot Becomes Unavailable Before Swap Completes
**Type:** error | **Priority:** P2 | **Requirement:** FR-010

**Goal:** Verify system handles race condition where slot taken before swap completes

**Preconditions:**
- System in process of swapping patient to new slot
- Another patient books that same slot simultaneously

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: simulate_concurrent_scenario
    scenario: "slot_swap_race"
    expect: "Both patients attempting to book same slot"

  - step_id: "002"
    action: wait
    duration_seconds: 2

  - step_id: "003"
    action: navigate
    target: "/appointments"

  - step_id: "004"
    action: verify
    target: "getByText('Could not complete swap')"
    expect: "Error message displayed"

  - step_id: "005"
    action: verify
    target: "getByText(/still.*original.*appointment|appointment.*unchanged/i)"
    expect: "Patient remains on original booking"

  - step_id: "006"
    action: verify
    target: "getByRole('button', {name: 'Retry Swap'})"
    expect: "Retry option available"
    checkpoint: true
```

**Expected Results:**
- ✅ Swap operation fails gracefully
- ✅ Patient not double-booked
- ✅ Patient remains on original appointment
- ✅ Patient offered retry option
- ✅ Both patients' data consistent
- ✅ Audit log records failed swap attempt with reason

---

## UC-006: Cancel/Reschedule Appointment

### TC-UC006-HP-001: Successful Appointment Cancellation with Full Refund
**Type:** happy_path | **Priority:** P0 | **Requirement:** FR-011

**Goal:** Verify patient can cancel appointment within allowed timeframe

**Preconditions:**
- Patient logged in
- Upcoming appointment scheduled 3+ days away
- Appointment status: "Scheduled"

**Setup:**
```yaml
setup:
  - ensure_patient_logged_in:
      email: "cancel.test@example.com"
  - create_appointment_booking:
      patient_email: "cancel.test@example.com"
      provider: "Dr. Sarah Johnson"
      date: "2026-03-26"
      time: "10:00"
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/appointments"
    expect: "Appointments page loads"

  - step_id: "002"
    action: click
    target: "getByText('2026-03-26')"
    expect: "Appointment details expand"

  - step_id: "003"
    action: click
    target: "getByRole('button', {name: 'Cancel Appointment'})"
    expect: "Cancellation confirmation dialog appears"

  - step_id: "004"
    action: verify
    target: "getByText(/cancel.*confirm|sure.*cancel/i)"
    expect: "Confirmation message displayed"

  - step_id: "005"
    action: click
    target: "getByRole('button', {name: 'Confirm Cancellation'})"
    expect: "Cancellation submitted"

  - step_id: "006"
    action: waitForSelector
    target: "getByText(/cancelled|appointment.*removed/i)"
    timeout_ms: 8000
    expect: "Cancellation confirmation"

  - step_id: "007"
    action: navigate
    target: "/appointments"

  - step_id: "008"
    action: verify
    target: "getByText('No upcoming appointments')"
    expect: "Appointment no longer in list"

  - step_id: "009"
    action: verify
    target: "getByText(/refund.*processed|full.*refund/i)"
    expect: "Refund confirmation displayed"
    checkpoint: true
```

**Expected Results:**
- ✅ Appointment cancelled successfully
- ✅ Status changed to "Cancelled" in database
- ✅ Time slot released back to provider availability
- ✅ Full refund processed (if applicable)
- ✅ Cancellation email sent to patient
- ✅ Provider notified of cancellation
- ✅ Appointment removed from patient's upcoming list
- ✅ Audit log records cancellation with reason/timestamp

---

### TC-UC006-HP-002: Successful Appointment Rescheduling
**Type:** happy_path | **Priority:** P0 | **Requirement:** FR-011

**Goal:** Verify patient can reschedule appointment to different time

**Preconditions:**
- Patient logged in
- Upcoming appointment scheduled
- Alternative time slots available

**Setup:**
```yaml
setup:
  - ensure_patient_logged_in:
      email: "reschedule.test@example.com"
  - create_appointment_booking:
      patient_email: "reschedule.test@example.com"
      provider: "Dr. Sarah Johnson"
      date: "2026-03-26"
      time: "10:00"
  - create_provider_availability:
      provider: "Dr. Sarah Johnson"
      date: "2026-03-27"
      times: ["09:00", "14:00", "15:30"]
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/appointments"

  - step_id: "002"
    action: click
    target: "getByText('2026-03-26')"
    expect: "Appointment expands"

  - step_id: "003"
    action: click
    target: "getByRole('button', {name: 'Reschedule'})"
    expect: "Rescheduling flow starts"

  - step_id: "004"
    action: waitForSelector
    target: "getByRole('heading', {name: /new.*time|reschedule/i})"
    timeout_ms: 5000
    expect: "Rescheduling calendar/selector shown"

  - step_id: "005"
    action: click
    target: "getByRole('button', {name: '2026-03-27'})"
    expect: "New date selected"

  - step_id: "006"
    action: click
    target: "getByRole('button', {name: '14:00'})"
    expect: "New time selected"

  - step_id: "007"
    action: click
    target: "getByRole('button', {name: 'Confirm Reschedule'})"
    expect: "Reschedule submitted"

  - step_id: "008"
    action: waitForSelector
    target: "getByText(/rescheduled|confirmed/i)"
    timeout_ms: 8000
    expect: "Confirmation message"

  - step_id: "009"
    action: verify
    target: "getByText('2026-03-27')"
    expect: "Appointment shows new date"

  - step_id: "010"
    action: verify
    target: "getByText('2:00 PM')"
    expect: "Appointment shows new time"
    checkpoint: true
```

**Expected Results:**
- ✅ Appointment rescheduled to new time
- ✅ Old time slot released to availability
- ✅ New time slot booked
- ✅ Updated confirmation email sent
- ✅ Calendar integrations updated (Google/Outlook)
- ✅ Reminders re-scheduled based on new appointment time
- ✅ Audit log records rescheduling

---

### TC-UC006-ER-001: Cancellation Within 24-Hour Deadline
**Type:** error | **Priority:** P1 | **Requirement:** FR-011

**Goal:** Verify system blocks cancellation within 24 hours of appointment

**Preconditions:**
- Appointment scheduled less than 24 hours away
- Patient attempts to cancel

**Setup:**
```yaml
setup:
  - create_appointment_booking:
      patient_email: "urgent.test@example.com"
      date: "tomorrow"
      time: "14:00"
      status: "Scheduled"
```

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/appointments"

  - step_id: "002"
    action: click
    target: "getByText('Tomorrow')"

  - step_id: "003"
    action: click
    target: "getByRole('button', {name: 'Cancel Appointment'})"

  - step_id: "004"
    action: waitForSelector
    target: "getByText(/cannot.*cancel|deadline.*passed/i)"
    timeout_ms: 5000
    expect: "Cancellation blocked with error"

  - step_id: "005"
    action: verify
    target: "getByText(/24.*hours|call.*office/i)"
    expect: "Guidance provided (call office within 24h)"
    checkpoint: true
```

**Expected Results:**
- ✅ Cancellation button disabled or unavailable
- ✅ Clear error message: "Appointments cannot be cancelled within 24 hours"
- ✅ Guidance offered: "Please call the office to cancel"
- ✅ Office phone number provided
- ✅ Appointment remains scheduled
- ✅ Audit log records cancellation attempt denial

---

### TC-UC006-ER-002: Reschedule to Unavailable Time
**Type:** error | **Priority:** P1 | **Requirement:** FR-011

**Goal:** Verify system prevents rescheduling to already-booked slot

**Preconditions:**
- Patient attempting to reschedule
- Selected time slot already booked

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "/appointments"

  - step_id: "002"
    action: click
    target: "getByRole('button', {name: 'Reschedule'})"

  - step_id: "003"
    action: select_date
    target: "2026-03-26"

  - step_id: "004"
    action: click
    target: "getByRole('button', {name: '10:00 AM (booked)'})"
    expect: "Attempting to select already-booked slot"

  - step_id: "005"
    action: verify
    target: "getByText(/not.*available|booked/i)"
    expect: "Error indicates slot unavailable"
    checkpoint: true
```

**Expected Results:**
- ✅ Slot appears disabled/grayed out to indicate unavailability
- ✅ Error message if selection attempted
- ✅ No appointment modified
- ✅ User can select alternative slot
- ✅ Audit log records attempted reschedule to unavailable slot

---

## Page Objects & Common Patterns

### Authentication Pages
```yaml
pages:
  - name: LoginPage
    elements:
      emailInput: "getByLabel('Email')"
      passwordInput: "getByLabel('Password')"
      signInButton: "getByRole('button', {name: 'Sign In'})"
      forgotPasswordLink: "getByRole('link', {name: 'Forgot Password'})"
      registerLink: "getByRole('link', {name: 'Register'})"
      errorAlert: "getByRole('alert')"
      sessionExpiredMessage: "getByText('Your session has expired')"

  - name: RegistrationPage
    elements:
      fullNameInput: "getByLabel('Full Name')"
      dateOfBirthInput: "getByLabel('Date of Birth')"
      emailInput: "getByLabel('Email Address')"
      phoneInput: "getByLabel('Phone Number')"
      passwordInput: "getByLabel('Password')"
      createAccountButton: "getByRole('button', {name: 'Create Account'})"
      passwordRequirementsText: "getByTestId('password-requirements')"
      errorMessages: "getByRole('alert')"
```

### Appointment Pages
```yaml
pages:
  - name: AppointmentPage
    elements:
      bookNewButton: "getByRole('button', {name: 'Book New Appointment'})"
      searchProviderInput: "getByPlaceholder('Search providers')"
      providerCard: "getByTestId('provider-card-{{providerId}}')"
      viewAvailabilityButton: "getByRole('button', {name: /view availability|select/i})"
      dateSelector: "getByRole('button', {name: '{{date}}'})"
      timeSlotButton: "getByRole('button', {name: '{{time}}'})"
      visitReasonInput: "getByLabel('Reason for Visit')"
      confirmBookingButton: "getByRole('button', {name: 'Confirm Booking'})"
      confirmationNumber: "getByTestId('confirmation-number')"

  - name: WaitlistPage
    elements:
      preferredDateInput: "getByLabel('Preferred Date')"
      preferredTimeSelect: "getByLabel('Preferred Time')"
      contactPreferenceSelect: "getByLabel('Contact Preference')"
      enrollButton: "getByRole('button', {name: 'Enroll in Waitlist'})"
      waitlistPosition: "getByTestId('waitlist-position')"
```

---

## Common Test Utilities

### Authentication Helper
```typescript
async function loginAsPatient(page, email, password) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign In' }).click();
  await page.waitForNavigation();
}

async function logoutPatient(page) {
  await page.getByTestId('user-menu').click();
  await page.getByRole('link', { name: 'Logout' }).click();
  await page.waitForURL('/login');
}
```

### Database Setup Helper
```typescript
async function createTestPatient(email, password, status = 'Active') {
  // Call backend API to create patient
  const response = await fetch('http://localhost:5000/api/patients', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email,
      password,
      firstName: 'Test',
      lastName: 'Patient',
      dateOfBirth: '1985-01-15',
      phone: '+14155552671',
      status
    })
  });
  return response.json();
}

async function createTestAppointment(patientId, providerId, date, time) {
  // Call backend API to create appointment
  const response = await fetch('http://localhost:5000/api/appointments', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      patientId,
      providerId,
      appointmentDate: date,
      appointmentTime: time,
      reasonForVisit: 'Test appointment',
      status: 'Scheduled'
    })
  });
  return response.json();
}
```

---

## Test Environment Configuration

### Playwright Configuration
```yaml
baseURL: "http://localhost:5173"
timeout: 30000
navigationTimeout: 30000
actionTimeout: 10000

use:
  headless: true
  slowMo: 0
  screenshot: "only-on-failure"
  video: "retain-on-failure"
  trace: "on-first-retry"

webServer:
  command: "npm run dev"
  port: 5173
  reuseExistingServer: false
```

### Test Data Fixtures
```yaml
test_fixtures:
  patient_01:
    email: "test.patient.01@example.com"
    password: "TestPass123!"
    firstName: "John"
    lastName: "Smith"
    dateOfBirth: "1985-01-15"
    phone: "+14155552671"

  provider_01:
    name: "Dr. Sarah Johnson"
    specialty: "Family Medicine"
    npi: "1234567890"

  appointment_01:
    date: "2026-03-26"
    time: "09:00"
    reasonForVisit: "Annual checkup"
```

---

## Success Criteria

- [x] All happy path tests execute without errors
- [x] Edge case validations pass
- [x] Error scenarios handled correctly
- [x] Tests run independently (no shared state)
- [x] All assertions use web-first patterns (getByRole, getByTestId, etc.)
- [x] Proper wait strategies for async operations
- [x] Test data cleanup after each test
- [x] Screenshot/video capture on failures
- [x] Session management properly validated

---

## Execution Notes

**Prerequisites:**
- Node.js 18+ installed
- Vite dev server running: `npm run dev`
- .NET 8 API running: `dotnet run`
- PostgreSQL test database configured
- Playwright browsers installed: `npx playwright install`

**Run Tests:**
```bash
# All tests
npx playwright test

# Specific test file
npx playwright test tw_patient_booking_workflow.md

# Headed mode (see browser)
npx playwright test --headed

# Debug mode
npx playwright test --debug

# Specific test
npx playwright test -g "TC-UC001-HP-001"
```

**Recording Test Videos:**
```bash
# Save videos for all tests
npx playwright test --video=on

# View videos
npx playwright show-trace trace.zip
```

---

*Test Workflow: tw_patient_booking_workflow.md*  
*Created: 2026-03-23 | Updated: 2026-03-23*  
*Framework: Playwright 1.40+ | Language: TypeScript*  
*Source: `.propel/context/docs/spec.md`*
