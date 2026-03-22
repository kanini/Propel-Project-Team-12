/**
 * ArrivalManagement Page Component for US_031 - Patient Arrival Status Marking (SCR-020).
 * Staff-only page for searching patients and marking them as arrived.
 */

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrivalSearchInput } from "../../features/staff/components/ArrivalSearchInput";
import { AppointmentCard } from "../../features/staff/components/AppointmentCard";
import type { ArrivalAppointment } from "../../types/arrival";

/**
 * Arrival Management page for staff to mark patient arrivals
 */
export function ArrivalManagement() {
  const navigate = useNavigate();
  const [selectedAppointment, setSelectedAppointment] =
    useState<ArrivalAppointment | null>(null);
  const [showSuccessToast, setShowSuccessToast] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");
  const [showErrorToast, setShowErrorToast] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  /**
   * Handle appointment selection from search
   */
  const handleSelectAppointment = (appointment: ArrivalAppointment) => {
    setSelectedAppointment(appointment);
  };

  /**
   * Handle no appointment found - navigate to walk-in booking
   */
  const handleNoAppointmentFound = (query: string) => {
    navigate(`/staff/walk-in?patientQuery=${encodeURIComponent(query)}`);
  };

  /**
   * Mark patient as arrived
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
          if (selectedAppointment) {
            setSelectedAppointment({
              ...selectedAppointment,
              status: "Arrived",
            });
          }

          setTimeout(() => setShowSuccessToast(false), 5000);
          return;
        }

        throw new Error(errorData.message || "Failed to mark arrival");
      }

      await response.json();

      // Show success toast
      const patientName = selectedAppointment?.patientName || "Patient";
      setSuccessMessage(`${patientName} marked as arrived and added to queue`);
      setShowSuccessToast(true);

      // Update appointment status locally
      if (selectedAppointment) {
        setSelectedAppointment({ ...selectedAppointment, status: "Arrived" });
      }

      // Clear selected appointment after 2 seconds
      setTimeout(() => {
        setSelectedAppointment(null);
        setShowSuccessToast(false);
      }, 2000);
    } catch (error) {
      console.error("Error marking arrival:", error);
      setErrorMessage(
        error instanceof Error ? error.message : "Failed to mark arrival",
      );
      setShowErrorToast(true);
      setTimeout(() => setShowErrorToast(false), 5000);
    }
  };

  /**
   * Clear selected appointment
   */
  const handleClear = () => {
    setSelectedAppointment(null);
  };

  return (
    <div className="min-h-screen bg-neutral-50">
      {/* Header */}
      <header className="bg-neutral-0 border-b border-neutral-200 shadow-sm">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div>
            <h1 className="text-2xl font-bold text-neutral-900">
              Patient Arrival Management
            </h1>
            <p className="mt-1 text-sm text-neutral-600">
              Search for patients with appointments today and mark them as
              arrived
            </p>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Search Section */}
        <div className="mb-6">
          <ArrivalSearchInput
            onSelectAppointment={handleSelectAppointment}
            onNoAppointmentFound={handleNoAppointmentFound}
          />
        </div>

        {/* Selected Appointment Card */}
        {selectedAppointment && (
          <div className="animate-fade-in">
            <AppointmentCard
              appointment={selectedAppointment}
              onMarkArrived={handleMarkArrived}
              onClear={handleClear}
            />
          </div>
        )}

        {/* Empty State - Only show when no appointment selected */}
        {!selectedAppointment && (
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
                d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"
              />
            </svg>
            <h3 className="text-lg font-semibold text-neutral-900 mb-2">
              Search for a Patient
            </h3>
            <p className="text-sm text-neutral-600">
              Use the search bar above to find patients with appointments today
            </p>
          </div>
        )}
      </main>

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
    </div>
  );
}
