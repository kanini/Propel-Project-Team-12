# Unit Test Plan - US_002: Multi-Role User Login Authentication

## Test Plan Metadata

| Attribute | Value |
|-----------|-------|
| **Story ID** | US_002 |
| **Story Title** | Multi-Role User Login Authentication |
| **Plan Version** | 1.0 |
| **Created Date** | 2026-03-17 |
| **Component Under Test** | Authentication Service & Role-Based Authorization |
| **Test Coverage Target** | 95%+ branch coverage |22

---

## Test Objectives

- Validate user authentication with email and password credentials
- Verify role-specific portal redirection (Patient, Staff, Admin)
- Ensure invalid credentials are handled securely without revealing which field failed
- Confirm unverified accounts are blocked from login
- Validate JWT token generation with role claims
- Test role-based access control enforcement
- Verify multi-role handling and primary role logic
- Test session management across multiple devices

---

## Test Scope

### In Scope
- User authentication service logic
- Email and password credential validation
- Role-specific dashboard routing
- JWT token generation with role claims
- Unverified account detection
- Invalid credential handling (secure error messages)
- Case-insensitive email matching
- Deactivated account detection
- Multi-device session management
- Session timeout and cleanup

### Out of Scope
- Frontend UI rendering (separate E2E/integration tests)
- Password reset flow (separate story)
- Role permission enforcement at feature level (separate authorization tests)
- Single sign-on / OAuth integration (future phase)
- Session persistence layer (integration tests)

---

## Test Suite Organization

### 1. Successful Login Flow Tests

#### 1.1 Patient Login
| Test ID | Test Case | Input | Expected Output | Acceptance Criteria |
|---------|-----------|-------|-----------------|-------------------|
| LOGIN-001 | Valid patient credentials | `email="patient@example.com"`, `password="SecurePass123!"` | Redirected to Patient Dashboard within 2 seconds | AC-1, AC-2 |
| LOGIN-002 | Patient portal displays correct UI | Patient logs in successfully | Patient-specific nav, patient color accents visible | AC-2 |
| LOGIN-003 | Patient features accessible | Patient authenticated | Appointments, intake, documents features available | AC-2 |
| LOGIN-004 | Patient JWT contains role claim | Token issued after login | Token claim: `role="Patient"` | AC-7 |

#### 1.2 Staff Login
| Test ID | Test Case | Input | Expected Output | Acceptance Criteria |
|---------|-----------|-------|-----------------|-------------------|
| LOGIN-005 | Valid staff credentials | `email="staff@example.com"`, `password="StaffPass123!"` | Redirected to Staff Dashboard within 2 seconds | AC-1, AC-3 |
| LOGIN-006 | Staff portal displays correct UI | Staff logs in successfully | Staff-specific nav, staff color accents visible | AC-3 |
| LOGIN-007 | Staff features accessible | Staff authenticated | Queue management, patient search, code verification available | AC-3 |
| LOGIN-008 | Staff JWT contains role claim | Token issued after login | Token claim: `role="Staff"` | AC-7 |

#### 1.3 Admin Login
| Test ID | Test Case | Input | Expected Output | Acceptance Criteria |
|---------|-----------|-------|-----------------|-------------------|
| LOGIN-009 | Valid admin credentials | `email="admin@example.com"`, `password="AdminPass123!"` | Redirected to Admin Dashboard within 2 seconds | AC-1, AC-4 |
| LOGIN-010 | Admin portal displays correct UI | Admin logs in successfully | Admin-specific nav, admin color accents visible | AC-4 |
| LOGIN-011 | Admin features accessible | Admin authenticated | User management, roles, audit logs features available | AC-4 |
| LOGIN-012 | Admin JWT contains role claim | Token issued after login | Token claim: `role="Admin"` | AC-7 |

#### 1.4 Login Performance
| Test ID | Test Case | Condition | Expected Result | Acceptance Criteria |
|---------|-----------|-----------|-----------------|-------------------|
| LOGIN-013 | Redirect within SLA | Valid credentials submitted | Redirect completes in ≤ 2 seconds | AC-1 |
| LOGIN-014 | Token generation performance | Authentication succeeds | JWT issued within 500ms | AC-1 |
| LOGIN-015 | Database lookup performance | Credential validation | Query completes within 200ms | AC-1 |

---

### 2. Invalid Credentials Tests

#### 2.1 Credential Validation
| Test ID | Test Case | Input | Expected Behavior | Acceptance Criteria |
|---------|-----------|-------|-------------------|-------------------|
| CRED-001 | Wrong password | `email="patient@example.com"`, `password="WrongPassword123!"` | Generic error: "Invalid email or password" | AC-5 |
| CRED-002 | Wrong email | `email="wrong@example.com"`, `password="SecurePass123!"` | Generic error: "Invalid email or password" | AC-5 |
| CRED-003 | Both wrong | `email="wrong@example.com"`, `password="WrongPassword123!"` | Generic error: "Invalid email or password" | AC-5 |
| CRED-004 | Empty email | `email=""`, `password="SecurePass123!"` | Generic error: "Invalid email or password" | AC-5 |
| CRED-005 | Empty password | `email="patient@example.com"`, `password=""` | Generic error: "Invalid email or password" | AC-5 |
| CRED-006 | Both empty | `email=""`, `password=""` | Generic error: "Invalid email or password" | AC-5 |
| CRED-007 | Null email | `email=null`, `password="SecurePass123!"` | Generic error: "Invalid email or password" | AC-5 |
| CRED-008 | Null password | `email="patient@example.com"`, `password=null` | Generic error: "Invalid email or password" | AC-5 |

#### 2.2 Error Message Security
| Test ID | Test Case | Scenario | Expected Behavior | Notes |
|---------|-----------|----------|-------------------|-------|
| SEC-001 | No field-specific errors | Invalid password provided | Error doesn't mention "password incorrect" | Security best practice |
| SEC-002 | No email enumeration | Non-existent email provided | Same error as wrong password | Prevents user enumeration |
| SEC-003 | Error consistent for all failures | Various invalid inputs | All failures return identical message | Consistent security posture |

---

### 3. Account Verification Tests

#### 3.1 Unverified Account Detection
| Test ID | Test Case | Scenario | Expected Output | Acceptance Criteria |
|---------|-----------|----------|-----------------|-------------------|
| VERIFY-001 | Unverified account login blocked | User registered but not verified | Login fails, error: "Please verify your email to continue" | AC-6 |
| VERIFY-002 | Resend link offered | Unverified account attempts login | Error message includes link to resend verification email | AC-6 |
| VERIFY-003 | Account status checked before auth | Login request received | System queries account verification status | AC-6 |
| VERIFY-004 | Verified account logs in | Account verified via email link | Login succeeds normally | AC-6 |

---

### 4. Role-Based Routing Tests

#### 4.1 Portal Redirection
| Test ID | Test Case | Role | Expected Redirect Target | Acceptance Criteria |
|---------|-----------|------|--------------------------|-------------------|
| ROUTE-001 | Patient redirects to patient portal | Patient | `/dashboard/patient` | AC-1, AC-2 |
| ROUTE-002 | Staff redirects to staff portal | Staff | `/dashboard/staff` | AC-1, AC-3 |
| ROUTE-003 | Admin redirects to admin portal | Admin | `/dashboard/admin` | AC-1, AC-4 |
| ROUTE-004 | Correct URL pattern | Any role after login | URL matches role-specific pattern | AC-1 |

#### 4.2 Portal Isolation
| Test ID | Test Case | Condition | Expected Behavior | Notes |
|---------|-----------|-----------|-------------------|-------|
| ISOLATE-001 | Patient cannot access staff portal | Patient token provided | Access to `/dashboard/staff` denied (403) | Authorization |
| ISOLATE-002 | Staff cannot access admin portal | Staff token provided | Access to `/dashboard/admin` denied (403) | Authorization |
| ISOLATE-003 | Patient cannot access admin portal | Patient token provided | Access to `/dashboard/admin` denied (403) | Authorization |
| ISOLATE-004 | Role validation on each request | Any mismatched role/URL | Request rejected before rendering portal | Security |

---

### 5. JWT Token Tests

#### 5.1 Token Generation
| Test ID | Test Case | Condition | Expected Output | Acceptance Criteria |
|---------|-----------|-----------|-----------------|-------------------|
| JWT-001 | Token issued on login | Authentication succeeds | JWT token generated and returned | AC-7 |
| JWT-002 | Token contains subject claim | Token generated | Claim: `sub: userId` | AC-7 |
| JWT-003 | Token contains role claim | Token generated | Claim: `role: "Patient"/"Staff"/"Admin"` | AC-7 |
| JWT-004 | Token contains email claim | Token generated | Claim: `email: user@example.com` | AC-7 |
| JWT-005 | Token contains issued-at claim | Token generated | Claim: `iat: timestamp` | AC-7 |
| JWT-006 | Token contains expiry claim | Token generated | Claim: `exp: timestamp+TTL` | AC-7 |
| JWT-007 | Token is signed | Token generated | Signature valid with private key | AC-7 |

#### 5.2 Token Validation
| Test ID | Test Case | Scenario | Expected Behavior | Notes |
|---------|-----------|----------|-------------------|-------|
| JWT-008 | Invalid token rejected | Malformed token provided | Authentication fails, 401 Unauthorized | Security |
| JWT-009 | Expired token rejected | Token past expiry time | Authentication fails, 401 Unauthorized | Session TTL |
| JWT-010 | Token signature verified | Token signature modified | Authentication fails, 401 Unauthorized | Integrity |
| JWT-011 | Token claims validated | Required claim missing | Authentication fails, 401 Unauthorized | Completeness |

#### 5.3 Token TTL & Expiry
| Test ID | Test Case | Condition | Expected Behavior | Notes |
|---------|-----------|-----------|-------------------|-------|
| JWT-012 | Token TTL default value | Token issued | Expiry = now + 24 hours (configurable) | Standard duration |
| JWT-013 | Token refresh available | Before expiry | Endpoint available to refresh token | Session extension |
| JWT-014 | Expired token requires re-login | After TTL expires | Token no longer valid, re-authentication needed | Security |

---

### 6. Email Handling Tests

#### 6.1 Case-Insensitive Email Matching
| Test ID | Test Case | Input | Expected Output | Edge Case Reference |
|---------|-----------|-------|-----------------|-------------------|
| EMAIL-001 | Lowercase email | `email="user@example.com"` | Login succeeds if password correct | E-001 |
| EMAIL-002 | Uppercase email | `email="USER@EXAMPLE.COM"` | Login succeeds if password correct | E-001 |
| EMAIL-003 | Mixed case email | `email="User@Example.Com"` | Login succeeds if password correct | E-001 |
| EMAIL-004 | Case variation matches existing | DB has `User@Example.com`, input `user@example.com` | Login succeeds if password correct | E-001 |
| EMAIL-005 | Whitespace trimmed | `email=" patient@example.com "` | Trimmed, then case-insensitive match performed | E-001 |

---

### 7. Account Status Tests

#### 7.1 Deactivated Account Detection
| Test ID | Test Case | Setup | Input | Expected Output | Edge Case Reference |
|---------|-----------|-------|-------|-----------------|-------------------|
| STATUS-001 | Deactivated account blocked | Account status = INACTIVE | Valid credentials | Error: "Account is inactive. Please contact support." | E-003 |
| STATUS-002 | Status checked before password validation | Account marked inactive | Any credentials | Login fails without password verification | E-003 |
| STATUS-003 | Reactivation enables login | Account reactivated | Valid credentials | Login succeeds | E-003 |

---

### 8. Multi-Role & Device Tests

#### 8.1 Multiple Roles Handling
| Test ID | Test Case | Setup | Expected Behavior | Edge Case Reference |
|---------|-----------|-------|-------------------|-------------------|
| MULTI-001 | User assigned multiple roles | User has roles: [Patient, Staff] | Login uses primary role for redirect | E-001 |
| MULTI-002 | Primary role redirect | Primary role = Staff | Redirect to Staff Dashboard | E-001 |
| MULTI-003 | Role switching available | Multiple roles assigned | Header option to switch roles (phase 2+) | E-001 |
| MULTI-004 | Token contains all roles | Multiple roles assigned | Token claim: `roles: ["Patient", "Staff"]` | E-001 |

#### 8.2 Simultaneous Login Handling
| Test ID | Test Case | Condition | Expected Behavior | Edge Case Reference |
|---------|-----------|-----------|-------------------|-------------------|
| DEVICE-001 | Login from device 1 | First login | Token A issued, session created | E-004 |
| DEVICE-002 | Login from device 2 | Same user, different device | Token B issued, both sessions valid | E-004 |
| DEVICE-003 | Both tokens valid simultaneously | Tokens A and B exist | Each token independently authenticates requests | E-004 |
| DEVICE-004 | Token from device 1 works | Using Token A | Request succeeds regardless of Device 2 activity | E-004 |
| DEVICE-005 | Logout device 1 | Invalidate Token A | Token A rejected, Token B still valid | E-004 |

---

### 9. Edge Cases & Error Handling Tests

#### 9.1 SQL Injection & Security
| Test ID | Test Case | Input | Expected Behavior | OWASP Coverage |
|---------|-----------|-------|-------------------|-----------------|
| INJECT-001 | SQL injection in email | `email="admin'--"`, `password="anything"` | Treated as literal string, login fails safely | A03:2021 - Injection |
| INJECT-002 | SQL injection in password | `email="user@example.com"`, `password="' OR '1'='1"` | Treated as literal string (hashed), login fails | A03:2021 - Injection |

#### 9.2 Brute Force & Rate Limiting
| Test ID | Test Case | Scenario | Expected Behavior | Notes |
|---------|-----------|----------|-------------------|-------|
| BRUTE-001 | Multiple failed attempts | 5+ failed logins in 5 minutes | Account temporarily locked (15 minutes) | Security |
| BRUTE-002 | Lockout notification | Account locked | User notified via email | A07:2021 - IDOR/ACL |
| BRUTE-003 | Lockout recovery | After 15 minutes | Account unlocked, login available | UX |
| BRUTE-004 | Unlock via email | Lock triggered | Email sent with unlock link / instructions | Recovery |

#### 9.3 Session Security
| Test ID | Test Case | Condition | Expected Behavior | Notes |
|---------|-----------|-----------|-------------------|-------|
| SESSION-001 | Session invalidation on logout | User clicks logout | All tokens invalidated, cannot reuse | Security |
| SESSION-002 | Session timeout | Inactive for 30+ minutes | Token automatically invalidated | A06:2021 - Broken Auth |
| SESSION-003 | Session fixation prevented | Attempt to use old session | New session ID on login | Security |
| SESSION-004 | Concurrent session limit | 3+ sessions from same user | Limit enforced (configurable) | Performance/Security |

---

### 10. Performance & Non-Functional Tests

#### 10.1 Response Time
| Test ID | Test Case | Condition | Expected Result | Requirement |
|---------|-----------|-----------|-----------------|-------------|
| PERF-001 | Login endpoint response time | Valid credentials | Response within 500ms | AC-1 |
| PERF-002 | Redirect latency | Authentication succeeds | Redirect initiated within 500ms | AC-1 |
| PERF-003 | Total login-to-dashboard time | Start at login form | Dashboard loads within 2 seconds | AC-1 |

#### 10.2 Scalability & Load
| Test ID | Test Case | Scenario | Expected Behavior | Notes |
|---------|-----------|----------|-------------------|-------|
| LOAD-001 | 100 concurrent logins | Load test | All complete within 2 seconds | Stress test |
| LOAD-002 | Token generation throughput | Generate 1000 tokens | All complete within 5 seconds | Benchmark |
| LOAD-003 | Database connection pooling | High concurrent load | Connections managed efficiently | Resource usage |

---

## Test Data Requirements

### Valid Test Data Sets

```json
{
  "validPatientLogins": [
    {
      "email": "john.patient@example.com",
      "password": "PatientPass123!",
      "expectedRole": "Patient",
      "expectedRedirect": "/dashboard/patient"
    }
  ],
  "validStaffLogins": [
    {
      "email": "jane.staff@example.com",
      "password": "StaffPass123!",
      "expectedRole": "Staff",
      "expectedRedirect": "/dashboard/staff"
    }
  ],
  "validAdminLogins": [
    {
      "email": "admin@example.com",
      "password": "AdminPass123!",
      "expectedRole": "Admin",
      "expectedRedirect": "/dashboard/admin"
    }
  ],
  "invalidCredentials": [
    { "email": "patient@example.com", "password": "WrongPassword123!" },
    { "email": "nonexistent@example.com", "password": "SecurePass123!" },
    { "email": "patient@example.com", "password": "" },
    { "email": "", "password": "SecurePass123!" }
  ],
  "unverifiedAccounts": [
    {
      "email": "unverified@example.com",
      "password": "UnverPass123!",
      "status": "UNVERIFIED",
      "expectedError": "Please verify your email to continue"
    }
  ],
  "inactiveAccounts": [
    {
      "email": "inactive@example.com",
      "password": "InactivePass123!",
      "status": "INACTIVE",
      "expectedError": "Account is inactive. Please contact support."
    }
  ],
  "caseVariationEmails": [
    "patient@example.com",
    "PATIENT@EXAMPLE.COM",
    "Patient@Example.Com",
    " patient@example.com ",
    "PATIENT@example.COM"
  ],
  "multiRoleUsers": [
    {
      "email": "dual@example.com",
      "roles": ["Patient", "Staff"],
      "primaryRole": "Staff",
      "expectedRedirect": "/dashboard/staff"
    }
  ]
}
```

---

## Test Execution Strategy

### Phase 1: Unit Tests (Isolated)
- Credential validation logic (email/password matching)
- Role determination logic
- JWT generation and validation
- Error message generation
- Email case-insensitivity
- Account status checks

### Phase 2: Integration Tests
- Database credential lookup
- Role assignment retrieval
- Session creation and storage
- Token storage in cache/session
- Email verification status check
- Account deactivation check

### Phase 3: E2E Tests
- Complete login workflow
- Portal redirection verification
- UI element visibility per role
- Feature access by role
- Multi-device login flows
- Logout and session invalidation

### Phase 4: Performance & Security Tests
- Login response time benchmarks
- Concurrent login load testing
- Brute force protection
- Rate limiting effectiveness
- SQL injection prevention
- Session fixation prevention

---

## Acceptance Criteria Coverage

| AC # | Test IDs | Status | Coverage |
|------|----------|--------|----------|
| AC-1 | LOGIN-001, LOGIN-005, LOGIN-009, LOGIN-013, LOGIN-014, PERF-001 | Planned | ✓ |
| AC-2 | LOGIN-001, LOGIN-002, LOGIN-003, LOGIN-004, ROUTE-001 | Planned | ✓ |
| AC-3 | LOGIN-005, LOGIN-006, LOGIN-007, LOGIN-008, ROUTE-002 | Planned | ✓ |
| AC-4 | LOGIN-009, LOGIN-010, LOGIN-011, LOGIN-012, ROUTE-003 | Planned | ✓ |
| AC-5 | CRED-001 through CRED-008, SEC-001 through SEC-003 | Planned | ✓ |
| AC-6 | VERIFY-001, VERIFY-002, VERIFY-003, VERIFY-004 | Planned | ✓ |
| AC-7 | JWT-001 through JWT-007, MULTI-004 | Planned | ✓ |

---

## Edge Cases Coverage

| Edge Case | Test IDs | Coverage |
|-----------|----------|----------|
| Multiple roles handling | MULTI-001, MULTI-002, MULTI-003, MULTI-004 | ✓ |
| Case-insensitive email | EMAIL-001 through EMAIL-005 | ✓ |
| Deactivated account | STATUS-001, STATUS-002, STATUS-003 | ✓ |
| Simultaneous multi-device login | DEVICE-001 through DEVICE-005 | ✓ |

---

## Dependencies & Assumptions

### Dependencies
- User entity with role field available
- Database with email unique constraint
- Email verification system from US_001 complete
- Token signing infrastructure in place
- Session management system available

### Assumptions
- Password validation handled prior (US_001)
- Database supports case-insensitive email queries
- JWT library available for token generation
- Email service available for notifications
- Rate limiting middleware available
- Session storage (Redis/database) configured

---

## Risk & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Case-sensitivity issues in email matching | Medium | High | Test all email variations, database collation verified |
| Token expiry not enforced | Low | High | Automated token refresh testing, clock validation |
| Brute force attacks on accounts | Medium | High | Implement rate limiting, test lockout mechanism |
| SQL injection vulnerabilities | Low | Critical | Use parameterized queries, penetration testing |
| Session fixation attacks | Low | High | Generate new session on each login, validate tokens |
| Performance degradation under load | Medium | Medium | Load testing, optimize database queries |

---

## Success Criteria

- ✓ All test cases executed
- ✓ 95%+ code coverage
- ✓ All acceptance criteria verified (AC-1 through AC-7)
- ✓ All edge cases handled gracefully
- ✓ Role-based routing confirmed for all three roles
- ✓ JWT tokens validated with all required claims
- ✓ Error messages are secure and user-friendly
- ✓ No security vulnerabilities (SQL injection, XSS, session fixation, etc.)
- ✓ Performance meets AC-1 SLA (2-second redirect, 500ms auth)
- ✓ Session management secure across multi-device scenarios
