/**
 * PreferredSlotSelector Component for US_026 - Dynamic Preferred Slot Swap
 * Allows patients to select an unavailable (booked) slot as their preferred swap target
 * Displays only booked slots for the same provider on the same day or nearby dates
 */

import { useState, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../store';
import { setPreferredSlotId } from '../../store/slices/appointmentSlice';
import type { TimeSlot } from '../../types/appointment';

/**
 * PreferredSlotSelector displays unavailable time slots for swap preference selection
 */
export function PreferredSlotSelector() {
    const dispatch = useDispatch<AppDispatch>();
    const {
        selectedDate,
        selectedTimeSlot,
        preferredSlotId,
        dailyTimeSlots,
    } = useSelector((state: RootState) => state.appointments);

    const unavailableSlots = useMemo(() => {
        if (!selectedTimeSlot) return [];

        return dailyTimeSlots.filter(
            (slot) =>
                slot.status === 'booked' &&
                slot.id !== selectedTimeSlot.id
        );
    }, [dailyTimeSlots, selectedTimeSlot]);

    const [selectedPreferredSlot, setSelectedPreferredSlot] = useState<string | null>(
        preferredSlotId
    );

    /**
     * Handle preferred slot selection
     */
    const handleSlotSelect = (slotId: string) => {
        setSelectedPreferredSlot(slotId);
        dispatch(setPreferredSlotId(slotId));
    };

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
     * Get time range label (e.g., "2:00 PM - 3:00 PM")
     */
    const getTimeRangeLabel = (slot: TimeSlot): string => {
        return `${formatTime(slot.startTime)} - ${formatTime(slot.endTime)}`;
    };

    if (!selectedTimeSlot || !selectedDate) {
        return (
            <div className="mt-4 p-4 bg-neutral-50 border border-neutral-200 rounded-lg">
                <p className="text-sm text-neutral-600">
                    Please select your appointment time first to enable preferred slot
                    swap.
                </p>
            </div>
        );
    }

    if (unavailableSlots.length === 0) {
        return (
            <div className="mt-4 p-4 bg-neutral-50 border border-neutral-200 rounded-lg">
                <div className="flex items-start gap-2">
                    <svg
                        className="w-5 h-5 text-neutral-400 flex-shrink-0 mt-0.5"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                        />
                    </svg>
                    <div>
                        <p className="text-sm font-medium text-neutral-700">
                            No earlier slots currently booked
                        </p>
                        <p className="text-xs text-neutral-500 mt-1">
                            All slots before your selected time are available. You can
                            book one of those directly instead.
                        </p>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="mt-4 p-4 bg-primary-50 border border-primary-200 rounded-lg">
            <div className="mb-3">
                <h4 className="text-sm font-semibold text-neutral-900 mb-1">
                    Select your preferred time
                </h4>
                <p className="text-xs text-neutral-600">
                    Choose a booked time slot. If it becomes available, we'll
                    automatically move your appointment and notify you.
                </p>
            </div>

            {/* Currently selected slot display */}
            <div className="mb-3 p-3 bg-neutral-0 border border-neutral-200 rounded-lg">
                <p className="text-xs text-neutral-500 mb-1">Your current appointment</p>
                <p className="text-sm font-medium text-neutral-900">
                    {selectedDate} at {getTimeRangeLabel(selectedTimeSlot)}
                </p>
            </div>

            {/* Unavailable slots grid */}
            <div className="space-y-2">
                <p className="text-xs font-medium text-neutral-700">
                    Earlier booked slots ({unavailableSlots.length})
                </p>

                <div className="grid grid-cols-2 gap-2 max-h-64 overflow-y-auto">
                    {unavailableSlots.map((slot) => {
                        const isSelected = selectedPreferredSlot === slot.id;

                        return (
                            <button
                                key={slot.id}
                                type="button"
                                onClick={() => handleSlotSelect(slot.id)}
                                className={`
                                    relative px-3 py-2 text-sm border rounded-lg
                                    transition-all duration-200
                                    focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2
                                    ${isSelected
                                        ? 'bg-primary-500 text-neutral-0 border-primary-600 shadow-md'
                                        : 'bg-neutral-0 text-neutral-700 border-neutral-300 hover:border-primary-400 hover:bg-primary-50'
                                    }
                                `}
                                aria-pressed={isSelected}
                            >
                                <div className="flex items-center justify-between">
                                    <span className="font-medium">
                                        {formatTime(slot.startTime)}
                                    </span>
                                    {isSelected && (
                                        <svg
                                            className="w-4 h-4"
                                            fill="currentColor"
                                            viewBox="0 0 20 20"
                                            aria-hidden="true"
                                        >
                                            <path
                                                fillRule="evenodd"
                                                d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                                                clipRule="evenodd"
                                            />
                                        </svg>
                                    )}
                                </div>
                                <p
                                    className={`text-xs mt-1 ${isSelected
                                            ? 'text-primary-100'
                                            : 'text-neutral-500'
                                        }`}
                                >
                                    Currently booked
                                </p>
                            </button>
                        );
                    })}
                </div>
            </div>

            {/* Selection summary */}
            {selectedPreferredSlot && (
                <div className="mt-3 p-3 bg-success-light border border-success rounded-lg">
                    <div className="flex items-start gap-2">
                        <svg
                            className="w-5 h-5 text-success flex-shrink-0 mt-0.5"
                            fill="currentColor"
                            viewBox="0 0 20 20"
                            aria-hidden="true"
                        >
                            <path
                                fillRule="evenodd"
                                d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                                clipRule="evenodd"
                            />
                        </svg>
                        <div>
                            <p className="text-xs font-medium text-success-dark">
                                Preferred slot selected
                            </p>
                            <p className="text-xs text-success-dark mt-1">
                                We'll monitor this slot and automatically move your
                                appointment if it becomes available. You'll receive email
                                and SMS notifications.
                            </p>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
