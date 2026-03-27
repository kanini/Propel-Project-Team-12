/**
 * Redux slice for Medical Code Verification state management (EP-008-US-052)
 */

import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import { medicalCodesApi } from '../../api/medicalCodesApi';
import type {
  MedicalCodeSuggestion,
  ModifyCodeRequest,
  RejectCodeRequest,
} from '../../types/medicalCode.types';
import type { RootState } from '../index';

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
  selectedCodes: [],
};

/**
 * Fetch AI-suggested medical codes for verification
 */
export const fetchSuggestions = createAsyncThunk(
  'medicalCodeVerification/fetchSuggestions',
  async (extractedDataId: string) => {
    return await medicalCodesApi.getSuggestionsForPatient(extractedDataId);
  }
);

/**
 * Accept an AI-suggested code (AC2)
 */
export const acceptCode = createAsyncThunk(
  'medicalCodeVerification/acceptCode',
  async (codeId: string) => {
    await medicalCodesApi.acceptCode(codeId);
    return codeId;
  }
);

/**
 * Modify an AI-suggested code with alternative (AC3)
 */
export const modifyCode = createAsyncThunk(
  'medicalCodeVerification/modifyCode',
  async (request: ModifyCodeRequest) => {
    await medicalCodesApi.modifyCode(request);
    return request;
  }
);

/**
 * Reject an AI-suggested code (AC4)
 */
export const rejectCode = createAsyncThunk(
  'medicalCodeVerification/rejectCode',
  async (request: RejectCodeRequest) => {
    await medicalCodesApi.rejectCode(request);
    return request;
  }
);

/**
 * Bulk accept high-confidence codes (Edge Case 2)
 */
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
    /**
     * Select a single code for side panel display
     */
    selectCode: (state, action: PayloadAction<string>) => {
      state.selectedCodeId = action.payload;
    },

    /**
     * Toggle code selection for bulk operations
     */
    toggleCodeSelection: (state, action: PayloadAction<string>) => {
      if (state.selectedCodes.includes(action.payload)) {
        state.selectedCodes = state.selectedCodes.filter(
          (id) => id !== action.payload
        );
      } else {
        state.selectedCodes.push(action.payload);
      }
    },

    /**
     * Select all high-confidence codes (>95%) for bulk acceptance
     */
    selectAllHighConfidence: (state) => {
      state.selectedCodes = state.suggestions
        .filter(
          (s) =>
            s.confidenceScore > 95 && s.verificationStatus === 'Pending'
        )
        .map((s) => s.id);
    },

    /**
     * Clear all selected codes
     */
    clearSelection: (state) => {
      state.selectedCodes = [];
    },

    /**
     * Clear error state
     */
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch suggestions
      .addCase(fetchSuggestions.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchSuggestions.fulfilled, (state, action) => {
        state.isLoading = false;
        state.suggestions = action.payload;
      })
      .addCase(fetchSuggestions.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to fetch suggestions';
      })
      // Accept code
      .addCase(acceptCode.fulfilled, (state, action) => {
        const code = state.suggestions.find((s) => s.id === action.payload);
        if (code) {
          code.verificationStatus = 'StaffVerified';
          code.verifiedAt = new Date().toISOString();
        }
      })
      .addCase(acceptCode.rejected, (state, action) => {
        state.error = action.error.message || 'Failed to accept code';
      })
      // Modify code
      .addCase(modifyCode.fulfilled, (state, action) => {
        const code = state.suggestions.find(
          (s) => s.id === action.payload.codeId
        );
        if (code) {
          code.code = action.payload.newCode;
          code.description = action.payload.newDescription;
          code.verificationStatus = 'StaffVerified';
          code.verifiedAt = new Date().toISOString();
        }
      })
      .addCase(modifyCode.rejected, (state, action) => {
        state.error = action.error.message || 'Failed to modify code';
      })
      // Reject code
      .addCase(rejectCode.fulfilled, (state, action) => {
        const code = state.suggestions.find(
          (s) => s.id === action.payload.codeId
        );
        if (code) {
          code.verificationStatus = 'StaffRejected';
          code.verifiedAt = new Date().toISOString();
        }
      })
      .addCase(rejectCode.rejected, (state, action) => {
        state.error = action.error.message || 'Failed to reject code';
      })
      // Bulk accept codes
      .addCase(bulkAcceptCodes.fulfilled, (state, action) => {
        action.payload.forEach((codeId) => {
          const code = state.suggestions.find((s) => s.id === codeId);
          if (code) {
            code.verificationStatus = 'StaffVerified';
            code.verifiedAt = new Date().toISOString();
          }
        });
        state.selectedCodes = [];
      })
      .addCase(bulkAcceptCodes.rejected, (state, action) => {
        state.error = action.error.message || 'Failed to bulk accept codes';
      });
  },
});

export const {
  selectCode,
  toggleCodeSelection,
  selectAllHighConfidence,
  clearSelection,
  clearError,
} = medicalCodeVerificationSlice.actions;

// Selectors
export const selectAllSuggestions = (state: RootState) =>
  state.medicalCodeVerification.suggestions;

export const selectSelectedCode = (state: RootState) => {
  const id = state.medicalCodeVerification.selectedCodeId;
  if (!id) return null;
  return state.medicalCodeVerification.suggestions.find((s) => s.id === id);
};

export const selectHighConfidenceCodes = (state: RootState) =>
  state.medicalCodeVerification.suggestions.filter(
    (s) => s.confidenceScore > 95 && s.verificationStatus === 'Pending'
  );

export default medicalCodeVerificationSlice.reducer;
