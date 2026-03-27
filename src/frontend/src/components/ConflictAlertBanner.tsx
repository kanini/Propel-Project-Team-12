/**
 * ConflictAlertBanner Component (US_048, AC2)
 * Prominently displayed banner at top of patient view showing conflict summary.
 * Background color and severity based on highest-priority unresolved conflict.
 */

import type { ConflictSummary } from "../types/conflict.types";
import { ConflictSeverity } from "../types/conflict.types";
import { SeverityBadge } from "./SeverityBadge";

interface ConflictAlertBannerProps {
  summary: ConflictSummary;
  onViewConflicts: () => void;
  className?: string;
}

export const ConflictAlertBanner = ({
  summary,
  onViewConflicts,
  className = "",
}: ConflictAlertBannerProps) => {
  // Don't render if no unresolved conflicts
  if (summary.totalUnresolved === 0) {
    return null;
  }

  // Determine highest severity for styling
  const highestSeverity =
    summary.criticalCount > 0
      ? ConflictSeverity.Critical
      : summary.warningCount > 0
        ? ConflictSeverity.Warning
        : ConflictSeverity.Info;

  // Background color based on highest severity
  const bgColor =
    highestSeverity === ConflictSeverity.Critical
      ? "bg-red-50"
      : highestSeverity === ConflictSeverity.Warning
        ? "bg-amber-50"
        : "bg-blue-50";

  const borderColor =
    highestSeverity === ConflictSeverity.Critical
      ? "border-red-200"
      : highestSeverity === ConflictSeverity.Warning
        ? "border-amber-200"
        : "border-blue-200";

  const iconColor =
    highestSeverity === ConflictSeverity.Critical
      ? "text-red-600"
      : highestSeverity === ConflictSeverity.Warning
        ? "text-amber-600"
        : "text-blue-600";

  return (
    <div
      className={`${bgColor} border ${borderColor} rounded-lg p-4 mb-6 ${className}`}
      role={highestSeverity === ConflictSeverity.Critical ? "alert" : "status"}
      aria-live={
        highestSeverity === ConflictSeverity.Critical ? "assertive" : "polite"
      }
    >
      <div className="flex items-center justify-between flex-wrap gap-4">
        {/* Left section: Icon + Heading */}
        <div className="flex items-center gap-3">
          <svg
            className={`h-6 w-6 ${iconColor} flex-shrink-0`}
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
          <div>
            <h3 className={`text-sm font-semibold ${iconColor}`}>
              {highestSeverity === ConflictSeverity.Critical &&
                "Critical Conflicts Detected"}
              {highestSeverity === ConflictSeverity.Warning &&
                "Conflicts Require Review"}
              {highestSeverity === ConflictSeverity.Info &&
                "Minor Conflicts Detected"}
            </h3>
            <p className="text-sm text-gray-700 mt-1">
              {summary.totalUnresolved} unresolved conflict
              {summary.totalUnresolved !== 1 ? "s" : ""} requiring staff
              verification
            </p>
          </div>
        </div>

        {/* Center section: Counts */}
        <div className="flex items-center gap-2 flex-wrap">
          {summary.criticalCount > 0 && (
            <SeverityBadge
              severity={ConflictSeverity.Critical}
              showIcon={false}
            />
          )}
          {summary.warningCount > 0 && (
            <SeverityBadge
              severity={ConflictSeverity.Warning}
              showIcon={false}
            />
          )}
          {summary.infoCount > 0 && (
            <SeverityBadge severity={ConflictSeverity.Info} showIcon={false} />
          )}
          <span className="text-sm text-gray-600">
            {summary.criticalCount > 0 && `${summary.criticalCount} Critical`}
            {summary.criticalCount > 0 &&
              (summary.warningCount > 0 || summary.infoCount > 0) &&
              ", "}
            {summary.warningCount > 0 && `${summary.warningCount} Warning`}
            {summary.warningCount > 0 && summary.infoCount > 0 && ", "}
            {summary.infoCount > 0 && `${summary.infoCount} Info`}
          </span>
        </div>

        {/* Right section: Action button */}
        <button
          onClick={onViewConflicts}
          className="px-4 py-2 bg-white border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
        >
          View All Conflicts
        </button>
      </div>
    </div>
  );
};
