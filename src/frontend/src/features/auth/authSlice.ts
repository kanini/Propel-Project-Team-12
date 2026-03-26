import {
  createSlice,
  createAsyncThunk,
  type PayloadAction,
} from "@reduxjs/toolkit";
import type { RootState } from "../../store";

/**
 * Registration request payload (matches backend RegisterUserRequestDto).
 */
export interface RegisterRequest {
  name: string;
  email: string;
  dateOfBirth: string; // ISO date string (YYYY-MM-DD)
  phone: string;
  password: string;
}

/**
 * Registration response payload (matches backend RegisterUserResponseDto).
 */
export interface RegisterResponse {
  userId: string;
  email: string;
  status: string;
  message: string;
}

/**
 * Login request payload (matches backend LoginRequestDto).
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Login response payload (matches backend LoginResponseDto).
 */
export interface LoginResponse {
  token: string;
  userId: string;
  email: string;
  name: string;
  role: string;
  expiresAt: string;
  message: string;
}

/**
 * User state for authenticated user.
 */
export interface User {
  userId: string;
  email: string;
  name: string;
  role: string;
}

/**
 * Auth state interface.
 */
interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isInitializing: boolean; // Track initial session restoration
  error: string | null;
  registrationSuccess: boolean;
}

const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: false,
  isInitializing: true, // Start as true until we check localStorage
  error: null,
  registrationSuccess: false,
};

/**
 * Async thunk for user registration (FR-001, AC1).
 * Calls POST /api/auth/register endpoint.
 */
export const registerUser = createAsyncThunk<
  RegisterResponse,
  RegisterRequest,
  { rejectValue: string }
>("auth/register", async (registerData, { rejectWithValue }) => {
  try {
    const apiBaseUrl =
      import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

    const response = await fetch(`${apiBaseUrl}/api/auth/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(registerData),
    });

    const data = await response.json();

    if (!response.ok) {
      // Extract error message from response
      const errorMessage = data.message || data.error || "Registration failed";
      return rejectWithValue(errorMessage);
    }

    return data as RegisterResponse;
  } catch (error) {
    return rejectWithValue(
      error instanceof Error
        ? error.message
        : "Network error. Please try again.",
    );
  }
});

/**
 * Async thunk for user login (FR-002, AC1).
 * Calls POST /api/auth/login endpoint.
 */
export const loginUser = createAsyncThunk<
  LoginResponse,
  LoginRequest,
  { rejectValue: string }
>("auth/login", async (loginData, { rejectWithValue }) => {
  try {
    const apiBaseUrl =
      import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

    const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(loginData),
    });

    const data = await response.json();

    if (!response.ok) {
      const errorMessage = data.message || data.error || "Login failed";
      return rejectWithValue(errorMessage);
    }

    // Store token and user data in localStorage for persistence (AC1)
    localStorage.setItem("token", data.token);
    localStorage.setItem("userId", data.userId);
    localStorage.setItem("userEmail", data.email);
    localStorage.setItem("userName", data.name);
    localStorage.setItem("userRole", data.role);

    return data as LoginResponse;
  } catch (error) {
    return rejectWithValue(
      error instanceof Error
        ? error.message
        : "Network error. Please try again.",
    );
  }
});

/**
 * Async thunk for refreshing session (US_022, AC5).
 * Currently client-side only - updates activity timestamp.
 * Future enhancement: Call backend refresh endpoint to get new token.
 */
export const refreshSession = createAsyncThunk<
  void,
  void,
  { rejectValue: string }
>("auth/refreshSession", async (_) => {
  try {
    const apiBaseUrl =
      import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";
    const token = localStorage.getItem("token");

    if (!token) {
      return;
    }

    const response = await fetch(`${apiBaseUrl}/api/auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      // Non-blocking: continue with client-side extension if backend fails
      return;
    }

    return;
  } catch {
    // Non-blocking: session extension continues client-side
    return;
  }
});

/**
 * Auth slice with registration and login reducers.
 */
const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    // Clear error message
    clearError: (state) => {
      state.error = null;
    },
    // Reset registration success flag
    resetRegistrationSuccess: (state) => {
      state.registrationSuccess = false;
    },
    // Logout action
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;
      // Clear all user data from localStorage
      localStorage.removeItem("token");
      localStorage.removeItem("userId");
      localStorage.removeItem("userEmail");
      localStorage.removeItem("userName");
      localStorage.removeItem("userRole");
    },
    // Restore session from localStorage (on app startup)
    restoreSession: (
      state,
      action: PayloadAction<{ token: string; user: User }>,
    ) => {
      state.token = action.payload.token;
      state.user = action.payload.user;
      state.isAuthenticated = true;
      state.isInitializing = false;
    },
    // Complete initialization without restoring session
    completeInitialization: (state) => {
      state.isInitializing = false;
    },
  },
  extraReducers: (builder) => {
    // Register user reducers
    builder
      .addCase(registerUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
        state.registrationSuccess = false;
      })
      .addCase(registerUser.fulfilled, (state) => {
        state.isLoading = false;
        state.registrationSuccess = true;
        state.error = null;
      })
      .addCase(registerUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error =
          action.payload || "Registration failed. Please try again.";
        state.registrationSuccess = false;
      });

    // Login user reducers
    builder
      .addCase(loginUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(loginUser.fulfilled, (state, action) => {
        state.isLoading = false;
        state.isAuthenticated = true;
        state.token = action.payload.token;
        state.user = {
          userId: action.payload.userId,
          email: action.payload.email,
          name: action.payload.name,
          role: action.payload.role,
        };
        state.error = null;
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload || "Login failed. Please try again.";
      });

    // Refresh session reducers
    builder
      .addCase(refreshSession.pending, (state) => {
        // Don't set isLoading to avoid blocking UI during refresh
        state.error = null;
      })
      .addCase(refreshSession.fulfilled, (state) => {
        // Session refreshed successfully - maintain current state
        state.error = null;
      })
      .addCase(refreshSession.rejected, (state, action) => {
        // Refresh failed - log out user for security
        state.user = null;
        state.token = null;
        state.isAuthenticated = false;
        state.error = action.payload || "Session refresh failed.";
        // Clear all user data from localStorage
        localStorage.removeItem("token");
        localStorage.removeItem("userId");
        localStorage.removeItem("userEmail");
        localStorage.removeItem("userName");
        localStorage.removeItem("userRole");
      });
  },
});

export const {
  clearError,
  resetRegistrationSuccess,
  logout,
  restoreSession,
  completeInitialization,
} = authSlice.actions;

// Selectors
export const selectAuth = (state: RootState) => state.auth;
export const selectIsAuthenticated = (state: RootState) =>
  state.auth.isAuthenticated;
export const selectIsInitializing = (state: RootState) =>
  state.auth.isInitializing;
export const selectUser = (state: RootState) => state.auth.user;
export const selectAuthLoading = (state: RootState) => state.auth.isLoading;
export const selectAuthError = (state: RootState) => state.auth.error;
export const selectRegistrationSuccess = (state: RootState) =>
  state.auth.registrationSuccess;

export default authSlice.reducer;
