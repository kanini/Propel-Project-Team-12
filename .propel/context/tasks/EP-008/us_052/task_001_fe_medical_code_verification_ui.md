# Task - task_001_fe_medical_code_verification_ui

## Requirement Reference
- User Story: US_052
- Story Location: .propel/context/tasks/EP-008/us_052/us_052.md
- Acceptance Criteria:
    - **AC1**: Given AI has suggested medical codes, When I access the verification interface, Then each suggested code displays with code value, description, confidence score, source clinical data reference, and accept/modify/reject buttons.
    - **AC2**: Given I accept a code, When I click "Accept", Then the code verification status changes to "Accepted", my user ID and timestamp are recorded, and the code badge changes from amber to green.
    - **AC3**: Given I want to modify a code, When I click "Modify", Then a search field allows me to look up alternative ICD-10/CPT codes, select the correct one, and save with modification rationale.
    - **AC4**: Given I reject a code, When I click "Reject", Then the code is marked as "Rejected" with required rejection reason, and the data point is flagged for manual coding.
- Edge Case:
    - What happens when a Staff member verifies a code that another Staff member already rejected? The most recent verification action takes precedence with full audit trail of both actions.
    - How does the system handle bulk verification? A "Select All / Accept All" option exists for low-risk codes (confidence >95%) with individual override capability.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | .propel/context/docs/figma_spec.md#SCR-023 |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-023-clinical-verification.html |
| **Screen Spec** | SCR-023 (Clinical Data Verification) |
| **UXR Requirements** | UXR-103 (Clear action CTAs), UXR-201 (Responsive), UXR-301 (4-8s loading), UXR-402 (Amber/green badges), UXR-501 (200ms action feedback) |
| **Design Tokens** | Tailwind CSS (color-success, color-warning, color-error, font-body, radius-md) |

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

Create Clinical Data Verification UI (SCR-023) for staff to review, accept, modify, or reject AI-suggested medical codes (ICD-10, CPT). This task implements the verification interface displaying suggested codes with confidence scores, source clinical data references, and verification action buttons per AC1. The UI includes accept/modify/reject workflows with inline editing (AC2-AC4), visual badge system (amber for AI-suggested, green for staff-verified per UXR-402), bulk verification for high-confidence codes (>95%), and real-time status updates with 200ms action feedback (UXR-501). Features responsive design (375px/768px/1440px per UXR-201), loading states (UXR-301), empty states (UXR-605), and integration with PATCH /api/medical-codes/{codeId}/verify endpoint from US_051.

**Key Capabilities:**
- MedicalCodeVerificationPage component (main page)
- CodeVerificationTable with code data, confidence bars, verification badges
- AcceptButton, ModifyButton, RejectButton action components
- ModifyCodeModal with code search and rationale input
- RejectCodeModal with rejection reason dropdown
- VerificationBadge component (amber = AI-suggested, green = staff-verified)
- ConfidenceBar component with color-coded confidence (green >85%, amber 70-85%, red <70%)
- BulkVerificationControls for "Select All / Accept All" (confidence >95%)
- Redux slice: medicalCodeVerificationSlice with actions (acceptCode, modifyCode, rejectCode)
- Toast notifications for action feedback (200ms per UXR-501)
- Audit trail display (shows previous verification actions)
- Source document viewer in side panel

## Dependent Tasks
- EP-008: US_051: task_002_be_code_mapping_api (PATCH /api/medical-codes/{codeId}/verify endpoint)
- EP-008: US_052: task_002_be_code_search_service (code lookup for Modify action)

## Impacted Components
- **NEW**: `src/frontend/src/pages/MedicalCodeVerificationPage.tsx` - Main verification page
- **NEW**: `src/frontend/src/components/medicalCodes/CodeVerificationTable.tsx` - Verification table component
- **NEW**: `src/frontend/src/components/medicalCodes/VerificationBadge.tsx` - Badge component (amber/green)
- **NEW**: `src/frontend/src/components/medicalCodes/ConfidenceBar.tsx` - Confidence score bar
- **NEW**: `src/frontend/src/components/medicalCodes/ModifyCodeModal.tsx` - Modal for modifying codes
- **NEW**: `src/frontend/src/components/medicalCodes/RejectCodeModal.tsx` - Modal for rejecting codes
- **NEW**: `src/frontend/src/components/medicalCodes/BulkVerificationControls.tsx` - Bulk accept controls
- **NEW**: `src/frontend/src/store/slices/medicalCodeVerificationSlice.ts` - Redux slice
- **NEW**: `src/frontend/src/api/medicalCodesApi.ts` - API client for medical codes
- **NEW**: `src/frontend/src/types/medicalCode.types.ts` - TypeScript types
- **MODIFY**: `src/frontend/src/store/store.ts` - Register medicalCodeVerificationSlice
- **MODIFY**: `src/frontend/src/App.tsx` - Add /staff/verification route

## Implementation Plan

1. **Create TypeScript Types**
   - File: `src/frontend/src/types/medicalCode.types.ts`
   - Types:
     ```typescript
     export interface MedicalCodeSuggestion {
       id: string;
       code: string; // "E11.9", "99213"
       description: string;
       codeSystem: 'ICD10' | 'CPT';
       confidenceScore: number; // 0-100
       rationale: string;
       rank: number;
       isTopSuggestion: boolean;
       verificationStatus: 'Pending' | 'StaffVerified' | 'StaffRejected';
       verifiedBy?: number; // User ID
       verifiedAt?: string; // ISO timestamp
       extractedClinicalDataId: string;
       sourceClinicalText: string; // For side panel display
     }
     
     export interface VerificationAuditEntry {
       userId: number;
       userName: string;
       action: 'Accepted' | 'Modified' | 'Rejected';
       timestamp: string;
       previousCode?: string; // For modified codes
       reason?: string; // For rejected codes
     }
     
     export interface ModifyCodeRequest {
       codeId: string;
       newCode: string;
       newDescription: string;
       rationale: string;
     }
     
     export interface RejectCodeRequest {
       codeId: string;
       reason: string;
     }
     ```

2. **Create API Client**
   - File: `src/frontend/src/api/medicalCodesApi.ts`
   - API methods:
     ```typescript
     import axios from 'axios';
     
     const API_BASE = '/api/medical-codes';
     
     export const medicalCodesApi = {
       getSuggestionsForPatient: async (extractedDataId: string): Promise<MedicalCodeSuggestion[]> => {
         const response = await axios.get(`${API_BASE}/${extractedDataId}/suggestions`);
         return response.data;
       },
       
       acceptCode: async (codeId: string): Promise<void> => {
         await axios.patch(`${API_BASE}/${codeId}/verify`, {
           verificationStatus: 'StaffVerified'
         });
       },
       
       modifyCode: async (request: ModifyCodeRequest): Promise<void> => {
         await axios.patch(`${API_BASE}/${request.codeId}/modify`, {
           newCode: request.newCode,
           newDescription: request.newDescription,
           rationale: request.rationale
         });
       },
       
       rejectCode: async (request: RejectCodeRequest): Promise<void> => {
         await axios.patch(`${API_BASE}/${request.codeId}/verify`, {
           verificationStatus: 'StaffRejected',
           notes: request.reason
         });
       },
       
       bulkAcceptCodes: async (codeIds: string[]): Promise<void> => {
         await Promise.all(codeIds.map(id => medicalCodesApi.acceptCode(id)));
       }
     };
     ```

3. **Create Redux Slice**
   - File: `src/frontend/src/store/slices/medicalCodeVerificationSlice.ts`
   - State management:
     ```typescript
     import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
     import { medicalCodesApi } from '../../api/medicalCodesApi';
     
     interface VerificationState {
       suggestions: MedicalCodeSuggestion[];
       selectedCodeId: string | null;
       isLoading: boolean;
       error: string | null;
       selectedCodes: string[]; // For bulk operations
     }
     
     const initialState: VerificationState = {
       suggestions: [],
       selectedCodeId: null,
       isLoading: false,
       error: null,
       selectedCodes: []
     };
     
     export const fetchSuggestions = createAsyncThunk(
       'medicalCodeVerification/fetchSuggestions',
       async (extractedDataId: string) => {
         return await medicalCodesApi.getSuggestionsForPatient(extractedDataId);
       }
     );
     
     export const acceptCode = createAsyncThunk(
       'medicalCodeVerification/acceptCode',
       async (codeId: string) => {
         await medicalCodesApi.acceptCode(codeId);
         return codeId;
       }
     );
     
     export const modifyCode = createAsyncThunk(
       'medicalCodeVerification/modifyCode',
       async (request: ModifyCodeRequest) => {
         await medicalCodesApi.modifyCode(request);
         return request;
       }
     );
     
     export const rejectCode = createAsyncThunk(
       'medicalCodeVerification/rejectCode',
       async (request: RejectCodeRequest) => {
         await medicalCodesApi.rejectCode(request);
         return request;
       }
     );
     
     export const bulkAcceptCodes = createAsyncThunk(
       'medicalCodeVerification/bulkAcceptCodes',
       async (codeIds: string[]) => {
         await medicalCodesApi.bulkAcceptCodes(codeIds);
         return codeIds;
       }
     );
     
     const medicalCodeVerificationSlice = createSlice({
       name: 'medicalCodeVerification',
       initialState,
       reducers: {
         selectCode: (state, action: PayloadAction<string>) => {
           state.selectedCodeId = action.payload;
         },
         toggleCodeSelection: (state, action: PayloadAction<string>) => {
           if (state.selectedCodes.includes(action.payload)) {
             state.selectedCodes = state.selectedCodes.filter(id => id !== action.payload);
           } else {
             state.selectedCodes.push(action.payload);
           }
         },
         selectAllHighConfidence: (state) => {
           state.selectedCodes = state.suggestions
             .filter(s => s.confidenceScore > 95 && s.verificationStatus === 'Pending')
             .map(s => s.id);
         },
         clearSelection: (state) => {
           state.selectedCodes = [];
         }
       },
       extraReducers: (builder) => {
         builder
           .addCase(fetchSuggestions.pending, (state) => {
             state.isLoading = true;
           })
           .addCase(fetchSuggestions.fulfilled, (state, action) => {
             state.isLoading = false;
             state.suggestions = action.payload;
           })
           .addCase(acceptCode.fulfilled, (state, action) => {
             const code = state.suggestions.find(s => s.id === action.payload);
             if (code) {
               code.verificationStatus = 'StaffVerified';
             }
           })
           .addCase(rejectCode.fulfilled, (state, action) => {
             const code = state.suggestions.find(s => s.id === action.payload.codeId);
             if (code) {
               code.verificationStatus = 'StaffRejected';
             }
           })
           .addCase(bulkAcceptCodes.fulfilled, (state, action) => {
             action.payload.forEach(codeId => {
               const code = state.suggestions.find(s => s.id === codeId);
               if (code) {
                 code.verificationStatus = 'StaffVerified';
               }
             });
             state.selectedCodes = [];
           });
       }
     });
     
     export const { selectCode, toggleCodeSelection, selectAllHighConfidence, clearSelection } = medicalCodeVerificationSlice.actions;
     export default medicalCodeVerificationSlice.reducer;
     ```

4. **Create VerificationBadge Component**
   - File: `src/frontend/src/components/medicalCodes/VerificationBadge.tsx`
   - UXR-402 compliance: amber for AI-suggested, green for staff-verified
   - Implementation:
     ```tsx
     import React from 'react';
     
     interface VerificationBadgeProps {
       status: 'Pending' | 'StaffVerified' | 'StaffRejected';
       size?: 'sm' | 'md';
     }
     
     export const VerificationBadge: React.FC<VerificationBadgeProps> = ({ status, size = 'md' }) => {
       const baseClasses = 'inline-flex items-center rounded-full font-medium';
       const sizeClasses = size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-3 py-1 text-sm';
       
       const statusConfig = {
         Pending: {
           bgColor: 'bg-amber-100',
           textColor: 'text-amber-800',
           label: 'AI-Suggested'
         },
         StaffVerified: {
           bgColor: 'bg-green-100',
           textColor: 'text-green-800',
           label: 'Staff-Verified'
         },
         StaffRejected: {
           bgColor: 'bg-red-100',
           textColor: 'text-red-800',
           label: 'Rejected'
         }
       };
       
       const config = statusConfig[status];
       
       return (
         <span className={`${baseClasses} ${sizeClasses} ${config.bgColor} ${config.textColor}`}>
           {config.label}
         </span>
       );
     };
     ```

5. **Create ConfidenceBar Component**
   - File: `src/frontend/src/components/medicalCodes/ConfidenceBar.tsx`
   - Color-coded confidence: green >85%, amber 70-85%, red <70%
   - Implementation:
     ```tsx
     import React from 'react';
     
     interface ConfidenceBarProps {
       score: number; // 0-100
       showLabel?: boolean;
     }
     
     export const ConfidenceBar: React.FC<ConfidenceBarProps> = ({ score, showLabel = true }) => {
       const getColorClass = (score: number): string => {
         if (score > 85) return 'bg-green-500';
         if (score >= 70) return 'bg-amber-500';
         return 'bg-red-500';
       };
       
       return (
         <div className="flex items-center gap-2">
           <div className="w-16 h-2 bg-gray-200 rounded-full overflow-hidden">
             <div
               className={`h-full transition-all duration-300 ${getColorClass(score)}`}
               style={{ width: `${score}%` }}
             />
           </div>
           {showLabel && <span className="text-sm text-gray-600">{score}%</span>}
         </div>
       );
     };
     ```

6. **Create ModifyCodeModal Component**
   - File: `src/frontend/src/components/medicalCodes/ModifyCodeModal.tsx`
   - Inline search for alternative codes, rationale input (AC3)
   - Implementation:
     ```tsx
     import React, { useState } from 'react';
     import { useDispatch } from 'react-redux';
     import { modifyCode } from '../../store/slices/medicalCodeVerificationSlice';
     import { toast } from 'react-toastify';
     
     interface ModifyCodeModalProps {
       code: MedicalCodeSuggestion;
       isOpen: boolean;
       onClose: () => void;
     }
     
     export const ModifyCodeModal: React.FC<ModifyCodeModalProps> = ({ code, isOpen, onClose }) => {
       const dispatch = useDispatch();
       const [searchQuery, setSearchQuery] = useState('');
       const [selectedCode, setSelectedCode] = useState<{ code: string; description: string } | null>(null);
       const [rationale, setRationale] = useState('');
       const [searchResults, setSearchResults] = useState<any[]>([]);
       
       const handleSearch = async () => {
         // Integration with task_002_be_code_search_service
         const response = await fetch(`/api/knowledge/search?query=${searchQuery}&codeSystem=${code.codeSystem}&topK=10`);
         const data = await response.json();
         setSearchResults(data.results);
       };
       
       const handleSave = async () => {
         if (!selectedCode || !rationale) {
           toast.error('Please select a code and provide rationale');
           return;
         }
         
         await dispatch(modifyCode({
           codeId: code.id,
           newCode: selectedCode.code,
           newDescription: selectedCode.description,
           rationale
         }));
         
         toast.success('Code modified successfully', { autoClose: 200 }); // UXR-501: 200ms feedback
         onClose();
       };
       
       if (!isOpen) return null;
       
       return (
         <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
           <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl p-6">
             <h3 className="text-xl font-semibold mb-4">Modify Code</h3>
             
             <div className="mb-4">
               <label className="block text-sm font-medium text-gray-700 mb-2">Current Code</label>
               <div className="p-3 bg-gray-50 rounded">
                 <span className="font-mono font-semibold">{code.code}</span>
                 <span className="text-gray-600 ml-2">— {code.description}</span>
               </div>
             </div>
             
             <div className="mb-4">
               <label className="block text-sm font-medium text-gray-700 mb-2">Search Alternative Codes</label>
               <div className="flex gap-2">
                 <input
                   type="text"
                   value={searchQuery}
                   onChange={(e) => setSearchQuery(e.target.value)}
                   onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                   placeholder={`Search ${code.codeSystem} codes...`}
                   className="flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                 />
                 <button
                   onClick={handleSearch}
                   className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                 >
                   Search
                 </button>
               </div>
             </div>
             
             {searchResults.length > 0 && (
               <div className="mb-4 max-h-64 overflow-y-auto border rounded-lg">
                 {searchResults.map((result, index) => (
                   <div
                     key={index}
                     onClick={() => setSelectedCode({ code: result.code, description: result.description })}
                     className={`p-3 cursor-pointer hover:bg-gray-50 border-b last:border-b-0 ${
                       selectedCode?.code === result.code ? 'bg-blue-50' : ''
                     }`}
                   >
                     <span className="font-mono font-semibold">{result.code}</span>
                     <span className="text-gray-600 ml-2">— {result.description}</span>
                     <span className="text-xs text-gray-500 ml-2">(Confidence: {result.similarityScore}%)</span>
                   </div>
                 ))}
               </div>
             )}
             
             <div className="mb-6">
               <label className="block text-sm font-medium text-gray-700 mb-2">Modification Rationale *</label>
               <textarea
                 value={rationale}
                 onChange={(e) => setRationale(e.target.value)}
                 rows={3}
                 placeholder="Explain why you are modifying this code..."
                 className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                 required
               />
             </div>
             
             <div className="flex justify-end gap-3">
               <button
                 onClick={onClose}
                 className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
               >
                 Cancel
               </button>
               <button
                 onClick={handleSave}
                 disabled={!selectedCode || !rationale}
                 className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
               >
                 Save Changes
               </button>
             </div>
           </div>
         </div>
       );
     };
     ```

7. **Create RejectCodeModal Component**
   - File: `src/frontend/src/components/medicalCodes/RejectCodeModal.tsx`
   - Required rejection reason dropdown (AC4)
   - Implementation:
     ```tsx
     import React, { useState } from 'react';
     import { useDispatch } from 'react-redux';
     import { rejectCode } from '../../store/slices/medicalCodeVerificationSlice';
     import { toast } from 'react-toastify';
     
     interface RejectCodeModalProps {
       code: MedicalCodeSuggestion;
       isOpen: boolean;
       onClose: () => void;
     }
     
     const REJECTION_REASONS = [
       'Incorrect diagnosis',
       'Incorrect procedure code',
       'Insufficient documentation',
       'Code not supported by clinical evidence',
       'Wrong code system (ICD-10 vs CPT)',
       'Other (specify below)'
     ];
     
     export const RejectCodeModal: React.FC<RejectCodeModalProps> = ({ code, isOpen, onClose }) => {
       const dispatch = useDispatch();
       const [reason, setReason] = useState('');
       const [customReason, setCustomReason] = useState('');
       
       const handleReject = async () => {
         if (!reason) {
           toast.error('Please select a rejection reason');
           return;
         }
         
         const finalReason = reason === 'Other (specify below)' ? customReason : reason;
         
         if (reason === 'Other (specify below)' && !customReason) {
           toast.error('Please specify rejection reason');
           return;
         }
         
         await dispatch(rejectCode({
           codeId: code.id,
           reason: finalReason
         }));
         
         toast.error('Code rejected', { autoClose: 200 }); // UXR-501: 200ms feedback
         onClose();
       };
       
       if (!isOpen) return null;
       
       return (
         <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
           <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
             <h3 className="text-xl font-semibold mb-4 text-red-600">Reject Code</h3>
             
             <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded">
               <span className="font-mono font-semibold">{code.code}</span>
               <span className="text-gray-600 ml-2">— {code.description}</span>
             </div>
             
             <div className="mb-4">
               <label className="block text-sm font-medium text-gray-700 mb-2">Rejection Reason *</label>
               <select
                 value={reason}
                 onChange={(e) => setReason(e.target.value)}
                 className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500"
                 required
               >
                 <option value="">Select a reason...</option>
                 {REJECTION_REASONS.map((r) => (
                   <option key={r} value={r}>{r}</option>
                 ))}
               </select>
             </div>
             
             {reason === 'Other (specify below)' && (
               <div className="mb-4">
                 <label className="block text-sm font-medium text-gray-700 mb-2">Specify Reason</label>
                 <textarea
                   value={customReason}
                   onChange={(e) => setCustomReason(e.target.value)}
                   rows={3}
                   placeholder="Please provide specific details..."
                   className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500"
                   required
                 />
               </div>
             )}
             
             <div className="bg-amber-50 border border-amber-200 rounded p-3 mb-6">
               <p className="text-sm text-amber-800">
                 ⚠️ This code will be flagged for manual coding. The data point will require manual review before use.
               </p>
             </div>
             
             <div className="flex justify-end gap-3">
               <button
                 onClick={onClose}
                 className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
               >
                 Cancel
               </button>
               <button
                 onClick={handleReject}
                 disabled={!reason || (reason === 'Other (specify below)' && !customReason)}
                 className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
               >
                 Reject Code
               </button>
             </div>
           </div>
         </div>
       );
     };
     ```

8. **Create BulkVerificationControls Component**
   - File: `src/frontend/src/components/medicalCodes/BulkVerificationControls.tsx`
   - "Select All / Accept All" for confidence >95% (edge case handling)
   - Implementation:
     ```tsx
     import React from 'react';
     import { useDispatch, useSelector } from 'react-redux';
     import { selectAllHighConfidence, clearSelection, bulkAcceptCodes } from '../../store/slices/medicalCodeVerificationSlice';
     
     export const BulkVerificationControls: React.FC = () => {
       const dispatch = useDispatch();
       const { suggestions, selectedCodes } = useSelector((state: RootState) => state.medicalCodeVerification);
       
       const highConfidenceCodes = suggestions.filter(s => s.confidenceScore > 95 && s.verificationStatus === 'Pending');
       
       const handleSelectAll = () => {
         dispatch(selectAllHighConfidence());
       };
       
       const handleAcceptAll = async () => {
         if (selectedCodes.length === 0) {
           toast.warning('No codes selected');
           return;
         }
         
         await dispatch(bulkAcceptCodes(selectedCodes));
         toast.success(`${selectedCodes.length} codes accepted`, { autoClose: 200 });
       };
       
       return (
         <div className="flex items-center gap-4 p-4 bg-blue-50 border border-blue-200 rounded-lg mb-4">
           <div className="flex-1">
             <span className="text-sm text-blue-800">
               <strong>{highConfidenceCodes.length}</strong> high-confidence codes (>95%) available for bulk verification
             </span>
           </div>
           <button
             onClick={handleSelectAll}
             className="px-4 py-2 border border-blue-300 text-blue-700 rounded-lg hover:bg-blue-100"
           >
             Select All High-Confidence
           </button>
           <button
             onClick={handleAcceptAll}
             disabled={selectedCodes.length === 0}
             className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
           >
             Accept Selected ({selectedCodes.length})
           </button>
           <button
             onClick={() => dispatch(clearSelection())}
             className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
           >
             Clear
           </button>
         </div>
       );
     };
     ```

9. **Create CodeVerificationTable Component**
   - File: `src/frontend/src/components/medicalCodes/CodeVerificationTable.tsx`
   - Main table with all verification data (AC1)
   - Implementation includes checkbox column for bulk selection, action buttons per row

10. **Create MedicalCodeVerificationPage**
    - File: `src/frontend/src/pages/MedicalCodeVerificationPage.tsx`
    - Main page component integrating all subcomponents
    - Responsive design (UXR-201): 375px, 768px, 1440px breakpoints
    - Loading states (UXR-301): skeleton loaders for 4-8s
    - Empty states (UXR-605): "No pending verifications" message

## Current Project State

```
src/frontend/src/
├── pages/
│   └── (other pages)
├── components/
│   └── (existing components)
├── store/
│   ├── store.ts
│   └── slices/
└── api/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/pages/MedicalCodeVerificationPage.tsx | Main verification page |
| CREATE | src/frontend/src/components/medicalCodes/CodeVerificationTable.tsx | Verification table |
| CREATE | src/frontend/src/components/medicalCodes/VerificationBadge.tsx | Amber/green badge |
| CREATE | src/frontend/src/components/medicalCodes/ConfidenceBar.tsx | Confidence score bar |
| CREATE | src/frontend/src/components/medicalCodes/ModifyCodeModal.tsx | Modify code modal |
| CREATE | src/frontend/src/components/medicalCodes/RejectCodeModal.tsx | Reject code modal |
| CREATE | src/frontend/src/components/medicalCodes/BulkVerificationControls.tsx | Bulk actions |
| CREATE | src/frontend/src/store/slices/medicalCodeVerificationSlice.ts | Redux slice |
| CREATE | src/frontend/src/api/medicalCodesApi.ts | API client |
| CREATE | src/frontend/src/types/medicalCode.types.ts | TypeScript types |
| MODIFY | src/frontend/src/store/store.ts | Register slice |
| MODIFY | src/frontend/src/App.tsx | Add /staff/verification route |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### React + TypeScript Best Practices
- **React TypeScript Cheatsheet**: https://react-typescript-cheatsheet.netlify.app/
- **Redux Toolkit TypeScript**: https://redux-toolkit.js.org/usage/usage-with-typescript

### Tailwind CSS Utilities
- **Badge Components**: https://tailwindcss.com/docs/badge
- **Modal/Dialog**: https://headlessui.com/react/dialog

### React Toast Notifications
- **React-Toastify**: https://fkhadra.github.io/react-toastify/introduction

### Design Requirements
- **UXR-402**: Visual distinction between AI-suggested (amber) and staff-verified (green) badges (figma_spec.md)
- **UXR-501**: Visual feedback for actions within 200ms (figma_spec.md)
- **UXR-201**: Responsive design (375px, 768px, 1440px) (figma_spec.md)
- **UXR-301**: Loading states for 4-8s operations (figma_spec.md)
- **FR-036**: Accept, modify, or reject AI suggestions (spec.md)

### Wireframe Reference
- **SCR-023**: wireframe-SCR-023-clinical-verification.html

## Build Commands
```powershell
# Install dependencies (if needed)
cd src/frontend
npm install react-toastify

# Run development server
npm run dev

# Build for production
npm run build

# Run tests
npm test
```

## Validation Strategy

### Unit Tests
- File: `src/frontend/src/__tests__/components/VerificationBadge.test.tsx`
- Test cases:
  1. **Test_VerificationBadge_DisplaysAmberForPending**
     - Render: <VerificationBadge status="Pending" />
     - Assert: badge has amber background (bg-amber-100), label = "AI-Suggested"
  2. **Test_VerificationBadge_DisplaysGreenForVerified**
     - Render: <VerificationBadge status="StaffVerified" />
     - Assert: badge has green background (bg-green-100), label = "Staff-Verified"
  3. **Test_ConfidenceBar_HighConfidenceGreen**
     - Render: <ConfidenceBar score={90} />
     - Assert: bar color = green (bg-green-500), width = 90%
  4. **Test_ConfidenceBar_MediumConfidenceAmber**
     - Render: <ConfidenceBar score={75} />
     - Assert: bar color = amber (bg-amber-500), width = 75%
  5. **Test_ModifyCodeModal_SavesWithRationale**
     - Render: <ModifyCodeModal code={mockCode} isOpen={true} />
     - User: selects alternative code, enters rationale, clicks Save
     - Assert: dispatch(modifyCode) called with correct payload
  6. **Test_RejectCodeModal_RequiresReason**
     - Render: <RejectCodeModal code={mockCode} isOpen={true} />
     - User: clicks Reject without selecting reason
     - Assert: toast.error displayed, dispatch not called
  7. **Test_BulkVerificationControls_SelectsHighConfidence**
     - Setup: 10 codes, 5 with confidence >95%
     - User: clicks "Select All High-Confidence"
     - Assert: selectedCodes array contains 5 IDs

### Integration Tests
- File: `src/frontend/src/__tests__/pages/MedicalCodeVerificationPage.test.tsx`
- Test cases:
  1. **Test_VerificationPage_LoadsSuggestions**
     - Render: <MedicalCodeVerificationPage />
     - Wait for: API call to complete
     - Assert: Suggestions displayed in table
  2. **Test_AcceptButton_Updates200msToast**
     - User: clicks Accept button
     - Assert: Toast appears within 200ms (UXR-501), badge changes to green
  3. **Test_ResponsiveDesign_MobileLayout**
     - Viewport: 375px width
     - Assert: Table scrolls horizontally, buttons stack vertically

### Acceptance Criteria Validation
- **AC1**: ✅ Code value, description, confidence, source reference, action buttons displayed
- **AC2**: ✅ Accept changes status to "Accepted", badge turns green, user ID/timestamp recorded
- **AC3**: ✅ Modify opens search field, allows code selection, requires rationale
- **AC4**: ✅ Reject requires rejection reason, flags for manual coding
- **Edge Case 1**: ✅ Audit trail shows previous actions (most recent verification takes precedence)
- **Edge Case 2**: ✅ Bulk verification for confidence >95% implemented

## Success Criteria Checklist
- [MANDATORY] WIREFRAME VALIDATION: Rendered UI matches wireframe-SCR-023-clinical-verification.html layout and component structure
- [MANDATORY] MedicalCodeVerificationPage component created with SCR-023 layout
- [MANDATORY] VerificationBadge displays amber for "Pending" (AI-suggested) per UXR-402
- [MANDATORY] VerificationBadge displays green for "StaffVerified" per UXR-402
- [MANDATORY] ConfidenceBar color-coded: green >85%, amber 70-85%, red <70%
- [MANDATORY] AcceptButton calls PATCH /api/medical-codes/{codeId}/verify with "StaffVerified"
- [MANDATORY] ModifyCodeModal includes code search field (integration with task_002)
- [MANDATORY] ModifyCodeModal requires rationale input (AC3)
- [MANDATORY] RejectCodeModal requires rejection reason dropdown (AC4)
- [MANDATORY] BulkVerificationControls "Select All" filters confidence >95%
- [MANDATORY] Toast notifications appear within 200ms (UXR-501)
- [MANDATORY] Redux slice handles accept/modify/reject actions with optimistic updates
- [MANDATORY] Responsive design: 375px (mobile), 768px (tablet), 1440px (desktop) per UXR-201
- [MANDATORY] Loading states with skeleton loaders (UXR-301)
- [MANDATORY] Empty state: "No pending verifications" message (UXR-605)
- [MANDATORY] Unit test: VerificationBadge renders correct colors
- [MANDATORY] Unit test: ConfidenceBar displays correct percentage width
- [MANDATORY] Integration test: Accept action updates status and shows toast
- [RECOMMENDED] Audit trail display shows previous verification actions (edge case handling)
- [RECOMMENDED] Keyboard navigation support (Tab, Enter, Escape)

## Estimated Effort
**5 hours** (Page + table + modals + Redux + responsive design + unit tests)
