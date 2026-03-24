/**
 * Dashboard API client functions for US_067 - Patient Dashboard
 * Handles all HTTP requests to dashboard endpoints with error handling
 */

import type {
  DashboardStatsDto,
  UpcomingAppointmentDto,
  NotificationDto,
  UnreadCountDto,
  RecentDocumentDto
} from '../types/dashboard';

/**
 * API base URL from environment variables with fallback
 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Get authentication token from localStorage
 */
function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
  };
}

/**
 * Fetch dashboard statistics for authenticated patient (US_067, AC2)
 * @returns Promise<DashboardStatsDto>
 */
export async function fetchDashboardStats(): Promise<DashboardStatsDto> {
  const url = `${API_BASE_URL}/api/dashboard/stats`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      if (response.status === 500) {
        throw new Error('Server error. Please try again later.');
      }
      throw new Error(`Failed to fetch dashboard stats: ${response.statusText}`);
    }

    const data: DashboardStatsDto = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching dashboard stats:', error);
    throw error;
  }
}

/**
 * Fetch upcoming appointments for dashboard display (US_067, AC4)
 * @param limit - Maximum number of appointments to return (default: 5, max: 20)
 * @returns Promise<UpcomingAppointmentDto[]>
 */
export async function fetchUpcomingAppointments(limit: number = 5): Promise<UpcomingAppointmentDto[]> {
  const url = `${API_BASE_URL}/api/appointments/upcoming?limit=${limit}`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      throw new Error(`Failed to fetch upcoming appointments: ${response.statusText}`);
    }

    const data: UpcomingAppointmentDto[] = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching upcoming appointments:', error);
    throw error;
  }
}

/**
 * Fetch recent notifications for dashboard panel (US_067, AC5)
 * @param limit - Maximum number of notifications to return (default: 5, max: 20)
 * @returns Promise<NotificationDto[]>
 */
export async function fetchRecentNotifications(limit: number = 5): Promise<NotificationDto[]> {
  const url = `${API_BASE_URL}/api/notifications/recent?limit=${limit}`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      throw new Error(`Failed to fetch notifications: ${response.statusText}`);
    }

    const data: NotificationDto[] = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching notifications:', error);
    throw error;
  }
}

/**
 * Fetch unread notification count for badge display (US_067, AC5)
 * @returns Promise<number>
 */
export async function fetchUnreadCount(): Promise<number> {
  const url = `${API_BASE_URL}/api/notifications/unread-count`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      throw new Error(`Failed to fetch unread count: ${response.statusText}`);
    }

    const data: UnreadCountDto = await response.json();
    return data.count;
  } catch (error) {
    console.error('Error fetching unread count:', error);
    throw error;
  }
}

/**
 * Mark notification as read (US_067, AC7)
 * @param notificationId - Notification GUID
 * @returns Promise<void>
 */
export async function markNotificationAsRead(notificationId: string): Promise<void> {
  const url = `${API_BASE_URL}/api/notifications/${notificationId}/read`;

  try {
    const response = await fetch(url, {
      method: 'PATCH',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      if (response.status === 404) {
        throw new Error('Notification not found');
      }
      throw new Error(`Failed to mark notification as read: ${response.statusText}`);
    }
  } catch (error) {
    console.error('Error marking notification as read:', error);
    throw error;
  }
}

/**
 * Fetch recent clinical documents for dashboard (US_067, AC6)
 * @param limit - Maximum number of documents to return (default: 3, max: 10)
 * @returns Promise<RecentDocumentDto[]>
 */
export async function fetchRecentDocuments(limit: number = 3): Promise<RecentDocumentDto[]> {
  const url = `${API_BASE_URL}/api/documents/recent?limit=${limit}`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      throw new Error(`Failed to fetch recent documents: ${response.statusText}`);
    }

    const data: RecentDocumentDto[] = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching recent documents:', error);
    throw error;
  }
}
