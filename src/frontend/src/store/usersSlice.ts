import {
  createSlice,
  createAsyncThunk,
  type PayloadAction,
} from "@reduxjs/toolkit";
import type { RootState } from "./index";

/**
 * User response DTO (matches backend UserDto).
 */
export interface User {
  userId: string;
  name: string;
  email: string;
  phone: string | null;
  role: "Patient" | "Staff" | "Admin"; // Enum values from backend
  status: "Pending" | "Active" | "Inactive" | "Locked"; // Enum values from backend
  createdAt: string;
  updatedAt: string | null;
}

/**
 * Create user request payload.
 */
export interface CreateUserRequest {
  name: string;
  email: string;
  role: "Patient" | "Staff" | "Admin"; // Must match backend UserRole enum names
  password: string;
  phone?: string;
}

/**
 * Update user request payload.
 */
export interface UpdateUserRequest {
  name?: string;
  email?: string;
  role?: "Patient" | "Staff" | "Admin"; // Must match backend UserRole enum names
  phone?: string;
  status?: "Pending" | "Active" | "Inactive" | "Locked"; // Must match backend UserStatus enum names
}

/**
 * Users state interface.
 */
interface UsersState {
  users: User[];
  isLoading: boolean;
  error: string | null;
  searchTerm: string;
  roleFilter: string | null;
  statusFilter: string | null;
}

const initialState: UsersState = {
  users: [],
  isLoading: false,
  error: null,
  searchTerm: "",
  roleFilter: null,
  statusFilter: null,
};

/**
 * Async thunk to fetch all users (US_021, AC4).
 */
export const fetchUsers = createAsyncThunk(
  "users/fetchAll",
  async (
    params: { searchTerm?: string; role?: string; status?: string } = {},
    { rejectWithValue },
  ) => {
    try {
      const queryParams = new URLSearchParams();
      if (params.searchTerm)
        queryParams.append("searchTerm", params.searchTerm);
      if (params.role) queryParams.append("role", params.role);
      if (params.status) queryParams.append("status", params.status);

      const token = localStorage.getItem("token");
      const queryString = queryParams.toString();
      const url = `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/admin/users${queryString ? `?${queryString}` : ""}`;

      const response = await fetch(url, {
        headers: {
          "Content-Type": "application/json",
          ...(token && { Authorization: `Bearer ${token}` }),
        },
      });

      if (!response.ok) {
        const error = await response.json();
        return rejectWithValue(error.message || "Failed to fetch users");
      }

      const data = await response.json();
      return data as User[];
    } catch (error) {
      return rejectWithValue("Network error");
    }
  },
);

/**
 * Async thunk to create a new user (US_021, AC1).
 */
export const createUser = createAsyncThunk(
  "users/create",
  async (userData: CreateUserRequest, { rejectWithValue }) => {
    try {
      const token = localStorage.getItem("token");
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/admin/users`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
          },
          body: JSON.stringify(userData),
        },
      );

      if (!response.ok) {
        const error = await response.json();
        return rejectWithValue(error.message || "Failed to create user");
      }

      const data = await response.json();
      return data as User;
    } catch (error) {
      return rejectWithValue("Network error");
    }
  },
);

/**
 * Async thunk to update a user (US_021).
 */
export const updateUser = createAsyncThunk(
  "users/update",
  async (
    { userId, userData }: { userId: string; userData: UpdateUserRequest },
    { rejectWithValue },
  ) => {
    try {
      const token = localStorage.getItem("token");
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/admin/users/${userId}`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
          },
          body: JSON.stringify(userData),
        },
      );

      if (!response.ok) {
        const error = await response.json();
        return rejectWithValue(error.message || "Failed to update user");
      }

      const data = await response.json();
      return data as User;
    } catch (error) {
      return rejectWithValue("Network error");
    }
  },
);

/**
 * Async thunk to deactivate a user (US_021, AC5).
 */
export const deactivateUser = createAsyncThunk(
  "users/deactivate",
  async (userId: string, { rejectWithValue }) => {
    try {
      const token = localStorage.getItem("token");
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/admin/users/${userId}`,
        {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        },
      );

      if (!response.ok) {
        const error = await response.json();
        return rejectWithValue(error.message || "Failed to deactivate user");
      }

      return userId;
    } catch (error) {
      return rejectWithValue("Network error");
    }
  },
);

const usersSlice = createSlice({
  name: "users",
  initialState,
  reducers: {
    setSearchTerm: (state, action: PayloadAction<string>) => {
      state.searchTerm = action.payload;
    },
    setRoleFilter: (state, action: PayloadAction<string | null>) => {
      state.roleFilter = action.payload;
    },
    setStatusFilter: (state, action: PayloadAction<string | null>) => {
      state.statusFilter = action.payload;
    },
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch users
    builder.addCase(fetchUsers.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(fetchUsers.fulfilled, (state, action) => {
      state.isLoading = false;
      state.users = action.payload;
    });
    builder.addCase(fetchUsers.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Create user
    builder.addCase(createUser.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(createUser.fulfilled, (state, action) => {
      state.isLoading = false;
      state.users.push(action.payload);
    });
    builder.addCase(createUser.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Update user
    builder.addCase(updateUser.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(updateUser.fulfilled, (state, action) => {
      state.isLoading = false;
      const index = state.users.findIndex(
        (u) => u.userId === action.payload.userId,
      );
      if (index !== -1) {
        state.users[index] = action.payload;
      }
    });
    builder.addCase(updateUser.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });

    // Deactivate user
    builder.addCase(deactivateUser.pending, (state) => {
      state.isLoading = true;
      state.error = null;
    });
    builder.addCase(deactivateUser.fulfilled, (state, action) => {
      state.isLoading = false;
      const index = state.users.findIndex((u) => u.userId === action.payload);
      if (index !== -1) {
        const user = state.users[index];
        if (user) {
          user.status = "Inactive";
        }
      }
    });
    builder.addCase(deactivateUser.rejected, (state, action) => {
      state.isLoading = false;
      state.error = action.payload as string;
    });
  },
});

export const { setSearchTerm, setRoleFilter, setStatusFilter, clearError } =
  usersSlice.actions;
export default usersSlice.reducer;

// Selectors
export const selectUsers = (state: RootState) => state.users.users;
export const selectIsLoading = (state: RootState) => state.users.isLoading;
export const selectError = (state: RootState) => state.users.error;
export const selectSearchTerm = (state: RootState) => state.users.searchTerm;
