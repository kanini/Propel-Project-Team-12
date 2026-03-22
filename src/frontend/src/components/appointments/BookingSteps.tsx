/**
 * BookingSteps Component for US_024 - Appointment Booking Calendar
 * Orchestrates the 4-step booking wizard with navigation (AC-5)
 * Handles step validation and navigation controls
 */

import { useSelector, useDispatch } from 'react-redux';
import type { AppDispatch, RootState } from '../../store';
import {
    previousStep,
    nextStep,
    submitBooking,
    clearBookingError,
} from '../../store/slices/appointmentSlice';
import { CalendarView } from './CalendarView';
import { TimeSlotGrid } from './TimeSlotGrid';
import { VisitReasonForm } from './VisitReasonForm';
import { ConfirmationDialog } from './ConfirmationDialog';
import { useState, useEffect } from 'react';

/**
 * BookingSteps orchestrates the booking wizard flow (AC-5)
 */
export function BookingSteps() {
    const dispatch = useDispatch<AppDispatch>();
    const {
        currentStep,
        selectedProviderId,
        selectedDate,
        selectedTimeSlot,
        visitReason,
        preferredSlotId,
        isBooking,
        bookingError,
    } = useSelector((state: RootState) => state.appointments);

    const [showErrorToast, setShowErrorToast] = useState(false);

    /**
     * Handle booking error (AC-4: Concurrent booking conflict)
     */
    useEffect(() => {
        if (bookingError) {
            // eslint-disable-next-line react-hooks/set-state-in-effect
            setShowErrorToast(true);

            // Auto-hide toast after 5 seconds
            const timer = setTimeout(() => {
                setShowErrorToast(false);
                dispatch(clearBookingError());
            }, 5000);

            // If conflict error, refresh calendar automatically (AC-4)
            if (bookingError.code === 'conflict' && selectedProviderId && selectedDate) {
                // Calendar will auto-refresh via useEffect in CalendarView
            }

            return () => clearTimeout(timer);
        }
    }, [bookingError, selectedProviderId, selectedDate, dispatch]);

    /**
     * Handle back button navigation
     */
    const handleBack = () => {
        dispatch(previousStep());
    };

    /**
     * Handle next button navigation with validation
     */
    const handleNext = () => {
        if (validateCurrentStep()) {
            dispatch(nextStep());
        }
    };

    /**
     * Validate current step before proceeding
     */
    const validateCurrentStep = (): boolean => {
        switch (currentStep) {
            case 1:
                return selectedProviderId !== null;
            case 2:
                return selectedDate !== null && selectedTimeSlot !== null;
            case 3:
                return visitReason.trim().length > 0;
            default:
                return true;
        }
    };

    /**
     * Handle final booking submission (AC-3)
     */
    const handleConfirmBooking = () => {
        if (!selectedProviderId || !selectedTimeSlot) return;

        dispatch(
            submitBooking({
                providerId: selectedProviderId,
                timeSlotId: selectedTimeSlot.id,
                visitReason: visitReason.trim(),
                preferredSlotId: preferredSlotId || undefined,
            })
        );
    };

    /**
     * Get button text based on step
     */
    const getNextButtonText = (): string => {
        if (currentStep === 3) return 'Confirm Booking';
        return 'Next';
    };

    /**
     * Check if next button should be disabled
     */
    const isNextDisabled = (): boolean => {
        return !validateCurrentStep() || isBooking;
    };

    /**
     * Render error toast (AC-4: Conflict handling)
     */
    const renderErrorToast = () => {
        if (!showErrorToast || !bookingError) return null;

        const getErrorIcon = () => {
            if (bookingError.code === 'conflict') {
                return (
                    <svg
                        className="w-5 h-5 text-error"
                        fill="currentColor"
                        viewBox="0 0 20 20"
                        aria-hidden="true"
                    >
                        <path
                            fillRule="evenodd"
                            d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                            clipRule="evenodd"
                        />
                    </svg>
                );
            }
            return (
                <svg
                    className="w-5 h-5 text-warning"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                    aria-hidden="true"
                >
                    <path
                        fillRule="evenodd"
                        d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                        clipRule="evenodd"
                    />
                </svg>
            );
        };

        return (
            <div
                className="fixed top-4 right-4 z-50 max-w-md bg-neutral-0 border-l-4 
                         border-error rounded-lg shadow-lg p-4 animate-slide-in"
                role="alert"
                aria-live="assertive"
            >
                <div className="flex items-start gap-3">
                    {getErrorIcon()}
                    <div className="flex-1">
                        <h5 className="text-sm font-semibold text-neutral-900 mb-1">
                            {bookingError.code === 'conflict'
                                ? 'Slot No Longer Available'
                                : 'Booking Error'}
                        </h5>
                        <p className="text-sm text-neutral-600">
                            {bookingError.message}
                        </p>
                        {bookingError.code === 'conflict' && (
                            <p className="text-xs text-neutral-500 mt-1">
                                Calendar has been refreshed. Please select another slot.
                            </p>
                        )}
                    </div>
                    <button
                        onClick={() => {
                            setShowErrorToast(false);
                            dispatch(clearBookingError());
                        }}
                        className="text-neutral-400 hover:text-neutral-600 focus:outline-none 
                                 focus:ring-2 focus:ring-primary-500 rounded"
                        aria-label="Close notification"
                    >
                        <svg
                            className="w-5 h-5"
                            fill="currentColor"
                            viewBox="0 0 20 20"
                        >
                            <path
                                fillRule="evenodd"
                                d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                                clipRule="evenodd"
                            />
                        </svg>
                    </button>
                </div>
            </div>
        );
    };

    /**
     * Render current step content
     */
    const renderStepContent = () => {
        switch (currentStep) {
            case 1:
                // Provider selection handled by provider browser page
                return null;
            case 2:
                // Date/Time selection
                return (
                    <div className="space-y-4">
                        <CalendarView />
                        <TimeSlotGrid />
                    </div>
                );
            case 3:
                // Details (visit reason and preferred swap)
                return <VisitReasonForm />;
            case 4:
                // Confirmation
                return <ConfirmationDialog />;
            default:
                return null;
        }
    };

    /**
     * Render navigation buttons (AC-5)
     */
    const renderNavigationButtons = () => {
        // No buttons on Step 4 (Confirmation)
        if (currentStep === 4) return null;

        return (
            <div className="flex gap-3 mt-6">
                {/* Back button (disabled on step 1, hidden on step 4) */}
                {currentStep > 1 && (
                    <button
                        onClick={handleBack}
                        disabled={isBooking}
                        className="inline-flex items-center justify-center h-11 px-6 
                                 border border-neutral-300 rounded-lg text-sm font-medium 
                                 text-neutral-700 bg-neutral-0 hover:bg-neutral-50 
                                 focus:outline-none focus:ring-2 focus:ring-primary-500 
                                 focus:ring-offset-2 transition-colors disabled:opacity-50 
                                 disabled:cursor-not-allowed"
                    >
                        Back
                    </button>
                )}

                {/* Next/Confirm button */}
                <button
                    onClick={currentStep === 3 ? handleConfirmBooking : handleNext}
                    disabled={isNextDisabled()}
                    className={`
                        flex-1 inline-flex items-center justify-center h-11 px-6 
                        rounded-lg text-sm font-medium transition-colors
                        focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2
                        ${isNextDisabled()
                            ? 'bg-neutral-300 text-neutral-500 cursor-not-allowed'
                            : 'bg-primary-500 text-neutral-0 hover:bg-primary-600'
                        }
                    `}
                >
                    {isBooking ? (
                        <>
                            <svg
                                className="animate-spin -ml-1 mr-2 h-4 w-4"
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
                            Processing...
                        </>
                    ) : (
                        getNextButtonText()
                    )}
                </button>
            </div>
        );
    };

    return (
        <>
            {/* Error toast */}
            {renderErrorToast()}

            {/* Step content */}
            <div className="mb-6">{renderStepContent()}</div>

            {/* Navigation buttons */}
            {renderNavigationButtons()}
        </>
    );
}
