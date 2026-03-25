# Task - task_001_fe_insurance_precheck_ui

## Requirement Reference

- User Story: us_036
- Story Location: .propel/context/tasks/EP-004/us_036/us_036.md
- Acceptance Criteria:
  - AC-1: Patient enters insurance provider name and ID; system validates against InsuranceRecord using pattern matching
  - AC-2: Valid insurance displays green checkmark with coverage type (e.g., "Insurance verified — PPO coverage")
  - AC-3: Invalid ID displays warning "Insurance could not be verified. Please check your insurance ID." with option to proceed
  - AC-4: Provider not found displays "Provider not found in our records" and allows manual entry to continue
- Edge Cases:
  - Empty reference data: System displays "Validation unavailable" status and allows intake to proceed
  - Insurance IDs with leading zeros or special formats: Frontend trims whitespace but preserves format, delegates regex validation to backend

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-013-manual-intake.html |
| **Screen Spec** | figma_spec.md#SCR-013 |
| **UXR Requirements** | UXR-601 |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**

**IF Wireframe Status = AVAILABLE:**

- **MUST** open and reference the wireframe file during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, loading, error, validation)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px
- Run `/analyze-ux` after implementation to verify pixel-perfect alignment

## Applicable Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React + TypeScript + Redux Toolkit + Tailwind CSS | React 18.x, TypeScript 5.x, Redux Toolkit 2.x |
| Backend | .NET 8 ASP.NET Core Web API | .NET 8.0 |
| Library | React Router | v7 |
| AI/ML | N/A | N/A |

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

Implement the frontend insurance pre-check validation UI within the InsuranceSection component (created in US_034). This task adds real-time insurance validation by calling the backend `POST /api/intake/insurance/validate` endpoint when the patient clicks "Verify Insurance". The component displays three distinct result states: a green checkmark with coverage type on success (AC-2), an inline warning on ID mismatch (AC-3), and a "Provider not found" message for unknown providers (AC-4). All validation feedback is displayed inline below the insurance fields following UXR-601. The patient can always proceed with intake regardless of validation outcome — the result is informational only.

## Dependent Tasks

- EP-004/us_034/task_001_fe_manual_intake_form — Provides InsuranceSection.tsx component shell, FormTooltip, intakeSlice manual form state, and intakeApi client
- EP-004/us_036/task_002_be_insurance_precheck_api — Provides the `POST /api/intake/insurance/validate` endpoint returning validation result

## Impacted Components

- **NEW** `src/frontend/src/features/intake/components/InsuranceValidationResult.tsx` — Presentational component rendering validation outcome (success/warning/not-found/unavailable)
- **NEW** `src/frontend/src/features/intake/hooks/useInsuranceValidation.ts` — Custom hook encapsulating validate API call, loading state, and result state
- **MODIFY** `src/frontend/src/features/intake/components/InsuranceSection.tsx` — Wire "Verify Insurance" button to useInsuranceValidation hook; render InsuranceValidationResult
- **MODIFY** `src/frontend/src/api/intakeApi.ts` — Add `validateInsurance` API function
- **MODIFY** `src/frontend/src/types/intake.ts` — Add `InsuranceValidationResult` and `InsuranceValidationStatus` types
- **MODIFY** `src/frontend/src/store/slices/intakeSlice.ts` — Add `insuranceValidation` state field with result and loading flag

## Implementation Plan

1. **Define TypeScript types** (`types/intake.ts`): Add types for the insurance validation flow:
   ```typescript
   type InsuranceValidationStatus = 'idle' | 'loading' | 'valid' | 'invalid' | 'not-found' | 'unavailable';

   interface InsuranceValidationResult {
     status: InsuranceValidationStatus;
     coverageType?: string; // e.g., "PPO", "HMO"
     providerName?: string;
     message?: string;
   }
   ```

2. **Extend API client** (`api/intakeApi.ts`): Add `validateInsurance(providerName: string, insuranceId: string)` function calling `POST /api/intake/insurance/validate` with `{ providerName, insuranceId }`. Returns `InsuranceValidationResult`. Follow existing fetch pattern from `providerApi.ts`.

3. **Build useInsuranceValidation hook** (`hooks/useInsuranceValidation.ts`):
   - State: `result: InsuranceValidationResult` (initially idle), `isValidating: boolean`
   - `validate(providerName, insuranceId)`: Sets loading, calls API, updates result
   - `reset()`: Clears result back to idle
   - Debounce is NOT applied — validation only triggers on explicit "Verify" button click
   - Return `{ result, isValidating, validate, reset }`

4. **Build InsuranceValidationResult component** (`components/InsuranceValidationResult.tsx`): A presentational component that renders one of four states based on `status`:
   - `valid`: Green checkmark icon + "Insurance verified — {coverageType} coverage" in green text
   - `invalid`: Yellow warning icon + "Insurance could not be verified. Please check your insurance ID." in amber text + "Proceed anyway" link
   - `not-found`: Gray info icon + "Provider not found in our records" in gray text + allows manual entry to continue
   - `unavailable`: Gray info icon + "Validation unavailable" in gray text
   - Uses `role="status"` and `aria-live="polite"` so screen readers announce the result (UXR-601)
   - All text and icons use design tokens from designsystem.md

5. **Extend InsuranceSection component**: Replace placeholder "Verify Insurance" button behavior:
   - Wire button `onClick` to `useInsuranceValidation.validate(providerName, insuranceId)`
   - Disable button while `isValidating` is true; show spinner inside button
   - Render `InsuranceValidationResult` below the insurance fields
   - On provider name or ID change, call `reset()` to clear previous result
   - Button disabled if either provider name or insurance ID is empty

6. **Extend intakeSlice**: Add `insuranceValidation` nested state:
   ```typescript
   insuranceValidation: {
     status: InsuranceValidationStatus;
     coverageType: string | null;
     message: string | null;
   }
   ```
   Add `setInsuranceValidation` reducer. The hook dispatches this on API response so the validation state persists when navigating between form steps.

7. **Update InsuranceSection form flow**: Ensure the "Next" button to Review section works regardless of validation outcome — insurance validation is informational only. The `InsuranceValidationStatus` is included in the form submission payload for the backend to record on the `IntakeRecord`.

## Current Project State

```
src/frontend/src/
├── api/
│   └── intakeApi.ts             # US_033 + US_034 (start, message, submit, draft)
├── features/
│   └── intake/
│       ├── components/
│       │   ├── ConversationalIntake.tsx   # US_033
│       │   ├── ChatBubble.tsx            # US_033
│       │   ├── TypingIndicator.tsx       # US_033
│       │   ├── IntakeSummary.tsx         # US_033
│       │   ├── ManualIntakeForm.tsx      # US_034
│       │   ├── MedicalHistorySection.tsx # US_034
│       │   ├── InsuranceSection.tsx      # US_034 (shell with fields + Verify button)
│       │   ├── ReviewSection.tsx         # US_034
│       │   ├── FormTooltip.tsx           # US_034
│       │   └── FormStepper.tsx           # US_034
│       ├── hooks/
│       │   └── useFormPersistence.ts     # US_034
│       └── pages/
│           └── IntakePage.tsx            # US_033 (mode toggle, conditional render)
├── store/
│   └── slices/
│       └── intakeSlice.ts               # US_033 + US_034 (chat, manual form, submit)
├── types/
│   └── intake.ts                        # US_033 + US_034 (session, messages, form data)
└── App.tsx                              # /intake route active
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/features/intake/components/InsuranceValidationResult.tsx | Presentational component for validation outcome states (valid/invalid/not-found/unavailable) |
| CREATE | src/frontend/src/features/intake/hooks/useInsuranceValidation.ts | Hook encapsulating validate API call, loading state, result management |
| MODIFY | src/frontend/src/features/intake/components/InsuranceSection.tsx | Wire Verify button to hook; render InsuranceValidationResult below fields |
| MODIFY | src/frontend/src/api/intakeApi.ts | Add validateInsurance API function |
| MODIFY | src/frontend/src/types/intake.ts | Add InsuranceValidationResult and InsuranceValidationStatus types |
| MODIFY | src/frontend/src/store/slices/intakeSlice.ts | Add insuranceValidation state field with status, coverageType, message |

## External References

- [React 18 Documentation](https://react.dev/reference/react)
- [ARIA Live Regions (WAI-ARIA)](https://www.w3.org/WAI/ARIA/apd/#aria-live)
- [Wireframe SCR-013](.propel/context/wireframes/Hi-Fi/wireframe-SCR-013-manual-intake.html)
- [Insurance Validation Result Modal spec](figma_spec.md line 218)

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
- [ ] Valid insurance ID shows green checkmark with coverage type (AC-2)
- [ ] Invalid insurance ID shows warning with "proceed anyway" option (AC-3)
- [ ] Unknown provider shows "Provider not found" with manual entry continuation (AC-4)
- [ ] Validation unavailable state renders correctly (edge case)
- [ ] Screen reader announces validation result via aria-live (UXR-601)
- [ ] Verify button disabled while loading and when fields are empty
- [ ] Patient can proceed to Review regardless of validation outcome

## Implementation Checklist

- [ ] Define `InsuranceValidationStatus` and `InsuranceValidationResult` types in `types/intake.ts`
- [ ] Add `validateInsurance` API function in `intakeApi.ts`
- [ ] Build `useInsuranceValidation` hook with validate, reset, and loading state
- [ ] Build `InsuranceValidationResult` component with four visual states and ARIA attributes
- [ ] Wire `InsuranceSection.tsx` Verify button to hook and render result component
- [ ] Extend `intakeSlice.ts` with `insuranceValidation` state for cross-step persistence
- [ ] Verify form progression works regardless of validation outcome
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
