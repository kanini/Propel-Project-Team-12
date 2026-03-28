/**
 * Health Dashboard Redux slice for SCR-016 - Patient Health Dashboard 360°
 * Manages state for clinical data, vitals, conditions, medications, allergies, and medical codes
 */

import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { RootState } from '../index';
import type { HealthDashboard360Dto } from '../../types/clinicalData';
import * as healthApi from '../../api/healthDashboardApi';

interface HealthDashboardState {
  dashboard: HealthDashboard360Dto | null;
  isLoading: boolean;
  error: string | null;
  activeTab: 'conditions' | 'medications' | 'allergies' | 'vitals' | 'labResults' | 'codes';
}

const initialState: HealthDashboardState = {
  dashboard: null,
  isLoading: false,
  error: null,
  activeTab: 'conditions',
};

/**
 * Fetch authenticated patient's own health dashboard
 */
export const fetchHealthDashboard = createAsyncThunk(
  'healthDashboard/fetch',
  async (_, { rejectWithValue }) => {
    try {
      return await healthApi.fetchHealthDashboard();
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to fetch health dashboard');
    }
  }
);

/**
 * Fetch a specific patient's health dashboard (staff view)
 */
export const fetchPatientHealthDashboard = createAsyncThunk(
  'healthDashboard/fetchPatient',
  async (patientId: string, { rejectWithValue }) => {
    try {
      return await healthApi.fetchPatientHealthDashboard(patientId);
    } catch (error) {
      if (error instanceof Error) return rejectWithValue(error.message);
      return rejectWithValue('Failed to fetch patient health dashboard');
    }
  }
);

const healthDashboardSlice = createSlice({
  name: 'healthDashboard',
  initialState,
  reducers: {
    setActiveTab: (state, action) => {
      state.activeTab = action.payload;
    },
    clearDashboard: (state) => {
      state.dashboard = null;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchHealthDashboard.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchHealthDashboard.fulfilled, (state, action) => {
        state.isLoading = false;
        state.dashboard = action.payload;
      })
      .addCase(fetchHealthDashboard.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(fetchPatientHealthDashboard.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchPatientHealthDashboard.fulfilled, (state, action) => {
        state.isLoading = false;
        state.dashboard = action.payload;
      })
      .addCase(fetchPatientHealthDashboard.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { setActiveTab, clearDashboard } = healthDashboardSlice.actions;
export default healthDashboardSlice.reducer;

export const selectHealthDashboard = (state: RootState) => state.healthDashboard.dashboard;
export const selectHealthLoading = (state: RootState) => state.healthDashboard.isLoading;
export const selectHealthError = (state: RootState) => state.healthDashboard.error;
export const selectActiveTab = (state: RootState) => state.healthDashboard.activeTab;
