/**
 * Patient Profile 360° API client (US_049).
 * Provides methods for fetching comprehensive patient health profiles.
 */

import type { PatientProfile360 } from "../types/patientProfile.types";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

/**
 * Fetch 360° patient profile with all health sections.
 * GET /api/patients/{patientId}/profile/360
 */
export interface GetProfile360Params {
  patientId: string;
  vitalRangeStart?: string; // ISO date string (optional)
  vitalRangeEnd?: string; // ISO date string (optional)
}

export async function getPatientProfile360(
  params: GetProfile360Params,
): Promise<PatientProfile360> {
  const token = localStorage.getItem("token");

  const queryParams = new URLSearchParams();
  if (params.vitalRangeStart)
    queryParams.append("vitalRangeStart", params.vitalRangeStart);
  if (params.vitalRangeEnd)
    queryParams.append("vitalRangeEnd", params.vitalRangeEnd);

  const queryString = queryParams.toString();
  const url = `${API_BASE_URL}/api/patients/${params.patientId}/profile/360${queryString ? `?${queryString}` : ""}`;

  try {
    const response = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        ...(token && { Authorization: `Bearer ${token}` }),
      },
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error(
          "Access denied. You do not have permission to view this patient profile.",
        );
      }
      if (response.status === 404) {
        throw new Error(
          "Patient profile not found. Upload clinical documents to build your health profile.",
        );
      }
      throw new Error(
        `Failed to fetch patient profile: ${response.statusText}`,
      );
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching patient profile 360:", error);
    throw error;
  }
}

/**
 * Invalidate cached profile for a patient (Staff/Admin only).
 * DELETE /api/patients/{patientId}/profile/360/cache
 */
export async function invalidateProfile360Cache(
  patientId: string,
): Promise<void> {
  const token = localStorage.getItem("token");
  const url = `${API_BASE_URL}/api/patients/${patientId}/profile/360/cache`;

  try {
    const response = await fetch(url, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        ...(token && { Authorization: `Bearer ${token}` }),
      },
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 403) {
        throw new Error(
          "Access denied. Only staff can invalidate profile cache.",
        );
      }
      throw new Error(`Failed to invalidate cache: ${response.statusText}`);
    }
  } catch (error) {
    console.error("Error invalidating profile cache:", error);
    throw error;
  }
}
