/**
 * Provider type definitions for US_023 - Provider and Service Browser
 * Matches backend API response structure for provider data
 */

/**
 * Provider entity representing a healthcare provider in the system
 */
export interface Provider {
    id: string;
    name: string;
    specialty: string;
    rating: number;
    reviewCount: number;
    nextAvailableSlot: string | null; // ISO datetime string or null if no availability
    avatarUrl?: string;
    gender?: string;
    location?: string;
}

/**
 * Provider search and filter parameters for API calls
 */
export interface ProviderFilters {
    search?: string;
    specialty?: string;
    availability?: string; // "today" | "this-week" | "this-month" | "any-time"
    gender?: string;
    serviceType?: string;
}

/**
 * Pagination parameters for provider list requests
 */
export interface PaginationParams {
    page: number;
    pageSize: number;
}

/**
 * Paginated provider response from backend API
 */
export interface ProviderListResponse {
    providers: Provider[];
    total: number;
    page: number;
    pageSize: number;
    totalPages: number;
}
