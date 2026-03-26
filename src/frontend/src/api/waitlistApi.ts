/**
 * Waitlist API client functions for US_041 - Waitlist Slot Availability Notifications
 * Handles waitlist enrollment, status checks, and notification response actions
 */

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

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
 * Confirm waitlist slot offer response (US_041 AC-2)
 * AllowAnonymous endpoint - uses response token for authentication
 */
export interface ConfirmWaitlistResponse {
    success: boolean;
    message: string;
    appointmentId?: string;
    appointmentDetails?: {
        providerName: string;
        scheduledDateTime: string;
        confirmationCode: string;
    };
}

/**
 * Confirm waitlist slot offer (US_041 AC-2)
 * @param token - Unique response token from notification email/SMS
 * @returns Promise<ConfirmWaitlistResponse>
 */
export async function confirmWaitlistSlot(token: string): Promise<ConfirmWaitlistResponse> {
    const url = `${API_BASE_URL}/api/waitlist/${token}/confirm`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (response.status === 404) {
            throw new Error('Invalid or expired notification token. This link may have already been used or expired.');
        }

        if (response.status === 409) {
            const data = await response.json();
            return {
                success: false,
                message: data.message || 'This slot is no longer available. It may have been booked by another patient.',
            };
        }

        if (response.status === 410) {
            throw new Error('This notification has expired or already been responded to.');
        }

        if (!response.ok) {
            throw new Error(`Failed to confirm waitlist slot: ${response.statusText}`);
        }

        const data: ConfirmWaitlistResponse = await response.json();
        return data;
    } catch (error) {
        if (error instanceof Error) {
            throw error;
        }
        throw new Error('An unexpected error occurred while confirming the appointment.');
    }
}

/**
 * Decline waitlist slot offer (US_041 AC-3)
 * @param token - Unique response token from notification email/SMS
 * @returns Promise<{ message: string }>
 */
export async function declineWaitlistSlot(token: string): Promise<{ message: string }> {
    const url = `${API_BASE_URL}/api/waitlist/${token}/decline`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (response.status === 404) {
            throw new Error('Invalid or expired notification token.');
        }

        if (response.status === 410) {
            throw new Error('This notification has expired or already been responded to.');
        }

        if (!response.ok) {
            throw new Error(`Failed to decline waitlist slot: ${response.statusText}`);
        }

        const data = await response.json();
        return { message: data.message || 'You remain on the waitlist. You\'ll be notified when another slot becomes available.' };
    } catch (error) {
        if (error instanceof Error) {
            throw error;
        }
        throw new Error('An unexpected error occurred while declining the appointment.');
    }
}

/**
 * Get waitlist entries for authenticated user
 * @returns Promise<WaitlistEntry[]>
 */
export interface WaitlistEntry {
    id: string;
    providerId: string;
    providerName: string;
    specialty: string;
    preferredStartDate: string;
    preferredEndDate: string;
    notificationPreference: 'sms' | 'email' | 'both';
    status: 'active' | 'notified' | 'cancelled' | 'expired';
    queuePosition: number;
    createdAt: string;
    notifiedAt?: string;
    responseDeadline?: string;
}

/**
 * Fetch current waitlist entries for authenticated user
 */
export async function fetchWaitlistEntries(): Promise<WaitlistEntry[]> {
    const url = `${API_BASE_URL}/api/waitlist`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: getAuthHeaders(),
        });

        if (response.status === 401) {
            throw new Error('Unauthorized. Please log in again.');
        }

        if (!response.ok) {
            throw new Error(`Failed to fetch waitlist entries: ${response.statusText}`);
        }

        const data: WaitlistEntry[] = await response.json();
        return data;
    } catch (error) {
        if (error instanceof Error) {
            throw error;
        }
        throw new Error('Failed to load waitlist entries.');
    }
}

/**
 * Cancel waitlist entry
 * @param waitlistEntryId - Waitlist entry ID to cancel
 */
export async function cancelWaitlistEntry(waitlistEntryId: string): Promise<void> {
    const url = `${API_BASE_URL}/api/waitlist/${waitlistEntryId}`;

    try {
        const response = await fetch(url, {
            method: 'DELETE',
            headers: getAuthHeaders(),
        });

        if (response.status === 401) {
            throw new Error('Unauthorized. Please log in again.');
        }

        if (response.status === 404) {
            throw new Error('Waitlist entry not found.');
        }

        if (!response.ok) {
            throw new Error(`Failed to cancel waitlist entry: ${response.statusText}`);
        }
    } catch (error) {
        if (error instanceof Error) {
            throw error;
        }
        throw new Error('Failed to cancel waitlist entry.');
    }
}
