/**
 * Calendar API client functions for US_039/US_040 - Multi-provider Calendar Synchronization
 * Handles calendar connection status, OAuth flow initiation, and disconnection for Google and Outlook
 */

/**
 * API base URL from environment variables with fallback
 */
const API_BASE_URL =
    import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Get authorization headers with token
 */
function getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('token');
    return {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }),
    };
}

/**
 * Multi-provider calendar connection status
 */
export interface CalendarStatusResponse {
    google: { isConnected: boolean };
    outlook: { isConnected: boolean };
}

/**
 * OAuth authorization URL response
 */
export interface ConnectUrlResponse {
    authorizationUrl: string;
    provider: string;
}

/**
 * Get connection status for all calendar providers
 * @returns Promise<CalendarStatusResponse>
 */
export async function getCalendarStatus(): Promise<CalendarStatusResponse> {
    const url = `${API_BASE_URL}/api/calendar/status`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            if (response.status === 401) {
                throw new Error('Unauthorized. Please log in again.');
            }
            throw new Error(`Failed to get calendar status: ${response.statusText}`);
        }

        const data: CalendarStatusResponse = await response.json();
        return data;
    } catch (error) {
        if (error instanceof Error) {
            throw error;
        }
        throw new Error('Network error. Please check your connection and try again.');
    }
}

/**
 * Get OAuth2 authorization URL for calendar connection
 * Redirect user to this URL to initiate OAuth flow
 * @param provider - Calendar provider ('google' or 'outlook')
 * @returns Promise<ConnectUrlResponse>
 */
export async function getCalendarConnectUrl(provider: 'google' | 'outlook'): Promise<ConnectUrlResponse> {
    const url = `${API_BASE_URL}/api/calendar/${provider}/connect`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            if (response.status === 401) {
                throw new Error('Unauthorized. Please log in again.');
            }
            throw new Error(`Failed to get calendar connect URL: ${response.statusText}`);
        }

        const data: ConnectUrlResponse = await response.json();
        return data;
    } catch (error) {
        if (error instanceof Error) {
            throw error;
        }
        throw new Error('Network error. Please check your connection and try again.');
    }
}

/**
 * Disconnect user's calendar integration
 * Revokes calendar connection and removes stored tokens
 * @param provider - Calendar provider ('google' or 'outlook')
 * @returns Promise<void>
 */
export async function disconnectCalendar(provider: 'google' | 'outlook'): Promise<void> {
    const url = `${API_BASE_URL}/api/calendar/${provider}/disconnect`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            if (response.status === 401) {
                throw new Error('Unauthorized. Please log in again.');
            }
            throw new Error(`Failed to disconnect calendar: ${response.statusText}`);
        }
    } catch (error) {
        if (error instanceof Error) {
            throw error;
        }
        throw new Error('Network error. Please check your connection and try again.');
    }
}
