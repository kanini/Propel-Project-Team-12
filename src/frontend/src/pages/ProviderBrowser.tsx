/**
 * ProviderBrowser Page for US_023 - Provider and Service Browser
 * Main page component that orchestrates provider browsing with search, filters, and pagination (FR-006)
 * Implements all required states: Default, Loading, Empty, Error
 */

import { useEffect, useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch } from '../store';
import {
    fetchProviders,
    setSearchFilter,
    setSpecialtyFilter,
    setAvailabilityFilter,
    setGenderFilter,
    clearAllFilters,
    setPage,
    clearError,
    selectProviders,
    selectFilters,
    selectPagination,
    selectTotal,
    selectTotalPages,
    selectIsLoading,
    selectError,
} from '../store/slices/providerSlice';
import { resetBooking } from '../store/slices/appointmentSlice';
import { ProviderSearch } from '../components/providers/ProviderSearch';
import { ProviderFilters } from '../components/providers/ProviderFilters';
import { ProviderCard } from '../components/providers/ProviderCard';
import { SkeletonLoader } from '../components/common/SkeletonLoader';
import { EmptyState } from '../components/common/EmptyState';
import { Pagination } from '../components/common/Pagination';

/**
 * ProviderBrowser page component (FR-006, AC1, AC2, AC3, AC4)
 */
export default function ProviderBrowser() {
    const dispatch = useDispatch<AppDispatch>();

    // Selectors
    const providers = useSelector(selectProviders);
    const filters = useSelector(selectFilters);
    const pagination = useSelector(selectPagination);
    const total = useSelector(selectTotal);
    const totalPages = useSelector(selectTotalPages);
    const isLoading = useSelector(selectIsLoading);
    const error = useSelector(selectError);

    // Reset booking state and filters when returning to provider browser  
    // This ensures users get a fresh start for both booking and searching
    useEffect(() => {
        dispatch(resetBooking());
        dispatch(clearAllFilters());
    }, [dispatch]);

    // Fetch providers on mount and when filters/pagination change
    useEffect(() => {
        dispatch(fetchProviders());
    }, [dispatch, filters, pagination.page]);

    // Scroll to top when page changes
    useEffect(() => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }, [pagination.page]);

    // Handler for search filter
    const handleSearchChange = useCallback((value: string) => {
        dispatch(setSearchFilter(value));
    }, [dispatch]);

    // Handler for specialty filter
    const handleSpecialtyChange = (value: string) => {
        dispatch(setSpecialtyFilter(value));
    };

    // Handler for availability filter
    const handleAvailabilityChange = (value: string) => {
        dispatch(setAvailabilityFilter(value));
    };

    // Handler for gender filter
    const handleGenderChange = (value: string) => {
        dispatch(setGenderFilter(value));
    };

    // Handler for clear all filters (FR-006, AC4)
    const handleClearAllFilters = () => {
        dispatch(clearAllFilters());
    };

    // Handler for page change (Edge Case: 100+ providers)
    const handlePageChange = (page: number) => {
        dispatch(setPage(page));
    };

    // Handler for error dismissal
    const handleDismissError = () => {
        dispatch(clearError());
    };

    return (
        <div className="min-h-screen bg-neutral-50">
            {/* Page header */}
            <div className="bg-neutral-0 border-b border-neutral-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                    <h1 className="text-3xl font-bold text-neutral-900">Find a provider</h1>
                    <p className="mt-2 text-sm text-neutral-600">
                        Search for healthcare providers by name, specialty, or service
                    </p>
                </div>
            </div>

            {/* Main content */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                {/* Search bar */}
                <div className="mb-6">
                    <ProviderSearch
                        value={filters.search || ''}
                        onChange={handleSearchChange}
                    />
                </div>

                {/* Content with filters */}
                <div className="grid grid-cols-1 lg:grid-cols-[240px_1fr] gap-6">
                    {/* Filter panel (hidden on mobile) */}
                    <aside className="hidden lg:block">
                        <ProviderFilters
                            specialty={filters.specialty || 'all'}
                            availability={filters.availability || 'any-time'}
                            gender={filters.gender || 'any'}
                            onSpecialtyChange={handleSpecialtyChange}
                            onAvailabilityChange={handleAvailabilityChange}
                            onGenderChange={handleGenderChange}
                            onClearAll={handleClearAllFilters}
                        />
                    </aside>

                    {/* Provider grid and pagination */}
                    <main>
                        {/* Error state */}
                        {error && (
                            <div
                                className="mb-6 bg-error-light border border-error-default rounded-lg p-4 flex items-start gap-3"
                                role="alert"
                            >
                                <svg
                                    className="w-5 h-5 text-error-default flex-shrink-0 mt-0.5"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                    stroke="currentColor"
                                    aria-hidden="true"
                                >
                                    <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth={2}
                                        d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                                    />
                                </svg>
                                <div className="flex-1">
                                    <h3 className="text-sm font-medium text-error-dark">Error loading providers</h3>
                                    <p className="text-sm text-error-dark mt-1">{error}</p>
                                </div>
                                <button
                                    onClick={handleDismissError}
                                    className="flex-shrink-0 text-error-default hover:text-error-dark 
                             focus:outline-none focus:ring-2 focus:ring-error-default rounded"
                                    aria-label="Dismiss error"
                                    type="button"
                                >
                                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path
                                            strokeLinecap="round"
                                            strokeLinejoin="round"
                                            strokeWidth={2}
                                            d="M6 18L18 6M6 6l12 12"
                                        />
                                    </svg>
                                </button>
                            </div>
                        )}

                        {/* Loading state with skeleton (UXR-502) */}
                        {isLoading && <SkeletonLoader count={6} delay={300} />}

                        {/* Empty state (FR-006, AC4) */}
                        {!isLoading && !error && providers.length === 0 && (
                            <EmptyState
                                title="No providers found"
                                message="No providers match your current search and filter criteria. Try adjusting your filters or search terms."
                                showClearButton
                                onClearFilters={handleClearAllFilters}
                            />
                        )}

                        {/* Provider grid (FR-006, AC1) */}
                        {!isLoading && !error && providers.length > 0 && (
                            <>
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
                                    {providers.map((provider) => (
                                        <ProviderCard key={provider.id} provider={provider} />
                                    ))}
                                </div>

                                {/* Pagination (Edge Case: 100+ providers, 20 per page) */}
                                <Pagination
                                    currentPage={pagination.page}
                                    totalPages={totalPages}
                                    totalItems={total}
                                    pageSize={pagination.pageSize}
                                    onPageChange={handlePageChange}
                                />
                            </>
                        )}
                    </main>
                </div>
            </div>
        </div>
    );
}
