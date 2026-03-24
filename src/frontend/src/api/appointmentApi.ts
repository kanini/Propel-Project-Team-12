/**
 * Appointment API client functions
 * Handles all HTTP requests to appointment endpoints with error handling
 */

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
 * Cancel an appointment (US_027 - AC-1, AC-4)
 * DELETE /api/appointments/{id}
 * @param appointmentId - Appointment GUID to cancel
 * @returns Promise<void>
 * @throws Error with message if cancellation fails or policy is violated
 */
export async function cancelAppointment(appointmentId: string): Promise<void> {
  const url = `${API_BASE_URL}/api/appointments/${appointmentId}`;

  try {
    const response = await fetch(url, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      
      if (response.status === 403) {
        // Policy violation (e.g., cancellation within 24 hours)
        throw new Error(errorData.message || 'Cancellation policy violation. Contact support for assistance.');
      }
      
      if (response.status === 400) {
        throw new Error(errorData.message || 'Invalid appointment. Unable to cancel.');
      }
      
      if (response.status === 500) {
        throw new Error('Server error. Please try again later.');
      }
      
      throw new Error(errorData.message || `Failed to cancel appointment: ${response.statusText}`);
    }
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Reschedule an appointment to a new time slot (US_027 - AC-2, AC-3)
 * PATCH /api/appointments/{id}/reschedule
 * @param appointmentId - Appointment GUID to reschedule
 * @param newTimeSlotId - New time slot GUID
 * @returns Promise with updated appointment data
 * @throws Error with message if reschedule fails or slot is unavailable
 */
export async function rescheduleAppointment(
  appointmentId: string,
  newTimeSlotId: string
): Promise<{
  id: string;
  scheduledDateTime: string;
  timeSlotId: string;
  providerName: string;
  providerSpecialty: string;
  visitReason: string;
  status: string;
}> {
  const url = `${API_BASE_URL}/api/appointments/${appointmentId}/reschedule`;

  try {
    const response = await fetch(url, {
      method: 'PATCH',
      headers: getAuthHeaders(),
      body: JSON.stringify({ newTimeSlotId }),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      
      if (response.status === 403) {
        throw new Error(errorData.message || 'You do not have permission to reschedule this appointment.');
      }
      
      if (response.status === 409) {
        // Conflict - slot no longer available
        throw new Error(errorData.message || 'The selected time slot is no longer available. Please select another slot.');
      }
      
      if (response.status === 400) {
        throw new Error(errorData.message || 'Invalid reschedule request.');
      }
      
      if (response.status === 500) {
        throw new Error('Server error. Please try again later.');
      }
      
      throw new Error(errorData.message || `Failed to reschedule appointment: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Fetch available time slots for a provider (for rescheduling - US_027 AC-2)
 * GET /api/providers/{providerId}/availability (monthly query)
 * @param providerId - Provider GUID
 * @param startDate - Start date for availability search (YYYY-MM-DD) - optional
 * @param endDate - End date for availability search (YYYY-MM-DD) - optional
 * @returns Promise with array of available time slots
 */
export async function fetchProviderAvailability(
  providerId: string,
  startDate?: string,
  endDate?: string
): Promise<Array<{
  id: string;
  providerId: string;
  startTime: string;
  endTime: string;
  status: string;
}>> {
  try {
    // Determine which months to fetch based on startDate and endDate
    const start = startDate ? new Date(startDate) : new Date();
    const end = endDate ? new Date(endDate) : new Date(start.getTime() + 30 * 24 * 60 * 60 * 1000);
    
    // Get list of unique year-month combinations to fetch
    const monthsToFetch: Array<{ year: number; month: number }> = [];
    const current = new Date(start);
    
    while (current <= end) {
      const year = current.getFullYear();
      const month = current.getMonth() + 1; // JavaScript months are 0-indexed
      
      // Check if we already have this month
      if (!monthsToFetch.some(m => m.year === year && m.month === month)) {
        monthsToFetch.push({ year, month });
      }
      
      // Move to next month
      current.setMonth(current.getMonth() + 1);
    }

    // Fetch availability for all required months
    const allSlots: Array<{
      id: string;
      providerId: string;
      startTime: string;
      endTime: string;
      status: string;
    }> = [];

    for (const { year, month } of monthsToFetch) {
      const url = `${API_BASE_URL}/api/providers/${providerId}/availability?year=${year}&month=${month}`;
      
      const response = await fetch(url, {
        method: 'GET',
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: response.statusText }));
        
        if (response.status === 401) {
          throw new Error('Unauthorized. Please log in again.');
        }
        
        if (response.status === 404) {
          throw new Error('Provider not found.');
        }
        
        if (response.status === 500) {
          throw new Error('Server error. Please try again later.');
        }
        
        throw new Error(errorData.message || `Failed to fetch availability: ${response.statusText}`);
      }

      // Backend returns array of AvailabilityResponseDto with Date and TimeSlots[]
      const monthlyData: Array<{
        date: string;
        timeSlots: Array<{
          id: string;
          startTime: string;
          endTime: string;
          isBooked: boolean;
        }>;
      }> = await response.json();

      // Flatten and transform the data
      for (const dayData of monthlyData) {
        for (const slot of dayData.timeSlots) {
          // Only include available slots
          if (!slot.isBooked) {
            allSlots.push({
              id: slot.id,
              providerId: providerId,
              startTime: slot.startTime,
              endTime: slot.endTime,
              status: 'available',
            });
          }
        }
      }
    }

    // Filter slots to only those within the requested date range
    const filteredSlots = allSlots.filter(slot => {
      const slotDate = new Date(slot.startTime);
      return slotDate >= start && slotDate <= end;
    });

    return filteredSlots;
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Network error. Please check your connection and try again.');
  }
}
