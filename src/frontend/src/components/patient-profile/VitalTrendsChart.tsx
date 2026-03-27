/**
 * Vital Trends Chart component.
 * Displays vital signs data in a simple tabular format.
 * TODO: Replace with Recharts LineChart once recharts is installed (npm install recharts)
 */

import React, { useState } from "react";
import type { VitalTrends } from "../../types/patientProfile.types";

interface VitalTrendsChartProps {
  vitalTrends: VitalTrends;
  className?: string;
}

export const VitalTrendsChart: React.FC<VitalTrendsChartProps> = ({
  vitalTrends,
  className = "",
}) => {
  const [selectedVitalType, setSelectedVitalType] = useState<
    "bloodPressure" | "heartRate" | "temperature" | "weight"
  >("bloodPressure");

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    }).format(date);
  };

  const vitalTypeLabels = {
    bloodPressure: "Blood Pressure",
    heartRate: "Heart Rate",
    temperature: "Temperature",
    weight: "Weight",
  };

  const selectedData = vitalTrends[selectedVitalType];
  const hasData = selectedData && selectedData.length > 0;

  return (
    <section
      className={`bg-white rounded-lg shadow-sm border border-gray-200 p-6 ${className}`}
      aria-labelledby="vital-trends-heading"
    >
      <div className="flex items-center justify-between mb-4">
        <h2
          id="vital-trends-heading"
          className="text-xl font-semibold text-gray-900"
        >
          Vital Signs Trends
        </h2>
        <span className="text-xs text-gray-500">
          {formatDate(vitalTrends.rangeStart)} -{" "}
          {formatDate(vitalTrends.rangeEnd)}
        </span>
      </div>

      {/* Vital type selector */}
      <div className="flex gap-2 mb-4 flex-wrap">
        {(
          Object.keys(vitalTypeLabels) as Array<keyof typeof vitalTypeLabels>
        ).map((type) => (
          <button
            key={type}
            type="button"
            onClick={() => setSelectedVitalType(type)}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
              selectedVitalType === type
                ? "bg-blue-600 text-white"
                : "bg-gray-100 text-gray-700 hover:bg-gray-200"
            }`}
          >
            {vitalTypeLabels[type]}
          </button>
        ))}
      </div>

      {/* Chart placeholder - will be replaced with Recharts LineChart */}
      {!hasData ? (
        <div className="text-center py-12 text-gray-500 bg-gray-50 rounded-lg">
          <svg
            className="mx-auto h-12 w-12 text-gray-400"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
            />
          </svg>
          <p className="mt-4 text-sm">
            No {vitalTypeLabels[selectedVitalType].toLowerCase()} data recorded
          </p>
          <p className="mt-1 text-xs text-gray-400">
            Upload clinical documents to populate vital trends
          </p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th
                  scope="col"
                  className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  Date
                </th>
                <th
                  scope="col"
                  className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  Value
                </th>
                <th
                  scope="col"
                  className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  Unit
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {selectedData.map((dataPoint, index) => (
                <tr key={index} className="hover:bg-gray-50">
                  <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900">
                    {formatDate(dataPoint.recordedAt)}
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap text-sm font-medium text-gray-900">
                    {dataPoint.value}
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-600">
                    {dataPoint.unit}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
        <p className="text-xs text-blue-800">
          <strong>Note:</strong> Chart visualization will be enhanced with
          interactive graphs once Recharts library is installed.
        </p>
      </div>
    </section>
  );
};
