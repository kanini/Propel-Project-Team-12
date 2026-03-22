# Task - task_001_fe_provider_browser

## Requirement Reference
- User Story: US_023
- Story Location: .propel/context/tasks/EP-002/us_023/us_023.md
- Acceptance Criteria:
    - AC-1: Display available providers with name, specialty, ratings summary, and next available slot
    - AC-2: Filter updates complete within 300ms showing only matching providers
    - AC-3: Real-time search with 300ms debounce matching provider name and specialty
    - AC-4: Empty state with illustration and CTA to clear filters when no results
- Edge Case:
    - Large provider list (100+ providers) requires pagination with 20 providers per page
    - Providers with no available slots show "No availability" label with "Join Waitlist" option

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-006-provider-browser.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-006 |
| **UXR Requirements** | UXR-004 (Inline search with real-time filtering), UXR-502 (Skeleton loading states) |
| **Design Tokens** | .propel/context/docs/designsystem.md#colors, #typography, #spacing |

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
| Frontend | Tailwind CSS | 3.x |
| Backend | N/A | N/A |
| Database | N/A | N/A |
| Library | React Router | 6.x |
| Library | Axios | 1.x |
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
Implement the Provider and Service Browser UI component for the Patient Portal, enabling patients to search, filter, and browse available healthcare providers. The interface displays providers with essential details (name, specialty, ratings, availability) and supports real-time search with 300ms debounce, filtering by specialty/availability/service type, and pagination for lists exceeding 20 items. Implement all required states (Default, Loading, Empty, Error) with skeleton loading for async data fetching and empty state illustrations for zero results.

## Dependent Tasks
- task_002_be_provider_api.md - Backend API endpoints for provider data retrieval

## Impacted Components
- Frontend (React): New components to be created
  - `src/frontend/src/pages/ProviderBrowser.tsx` (NEW)
  - `src/frontend/src/components/providers/ProviderCard.tsx` (NEW)
  - `src/frontend/src/components/providers/ProviderFilters.tsx` (NEW)
  - `src/frontend/src/components/providers/ProviderSearch.tsx` (NEW)
  - `src/frontend/src/components/common/EmptyState.tsx` (NEW or UPDATE if exists)
  - `src/frontend/src/components/common/Pagination.tsx` (NEW or UPDATE if exists)
  - `src/frontend/src/components/common/SkeletonLoader.tsx` (NEW or UPDATE if exists)
  - `src/frontend/src/store/slices/providerSlice.ts` (NEW) - Redux state management

## Implementation Plan
1. **Create Redux state management for providers**:
   - Define provider state structure (providers list, filters, pagination, loading states)
   - Create async thunks for API calls with 300ms debounce for search
   - Implement filter logic (specialty, availability date, service type) with 300ms update target (UXR-004)

2. **Build core ProviderBrowser page component**:
   - Implement responsive layout with search bar, filter panel, provider grid, and pagination
   - Add skeleton loading state that appears after 300ms (UXR-502)
   - Handle all screen states: Default, Loading, Empty (with illustration + CTA), Error

3. **Create ProviderCard component**:
   - Display provider name, specialty, ratings summary (star rating), next available slot
   - Handle "No availability" state with "Join Waitlist" CTA button
   - Implement card hover/focus states per design tokens
   - Add "Book Appointment" primary action button

4. **Implement ProviderSearch component**:
   - Real-time search input with 300ms debounce (UXR-004)
   - Match against provider name and specialty fields
   - Clear search button when input has value

5. **Develop ProviderFilters component**:
   - Dropdown/checkbox filters for specialty, availability date range, service type
   - "Clear All Filters" button
   - Filter changes trigger provider list update within 300ms (AC-2)

6. **Add pagination logic**:
   - Display 20 providers per page (Edge Case requirement)
   - Previous/Next navigation buttons
   - Page number indicator (e.g., "Showing 1-20 of 150")
   - Scroll to top on page change

7. **Integrate with backend API**:
   - Create API client functions for GET /api/providers with query params (search, filters, page, pageSize)
   - Handle rate limiting and error responses (404, 500)
   - Implement retry logic for failed requests

8. **Responsive design validation**:
   - Test layout at breakpoints: 375px (mobile), 768px (tablet), 1440px (desktop)
   - Ensure touch targets meet 44x44px minimum on mobile (UXR-304)
   - Stack filter panel vertically on mobile

## Current Project State
```
src/frontend/
├── src/
│   ├── pages/
│   │   └── (ProviderBrowser.tsx to be created)
│   ├── components/
│   │   ├── common/
│   │   │   └── (EmptyState.tsx, Pagination.tsx, SkeletonLoader.tsx to be created/updated)
│   │   └── providers/
│   │       └── (ProviderCard.tsx, ProviderFilters.tsx, ProviderSearch.tsx to be created)
│   ├── store/
│   │   └── slices/
│   │       └── (providerSlice.ts to be created)
│   └── api/
│       └── (providerApi.ts to be created)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/pages/ProviderBrowser.tsx | Main provider browser page component with layout and state orchestration |
| CREATE | src/frontend/src/components/providers/ProviderCard.tsx | Reusable provider card displaying name, specialty, ratings, availability |
| CREATE | src/frontend/src/components/providers/ProviderFilters.tsx | Filter panel with specialty, date, service type filters |
| CREATE | src/frontend/src/components/providers/ProviderSearch.tsx | Search input with 300ms debounce for real-time filtering |
| CREATE | src/frontend/src/components/common/EmptyState.tsx | Empty state component with illustration and CTA |
| CREATE | src/frontend/src/components/common/Pagination.tsx | Pagination controls for 20 items per page |
| CREATE | src/frontend/src/components/common/SkeletonLoader.tsx | Skeleton loading component for async data fetching (300ms delay) |
| CREATE | src/frontend/src/store/slices/providerSlice.ts | Redux slice for provider state, filters, pagination, loading |
| CREATE | src/frontend/src/api/providerApi.ts | API client functions for provider endpoints |
| MODIFY | src/frontend/src/App.tsx | Add route for /providers path to ProviderBrowser page |
| MODIFY | src/frontend/src/store/store.ts | Register providerSlice reducer |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [React TypeScript Best Practices 2024](https://react-typescript-cheatsheet.netlify.app/)
- [Redux Toolkit RTK Query Guide](https://redux-toolkit.js.org/rtk-query/overview)
- [Tailwind CSS Responsive Design](https://tailwindcss.com/docs/responsive-design)
- [Debouncing in React with useDebounce](https://usehooks-ts.com/react-hook/use-debounce)
- [WCAG 2.2 AA Focus Indicators](https://www.w3.org/WAI/WCAG22/Understanding/focus-visible)
- [Axios API Client Setup](https://axios-http.com/docs/instance)

## Build Commands
- `npm run dev` - Start Vite development server (http://localhost:5173)
- `npm run build` - Build production bundle
- `npm run test` - Run Vitest unit tests
- `npm run lint` - Run ESLint checks

## Implementation Validation Strategy
- [ ] Unit tests pass for all provider components (ProviderCard, ProviderSearch, ProviderFilters)
- [ ] Redux slice tests verify filter/search/pagination logic
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] Search debounce verified (300ms delay before API call)
- [ ] Filter updates complete within 300ms (UXR-004)
- [ ] Skeleton loader appears after 300ms for API calls (UXR-502)
- [ ] Pagination shows 20 items per page for 100+ providers
- [ ] Empty state displays when no providers match filters
- [ ] "Join Waitlist" button appears for providers with no availability
- [ ] Accessibility: WCAG 2.2 AA compliance verified with axe or WAVE

## Implementation Checklist
- [X] Create Redux providerSlice with state structure (providers, filters, pagination, loading, error)
- [X] Implement async thunks for fetchProviders with 300ms search debounce
- [X] Build ProviderBrowser page component with responsive layout
- [X] Create ProviderCard component displaying name, specialty, ratings, next slot
- [X] Implement ProviderSearch with 300ms debounce (useDebouncedValue hook)
- [X] Build ProviderFilters panel (specialty, availability date, service type)
- [X] Add Pagination component for 20 items per page
- [X] Create SkeletonLoader component with 300ms delay before display
- [X] **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
- [X] Add EmptyState component with illustration and "Clear Filters" CTA
- [X] Integrate API client (GET /api/providers) with query params
- [X] Add routing for /providers path in App.tsx
- [X] Register providerSlice in Redux store
- [ ] Test responsive breakpoints (375px, 768px, 1440px)
- [ ] Verify WCAG 2.2 AA compliance (focus indicators, alt text, ARIA labels)
- [ ] Write unit tests for ProviderCard, ProviderSearch, ProviderFilters
- [ ] Write Redux slice tests for filter/search/pagination logic
- [ ] Manual test with 100+ provider dataset to verify pagination
- [ ] Verify "Join Waitlist" button appears only for providers with no availability
