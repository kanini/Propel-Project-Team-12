/**
 * AppointmentCard Component for displaying appointment details with swap status
 * Shows appointment information and allows patients to manage swap preferences (US_026)
 * Extended with cancel and reschedule actions (US_027)
 */

import { useState } from 'react';
import type { Appointment } from '../../types/appointment';

interface AppointmentCardProps {
    appointment: Appointment;
    onCancelSwap?: (appointmentId: string) => void;
    onCancel?: (appointmentId: string) => void;
    onReschedule?: (appointmentId: string) => void;
    isCancelingSwap?: boolean;
    showActions?: boolean;
}

/**
 * AppointmentCard displays appointment details with optional swap status and management
 */
export function AppointmentCard({
    appointment,
    onCancelSwap,
    onCancel,
    onReschedule,
    isCancelingSwap = false,
    showActions = true,
}: AppointmentCardProps) {
    const [showCancelConfirm, setShowCancelConfirm] = useState(false);

    /**
     * Format date and time for display
     */
    const formatDateTime = (isoDateString: string): string => {
        const date = new Date(isoDateString);
        return date.toLocaleString('en-US', {
            weekday: 'short',
            month: 'short',
            day: 'numeric',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
        });
    };

    /**
     * Get status badge color
     */
    const getStatusColor = (status: string): string => {
        switch (status.toLowerCase()) {
            case 'scheduled':
                return 'bg-blue-100 text-blue-800';
            case 'confirmed':
                return 'bg-green-100 text-green-800';
            case 'completed':
                return 'bg-gray-100 text-gray-800';
            case 'cancelled':
                return 'bg-red-100 text-red-800';
            default:
                return 'bg-gray-100 text-gray-800';
        }
    };

    /**
     * Handle cancel swap confirmation
     */
    const handleConfirmCancelSwap = () => {
        if (onCancelSwap) {
            onCancelSwap(appointment.id);
            setShowCancelConfirm(false);
        }
    };

    const hasSwapPreference = !!appointment.preferredSlotId;

    return (
        <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-5 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                    <h3 className="text-lg font-semibold text-neutral-900">
                        {appointment.providerName}
                    </h3>
                    <p className="text-sm text-neutral-600">
                        {appointment.providerSpecialty}
                    </p>
                </div>
                <span
                    className={`px-3 py-1 rounded-full text-xs font-medium ${getStatusColor(
                        appointment.status
                    )}`}
                >
                    {appointment.status.charAt(0).toUpperCase() +
                        appointment.status.slice(1)}
                </span>
            </div>

            {/* Appointment Details */}
            <div className="space-y-2 mb-4">
                <div className="flex items-center gap-2 text-sm text-neutral-700">
                    <svg
                        className="w-5 h-5 text-neutral-400"
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
                    <span>{formatDateTime(appointment.scheduledDateTime)}</span>
                </div>

                <div className="flex items-start gap-2 text-sm text-neutral-700">
                    <svg
                        className="w-5 h-5 text-neutral-400 mt-0.5"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                        />
                    </svg>
                    <span>{appointment.visitReason}</span>
                </div>
            </div>

            {/* Swap Status - Show if swap preference is active */}
            {hasSwapPreference && (
                <div className="mt-4 p-3 bg-primary-50 border border-primary-200 rounded-lg">
                    <div className="flex items-start gap-2 mb-3">
                        <svg
                            className="w-5 h-5 text-primary-600 flex-shrink-0 mt-0.5"
                            fill="currentColor"
                            viewBox="0 0 20 20"
                            aria-hidden="true"
                        >
                            <path
                                fillRule="evenodd"
                                d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z"
                                clipRule="evenodd"
                            />
                        </svg>
                        <div className="flex-1">
                            <p className="text-sm font-medium text-primary-900">
                                Preferred slot swap active
                            </p>
                            <p className="text-xs text-primary-700 mt-1">
                                We're monitoring an earlier time slot. You'll be automatically
                                moved if it becomes available.
                            </p>
                        </div>
                    </div>

                    {/* Cancel Swap Button */}
                    {!showCancelConfirm ? (
                        <button
                            type="button"
                            onClick={() => setShowCancelConfirm(true)}
                            disabled={isCancelingSwap}
                            className="text-xs text-primary-700 hover:text-primary-900 font-medium 
                                     underline focus:outline-none focus:ring-2 focus:ring-primary-500 
                                     focus:ring-offset-2 rounded px-1 transition-colors
                                     disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {isCancelingSwap ? 'Canceling...' : 'Cancel swap preference'}
                        </button>
                    ) : (
                        <div className="flex items-center gap-2">
                            <p className="text-xs text-neutral-700">
                                Cancel swap preference?
                            </p>
                            <button
                                type="button"
                                onClick={handleConfirmCancelSwap}
                                disabled={isCancelingSwap}
                                className="text-xs px-2 py-1 bg-error text-neutral-0 rounded 
                                         hover:bg-error-dark focus:outline-none focus:ring-2 
                                         focus:ring-error focus:ring-offset-2 transition-colors
                                         disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                Yes, cancel
                            </button>
                            <button
                                type="button"
                                onClick={() => setShowCancelConfirm(false)}
                                disabled={isCancelingSwap}
                                className="text-xs px-2 py-1 bg-neutral-200 text-neutral-700 
                                         rounded hover:bg-neutral-300 focus:outline-none 
                                         focus:ring-2 focus:ring-neutral-400 focus:ring-offset-2 
                                         transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                No, keep it
                            </button>
                        </div>
                    )}
                </div>
            )}

            {/* Action Buttons - Show for upcoming appointments (US_027) */}
            {showActions &&
                (appointment.status.toLowerCase() === 'scheduled' ||
                    appointment.status.toLowerCase() === 'confirmed') && (
                    <div className="mt-4 pt-4 border-t border-neutral-200 flex gap-2">
                        <button
                            type="button"
                            onClick={() => onReschedule?.(appointment.id)}
                            className="flex-1 px-4 py-2 bg-primary-500 text-neutral-0 rounded-lg
                                 hover:bg-primary-600 focus:outline-none focus:ring-2 
                                 focus:ring-primary-500 focus:ring-offset-2 
                                 transition-colors text-sm font-medium"
                        >
                            Reschedule
                        </button>
                        <button
                            type="button"
                            onClick={() => onCancel?.(appointment.id)}
                            className="flex-1 px-4 py-2 bg-neutral-200 text-neutral-700 rounded-lg
                                 hover:bg-neutral-300 focus:outline-none focus:ring-2 
                                 focus:ring-neutral-400 focus:ring-offset-2 
                                 transition-colors text-sm font-medium"
                        >
                            Cancel
                        </button>
                    </div>
                )}
        </div>
    );
}
