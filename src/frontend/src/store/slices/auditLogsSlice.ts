import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import {
  fetchAuditLogs,
  type AuditLogEntry,
  type AuditLogFilters,
} from "../../api/auditApi";

interface AuditLogsState {
  items: AuditLogEntry[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  isLoading: boolean;
  error: string | null;
}

const initialState: AuditLogsState = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 25,
  totalPages: 0,
  isLoading: false,
  error: null,
};

export const loadAuditLogs = createAsyncThunk<
  {
    items: AuditLogEntry[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  },
  AuditLogFilters,
  { rejectValue: string }
>("auditLogs/load", async (filters, { rejectWithValue }) => {
  try {
    return await fetchAuditLogs(filters);
  } catch (error) {
    return rejectWithValue(
      error instanceof Error ? error.message : "Failed to load audit logs.",
    );
  }
});

const auditLogsSlice = createSlice({
  name: "auditLogs",
  initialState,
  reducers: {
    clearAuditLogs: (state) => {
      state.items = [];
      state.totalCount = 0;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(loadAuditLogs.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(loadAuditLogs.fulfilled, (state, action) => {
        state.isLoading = false;
        state.items = action.payload.items;
        state.totalCount = action.payload.totalCount;
        state.page = action.payload.page;
        state.pageSize = action.payload.pageSize;
        state.totalPages = action.payload.totalPages;
      })
      .addCase(loadAuditLogs.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload ?? "Failed to load audit logs.";
      });
  },
});

export const { clearAuditLogs } = auditLogsSlice.actions;
export default auditLogsSlice.reducer;
