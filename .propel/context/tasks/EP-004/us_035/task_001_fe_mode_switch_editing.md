# Task - task_001_fe_mode_switch_editing

## Requirement Reference

- User Story: us_035
- Story Location: .propel/context/tasks/EP-004/us_035/us_035.md
- Acceptance Criteria:
  - AC-1: When patient clicks "Switch to Manual Form" from AI intake, manual form loads pre-populated with all AI-extracted data with no data loss
  - AC-2: When patient clicks "Switch to AI Intake" from manual form, AI conversation resumes with awareness of entered form data, avoiding re-asking completed sections
  - AC-3: After intake submission and before the appointment, patient can edit any field without staff assistance and changes save immediately
  - AC-4: During mode switching, a brief loading indicator appears and all data mappings are preserved accurately between structured form fields and AI extraction
- Edge Cases:
  - AI-extracted data cannot cleanly map to form fields: Unmapped data placed in "Additional Notes" section for manual review
  - Switching modes during partially completed intake: All partial data preserved and transferred to the new mode

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-012-ai-intake.html |
| **Screen Spec** | figma_spec.md#SCR-012, figma_spec.md#SCR-013 |
| **UXR Requirements** | UXR-102, UXR-103 |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**

**IF Wireframe Status = AVAILABLE:**

- **MUST** open and reference the wireframe file during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, loading, error)
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

Implement the frontend logic for seamless bidirectional mode switching between AI conversational intake (SCR-012) and manual form intake (SCR-013), plus post-submission intake editing. This task creates a data mapping utility that transforms between AI-extracted structured data and manual form field values, extends the IntakePage mode toggle to trigger data-preserving transitions with a loading indicator, and adds an editable intake view accessible before the appointment. The core challenge is ensuring zero data loss when converting between the flat JSONB extraction format (medications array, allergies array) and the structured form field format (textareas, checkboxes, radios).

## Dependent Tasks

- EP-004/us_033/task_001_fe_ai_intake_ui — Provides IntakePage, ConversationalIntake, intakeSlice, intakeApi, and types/intake.ts
- EP-004/us_034/task_001_fe_manual_intake_form — Provides ManualIntakeForm, MedicalHistorySection, InsuranceSection, ReviewSection, FormStepper, and manual form types

## Impacted Components

- **NEW** `src/frontend/src/features/intake/utils/intakeDataMapper.ts` — Bidirectional mapping functions between AI extraction format and manual form field format
- **NEW** `src/frontend/src/features/intake/components/ModeSwitchOverlay.tsx` — Loading overlay displayed during mode transition with progress spinner
- **NEW** `src/frontend/src/features/intake/components/IntakeEditView.tsx` — Post-submission editable view of intake data (AC-3)
- **NEW** `src/frontend/src/features/intake/hooks/useModeSwitch.ts` — Custom hook encapsulating mode switch logic: save current state, call API, map data, update Redux
- **MODIFY** `src/frontend/src/features/intake/pages/IntakePage.tsx` — Wire mode toggle to `useModeSwitch` hook; add route for edit view
- **MODIFY** `src/frontend/src/store/slices/intakeSlice.ts` — Add `switchMode` async thunk, `updateIntakeField` reducer, `isSwitching` loading flag
- **MODIFY** `src/frontend/src/api/intakeApi.ts` — Add `switchIntakeMode` and `updateIntakeField` API functions
- **MODIFY** `src/frontend/src/types/intake.ts` — Add `IntakeDataMap`, `ModeSwitchDirection` types, and `AdditionalNotes` field for unmapped data

## Implementation Plan

1. **Define data mapping types** (`types/intake.ts`): Add `IntakeDataMap` interface representing the canonical intermediate format that bridges AI extraction and manual form. Add `ModeSwitchDirection` union type `'ai-to-manual' | 'manual-to-ai'`. Add `AdditionalNotes` string field for unmapped AI data.

2. **Build intakeDataMapper utility** (`utils/intakeDataMapper.ts`):
   - `mapAiToManualForm(extractedData: ExtractedIntakeData): ManualIntakeFormData` — Transforms AI extraction arrays into form field values. Maps `medications[]` → textarea string (one per line), `allergies[]` → text input (comma-separated), `symptoms[]` / `history[]` → textarea, `concerns[]` → textarea. Any extracted item without a clear form field mapping goes into `additionalNotes`.
   - `mapManualToAiContext(formData: ManualIntakeFormData): { completedCategories: string[], contextSummary: string }` — Analyzes which form sections have data and produces a summary string the AI can use to skip completed sections. Returns list of categories already filled.
   - Both functions are pure and unit-testable with no side effects.

3. **Build ModeSwitchOverlay component**: A centered full-screen overlay with semi-transparent backdrop, spinner animation, and text "Transferring your data..." Appears during mode switch (controlled by `isSwitching` Redux state). Includes `aria-live="assertive"` and `role="alert"` for accessibility (AC-4).

4. **Build useModeSwitch hook**: Encapsulates the mode switch flow:
   - Step 1: Set `isSwitching = true` in Redux (shows overlay)
   - Step 2: Save current mode's data to server via `PUT /api/intake/{id}/draft` (persist before switching)
   - Step 3: Call `PATCH /api/intake/{id}/mode` API to update the `IntakeMode` on the server
   - Step 4: Call appropriate mapper (`mapAiToManualForm` or `mapManualToAiContext`) to transform local state
   - Step 5: Hydrate the target mode's Redux state with mapped data
   - Step 6: Set `isSwitching = false`, update `activeMode` in Redux
   - Error handling: If API call fails, revert to previous mode and show error toast

5. **Extend IntakePage mode toggle**: Replace the static link in the mode toggle with `useModeSwitch` hook invocation. When toggle is clicked: call `switchMode(direction)`. Conditionally render `ConversationalIntake` or `ManualIntakeForm` based on `activeMode` Redux state. Render `ModeSwitchOverlay` when `isSwitching` is true.

6. **Build IntakeEditView component** (AC-3): A page accessible at `/intake/{id}/edit` for completed intake records. Renders all intake fields in editable mode (using the same form components from ManualIntakeForm). On field change, immediately dispatch `updateIntakeField` thunk which calls `PATCH /api/intake/{id}` with the changed field. Show inline save confirmation ("Saved ✓") on each field blur. Only accessible before the appointment date.

7. **Extend intakeSlice**: Add `switchMode` async thunk (wraps useModeSwitch API calls), `activeMode: 'ai' | 'manual'` state field, `isSwitching: boolean` flag, `updateIntakeField` async thunk for inline edits, and `additionalNotes` string for unmapped data.

8. **Extend intakeApi**: Add `switchIntakeMode(intakeId, newMode)` calling `PATCH /api/intake/{id}/mode`, and `updateIntakeField(intakeId, fieldName, value)` calling `PATCH /api/intake/{id}` with partial body.

## Current Project State

```
src/frontend/src/
├── api/
│   ├── intakeApi.ts             # US_033 (start, message, summary, complete)
│   │                            # US_034 (submitManualIntake, draft)
├── features/
│   └── intake/
│       ├── components/
│       │   ├── ConversationalIntake.tsx   # US_033
│       │   ├── ChatBubble.tsx            # US_033
│       │   ├── TypingIndicator.tsx       # US_033
│       │   ├── IntakeSummary.tsx         # US_033
│       │   ├── ManualIntakeForm.tsx      # US_034
│       │   ├── MedicalHistorySection.tsx # US_034
│       │   ├── InsuranceSection.tsx      # US_034
│       │   ├── ReviewSection.tsx         # US_034
│       │   ├── FormTooltip.tsx           # US_034
│       │   └── FormStepper.tsx           # US_034
│       ├── hooks/
│       │   └── useFormPersistence.ts     # US_034
│       └── pages/
│           └── IntakePage.tsx            # US_033 (mode toggle, conditional render)
├── store/
│   └── slices/
│       └── intakeSlice.ts               # US_033 + US_034 (chat & manual form state)
├── types/
│   └── intake.ts                        # US_033 + US_034 (session, messages, form data)
└── App.tsx                              # /intake route active
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/features/intake/utils/intakeDataMapper.ts | Bidirectional mapping between AI extraction and manual form formats |
| CREATE | src/frontend/src/features/intake/components/ModeSwitchOverlay.tsx | Loading overlay during mode transition with spinner and aria-live |
| CREATE | src/frontend/src/features/intake/components/IntakeEditView.tsx | Post-submission editable intake view at /intake/{id}/edit |
| CREATE | src/frontend/src/features/intake/hooks/useModeSwitch.ts | Custom hook for mode switch: save, API call, map data, hydrate |
| MODIFY | src/frontend/src/features/intake/pages/IntakePage.tsx | Wire mode toggle to useModeSwitch; add edit route |
| MODIFY | src/frontend/src/store/slices/intakeSlice.ts | Add switchMode thunk, activeMode, isSwitching, updateIntakeField |
| MODIFY | src/frontend/src/api/intakeApi.ts | Add switchIntakeMode and updateIntakeField API functions |
| MODIFY | src/frontend/src/types/intake.ts | Add IntakeDataMap, ModeSwitchDirection, AdditionalNotes types |

## External References

- [React 18 Documentation](https://react.dev/reference/react)
- [Redux Toolkit createAsyncThunk](https://redux-toolkit.js.org/api/createAsyncThunk)
- [ARIA Live Regions (WAI-ARIA)](https://www.w3.org/WAI/ARIA/apd/#aria-live)
- [Wireframe SCR-012](.propel/context/wireframes/Hi-Fi/wireframe-SCR-012-ai-intake.html)
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
- [ ] AI→Manual switch pre-populates all form fields with AI-extracted data (AC-1)
- [ ] Manual→AI switch resumes conversation with awareness of completed sections (AC-2)
- [ ] Post-submission edit view allows field changes with inline save (AC-3)
- [ ] Loading overlay appears during mode switch transition (AC-4)
- [ ] Unmapped AI data appears in "Additional Notes" section (edge case)
- [ ] Partial intake data preserved during mid-process switch (edge case)
- [ ] Data preservation verified in both switch directions (UXR-102)

## Implementation Checklist

- [ ] Define `IntakeDataMap`, `ModeSwitchDirection`, and `AdditionalNotes` types in `types/intake.ts`
- [ ] Build `intakeDataMapper.ts` with `mapAiToManualForm` and `mapManualToAiContext` pure functions
- [ ] Build `ModeSwitchOverlay.tsx` with spinner, transition text, and ARIA live region
- [ ] Build `useModeSwitch.ts` hook with save-current → API-switch → map-data → hydrate-target flow
- [ ] Wire mode toggle in `IntakePage.tsx` to `useModeSwitch` hook with conditional rendering
- [ ] Build `IntakeEditView.tsx` with editable fields and inline auto-save on field blur (AC-3)
- [ ] Extend `intakeSlice.ts` with `switchMode` thunk, `activeMode`, `isSwitching`, and `updateIntakeField`
- [ ] Extend `intakeApi.ts` with `switchIntakeMode` and `updateIntakeField` API functions
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
