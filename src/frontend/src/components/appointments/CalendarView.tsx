/**
 * CalendarView Component for US_024 - Appointment Booking Calendar
 * Displays monthly calendar with available/unavailable dates (AC-1, FR-007)
 * Handles month navigation and date selection
 * Updated for US_025 - Join Waitlist integration
 */

import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../store';
import {
    fetchMonthlyAvailability,
    setSelectedDate,
    setCurrentMonth,
} from '../../store/slices/appointmentSlice';
import { openEnrollmentModal } from '../../store/slices/waitlistSlice';

/**
 * CalendarView displays monthly availability calendar (AC-1, FR-007)
 */
export function CalendarView() {
    const dispatch = useDispatch<AppDispatch>();
    const {
        selectedProviderId,
        selectedProviderName,
        selectedProviderSpecialty,
        selectedDate,
        currentMonth,
        monthlyAvailability,
        isLoadingAvailability,
    } = useSelector((state: RootState) => state.appointments);

    const [showNoAvailability, setShowNoAvailability] = useState(false);

    /**
     * Fetch availability when month or provider changes
     */
    useEffect(() => {
        if (selectedProviderId && currentMonth) {
            dispatch(
                fetchMonthlyAvailability({
                    providerId: selectedProviderId,
                    month: currentMonth,
                })
            );
        }
    }, [selectedProviderId, currentMonth, dispatch]);

    /**
     * Check for no availability in future months (Edge Case)
     */
    useEffect(() => {
        if (
            monthlyAvailability &&
            monthlyAvailability.availableDates.length === 0
        ) {
            // eslint-disable-next-line react-hooks/set-state-in-effect
            setShowNoAvailability(true);
        } else {
             
            setShowNoAvailability(false);
        }
    }, [monthlyAvailability]);

    /**
     * Navigate to previous month
     */
    const handlePreviousMonth = () => {
        const date = new Date(currentMonth + '-01');
        date.setMonth(date.getMonth() - 1);
        const newMonth = date.toISOString().slice(0, 7);
        dispatch(setCurrentMonth(newMonth));
    };

    /**
     * Navigate to next month
     */
    const handleNextMonth = () => {
        const date = new Date(currentMonth + '-01');
        date.setMonth(date.getMonth() + 1);
        const newMonth = date.toISOString().slice(0, 7);
        dispatch(setCurrentMonth(newMonth));
    };

    /**
     * Handle date selection
     */
    const handleDateClick = (dateString: string) => {
        if (isDateAvailable(dateString)) {
            dispatch(setSelectedDate(dateString));
        }
    };

    /**
     * Check if date is available
     */
    const isDateAvailable = (dateString: string): boolean => {
        if (!monthlyAvailability) return false;
        return monthlyAvailability.availableDates.includes(dateString);
    };

    /**
     * Check if date is today
     */
    const isToday = (dateString: string): boolean => {
        const today = new Date();
        return dateString === today.toISOString().slice(0, 10);
    };

    /**
     * Get calendar month display name
     */
    const getMonthDisplayName = (): string => {
        const date = new Date(currentMonth + '-01');
        return date.toLocaleDateString('en-US', {
            month: 'long',
            year: 'numeric',
        });
    };

    /**
     * Generate calendar grid with dates
     */
    const generateCalendarDays = (): (string | null)[] => {
        const date = new Date(currentMonth + '-01');
        const year = date.getFullYear();
        const month = date.getMonth();

        // Get first day of month (0-6, Sunday-Saturday)
        const firstDay = new Date(year, month, 1).getDay();

        // Get number of days in month
        const daysInMonth = new Date(year, month + 1, 0).getDate();

        // Create array with leading nulls for alignment
        const days: (string | null)[] = Array(firstDay).fill(null);

        // Add all days of the month
        for (let day = 1; day <= daysInMonth; day++) {
            const dateString = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            days.push(dateString);
        }

        return days;
    };

    const calendarDays = generateCalendarDays();

    /**
     * Skeleton loading with 300ms delay (UXR-502)
     */
    if (isLoadingAvailability) {
        return (
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
                <div className="h-6 bg-neutral-200 rounded w-1/3 mb-4 animate-pulse" />
                <div className="grid grid-cols-7 gap-2">
                    {Array.from({ length: 35 }).map((_, i) => (
                        <div
                            key={i}
                            className="aspect-square bg-neutral-200 rounded-lg animate-pulse"
                        />
                    ))}
                </div>
            </div>
        );
    }

    /**
     * Render "No availability" message with waitlist CTA (Edge Case)
     */
    if (showNoAvailability) {
        return (
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
                <div className="flex justify-between items-center mb-4">
                    <h4 className="text-lg font-semibold text-neutral-900">
                        {getMonthDisplayName()}
                    </h4>
                    <div className="flex gap-2">
                        <button
                            onClick={handlePreviousMonth}
                            className="w-8 h-8 flex items-center justify-center border border-neutral-200 
                                     rounded-lg hover:bg-neutral-50 focus:outline-none focus:ring-2 
                                     focus:ring-primary-500 focus:ring-offset-2 transition-colors"
                            aria-label="Previous month"
                        >
                            ‹
                        </button>
                        <button
                            onClick={handleNextMonth}
                            className="w-8 h-8 flex items-center justify-center border border-neutral-200 
                                     rounded-lg hover:bg-neutral-50 focus:outline-none focus:ring-2 
                                     focus:ring-primary-500 focus:ring-offset-2 transition-colors"
                            aria-label="Next month"
                        >
                            ›
                        </button>
                    </div>
                </div>

                <div className="text-center py-12">
                    <svg
                        className="w-16 h-16 mx-auto mb-4 text-neutral-300"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                        aria-hidden="true"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={1.5}
                            d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                        />
                    </svg>
                    <h5 className="text-lg font-medium text-neutral-900 mb-2">
                        No availability
                    </h5>
                    <p className="text-sm text-neutral-500 mb-4">
                        This provider has no available slots this month.
                    </p>
                    <button
                        onClick={() =>
                            dispatch(
                                openEnrollmentModal({
                                    providerId: selectedProviderId || undefined,
                                    providerName: selectedProviderName || undefined,
                                    providerSpecialty: selectedProviderSpecialty || undefined,
                                })
                            )
                        }
                        className="inline-flex items-center gap-2 px-6 py-2 bg-primary-500 text-neutral-0 
                                 rounded-lg hover:bg-primary-600 focus:outline-none focus:ring-2 
                                 focus:ring-primary-500 focus:ring-offset-2 transition-colors"
                    >
                        Join Waitlist
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
            {/* Calendar header with month navigation */}
            <div className="flex justify-between items-center mb-4">
                <h4 className="text-lg font-semibold text-neutral-900">{getMonthDisplayName()}</h4>
                <div className="flex gap-2">
                    <button
                        onClick={handlePreviousMonth}
                        className="w-8 h-8 flex items-center justify-center border border-neutral-200 
                                 rounded-lg hover:bg-neutral-50 focus:outline-none focus:ring-2 
                                 focus:ring-primary-500 focus:ring-offset-2 transition-colors"
                        aria-label="Previous month"
                    >
                        ‹
                    </button>
                    <button
                        onClick={handleNextMonth}
                        className="w-8 h-8 flex items-center justify-center border border-neutral-200 
                                 rounded-lg hover:bg-neutral-50 focus:outline-none focus:ring-2 
                                 focus:ring-primary-500 focus:ring-offset-2 transition-colors"
                        aria-label="Next month"
                    >
                        ›
                    </button>
                </div>
            </div>

            {/* Calendar grid */}
            <div className="grid grid-cols-7 gap-1 text-center">
                {/* Day labels */}
                {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map((day) => (
                    <div
                        key={day}
                        className="text-xs font-semibold text-neutral-500 py-2"
                    >
                        {day}
                    </div>
                ))}

                {/* Calendar days */}
                {calendarDays.map((dateString, index) => {
                    if (!dateString) {
                        return <div key={`empty-${index}`} />;
                    }

                    const available = isDateAvailable(dateString);
                    const today = isToday(dateString);
                    const selected = dateString === selectedDate;

                    return (
                        <button
                            key={dateString}
                            onClick={() => handleDateClick(dateString)}
                            disabled={!available}
                            className={`
                                aspect-square rounded-lg text-sm font-normal transition-all duration-200
                                flex items-center justify-center min-h-[44px] focus:outline-none focus:ring-2 
                                focus:ring-primary-500 focus:ring-offset-2
                                ${selected
                                    ? 'bg-primary-500 text-neutral-0 font-semibold'
                                    : ''
                                }
                                ${available && !selected
                                    ? 'hover:bg-primary-50 text-neutral-900 cursor-pointer'
                                    : ''
                                }
                                ${!available
                                    ? 'text-neutral-300 cursor-not-allowed line-through'
                                    : ''
                                }
                                ${today && !selected
                                    ? 'ring-2 ring-primary-500 ring-inset'
                                    : ''
                                }
                            `}
                            aria-label={`${dateString}${today ? ' (today)' : ''}${!available ? ' unavailable' : ''}`}
                            aria-pressed={selected}
                        >
                            {dateString ? parseInt(dateString.split('-')[2] || '0', 10) : ''}
                        </button>
                    );
                })}
            </div>
        </div>
    );
}
