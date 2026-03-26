/**
 * AppointmentCard Component for US_031 - Display Appointment with Mark Arrived Action
 * Shows appointment details and handles arrival status marking
 */

import { useState } from "react";
import type { ArrivalAppointment } from "../../../types/arrival";
import { RiskBadge } from "../../../components/common/RiskBadge";

interface AppointmentCardProps {
  appointment: ArrivalAppointment;
  onMarkArrived: (appointmentId: string) => Promise<void>;
  onClear: () => void;
}

/**
 * Status badge configuration
 */
const statusConfig = {
  Scheduled: { color: "bg-neutral-100 text-neutral-700", label: "Scheduled" },
  Confirmed: { color: "bg-blue-100 text-blue-700", label: "Confirmed" },
  Arrived: { color: "bg-success text-neutral-0", label: "Arrived" },
  Cancelled: { color: "bg-error text-neutral-0", label: "Cancelled" },
  Completed: { color: "bg-neutral-200 text-neutral-700", label: "Completed" },
  NoShow: { color: "bg-warning text-neutral-900", label: "No Show" },
};

/**
 * Appointment card with mark arrived functionality
 */
export function AppointmentCard({
  appointment,
  onMarkArrived,
  onClear,
}: AppointmentCardProps) {
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [isMarking, setIsMarking] = useState(false);

  const statusInfo = statusConfig[appointment.status] || statusConfig.Scheduled;

  const canMarkArrived =
    appointment.status === "Scheduled" || appointment.status === "Confirmed";
  const isAlreadyArrived = appointment.status === "Arrived";
  const isCancelled = appointment.status === "Cancelled";

  /**
   * Handle mark arrived confirmation
   */
  const handleConfirmMarkArrived = async () => {
    setIsMarking(true);
    try {
      await onMarkArrived(appointment.appointmentId);
      setShowConfirmModal(false);
    } catch (error) {
      console.error("Error marking arrived:", error);
    } finally {
      setIsMarking(false);
    }
  };

  /**
   * Format date/time for display
   */
  const formatDateTime = (dateTime: string) => {
    const date = new Date(dateTime);
    return {
      date: date.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
      }),
      time: date.toLocaleTimeString("en-US", {
        hour: "numeric",
        minute: "2-digit",
        hour12: true,
      }),
    };
  };

  const { date, time } = formatDateTime(appointment.scheduledDateTime);

  return (
    <>
      <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
        {/* Header with Status Badge and Risk Badge */}
        <div className="flex justify-between items-start mb-4">
          <div>
            <h3 className="text-xl font-bold text-neutral-900">
              {appointment.patientName}
            </h3>
            <p className="text-sm text-neutral-600 mt-1">
              DOB: {appointment.dateOfBirth}
            </p>
          </div>
          <div className="flex items-center gap-2">
            {/* US_038 AC-3: Risk indicator badge (only shown when score available) */}
            {appointment.noShowRiskScore != null && appointment.riskLevel && (
              <RiskBadge
                score={appointment.noShowRiskScore}
                riskLevel={appointment.riskLevel}
              />
            )}
            <span
              className={`px-3 py-1 rounded-full text-sm font-medium ${statusInfo.color}`}
              aria-label={`Appointment status: ${statusInfo.label}`}
            >
              {statusInfo.label}
            </span>
          </div>
        </div>

        {/* Appointment Details */}
        <div className="space-y-3 mb-6">
          <div className="flex items-start gap-3">
            <svg
              className="w-5 h-5 text-neutral-500 mt-0.5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
            <div>
              <p className="text-sm font-medium text-neutral-700">
                Appointment Date & Time
              </p>
              <p className="text-sm text-neutral-900">
                {date} at {time}
              </p>
            </div>
          </div>

          <div className="flex items-start gap-3">
            <svg
              className="w-5 h-5 text-neutral-500 mt-0.5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              />
            </svg>
            <div>
              <p className="text-sm font-medium text-neutral-700">Provider</p>
              <p className="text-sm text-neutral-900">
                {appointment.providerName}
              </p>
            </div>
          </div>

          <div className="flex items-start gap-3">
            <svg
              className="w-5 h-5 text-neutral-500 mt-0.5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
            <div>
              <p className="text-sm font-medium text-neutral-700">
                Visit Reason
              </p>
              <p className="text-sm text-neutral-900">
                {appointment.visitReason || "Not specified"}
              </p>
            </div>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-3">
          {canMarkArrived && (
            <button
              onClick={() => setShowConfirmModal(true)}
              className="flex-1 px-6 py-3 bg-primary-500 text-neutral-0 font-semibold rounded-lg 
                                hover:bg-primary-600 focus:outline-none focus:ring-2 focus:ring-primary-500 
                                focus:ring-offset-2 transition-colors"
              aria-label={`Mark ${appointment.patientName} as arrived`}
            >
              Mark Arrived
            </button>
          )}

          {isAlreadyArrived && (
            <div className="flex-1 px-6 py-3 bg-success text-neutral-0 font-semibold rounded-lg text-center">
              ✓ Patient Already Arrived
            </div>
          )}

          {isCancelled && (
            <div className="flex-1 px-6 py-3 bg-neutral-100 text-neutral-600 font-medium rounded-lg text-center">
              Appointment Cancelled — Cannot Mark Arrived
            </div>
          )}

          <button
            onClick={onClear}
            className="px-6 py-3 bg-neutral-100 text-neutral-700 font-medium rounded-lg hover:bg-neutral-200 
                            focus:outline-none focus:ring-2 focus:ring-neutral-500 focus:ring-offset-2 transition-colors"
            aria-label="Clear and search for another patient"
          >
            Clear
          </button>
        </div>
      </div>

      {/* Confirmation Modal */}
      {showConfirmModal && (
        <div
          className="fixed inset-0 bg-neutral-900 bg-opacity-50 z-50 flex items-center justify-center p-4"
          role="dialog"
          aria-modal="true"
          aria-labelledby="modal-title"
        >
          <div className="bg-neutral-0 rounded-lg shadow-xl max-w-md w-full p-6">
            <h3
              id="modal-title"
              className="text-lg font-bold text-neutral-900 mb-2"
            >
              Confirm Patient Arrival
            </h3>
            <p className="text-sm text-neutral-600 mb-6">
              Mark <strong>{appointment.patientName}</strong> as arrived for the{" "}
              <strong>{time}</strong> appointment?
            </p>
            <div className="flex gap-3">
              <button
                onClick={handleConfirmMarkArrived}
                disabled={isMarking}
                className="flex-1 px-4 py-2.5 bg-primary-500 text-neutral-0 font-medium rounded-lg 
                                    hover:bg-primary-600 focus:outline-none focus:ring-2 focus:ring-primary-500 
                                    focus:ring-offset-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isMarking ? "Marking..." : "Confirm"}
              </button>
              <button
                onClick={() => setShowConfirmModal(false)}
                disabled={isMarking}
                className="flex-1 px-4 py-2.5 bg-neutral-100 text-neutral-700 font-medium rounded-lg 
                                    hover:bg-neutral-200 focus:outline-none focus:ring-2 focus:ring-neutral-500 
                                    focus:ring-offset-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
