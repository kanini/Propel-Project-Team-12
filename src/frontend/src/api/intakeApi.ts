/**
 * API client for AI conversational intake endpoints (US_033)
 * Handles HTTP requests for intake sessions and chat messages
 */

import type {
  StartIntakeRequest,
  StartIntakeResponse,
  SendMessageRequest,
  SendMessageResponse,
  CompleteIntakeRequest,
  CompleteIntakeResponse,
  ExtractedIntakeData,
  ManualIntakeFormData,
} from '../types/intake';

/**
 * API base URL from environment variables with fallback
 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Get authorization header with JWT token
 */
function getAuthHeader(): Record<string, string> {
  const token = localStorage.getItem('token');
  return token ? { Authorization: `Bearer ${token}` } : {};
}

/**
 * Start a new intake session (US_033, AC-1)
 * @param request - Appointment ID and intake mode
 * @returns Session info with welcome message
 */
export async function startIntakeSession(
  request: StartIntakeRequest
): Promise<StartIntakeResponse> {
  const response = await fetch(`${API_BASE_URL}/api/intake/start`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...getAuthHeader(),
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Unauthorized. Please log in again.');
    }
    if (response.status === 404) {
      throw new Error('Appointment not found.');
    }
    throw new Error(`Failed to start intake session: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Send a message in the intake chat (US_033, AC-2)
 * @param request - Session ID and user message
 * @returns AI response with extracted data
 */
export async function sendIntakeMessage(
  request: SendMessageRequest
): Promise<SendMessageResponse> {
  const response = await fetch(`${API_BASE_URL}/api/intake/message`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...getAuthHeader(),
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Unauthorized. Please log in again.');
    }
    if (response.status === 404) {
      throw new Error('Intake session not found.');
    }
    throw new Error(`Failed to send message: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Update intake data during session (partial update)
 * @param sessionId - Intake session ID
 * @param data - Partial intake data to update
 */
export async function updateIntakeData(
  sessionId: string,
  data: Partial<ExtractedIntakeData>
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/intake/${sessionId}`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      ...getAuthHeader(),
    },
    body: JSON.stringify({ data }),
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Unauthorized. Please log in again.');
    }
    if (response.status === 404) {
      throw new Error('Intake session not found.');
    }
    throw new Error(`Failed to update intake data: ${response.statusText}`);
  }
}

/**
 * Complete and submit the intake (US_033, AC-3)
 * @param request - Session ID and final summary data
 * @returns Completion confirmation
 */
export async function completeIntake(
  request: CompleteIntakeRequest
): Promise<CompleteIntakeResponse> {
  const response = await fetch(
    `${API_BASE_URL}/api/intake/${request.sessionId}/complete`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeader(),
      },
      body: JSON.stringify({ summary: request.summary }),
    }
  );

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Unauthorized. Please log in again.');
    }
    if (response.status === 404) {
      throw new Error('Intake session not found.');
    }
    throw new Error(`Failed to complete intake: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Submit manual intake form (US_034)
 * @param sessionId - Intake session ID
 * @param formData - Complete manual form data
 * @returns Completion confirmation
 */
export async function submitManualIntake(
  sessionId: string,
  formData: ManualIntakeFormData
): Promise<CompleteIntakeResponse> {
  const response = await fetch(
    `${API_BASE_URL}/api/intake/${sessionId}/submit`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeader(),
      },
      body: JSON.stringify({ mode: 'manual', data: formData }),
    }
  );

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Unauthorized. Please log in again.');
    }
    if (response.status === 404) {
      throw new Error('Intake session not found.');
    }
    if (response.status === 400) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Invalid form data.');
    }
    throw new Error(`Failed to submit intake: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Get existing intake session data (for resuming)
 * @param sessionId - Intake session ID
 * @returns Session data including messages and extracted data
 */
export async function getIntakeSession(sessionId: string): Promise<{
  session: {
    sessionId: string;
    appointmentId: string;
    mode: 'ai' | 'manual';
    status: string;
    progress: number;
  };
  messages: Array<{
    id: string;
    sender: 'ai' | 'user';
    content: string;
    timestamp: string;
  }>;
  extractedData: ExtractedIntakeData;
}> {
  const response = await fetch(`${API_BASE_URL}/api/intake/${sessionId}`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      ...getAuthHeader(),
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Unauthorized. Please log in again.');
    }
    if (response.status === 404) {
      throw new Error('Intake session not found.');
    }
    throw new Error(`Failed to get intake session: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Switch intake mode (AI to Manual or vice versa) (US_035)
 * @param sessionId - Current session ID
 * @param newMode - New intake mode
 * @returns Updated session info
 */
export async function switchIntakeMode(
  sessionId: string,
  newMode: 'ai' | 'manual'
): Promise<{ sessionId: string; mode: 'ai' | 'manual'; dataPreserved: boolean }> {
  const response = await fetch(`${API_BASE_URL}/api/intake/${sessionId}/switch`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...getAuthHeader(),
    },
    body: JSON.stringify({ mode: newMode }),
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Unauthorized. Please log in again.');
    }
    if (response.status === 404) {
      throw new Error('Intake session not found.');
    }
    throw new Error(`Failed to switch mode: ${response.statusText}`);
  }

  return response.json();
}
