/**
 * Verification Badge component (UXR-402).
 * Displays amber badge for AI-suggested data, green badge for staff-verified data.
 */

import React from "react";
import type { VerificationBadge as VerificationBadgeType } from "../types/patientProfile.types";

interface VerificationBadgeProps {
  badge: VerificationBadgeType;
  showLabel?: boolean;
  className?: string;
}

export const VerificationBadge: React.FC<VerificationBadgeProps> = ({
  badge,
  showLabel = true,
  className = "",
}) => {
  const isVerified = badge === "StaffVerified";

  const badgeStyles = isVerified
    ? "bg-green-100 text-green-800 border-green-300"
    : "bg-amber-100 text-amber-800 border-amber-300";

  const iconPath = isVerified
    ? "M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" // CheckCircle
    : "M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09zM18.259 8.715L18 9.75l-.259-1.035a3.375 3.375 0 00-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 002.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 002.456 2.456L21.75 6l-1.035.259a3.375 3.375 0 00-2.456 2.456zM16.894 20.567L16.5 21.75l-.394-1.183a2.25 2.25 0 00-1.423-1.423L13.5 18.75l1.183-.394a2.25 2.25 0 001.423-1.423l.394-1.183.394 1.183a2.25 2.25 0 001.423 1.423l1.183.394-1.183.394a2.25 2.25 0 00-1.423 1.423z"; // Sparkles

  const label = isVerified ? "Staff-verified" : "AI-suggested";
  const tooltipText = isVerified
    ? "This data has been manually reviewed and verified by staff"
    : "This data was extracted by AI and is pending staff verification";

  return (
    <span
      className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium border ${badgeStyles} ${className}`}
      title={tooltipText}
      aria-label={`${label} data`}
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        strokeWidth={1.5}
        stroke="currentColor"
        className="w-4 h-4"
        aria-hidden="true"
      >
        <path strokeLinecap="round" strokeLinejoin="round" d={iconPath} />
      </svg>
      {showLabel && <span>{label}</span>}
    </span>
  );
};
