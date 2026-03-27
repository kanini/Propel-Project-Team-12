/**
 * Demographics Section component.
 * Displays patient demographic information in a 2-column grid.
 */

import React from "react";
import type { Demographics } from "../../types/patientProfile.types";

interface DemographicsSectionProps {
  demographics: Demographics;
  className?: string;
}

export const DemographicsSection: React.FC<DemographicsSectionProps> = ({
  demographics,
  className = "",
}) => {
  const formatDate = (dateString: string | null): string => {
    if (!dateString) return "Not provided";
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
    }).format(date);
  };

  const formatPhone = (phone: string | null): string => {
    if (!phone) return "Not provided";
    // Simple phone formatting: (555) 123-4567
    const cleaned = phone.replace(/\D/g, "");
    if (cleaned.length === 10) {
      return `(${cleaned.substring(0, 3)}) ${cleaned.substring(3, 6)}-${cleaned.substring(6)}`;
    }
    return phone;
  };

  return (
    <section
      className={`bg-white rounded-lg shadow-sm border border-gray-200 p-6 ${className}`}
      aria-labelledby="demographics-heading"
    >
      <h2
        id="demographics-heading"
        className="text-xl font-semibold text-gray-900 mb-4"
      >
        Demographics
      </h2>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Full Name */}
        <div>
          <dt className="text-sm font-medium text-gray-500">Full Name</dt>
          <dd className="mt-1 text-sm text-gray-900">
            {demographics.firstName} {demographics.lastName}
          </dd>
        </div>

        {/* Date of Birth */}
        <div>
          <dt className="text-sm font-medium text-gray-500">Date of Birth</dt>
          <dd className="mt-1 text-sm text-gray-900">
            {formatDate(demographics.dateOfBirth)}
          </dd>
        </div>

        {/* Gender */}
        <div>
          <dt className="text-sm font-medium text-gray-500">Gender</dt>
          <dd className="mt-1 text-sm text-gray-900">
            {demographics.gender || "Not specified"}
          </dd>
        </div>

        {/* Phone Number */}
        <div>
          <dt className="text-sm font-medium text-gray-500">Phone Number</dt>
          <dd className="mt-1 text-sm text-gray-900">
            {formatPhone(demographics.phoneNumber)}
          </dd>
        </div>

        {/* Email */}
        <div>
          <dt className="text-sm font-medium text-gray-500">Email</dt>
          <dd className="mt-1 text-sm text-gray-900">{demographics.email}</dd>
        </div>

        {/* Emergency Contact */}
        <div>
          <dt className="text-sm font-medium text-gray-500">
            Emergency Contact
          </dt>
          <dd className="mt-1 text-sm text-gray-900">
            {demographics.emergencyContact || "Not provided"}
          </dd>
        </div>
      </div>
    </section>
  );
};
