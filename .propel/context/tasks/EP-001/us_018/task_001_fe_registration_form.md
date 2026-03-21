# Task - TASK_001

## Requirement Reference
- User Story: US_018
- Story Location: .propel/context/tasks/EP-001/us_018/us_018.md
- Acceptance Criteria:
    - AC1: System creates account with status "pending" and sends verification email within 2 minutes when valid information is entered
    - AC4: Inline validation displays specific missing password requirements below password field
- Edge Case:
    - Date of birth validation rejects future dates with clear error message

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-001-registration.html |
| **Screen Spec** | figma_spec.md#SCR-001 |
| **UXR Requirements** | UXR-101 (Progress indicators), UXR-201 (WCAG 2.2 AA), UXR-203 (Form labels), UXR-601 (Inline validation) |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing |

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
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | TypeScript | 5.x |
| Library | Tailwind CSS | Latest |
| Library | Redux Toolkit | 2.x |
| AI/ML | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

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
Implement the patient registration form UI component that enables new patients to create accounts with email validation, capturing name, date of birth, contact information, and password with real-time validation. This component is the entry point for new users to access the platform and must provide clear inline error messages and accessibility compliance per WCAG 2.2 AA standards.

## Dependent Tasks
- None (This is an initial task for the EP-001 epic)

## Impacted Components
- NEW: src/frontend/src/features/auth/pages/RegisterPage.tsx
- NEW: src/frontend/src/features/auth/components/RegistrationForm.tsx
- NEW: src/frontend/src/components/forms/PasswordStrengthIndicator.tsx
- NEW: src/frontend/src/features/auth/authSlice.ts (registration state management)

## Implementation Plan
1. Create RegisterPage.tsx as the main registration page component
2. Build RegistrationForm.tsx with TextField components for name, DOB, email, phone, password
3. Implement PasswordStrengthIndicator.tsx showing real-time password validation (8+ chars, uppercase, number, special char)
4. Add inline validation logic using React Hook Form or similar for form state management
5. Implement date picker or date input for DOB with future date rejection
6. Create Redux slice for managing registration state (loading, success, error)
7. Style components using Tailwind CSS classes aligned with design tokens from designsystem.md
8. Ensure all form inputs have proper labels and ARIA attributes for accessibility

## Current Project State
```
src/frontend/src/
├── features/
│   └── auth/
│       ├── components/ (to be created)
│       ├── pages/ (to be created)
│       └── authSlice.ts (to be created)
├── components/
│   └── forms/ (to be created)
├── store/
│   └── index.ts (existing)
└── utils/
    └── validators.ts (existing)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/features/auth/pages/RegisterPage.tsx | Main registration page route component |
| CREATE | src/frontend/src/features/auth/components/RegistrationForm.tsx | Registration form with 5 input fields and submit button |
| CREATE | src/frontend/src/components/forms/PasswordStrengthIndicator.tsx | Real-time password strength indicator component |
| CREATE | src/frontend/src/features/auth/authSlice.ts | Redux slice for auth state (registration, login, session) |
| MODIFY | src/frontend/src/store/index.ts | Add authSlice reducer to store configuration |
| MODIFY | src/frontend/src/utils/validators.ts | Add email, phone, DOB, password validation functions |

## External References
- **React Hook Form**: https://react-hook-form.com/get-started (v7)
- **Tailwind CSS Forms**: https://tailwindcss.com/docs/forms
- **WCAG 2.2 Form Guidelines**: https://www.w3.org/WAI/WCAG22/quickref/#input-purposes
- **Date Input Accessibility**: https://www.w3.org/WAI/ARIA/apg/patterns/date-picker/
- **Redux Toolkit Async Thunks**: https://redux-toolkit.js.org/api/createAsyncThunk

## Build Commands
```powershell
# Navigate to frontend directory
cd src/frontend

# Install dependencies (if needed)
npm install

# Run development server
npm run dev

# Run linting
npm run lint

# Run type checking
npx tsc --noEmit

# Run component tests
npm run test
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [x] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [x] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] **[AI Tasks]** Prompt templates validated with test inputs
- [ ] **[AI Tasks]** Guardrails tested for input sanitization and output validation
- [ ] **[AI Tasks]** Fallback logic tested with low-confidence/error scenarios
- [ ] **[AI Tasks]** Token budget enforcement verified
- [ ] **[AI Tasks]** Audit logging verified (no PII in logs)
- [ ] **[Mobile Tasks]** Headless platform compilation succeeds
- [ ] **[Mobile Tasks]** Native dependency linking verified
- [ ] **[Mobile Tasks]** Permission manifests validated against task requirements

## Implementation Checklist
- [x] Create RegisterPage.tsx with route configuration
- [x] Implement RegistrationForm component with 5 TextField components (name, DOB, email, phone, password)
- [x] Build PasswordStrengthIndicator with visual feedback (color-coded: red→yellow→green)
- [x] Implement inline validation for each field (blur and submit events)
- [x] Add date picker with future date rejection for DOB field
- [x] Create authSlice.ts with registerUser async thunk
- [x] Style form using Tailwind CSS tokens from designsystem.md (colors, spacing, typography)
- [x] Add ARIA labels and error descriptions for accessibility (UXR-203)
- [x] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [x] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
