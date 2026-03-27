/**
 * Conditions Section component.
 * Displays list of active conditions with verification badges and completion bar.
 */

import React from "react";
import type { Conditions } from "../../types/patientProfile.types";
import { VerificationBadge } from "../VerificationBadge";
import { ProfileCompletionBar } from "../ProfileCompletionBar";

interface ConditionsSectionProps {
  conditions: Conditions;
  isStaffView?: boolean;
  className?: string;
}

export const ConditionsSection: React.FC<ConditionsSectionProps> = ({
  conditions,
  isStaffView = false,
  className = "",
}) => {
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
      aria-labelledby="conditions-heading"
    >
      <div className="flex items-center justify-between mb-4">
        <h2
          id="conditions-heading"
          className="text-xl font-semibold text-gray-900"
        >
          Active Conditions
        </h2>
        <span className="text-sm text-gray-600">
          {conditions.totalCount} total
        </span>
      </div>

      <ProfileCompletionBar
        verifiedCount={conditions.verifiedCount}
        totalCount={conditions.totalCount}
        className="mb-4"
      />

      {conditions.activeConditions.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <p className="text-sm">No active conditions recorded</p>
        </div>
      ) : (
        <ul className="space-y-3" role="list">
          {conditions.activeConditions.map((condition) => (
            <li
              key={condition.id}
              className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50 transition-colors"
            >
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1 min-w-0">
                  <h3 className="text-sm font-semibold text-gray-900">
                    {condition.conditionName}
                    {condition.icd10Code && (
                      <span className="ml-2 text-gray-500 font-normal">
                        ({condition.icd10Code})
                      </span>
                    )}
                  </h3>
                  {condition.diagnosisDate && (
                    <p className="text-xs text-gray-600 mt-1">
                      Diagnosed: {formatDate(condition.diagnosisDate)}
                    </p>
                  )}
                  {condition.severity && (
                    <span className="inline-block mt-2 px-2 py-0.5 text-xs rounded-full bg-gray-100 text-gray-700">
                      {condition.severity}
                    </span>
                  )}
                </div>
                <div className="flex flex-col items-end gap-2">
                  <VerificationBadge
                    badge={condition.badge}
                    showLabel={false}
                  />
                  {isStaffView && condition.badge !== "StaffVerified" && (
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
