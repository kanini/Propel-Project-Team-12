/**
 * Patient Profile 360° Redux slice (US_049).
 * Manages state for comprehensive patient health dashboard.
 */

import {
  createSlice,
  createAsyncThunk,
  type PayloadAction,
} from "@reduxjs/toolkit";
import type { PatientProfile360 } from "../../types/patientProfile.types";
import {
  getPatientProfile360,
  type GetProfile360Params,
} from "../../api/patientProfileApi";

interface PatientProfileState {
  profile: PatientProfile360 | null;
  loading: boolean;
  error: string | null;
  vitalRangeStart: string | null; // ISO date for vital trends filtering
  vitalRangeEnd: string | null;
}

const initialState: PatientProfileState = {
  profile: null,
  loading: false,
  error: null,
  vitalRangeStart: null,
  vitalRangeEnd: null,
};

// Async thunk for fetching 360° profile
export const fetchPatientProfile360 = createAsyncThunk(
  "patientProfile/fetchProfile360",
  async (params: GetProfile360Params, { rejectWithValue }) => {
    try {
      return await getPatientProfile360(params);
    } catch (error) {
      if (error instanceof Error) {
        return rejectWithValue(error.message);
      }
      return rejectWithValue("An unknown error occurred");
    }
  },
);

const patientProfileSlice = createSlice({
  name: "patientProfile",
  initialState,
  reducers: {
    setVitalRangeDates: (
      state,
      action: PayloadAction<{ start: string | null; end: string | null }>,
    ) => {
      state.vitalRangeStart = action.payload.start;
      state.vitalRangeEnd = action.payload.end;
    },
    clearProfile: (state) => {
      state.profile = null;
      state.error = null;
      state.vitalRangeStart = null;
      state.vitalRangeEnd = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch profile 360
      .addCase(fetchPatientProfile360.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        fetchPatientProfile360.fulfilled,
        (state, action: PayloadAction<PatientProfile360>) => {
          state.loading = false;
          state.profile = action.payload;
          state.error = null;
        },
      )
      .addCase(fetchPatientProfile360.rejected, (state, action) => {
        state.loading = false;
        state.error =
          (action.payload as string) || "Failed to fetch patient profile";
      });
  },
});

export const { setVitalRangeDates, clearProfile } = patientProfileSlice.actions;
export default patientProfileSlice.reducer;
