# Task: Frontend - Redux State Management + API Client

## Task Metadata
- **Task ID:** task_002_fe_redux_state_api  
- **Parent Story:** [us_068](us_068.md) - Staff Dashboard
- **Epic:** EP-003  
- **Technology Layer:** Frontend (React 18 + TypeScript + Redux Toolkit)  
- **Estimated Effort:** 2 hours  
- **Priority:** P0  
- **Status:** COMPLETED  

---

## Objective
Set up Redux state management for staff dashboard and extend API client with dashboard endpoints.

---

## Implementation Checklist
- [x] Create `src/store/staffDashboardSlice.ts` with state interface
- [x] Implement `fetchMetrics` async thunk
- [x] Implement `fetchQueuePreview` async thunk
- [x] Implement `loadDashboardData` thunk (parallel fetch)
- [x] Add `updateQueueRealtime` sync action for Pusher updates
- [x] Register slice in `rootReducer.ts`
- [x] Extend `src/api/staffApi.ts` with `getDashboardMetrics()` function
- [x] Extend `src/api/staffApi.ts` with `getQueuePreview()` function

---

## Technical Details

### Redux Slice Implementation
```typescript
// src/store/staffDashboardSlice.ts
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { RootState } from './index';
import { getDashboardMetrics, getQueuePreview } from '../api/staffApi';

export interface DashboardMetricsDto {
  todayAppointments: number;
  currentQueueSize: number;
  pendingVerifications: number;
}

export interface QueuePreviewDto {
  appointmentId: string;
  patientName: string;
  providerName: string;
  appointmentTime: string;
  estimatedWait: string;
  riskLevel: 'low' | 'medium' | 'high';
  status: string;
}

interface StaffDashboardState {
  metrics: DashboardMetricsDto | null;
  queuePreview: QueuePreviewDto[];
  isLoading: boolean;
  error: string | null;
  lastUpdated: string | null;
}

const initialState: StaffDashboardState = {
  metrics: null,
  queuePreview: [],
  isLoading: false,
  error: null,
  lastUpdated: null,
};

export const fetchMetrics = createAsyncThunk(
  'staffDashboard/fetchMetrics',
  async (_, { rejectWithValue }) => {
    try {
      return await getDashboardMetrics();
    } catch (error: any) {
      return rejectWithValue(error.message);
    }
  }
);

export const fetchQueuePreview = createAsyncThunk(
  'staffDashboard/fetchQueuePreview',
  async (count: number = 5, { rejectWithValue }) => {
    try {
      return await getQueuePreview(count);
    } catch (error: any) {
      return rejectWithValue(error.message);
    }
  }
);

export const loadDashboardData = createAsyncThunk(
  'staffDashboard/loadDashboardData',
  async (_, { dispatch }) => {
    const [metrics, queue] = await Promise.all([
      dispatch(fetchMetrics()).unwrap(),
      dispatch(fetchQueuePreview(5)).unwrap(),
    ]);
    return { metrics, queue };
  }
);

const staffDashboardSlice = createSlice({
  name: 'staffDashboard',
  initialState,
  reducers: {
    updateQueueRealtime: (state, action) => {
      state.queuePreview = action.payload;
      state.lastUpdated = new Date().toISOString();
    },
  },
  extraReducers: (builder) => {
    builder
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
      .addCase(fetchQueuePreview.fulfilled, (state, action) => {
        state.queuePreview = action.payload;
      });
  },
});

export const { updateQueueRealtime } = staffDashboardSlice.actions;
export default staffDashboardSlice.reducer;
```

### API Client Extension
```typescript
// src/api/staffApi.ts - ADD these functions
export async function getDashboardMetrics(): Promise<DashboardMetricsDto> {
  const url = `${API_BASE_URL}/api/staff/dashboard/metrics`;
  const response = await fetch(url, {
    method: 'GET',
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    if (response.status === 401) throw new Error('Unauthorized');
    if (response.status === 403) throw new Error('Access denied');
    throw new Error('Failed to fetch metrics');
  }

  return await response.json();
}

export async function getQueuePreview(count: number = 5): Promise<QueuePreviewDto[]> {
  const url = `${API_BASE_URL}/api/staff/dashboard/queue-preview?count=${count}`;
  const response = await fetch(url, {
    method: 'GET',
    headers: getAuthHeaders(),
  });

  if (!response.ok) throw new Error('Failed to fetch queue');
  return await response.json();
}
```

---

## Design References
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Path** | N/A |

---

## Validation
- Dispatch actions in Redux DevTools
- Verify state updates correctly
- Test API functions in browser console

---

## Traceability
- **Parent Story:** US_068
- **Requirements:** NFR-001 (500ms response), UXR-502 (loading states)

---

**Status:** Ready  
**Next:** task_003_fe_stat_cards_actions (Dashboard components)
