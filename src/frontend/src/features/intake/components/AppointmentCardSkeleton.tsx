/**
 * AppointmentCardSkeleton component (US_037)
 * Loading skeleton for appointment cards
 */

import { memo } from 'react';

/**
 * AppointmentCardSkeleton - Loading placeholder for appointment cards
 */
function AppointmentCardSkeleton() {
  return (
    <div
      className="bg-white shadow-md rounded-lg p-4 border border-neutral-200 animate-pulse"
      aria-hidden="true"
    >
      <div className="flex items-start gap-4">
        {/* Avatar skeleton */}
        <div className="w-12 h-12 rounded-full bg-neutral-200 flex-shrink-0" />

        {/* Content skeleton */}
        <div className="flex-1">
          <div className="flex items-start justify-between gap-2 mb-1">
            <div className="h-5 bg-neutral-200 rounded w-32" />
            <div className="h-5 bg-neutral-200 rounded-full w-20" />
          </div>
          <div className="h-4 bg-neutral-200 rounded w-24 mb-2" />
          <div className="flex items-center gap-4">
            <div className="h-4 bg-neutral-200 rounded w-28" />
            <div className="h-4 bg-neutral-200 rounded w-16" />
          </div>
        </div>
      </div>

      {/* Button skeleton */}
      <div className="mt-4 flex justify-end">
        <div className="h-9 bg-neutral-200 rounded w-28" />
      </div>
    </div>
  );
}

export default memo(AppointmentCardSkeleton);
