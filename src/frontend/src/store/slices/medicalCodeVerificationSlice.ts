/**
 * Medical Code Verification Redux Slice
 * State management for medical code verification workflow
 */

import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import { medicalCodesApi } from '../../api/medicalCodesApi';
import type {
  MedicalCodeSuggestion,
  ModifyCodeRequest,
  RejectCodeRequest,
  AcceptCodeRequest,
} from '../../types/medicalCode.types';

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
 * Fetch all code suggestions for a patient
 */
export const fetchSuggestions = createAsyncThunk(
  'medicalCodeVerification/fetchSuggestions',
  async (extractedDataId: string, { rejectWithValue }) => {
    try {
      return await medicalCodesApi.getSuggestionsForPatient(extractedDataId);
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch code suggestions'
      );
    }
  }
);

/**
 * Accept a code suggestion (AC2)
 */
export const acceptCode = createAsyncThunk(
  'medicalCodeVerification/acceptCode',
  async (request: AcceptCodeRequest, { rejectWithValue }) => {
    try {
      await medicalCodesApi.acceptCode(request);
      return request.codeId;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to accept code'
      );
    }
  }
);

/**
 * Modify a code suggestion (AC3)
 */
export const modifyCode = createAsyncThunk(
  'medicalCodeVerification/modifyCode',
  async (request: ModifyCodeRequest, { rejectWithValue }) => {
    try {
      await medicalCodesApi.modifyCode(request);
      return request;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to modify code'
      );
    }
  }
);

/**
 * Reject a code suggestion (AC4)
 */
export const rejectCode = createAsyncThunk(
  'medicalCodeVerification/rejectCode',
  async (request: RejectCodeRequest, { rejectWithValue }) => {
    try {
      await medicalCodesApi.rejectCode(request);
      return request;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to reject code'
      );
    }
  }
);

/**
 * Bulk accept high-confidence codes (Edge case)
 */
export const bulkAcceptCodes = createAsyncThunk(
  'medicalCodeVerification/bulkAcceptCodes',
  async (codeIds: string[], { rejectWithValue }) => {
    try {
      await medicalCodesApi.bulkAcceptCodes(codeIds);
      return codeIds;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to bulk accept codes'
      );
    }
  }
);

const medicalCodeVerificationSlice = createSlice({
  name: 'medicalCodeVerification',
  initialState,
  reducers: {
    /**
     * Select a code for side panel display
     */
    selectCode: (state, action: PayloadAction<string | null>) => {
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
     * Select all high-confidence codes (>95%)
     */
    selectAllHighConfidence: (state) => {
      state.selectedCodes = state.suggestions
        .filter(
          (s) =>
            s.confidenceScore > 95 && s.verificationStatus === 'AISuggested'
        )
        .map((s) => s.id);
    },

    /**
     * Clear all selections
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
        state.error = action.payload as string;
      })

      // Accept code
      .addCase(acceptCode.pending, (state) => {
        state.error = null;
      })
      .addCase(acceptCode.fulfilled, (state, action) => {
        const code = state.suggestions.find((s) => s.id === action.payload);
        if (code) {
          code.verificationStatus = 'Accepted';
          code.verifiedAt = new Date().toISOString();
        }
      })
      .addCase(acceptCode.rejected, (state, action) => {
        state.error = action.payload as string;
      })

      // Modify code
      .addCase(modifyCode.pending, (state) => {
        state.error = null;
      })
      .addCase(modifyCode.fulfilled, (state, action) => {
        const code = state.suggestions.find(
          (s) => s.id === action.payload.codeId
        );
        if (code) {
          code.code = action.payload.newCode;
          code.description = action.payload.newDescription;
          code.rationale = action.payload.rationale;
          code.verificationStatus = 'Modified';
          code.verifiedAt = new Date().toISOString();
        }
      })
      .addCase(modifyCode.rejected, (state, action) => {
        state.error = action.payload as string;
      })

      // Reject code
      .addCase(rejectCode.pending, (state) => {
        state.error = null;
      })
      .addCase(rejectCode.fulfilled, (state, action) => {
        const code = state.suggestions.find(
          (s) => s.id === action.payload.codeId
        );
        if (code) {
          code.verificationStatus = 'Rejected';
          code.verifiedAt = new Date().toISOString();
        }
      })
      .addCase(rejectCode.rejected, (state, action) => {
        state.error = action.payload as string;
      })

      // Bulk accept codes
      .addCase(bulkAcceptCodes.pending, (state) => {
        state.error = null;
      })
      .addCase(bulkAcceptCodes.fulfilled, (state, action) => {
        action.payload.forEach((codeId) => {
          const code = state.suggestions.find((s) => s.id === codeId);
          if (code) {
            code.verificationStatus = 'Accepted';
            code.verifiedAt = new Date().toISOString();
          }
        });
        state.selectedCodes = [];
      })
      .addCase(bulkAcceptCodes.rejected, (state, action) => {
        state.error = action.payload as string;
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

export default medicalCodeVerificationSlice.reducer;
