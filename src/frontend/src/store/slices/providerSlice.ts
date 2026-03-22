/**
 * Provider slice for US_023 - Provider and Service Browser
 * Manages provider state, filters, pagination, and async data fetching
 */

import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { RootState } from '../index';
import type { Provider, ProviderFilters, PaginationParams, ProviderListResponse } from '../../types/provider';
import { fetchProvidersWithRetry } from '../../api/providerApi';

/**
 * Provider slice state interface
 */
interface ProviderState {
    providers: Provider[];
    filters: ProviderFilters;
    pagination: PaginationParams;
    total: number;
    totalPages: number;
    isLoading: boolean;
    error: string | null;
    lastFetchTimestamp: number;
}

/**
 * Initial state for provider slice
 */
const initialState: ProviderState = {
    providers: [],
    filters: {
        search: '',
        specialty: 'all',
        availability: 'any-time',
        gender: 'any',
        serviceType: '',
    },
    pagination: {
        page: 1,
        pageSize: 20,
    },
    total: 0,
    totalPages: 0,
    isLoading: false,
    error: null,
    lastFetchTimestamp: 0,
};

/**
 * Async thunk for fetching providers (FR-006, AC1, AC2, AC3)
 * Implements retry logic and 300ms performance target
 */
export const fetchProviders = createAsyncThunk<
    ProviderListResponse,
    void,
    { state: RootState; rejectValue: string }
>(
    'providers/fetchProviders',
    async (_, { getState, rejectWithValue }) => {
        const { providers: providerState } = getState();
        const { filters, pagination } = providerState;

        try {
            const response = await fetchProvidersWithRetry(filters, pagination);
            return response;
        } catch (error) {
            return rejectWithValue(
                error instanceof Error ? error.message : 'Failed to fetch providers'
            );
        }
    }
);

/**
 * Provider slice with reducers and actions
 */
const providerSlice = createSlice({
    name: 'providers',
    initialState,
    reducers: {
        /**
         * Update search filter (FR-006, AC3)
         * Reset to page 1 when search changes
         */
        setSearchFilter: (state, action: PayloadAction<string>) => {
            state.filters.search = action.payload;
            state.pagination.page = 1;
        },

        /**
         * Update specialty filter (FR-006, AC2)
         * Reset to page 1 when filter changes
         */
        setSpecialtyFilter: (state, action: PayloadAction<string>) => {
            state.filters.specialty = action.payload;
            state.pagination.page = 1;
        },

        /**
         * Update availability filter (FR-006, AC2)
         * Reset to page 1 when filter changes
         */
        setAvailabilityFilter: (state, action: PayloadAction<string>) => {
            state.filters.availability = action.payload;
            state.pagination.page = 1;
        },

        /**
         * Update gender filter (FR-006, AC2)
         * Reset to page 1 when filter changes
         */
        setGenderFilter: (state, action: PayloadAction<string>) => {
            state.filters.gender = action.payload;
            state.pagination.page = 1;
        },

        /**
         * Update service type filter (FR-006, AC2)
         * Reset to page 1 when filter changes
         */
        setServiceTypeFilter: (state, action: PayloadAction<string>) => {
            state.filters.serviceType = action.payload;
            state.pagination.page = 1;
        },

        /**
         * Clear all filters (FR-006, AC4)
         * Reset to initial filter state and page 1
         */
        clearAllFilters: (state) => {
            state.filters = {
                search: '',
                specialty: 'all',
                availability: 'any-time',
                gender: 'any',
                serviceType: '',
            };
            state.pagination.page = 1;
        },

        /**
         * Set page number for pagination (Edge Case: 100+ providers)
         * Implements 20 providers per page
         */
        setPage: (state, action: PayloadAction<number>) => {
            state.pagination.page = action.payload;
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
            // Fetch providers - pending
            .addCase(fetchProviders.pending, (state) => {
                state.isLoading = true;
                state.error = null;
            })
            // Fetch providers - fulfilled
            .addCase(fetchProviders.fulfilled, (state, action) => {
                state.isLoading = false;
                state.providers = action.payload.providers;
                state.total = action.payload.total;
                state.totalPages = action.payload.totalPages;
                state.pagination.page = action.payload.page;
                state.pagination.pageSize = action.payload.pageSize;
                state.lastFetchTimestamp = Date.now();
            })
            // Fetch providers - rejected
            .addCase(fetchProviders.rejected, (state, action) => {
                state.isLoading = false;
                state.error = action.payload || 'Failed to load providers';
                state.providers = [];
            });
    },
});

/**
 * Export actions
 */
export const {
    setSearchFilter,
    setSpecialtyFilter,
    setAvailabilityFilter,
    setGenderFilter,
    setServiceTypeFilter,
    clearAllFilters,
    setPage,
    clearError,
} = providerSlice.actions;

/**
 * Selectors for provider state
 */
export const selectProviders = (state: RootState) => state.providers.providers;
export const selectFilters = (state: RootState) => state.providers.filters;
export const selectPagination = (state: RootState) => state.providers.pagination;
export const selectTotal = (state: RootState) => state.providers.total;
export const selectTotalPages = (state: RootState) => state.providers.totalPages;
export const selectIsLoading = (state: RootState) => state.providers.isLoading;
export const selectError = (state: RootState) => state.providers.error;

/**
 * Export reducer
 */
export default providerSlice.reducer;
