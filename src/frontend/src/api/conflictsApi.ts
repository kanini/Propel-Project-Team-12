/**
 * Conflicts API client for managing data conflicts (US_048).
 * Provides methods for fetching, filtering, and resolving conflicts.
 */

import type {
  DataConflict,
  ConflictSummary,
  ResolveConflictRequest,
} from "../types/conflict.types";
import { ConflictSeverity } from "../types/conflict.types";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

/**
 * Get conflicts for a patient with optional filtering.
 * GET /api/patients/{patientId}/conflicts
 */
export interface GetConflictsParams {
  patientId: number;
  severity?: ConflictSeverity;
  unresolvedOnly?: boolean;
  page?: number;
  pageSize?: number;
}

export async function getPatientConflicts(
  params: GetConflictsParams,
): Promise<DataConflict[]> {
  const token = localStorage.getItem("token");

  const queryParams = new URLSearchParams();
  if (params.severity) queryParams.append("severity", params.severity);
  if (params.unresolvedOnly !== undefined)
    queryParams.append("unresolvedOnly", String(params.unresolvedOnly));
  if (params.page) queryParams.append("page", String(params.page));
  if (params.pageSize) queryParams.append("pageSize", String(params.pageSize));

  const url = `${API_BASE_URL}/api/patients/${params.patientId}/conflicts?${queryParams.toString()}`;

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
      if (response.status === 404) {
        throw new Error("Patient conflicts not found");
      }
      throw new Error(`Failed to fetch conflicts: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching patient conflicts:", error);
    throw error;
  }
}

/**
 * Get conflict summary statistics for a patient.
 * GET /api/patients/{patientId}/conflicts/summary
 */
export async function getConflictSummary(
  patientId: number,
): Promise<ConflictSummary> {
  const token = localStorage.getItem("token");
  const url = `${API_BASE_URL}/api/patients/${patientId}/conflicts/summary`;

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
      throw new Error(
        `Failed to fetch conflict summary: ${response.statusText}`,
      );
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching conflict summary:", error);
    throw error;
  }
}

/**
 * Resolve a specific conflict.
 * POST /api/conflicts/{conflictId}/resolve
 */
export async function resolveConflict(
  conflictId: string,
  request: ResolveConflictRequest,
): Promise<DataConflict> {
  const token = localStorage.getItem("token");
  const url = `${API_BASE_URL}/api/conflicts/${conflictId}/resolve`;

  try {
    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...(token && { Authorization: `Bearer ${token}` }),
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Unauthorized. Please log in again.");
      }
      if (response.status === 404) {
        throw new Error("Conflict not found");
      }
      const error = await response.json();
      throw new Error(
        error.message || `Failed to resolve conflict: ${response.statusText}`,
      );
    }

    return await response.json();
  } catch (error) {
    console.error("Error resolving conflict:", error);
    throw error;
  }
}
