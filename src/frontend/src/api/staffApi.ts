/**
 * Staff API client functions for staff-specific operations
 * Handles patient search, patient creation, and walk-in appointment booking
 */

import type {
  PatientSearchResult,
  CreatePatientData,
  WalkinAppointmentData,
} from "../types/staff";

/**
 * API base URL from environment variables with fallback
 */
const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

/**
 * Get authorization headers with token
 */
function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem("token");
  return {
    "Content-Type": "application/json",
    ...(token && { Authorization: `Bearer ${token}` }),
  };
}

/**
 * Search for patients by name, email, or phone (US_029, AC1 / US_032, AC1)
 * @param query - Search term (name, email, or phone)
 * @returns Promise<PatientSearchResult[]>
 */
export async function searchPatients(
  query: string,
): Promise<PatientSearchResult[]> {
  if (!query || query.trim().length < 2) {
    return [];
  }

  const url = `${API_BASE_URL}/api/staff/patients/search?query=${encodeURIComponent(query.trim())}`;

  try {
    const response = await fetch(url, {
      method: "GET",
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error("Access denied. Staff privileges required.");
      }
      throw new Error(`Failed to search patients: ${response.statusText}`);
    }

    const data = await response.json();
    return data as PatientSearchResult[];
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(
      "Network error. Please check your connection and try again.",
    );
  }
}

/**
 * Create a new patient with minimal information (US_029, AC2)
 * @param patientData - Minimal patient information (name, DOB, phone, optional email)
 * @returns Promise<PatientSearchResult> - Created patient data
 */
export async function createPatient(
  patientData: CreatePatientData,
): Promise<PatientSearchResult> {
  const url = `${API_BASE_URL}/api/staff/patients`;

  try {
    const response = await fetch(url, {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify(patientData),
    });

    if (!response.ok) {
      if (response.status === 400) {
        const errorData = await response.json();
        throw new Error(errorData.message || "Invalid patient data");
      }
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error("Access denied. Staff privileges required.");
      }
      // Handle duplicate email case (200 OK with existing patient)
      if (response.status === 409) {
        const existingPatient = await response.json();
        return existingPatient as PatientSearchResult;
      }
      throw new Error(`Failed to create patient: ${response.statusText}`);
    }

    const data = await response.json();
    return data as PatientSearchResult;
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(
      "Network error. Please check your connection and try again.",
    );
  }
}

/**
 * Fetch available time slots for a provider on a specific date
 * @param providerId - Provider ID
 * @param date - Date in YYYY-MM-DD format
 * @returns Promise<TimeSlot[]>
 */
export async function fetchProviderSlots(
  providerId: string,
  date: string,
): Promise<unknown[]> {
  const url = `${API_BASE_URL}/api/providers/${providerId}/availability?date=${date}`;

  try {
    const response = await fetch(url, {
      method: "GET",
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 404) {
        throw new Error("Provider not found");
      }
      throw new Error(`Failed to fetch slots: ${response.statusText}`);
    }

    const data = await response.json();
    const timeSlots = data.timeSlots || [];

    // Transform backend data structure to match frontend expectations
    // Backend: { id, startTime, endTime, isBooked }
    // Frontend: { id, providerId, startTime, endTime, status }
    return timeSlots.map((slot: any) => ({
      id: slot.id,
      providerId: providerId,
      startTime: slot.startTime,
      endTime: slot.endTime,
      status: slot.isBooked ? "booked" : "available",
    }));
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(
      "Network error. Please check your connection and try again.",
    );
  }
}

/**
 * Create a walk-in appointment (US_029, AC3)
 * @param appointmentData - Walk-in appointment details
 * @returns Promise<unknown> - Created appointment data
 */
export async function createWalkinAppointment(
  appointmentData: WalkinAppointmentData,
): Promise<unknown> {
  const url = `${API_BASE_URL}/api/appointments/walkin`;

  try {
    const response = await fetch(url, {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify(appointmentData),
    });

    if (!response.ok) {
      if (response.status === 400) {
        const errorData = await response.json();
        throw new Error(errorData.message || "Invalid appointment data");
      }
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error("Access denied. Staff privileges required.");
      }
      if (response.status === 409) {
        throw new Error(
          "Time slot is no longer available. Please select another slot.",
        );
      }
      throw new Error(`Failed to create appointment: ${response.statusText}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(
      "Network error. Please check your connection and try again.",
    );
  }
}

/**
 * Fetch current queue of same-day patients (US_030, AC1)
 * @returns Promise<QueuePatient[]> - Array of queue patients
 */
export async function fetchQueue(): Promise<unknown[]> {
  const url = `${API_BASE_URL}/api/staff/queue`;

  try {
    const response = await fetch(url, {
      method: "GET",
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error("Access denied. Staff privileges required.");
      }
      throw new Error(`Failed to fetch queue: ${response.statusText}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(
      "Network error. Please check your connection and try again.",
    );
  }
}

/**
 * Update patient priority flag in queue (US_030, AC3)
 * @param patientId - Patient ID
 * @param isPriority - Priority flag (true for emergency)
 * @returns Promise<void>
 */
export async function updatePatientPriority(
  patientId: string,
  isPriority: boolean,
): Promise<void> {
  const url = `${API_BASE_URL}/api/staff/queue/${patientId}/priority`;

  try {
    const response = await fetch(url, {
      method: "PATCH",
      headers: getAuthHeaders(),
      body: JSON.stringify({ isPriority }),
    });

    if (!response.ok) {
      if (response.status === 400) {
        throw new Error("Invalid request");
      }
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error("Access denied. Staff privileges required.");
      }
      if (response.status === 404) {
        throw new Error("Patient not found in queue");
      }
      throw new Error(`Failed to update priority: ${response.statusText}`);
    }

    // Success - no content returned
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(
      "Network error. Please check your connection and try again.",
    );
  }
}

/**
 * Staff Dashboard Metrics Data (US_068, AC2)
 */
export interface DashboardMetricsDto {
  todayAppointments: number;
  currentQueueSize: number;
  pendingVerifications: number;
}

/**
 * Queue Preview Data (US_068, AC4)
 */
export interface QueuePreviewDto {
  appointmentId: string;
  patientName: string;
  providerName: string;
  appointmentTime: string;
  estimatedWait: string;
  riskLevel: 'low' | 'medium' | 'high';
  status: string;
}

/**
 * Get staff dashboard metrics (US_068, AC2)
 * @returns Promise<DashboardMetricsDto> - Dashboard stat cards data
 */
export async function getDashboardMetrics(): Promise<DashboardMetricsDto> {
  const url = `${API_BASE_URL}/api/staff/dashboard/metrics`;

  try {
    const response = await fetch(url, {
      method: "GET",
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error("Access denied. Staff privileges required.");
      }
      throw new Error(`Failed to fetch metrics: ${response.statusText}`);
    }

    const data = await response.json();
    return data as DashboardMetricsDto;
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(
      "Network error. Please check your connection and try again.",
    );
  }
}

/**
 * Get queue preview for dashboard (US_068, AC4)
 * @param count - Number of patients to return (default: 5)
 * @returns Promise<QueuePreviewDto[]> - Next N patients in queue
 */
export async function getQueuePreview(count: number = 5): Promise<QueuePreviewDto[]> {
  const url = `${API_BASE_URL}/api/staff/dashboard/queue-preview?count=${count}`;

  try {
    const response = await fetch(url, {
      method: "GET",
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error("Access denied. Staff privileges required.");
      }
      throw new Error(`Failed to fetch queue preview: ${response.statusText}`);
    }

    const data = await response.json();
    return data as QueuePreviewDto[];
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(
      "Network error. Please check your connection and try again.",
    );
  }
}
