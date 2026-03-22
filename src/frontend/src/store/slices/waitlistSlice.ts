/**
 * Waitlist slice for US_025 - Waitlist Enrollment
 * Manages waitlist enrollment state and async operations
 */

import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { RootState } from '../index';
import type {
    WaitlistEntry,
    JoinWaitlistRequest,
    UpdateWaitlistRequest,
    WaitlistError,
} from '../../types/waitlist';

/**
 * API base URL from environment variables with fallback
 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Waitlist slice state interface
 */
interface WaitlistState {
    // Waitlist entries
    entries: WaitlistEntry[];
    selectedEntry: WaitlistEntry | null;

    // Loading states
    isLoading: boolean;
    isJoining: boolean;
    isUpdating: boolean;
    isDeleting: boolean;

    // Error state
    error: WaitlistError | null;

    // Modal state
    isEnrollmentModalOpen: boolean;
    preSelectedProviderId: string | null;
    preSelectedProviderName: string | null;
    preSelectedProviderSpecialty: string | null;
}

/**
 * Initial state for waitlist slice
 */
const initialState: WaitlistState = {
    entries: [],
    selectedEntry: null,
    isLoading: false,
    isJoining: false,
    isUpdating: false,
    isDeleting: false,
    error: null,
    isEnrollmentModalOpen: false,
    preSelectedProviderId: null,
    preSelectedProviderName: null,
    preSelectedProviderSpecialty: null,
};

/**
 * Get authorization token from Redux state
 */
function getAuthToken(getState: () => unknown): string | null {
    const state = getState() as RootState;
    return state.auth?.token || null;
}

/**
 * Async thunk for fetching patient's waitlist entries (AC-4)
 */
export const fetchWaitlist = createAsyncThunk<
    WaitlistEntry[],
    void,
    { state: RootState; rejectValue: WaitlistError }
>(
    'waitlist/fetchWaitlist',
    async (_, { rejectWithValue, getState }) => {
        try {
            const token = getAuthToken(getState);
            const headers: HeadersInit = {
                'Content-Type': 'application/json',
            };

            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }

            const response = await fetch(`${API_BASE_URL}/api/waitlist`, {
                method: 'GET',
                headers,
            });

            if (!response.ok) {
                if (response.status === 401) {
                    return rejectWithValue({
                        code: 'unauthorized',
                        message: 'Please log in to view waitlist',
                    });
                }

                throw new Error('Failed to fetch waitlist');
            }

            const data = await response.json();
            return data;
        } catch (error) {
            return rejectWithValue({
                code: 'server',
                message: error instanceof Error ? error.message : 'Failed to fetch waitlist',
            });
        }
    }
);

/**
 * Async thunk for joining waitlist (AC-1, AC-2, AC-3)
 */
export const joinWaitlist = createAsyncThunk<
    WaitlistEntry,
    JoinWaitlistRequest,
    { state: RootState; rejectValue: WaitlistError }
>(
    'waitlist/joinWaitlist',
    async (request, { rejectWithValue, getState }) => {
        try {
            const token = getAuthToken(getState);
            const headers: HeadersInit = {
                'Content-Type': 'application/json',
            };

            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }

            const response = await fetch(`${API_BASE_URL}/api/waitlist`, {
                method: 'POST',
                headers,
                body: JSON.stringify(request),
            });

            const data = await response.json();

            // Handle 409 Conflict (AC-3: Duplicate enrollment)
            if (response.status === 409) {
                return rejectWithValue({
                    code: 'conflict',
                    message: 'You are already on this waitlist',
                    existingEntry: data, // Backend returns existing entry in response
                });
            }

            // Handle 400 Validation Error
            if (response.status === 400) {
                return rejectWithValue({
                    code: 'validation',
                    message: 'Please check your input',
                    details: data.errors || {},
                });
            }

            // Handle 401 Unauthorized
            if (response.status === 401) {
                return rejectWithValue({
                    code: 'unauthorized',
                    message: 'Please log in to join waitlist',
                });
            }

            if (!response.ok) {
                throw new Error('Failed to join waitlist');
            }

            return data;
        } catch (error) {
            return rejectWithValue({
                code: 'server',
                message: error instanceof Error ? error.message : 'Failed to join waitlist',
            });
        }
    }
);

/**
 * Async thunk for updating waitlist preferences (AC-3)
 */
export const updateWaitlist = createAsyncThunk<
    WaitlistEntry,
    { id: string; request: UpdateWaitlistRequest },
    { state: RootState; rejectValue: WaitlistError }
>(
    'waitlist/updateWaitlist',
    async ({ id, request }, { rejectWithValue, getState }) => {
        try {
            const token = getAuthToken(getState);
            const headers: HeadersInit = {
                'Content-Type': 'application/json',
            };

            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }

            const response = await fetch(`${API_BASE_URL}/api/waitlist/${id}`, {
                method: 'PUT',
                headers,
                body: JSON.stringify(request),
            });

            const data = await response.json();

            // Handle 404 Not Found (entry doesn't exist or unauthorized)
            if (response.status === 404) {
                return rejectWithValue({
                    code: 'unauthorized',
                    message: 'Waitlist entry not found',
                });
            }

            // Handle 400 Validation Error
            if (response.status === 400) {
                return rejectWithValue({
                    code: 'validation',
                    message: 'Please check your input',
                    details: data.errors || {},
                });
            }

            // Handle 401 Unauthorized
            if (response.status === 401) {
                return rejectWithValue({
                    code: 'unauthorized',
                    message: 'Please log in to update waitlist',
                });
            }

            if (!response.ok) {
                throw new Error('Failed to update waitlist');
            }

            return data;
        } catch (error) {
            return rejectWithValue({
                code: 'server',
                message: error instanceof Error ? error.message : 'Failed to update waitlist',
            });
        }
    }
);

/**
 * Async thunk for leaving waitlist (delete entry)
 */
export const leaveWaitlist = createAsyncThunk<
    string, // Returns the deleted entry ID
    string,
    { state: RootState; rejectValue: WaitlistError }
>(
    'waitlist/leaveWaitlist',
    async (id, { rejectWithValue, getState }) => {
        try {
            const token = getAuthToken(getState);
            const headers: HeadersInit = {
                'Content-Type': 'application/json',
            };

            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }

            const response = await fetch(`${API_BASE_URL}/api/waitlist/${id}`, {
                method: 'DELETE',
                headers,
            });

            // Handle 404 Not Found
            if (response.status === 404) {
                return rejectWithValue({
                    code: 'unauthorized',
                    message: 'Waitlist entry not found',
                });
            }

            // Handle 401 Unauthorized
            if (response.status === 401) {
                return rejectWithValue({
                    code: 'unauthorized',
                    message: 'Please log in to leave waitlist',
                });
            }

            if (!response.ok) {
                throw new Error('Failed to leave waitlist');
            }

            return id;
        } catch (error) {
            return rejectWithValue({
                code: 'server',
                message: error instanceof Error ? error.message : 'Failed to leave waitlist',
            });
        }
    }
);

/**
 * Waitlist slice with reducers and actions
 */
const waitlistSlice = createSlice({
    name: 'waitlist',
    initialState,
    reducers: {
        /**
         * Open enrollment modal with optional pre-selected provider
         */
        openEnrollmentModal: (
            state,
            action: PayloadAction<{
                providerId?: string;
                providerName?: string;
                providerSpecialty?: string;
            }>
        ) => {
            state.isEnrollmentModalOpen = true;
            if (action.payload) {
                state.preSelectedProviderId = action.payload.providerId || null;
                state.preSelectedProviderName = action.payload.providerName || null;
                state.preSelectedProviderSpecialty = action.payload.providerSpecialty || null;
            }
        },

        /**
         * Close enrollment modal and clear pre-selected provider
         */
        closeEnrollmentModal: (state) => {
            state.isEnrollmentModalOpen = false;
            state.preSelectedProviderId = null;
            state.preSelectedProviderName = null;
            state.preSelectedProviderSpecialty = null;
            state.selectedEntry = null;
            state.error = null;
        },

        /**
         * Set selected entry for editing
         */
        setSelectedEntry: (state, action: PayloadAction<WaitlistEntry | null>) => {
            state.selectedEntry = action.payload;
        },

        /**
         * Clear error
         */
        clearError: (state) => {
            state.error = null;
        },
    },
    extraReducers: (builder) => {
        // Fetch waitlist entries
        builder
            .addCase(fetchWaitlist.pending, (state) => {
                state.isLoading = true;
                state.error = null;
            })
            .addCase(fetchWaitlist.fulfilled, (state, action) => {
                state.isLoading = false;
                state.entries = action.payload;
            })
            .addCase(fetchWaitlist.rejected, (state, action) => {
                state.isLoading = false;
                state.error = action.payload || {
                    code: 'server',
                    message: 'Failed to fetch waitlist',
                };
            });

        // Join waitlist
        builder
            .addCase(joinWaitlist.pending, (state) => {
                state.isJoining = true;
                state.error = null;
            })
            .addCase(joinWaitlist.fulfilled, (state, action) => {
                state.isJoining = false;
                state.entries.push(action.payload);
                state.isEnrollmentModalOpen = false;
                state.preSelectedProviderId = null;
                state.preSelectedProviderName = null;
                state.preSelectedProviderSpecialty = null;
            })
            .addCase(joinWaitlist.rejected, (state, action) => {
                state.isJoining = false;
                state.error = action.payload || {
                    code: 'server',
                    message: 'Failed to join waitlist',
                };
            });

        // Update waitlist
        builder
            .addCase(updateWaitlist.pending, (state) => {
                state.isUpdating = true;
                state.error = null;
            })
            .addCase(updateWaitlist.fulfilled, (state, action) => {
                state.isUpdating = false;
                const index = state.entries.findIndex((e) => e.id === action.payload.id);
                if (index !== -1) {
                    state.entries[index] = action.payload;
                }
                state.isEnrollmentModalOpen = false;
                state.selectedEntry = null;
            })
            .addCase(updateWaitlist.rejected, (state, action) => {
                state.isUpdating = false;
                state.error = action.payload || {
                    code: 'server',
                    message: 'Failed to update waitlist',
                };
            });

        // Leave waitlist
        builder
            .addCase(leaveWaitlist.pending, (state) => {
                state.isDeleting = true;
                state.error = null;
            })
            .addCase(leaveWaitlist.fulfilled, (state, action) => {
                state.isDeleting = false;
                state.entries = state.entries.filter((e) => e.id !== action.payload);
            })
            .addCase(leaveWaitlist.rejected, (state, action) => {
                state.isDeleting = false;
                state.error = action.payload || {
                    code: 'server',
                    message: 'Failed to leave waitlist',
                };
            });
    },
});

/**
 * Export actions
 */
export const {
    openEnrollmentModal,
    closeEnrollmentModal,
    setSelectedEntry,
    clearError,
} = waitlistSlice.actions;

/**
 * Export selectors
 */
export const selectWaitlistEntries = (state: RootState) => state.waitlist.entries;
export const selectSelectedEntry = (state: RootState) => state.waitlist.selectedEntry;
export const selectIsLoading = (state: RootState) => state.waitlist.isLoading;
export const selectIsJoining = (state: RootState) => state.waitlist.isJoining;
export const selectIsUpdating = (state: RootState) => state.waitlist.isUpdating;
export const selectIsDeleting = (state: RootState) => state.waitlist.isDeleting;
export const selectError = (state: RootState) => state.waitlist.error;
export const selectIsEnrollmentModalOpen = (state: RootState) => state.waitlist.isEnrollmentModalOpen;
export const selectPreSelectedProvider = (state: RootState) => ({
    id: state.waitlist.preSelectedProviderId,
    name: state.waitlist.preSelectedProviderName,
    specialty: state.waitlist.preSelectedProviderSpecialty,
});

/**
 * Export reducer
 */
export default waitlistSlice.reducer;
