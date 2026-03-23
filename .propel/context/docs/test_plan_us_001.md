# E2E Test Plan: Patient Account Registration (US-001)

**Document Version:** 1.0  
**Last Updated:** 2026-03-20  
**Status:** Final  /
**Scope:** Patient Account Registration with Email Validation

---

## 1. Test Objectives

- Ensure patient account creation with email validation works end-to-end without errors
- Validate secure password enforcement meets all requirements (minimum 8 chars, uppercase, lowercase, number, special character)
- Verify email delivery within 2-minute SLA and verification link expiration after 24 hours
- Validate graceful error handling for duplicate emails, malformed inputs, and service failures
- Ensure form data persistence and inline validation feedback enhance user experience
- Confirm account activation only occurs after successful email verification
- Verify system prevents concurrent registration attempts with the same email address

---

## 2. Scope

### In Scope

| Category | Items | Requirement IDs |
|----------|-------|-----------------|
| Functional | Account creation with email/password, verification email delivery, link validation, duplicate email detection, password requirement enforcement, form validation | FR-001, FR-002, FR-003, FR-004 |
| User Journeys | Complete registration → email verification → sign-in redirect, Error recovery (duplicate email, expired link, invalid password) | UC-001 |
| Non-Functional | Email delivery SLA (2 minutes), Registration submit response time (<2 seconds), Password encryption, Token cryptographic security | NFR-001, NFR-002, NFR-003 |
| Technical | Email service integration, Account database persistence, Token generation and validation | TR-001, TR-002 |
| Data | Patient account entity creation, Email verification token storage, Audit trail for registration events | DR-001, DR-002 |
| User Experience | Inline validation feedback, Error message clarity, Form data persistence on validation errors, Visual distinction for patient portal | UXR-002, UXR-504, UXR-601, UXR-603 |

### Out of Scope

- Temporary/disposable email domain filtering (planned for future phase)
- Multi-factor authentication (separate epic)
- Social login options (EP-002)
- Account recovery workflows beyond "Sign In" and "Reset Password" links
- Extensive load testing beyond baseline performance thresholds
- Email template rendering in different email clients (separate QA phase)

---

## 3. Risk Assessment

### High-Risk Areas (P0 - Critical)

| Risk | Impact | Likelihood | Mitigation | Test Coverage |
|------|--------|-----------|------------|-----------------|
| Email delivery fails or delays | User cannot complete onboarding; registration appears broken | Medium | Mock email service in DEV; verify SLA in QA/Staging | TC-FR-002-ER-EmailFailure, TC-NFR-002-PERF |
| Password validation not enforced | Weak passwords compromise security; compliance violation | High | Unit test each requirement; integration test all combinations | TC-FR-003-EC-*, TC-FR-003-ER-* |
| Duplicate email not detected correctly | Data integrity violation; user confusion with multiple accounts | High | Test concurrent registrations; verify database unique constraint | TC-FR-001-EC-DupEmail, E2E-UC-001-DuplicateEmail |
| Verification link expires unexpectedly | User locked out; poor user experience | High | Test 24h expiration boundary; verify resend mechanism | TC-FR-002-EC-TokenExpired, TC-FR-002-ER-BadToken |

### Medium-Risk Areas (P1 - High)

| Risk | Impact | Likelihood | Mitigation | Test Coverage |
|------|--------|-----------|------------|-----------------|
| Form data lost on validation error | User frustration; form abandonment | Medium | Test data persistence across multiple validation attempts | TC-FR-004-HP, TC-FR-004-EC-PartialInput |
| Error messages unclear or unhelpful | User confusion; increased support requests | Low | Review error messages with UX; verify links in errors | TC-FR-001-ER-*, TC-NFR-003-UX |
| Inline validation performance impacts UX | Perceived sluggishness; user frustration | Low | Performance test validation logic; monitor response times | TC-NFR-001-PERF |

### Low-Risk Areas (P2)

- Non-standard email formats (already have regex validation)
- Edge case password characters (covered by character class tests)
- Session timeout during registration (beyond scope for this US)

---

## 4. Test Strategy

### Test Pyramid Allocation

| Level | Target Coverage | Focus | Expected Test Count |
|-------|-----------------|-------|---------------------|
| E2E | 8-10% | Critical user journeys: happy path, error recovery, email verification | 3-4 tests |
| Integration | 25-30% | Email service, token validation, database persistence, API contracts | 5-6 tests |
| Unit | 60-70% | Password validation (each requirement), email format validation, form state management | 15-18 tests |
| **Total** | **100%** | **All requirement coverage with traceability** | **~33 tests** |

### E2E Approach

**Horizontal Testing (UI-Driven):**
- User interacts with registration form
- Validates email/password inputs through UI
- Submits form and verifies response
- Receives and validates verification email
- Clicks verification link and confirms account activation

**Vertical Testing (API → DB):**
- API layer receives registration request
- Database validates unique email constraint
- Verification token stored with expiration metadata
- Account status transitions from "pending" to "active"
- Audit trail recorded for compliance

### Environment Strategy

| Environment | Purpose | Data Strategy | Email Service | Validation Focus |
|-------------|---------|--------|---|---|
| **DEV** | Rapid iteration, smoke tests | Mocked data, cleaned between runs | Mocked/captured | Happy path validation |
| **QA** | Full regression, edge cases | Snapshot test data with cleanup | Mock with capture capability | All requirements including failures |
| **Staging** | Pre-production validation, performance | Production-like test data | Real service (staging account) | SLA verification, end-to-end realism |

---

## 5. Test Cases

### 5.1 Functional Test Cases - Account Creation (FR-001)

#### TC-FR-001-HP: Happy Path Registration with Valid Credentials

| Field | Value |
|-------|-------|
| Requirement | FR-001 (Account creation with email & password) |
| Use Case | UC-001 (Patient Registration) |
| Type | Happy Path |
| Priority | P0 |
| Preconditions | Landing page loaded; Sign Up button visible; email not previously registered |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | User on Sign Up page | Enters email "patient@example.com" | Email field accepts input and displays no error |
| 2 | Email entered | Enters password "SecurePass123!" | Password field accepts input and validates in real-time |
| 3 | Email & password valid | Clicks "Register" button | Request submitted successfully |
| 4 | Request in flight | System processes registration | Account created in database; response received in <2s |
| 5 | Account created | Response indicates success | Page displays "Verification email sent to patient@example.com" |
| 6 | Success message shown | Email service delivers | Verification email arrives in <2 minutes |

**Test Data:**

| Field | Valid Value | Invalid Value Tested | Boundary Value |
|-------|-------------|-------------------|-----------------|
| Email | patient@example.com | **See TC-FR-001-EC-InvalidEmail** | {single char}@{domain}.com |
| Password | SecurePass123! | **See TC-FR-003 tests** | Exactly 8 chars with all requirements |
| Full Name | John Doe | Empty string | Null |

**Expected Results:**

- [ ] Account created successfully in database (verify with direct DB query)
- [ ] HTTP Response 201 (Created) with account details (no sensitive data)
- [ ] Verification email queued for delivery
- [ ] Email sent timestamp within registration transaction
- [ ] Account status = "pending" until email verified
- [ ] User can see confirmation message within 2 seconds

**Postconditions:**

- Account exists with status "pending"
- Verification token generated and stored
- Email queued in outbound message service
- Registration event logged in audit trail
- Session created for user (if auto-login enabled)

---

#### TC-FR-001-EC-DupEmail: Duplicate Email Detection

| Field | Value |
|-------|-------|
| Requirement | FR-001 (AC #3: Duplicate email handling) |
| Use Case | UC-001 |
| Type | Edge Case |
| Priority | P0 |
| Preconditions | Email "john@example.com" already registered in system |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Duplicate email exists in system | New user attempts registration with same email | System detects duplicate before form submission |
| 2 | User enters duplicate email | System validates in real-time or on submit | Error message displayed below email field |
| 3 | Error shown | User clicks "Sign In" link in error message | Redirected to sign-in page |
| 4 | User on sign-in page | User enters email and correct password | Sign-in succeeds; user authenticated |

**Test Data:**

| Field | Value |
|-------|-------|
| Existing Email | john@example.com (pre-seeded in test DB) |
| Duplicate Email Attempt | john@example.com |
| Duplicate Error Message | "This email is already registered" (exact text verification) |

**Expected Results:**

- [ ] Form submission prevented or error returned before DB insert attempt
- [ ] Error message text: "This email is already registered"
- [ ] Error includes functional links: "Sign In" and "Reset Password"
- [ ] No account created; existing account untouched
- [ ] Database unique constraint not violated (no duplicate attempts in logs)
- [ ] User can successfully recover via provided links

**Postconditions:**

- No new account created
- Form data cleared or message dismissed
- User can retry with different email or use recovery links

---

#### TC-FR-001-EC-InvalidEmail: Malformed Email Address

| Field | Value |
|-------|-------|
| Requirement | FR-001, AC #1 (Email validation) |
| Type | Edge Case |
| Priority | P1 |
| Preconditions | Registration form loaded |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Empty email field | User enters "notanemail" (no @) | Real-time validation error appears |
| 2 | Invalid email shown | User enters "missing@domain" (no TLD) | Error message updates |
| 3 | Error visible | User enters "user@.com" (no domain) | Error persists |
| 4 | Invalid formats tested | User enters valid "test@example.com" | Error clears; form progresses |

**Test Data:**

| Invalid Format | Expected Error | Valid Replacement |
|---|---|---|
| notanemail | Please enter a valid email address | test@example.com |
| user@domain | Invalid email domain | test@example.com |
| @example.com | Missing username | test@example.com |
| user@domain@com | Multiple @ symbols | test@example.com |

**Expected Results:**

- [ ] Each invalid format triggers validation error immediately
- [ ] Error message: "Please enter a valid email address"
- [ ] Submit button disabled while email invalid
- [ ] Valid email clears error and enables submit
- [ ] No server calls made for invalid formats (client-side validation first)

**Postconditions:**

- User can correct email and proceed
- No partial account creation in database

---

#### TC-FR-001-ER-EmptyFields: Required Fields Validation

| Field | Value |
|-------|-------|
| Requirement | FR-001, AC #5 (Form validation with error highlighting) |
| Type | Error Case |
| Priority | P1 |
| Preconditions | Registration form loaded |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | All fields empty | User clicks "Register" | Form prevents submission |
| 2 | Submit blocked | System highlights all required fields | Red border or error icon visible on each field |
| 3 | Fields highlighted | Error messages appear | Each field shows specific error (e.g., "Email required") |
| 4 | User sees all errors | User enters email in one field | That field's error clears; others remain |
| 5 | Partial data entered | User completes all fields | Form enables submit; all errors cleared |

**Test Data:**

| Field | Empty | Filled |
|-------|-------|--------|
| Email | (blank) | test@example.com |
| Password | (blank) | SecurePass123! |
| All | (all blank) | (all filled) |

**Expected Results:**

- [ ] Form submission prevented when any required field empty
- [ ] All empty fields highlighted with visual indicator (red border, icon, or color change)
- [ ] Error message per field: email shows different error than password
- [ ] Error messages specific and actionable ("Email is required" vs generic "Invalid")
- [ ] User can correct one field at a time; form evaluates incrementally

**Postconditions:**

- Form data preserved while user corrects errors (AC #5)
- No form reset unless user explicitly clicks reset button
- Submit enabled only when all validations pass

---

#### TC-FR-001-ER-ServiceUnavailable: Registration Service Failure

| Field | Value |
|-------|-------|
| Requirement | FR-001 (Account creation robustness) |
| Type | Error Case |
| Priority | P0 |
| Preconditions | Registration form loaded; backend service configured to fail |
| Environment | QA (with service failure injection) |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Backend unavailable (simulate timeout) | User submits valid registration | Network request hangs then times out |
| 2 | Timeout occurred | System waits >5 seconds | User sees error message with retry option |
| 3 | Error shown | User clicks "Retry" | Form resubmits; catches same error or succeeds |
| 4 | Service recovered | User retries after service restart | Registration succeeds; account created |

**Test Data:**

| Scenario | Backend Response | Expected UI Response |
|---|---|---|
| Timeout (connection reset) | No response after 5s | "Connection error. Please try again." |
| 500 Server Error | HTTP 500 response | "Server error. Please contact support." |
| 503 Service Unavailable | HTTP 503 response | "Service temporarily unavailable. Please try again later." |
| DB Connection Error | HTTP 500 (internal error) | Generic error with support contact |

**Expected Results:**

- [ ] Request/response handling respects timeouts (max 5-10 seconds)
- [ ] User-friendly error message (not technical stack trace)
- [ ] Retry mechanism available without form reset
- [ ] On successful retry, account created without duplicate attempts
- [ ] Server logs capture error details for debugging
- [ ] Audit trail records failed attempt and successful retry

**Postconditions:**

- Account created on successful retry
- User can proceed to email verification step
- No orphaned account records or incomplete state

---

### 5.2 Functional Test Cases - Email Verification (FR-002)

#### TC-FR-002-HP: Happy Path Email Verification

| Field | Value |
|-------|-------|
| Requirement | FR-002 (Email verification with link validation) |
| Use Case | UC-001 |
| Type | Happy Path |
| Priority | P0 |
| Preconditions | Account registered with status "pending"; verification email received |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Verification email in inbox | User clicks verification link | Browser navigates to verification page |
| 2 | Verification page loaded | User views page (link already consumed for verification) | Page displays "Email verified successfully!" |
| 3 | Account verified | User redirected | Redirected to sign-in page with success banner |
| 4 | Sign-in page shown | User enters email & password | Authentication succeeds; account marked "active" |
| 5 | Account active | User accesses dashboard | Dashboard loads; patient portal accessible |

**Test Data:**

| Field | Value |
|-------|-------|
| Registered Email | patient@example.com |
| Verification Link | {base_url}/verify?token={jwt_token_with_24h_expiry} |
| Link Format | HTTPS with cryptographically secure token |
| Response Time | Verification page loads <2 seconds |

**Expected Results:**

- [ ] Verification email arrives within 2 minutes of registration
- [ ] Email contains single-use verification link
- [ ] Link redirects to dedicated verification page (not generic confirmation)
- [ ] Verification page displays success message
- [ ] Account status changes from "pending" to "active" in database
- [ ] Verification token marked as "used" (cannot be reused)
- [ ] User can sign-in immediately after verification
- [ ] Dashboard loads without additional verification

**Postconditions:**

- Account status = "active"
- Verification token marked used; null expiry or removed
- User session established; can access protected resources
- Email delivery and verification event logged

---

#### TC-FR-002-EC-TokenExpired: Verification Link Expiration (24 Hours)

| Field | Value |
|-------|-------|
| Requirement | FR-002, AC #6 (Link expiration after 24 hours) |
| Type | Edge Case |
| Priority | P0 |
| Preconditions | Verification link generated >24 hours ago; token stored in DB with expiry timestamp |
| Environment | QA (with time manipulation or pre-generated expired token) |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Expired verification link in email | User clicks link after 24+ hours | System validates token against current time |
| 2 | Token validation fails | System rejects token | Page displays "Verification link has expired" message |
| 3 | Expired message shown | User clicks "Resend verification email" link | New email sent with fresh token |
| 4 | New email arrives | User clicks new link (within 24h) | Verification succeeds; account activated |
| 5 | Account active | User can sign-in | Authentication succeeds |

**Test Data:**

| Field | Value |
|-------|-------|
| Original Link Expiry | 24 hours from registration |
| Expired Link Timestamp | Current_time = registration_time + 24h + 1 second |
| New Token TTL | 24 hours from resend request |
| Max Resend Attempts | 3 (prevent abuse; beyond scope but noted) |

**Expected Results:**

- [ ] Links expire exactly 24 hours from generation (not 23h 59m or 24h 1m)
- [ ] Expired link returns user-friendly message (not "Invalid token")
- [ ] "Resend" link available and functional
- [ ] New verification email generated with fresh token
- [ ] Old token cannot be reused even if new token sent
- [ ] Resend counts tracked to prevent brute force

**Postconditions:**

- New token valid for another 24 hours
- User can verify account with fresh link
- Original expired token cleaned up (optional: retention for audit)

---

#### TC-FR-002-EC-TokenReuse: Single-Use Token Enforcement

| Field | Value |
|-------|-------|
| Requirement | FR-002 (Token single-use verification) |
| Type | Edge Case |
| Priority | P1 |
| Preconditions | Valid verification token available; account verified using that token |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Token used once successfully | User attempts to click same link again | System checks token status |
| 2 | Token already used | System validates against "used" flag | Request rejected; error shown |
| 3 | Reuse attempt blocked | User sees message | Page displays "Link already used" or similar |
| 4 | User on error page | User can request new token | "Resend verification email" option available |

**Test Data:**

| Item | Value |
|---|---|
| Token Status After Use | Marked "used" in database |
| Reuse Attempt Result | HTTP 400/403 with clear error message |
| Error Message | "This verification link has already been used. [Resend link]" |

**Expected Results:**

- [ ] Token marked as used immediately after successful verification
- [ ] Reuse attempt returns error (not silent success or account state change)
- [ ] Account not modified on reuse attempt
- [ ] Audit log shows reuse attempt (possible security concern)
- [ ] User can request new token via "Resend" button

**Postconditions:**

- Token useless after single use
- Account remains in consistent state regardless of reuse attempts

---

#### TC-FR-002-ER-EmailFailure: Email Delivery Failure Recovery

| Field | Value |
|-------|-------|
| Requirement | FR-002 (Edge case: email fails to send) |
| Type | Error Case |
| Priority | P0 |
| Preconditions | Email service down or temporarily unavailable; registration submitted |
| Environment | QA with email service failure injection |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Email service down | User submits registration | System attempts to queue email |
| 2 | Queue fails | System catches email service error | User sees error message: "Email could not be sent" |
| 3 | Error displayed | User clicks "Retry sending email" or similar | Email retry triggered |
| 4 | Retry initiated | Email service recovers | Email delivers successfully |
| 5 | Email received | User clicks verification link | Account activation completes normally |

**Test Data:**

| Scenario | Trigger | Recovery Path |
|---|---|---|
| Email service timeout | Timeout after 5s | Retry button shown; manual retry works |
| SMTP connection refused | Unable to connect | Error message with support contact |
| Email validation error | Invalid recipient format | Support team contacts user (outside scope) |

**Expected Results:**

- [ ] Error message clear and actionable (not "500 Internal Server Error")
- [ ] Registration account created even if email fails (async email pattern)
- [ ] Verification email can be manually resent
- [ ] Account remains "pending" until email verified (no auto-activation)
- [ ] Support team alerted of failed email delivery (if applicable)
- [ ] Audit log shows email failure and retry attempts

**Postconditions:**

- Account created and saved (not lost due to email failure)
- User can retry email or request new link
- Email eventually delivers and user can verify

---

#### TC-FR-002-ER-BadToken: Malformed or Invalid Verification Token

| Field | Value |
|-------|-------|
| Requirement | FR-002 (Token validation) |
| Type | Error Case |
| Priority | P1 |
| Preconditions | Verification URL with invalid/malformed token |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | URL with malformed token (e.g., random string, empty, null) | User clicks bad link or manually constructs URL | System parses token |
| 2 | Token invalid | System validates token format and signature | Validation fails |
| 3 | Validation fails | User sees error page | Message: "Link is invalid or corrupted" |
| 4 | Error shown | "Request new verification email" link available | User can trigger resend |
| 5 | Resend clicked | New email sent | User receives valid link and verifies account |

**Test Data:**

| Invalid Token Scenario | URL Example | Expected Error |
|---|---|---|
| Empty token | /verify?token= | "Link is invalid" |
| Random string | /verify?token=abc123xyz | "Link is invalid or corrupted" |
| Null/missing | /verify | "Link is invalid" |
| Tampered JWT | /verify?token={modified_jwt} | "Link is invalid" (signature check fails) |

**Expected Results:**

- [ ] Malformed tokens rejected before database query
- [ ] User-friendly error message (not stack trace)
- [ ] No error details exposed (don't reveal token generation algorithm)
- [ ] Request new email option available
- [ ] No account state modified by bad token
- [ ] Security log captures invalid token attempts

**Postconditions:**

- Account in same state as before verification attempt
- User can request fresh verification email
- No security vulnerability in token validation

---

### 5.3 Functional Test Cases - Password Requirements (FR-003)

Each of the following tests validates one specific password requirement per AC #4.

#### TC-FR-003-HP: Password Meets All Requirements

| Field | Value |
|-------|-------|
| Requirement | FR-003 (Password: 8+ chars, 1 uppercase, 1 lowercase, 1 number, 1 special char) |
| Type | Happy Path |
| Priority | P0 |
| Preconditions | Registration form loaded |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Password field empty | User enters "SecurePass123!" | Real-time validation checks each requirement |
| 2 | Full password entered | Validation logic evaluates | All 5 requirement indicators turn green |
| 3 | All requirements met | String length = 13 | "✓ Password is secure" or similar confirmation |
| 4 | All requirements shown as passing | User can proceed | Form validation passes; can submit |

**Test Data:**

| Requirement | Example Password | Status |
|---|---|---|
| Min 8 characters | SecurePass123! (13 chars) | ✓ Pass |
| 1 Uppercase | SecurePass123! (S, P) | ✓ Pass |
| 1 Lowercase | SecurePass123! (ecureass) | ✓ Pass |
| 1 Number | SecurePass123! (1, 2, 3) | ✓ Pass |
| 1 Special char | SecurePass123! (!) | ✓ Pass |

**Expected Results:**

- [ ] Password accepted without error
- [ ] All 5 requirement indicators show as met/green
- [ ] Real-time validation provides immediate feedback
- [ ] Form submission succeeds
- [ ] No password complexity errors in response

**Postconditions:**

- Account created with password hash stored (never plain text)
- Bcrypt or similar algorithm used (dev-to-verify in code review)

---

#### TC-FR-003-EC-MinLength: Password Exactly 8 Characters (Boundary)

| Field | Value |
|-------|-------|
| Requirement | FR-003 (Minimum 8 characters) |
| Type | Edge Case |
| Priority | P0 |
| Preconditions | Registration form loaded |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Password field empty | User enters "Pass123!" (exactly 8 chars) | System validates; length requirement met |
| 2 | 8 characters entered | User adds all other requirements | Password accepted; no "too short" error |
| 3 | 8-char password valid | User submits form | Registration proceeds |

**Test Data:**

| Scenario | Password | Passes | Fails |
|---|---|---|---|
| Exactly 8 (lower boundary) | Pass123! | ✓ Length OK | |
| 7 characters (below boundary) | Pass123 | | ✗ "Minimum 8 characters" |
| 9 characters (above boundary) | Pass123!X | ✓ Length OK | |

**Expected Results:**

- [ ] Exactly 8 characters accepted (not rejected as "too short")
- [ ] 7 characters rejected with specific message
- [ ] 9+ characters always accepted
- [ ] Length check uses >= 8 logic (not > 8)

**Postconditions:**

- Boundary condition properly implemented
- No off-by-one errors

---

#### TC-FR-003-EC-NoUppercase: Password Missing Uppercase (Fails)

| Field | Value |
|-------|-------|
| Requirement | FR-003 (At least 1 uppercase letter) |
| Type | Edge Case |
| Priority | P0 |
| Preconditions | Registration form loaded |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Password field empty | User types "securepass123!" (all lowercase except digits/special) | System validates |
| 2 | No uppercase | Validation runs | Error message appears: "Password must contain at least 1 uppercase letter" |
| 3 | Error shown | User adds "S" at start → "SecurePass123!" | Error clears; password accepted |

**Test Data:**

| Password | Has Uppercase? | Validation Result |
|---|---|---|
| securepass123! | No | ✗ Error |
| Securepass123! | Yes (S) | ✓ OK |
| SECUREpass123! | Yes (S,E,C,U,R,E) | ✓ OK |

**Expected Results:**

- [ ] Missing uppercase triggers specific error
- [ ] Error message instructs user to "add uppercase letter"
- [ ] Adding any [A-Z] clears error
- [ ] Multiple uppercase accepted (no maximum)
- [ ] Form submission blocked until uppercase added

**Postconditions:**

- Uppercase requirement enforced
- User understands what's missing

---

#### TC-FR-003-EC-NoLowercase: Password Missing Lowercase (Fails)

| Field | Value |
|-------|-------|
| Requirement | FR-003 (At least 1 lowercase letter) |
| Type | Edge Case |
| Priority | P0 |
| Preconditions | Registration form loaded |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Password field empty | User types "SECUREPASS123!" (all uppercase except digits/special) | System validates |
| 2 | No lowercase | Validation runs | Error: "Password must contain at least 1 lowercase letter" |
| 3 | Error shown | User corrects → "SecurePass123!" | Error clears |

**Test Data:**

| Password | Has Lowercase? | Result |
|---|---|---|
| SECUREPASS123! | No | ✗ Error |
| SecurePass123! | Yes (ecureass) | ✓ OK |

**Expected Results:**

- [ ] Missing lowercase triggers specific error
- [ ] Error message clear
- [ ] Adding [a-z] clears error immediately
- [ ] Form submission enabled

**Postconditions:**

- Lowercase enforced
- User feedback immediate and actionable

---

#### TC-FR-003-EC-NoNumber: Password Missing Number (Fails)

| Field | Value |
|-------|-------|
| Requirement | FR-003 (At least 1 number) |
| Type | Edge Case |
| Priority | P0 |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Password field empty | User types "SecurePass!" (no digits) | System validates |
| 2 | No number | Validation runs | Error: "Password must contain at least 1 number" |
| 3 | Error shown | User adds "1" → "SecurePass1!" | Error clears immediately |

**Test Data:**

| Password | Has Number? | Result |
|---|---|---|
| SecurePass! | No | ✗ Error |
| SecurePass1! | Yes (1) | ✓ OK |
| SecurePass123! | Yes (1,2,3) | ✓ OK |

**Expected Results:**

- [ ] Missing number blocked with specific error
- [ ] Adding any digit [0-9] clears error
- [ ] Multiple digits always OK

---

#### TC-FR-003-EC-NoSpecialChar: Password Missing Special Character (Fails)

| Field | Value |
|-------|-------|
| Requirement | FR-003 (At least 1 special character) |
| Type | Edge Case |
| Priority | P0 |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Password field empty | User types "SecurePass123" (no special char) | System validates |
| 2 | No special char | Validation runs | Error: "Password must contain at least 1 special character" |
| 3 | Error shown | User adds "!" → "SecurePass123!" | Error clears |

**Test Data:**

| Password | Has Special Char? | Result |
|---|---|---|
| SecurePass123 | No | ✗ Error |
| SecurePass123! | Yes (!) | ✓ OK |
| SecurePass123@#$ | Yes (!,@,#,$) | ✓ OK |

**Expected Results:**

- [ ] Missing special character triggers error
- [ ] Adding [!@#$%^&*()_+-=\[\]{};:'",.<>?/] or similar allowed set clears error
- [ ] Multiple special chars always accepted

---

#### TC-FR-003-ER-TooShort: Password Less Than 8 Characters

| Field | Value |
|-------|-------|
| Requirement | FR-003 (Minimum 8 characters) |
| Type | Error Case |
| Priority | P0 |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Password field empty | User types "Pass1!" (6 chars, meets other requirements) | Validation checks length |
| 2 | Password is short | Validation fails | Error: "Password must be at least 8 characters" |
| 3 | User attempts submit | Form prevents submission | Error blocks; submit button disabled |
| 4 | User corrects length | Adds 2 chars → "Pass123!" (8 chars) | Error clears; form enabled |

**Test Data:**

| Length | Password | Result |
|---|---|---|
| 6 | Pass1! | ✗ Too short |
| 7 | Pass12! | ✗ Too short |
| 8 | Pass123! | ✓ OK (if other requirements met) |

**Expected Results:**

- [ ] Passwords <8 characters always rejected
- [ ] Clear error message about minimum length
- [ ] Submit blocked until corrected
- [ ] User can fix by adding characters

---

### 5.4 Functional Test Cases - Form Validation & State (FR-004)

#### TC-FR-004-HP: Form Data Persistence on Validation Error

| Field | Value |
|-------|-------|
| Requirement | FR-004, AC #5, #6 (Form data persists on error; no data loss) |
| Type | Happy Path |
| Priority | P1 |
| Preconditions | Registration form loaded |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Empty form | User enters email "test@example.com" | Input visible in field |
| 2 | Email entered | User enters password "abc" (invalid) | Password field shows "abc" |
| 3 | Invalid password | User clicks "Register" | Submit fails; form remains with data |
| 4 | Form rejected | Email field still shows "test@example.com" | Password field still shows "abc" |
| 5 | Data visible | User corrects password → "SecurePass123!" | Both fields retain their values; user only fixes password |
| 6 | Form corrected | User clicks "Register" again | Submission succeeds; no need to re-enter email |

**Test Data:**

| Field | Initial (Invalid) | Corrected (Valid) |
|---|---|---|
| Email | test@example.com | test@example.com (unchanged) |
| Password | abc | SecurePass123! |

**Expected Results:**

- [ ] Form data persists after validation error (AC #5 "without losing entered data")
- [ ] User doesn't need to re-enter previously correct fields
- [ ] Error message clear but doesn't focus only on one field
- [ ] User experience smooth; no frustration from data loss

**Postconditions:**

- Registration succeeds on second attempt with minimal re-entry
- User appreciates form design

---

#### TC-FR-004-EC-PartialInput: Multiple Fields Invalid, Some Valid

| Field | Value |
|-------|-------|
| Requirement | FR-004 (Highlight all fields with errors) |
| Type | Edge Case |
| Priority | P1 |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Form loaded | User enters: email "test@.com" (invalid), password empty | Validation checks both |
| 2 | Both invalid | User clicks "Register" | Both fields highlighted; multiple errors shown |
| 3 | Errors visible | User can see which fields need fixing | Email error: "Invalid email"; Password error: "Password required" |
| 4 | User corrects first field | Enters valid email | Email error disappears; Password error remains |
| 5 | Partial success | User still needs to fill password | Form shows updated state with only password error |
| 6 | All corrected | User adds password | All errors gone; submit succeeds |

**Expected Results:**

- [ ] All invalid fields highlighted simultaneously (not one-by-one)
- [ ] Each field has specific error message
- [ ] Errors clear individually as user fixes each field
- [ ] Form doesn't enable submit until all errors resolved

---

#### TC-FR-004-ER-FormReset: Reset Button Clears Form

| Field | Value |
|-------|-------|
| Requirement | FR-004 (Form has reset capability) |
| Type | Error Case |
| Priority | P2 |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Form with data | User enters "test@example.com" and password | Fields populated |
| 2 | Data in fields | User clicks "Reset" button | All fields cleared |
| 3 | Form cleared | Form returns to empty state | No errors shown; form ready for new input |

**Expected Results:**

- [ ] Reset button clears all fields
- [ ] Form state resets (no lingering validation errors)
- [ ] User can start fresh

---

#### TC-FR-004-ER-SessionTimeout: Session Expires During Registration

| Field | Value |
|-------|-------|
| Requirement | FR-004 (Session management during registration) |
| Type | Error Case |
| Priority | P2 |
| Preconditions | User with valid session; registration in progress |
| Environment | QA with session timeout simulation |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | User filling form; session active | >30 min passes (simulated) | Session token expires |
| 2 | User submits form | System checks session | Session invalid; request rejected |
| 3 | Submission fails | System redirects | User sent to re-login page or session timeout page |
| 4 | User sees message | Message explains session expired | User can sign in again and restart registration |

**Expected Results:**

- [ ] Session timeout handled gracefully (not cryptic error)
- [ ] User doesn't lose form data before timeout (form submitted before impact)
- [ ] Clear message about session expiration
- [ ] Redirect to login to re-establish session

---

### 5.5 Non-Functional Test Cases

#### TC-NFR-001-PERF: Registration Submit Response Time

| Field | Value |
|-------|-------|
| Requirement | NFR-001 (Performance: registration response <2s) |
| Type | Performance |
| Priority | P0 |
| Environment | QA with baseline load |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | User with valid credentials; system at baseline | User submits registration | Timer starts at submit click |
| 2 | Request in flight | Server processes (create account, generate token, queue email) | Timer stops when response received |
| 3 | Response received | Response time measured | P95 response time < 2000ms |
| 4 | 10+ successful registrations | Measure P95 | Consistently < 2 seconds |

**Acceptance Criteria:**

- [ ] Response time P95 < 2000ms (milliseconds)
- [ ] No registration takes >5 seconds (hard limit)
- [ ] Database query optimization verified (no N+1 queries)
- [ ] Email queue add doesn't block response
- [ ] Response returned before email service call completes

**Measurement Tool:** Browser DevTools (Playwright recorder), APM monitoring

---

#### TC-NFR-002-EML: Email Delivery SLA (2 Minutes)

| Field | Value |
|-------|-------|
| Requirement | NFR-002 (Email delivered <120 seconds) |
| Type | Performance |
| Priority | P0 |
| Environment | QA with real or staging email service |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Registration submitted | Email queued | Timestamp recorded in database |
| 2 | Email service processes | Email sent to SMTP relay | Sent timestamp recorded |
| 3 | Email in transit | Email arrives in recipient inbox | Received timestamp recorded |
| 4 | Measure end-to-end | Calculate received_time - queued_time | Time < 120 seconds |
| 5 | Repeat 5 times | Run 5 separate registrations | All complete <120 seconds |

**Acceptance Criteria:**

- [ ] All 5 emails arrive within 120 seconds
- [ ] P95 delivery time < 120 seconds
- [ ] Email subject and from address correct
- [ ] Verification link present and functional

---

#### TC-NFR-003-SEC: Password Encryption & No Logging

| Field | Value |
|-------|-------|
| Requirement | NFR-003 (Security: passwords encrypted; never logged in plaintext) |
| Type | Security |
| Priority | P0 |
| Environment | Code review + integration test |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | User submits password "SecurePass123!" | System receives in registration request | Request body contains plain password (HTTPS only) |
| 2 | Password received | Backend hashes password with Bcrypt (or similar) | Hash stored in database; plaintext discarded |
| 3 | Database checked | Query SELECT password FROM users | Stored value is bcrypt hash, not plaintext |
| 4 | Logs reviewed | Application logs examined | Password never appears in logs (even at DEBUG level) |
| 5 | Audit trail | Check audit log for registration | No plaintext password in audit entry |

**Acceptance Criteria:**

- [ ] Password stored as bcrypt hash (cost factor ≥10)
- [ ] Password never logged (even at DEBUG log level)
- [ ] Password not transmitted in response body
- [ ] HTTPS enforced (no HTTP fallback)
- [ ] Code review confirms hashing library used correctly

**Tools:** Code inspection, database query, log file grep

---

#### TC-NFR-004-SEC: Verification Token Cryptographic Security

| Field | Value |
|-------|-------|
| Requirement | NFR-004 (Token security: cryptographically random, unique, signed) |
| Type | Security |
| Priority | P0 |

**Test Steps:**

| Step | Given | When | Then |
|------|-------|------|------|
| 1 | 10 registrations | Each generates verification token | 10 different tokens generated |
| 2 | Tokens generated | Examine randomness and format | Tokens use cryptographically secure random (not Math.random()) |
| 3 | Token format | Tokens are JWT or similar signed format | Token includes signature preventing tampering |
| 4 | Token tampering | Modify token in URL (e.g., change 1 char) | Modified token rejected; signature validation fails |
| 5 | Token reuse | Use same token twice | Second use fails; token marked consumed |

**Acceptance Criteria:**

- [ ] Tokens generated from crypto/random (not pseudo-random)
- [ ] Each token unique (no collisions in 10 registrations)
- [ ] Token signed with private key (JWT with HS256 or RS256)
- [ ] Token verification prevents tampering
- [ ] Token includes expiration claim (24 hours)

**Tools:** Code inspection, unit tests for token generation

---

### 5.6 E2E Journey Test Cases

#### E2E-UC-001-HP: Complete Registration & Email Verification Happy Path

| Field | Value |
|-------|-------|
| UC Chain | UC-001: Patient Registration → UC-001b: Email Verification → Sign-In |
| Session | Guest (no auth required for registration); Authenticated (after verification) |
| Priority | P0 |
| Preconditions | Landing page accessible; email service running |
| Environment | QA |

**Journey Overview:**

This end-to-end test validates the complete registration flow from initial form input through email verification to first sign-in.

**Phase 1: Registration Form Submission**

| Step | Given | When | Then |
|------|-------|------|------|
| 1.1 | Landing page loaded | User clicks "Sign Up" button | Registration form displayed |
| 1.2 | Form visible | User enters email "newpatient@example.com" | Email field accepts input |
| 1.3 | Email entered | User enters password "SecurePass123!" | Password field accepts input; inline validation passes all 5 requirements |
| 1.4 | Credentials valid | User clicks "Register" button | Form submits; loading indicator shown |
| 1.5 | Submission in progress | Server processes request | Response received in <2 seconds |
| 1.6 | Request complete | Form displays success message | Message: "Verification email sent to newpatient@example.com" |

**Phase 2: Email Reception & Link Validation**

| Step | Given | When | Then |
|------|-------|------|------|
| 2.1 | Success message shown | User checks email inbox (after ~30 seconds) | Verification email arrives with subject "Verify Your Account" |
| 2.2 | Email received | User opens email | Sender: "noreply@patientportal.com"; message body clear and professional |
| 2.3 | Email content reviewed | User clicks "Verify Email" button/link | Link contains valid JWT token with 24h expiry |
| 2.4 | Link clicked | Browser navigates to verification URL | Verification endpoint processes token |
| 2.5 | Token validated | System checks token signature and expiry | Token valid; account status updated to "active" |
| 2.6 | Account activated | Page displays success message | Page shows "Email verified successfully! Redirecting to sign-in..." |

**Phase 3: Sign-In After Verification**

| Step | Given | When | Then |
|------|-------|------|------|
| 3.1 | Success page displayed | Page redirects after 2-3 seconds | User lands on sign-in page |
| 3.2 | Sign-in form loaded | User enters email "newpatient@example.com" | Email field accepts input |
| 3.3 | Email entered | User enters password "SecurePass123!" | Password field accepts input |
| 3.4 | Credentials ready | User clicks "Sign In" button | Credentials validated against database |
| 3.5 | Authentication succeeds | Session token created | User authenticated; cookie/token set |
| 3.6 | Sign-in complete | Browser redirects | Dashboard loads; patient portal accessible |
| 3.7 | Dashboard visible | User can see appointment booking, health records | Journey complete; account fully functional |

**Test Data:**

| Entity | Value |
|---|---|
| Test User Email | newpatient@example.com |
| Test User Password | SecurePass123! |
| Verification Link Format | https://staging.patientportal.com/verify?token=eyJhbGc... |
| Expected Timeline | Registration: <2s, Email delivery: <2min, Full journey: <5min |

**Expected Results:**

- [ ] All 7 substeps (1.1-1.6, 2.1-2.6, 3.1-3.7) complete without error
- [ ] No user interaction required except initial registration and email click
- [ ] Account status transitions: "pending" → "active"
- [ ] Verification token consumed after use (cannot be reused)
- [ ] User successfully signs in with created credentials
- [ ] Dashboard loads without additional verification step

**Postconditions:**

- Patient account fully active in system
- User can book appointments, view health records, etc.
- Session established; user remains authenticated

---

#### E2E-UC-001-LinkExpiry: Email Verification Link Expiration Recovery

| Field | Value |
|-------|-------|
| UC Chain | UC-001 → Link Expires → Resend Email → UC-001b (Verify) |
| Session | Guest → Authenticated |
| Priority | P0 |
| Preconditions | Account registered; verification email sent; >24 hours passes |
| Environment | QA (time manipulation or pre-generated expired token) |

**Phase 1: Initial Registration (Standard)**

| Step | Given | When | Then |
|------|-------|------|------|
| 1.1 | Landing page | User registers with "patient2@example.com" and valid password | Account created; verification email sent; time recorded |
| 1.2 | Email queued | System stamps token with expiry: now + 24 hours | Token TTL = 24 hours |

**Phase 2: Link Expires (Time Advances)**

| Step | Given | When | Then |
|------|-------|------|------|
| 2.1 | 24+ hours pass (simulated via test) | Current time = registration_time + 24h + 1 second | System time advanced using test utilities |
| 2.2 | User finds old email | User clicks original verification link from email | Request sent to verification endpoint |
| 2.3 | Token validation | System checks: token_creation + 24h > now | Validation fails; token expired |
| 2.4 | Expiration detected | System returns error response | Page displays: "Verification link has expired" |

**Phase 3: Request New Verification Email (Recovery)**

| Step | Given | When | Then |
|------|-------|------|------|
| 3.1 | Error page shown | User clicks "Resend verification email" link | System generates new token |
| 3.2 | New token created | Token generation: new token TTL = now + 24 hours | Fresh token stored; old token remains (for audit) |
| 3.3 | New email sent | Email service queues new verification email | Email contains new verification link with new token |
| 3.4 | User receives new email | User checks inbox; new email arrives | Subject same "Verify Your Account"; link updated |

**Phase 4: Verify Account with Fresh Token**

| Step | Given | When | Then |
|------|-------|------|------|
| 4.1 | New email received | User clicks fresh verification link | New token validated |
| 4.2 | Token valid | System checks: token_creation + 24h > now | Token within 24h window; signature valid |
| 4.3 | Verification succeeds | Account activated; token marked used | Status = "active" |
| 4.4 | Success shown | Redirect to sign-in | User can now sign in |
| 4.5 | Sign-in succeeds | User enters email and password | Authentication succeeds; dashboard loads |

**Test Data:**

| Item | Value |
|---|---|
| Initial Registration Time | 2026-03-20 10:00:00 UTC |
| Verification Link Clicked | 2026-03-21 10:00:01 UTC (expired by 1 second) |
| New Verification Sent | 2026-03-21 10:00:05 UTC |
| New Link Clicked | 2026-03-21 10:00:10 UTC (within 24h of new token) |

**Expected Results:**

- [ ] Expired link rejected with clear user message
- [ ] Resend mechanism works without re-submitting registration
- [ ] New token valid for another 24 hours from generation
- [ ] Old expired token cannot be reused (even after resend)
- [ ] Account successfully activated after verification with fresh token
- [ ] User can sign in immediately after successful verification

**Postconditions:**

- Account status = "active"
- Both tokens (old and new) logged for audit (old marked expired; new marked used)
- No duplicate account created

---

#### E2E-UC-001-DuplicateEmail: Duplicate Email Recovery Flow

| Field | Value |
|-------|-------|
| UC Chain | UC-001 (Duplicate Email) → Sign In → Account Access |
| Session | Guest → Existing User Authentication |
| Priority | P1 |
| Preconditions | Account with "patient@example.com" already registered and active |

**Phase 1: Duplicate Email Detection**

| Step | Given | When | Then |
|------|-------|------|------|
| 1.1 | Landing page; "patient@example.com" already registered | New user attempts registration with same email | Email field contains "patient@example.com" |
| 1.2 | Duplicate email entered | User clicks "Register" or system validates on blur | Duplicate detection triggered |
| 1.3 | Duplicate found | System checks unique constraint | Error message displays: "This email is already registered" |
| 1.4 | Error shown | Message includes helpful recovery actions | Links shown: "Sign In" and "Reset Password" |

**Phase 2: Sign In (Recovery Path)**

| Step | Given | When | Then |
|------|-------|------|------|
| 2.1 | Error message visible | User clicks "Sign In" link in error message | Redirected to sign-in page |
| 2.2 | Sign-in form loaded | User enters email "patient@example.com" | Email field accepts input |
| 2.3 | Email entered | User enters correct password for existing account | Password field accepts input |
| 2.4 | Credentials ready | User clicks "Sign In" button | Credentials validated |
| 2.5 | Authentication succeeds | Session established | User authenticated; cookie/token set |
| 2.6 | Dashboard loads | User can access patient portal | Account successfully accessed |

**Test Data:**

| Entity | Value |
|---|---|
| Existing Account Email | patient@example.com |
| Existing Account Password | ExistingPass456! |
| Duplicate Email Attempt | patient@example.com |
| Error Message | "This email is already registered" (exact) |
| Recovery Links | "Sign In" href="/signin", "Reset Password" href="/reset" |

**Expected Results:**

- [ ] Duplicate email detected before account creation
- [ ] Error message clear and helpful (not "Email already exists silently failing")
- [ ] Recovery links present and functional
- [ ] User can sign in with existing account using provided link
- [ ] No new account created
- [ ] Existing account accessible with correct password

**Postconditions:**

- No duplicate account created
- User signed into existing account
- Access to all patient portal features

---

## 6. Traceability Matrix

### Requirement → Test Case Mapping

| Requirement ID | Requirement Description | Test Cases | Coverage |
|---|---|---|---|
| FR-001 | Account creation with email & password | TC-FR-001-HP, TC-FR-001-EC-DupEmail, TC-FR-001-EC-InvalidEmail, TC-FR-001-ER-EmptyFields, TC-FR-001-ER-ServiceUnavailable, E2E-UC-001-HP, E2E-UC-001-LinkExpiry, E2E-UC-001-DuplicateEmail | 8 tests |
| FR-002 | Email verification with link validation | TC-FR-002-HP, TC-FR-002-EC-TokenExpired, TC-FR-002-EC-TokenReuse, TC-FR-002-ER-EmailFailure, TC-FR-002-ER-BadToken, E2E-UC-001-HP, E2E-UC-001-LinkExpiry | 7 tests |
| FR-003 | Password requirements (8 chars, uppercase, lowercase, number, special) | TC-FR-003-HP, TC-FR-003-EC-MinLength, TC-FR-003-EC-NoUppercase, TC-FR-003-EC-NoLowercase, TC-FR-003-EC-NoNumber, TC-FR-003-EC-NoSpecialChar, TC-FR-003-ER-TooShort | 7 tests |
| FR-004 | Form validation & data persistence | TC-FR-004-HP, TC-FR-004-EC-PartialInput, TC-FR-004-ER-FormReset, TC-FR-004-ER-SessionTimeout | 4 tests |
| NFR-001 | Performance: registration <2 seconds | TC-NFR-001-PERF | 1 test |
| NFR-002 | Performance: email delivery <2 minutes | TC-NFR-002-EML | 1 test |
| NFR-003 | Security: password encryption & no logging | TC-NFR-003-SEC | 1 test |
| NFR-004 | Security: token cryptographic strength | TC-NFR-004-SEC | 1 test |
| UXR-002 | Visual distinction for patient portal | E2E-UC-001-HP, E2E-UC-001-DuplicateEmail (UI validation) | 2 tests |
| UXR-504 | Inline validation feedback | TC-FR-003-HP (password validation), TC-FR-004-HP (form state) | 2 tests |
| UXR-601 | User-friendly error messages with recovery | TC-FR-001-ER-*, TC-FR-002-ER-*, E2E-UC-001-LinkExpiry, E2E-UC-001-DuplicateEmail | 5 tests |
| UXR-603 | Form data persistence on validation errors | TC-FR-004-HP, TC-FR-004-EC-PartialInput | 2 tests |

### Test Case → Acceptance Criteria Mapping

| AC # | Acceptance Criteria | Test Case(s) |
|---|---|---|
| AC1 | Sign Up → Enter valid email & secure password | TC-FR-001-HP, E2E-UC-001-HP |
| AC2 | Verification email sent within 2 minutes | TC-NFR-002-EML, E2E-UC-001-HP |
| AC3 | Link valid for 24h; displays "Link expired" after | TC-FR-002-EC-TokenExpired, E2E-UC-001-LinkExpiry |
| AC4 | Duplicate email error: "This email is already registered" | TC-FR-001-EC-DupEmail, E2E-UC-001-DuplicateEmail |
| AC5 | Password validation errors shown inline; form doesn't reset | TC-FR-003-EC-*, TC-FR-004-HP |
| AC6 | Required fields highlighted; errors shown without data loss | TC-FR-001-ER-EmptyFields, TC-FR-004-HP |
| AC7 | Link expiration offers "Resend" option | TC-FR-002-EC-TokenExpired, E2E-UC-001-LinkExpiry |

### Edge Cases Coverage

| Edge Case | Test Case |
|---|---|
| Malformed email address | TC-FR-001-EC-InvalidEmail |
| Temporary/disposable email domains | Deferred to future phase |
| Email delivery failure | TC-FR-002-ER-EmailFailure |
| Concurrent registration attempts with same email | TC-FR-001-EC-DupEmail |
| Token reuse after successful verification | TC-FR-002-EC-TokenReuse |
| Token tampering/modification | TC-NFR-004-SEC |
| Session timeout during registration | TC-FR-004-ER-SessionTimeout |

---

## 7. Test Execution Strategy

### Environment Configuration

| Aspect | DEV | QA | Staging |
|---|---|---|---|
| Email Service | Mock/Captured | Mock with capture | Real service (staging account) |
| Database | Local SQLite or in-memory | Test snapshot with cleanup | Staging DB (isolated from prod) |
| Password Requirements | Enforced client & server | Enforced client & server | Enforced client & server |
| HTTPS | Optional | Required | Required |
| Load | Single user | 5-10 concurrent users for perf tests | <100 concurrent users (shared staging) |
| Test Data Cleanup | Per test | Per test suite | Daily reset |

### Test Execution Sequence

1. **Unit Tests** (Dev machine or CI) - Password validators, email regex, token generation
   - Runtime: <5 minutes
   - Execution: On every commit (pre-push hook)

2. **Integration Tests** (QA environment) - Email service, database, API contracts
   - Runtime: ~15 minutes
   - Execution: On PR merge to develop branch

3. **E2E Tests** (QA/Staging) - Full user journeys
   - Runtime: ~5-10 minutes per journey
   - Execution: Post-deployment to QA; before release to staging

4. **Performance Tests** (Staging) - Response time SLA, email delivery SLA
   - Runtime: ~10 minutes
   - Execution: Pre-release; weekly regression

5. **Security Tests** (Code review + Integration) - Password encryption, token security
   - Runtime: <5 minutes (automated checks)
   - Execution: On PR merge; pre-release

### Roles & Responsibilities

- **Test Engineer (QA)**: Execute manual & automated E2E tests; report defects
- **Developer**: Fix bugs discovered in testing; implement password & token validation
- **DevOps**: Manage test environments; configure email service mocking
- **Security**: Review password encryption implementation; audit token generation

---

## 8. Success Criteria

- [ ] All 33 test cases defined and documented
- [ ] All Functional Requirements (FR-001 through FR-004) covered by ≥2 test cases each
- [ ] All Acceptance Criteria (AC 1-7) mappedto test cases
- [ ] All high-risk scenarios (P0) covered with automated E2E tests
- [ ] Performance thresholds in NFR tests verified (e.g., <2s registration, <2min email)
- [ ] Security requirements validated (password encryption, token security)
- [ ] Traceability matrix 100% complete (no orphaned tests or requirements)
- [ ] Test data strategy defined for all environments
- [ ] E2E test execution completes in <20 minutes
- [ ] Defect escape rate target: <2% (defects escaping to production)

---

## 9. Appendix

### A. Test Data Specifications

#### Valid Registration Scenarios

```yaml
valid_credentials:
  - email: "john.doe@example.com"
    password: "SecurePass123!"
    first_name: "John"
    last_name: "Doe"
  - email: "patient@hospital.org"
    password: "MyStr@ng99Pass"
    first_name: "Jane"
    last_name: "Smith"
  - email: "user+tag@subdomain.example.com"
    password: "P@ssw0rdRequir123"
    first_name: "Alex"
    last_name: "Johnson"

password_boundary_cases:
  valid:
    - "Pass123!" # Exactly 8 chars, all requirements
    - "VeryLongPassword123!@#$" # Multiple requirements met multiple times
  invalid:
    - "Pass123" # Missing special character
    - "pass123!" # Missing uppercase
    - "PASS123!" # Missing lowercase
    - "Pass!" # Missing number
    - "Pass123" # Missing special character
    - "Pas1!" # 6 chars, too short
```

#### Email Verification Scenarios

```yaml
email_scenarios:
  success:
    - token_expires_in_hours: 24
      clicks_within: 1
      result: "account_activated"
  failure:
    - token_expires_in_hours: 24
      clicks_within: 24.01
      result: "link_expired_error"
    - token_tampered: true
      clicks_within: 0.5
      result: "invalid_token_error"
    - token_reused: true
      result: "already_used_error"
```

### B. Expected Error Messages

| Scenario | Expected Message |
|---|---|
| Invalid email format | "Please enter a valid email address" |
| Duplicate email | "This email is already registered" |
| Password too short | "Password must be at least 8 characters" |
| Missing uppercase | "Password must contain at least 1 uppercase letter" |
| Missing lowercase | "Password must contain at least 1 lowercase letter" |
| Missing number | "Password must contain at least 1 number" |
| Missing special character | "Password must contain at least 1 special character" |
| Email delivery failed | "Email could not be sent. Please try again or contact support." |
| Link expired | "Verification link has expired. [Resend email]" |
| Link already used | "This verification link has already been used. [Request new link]" |
| Invalid token | "Link is invalid or corrupted. [Request new link]" |
| Missing field | "[Field name] is required" |

### C. Related Documentation

- System Architecture: `.propel/context/docs/design.md`
- API Specification: `.propel/context/docs/spec.md`
- Email Service Integration: [Team Wiki - Email Service]
- Security Standards: `.propel/rules/security-standards-owasp.md`
- Playwright Testing Guide: `.propel/rules/playwright-testing-guide.md`

---

**End of Test Plan**

**Document Version History:**
- v1.0 (2026-03-20): Initial comprehensive test plan for US-001
