# Task - task_002_fe_conflict_alerts_ui

## Requirement Reference
- User Story: US_048
- Story Location: .propel/context/tasks/EP-007/us_048/us_048.md
- Acceptance Criteria:
    - **AC2**: Given critical conflicts are detected, When viewing the patient profile, Then conflict alerts are prominently displayed at the top of the view with count, severity indicators, and links to resolution workflow.
    - **AC3**: Given a conflict involves medications, When the conflict is categorized, Then medication conflicts are marked as "Critical" (highest severity) due to patient safety implications.
- Edge Case:
    - What happens when a patient has 50+ data points with multiple conflicts? Conflicts are prioritized by severity (Critical > Warning > Info) and paginated.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A (Wireframe-based) |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-017-staff-patient-view.html |
| **Screen Spec** | figma_spec.md#SCR-017 |
| **UXR Requirements** | UXR-402 (AI-suggested amber vs. verified green badges) |
| **Design Tokens** | designsystem.md - Red-600 for Critical conflicts, Amber-500 for Warning, Blue-400 for Info |

> IF Wireframe Status = AVAILABLE or EXTERNAL:
> - **MUST** open and reference the wireframe file/URL during UI implementation
> - **MUST** match layout, spacing, typography, and colors from the wireframe
> - **MUST** implement all states shown in wireframe (default, hover, focus, error, loading)
> - **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Frontend | TypeScript | 5.x |
| Frontend | Redux Toolkit | 2.x |
| Frontend | Tailwind CSS | Latest |
| Library | React Router | v7 |
| Library | Pusher Channels (pusher-js) | 8.x |
| Real-time | Pusher Channels | 8.x |
| Backend | N/A | N/A |
| Database | N/A | N/A |
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

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement conflict alerts UI for Staff Patient View (SCR-017) displaying critical data conflicts at the top of the patient profile with severity-based visual indicators, conflict counts, and resolution workflow links. This task creates ConflictAlertBanner component (prominently displayed at page top), ConflictList component (paginated list with severity badges), ConflictDetailModal component (shows source references and resolution options), Redux slice for conflict state management, Pusher real-time listener for conflict notifications, and API integration for fetching and resolving conflicts. The UI follows UXR-402 design pattern (amber for AI-suggested, green for staff-verified) and implements responsive layouts per wireframe SCR-017.

**Key Capabilities:**
- ConflictAlertBanner (top-of-page alert with count and severity breakdown)
- ConflictList (scrollable, paginated, severity-sorted)
- ConflictDetailModal (source references, resolution workflow)
- SeverityBadge (Critical=red, Warning=amber, Info=blue)
- Real-time Pusher notifications for new conflicts
- Pagination for 50+ conflicts
- Accessibility (ARIA live regions, keyboard navigation)
- Responsive design (375px, 768px, 1440px breakpoints)

## Dependent Tasks
- EP-007: US_048: task_001_be_conflict_detection_service (Conflicts API endpoints)
- EP-007: US_047: task_003_be_aggregation_service (Pusher notifications)

## Impacted Components
- **NEW**: `src/frontend/src/components/ConflictAlertBanner.tsx` - Top-level conflict alert
- **NEW**: `src/frontend/src/components/ConflictList.tsx` - Paginated conflict list
- **NEW**: `src/frontend/src/components/ConflictDetailModal.tsx` - Conflict resolution modal
- **NEW**: `src/frontend/src/components/SeverityBadge.tsx` - Severity indicator badge
- **NEW**: `src/frontend/src/store/slices/conflictsSlice.ts` - Redux state management
- **NEW**: `src/frontend/src/api/conflictsApi.ts` - RTK Query API
- **NEW**: `src/frontend/src/hooks/usePusherConflicts.ts` - Pusher real-time hook
- **NEW**: `src/frontend/src/types/conflict.types.ts` - TypeScript types
- **MODIFY**: `src/frontend/src/pages/StaffPatientView.tsx` - Add ConflictAlertBanner
- **NEW**: `src/frontend/src/__tests__/components/ConflictAlertBanner.test.tsx` - Unit tests

## Implementation Plan

1. **Create TypeScript Types**
   - Define types in `conflict.types.ts`:
     ```typescript
     export enum ConflictSeverity {
       Critical = 'Critical',
       Warning = 'Warning',
       Info = 'Info'
     }
     
     export enum ResolutionStatus {
       Unresolved = 'Unresolved',
       Resolved = 'Resolved',
       Dismissed = 'Dismissed'
     }
     
     export interface DataConflict {
       id: string;
       patientProfileId: number;
       conflictType: string;
       entityType: string;
       entityId: string;
       description: string;
       severity: ConflictSeverity;
       sourceDataIds: string[];
       resolutionStatus: ResolutionStatus;
       resolvedBy?: number;
       resolvedAt?: string;
       createdAt: string;
     }
     
     export interface ConflictSummary {
       totalUnresolved: number;
       criticalCount: number;
       warningCount: number;
       infoCount: number;
       oldestConflictDate?: string;
     }
     ```

2. **Create RTK Query API**
   - File: `conflictsApi.ts`
   - Endpoints:
     ```typescript
     export const conflictsApi = createApi({
       reducerPath: 'conflictsApi',
       baseQuery: fetchBaseQuery({ baseUrl: '/api' }),
       endpoints: (builder) => ({
         getConflictsByPatient: builder.query<DataConflict[], { patientId: number; severity?: ConflictSeverity; page?: number; pageSize?: number }>({
           query: ({ patientId, severity, page = 1, pageSize = 10 }) => 
             `/patients/${patientId}/conflicts?severity=${severity ?? ''}&page=${page}&pageSize=${pageSize}`,
         }),
         getConflictSummary: builder.query<ConflictSummary, number>({
           query: (patientId) => `/patients/${patientId}/conflicts/summary`,
         }),
         resolveConflict: builder.mutation<DataConflict, { conflictId: string; resolution: string; chosenEntityId?: string }>({
           query: ({ conflictId, resolution, chosenEntityId }) => ({
             url: `/conflicts/${conflictId}/resolve`,
             method: 'POST',
             body: { resolution, chosenEntityId },
           }),
         }),
       }),
     });
     ```

3. **Create SeverityBadge Component**
   - Props: severity (ConflictSeverity)
   - Styling based on severity:
     - Critical: `bg-red-100 text-red-800 border-red-300`
     - Warning: `bg-amber-100 text-amber-800 border-amber-300`
     - Info: `bg-blue-100 text-blue-800 border-blue-300`
   - Icon: Use appropriate Heroicons (ExclamationTriangleIcon for Critical, ExclamationCircleIcon for Warning, InformationCircleIcon for Info)
   - Accessibility: aria-label with severity level

4. **Create ConflictAlertBanner Component**
   - Props: patientId (number), summary (ConflictSummary), onViewConflicts (callback)
   - Layout:
     - Display at top of StaffPatientView (above patient profile content)
     - Background: Red-50 for Critical, Amber-50 for Warning, Blue-50 for Info (based on highest severity)
     - Left section: Icon + "Critical Conflicts Detected" heading
     - Center section: Counts (e.g., "3 Critical, 5 Warning, 2 Info")
     - Right section: "View All Conflicts" button
   - Conditional rendering: Only show if totalUnresolved > 0
   - Click handler: Opens ConflictList (scroll to conflict section or modal)
   - ARIA: role="alert" for Critical severity, aria-live="polite" for others

5. **Create ConflictList Component**
   - Props: patientId (number), severityFilter (ConflictSeverity?), page (number), onResolve (callback)
   - Fetch conflicts using RTK Query: `useGetConflictsByPatientQuery({ patientId, severity: severityFilter, page, pageSize: 10 })`
   - Display:
     - List header with severity filter tabs (All, Critical, Warning, Info)
     - Each conflict row:
       * SeverityBadge on left
       * Conflict description (truncated to 100 chars)
       * Entity type badge (Medication, Allergy, Diagnosis)
       * Timestamp (relative: "2 hours ago")
       * "Resolve" button on right
     - Pagination controls at bottom (Previous, Page X of Y, Next)
   - Sort: Always sort by severity (Critical first), then by createdAt (newest first)
   - Empty state: "No conflicts detected" with green checkmark icon
   - Loading: Skeleton rows (3 placeholder rows)

6. **Create ConflictDetailModal Component**
   - Props: conflict (DataConflict), onClose (callback), onResolve (callback)
   - Layout:
     - Header: Conflict type + SeverityBadge
     - Body:
       * Description section (full text)
       * Source references section (list of source documents with links)
       * Entity comparison (if applicable: e.g., "Document A: 10mg" vs "Document B: 20mg")
     - Footer:
       * Resolution text area (staff enters resolution notes)
       * "Choose Document A" / "Choose Document B" buttons (if applicable)
       * "Dismiss Conflict" button (ResolutionStatus = Dismissed)
       * "Close" and "Resolve" buttons
   - On resolve: Call `resolveConflict` mutation, close modal, refetch conflicts
   - Accessibility: Focus trap, ESC to close, ARIA labelledby/describedby

7. **Create Conflicts Redux Slice**
   - File: `conflictsSlice.ts`
   - State:
     ```typescript
     interface ConflictsState {
       selectedConflict: DataConflict | null;
       isModalOpen: boolean;
       filterSeverity: ConflictSeverity | null;
       currentPage: number;
     }
     ```
   - Actions:
     - openConflictModal(conflict: DataConflict)
     - closeConflictModal()
     - setFilterSeverity(severity: ConflictSeverity | null)
     - setCurrentPage(page: number)

8. **Create usePusherConflicts Hook**
   - Subscribe to Pusher channel: `private-patient-{patientId}`
   - Listen for event: `critical-conflict-detected`
   - On event:
     - Show toast notification: "Critical conflict detected"
     - Invalidate RTK Query cache for conflicts (trigger refetch)
     - If ConflictAlertBanner is visible, update counts
   - Cleanup: Unsubscribe on unmount

9. **Integrate ConflictAlertBanner into StaffPatientView**
   - Add ConflictAlertBanner at top of page (before patient profile sections)
   - Fetch conflict summary using `useGetConflictSummaryQuery(patientId)`
   - Conditionally render only if totalUnresolved > 0
   - Wire up "View All Conflicts" button to scroll to ConflictList section or open modal

10. **Implement Responsive Design**
    - Reference wireframe SCR-017 for layouts at breakpoints:
      - 375px (mobile): Stack conflict list, hide descriptions, show only counts
      - 768px (tablet): 2-column layout, truncated descriptions
      - 1440px (desktop): 3-column layout, full descriptions
    - Use Tailwind responsive classes: `hidden sm:block md:flex lg:grid-cols-3`
    - Test on actual devices/browser DevTools

11. **Add Accessibility Features**
    - ARIA live regions for alert updates
    - Keyboard navigation: Tab through conflicts, Enter to open detail modal
    - Focus management: Focus "Resolve" button when modal opens, return focus on close
    - Screen reader announcements: "X critical conflicts detected"
    - Color contrast: WCAG AA compliance (red-600 on red-50, amber-600 on amber-50)
    - Skip link: "Skip to conflicts section"

## Current Project State

```
src/frontend/
├── src/
│   ├── components/
│   │   └── (existing components)
│   ├── pages/
│   │   └── StaffPatientView.tsx (exists, to be enhanced)
│   ├── store/
│   │   ├── index.ts
│   │   └── slices/
│   ├── api/
│   │   └── (existing API files)
│   ├── hooks/
│   │   └── (existing hooks)
│   └── types/
│       └── (existing types)
└── __tests__/
    └── components/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/ConflictAlertBanner.tsx | Top-level conflict alert component |
| CREATE | src/frontend/src/components/ConflictList.tsx | Paginated conflict list component |
| CREATE | src/frontend/src/components/ConflictDetailModal.tsx | Conflict resolution modal |
| CREATE | src/frontend/src/components/SeverityBadge.tsx | Severity indicator badge |
| CREATE | src/frontend/src/store/slices/conflictsSlice.ts | Redux state management |
| CREATE | src/frontend/src/api/conflictsApi.ts | RTK Query API endpoints |
| CREATE | src/frontend/src/hooks/usePusherConflicts.ts | Pusher real-time hook |
| CREATE | src/frontend/src/types/conflict.types.ts | TypeScript types |
| CREATE | src/frontend/src/__tests__/components/ConflictAlertBanner.test.tsx | Unit tests |
| MODIFY | src/frontend/src/pages/StaffPatientView.tsx | Add ConflictAlertBanner integration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### React Documentation
- **Hooks**: https://react.dev/reference/react
- **TypeScript**: https://react.dev/learn/typescript

### Redux Toolkit Documentation
- **RTK Query**: https://redux-toolkit.js.org/rtk-query/overview
- **Slices**: https://redux-toolkit.js.org/api/createSlice

### Tailwind CSS Documentation
- **Responsive Design**: https://tailwindcss.com/docs/responsive-design
- **Colors**: https://tailwindcss.com/docs/customizing-colors

### Pusher Documentation
- **Channels**: https://pusher.com/docs/channels/getting_started/javascript/
- **React Integration**: https://pusher.com/docs/channels/getting_started/react/

### Accessibility Guidelines
- **WCAG 2.1**: https://www.w3.org/WAI/WCAG21/quickref/
- **ARIA Live Regions**: https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions

### Design Requirements
- **FR-031**: System MUST explicitly highlight critical data conflicts requiring staff verification (spec.md)
- **AIR-006**: System MUST detect and highlight critical data conflicts for staff verification (design.md)
- **UXR-402**: System MUST visually distinguish AI-suggested data (amber badge) from staff-verified data (green badge) (figma_spec.md)
- **SCR-017**: Staff Patient View with 360° + verification UI (figma_spec.md)

### Existing Codebase Patterns
- **Component Pattern**: `src/frontend/src/components/DocumentUpload.tsx`
- **Redux Slice Pattern**: `src/frontend/src/store/slices/documentsSlice.ts`
- **RTK Query Pattern**: `src/frontend/src/api/documentsApi.ts`
- **Pusher Hook Pattern**: `src/frontend/src/hooks/usePusherUpload.ts`

## Build Commands
```powershell
# Build frontend
cd src/frontend
npm run build

# Run dev server
npm run dev

# Run tests
npm test ConflictAlertBanner

# Run all tests
npm test

# Type check
npm run type-check

# Lint
npm run lint
```

## Implementation Validation Strategy
- [ ] Unit tests pass (ConflictAlertBanner, ConflictList, SeverityBadge)
- [ ] Integration tests pass (RTK Query API, Pusher notifications)
- [ ] **[UI Tasks]** Visual comparison against wireframe SCR-017 completed at 375px, 768px, 1440px
- [ ] ConflictAlertBanner displays at top of StaffPatientView
- [ ] Severity badges use correct colors (red for Critical, amber for Warning, blue for Info)
- [ ] Pagination works for 50+ conflicts
- [ ] Pusher real-time notifications trigger conflict list refetch
- [ ] Conflict resolution modal opens and closes correctly
- [ ] Resolve mutation updates conflict status and refetches list
- [ ] ARIA live regions announce conflict updates
- [ ] Keyboard navigation works (Tab, Enter, ESC)
- [ ] Color contrast meets WCAG AA standards
- [ ] Responsive layouts match wireframe at all breakpoints
- [ ] Empty state displays when no conflicts exist
- [ ] Loading skeleton displays during API fetch

## Implementation Checklist
- [ ] Create conflict.types.ts with ConflictSeverity and DataConflict types
- [ ] Create conflictsApi.ts with RTK Query endpoints (GET, POST)
- [ ] Create SeverityBadge component with severity-based styling
- [ ] Create ConflictAlertBanner component with count display and "View All" button
- [ ] Create ConflictList component with pagination and severity filtering
- [ ] Create ConflictDetailModal component with source references and resolution workflow
- [ ] Create conflictsSlice.ts with Redux state management
- [ ] Create usePusherConflicts hook for real-time notifications
- [ ] Integrate ConflictAlertBanner into StaffPatientView.tsx
- [ ] Implement responsive design for 375px, 768px, 1440px breakpoints
- [ ] Add ARIA live regions and keyboard navigation for accessibility
- [ ] Write unit tests for ConflictAlertBanner, ConflictList, SeverityBadge
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe SCR-017 during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
