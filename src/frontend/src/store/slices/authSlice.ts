/**
 * Authentication Redux slice (FR-001 - User Registration, FR-002 - User Authentication)
 * Manages registration and login state and API calls
 */

import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type {
  RegistrationRequest,
  RegistrationResponse,
  LoginRequest,
  LoginResponse,
  ApiError,
} from '../../types/auth.types';
import { setToken, setUser, clearAuth, setLoginTime } from '../../utils/tokenStorage';

// API base URL from environment or default to development
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

/**
 * Register new user async thunk
 * Calls POST /api/auth/register endpoint
 */
export const registerUser = createAsyncThunk<
  RegistrationResponse,
  RegistrationRequest,
  {
    rejectValue: string;
  }
>('auth/register', async (registrationData, { rejectWithValue }) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(registrationData),
    });

    if (!response.ok) {
      // Handle HTTP errors
      if (response.status === 429) {
        return rejectWithValue(
          'Too many registration attempts. Please try again in 5 minutes.'
        );
      }

      if (response.status === 409) {
        return rejectWithValue(
          'An account with this email already exists or is pending verification.'
        );
      }

      // Parse ProblemDetails response
      const error: ApiError = await response.json();
      return rejectWithValue(error.detail || error.title || 'Registration failed');
    }

    const data: RegistrationResponse = await response.json();
    return data;
  } catch (error) {
    // Network or other errors
    if (error instanceof Error) {
      return rejectWithValue(error.message);
    }
    return rejectWithValue('An unexpected error occurred during registration');
  }
});

/**
 * Verify email async thunk
 * Calls GET /api/auth/verify-email?token={token}
 */
export const verifyEmail = createAsyncThunk<
  void,
  string,
  {
    rejectValue: string;
  }
>('auth/verifyEmail', async (token, { rejectWithValue }) => {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/auth/verify-email?token=${encodeURIComponent(token)}`,
      {
        method: 'GET',
      }
    );

    if (!response.ok) {
      const error: ApiError = await response.json();
      return rejectWithValue(
        error.detail || 'Email verification failed. The link may be expired or invalid.'
      );
    }
  } catch (error) {
    if (error instanceof Error) {
      return rejectWithValue(error.message);
    }
    return rejectWithValue('An unexpected error occurred during email verification');
  }
});

/**
 * Login user async thunk (FR-002)
 * Calls POST /api/auth/login endpoint
 */
export const loginUser = createAsyncThunk<
  LoginResponse,
  LoginRequest,
  {
    rejectValue: string;
  }
>('auth/login', async (loginData, { rejectWithValue }) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(loginData),
    });

    if (!response.ok) {
      // Handle HTTP errors
      if (response.status === 401) {
        return rejectWithValue(
          'Invalid email or password. Please check your credentials and try again.'
        );
      }

      if (response.status === 403) {
        return rejectWithValue(
          'Your account is locked or inactive. Please contact support for assistance.'
        );
      }

      // Parse ProblemDetails response
      const error: ApiError = await response.json();
      return rejectWithValue(error.detail || error.title || 'Login failed');
    }

    const data: LoginResponse = await response.json();
    
    // Store token and user data in sessionStorage
    setToken(data.token);
    setUser(data.userId, data.name, data.role);
    
    // Store login time for session timeout tracking (UXR-604)
    setLoginTime(Date.now());
    
    return data;
  } catch (error) {
    // Network or other errors
    if (error instanceof Error) {
      return rejectWithValue(error.message);
    }
    return rejectWithValue('An unexpected error occurred during login');
  }
});

/**
 * Refresh session async thunk (UXR-604)
 * Calls POST /api/auth/refresh-session endpoint to extend session TTL
 * Returns void on success, used internally by SessionTimeoutModal
 */
export const refreshSession = createAsyncThunk<
  void,
  void,
  {
    rejectValue: string;
  }
>('auth/refreshSession', async (_, { rejectWithValue }) => {
  try {
    const token = sessionStorage.getItem('auth_token');

    if (!token) {
      return rejectWithValue('No authentication token found');
    }

    const response = await fetch(`${API_BASE_URL}/api/auth/refresh-session`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      if (response.status === 404) {
        return rejectWithValue('Session not found. Please log in again.');
      }

      if (response.status === 401) {
        return rejectWithValue('Invalid authentication token');
      }

      const error: ApiError = await response.json();
      return rejectWithValue(error.detail || 'Failed to refresh session');
    }

    // Update login time to reset session age
    setLoginTime(Date.now());

    // Sync across tabs via localStorage event
    localStorage.setItem('session_extended', Date.now().toString());
  } catch (error) {
    if (error instanceof Error) {
      return rejectWithValue(error.message);
    }
    return rejectWithValue('An unexpected error occurred while refreshing session');
  }
});

// Initial state - combined registration and login states
const initialState = {
  // Registration state
  registration: {
    isLoading: false,
    isSuccess: false,
    error: null as string | null,
    userId: null as string | null,
    registeredEmail: null as string | null,
  },
  // Login state
  login: {
    isLoading: false,
    isAuthenticated: false,
    error: null as string | null,
    token: null as string | null,
    role: null as string | null,
    userId: null as string | null,
    name: null as string | null,
  },
};

// Slice definition
const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    // Clear registration state (for resetting form after navigation)
    clearRegistrationState: (state) => {
      state.registration.isLoading = false;
      state.registration.isSuccess = false;
      state.registration.error = null;
      state.registration.userId = null;
      state.registration.registeredEmail = null;
    },
    // Clear login error only
    clearLoginError: (state) => {
      state.login.error = null;
    },
    // Clear registration error only
    clearRegistrationError: (state) => {
      state.registration.error = null;
    },
    // Logout user
    logout: (state) => {
      clearAuth();
      state.login.isAuthenticated = false;
      state.login.token = null;
      state.login.role = null;
      state.login.userId = null;
      state.login.name = null;
      state.login.error = null;
    },
  },
  extraReducers: (builder) => {
    // registerUser pending
    builder.addCase(registerUser.pending, (state) => {
      state.registration.isLoading = true;
      state.registration.error = null;
      state.registration.isSuccess = false;
    });

    // registerUser fulfilled
    builder.addCase(registerUser.fulfilled, (state, action: PayloadAction<RegistrationResponse>) => {
      state.registration.isLoading = false;
      state.registration.isSuccess = true;
      state.registration.userId = action.payload.userId;
      state.registration.registeredEmail = action.payload.email;
      state.registration.error = null;
    });

    // registerUser rejected
    builder.addCase(registerUser.rejected, (state, action) => {
      state.registration.isLoading = false;
      state.registration.isSuccess = false;
      state.registration.error = action.payload || 'Registration failed';
    });

    // verifyEmail pending
    builder.addCase(verifyEmail.pending, (state) => {
      state.registration.isLoading = true;
      state.registration.error = null;
    });

    // verifyEmail fulfilled
    builder.addCase(verifyEmail.fulfilled, (state) => {
      state.registration.isLoading = false;
      state.registration.error = null;
    });

    // verifyEmail rejected
    builder.addCase(verifyEmail.rejected, (state, action) => {
      state.registration.isLoading = false;
      state.registration.error = action.payload || 'Email verification failed';
    });

    // loginUser pending
    builder.addCase(loginUser.pending, (state) => {
      state.login.isLoading = true;
      state.login.error = null;
    });

    // loginUser fulfilled
    builder.addCase(loginUser.fulfilled, (state, action: PayloadAction<LoginResponse>) => {
      state.login.isLoading = false;
      state.login.isAuthenticated = true;
      state.login.token = action.payload.token;
      state.login.role = action.payload.role;
      state.login.userId = action.payload.userId;
      state.login.name = action.payload.name;
      state.login.error = null;
    });

    // loginUser rejected
    builder.addCase(loginUser.rejected, (state, action) => {
      state.login.isLoading = false;
      state.login.isAuthenticated = false;
      state.login.error = action.payload || 'Login failed';
    });
  },
});

export const { clearRegistrationState, clearLoginError, clearRegistrationError, logout } = authSlice.actions;
export default authSlice.reducer;
