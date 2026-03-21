import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import axios from 'axios';
import type { User, CreateUserRequest, UpdateUserRequest } from '../../types/user.types';
import type { UserManagementState } from '../../types/user.types';
import { getToken } from '../../utils/tokenStorage';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5268/api';

/**
 * Fetches all users with optional filtering and sorting (US_021 AC4).
 */
export const fetchUsers = createAsyncThunk<User[], { search?: string; sortBy?: string; ascending?: boolean }>(
  'userManagement/fetchUsers',
  async ({ search, sortBy, ascending = true }, { rejectWithValue }) => {
    try {
      const token = getToken();
      const params = new URLSearchParams();
      if (search) params.append('search', search);
      if (sortBy) params.append('sortBy', sortBy);
      params.append('ascending', String(ascending));

      const response = await axios.get<User[]>(`${API_BASE_URL}/users?${params.toString()}`, {
        headers: { Authorization: `Bearer ${token}` }
      });

      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch users');
    }
  }
);

/**
 * Fetches count of active Admin users (US_021 Edge Case validation).
 */
export const fetchActiveAdminCount = createAsyncThunk<number>(
  'userManagement/fetchActiveAdminCount',
  async (_, { rejectWithValue }) => {
    try {
      const token = getToken();
      const response = await axios.get<number>(`${API_BASE_URL}/users/admin-count`, {
        headers: { Authorization: `Bearer ${token}` }
      });

      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch admin count');
    }
  }
);

/**
 * Creates a new Staff or Admin user (US_021 AC1).
 */
export const createUser = createAsyncThunk<User, CreateUserRequest>(
  'userManagement/createUser',
  async (request, { rejectWithValue }) => {
    try {
      const token = getToken();
      const response = await axios.post<User>(`${API_BASE_URL}/users`, request, {
        headers: { Authorization: `Bearer ${token}` }
      });

      return response.data;
    } catch (error: any) {
      if (error.response?.status === 409) {
        return rejectWithValue('Email address is already registered.');
      }
      return rejectWithValue(error.response?.data?.message || 'Failed to create user');
    }
  }
);

/**
 * Updates existing user's name and/or role (US_021 AC2).
 */
export const updateUser = createAsyncThunk<User, { id: string; request: UpdateUserRequest }>(
  'userManagement/updateUser',
  async ({ id, request }, { rejectWithValue }) => {
    try {
      const token = getToken();
      const response = await axios.put<User>(`${API_BASE_URL}/users/${id}`, request, {
        headers: { Authorization: `Bearer ${token}` }
      });

      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update user');
    }
  }
);

/**
 * Deactivates user account (US_021 AC3).
 * Prevents self-deactivation and last Admin deletion.
 */
export const deactivateUser = createAsyncThunk<string, string>(
  'userManagement/deactivateUser',
  async (userId, { rejectWithValue }) => {
    try {
      const token = getToken();
      await axios.delete(`${API_BASE_URL}/users/${userId}`, {
        headers: { Authorization: `Bearer ${token}` }
      });

      return userId;
    } catch (error: any) {
      if (error.response?.status === 400) {
        return rejectWithValue(error.response.data.message);
      }
      return rejectWithValue(error.response?.data?.message || 'Failed to deactivate user');
    }
  }
);

const initialState: UserManagementState = {
  users: [],
  loading: false,
  error: null,
  activeAdminCount: 0
};

const userManagementSlice = createSlice({
  name: 'userManagement',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    }
  },
  extraReducers: (builder) => {
    // Fetch Users
    builder
      .addCase(fetchUsers.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchUsers.fulfilled, (state, action: PayloadAction<User[]>) => {
        state.loading = false;
        state.users = action.payload;
      })
      .addCase(fetchUsers.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });

    // Fetch Active Admin Count
    builder
      .addCase(fetchActiveAdminCount.fulfilled, (state, action: PayloadAction<number>) => {
        state.activeAdminCount = action.payload;
      });

    // Create User
    builder
      .addCase(createUser.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createUser.fulfilled, (state, action: PayloadAction<User>) => {
        state.loading = false;
        state.users.push(action.payload);
      })
      .addCase(createUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });

    // Update User
    builder
      .addCase(updateUser.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateUser.fulfilled, (state, action: PayloadAction<User>) => {
        state.loading = false;
        const index = state.users.findIndex(u => u.userId === action.payload.userId);
        if (index !== -1) {
          state.users[index] = action.payload;
        }
      })
      .addCase(updateUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });

    // Deactivate User
    builder
      .addCase(deactivateUser.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deactivateUser.fulfilled, (state, action: PayloadAction<string>) => {
        state.loading = false;
        state.users = state.users.filter(u => u.userId !== action.payload);
      })
      .addCase(deactivateUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  }
});

export const { clearError } = userManagementSlice.actions;
export default userManagementSlice.reducer;
