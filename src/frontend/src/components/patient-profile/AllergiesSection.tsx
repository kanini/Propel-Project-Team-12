/**
 * Allergies Section component.
 * Displays list of active allergies with severity badges and verification status.
 */

import React from "react";
import type { Allergies } from "../../types/patientProfile.types";
import { VerificationBadge } from "../VerificationBadge";
import { ProfileCompletionBar } from "../ProfileCompletionBar";

interface AllergiesSectionProps {
  allergies: Allergies;
  isStaffView?: boolean;
  className?: string;
}

export const AllergiesSection: React.FC<AllergiesSectionProps> = ({
  allergies,
  isStaffView = false,
  className = "",
}) => {
  const getSeverityStyles = (severity: string): string => {
    const lowerSeverity = severity.toLowerCase();
    if (
      lowerSeverity.includes("critical") ||
      lowerSeverity.includes("severe")
    ) {
      return "bg-red-100 text-red-800 border-red-300";
    }
    if (lowerSeverity.includes("moderate")) {
      return "bg-orange-100 text-orange-800 border-orange-300";
    }
    return "bg-gray-100 text-gray-700 border-gray-300";
  };

  const isCriticalOrSevere = (severity: string): boolean => {
    const lowerSeverity = severity.toLowerCase();
    return (
      lowerSeverity.includes("critical") || lowerSeverity.includes("severe")
    );
  };

  const formatDate = (dateString: string | null): string => {
    if (!dateString) return "";
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("en-US", {
      year: "numeric",
      month: "short",
    }).format(date);
  };

  return (
    <section
      className={`bg-white rounded-lg shadow-sm border border-gray-200 p-6 ${className}`}
      aria-labelledby="allergies-heading"
    >
      <div className="flex items-center justify-between mb-4">
        <h2
          id="allergies-heading"
          className="text-xl font-semibold text-gray-900"
        >
          Known Allergies
        </h2>
        <span className="text-sm text-gray-600">
          {allergies.totalCount} total
        </span>
      </div>

      <ProfileCompletionBar
        verifiedCount={allergies.verifiedCount}
        totalCount={allergies.totalCount}
        className="mb-4"
      />

      {allergies.activeAllergies.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <p className="text-sm">No known allergies</p>
        </div>
      ) : (
        <ul className="space-y-3" role="list">
          {allergies.activeAllergies.map((allergy) => (
            <li
              key={allergy.id}
              className={`border rounded-lg p-4 transition-colors ${
                isCriticalOrSevere(allergy.severity)
                  ? "border-red-300 bg-red-50"
                  : "border-gray-200 hover:bg-gray-50"
              }`}
            >
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <h3 className="text-sm font-semibold text-gray-900">
                      {allergy.allergenName}
                    </h3>
                    <span
                      className={`inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-full border ${getSeverityStyles(allergy.severity)}`}
                    >
                      {allergy.severity}
                    </span>
                  </div>
                  {allergy.reaction && (
                    <p className="text-sm text-gray-600 mt-1">
                      Reaction: {allergy.reaction}
                    </p>
                  )}
                  {allergy.onsetDate && (
                    <p className="text-xs text-gray-500 mt-1">
                      Onset: {formatDate(allergy.onsetDate)}
                    </p>
                  )}
                </div>
                <div className="flex flex-col items-end gap-2">
                  <VerificationBadge badge={allergy.badge} showLabel={false} />
                  {isStaffView && allergy.badge !== "StaffVerified" && (
                    <button
                      type="button"
                      className="text-xs text-blue-600 hover:text-blue-700 font-medium"
                      onClick={() => {
                        /* Verification modal - future story */
                      }}
                    >
                      Verify
                    </button>
                  )}
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
};
