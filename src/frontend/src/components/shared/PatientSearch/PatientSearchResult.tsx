/**
 * PatientSearchResult Row Component for US_032
 * Displays individual patient search result with hover state
 */

import type { PatientSearchResultProps } from "./types";

/**
 * Format date of birth for display (MM/DD/YYYY)
 */
function formatDateOfBirth(dob: string): string {
  if (!dob) return "";
  const date = new Date(dob);
  return date.toLocaleDateString("en-US", {
    month: "2-digit",
    day: "2-digit",
    year: "numeric",
  });
}

/**
 * Format last appointment date for display
 */
function formatLastAppointment(date: string | null | undefined): string {
  if (!date) return "No previous appointments";
  const appointmentDate = new Date(date);
  return `Last seen: ${appointmentDate.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}`;
}

/**
 * Individual patient search result row
 */
export function PatientSearchResult({
  patient,
  isSelected,
  onSelect,
}: PatientSearchResultProps) {
  return (
    <button
      type="button"
      onClick={onSelect}
      className={`w-full px-4 py-3 text-left transition-colors border-b border-neutral-100 last:border-b-0 
                ${isSelected ? "bg-primary-50" : "hover:bg-neutral-50"}`}
      aria-label={`Select patient: ${patient.fullName}`}
    >
      <div className="flex justify-between items-start gap-4">
        <div className="flex-1 min-w-0">
          {/* Patient Name */}
          <p className="font-semibold text-neutral-900 truncate">
            {patient.fullName}
          </p>

          {/* DOB and Contact Info */}
          <div className="mt-1 space-y-0.5">
            <p className="text-sm text-neutral-600">
              DOB: {formatDateOfBirth(patient.dateOfBirth)}
            </p>

            {patient.email && (
              <p
                className="text-sm text-neutral-600 truncate"
                title={patient.email}
              >
                {patient.email}
              </p>
            )}

            {patient.phone && (
              <p className="text-sm text-neutral-600">{patient.phone}</p>
            )}
          </div>
        </div>

        {/* Last Appointment Date */}
        <div className="flex-shrink-0 text-right">
          <p className="text-xs text-neutral-500">
            {formatLastAppointment(patient.lastAppointmentDate)}
          </p>
        </div>
      </div>
    </button>
  );
}
