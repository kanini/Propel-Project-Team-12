/**
 * WalkinBooking Page Component for US_029 - Staff Walk-in Booking UI
 * Main page for staff to create immediate walk-in appointments
 * Orchestrates patient search, patient creation, and appointment booking
 */

import { useState } from "react";
import {
  PatientSearch,
  PatientSearchContext,
} from "../../../components/shared/PatientSearch";
import type { PatientSearchResult } from "../../../components/shared/PatientSearch";
import { CreatePatientModal } from "../components/CreatePatientModal";
import { WalkinBookingForm } from "../components/WalkinBookingForm";

/**
 * WalkinBooking page for staff to book immediate appointments (US_029)
 */
export function WalkinBooking() {
  const [selectedPatient, setSelectedPatient] =
    useState<PatientSearchResult | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [toast, setToast] = useState<{
    type: "success" | "error";
    message: string;
  } | null>(null);

  /**
   * Handle patient selection from search
   */
  const handlePatientSelect = (patient: PatientSearchResult) => {
    setSelectedPatient(patient);
    setToast(null); // Clear any existing toasts
  };

  /**
   * Handle opening create patient modal
   */
  const handleCreatePatient = () => {
    setIsCreateModalOpen(true);
  };

  /**
   * Handle successful patient creation
   */
  const handlePatientCreated = (patient: PatientSearchResult) => {
    setSelectedPatient(patient);
    setIsCreateModalOpen(false);
    setToast({
      type: "success",
      message: "Patient created successfully",
    });
    // Auto-hide toast after 5 seconds
    setTimeout(() => setToast(null), 5000);
  };

  /**
   * Handle successful appointment booking
   */
  const handleBookingSuccess = () => {
    setToast({
      type: "success",
      message: `Appointment booked successfully for ${selectedPatient?.fullName}`,
    });
    // Clear patient selection to allow new booking
    setSelectedPatient(null);
    // Auto-hide toast after 5 seconds
    setTimeout(() => setToast(null), 5000);
  };

  /**
   * Handle booking error
   */
  const handleBookingError = (error: string) => {
    setToast({
      type: "error",
      message: error,
    });
    // Auto-hide toast after 8 seconds
    setTimeout(() => setToast(null), 8000);
  };

  /**
   * Handle clearing patient selection
   */
  const handleClearPatient = () => {
    setSelectedPatient(null);
    setToast(null);
  };

  /**
   * Close toast notification
   */
  const handleCloseToast = () => {
    setToast(null);
  };

  return (
    <>
      {/* Header */}
      <header className="bg-neutral-0 border-b border-neutral-200 shadow-sm mb-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <h1 className="text-2xl font-bold text-neutral-900">
            Walk-in Appointment Booking
          </h1>
          <p className="mt-1 text-sm text-neutral-600">
            Book immediate appointments for walk-in patients
          </p>
        </div>
      </header>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Left Column - Patient Search */}
          <div>
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-neutral-900 mb-4">
                Step 1: Select Patient
              </h2>

              <PatientSearch
                onSelectPatient={handlePatientSelect}
                onCreatePatient={handleCreatePatient}
                showCreateButton={true}
                placeholder="Search by name, email, or phone..."
                context={PatientSearchContext.WalkinBooking}
                clearOnSelect={false}
              />

              {/* Selected Patient Display */}
              {selectedPatient && (
                <div className="mt-4 bg-primary-50 border border-primary-200 rounded-lg p-4">
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="text-sm font-semibold text-neutral-900">
                        Selected Patient
                      </h3>
                      <p className="text-sm text-neutral-700 mt-1">
                        <span className="font-medium">
                          {selectedPatient.fullName}
                        </span>
                        {" • "}
                        DOB:{" "}
                        {new Date(
                          selectedPatient.dateOfBirth,
                        ).toLocaleDateString("en-US")}
                      </p>
                      {selectedPatient.email && (
                        <p className="text-sm text-neutral-600 mt-1">
                          {selectedPatient.email}
                        </p>
                      )}
                      {selectedPatient.phone && (
                        <p className="text-sm text-neutral-600">
                          {selectedPatient.phone}
                        </p>
                      )}
                    </div>
                    <button
                      onClick={handleClearPatient}
                      className="text-neutral-500 hover:text-neutral-700 focus:outline-none 
                                                focus:ring-2 focus:ring-primary-500 rounded p-1 transition-colors"
                      aria-label="Clear patient selection"
                    >
                      <svg
                        className="w-5 h-5"
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
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Right Column - Booking Form */}
          <div>
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-neutral-900 mb-4">
                Step 2: Book Appointment
              </h2>

              {selectedPatient ? (
                <WalkinBookingForm
                  patient={selectedPatient}
                  onSuccess={handleBookingSuccess}
                  onError={handleBookingError}
                />
              ) : (
                <div className="text-center py-12">
                  <svg
                    className="w-16 h-16 mx-auto text-neutral-300 mb-3"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    aria-hidden="true"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={1.5}
                      d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                    />
                  </svg>
                  <p className="text-sm text-neutral-500">
                    Select a patient to begin booking
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Toast Notification */}
      {toast && (
        <div
          className="fixed bottom-6 right-6 max-w-md w-full bg-neutral-0 border rounded-lg shadow-lg 
                        animate-slide-up z-50"
          role="alert"
          aria-live="polite"
        >
          <div
            className={`flex items-start gap-3 p-4 border-l-4 ${
              toast.type === "success"
                ? "border-success bg-success-50"
                : "border-error bg-error-50"
            }`}
          >
            {/* Icon */}
            <div className="flex-shrink-0">
              {toast.type === "success" ? (
                <svg
                  className="w-6 h-6 text-success"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              ) : (
                <svg
                  className="w-6 h-6 text-error"
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
              )}
            </div>

            {/* Message */}
            <div className="flex-1">
              <p
                className={`text-sm font-medium ${
                  toast.type === "success" ? "text-success" : "text-error"
                }`}
              >
                {toast.type === "success" ? "Success" : "Error"}
              </p>
              <p className="mt-1 text-sm text-neutral-700">{toast.message}</p>
            </div>

            {/* Close button */}
            <button
              onClick={handleCloseToast}
              className="flex-shrink-0 text-neutral-400 hover:text-neutral-600 
                                focus:outline-none focus:ring-2 focus:ring-primary-500 rounded p-1 
                                transition-colors"
              aria-label="Close notification"
            >
              <svg
                className="w-5 h-5"
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
          </div>
        </div>
      )}

      {/* Create Patient Modal */}
      <CreatePatientModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onPatientCreated={handlePatientCreated}
      />
    </>
  );
}
