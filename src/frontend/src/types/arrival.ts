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
  noShowRiskScore?: number; // 0-100, US_038 - FR-023
  riskLevel?: 'Low' | 'Medium' | 'High'; // US_038 - FR-023
}

/**
 * Mark arrival response
 */
export interface MarkArrivalResponse {
  success: boolean;
  message: string;
  appointment?: ArrivalAppointment;
}
