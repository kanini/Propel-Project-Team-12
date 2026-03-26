/**
 * Audit Log API client functions (US_022, US_055).
 * Handles HTTP requests to audit log and session endpoints.
 */



function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem("token");
  return {
    "Content-Type": "application/json",
    ...(token && { Authorization: `Bearer ${token}` }),
  };
}

export interface AuditLogEntry {
  auditLogId: string;
  userId: string | null;
  userName: string | null;
  userEmail: string | null;
  timestamp: string;
  actionType: string;
  resourceType: string;
  actionDetails: string;
  ipAddress: string | null;
}

export interface AuditLogQueryResult {
  items: AuditLogEntry[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AuditLogFilters {
  userId?: string;
  actionType?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

/**
 * Fetches paginated audit logs with optional filters (Admin only).
 * GET /api/audit-logs
 */
export async function fetchAuditLogs(
  filters: AuditLogFilters = {},
): Promise<AuditLogQueryResult> {
  const params = new URLSearchParams();

  if (filters.userId) params.append("userId", filters.userId);
  if (filters.actionType) params.append("actionType", filters.actionType);
  if (filters.startDate) params.append("startDate", filters.startDate);
  if (filters.endDate) params.append("endDate", filters.endDate);
  if (filters.page) params.append("page", filters.page.toString());
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString());

  const url = `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/audit-logs?${params.toString()}`;

  const response = await fetch(url, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    if (response.status === 401)
      throw new Error("Unauthorized. Please log in again.");
    if (response.status === 403)
      throw new Error("Access denied. Admin role required.");
    throw new Error("Failed to fetch audit logs.");
  }

  return response.json();
}

/**
 * Refreshes the session TTL on the backend (US_022, AC5).
 * POST /api/auth/refresh
 */
export async function refreshSessionApi(): Promise<{
  expiresAt: string;
  message: string;
}> {
  const url = `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/auth/refresh`;

  const response = await fetch(url, {
    method: "POST",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    throw new Error("Failed to refresh session.");
  }

  return response.json();
}

/**
 * Logs a session timeout event to the backend (US_022, AC3).
 * POST /api/auth/session-timeout
 */
export async function logSessionTimeout(
  userId: string,
  lastActivityTimestamp?: string,
): Promise<void> {
  const url = `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/auth/session-timeout`;

  await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      userId,
      lastActivityTimestamp: lastActivityTimestamp ?? new Date().toISOString(),
    }),
  }).catch(() => {
    // Non-blocking: session timeout logging failure should not prevent logout
  });
}
