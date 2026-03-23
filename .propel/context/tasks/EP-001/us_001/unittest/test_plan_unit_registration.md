# Unit Test Plan: Patient Account Registration (US-001)

**Document Version:** 1.0  
**Last Updated:** 2026-03-20  
**Status:** Final  
**Parent User Story:** [US-001](./us_001.md) - Patient Account Registration with Email Validation  
**Parent Epic:** EP-001 (User Account & Authentication)

---

## 1. Test Objectives

- Validate password security requirements are correctly enforced (8+ chars, uppercase, lowercase, number, special character)
- Verify email address format validation rejects malformed addresses
- Ensure account registration creates account with hashed password, prevents duplicates
- Confirm email verification tokens are cryptographically secure and single-use
- Validate form state management preserves user input on validation errors
- Test error handling for service failures (database, email service)
- Achieve 85%+ code coverage for core registration business logic

---

## 2. Test Scope

### In Scope

Unit tests for the following services and utilities:

| Component | File Path | Responsibility | Priority |
|-----------|-----------|-----------------|----------|
| PasswordValidator | `src/services/password-validator.ts` | Validate 5 password requirements | High |
| EmailValidator | `src/services/email-validator.ts` | Validate RFC 5322 email format | High |
| RegistrationService | `src/services/registration.service.ts` | Create account, handle duplicates, hash password | High |
| EmailVerificationService | `src/services/email-verification.service.ts` | Generate tokens, validate expiration, enforce single-use | High |
| FormValidator | `src/services/form-validator.ts` | Validate form fields, preserve state | Medium |
| TokenGenerator | `src/utils/token-generator.ts` | Generate secure random tokens | High |

### Out of Scope

- E2E user workflows (covered in test_plan_us_001.md)
- UI component rendering and interaction (Playwright E2E tests)
- Email service delivery (mocked in unit tests)
- Database integration (mocked repository in unit tests)
- Performance/load testing
- Security vulnerability scanning (OWASP handled separately)

---

## 3. Test Strategy

### Test Pyramid Allocation (for US-001)

| Level | Percentage | Focus | Count |
|-------|-----------|-------|-------|
| **Unit** | 60-70% | Business logic, validation, algorithms | ~40 tests |
| **Integration** | 20-30% | Service-to-service, API contracts | ~8 tests |
| **E2E** | 5-10% | User journeys, browser automation | ~3 tests |

### Unit Test Isolation Strategy

**Mocking Dependencies:**

| Dependency | Mock Strategy | Location | Purpose |
|-----------|---------------|----------|---------|
| User Repository | Mock interface | `test_data/mocks/mock-user-repository.ts` | Prevent DB access; return synthetic data |
| Email Service | Mock interface | `test_data/mocks/mock-email-service.ts` | Capture sent emails; simulate failures |
| Token Service | Mock/Real (tested separately) | `test_data/mocks/mock-token-service.ts` | Validate token behavior |
| Clock/Date | Mock/Time travel | Jest fake timers or explicit mock | Test expiration logic |
| Crypto Library | Real (critical path) | Use actual crypto; no mock | Password hashing must be real |

**Synthetic Test Data:**

All test data is defined in `test_data/` folder; no cross-dependency on other user stories:
- `fixtures.json` - Valid/invalid email/password combinations
- `mock_responses.json` - Expected API responses
- `edge_cases.json` - Boundary conditions
- `factories.ts` - Object builders for complex test data

### Framework & Tools

| Tool | Version | Purpose |
|------|---------|---------|
| Jest | [version from package.json] | Unit test execution |
| @testing-library/jest-dom | Latest | DOM assertions |
| jest-mock-extended | Latest | Advanced mocking capabilities |
| supertest (if API routes) | ^6.x | API endpoint testing |

---

## 4. Components Under Test

### 4.1 PasswordValidator Service

| Component | Type | File | Functions to Test |
|-----------|------|------|-------------------|
| PasswordValidator | Class/Service | `src/services/password-validator.ts` | `validate(pwd: string): ValidationResult` |

**Public Methods:**
- `validate(password: string): { isValid: boolean; errors: string[] }`
- Private methods: `hasMinLength()`, `hasUppercase()`, `hasLowercase()`, `hasNumber()`, `hasSpecialChar()`

**Logic Under Test:**
- Minimum 8 characters check (boundary: exactly 8, 7, 9)
- At least 1 uppercase letter [A-Z]
- At least 1 lowercase letter [a-z]
- At least 1 number [0-9]
- At least 1 special character [!@#$%^&*()_+\-=\[\]{};:'",.<>?/\\|`~]
- Return all failed requirements in error array

---

### 4.2 EmailValidator Service

| Component | Type | File | Functions to Test |
|-----------|------|------|-------------------|
| EmailValidator | Class/Service | `src/services/email-validator.ts` | `validate(email: string): ValidationResult` |

**Public Methods:**
- `validate(email: string): { isValid: boolean; error?: string }`
- Optional: `isDomainDisposable(domain: string): boolean` (for future phase)

**Logic Under Test:**
- Valid RFC 5322 email format (regex or validation library)
- Reject: missing @ symbol
- Reject: missing local part (before @)
- Reject: missing domain (after @)
- Reject: multiple @ symbols
- Reject: spaces in email
- Reject: empty string
- Accept: subdomains (user@sub.domain.com)
- Accept: email with + modifier (user+tag@domain.com)
- Accept: single character local/domain

---

### 4.3 RegistrationService

| Component | Type | File | Functions to Test |
|-----------|------|------|-------------------|
| RegistrationService | Class/Service | `src/services/registration.service.ts` | `register(email, password): Promise<RegistrationResult>` |

**Public Methods:**
- `async register(email: string, password: string): Promise<{ accountId: string; email: string; status: 'pending' }>`
- `async checkEmailExists(email: string): Promise<boolean>`

**Logic Under Test:**
- Accept valid credentials
- Hash password with bcrypt (verify output is not plaintext)
- Prevent duplicate email (unique constraint)
- Create account with status = 'pending'
- Generate verification token
- Queue email for delivery
- Return account ID
- Handle service failures (DB timeout, crypto error)

**Dependencies to Mock:**
- UserRepository.create(account)
- UserRepository.findByEmail(email)
- EmailService.queueVerificationEmail(email, token)
- PasswordHasher.hash(password)
- TokenGenerator.generate()

---

### 4.4 EmailVerificationService

| Component | Type | File | Functions to Test |
|-----------|------|------|-------------------|
| EmailVerificationService | Class/Service | `src/services/email-verification.service.ts` | `verify(token)`, `generateToken(email)` |

**Public Methods:**
- `async generateToken(email: string): Promise<{ token: string; expiresAt: Date }>`
- `async verifyToken(token: string): Promise<{ email: string; valid: boolean }>`
- `async markTokenUsed(token: string): Promise<void>`

**Logic Under Test:**
- Generate cryptographically secure token
- Token includes email and expiration (24 hours from now)
- Token is signed (JWT or HMAC-based)
- Validate token signature and structure
- Reject expired tokens
- Enforce single-use (reject reused tokens)
- Handle token tampering (invalid signature raises error)
- Handle service failures (token store unavailable)

**Dependencies to Mock:**
- TokenStore.save(token)
- TokenStore.find(token)
- Crypto library (real, not mocked - critical path)
- Clock/DateService (mock for expiration tests)

---

### 4.5 FormValidator Service

| Component | Type | File | Functions to Test |
|-----------|------|------|-------------------|
| FormValidator | Class/Service | `src/services/form-validator.ts` | `validate(form)`, `getFieldErrors()` |

**Public Methods:**
- `validate(formData: RegistrationForm): ValidationError[]`
- `validateField(fieldName: string, value: any): string | null`

**Logic Under Test:**
- Require email field
- Require password field
- Return specific error per field (e.g., "Email is required" vs "Password is required")
- Preserve form data structure on validation error (return formData alongside errors)
- Support incremental field validation (validate one field without affecting others)
- Handle empty/null/undefined values

**Dependencies:**
- None (pure validation logic)
- Reuse PasswordValidator and EmailValidator

---

## 5. Test Case Design

### 5.1 Password Validator Test Cases

#### Test Case Summary

| Test-ID | Name | Type | AC | Coverage |
|---------|------|------|----|---------| 
| PWD-001 | Valid password with all requirements | positive | AC1 | Main flow |
| PWD-002 | Password exactly 8 characters | positive | AC1 | Boundary |
| PWD-003 | Password with 7 characters | negative | AC1 | Boundary |
| PWD-004 | Password missing uppercase | negative | AC1 | Single requirement |
| PWD-005 | Password missing lowercase | negative | AC1 | Single requirement |
| PWD-006 | Password missing number | negative | AC1 | Single requirement |
| PWD-007 | Password missing special char | negative | AC1 | Single requirement |
| PWD-008 | Password with only uppercase | negative | AC1 | Multiple violations |
| PWD-009 | Password with multiple special chars | positive | AC1 | Extension |
| PWD-010 | Very long password (100+ chars) | positive | AC1 | Extension |
| PWD-011 | All special characters | negative | AC1 | Invalid format |
| PWD-012 | Space in password (valid) | positive | AC1 | Extension |

#### Test Case Details

**PWD-001: Valid password with all requirements**

```yaml
Test-ID: PWD-001
Type: positive (happy path)
Given: PasswordValidator instance
When: validate("SecurePass123!")
Then:
  - isValid === true
  - errors.length === 0
  - result = { isValid: true, errors: [] }
Assertions:
  - expect(result.isValid).toBe(true)
  - expect(result.errors).toEqual([])
```

**PWD-002: Password exactly 8 characters (boundary)**

```yaml
Test-ID: PWD-002
Type: positive (boundary)
Given: PasswordValidator instance
When: validate("Pass123!") # exactly 8 chars
Then:
  - isValid === true (not "too short")
  - all requirements met
Assertions:
  - expect(result.isValid).toBe(true)
  - expect(result.errors).not.toContain("minimum 8 characters")
```

**PWD-003: Password 7 characters (below boundary)**

```yaml
Test-ID: PWD-003
Type: negative (boundary)
Given: PasswordValidator instance
When: validate("Pass12!") # 7 chars
Then:
  - isValid === false
  - errors includes "minimum 8 characters"
Assertions:
  - expect(result.isValid).toBe(false)
  - expect(result.errors).toContain("must be at least 8 characters")
```

**PWD-004: Missing uppercase**

```yaml
Test-ID: PWD-004
Type: negative (single requirement violation)
Given: PasswordValidator instance, password meets all other requirements
When: validate("securepass123!") # no uppercase
Then:
  - isValid === false
  - errors includes uppercase requirement error
  - other 4 requirements pass
Assertions:
  - expect(result.isValid).toBe(false)
  - expect(result.errors).toContain("must contain at least 1 uppercase letter")
  - expect(result.errors.length).toBe(1)
```

**PWD-005: Missing lowercase**

```yaml
Test-ID: PWD-005
Type: negative
When: validate("SECUREPASS123!")
Then:
  - errors includes lowercase requirement error
Assertions:
  - expect(result.errors).toContain("must contain at least 1 lowercase letter")
```

**PWD-006: Missing number**

```yaml
Test-ID: PWD-006
Type: negative
When: validate("SecurePass!")
Then:
  - errors includes number requirement error
Assertions:
  - expect(result.errors).toContain("must contain at least 1 number")
```

**PWD-007: Missing special character**

```yaml
Test-ID: PWD-007
Type: negative
When: validate("SecurePass123")
Then:
  - errors includes special character requirement error
Assertions:
  - expect(result.errors).toContain("must contain at least 1 special character")
```

**PWD-008: Only uppercase (multiple violations)**

```yaml
Test-ID: PWD-008
Type: negative (multiple violations)
When: validate("PASS")
Then:
  - isValid === false
  - errors include: lowercase, number, special char, length (4 errors)
Assertions:
  - expect(result.isValid).toBe(false)
  - expect(result.errors.length).toBe(4)
```

**PWD-009: Multiple special characters (extension)**

```yaml
Test-ID: PWD-009
Type: positive
When: validate("Secure@Pass$123!")
Then:
  - isValid === true (multiple special chars OK)
Assertions:
  - expect(result.isValid).toBe(true)
```

**PWD-010: Very long password (100+ chars)**

```yaml
Test-ID: PWD-010
Type: positive (extension)
When: validate("A" + "a".repeat(50) + "123!" ) # 100 chars
Then:
  - isValid === true (no max length limit)
Assertions:
  - expect(result.isValid).toBe(true)
```

**PWD-011: All special characters (invalid format)**

```yaml
Test-ID: PWD-011
Type: negative
When: validate("!@#$%^&*()")
Then:
  - errors include: uppercase, lowercase, number, length
Assertions:
  - expect(result.errors).toContain("must contain at least 1 uppercase letter")
  - expect(result.errors).toContain("must contain at least 1 lowercase letter")
  - expect(result.errors).toContain("must contain at least 1 number")
```

**PWD-012: Space in password (valid character)**

```yaml
Test-ID: PWD-012
Type: positive (extension)
When: validate("Secure Pass 123!") # includes space
Then:
  - isValid === true (space is allowed)
Assertions:
  - expect(result.isValid).toBe(true)
```

---

### 5.2 Email Validator Test Cases

#### Test Case Summary

| Test-ID | Name | Type | Coverage |
|---------|------|------|----------|
| EMAIL-001 | Valid standard email | positive | Basic valid format |
| EMAIL-002 | Valid email with subdomain | positive | Extension |
| EMAIL-003 | Valid email with + modifier | positive | Extension |
| EMAIL-004 | Missing @ symbol | negative | Invalid format |
| EMAIL-005 | Missing local part | negative | Invalid format |
| EMAIL-006 | Missing domain part | negative | Invalid format |
| EMAIL-007 | Multiple @ symbols | negative | Invalid format |
| EMAIL-008 | Space in email | negative | Invalid format |

#### Test Case Details

**EMAIL-001: Valid standard email**

```yaml
Test-ID: EMAIL-001
Type: positive
When: validate("patient@example.com")
Then:
  - isValid === true
Assertions:
  - expect(result.isValid).toBe(true)
```

**EMAIL-002: Valid email with subdomain**

```yaml
Test-ID: EMAIL-002
Type: positive
When: validate("patient@mail.hospital.com")
Then:
  - isValid === true
Assertions:
  - expect(result.isValid).toBe(true)
```

**EMAIL-003: Valid email with + modifier**

```yaml
Test-ID: EMAIL-003
Type: positive (extension)
When: validate("patient+test123@example.com")
Then:
  - isValid === true
Assertions:
  - expect(result.isValid).toBe(true)
```

**EMAIL-004: Missing @ symbol**

```yaml
Test-ID: EMAIL-004
Type: negative
When: validate("patientexample.com")
Then:
  - isValid === false
  - error message indicates missing @
Assertions:
  - expect(result.isValid).toBe(false)
  - expect(result.error).toContain("Please enter a valid email address")
```

**EMAIL-005: Missing local part**

```yaml
Test-ID: EMAIL-005
Type: negative
When: validate("@example.com")
Then:
  - isValid === false
Assertions:
  - expect(result.isValid).toBe(false)
```

**EMAIL-006: Missing domain part**

```yaml
Test-ID: EMAIL-006
Type: negative
When: validate("patient@")
Then:
  - isValid === false
Assertions:
  - expect(result.isValid).toBe(false)
```

**EMAIL-007: Multiple @ symbols**

```yaml
Test-ID: EMAIL-007
Type: negative
When: validate("patient@example@com")
Then:
  - isValid === false
Assertions:
  - expect(result.isValid).toBe(false)
```

**EMAIL-008: Space in email**

```yaml
Test-ID: EMAIL-008
Type: negative
When: validate("patient @example.com")
Then:
  - isValid === false
Assertions:
  - expect(result.isValid).toBe(false)
```

---

### 5.3 Registration Service Test Cases

#### Test Case Summary

| Test-ID | Name | Type | Coverage |
|---------|------|------|----------|
| REG-001 | Successful account creation | positive | Happy path |
| REG-002 | Duplicate email rejected | negative | Business logic |
| REG-003 | Password is hashed | positive | Security |
| REG-004 | Account status set to pending | positive | State |
| REG-005 | Verification token generated | positive | Integration |
| REG-006 | Email queued for delivery | positive | Integration |
| REG-007 | Database error handling | negative | Error case |
| REG-008 | Email service error handling | negative | Error case |

#### Test Case Details

**REG-001: Successful account creation**

```yaml
Test-ID: REG-001
Type: positive (happy path)
Given:
  - MockUserRepository instance
  - RegistrationService instance
  - Valid email "patient@example.com", password "SecurePass123!"
When: await service.register(email, password)
Then:
  - Account created with email, hashed password, status='pending'
  - Verification token generated
  - Email queued for delivery
  - Returns { accountId, email, status='pending' }
Assertions:
  - expect(userRepository.create).toHaveBeenCalledWith( { email, hashedPassword, status: 'pending' } )
  - expect(emailService.queueVerificationEmail).toHaveBeenCalled()
  - expect(result.status).toBe('pending')
```

**REG-002: Duplicate email rejected**

```yaml
Test-ID: REG-002
Type: negative
Given:
  - User "patient@example.com" already exists
  - MockUserRepository.findByEmail returns existing user
When: await service.register("patient@example.com", "ValidPass123!")
Then:
  - Error thrown: "This email is already registered"
  - No account created
  - No verification email sent
Assertions:
  - expect(() => service.register(...)).rejects.toThrow("already registered")
  - expect(userRepository.create).not.toHaveBeenCalled()
  - expect(emailService.queueVerificationEmail).not.toHaveBeenCalled()
```

**REG-003: Password is hashed (not stored plaintext)**

```yaml
Test-ID: REG-003
Type: positive (security)
Given: Valid registration
When: await service.register(email, "SecurePass123!")
Then:
  - Password passed to repository is bcrypt hash
  - Hash is not equal to original password
  - Hash starts with "$2a$" or "$2b$" (bcrypt prefix)
Assertions:
  - const passedAccount = userRepository.create.mock.calls[0][0]
  - expect(passedAccount.hashedPassword).not.toBe("SecurePass123!")
  - expect(passedAccount.hashedPassword).toMatch(/^\$2[ay]\$/)
```

**REG-004: Account status set to pending**

```yaml
Test-ID: REG-004
Type: positive
When: await service.register(email, password)
Then:
  - Account.status === 'pending'
  - Not 'active' or 'verified' until email clicked
Assertions:
  - const account = userRepository.create.mock.calls[0][0]
  - expect(account.status).toBe('pending')
```

**REG-005: Verification token generated**

```yaml
Test-ID: REG-005
Type: positive
When: await service.register(email, password)
Then:
  - Token generated by TokenGenerator.generate()
  - Token included in email
Assertions:
  - expect(tokenGenerator.generate).toHaveBeenCalled()
```

**REG-006: Email queued for delivery**

```yaml
Test-ID: REG-006
Type: positive
When: await service.register(email, password)
Then:
  - EmailService.queueVerificationEmail called with (email, token)
Assertions:
  - expect(emailService.queueVerificationEmail).toHaveBeenCalledWith(email, expect.any(String))
```

**REG-007: Database error handling**

```yaml
Test-ID: REG-007
Type: negative (error case)
Given: UserRepository.create throws error
When: await service.register(email, password)
Then:
  - Error re-thrown with user-friendly message
  - No partial state (no email queued if DB fails)
Assertions:
  - expect(() => service.register(...)).rejects.toThrow("Unable to create account")
```

**REG-008: Email service error handling**

```yaml
Test-ID: REG-008
Type: negative (error case)
Given: EmailService.queueVerificationEmail throws error
When: await service.register(email, password)
Then:
  - Account still created (email is async)
  - Error logged for support
  - User directed to contact support or retry
Assertions:
  - expect(userRepository.create).toHaveBeenCalled()
  - expect(logger.error).toHaveBeenCalled()
```

---

### 5.4 Email Verification Service Test Cases

#### Test Case Summary

| Test-ID | Name | Type | Coverage |
|---------|------|------|----------|
| VERIFY-001 | Generate valid token | positive | Token generation |
| VERIFY-002 | Token includes email | positive | Token structure |
| VERIFY-003 | Token expiration set to 24h | positive | Expiration |
| VERIFY-004 | Validate non-expired token | positive | Validation |
| VERIFY-005 | Reject expired token | negative | Expiration |
| VERIFY-006 | Reject reused token | negative | Single-use |
| VERIFY-007 | Reject tampered token | negative | Security |
| VERIFY-008 | Token signature validation | positive | Security |

#### Test Case Details

**VERIFY-001: Generate valid token**

```yaml
Test-ID: VERIFY-001
Type: positive
When: await service.generateToken("patient@example.com")
Then:
  - Returns { token, expiresAt }
  - token is non-empty string
  - expiresAt is Date object
Assertions:
  - expect(result.token).toBeDefined()
  - expect(typeof result.token).toBe('string')
  - expect(result.token.length).toBeGreaterThan(20)
  - expect(result.expiresAt).toBeInstanceOf(Date)
```

**VERIFY-002: Token includes email (JWT claim)**

```yaml
Test-ID: VERIFY-002
Type: positive
When: await service.generateToken("patient@example.com")
Then:
  - Decode token payload
  - Payload contains email claim
Assertions:
  - const payload = jwt.decode(result.token)
  - expect(payload.email).toBe("patient@example.com")
```

**VERIFY-003: Token expiration exactly 24 hours**

```yaml
Test-ID: VERIFY-003
Type: positive (boundary)
Given: Current time = mockTime.now()
When: await service.generateToken(email)
Then:
  - expiresAt = now + 24 hours
  - Within 1 second tolerance
Assertions:
  - const expectedExpiry = new Date(now.getTime() + 24 * 60 * 60 * 1000)
  - expect(Math.abs(result.expiresAt - expectedExpiry)).toBeLessThan(1000) # 1 second tolerance
```

**VERIFY-004: Validate non-expired token**

```yaml
Test-ID: VERIFY-004
Type: positive
Given: Valid token generated 1 hour ago
When: await service.verifyToken(token)
Then:
  - Valid === true
  - Email extracted from token
Assertions:
  - expect(result.valid).toBe(true)
  - expect(result.email).toBe("patient@example.com")
```

**VERIFY-005: Reject expired token**

```yaml
Test-ID: VERIFY-005
Type: negative
Given: Token generated 25 hours ago (past expiration)
When: await service.verifyToken(token)
Then:
  - Valid === false
  - Error: "Token has expired"
Assertions:
  - expect(result.valid).toBe(false)
  - expect(result.error).toContain("expired")
```

**VERIFY-006: Reject reused token**

```yaml
Test-ID: VERIFY-006
Type: negative (single-use enforcement)
Given: Valid token, marked as used
When: await service.verifyToken(token)
Then:
  - Valid === false
  - Error: "Token has already been used"
Assertions:
  - expect(result.valid).toBe(false)
  - expect(result.error).toContain("already been used")
```

**VERIFY-007: Reject tampered token**

```yaml
Test-ID: VERIFY-007
Type: negative (security)
Given: Token with modified payload (e.g., email changed)
When: await service.verifyToken(tamperedToken)
Then:
  - Signature validation fails
  - Valid === false
Assertions:
  - expect(result.valid).toBe(false)
```

**VERIFY-008: Token signature validation (HMAC/RSA)**

```yaml
Test-ID: VERIFY-008
Type: positive (security)
Given: Valid token signed with secret key
When: Verify signature
Then:
  - Signature is valid
  - Token not modified by client
Assertions:
  - expect(jwt.verify(token, SECRET_KEY)).not.toThrow()
```

---

### 5.5 Form Validator Test Cases

#### Test Case Summary

| Test-ID | Name | Type | Coverage |
|---------|------|------|----------|
| FORM-001 | All fields valid | positive | Happy path |
| FORM-002 | Email field required | negative | Required field |
| FORM-003 | Password field required | negative | Required field |
| FORM-004 | Specific error per field | positive | Error specificity |
| FORM-005 | Form data preserved on error | positive | State preservation |
| FORM-006 | Incremental field validation | positive | User experience |

#### Test Case Details

**FORM-001: All fields valid**

```yaml
Test-ID: FORM-001
Type: positive
When: validate({ email: "patient@example.com", password: "SecurePass123!" })
Then:
  - errors.length === 0
Assertions:
  - expect(result.errors).toEqual([])
```

**FORM-002: Email field required**

```yaml
Test-ID: FORM-002
Type: negative
When: validate({ email: "", password: "SecurePass123!" })
Then:
  - errors includes: "Email is required"
Assertions:
  - expect(result.errors).toContainEqual(expect.objectContaining({ field: "email", message: "Email is required" }))
```

**FORM-003: Password field required**

```yaml
Test-ID: FORM-003
Type: negative
When: validate({ email: "patient@example.com", password: "" })
Then:
  - errors includes: "Password is required"
Assertions:
  - expect(result.errors).toContainEqual(expect.objectContaining({ field: "password" }))
```

**FORM-004: Specific error per field**

```yaml
Test-ID: FORM-004
Type: positive (error specificity)
When: validate({ email: "invalid", password: "short" })
Then:
  - errors[0].field === "email"
  - errors[0].message is different from errors[1].message
Assertions:
  - expect(errors.find(e => e.field === "email").message).toContain("valid email")
  - expect(errors.find(e => e.field === "password").message).toContain("password")
```

**FORM-005: Form data preserved on error**

```yaml
Test-ID: FORM-005
Type: positive (AC #5: data persistence)
Given: Form with partial valid data
When: validate({ email: "patient@example.com", password: "invalid" })
Then:
  - errors returned
  - form data structure preserved
  - email field still contains "patient@example.com"
Assertions:
  - expect(result.formData.email).toBe("patient@example.com")
  - expect(result.formData.password).toBe("invalid") # preserved, not cleared
```

**FORM-006: Incremental field validation**

```yaml
Test-ID: FORM-006
Type: positive (user experience)
When: validateField("email", "patient@example.com") called independently
Then:
  - Only email field validated
  - No validation of password required
  - Returns: null if valid, error string if invalid
Assertions:
  - expect(result).toBeNull() # or undefined, depending on implementation
```

---

## 6. Test Data & Fixtures

### 6.1 Test Data Files

Create the following files in `.propel/context/tasks/EP-001/us_001/unittest/test_data/`:

#### fixtures.json

```json
{
  "validEmails": [
    "john.doe@example.com",
    "patient+test123@hospital.org",
    "user@clinic.hospital.com",
    "patient+test@example.com",
    "valid.email@domain.co.uk",
    "a@b.co"
  ],
  "validPasswords": [
    "SecurePass123!",
    "MyP@ssw0rd",
    "Pass123!",
    "Complex@Password1",
    "Test@12345"
  ],
  "invalidEmails": {
    "noAtSymbol": "userexample.com",
    "missingDomain": "user@",
    "missingLocal": "@example.com",
    "multipleAt": "user@@example.com",
    "withSpace": "user @example.com",
    "empty": ""
  },
  "invalidPasswords": {
    "shortPassword": "Short1!",
    "noUppercase": "securepass123!",
    "noLowercase": "SECUREPASS123!",
    "noNumber": "SecurePass!",
    "noSpecialChar": "SecurePass123"
  },
  "boundaryPasswords": {
    "exactly8chars": "Pass123!",
    "exactly7chars": "Pass123",
    "veryLong": "Aa1!BbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz1234567890"
  },
  "users": {
    "newPatient": {
      "id": "test-user-001",
      "email": "patient@example.com",
      "hashedPassword": "$2b$10$...", // bcrypt hash of "SecurePass123!"
      "status": "pending",
      "createdAt": "2026-03-20T10:00:00Z"
    },
    "existingPatient": {
      "id": "test-user-002",
      "email": "existing@example.com",
      "hashedPassword": "$2b$10$...",
      "status": "active",
      "createdAt": "2026-01-01T10:00:00Z"
    }
  },
  "tokens": {
    "validToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiredToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tamperedToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

#### edge_cases.json

```json
{
  "passwordEdgeCases": {
    "boundaryLength": {
      "exactly8": "Pass123!",
      "exactly7": "Pass123",
      "exactly9": "Pass123!X"
    },
    "specialCharacters": {
      "various": "P@ss!Word#123",
      "unicode": "Pässwörd123!",
      "space": "Pass Word 123!"
    }
  },
  "emailEdgeCases": {
    "singleChar": "a@b.co",
    "subdomain": "user@mail.hospital.org",
    "plusModifier": "user+tag@example.com",
    "longLocal": "verylongemailaddresswithnumbersandletters123456789@example.com",
    "numericLocal": "12345@example.com"
  }
}
```

### 6.2 Mock Implementations

#### mock-user-repository.ts

```typescript
/**
 * Mock UserRepository for unit testing
 * Prevents database access during unit tests
 */

export class MockUserRepository {
  private users: Map<string, any> = new Map();

  constructor(initialUsers: any[] = []) {
    initialUsers.forEach(user => this.users.set(user.email, user));
  }

  async create(account: any): Promise<any> {
    if (this.users.has(account.email)) {
      throw new Error("This email is already registered");
    }
    const newUser = { id: `user-${Date.now()}`, ...account };
    this.users.set(account.email, newUser);
    return newUser;
  }

  async findByEmail(email: string): Promise<any | null> {
    return this.users.get(email) || null;
  }

  reset(): void {
    this.users.clear();
  }

  getAllUsers(): any[] {
    return Array.from(this.users.values());
  }
}
```

#### mock-email-service.ts

```typescript
/**
 * Mock EmailService for unit testing
 * Captures sent emails without actually sending
 */

export class MockEmailService {
  private sentEmails: any[] = [];
  private failureMode: boolean = false;

  async queueVerificationEmail(email: string, token: string): Promise<void> {
    if (this.failureMode) {
      throw new Error("Email service temporarily unavailable");
    }
    this.sentEmails.push({ to: email, subject: "Verify Your Account", token, timestamp: Date.now() });
  }

  getSentEmails(): any[] {
    return this.sentEmails;
  }

  getLastEmail(): any | null {
    return this.sentEmails[this.sentEmails.length - 1] || null;
  }

  setFailureMode(fail: boolean): void {
    this.failureMode = fail;
  }

  reset(): void {
    this.sentEmails = [];
    this.failureMode = false;
  }
}
```

---

## 7. Mocking Strategy

### Dependencies to Mock

| Dependency | Type | Reason | Mock Location |
|-----------|------|--------|---------------|
| UserRepository | Interface/Abstract | DB isolation | `mock-user-repository.ts` |
| EmailService | Interface/Abstract | No actual email delivery | `mock-email-service.ts` |
| TokenStore | Interface/Abstract | Memory only, no persistence | In-memory map |
| PasswordHasher | Real/Mock hybrid | Use real bcrypt for crypto tests; mock for performance | Conditionally real |
| Clock/Date | Partial mock | Test expiration logic without waiting 24h | Jest fake timers |
| Logger | Mock | Capture log messages | jest.mock('logger') |

### Services NOT to Mock

- **PasswordValidator**: Pure validation logic; no external dependencies
- **EmailValidator**: Pure validation logic; no external dependencies
- **Crypto library**: Real crypto for password hashing and token signing (security critical)

---

## 8. Test Execution Strategy

### Test Organization Structure

```
tests/us-001/
├── unit/
│   ├── password-validator.test.ts         (PWD-001 to PWD-012)
│   ├── email-validator.test.ts            (EMAIL-001 to EMAIL-008)
│   ├── registration.service.test.ts       (REG-001 to REG-008)
│   ├── email-verification.test.ts         (VERIFY-001 to VERIFY-008)
│   └── form-validator.test.ts             (FORM-001 to FORM-006)
├── integration/
│   ├── registration-flow.test.ts          (End-to-end service tests)
│   └── email-verification-flow.test.ts
└── e2e/
    └── registration-ui.spec.ts            (Playwright browser tests)
```

### Execution Order

1. **Fast feedback loop**: Run unit tests on file save (watch mode)
2. **Pre-commit**: Run all unit tests + linting
3. **CI/CD [on PR]**: Run unit tests + integration tests + coverage report
4. **Pre-release**: Run all tests + E2E tests

### Required NPM Scripts

```json
{
  "scripts": {
    "test": "jest",
    "test:watch": "jest --watch",
    "test:coverage": "jest --coverage",
    "test:us001": "jest --testPathPattern=us-001",
    "test:us001:unit": "jest --testPathPattern=us-001/unit"
  }
}
```

---

## 9. Success Criteria & Coverage

### Code Coverage Targets

| Component | Target | Threshold |
|-----------|--------|-----------|
| PasswordValidator | 100% | Line + Branch coverage |
| EmailValidator | 100% | Line + Branch coverage |
| RegistrationService | 85%+ | Critical paths 95% |
| EmailVerificationService | 90%+ | Crypto logic 100% |
| FormValidator | 80% | Core validation paths |
| **Overall** | **85%** | **Global target** |

### Test Count Summary

| Category | Target | Actual |
|----------|--------|--------|
| Password Validation | ~12 | 12 |
| Email Validation | ~8 | 8 |
| Registration Logic | ~8 | 8 |
| Email Verification | ~8 | 8 |
| Form Validation | ~6 | 6 |
| **Total Unit Tests** | **~45** | **42-45** |

### Acceptance Criteria Mapping

- [ ] AC1 (Password requirements) → PWD-001 to PWD-012 ✓
- [ ] AC2 (Email verification) → VERIFY-001 to VERIFY-008 ✓
- [ ] AC3 (Duplicate email) → REG-002 ✓
- [ ] AC4 (Inline validation) → PWD-001, EMAIL-001, all negative cases ✓
- [ ] AC5 (Form data persistence) → FORM-005, REG-001, REG-006 ✓
- [ ] AC6 (Link expiration) → VERIFY-003, VERIFY-005 ✓

### Quality Assurance Checklist

- [ ] All 42-45 test cases designed with Given/When/Then
- [ ] All test cases use synthetic data (no cross-story dependencies)
- [ ] All external dependencies mocked (DB, email, tokens)
- [ ] Test isolation verified (tests run in any order, independently)
- [ ] Mocks documented with behavior specifications
- [ ] Test data fixtures defined in JSON files
- [ ] Edge cases covered for all password requirements
- [ ] Email validation covers RFC 5322 compliance
- [ ] Service failure scenarios tested (DB error, email error)
- [ ] Error message specificity validated
- [ ] Coverage report shows 85%+ line coverage
- [ ] All tests follow Jest best practices
- [ ] Tests are framework-agnostic where possible
- [ ] Fixtures support parameterized testing

---

## 10. References

### User Story Reference
- [US-001](./us_001.md) - Patient Account Registration with Email Validation

### Test Plan Reference
- [E2E Test Plan](../../docs/test_plan_us_001.md) - Comprehensive E2E test coverage

### Technology Documentation
- [Jest Documentation](https://jestjs.io/) - Testing framework
- [Jest Mock Extended](https://github.com/marchaos/jest-mock-extended) - Advanced mocking

### Security Standards
- [OWASP Top 10](https://owasp.org/www-project-top-ten/) - Security validation
- [RFC 5322](https://www.rfc-editor.org/rfc/rfc5322) - Email format specification
- [NIST Password Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html) - Password requirements

### Codebase References
- Test tools configuration: `package.json`
- TypeScript configuration: `tsconfig.json`
- Jest config: `jest.config.ts` (if exists)

---

**End of Unit Test Plan**

**Document Version History:**
- v1.0 (2026-03-20): Initial comprehensive unit test plan for US-001
