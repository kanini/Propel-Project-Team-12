# Task - task_001_fe_inline_validation_error_patterns

## Requirement Reference
- User Story: US_066 - Error Handling Patterns
- Story Location: .propel/context/tasks/EP-011-II/us_066/us_066.md
- Acceptance Criteria:
    - AC-1: Inline validation errors appear below fields, styled in red with error icon, providing specific guidance (e.g., "Phone number must be 10 digits" not "Invalid input")
- Edge Case:
    - Multiple validation errors simultaneously: All errors display at once with summary at top ("3 issues found"), individual inline messages per field, focus shifts to first error
    - Errors in modals: Modal errors display within modal context, modal does not auto-close on error

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A (cross-cutting pattern affecting all forms) |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-001-registration.html (validation), .propel/context/wireframes/Hi-Fi/wireframe-SCR-002-login.html (error state), .propel/context/wireframes/Hi-Fi/wireframe-SCR-007-appointment-booking.html (multi-field validation) |
| **Screen Spec** | .propel/context/docs/figma_spec.md#UXR-601 (inline validation errors) |
| **UXR Requirements** | UXR-601 (Inline field-level validation errors below form field) |
| **Design Tokens** | .propel/context/docs/designsystem.md#colors (error colors), designsystem.md#typography (caption size for error text) |

> **Wireframe Status Legend:**
> - **AVAILABLE**: Local file exists at specified path
> - **PENDING**: UI-impacting task awaiting wireframe (provide file or URL)
> - **EXTERNAL**: Wireframe provided via external URL
> - **N/A**: Task has no UI impact
>
> If UI Impact = No, all design references should be N/A

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**
**IF Wireframe Status = AVAILABLE or EXTERNAL:**
- **MUST** open and reference the wireframe file/URL during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, hover, focus, error, loading)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px
- Run `/analyze-ux` after implementation to verify pixel-perfect alignment

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Frontend | TypeScript | 5.x |
| Frontend | Redux Toolkit | 2.x |
| Frontend | Tailwind CSS | Latest |
| Library | React Hook Form | (if adopted) or custom validation |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> **Mobile Impact Legend:**
> - **Yes**: Task involves mobile app development (native or cross-platform)
> - **No**: Task is web, backend, or infrastructure only
>
> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview
Standardize inline validation error patterns across all forms to meet UXR-601 requirement (field-level error display). This task creates reusable validation utilities, error message components, and form validation hooks following existing LoginForm.tsx pattern. It implements error summary for multiple simultaneous errors, focus management to first error field, and ensures modal-based forms display errors within modal context without auto-closing.

## Dependent Tasks
- None (enhances existing form components)

## Impacted Components
- **CREATE** `src/frontend/src/components/common/FieldError.tsx` - Reusable field error component with red styling and error icon
- **CREATE** `src/frontend/src/components/common/FormErrorSummary.tsx` - Multi-error summary component displaying at top of form
- **CREATE** `src/frontend/src/hooks/useFormValidation.ts` - Custom hook for form validation logic with touched fields tracking
- **CREATE** `src/frontend/src/utils/validators.ts` - Validation utility functions (phone, date, required, etc.) - EXTEND EXISTING
- **CREATE** `src/frontend/src/utils/errorMessages.ts` - Centralized error message templates with specific guidance
- **MODIFY** `src/frontend/src/features/auth/components/LoginForm.tsx` - Refactor to use FieldError component (already follows UXR-601 pattern, just needs component extraction)
- **MODIFY** `src/frontend/src/features/auth/components/RegistrationForm.tsx` - Use FieldError and FormErrorSummary components
- **MODIFY** `src/frontend/src/features/admin/components/UserFormModal.tsx` - Implement modal error handling without auto-close
- **MODIFY** `src/frontend/src/components/waitlist/WaitlistEnrollmentModal.tsx` - Implement modal error handling

## Implementation Plan

1. **Create FieldError Component**
   - Props: `error: string | undefined`, `id: string` (for aria-describedby linking)
   - Visual: Red text (text-error), caption size (12px), error icon (AlertCircle from Lucide or inline SVG)
   - Layout: Display below input field with 4px top margin
   - ARIA: Use role="alert" for live announcement, link to field via aria-describedby
   - Reference designsystem.md: error-default (#DC2626), caption typography (12px/18px)
   - Only render if `error` is truthy

2. **Create FormErrorSummary Component**
   - Props: `errors: Record<string, string>`, `onFieldClick: (fieldId: string) => void`
   - Display: Styled box at top of form with error-light background, error border-left-4
   - Content: "X issues found. Please correct the following:" + clickable list of field labels
   - Behavior: When field label clicked, focus that field and scroll into view
   - ARIA: role="alert", aria-live="assertive" for immediate announcement
   - Only render if `Object.keys(errors).length > 0`

3. **Create useFormValidation Hook**
   - Signature: `useFormValidation<T>(validationRules: ValidationRules<T>, initialValues: T)`
   - State: `values`, `errors`, `touched`, `isSubmitting`
   - Methods: 
     - `handleChange(field, value)`: Update value, clear error if touched
     - `handleBlur(field)`: Mark field as touched, validate field
     - `validateField(field, value)`: Run validation rules, return error or undefined
     - `validateForm()`: Validate all fields, mark all as touched, return isValid
     - `resetForm()`: Clear values, errors, touched state
   - Focus Management: On submit with errors, focus first error field using `document.getElementById(firstErrorField)?.focus()`
   - Return: `{ values, errors, touched, isSubmitting, handleChange, handleBlur, validateForm, resetForm, setFieldError }`

4. **Extend validators.ts Utility**
   - Add validators: `validatePhone`, `validateDate`, `validateRequired`, `validateMinLength`, `validateMaxLength`, `validatePattern`
   - Phone validator: Check 10-digit format, return specific message "Phone number must be 10 digits"
   - Date validator: Check valid date format (YYYY-MM-DD), return "Please enter a valid date (MM/DD/YYYY)"
   - Required validator: Check non-empty, return "This field is required"
   - Pattern validator: Check regex match, return custom message
   - All validators return `string | undefined` (error message or undefined if valid)

5. **Create errorMessages.ts Centralized Messages**
   - Export const object with field-specific error templates
   - Examples:
     - `PHONE_INVALID: "Phone number must be 10 digits"`
     - `EMAIL_INVALID: "Please enter a valid email address"`
     - `PASSWORD_TOO_SHORT: "Password must be at least 8 characters"`
     - `DATE_INVALID: "Please enter a valid date (MM/DD/YYYY)"`
     - `REQUIRED: (fieldName: string) => \`${fieldName} is required\``
   - Avoid generic messages like "Invalid input"

6. **Refactor LoginForm.tsx**
   - Extract inline error rendering to use `<FieldError>` component
   - Replace `touched.email && errors.email` pattern with FieldError component
   - Current pattern already follows UXR-601 (errors below field, red text, specific messages)
   - Add aria-describedby linking: `<input aria-describedby="email-error" />` + `<FieldError id="email-error" />`

7. **Refactor RegistrationForm.tsx**
   - Implement useFormValidation hook with validation rules for all fields
   - Add FormErrorSummary at top of form (only shows when multiple errors exist)
   - Use FieldError for all form fields (name, email, password, phone, dateOfBirth)
   - Implement focus management: on submit with errors, focus first error field
   - Test with multiple simultaneous errors (leave all fields blank and submit)

8. **Implement Modal Error Handling (UserFormModal.tsx)**
   - Add useFormValidation hook for modal form fields
   - Use FieldError components within modal body
   - Add FormErrorSummary within modal (not outside)
   - **Critical**: Do NOT call `onClose()` on form validation errors
   - Only close modal on successful submission
   - Test: Submit modal form with errors → modal stays open, errors displayed

9. **Implement Modal Error Handling (WaitlistEnrollmentModal.tsx)**
   - Similar to UserFormModal pattern
   - Errors displayed within modal context
   - Modal remains open until successful submission

10. **Add Validation Tests**
    - Test: Single field error displays below field with red text and icon
    - Test: Multiple field errors display FormErrorSummary + individual FieldError components
    - Test: Focus shifts to first error field on submit
    - Test: Clicking field label in FormErrorSummary focuses that field
    - Test: Modal form errors display within modal, modal does not auto-close
    - Test: Error clears when user starts typing (immediate feedback)
    - Test: Error appears on blur after field is touched

## Current Project State
```
src/frontend/
├── src/
│   ├── components/
│   │   ├── common/
│   │   │   ├── [FieldError.tsx TO BE CREATED]
│   │   │   └── [FormErrorSummary.tsx TO BE CREATED]
│   │   └── waitlist/
│   │       └── WaitlistEnrollmentModal.tsx (TO REFACTOR)
│   ├── features/
│   │   ├── auth/
│   │   │   └── components/
│   │   │       ├── LoginForm.tsx (HAS INLINE VALIDATION - TO REFACTOR WITH FieldError)
│   │   │       └── RegistrationForm.tsx (TO IMPLEMENT VALIDATION)
│   │   └── admin/
│   │       └── components/
│   │           └── UserFormModal.tsx (HAS PARTIAL VALIDATION - TO ENHANCE)
│   ├── hooks/
│   │   └── [useFormValidation.ts TO BE CREATED]
│   ├── utils/
│   │   ├── validators.ts (EXISTS - TO EXTEND)
│   │   └── [errorMessages.ts TO BE CREATED]
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/common/FieldError.tsx | Reusable inline error component with red text, error icon, and ARIA alert |
| CREATE | src/frontend/src/components/common/FormErrorSummary.tsx | Multi-error summary box at top of form with clickable field list |
| CREATE | src/frontend/src/hooks/useFormValidation.ts | Custom form validation hook with touched state, focus management, field/form validation |
| CREATE | src/frontend/src/utils/errorMessages.ts | Centralized error message templates with specific field guidance |
| MODIFY | src/frontend/src/utils/validators.ts | Add phone, date, required, minLength, maxLength, pattern validators |
| MODIFY | src/frontend/src/features/auth/components/LoginForm.tsx | Extract error rendering to FieldError component, add aria-describedby (lines 187-191) |
| MODIFY | src/frontend/src/features/auth/components/RegistrationForm.tsx | Implement complete form validation with useFormValidation, FieldError, FormErrorSummary |
| MODIFY | src/frontend/src/features/admin/components/UserFormModal.tsx | Add validation with modal error handling, prevent auto-close on errors (lines 203-210) |
| MODIFY | src/frontend/src/components/waitlist/WaitlistEnrollmentModal.tsx | Add validation with modal error handling |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [ARIA Form Validation](https://www.w3.org/WAI/WCAG22/Techniques/aria/ARIA21) - Using aria-invalid and role="alert"
- [ARIA aria-describedby](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Attributes/aria-describedby) - Linking error messages to form fields
- [Inclusive Components: Form Validation](https://inclusive-components.design/a-todo-list/) - Accessible error patterns
- [NN Group: Error Message Guidelines](https://www.nngroup.com/articles/error-message-guidelines/) - Specific, actionable error messages
- [WCAG 2.2 Success Criterion 3.3.1: Error Identification](https://www.w3.org/WAI/WCAG22/Understanding/error-identification.html) - Error detection and description
- [React Hook Form Documentation](https://react-hook-form.com/) - Form validation library reference

## Build Commands
```bash
# Development
cd src/frontend
npm run dev

# Type checking
npm run type-check

# Linting
npm run lint

# Unit tests
npm run test

# Build production
npm run build
```

## Implementation Validation Strategy
- [x] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [x] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] **[AI Tasks]** Prompt templates validated with test inputs
- [ ] **[AI Tasks]** Guardrails tested for input sanitization and output validation
- [ ] **[AI Tasks]** Fallback logic tested with low-confidence/error scenarios
- [ ] **[AI Tasks]** Token budget enforcement verified
- [ ] **[AI Tasks]** Audit logging verified (no PII in logs)
- [ ] **[Mobile Tasks]** Headless platform compilation succeeds
- [ ] **[Mobile Tasks]** Native dependency linking verified
- [ ] **[Mobile Tasks]** Permission manifests validated against task requirements

### Custom Validation Criteria
- [ ] FieldError component displays with red text (text-error) and error icon
- [ ] Error messages are specific and actionable (not generic "Invalid input")
- [ ] FormErrorSummary appears only when multiple errors exist
- [ ] Focus shifts to first error field on form submit with errors
- [ ] Clicking field in FormErrorSummary focuses that field and scrolls into view
- [ ] Modal forms display errors within modal context
- [ ] Modals do NOT auto-close when validation errors occur
- [ ] Screen reader announces error messages via role="alert" and aria-live
- [ ] Errors clear immediately when user starts typing in touched field
- [ ] All form screens (SCR-001, SCR-002, SCR-007, SCR-009, SCR-011, SCR-013, SCR-018, SCR-022) follow consistent error pattern

## Implementation Checklist
- [ ] Create FieldError component with red text, error icon, role="alert", and aria-describedby linking
- [ ] Create FormErrorSummary component with error count, clickable field list, and focus management
- [ ] Create useFormValidation hook with values/errors/touched state, handleChange/handleBlur/validateForm methods
- [ ] Extend validators.ts with phone, date, required, minLength, maxLength, pattern validators
- [ ] Create errorMessages.ts with specific field-level error templates (avoid generic messages)
- [ ] Refactor LoginForm.tsx to use FieldError component (extract inline error rendering)
- [ ] Implement RegistrationForm.tsx validation with useFormValidation, FieldError, FormErrorSummary
- [ ] Refactor UserFormModal.tsx to display errors within modal, prevent auto-close on validation errors
- [ ] Refactor WaitlistEnrollmentModal.tsx with modal error handling pattern
- [ ] Add unit tests for useFormValidation hook (values, errors, touched, focus management)
- [ ] Add unit tests for validators (phone format, email format, required fields)
- [ ] Test multi-field error scenario: submit blank form → see FormErrorSummary + all FieldErrors + focus first field
- [ ] Test modal error scenario: submit modal form with errors → modal stays open, errors display within modal
- [ ] Validate ARIA announcements with screen reader (NVDA/JAWS/VoiceOver)
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
