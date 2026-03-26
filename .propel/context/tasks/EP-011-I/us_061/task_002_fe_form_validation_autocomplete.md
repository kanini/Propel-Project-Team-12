# Task - task_002_fe_form_validation_autocomplete

## Requirement Reference
- User Story: US_061 - Form Accessibility
- Story Location: .propel/context/tasks/EP-011-I/us_061/us_061.md
- Acceptance Criteria:
    - AC-2: **Given** inline validation, **When** a field fails validation, **Then** the error message is programmatically associated via `aria-describedby`, prefixed with an error icon, and announced by screen readers.
    - AC-4: **Given** autocomplete, **When** personal data fields render (name, email, phone, address), **Then** appropriate `autocomplete` attribute values are set per HTML5 specification to aid autofill.
- Edge Case:
    - Multi-select fields: Component follows WAI-ARIA listbox pattern with `aria-multiselectable="true"` and announces selection count changes.
    - Date picker fields: Provide keyboard-operable calendar widgets and manual text input fallback accepting standard date formats.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-001-registration.html, wireframe-SCR-007-appointment-booking.html, wireframe-SCR-013-manual-intake.html |
| **Screen Spec** | figma_spec.md#SCR-001, SCR-007, SCR-013 |
| **UXR Requirements** | UXR-203 (Accessible form controls and error association), UXR-601 (Inline validation errors) |
| **Design Tokens** | designsystem.md#semantic-colors (error colors) |

> **Wireframe Status Legend:**
> - **AVAILABLE**: Local file exists at specified path
> - **PENDING**: UI-impacting task awaiting wireframe (provide file or URL)
> - **EXTERNAL**: Wireframe provided via external URL
> - **N/A**: Task has no UI impact

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
| Frontend | React + TypeScript | React 18.x, TypeScript 5.x |
| Frontend | Redux Toolkit | 2.x |
| Frontend | Tailwind CSS | Latest |
| Library | React Hook Form | Latest (if used for validation) |
| Library | Zod or Yup | Latest (if used for schema validation) |
| Testing | Vitest + React Testing Library | Latest |

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

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview

Implement accessible form validation with programmatic error associations and HTML5 autocomplete attributes across all form components. This task ensures inline validation errors are properly associated with their inputs via `aria-describedby`, displayed with error icons, and announced by screen readers. Additionally, personal data fields (name, email, phone, address) include appropriate `autocomplete` attribute values to aid browser autofill and assistive technologies.

## Dependent Tasks
- task_001_fe_form_labels_required_fields (provides base form component structure)

## Impacted Components
- **Form Components**: FormField component (add error handling)
- **Page Forms**: Registration, Login, Appointment Booking, Manual Intake
- **Validation Logic**: Form validation hooks/utilities
- **Special Controls**: Multi-select components, date pickers

## Implementation Plan

### 1. Implement Error Message Association
- Update FormField component to accept `error` prop (error message string)
- Generate unique error message ID using field ID: `${fieldId}-error`
- Add `aria-describedby` attribute to input pointing to error message ID
- Add `aria-invalid="true"` to input when error is present
- Display error message below input with matching ID
- Style error message in error color from designsystem.md (text-red-600)
- Add error icon (⚠️ or similar) before error text with `aria-hidden="true"`

### 2. Implement Screen Reader Error Announcements
- Use ARIA live region for error announcements (from task_003 of US_060)
- When validation fails, announce error count: "[N] errors found"
- Focus first field with error (already implemented in US_060 task_003)
- Ensure error messages are descriptive and actionable
- Test with screen reader to verify announcements work correctly

### 3. Add HTML5 Autocomplete Attributes
- Reference [HTML5 Autocomplete Tokens](https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#autofill)
- Add autocomplete to Registration form fields:
  - First name: `autocomplete="given-name"`
  - Last name: `autocomplete="family-name"`
  - Email: `autocomplete="email"`
  - Phone: `autocomplete="tel"`
  - Date of birth: `autocomplete="bday"`
  - Address: `autocomplete="street-address"`, `autocomplete="address-level1"`, `autocomplete="address-level2"`, `autocomplete="postal-code"`
- Add autocomplete to Login form:
  - Email: `autocomplete="username"` or `autocomplete="email"`
  - Password: `autocomplete="current-password"` (login) or `autocomplete="new-password"` (registration)
- Document autocomplete values in form component documentation

### 4. Implement Date Picker Accessibility
- Use accessible date picker component (e.g., react-datepicker with accessibility enhancements)
- Provide keyboard navigation: Arrow keys to select date, Enter to confirm, Escape to close
- Provide manual text input fallback accepting formats: MM/DD/YYYY, YYYY-MM-DD
- Add `aria-label` or `aria-labelledby` to date picker button
- Add `aria-describedby` for format hint: "Format: MM/DD/YYYY"
- Test with keyboard only and screen reader

### 5. Implement Multi-Select Accessibility (Edge Case)
- If multi-select components exist (e.g., specialty selection, preferences)
- Follow WAI-ARIA Listbox pattern with multi-select:
  - Add `role="listbox"` to container
  - Add `aria-multiselectable="true"`
  - Add `role="option"` to each item
  - Add `aria-selected="true"` to selected items
- Announce selection count changes via ARIA live region: "[N] items selected"
- Keyboard support: Space to toggle selection, Arrow keys to navigate
- Visual indication of selected items (checkboxes or background color)

### 6. Update Form Validation Logic
- Ensure validation runs on blur (when user leaves field)
- Optionally validate on change for real-time feedback (after first blur)
- Clear error messages when user corrects input
- Display error summary at top of form with link to first error (optional)
- Use consistent error message patterns (e.g., "This field is required", "Please enter a valid email address")

### 7. Create Automated Tests
- Test error message association: Assert `aria-describedby` links to error ID
- Test `aria-invalid` attribute appears when validation fails
- Test error icon is visually present but hidden from screen readers
- Test autocomplete attributes present on appropriate fields
- Test date picker keyboard navigation
- Test multi-select announces selection count changes

### 8. Document Validation Patterns
- Create validation error message guidelines
- Document autocomplete attribute usage
- Document accessible date picker integration
- Document multi-select accessibility pattern
- Provide code examples for common validation scenarios

## Current Project State

```
src/frontend/src/
├── components/
│   └── forms/
│       ├── FormField.tsx (to be updated with error handling)
│       └── PasswordStrengthIndicator.tsx
├── features/
│   └── auth/
│       └── pages/
│           ├── LoginPage.tsx (needs validation + autocomplete)
│           └── RegisterPage.tsx (needs validation + autocomplete)
├── pages/
│   └── AppointmentBooking.tsx (needs validation)
├── utils/
│   └── validation/ (potential location for validation helpers)
└── __tests__/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/components/forms/FormField.tsx | Add error handling with aria-describedby and aria-invalid |
| CREATE | src/components/forms/DatePicker.tsx | Accessible date picker component (or enhance existing) |
| CREATE | src/components/forms/MultiSelect.tsx | Accessible multi-select component following listbox pattern |
| MODIFY | src/features/auth/pages/RegisterPage.tsx | Add validation logic and autocomplete attributes |
| MODIFY | src/features/auth/pages/LoginPage.tsx | Add validation logic and autocomplete attributes |
| MODIFY | src/pages/AppointmentBooking.tsx | Add validation logic |
| CREATE | src/utils/validation/formValidation.ts | Reusable validation utilities |
| CREATE | src/__tests__/components/forms/FormFieldError.test.tsx | Error association tests |
| CREATE | docs/FORM_VALIDATION_PATTERNS.md | Validation accessibility guidelines |

## External References

### WCAG 2.2 Standards
- [Error Identification (3.3.1)](https://www.w3.org/WAI/WCAG22/Understanding/error-identification.html)
- [Labels or Instructions (3.3.2)](https://www.w3.org/WAI/WCAG22/Understanding/labels-or-instructions.html)
- [Error Suggestion (3.3.3)](https://www.w3.org/WAI/WCAG22/Understanding/error-suggestion.html)
- [Error Prevention (3.3.4)](https://www.w3.org/WAI/WCAG22/Understanding/error-prevention-legal-financial-data.html)

### ARIA Attributes
- [aria-describedby](https://www.w3.org/TR/wai-aria-1.2/#aria-describedby)
- [aria-invalid](https://www.w3.org/TR/wai-aria-1.2/#aria-invalid)
- [aria-multiselectable](https://www.w3.org/TR/wai-aria-1.2/#aria-multiselectable)

### HTML5 Autocomplete
- [HTML5 Autocomplete Specification](https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#autofill)
- [MDN: autocomplete attribute](https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/autocomplete)
- [Google: Autofill Best Practices](https://web.dev/learn/forms/autofill/)

### WAI-ARIA Patterns
- [Listbox Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/listbox/)
- [Date Picker Dialog Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/examples/datepicker-dialog/)

### React Form Libraries
- [React Hook Form: Error Handling](https://react-hook-form.com/get-started#Handleerrors)
- [react-datepicker](https://reactdatepicker.com/) (one option for accessible date picker)

### Testing
- [Testing Library: Form Validation Testing](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library#not-using-waitfor)
- [Testing Library: User Event](https://testing-library.com/docs/user-event/intro/)

### Project Documentation
- [Web Accessibility Standards Rule](.propel/rules/web-accessibility-standards.md)
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)

## Build Commands

```bash
# Navigate to frontend directory
cd src/frontend

# Install dependencies (if adding date picker library)
npm install react-datepicker
npm install -D @types/react-datepicker

# Run type checking
npm run typecheck

# Run linting
npm run lint

# Run unit tests
npm test

# Run validation tests
npm test -- --grep="validation"

# Build for production
npm run build

# Development server (manual testing)
npm run dev
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Error messages properly associated via aria-describedby
- [ ] aria-invalid attribute present on fields with errors
- [ ] Error icons displayed with aria-hidden="true"
- [ ] Autocomplete attributes present on all personal data fields
- [ ] Date picker keyboard-operable with text fallback
- [ ] Multi-select follows listbox pattern with selection announcements
- [ ] Screen reader announces validation errors correctly
- [ ] All validation error messages are descriptive and actionable

## Implementation Checklist
- [ ] Update FormField component to accept `error` prop
- [ ] Generate unique error message IDs: `${fieldId}-error`
- [ ] Add `aria-describedby` to inputs linking to error message ID
- [ ] Add `aria-invalid="true"` to inputs when error is present
- [ ] Display error messages below inputs with matching IDs
- [ ] Add error icon (⚠️) before error text with `aria-hidden="true"`
- [ ] Style error messages in error color from designsystem.md
- [ ] Add HTML5 autocomplete attributes to Registration form (given-name, family-name, email, tel, bday)
- [ ] Add HTML5 autocomplete to Login form (username/email, current-password)
- [ ] Add autocomplete to address fields (street-address, address-level1, address-level2, postal-code)
- [ ] Implement or integrate accessible date picker component
- [ ] Add keyboard navigation to date picker (Arrow keys, Enter, Escape)
- [ ] Provide manual text input fallback for date picker (MM/DD/YYYY, YYYY-MM-DD)
- [ ] Implement multi-select component with aria-multiselectable and listbox pattern (if needed)
- [ ] Announce selection count changes via ARIA live region for multi-selects
- [ ] Test error association with accessibility tools
- [ ] Test with screen reader to verify error announcements (NVDA/VoiceOver)
- [ ] Test autocomplete attributes work with browser autofill
- [ ] Test date picker with keyboard only
- [ ] Create developer documentation for validation patterns
