# Task - task_001_fe_reusable_patient_search

## Requirement Reference

- User Story: US_032 - Patient Search Functionality
- Story Location: .propel/context/tasks/EP-003/us_032/us_032.md
- Acceptance Criteria:
  - AC-1: Given I am on any Staff page with patient search, When I type in the search field, Then real-time filtering begins after 300ms debounce matching against patient name, email, and phone number.
  - AC-2: Given search results are returned, When I view the results, Then each result shows patient name, date of birth, email, phone, and last appointment date for quick identification.
  - AC-3: Given I select a patient from results, When I click on the record, Then I am navigated to that patient's profile context (booking, verification, or queue depending on the source page).
  - AC-4: Given no patients match, When the search returns empty, Then a "No patients found" message displays with option to create a new patient record.
- Edge Case:
  - What happens when multiple patients have similar names? Search results include DOB and email for disambiguation.
  - How does the system handle special characters in search queries? Search input is sanitized to prevent injection while still matching names with accents or hyphens.

## Design References (Frontend Tasks Only)

| Reference Type         | Value                                                                                        |
| ---------------------- | -------------------------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                                          |
| **Figma URL**          | N/A                                                                                          |
| **Wireframe Status**   | AVAILABLE                                                                                    |
| **Wireframe Type**     | HTML                                                                                         |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-004-staff-dashboard.html                      |
| **Screen Spec**        | .propel/context/docs/figma_spec.md#SCR-004                                                   |
| **UXR Requirements**   | UXR-004 (Inline search with real-time filtering)                                             |
| **Design Tokens**      | .propel/context/docs/designsystem.md#typography, .propel/context/docs/designsystem.md#colors |

> **Wireframe Status Legend:**
>
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

| Layer        | Technology    | Version |
| ------------ | ------------- | ------- |
| Frontend     | React         | 18.x    |
| Frontend     | TypeScript    | 5.x     |
| Frontend     | Redux Toolkit | 2.x     |
| Frontend     | Tailwind CSS  | Latest  |
| Frontend     | React Router  | 6.x     |
| Library      | Axios         | Latest  |
| Backend      | N/A           | N/A     |
| Database     | N/A           | N/A     |
| AI/ML        | N/A           | N/A     |
| Vector Store | N/A           | N/A     |
| AI Gateway   | N/A           | N/A     |
| Mobile       | N/A           | N/A     |

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

> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)

| Reference Type       | Value |
| -------------------- | ----- |
| **Mobile Impact**    | No    |
| **Platform Target**  | N/A   |
| **Min OS Version**   | N/A   |
| **Mobile Framework** | N/A   |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Create a reusable, context-aware Patient Search component that can be used across multiple Staff pages (Walk-in Booking, Arrival Management, Staff Dashboard, Queue Management). The component features real-time search with 300ms debounce, rich result display (name, DOB, email, phone, last appointment), configurable navigation behavior based on context, and empty state handling with optional "Create Patient" CTA. This component consolidates and refactors patient search functionality from US_029 into a shareable, customizable component following DRY principles.

## Dependent Tasks

- US_029 Task 001 (Walk-in Booking UI) - contains initial patient search implementation to refactor
- US_031 Task 001 (Arrival Management UI) - will use this reusable component

## Impacted Components

- **NEW**: `src/frontend/src/components/shared/PatientSearch/PatientSearch.tsx` - Main reusable patient search component
- **NEW**: `src/frontend/src/components/shared/PatientSearch/PatientSearchResult.tsx` - Individual search result row component
- **NEW**: `src/frontend/src/components/shared/PatientSearch/types.ts` - TypeScript types and interfaces for search component
- **NEW**: `src/frontend/src/hooks/usePatientSearch.ts` - Custom hook for patient search logic and API integration
- **MODIFY**: `src/frontend/src/pages/staff/WalkinBooking.tsx` - Replace inline search with reusable PatientSearch component
- **MODIFY**: `src/frontend/src/pages/staff/ArrivalManagement.tsx` - Replace inline search with reusable PatientSearch component
- **MODIFY**: `src/frontend/src/api/client.ts` - Ensure searchPatients method is generic and returns full patient details

## Implementation Plan

1. **Define Component Interface and Types**
   - Create `types.ts` with interfaces:
     - `PatientSearchResult` (id, name, dateOfBirth, email, phone, lastAppointmentDate)
     - `PatientSearchProps` (onSelectPatient, showCreateButton, placeholder, context)
     - `SearchContext` enum (WalkinBooking, ArrivalManagement, QueueManagement, Dashboard)
   - Define navigation behavior per context:
     - WalkinBooking: populate booking form
     - ArrivalManagement: display appointment card
     - QueueManagement: filter queue by patient
     - Dashboard: navigate to patient profile

2. **Create Custom Hook for Search Logic**
   - Create `usePatientSearch.ts` custom hook:
     - Accept search query state (controlled by parent or internal)
     - Implement 300ms debounce using `useDebouncedValue` or `useEffect` + setTimeout
     - Make API call to `/api/staff/patients/search?query={debouncedQuery}` when query length >= 2
     - Return: `{ results, isLoading, error, clearResults }`
     - Handle empty query (clear results)
     - Handle API errors gracefully

3. **Implement PatientSearchResult Row Component**
   - Create `PatientSearchResult.tsx` displaying:
     - Patient name (bold, primary text)
     - Date of Birth (secondary text, formatted as MM/DD/YYYY)
     - Email (secondary text, truncated if long)
     - Phone (secondary text, formatted)
     - Last Appointment Date (if available, formatted as "Last seen: MM/DD/YYYY")
   - Add hover state (background highlight)
   - Add click handler passed from parent (onSelect)
   - Ensure accessibility: use semantic button element, add ARIA labels

4. **Implement Main PatientSearch Component**
   - Create `PatientSearch.tsx` with controlled search input
   - Use `usePatientSearch` hook for search logic
   - Display loading spinner overlay during search
   - Render dropdown results container when results exist and input is focused
   - Map search results to `PatientSearchResult` components
   - Implement keyboard navigation:
     - Arrow Up/Down to navigate results
     - Enter to select highlighted result
     - Escape to close dropdown
   - Display empty state when no results:
     - Message: "No patients found matching '{query}'"
     - Conditional "Create New Patient" button (if showCreateButton prop is true)
   - Implement click-outside to close dropdown
   - Add input sanitization for special characters (allow accents, hyphens, apostrophes)

5. **Implement Context-Aware Navigation**
   - Accept `onSelectPatient(patient: PatientSearchResult)` callback prop
   - Parent component determines navigation behavior based on context:
     - WalkinBooking: populate form fields with patient data
     - ArrivalManagement: fetch and display appointment card
     - QueueManagement: filter queue table by selected patient
     - Dashboard: navigate to patient detail page
   - Clear search input after selection (configurable via prop)

6. **Refactor Existing Pages to Use Component**
   - Update `WalkinBooking.tsx`:
     - Replace inline PatientSearchInput with shared PatientSearch component
     - Pass `onSelectPatient` handler to populate booking form
     - Remove duplicate search logic
   - Update `ArrivalManagement.tsx` (when created):
     - Use shared PatientSearch component
     - Pass `onSelectPatient` handler to display appointment card
   - Ensure both pages use the same underlying API endpoint

7. **Implement Input Sanitization**
   - Allow letters (including accented: á, é, ñ, etc.)
   - Allow hyphens, apostrophes, spaces
   - Prevent SQL injection characters: semicolons, quotes (escaped on backend)
   - Use regex whitelist: `/^[a-zA-Z0-9\s\-'À-ÿ@.]+$/`

8. **Apply Design Tokens and Styling**
   - Reference design tokens from designsystem.md
   - Use Tailwind CSS utility classes for consistent styling
   - Match wireframe layout for search input and dropdown
   - Implement responsive dropdown width (full-width on mobile, max-width on desktop)
   - Add z-index layering for dropdown to appear above other content

9. **Implement Accessibility**
   - Add ARIA attributes: `role="combobox"`, `aria-expanded`, `aria-activedescendant`
   - Announce result count via ARIA live region (e.g., "3 patients found")
   - Ensure keyboard navigation follows ARIA combobox pattern
   - Provide screen reader-friendly result descriptions

## Current Project State

```
src/frontend/src/
├── api/
│   └── client.ts                              # API client with searchPatients method (from US_029)
├── App.tsx                                    # Main app with routing
├── features/
│   └── staff/
│       └── components/
│           ├── PatientSearchInput.tsx        # US_029 inline search (to be replaced)
│           └── CreatePatientModal.tsx        # US_029 patient creation modal
├── pages/
│   └── staff/
│       ├── WalkinBooking.tsx                 # Uses inline patient search (US_029)
│       └── ArrivalManagement.tsx             # Will use reusable search (US_031)
├── components/
│   └── common/                               # Shared components
└── hooks/                                    # Custom hooks directory
```

## Expected Changes

| Action | File Path                                                                | Description                                                                                 |
| ------ | ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------- |
| CREATE | src/frontend/src/components/shared/PatientSearch/PatientSearch.tsx       | Main reusable patient search component with debounce, dropdown results, keyboard navigation |
| CREATE | src/frontend/src/components/shared/PatientSearch/PatientSearchResult.tsx | Individual search result row (name, DOB, email, phone, last appointment)                    |
| CREATE | src/frontend/src/components/shared/PatientSearch/types.ts                | TypeScript interfaces for PatientSearchResult, PatientSearchProps, SearchContext            |
| CREATE | src/frontend/src/hooks/usePatientSearch.ts                               | Custom hook for search logic with 300ms debounce and API integration                        |
| MODIFY | src/frontend/src/pages/staff/WalkinBooking.tsx                           | Replace inline PatientSearchInput with shared PatientSearch component                       |
| MODIFY | src/frontend/src/pages/staff/ArrivalManagement.tsx                       | Use shared PatientSearch component (when page is created in US_031)                         |
| MODIFY | src/frontend/src/api/client.ts                                           | Ensure searchPatients returns full patient details including lastAppointmentDate            |
| DELETE | src/frontend/src/features/staff/components/PatientSearchInput.tsx        | Remove duplicate inline search from US_029 (replaced by shared component)                   |

## External References

- React Debounce Hook: https://usehooks-ts.com/react-hook/use-debounce
- ARIA Combobox Pattern: https://www.w3.org/WAI/ARIA/apg/patterns/combobox/
- React Custom Hooks: https://react.dev/learn/reusing-logic-with-custom-hooks
- Tailwind CSS Dropdown: https://tailwindui.com/components/application-ui/elements/dropdowns
- Input Sanitization Regex: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_Expressions

## Build Commands

- `npm run dev` - Start development server on http://localhost:5173
- `npm run build` - Build for production
- `npm run test` - Run Vitest unit tests
- `npm run lint` - Run ESLint
- `npm run format` - Run Prettier

## Implementation Validation Strategy

- [ ] Unit tests pass for PatientSearch, PatientSearchResult, usePatientSearch hook
- [ ] Integration tests pass for search API call with 300ms debounce
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Component successfully used in WalkinBooking and ArrivalManagement pages
- [ ] Keyboard navigation works (arrow keys, Enter, Escape)
- [ ] Accessibility audit passes with axe DevTools (zero critical violations)
- [ ] ARIA combobox pattern correctly implemented
- [ ] Input sanitization allows accented characters, hyphens, apostrophes
- [ ] Empty state displays with "No patients found" message
- [ ] "Create Patient" button displays when showCreateButton=true
- [ ] Search results include all required fields (name, DOB, email, phone, last appointment)

## Implementation Checklist

- [ ] Create types.ts with PatientSearchResult, PatientSearchProps, SearchContext interfaces
- [ ] Create usePatientSearch.ts custom hook:
  - [ ] Accept search query parameter
  - [ ] Implement 300ms debounce using useEffect + setTimeout
  - [ ] Make API call to /api/staff/patients/search when query length >= 2
  - [ ] Return results, isLoading, error, clearResults
  - [ ] Handle empty query (clear results)
  - [ ] Handle API errors gracefully
- [ ] Create PatientSearchResult.tsx row component:
  - [ ] Display patient name (bold)
  - [ ] Display DOB (formatted MM/DD/YYYY)
  - [ ] Display email (truncated if long)
  - [ ] Display phone (formatted)
  - [ ] Display last appointment date (if available)
  - [ ] Add hover state (background highlight)
  - [ ] Use semantic button element with onClick handler
  - [ ] Add ARIA label for screen readers
- [ ] Create PatientSearch.tsx main component:
  - [ ] Add controlled search input with placeholder prop
  - [ ] Use usePatientSearch hook for search logic
  - [ ] Display loading spinner during search
  - [ ] Render dropdown results container
  - [ ] Map results to PatientSearchResult components
  - [ ] Implement keyboard navigation (arrow keys, Enter, Escape)
  - [ ] Implement click-outside to close dropdown
  - [ ] Display empty state with "No patients found" message
  - [ ] Conditionally show "Create New Patient" button
  - [ ] Add input sanitization regex: `/^[a-zA-Z0-9\s\-'À-ÿ@.]+$/`
- [ ] Implement ARIA combobox pattern:
  - [ ] Add role="combobox" to input
  - [ ] Add aria-expanded attribute (true when dropdown open)
  - [ ] Add aria-activedescendant for keyboard-selected result
  - [ ] Add ARIA live region to announce result count
- [ ] Refactor WalkinBooking.tsx to use shared PatientSearch:
  - [ ] Import PatientSearch component
  - [ ] Remove inline PatientSearchInput
  - [ ] Pass onSelectPatient handler to populate booking form
  - [ ] Set showCreateButton=true
- [ ] Delete PatientSearchInput.tsx (duplicate inline search from US_029)
- [ ] Update api/client.ts searchPatients method to return lastAppointmentDate
- [ ] Apply design tokens from designsystem.md (colors, spacing, typography)
- [ ] Implement responsive dropdown (full-width mobile, max-width desktop)
- [ ] Add z-index layering for dropdown overlay
- [ ] Write unit tests for usePatientSearch hook (mock API, verify debounce)
- [ ] Write unit tests for PatientSearch component (keyboard navigation, empty state)
- [ ] Write unit tests for PatientSearchResult component (display, click handler)
- [ ] Test keyboard-only navigation through search and results
- [ ] Test input sanitization (accents, hyphens, apostrophes allowed)
- [ ] Verify search works in both WalkinBooking and ArrivalManagement pages
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
