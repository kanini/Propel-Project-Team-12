/**
 * TypeScript type definitions for staff-related operations
 */

/**
 * Patient search result (US_029, US_032)
 */
export interface PatientSearchResult {
  id: string;
  fullName: string;
  dateOfBirth: string;
  email?: string | null;
  phone?: string | null;
  lastAppointmentDate?: string | null;
}

/**
 * Create patient data (US_029, AC2)
 */
export interface CreatePatientData {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  phone: string;
  email?: string;
}

/**
 * Walk-in appointment data (US_029, AC3)
 */
export interface WalkinAppointmentData {
  patientId: string;
  providerId: string;
  timeSlotId: string;
  visitReason: string;
}

/**
 * Time slot entity for walk-in booking
 */
export interface TimeSlot {
  id: string;
  providerId: string;
  startTime: string; // ISO datetime string
  endTime: string; // ISO datetime string
  status: "available" | "booked" | "unavailable";
}
