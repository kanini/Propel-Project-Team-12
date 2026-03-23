/**
 * AppointmentsList Component - Display all today's appointments with mark arrival action
 * Part of US_031 - Patient Arrival Management
 */

import type { ArrivalAppointment } from "../../../types/arrival";

interface AppointmentsListProps {
  appointments: ArrivalAppointment[];
  onMarkArrived: (appointmentId: string) => void;
  isLoading?: boolean;
}

/**
 * Display list of today's appointments with arrival marking capability
 */
export function AppointmentsList({
  appointments,
  onMarkArrived,
  isLoading = false,
}: AppointmentsListProps) {
  if (isLoading) {
    return (
      <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-8">
        <div className="flex items-center justify-center">
          <svg
            className="animate-spin h-8 w-8 text-primary-500"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
          <span className="ml-3 text-neutral-600">Loading appointments...</span>
        </div>
      </div>
    );
  }

  if (appointments.length === 0) {
    return (
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
            d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
        </svg>
        <h3 className="text-lg font-semibold text-neutral-900 mb-2">
          No Appointments Today
        </h3>
        <p className="text-sm text-neutral-600">
          There are no scheduled appointments for today
        </p>
      </div>
    );
  }

  return (
    <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm overflow-hidden">
      <div className="px-6 py-4 bg-neutral-50 border-b border-neutral-200">
        <h2 className="text-lg font-semibold text-neutral-900">
          Today's Appointments
        </h2>
        <p className="text-sm text-neutral-600 mt-1">
          {appointments.length} appointment
          {appointments.length !== 1 ? "s" : ""} scheduled
        </p>
      </div>

      <div className="divide-y divide-neutral-200">
        {appointments.map((appointment) => (
          <div
            key={appointment.appointmentId}
            className="px-6 py-4 hover:bg-neutral-50 transition-colors"
          >
            <div className="flex items-center justify-between gap-4">
              {/* Patient Info */}
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-3 mb-2">
                  <h3 className="text-base font-semibold text-neutral-900 truncate">
                    {appointment.patientName}
                  </h3>
                  <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
                      ${
                        appointment.status === "Arrived"
                          ? "bg-success-50 text-success-700"
                          : appointment.status === "Confirmed"
                            ? "bg-primary-50 text-primary-700"
                            : "bg-neutral-100 text-neutral-700"
                      }`}
                  >
                    {appointment.status}
                  </span>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-sm text-neutral-600">
                  <div className="flex items-center gap-2">
                    <svg
                      className="w-4 h-4 text-neutral-400"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      aria-hidden="true"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                    <span>
                      {new Date(
                        appointment.scheduledDateTime,
                      ).toLocaleTimeString("en-US", {
                        hour: "numeric",
                        minute: "2-digit",
                        hour12: true,
                      })}
                    </span>
                  </div>

                  <div className="flex items-center gap-2">
                    <svg
                      className="w-4 h-4 text-neutral-400"
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
                    <span className="truncate">{appointment.providerName}</span>
                  </div>

                  <div className="flex items-center gap-2">
                    <svg
                      className="w-4 h-4 text-neutral-400"
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
                    <span>DOB: {appointment.dateOfBirth}</span>
                  </div>

                  {appointment.visitReason && (
                    <div className="flex items-center gap-2 sm:col-span-2">
                      <svg
                        className="w-4 h-4 text-neutral-400"
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
                      <span className="truncate">
                        {appointment.visitReason}
                      </span>
                    </div>
                  )}
                </div>
              </div>

              {/* Action Button */}
              <div className="flex-shrink-0">
                {appointment.status === "Arrived" ? (
                  <div className="flex items-center gap-2 text-success-600">
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
                        d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                    <span className="text-sm font-medium">Checked In</span>
                  </div>
                ) : (
                  <button
                    onClick={() => onMarkArrived(appointment.appointmentId)}
                    className="px-4 py-2 bg-primary-600 text-neutral-0 rounded-lg hover:bg-primary-700 
                              focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 
                              transition-colors text-sm font-medium whitespace-nowrap"
                    aria-label={`Mark ${appointment.patientName} as arrived`}
                  >
                    Mark Arrived
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
