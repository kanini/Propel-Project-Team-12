/**
 * Waitlist type definitions for US_025 - Waitlist Enrollment
 * Manages waitlist enrollment state, queue positions, and notification preferences
 */

/**
 * Notification preference for waitlist notifications
 */
export type NotificationPreference = 'sms' | 'email' | 'both';

/**
 * Waitlist entry status
 */
export type WaitlistStatus = 'active' | 'notified' | 'cancelled' | 'expired';

/**
 * Waitlist entry entity
 */
export interface WaitlistEntry {
    id: string;
    providerId: string;
    providerName: string;
    specialty: string;
    preferredStartDate: string; // YYYY-MM-DD format
    preferredEndDate: string; // YYYY-MM-DD format
    notificationPreference: NotificationPreference;
    status: WaitlistStatus;
    queuePosition: number;
    createdAt: string; // ISO datetime string
}

/**
 * Join waitlist request payload (FR-009, AC-1, AC-2)
 */
export interface JoinWaitlistRequest {
    providerId: string;
    preferredStartDate: string; // YYYY-MM-DD format
    preferredEndDate: string; // YYYY-MM-DD format
    notificationPreference: NotificationPreference;
    reason?: string;
}

/**
 * Update waitlist request payload (AC-3)
 */
export interface UpdateWaitlistRequest {
    preferredStartDate?: string;
    preferredEndDate?: string;
    notificationPreference?: NotificationPreference;
    reason?: string;
}

/**
 * Waitlist error types
 */
export interface WaitlistError {
    code: 'conflict' | 'validation' | 'server' | 'unauthorized';
    message: string;
    existingEntry?: WaitlistEntry; // For conflict errors (AC-3)
    details?: Record<string, string[]>;
}
