# Task - task_001_fe_registration_ui

## Requirement Reference
- User Story: us_018
- Story Location: .propel/context/tasks/EP-001/us_018/us_018.md
- Acceptance Criteria:
    - AC1: Enter valid personal information (name, date of birth, email, phone, password) -> System creates account with status "pending" and sends verification email within 2 minutes
    - AC2: Password does not meet security requirements (8+ characters, 1 uppercase, 1 number, 1 special character) -> Inline validation displays specific missing requirements below the password field
    - AC3: Email that is already registered -> System displays error message and offers password recovery option without revealing whether email exists
    - AC4: Rate limiting allows max 3 verification email requests per 5 minutes
- Edge Case:
    - Date of birth future date -> Inline validation rejects with clear error message

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-001-registration.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-001 |
| **UXR Requirements** | UXR-001, UXR-101, UXR-201, UXR-203, UXR-301, UXR-601 |
| **Design Tokens** | designsystem.md#form-components, designsystem.md#typography, designsystem.md#colors |

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
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | React Router | v7 |
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
Build patient registration UI form (SCR-001) with multi-step progress indication, inline validation for password complexity and date of birth, email uniqueness check, and responsive design compliant with WCAG 2.2 Level AA accessibility standards. Form integrates with backend registration API and handles verification email flow.

## Dependent Tasks
- None (foundational task)

## Impacted Components
- **NEW**: src/frontend/src/pages/Registration/RegistrationPage.tsx
- **NEW**: src/frontend/src/components/forms/RegistrationForm.tsx
- **NEW**: src/frontend/src/store/slices/authSlice.ts
- **NEW**: src/frontend/src/utils/validators/registrationValidators.ts
- **NEW**: src/frontend/src/types/auth.types.ts

## Implementation Plan
1. Create type definitions for registration data (name, DOB, email, phone, password, confirmation)
2. Build RegistrationForm component with controlled inputs and local validation state
3. Implement real-time password strength validator (8+ chars, 1 uppercase, 1 number, 1 special char)
4. Implement date validation preventing future dates
5. Create Redux authSlice with registration async thunk calling POST /api/auth/register
6. Handle API responses: success redirect to verification notice, error display (email exists, rate limit)
7. Implement progress indicator showing current step
8. Apply Tailwind styling per wireframe with proper focus indicators and ARIA labels
9. Ensure responsive layout (mobile 320px+, tablet 768px+, desktop 1024px+)

## Current Project State
```
src/frontend/src/
├── pages/
│   └── README.md (placeholder)
├── components/
│   └── README.md (placeholder)
├── store/
│   ├── hooks.ts
│   ├── index.ts
│   ├── rootReducer.ts
│   └── slices/
├── types/
├── utils/
└── App.tsx
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/pages/Registration/RegistrationPage.tsx | Main registration page component with routing |
| CREATE | src/frontend/src/components/forms/RegistrationForm.tsx | Registration form with validation and submission logic |
| CREATE | src/frontend/src/components/forms/PasswordStrengthIndicator.tsx | Visual password strength meter |
| CREATE | src/frontend/src/store/slices/authSlice.ts | Redux slice for auth state (register, login, logout) |
| CREATE | src/frontend/src/types/auth.types.ts | TypeScript interfaces for auth data models |
| CREATE | src/frontend/src/utils/validators/registrationValidators.ts | Validation functions for registration fields |
| CREATE | src/frontend/src/utils/validators/passwordValidator.ts | Password complexity validation logic |
| CREATE | src/frontend/src/utils/validators/dateValidator.ts | Date of birth validation logic |
| MODIFY | src/frontend/src/App.tsx | Add /register route to router configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [WCAG 2.2 Form Input Guidelines](https://www.w3.org/WAI/WCAG22/quickref/?showtechniques=332#input-purposes)
- [React Hook Form Documentation](https://react-hook-form.com/)
- [Redux Toolkit Async Thunks](https://redux-toolkit.js.org/api/createAsyncThunk)
- [Tailwind CSS Form Plugin](https://github.com/tailwindlabs/tailwindcss-forms)

## Build Commands
```bash
# Install dependencies
cd src/frontend
npm install

# Run development server
npm run dev

# Run tests
npm test

# Build for production
npm run build
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
- [ ] Create auth.types.ts with RegistrationRequest, RegistrationResponse, ValidationError interfaces
- [ ] Implement passwordValidator.ts with regex checks for complexity requirements
- [ ] Implement dateValidator.ts preventing future dates and invalid date formats
- [ ] Create registrationValidators.ts aggregating all field validators
- [ ] Build authSlice.ts with registerUser async thunk and loading/error state
- [ ] Create PasswordStrengthIndicator component with color-coded strength levels (red/yellow/green)
- [ ] Build RegistrationForm component with controlled inputs and onChange validation
- [ ] Display inline validation errors below each field (UXR-601)
- [ ] Implement progress indicator for multi-step flow (UXR-101)
- [ ] Add "Already have account? Login" link
- [ ] Style with Tailwind CSS matching wireframe (colors, spacing, typography)
- [ ] Ensure ARIA labels, roles, and error announcements (UXR-201, UXR-203)
- [ ] Add visible focus indicators with 3:1 contrast ratio (UXR-202)
- [ ] Test keyboard-only navigation (UXR-205)
- [ ] Implement responsive breakpoints: mobile (<768px), tablet (768-1023px), desktop (1024px+)
- [ ] Add RegistrationPage routing to App.tsx
- [ ] Test error scenarios: email exists, rate limit exceeded, API failure
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
