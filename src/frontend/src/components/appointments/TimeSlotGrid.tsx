/**
 * TimeSlotGrid Component for US_024 - Appointment Booking Calendar
 * Displays available time slots for selected date (AC-1, FR-007)
 * Handles time slot selection and loading states
 */

import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../store';
import {
    fetchDailyTimeSlots,
    setSelectedTimeSlot,
} from '../../store/slices/appointmentSlice';
import type { TimeSlot } from '../../types/appointment';

/**
 * TimeSlotGrid displays available time slots for selected date (AC-1, FR-007)
 */
export function TimeSlotGrid() {
    const dispatch = useDispatch<AppDispatch>();
    const {
        selectedProviderId,
        selectedDate,
        selectedTimeSlot,
        dailyTimeSlots,
        isLoadingTimeSlots,
    } = useSelector((state: RootState) => state.appointments);

    const [showTimeout, setShowTimeout] = useState(false);

    /**
     * Fetch time slots when date changes
     */
    useEffect(() => {
        if (selectedProviderId && selectedDate) {
             
            setShowTimeout(false);

            dispatch(
                fetchDailyTimeSlots({
                    providerId: selectedProviderId,
                    date: selectedDate,
                })
            );

            // Set timeout for 10 seconds (Edge Case: timeout handling)
            const timeoutId = setTimeout(() => {
                 
                if (isLoadingTimeSlots) {
                    setShowTimeout(true);
                }
            }, 10000);

            return () => clearTimeout(timeoutId);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedProviderId, selectedDate, dispatch]);

    /**
     * Handle time slot selection
     */
    const handleSlotClick = (slot: TimeSlot) => {
        if (slot.status === 'available') {
            dispatch(setSelectedTimeSlot(slot));
        }
    };

    /**
     * Format time slot time for display
     */
    const formatSlotTime = (isoString: string): string => {
        const date = new Date(isoString);
        return date.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
        });
    };

    /**
     * Format selected date for display
     */
    const formatSelectedDate = (): string => {
        if (!selectedDate) return '';
        const date = new Date(selectedDate);
        return date.toLocaleDateString('en-US', {
            month: 'long',
            day: 'numeric',
        });
    };

    /**
     * Retry loading after timeout
     */
    const handleRetry = () => {
        if (selectedProviderId && selectedDate) {
            setShowTimeout(false);
            dispatch(
                fetchDailyTimeSlots({
                    providerId: selectedProviderId,
                    date: selectedDate,
                })
            );
        }
    };

    /**
     * Show skeleton loading with 300ms delay (UXR-502)
     */
    if (isLoadingTimeSlots && !showTimeout) {
        return (
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
                <div className="h-5 bg-neutral-200 rounded w-1/2 mb-3 animate-pulse" />
                <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
                    {Array.from({ length: 9 }).map((_, i) => (
                        <div
                            key={i}
                            className="h-11 bg-neutral-200 rounded-lg animate-pulse"
                        />
                    ))}
                </div>
            </div>
        );
    }

    /**
     * Show timeout error with retry button (Edge Case)
     */
    if (showTimeout) {
        return (
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
                <div className="text-center py-8">
                    <svg
                        className="w-12 h-12 mx-auto mb-3 text-error"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                        aria-hidden="true"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={1.5}
                            d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                        />
                    </svg>
                    <h5 className="text-base font-medium text-neutral-900 mb-1">
                        Request Timeout
                    </h5>
                    <p className="text-sm text-neutral-500 mb-4">
                        Unable to load time slots. Please try again.
                    </p>
                    <button
                        onClick={handleRetry}
                        className="inline-flex items-center gap-2 px-5 py-2 bg-primary-500 text-neutral-0 
                                 rounded-lg hover:bg-primary-600 focus:outline-none focus:ring-2 
                                 focus:ring-primary-500 focus:ring-offset-2 transition-colors"
                    >
                        Retry
                    </button>
                </div>
            </div>
        );
    }

    /**
     * Show message when no date selected
     */
    if (!selectedDate) {
        return (
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
                <h4 className="text-base font-semibold text-neutral-900 mb-3">
                    Available times
                </h4>
                <div className="text-center py-8">
                    <p className="text-sm text-neutral-500">
                        Select a date to view available time slots
                    </p>
                </div>
            </div>
        );
    }

    /**
     * Show message when no slots available
     */
    if (dailyTimeSlots.length === 0) {
        return (
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
                <h4 className="text-base font-semibold text-neutral-900 mb-3">
                    Available times — {formatSelectedDate()}
                </h4>
                <div className="text-center py-8">
                    <p className="text-sm text-neutral-500">
                        No time slots available for this date
                    </p>
                </div>
            </div>
        );
    }

    return (
        <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
            <h4 className="text-base font-semibold text-neutral-900 mb-3">
                Available times — {formatSelectedDate()}
            </h4>

            <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
                {dailyTimeSlots.map((slot) => {
                    const isSelected =
                        selectedTimeSlot && selectedTimeSlot.id === slot.id;
                    const isAvailable = slot.status === 'available';
                    const isBooked = slot.status === 'booked';

                    return (
                        <button
                            key={slot.id}
                            onClick={() => handleSlotClick(slot)}
                            disabled={!isAvailable}
                            className={`
                                h-11 px-3 rounded-lg text-sm font-medium transition-all duration-200
                                flex items-center justify-center focus:outline-none focus:ring-2 
                                focus:ring-primary-500 focus:ring-offset-2
                                ${isSelected
                                    ? 'bg-primary-500 text-neutral-0 border-2 border-primary-500'
                                    : ''
                                }
                                ${isAvailable && !isSelected
                                    ? 'bg-primary-50 text-primary-700 border border-primary-200 hover:bg-primary-100 hover:border-primary-300 cursor-pointer'
                                    : ''
                                }
                                ${!isAvailable
                                    ? 'bg-neutral-100 text-neutral-400 border border-neutral-200 cursor-not-allowed'
                                    : ''
                                }
                            `}
                            aria-label={`${formatSlotTime(slot.startTime)}${isBooked ? ' (booked)' : ''}${isSelected ? ' (selected)' : ''}`}
                            aria-pressed={isSelected ? 'true' : undefined}
                        >
                            {formatSlotTime(slot.startTime)}
                        </button>
                    );
                })}
            </div>
        </div>
    );
}
