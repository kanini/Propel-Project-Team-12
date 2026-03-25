/**
 * API client for intake appointment selection endpoints (US_037)
 * Handles HTTP requests for fetching appointments requiring intake
 */

import type { IntakeAppointment } from '../types/intakeAppointment';

/**
 * API base URL from environment variables with fallback
 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Fetch appointments requiring intake for the authenticated patient
 * @returns Promise<IntakeAppointment[]> - List of appointments needing intake
 * @throws Error on network or authentication failure
 */
export async function fetchIntakeAppointments(): Promise<IntakeAppointment[]> {
  const token = localStorage.getItem('token');
  
  const response = await fetch(`${API_BASE_URL}/api/intake/appointments`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` }),
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Unauthorized. Please log in again.');
    }
    if (response.status === 403) {
      throw new Error('Access denied.');
    }
    throw new Error(`Failed to fetch intake appointments: ${response.statusText}`);
  }

  const data = await response.json();
  
  // Transform API response to match frontend interface
  // Backend returns camelCase properties that map directly
  return data.map((appointment: Record<string, unknown>) => ({
    appointmentId: appointment.appointmentId,
    providerId: appointment.providerId,
    providerName: appointment.providerName,
    providerSpecialty: appointment.providerSpecialty,
    appointmentDate: appointment.appointmentDate,
    appointmentTime: appointment.appointmentTime,
    intakeStatus: mapIntakeStatus(appointment.intakeStatus as string),
    intakeSessionId: appointment.intakeSessionId,
  }));
}

/**
 * Map backend intake status to frontend enum value
 */
function mapIntakeStatus(status: string): 'pending' | 'inProgress' | 'completed' {
  const statusMap: Record<string, 'pending' | 'inProgress' | 'completed'> = {
    'Pending': 'pending',
    'pending': 'pending',
    'InProgress': 'inProgress',
    'inProgress': 'inProgress',
    'Completed': 'completed',
    'completed': 'completed',
  };
  return statusMap[status] || 'pending';
}
