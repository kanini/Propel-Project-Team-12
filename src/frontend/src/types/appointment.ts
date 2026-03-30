/**
 * Appointment type definitions for US_024 - Appointment Booking Calendar
 * Manages appointment booking state, time slots, and booking flow
 */

/**
 * Time slot entity representing availability window for a provider
 */
export interface TimeSlot {
    id: string;
    providerId: string;
    startTime: string; // ISO datetime string
    endTime: string; // ISO datetime string
    status: 'available' | 'booked' | 'unavailable';
}

/**
 * Appointment booking request payload
 */
export interface BookingRequest {
    providerId: string;
    timeSlotId: string;
    visitReason: string;
    preferredSlotId?: string; // Optional preferred slot for swap (FR-010)
}

/**
 * Appointment entity
 */
export interface Appointment {
    id: string;
    providerId: string;
    providerName: string;
    providerSpecialty: string;
    timeSlotId: string;
    scheduledDateTime: string; // ISO datetime string
    visitReason: string;
    status: 'scheduled' | 'confirmed' | 'arrived' | 'completed' | 'cancelled' | 'no-show';
    preferredSlotId?: string;
    createdAt: string;
    confirmationNumber?: string;
    noShowRiskScore?: number; // 0-100, US_038 - FR-023
    riskLevel?: 'Low' | 'Medium' | 'High'; // US_038 - FR-023
    intakeStatus?: 'pending' | 'inProgress' | 'completed';
}

/**
 * Provider availability response for a specific month
 */
export interface MonthlyAvailability {
    providerId: string;
    month: string; // YYYY-MM format
    availableDates: string[]; // Array of date strings (YYYY-MM-DD)
}

/**
 * Daily time slots response for a specific date
 */
export interface DailyTimeSlotsResponse {
    date: string; // YYYY-MM-DD format
    slots: TimeSlot[];
}

/**
 * Booking confirmation response
 */
export interface BookingConfirmation {
    appointment: Appointment;
    confirmationCode: string;
    pdfUrl?: string;
}

/**
 * Booking wizard steps
 */
export type BookingStep = 1 | 2 | 3 | 4;

/**
 * Booking error types for conflict handling (AC-4)
 */
export interface BookingError {
    code: 'conflict' | 'validation' | 'server' | 'timeout';
    message: string;
    details?: Record<string, string[]>;
}
