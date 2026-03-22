/**
 * WaitlistEnrollmentModal Component for US_025 - Waitlist Enrollment
 * Modal form for joining or updating waitlist with date range and notification preferences
 * Implements AC-1, AC-2, AC-3 with WCAG 2.2 AA compliance (UXR-201)
 */

import React, { useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import {
    joinWaitlist,
    updateWaitlist,
    closeEnrollmentModal,
    selectIsEnrollmentModalOpen,
    selectPreSelectedProvider,
    selectSelectedEntry,
    selectIsJoining,
    selectIsUpdating,
    selectError,
} from '../../store/slices/waitlistSlice';
import type { NotificationPreference } from '../../types/waitlist';

/**
 * WaitlistEnrollmentModal component
 */
export const WaitlistEnrollmentModal: React.FC = () => {
    const dispatch = useAppDispatch();

    // Redux state
    const isOpen = useAppSelector(selectIsEnrollmentModalOpen);
    const preSelectedProvider = useAppSelector(selectPreSelectedProvider);
    const selectedEntry = useAppSelector(selectSelectedEntry);
    const isJoining = useAppSelector(selectIsJoining);
    const isUpdating = useAppSelector(selectIsUpdating);
    const error = useAppSelector(selectError);

    // Form state
    const [preferredStartDate, setPreferredStartDate] = useState('');
    const [preferredEndDate, setPreferredEndDate] = useState('');
    const [notificationPreference, setNotificationPreference] = useState<NotificationPreference>('both');
    const [reason, setReason] = useState('');

    // Validation errors
    const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

    // Editing mode: true if updating existing entry
    const isEditing = !!selectedEntry;

    // Modal title based on mode
    const modalTitle = isEditing ? 'Update Waitlist Preferences' : 'Join Waitlist';

    // Provider display name
    const providerDisplay = isEditing
        ? `${selectedEntry.providerName} (${selectedEntry.specialty})`
        : preSelectedProvider.name
            ? `${preSelectedProvider.name} (${preSelectedProvider.specialty})`
            : 'Provider';

    /**
     * Initialize form with existing entry data when editing
     */
    useEffect(() => {
        if (selectedEntry) {
            setPreferredStartDate(selectedEntry.preferredStartDate);
            setPreferredEndDate(selectedEntry.preferredEndDate);
            setNotificationPreference(selectedEntry.notificationPreference);
            setReason('');
        } else {
            // Reset form when opening for new entry
            const today = new Date().toISOString().split('T')[0];
            setPreferredStartDate(today || '');
            setPreferredEndDate(today || '');
            setNotificationPreference('both');
            setReason('');
        }
        setValidationErrors({});
    }, [selectedEntry, isOpen]);

    /**
     * Handle modal close with escape key (UXR-201)
     */
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && isOpen) {
                handleClose();
            }
        };

        document.addEventListener('keydown', handleEscape);
        return () => document.removeEventListener('keydown', handleEscape);
    }, [isOpen]);

    /**
     * Focus trap: Focus first input when modal opens (UXR-201)
     */
    useEffect(() => {
        if (isOpen) {
            const firstInput = document.getElementById('preferredStartDate');
            firstInput?.focus();
        }
    }, [isOpen]);

    /**
     * Validate form data
     */
    const validateForm = (): boolean => {
        const errors: Record<string, string> = {};

        if (!preferredStartDate) {
            errors.preferredStartDate = 'Start date is required';
        }

        if (!preferredEndDate) {
            errors.preferredEndDate = 'End date is required';
        }

        if (preferredStartDate && preferredEndDate && preferredEndDate < preferredStartDate) {
            errors.preferredEndDate = 'End date must be on or after start date';
        }

        setValidationErrors(errors);
        return Object.keys(errors).length === 0;
    };

    /**
     * Handle form submission
     */
    const handleSubmit = (event: React.FormEvent) => {
        event.preventDefault();

        if (!validateForm()) {
            return;
        }

        if (isEditing && selectedEntry) {
            // Update existing waitlist entry (AC-3)
            dispatch(
                updateWaitlist({
                    id: selectedEntry.id,
                    request: {
                        preferredStartDate,
                        preferredEndDate,
                        notificationPreference,
                        reason: reason || undefined,
                    },
                })
            );
        } else if (preSelectedProvider.id) {
            // Join new waitlist (AC-1, AC-2)
            dispatch(
                joinWaitlist({
                    providerId: preSelectedProvider.id,
                    preferredStartDate,
                    preferredEndDate,
                    notificationPreference,
                    reason: reason || undefined,
                })
            );
        }
    };

    /**
     * Handle modal close
     */
    const handleClose = () => {
        dispatch(closeEnrollmentModal());
        setValidationErrors({});
    };

    /**
     * Handle conflict error: switch to editing mode (AC-3)
     */
    useEffect(() => {
        if (error?.code === 'conflict' && error.existingEntry) {
            // Pre-populate form with existing entry data
            setPreferredStartDate(error.existingEntry.preferredStartDate);
            setPreferredEndDate(error.existingEntry.preferredEndDate);
            setNotificationPreference(error.existingEntry.notificationPreference);
        }
    }, [error]);

    if (!isOpen) return null;

    const isSubmitting = isJoining || isUpdating;

    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
            role="dialog"
            aria-modal="true"
            aria-labelledby="waitlist-modal-title"
            aria-describedby="waitlist-modal-description"
        >
            <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4 p-6">
                {/* Header */}
                <div className="flex items-center justify-between mb-4">
                    <h2 id="waitlist-modal-title" className="text-xl font-semibold text-gray-900">
                        {modalTitle}
                    </h2>
                    <button
                        onClick={handleClose}
                        className="text-gray-400 hover:text-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded"
                        aria-label="Close modal"
                    >
                        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Description */}
                <p id="waitlist-modal-description" className="text-sm text-gray-600 mb-4">
                    {isEditing
                        ? 'Update your waitlist preferences for notifications and preferred dates.'
                        : 'Join the waitlist to be notified when matching appointment slots become available.'}
                </p>

                {/* Provider Display */}
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-4">
                    <label className="text-sm font-medium text-gray-700 block mb-1">Provider</label>
                    <div className="text-sm text-gray-900">{providerDisplay}</div>
                </div>

                {/* Conflict Error Message (AC-3) */}
                {error?.code === 'conflict' && error.existingEntry && (
                    <div
                        className="bg-amber-50 border border-amber-200 rounded-lg p-3 mb-4"
                        role="alert"
                        aria-live="assertive"
                    >
                        <p className="text-sm text-amber-800">
                            You are already on the waitlist for this provider (Position #{error.existingEntry.queuePosition}).
                            You can update your preferences below.
                        </p>
                    </div>
                )}

                {/* Other Errors */}
                {error && error.code !== 'conflict' && (
                    <div
                        className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4"
                        role="alert"
                        aria-live="assertive"
                    >
                        <p className="text-sm text-red-800">{error.message}</p>
                    </div>
                )}

                {/* Form */}
                <form onSubmit={handleSubmit} className="space-y-4">
                    {/* Preferred Date Range Start */}
                    <div>
                        <label htmlFor="preferredStartDate" className="block text-sm font-medium text-gray-700 mb-1">
                            Preferred Start Date <span className="text-red-500">*</span>
                        </label>
                        <input
                            type="date"
                            id="preferredStartDate"
                            value={preferredStartDate}
                            onChange={(e) => setPreferredStartDate(e.target.value)}
                            min={new Date().toISOString().split('T')[0]}
                            className={`w-full px-3 py-2 border ${validationErrors.preferredStartDate ? 'border-red-500' : 'border-gray-300'
                                } rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500`}
                            aria-invalid={!!validationErrors.preferredStartDate}
                            aria-describedby={validationErrors.preferredStartDate ? 'start-date-error' : undefined}
                        />
                        {validationErrors.preferredStartDate && (
                            <p id="start-date-error" className="text-sm text-red-600 mt-1" role="alert">
                                {validationErrors.preferredStartDate}
                            </p>
                        )}
                    </div>

                    {/* Preferred Date Range End */}
                    <div>
                        <label htmlFor="preferredEndDate" className="block text-sm font-medium text-gray-700 mb-1">
                            Preferred End Date <span className="text-red-500">*</span>
                        </label>
                        <input
                            type="date"
                            id="preferredEndDate"
                            value={preferredEndDate}
                            onChange={(e) => setPreferredEndDate(e.target.value)}
                            min={preferredStartDate || new Date().toISOString().split('T')[0]}
                            className={`w-full px-3 py-2 border ${validationErrors.preferredEndDate ? 'border-red-500' : 'border-gray-300'
                                } rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500`}
                            aria-invalid={!!validationErrors.preferredEndDate}
                            aria-describedby={validationErrors.preferredEndDate ? 'end-date-error' : undefined}
                        />
                        {validationErrors.preferredEndDate && (
                            <p id="end-date-error" className="text-sm text-red-600 mt-1" role="alert">
                                {validationErrors.preferredEndDate}
                            </p>
                        )}
                    </div>

                    {/* Notification Preferences */}
                    <div>
                        <fieldset>
                            <legend className="block text-sm font-medium text-gray-700 mb-2">
                                Notification Preferences <span className="text-red-500">*</span>
                            </legend>
                            <div className="space-y-2">
                                <label className="flex items-center">
                                    <input
                                        type="radio"
                                        name="notificationPreference"
                                        value="sms"
                                        checked={notificationPreference === 'sms'}
                                        onChange={(e) => setNotificationPreference(e.target.value as NotificationPreference)}
                                        className="mr-2 focus:ring-2 focus:ring-blue-500"
                                    />
                                    <span className="text-sm text-gray-700">SMS Only</span>
                                </label>
                                <label className="flex items-center">
                                    <input
                                        type="radio"
                                        name="notificationPreference"
                                        value="email"
                                        checked={notificationPreference === 'email'}
                                        onChange={(e) => setNotificationPreference(e.target.value as NotificationPreference)}
                                        className="mr-2 focus:ring-2 focus:ring-blue-500"
                                    />
                                    <span className="text-sm text-gray-700">Email Only</span>
                                </label>
                                <label className="flex items-center">
                                    <input
                                        type="radio"
                                        name="notificationPreference"
                                        value="both"
                                        checked={notificationPreference === 'both'}
                                        onChange={(e) => setNotificationPreference(e.target.value as NotificationPreference)}
                                        className="mr-2 focus:ring-2 focus:ring-blue-500"
                                    />
                                    <span className="text-sm text-gray-700">Both SMS and Email</span>
                                </label>
                            </div>
                        </fieldset>
                    </div>

                    {/* Reason (Optional) */}
                    <div>
                        <label htmlFor="reason" className="block text-sm font-medium text-gray-700 mb-1">
                            Reason (Optional)
                        </label>
                        <textarea
                            id="reason"
                            value={reason}
                            onChange={(e) => setReason(e.target.value)}
                            maxLength={500}
                            rows={3}
                            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Add any additional details..."
                        />
                        <p className="text-xs text-gray-500 mt-1">{reason.length}/500 characters</p>
                    </div>

                    {/* Action Buttons */}
                    <div className="flex flex-col-reverse sm:flex-row gap-3 pt-2">
                        <button
                            type="button"
                            onClick={handleClose}
                            className="flex-1 px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
                            disabled={isSubmitting}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                            disabled={isSubmitting}
                        >
                            {isSubmitting ? (
                                <span className="flex items-center justify-center">
                                    <svg
                                        className="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
                                        fill="none"
                                        viewBox="0 0 24 24"
                                    >
                                        <circle
                                            className="opacity-25"
                                            cx="12"
                                            cy="12"
                                            r="10"
                                            stroke="currentColor"
                                            strokeWidth="4"
                                        />
                                        <path
                                            className="opacity-75"
                                            fill="currentColor"
                                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                                        />
                                    </svg>
                                    {isEditing ? 'Updating...' : 'Joining...'}
                                </span>
                            ) : isEditing ? (
                                'Update Preferences'
                            ) : (
                                'Join Waitlist'
                            )}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};
