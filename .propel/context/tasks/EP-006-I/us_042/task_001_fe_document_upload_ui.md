# Task - task_001_fe_document_upload_ui

## Requirement Reference
- User Story: US_042
- Story Location: .propel/context/tasks/EP-006-I/us_042/us_042.md
- Acceptance Criteria:
    - **AC1**: Given I am on the Document Upload page, When I drag-and-drop or select a PDF file, Then the system validates file format (PDF only) and size (max 10MB) before initiating upload.
    - **AC2**: Given the file is valid, When the upload begins, Then a real-time progress bar updates continuously via Pusher Channels showing percentage complete using chunked upload (TR-022).
    - **AC3**: Given the upload completes successfully, When the file is stored, Then a confirmation message displays with the document name, size, and status "Uploaded — Processing pending".
    - **AC4**: Given the file is invalid, When I select a non-PDF or oversized file, Then inline validation displays "Only PDF files up to 10MB are supported" without initiating the upload.
- Edge Case:
    - What happens when the upload is interrupted (network drop)? Chunked upload allows resume from the last successful chunk; user sees "Upload paused — Retrying..." message.
    - How does the system handle simultaneous uploads of multiple documents? Each upload tracks progress independently with individual progress bars.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-014-document-upload.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-014 |
| **UXR Requirements** | UXR-201, UXR-207, UXR-301, UXR-503, UXR-601 |
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

Implement the Clinical Document Upload UI component (SCR-014) with drag-and-drop support, real-time progress tracking via Pusher Channels, and comprehensive inline validation. This feature enables patients to upload PDF clinical documents up to 10MB with visual feedback throughout the upload lifecycle (validating → uploading → complete/error). The component must support multiple simultaneous uploads with independent progress tracking per file and graceful handling of network interruptions with resume capability.

**Key Capabilities:**
- Drag-and-drop file zone with hover states
- File format (PDF only) and size (max 10MB) validation
- Real-time progress bar updates via Pusher Channels (chunked upload)
- Simultaneous multi-file uploads with individual progress indicators
- Success confirmation with document metadata display
- Error states with actionable recovery instructions
- Network interruption handling with resume support

## Dependent Tasks
- None (this is the first task in the epic)

## Impacted Components
- **NEW**: `src/frontend/src/components/documents/DocumentUpload.tsx` - Main upload component
- **NEW**: `src/frontend/src/components/documents/FileDropZone.tsx` - Drag-drop zone component
- **NEW**: `src/frontend/src/components/documents/UploadProgressBar.tsx` - Progress indicator component
- **NEW**: `src/frontend/src/hooks/usePusherUpload.ts` - Custom hook for Pusher upload events
- **NEW**: `src/frontend/src/store/documentsSlice.ts` - Redux state management for documents
- **NEW**: `src/frontend/src/api/documentsApi.ts` - API client for document upload endpoints
- **MODIFY**: `src/frontend/src/store/rootReducer.ts` - Add documentsSlice to root reducer
- **MODIFY**: `src/frontend/src/pages/PatientDashboard.tsx` - Add route to Document Upload page

## Implementation Plan

1. **Create Documents Redux Slice**
   - Define state interface for document uploads (files, progress, status, errors)
   - Implement async thunks for chunked upload initiation and chunk submission
   - Add reducers for progress updates, completion, failure, and retry logic
   - Follow existing pattern from `authSlice.ts` for error handling

2. **Create Documents API Client**
   - Implement `initializeChunkedUpload(file: File)` → returns upload session ID
   - Implement `uploadChunk(sessionId, chunk, chunkIndex)` → uploads chunk with retry logic
   - Implement `finalizeUpload(sessionId)` → completes upload and returns document metadata
   - Use FormData for file chunks (reference existing API patterns in `staffApi.ts`)
   - Include exponential backoff retry logic (3 retries with 1s, 2s, 4s delays)
   - Follow `providerApi.ts` error handling pattern

3. **Create Pusher Upload Hook**
   - Extend existing `usePusherQueue.ts` pattern for upload progress events
   - Subscribe to channel: `document-upload-{sessionId}`
   - Bind to events: `chunk-uploaded`, `upload-complete`, `upload-failed`, `upload-paused`
   - Update Redux state on each Pusher event received
   - Implement graceful degradation (fallback to polling if Pusher unavailable)

4. **Build FileDropZone Component**
   - Implement drag-and-drop event handlers (dragover, dragleave, drop)
   - Show visual hover state on dragover (refer to designsystem.md colors)
   - Validate file on drop: check MIME type (`application/pdf`) and size (<= 10MB)
   - Display inline error messages for invalid files (UXR-601)
   - Support file input fallback (click to browse)
   - Reference wireframe for exact layout and spacing

5. **Build UploadProgressBar Component**
   - Create progress bar similar to `ProgressIndicator.tsx` but horizontal
   - Display percentage complete (0-100%)
   - Show current status: "Validating...", "Uploading X%", "Processing...", "Complete", "Error"
   - Use Tailwind classes for progress styling (reference designsystem.md#colors)
   - Implement ARIA live region for screen reader announcements (UXR-207)

6. **Build DocumentUpload Container Component**
   - Integrate FileDropZone, UploadProgressBar, and status messages
   - Manage multiple simultaneous uploads (array of upload sessions)
   - Trigger Redux actions on file selection
   - Display success confirmation with document name, size, status
   - Show error states with actionable recovery ("Retry Upload", "Remove File")
   - Implement responsive layout (mobile: stack vertically, desktop: side-by-side)
   - Reference wireframe SCR-014 for all states: Default, Loading, Error, Validation

7. **Add Routing and Navigation**
   - Add route `/documents/upload` in React Router configuration
   - Add navigation link from Patient Dashboard (modify `PatientDashboard.tsx`)
   - Follow existing routing pattern from appointment features

8. **Implement Accessibility Features**
   - Add ARIA labels to all interactive elements
   - Ensure keyboard navigation support (tab, enter, space)
   - Add focus indicators (UXR-202)
   - Use semantic HTML landmarks (UXR-206)
   - Test with screen reader (NVDA/JAWS)

## Current Project State

```
src/frontend/src/
├── api/
│   ├── appointmentsApi.ts
│   ├── authApi.ts
│   ├── providerApi.ts
│   ├── staffApi.ts
│   └── waitlistApi.ts
├── components/
│   ├── appointments/
│   │   ├── AppointmentCard.tsx
│   │   ├── BookingSteps.tsx
│   │   ├── ProgressIndicator.tsx
│   │   └── TimeSlotGrid.tsx
│   ├── common/
│   │   ├── EmptyState.tsx
│   │   ├── Pagination.tsx
│   │   └── SkeletonLoader.tsx
│   ├── forms/
│   │   └── PasswordStrengthIndicator.tsx
│   └── shared/
│       └── PatientSearch/
├── features/
│   ├── auth/
│   │   ├── authSlice.ts
│   │   └── components/
│   │       └── RegistrationForm.tsx
│   └── appointments/
│       └── appointmentsSlice.ts
├── hooks/
│   ├── useAuth.ts
│   ├── useDebounce.ts
│   ├── usePatientSearch.ts
│   ├── usePusherQueue.ts
│   └── useSessionTimeout.ts
├── pages/
│   ├── PatientDashboard.tsx
│   ├── LoginPage.tsx
│   └── AppointmentBookingPage.tsx
├── store/
│   ├── index.ts
│   └── rootReducer.ts
├── utils/
│   └── validators.ts
├── App.tsx
└── main.tsx
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/documents/DocumentUpload.tsx | Main container component managing upload lifecycle |
| CREATE | src/frontend/src/components/documents/FileDropZone.tsx | Drag-and-drop zone with validation |
| CREATE | src/frontend/src/components/documents/UploadProgressBar.tsx | Progress indicator with Pusher integration |
| CREATE | src/frontend/src/hooks/usePusherUpload.ts | Custom hook for real-time upload events via Pusher |
| CREATE | src/frontend/src/store/documentsSlice.ts | Redux slice for document state management |
| CREATE | src/frontend/src/api/documentsApi.ts | API client for chunked upload endpoints |
| CREATE | src/frontend/src/pages/DocumentUploadPage.tsx | Page wrapper for DocumentUpload component |
| MODIFY | src/frontend/src/store/rootReducer.ts | Add documentsSlice reducer to combineReducers |
| MODIFY | src/frontend/src/App.tsx | Add route for /documents/upload |
| MODIFY | src/frontend/src/pages/PatientDashboard.tsx | Add navigation link to Document Upload |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Pusher Channels Documentation
- **Pusher React Integration**: https://pusher.com/docs/channels/getting_started/javascript/
- **Channel Subscription Pattern**: https://pusher.com/docs/channels/using_channels/channels/
- **Event Binding**: https://pusher.com/docs/channels/using_channels/events/

### React Documentation
- **File Input Handling**: https://react.dev/reference/react-dom/components/input#reading-the-files-when-submitting-the-form
- **Drag and Drop API**: https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API
- **FormData API**: https://developer.mozilla.org/en-US/docs/Web/API/FormData

### Redux Toolkit
- **Async Thunks**: https://redux-toolkit.js.org/api/createAsyncThunk
- **RTK Best Practices**: https://redux-toolkit.js.org/usage/usage-guide

### Accessibility
- **ARIA Live Regions**: https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions
- **WCAG 2.2 File Upload**: https://www.w3.org/WAI/WCAG22/Understanding/file-upload

### Existing Codebase Patterns
- **API Client Pattern**: `src/frontend/src/api/staffApi.ts` (error handling, retry logic)
- **Pusher Hook Pattern**: `src/frontend/src/hooks/usePusherQueue.ts` (connection management)
- **Form Validation Pattern**: `src/frontend/src/features/auth/components/RegistrationForm.tsx`
- **Progress Indicator Pattern**: `src/frontend/src/components/appointments/ProgressIndicator.tsx`
- **Redux Slice Pattern**: `src/frontend/src/features/auth/authSlice.ts`

## Build Commands
```powershell
# Install dependencies (if pusher-js not already installed)
cd src/frontend
npm install pusher-js@8

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
- [ ] Unit tests pass (document upload component, file validation, progress updates)
- [ ] Integration tests pass (upload flow, Pusher event handling, Redux state updates)
- [x] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [x] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] WCAG 2.2 AA compliance verified (screen reader, keyboard navigation)
- [ ] Pusher events received and processed correctly (test with real backend)
- [ ] File validation works (PDF only, max 10MB)
- [ ] Multiple simultaneous uploads tracked independently
- [ ] Network interruption handled gracefully (pause/resume)
- [ ] Error states display actionable messages
- [ ] Success confirmation shows document metadata

## Implementation Checklist
- [ ] Create documentsSlice.ts with upload state management (status, progress, error)
- [ ] Implement async thunks for initializeChunkedUpload, uploadChunk, finalizeUpload
- [ ] Create documentsApi.ts with chunked upload endpoints and retry logic
- [ ] Build usePusherUpload.ts hook subscribing to upload progress events
- [ ] Implement FileDropZone component with drag-drop, file validation, error display
- [ ] Implement UploadProgressBar component with ARIA live regions and Pusher updates
- [ ] Build DocumentUpload container managing multi-file uploads, success/error states
- [ ] Add /documents/upload route and navigation from Patient Dashboard
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
