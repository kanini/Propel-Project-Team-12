/**
 * IntakeSummary component (US_033, AC-3)
 * Displays extracted intake data for patient review before submission
 */

import { memo, useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch } from '../../../store';
import {
  completeIntake,
  selectExtractedData,
  selectIntakeStatus,
} from '../../../store/slices/intakeSlice';

interface IntakeSummaryProps {
  onEdit?: () => void;
  onConfirm?: () => void;
}

/**
 * IntakeSummary - Review and confirm extracted intake data
 * @param onEdit - Callback to return to editing
 * @param onConfirm - Optional callback after confirmation
 */
function IntakeSummary({ onEdit, onConfirm }: IntakeSummaryProps) {
  const dispatch = useDispatch<AppDispatch>();
  const extractedData = useSelector(selectExtractedData);
  const status = useSelector(selectIntakeStatus);

  const handleConfirm = useCallback(async () => {
    await dispatch(completeIntake());
    onConfirm?.();
  }, [dispatch, onConfirm]);

  const isSubmitting = status === 'loading';

  return (
    <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
      <h2 className="text-xl font-semibold text-neutral-900 mb-4">
        Review Your Information
      </h2>
      <p className="text-neutral-600 mb-6">
        Please review the information below before submitting. Click "Edit" to make
        changes.
      </p>

      <div className="space-y-6">
        {/* Chief Complaint */}
        <SummarySection
          title="Reason for Visit"
          content={extractedData.chiefComplaint}
          emptyText="Not provided"
        />

        {/* Symptoms */}
        <SummarySection
          title="Current Symptoms"
          content={
            extractedData.symptoms?.length
              ? extractedData.symptoms.join(', ')
              : undefined
          }
          emptyText="None reported"
        />

        {/* Medications */}
        <SummarySection title="Current Medications">
          {extractedData.medications?.length ? (
            <ul className="list-disc list-inside space-y-1">
              {extractedData.medications.map((med, index) => (
                <li key={index} className="text-neutral-700">
                  <span className="font-medium">{med.name}</span>
                  {med.dosage && ` - ${med.dosage}`}
                  {med.frequency && ` (${med.frequency})`}
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-neutral-500 italic">None reported</p>
          )}
        </SummarySection>

        {/* Allergies */}
        <SummarySection title="Known Allergies">
          {extractedData.allergies?.length ? (
            <ul className="list-disc list-inside space-y-1">
              {extractedData.allergies.map((allergy, index) => (
                <li key={index} className="text-neutral-700">
                  <span className="font-medium">{allergy.allergen}</span>
                  {allergy.reaction && ` - ${allergy.reaction}`}
                  {allergy.severity && (
                    <span
                      className={`ml-2 text-xs px-2 py-0.5 rounded ${
                        allergy.severity === 'severe'
                          ? 'bg-red-100 text-red-700'
                          : allergy.severity === 'moderate'
                          ? 'bg-yellow-100 text-yellow-700'
                          : 'bg-green-100 text-green-700'
                      }`}
                    >
                      {allergy.severity}
                    </span>
                  )}
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-neutral-500 italic">None reported</p>
          )}
        </SummarySection>

        {/* Medical History */}
        <SummarySection title="Medical History">
          {extractedData.medicalHistory?.length ? (
            <ul className="list-disc list-inside space-y-1">
              {extractedData.medicalHistory.map((item, index) => (
                <li key={index} className="text-neutral-700">
                  <span className="font-medium">{item.condition}</span>
                  {item.diagnosedYear && ` (diagnosed ${item.diagnosedYear})`}
                  <span
                    className={`ml-2 text-xs px-2 py-0.5 rounded ${
                      item.status === 'active'
                        ? 'bg-blue-100 text-blue-700'
                        : item.status === 'managed'
                        ? 'bg-green-100 text-green-700'
                        : 'bg-neutral-100 text-neutral-600'
                    }`}
                  >
                    {item.status}
                  </span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-neutral-500 italic">None reported</p>
          )}
        </SummarySection>

        {/* Family History */}
        <SummarySection
          title="Family History"
          content={
            extractedData.familyHistory?.length
              ? extractedData.familyHistory.join(', ')
              : undefined
          }
          emptyText="None reported"
        />

        {/* Lifestyle */}
        <SummarySection title="Lifestyle">
          {extractedData.lifestyle ? (
            <div className="space-y-1">
              {extractedData.lifestyle.smokingStatus && (
                <p className="text-neutral-700">
                  <span className="font-medium">Smoking:</span>{' '}
                  {formatSmokingStatus(extractedData.lifestyle.smokingStatus)}
                </p>
              )}
              {extractedData.lifestyle.alcoholUse && (
                <p className="text-neutral-700">
                  <span className="font-medium">Alcohol:</span>{' '}
                  {extractedData.lifestyle.alcoholUse}
                </p>
              )}
              {extractedData.lifestyle.exerciseFrequency && (
                <p className="text-neutral-700">
                  <span className="font-medium">Exercise:</span>{' '}
                  {extractedData.lifestyle.exerciseFrequency}
                </p>
              )}
            </div>
          ) : (
            <p className="text-neutral-500 italic">Not provided</p>
          )}
        </SummarySection>

        {/* Additional Concerns */}
        {extractedData.additionalConcerns && (
          <SummarySection
            title="Additional Concerns"
            content={extractedData.additionalConcerns}
          />
        )}
      </div>

      {/* Actions */}
      <div className="mt-8 flex items-center justify-end gap-4 border-t border-neutral-200 pt-6">
        <button
          type="button"
          onClick={onEdit}
          className="px-4 py-2 text-neutral-700 border border-neutral-300 rounded-md font-medium hover:bg-neutral-50 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 transition-colors"
        >
          Edit Information
        </button>
        <button
          type="button"
          onClick={handleConfirm}
          disabled={isSubmitting}
          className="px-6 py-2 bg-primary-600 text-white rounded-md font-medium hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {isSubmitting ? 'Submitting...' : 'Confirm & Submit'}
        </button>
      </div>
    </div>
  );
}

/**
 * SummarySection - Reusable section component for summary display
 */
interface SummarySectionProps {
  title: string;
  content?: string;
  emptyText?: string;
  children?: React.ReactNode;
}

function SummarySection({
  title,
  content,
  emptyText = 'Not provided',
  children,
}: SummarySectionProps) {
  return (
    <div className="border-b border-neutral-100 pb-4">
      <h3 className="font-medium text-neutral-900 mb-2">{title}</h3>
      {children ? (
        children
      ) : content ? (
        <p className="text-neutral-700">{content}</p>
      ) : (
        <p className="text-neutral-500 italic">{emptyText}</p>
      )}
    </div>
  );
}

/**
 * Format smoking status for display
 */
function formatSmokingStatus(status: string): string {
  const statusMap: Record<string, string> = {
    never: 'Never smoked',
    former: 'Former smoker',
    current: 'Current smoker',
  };
  return statusMap[status] || status;
}

export default memo(IntakeSummary);
