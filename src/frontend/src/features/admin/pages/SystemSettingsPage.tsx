import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../../store';
import {
    fetchSettings,
    updateSettings,
    clearError,
    selectReminderSettings,
} from '../../../store/slices/settingsSlice';
import type { SystemSetting } from '../../../types/settings';
import { Toggle } from '../../../components/ui/Toggle';

/**
 * System Settings page for Admin (US_037 - AC-4).
 * Allows configuration of reminder intervals and notification channels.
 * Implements Default, Loading, Error, and Validation states per SCR-026.
 */
export const SystemSettingsPage = () => {
    const dispatch = useDispatch<AppDispatch>();

    const { settings, loading, saving, error } = useSelector((state: RootState) => state.settings);
    const reminderSettings = useSelector((state: RootState) => selectReminderSettings(state));

    // Local form state
    const [intervals, setIntervals] = useState<number[]>([]);
    const [smsEnabled, setSmsEnabled] = useState(true);
    const [emailEnabled, setEmailEnabled] = useState(true);
    const [validationErrors, setValidationErrors] = useState<string[]>([]);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);

    useEffect(() => {
        dispatch(fetchSettings());
    }, [dispatch]);

    useEffect(() => {
        // Sync local state with Redux state when settings are loaded
        if (settings.length > 0) {
            setIntervals(reminderSettings.intervals);
            setSmsEnabled(reminderSettings.smsEnabled);
            setEmailEnabled(reminderSettings.emailEnabled);
        }
    }, [settings, reminderSettings]);

    useEffect(() => {
        if (error) {
            const timer = setTimeout(() => {
                dispatch(clearError());
            }, 5000);
            return () => clearTimeout(timer);
        }
    }, [error, dispatch]);

    useEffect(() => {
        if (successMessage) {
            const timer = setTimeout(() => {
                setSuccessMessage(null);
            }, 5000);
            return () => clearTimeout(timer);
        }
    }, [successMessage]);

    const addInterval = () => {
        setIntervals([...intervals, 24]); // Default to 24 hours
    };

    const removeInterval = (index: number) => {
        if (intervals.length > 1) {
            setIntervals(intervals.filter((_, i) => i !== index));
        }
    };

    const updateInterval = (index: number, value: string) => {
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
            errors.push('At least one reminder interval is required');
        }

        intervals.forEach((interval, index) => {
            if (interval <= 0) {
                errors.push(`Interval ${index + 1} must be greater than 0`);
            }
            if (!Number.isInteger(interval)) {
                errors.push(`Interval ${index + 1} must be a whole number`);
            }
        });

        // Validate at least one channel enabled
        if (!smsEnabled && !emailEnabled) {
            errors.push('At least one notification channel (SMS or Email) must be enabled');
        }

        setValidationErrors(errors);
        return errors.length === 0;
    };

    const handleSave = async () => {
        if (!validateForm()) {
            return;
        }

        // Sort intervals descending (e.g., 48, 24, 2)
        const sortedIntervals = [...intervals].sort((a, b) => b - a);

        const updatedSettings: SystemSetting[] = [
            {
                key: 'Reminder.Intervals',
                value: JSON.stringify(sortedIntervals),
                description: 'Reminder intervals in hours before appointment. JSON array format.',
            },
            {
                key: 'Reminder.SmsEnabled',
                value: smsEnabled.toString(),
                description: 'Enable SMS reminder notifications via Twilio.',
            },
            {
                key: 'Reminder.EmailEnabled',
                value: emailEnabled.toString(),
                description: 'Enable Email reminder notifications via SendGrid.',
            },
        ];

        try {
            await dispatch(updateSettings(updatedSettings)).unwrap();
            setSuccessMessage('Settings saved successfully! Future appointments will use the new intervals.');
            setValidationErrors([]);
            // Refresh settings to confirm save
            dispatch(fetchSettings());
        } catch (err) {
            console.error('Failed to save settings:', err);
        }
    };

    const handleRetry = () => {
        dispatch(fetchSettings());
    };

    // Loading state
    if (loading && settings.length === 0) {
        return (
            <div className="space-y-6">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">System Settings</h1>
                    <p className="mt-1 text-sm text-gray-500">
                        Configure reminder intervals and notification channels
                    </p>
                </div>
                <div className="bg-white shadow rounded-lg p-6">
                    <div className="flex items-center justify-center h-64">
                        <div className="text-center">
                            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                            <p className="mt-4 text-sm text-gray-500">Loading settings...</p>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    // Error state
    if (error && settings.length === 0) {
        return (
            <div className="space-y-6">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">System Settings</h1>
                    <p className="mt-1 text-sm text-gray-500">
                        Configure reminder intervals and notification channels
                    </p>
                </div>
                <div className="bg-white shadow rounded-lg p-6">
                    <div className="flex flex-col items-center justify-center h-64 space-y-4">
                        <svg className="h-12 w-12 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <div className="text-center">
                            <p className="text-lg font-medium text-gray-900">Failed to load settings</p>
                            <p className="mt-1 text-sm text-gray-500">{error}</p>
                        </div>
                        <button
                            onClick={handleRetry}
                            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                        >
                            Retry
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    // Default state (with form)
    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold text-gray-900">System Settings</h1>
                <p className="mt-1 text-sm text-gray-500">
                    Configure reminder intervals and notification channels. Future appointments will use new intervals; already-scheduled reminders remain unchanged.
                </p>
            </div>

            {/* Success Message */}
            {successMessage && (
                <div
                    className="bg-green-50 border border-green-200 rounded-lg p-4 flex items-start gap-3"
                    role="alert"
                    aria-live="polite"
                >
                    <svg className="h-5 w-5 text-green-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                    </svg>
                    <p className="text-sm text-green-800">{successMessage}</p>
                </div>
            )}

            {/* Error Message */}
            {error && (
                <div
                    className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3"
                    role="alert"
                    aria-live="assertive"
                >
                    <svg className="h-5 w-5 text-red-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <p className="text-sm text-red-800">{error}</p>
                </div>
            )}

            {/* Validation Errors */}
            {validationErrors.length > 0 && (
                <div
                    className="bg-yellow-50 border border-yellow-200 rounded-lg p-4"
                    role="alert"
                    aria-live="assertive"
                >
                    <div className="flex items-start gap-3">
                        <svg className="h-5 w-5 text-yellow-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <div className="flex-1">
                            <h3 className="text-sm font-medium text-yellow-800">Please fix the following errors:</h3>
                            <ul className="mt-2 list-disc list-inside text-sm text-yellow-700 space-y-1">
                                {validationErrors.map((err, index) => (
                                    <li key={index}>{err}</li>
                                ))}
                            </ul>
                        </div>
                    </div>
                </div>
            )}

            {/* Settings Form */}
            <div className="bg-white shadow rounded-lg">
                <div className="p-6 space-y-6">
                    <div>
                        <h3 className="text-lg font-semibold text-gray-900">Reminder Settings</h3>
                        <p className="mt-1 text-sm text-gray-500">
                            Configure when reminders are sent and which channels are enabled
                        </p>
                    </div>

                    {/* Reminder Intervals Section */}
                    <div>
                        <label className="block text-sm font-semibold text-gray-900 mb-1">
                            Reminder Intervals
                        </label>
                        <p className="text-sm text-gray-500 mb-4">
                            Configure when reminders are sent before appointments (in hours)
                        </p>
                        <div className="space-y-3">
                            {intervals.map((interval, index) => (
                                <div key={index} className="flex items-center gap-3">
                                    <input
                                        type="number"
                                        min="1"
                                        step="1"
                                        value={interval}
                                        onChange={(e) => updateInterval(index, e.target.value)}
                                        className="flex-1 rounded-lg border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                                        aria-label={`Reminder interval ${index + 1}`}
                                        aria-describedby={validationErrors.some(e => e.includes(`Interval ${index + 1}`)) ? `interval-${index}-error` : undefined}
                                    />
                                    <span className="text-sm text-gray-500 min-w-[100px]">hours before</span>
                                    <button
                                        type="button"
                                        onClick={() => removeInterval(index)}
                                        disabled={intervals.length === 1}
                                        className="p-2 text-gray-400 hover:text-red-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                        aria-label={`Remove interval ${index + 1}`}
                                    >
                                        <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                        </svg>
                                    </button>
                                </div>
                            ))}
                        </div>
                        <button
                            type="button"
                            onClick={addInterval}
                            className="mt-3 flex items-center gap-2 px-3 py-2 text-sm text-blue-700 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition-colors"
                        >
                            <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                            </svg>
                            Add Interval
                        </button>
                    </div>

                    {/* Notification Channels Section */}
                    <div className="border-t pt-6">
                        <h3 className="text-base font-semibold text-gray-900 mb-1">
                            Notification Channels
                        </h3>
                        <p className="text-sm text-gray-500 mb-4">
                            Enable or disable notification channels for reminders
                        </p>
                        <div className="space-y-0">
                            <Toggle
                                id="email-enabled"
                                checked={emailEnabled}
                                onChange={setEmailEnabled}
                                label="Email Notifications"
                                sublabel="Send email for appointment confirmations and reminders"
                            />
                            <Toggle
                                id="sms-enabled"
                                checked={smsEnabled}
                                onChange={setSmsEnabled}
                                label="SMS Notifications"
                                sublabel="Send SMS reminders for upcoming appointments"
                            />
                        </div>
                    </div>

                    {/* Save Button */}
                    <div className="border-t pt-6 flex justify-end">
                        <button
                            type="button"
                            onClick={handleSave}
                            disabled={saving}
                            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
                        >
                            {saving ? (
                                <>
                                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                                    Saving...
                                </>
                            ) : (
                                'Save Settings'
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};
