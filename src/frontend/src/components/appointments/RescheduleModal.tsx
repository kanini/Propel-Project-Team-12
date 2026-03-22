/**
 * RescheduleModal Component for US_027 - Cancel and Reschedule Appointments
 * Displays available alternative slots for rescheduling (AC-2, AC-3)
 */

import { useState, useEffect } from 'react';
import type { Appointment, TimeSlot } from '../../types/appointment';

interface RescheduleModalProps {
    isOpen: boolean;
    appointment: Appointment | null;
    availableSlots: TimeSlot[];
    onConfirm: (newTimeSlotId: string) => void;
    onCancel: () => void;
    isRescheduling: boolean;
    isLoadingSlots: boolean;
}

/**
 * Modal for rescheduling appointment with alternative slot selection
 */
export function RescheduleModal({
    isOpen,
    appointment,
    availableSlots,
    onConfirm,
    onCancel,
    isRescheduling,
    isLoadingSlots,
}: RescheduleModalProps) {
    const [selectedSlotId, setSelectedSlotId] = useState<string | null>(null);

    // Reset selection when modal opens
    useEffect(() => {
        if (isOpen) {
            setSelectedSlotId(null);
        }
    }, [isOpen]);

    if (!isOpen || !appointment) return null;

    /**
     * Format time for display (e.g., "2:00 PM")
     */
    const formatTime = (isoDateString: string): string => {
        const date = new Date(isoDateString);
        return date.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
        });
    };

    /**
     * Format date for display (e.g., "Monday, March 25, 2026")
     */
    const formatDate = (isoDateString: string): string => {
        const date = new Date(isoDateString);
        return date.toLocaleDateString('en-US', {
            weekday: 'long',
            month: 'long',
            day: 'numeric',
            year: 'numeric',
        });
    };

    /**
     * Group slots by date
     */
    const groupedSlots = availableSlots.reduce((groups, slot) => {
        const dateKey = new Date(slot.startTime).toDateString();
        if (!groups[dateKey]) {
            groups[dateKey] = [];
        }
        groups[dateKey].push(slot);
        return groups;
    }, {} as Record<string, TimeSlot[]>);

    const handleConfirm = () => {
        if (selectedSlotId) {
            onConfirm(selectedSlotId);
        }
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
                aria-labelledby="reschedule-modal-title"
            >
                <div className="bg-neutral-0 rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-hidden flex flex-col">
                    {/* Header */}
                    <div className="p-6 border-b border-neutral-200">
                        <h3
                            id="reschedule-modal-title"
                            className="text-lg font-semibold text-neutral-900"
                        >
                            Reschedule Appointment
                        </h3>
                        <p className="text-sm text-neutral-600 mt-1">
                            Select a new time with {appointment.providerName}
                        </p>

                        {/* Current Appointment */}
                        <div className="mt-3 p-3 bg-neutral-50 rounded-lg border border-neutral-200">
                            <p className="text-xs text-neutral-500">Current appointment</p>
                            <p className="text-sm font-medium text-neutral-900 mt-1">
                                {formatDate(appointment.scheduledDateTime)} at{' '}
                                {formatTime(appointment.scheduledDateTime)}
                            </p>
                        </div>
                    </div>

                    {/* Available Slots */}
                    <div className="flex-1 overflow-y-auto p-6">
                        {isLoadingSlots ? (
                            <div className="flex items-center justify-center py-12">
                                <div className="text-center">
                                    <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-500 mx-auto"></div>
                                    <p className="text-sm text-neutral-600 mt-3">
                                        Loading available slots...
                                    </p>
                                </div>
                            </div>
                        ) : Object.keys(groupedSlots).length === 0 ? (
                            <div className="text-center py-12">
                                <svg
                                    className="w-16 h-16 text-neutral-300 mx-auto mb-3"
                                    fill="none"
                                    stroke="currentColor"
                                    viewBox="0 0 24 24"
                                >
                                    <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth={2}
                                        d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                                    />
                                </svg>
                                <p className="text-sm text-neutral-600">
                                    No available slots found for this provider.
                                </p>
                                <p className="text-xs text-neutral-500 mt-1">
                                    Please try again later or contact support.
                                </p>
                            </div>
                        ) : (
                            <div className="space-y-6">
                                {Object.entries(groupedSlots).map(([dateKey, slots]) => {
                                    const firstSlot = slots[0];
                                    if (!firstSlot) return null;

                                    return (
                                        <div key={dateKey}>
                                            <h4 className="text-sm font-semibold text-neutral-900 mb-3">
                                                {formatDate(firstSlot.startTime)}
                                            </h4>
                                            <div className="grid grid-cols-3 sm:grid-cols-4 gap-2">
                                                {slots.map((slot) => {
                                                    const isSelected = selectedSlotId === slot.id;
                                                    return (
                                                        <button
                                                            key={slot.id}
                                                            type="button"
                                                            onClick={() => setSelectedSlotId(slot.id)}
                                                            className={`
                                                                px-3 py-2 text-sm border rounded-lg
                                                                transition-all duration-200
                                                                focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2
                                                                ${isSelected
                                                                    ? 'bg-primary-500 text-neutral-0 border-primary-600 shadow-md'
                                                                    : 'bg-neutral-0 text-neutral-700 border-neutral-300 hover:border-primary-400 hover:bg-primary-50'
                                                                }
                                                            `}
                                                            aria-pressed={isSelected}
                                                        >
                                                            {formatTime(slot.startTime)}
                                                        </button>
                                                    );
                                                })}
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>

                    {/* Footer */}
                    <div className="p-6 border-t border-neutral-200">
                        <div className="flex gap-3">
                            <button
                                type="button"
                                onClick={onCancel}
                                disabled={isRescheduling}
                                className="flex-1 px-4 py-2 bg-neutral-200 text-neutral-700 rounded-lg
                                         hover:bg-neutral-300 focus:outline-none focus:ring-2 
                                         focus:ring-neutral-400 focus:ring-offset-2 
                                         transition-colors text-sm font-medium
                                         disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                Cancel
                            </button>
                            <button
                                type="button"
                                onClick={handleConfirm}
                                disabled={!selectedSlotId || isRescheduling || isLoadingSlots}
                                className="flex-1 px-4 py-2 bg-primary-500 text-neutral-0 rounded-lg
                                         hover:bg-primary-600 focus:outline-none focus:ring-2 
                                         focus:ring-primary-500 focus:ring-offset-2 
                                         transition-colors text-sm font-medium
                                         disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                {isRescheduling ? 'Rescheduling...' : 'Confirm Reschedule'}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}
