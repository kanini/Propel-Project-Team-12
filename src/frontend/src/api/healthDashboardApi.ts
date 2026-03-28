/**
 * Health Dashboard API client for EP006/US_044/US_045
 * Fetches 360° health dashboard data for patients and staff views
 */

import type { HealthDashboard360Dto } from '../types/clinicalData';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` }),
  };
}

/**
 * Fetch authenticated patient's own health dashboard (SCR-016)
 * GET /api/health-dashboard
 */
export async function fetchHealthDashboard(): Promise<HealthDashboard360Dto> {
  const url = `${API_BASE_URL}/api/health-dashboard`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      if (response.status === 404) {
        throw new Error('No health data found. Upload clinical documents to get started.');
      }
      throw new Error(`Failed to fetch health dashboard: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Fetch a patient's health dashboard as staff (SCR-016)
 * GET /api/health-dashboard/patient/{patientId}
 */
export async function fetchPatientHealthDashboard(
  patientId: string
): Promise<HealthDashboard360Dto> {
  const url = `${API_BASE_URL}/api/health-dashboard/patient/${encodeURIComponent(patientId)}`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      if (response.status === 403) {
        throw new Error('You do not have permission to view this patient\'s data.');
      }
      if (response.status === 404) {
        throw new Error('Patient not found or no health data available.');
      }
      throw new Error(`Failed to fetch patient dashboard: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}
