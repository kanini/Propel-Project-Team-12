/**
 * QueueManagement Page Component for US_030 - Queue Management Interface.
 * Displays same-day patients in chronological order with real-time updates via Pusher.
 */

import { useState, useEffect, useCallback } from "react";
import { Link } from "react-router-dom";
import { usePusherQueue } from "../../hooks/usePusherQueue";
import { QueueList } from "../../features/staff/components/QueueList";
import { ProviderFilter } from "../../features/staff/components/ProviderFilter";
import {
  reorderQueue,
  filterQueueByProvider,
  generateQueueAnnouncement,
  type QueuePatient,
} from "../../utils/queueHelpers";

/**
 * Queue Management page for staff
 */
export function QueueManagement() {
  const [queue, setQueue] = useState<QueuePatient[]>([]);
  const [filteredQueue, setFilteredQueue] = useState<QueuePatient[]>([]);
  const [selectedProvider, setSelectedProvider] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [announcement, setAnnouncement] = useState("");

  /**
   * Fetch queue data from API
   */
  const fetchQueue = useCallback(async () => {
    try {
      const token = localStorage.getItem("token");
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/staff/queue`,
        {
          headers: {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
          },
        },
      );

      if (!response.ok) {
        throw new Error("Failed to fetch queue");
      }

      const data = await response.json();
      const orderedQueue = reorderQueue(data);
      setQueue(orderedQueue);
      setError(null);
    } catch (err) {
      console.error("Error fetching queue:", err);
      setError("Failed to load queue data");
    } finally {
      setIsLoading(false);
    }
  }, []);

  /**
   * Handle patient added to queue
   */
  const handlePatientAdded = useCallback((patient: QueuePatient) => {
    setQueue((prevQueue) => {
      const updated = [...prevQueue, patient];
      const reordered = reorderQueue(updated);
      setAnnouncement(`New patient added: ${patient.patientName}`);
      return reordered;
    });
  }, []);

  /**
   * Handle patient removed from queue
   */
  const handlePatientRemoved = useCallback((patientId: string) => {
    setQueue((prevQueue) => {
      const updated = prevQueue.filter((p) => p.patientId !== patientId);
      const reordered = reorderQueue(updated);
      setAnnouncement("Patient removed from queue");
      return reordered;
    });
  }, []);

  /**
   * Handle priority change
   */
  const handlePriorityChanged = useCallback(
    (patientId: string, isPriority: boolean) => {
      setQueue((prevQueue) => {
        const updated = prevQueue.map((p) =>
          p.patientId === patientId ? { ...p, isPriority } : p,
        );
        const reordered = reorderQueue(updated);
        const patient = updated.find((p) => p.patientId === patientId);
        setAnnouncement(
          isPriority
            ? `${patient?.patientName} flagged as priority`
            : `Priority removed from ${patient?.patientName}`,
        );
        return reordered;
      });
    },
    [],
  );

  /**
   * Toggle patient priority via API
   */
  const togglePatientPriority = async (
    patientId: string,
    isPriority: boolean,
  ) => {
    try {
      const token = localStorage.getItem("token");
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/staff/queue/${patientId}/priority`,
        {
          method: "PATCH",
          headers: {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
          },
          body: JSON.stringify({ isPriority }),
        },
      );

      if (!response.ok) {
        throw new Error("Failed to update priority");
      }

      // Update local state immediately for optimistic UI
      handlePriorityChanged(patientId, isPriority);
    } catch (err) {
      console.error("Error updating priority:", err);
      throw err;
    }
  };

  /**
   * Initialize Pusher with queue event handlers
   */
  const { isUsingFallback, reconnectAttempts, retryConnection } =
    usePusherQueue({
      onPatientAdded: handlePatientAdded,
      onPatientRemoved: handlePatientRemoved,
      onPriorityChanged: handlePriorityChanged,
      fetchQueueFallback: fetchQueue,
    });

  /**
   * Fetch initial queue data on mount
   */
  useEffect(() => {
    fetchQueue();
  }, [fetchQueue]);

  /**
   * Filter queue by selected provider
   */
  useEffect(() => {
    const filtered = filterQueueByProvider(queue, selectedProvider);
    setFilteredQueue(filtered);
    setAnnouncement(generateQueueAnnouncement(filtered));
  }, [queue, selectedProvider]);

  /**
   * Handle provider filter change
   */
  const handleProviderChange = (providerId: string) => {
    setSelectedProvider(providerId);
  };

  return (
    <div className="min-h-screen bg-neutral-50">
      {/* Header */}
      <header className="bg-neutral-0 border-b border-neutral-200 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-neutral-900">
                Queue Management
              </h1>
              <p className="mt-1 text-sm text-neutral-600">
                Same-day patient queue with real-time updates
              </p>
            </div>
            <Link
              to="/staff/walk-in"
              className="inline-flex items-center gap-2 px-4 py-2 bg-primary-500 text-neutral-0 
                                font-medium rounded-lg hover:bg-primary-600 focus:outline-none focus:ring-2 
                                focus:ring-primary-500 focus:ring-offset-2 transition-colors"
            >
              <svg
                className="w-5 h-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 4v16m8-8H4"
                />
              </svg>
              Book Walk-in
            </Link>
          </div>
        </div>
      </header>

      {/* Connection Status Banner */}
      {isUsingFallback && (
        <div className="bg-warning text-neutral-900 px-4 py-3 border-b border-warning">
          <div className="max-w-7xl mx-auto flex items-center justify-between">
            <div className="flex items-center gap-2">
              <svg
                className="w-5 h-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <span className="text-sm font-medium">
                Live updates paused. Using fallback polling (30s interval).
                {reconnectAttempts > 0 &&
                  ` Reconnection attempts: ${reconnectAttempts}`}
              </span>
            </div>
            <button
              onClick={retryConnection}
              className="text-sm font-medium underline hover:no-underline focus:outline-none 
                                focus:ring-2 focus:ring-primary-500 rounded px-2 py-1"
            >
              Retry Connection
            </button>
          </div>
        </div>
      )}

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Provider Filter */}
        <div className="mb-6">
          <ProviderFilter
            value={selectedProvider}
            onChange={handleProviderChange}
          />
        </div>

        {/* ARIA Live Region for Screen Readers */}
        <div
          className="sr-only"
          role="status"
          aria-live="polite"
          aria-atomic="true"
        >
          {announcement}
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-8">
            <div className="animate-pulse space-y-4">
              <div className="h-10 bg-neutral-200 rounded" />
              <div className="h-10 bg-neutral-200 rounded" />
              <div className="h-10 bg-neutral-200 rounded" />
            </div>
          </div>
        )}

        {/* Error State */}
        {error && !isLoading && (
          <div className="bg-error-50 border border-error text-error px-4 py-3 rounded-lg">
            <p className="text-sm font-medium">{error}</p>
          </div>
        )}

        {/* Queue List */}
        {!isLoading && !error && filteredQueue.length > 0 && (
          <QueueList
            queue={filteredQueue}
            onTogglePriority={togglePatientPriority}
          />
        )}

        {/* Empty State */}
        {!isLoading && !error && filteredQueue.length === 0 && (
          <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-12 text-center">
            <svg
              className="w-24 h-24 mx-auto text-neutral-300 mb-4"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1.5}
                d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
              />
            </svg>
            <h3 className="text-lg font-semibold text-neutral-900 mb-2">
              No patients in queue
            </h3>
            <p className="text-sm text-neutral-600 mb-6">
              {selectedProvider
                ? "No patients waiting for the selected provider."
                : "The queue is empty. Book a walk-in appointment to get started."}
            </p>
            <Link
              to="/staff/walk-in"
              className="inline-flex items-center gap-2 px-6 py-2.5 bg-primary-500 text-neutral-0 
                                font-medium rounded-lg hover:bg-primary-600 focus:outline-none focus:ring-2 
                                focus:ring-primary-500 focus:ring-offset-2 transition-colors"
            >
              <svg
                className="w-5 h-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 4v16m8-8H4"
                />
              </svg>
              Book Walk-in Appointment
            </Link>
          </div>
        )}
      </main>
    </div>
  );
}
