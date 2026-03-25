/**
 * EmptyStateIntake component (US_037)
 * Displays empty state when patient has no appointments requiring intake
 * Provides link to book new appointment (UXR-105)
 */

import { memo } from 'react';
import { Link } from 'react-router-dom';

/**
 * EmptyStateIntake - Shows when no appointments need intake
 */
function EmptyStateIntake() {
  return (
    <div
      className="flex flex-col items-center justify-center py-16 px-4 text-center"
      role="status"
      aria-label="No appointments requiring intake"
    >
      {/* Empty state icon */}
      <div className="w-24 h-24 mb-6 rounded-full bg-neutral-100 flex items-center justify-center">
        <svg
          className="w-12 h-12 text-neutral-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          aria-hidden="true"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={1.5}
            d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={1.5}
            d="M6 18l6-6m0 0l6-6m-6 6l-6-6m6 6l6 6"
          />
        </svg>
      </div>

      {/* Heading */}
      <h2 className="text-xl font-semibold text-neutral-900 mb-2">
        No upcoming appointments requiring intake
      </h2>

      {/* Description */}
      <p className="text-neutral-500 max-w-md mb-6">
        You don't have any appointments that need intake forms right now. Book an
        appointment with a provider to get started.
      </p>

      {/* Action button */}
      <Link
        to="/providers"
        className="inline-flex items-center px-6 py-3 bg-primary-600 text-white font-medium rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
      >
        <svg
          className="w-5 h-5 mr-2"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          aria-hidden="true"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M12 6v6m0 0v6m0-6h6m-6 0H6"
          />
        </svg>
        Book Appointment
      </Link>
    </div>
  );
}

export default memo(EmptyStateIntake);
