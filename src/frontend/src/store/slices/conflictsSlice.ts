import {
  createSlice,
  createAsyncThunk,
  type PayloadAction,
} from "@reduxjs/toolkit";
import type {
  DataConflict,
  ConflictSummary,
  ResolveConflictRequest,
} from "../../types/conflict.types";
import { ConflictSeverity } from "../../types/conflict.types";
import * as conflictsApi from "../../api/conflictsApi";

/**
 * Conflicts slice state (US_048)
 */
interface ConflictsState {
  conflicts: DataConflict[];
  conflictsLoading: boolean;
  conflictsError: string | null;
  summary: ConflictSummary | null;
  summaryLoading: boolean;
  summaryError: string | null;
  selectedConflict: DataConflict | null;
  isModalOpen: boolean;
  filterSeverity: ConflictSeverity | null;
  currentPage: number;
  pageSize: number;
  resolvingConflictId: string | null;
}

const initialState: ConflictsState = {
  conflicts: [],
  conflictsLoading: false,
  conflictsError: null,
  summary: null,
  summaryLoading: false,
  summaryError: null,
  selectedConflict: null,
  isModalOpen: false,
  filterSeverity: null,
  currentPage: 1,
  pageSize: 10,
  resolvingConflictId: null,
};

/**
 * Async thunk: Fetch patient conflicts with filtering (US_048, AC2)
 */
export const fetchPatientConflicts = createAsyncThunk(
  "conflicts/fetchPatientConflicts",
  async (params: conflictsApi.GetConflictsParams, { rejectWithValue }) => {
    try {
      return await conflictsApi.getPatientConflicts(params);
    } catch (error) {
      if (error instanceof Error) {
        return rejectWithValue(error.message);
      }
      return rejectWithValue("Failed to fetch conflicts");
    }
  },
);

/**
 * Async thunk: Fetch conflict summary (US_048, AC2)
 */
export const fetchConflictSummary = createAsyncThunk(
  "conflicts/fetchConflictSummary",
  async (patientId: number, { rejectWithValue }) => {
    try {
      return await conflictsApi.getConflictSummary(patientId);
    } catch (error) {
      if (error instanceof Error) {
        return rejectWithValue(error.message);
      }
      return rejectWithValue("Failed to fetch conflict summary");
    }
  },
);

/**
 * Async thunk: Resolve conflict (US_048, AC2)
 */
export const resolveConflictThunk = createAsyncThunk(
  "conflicts/resolveConflict",
  async (
    {
      conflictId,
      request,
    }: { conflictId: string; request: ResolveConflictRequest },
    { rejectWithValue },
  ) => {
    try {
      return await conflictsApi.resolveConflict(conflictId, request);
    } catch (error) {
      if (error instanceof Error) {
        return rejectWithValue(error.message);
      }
      return rejectWithValue("Failed to resolve conflict");
    }
  },
);

const conflictsSlice = createSlice({
  name: "conflicts",
  initialState,
  reducers: {
    /**
     * Open conflict detail modal with selected conflict
     */
    openConflictModal: (state, action: PayloadAction<DataConflict>) => {
      state.selectedConflict = action.payload;
      state.isModalOpen = true;
    },

    /**
     * Close conflict detail modal
     */
    closeConflictModal: (state) => {
      state.isModalOpen = false;
      state.selectedConflict = null;
    },

    /**
     * Set severity filter for conflict list
     */
    setFilterSeverity: (
      state,
      action: PayloadAction<ConflictSeverity | null>,
    ) => {
      state.filterSeverity = action.payload;
      state.currentPage = 1; // Reset to first page when filter changes
    },

    /**
     * Set current page for pagination
     */
    setCurrentPage: (state, action: PayloadAction<number>) => {
      state.currentPage = action.payload;
    },

    /**
     * Add new conflict from Pusher real-time event
     */
    addConflictFromPusher: (state, action: PayloadAction<DataConflict>) => {
      // Prepend new conflict to list (newest first)
      state.conflicts.unshift(action.payload);

      // Update summary counts
      if (state.summary) {
        state.summary.totalUnresolved++;
        if (action.payload.severity === ConflictSeverity.Critical) {
          state.summary.criticalCount++;
        } else if (action.payload.severity === ConflictSeverity.Warning) {
          state.summary.warningCount++;
        } else {
          state.summary.infoCount++;
        }
      }
    },

    /**
     * Clear conflicts (used when switching patients)
     */
    clearConflicts: (state) => {
      state.conflicts = [];
      state.summary = null;
      state.selectedConflict = null;
      state.isModalOpen = false;
      state.filterSeverity = null;
      state.currentPage = 1;
    },
  },
  extraReducers: (builder) => {
    // Fetch patient conflicts
    builder.addCase(fetchPatientConflicts.pending, (state) => {
      state.conflictsLoading = true;
      state.conflictsError = null;
    });
    builder.addCase(fetchPatientConflicts.fulfilled, (state, action) => {
      state.conflictsLoading = false;
      state.conflicts = action.payload;
    });
    builder.addCase(fetchPatientConflicts.rejected, (state, action) => {
      state.conflictsLoading = false;
      state.conflictsError = action.payload as string;
    });

    // Fetch conflict summary
    builder.addCase(fetchConflictSummary.pending, (state) => {
      state.summaryLoading = true;
      state.summaryError = null;
    });
    builder.addCase(fetchConflictSummary.fulfilled, (state, action) => {
      state.summaryLoading = false;
      state.summary = action.payload;
    });
    builder.addCase(fetchConflictSummary.rejected, (state, action) => {
      state.summaryLoading = false;
      state.summaryError = action.payload as string;
    });

    // Resolve conflict
    builder.addCase(resolveConflictThunk.pending, (state, action) => {
      state.resolvingConflictId = action.meta.arg.conflictId;
    });
    builder.addCase(resolveConflictThunk.fulfilled, (state, action) => {
      state.resolvingConflictId = null;

      // Update conflict in list
      const index = state.conflicts.findIndex(
        (c) => c.id === action.payload.id,
      );
      if (index !== -1) {
        state.conflicts[index] = action.payload;
      }

      // Update summary
      if (state.summary) {
        state.summary.totalUnresolved = Math.max(
          0,
          state.summary.totalUnresolved - 1,
        );
        if (action.payload.severity === ConflictSeverity.Critical) {
          state.summary.criticalCount = Math.max(
            0,
            state.summary.criticalCount - 1,
          );
        } else if (action.payload.severity === ConflictSeverity.Warning) {
          state.summary.warningCount = Math.max(
            0,
            state.summary.warningCount - 1,
          );
        } else {
          state.summary.infoCount = Math.max(0, state.summary.infoCount - 1);
        }
      }

      // Close modal after resolution
      state.isModalOpen = false;
      state.selectedConflict = null;
    });
    builder.addCase(resolveConflictThunk.rejected, (state, action) => {
      state.resolvingConflictId = null;
      state.conflictsError = action.payload as string;
    });
  },
});

export const {
  openConflictModal,
  closeConflictModal,
  setFilterSeverity,
  setCurrentPage,
  addConflictFromPusher,
  clearConflicts,
} = conflictsSlice.actions;

export default conflictsSlice.reducer;
