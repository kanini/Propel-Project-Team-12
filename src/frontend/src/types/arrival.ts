/**
 * Arrival Management Types for US_031 - Patient Arrival Status Marking
 */

/**
 * Appointment search result for arrival management
 */
export interface ArrivalAppointment {
  appointmentId: string;
  patientId: string;
  patientName: string;
  dateOfBirth: string;
  scheduledDateTime: string;
  providerName: string;
  visitReason: string;
  status:
    | "Scheduled"
    | "Confirmed"
    | "Arrived"
    | "Cancelled"
    | "Completed"
    | "NoShow";
}

/**
 * Mark arrival response
 */
export interface MarkArrivalResponse {
  success: boolean;
  message: string;
  appointment?: ArrivalAppointment;
}
