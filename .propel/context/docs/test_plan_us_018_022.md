---
id: test_plan_us_018_022
title: Test Plan - EP-001 Authentication & User Management (US_018-022)
version: 1.0.0
status: draft
author: AI Assistant
created: 2026-03-23
scope: "EP-001 authentication, registration, RBAC, user lifecycle"
---

# Test Plan: EP-001 Authentication & User Management (US_018-022)

## Overview

This test plan covers **5 critical security-focused user stories** handling patient registration, login, role-based access control, admin user management, and compliance logging. These stories implement the authentication and authorization layer supporting all role-specific functionality.

**User Stories Covered:**
- US_018: Patient Account Registration
- US_019: User Login & Session Management
- US_020: Role-Based Access Control Enforcement
- US_021: Admin User Management
- US_022: Auth Audit Logging & Session Timeout Warning

---

## 1. US_018: Patient Account Registration

### Test Objectives
- Verify patient self-registration with email validation
- Confirm verification email sent within 2 minutes
- Test account status transitions (Pending → Active)
- Validate password strength requirements
- Prevent duplicate email registration

### Test Cases

#### TC-US-018-HP-01: Successful Patient Registration
| Field | Value |
|-------|-------|
| Requirement | FR-001 |
| Type | happy_path |
| Priority | P0 |

**Given**: User on registration page  
**When**: Enter valid personal info (name, DOB, email, phone, password)  
**Then**: Account created with status "Pending" and verification email sent

**Test Data:**
```yaml
valid_registration:
  name: "John Smith"
  date_of_birth: "1990-05-15"
  email: "john.smith@example.com"
  phone: "+1-555-0100"
  password: "SecurePass123!"
  
password_requirements:
  - min_length: 8
  - requires_uppercase: true
  - requires_number: true
  - requires_special: true
```

**Expected Results:**
- [ ] Account created in database
- [ ] Status set to "Pending"
- [ ] Password hashed with BCrypt (cost factor 12)
- [ ] Email verification sent within 30 seconds
- [ ] Unique verification token generated (36+ char, URL-safe)
- [ ] Verification link valid for 24 hours
- [ ] User redirected to email confirmation page with instructions

---

#### TC-US-018-HP-02: Email Verification Flow
| Field | Value |
|-------|-------|
| Requirement | FR-001 |
| Type | happy_path |
| Priority | P0 |

**Given**: Verification email received  
**When**: Click verification link in email  
**Then**: Account activated and status changed to "Active"

**Expected Results:**
- [ ] Link navigates to verification endpoint
- [ ] Token validated without expiration errors
- [ ] Account status updated to "Active"
- [ ] User redirected to login page with success message
- [ ] Token invalidated (cannot be reused)
- [ ] User can now log in

---

#### TC-US-018-ER-01: Duplicate Email Rejection
| Field | Value |
|-------|-------|
| Requirement | DR-001 |
| Type | error |
| Priority | P0 |

**Given**: Email "existing@example.com" already registered  
**When**: Attempt to register with same email  
**Then**: Error message displayed; registration fails

**Expected Results:**
- [ ] System does NOT reveal whether email exists (prevent enumeration)
- [ ] User sees: "If this email is registered, check your inbox for password recovery"
- [ ] No account created
- [ ] Password recovery email sent instead
- [ ] Transaction rolled back

---

#### TC-US-018-ER-02: Invalid Password Strength
| Field | Value |
|-------|-------|
| Requirement | FR-001, TR-013 |
| Type | error |
| Priority | P1 |

**Given**: Password does not meet requirements  
**When**: Submit registration form  
**Then**: Validation error with specific requirements shown

**Test Cases:**
```yaml
invalid_passwords:
  - "short1!" # Less than 8 chars
  - "nouppercase123!" # No uppercase
  - "NoNumbers!" # No numbers
  - "NoSpecial123" # No special char
  
error_messages:
  - "Password must be at least 8 characters"
  - "Password must contain an uppercase letter"
  - "Password must contain a number"
  - "Password must contain a special character (!@#$%^&*)"
```

**Expected Results:**
- [ ] Specific requirement failures identified
- [ ] All requirements clearly stated
- [ ] Password field highlighted in red
- [ ] No account created

---

#### TC-US-018-ER-03: Expired Verification Link
| Field | Value |
|-------|-------|
| Requirement | FR-001 |
| Type | error |
| Priority | P1 |

**Given**: Verification link expired (>24 hours old)  
**When**: Click expired link  
**Then**: Error message; option to resend verification

**Expected Results:**
- [ ] Error: "Verification link has expired"
- [ ] "Resend Verification Email" button displayed
- [ ] Clicking resend generates new token
- [ ] New email sent with fresh link
- [ ] Account remains in "Pending" state

---

#### TC-US-018-ER-04: Missing Required Fields
| Field | Value |
|-------|-------|
| Requirement | FR-001 |
| Type | error |
| Priority | P1 |

**Given**: Form submitted with missing required field  
**When**: User skips one or more required fields  
**Then**: Inline validation errors shown

**Required Fields:**
- Name (non-empty)
- Date of birth (valid date, minimum age 18)
- Email (valid format)
- Phone (10+ digits)
- Password (meets strength requirements)
- Terms acceptance (checkbox)

**Expected Results:**
- [ ] Each missing field highlighted
- [ ] Error messages appear inline below field
- [ ] Form not submitted
- [ ] No partial records created

---

### Security Considerations
- **Email Enumeration**: Do NOT confirm/deny if email exists (FR-001 AC3)
- **Password Storage**: BCrypt cost factor 12, never log plaintext
- **Token Generation**: Cryptographically secure, 36+ chars, URL-safe Base64
- **Session Fixation**: New session created post-verification
- **CSRF Protection**: Form includes CSRF token
- **Rate Limiting**: Max 5 registration attempts per IP per hour

---

## 2. US_019: User Login & Session Management

### Test Objectives
- Verify successful login with JWT token generation
- Confirm session stored in Redis with 15-minute TTL
- Test session timeout and automatic logout
- Validate login failure responses (generic message)
- Confirm role-appropriate dashboard redirect

### Test Cases

#### TC-US-019-HP-01: Successful Login
| Field | Value |
|-------|-------|
| Requirement | FR-002 |
| Type | happy_path |
| Priority | P0 |

**Given**: Active user account with email "patient@example.com"  
**When**: Submit valid credentials (email + password)  
**Then**: JWT token generated; user redirected to dashboard

**Expected Results:**
- [ ] API returns 200 OK with JWT token
- [ ] JWT contains claims: user_id, email, role, iat, exp
- [ ] Token signed with RS256 algorithm
- [ ] Token expiration: 15 minutes from now
- [ ] Session stored in Redis with 900s TTL
- [ ] Session token matches JWT sub claim
- [ ] Frontend stores token in secure HTTP-only cookie
- [ ] User redirected to role-appropriate dashboard (Patient/Staff/Admin)
- [ ] No sensitive data in response body beyond token

---

#### TC-US-019-HP-02: Session Expires After 15 Minutes
| Field | Value |
|-------|-------|
| Requirement | NFR-005 |
| Type | happy_path |
| Priority | P1 |

**Given**: Valid session token with 15-minute TTL  
**When**: No requests made for 15+ minutes  
**Then**: Session automatically expires; user must re-login

**Expected Results:**
- [ ] Redis TTL countdown begins on login
- [ ] Token evicted from Redis after 900 seconds
- [ ] Subsequent request with expired token returns 401 Unauthorized
- [ ] Frontend detects 401 and redirects to login
- [ ] User prompted to log in again
- [ ] Clear message: "Your session has expired. Please log in again."

---

#### TC-US-019-ER-01: Invalid Credentials Response
| Field | Value |
|-------|-------|
| Requirement | FR-002, FR-001 AC3 |
| Type | error |
| Priority | P0 |

**Given**: Invalid email or wrong password  
**When**: Submit login form  
**Then**: Generic error message; no account enumeration

**Test Cases:**
```yaml
invalid_credentials:
  - email: "nonexistent@example.com"
    password: "AnyPassword123!"
  - email: "patient@example.com"
    password: "WrongPassword123!"
  - email: "invalid-email-format"
    password: "ValidPassword123!"
```

**Expected Results:**
- [ ] **SAME** generic error for all failures: "Invalid email or password"
- [ ] No indication whether email exists
- [ ] No indication which field is wrong
- [ ] Login attempt logged for security audit
- [ ] No partial session created
- [ ] API returns 401 Unauthorized
- [ ] Response time consistent (timing attack prevention)

---

#### TC-US-019-ER-02: Inactive Account Login Blocked
| Field | Value |
|-------|-------|
| Requirement | FR-002, NFR-006 |
| Type | error |
| Priority | P1 |

**Given**: Account with status "Inactive" or "Pending"  
**When**: Attempt to log in  
**Then**: Login rejected with generic error

**Expected Results:**
- [ ] Pending accounts cannot log in (must verify email first)
- [ ] Deactivated accounts cannot log in
- [ ] Generic error message (same as invalid credentials)
- [ ] No token generated
- [ ] Event logged for audit trail
- [ ] Admin notified if repeated attempts detected

---

#### TC-US-019-ER-03: Multiple Failed Login Attempts
| Field | Value |
|-------|-------|
| Requirement | TR-014 (rate limiting) |
| Type | error |
| Priority | P1 |

**Given**: User makes 5+ failed login attempts  
**When**: Continue attempting login  
**Then**: Account temporarily locked; IP rate-limited

**Expected Results:**
- [ ] After 5 failed attempts: account locked for 30 minutes
- [ ] IP rate-limited: max 10 attempts per hour
- [ ] User sees: "Too many failed attempts. Try again in 30 minutes."
- [ ] CAPTCHA required after 3 failures
- [ ] Admin/security team alerted to suspicious activity
- [ ] Logs record failed attempts with IP, timestamp, user agent

---

#### TC-US-019-HP-03: Session Activity Updates
| Field | Value |
|-------|-------|
| Requirement | NFR-005 |
| Type | happy_path |
| Priority | P1 |

**Given**: Active user making API requests  
**When**: Request made within session window  
**Then**: Session TTL resets (sliding window)

**Expected Results:**
- [ ] Each API request resets 15-minute TTL
- [ ] User remains logged in if continuously active
- [ ] Inactivity timeout enforced (no requests for 15 min)
- [ ] Last activity timestamp updated in audit log

---

### Session Management Architecture
```
Login Flow:
  1. POST /api/auth/login with credentials
  2. Validate password with BCrypt
  3. Generate JWT (RS256, 15-min exp)
  4. Store session in Redis: key=user_id, value=JWT, TTL=15min
  5. Return JWT to frontend
  6. Frontend stores in secure httpOnly cookie
  
Request Flow:
  1. Client includes JWT in Authorization header
  2. Middleware validates JWT signature & exp
  3. Extract claims and load user context
  4. Authorize based on role
  5. Execute endpoint logic
  
Logout Flow:
  1. Delete session from Redis
  2. Frontend clears cookie
  3. Redirect to login page
```

---

## 3. US_020: Role-Based Access Control (RBAC) Enforcement

### Test Objectives
- Verify role enforcement on API endpoints
- Test frontend navigation reflects user role
- Confirm Patient cannot access Staff/Admin resources
- Validate proper 403 responses
- Test minimum necessary access principle

### Test Cases

#### TC-US-020-HP-01: Patient Access Patient Resources
| Field | Value |
|-------|-------|
| Requirement | NFR-006, FR-003 |
| Type | happy_path |
| Priority | P0 |

**Given**: Authenticated patient user  
**When**: Access patient-allowed endpoints  
**Then**: Resources returned successfully

**Patient-Allowed Endpoints:**
```
GET  /api/patients/{id}/profile          - View own profile
GET  /api/appointments                    - View own appointments
POST /api/appointments                    - Book appointment
GET  /api/documents                       - View own documents
POST /api/documents                       - Upload document
GET  /api/health-dashboard                - View health dashboard
```

**Expected Results:**
- [ ] All patient endpoints return 200 OK
- [ ] User can only access their own data (FR-043)
- [ ] Cannot access other patient records
- [ ] Role claim verified in JWT
- [ ] No Staff/Admin endpoints accessible

---

#### TC-US-020-ER-01: Patient Blocked from Staff Endpoints
| Field | Value |
|-------|-------|
| Requirement | NFR-006, FR-003 |
| Type | error |
| Priority | P0 |

**Given**: Patient user token  
**When**: Attempt to access staff-only endpoint  
**Then**: 403 Forbidden returned

**Staff-Only Endpoints (Patient blocked):**
```
GET  /api/queue                           - Queue management
POST /api/arrivals                        - Mark patient arrived
GET  /api/patients/{id}                   - View patient records (on demand)
POST /api/walkins                         - Create walk-in booking
GET  /api/staff/reports                   - Staff reports
```

**Expected Results:**
- [ ] API returns 403 Forbidden
- [ ] Error message: "Insufficient permissions for this resource"
- [ ] No error details revealing endpoint existence
- [ ] Request logged to audit trail
- [ ] Frontend does NOT render Staff menu items for patient

---

#### TC-US-020-ER-02: Staff Blocked from Admin Endpoints
| Field | Value |
|-------|-------|
| Requirement | NFR-006, FR-003 |
| Type | error |
| Priority | P0 |

**Given**: Staff user token  
**When**: Attempt to access admin-only endpoint  
**Then**: 403 Forbidden returned

**Admin-Only Endpoints (Staff blocked):**
```
GET  /api/admin/users                     - User management
POST /api/admin/users                     - Create user
POST /api/admin/users/{id}/deactivate     - Deactivate user
GET  /api/admin/audit-logs                - Audit log access
```

**Expected Results:**
- [ ] API returns 403 Forbidden
- [ ] Frontend redirects to Staff dashboard
- [ ] Staff menu does NOT show Admin items
- [ ] Request logged to security audit
- [ ] No SQL errors or stack traces

---

#### TC-US-020-HP-02: Role-Based Navigation
| Field | Value |
|-------|-------|
| Requirement | FR-003 |
| Type | happy_path |
| Priority | P1 |

**Given**: User logged in with specific role  
**When**: View main navigation  
**Then**: Navigation items match user role

**Navigation Items by Role:**
```yaml
Patient:
  - Dashboard
  - Appointments
  - Documents
  - Health Record
  - Profile
  - Resources

Staff:
  - Dashboard
  - Queue Management
  - Patient Search
  - Arrivals
  - Verification
  - Reports

Admin:
  - Dashboard
  - User Management
  - Audit Logs
  - System Configuration
  - Reports
```

**Expected Results:**
- [ ] Patient role shows only Patient navigation
- [ ] Staff role shows only Staff navigation
- [ ] Admin role shows full navigation
- [ ] No admin links visible to patient users
- [ ] Navigation updates on role change (logout/login)

---

#### TC-US-020-ER-03: Data Isolation Enforced
| Field | Value |
|-------|-------|
| Requirement | FR-043, NFR-014 |
| Type | error |
| Priority | P0 |

**Given**: Patient A with ID "patient-uuid-001"  
**When**: Attempt to access Patient B's data (patient-uuid-002)  
**Then**: 403 Forbidden and no data returned

**Test Scenarios:**
```
GET  /api/patients/patient-uuid-002/profile       → 403
GET  /api/patients/patient-uuid-002/appointments  → 403
GET  /api/patients/patient-uuid-002/documents     → 403
```

**Expected Results:**
- [ ] Patient cannot access other patient records
- [ ] No error message revealing other patient exists
- [ ] Request logged with attempted unauthorized access
- [ ] Same test applies to Staff accessing other staff records

---

### RBAC Architecture
```csharp
// Role Enum
public enum UserRole
{
  Patient = 1,
  Staff = 2,
  Admin = 3
}

// Authorization Attributes
[Authorize(Roles = "Patient")]
[Authorize(Roles = "Staff,Admin")]
[Authorize(Roles = "Admin")]
[MinimumNecessaryAccessPolicy]  // Enforces FR-043
```

---

## 4. US_021: Admin User Management

### Test Objectives
- Verify admin can create Staff and Admin accounts
- Test user activation email workflow
- Confirm role assignment and permissions
- Validate account deactivation and session termination
- Test user update operations and audit tracking

### Test Cases

#### TC-US-021-HP-01: Create New Staff User
| Field | Value |
|-------|-------|
| Requirement | FR-004 |
| Type | happy_path |
| Priority | P1 |

**Given**: Admin user on User Management page  
**When**: Click "Create User" and submit form  
**Then**: New Staff account created; activation email sent

**Form Fields:**
```yaml
create_user_form:
  full_name: "Jane Doe"
  email: "jane.doe@clinic.com"
  role: "Staff"  # Dropdown: Staff or Admin
  department: "Front Desk"  # Optional
```

**Expected Results:**
- [ ] User created in database with status "Active"
- [ ] Account locked until first login (password reset)
- [ ] Activation email sent with password reset link
- [ ] Email sent within 2 minutes
- [ ] Reset link valid for 7 days
- [ ] Audit log entry created (user: admin_id, action: CREATE_USER)
- [ ] User role set correctly

---

#### TC-US-021-HP-02: User Update and Audit Trail
| Field | Value |
|-------|-------|
| Requirement | FR-004, FR-040 |
| Type | happy_path |
| Priority | P1 |

**Given**: Existing user "jane.doe@clinic.com" (Staff)  
**When**: Admin updates user details  
**Then**: Changes persisted and logged

**Update Fields:**
```yaml
updates:
  full_name: "Jane Smith"  # Name change
  role: "Admin"            # Role change
  department: "Management" # Department change
```

**Expected Results:**
- [ ] Changes saved to database
- [ ] Audit log entry created with old/new values
- [ ] Email sent to user notifying of role change
- [ ] Session NOT terminated (change takes effect on next login)
- [ ] Timestamp of change recorded
- [ ] Admin ID recorded (who made change)

---

#### TC-US-021-HP-03: Deactivate User Account
| Field | Value |
|-------|-------|
| Requirement | FR-004 |
| Type | happy_path |
| Priority | P1 |

**Given**: Active user "john.smith@clinic.com"  
**When**: Admin clicks "Deactivate" and confirms  
**Then**: Account deactivated; all sessions terminated

**Expected Results:**
- [ ] User status changed to "Inactive"
- [ ] All active sessions deleted from Redis
- [ ] User forced to log out immediately
- [ ] Login attempts blocked with account inactive message
- [ ] Deactivation timestamp recorded
- [ ] Audit log entry: action=DEACTIVATE_USER
- [ ] Notification sent to user (optional) if configured
- [ ] Change reversible (can reactivate)

---

#### TC-US-021-ER-01: Prevent Duplicate User Email
| Field | Value |
|-------|-------|
| Requirement | DR-001 |
| Type | error |
| Priority | P1 |

**Given**: "jane.doe@clinic.com" already exists  
**When**: Attempt to create user with same email  
**Then**: Validation error; user not created

**Expected Results:**
- [ ] Error: "Email already registered"
- [ ] Suggestions provided to update existing user
- [ ] Form not submitted
- [ ] Database remains consistent

---

#### TC-US-021-ER-02: Invalid Role Assignment
| Field | Value |
|-------|-------|
| Requirement | NFR-006 |
| Type | error |
| Priority | P1 |

**Given**: Admin attempting invalid role assignment  
**When**: Form submitted with invalid role  
**Then**: Validation error; user not created

**Invalid Scenarios:**
```yaml
invalid_scenarios:
  - role_value: "SuperAdmin"  # Not in enum
  - role_value: "Patient"     # Admin cannot create patient via this flow
  - role_value: ""            # Empty role
```

---

### User Management Workflow
```
Create User:
  1. Admin submits form with name, email, role
  2. System generates temporary password
  3. Creates active user account
  4. Sends activation email with password reset link
  5. User clicks link, sets own password
  6. User can then log in

Deactivate User:
  1. Admin clicks deactivate button
  2. Confirmation dialog shows
  3. On confirm: status changed to Inactive
  4. All Redis sessions deleted
  5. Audit log entry created
  6. Current request terminates session
```

---

## 5. US_022: Auth Audit Logging & Session Timeout Warning

### Test Objectives
- Verify comprehensive audit logging for auth events
- Test session timeout warning modal
- Validate failed login attempt logging
- Confirm audit logs immutability
- Test logging of sensitive data handling (masking)

### Test Cases

#### TC-US-022-HP-01: Successful Login Audit Log
| Field | Value |
|-------|-------|
| Requirement | FR-005, FR-040 |
| Type | happy_path |
| Priority | P0 |

**Given**: User successfully logs in  
**When**: Login completes  
**Then**: Audit log entry created immediately

**Audit Log Entry:**
```yaml
audit_entry:
  user_id: "user-uuid-001"
  action: "LOGIN_SUCCESS"
  timestamp: "2026-03-23T14:30:00Z"  # UTC
  ip_address: "192.0.2.1"
  user_agent: "Mozilla/5.0..."
  result: "SUCCESS"
  details:
    login_method: "email_password"
    session_duration: 900  # seconds
```

**Expected Results:**
- [ ] Audit entry created within 200ms
- [ ] User ID recorded (who)
- [ ] Action type recorded (LOGIN_SUCCESS)
- [ ] Timestamp in UTC
- [ ] IP address captured
- [ ] User agent captured (browser, OS)
- [ ] Entry persisted to database immediately
- [ ] No sensitive data in details

---

#### TC-US-022-HP-02: Failed Login Audit Log
| Field | Value |
|-------|-------|
| Requirement | FR-005 |
| Type | happy_path |
| Priority | P1 |

**Given**: User provides invalid credentials  
**When**: Login fails  
**Then**: Audit log entry records failed attempt

**Audit Log Entry:**
```yaml
failed_attempt:
  email_hash: "sha256:a4f82900..."  # Email hashed, not plaintext
  action: "LOGIN_FAILED"
  timestamp: "2026-03-23T14:35:00Z"
  ip_address: "192.0.2.2"
  user_agent: "Mozilla/5.0..."
  failure_reason: "invalid_credentials"
  attempt_count: 3  # Per IP
```

**Expected Results:**
- [ ] Email NOT stored in plaintext (hashed with SHA-256)
- [ ] Failure reason recorded (invalid_credentials, account_inactive, etc.)
- [ ] Attempt count tracked per IP
- [ ] No password attempt stored
- [ ] Entry persists to database
- [ ] IP address tracked for rate limiting

---

#### TC-US-022-HP-03: Session Timeout Warning Modal
| Field | Value |
|-------|-------|
| Requirement | FR-022 (implied), NFR-005 |
| Type | happy_path |
| Priority | P1 |

**Given**: User logged in for 13+ minutes (2 min before timeout)  
**When**: System detects inactivity approaching limit  
**Then**: Warning modal displayed

**Warning Modal Behavior:**
```
Title: "Session Expires Soon"
Body: "Your session will expire in 2 minutes due to inactivity. 
       Click 'Continue Session' to stay logged in."

Buttons:
  - "Continue Session" → Resets timer, dismisses modal
  - "Logout" → Logs out immediately
  - Auto-dismiss after 2 min (if no action) → Logs out
```

**Expected Results:**
- [ ] Modal appears when 2 minutes remain
- [ ] Modal is non-dismissible (cannot close via X button)
- [ ] "Continue Session" resets 15-minute timer
- [ ] "Logout" immediately terminates session
- [ ] Auto-logout if no action taken
- [ ] Warning not shown if active (request made within 1 min)
- [ ] Manual logout removes session from Redis

---

#### TC-US-022-HP-04: Session Timeout Audit Log
| Field | Value |
|-------|-------|
| Requirement | FR-005 |
| Type | happy_path |
| Priority | P1 |

**Given**: Session expires after 15 minutes of inactivity  
**When**: TTL expires in Redis  
**Then**: Audit log entry records timeout

**Audit Log Entry:**
```yaml
timeout_entry:
  user_id: "user-uuid-001"
  action: "SESSION_TIMEOUT"
  timestamp: "2026-03-23T14:45:00Z"
  last_activity: "2026-03-23T14:30:00Z"
  inactivity_duration: 900  # seconds
  ip_address: "192.0.2.1"
```

**Expected Results:**
- [ ] Timeout event logged
- [ ] User ID recorded
- [ ] Last activity timestamp recorded
- [ ] Session duration calculated
- [ ] Entry findable for compliance audit

---

#### TC-US-022-ER-01: Audit Log Immutability
| Field | Value |
|-------|-------|
| Requirement | AD-007, FR-040 |
| Type | error |
| Priority | P0 |

**Given**: Audit log entry exists  
**When**: Attempt to UPDATE or DELETE  
**Then**: Database constraint prevents operation

**Expected Results:**
- [ ] UPDATE attempt → Exception raised
- [ ] DELETE attempt → Exception raised
- [ ] Append-only pattern enforced
- [ ] No audit tampering possible
- [ ] Audit log serves as proof in investigations

---

#### TC-US-022-HP-05: Audit Log Query and Compliance Report
| Field | Value |
|-------|-------|
| Requirement | FR-040, NFR-007 |
| Type | happy_path |
| Priority | P1 |

**Given**: Admin accessing audit logs  
**When**: Filter and query audit entries  
**Then**: Compliance-ready report generated

**Filter Capabilities:**
```yaml
filters:
  - date_range: "2026-03-01 to 2026-03-31"
  - user_id: "user-uuid-001"
  - action_type: "LOGIN_SUCCESS, LOGIN_FAILED, CREATE_USER"
  - resource_type: "Appointment, Document, User"
```

**Expected Results:**
- [ ] Query returns matching entries within 2 seconds
- [ ] Results paginated (100 per page)
- [ ] CSV export available for compliance
- [ ] Timestamp filters precise (to second)
- [ ] Results include all relevant fields
- [ ] No modification of returned data

---

### Audit Log Service Architecture
```csharp
// Audit Log Schema
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }              // Who performed action
    public DateTime Timestamp { get; set; }         // When (UTC)
    public string ActionType { get; set; }          // What (LOGIN_SUCCESS, etc)
    public string ResourceType { get; set; }        // Which entity
    public Guid? ResourceId { get; set; }           // Which record ID
    public string ActionDetails { get; set; }       // JSONB with details
    public string IpAddress { get; set; }           // Where
    public string UserAgent { get; set; }           // Browser info
    
    // Immutable via DB trigger (no UPDATE/DELETE allowed)
}

// Audit Event Triggers
- OnUserRegistered → Log (action: REGISTER)
- OnLoginSuccess → Log (action: LOGIN_SUCCESS)
- OnLoginFailed → Log (action: LOGIN_FAILED, email_hash)
- OnSessionTimeout → Log (action: SESSION_TIMEOUT)
- OnCreateUser → Log (action: CREATE_USER)
- OnUpdateUser → Log (action: UPDATE_USER)
- OnDeactivateUser → Log (action: DEACTIVATE_USER)
```

---

## Test Execution Strategy

### Execution Sequence
1. **US_018** (Registration): Foundational for user lifecycle
2. **US_019** (Login): Prerequisite for session management
3. **US_020** (RBAC): Required to test role isolation
4. **US_021** (User Management): Admin workflows
5. **US_022** (Audit Logging): Cross-cutting, can run in parallel after 4

### Security-Focused Testing Approach
- **Negative Test Emphasis**: Error paths critical for auth
- **Threat Modeling**: OWASP Top 10 alignment (A01: Authentication)
- **Penetration Test Scenarios**: Enumeration, brute force, privilege escalation
- **Compliance Validation**: HIPAA audit trail requirements

### Test Environment Isolation
- **Separate Test Database**: No production data
- **Seeded Test Users**: Known credentials for all roles
- **Email Interception**: Capture/verify email sends without SMTP outbound
- **Security Headers**: Verify CORS, CSP, HSTS, etc.

---

## Security Test Considerations

### OWASP Top 10 Coverage

| OWASP Risk | Story | Test Case | Mitigation |
|------------|-------|-----------|-----------|
| A01: Broken Auth | US_019, US_020 | TC-019-ER-01, TC-020-ER-01 | Generic errors, session management |
| A02: Cryptography | US_004, US_019 | TC-004-HP-03 | BCrypt (12), RS256, TLS 1.2+ |
| A03: Injection | US_018 | Input validation | Parameterized queries, no SQL concatenation |
| A04: Insecure Design | US_018, US_020 | TC-020-ER-03 | Data isolation, minimum access |
| A07: Identification | US_022 | TC-022-HP-02 | Hashed emails in logs |
| A09: Logging | US_022 | TC-022-HP-01 | Immutable audit logs |

---

## Success Criteria

- [ ] All 5 user stories have comprehensive test coverage
- [ ] 100% authentication paths tested (happy + error)
- [ ] 100% RBAC enforcement verified
- [ ] 100% audit logging implementation tested
- [ ] Zero security bypass scenarios identified
- [ ] OWASP A01-A09 mitigations validated
- [ ] Session management working correctly
- [ ] Email workflows functional

---

## Sign-Off

**Status**: ✅ **READY FOR IMPLEMENTATION**  
**Scope**: EP-001 authentication and user management  
**Coverage**: 5 user stories, 28+ test cases  
**Security Risk Level**: HIGH (careful peer review required)  
**Completion Target**: Before EP-002 (appointment booking)
