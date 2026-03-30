/**
 * MedicalCodesPanel component for SCR-016 Health Dashboard 360°
 * Displays ICD-10 and CPT medical codes extracted from documents
 */

import type { MedicalCodeDto } from '../../types/clinicalData';
import { ConfidenceBadge } from './ConfidenceBadge';

interface MedicalCodesPanelProps {
  codes: MedicalCodeDto[];
}

export function MedicalCodesPanel({ codes }: MedicalCodesPanelProps) {
  const icd10Codes = codes.filter(c => c.codeSystem === 'ICD10');
  const cptCodes = codes.filter(c => c.codeSystem === 'CPT');

  if (codes.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
        <h3 className="text-lg font-semibold text-neutral-900 mb-3">Medical Codes</h3>
        <p className="text-neutral-500 text-sm">No medical codes extracted yet.</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
      <h3 className="text-lg font-semibold text-neutral-900 mb-4">Medical Codes</h3>

      {icd10Codes.length > 0 && (
        <div className="mb-6">
          <h4 className="text-sm font-semibold text-neutral-700 mb-2 flex items-center gap-2">
            <span className="inline-block w-2 h-2 rounded-full bg-blue-500" />
            ICD-10 Diagnosis Codes ({icd10Codes.length})
          </h4>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-neutral-200">
                  <th className="text-left py-2 pr-4 font-medium text-neutral-600">Code</th>
                  <th className="text-left py-2 pr-4 font-medium text-neutral-600">Description</th>
                  <th className="text-left py-2 font-medium text-neutral-600">Status</th>
                </tr>
              </thead>
              <tbody>
                {icd10Codes.map((code) => (
                  <tr key={code.medicalCodeId ?? code.codeValue} className="border-b border-neutral-50">
                    <td className="py-2 pr-4 font-mono font-medium text-blue-700">{code.codeValue}</td>
                    <td className="py-2 pr-4 text-neutral-700">{code.codeDescription}</td>
                    <td className="py-2">
                      <ConfidenceBadge
                        confidenceScore={code.confidenceScore}
                        verificationStatus={code.verificationStatus}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {cptCodes.length > 0 && (
        <div>
          <h4 className="text-sm font-semibold text-neutral-700 mb-2 flex items-center gap-2">
            <span className="inline-block w-2 h-2 rounded-full bg-purple-500" />
            CPT Procedure Codes ({cptCodes.length})
          </h4>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-neutral-200">
                  <th className="text-left py-2 pr-4 font-medium text-neutral-600">Code</th>
                  <th className="text-left py-2 pr-4 font-medium text-neutral-600">Description</th>
                  <th className="text-left py-2 font-medium text-neutral-600">Status</th>
                </tr>
              </thead>
              <tbody>
                {cptCodes.map((code) => (
                  <tr key={code.medicalCodeId ?? code.codeValue} className="border-b border-neutral-50">
                    <td className="py-2 pr-4 font-mono font-medium text-purple-700">{code.codeValue}</td>
                    <td className="py-2 pr-4 text-neutral-700">{code.codeDescription}</td>
                    <td className="py-2">
                      <ConfidenceBadge
                        confidenceScore={code.confidenceScore}
                        verificationStatus={code.verificationStatus}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
