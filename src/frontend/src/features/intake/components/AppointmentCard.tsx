/**
 * AppointmentCard component (US_037)
 * Displays appointment details with intake status and action button
 * Keyboard accessible with focus indicators (UXR-203)
 */

import { memo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import type { IntakeAppointment, IntakeStatus } from '../../../types/intakeAppointment';
import IntakeStatusBadge from './IntakeStatusBadge';

interface AppointmentCardProps {
  appointment: IntakeAppointment;
}

/**
 * Action button configuration based on intake status
 */
const actionConfig: Record<IntakeStatus, { text: string; className: string }> = {
  pending: {
    text: 'Start Intake',
    className: 'bg-primary-600 hover:bg-primary-700 text-white',
  },
  inProgress: {
    text: 'Continue Intake',
    className: 'bg-blue-600 hover:bg-blue-700 text-white',
  },
  completed: {
    text: 'Edit Intake',
    className: 'bg-neutral-600 hover:bg-neutral-700 text-white',
  },
};

/**
 * Format date string to human-readable format
 */
function formatDate(dateStr: string): string {
  const date = new Date(dateStr);
  return date.toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

/**
 * Format time string to 12-hour format
 */
function formatTime(timeStr: string): string {
  // Handle both "HH:mm" and "HH:mm:ss" formats
  const parts = timeStr.split(':').map(Number);
  const hours = parts[0] ?? 0;
  const minutes = parts[1] ?? 0;
  const period = hours >= 12 ? 'PM' : 'AM';
  const displayHours = hours % 12 || 12;
  return `${displayHours}:${minutes.toString().padStart(2, '0')} ${period}`;
}

/**
 * Get provider initials for avatar
 */
function getInitials(name: string): string {
  return name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

/**
 * AppointmentCard - Displays an appointment with intake action
 * @param appointment - The appointment data to display
 */
function AppointmentCard({ appointment }: AppointmentCardProps) {
  const navigate = useNavigate();
  const action = actionConfig[appointment.intakeStatus];

  const handleClick = useCallback(() => {
    navigate(`/intake/${appointment.appointmentId}`);
  }, [navigate, appointment.appointmentId]);

  const handleKeyDown = useCallback(
    (event: React.KeyboardEvent) => {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        handleClick();
      }
    },
    [handleClick]
  );

  return (
    <article
      className="bg-white shadow-md rounded-lg p-4 hover:shadow-lg transition-shadow duration-200 border border-neutral-200"
      role="article"
      aria-label={`Appointment with ${appointment.providerName} on ${formatDate(appointment.appointmentDate)}`}
    >
      <div className="flex items-start gap-4">
        {/* Provider Avatar */}
        <div
          className="w-12 h-12 rounded-full bg-primary-100 text-primary-700 flex items-center justify-center font-semibold text-sm flex-shrink-0"
          aria-hidden="true"
        >
          {getInitials(appointment.providerName)}
        </div>

        {/* Appointment Details */}
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2 mb-1">
            <h3 className="font-semibold text-neutral-900 truncate">
              {appointment.providerName}
            </h3>
            <IntakeStatusBadge status={appointment.intakeStatus} />
          </div>

          <p className="text-sm text-neutral-600 mb-2">{appointment.providerSpecialty}</p>

          <div className="flex items-center gap-4 text-sm text-neutral-500">
            {/* Date */}
            <span className="flex items-center gap-1">
              <svg
                className="w-4 h-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
              <span>{formatDate(appointment.appointmentDate)}</span>
            </span>

            {/* Time */}
            <span className="flex items-center gap-1">
              <svg
                className="w-4 h-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <span>{formatTime(appointment.appointmentTime)}</span>
            </span>
          </div>
        </div>
      </div>

      {/* Action Button */}
      <div className="mt-4 flex justify-end">
        <button
          type="button"
          onClick={handleClick}
          onKeyDown={handleKeyDown}
          className={`px-4 py-2 rounded-md text-sm font-medium focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors ${action.className}`}
          aria-label={`${action.text} for appointment with ${appointment.providerName}`}
        >
          {action.text}
        </button>
      </div>
    </article>
  );
}

export default memo(AppointmentCard);
