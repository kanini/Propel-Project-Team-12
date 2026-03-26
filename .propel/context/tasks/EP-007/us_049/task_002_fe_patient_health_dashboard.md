# Task - task_002_fe_patient_health_dashboard

## Requirement Reference
- User Story: US_049
- Story Location: .propel/context/tasks/EP-007/us_049/us_049.md
- Acceptance Criteria:
    - **AC1**: Given I am a patient on the Health Dashboard, When the page loads, Then my 360-Degree Patient View displays demographics, active conditions, current medications, allergies, vital trends (chart), and recent encounters within 2 seconds (NFR-002).
    - **AC2**: Given I am a Staff member viewing a patient, When the 360-Degree View loads, Then each data element displays an amber badge (AI-suggested) or green badge (staff-verified) per UXR-402.
    - **AC4**: Given the patient has no clinical data, When the dashboard loads, Then an empty state shows with "Upload your clinical documents to build your health profile" CTA.
    - **AC5**: Given the patient view is read-only for patients, When a patient views their dashboard, Then no edit or verification buttons are visible — only Staff sees verification actions.
- Edge Case:
    - What happens when vital trend data spans multiple years? Chart displays a scrollable timeline with zoom controls defaulting to the last 12 months.
    - How does the system handle partially verified data? Sections show a completion bar indicating percentage verified with "X of Y items verified" count.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A (Wireframe-based) |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-016-patient-health-dashboard.html |
| **Screen Spec** | figma_spec.md#SCR-016 |
| **UXR Requirements** | UXR-402 (AI vs. verified badges), UXR-103 (Tooltips), UXR-502 (Skeleton loading), UXR-605 (Empty states) |
| **Design Tokens** | designsystem.md - Amber-500 for AI-suggested, Green-600 for staff-verified, Blue-50 for section backgrounds |

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
| Library | Chart.js / Recharts | Latest |
| Library | React Testing Library | Latest |
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

Implement 360-Degree Patient Health Dashboard (SCR-016) displaying consolidated patient profile with demographics, conditions, medications, allergies, vital trends chart, and recent encounters. This task creates PatientHealthDashboard page component (top-level container), section components (DemographicsSection, ConditionsSection, MedicationsSection, AllergiesSection, VitalTrendsChart, EncountersSection), VerificationBadge component (amber for AI-suggested, green for staff-verified), Redux slice for profile state management, RTK Query API integration, skeleton loading states (UXR-502), empty state with CTA (UXR-605), role-based UI rendering (Patient: read-only, Staff: verification actions), and responsive design per wireframe SCR-016. The implementation targets 2-second load time with optimized API calls and progressive rendering.

**Key Capabilities:**
- PatientHealthDashboard page (6 logical sections)
- DemographicsSection (name, DOB, gender, contact info)
- ConditionsSection (list with verification badges, completion bar)
- MedicationsSection (list with dosage, verification badges, completion bar)
- AllergiesSection (list with severity, verification badges, completion bar)
- VitalTrendsChart (line chart, scrollable timeline, zoom controls, 12-month default)
- EncountersSection (recent 10 encounters, date/type/provider)
- VerificationBadge (amber/green with UXR-402 styling)
- Skeleton loading (UXR-502)
- Empty state with "Upload documents" CTA
- Responsive design (375px, 768px, 1440px)

## Dependent Tasks
- EP-007: US_049: task_001_be_patient_profile_api (API endpoint)

## Impacted Components
- **NEW**: `src/frontend/src/pages/PatientHealthDashboard.tsx` - Main dashboard page
- **NEW**: `src/frontend/src/components/patient-profile/DemographicsSection.tsx` - Demographics display
- **NEW**: `src/frontend/src/components/patient-profile/ConditionsSection.tsx` - Conditions list
- **NEW**: `src/frontend/src/components/patient-profile/MedicationsSection.tsx` - Medications list
- **NEW**: `src/frontend/src/components/patient-profile/AllergiesSection.tsx` - Allergies list
- **NEW**: `src/frontend/src/components/patient-profile/VitalTrendsChart.tsx` - Vital signs chart
- **NEW**: `src/frontend/src/components/patient-profile/EncountersSection.tsx` - Recent encounters
- **NEW**: `src/frontend/src/components/VerificationBadge.tsx` - Badge component
- **NEW**: `src/frontend/src/components/ProfileCompletionBar.tsx` - Completion indicator
- **NEW**: `src/frontend/src/store/slices/patientProfileSlice.ts` - Redux state
- **NEW**: `src/frontend/src/api/patientProfileApi.ts` - RTK Query API
- **NEW**: `src/frontend/src/types/patientProfile.types.ts` - TypeScript types
- **NEW**: `src/frontend/src/__tests__/pages/PatientHealthDashboard.test.tsx` - Unit tests

## Implementation Plan

1. **Create TypeScript Types**
   - Define types in `patientProfile.types.ts`:
     ```typescript
     export enum VerificationBadge {
       AISuggested = 'AI-suggested',
       StaffVerified = 'Staff-verified'
     }
     
     export interface PatientProfile360 {
       patientId: number;
       demographics: DemographicsSection;
       conditions: ConditionsSection;
       medications: MedicationsSection;
       allergies: AllergiesSection;
       vitalTrends: VitalTrendsSection;
       encounters: EncountersSection;
       profileCompleteness: number; // 0-100
       lastAggregatedAt: string;
       hasUnresolvedConflicts: boolean;
     }
     
     export interface ConditionItem {
       id: string;
       conditionName: string;
       icd10Code?: string;
       status: string;
       diagnosisDate?: string;
       badge: VerificationBadge;
     }
     
     export interface ConditionsSection {
       activeConditions: ConditionItem[];
       verifiedCount: number;
       totalCount: number;
     }
     // Similar interfaces for Medications, Allergies, Encounters
     ```

2. **Create RTK Query API**
   - File: `patientProfileApi.ts`
   - Endpoints:
     ```typescript
     export const patientProfileApi = createApi({
       reducerPath: 'patientProfileApi',
       baseQuery: fetchBaseQuery({ baseUrl: '/api' }),
       endpoints: (builder) => ({
         getPatientProfile360: builder.query<PatientProfile360, { patientId: number; vitalRangeStart?: string; vitalRangeEnd?: string }>({
           query: ({ patientId, vitalRangeStart, vitalRangeEnd }) => ({
             url: `/patients/${patientId}/profile/360`,
             params: { vitalRangeStart, vitalRangeEnd },
           }),
         }),
       }),
     });
     ```

3. **Create VerificationBadge Component**
   - Props: badge (VerificationBadge), showLabel (boolean, default true)
   - Styling based on badge type:
     - AISuggested: `bg-amber-100 text-amber-800 border-amber-300` (UXR-402)
     - StaffVerified: `bg-green-100 text-green-800 border-green-300` (UXR-402)
   - Icon: Use CheckCircleIcon for StaffVerified, SparklesIcon for AISuggested
   - Layout: Icon + Label (e.g., "AI-suggested" or "Staff-verified")
   - Tooltip: Contextual help per UXR-103 ("AI-suggested data pending staff verification")

4. **Create ProfileCompletionBar Component**
   - Props: completeness (number 0-100), verifiedCount (number), totalCount (number)
   - Display: Progress bar + Text label "X of Y items verified"
   - Styling:
     - Progress bar: `bg-gray-200` background, `bg-green-600` filled portion
     - Width: Full width of section
     - Height: 8px
     - Label: Below bar, small text (text-sm)
   - Accessibility: aria-label with completion percentage

5. **Create DemographicsSection Component**
   - Props: demographics (DemographicsSection)
   - Layout: Card with 2-column grid (375px: 1 column, 768px+: 2 columns)
   - Fields:
     - Name (FirstName + LastName)
     - Date of Birth (formatted: "Jan 15, 1985")
     - Gender
     - Phone Number (formatted: "(555) 123-4567")
     - Email
     - Emergency Contact (name + phone)
   - No verification badges (demographics are user-entered, not AI-extracted)

6. **Create ConditionsSection Component**
   - Props: conditions (ConditionsSection), isStaffView (boolean)
   - Layout: Card with header + list
   - Header:
     - Title: "Active Conditions"
     - ProfileCompletionBar (verifiedCount, totalCount)
   - List items: Each condition displays:
     - Condition name (bold)
     - ICD-10 code (if available, in parentheses)
     - Diagnosis date (formatted: "Diagnosed: Jan 2024")
     - VerificationBadge (right side)
     - Verification button (if isStaffView = true, show "Verify" button)
   - Empty state: "No active conditions recorded"
   - Accessibility: List with proper ARIA labels

7. **Create MedicationsSection Component**
   - Props: medications (MedicationsSection), isStaffView (boolean)
   - Similar structure to ConditionsSection
   - List items display:
     - Drug name (bold)
     - Dosage + Frequency ("10mg, twice daily")
     - Status badge ("Active" in green, "Discontinued" in gray)
     - VerificationBadge
     - Verification button (if isStaffView)
   - Empty state: "No current medications"

8. **Create AllergiesSection Component**
   - Props: allergies (AllergiesSection), isStaffView (boolean)
   - List items display:
     - Allergen name (bold)
     - Severity badge (Critical=red, Severe=orange, Moderate=yellow, Mild=gray)
     - Reaction details
     - VerificationBadge
     - Verification button (if isStaffView)
   - Empty state: "No known allergies"
   - Highlight: Critical/Severe allergies with red background per wireframe

9. **Create VitalTrendsChart Component**
   - Props: vitalTrends (VitalTrendsSection), onDateRangeChange (callback)
   - Chart library: Recharts (responsive, accessible)
   - Chart type: LineChart with multiple series (blood pressure, heart rate, temperature, weight)
   - X-axis: Date (RecordedAt)
   - Y-axis: Value + Unit
   - Features:
     - Scrollable timeline (default: last 12 months)
     - Zoom controls: "1M", "3M", "6M", "1Y", "All"
     - Date range picker (start/end date selectors)
     - Legend: Toggle visibility of each vital type
     - Tooltip: Show value + date on hover
   - Loading: Skeleton placeholder (shimmer effect per UXR-502)
   - Empty state: "No vital signs recorded. Upload clinical documents to populate trends."
   - Accessibility: Chart data available in table format for screen readers

10. **Create EncountersSection Component**
    - Props: encounters (EncountersSection)
    - Layout: Card with table (375px: stacked cards, 768px+: table)
    - Columns: Date, Type, Provider, Facility
    - Rows: Most recent 10 encounters
    - Date formatting: "Jan 15, 2026"
    - Encounter type badge: Color-coded (Inpatient=blue, Outpatient=green, Emergency=red, Telehealth=purple)
    - Empty state: "No recent encounters"

11. **Create PatientHealthDashboard Page**
    - Layout: Container with 6 sections in grid
    - Desktop (1440px): 2-column grid
    - Tablet (768px): 1-column grid
    - Mobile (375px): Stacked sections
    - Order of sections:
      1. Demographics (full width)
      2. Conditions (left column)
      3. Medications (right column)
      4. Allergies (left column)
      5. Vital Trends Chart (full width)
      6. Recent Encounters (full width)
    - Header: Title "My Health Dashboard" (patient) or "Patient Health Profile" (staff)
    - Detect role from JWT token: `const isStaff = user.role === 'Staff' || user.role === 'Admin'`
    - Pass isStaffView to section components

12. **Implement Skeleton Loading States**
    - Create SkeletonCard component (shimmer effect per UXR-502)
    - Show skeletons for each section while fetching data
    - Animate: Smooth fade-in when data loads
    - Ensure skeletons match final layout (same height, structure)

13. **Implement Empty State**
    - Detect empty profile: `profileCompleteness === 0 || !demographics`
    - Display:
      - Large icon (DocumentPlusIcon)
      - Heading: "Build Your Health Profile"
      - Description: "Upload your clinical documents to see your consolidated health summary"
      - CTA button: "Upload Documents" (links to document upload page)
    - Styling per UXR-605: Centered layout, muted colors, prominent CTA

14. **Implement Role-Based UI Rendering**
    - If user role = "Patient": Hide verification buttons, show read-only view
    - If user role = "Staff" or "Admin": Show verification buttons on each data item
    - Verification button onClick: Opens verification modal (implementation in future story)

15. **Optimize Performance for 2-Second Load**
    - Use React.lazy() for code splitting (chart library loaded separately)
    - Implement progressive rendering: Load sections sequentially (demographics first, then conditions, etc.)
    - Use `useMemo` for expensive calculations (e.g., chart data transformation)
    - Implement request deduplication (RTK Query handles this)
    - Prefetch profile data on navigation (router-level prefetch)

16. **Add Accessibility Features**
    - Semantic HTML: `<main>`, `<section>`, `<article>` tags
    - ARIA landmarks: role="region" for each section with aria-labelledby
    - Screen reader support: Descriptive text for verification badges
    - Keyboard navigation: Tab through sections, Enter to expand/collapse
    - Focus management: Focus header on section load
    - Color contrast: WCAG AA compliance (test with axe-core)
    - Skip links: "Skip to main content", "Skip to conditions"

## Current Project State

```
src/frontend/
├── src/
│   ├── components/
│   │   └── (existing components)
│   ├── pages/
│   │   └── (existing pages)
│   ├── store/
│   │   ├── index.ts
│   │   └── slices/
│   ├── api/
│   │   └── (existing API files)
│   └── types/
│       └── (existing types)
└── __tests__/
    └── pages/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/pages/PatientHealthDashboard.tsx | Main dashboard page component |
| CREATE | src/frontend/src/components/patient-profile/DemographicsSection.tsx | Demographics section |
| CREATE | src/frontend/src/components/patient-profile/ConditionsSection.tsx | Conditions section |
| CREATE | src/frontend/src/components/patient-profile/MedicationsSection.tsx | Medications section |
| CREATE | src/frontend/src/components/patient-profile/AllergiesSection.tsx | Allergies section |
| CREATE | src/frontend/src/components/patient-profile/VitalTrendsChart.tsx | Vital trends chart |
| CREATE | src/frontend/src/components/patient-profile/EncountersSection.tsx | Recent encounters section |
| CREATE | src/frontend/src/components/VerificationBadge.tsx | Verification badge component |
| CREATE | src/frontend/src/components/ProfileCompletionBar.tsx | Completion progress bar |
| CREATE | src/frontend/src/store/slices/patientProfileSlice.ts | Redux state management |
| CREATE | src/frontend/src/api/patientProfileApi.ts | RTK Query API endpoints |
| CREATE | src/frontend/src/types/patientProfile.types.ts | TypeScript types |
| CREATE | src/frontend/src/__tests__/pages/PatientHealthDashboard.test.tsx | Unit tests |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### React Documentation
- **Lazy Loading**: https://react.dev/reference/react/lazy
- **useMemo**: https://react.dev/reference/react/useMemo

### Recharts Documentation
- **LineChart**: https://recharts.org/en-US/api/LineChart
- **Responsive Container**: https://recharts.org/en-US/api/ResponsiveContainer

### Tailwind CSS Documentation
- **Grid Layout**: https://tailwindcss.com/docs/grid-template-columns
- **Responsive Design**: https://tailwindcss.com/docs/responsive-design

### Accessibility Guidelines
- **WCAG 2.1**: https://www.w3.org/WAI/WCAG21/quickref/
- **ARIA Landmarks**: https://www.w3.org/WAI/ARIA/apg/patterns/landmarks/

### Design Requirements
- **FR-032**: System MUST generate 360-Degree Patient View displaying unified patient health summary (spec.md)
- **FR-033**: System MUST display 360-Degree Patient View as read-only for patients (spec.md)
- **AIR-007**: System MUST generate 360-Degree Patient View from aggregated clinical data (design.md)
- **NFR-002**: System MUST retrieve and display 360-Degree Patient View within 2 seconds (design.md)
- **UXR-402**: System MUST visually distinguish AI-suggested (amber) vs staff-verified (green) data (figma_spec.md)
- **UXR-502**: System MUST display skeleton loading states when load exceeds 300ms (figma_spec.md)
- **UXR-605**: When dashboard has no data, show empty state with clear CTA (figma_spec.md)
- **SCR-016**: Patient Health Dashboard screen specification (figma_spec.md)

### Existing Codebase Patterns
- **Page Pattern**: `src/frontend/src/pages/AppointmentBooking.tsx`
- **Component Pattern**: `src/frontend/src/components/DocumentUpload.tsx`
- **Redux Slice Pattern**: `src/frontend/src/store/slices/documentsSlice.ts`
- **RTK Query Pattern**: `src/frontend/src/api/documentsApi.ts`

## Build Commands
```powershell
# Install Recharts for charts
cd src/frontend
npm install recharts

# Build frontend
npm run build

# Run dev server
npm run dev

# Run tests
npm test PatientHealthDashboard

# Type check
npm run type-check

# Lint
npm run lint

# Accessibility audit
npm run test:a11y
```

## Implementation Validation Strategy
- [ ] Unit tests pass (dashboard, sections, verification badge)
- [ ] Integration tests pass (API integration, role-based rendering)
- [ ] **[UI Tasks]** Visual comparison against wireframe SCR-016 completed at 375px, 768px, 1440px
- [ ] Dashboard loads within 2 seconds (NFR-002)
- [ ] Skeleton loading displays for <300ms loads (UXR-502)
- [ ] Empty state displays with CTA when no data (UXR-605)
- [ ] Verification badges use correct colors (amber for AI, green for verified per UXR-402)
- [ ] Profile completion bar displays correct percentage
- [ ] Vital trends chart scrolls and zooms correctly (default: 12 months)
- [ ] Role-based rendering works (Patient: read-only, Staff: verification buttons)
- [ ] Responsive layouts match wireframe at all breakpoints
- [ ] Accessibility audit passes (WCAG AA, no critical issues)
- [ ] Keyboard navigation works (Tab through sections)
- [ ] Screen reader support verified (NVDA/JAWS)
- [ ] Performance metrics: FCP <1s, LCP <2s, TTI <3s

## Implementation Checklist
- [ ] Create patientProfile.types.ts with PatientProfile360 and section interfaces
- [ ] Create patientProfileApi.ts with RTK Query endpoint
- [ ] Create VerificationBadge component with amber/green styling per UXR-402
- [ ] Create ProfileCompletionBar component with progress bar
- [ ] Create DemographicsSection component with 2-column grid
- [ ] Create ConditionsSection with verification badges and completion bar
- [ ] Create MedicationsSection with dosage/frequency display
- [ ] Create AllergiesSection with severity badges
- [ ] Create VitalTrendsChart with Recharts LineChart and zoom controls
- [ ] Create EncountersSection with table layout
- [ ] Create PatientHealthDashboard page with 6-section grid layout
- [ ] Implement skeleton loading states for all sections per UXR-502
- [ ] Implement empty state with "Upload documents" CTA per UXR-605
- [ ] Implement role-based UI rendering (hide verification buttons for patients)
- [ ] Optimize for 2-second load with code splitting and progressive rendering
- [ ] Add ARIA landmarks and keyboard navigation for accessibility
- [ ] Write unit tests for dashboard and section components
- [ ] **[UI Tasks - MANDATORY]** Reference wireframe SCR-016 during implementation
- [ ] **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
