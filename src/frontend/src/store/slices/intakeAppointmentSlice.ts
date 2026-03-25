/**
 * Redux slice for intake appointment selection (US_037)
 * Manages state for appointments requiring intake
 */

import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { RootState } from '../index';
import type { IntakeAppointment, IntakeAppointmentState } from '../../types/intakeAppointment';
import { fetchIntakeAppointments as apiFetchIntakeAppointments } from '../../api/intakeAppointmentApi';

/**
 * Initial state for intake appointment slice
 */
const initialState: IntakeAppointmentState = {
  appointments: [],
  status: 'idle',
  error: null,
};

/**
 * Async thunk to fetch appointments requiring intake
 */
export const fetchIntakeAppointments = createAsyncThunk<
  IntakeAppointment[],
  void,
  { state: RootState; rejectValue: string }
>(
  'intakeAppointments/fetchIntakeAppointments',
  async (_, { rejectWithValue }) => {
    try {
      const appointments = await apiFetchIntakeAppointments();
      return appointments;
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to fetch intake appointments'
      );
    }
  }
);

/**
 * Intake appointment slice with reducers and async thunk handlers
 */
const intakeAppointmentSlice = createSlice({
  name: 'intakeAppointments',
  initialState,
  reducers: {
    /**
     * Reset intake appointments state
     */
    resetIntakeAppointments: (state) => {
      state.appointments = [];
      state.status = 'idle';
      state.error = null;
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
      .addCase(fetchIntakeAppointments.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchIntakeAppointments.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.appointments = action.payload;
        state.error = null;
      })
      .addCase(fetchIntakeAppointments.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? 'An error occurred';
      });
  },
});

// Export actions
export const { resetIntakeAppointments, clearError } = intakeAppointmentSlice.actions;

// Export selectors
export const selectIntakeAppointments = (state: RootState) => state.intakeAppointments.appointments;
export const selectIntakeAppointmentsStatus = (state: RootState) => state.intakeAppointments.status;
export const selectIntakeAppointmentsError = (state: RootState) => state.intakeAppointments.error;

// Export reducer
export default intakeAppointmentSlice.reducer;
