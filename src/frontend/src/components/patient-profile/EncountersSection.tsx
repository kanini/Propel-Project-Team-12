/**
 * Encounters Section component.
 * Displays recent patient encounters in a responsive table/card layout.
 */

import React from "react";
import type { Encounters } from "../../types/patientProfile.types";

interface EncountersSectionProps {
  encounters: Encounters;
  className?: string;
}

export const EncountersSection: React.FC<EncountersSectionProps> = ({
  encounters,
  className = "",
}) => {
  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    }).format(date);
  };

  const getEncounterTypeStyles = (type: string): string => {
    const lowerType = type.toLowerCase();
    if (lowerType.includes("inpatient")) return "bg-blue-100 text-blue-800";
    if (lowerType.includes("outpatient")) return "bg-green-100 text-green-800";
    if (lowerType.includes("emergency")) return "bg-red-100 text-red-800";
    if (lowerType.includes("telehealth"))
      return "bg-purple-100 text-purple-800";
    return "bg-gray-100 text-gray-800";
  };

  return (
    <section
      className={`bg-white rounded-lg shadow-sm border border-gray-200 p-6 ${className}`}
      aria-labelledby="encounters-heading"
    >
      <div className="flex items-center justify-between mb-4">
        <h2
          id="encounters-heading"
          className="text-xl font-semibold text-gray-900"
        >
          Recent Encounters
        </h2>
        <span className="text-sm text-gray-600">
          {encounters.totalCount} total
        </span>
      </div>

      {encounters.recentEncounters.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <p className="text-sm">No recent encounters</p>
        </div>
      ) : (
        <>
          {/* Desktop table view (hidden on mobile) */}
          <div className="hidden md:block overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th
                    scope="col"
                    className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  >
                    Date
                  </th>
                  <th
                    scope="col"
                    className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  >
                    Type
                  </th>
                  <th
                    scope="col"
                    className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  >
                    Provider
                  </th>
                  <th
                    scope="col"
                    className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  >
                    Facility
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {encounters.recentEncounters.map((encounter) => (
                  <tr key={encounter.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900">
                      {formatDate(encounter.encounterDate)}
                    </td>
                    <td className="px-4 py-3 whitespace-nowrap">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getEncounterTypeStyles(encounter.encounterType)}`}
                      >
                        {encounter.encounterType}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {encounter.provider || "N/A"}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {encounter.facility || "N/A"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Mobile card view (visible only on mobile) */}
          <div className="md:hidden space-y-3">
            {encounters.recentEncounters.map((encounter) => (
              <div
                key={encounter.id}
                className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-start justify-between mb-2">
                  <span className="text-sm font-semibold text-gray-900">
                    {formatDate(encounter.encounterDate)}
                  </span>
                  <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getEncounterTypeStyles(encounter.encounterType)}`}
                  >
                    {encounter.encounterType}
                  </span>
                </div>
                <div className="space-y-1 text-sm">
                  <p className="text-gray-600">
                    <span className="font-medium">Provider:</span>{" "}
                    {encounter.provider || "N/A"}
                  </p>
                  <p className="text-gray-600">
                    <span className="font-medium">Facility:</span>{" "}
                    {encounter.facility || "N/A"}
                  </p>
                  {encounter.chiefComplaint && (
                    <p className="text-gray-600 mt-2">
                      <span className="font-medium">Chief Complaint:</span>{" "}
                      {encounter.chiefComplaint}
                    </p>
                  )}
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </section>
  );
};
