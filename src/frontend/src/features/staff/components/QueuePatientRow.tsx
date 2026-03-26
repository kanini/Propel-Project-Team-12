/**
 * QueuePatientRow Component for US_030 - Queue Management.
 * Individual patient row in queue with priority flag action.
 */

import { calculateWaitTime } from "../../../utils/queueHelpers";
import type { QueuePatient } from "../../../utils/queueHelpers";

interface QueuePatientRowProps {
  patient: QueuePatient;
  onTogglePriority: (patientId: string, isPriority: boolean) => Promise<void>;
}

/**
 * Patient row component for queue display
 */
export function QueuePatientRow({
  patient,
  onTogglePriority,
}: QueuePatientRowProps) {
  const [isUpdating, setIsUpdating] = React.useState(false);

  const handleTogglePriority = async () => {
    setIsUpdating(true);
    try {
      await onTogglePriority(patient.patientId, !patient.isPriority);
    } catch (error) {
      console.error("Failed to toggle priority:", error);
    } finally {
      setIsUpdating(false);
    }
  };

  const waitTime = calculateWaitTime(patient.arrivalTime);
  const arrivalTimeFormatted = patient.arrivalTime
    ? new Date(patient.arrivalTime).toLocaleTimeString("en-US", {
        hour: "numeric",
        minute: "2-digit",
        hour12: true,
      })
    : "-";

  return (
    <tr className="border-b border-neutral-200 hover:bg-neutral-50 transition-colors">
      {/* Position */}
      <td className="px-4 py-3 text-sm text-neutral-900 font-medium">
        {patient.position}
      </td>

      {/* Patient Name */}
      <td className="px-4 py-3 text-sm text-neutral-900">
        {patient.patientName}
      </td>

      {/* Appointment Type */}
      <td className="px-4 py-3">
        <span
          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
            patient.appointmentType === "Walk-in"
              ? "bg-primary-100 text-primary-700"
              : "bg-secondary-100 text-secondary-700"
          }`}
        >
          {patient.appointmentType}
        </span>
      </td>

      {/* Provider */}
      <td className="px-4 py-3 text-sm text-neutral-700">
        {patient.providerName}
      </td>

      {/* Arrival Time */}
      <td className="px-4 py-3 text-sm text-neutral-700">
        {arrivalTimeFormatted}
      </td>

      {/* Estimated Wait Time */}
      <td className="px-4 py-3 text-sm text-neutral-900 font-medium">
        {waitTime}
      </td>

      {/* Priority Badge */}
      <td className="px-4 py-3">
        {patient.isPriority && (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-error text-neutral-0">
            Emergency
          </span>
        )}
      </td>

      {/* Actions */}
      <td className="px-4 py-3 text-right">
        <button
          onClick={handleTogglePriority}
          disabled={isUpdating}
          className={`inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium rounded-lg transition-colors
                        ${
                          patient.isPriority
                            ? "text-neutral-700 bg-neutral-100 hover:bg-neutral-200"
                            : "text-error bg-error-50 hover:bg-error-100"
                        }
                        focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2
                        disabled:opacity-50 disabled:cursor-not-allowed`}
          aria-label={
            patient.isPriority ? "Remove priority" : "Flag as priority"
          }
        >
          {patient.isPriority ? (
            <>
              <svg
                className="w-4 h-4"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
              <span>Remove</span>
            </>
          ) : (
            <>
              <svg
                className="w-4 h-4"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9"
                />
              </svg>
              <span>Priority</span>
            </>
          )}
        </button>
      </td>
    </tr>
  );
}

import React from "react";
