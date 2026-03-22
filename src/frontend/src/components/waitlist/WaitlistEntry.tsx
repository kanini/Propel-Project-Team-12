/**
 * WaitlistEntry Component for US_025 - Waitlist Enrollment
 * Displays individual waitlist entry with queue position, provider info, and actions
 * Implements AC-4 with queue position display
 */

import React, { useState } from 'react';
import { useAppDispatch } from '../../store/hooks';
import { leaveWaitlist, openEnrollmentModal, setSelectedEntry } from '../../store/slices/waitlistSlice';
import type { WaitlistEntry as WaitlistEntryType } from '../../types/waitlist';

interface WaitlistEntryProps {
    entry: WaitlistEntryType;
}

/**
 * Get status badge color based on waitlist status
 */
const getStatusBadgeClass = (status: string): string => {
    switch (status) {
        case 'active':
            return 'bg-green-100 text-green-800';
        case 'notified':
            return 'bg-blue-100 text-blue-800';
        case 'expired':
            return 'bg-gray-100 text-gray-800';
        case 'cancelled':
            return 'bg-red-100 text-red-800';
        default:
            return 'bg-gray-100 text-gray-800';
    }
};

/**
 * Format notification preference for display
 */
const formatNotificationPreference = (preference: string): string => {
    switch (preference) {
        case 'sms':
            return 'SMS';
        case 'email':
            return 'Email';
        case 'both':
            return 'SMS & Email';
        default:
            return preference;
    }
};

/**
 * WaitlistEntry component (AC-4)
 */
export const WaitlistEntry: React.FC<WaitlistEntryProps> = ({ entry }) => {
    const dispatch = useAppDispatch();
    const [isDeleting, setIsDeleting] = useState(false);
    const [showConfirmDelete, setShowConfirmDelete] = useState(false);

    /**
     * Handle update preferences button click
     */
    const handleUpdatePreferences = () => {
        dispatch(setSelectedEntry(entry));
        dispatch(
            openEnrollmentModal({
                providerId: entry.providerId,
                providerName: entry.providerName,
                providerSpecialty: entry.specialty,
            })
        );
    };

    /**
     * Handle leave waitlist button click
     */
    const handleLeaveWaitlist = () => {
        setShowConfirmDelete(true);
    };

    /**
     * Confirm delete action
     */
    const confirmDelete = async () => {
        setIsDeleting(true);
        await dispatch(leaveWaitlist(entry.id));
        setIsDeleting(false);
        setShowConfirmDelete(false);
    };

    /**
     * Cancel delete action
     */
    const cancelDelete = () => {
        setShowConfirmDelete(false);
    };

    /**
     * Format date range for display
     */
    const formatDateRange = () => {
        try {
            const startDate = new Date(entry.preferredStartDate);
            const endDate = new Date(entry.preferredEndDate);
            const formattedStart = startDate.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
            const formattedEnd = endDate.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

            if (formattedStart === formattedEnd) {
                return formattedStart;
            }
            return `${formattedStart} - ${formattedEnd}`;
        } catch {
            return `${entry.preferredStartDate} - ${entry.preferredEndDate}`;
        }
    };

    /**
     * Format created date for display
     */
    const formatCreatedDate = () => {
        try {
            const date = new Date(entry.createdAt);
            return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) + ' ' +
                date.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true });
        } catch {
            return entry.createdAt;
        }
    };

    return (
        <div className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
            {/* Header: Queue Position and Status */}
            <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-3">
                    {/* Queue Position Badge */}
                    <div className="bg-blue-600 text-white rounded-full w-12 h-12 flex items-center justify-center font-bold text-sm">
                        #{entry.queuePosition}
                    </div>
                    <div>
                        <h3 className="font-semibold text-gray-900 text-lg">{entry.providerName}</h3>
                        <p className="text-sm text-gray-600">{entry.specialty}</p>
                    </div>
                </div>

                {/* Status Badge */}
                <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusBadgeClass(
                        entry.status
                    )}`}
                >
                    {entry.status.charAt(0).toUpperCase() + entry.status.slice(1)}
                </span>
            </div>

            {/* Preferred Date Range */}
            <div className="mb-3">
                <div className="flex items-start gap-2">
                    <svg
                        className="w-5 h-5 text-gray-400 mt-0.5"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                        />
                    </svg>
                    <div>
                        <p className="text-xs text-gray-500">Preferred Date Range</p>
                        <p className="text-sm text-gray-900">{formatDateRange()}</p>
                    </div>
                </div>
            </div>

            {/* Notification Preference */}
            <div className="mb-4">
                <div className="flex items-start gap-2">
                    <svg
                        className="w-5 h-5 text-gray-400 mt-0.5"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
                        />
                    </svg>
                    <div>
                        <p className="text-xs text-gray-500">Notifications</p>
                        <p className="text-sm text-gray-900">{formatNotificationPreference(entry.notificationPreference)}</p>
                    </div>
                </div>
            </div>

            {/* Confirmation Dialog */}
            {showConfirmDelete && (
                <div className="bg-red-50 border border-red-200 rounded-md p-3 mb-3">
                    <p className="text-sm text-red-800 mb-2">Are you sure you want to leave this waitlist?</p>
                    <div className="flex gap-2">
                        <button
                            onClick={confirmDelete}
                            disabled={isDeleting}
                            className="px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 disabled:opacity-50"
                        >
                            {isDeleting ? 'Removing...' : 'Yes, Leave'}
                        </button>
                        <button
                            onClick={cancelDelete}
                            disabled={isDeleting}
                            className="px-3 py-1 bg-white border border-gray-300 text-gray-700 text-sm rounded hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 disabled:opacity-50"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}

            {/* Action Buttons */}
            {entry.status === 'active' && !showConfirmDelete && (
                <div className="flex flex-col sm:flex-row gap-2">
                    <button
                        onClick={handleUpdatePreferences}
                        className="flex-1 px-4 py-2 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                    >
                        Update Preferences
                    </button>
                    <button
                        onClick={handleLeaveWaitlist}
                        className="flex-1 px-4 py-2 border border-red-600 text-red-600 text-sm rounded-md hover:bg-red-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
                    >
                        Leave Waitlist
                    </button>
                </div>
            )}

            {/* Created Date (Footer) */}
            <div className="mt-3 pt-3 border-t border-gray-100">
                <p className="text-xs text-gray-500">
                    Joined {formatCreatedDate()}
                </p>
            </div>
        </div>
    );
};
