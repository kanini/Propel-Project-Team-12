/**
 * Clinical Verification API client for SCR-023
 * Handles verification actions for extracted clinical data and medical codes
 */

import type {
  ClinicalVerificationDashboardDto,
  VerificationQueueResponseDto,
  RejectActionDto,
  ModifyCodeDto,
} from '../types/clinicalData';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` }),
  };
}

/**
 * Fetch verification queue — patients with pending verifications (SCR-023A)
 * GET /api/clinical-verification/queue
 */
export async function fetchVerificationQueue(
  limit = 10,
  search?: string
): Promise<VerificationQueueResponseDto> {
  const params = new URLSearchParams({ limit: String(limit) });
  if (search) params.set('search', search);
  const url = `${API_BASE_URL}/api/clinical-verification/queue?${params.toString()}`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) throw new Error('Unauthorized. Please log in again.');
      if (response.status === 403) throw new Error('You do not have permission to access verification data.');
      throw new Error(`Failed to fetch verification queue: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Fetch full verification dashboard for a patient (SCR-023)
 * GET /api/clinical-verification/patient/{patientId}
 */
export async function fetchVerificationDashboard(
  patientId: string
): Promise<ClinicalVerificationDashboardDto> {
  const url = `${API_BASE_URL}/api/clinical-verification/patient/${encodeURIComponent(patientId)}`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) throw new Error('Unauthorized. Please log in again.');
      if (response.status === 403) throw new Error('You do not have permission to access verification data.');
      if (response.status === 404) throw new Error('Patient not found.');
      throw new Error(`Failed to fetch verification data: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Verify a clinical data point
 * POST /api/clinical-verification/data/{id}/verify
 */
export async function verifyDataPoint(dataId: string): Promise<void> {
  const url = `${API_BASE_URL}/api/clinical-verification/data/${encodeURIComponent(dataId)}/verify`;

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) throw new Error('Unauthorized. Please log in again.');
      if (response.status === 404) throw new Error('Data point not found.');
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(errorData.message || `Failed to verify data point: ${response.statusText}`);
    }
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Reject a clinical data point
 * POST /api/clinical-verification/data/{id}/reject
 */
export async function rejectDataPoint(payload: RejectActionDto): Promise<void> {
  const url = `${API_BASE_URL}/api/clinical-verification/data/${encodeURIComponent(payload.id)}/reject`;

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ reason: payload.reason }),
    });

    if (!response.ok) {
      if (response.status === 401) throw new Error('Unauthorized. Please log in again.');
      if (response.status === 404) throw new Error('Data point not found.');
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(errorData.message || `Failed to reject data point: ${response.statusText}`);
    }
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Accept/verify a medical code
 * POST /api/clinical-verification/codes/{id}/accept
 */
export async function acceptMedicalCode(codeId: string): Promise<void> {
  const url = `${API_BASE_URL}/api/clinical-verification/codes/${encodeURIComponent(codeId)}/accept`;

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) throw new Error('Unauthorized. Please log in again.');
      if (response.status === 404) throw new Error('Medical code not found.');
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(errorData.message || `Failed to verify code: ${response.statusText}`);
    }
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Reject a medical code
 * POST /api/clinical-verification/codes/{id}/reject
 */
export async function rejectMedicalCode(codeId: string, reason?: string): Promise<void> {
  const url = `${API_BASE_URL}/api/clinical-verification/codes/${encodeURIComponent(codeId)}/reject`;

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ reason }),
    });

    if (!response.ok) {
      if (response.status === 401) throw new Error('Unauthorized. Please log in again.');
      if (response.status === 404) throw new Error('Medical code not found.');
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(errorData.message || `Failed to reject code: ${response.statusText}`);
    }
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Modify a medical code (update code value and description)
 * POST /api/clinical-verification/codes/{id}/modify
 */
export async function modifyMedicalCode(payload: ModifyCodeDto): Promise<void> {
  const url = `${API_BASE_URL}/api/clinical-verification/codes/${encodeURIComponent(payload.medicalCodeId)}/modify`;

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({
        medicalCodeId: payload.medicalCodeId,
        codeValue: payload.codeValue,
        codeDescription: payload.codeDescription,
      }),
    });

    if (!response.ok) {
      if (response.status === 401) throw new Error('Unauthorized. Please log in again.');
      if (response.status === 404) throw new Error('Medical code not found.');
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(errorData.message || `Failed to modify code: ${response.statusText}`);
    }
  } catch (error) {
    if (error instanceof Error) throw error;
    throw new Error('Network error. Please check your connection and try again.');
  }
}
