/**
 * Medications Section component.
 * Displays list of active medications with verification badges and completion bar.
 */

import React from "react";
import type { Medications } from "../../types/patientProfile.types";
import { VerificationBadge } from "../VerificationBadge";
import { ProfileCompletionBar } from "../ProfileCompletionBar";

interface MedicationsSectionProps {
  medications: Medications;
  isStaffView?: boolean;
  className?: string;
}

export const MedicationsSection: React.FC<MedicationsSectionProps> = ({
  medications,
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
      aria-labelledby="medications-heading"
    >
      <div className="flex items-center justify-between mb-4">
        <h2
          id="medications-heading"
          className="text-xl font-semibold text-gray-900"
        >
          Current Medications
        </h2>
        <span className="text-sm text-gray-600">
          {medications.totalCount} total
        </span>
      </div>

      <ProfileCompletionBar
        verifiedCount={medications.verifiedCount}
        totalCount={medications.totalCount}
        className="mb-4"
      />

      {medications.activeMedications.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <p className="text-sm">No current medications</p>
        </div>
      ) : (
        <ul className="space-y-3" role="list">
          {medications.activeMedications.map((medication) => (
            <li
              key={medication.id}
              className={`border rounded-lg p-4 transition-colors ${
                medication.hasConflict
                  ? "border-red-300 bg-red-50"
                  : "border-gray-200 hover:bg-gray-50"
              }`}
            >
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <h3 className="text-sm font-semibold text-gray-900">
                      {medication.drugName}
                    </h3>
                    {medication.hasConflict && (
                      <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-full bg-red-100 text-red-800">
                        ⚠ Conflict
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-gray-600 mt-1">
                    {medication.dosage}, {medication.frequency}
                    {medication.routeOfAdministration &&
                      ` • ${medication.routeOfAdministration}`}
                  </p>
                  {medication.startDate && (
                    <p className="text-xs text-gray-500 mt-1">
                      Started: {formatDate(medication.startDate)}
                      {medication.endDate &&
                        ` • Ended: ${formatDate(medication.endDate)}`}
                    </p>
                  )}
                  <span
                    className={`inline-block mt-2 px-2 py-0.5 text-xs rounded-full ${
                      medication.status === "Active"
                        ? "bg-green-100 text-green-800"
                        : "bg-gray-100 text-gray-700"
                    }`}
                  >
                    {medication.status}
                  </span>
                </div>
                <div className="flex flex-col items-end gap-2">
                  <VerificationBadge
                    badge={medication.badge}
                    showLabel={false}
                  />
                  {isStaffView && medication.badge !== "StaffVerified" && (
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
