# Task - task_001_fe_ai_intake_ui

## Requirement Reference

- User Story: us_033
- Story Location: .propel/context/tasks/EP-004/us_033/us_033.md
- Acceptance Criteria:
  - AC-1: AI conversational interface loads with welcome message and begins guided health data collection
  - AC-2: Patient responds in natural language; AI extracts structured data and confirms understanding (UI displays extracted data confirmation)
  - AC-3: Summary displayed for patient review before submission after all categories collected
  - AC-4: When confidence drops below 70%, AI suggests switching to manual form mode while preserving entered data
- Edge Cases:
  - AI cannot understand patient response: UI displays clarification request; after 3 consecutive failures, offers manual form option
  - Patient provides minimal responses: UI shows follow-up prompts from AI

## Design References (Frontend Tasks Only)

| Reference Type         | Value                                                                       |
| ---------------------- | --------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                         |
| **Figma URL**          | N/A                                                                         |
| **Wireframe Status**   | AVAILABLE                                                                   |
| **Wireframe Type**     | HTML                                                                        |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-012-ai-intake.html           |
| **Screen Spec**        | figma_spec.md#SCR-012                                                       |
| **UXR Requirements**   | UXR-101, UXR-102, UXR-103, UXR-207                                          |
| **Design Tokens**      | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**

**IF Wireframe Status = AVAILABLE:**

- **MUST** open and reference the wireframe file during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, loading, error)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px
- Run `/analyze-ux` after implementation to verify pixel-perfect alignment

## Applicable Technology Stack

| Layer    | Technology                                        | Version                                       |
| -------- | ------------------------------------------------- | --------------------------------------------- |
| Frontend | React + TypeScript + Redux Toolkit + Tailwind CSS | React 18.x, TypeScript 5.x, Redux Toolkit 2.x |
| Backend  | .NET 8 ASP.NET Core Web API                       | .NET 8.0                                      |
| Library  | React Router                                      | v7                                            |
| AI/ML    | N/A (consumed via BE API)                         | N/A                                           |

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

Implement the AI Conversational Intake frontend UI for SCR-012. This task builds the chat-based interface where patients interact with an AI assistant during pre-visit intake. The component renders chat bubbles (AI and user), a text input area, a progress bar tracking intake completion, a mode toggle to switch between AI and manual form, and a summary review screen before submission. The UI consumes backend API endpoints for sending messages and receiving AI-extracted structured data.

## Dependent Tasks

- EP-004/us_037/task_001_fe_appointment_selection_ui — Provides appointment selection UI; user must select appointment before accessing AI intake
- EP-004/us_037/task_002_be_appointment_selection_api — Backend API for fetching appointments requiring intake

## Impacted Components

- **NEW** `src/frontend/src/features/intake/components/ConversationalIntake.tsx` — Main chat component
- **NEW** `src/frontend/src/features/intake/components/ChatBubble.tsx` — Reusable chat bubble (AI/user variants)
- **NEW** `src/frontend/src/features/intake/components/TypingIndicator.tsx` — Animated typing dots
- **NEW** `src/frontend/src/features/intake/components/IntakeSummary.tsx` — Review summary before submission
- **NEW** `src/frontend/src/features/intake/pages/IntakePage.tsx` — Intake page with mode toggle
- **NEW** `src/frontend/src/store/slices/intakeSlice.ts` — Redux slice for intake state
- **NEW** `src/frontend/src/api/intakeApi.ts` — API client for intake endpoints
- **NEW** `src/frontend/src/types/intake.ts` — TypeScript types for intake data
- **MODIFY** `src/frontend/src/App.tsx` — Update `/intake` route to render IntakePage
- **MODIFY** `src/frontend/src/store/rootReducer.ts` — Register intake slice

## Implementation Plan

1. **Define TypeScript types** (`types/intake.ts`): Create interfaces for `IntakeSession`, `ChatMessage`, `ExtractedIntakeData`, `IntakeSummaryData`, and API request/response shapes matching the backend contract (POST `/api/intake/start`, POST `/api/intake/message`, PATCH `/api/intake/{id}`, POST `/api/intake/{id}/complete`).

2. **Build API client** (`api/intakeApi.ts`): Implement functions `startIntakeSession`, `sendIntakeMessage`, `updateIntakeData`, and `completeIntake` using fetch with `VITE_API_BASE_URL`. Include JWT bearer token from auth state. Follow existing pattern in `providerApi.ts`.

3. **Create Redux slice** (`store/slices/intakeSlice.ts`): Define state shape holding `messages[]`, `sessionId`, `extractedData`, `progress` (0-100), `confidenceLevel`, `status` (idle | loading | error | complete), `consecutiveFailures` counter. Add async thunks for each API call. Follow existing `appointmentSlice.ts` pattern.

4. **Build ChatBubble component**: Render directional chat bubbles (AI left-aligned with `AI` avatar, user right-aligned with user initials). Use Tailwind classes matching wireframe colors: AI bubble `bg-neutral-100 text-neutral-800`, user bubble `bg-primary-500 text-white`. Include ARIA roles.

5. **Build TypingIndicator component**: Three animated dots matching wireframe CSS keyframe animation (`typing` animation with 1.4s infinite). Wrap in `aria-label="AI is typing"`.

6. **Build ConversationalIntake component**: Render scrollable chat message list with auto-scroll to bottom on new messages. Include textarea input with send button. On send, dispatch `sendIntakeMessage` thunk. Display TypingIndicator during loading state. Track `consecutiveFailures`; when reaching 3, show a banner suggesting manual form switch (AIR-S03 / AC-4).

7. **Build IntakeSummary component**: When all required categories are collected (`progress === 100`), display extracted data summary (medications, allergies, history, symptoms, concerns) in a structured card layout. Include "Confirm" and "Edit" actions (AC-3).

8. **Build IntakePage with mode toggle**: Render header with "Pre-visit intake" title, a toggle switch (AI Mode on/off), and link to manual form. Include progress bar. Wrap ConversationalIntake and IntakeSummary. Use `aria-live="polite"` on chat messages region (UXR-207). Show progress indicator (UXR-101).

9. **Wire routing**: Update `App.tsx` to replace the placeholder at `/intake` with `<IntakePage />`. Register intake reducer in `rootReducer.ts`.

## Current Project State

```
src/frontend/src/
├── api/
│   ├── providerApi.ts
│   └── staffApi.ts
├── features/
│   ├── auth/
│   ├── appointments/
│   └── intake/          # (empty — to be populated)
├── store/
│   ├── index.ts
│   ├── rootReducer.ts
│   └── slices/
│       ├── appointmentSlice.ts
│       ├── providerSlice.ts
│       └── waitlistSlice.ts
├── types/
├── App.tsx              # /intake route is placeholder "Coming soon"
└── ...
```

## Expected Changes

| Action | File Path                                                            | Description                                                        |
| ------ | -------------------------------------------------------------------- | ------------------------------------------------------------------ |
| CREATE | src/frontend/src/types/intake.ts                                     | TypeScript interfaces for intake session, messages, extracted data |
| CREATE | src/frontend/src/api/intakeApi.ts                                    | API client functions for intake endpoints                          |
| CREATE | src/frontend/src/store/slices/intakeSlice.ts                         | Redux slice with async thunks for intake flow                      |
| CREATE | src/frontend/src/features/intake/components/ChatBubble.tsx           | Chat bubble component with AI/user variants                        |
| CREATE | src/frontend/src/features/intake/components/TypingIndicator.tsx      | Animated typing indicator                                          |
| CREATE | src/frontend/src/features/intake/components/ConversationalIntake.tsx | Main chat interface with message list and input                    |
| CREATE | src/frontend/src/features/intake/components/IntakeSummary.tsx        | Extracted data summary for review/confirm                          |
| CREATE | src/frontend/src/features/intake/pages/IntakePage.tsx                | Intake page with mode toggle                                       |
| MODIFY | src/frontend/src/App.tsx                                             | Replace /intake placeholder with IntakePage component              |
| MODIFY | src/frontend/src/store/rootReducer.ts                                | Register intakeReducer                                             |

## External References

- [React 18 Documentation](https://react.dev/reference/react)
- [Redux Toolkit createAsyncThunk](https://redux-toolkit.js.org/api/createAsyncThunk)
- [Tailwind CSS v4 Documentation](https://tailwindcss.com/docs)
- [ARIA Live Regions (WAI-ARIA)](https://www.w3.org/WAI/ARIA/apd/#aria-live)
- [Wireframe SCR-012](.propel/context/wireframes/Hi-Fi/wireframe-SCR-012-ai-intake.html)

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
- [ ] Chat messages render correctly for AI and user variants
- [ ] Progress bar updates as intake categories are completed
- [ ] Mode toggle navigates to manual form preserving data (UXR-102)
- [ ] ARIA live region announces new messages (UXR-207)
- [ ] After 3 consecutive low-confidence responses, fallback banner appears (AIR-S03)
- [ ] Summary screen displays all extracted categories for review (AC-3)

## Implementation Checklist

- [ ] Create TypeScript interfaces in `types/intake.ts` for session, messages, and extracted data structures
- [ ] Implement API client functions in `api/intakeApi.ts` following existing `providerApi.ts` pattern
- [ ] Create `intakeSlice.ts` with state, async thunks, and reducers for chat flow management
- [ ] Build `ChatBubble.tsx` with directional styling matching wireframe (AI left, user right)
- [ ] Build `TypingIndicator.tsx` with animated three-dot pattern and ARIA label
- [ ] Build `ConversationalIntake.tsx` with auto-scrolling message list, textarea input, and send handling
- [ ] Build `IntakeSummary.tsx` displaying structured extracted data with confirm/edit actions
- [ ] Build `IntakePage.tsx` with header, mode toggle, progress bar, and chat area; wire routing in App.tsx
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
