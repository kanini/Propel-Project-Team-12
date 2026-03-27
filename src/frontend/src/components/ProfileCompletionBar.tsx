/**
 * Profile Completion Bar component.
 * Displays progress bar showing percentage of verified data items.
 */

import React from "react";

interface ProfileCompletionBarProps {
  verifiedCount: number;
  totalCount: number;
  className?: string;
}

export const ProfileCompletionBar: React.FC<ProfileCompletionBarProps> = ({
  verifiedCount,
  totalCount,
  className = "",
}) => {
  const percentage =
    totalCount > 0 ? Math.round((verifiedCount / totalCount) * 100) : 0;

  return (
    <div className={`w-full ${className}`}>
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm text-gray-700 font-medium">
          Data Verification Status
        </span>
        <span className="text-sm text-gray-600">
          {verifiedCount} of {totalCount} verified
        </span>
      </div>
      <div
        className="w-full bg-gray-200 rounded-full h-2"
        role="progressbar"
        aria-valuenow={percentage}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`${percentage}% of data verified by staff`}
      >
        <div
          className="bg-green-600 h-2 rounded-full transition-all duration-300 ease-in-out"
          style={{ width: `${percentage}%` }}
        />
      </div>
      <p className="text-xs text-gray-500 mt-1">{percentage}% verified</p>
    </div>
  );
};
