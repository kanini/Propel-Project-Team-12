/**
 * Queue helper utilities for US_030 - Queue Management.
 * Provides functions for wait time calculation and queue reordering.
 */

import { formatDistanceToNow } from "date-fns";

/**
 * Queue patient interface matching backend response
 */
export interface QueuePatient {
  id: string;
  patientId: string;
  patientName: string;
  appointmentType: "Walk-in" | "Scheduled";
  providerId: string;
  providerName: string;
  arrivalTime: string; // ISO datetime string
  isPriority: boolean;
  position: number;
}

/**
 * Calculate wait time from arrival time to now
 * @param arrivalTime ISO datetime string
 * @returns Formatted wait time string (e.g., "15 min", "1 hr 30 min")
 */
export function calculateWaitTime(arrivalTime: string): string {
  const arrival = new Date(arrivalTime);
  const now = new Date();
  const diffMs = now.getTime() - arrival.getTime();
  const diffMin = Math.floor(diffMs / 60000);

  if (diffMin < 60) {
    return `${diffMin} min`;
  }

  const hours = Math.floor(diffMin / 60);
  const minutes = diffMin % 60;

  if (minutes === 0) {
    return `${hours} hr`;
  }

  return `${hours} hr ${minutes} min`;
}

/**
 * Format arrival time for display (e.g., "2 minutes ago", "1 hour ago")
 * @param arrivalTime ISO datetime string
 * @returns Formatted relative time string
 */
export function formatArrivalTime(arrivalTime: string): string {
  return formatDistanceToNow(new Date(arrivalTime), { addSuffix: true });
}

/**
 * Reorder queue: priority patients first, then chronological by arrival time
 * @param queue Array of queue patients
 * @returns Reordered queue with updated positions
 */
export function reorderQueue(queue: QueuePatient[]): QueuePatient[] {
  // Separate priority and non-priority patients
  const priorityPatients = queue.filter((p) => p.isPriority);
  const regularPatients = queue.filter((p) => !p.isPriority);

  // Sort both groups by arrival time (earliest first)
  const sortByArrival = (a: QueuePatient, b: QueuePatient) =>
    new Date(a.arrivalTime).getTime() - new Date(b.arrivalTime).getTime();

  priorityPatients.sort(sortByArrival);
  regularPatients.sort(sortByArrival);

  // Combine: priority first, then regular
  const reordered = [...priorityPatients, ...regularPatients];

  // Update positions (1-based index)
  return reordered.map((patient, index) => ({
    ...patient,
    position: index + 1,
  }));
}

/**
 * Filter queue by provider ID
 * @param queue Array of queue patients
 * @param providerId Provider ID filter (empty string for "All Providers")
 * @returns Filtered queue
 */
export function filterQueueByProvider(
  queue: QueuePatient[],
  providerId: string,
): QueuePatient[] {
  if (!providerId || providerId === "") {
    return queue; // "All Providers"
  }
  return queue.filter((p) => p.providerId === providerId);
}

/**
 * Generate announcement message for ARIA live region
 * @param queue Current queue state
 * @returns Announcement string for screen readers
 */
export function generateQueueAnnouncement(queue: QueuePatient[]): string {
  const count = queue.length;
  if (count === 0) {
    return "Queue is empty. No patients waiting.";
  }
  if (count === 1) {
    return "1 patient in queue.";
  }
  return `${count} patients in queue.`;
}
