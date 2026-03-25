/**
 * Insurance Precheck API client (US_036)
 */

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000';

/**
 * Get authentication headers for API requests
 */
function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` }),
  };
}

export interface InsurancePrecheckRequest {
  providerId: string;
  memberId: string;
  groupNumber?: string;
  dateOfBirth: string;
  appointmentId: number;
}

export interface InsurancePrecheckResponse {
  status: 'verified' | 'pending' | 'failed' | 'not_found';
  providerName?: string;
  memberId?: string;
  effectiveDate?: string;
  expirationDate?: string;
  copayAmount?: number;
  deductibleRemaining?: number;
  message?: string;
}

/**
 * Verify insurance eligibility
 */
export async function verifyInsurance(
  request: InsurancePrecheckRequest
): Promise<InsurancePrecheckResponse> {
  const response = await fetch(`${API_BASE}/api/insurance/precheck`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Failed to verify insurance');
  }

  return response.json();
}

/**
 * Get cached precheck result for an appointment
 */
export async function getPrecheckResult(
  appointmentId: number
): Promise<InsurancePrecheckResponse | null> {
  const response = await fetch(`${API_BASE}/api/insurance/precheck/${appointmentId}`, {
    headers: getAuthHeaders(),
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Failed to get precheck result');
  }

  return response.json();
}
