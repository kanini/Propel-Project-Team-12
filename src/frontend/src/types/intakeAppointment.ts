/**
 * TypeScript types for intake appointment selection (US_037)
 * Defines interfaces for appointments requiring intake
 */

/**
 * Intake status values for appointments
 */
export type IntakeStatus = 'pending' | 'inProgress' | 'completed';

/**
 * Intake appointment data returned from API
 */
export interface IntakeAppointment {
  appointmentId: string;
  providerId: string;
  providerName: string;
  providerSpecialty: string;
  appointmentDate: string; // ISO date string (YYYY-MM-DD)
  appointmentTime: string; // Time string (HH:mm)
  intakeStatus: IntakeStatus;
  intakeSessionId?: string;
}

/**
 * API response for intake appointments list
 */
export interface IntakeAppointmentListResponse {
  appointments: IntakeAppointment[];
}

/**
 * Redux state for intake appointment selection
 */
export interface IntakeAppointmentState {
  appointments: IntakeAppointment[];
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
}
