/**
 * AppointmentBooking Page for US_024 - Appointment Booking Calendar
 * Main page component for multi-step appointment booking wizard (AC-5)
 * Integrates all booking components with progress indicator
 */

import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../store';
import { setSelectedProvider, resetBooking } from '../store/slices/appointmentSlice';
import { ProgressIndicator } from '../components/appointments/ProgressIndicator';
import { BookingSteps } from '../components/appointments/BookingSteps';

/**
 * AppointmentBooking is the main booking wizard page (AC-5, AC-1)
 * Manages 4-step booking flow: Provider → Date/Time → Details → Confirm
 */
export default function AppointmentBooking() {
    const { providerId } = useParams<{ providerId: string }>();
    const navigate = useNavigate();
    const dispatch = useDispatch<AppDispatch>();

    const { 
        selectedProviderId, 
        selectedProviderName, 
        selectedProviderSpecialty, 
        currentStep, 
        selectedDate, 
        selectedTimeSlot
    } = useSelector(
        (state: RootState) => state.appointments
    );

    /**
     * Handle provider selection and reset when switching providers
     * Only triggers when providerId or selectedProviderId changes, not on booking completion
     */
    useEffect(() => {
        if (!providerId) {
            navigate('/providers');
            return;
        }

        // Reset if user is switching to a DIFFERENT provider
        const isSelectingNewProvider = selectedProviderId && selectedProviderId !== providerId;
        
        if (isSelectingNewProvider) {
            dispatch(resetBooking());
        }

        // If provider not set or different provider, fetch and set
        if (!selectedProviderId || selectedProviderId !== providerId) {
            // TODO: Fetch provider details from API
            // For now, set placeholder data
            // This would be replaced with actual API call in task_002_be_appointment_booking_api
            dispatch(
                setSelectedProvider({
                    id: providerId,
                    name: 'Dr. Sarah Chen', // Placeholder
                    specialty: 'Family Medicine', // Placeholder
                })
            );
        }
    }, [providerId, selectedProviderId, dispatch, navigate]);

    /**
     * Clean up booking state when component unmounts and user has completed booking
     * This ensures fresh state when returning to booking flow from other pages
     */
    useEffect(() => {
        return () => {
            // Only reset on unmount if user completed a booking (step 4)
            // This prevents resetting during in-progress bookings
            if (currentStep === 4) {
                dispatch(resetBooking());
            }
        };
    }, [currentStep, dispatch]);

    /**
     * Render booking summary sidebar (matches wireframe)
     */
    const renderBookingSummary = () => {
        // Don't show summary on confirmation step
        if (currentStep === 4) return null;

        return (
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6 sticky top-8">
                <h3 className="text-lg font-semibold text-neutral-900 mb-4">
                    Booking Summary
                </h3>

                <div className="space-y-3">
                    {/* Provider */}
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Provider</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {selectedProviderName || '—'}
                        </span>
                    </div>

                    {/* Specialty */}
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Specialty</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {selectedProviderSpecialty || '—'}
                        </span>
                    </div>

                    {/* Date */}
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Date</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {selectedDate
                                ? new Date(selectedDate).toLocaleDateString('en-US', {
                                    month: 'short',
                                    day: 'numeric',
                                    year: 'numeric',
                                })
                                : '—'}
                        </span>
                    </div>

                    {/* Time */}
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Time</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {selectedTimeSlot
                                ? new Date(selectedTimeSlot.startTime).toLocaleTimeString(
                                    'en-US',
                                    {
                                        hour: 'numeric',
                                        minute: '2-digit',
                                        hour12: true,
                                    }
                                )
                                : '—'}
                        </span>
                    </div>

                    {/* Type */}
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Type</span>
                        <span className="text-sm font-medium text-neutral-800">
                            In-person
                        </span>
                    </div>

                    {/* Status */}
                    <div className="flex justify-between py-2">
                        <span className="text-sm text-neutral-500">Status</span>
                        <span className="text-sm font-medium">
                            <span
                                className="inline-flex items-center px-2 py-0.5 rounded-full 
                                         text-xs font-medium bg-success-light text-success-dark"
                            >
                                Available
                            </span>
                        </span>
                    </div>
                </div>
            </div>
        );
    };

    return (
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {/* Page header */}
            <div className="mb-6">
                <h1 className="text-2xl font-bold text-neutral-900">Book an appointment</h1>
            </div>

            {/* Progress indicator (UXR-101) */}
            <ProgressIndicator currentStep={currentStep} />

            {/* Booking layout: main content + summary sidebar */}
            <div className="grid grid-cols-1 lg:grid-cols-[1fr_340px] gap-6">
                {/* Main booking content */}
                <div>
                    <BookingSteps />
                </div>

                {/* Booking summary sidebar (hidden on mobile, shown on desktop) */}
                <div className="hidden lg:block">{renderBookingSummary()}</div>
            </div>

            {/* Mobile summary (shown below content on mobile) */}
            <div className="lg:hidden mt-6">{renderBookingSummary()}</div>
        </div>
    );
}
