/**
 * ClinicalDataSection component for SCR-016 Health Dashboard 360°
 * Renders a list of clinical items (conditions, medications, allergies, vitals, lab results)
 * with AI confidence badges and verification status indicators
 */

import type { ClinicalItemDto } from '../../types/clinicalData';
import { ConfidenceBadge } from './ConfidenceBadge';

interface ClinicalDataSectionProps {
  title: string;
  items: ClinicalItemDto[];
  emptyMessage?: string;
}

export function ClinicalDataSection({ title, items, emptyMessage }: ClinicalDataSectionProps) {
  if (items.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
        <h3 className="text-lg font-semibold text-neutral-900 mb-3">{title}</h3>
        <p className="text-neutral-500 text-sm">{emptyMessage ?? 'No data extracted yet.'}</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-neutral-900">{title}</h3>
        <span className="text-sm text-neutral-500">{items.length} item{items.length !== 1 ? 's' : ''}</span>
      </div>
      <div className="space-y-3">
        {items.map((item) => (
          <div
            key={item.extractedDataId}
            className="flex items-start justify-between gap-4 p-3 rounded-md border border-neutral-100 hover:bg-neutral-50 transition-colors"
          >
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <span className="font-medium text-neutral-900 text-sm">{item.dataKey}</span>
                <ConfidenceBadge
                  confidenceScore={item.confidenceScore}
                  verificationStatus={item.verificationStatus}
                />
              </div>
              <p className="text-sm text-neutral-700">{item.dataValue}</p>
              {item.sourceTextExcerpt && (
                <p className="text-xs text-neutral-400 mt-1 italic truncate">
                  Source: &ldquo;{item.sourceTextExcerpt}&rdquo;
                </p>
              )}
              {item.medicalCodes.length > 0 && (
                <div className="mt-2 flex flex-wrap gap-1">
                  {item.medicalCodes.map((code) => (
                    <span
                      key={code.medicalCodeId ?? code.codeValue}
                      className="inline-flex items-center rounded-md bg-indigo-50 px-2 py-0.5 text-xs font-medium text-indigo-700 ring-1 ring-inset ring-indigo-200"
                      title={code.codeDescription}
                    >
                      {code.codeSystem}: {code.codeValue}
                    </span>
                  ))}
                </div>
              )}
            </div>
            {item.source && (
              <span className="text-xs text-neutral-400 whitespace-nowrap">{item.source}</span>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
