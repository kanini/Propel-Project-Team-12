# Task - task_001_fe_document_status_ui

## Requirement Reference
- User Story: US_044
- Story Location: .propel/context/tasks/EP-006-I/us_044/us_044.md
- Acceptance Criteria:
    - **AC1**: Given I have uploaded documents, When I navigate to the Document Status page, Then all my documents are listed with name, upload date, file size, and current processing status.
    - **AC2**: Given a document is being processed, When the status changes, Then the status updates in real-time via Pusher Channels without requiring page refresh, and ARIA live regions announce the change for accessibility (UXR-207).
    - **AC3**: Given a document has completed processing, When I view its status, Then a "View Extracted Data" link appears allowing me to see the data extracted from the document.
    - **AC4**: Given no documents have been uploaded, When the page loads, Then an empty state with illustration and "Upload your first document" CTA is displayed.
- Edge Case:
    - What happens when a document is stuck in "Processing" for over 5 minutes? System should show "Processing is taking longer than expected" message with contact support option.
    - How does the system handle documents that fail extraction? Status shows "Failed" with a "Retry" button that re-queues the processing job.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-015-document-status.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-015 |
| **UXR Requirements** | UXR-207, UXR-502, UXR-605 |
| **Design Tokens** | .propel/context/docs/designsystem.md#colors, .propel/context/docs/designsystem.md#typography |

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
| Frontend | Vite | Latest |
| Library | Pusher Channels | pusher-js 8.x |
| Library | React Router | v6/v7 |
| Library | date-fns | 3.x |
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

Implement the Document Status tracking UI (SCR-015) that displays all uploaded clinical documents with real-time status updates via Pusher Channels. This feature enables patients to monitor document processing lifecycle (Uploaded → Processing → Completed/Failed) with accessibility-compliant status announcements, empty state handling, and actionable error recovery options. The component integrates with the existing documentsSlice Redux state from US_042 and subscribes to Pusher status change events from US_043.

**Key Capabilities:**
- Document list table with name, upload date, file size, status columns
- Real-time status updates via Pusher without page refresh
- ARIA live region announcements for status changes (UXR-207)
- Skeleton loading state while fetching initial data (UXR-502)
- Empty state with illustration and "Upload Document" CTA (UXR-605)
- "View Extracted Data" link for completed documents
- "Retry" button for failed documents
- Warning message for processing delays (>5 minutes)
- Status badges with color coding (Uploaded: blue, Processing: amber, Completed: green, Failed: red)

## Dependent Tasks
- US_042: task_001_fe_document_upload_ui (documentsSlice Redux state must exist)
- task_002_be_document_list_api (API endpoint must be available)

## Impacted Components
- **NEW**: `src/frontend/src/components/documents/DocumentStatusList.tsx` - Main status list component
- **NEW**: `src/frontend/src/components/documents/DocumentStatusRow.tsx` - Individual document row
- **NEW**: `src/frontend/src/components/documents/StatusBadge.tsx` - Status badge component
- **NEW**: `src/frontend/src/hooks/usePusherDocumentStatus.ts` - Pusher subscription for status updates
- **NEW**: `src/frontend/src/pages/DocumentStatusPage.tsx` - Page wrapper component
- **MODIFY**: `src/frontend/src/store/documentsSlice.ts` - Add fetchDocuments thunk and status update reducers
- **MODIFY**: `src/frontend/src/api/documentsApi.ts` - Add fetchDocuments API call
- **MODIFY**: `src/frontend/src/App.tsx` - Add route for /documents/status

## Implementation Plan

1. **Enhance documentsSlice for Status Fetching**
   - Add async thunk `fetchUserDocuments()` to fetch all user documents
   - Add reducer for status updates from Pusher events
   - Add selectors for filtering documents by status
   - Follow existing slice patterns from `appointmentsSlice.ts`

2. **Enhance documentsApi for List Endpoint**
   - Implement `fetchDocuments()` → calls GET /api/documents
   - Return DocumentStatus[] with id, fileName, uploadedAt, fileSize, status, processingTime
   - Include error handling and retry logic
   - Follow existing API patterns from `staffApi.ts`

3. **Create usePusherDocumentStatus Hook**
   - Subscribe to Pusher channel: `patient-{userId}-documents`
   - Bind to events: `processing-started`, `processing-completed`, `processing-failed`
   - On event received, dispatch Redux action to update document status
   - Announce status change via ARIA live region
   - Extend existing `usePusherQueue.ts` pattern

4. **Build StatusBadge Component**
   - Display status text with colored background
   - Map statuses to design tokens: Uploaded (blue), Processing (amber), Completed (green), Failed (red)
   - Use Tailwind classes from designsystem.md
   - Include icon for each status (upload, spinner, checkmark, error)
   - Make semantically accessible with sr-only text

5. **Build DocumentStatusRow Component**
   - Display document metadata: name, upload date (formatted), file size (human-readable)
   - Include StatusBadge component
   - Conditional rendering of action buttons:
     - "View Extracted Data" link for Completed status (navigates to EP-006-II feature)
     - "Retry" button for Failed status (re-enqueues processing job)
     - Warning icon + tooltip for Processing >5 minutes
   - Use responsive layout (mobile: stack vertically, desktop: table row)
   - Reference wireframe SCR-015 for exact layout

6. **Build DocumentStatusList Component**
   - Fetch documents on mount using `useEffect` + `fetchUserDocuments` thunk
   - Subscribe to Pusher status updates using `usePusherDocumentStatus` hook
   - Display skeleton loader while loading (UXR-502)
   - Map documents to DocumentStatusRow components
   - Implement ARIA live region for status announcements (UXR-207)
   - Handle empty state: show EmptyState component with illustration and CTA
   - Sort documents by upload date (newest first)

7. **Add Retry Functionality**
   - Implement `retryDocumentProcessing(documentId)` API call
   - POST /api/documents/{documentId}/retry endpoint
   - On success: update status to "Processing" and show toast notification
   - On error: show error toast with actionable message
   - Follow existing error handling patterns

8. **Implement Accessibility Features**
   - Add ARIA live region with `aria-live="polite"` for status updates
   - Announce status changes: "{fileName} is now {status}"
   - Ensure keyboard navigation support (tab through rows and buttons)
   - Add focus indicators (UXR-202)
   - Use semantic HTML table for document list
   - Test with screen reader (NVDA/JAWS)

## Current Project State

```
src/frontend/src/
├── api/
│   ├── documentsApi.ts (from US_042)
│   └── staffApi.ts
├── components/
│   ├── appointments/
│   ├── common/
│   │   ├── EmptyState.tsx
│   │   └── SkeletonLoader.tsx
│   └── documents/ (from US_042)
│       ├── DocumentUpload.tsx
│       └── FileDropZone.tsx
├── hooks/
│   ├── usePusherQueue.ts
│   └── usePusherUpload.ts (from US_042)
├── pages/
│   ├── PatientDashboard.tsx
│   └── DocumentUploadPage.tsx (from US_042)
├── store/
│   ├── documentsSlice.ts (from US_042)
│   └── rootReducer.ts
└── App.tsx
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/documents/DocumentStatusList.tsx | Main status list with Pusher integration |
| CREATE | src/frontend/src/components/documents/DocumentStatusRow.tsx | Individual document row with actions |
| CREATE | src/frontend/src/components/documents/StatusBadge.tsx | Status indicator badge |
| CREATE | src/frontend/src/hooks/usePusherDocumentStatus.ts | Pusher hook for status updates |
| CREATE | src/frontend/src/pages/DocumentStatusPage.tsx | Page wrapper for status list |
| MODIFY | src/frontend/src/store/documentsSlice.ts | Add fetchUserDocuments thunk and status reducers |
| MODIFY | src/frontend/src/api/documentsApi.ts | Add fetchDocuments and retryProcessing API calls |
| MODIFY | src/frontend/src/App.tsx | Add /documents/status route |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Pusher Channels Documentation
- **Event Handling**: https://pusher.com/docs/channels/using_channels/events/
- **React Integration**: https://pusher.com/docs/channels/getting_started/javascript/

### React Documentation
- **useEffect Hook**: https://react.dev/reference/react/useEffect
- **Conditional Rendering**: https://react.dev/learn/conditional-rendering

### Accessibility
- **ARIA Live Regions**: https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions
- **Table Accessibility**: https://www.w3.org/WAI/tutorials/tables/

### Date Formatting
- **date-fns format**: https://date-fns.org/v3.3.1/docs/format
- **Relative Time**: https://date-fns.org/v3.3.1/docs/formatDistance

### Existing Codebase Patterns
- **Redux Slice Pattern**: `src/frontend/src/store/documentsSlice.ts`
- **Pusher Hook Pattern**: `src/frontend/src/hooks/usePusherQueue.ts`
- **Empty State Pattern**: `src/frontend/src/components/common/EmptyState.tsx`
- **Skeleton Loader**: `src/frontend/src/components/common/SkeletonLoader.tsx`

## Build Commands
```powershell
# Install date-fns for date formatting (if not already installed)
cd src/frontend
npm install date-fns@3

# Run development server
npm run dev

# Run type checking
npm run type-check

# Run linting
npm run lint

# Build for production
npm run build
```

## Implementation Validation Strategy
- [ ] Unit tests pass (DocumentStatusList, StatusBadge, row rendering)
- [ ] Integration tests pass (Pusher status updates, API fetching, retry functionality)
- [x] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [x] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] WCAG 2.2 AA compliance verified (ARIA live regions, keyboard navigation, screen reader)
- [ ] Pusher events received and update status correctly
- [ ] Empty state displays when no documents exist
- [ ] Skeleton loader shows during initial fetch
- [ ] Retry button re-enqueues processing job successfully
- [ ] Warning message appears for processing >5 minutes
- [ ] "View Extracted Data" link appears for completed documents

## Implementation Checklist
- [ ] Enhance documentsSlice with fetchUserDocuments thunk and status update reducers
- [ ] Add fetchDocuments and retryProcessing API calls to documentsApi
- [ ] Create usePusherDocumentStatus hook subscribing to status change events
- [ ] Build StatusBadge component with color-coded status indicators
- [ ] Build DocumentStatusRow with conditional action buttons and warning messages
- [ ] Build DocumentStatusList with skeleton loading, empty state, and ARIA live region
- [ ] Add /documents/status route to App.tsx
- [ ] Implement retry functionality with error handling
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
