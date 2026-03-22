/**
 * CancelConfirmationModal Component for US_027 - Cancel and Reschedule Appointments
 * Displays cancellation policy and confirmation dialog (AC-1, AC-4)
 */

import type { Appointment } from '../../types/appointment';

interface CancelConfirmationModalProps {
    isOpen: boolean;
    appointment: Appointment | null;
    onConfirm: () => void;
    onCancel: () => void;
    isCanceling: boolean;
    cancellationPolicy?: string;
}

/**
 * Modal for confirming appointment cancellation with policy notice
 */
export function CancelConfirmationModal({
    isOpen,
    appointment,
    onConfirm,
    onCancel,
    isCanceling,
    cancellationPolicy = 'Appointments must be cancelled at least 24 hours in advance.',
}: CancelConfirmationModalProps) {
    if (!isOpen || !appointment) return null;

    /**
     * Format date and time for display
     */
    const formatDateTime = (isoDateString: string): string => {
        const date = new Date(isoDateString);
        return date.toLocaleString('en-US', {
            weekday: 'long',
            month: 'long',
            day: 'numeric',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
        });
    };

    return (
        <>
            {/* Backdrop */}
            <div
                className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
                onClick={onCancel}
                aria-hidden="true"
            />

            {/* Modal */}
            <div
                className="fixed inset-0 z-50 flex items-center justify-center p-4"
                role="dialog"
                aria-modal="true"
                aria-labelledby="cancel-modal-title"
            >
                <div className="bg-neutral-0 rounded-lg shadow-xl max-w-md w-full p-6">
                    {/* Header */}
                    <div className="flex items-start gap-3 mb-4">
                        <div className="flex-shrink-0 w-10 h-10 rounded-full bg-warning-light flex items-center justify-center">
                            <svg
                                className="w-6 h-6 text-warning"
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                                />
                            </svg>
                        </div>
                        <div className="flex-1">
                            <h3
                                id="cancel-modal-title"
                                className="text-lg font-semibold text-neutral-900"
                            >
                                Cancel Appointment?
                            </h3>
                            <p className="text-sm text-neutral-600 mt-1">
                                This action cannot be undone.
                            </p>
                        </div>
                    </div>

                    {/* Appointment Details */}
                    <div className="mb-4 p-3 bg-neutral-50 rounded-lg border border-neutral-200">
                        <p className="text-sm font-medium text-neutral-900">
                            {appointment.providerName}
                        </p>
                        <p className="text-xs text-neutral-600 mt-1">
                            {formatDateTime(appointment.scheduledDateTime)}
                        </p>
                        <p className="text-xs text-neutral-600 mt-1">
                            Reason: {appointment.visitReason}
                        </p>
                    </div>

                    {/* Cancellation Policy Notice (AC-4) */}
                    <div className="mb-6 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                        <div className="flex items-start gap-2">
                            <svg
                                className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5"
                                fill="currentColor"
                                viewBox="0 0 20 20"
                                aria-hidden="true"
                            >
                                <path
                                    fillRule="evenodd"
                                    d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                                    clipRule="evenodd"
                                />
                            </svg>
                            <div className="flex-1">
                                <p className="text-xs font-medium text-blue-900">
                                    Cancellation Policy
                                </p>
                                <p className="text-xs text-blue-800 mt-1">
                                    {cancellationPolicy}
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Actions */}
                    <div className="flex gap-3">
                        <button
                            type="button"
                            onClick={onCancel}
                            disabled={isCanceling}
                            className="flex-1 px-4 py-2 bg-neutral-200 text-neutral-700 rounded-lg
                                     hover:bg-neutral-300 focus:outline-none focus:ring-2 
                                     focus:ring-neutral-400 focus:ring-offset-2 
                                     transition-colors text-sm font-medium
                                     disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Keep Appointment
                        </button>
                        <button
                            type="button"
                            onClick={onConfirm}
                            disabled={isCanceling}
                            className="flex-1 px-4 py-2 bg-error text-neutral-0 rounded-lg
                                     hover:bg-error-dark focus:outline-none focus:ring-2 
                                     focus:ring-error focus:ring-offset-2 
                                     transition-colors text-sm font-medium
                                     disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {isCanceling ? 'Canceling...' : 'Yes, Cancel'}
                        </button>
                    </div>
                </div>
            </div>
        </>
    );
}
