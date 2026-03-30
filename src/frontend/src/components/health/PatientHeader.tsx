/**
 * PatientHeader component for SCR-016 Health Dashboard 360°
 * Displays patient demographics at the top of the health dashboard
 */

import type { PatientDemographicsDto } from '../../types/clinicalData';

interface PatientHeaderProps {
  demographics: PatientDemographicsDto;
}

export function PatientHeader({ demographics }: PatientHeaderProps) {
  const initials = demographics.name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);

  return (
    <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
      <div className="flex items-center gap-4">
        <div className="w-14 h-14 rounded-full bg-blue-600 text-white flex items-center justify-center text-xl font-bold">
          {initials}
        </div>
        <div className="flex-1">
          <h2 className="text-xl font-bold text-neutral-900">{demographics.name}</h2>
          <div className="flex flex-wrap gap-x-6 gap-y-1 mt-1 text-sm text-neutral-600">
            {demographics.dateOfBirth && (
              <span>DOB: {demographics.dateOfBirth}</span>
            )}
            {demographics.mrn && <span>MRN: {demographics.mrn}</span>}
            {demographics.bloodType && <span>Blood: {demographics.bloodType}</span>}
          </div>
        </div>
        <div className="hidden md:flex flex-col text-sm text-neutral-500 gap-1 text-right">
          {demographics.phone && <span>{demographics.phone}</span>}
          {demographics.email && <span>{demographics.email}</span>}
          {demographics.insurance && <span>{demographics.insurance}</span>}
        </div>
      </div>
    </div>
  );
}
