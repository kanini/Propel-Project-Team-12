import { useEffect, useState } from "react";
import type { ChangeEvent } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../../../store";
import {
    fetchSettings,
    updateSettings,
    selectReminderSettings,
    clearError,
} from "../../../store/settingsSlice";

/**
 * System Settings page for Admin (US_037 - AC-4).
 * Allows configuration of reminder intervals and notification channel toggles.
 */
export const SystemSettingsPage = () => {
    const dispatch = useDispatch<AppDispatch>();

    const reminderSettings = useSelector((state: RootState) =>
        selectReminderSettings(state)
    );
    const isLoading = useSelector((state: RootState) => state.settings.isLoading);
    const isSaving = useSelector((state: RootState) => state.settings.isSaving);
    const error = useSelector((state: RootState) => state.settings.error);

    const [intervals, setIntervals] = useState<number[]>([48, 24, 2]);
    const [smsEnabled, setSmsEnabled] = useState(true);
    const [emailEnabled, setEmailEnabled] = useState(true);
    const [validationErrors, setValidationErrors] = useState<string[]>([]);
    const [showSuccess, setShowSuccess] = useState(false);

    useEffect(() => {
        dispatch(fetchSettings());
    }, [dispatch]);

    useEffect(() => {
        if (reminderSettings) {
            setIntervals(reminderSettings.intervals);
            setSmsEnabled(reminderSettings.smsEnabled);
            setEmailEnabled(reminderSettings.emailEnabled);
        }
    }, [reminderSettings]);

    const handleAddInterval = () => {
        setIntervals([...intervals, 2]);
    };

    const handleRemoveInterval = (index: number) => {
        if (intervals.length > 1) {
            setIntervals(intervals.filter((_: number, i: number) => i !== index));
        }
    };

    const handleIntervalChange = (index: number, value: string) => {
        const numValue = parseInt(value, 10);
        if (!isNaN(numValue)) {
            const newIntervals = [...intervals];
            newIntervals[index] = numValue;
            setIntervals(newIntervals);
        }
    };

    const validateForm = (): boolean => {
        const errors: string[] = [];

        // Validate intervals
        if (intervals.length === 0) {
            errors.push("At least one reminder interval is required");
        }

        intervals.forEach((interval: number, index: number) => {
            if (interval <= 0) {
                errors.push(`Interval ${index + 1} must be greater than 0`);
            }
            if (!Number.isInteger(interval)) {
                errors.push(`Interval ${index + 1} must be a whole number`);
            }
        });

        // Validate at least one channel enabled
        if (!smsEnabled && !emailEnabled) {
            errors.push("At least one notification channel (SMS or Email) must be enabled");
        }

        setValidationErrors(errors);
        return errors.length === 0;
    };

    const handleSave = async () => {
        if (!validateForm()) {
            return;
        }

        try {
            // Sort intervals descending (largest first)
            const sortedIntervals = [...intervals].sort((a, b) => b - a);

            await dispatch(
                updateSettings({
                    settings: [
                        {
                            key: "Reminder.Intervals",
                            value: JSON.stringify(sortedIntervals),
                        },
                        {
                            key: "Reminder.SmsEnabled",
                            value: smsEnabled.toString(),
                        },
                        {
                            key: "Reminder.EmailEnabled",
                            value: emailEnabled.toString(),
                        },
                    ],
                })
            ).unwrap();

            setShowSuccess(true);
            setTimeout(() => setShowSuccess(false), 3000);
        } catch (error) {
            console.error("Failed to save settings:", error);
        }
    };

    const handleClearError = () => {
        dispatch(clearError());
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
                <span className="ml-3 text-lg">Loading settings...</span>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="border-b border-neutral-200 pb-4">
                <h1 className="text-3xl font-bold text-neutral-900">System Settings</h1>
                <p className="mt-2 text-neutral-600">
                    Configure appointment reminder intervals and notification channels.
                </p>
            </div>

            {/* Error Display */}
            {error && (
                <div className="bg-error-50 border border-error-200 text-error-700 px-4 py-3 rounded-lg flex items-center justify-between">
                    <p>{error}</p>
                    <button
                        onClick={handleClearError}
                        className="text-error-700 hover:text-error-900 font-medium"
                    >
                        Dismiss
                    </button>
                </div>
            )}

            {/* Success Message */}
            {showSuccess && (
                <div className="bg-success-50 border border-success-200 text-success-700 px-4 py-3 rounded-lg">
                    <p>Settings updated successfully. Changes will apply to future appointments only.</p>
                </div>
            )}

            {/* Validation Errors */}
            {validationErrors.length > 0 && (
                <div className="bg-warning-50 border border-warning-200 text-warning-700 px-4 py-3 rounded-lg">
                    <p className="font-medium mb-2">Please fix the following errors:</p>
                    <ul className="list-disc list-inside space-y-1">
                        {validationErrors.map((error: string, index: number) => (
                            <li key={index}>{error}</li>
                        ))}
                    </ul>
                </div>
            )}

            {/* Settings Form */}
            <div className="bg-white shadow-sm rounded-lg p-6 space-y-6">
                {/* Reminder Intervals Section */}
                <div>
                    <label className="block text-lg font-medium text-neutral-900 mb-2">
                        Reminder Intervals (hours before appointment)
                    </label>
                    <p className="text-sm text-neutral-600 mb-4">
                        Configure when reminder notifications should be sent before scheduled appointments.
                    </p>
                    <div className="space-y-3">
                        {intervals.map((interval: number, index: number) => (
                            <div key={index} className="flex items-center gap-3">
                                <input
                                    type="number"
                                    min="1"
                                    value={interval}
                                    onChange={(e: ChangeEvent<HTMLInputElement>) => handleIntervalChange(index, e.target.value)}
                                    className="w-32 px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                                    aria-label={`Interval ${index + 1}`}
                                />
                                <span className="text-neutral-700">hours before</span>
                                {intervals.length > 1 && (
                                    <button
                                        onClick={() => handleRemoveInterval(index)}
                                        className="ml-auto text-error-600 hover:text-error-800 font-medium"
                                        aria-label={`Remove interval ${index + 1}`}
                                    >
                                        Remove
                                    </button>
                                )}
                            </div>
                        ))}
                    </div>
                    <button
                        onClick={handleAddInterval}
                        className="mt-3 px-4 py-2 bg-primary-100 text-primary-700 rounded-lg hover:bg-primary-200 font-medium"
                    >
                        Add Interval
                    </button>
                </div>

                {/* Notification Channels Section */}
                <div className="border-t border-neutral-200 pt-6">
                    <label className="block text-lg font-medium text-neutral-900 mb-2">
                        Notification Channels
                    </label>
                    <p className="text-sm text-neutral-600 mb-4">
                        Enable or disable SMS and email reminder notifications.
                    </p>
                    <div className="space-y-3">
                        <div className="flex items-center">
                            <input
                                id="sms-enabled"
                                type="checkbox"
                                checked={smsEnabled}
                                onChange={(e: ChangeEvent<HTMLInputElement>) => setSmsEnabled(e.target.checked)}
                                className="h-5 w-5 text-primary-600 focus:ring-primary-500 border-neutral-300 rounded"
                                aria-checked={smsEnabled}
                                aria-label="Enable SMS reminders"
                            />
                            <label htmlFor="sms-enabled" className="ml-3 text-neutral-900">
                                Enable SMS Reminders (via Twilio)
                            </label>
                        </div>
                        <div className="flex items-center">
                            <input
                                id="email-enabled"
                                type="checkbox"
                                checked={emailEnabled}
                                onChange={(e: ChangeEvent<HTMLInputElement>) => setEmailEnabled(e.target.checked)}
                                className="h-5 w-5 text-primary-600 focus:ring-primary-500 border-neutral-300 rounded"
                                aria-checked={emailEnabled}
                                aria-label="Enable email reminders"
                            />
                            <label htmlFor="email-enabled" className="ml-3 text-neutral-900">
                                Enable Email Reminders
                            </label>
                        </div>
                    </div>
                </div>

                {/* Save Button */}
                <div className="border-t border-neutral-200 pt-6 flex justify-end">
                    <button
                        onClick={handleSave}
                        disabled={isSaving}
                        className="px-6 py-3 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:bg-neutral-400 disabled:cursor-not-allowed font-medium"
                    >
                        {isSaving ? "Saving..." : "Save Settings"}
                    </button>
                </div>
            </div>

            {/* Info Notice */}
            <div className="bg-primary-50 border border-primary-200 text-primary-700 px-4 py-3 rounded-lg">
                <p className="font-medium">Important:</p>
                <p className="mt-1 text-sm">
                    Changes to reminder intervals will apply only to future appointments.
                    Reminders already scheduled for existing appointments will not be affected.
                </p>
            </div>
        </div>
    );
};
