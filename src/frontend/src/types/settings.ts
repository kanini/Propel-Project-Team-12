/**
 * System settings types for Admin configuration (US_037).
 */

export interface SystemSetting {
    key: string;
    value: string;
    description?: string;
}

export interface ReminderSettings {
    intervals: number[];      // Parsed from "Reminder.Intervals" JSON string
    smsEnabled: boolean;      // Parsed from "Reminder.SmsEnabled"
    emailEnabled: boolean;    // Parsed from "Reminder.EmailEnabled"
}

export interface UpdateSystemSettingsRequest {
    settings: SystemSetting[];
}
