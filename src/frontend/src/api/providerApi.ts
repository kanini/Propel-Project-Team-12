/**
 * Provider API client functions for US_023 - Provider and Service Browser
 * Handles all HTTP requests to provider endpoints with error handling and retry logic
 */

import type { ProviderListResponse, ProviderFilters, PaginationParams } from '../types/provider';

/**
 * API base URL from environment variables with fallback
 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Build query string from filters and pagination parameters
 */
function buildQueryString(filters: ProviderFilters, pagination: PaginationParams): string {
    const params = new URLSearchParams();

    // Add pagination params
    params.append('page', pagination.page.toString());
    params.append('pageSize', pagination.pageSize.toString());

    // Add filter params if present
    if (filters.search) {
        params.append('search', filters.search);
    }
    if (filters.specialty && filters.specialty !== 'all') {
        params.append('specialty', filters.specialty);
    }
    if (filters.availability && filters.availability !== 'any-time') {
        params.append('availability', filters.availability);
    }
    if (filters.gender && filters.gender !== 'any') {
        params.append('gender', filters.gender);
    }
    if (filters.serviceType) {
        params.append('serviceType', filters.serviceType);
    }

    return params.toString();
}

/**
 * Fetch providers list with filtering and pagination (FR-006, AC1, AC2, AC3)
 * Implements retry logic for failed requests
 * @param filters - Search and filter criteria
 * @param pagination - Page number and page size
 * @returns Promise<ProviderListResponse>
 */
export async function fetchProviders(
    filters: ProviderFilters = {},
    pagination: PaginationParams = { page: 1, pageSize: 20 }
): Promise<ProviderListResponse> {
    const queryString = buildQueryString(filters, pagination);
    const url = `${API_BASE_URL}/api/providers?${queryString}`;

    // Get token from localStorage
    const token = localStorage.getItem('token');

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                ...(token && { 'Authorization': `Bearer ${token}` }),
            },
        });

        if (!response.ok) {
            // Handle HTTP error responses
            if (response.status === 401) {
                throw new Error('Unauthorized. Please log in again.');
            }
            if (response.status === 404) {
                throw new Error('Provider service not found');
            }
            if (response.status === 500) {
                throw new Error('Server error. Please try again later.');
            }
            throw new Error(`Failed to fetch providers: ${response.statusText}`);
        }

        const data = await response.json();
        return data as ProviderListResponse;

    } catch (error) {
        // Handle network errors
        if (error instanceof Error) {
            throw error;
        }
        throw new Error('Network error. Please check your connection and try again.');
    }
}

/**
 * Retry fetch with exponential backoff
 * Used for failed provider API requests
 * @param maxRetries - Maximum number of retry attempts (default: 3)
 */
export async function fetchProvidersWithRetry(
    filters: ProviderFilters = {},
    pagination: PaginationParams = { page: 1, pageSize: 20 },
    maxRetries = 3
): Promise<ProviderListResponse> {
    let lastError: Error | null = null;

    for (let attempt = 0; attempt < maxRetries; attempt++) {
        try {
            return await fetchProviders(filters, pagination);
        } catch (error) {
            lastError = error instanceof Error ? error : new Error('Unknown error');

            // Don't retry on 404 errors
            if (lastError.message.includes('not found')) {
                throw lastError;
            }

            // Wait before retrying (exponential backoff: 1s, 2s, 4s)
            if (attempt < maxRetries - 1) {
                await new Promise(resolve => setTimeout(resolve, Math.pow(2, attempt) * 1000));
            }
        }
    }

    throw lastError || new Error('Failed to fetch providers after multiple attempts');
}
