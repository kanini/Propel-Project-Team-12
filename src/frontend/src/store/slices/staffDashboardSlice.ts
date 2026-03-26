/**
 * Staff Dashboard slice for US_068 - Staff Dashboard
 * Manages staff dashboard metrics and queue preview state
 */

import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { RootState } from '../index';
import { getDashboardMetrics, getQueuePreview } from '../../api/staffApi';

/**
 * Dashboard metrics DTO
 */
export interface DashboardMetricsDto {
  todayAppointments: number;
  currentQueueSize: number;
  pendingVerifications: number;
}

/**
 * Queue preview DTO
 */
export interface QueuePreviewDto {
  appointmentId: string;
  patientName: string;
  providerName: string;
  appointmentTime: string;
  estimatedWait: string;
  riskLevel: 'low' | 'medium' | 'high';
  status: string;
}

/**
 * Staff dashboard slice state interface
 */
interface StaffDashboardState {
  metrics: DashboardMetricsDto | null;
  queuePreview: QueuePreviewDto[];
  isLoading: boolean;
  error: string | null;
  lastUpdated: string | null;
}

/**
 * Initial state for staff dashboard slice
 */
const initialState: StaffDashboardState = {
  metrics: null,
  queuePreview: [],
  isLoading: false,
  error: null,
  lastUpdated: null,
};

/**
 * Async thunk for fetching dashboard metrics (US_068, AC2, NFR-001: 500ms target)
 */
export const fetchMetrics = createAsyncThunk<
  DashboardMetricsDto,
  void,
  { state: RootState; rejectValue: string }
>(
  'staffDashboard/fetchMetrics',
  async (_, { rejectWithValue }) => {
    try {
      return await getDashboardMetrics();
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to fetch metrics'
      );
    }
  }
);

/**
 * Async thunk for fetching queue preview (US_068, AC4, NFR-001: 500ms target)
 */
export const fetchQueuePreview = createAsyncThunk<
  QueuePreviewDto[],
  number | undefined,
  { state: RootState; rejectValue: string }
>(
  'staffDashboard/fetchQueuePreview',
  async (count = 5, { rejectWithValue }) => {
    try {
      return await getQueuePreview(count);
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to fetch queue preview'
      );
    }
  }
);

/**
 * Combined async thunk for loading all dashboard data in parallel
 */
export const loadDashboardData = createAsyncThunk<
  { metrics: DashboardMetricsDto; queue: QueuePreviewDto[] },
  void,
  { state: RootState; rejectValue: string }
>(
  'staffDashboard/loadDashboardData',
  async (_, { dispatch, rejectWithValue }) => {
    try {
      const [metricsResult, queueResult] = await Promise.all([
        dispatch(fetchMetrics()).unwrap(),
        dispatch(fetchQueuePreview(5)).unwrap(),
      ]);
      return { metrics: metricsResult, queue: queueResult };
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to load dashboard data'
      );
    }
  }
);

/**
 * Staff dashboard slice
 */
const staffDashboardSlice = createSlice({
  name: 'staffDashboard',
  initialState,
  reducers: {
    /**
     * Update queue preview in real-time (US_068, AC7 - Pusher integration)
     */
    updateQueueRealtime: (state, action: PayloadAction<QueuePreviewDto[]>) => {
      state.queuePreview = action.payload;
      state.lastUpdated = new Date().toISOString();
    },
    /**
     * Clear dashboard errors
     */
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // fetchMetrics
      .addCase(fetchMetrics.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchMetrics.fulfilled, (state, action) => {
        state.metrics = action.payload;
        state.isLoading = false;
        state.lastUpdated = new Date().toISOString();
      })
      .addCase(fetchMetrics.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // fetchQueuePreview
      .addCase(fetchQueuePreview.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchQueuePreview.fulfilled, (state, action) => {
        state.queuePreview = action.payload;
        state.isLoading = false;
        state.lastUpdated = new Date().toISOString();
      })
      .addCase(fetchQueuePreview.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // loadDashboardData
      .addCase(loadDashboardData.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(loadDashboardData.fulfilled, (state, action) => {
        state.metrics = action.payload.metrics;
        state.queuePreview = action.payload.queue;
        state.isLoading = false;
        state.lastUpdated = new Date().toISOString();
      })
      .addCase(loadDashboardData.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

/**
 * Export actions
 */
export const { updateQueueRealtime, clearError } = staffDashboardSlice.actions;

/**
 * Selectors
 */
export const selectDashboardMetrics = (state: RootState) => state.staffDashboard.metrics;
export const selectQueuePreview = (state: RootState) => state.staffDashboard.queuePreview;
export const selectDashboardLoading = (state: RootState) => state.staffDashboard.isLoading;
export const selectDashboardError = (state: RootState) => state.staffDashboard.error;
export const selectLastUpdated = (state: RootState) => state.staffDashboard.lastUpdated;

/**
 * Export reducer
 */
export default staffDashboardSlice.reducer;
