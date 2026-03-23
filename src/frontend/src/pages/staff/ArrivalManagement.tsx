/**
 * ArrivalManagement Page Component for US_031 - Patient Arrival Status Marking (SCR-020).
 * Staff-only page for viewing today's appointments and marking patients as arrived.
 */

import { useState, useEffect } from "react";
import { AppointmentsList } from "../../features/staff/components/AppointmentsList";
import type { ArrivalAppointment } from "../../types/arrival";

/**
 * Arrival Management page for staff to mark patient arrivals
 */
export function ArrivalManagement() {
  const [appointments, setAppointments] = useState<ArrivalAppointment[]>([]);
  const [filteredAppointments, setFilteredAppointments] = useState<
    ArrivalAppointment[]
  >([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [showSuccessToast, setShowSuccessToast] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");
  const [showErrorToast, setShowErrorToast] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  /**
   * Load all today's appointments on mount
   */
  useEffect(() => {
    loadTodayAppointments();
  }, []);

  /**
   * Filter appointments based on search query
   */
  useEffect(() => {
    if (!searchQuery.trim()) {
      setFilteredAppointments(appointments);
      return;
    }

    const query = searchQuery.toLowerCase();
    const filtered = appointments.filter(
      (apt) =>
        apt.patientName.toLowerCase().includes(query) ||
        apt.dateOfBirth.includes(query) ||
        apt.providerName.toLowerCase().includes(query) ||
        (apt.visitReason && apt.visitReason.toLowerCase().includes(query)),
    );
    setFilteredAppointments(filtered);
  }, [searchQuery, appointments]);

  /**
   * Fetch all today's appointments from API
   */
  const loadTodayAppointments = async () => {
    setIsLoading(true);
    try {
      const token = localStorage.getItem("token");
      const today = new Date().toISOString().split("T")[0]; // YYYY-MM-DD format
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/staff/arrivals/search?date=${today}`,
        {
          headers: {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
          },
        },
      );

      if (!response.ok) {
        throw new Error("Failed to load appointments");
      }

      const data = await response.json();
      setAppointments(data);
      setFilteredAppointments(data);
    } catch (error) {
      console.error("Error loading appointments:", error);
      setErrorMessage("Failed to load today's appointments");
      setShowErrorToast(true);
      setTimeout(() => setShowErrorToast(false), 5000);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Mark patient as arrived and update local state
   */
  const handleMarkArrived = async (appointmentId: string) => {
    try {
      const token = localStorage.getItem("token");
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/staff/arrivals/${appointmentId}/mark-arrived`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
          },
        },
      );

      if (!response.ok) {
        const errorData = await response.json();

        // Handle 409 Conflict (already arrived)
        if (response.status === 409) {
          setSuccessMessage(
            errorData.message || "Patient already marked as arrived",
          );
          setShowSuccessToast(true);

          // Update local state to show "Arrived" status
          setAppointments((prev) =>
            prev.map((apt) =>
              apt.appointmentId === appointmentId
                ? { ...apt, status: "Arrived" }
                : apt,
            ),
          );

          setTimeout(() => setShowSuccessToast(false), 5000);
          return;
        }

        throw new Error(errorData.message || "Failed to mark arrival");
      }

      const updatedAppointment = await response.json();

      // Show success toast
      setSuccessMessage(
        `${updatedAppointment.patientName} marked as arrived and added to queue`,
      );
      setShowSuccessToast(true);

      // Update appointment status locally
      setAppointments((prev) =>
        prev.map((apt) =>
          apt.appointmentId === appointmentId
            ? { ...apt, status: "Arrived" }
            : apt,
        ),
      );

      // Auto-hide toast after 3 seconds
      setTimeout(() => {
        setShowSuccessToast(false);
      }, 3000);
    } catch (error) {
      console.error("Error marking arrival:", error);
      setErrorMessage(
        error instanceof Error ? error.message : "Failed to mark arrival",
      );
      setShowErrorToast(true);
      setTimeout(() => setShowErrorToast(false), 5000);
    }
  };

  return (
    <>
      {/* Header */}
      <header className="bg-neutral-0 border-b border-neutral-200 shadow-sm mb-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div>
            <h1 className="text-2xl font-bold text-neutral-900">
              Patient Arrival Management
            </h1>
            <p className="mt-1 text-sm text-neutral-600">
              View today's appointments and mark patients as arrived
            </p>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto">
        {/* Search/Filter Section */}
        <div className="mb-6">
          <label
            htmlFor="appointment-search"
            className="block text-sm font-medium text-neutral-700 mb-2"
          >
            Filter Appointments
          </label>
          <div className="relative">
            <input
              id="appointment-search"
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Filter by patient name, DOB, provider, or reason..."
              className="w-full px-4 py-2.5 pl-10 border border-neutral-300 rounded-lg focus:ring-2 
                        focus:ring-primary-500 focus:border-primary-500 transition-colors"
              aria-label="Filter appointments"
            />
            <svg
              className="absolute left-3 top-3 h-5 w-5 text-neutral-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              />
            </svg>
            {searchQuery && (
              <button
                onClick={() => setSearchQuery("")}
                className="absolute right-3 top-3 text-neutral-400 hover:text-neutral-600"
                aria-label="Clear search"
              >
                <svg
                  className="h-5 w-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </button>
            )}
          </div>
          {searchQuery && (
            <p className="mt-2 text-sm text-neutral-600">
              Showing {filteredAppointments.length} of {appointments.length}{" "}
              appointments
            </p>
          )}
        </div>

        {/* Appointments List */}
        <AppointmentsList
          appointments={filteredAppointments}
          onMarkArrived={handleMarkArrived}
          isLoading={isLoading}
        />
      </div>

      {/* Success Toast */}
      {showSuccessToast && (
        <div
          className="fixed bottom-8 right-8 bg-success text-neutral-0 px-6 py-4 rounded-lg shadow-lg 
                        flex items-center gap-3 animate-slide-up z-50 max-w-md"
          role="alert"
          aria-live="polite"
        >
          <svg
            className="w-6 h-6 flex-shrink-0"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M5 13l4 4L19 7"
            />
          </svg>
          <p className="font-medium">{successMessage}</p>
        </div>
      )}

      {/* Error Toast */}
      {showErrorToast && (
        <div
          className="fixed bottom-8 right-8 bg-error text-neutral-0 px-6 py-4 rounded-lg shadow-lg 
                        flex items-center gap-3 animate-slide-up z-50 max-w-md"
          role="alert"
          aria-live="assertive"
        >
          <svg
            className="w-6 h-6 flex-shrink-0"
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
          <p className="font-medium">{errorMessage}</p>
        </div>
      )}
    </>
  );
}
