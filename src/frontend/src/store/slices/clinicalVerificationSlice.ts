/**
 * Clinical Verification Redux slice for SCR-023
 * Manages state for staff clinical data verification workflow
 */

import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { RootState } from '../index';
import type {
  ClinicalVerificationDashboardDto,
  VerificationQueueItemDto,
  RejectActionDto,
  ModifyCodeDto,
} from '../../types/clinicalData';
import * as verificationApi from '../../api/clinicalVerificationApi';

interface ClinicalVerificationState {
  queue: VerificationQueueItemDto[];
  queueLoading: boolean;
  queueError: string | null;
  dashboard: ClinicalVerificationDashboardDto | null;
  isLoading: boolean;
  error: string | null;
  activeTab: 'clinicalData' | 'medicalCodes';
  actionInProgress: string | null; // ID of item being acted on
  searchTerm: string;
  statusFilter: string | null;
}

const initialState: ClinicalVerificationState = {
  queue: [],
  queueLoading: false,
  queueError: null,
  dashboard: null,
  isLoading: false,
  error: null,
  activeTab: 'clinicalData',
  actionInProgress: null,
  searchTerm: '',
  statusFilter: null,
};

/**
 * Fetch verification queue — patients with pending verifications
 */
export const fetchVerificationQueue = createAsyncThunk(
  'clinicalVerification/fetchQueue',
  async ({ limit = 10, search }: { limit?: number; search?: string } = {}, { rejectWithValue }) => {
    try {
      return await verificationApi.fetchVerificationQueue(limit, search);
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to fetch verification queue');
    }
  }
);

/**
 * Fetch verification dashboard for a patient
 */
export const fetchVerificationDashboard = createAsyncThunk(
  'clinicalVerification/fetchDashboard',
  async (patientId: string, { rejectWithValue }) => {
    try {
      return await verificationApi.fetchVerificationDashboard(patientId);
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to fetch verification data');
    }
  }
);

/**
 * Verify a clinical data point
 */
export const verifyDataPoint = createAsyncThunk(
  'clinicalVerification/verifyData',
  async (dataId: string, { rejectWithValue }) => {
    try {
      await verificationApi.verifyDataPoint(dataId);
      return dataId;
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to verify data point');
    }
  }
);

/**
 * Reject a clinical data point
 */
export const rejectDataPoint = createAsyncThunk(
  'clinicalVerification/rejectData',
  async (payload: RejectActionDto, { rejectWithValue }) => {
    try {
      await verificationApi.rejectDataPoint(payload);
      return payload.id;
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to reject data point');
    }
  }
);

/**
 * Accept/verify a medical code
 */
export const acceptMedicalCode = createAsyncThunk(
  'clinicalVerification/acceptCode',
  async (codeId: string, { rejectWithValue }) => {
    try {
      await verificationApi.acceptMedicalCode(codeId);
      return codeId;
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to verify medical code');
    }
  }
);

/**
 * Reject a medical code
 */
export const rejectMedicalCode = createAsyncThunk(
  'clinicalVerification/rejectCode',
  async ({ codeId, reason }: { codeId: string; reason?: string }, { rejectWithValue }) => {
    try {
      await verificationApi.rejectMedicalCode(codeId, reason);
      return codeId;
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to reject medical code');
    }
  }
);

/**
 * Modify a medical code
 */
export const modifyMedicalCode = createAsyncThunk(
  'clinicalVerification/modifyCode',
  async (payload: ModifyCodeDto, { rejectWithValue }) => {
    try {
      await verificationApi.modifyMedicalCode(payload);
      return payload;
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to modify medical code');
    }
  }
);

const clinicalVerificationSlice = createSlice({
  name: 'clinicalVerification',
  initialState,
  reducers: {
    setVerificationTab: (state, action: PayloadAction<'clinicalData' | 'medicalCodes'>) => {
      state.activeTab = action.payload;
    },
    setVerificationSearchTerm: (state, action: PayloadAction<string>) => {
      state.searchTerm = action.payload;
    },
    setVerificationStatusFilter: (state, action: PayloadAction<string | null>) => {
      state.statusFilter = action.payload;
    },
    clearVerificationState: (state) => {
      state.dashboard = null;
      state.error = null;
      state.actionInProgress = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch queue
    builder
      .addCase(fetchVerificationQueue.pending, (state) => {
        state.queueLoading = true;
        state.queueError = null;
      })
      .addCase(fetchVerificationQueue.fulfilled, (state, action) => {
        state.queueLoading = false;
        state.queue = action.payload.items;
      })
      .addCase(fetchVerificationQueue.rejected, (state, action) => {
        state.queueLoading = false;
        state.queueError = action.payload as string;
      });

    // Fetch dashboard
    builder
      .addCase(fetchVerificationDashboard.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchVerificationDashboard.fulfilled, (state, action) => {
        state.isLoading = false;
        state.dashboard = action.payload;
      })
      .addCase(fetchVerificationDashboard.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Verify data point
    builder
      .addCase(verifyDataPoint.pending, (state, action) => {
        state.actionInProgress = action.meta.arg;
      })
      .addCase(verifyDataPoint.fulfilled, (state, action) => {
        state.actionInProgress = null;
        if (state.dashboard) {
          const item = state.dashboard.clinicalData.find(d => d.extractedDataId === action.payload);
          if (item) {
            item.verificationStatus = 'Verified';
            state.dashboard.verifiedCount++;
            state.dashboard.pendingCount = Math.max(0, state.dashboard.pendingCount - 1);
          }
        }
      })
      .addCase(verifyDataPoint.rejected, (state) => {
        state.actionInProgress = null;
      });

    // Reject data point
    builder
      .addCase(rejectDataPoint.pending, (state, action) => {
        state.actionInProgress = action.meta.arg.id;
      })
      .addCase(rejectDataPoint.fulfilled, (state, action) => {
        state.actionInProgress = null;
        if (state.dashboard) {
          const item = state.dashboard.clinicalData.find(d => d.extractedDataId === action.payload);
          if (item) {
            item.verificationStatus = 'Rejected';
            state.dashboard.rejectedCount++;
            state.dashboard.pendingCount = Math.max(0, state.dashboard.pendingCount - 1);
          }
        }
      })
      .addCase(rejectDataPoint.rejected, (state) => {
        state.actionInProgress = null;
      });

    // Accept medical code
    builder
      .addCase(acceptMedicalCode.pending, (state, action) => {
        state.actionInProgress = action.meta.arg;
      })
      .addCase(acceptMedicalCode.fulfilled, (state, action) => {
        state.actionInProgress = null;
        if (state.dashboard) {
          const code = state.dashboard.medicalCodes.find(c => c.medicalCodeId === action.payload);
          if (code) code.verificationStatus = 'Verified';
        }
      })
      .addCase(acceptMedicalCode.rejected, (state) => {
        state.actionInProgress = null;
      });

    // Reject medical code
    builder
      .addCase(rejectMedicalCode.pending, (state, action) => {
        state.actionInProgress = action.meta.arg.codeId;
      })
      .addCase(rejectMedicalCode.fulfilled, (state, action) => {
        state.actionInProgress = null;
        if (state.dashboard) {
          const code = state.dashboard.medicalCodes.find(c => c.medicalCodeId === action.payload);
          if (code) code.verificationStatus = 'Rejected';
        }
      })
      .addCase(rejectMedicalCode.rejected, (state) => {
        state.actionInProgress = null;
      });

    // Modify medical code
    builder
      .addCase(modifyMedicalCode.pending, (state, action) => {
        state.actionInProgress = action.meta.arg.medicalCodeId;
      })
      .addCase(modifyMedicalCode.fulfilled, (state, action) => {
        state.actionInProgress = null;
        if (state.dashboard) {
          const code = state.dashboard.medicalCodes.find(c => c.medicalCodeId === action.payload.medicalCodeId);
          if (code) {
            code.codeValue = action.payload.codeValue;
            code.codeDescription = action.payload.codeDescription;
            code.verificationStatus = 'Verified';
          }
        }
      })
      .addCase(modifyMedicalCode.rejected, (state) => {
        state.actionInProgress = null;
      });
  },
});

export const {
  setVerificationTab,
  setVerificationSearchTerm,
  setVerificationStatusFilter,
  clearVerificationState,
} = clinicalVerificationSlice.actions;

export default clinicalVerificationSlice.reducer;

export const selectVerificationQueue = (state: RootState) => state.clinicalVerification.queue;
export const selectQueueLoading = (state: RootState) => state.clinicalVerification.queueLoading;
export const selectQueueError = (state: RootState) => state.clinicalVerification.queueError;
export const selectVerificationDashboard = (state: RootState) => state.clinicalVerification.dashboard;
export const selectVerificationLoading = (state: RootState) => state.clinicalVerification.isLoading;
export const selectVerificationError = (state: RootState) => state.clinicalVerification.error;
export const selectVerificationTab = (state: RootState) => state.clinicalVerification.activeTab;
export const selectVerificationAction = (state: RootState) => state.clinicalVerification.actionInProgress;
