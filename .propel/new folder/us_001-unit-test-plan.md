# Unit Test Plan - US_001: Patient Account Registration with Email Validation

## Test Plan Metadata

| Attribute | Value |
|-----------|-------|
| **Story ID** | US_001 |
| **Story Title** | Patient Account Registration with Email Validation |
| **Plan Version** | 1.0 |
| **Created Date** | 2026-03-17 |
| **Component Under Test** | Patient Registration Service |
| **Test Coverage Target** | 95%+ branch coverage |

---

## Test Objectives

- Validate account creation functionality with email and password
- Verify email validation logic for valid/invalid formats
- Ensure password security requirements are enforced
- Confirm email verification workflows and link expiry
- Validate error handling and user feedback mechanisms
- Test duplicate email detection
- Verify form data persistence on validation failures

---

## Test Scope

### In Scope
- Account registration service logic
- Email validation and format checking
- Password strength validation
- Email verification workflow (creation, expiry, resend)
- Duplicate email detection
- Form field validation and error messaging
- Email delivery retries and failure handling

### Out of Scope
- Email provider integration (mock only)
- Frontend UI component rendering (separate E2E/integration tests)
- Authentication token generation (separate security tests)
- Database persistence layer (integration tests)

---

## Test Suite Organization

### 1. Account Registration Tests

#### 1.1 Successful Registration Flow
| Test ID | Test Case | Input | Expected Output | Acceptance Criteria |
|---------|-----------|-------|-----------------|-------------------|
| REG-001 | Valid account creation | `email="user@example.com"`, `password="SecurePass123!"` | Account created, verification email queued | AC-1 |
| REG-002 | Verification email sent within SLA | Account created | Email sent, timestamp logged within 2 minutes | AC-1 |
| REG-003 | Account persists to repository | Valid registration data | Account object stored with id, email, hashed_password | AC-1 |

#### 1.2 Password Validation Tests
| Test ID | Test Case | Input | Expected Behavior | Requirement |
|---------|-----------|-------|-------------------|-------------|
| PWD-001 | Password < 8 characters | `password="Short1!"` | Validation fails, error: "minimum 8 characters" | FR-001 |
| PWD-002 | Password missing uppercase | `password="securepass123!"` | Validation fails, error: "1 uppercase letter" | FR-001 |
| PWD-003 | Password missing lowercase | `password="SECUREPASS123!"` | Validation fails, error: "1 lowercase letter" | FR-001 |
| PWD-004 | Password missing number | `password="SecurePass!"` | Validation fails, error: "1 number" | FR-001 |
| PWD-005 | Password missing special char | `password="SecurePass123"` | Validation fails, error: "1 special character" | FR-001 |
| PWD-006 | Valid complex password | `password="SecurePass123!"` | Validation passes | FR-001 |
| PWD-007 | Password exactly 8 chars (minimum) | `password="Pass123!"` | Validation passes | FR-001 |

#### 1.3 Email Validation Tests
| Test ID | Test Case | Input | Expected Behavior | Requirement |
|---------|-----------|-------|-------------------|-------------|
| EMAIL-001 | Valid standard email | `email="patient@hospital.com"` | Validation passes | FR-001 |
| EMAIL-002 | Email with subdomain | `email="user@clinic.hospital.com"` | Validation passes | FR-001 |
| EMAIL-003 | Email with plus addressing | `email="patient+test@example.com"` | Validation passes | FR-001 |
| EMAIL-004 | Missing @ symbol | `email="userexample.com"` | Validation fails, error: "valid email address" | UXR-601 |
| EMAIL-005 | Missing domain | `email="user@"` | Validation fails, error: "valid email address" | UXR-601 |
| EMAIL-006 | Missing local part | `email="@example.com"` | Validation fails, error: "valid email address" | UXR-601 |
| EMAIL-007 | Multiple @ symbols | `email="user@@example.com"` | Validation fails, error: "valid email address" | UXR-601 |
| EMAIL-008 | Spaces in email | `email="user @example.com"` | Validation fails, error: "valid email address" | UXR-601 |
| EMAIL-009 | Empty email field | `email=""` | Validation fails, error: "required field" | UXR-601 |

---

### 2. Duplicate Email Detection Tests

#### 2.1 Existing Email Conflict
| Test ID | Test Case | Setup | Input | Expected Output | Acceptance Criteria |
|---------|-----------|-------|-------|-----------------|-------------------|
| DUP-001 | Register with duplicate email | Email already in system | `email="existing@example.com"` | Error: "This email is already registered" | AC-3 |
| DUP-002 | Case-insensitive duplicate check | Email: "User@Example.com" exists | `email="user@example.com"` | Error: "already registered" | AC-3 |
| DUP-003 | Whitespace-trimmed duplicate check | Email: "patient@example.com " exists | `email=" patient@example.com"` | Error: "already registered" | AC-3 |
| DUP-004 | Sign-in link offered on duplicate | Duplicate email attempt | N/A | Error message includes link to sign-in | AC-3 |
| DUP-005 | Password reset link offered on duplicate | Duplicate email attempt | N/A | Error message includes "reset password" link | AC-3 |

---

### 3. Form Validation & Error Handling Tests

#### 3.1 Required Field Validation
| Test ID | Test Case | Input | Expected Behavior | Acceptance Criteria |
|---------|-----------|-------|-------------------|-------------------|
| FORM-001 | Empty email field | `email=""`, `password="SecurePass123!"` | Submission blocked, error highlighted | AC-5 |
| FORM-002 | Empty password field | `email="user@example.com"`, `password=""` | Submission blocked, error highlighted | AC-5 |
| FORM-003 | Both fields empty | `email=""`, `password=""` | Submission blocked, all errors highlighted | AC-5 |
| FORM-004 | Null email value | `email=null`, `password="SecurePass123!"` | Submission blocked, error message shown | AC-5 |

#### 3.2 Form Data Persistence
| Test ID | Test Case | Action | Expected Outcome | Acceptance Criteria |
|---------|-----------|--------|-------------------|-------------------|
| FORM-005 | Data retained on validation error | Enter email/password, submit with invalid password | Email field retains user input | AC-5 |
| FORM-006 | Password field cleared on error | Enter credentials, validation fails | Password field is cleared (security best practice) | AC-5 |
| FORM-007 | Multiple validation errors shown | Invalid email + weak password | All errors listed, form not cleared | AC-5 |

---

### 4. Email Verification Workflow Tests

#### 4.1 Verification Link Generation & Validity
| Test ID | Test Case | Scenario | Expected Behavior | Acceptance Criteria |
|---------|-----------|----------|-------------------|-------------------|
| VERIFY-001 | Verification link created | Account registered | Link generated and queued for email send | AC-2 |
| VERIFY-002 | Link valid within 24 hours | Link clicked within 23 hours | Account activated, redirect to sign-in | AC-2 |
| VERIFY-003 | Link expires after 24 hours | Link clicked at 25+ hours | Error: "Link expired" message shown | AC-6 |
| VERIFY-004 | Link format validation | Valid UUID token | Link can be parsed and matched to account | AC-2 |
| VERIFY-005 | One-time link consumption | Link clicked once | Link marked as consumed, cannot reuse | AC-2 |
| VERIFY-006 | Prevent reuse of expired link | Attempt click after expiry | Error: "Link expired", resend option offered | AC-6 |

#### 4.2 Account Activation
| Test ID | Test Case | Condition | Expected Result | Acceptance Criteria |
|---------|-----------|-----------|-----------------|-------------------|
| ACTIVATE-001 | Account marked active | Valid link clicked | Account status = ACTIVE | AC-2 |
| ACTIVATE-002 | User redirected post-activation | Link validated | Redirect to sign-in page with success message | AC-2 |
| ACTIVATE-003 | Inactive account cannot sign in | Before email verified | Sign-in fails, prompt to verify email | AC-2 |

#### 4.3 Link Resend Functionality
| Test ID | Test Case | Scenario | Expected Output | Acceptance Criteria |
|---------|-----------|----------|-----------------|-------------------|
| RESEND-001 | Resend link option offered | Expired link page | Button/link to "Resend verification email" | AC-6 |
| RESEND-002 | New link generated on resend | Click resend | New link created with fresh 24-hour expiry | AC-6 |
| RESEND-003 | Resend rate limiting | Click resend multiple times | Max 3 resend attempts per 1 hour | UXR-601 |
| RESEND-004 | Email resent on resend | Resend requested | New verification email queued | AC-6 |

---

### 5. Email Delivery & Error Handling Tests

#### 5.1 Email Service Integration
| Test ID | Test Case | Condition | Expected Behavior | Edge Case Reference |
|---------|-----------|-----------|-------------------|-------------------|
| EMAIL-SVC-001 | Email sent successfully | Valid registration | Email provider receives request, 200 response | E-001 |
| EMAIL-SVC-002 | Email delivery timeout | Service unavailable | Retry queue populated, error logged | E-003 |
| EMAIL-SVC-003 | Email delivery failure | Invalid SMTP config | System displays error with retry option | E-003 |
| EMAIL-SVC-004 | Retry mechanism works | First attempt failed | Second retry succeeds, email sent | E-003 |
| EMAIL-SVC-005 | Max retries exceeded | Multiple failures | Failure logged for support, user notified | E-003 |

#### 5.2 Disposable Email Handling
| Test ID | Test Case | Input | Expected Behavior | Edge Case Reference |
|---------|-----------|-------|-------------------|-------------------|
| DISPOSABLE-001 | Accept temp email for now | `email="user@tempmail.com"` | Account created (future phase may flag) | E-002 |
| DISPOSABLE-002 | Log suspicious registrations | Temp email domain detected | Entry logged for manual review | E-002 |

---

### 6. Concurrent & Error State Tests

#### 6.1 Race Condition & Concurrency
| Test ID | Test Case | Condition | Expected Outcome | Edge Case Reference |
|---------|-----------|-----------|-----------------|-------------------|
| RACE-001 | Simultaneous registrations same email | Two requests within 100ms | First wins, second gets "already registered" | E-004 |
| RACE-002 | Transaction rollback on error | Invalid data after duplicate check | Transaction rolled back, email not locked | E-004 |

#### 6.2 Recovery & Cleanup
| Test ID | Test Case | Scenario | Expected Behavior | Notes |
|---------|-----------|----------|-------------------|-------|
| RECOVERY-001 | Incomplete registration cleanup | Unverified after 30 days | Account auto-deleted or marked for cleanup | Data governance |
| RECOVERY-002 | Expired token cleanup | Token older than 30 days | Token removed from system | Storage optimization |

---

## Test Data Requirements

### Valid Test Data Sets

```json
{
  "validRegistrations": [
    {
      "email": "john.doe@example.com",
      "password": "SecurePass123!"
    },
    {
      "email": "patient+test123@hospital.org",
      "password": "MyP@ssw0rd"
    }
  ],
  "invalidPasswords": [
    { "password": "short1!", "reason": "< 8 chars" },
    { "password": "nouppercase123!", "reason": "no uppercase" },
    { "password": "NOLOWERCASE123!", "reason": "no lowercase" },
    { "password": "NoNumbers!", "reason": "no digit" },
    { "password": "NoSpecialChar123", "reason": "no special char" }
  ],
  "invalidEmails": [
    "userexample.com",
    "user@",
    "@example.com",
    "user@@example.com",
    "user @example.com"
  ],
  "disposableEmailDomains": [
    "tempmail.com",
    "throwaway.email",
    "10minutemail.com"
  ]
}
```

---

## Test Execution Strategy

### Phase 1: Unit Tests (Isolated)
- Password validation logic
- Email format validation
- Duplicate detection
- Link expiry calculations
- Error message generation

### Phase 2: Integration Tests
- Account creation with database
- Email queue population
- Verification link storage
- Transaction management

### Phase 3: E2E Tests
- Complete registration workflow
- Email verification workflow
- Error recovery flows

---

## Acceptance Criteria Coverage

| AC # | Test IDs | Status | Coverage |
|------|----------|--------|----------|
| AC-1 | REG-001, REG-002, REG-003, EMAIL-SVC-001 | Planned | ✓ |
| AC-2 | VERIFY-001, VERIFY-002, ACTIVATE-001, ACTIVATE-002 | Planned | ✓ |
| AC-3 | DUP-001, DUP-002, DUP-003, DUP-004, DUP-005 | Planned | ✓ |
| AC-4 | PWD-001 through PWD-007 | Planned | ✓ |
| AC-5 | FORM-001 through FORM-007 | Planned | ✓ |
| AC-6 | VERIFY-003, VERIFY-006, RESEND-001 through RESEND-004 | Planned | ✓ |

---

## Dependencies & Assumptions

### Dependencies
- Authentication service available (for token generation)
- Email service mocked/available in test environment
- Database layer implemented and testable
- Password hashing algorithm selected

### Assumptions
- Email service has standard retry behavior
- Database supports transactions
- UI form validation will be tested separately (E2E)
- Rate limiting implemented at service level

---

## Risk & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Email delivery failures | Medium | High | Mock email service, test retry logic thoroughly |
| Race conditions on email uniqueness | Low | High | Database unique constraint + application logic test |
| Password hashing performance | Low | Medium | Benchmark hashing before DB integration |
| Expired link cleanup | Low | Medium | Scheduled job tests in integration phase |

---

## Success Criteria

- ✓ All test cases executed
- ✓ 95%+ code coverage
- ✓ All acceptance criteria verified
- ✓ Edge cases handled gracefully
- ✓ Error messages user-friendly
- ✓ No security vulnerabilities (SQL injection, XSS, etc.)
