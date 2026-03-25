# Task - task_001_fe_manual_intake_form

## Requirement Reference

- User Story: us_034
- Story Location: .propel/context/tasks/EP-004/us_034/us_034.md
- Acceptance Criteria:
  - AC-1: When patient selects "Manual Form" on the intake page, a structured intake form displays with sections for medical history, current medications, allergies, visit concerns, and insurance information
  - AC-2: When patient completes required fields and submits, all intake data is saved as an IntakeRecord linked to appointment with mode set to "Manual"
  - AC-3: When required fields are blank or data is invalid, inline validation errors display below the corresponding fields with specific correction instructions
  - AC-4: When hovering over or focusing on fields with medical terms, contextual help tooltips explain the terminology in plain language (UXR-103)
- Edge Cases:
  - Form submission fails mid-save: Entered data is preserved in browser local storage and restored on retry
  - Extremely long text in free-text fields: Character limits enforced (2000 characters for visit concerns) with visible counter

## Design References (Frontend Tasks Only)

| Reference Type         | Value                                                                       |
| ---------------------- | --------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                         |
| **Figma URL**          | N/A                                                                         |
| **Wireframe Status**   | AVAILABLE                                                                   |
| **Wireframe Type**     | HTML                                                                        |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-013-manual-intake.html       |
| **Screen Spec**        | figma_spec.md#SCR-013                                                       |
| **UXR Requirements**   | UXR-101, UXR-103, UXR-203, UXR-601                                          |
| **Design Tokens**      | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**

**IF Wireframe Status = AVAILABLE:**

- **MUST** open and reference the wireframe file during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, loading, error, validation)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px
- Run `/analyze-ux` after implementation to verify pixel-perfect alignment

## Applicable Technology Stack

| Layer    | Technology                                        | Version                                       |
| -------- | ------------------------------------------------- | --------------------------------------------- |
| Frontend | React + TypeScript + Redux Toolkit + Tailwind CSS | React 18.x, TypeScript 5.x, Redux Toolkit 2.x |
| Backend  | .NET 8 ASP.NET Core Web API                       | .NET 8.0                                      |
| Library  | React Router                                      | v7                                            |
| AI/ML    | N/A                                               | N/A                                           |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)

| Reference Type           | Value |
| ------------------------ | ----- |
| **AI Impact**            | No    |
| **AIR Requirements**     | N/A   |
| **AI Pattern**           | N/A   |
| **Prompt Template Path** | N/A   |
| **Guardrails Config**    | N/A   |
| **Model Provider**       | N/A   |

## Mobile References (Mobile Tasks Only)

| Reference Type       | Value |
| -------------------- | ----- |
| **Mobile Impact**    | No    |
| **Platform Target**  | N/A   |
| **Min OS Version**   | N/A   |
| **Mobile Framework** | N/A   |

## Task Overview

Implement the Manual Intake Form UI for SCR-013. This task builds a multi-step structured form where patients enter health information through familiar form controls (text fields, selects, checkboxes, radios). The form follows a stepper pattern with four sections: Demographics, Medical History, Insurance, and Review. Each section is rendered as a card with inline field validation (UXR-601), contextual help tooltips for medical terminology (UXR-103), programmatically associated labels (UXR-203), and a progress bar (UXR-101). Form data is auto-saved to localStorage on each section change to prevent data loss. On final submission, the form dispatches a Redux thunk that calls `POST /api/intake/{id}/submit` to persist the IntakeRecord with mode "ManualForm".

## Dependent Tasks

- EP-004/us_037/task_001_fe_appointment_selection_ui — Provides appointment selection UI; user must select appointment before accessing manual intake
- EP-004/us_037/task_002_be_appointment_selection_api — Backend API for fetching appointments requiring intake
- EP-004/us_033/task_001_fe_ai_intake_ui — Provides shared `IntakePage` with mode toggle, `intakeSlice` Redux state, `intakeApi` client, and `types/intake.ts` type definitions. This task adds the manual form branch to the existing IntakePage.

## Impacted Components

- **NEW** `src/frontend/src/features/intake/components/ManualIntakeForm.tsx` — Multi-step form container with stepper navigation
- **NEW** `src/frontend/src/features/intake/components/MedicalHistorySection.tsx` — Medical history, medications, allergies, surgical history, family history, lifestyle
- **NEW** `src/frontend/src/features/intake/components/InsuranceSection.tsx` — Insurance provider name, ID, and pre-check validation
- **NEW** `src/frontend/src/features/intake/components/ReviewSection.tsx` — Read-only summary of all entered data with edit links per section
- **NEW** `src/frontend/src/features/intake/components/FormTooltip.tsx` — Reusable tooltip component for clinical terminology help
- **NEW** `src/frontend/src/features/intake/components/FormStepper.tsx` — Step indicator with completed/active/pending states
- **NEW** `src/frontend/src/features/intake/hooks/useFormPersistence.ts` — Custom hook for localStorage auto-save and restore
- **MODIFY** `src/frontend/src/features/intake/pages/IntakePage.tsx` — Add manual form mode branch (conditional rendering based on toggle state)
- **MODIFY** `src/frontend/src/store/slices/intakeSlice.ts` — Add `submitManualIntake` async thunk and manual form state fields
- **MODIFY** `src/frontend/src/api/intakeApi.ts` — Add `submitManualIntake` API function
- **MODIFY** `src/frontend/src/types/intake.ts` — Add `ManualIntakeFormData` interface with section types

## Implementation Plan

1. **Extend TypeScript types** (`types/intake.ts`): Add `ManualIntakeFormData` interface with sub-types for each form section — `MedicalHistoryData` (conditions, medications, allergies, surgicalHistory, familyHistory checkboxes, lifestyle radios), `InsuranceData` (providerName, insuranceId), and `VisitConcernsData` (chiefComplaint, additionalConcerns). Add `FormValidationErrors` type mapping field names to error messages.

2. **Extend API client** (`api/intakeApi.ts`): Add `submitManualIntake(sessionId, formData: ManualIntakeFormData)` function calling `POST /api/intake/{id}/submit` with the full structured form body. Follow existing pattern from `providerApi.ts`.

3. **Extend Redux slice** (`store/slices/intakeSlice.ts`): Add `formData` field to state holding partial `ManualIntakeFormData`. Add `submitManualIntake` async thunk. Add `setFormSection` reducer for updating individual sections. Add `currentStep` (0-3) for stepper tracking.

4. **Build FormStepper component**: Render 4-step horizontal stepper matching wireframe (Demographics, Medical History, Insurance, Review). Use completed/active/pending dot states with checkmark for completed. Include `role="navigation"` and `aria-label="Form sections"` (UXR-101).

5. **Build FormTooltip component**: Reusable tooltip that appears on hover/focus for fields with clinical terminology. Renders a small info icon next to the label; on hover/focus, shows a floating tooltip with plain-language explanation. Use `aria-describedby` linking tooltip to the field (UXR-103).

6. **Build MedicalHistorySection component**: Card-based form matching wireframe layout — textareas for conditions and medications, text input for allergies, textarea for surgical history, checkbox group for family history, radio groups for lifestyle (smoking, alcohol). All required fields marked with red asterisk. Inline validation errors below fields (UXR-601). Character counter on textareas (max 2000). All inputs have associated `<label>` elements with `htmlFor` (UXR-203).

7. **Build InsuranceSection component**: Form with insurance provider name (text input), insurance ID (text input), and a "Verify Insurance" button that calls the backend pre-check endpoint. Display validation result inline (valid/invalid/not found).

8. **Build ReviewSection component**: Read-only summary of all entered data organized by section. Each section heading includes an "Edit" link that navigates back to that step. "Submit" button triggers final `submitManualIntake` thunk.

9. **Build ManualIntakeForm container**: Orchestrate stepper navigation between the 4 sections. Track `currentStep` in Redux. "Next"/"Back" buttons navigate between steps. "Save draft" button persists to localStorage. Validate current section before allowing "Next". Render the active section component conditionally.

10. **Build useFormPersistence hook**: On each section change or unmount, serialize current `formData` to `localStorage` keyed by `intake-draft-{appointmentId}`. On mount, check for existing draft and hydrate Redux state. Clear draft on successful submit.

11. **Wire into IntakePage**: In `IntakePage.tsx`, conditionally render `ManualIntakeForm` when mode toggle is set to manual (or when navigated via `/intake?mode=manual`). Preserve entered data when switching modes (UXR-102) by keeping Redux state intact.

## Current Project State

```
src/frontend/src/
├── api/
│   ├── providerApi.ts
│   ├── staffApi.ts
│   └── intakeApi.ts             # Created in US_033 task_001
├── features/
│   └── intake/
│       ├── components/
│       │   ├── ConversationalIntake.tsx   # US_033
│       │   ├── ChatBubble.tsx            # US_033
│       │   ├── TypingIndicator.tsx       # US_033
│       │   └── IntakeSummary.tsx         # US_033
│       └── pages/
│           └── IntakePage.tsx            # US_033 (mode toggle exists)
├── store/
│   ├── index.ts
│   ├── rootReducer.ts                   # intakeReducer registered by US_033
│   └── slices/
│       ├── intakeSlice.ts               # US_033 (AI chat state)
│       ├── appointmentSlice.ts
│       ├── providerSlice.ts
│       └── waitlistSlice.ts
├── types/
│   └── intake.ts                        # US_033 (session, messages, extracted data)
├── App.tsx                              # /intake route wired to IntakePage by US_033
└── ...
```

## Expected Changes

| Action | File Path                                                             | Description                                                                    |
| ------ | --------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| CREATE | src/frontend/src/features/intake/components/ManualIntakeForm.tsx      | Multi-step form container with stepper and section navigation                  |
| CREATE | src/frontend/src/features/intake/components/MedicalHistorySection.tsx | Medical history, meds, allergies, family history, lifestyle fields             |
| CREATE | src/frontend/src/features/intake/components/InsuranceSection.tsx      | Insurance provider/ID fields with pre-check validation                         |
| CREATE | src/frontend/src/features/intake/components/ReviewSection.tsx         | Read-only summary of all form data with edit and submit actions                |
| CREATE | src/frontend/src/features/intake/components/FormTooltip.tsx           | Reusable tooltip for clinical terminology help (UXR-103)                       |
| CREATE | src/frontend/src/features/intake/components/FormStepper.tsx           | 4-step progress indicator with completed/active/pending states                 |
| CREATE | src/frontend/src/features/intake/hooks/useFormPersistence.ts          | localStorage auto-save/restore hook for form draft                             |
| MODIFY | src/frontend/src/features/intake/pages/IntakePage.tsx                 | Add manual form mode branch with conditional rendering                         |
| MODIFY | src/frontend/src/store/slices/intakeSlice.ts                          | Add manual form state fields, submitManualIntake thunk, setFormSection reducer |
| MODIFY | src/frontend/src/api/intakeApi.ts                                     | Add submitManualIntake API function                                            |
| MODIFY | src/frontend/src/types/intake.ts                                      | Add ManualIntakeFormData, section types, FormValidationErrors                  |

## External References

- [React 18 Forms Documentation](https://react.dev/reference/react-dom/components/form)
- [Redux Toolkit createAsyncThunk](https://redux-toolkit.js.org/api/createAsyncThunk)
- [Tailwind CSS v4 Documentation](https://tailwindcss.com/docs)
- [ARIA Forms Best Practices (WAI-ARIA)](https://www.w3.org/WAI/tutorials/forms/)
- [Wireframe SCR-013](.propel/context/wireframes/Hi-Fi/wireframe-SCR-013-manual-intake.html)

## Build Commands

```bash
cd src/frontend
npm install
npm run build
npm run typecheck
npm run lint
```

## Implementation Validation Strategy

- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] All 4 form sections (Demographics, Medical History, Insurance, Review) render correctly
- [ ] Required field validation shows inline errors below fields (UXR-601)
- [ ] Tooltip appears on hover/focus for clinical terminology fields (UXR-103)
- [ ] All form inputs have programmatically associated labels (UXR-203)
- [ ] Progress bar and stepper update on section navigation (UXR-101)
- [ ] Form data persists in localStorage and restores on page reload
- [ ] Data preserved when switching between AI and manual mode (UXR-102)
- [ ] Character counters visible on textarea fields with 2000-char limit
- [ ] Successful submission creates IntakeRecord with mode "ManualForm"

## Implementation Checklist

- [ ] Extend `types/intake.ts` with `ManualIntakeFormData`, section sub-types, and `FormValidationErrors`
- [ ] Add `submitManualIntake` API function to `intakeApi.ts`
- [ ] Extend `intakeSlice.ts` with manual form state, `submitManualIntake` thunk, and `setFormSection` reducer
- [ ] Build `FormStepper.tsx` with 4-step navigation matching wireframe stepper pattern
- [ ] Build `FormTooltip.tsx` with hover/focus tooltip and `aria-describedby` linking (UXR-103)
- [ ] Build `MedicalHistorySection.tsx` with inline validation, character counters, and associated labels
- [ ] Build `InsuranceSection.tsx` with insurance pre-check validation UI
- [ ] Build `ManualIntakeForm.tsx` container with step navigation, `useFormPersistence` hook, and `ReviewSection`
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
