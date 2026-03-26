# Task - task_001_fe_clinical_data_review_ui

## Requirement Reference
- User Story: US_053
- Story Location: .propel/context/tasks/EP-009/us_053/us_053.md
- Acceptance Criteria:
    - **AC1**: Given a patient has AI-extracted data, When I access the review interface, Then each data point is displayed with its value, confidence score, source page number, source text excerpt, and extraction date.
    - **AC2**: Given I need to verify an extraction, When I click on a source reference, Then the original document page is displayed alongside the extracted data for side-by-side comparison.
    - **AC3**: Given all data must be verified before clinical use (AIR-S04), When unverified data elements exist, Then a "Verification Required" banner displays with count of pending items and the data cannot be used in clinical workflows.
    - **AC4**: Given the AI vs. verified distinction (AIR-S05), When I view the patient data, Then AI-suggested data shows amber badges and staff-verified data shows green badges clearly.
- Edge Case:
    - What happens when the source document has been deleted after extraction? The extracted data remains with a "Source document unavailable" note; verification relies on the stored text excerpt.
    - How does the system handle verification of 100+ data points for a patient? Data is grouped by type (medications, allergies, vitals) with section-level verification progress bars.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | .propel/context/docs/figma_spec.md#SCR-023 |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-023-clinical-verification.html |
| **Screen Spec** | SCR-023 (Clinical Data Verification), SCR-024 (Conflict Resolution) |
| **UXR Requirements** | UXR-402 (AI vs. verified badges), UXR-501 (200ms action feedback), UXR-201 (Responsive), UXR-301 (Loading states), UXR-605 (Empty states) |
| **Design Tokens** | Tailwind CSS (color-amber, color-green, color-error, font-body, radius-md) |

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Frontend | TypeScript | 5.x |
| Library | Redux Toolkit | 2.x |
| Library | React Router | 6.x |
| Styling | Tailwind CSS | 3.x |
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

Create Clinical Data Review UI (SCR-023) for staff to review AI-extracted clinical data (conditions, medications, allergies, vitals) with side-by-side source document references per AC1-AC2. This task implements the verification interface displaying extracted data with confidence scores, source page numbers, text excerpts, and extraction dates. The UI includes side-by-side document viewer for source verification (AC2), "Verification Required" banner for unverified data blocking clinical workflows (AC3, AIR-S04), amber/green badge system (AC4, AIR-S05, UXR-402), section grouping with progress bars for 100+ data points (edge case), and handling of deleted source documents (edge case). Features responsive design (375px/768px/1440px per UXR-201), loading/empty states (UXR-301, UXR-605), and integration with existing verification endpoints from US_051/US_052.

**Key Capabilities:**
- ClinicalDataReviewPage component (main page, SCR-023 layout)
- ExtractedDataTable with fields: value, confidence, source page, excerpt, extraction date (AC1)
- SourceDocumentViewer side panel with document page display (AC2)
- VerificationRequiredBanner blocking clinical workflows for unverified data (AC3, AIR-S04)
- VerificationBadge (amber = AI-suggested, green = staff-verified per AC4, AIR-S05)
- SectionGrouping: medications, allergies, conditions, vitals (edge case: 100+ data points)
- VerificationProgressBar per section showing verified/total count
- SourceUnavailableNote for deleted documents (edge case)
- Redux slice: clinicalDataReviewSlice with fetchExtractedData action
- Integration with PATCH /api/extracted-data/{id}/verify endpoint
- Toast notifications for verification actions (200ms per UXR-501)
- Responsive design (375px/768px/1440px per UXR-201)

## Dependent Tasks
- EP-006-II: US_045: task_002_be_azure_document_intelligence (ExtractedClinicalData entity, extraction date field)
- EP-009: US_053: task_002_be_document_viewer_service (Document retrieval API for side-by-side viewing)

## Impacted Components
- **NEW**: `src/frontend/src/pages/ClinicalDataReviewPage.tsx` - Main review page
- **NEW**: `src/frontend/src/components/clinicalData/ExtractedDataTable.tsx` - Data table with source references
- **NEW**: `src/frontend/src/components/clinicalData/SourceDocumentViewer.tsx` - Side-by-side document viewer
- **NEW**: `src/frontend/src/components/clinicalData/VerificationRequiredBanner.tsx` - Warning banner (AIR-S04)
- **NEW**: `src/frontend/src/components/clinicalData/SectionGrouping.tsx` - Group by data type
- **NEW**: `src/frontend/src/components/clinicalData/VerificationProgressBar.tsx` - Section progress indicator
- **NEW**: `src/frontend/src/components/clinicalData/SourceUnavailableNote.tsx` - Deleted document note
- **NEW**: `src/frontend/src/store/slices/clinicalDataReviewSlice.ts` - Redux slice
- **NEW**: `src/frontend/src/api/clinicalDataApi.ts` - API client for extracted data
- **NEW**: `src/frontend/src/types/clinicalData.types.ts` - TypeScript types
- **MODIFY**: `src/frontend/src/store/store.ts` - Register clinicalDataReviewSlice
- **MODIFY**: `src/frontend/src/App.tsx` - Add /staff/clinical-review route

## Implementation Plan

1. **Create TypeScript Types**
   - File: `src/frontend/src/types/clinicalData.types.ts`
   - Types:
     ```typescript
     export interface ExtractedDataPoint {
       id: string;
       patientId: number;
       documentId: string;
       documentName: string;
       dataType: 'Condition' | 'Medication' | 'Allergy' | 'Vital' | 'LabResult';
       fieldName: string; // "Blood Pressure", "Medication Name"
       extractedValue: string;
       confidenceScore: number; // 0-100
       sourcePageNumber: number;
       sourceTextExcerpt: string; // Text snippet from document
       extractionDate: string; // ISO timestamp
       verificationStatus: 'Pending' | 'StaffVerified' | 'StaffRejected';
       verifiedBy?: number; // User ID
       verifiedAt?: string; // ISO timestamp
       isDocumentAvailable: boolean; // false if source document deleted
     }
     
     export interface DataSection {
       sectionType: 'Condition' | 'Medication' | 'Allergy' | 'Vital' | 'LabResult';
       dataPoints: ExtractedDataPoint[];
       totalCount: number;
       verifiedCount: number;
       pendingCount: number;
     }
     
     export interface VerificationStats {
       totalDataPoints: number;
       verifiedCount: number;
       pendingCount: number;
       percentageComplete: number;
     }
     ```

2. **Create API Client**
   - File: `src/frontend/src/api/clinicalDataApi.ts`
   - API methods:
     ```typescript
     import axios from 'axios';
     
     const API_BASE = '/api/extracted-data';
     
     export const clinicalDataApi = {
       getExtractedDataForPatient: async (patientId: number): Promise<ExtractedDataPoint[]> => {
         const response = await axios.get(`${API_BASE}/patient/${patientId}`);
         return response.data;
       },
       
       getSourceDocument: async (documentId: string, pageNumber: number): Promise<{ documentUrl: string; pageImageUrl: string }> => {
         const response = await axios.get(`/api/documents/${documentId}/page/${pageNumber}`);
         return response.data;
       },
       
       verifyDataPoint: async (dataPointId: string): Promise<void> => {
         await axios.patch(`${API_BASE}/${dataPointId}/verify`, {
           verificationStatus: 'StaffVerified'
         });
       },
       
       rejectDataPoint: async (dataPointId: string, reason: string): Promise<void> => {
         await axios.patch(`${API_BASE}/${dataPointId}/verify`, {
           verificationStatus: 'StaffRejected',
           notes: reason
         });
       }
     };
     ```

3. **Create Redux Slice**
   - File: `src/frontend/src/store/slices/clinicalDataReviewSlice.ts`
   - State management:
     ```typescript
     import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
     import { clinicalDataApi } from '../../api/clinicalDataApi';
     
     interface ReviewState {
       dataPoints: ExtractedDataPoint[];
       sections: DataSection[];
       verificationStats: VerificationStats;
       selectedDataPointId: string | null;
       isLoading: boolean;
       error: string | null;
     }
     
     const initialState: ReviewState = {
       dataPoints: [],
       sections: [],
       verificationStats: { totalDataPoints: 0, verifiedCount: 0, pendingCount: 0, percentageComplete: 0 },
       selectedDataPointId: null,
       isLoading: false,
       error: null
     };
     
     export const fetchExtractedData = createAsyncThunk(
       'clinicalDataReview/fetchExtractedData',
       async (patientId: number) => {
         return await clinicalDataApi.getExtractedDataForPatient(patientId);
       }
     );
     
     export const verifyDataPoint = createAsyncThunk(
       'clinicalDataReview/verifyDataPoint',
       async (dataPointId: string) => {
         await clinicalDataApi.verifyDataPoint(dataPointId);
         return dataPointId;
       }
     );
     
     const clinicalDataReviewSlice = createSlice({
       name: 'clinicalDataReview',
       initialState,
       reducers: {
         selectDataPoint: (state, action: PayloadAction<string>) => {
           state.selectedDataPointId = action.payload;
         },
         clearSelection: (state) => {
           state.selectedDataPointId = null;
         }
       },
       extraReducers: (builder) => {
         builder
           .addCase(fetchExtractedData.pending, (state) => {
             state.isLoading = true;
           })
           .addCase(fetchExtractedData.fulfilled, (state, action) => {
             state.isLoading = false;
             state.dataPoints = action.payload;
             
             // Group data by type (edge case: 100+ data points)
             state.sections = groupDataByType(action.payload);
             
             // Calculate verification stats
             state.verificationStats = calculateStats(action.payload);
           })
           .addCase(verifyDataPoint.fulfilled, (state, action) => {
             const dataPoint = state.dataPoints.find(dp => dp.id === action.payload);
             if (dataPoint) {
               dataPoint.verificationStatus = 'StaffVerified';
               state.verificationStats = calculateStats(state.dataPoints);
               state.sections = groupDataByType(state.dataPoints);
             }
           });
       }
     });
     
     function groupDataByType(dataPoints: ExtractedDataPoint[]): DataSection[] {
       const types: ('Condition' | 'Medication' | 'Allergy' | 'Vital' | 'LabResult')[] = 
         ['Condition', 'Medication', 'Allergy', 'Vital', 'LabResult'];
       
       return types.map(type => {
         const sectionData = dataPoints.filter(dp => dp.dataType === type);
         return {
           sectionType: type,
           dataPoints: sectionData,
           totalCount: sectionData.length,
           verifiedCount: sectionData.filter(dp => dp.verificationStatus === 'StaffVerified').length,
           pendingCount: sectionData.filter(dp => dp.verificationStatus === 'Pending').length
         };
       }).filter(section => section.totalCount > 0);
     }
     
     function calculateStats(dataPoints: ExtractedDataPoint[]): VerificationStats {
       const totalDataPoints = dataPoints.length;
       const verifiedCount = dataPoints.filter(dp => dp.verificationStatus === 'StaffVerified').length;
       const pendingCount = dataPoints.filter(dp => dp.verificationStatus === 'Pending').length;
       const percentageComplete = totalDataPoints > 0 ? (verifiedCount / totalDataPoints) * 100 : 0;
       
       return { totalDataPoints, verifiedCount, pendingCount, percentageComplete };
     }
     
     export const { selectDataPoint, clearSelection } = clinicalDataReviewSlice.actions;
     export default clinicalDataReviewSlice.reducer;
     ```

4. **Create VerificationRequiredBanner Component**
   - File: `src/frontend/src/components/clinicalData/VerificationRequiredBanner.tsx`
   - AIR-S04 compliance: mandatory verification before clinical use (AC3)
   - Implementation:
     ```tsx
     import React from 'react';
     
     interface VerificationRequiredBannerProps {
       pendingCount: number;
       percentageComplete: number;
     }
     
     export const VerificationRequiredBanner: React.FC<VerificationRequiredBannerProps> = ({
       pendingCount,
       percentageComplete
     }) => {
       if (pendingCount === 0) return null;
       
       return (
         <div className="bg-amber-50 border-l-4 border-amber-500 p-4 mb-6" role="alert">
           <div className="flex items-start">
             <div className="flex-shrink-0">
               <svg className="h-5 w-5 text-amber-500" viewBox="0 0 20 20" fill="currentColor">
                 <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
               </svg>
             </div>
             <div className="ml-3 flex-1">
               <h3 className="text-sm font-semibold text-amber-800">
                 Verification Required Before Clinical Use
               </h3>
               <div className="mt-2 text-sm text-amber-700">
                 <p>
                   <strong>{pendingCount}</strong> data point{pendingCount !== 1 ? 's' : ''} require staff verification before this data can be used in clinical workflows.
                 </p>
                 <div className="mt-3">
                   <div className="w-full bg-amber-200 rounded-full h-2">
                     <div 
                       className="bg-amber-600 h-2 rounded-full transition-all duration-300"
                       style={{ width: `${percentageComplete}%` }}
                     />
                   </div>
                   <p className="mt-1 text-xs text-amber-600">{percentageComplete.toFixed(0)}% verified</p>
                 </div>
               </div>
               <div className="mt-4">
                 <p className="text-xs text-amber-600 italic">
                   ⚠️ Per AIR-S04 requirement: All AI-extracted clinical data must be verified by staff before clinical use.
                 </p>
               </div>
             </div>
           </div>
         </div>
       );
     };
     ```

5. **Create SectionGrouping Component**
   - File: `src/frontend/src/components/clinicalData/SectionGrouping.tsx`
   - Edge case handling: group 100+ data points by type
   - Implementation:
     ```tsx
     import React, { useState } from 'react';
     import { VerificationProgressBar } from './VerificationProgressBar';
     import { ExtractedDataTable } from './ExtractedDataTable';
     
     interface SectionGroupingProps {
       sections: DataSection[];
       onSelectDataPoint: (dataPointId: string) => void;
     }
     
     export const SectionGrouping: React.FC<SectionGroupingProps> = ({ sections, onSelectDataPoint }) => {
       const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set(sections.map(s => s.sectionType)));
       
       const toggleSection = (sectionType: string) => {
         setExpandedSections(prev => {
           const next = new Set(prev);
           if (next.has(sectionType)) {
             next.delete(sectionType);
           } else {
             next.add(sectionType);
           }
           return next;
         });
       };
       
       const sectionIcons = {
         Condition: '🩺',
         Medication: '💊',
         Allergy: '⚠️',
         Vital: '❤️',
         LabResult: '🔬'
       };
       
       return (
         <div className="space-y-4">
           {sections.map(section => (
             <div key={section.sectionType} className="border border-gray-200 rounded-lg overflow-hidden">
               <button
                 onClick={() => toggleSection(section.sectionType)}
                 className="w-full px-4 py-3 bg-gray-50 hover:bg-gray-100 flex items-center justify-between transition-colors"
               >
                 <div className="flex items-center gap-3">
                   <span className="text-2xl">{sectionIcons[section.sectionType]}</span>
                   <div className="text-left">
                     <h3 className="font-semibold text-gray-900">{section.sectionType}s</h3>
                     <p className="text-sm text-gray-600">
                       {section.verifiedCount} of {section.totalCount} verified
                     </p>
                   </div>
                 </div>
                 <div className="flex items-center gap-4">
                   <VerificationProgressBar
                     verifiedCount={section.verifiedCount}
                     totalCount={section.totalCount}
                     width="w-32"
                   />
                   <svg
                     className={`w-5 h-5 text-gray-500 transition-transform ${expandedSections.has(section.sectionType) ? 'rotate-180' : ''}`}
                     fill="none"
                     stroke="currentColor"
                     viewBox="0 0 24 24"
                   >
                     <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                   </svg>
                 </div>
               </button>
               
               {expandedSections.has(section.sectionType) && (
                 <div className="p-4">
                   <ExtractedDataTable
                     dataPoints={section.dataPoints}
                     onSelectDataPoint={onSelectDataPoint}
                   />
                 </div>
               )}
             </div>
           ))}
         </div>
       );
     };
     ```

6. **Create SourceDocumentViewer Component**
   - File: `src/frontend/src/components/clinicalData/SourceDocumentViewer.tsx`
   - Side-by-side document viewer for AC2
   - Implementation:
     ```tsx
     import React, { useState, useEffect } from 'react';
     import { clinicalDataApi } from '../../api/clinicalDataApi';
     
     interface SourceDocumentViewerProps {
       dataPoint: ExtractedDataPoint | null;
     }
     
     export const SourceDocumentViewer: React.FC<SourceDocumentViewerProps> = ({ dataPoint }) => {
       const [documentPageUrl, setDocumentPageUrl] = useState<string | null>(null);
       const [isLoading, setIsLoading] = useState(false);
       
       useEffect(() => {
         if (dataPoint && dataPoint.isDocumentAvailable) {
           setIsLoading(true);
           clinicalDataApi.getSourceDocument(dataPoint.documentId, dataPoint.sourcePageNumber)
             .then(result => {
               setDocumentPageUrl(result.pageImageUrl);
               setIsLoading(false);
             })
             .catch(error => {
               console.error('Failed to load document:', error);
               setIsLoading(false);
             });
         }
       }, [dataPoint]);
       
       if (!dataPoint) {
         return (
           <div className="h-full flex items-center justify-center text-gray-500">
             <p>Select a data point to view source document</p>
           </div>
         );
       }
       
       return (
         <div className="h-full flex flex-col">
           <div className="p-4 border-b border-gray-200">
             <h3 className="font-semibold text-gray-900">Source Document</h3>
             <p className="text-sm text-gray-600 mt-1">{dataPoint.documentName}</p>
             <p className="text-xs text-gray-500">Page {dataPoint.sourcePageNumber}</p>
           </div>
           
           {!dataPoint.isDocumentAvailable ? (
             <div className="flex-1 p-4">
               <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
                 <p className="text-sm text-amber-800 font-medium">Source Document Unavailable</p>
                 <p className="text-xs text-amber-700 mt-2">
                   The source document has been deleted. Verification relies on the stored text excerpt below.
                 </p>
               </div>
               <div className="mt-4 p-4 bg-gray-50 border border-gray-200 rounded-lg">
                 <p className="text-xs text-gray-500 mb-2">Extracted Text:</p>
                 <p className="text-sm text-gray-900">{dataPoint.sourceTextExcerpt}</p>
               </div>
             </div>
           ) : isLoading ? (
             <div className="flex-1 flex items-center justify-center">
               <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
             </div>
           ) : documentPageUrl ? (
             <div className="flex-1 overflow-auto p-4">
               <img 
                 src={documentPageUrl} 
                 alt={`Document page ${dataPoint.sourcePageNumber}`}
                 className="w-full border border-gray-300 rounded"
               />
               <div className="mt-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                 <p className="text-xs text-blue-600 font-medium mb-2">Extracted Text Excerpt:</p>
                 <p className="text-sm text-blue-900">{dataPoint.sourceTextExcerpt}</p>
               </div>
             </div>
           ) : null}
         </div>
       );
     };
     ```

7. **Create ExtractedDataTable Component**
   - File: `src/frontend/src/components/clinicalData/ExtractedDataTable.tsx`
   - Display all fields per AC1: value, confidence, source page, excerpt, extraction date
   - Implementation includes verification action buttons, badge display (AC4)

8. **Create VerificationProgressBar Component**
   - File: `src/frontend/src/components/clinicalData/VerificationProgressBar.tsx`
   - Visual indicator of section verification progress

9. **Create ClinicalDataReviewPage**
   - File: `src/frontend/src/pages/ClinicalDataReviewPage.tsx`
   - Main page integrating all components
   - Two-panel layout: data table (left) + document viewer (right)
   - Responsive design: stacks vertically on mobile (<768px)

10. **Add Route in App.tsx**
    - Path: `/staff/clinical-review/:patientId`
    - Protected route (Staff, Admin roles only)

## Current Project State

```
src/frontend/src/
├── pages/
│   └── MedicalCodeVerificationPage.tsx (from US_052)
├── components/
│   └── medicalCodes/
│       └── VerificationBadge.tsx (from US_052)
├── store/
│   ├── store.ts
│   └── slices/
└── api/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/pages/ClinicalDataReviewPage.tsx | Main review page |
| CREATE | src/frontend/src/components/clinicalData/ExtractedDataTable.tsx | Data table component |
| CREATE | src/frontend/src/components/clinicalData/SourceDocumentViewer.tsx | Document viewer |
| CREATE | src/frontend/src/components/clinicalData/VerificationRequiredBanner.tsx | Warning banner (AIR-S04) |
| CREATE | src/frontend/src/components/clinicalData/SectionGrouping.tsx | Section grouping |
| CREATE | src/frontend/src/components/clinicalData/VerificationProgressBar.tsx | Progress bar |
| CREATE | src/frontend/src/components/clinicalData/SourceUnavailableNote.tsx | Deleted document note |
| CREATE | src/frontend/src/store/slices/clinicalDataReviewSlice.ts | Redux slice |
| CREATE | src/frontend/src/api/clinicalDataApi.ts | API client |
| CREATE | src/frontend/src/types/clinicalData.types.ts | TypeScript types |
| MODIFY | src/frontend/src/store/store.ts | Register slice |
| MODIFY | src/frontend/src/App.tsx | Add route |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### React + TypeScript Best Practices
- **React TypeScript Cheatsheet**: https://react-typescript-cheatsheet.netlify.app/
- **Redux Toolkit TypeScript**: https://redux-toolkit.js.org/usage/usage-with-typescript

### Tailwind CSS Utilities
- **Layout**: https://tailwindcss.com/docs/flex
- **Colors**: https://tailwindcss.com/docs/customizing-colors

### Design Requirements
- **AIR-S04**: Mandatory staff verification before clinical use (design.md)
- **AIR-S05**: Clear AI-suggested vs. staff-verified distinction (design.md)
- **UXR-402**: Amber badges for AI-suggested, green for staff-verified (figma_spec.md)
- **UXR-501**: 200ms action feedback (figma_spec.md)
- **FR-037**: Staff review with source references (spec.md)

### Wireframe Reference
- **SCR-023**: wireframe-SCR-023-clinical-verification.html

## Build Commands
```powershell
# Run development server
cd src/frontend
npm run dev

# Build for production
npm run build

# Run tests
npm test
```

## Validation Strategy

### Unit Tests
- File: `src/frontend/src/__tests__/components/VerificationRequiredBanner.test.tsx`
- Test cases:
  1. **Test_VerificationRequiredBanner_DisplaysPendingCount**
     - Render: <VerificationRequiredBanner pendingCount={10} percentageComplete={50} />
     - Assert: "10 data points require staff verification" displayed
  2. **Test_VerificationRequiredBanner_HiddenWhenNoPending**
     - Render: <VerificationRequiredBanner pendingCount={0} percentageComplete={100} />
     - Assert: Component not rendered (returns null)
  3. **Test_SectionGrouping_GroupsByType**
     - Input: 50 conditions, 30 medications, 20 allergies
     - Assert: 3 sections rendered with correct counts
  4. **Test_SourceDocumentViewer_ShowsUnavailableNote**
     - Input: ExtractedDataPoint with isDocumentAvailable = false
     - Assert: "Source Document Unavailable" note displayed
  5. **Test_SourceDocumentViewer_LoadsDocumentPage**
     - Input: ExtractedDataPoint with valid documentId
     - Assert: API call made, document image displayed

### Integration Tests
- File: `src/frontend/src/__tests__/pages/ClinicalDataReviewPage.test.tsx`
- Test cases:
  1. **Test_ReviewPage_LoadsExtractedData**
     - Render: <ClinicalDataReviewPage patientId={123} />
     - Wait for: API call completes
     - Assert: Data points displayed in sections
  2. **Test_ReviewPage_ClickSourceReference_ShowsDocument**
     - User: Clicks on data point row
     - Assert: SourceDocumentViewer displays document page
  3. **Test_VerificationBanner_HidesAfterAllVerified**
     - Setup: 5 pending data points
     - User: Verifies all 5 data points
     - Assert: Banner no longer displayed

### Acceptance Criteria Validation
- **AC1**: ✅ Data displayed with value, confidence, source page, excerpt, extraction date
- **AC2**: ✅ Click source reference shows document in side panel
- **AC3**: ✅ "Verification Required" banner blocks clinical workflows for unverified data
- **AC4**: ✅ Amber badges for AI-suggested, green for staff-verified (AIR-S05)
- **Edge Case 1**: ✅ Deleted documents show "Source document unavailable" note
- **Edge Case 2**: ✅ 100+ data points grouped by type with progress bars

## Success Criteria Checklist
- [MANDATORY] WIREFRAME VALIDATION: Rendered UI matches wireframe-SCR-023-clinical-verification.html layout
- [MANDATORY] ClinicalDataReviewPage displays extracted data with all AC1 fields
- [MANDATORY] VerificationRequiredBanner displays for unverified data (AIR-S04)
- [MANDATORY] VerificationBadge shows amber for "Pending", green for "StaffVerified" (AIR-S05, UXR-402)
- [MANDATORY] SourceDocumentViewer displays document page on click (AC2)
- [MANDATORY] SectionGrouping groups data by type: Condition, Medication, Allergy, Vital, LabResult
- [MANDATORY] VerificationProgressBar shows verified/total count per section
- [MANDATORY] SourceUnavailableNote displayed when isDocumentAvailable = false
- [MANDATORY] Redux slice fetches extracted data via API
- [MANDATORY] Verification actions call PATCH /api/extracted-data/{id}/verify
- [MANDATORY] Toast notifications for verification (200ms per UXR-501)
- [MANDATORY] Responsive design: 375px (mobile), 768px (tablet), 1440px (desktop) per UXR-201
- [MANDATORY] Loading states with skeleton loaders (UXR-301)
- [MANDATORY] Empty state: "No extracted data found" (UXR-605)
- [MANDATORY] Unit test: Banner displays pending count correctly
- [MANDATORY] Unit test: Section grouping creates correct number of sections
- [MANDATORY] Integration test: Document viewer loads on data point selection
- [RECOMMENDED] Keyboard navigation support (Tab, Enter, Escape)
- [RECOMMENDED] Print-friendly view for verification reports

## Estimated Effort
**6 hours** (Page + sections + document viewer + banner + Redux + responsive design + unit tests)
