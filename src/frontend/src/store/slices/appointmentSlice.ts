/**
 * Appointment slice for US_024 - Appointment Booking Calendar
 * Manages appointment booking wizard state and async booking operations
 */

import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { RootState } from '../index';
import type {
    TimeSlot,
    BookingRequest,
    BookingConfirmation,
    BookingStep,
    BookingError,
    MonthlyAvailability,
    DailyTimeSlotsResponse,
    Appointment,
} from '../../types/appointment';

/**
 * Appointment slice state interface
 */
interface AppointmentState {
    // Wizard state
    currentStep: BookingStep;
    selectedProviderId: string | null;
    selectedProviderName: string | null;
    selectedProviderSpecialty: string | null;
    selectedDate: string | null; // YYYY-MM-DD format
    selectedTimeSlot: TimeSlot | null;
    visitReason: string;
    preferredSlotId: string | null;
    enablePreferredSwap: boolean;

    // Availability data
    monthlyAvailability: MonthlyAvailability | null;
    dailyTimeSlots: TimeSlot[];
    currentMonth: string; // YYYY-MM format

    // Booking state
    isBooking: boolean;
    bookingConfirmation: BookingConfirmation | null;
    bookingError: BookingError | null;

    // My Appointments (US_027)
    myAppointments: Appointment[];
    isLoadingAppointments: boolean;
    appointmentsError: string | null;

    // Loading states (NFR-001: 500ms target)
    isLoadingAvailability: boolean;
    isLoadingTimeSlots: boolean;
}

/**
 * Initial state for appointment slice
 */
const initialState: AppointmentState = {
    currentStep: 1,
    selectedProviderId: null,
    selectedProviderName: null,
    selectedProviderSpecialty: null,
    selectedDate: null,
    selectedTimeSlot: null,
    visitReason: '',
    preferredSlotId: null,
    enablePreferredSwap: false,
    monthlyAvailability: null,
    dailyTimeSlots: [],
    currentMonth: new Date().toISOString().slice(0, 7), // YYYY-MM
    isBooking: false,
    bookingConfirmation: null,
    bookingError: null,
    myAppointments: [],
    isLoadingAppointments: false,
    appointmentsError: null,
    isLoadingAvailability: false,
    isLoadingTimeSlots: false,
};

/**
 * Async thunk for fetching monthly availability (AC-2, NFR-001: 500ms target)
 */
export const fetchMonthlyAvailability = createAsyncThunk<
    MonthlyAvailability,
    { providerId: string; month: string },
    { state: RootState; rejectValue: string }
>(
    'appointments/fetchMonthlyAvailability',
    async ({ providerId, month }, { rejectWithValue }) => {
        try {
            const token = localStorage.getItem('token');
            const response = await fetch(
                `/api/providers/${providerId}/availability?month=${month}`,
                {
                    headers: {
                        'Content-Type': 'application/json',
                        ...(token && { 'Authorization': `Bearer ${token}` }),
                    },
                }
            );

            if (!response.ok) {
                throw new Error('Failed to fetch availability');
            }

            const data = await response.json();
            return data;
        } catch (error) {
            return rejectWithValue(
                error instanceof Error ? error.message : 'Failed to fetch availability'
            );
        }
    }
);

/**
 * Async thunk for fetching daily time slots (AC-2, NFR-001: 500ms target)
 */
export const fetchDailyTimeSlots = createAsyncThunk<
    DailyTimeSlotsResponse,
    { providerId: string; date: string },
    { state: RootState; rejectValue: string }
>(
    'appointments/fetchDailyTimeSlots',
    async ({ providerId, date }, { rejectWithValue }) => {
        try {
            const token = localStorage.getItem('token');
            const response = await fetch(
                `/api/providers/${providerId}/availability?date=${date}`,
                {
                    headers: {
                        'Content-Type': 'application/json',
                        ...(token && { 'Authorization': `Bearer ${token}` }),
                    },
                }
            );

            if (!response.ok) {
                throw new Error('Failed to fetch time slots');
            }

            const data = await response.json();
            return data;
        } catch (error) {
            return rejectWithValue(
                error instanceof Error ? error.message : 'Failed to fetch time slots'
            );
        }
    }
);

/**
 * Async thunk for submitting appointment booking (AC-3, AC-4)
 */
export const submitBooking = createAsyncThunk<
    BookingConfirmation,
    BookingRequest,
    { state: RootState; rejectValue: BookingError }
>(
    'appointments/submitBooking',
    async (bookingRequest, { rejectWithValue }) => {
        try {
            const token = localStorage.getItem('token');
            const response = await fetch('/api/appointments', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(token && { 'Authorization': `Bearer ${token}` }),
                },
                body: JSON.stringify(bookingRequest),
            });

            const data = await response.json();

            if (!response.ok) {
                // Handle 409 Conflict (AC-4: Concurrent booking conflict)
                if (response.status === 409) {
                    return rejectWithValue({
                        code: 'conflict',
                        message: 'Slot no longer available',
                    });
                }

                // Handle 400 Validation Error
                if (response.status === 400) {
                    return rejectWithValue({
                        code: 'validation',
                        message: 'Validation failed',
                        details: data.errors || {},
                    });
                }

                // Handle 500 Server Error
                if (response.status === 500) {
                    return rejectWithValue({
                        code: 'server',
                        message: 'Server error occurred. Please try again.',
                    });
                }

                throw new Error('Booking failed');
            }

            return data;
        } catch (error) {
            return rejectWithValue({
                code: 'server',
                message: error instanceof Error ? error.message : 'Booking failed',
            });
        }
    }
);

/**
 * Async thunk for cancelling an appointment (US_027 - FR-011, AC-1, AC-4)
 */
export const cancelAppointment = createAsyncThunk<
    { appointmentId: string },
    string,
    { state: RootState; rejectValue: string }
>(
    'appointments/cancelAppointment',
    async (appointmentId, { rejectWithValue }) => {
        try {
            const response = await fetch(`/api/appointments/${appointmentId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok) {
                const data = await response.json();

                // Handle 403 Forbidden (Policy violation - AC-4)
                if (response.status === 403) {
                    return rejectWithValue(data.message || 'Cancellation not allowed within restricted window');
                }

                // Handle 400 Bad Request
                if (response.status === 400) {
                    return rejectWithValue(data.message || 'Invalid cancellation request');
                }

                throw new Error('Cancellation failed');
            }

            return { appointmentId };
        } catch (error) {
            return rejectWithValue(
                error instanceof Error ? error.message : 'Failed to cancel appointment'
            );
        }
    }
);

/**
 * Async thunk for rescheduling an appointment (US_027 - FR-011, AC-2, AC-3)
 */
export const rescheduleAppointment = createAsyncThunk<
    Appointment,
    { appointmentId: string; newTimeSlotId: string },
    { state: RootState; rejectValue: string }
>(
    'appointments/rescheduleAppointment',
    async ({ appointmentId, newTimeSlotId }, { rejectWithValue }) => {
        try {
            const response = await fetch(`/api/appointments/${appointmentId}/reschedule`, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ newTimeSlotId }),
            });

            const data = await response.json();

            if (!response.ok) {
                // Handle 409 Conflict (AC-3: New slot already booked)
                if (response.status === 409) {
                    return rejectWithValue(data.message || 'Selected slot is no longer available');
                }

                // Handle 403 Forbidden
                if (response.status === 403) {
                    return rejectWithValue(data.message || 'Unauthorized to reschedule this appointment');
                }

                // Handle 400 Bad Request
                if (response.status === 400) {
                    return rejectWithValue(data.message || 'Invalid reschedule request');
                }

                throw new Error('Reschedule failed');
            }

            return data;
        } catch (error) {
            return rejectWithValue(
                error instanceof Error ? error.message : 'Failed to reschedule appointment'
            );
        }
    }
);

/**
 * Async thunk for fetching user's appointments (US_027)
 */
export const fetchMyAppointments = createAsyncThunk<
    { upcoming: Appointment[]; past: Appointment[] },
    void,
    { state: RootState; rejectValue: string }
>(
    'appointments/fetchMyAppointments',
    async (_, { rejectWithValue }) => {
        try {
            const response = await fetch('/api/appointments/my-appointments', {
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok) {
                throw new Error('Failed to fetch appointments');
            }

            const data = await response.json();
            return data;
        } catch (error) {
            return rejectWithValue(
                error instanceof Error ? error.message : 'Failed to fetch appointments'
            );
        }
    }
);

/**
 * Async thunk for downloading appointment confirmation PDF (US_028 - FR-012, AC-4)
 * Downloads PDF from GET /api/appointments/{id}/confirmation-pdf endpoint
 */
export const downloadConfirmationPDF = createAsyncThunk<
    void,
    { appointmentId: string; confirmationNumber: string },
    { state: RootState; rejectValue: string }
>(
    'appointments/downloadConfirmationPDF',
    async ({ appointmentId, confirmationNumber }, { rejectWithValue }) => {
        try {
            const response = await fetch(`/api/appointments/${appointmentId}/confirmation-pdf`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok) {
                // Handle 404 - PDF not available yet (still generating)
                if (response.status === 404) {
                    return rejectWithValue('PDF not available yet. Please try again in a moment.');
                }

                // Handle 403 - Unauthorized
                if (response.status === 403) {
                    return rejectWithValue('You do not have permission to download this PDF.');
                }

                throw new Error('Failed to download PDF');
            }

            // Get PDF blob from response
            const blob = await response.blob();

            // Create a download link and trigger download
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `Appointment_Confirmation_${confirmationNumber}.pdf`;
            document.body.appendChild(link);
            link.click();

            // Cleanup
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);

            return;
        } catch (error) {
            return rejectWithValue(
                error instanceof Error ? error.message : 'Failed to download PDF'
            );
        }
    }
);

/**
 * Appointment slice with reducers and actions
 */
const appointmentSlice = createSlice({
    name: 'appointments',
    initialState,
    reducers: {
        /**
         * Initialize booking with provider selection (Step 1)
         */
        setSelectedProvider: (
            state,
            action: PayloadAction<{
                id: string;
                name: string;
                specialty: string;
            }>
        ) => {
            state.selectedProviderId = action.payload.id;
            state.selectedProviderName = action.payload.name;
            state.selectedProviderSpecialty = action.payload.specialty;
            state.currentStep = 2; // Move to Date/Time step
        },

        /**
         * Set selected date (Step 2)
         */
        setSelectedDate: (state, action: PayloadAction<string>) => {
            state.selectedDate = action.payload;
            state.selectedTimeSlot = null; // Reset time slot when date changes
        },

        /**
         * Set selected time slot (Step 2)
         */
        setSelectedTimeSlot: (state, action: PayloadAction<TimeSlot>) => {
            state.selectedTimeSlot = action.payload;
        },

        /**
         * Set visit reason (Step 3)
         */
        setVisitReason: (state, action: PayloadAction<string>) => {
            state.visitReason = action.payload;
        },

        /**
         * Toggle preferred slot swap (Step 3)
         */
        setEnablePreferredSwap: (state, action: PayloadAction<boolean>) => {
            state.enablePreferredSwap = action.payload;
            if (!action.payload) {
                state.preferredSlotId = null;
            }
        },

        /**
         * Set preferred slot for swap (Step 3)
         */
        setPreferredSlotId: (state, action: PayloadAction<string>) => {
            state.preferredSlotId = action.payload;
        },

        /**
         * Navigate to next step
         */
        nextStep: (state) => {
            if (state.currentStep < 4) {
                state.currentStep = (state.currentStep + 1) as BookingStep;
            }
        },

        /**
         * Navigate to previous step
         */
        previousStep: (state) => {
            if (state.currentStep > 1) {
                state.currentStep = (state.currentStep - 1) as BookingStep;
            }
        },

        /**
         * Set current step directly
         */
        setStep: (state, action: PayloadAction<BookingStep>) => {
            state.currentStep = action.payload;
        },

        /**
         * Set current month for calendar navigation
         */
        setCurrentMonth: (state, action: PayloadAction<string>) => {
            state.currentMonth = action.payload;
        },

        /**
         * Clear booking error
         */
        clearBookingError: (state) => {
            state.bookingError = null;
        },

        /**
         * Reset entire booking flow
         */
        resetBooking: (state) => {
            return {
                ...initialState,
                currentMonth: state.currentMonth, // Preserve current month
            };
        },
    },
    extraReducers: (builder) => {
        // Fetch monthly availability
        builder
            .addCase(fetchMonthlyAvailability.pending, (state) => {
                state.isLoadingAvailability = true;
            })
            .addCase(fetchMonthlyAvailability.fulfilled, (state, action) => {
                state.isLoadingAvailability = false;
                state.monthlyAvailability = action.payload;
            })
            .addCase(fetchMonthlyAvailability.rejected, (state) => {
                state.isLoadingAvailability = false;
                state.monthlyAvailability = null;
            });

        // Fetch daily time slots
        builder
            .addCase(fetchDailyTimeSlots.pending, (state) => {
                state.isLoadingTimeSlots = true;
            })
            .addCase(fetchDailyTimeSlots.fulfilled, (state, action) => {
                state.isLoadingTimeSlots = false;
                state.dailyTimeSlots = action.payload.slots;
            })
            .addCase(fetchDailyTimeSlots.rejected, (state) => {
                state.isLoadingTimeSlots = false;
                state.dailyTimeSlots = [];
            });

        // Submit booking
        builder
            .addCase(submitBooking.pending, (state) => {
                state.isBooking = true;
                state.bookingError = null;
            })
            .addCase(submitBooking.fulfilled, (state, action) => {
                state.isBooking = false;
                state.bookingConfirmation = action.payload;
                state.currentStep = 4; // Move to confirmation step
            })
            .addCase(submitBooking.rejected, (state, action) => {
                state.isBooking = false;
                state.bookingError = action.payload || {
                    code: 'server',
                    message: 'An error occurred',
                };
            });

        // Cancel appointment (US_027)
        builder
            .addCase(cancelAppointment.pending, (_state) => {
                // Handled in component local state
            })
            .addCase(cancelAppointment.fulfilled, (_state) => {
                // Success handled in component with toast notification
            })
            .addCase(cancelAppointment.rejected, (_state) => {
                // Error handled in component with toast notification
            });

        // Reschedule appointment (US_027)
        builder
            .addCase(rescheduleAppointment.pending, (_state) => {
                // Handled in component local state
            })
            .addCase(rescheduleAppointment.fulfilled, (_state) => {
                // Success handled in component with toast notification
            })
            .addCase(rescheduleAppointment.rejected, (_state) => {
                // Error handled in component with toast notification
            });

        // Fetch my appointments (US_027)
        builder
            .addCase(fetchMyAppointments.pending, (state) => {
                state.isLoadingAppointments = true;
                state.appointmentsError = null;
            })
            .addCase(fetchMyAppointments.fulfilled, (state, action) => {
                state.isLoadingAppointments = false;
                state.myAppointments = [...action.payload.upcoming, ...action.payload.past];
            })
            .addCase(fetchMyAppointments.rejected, (state, action) => {
                state.isLoadingAppointments = false;
                state.appointmentsError = action.payload || 'Failed to load appointments';
            });
    },
});

export const {
    setSelectedProvider,
    setSelectedDate,
    setSelectedTimeSlot,
    setVisitReason,
    setEnablePreferredSwap,
    setPreferredSlotId,
    nextStep,
    previousStep,
    setStep,
    setCurrentMonth,
    clearBookingError,
    resetBooking,
} = appointmentSlice.actions;

// Selectors for My Appointments (US_027)
export const selectMyAppointments = (state: RootState) => state.appointments.myAppointments;
export const selectIsLoadingAppointments = (state: RootState) => state.appointments.isLoadingAppointments;
export const selectAppointmentsError = (state: RootState) => state.appointments.appointmentsError;

export const selectUpcomingAppointments = (state: RootState) => {
    const now = new Date();
    return state.appointments.myAppointments.filter(
        (apt) => new Date(apt.scheduledDateTime) > now && apt.status !== 'cancelled'
    );
};

export const selectPastAppointments = (state: RootState) => {
    const now = new Date();
    return state.appointments.myAppointments.filter(
        (apt) => new Date(apt.scheduledDateTime) <= now || apt.status === 'cancelled' || apt.status === 'completed'
    );
};

export default appointmentSlice.reducer;
