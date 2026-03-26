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
  noShowRiskScore?: number; // US_038 AC-3: 0-100 risk score (nullable for legacy appointments)
  riskLevel?: 'Low' | 'Medium' | 'High'; // US_038 AC-3: Risk level derived from score
}

/**
 * Mark arrival response
 */
export interface MarkArrivalResponse {
  success: boolean;
  message: string;
  appointment?: ArrivalAppointment;
}
