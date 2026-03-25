# Task - task_001_fe_form_labels_required_fields

## Requirement Reference
- User Story: US_061 - Form Accessibility
- Story Location: .propel/context/tasks/EP-011-I/us_061/us_061.md
- Acceptance Criteria:
    - AC-1: **Given** form accessibility (UXR-203), **When** a form renders, **Then** every input has a visible `<label>` element with a matching `for` attribute, and group labels use `<fieldset>` and `<legend>`.
    - AC-3: **Given** required fields, **When** a form displays, **Then** required fields show both a visual asterisk indicator and `aria-required="true"` attribute.
- Edge Case:
    - None specific to this task

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-001-registration.html, wireframe-SCR-007-appointment-booking.html, wireframe-SCR-013-manual-intake.html |
| **Screen Spec** | figma_spec.md#SCR-001, SCR-007, SCR-013 |
| **UXR Requirements** | UXR-203 (Accessible form controls and error association) |
| **Design Tokens** | designsystem.md#accessibility-requirements |

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
| Library | React Hook Form | Latest (if used for form management) |
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

Implement proper form label associations and required field indicators across all form components to meet WCAG 2.2 Level AA standards. This task ensures every form input has a visible `<label>` element with a matching `for` attribute, group inputs use `<fieldset>` and `<legend>` for proper semantic grouping, and required fields display both visual asterisk indicators and `aria-required="true"` attributes for assistive technology users.

## Dependent Tasks
- None

## Impacted Components
- **Form Components**: All input components in src/components/forms/
- **Page Forms**: Registration, Login, Appointment Booking, Manual Intake, Walk-in Booking
- **Input Components**: TextField, Select, Textarea, Checkbox, Radio, DatePicker
- **Fieldset Components**: RadioGroup, CheckboxGroup (create if missing)

## Implementation Plan

### 1. Audit Existing Form Components
- Identify all form pages: Registration (SCR-001), Login (SCR-002), Appointment Booking (SCR-007), Manual Intake (SCR-013), Walk-in Booking (SCR-018)
- Document current label association patterns
- Identify inputs without proper label associations
- Identify checkbox/radio groups needing fieldset/legend
- Document required field indicator patterns (if any exist)

### 2. Create Reusable Form Input Components
- Create/update `FormField` wrapper component accepting:
  - `label`: Label text (required)
  - `htmlFor`: ID of input element (required)
  - `required`: Boolean for required field indicator
  - `helpText`: Optional helper text
  - `error`: Optional error message
- Create `FormFieldset` component for grouping related inputs:
  - `legend`: Group label (required)
  - `required`: Boolean for required indicator on legend
  - `children`: Form inputs
- Ensure proper TypeScript types for all props

### 3. Implement Label-Input Associations
- Update all input components to accept `id` prop
- Ensure `<label>` uses `htmlFor` attribute matching input `id`
- Generate unique IDs using `useId()` React hook or custom hook
- Position labels appropriately (above input for text fields, beside for checkbox/radio)
- Ensure label text is visible and not hidden (no display:none or visibility:hidden)
- Style labels with appropriate typography from designsystem.md

### 4. Implement Fieldset/Legend for Input Groups
- Identify radio button groups (e.g., gender selection, appointment type)
- Identify checkbox groups (e.g., multi-select preferences, appointment reminders)
- Wrap groups in `<fieldset>` element
- Add `<legend>` element as first child with group label
- Style fieldset border (typically removed or minimal) from designsystem.md
- Style legend with appropriate typography

### 5. Implement Required Field Indicators
- Add visual asterisk (*) next to required field labels
- Style asterisk in error color (e.g., text-red-600) from designsystem.md
- Add `aria-required="true"` attribute to required input elements
- Consider screen reader-friendly text: "Required field" or "(required)" in label
- Document pattern: `<label>Name <span className="text-red-600" aria-hidden="true">*</span></label>`
- Add global required field explanation text at top of forms: "Fields marked with * are required"

### 6. Update All Form Pages
- **Registration Page (SCR-001)**: Update all fields with proper labels, mark required fields
- **Login Page (SCR-002)**: Update email/password fields (likely both required)
- **Appointment Booking (SCR-007)**: Update provider selection, date, time, reason fields
- **Manual Intake (SCR-013)**: Update all health history fields, use fieldsets for grouped questions
- **Walk-in Booking (SCR-018)**: Similar to appointment booking with patient search

### 7. Create Automated Tests
- Test label-input association: Assert label `htmlFor` matches input `id`
- Test required field indicators: Assert `aria-required="true"` and visual asterisk present
- Test fieldset/legend: Assert proper semantic structure for grouped inputs
- Test screen reader announcements for required fields
- Use `getByLabelText()` query to verify label associations work correctly

### 8. Document Form Accessibility Patterns
- Create developer guidelines for form component usage
- Document proper label association patterns
- Document fieldset/legend usage for groups
- Document required field indicator pattern
- Provide code examples for common form patterns

## Current Project State

```
src/frontend/src/
├── components/
│   └── forms/
│       └── PasswordStrengthIndicator.tsx (existing form component)
├── features/
│   └── auth/
│       └── pages/
│           ├── LoginPage.tsx (needs label updates)
│           └── RegisterPage.tsx (needs label/required updates)
├── pages/
│   ├── AppointmentBooking.tsx (needs form accessibility)
│   └── (other form pages)
└── __tests__/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/forms/FormField.tsx | Reusable form field component with label and required indicator |
| CREATE | src/components/forms/FormFieldset.tsx | Fieldset/legend wrapper for input groups |
| CREATE | src/hooks/useFormFieldId.ts | Custom hook for generating unique field IDs |
| MODIFY | src/features/auth/pages/RegisterPage.tsx | Add proper labels and required indicators |
| MODIFY | src/features/auth/pages/LoginPage.tsx | Add proper labels and required indicators |
| MODIFY | src/pages/AppointmentBooking.tsx | Add proper labels and fieldsets |
| MODIFY | src/components/forms/PasswordStrengthIndicator.tsx | Ensure proper label association |
| CREATE | src/__tests__/components/forms/FormField.test.tsx | Form field accessibility tests |
| CREATE | docs/FORM_ACCESSIBILITY_PATTERNS.md | Developer guidelines for accessible forms |

## External References

### WCAG 2.2 Standards
- [Labels or Instructions (3.3.2)](https://www.w3.org/WAI/WCAG22/Understanding/labels-or-instructions.html)
- [Info and Relationships (1.3.1)](https://www.w3.org/WAI/WCAG22/Understanding/info-and-relationships.html)

### HTML Form Accessibility
- [MDN: label element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/label)
- [MDN: fieldset element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/fieldset)
- [MDN: legend element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/legend)
- [WebAIM: Creating Accessible Forms](https://webaim.org/techniques/forms/)

### ARIA Attributes
- [aria-required](https://www.w3.org/TR/wai-aria-1.2/#aria-required)
- [aria-labelledby](https://www.w3.org/TR/wai-aria-1.2/#aria-labelledby)

### React Form Libraries
- [React Hook Form](https://react-hook-form.com/) (if used)
- [React useId Hook](https://react.dev/reference/react/useId)

### Testing
- [Testing Library: getByLabelText](https://testing-library.com/docs/queries/bylabeltext/)
- [Testing Library: Form Testing](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library#not-using-screen)

### Project Documentation
- [Web Accessibility Standards Rule](.propel/rules/web-accessibility-standards.md)
- [Frontend Development Standards](.propel/rules/frontend-development-standards.md)

## Build Commands

```bash
# Navigate to frontend directory
cd src/frontend

# Install dependencies
npm install

# Run type checking
npm run typecheck

# Run linting
npm run lint

# Run unit tests
npm test

# Run form accessibility tests
npm test -- --grep="form"

# Build for production
npm run build

# Development server (manual testing)
npm run dev
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] All form inputs have visible labels with matching `for` attribute
- [ ] All required fields have visual asterisk and aria-required="true"
- [ ] Input groups use fieldset and legend elements
- [ ] getByLabelText queries work for all form inputs
- [ ] Screen reader announces required field status
- [ ] All form pages updated with proper label associations

## Implementation Checklist
- [ ] Audit all form pages and document current label/required patterns
- [ ] Create `FormField` wrapper component with label, htmlFor, required, error props
- [ ] Create `FormFieldset` component for grouping inputs with legend
- [ ] Create `useFormFieldId` hook for generating unique IDs (or use React.useId())
- [ ] Update Registration page fields with proper labels and required indicators
- [ ] Update Login page fields with proper labels and required indicators (email, password)
- [ ] Update Appointment Booking form with labels and fieldsets
- [ ] Update Manual Intake form with labels and fieldsets for grouped questions
- [ ] Add visual asterisk indicator to all required fields (styled from designsystem.md)
- [ ] Add `aria-required="true"` to all required input elements
- [ ] Add global "Fields marked with * are required" text at top of all forms
- [ ] Ensure all labels are visible (no display:none or visibility:hidden)
- [ ] Test label-input associations using getByLabelText queries
- [ ] Test fieldset/legend semantic structure with accessibility tools
- [ ] Test with screen reader to verify required fields are announced (NVDA/VoiceOver)
- [ ] Create developer documentation for form accessibility patterns
