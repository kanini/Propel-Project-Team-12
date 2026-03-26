/**
 * System settings types for admin configuration (US_037 - FR-014, AC-4).
 * Supports reminder interval and notification channel management.
 */

/**
 * System setting key-value pair from API.
 */
export interface SystemSetting {
    key: string;
    value: string;
    description?: string;
}

/**
 * Parsed reminder settings for UI display and editing.
 */
export interface ReminderSettings {
    intervals: number[];      // Hours before appointment (e.g., [48, 24, 2])
    smsEnabled: boolean;      // SMS notification toggle
    emailEnabled: boolean;    // Email notification toggle
}

/**
 * Request payload for updating system settings.
 */
export interface UpdateSystemSettingsRequest {
    settings: SystemSetting[];
}
