/**
 * ManualIntakeForm component (US_034)
 * Multi-step structured intake form with validation
 */

import { useState, useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch } from '../../../store';
import {
  submitManualIntakeForm,
  setManualFormStep,
  selectIntakeStatus,
} from '../../../store/slices/intakeSlice';
import type { ManualIntakeFormData } from '../../../types/intake';
import FormStepper from './FormStepper';
import FormTooltip from './FormTooltip';

const FORM_STEPS = [
  { id: 1, label: 'Demographics' },
  { id: 2, label: 'Medical History' },
  { id: 3, label: 'Insurance' },
  { id: 4, label: 'Visit Concerns' },
  { id: 5, label: 'Review' },
];

const initialFormData: ManualIntakeFormData = {
  demographics: {
    preferredName: '',
    dateOfBirth: '',
    phone: '',
    emergencyContact: '',
    emergencyPhone: '',
  },
  medicalHistory: {
    currentConditions: '',
    medications: '',
    allergies: '',
    surgicalHistory: '',
    familyHistory: {
      heartDisease: false,
      diabetes: false,
      cancer: false,
      hypertension: false,
      stroke: false,
      mentalHealth: false,
      other: '',
    },
    lifestyle: {
      smoking: 'never',
      alcohol: 'none',
    },
  },
  insurance: {
    hasInsurance: true,
    providerName: '',
    memberId: '',
    groupNumber: '',
  },
  visitConcerns: {
    chiefComplaint: '',
    symptomDuration: '',
    symptomSeverity: 'moderate',
    additionalConcerns: '',
  },
};

interface ManualIntakeFormProps {
  onComplete?: () => void;
}

/**
 * ManualIntakeForm - Multi-step form for manual intake data entry
 */
export default function ManualIntakeForm({ onComplete }: ManualIntakeFormProps) {
  const dispatch = useDispatch<AppDispatch>();
  const status = useSelector(selectIntakeStatus);

  const [currentStep, setCurrentStep] = useState(0);
  const [formData, setFormData] = useState<ManualIntakeFormData>(initialFormData);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Update Redux progress when step changes
  useEffect(() => {
    dispatch(setManualFormStep(currentStep + 1));
  }, [currentStep, dispatch]);

  // Load from localStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem('manual-intake-draft');
    if (saved) {
      try {
        setFormData(JSON.parse(saved));
      } catch {
        // Ignore invalid data
      }
    }
  }, []);

  // Save to localStorage on data change
  useEffect(() => {
    localStorage.setItem('manual-intake-draft', JSON.stringify(formData));
  }, [formData]);

  const handleInputChange = useCallback(
    (
      section: keyof ManualIntakeFormData,
      field: string,
      value: string | boolean
    ) => {
      setFormData((prev) => ({
        ...prev,
        [section]: {
          ...prev[section],
          [field]: value,
        },
      }));
      // Clear error on change
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[`${section}.${field}`];
        return newErrors;
      });
    },
    []
  );

  const handleNestedChange = useCallback(
    (
      section: keyof ManualIntakeFormData,
      subsection: string,
      field: string,
      value: string | boolean
    ) => {
      setFormData((prev) => {
        const currentSection = prev[section] as unknown as Record<string, Record<string, unknown>>;
        const currentSubsection = currentSection[subsection] || {};
        return {
          ...prev,
          [section]: {
            ...currentSection,
            [subsection]: {
              ...currentSubsection,
              [field]: value,
            },
          },
        };
      });
    },
    []
  );

  const validateStep = useCallback((step: number): boolean => {
    const newErrors: Record<string, string> = {};

    if (step === 0) {
      // Demographics validation
      if (!formData.demographics.dateOfBirth) {
        newErrors['demographics.dateOfBirth'] = 'Date of birth is required';
      }
      if (!formData.demographics.phone) {
        newErrors['demographics.phone'] = 'Phone number is required';
      }
    } else if (step === 1) {
      // Medical history - allergies required
      // (optional fields, no strict validation)
    } else if (step === 2) {
      // Insurance validation if hasInsurance is true
      if (formData.insurance.hasInsurance) {
        if (!formData.insurance.providerName) {
          newErrors['insurance.providerName'] = 'Insurance provider name is required';
        }
        if (!formData.insurance.memberId) {
          newErrors['insurance.memberId'] = 'Member ID is required';
        }
      }
    } else if (step === 3) {
      // Visit concerns validation
      if (!formData.visitConcerns.chiefComplaint) {
        newErrors['visitConcerns.chiefComplaint'] = 'Chief complaint is required';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, [formData]);

  const handleNext = useCallback(() => {
    if (validateStep(currentStep)) {
      setCurrentStep((prev) => Math.min(prev + 1, FORM_STEPS.length - 1));
    }
  }, [currentStep, validateStep]);

  const handleBack = useCallback(() => {
    setCurrentStep((prev) => Math.max(prev - 1, 0));
  }, []);

  const handleSubmit = useCallback(async () => {
    if (!validateStep(currentStep)) return;

    await dispatch(submitManualIntakeForm(formData));
    localStorage.removeItem('manual-intake-draft');
    onComplete?.();
  }, [currentStep, formData, dispatch, validateStep, onComplete]);

  const isSubmitting = status === 'loading';

  return (
    <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
      <FormStepper steps={FORM_STEPS} currentStep={currentStep} />

      {/* Step content */}
      <div className="min-h-[400px]">
        {currentStep === 0 && (
          <DemographicsSection
            data={formData.demographics}
            errors={errors}
            onChange={(field, value) =>
              handleInputChange('demographics', field, value)
            }
          />
        )}

        {currentStep === 1 && (
          <MedicalHistorySection
            data={formData.medicalHistory}
            errors={errors}
            onChange={(field, value) =>
              handleInputChange('medicalHistory', field, value)
            }
            onNestedChange={(subsection, field, value) =>
              handleNestedChange('medicalHistory', subsection, field, value)
            }
          />
        )}

        {currentStep === 2 && (
          <InsuranceSection
            data={formData.insurance}
            errors={errors}
            onChange={(field, value) =>
              handleInputChange('insurance', field, value)
            }
          />
        )}

        {currentStep === 3 && (
          <VisitConcernsSection
            data={formData.visitConcerns}
            errors={errors}
            onChange={(field, value) =>
              handleInputChange('visitConcerns', field, value)
            }
          />
        )}

        {currentStep === 4 && (
          <ReviewSection
            data={formData}
            onEdit={(step) => setCurrentStep(step)}
          />
        )}
      </div>

      {/* Navigation buttons */}
      <div className="flex justify-between mt-6 pt-6 border-t border-neutral-200">
        <button
          type="button"
          onClick={handleBack}
          disabled={currentStep === 0}
          className="px-4 py-2 text-neutral-600 border border-neutral-300 rounded-md hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Back
        </button>

        {currentStep < FORM_STEPS.length - 1 ? (
          <button
            type="button"
            onClick={handleNext}
            className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
          >
            Next
          </button>
        ) : (
          <button
            type="button"
            onClick={handleSubmit}
            disabled={isSubmitting}
            className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50"
          >
            {isSubmitting ? 'Submitting...' : 'Submit Intake'}
          </button>
        )}
      </div>
    </div>
  );
}

// Demographics Section Component
function DemographicsSection({
  data,
  errors,
  onChange,
}: {
  data: ManualIntakeFormData['demographics'];
  errors: Record<string, string>;
  onChange: (field: string, value: string) => void;
}) {
  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-neutral-900">Demographics</h2>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Preferred Name
          </label>
          <input
            type="text"
            value={data.preferredName || ''}
            onChange={(e) => onChange('preferredName', e.target.value)}
            className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            placeholder="How you prefer to be called"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Date of Birth <span className="text-red-500">*</span>
          </label>
          <input
            type="date"
            value={data.dateOfBirth}
            onChange={(e) => onChange('dateOfBirth', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-primary-500 ${
              errors['demographics.dateOfBirth']
                ? 'border-red-500'
                : 'border-neutral-300'
            }`}
            required
          />
          {errors['demographics.dateOfBirth'] && (
            <p className="mt-1 text-sm text-red-500">
              {errors['demographics.dateOfBirth']}
            </p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Phone Number <span className="text-red-500">*</span>
          </label>
          <input
            type="tel"
            value={data.phone}
            onChange={(e) => onChange('phone', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-primary-500 ${
              errors['demographics.phone'] ? 'border-red-500' : 'border-neutral-300'
            }`}
            placeholder="(555) 555-5555"
            required
          />
          {errors['demographics.phone'] && (
            <p className="mt-1 text-sm text-red-500">
              {errors['demographics.phone']}
            </p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Emergency Contact Name
          </label>
          <input
            type="text"
            value={data.emergencyContact || ''}
            onChange={(e) => onChange('emergencyContact', e.target.value)}
            className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Emergency Contact Phone
          </label>
          <input
            type="tel"
            value={data.emergencyPhone || ''}
            onChange={(e) => onChange('emergencyPhone', e.target.value)}
            className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
            placeholder="(555) 555-5555"
          />
        </div>
      </div>
    </div>
  );
}

// Medical History Section Component
function MedicalHistorySection({
  data,
  errors: _errors, // Reserved for future validation
  onChange,
  onNestedChange,
}: {
  data: ManualIntakeFormData['medicalHistory'];
  errors: Record<string, string>;
  onChange: (field: string, value: string) => void;
  onNestedChange: (subsection: string, field: string, value: string | boolean) => void;
}) {
  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-neutral-900">Medical History</h2>

      <div className="space-y-4">
        <div>
          <FormTooltip content="List any ongoing health conditions such as diabetes, high blood pressure, asthma, etc.">
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Current Medical Conditions
            </label>
          </FormTooltip>
          <textarea
            value={data.currentConditions}
            onChange={(e) => onChange('currentConditions', e.target.value)}
            rows={3}
            className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
            placeholder="e.g., Type 2 Diabetes, Hypertension"
          />
        </div>

        <div>
          <FormTooltip content="Include all prescription and over-the-counter medications with dosage if known">
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Current Medications
            </label>
          </FormTooltip>
          <textarea
            value={data.medications}
            onChange={(e) => onChange('medications', e.target.value)}
            rows={3}
            className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
            placeholder="e.g., Metformin 500mg twice daily"
          />
        </div>

        <div>
          <FormTooltip content="Include medication allergies, food allergies, and environmental allergies">
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Known Allergies
            </label>
          </FormTooltip>
          <textarea
            value={data.allergies}
            onChange={(e) => onChange('allergies', e.target.value)}
            rows={2}
            className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
            placeholder="e.g., Penicillin (rash), Shellfish"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Surgical History
          </label>
          <textarea
            value={data.surgicalHistory}
            onChange={(e) => onChange('surgicalHistory', e.target.value)}
            rows={2}
            className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
            placeholder="e.g., Appendectomy (2015)"
          />
        </div>

        {/* Lifestyle */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-2">
              Smoking Status
            </label>
            <div className="space-y-2">
              {['never', 'former', 'current'].map((option) => (
                <label key={option} className="flex items-center">
                  <input
                    type="radio"
                    name="smoking"
                    value={option}
                    checked={data.lifestyle.smoking === option}
                    onChange={(e) =>
                      onNestedChange('lifestyle', 'smoking', e.target.value)
                    }
                    className="mr-2 text-primary-600 focus:ring-primary-500"
                  />
                  <span className="text-sm text-neutral-700 capitalize">
                    {option === 'never' ? 'Never smoked' : option === 'former' ? 'Former smoker' : 'Current smoker'}
                  </span>
                </label>
              ))}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-2">
              Alcohol Use
            </label>
            <div className="space-y-2">
              {['none', 'occasional', 'moderate', 'heavy'].map((option) => (
                <label key={option} className="flex items-center">
                  <input
                    type="radio"
                    name="alcohol"
                    value={option}
                    checked={data.lifestyle.alcohol === option}
                    onChange={(e) =>
                      onNestedChange('lifestyle', 'alcohol', e.target.value)
                    }
                    className="mr-2 text-primary-600 focus:ring-primary-500"
                  />
                  <span className="text-sm text-neutral-700 capitalize">{option}</span>
                </label>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

// Insurance Section Component
function InsuranceSection({
  data,
  errors,
  onChange,
}: {
  data: ManualIntakeFormData['insurance'];
  errors: Record<string, string>;
  onChange: (field: string, value: string | boolean) => void;
}) {
  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-neutral-900">Insurance Information</h2>

      <div className="space-y-4">
        <label className="flex items-center">
          <input
            type="checkbox"
            checked={data.hasInsurance}
            onChange={(e) => onChange('hasInsurance', e.target.checked)}
            className="mr-2 text-primary-600 focus:ring-primary-500 rounded"
          />
          <span className="text-sm text-neutral-700">I have health insurance</span>
        </label>

        {data.hasInsurance && (
          <div className="space-y-4 pl-6">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Insurance Provider <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={data.providerName || ''}
                onChange={(e) => onChange('providerName', e.target.value)}
                className={`w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-primary-500 ${
                  errors['insurance.providerName']
                    ? 'border-red-500'
                    : 'border-neutral-300'
                }`}
                placeholder="e.g., Blue Cross Blue Shield"
              />
              {errors['insurance.providerName'] && (
                <p className="mt-1 text-sm text-red-500">
                  {errors['insurance.providerName']}
                </p>
              )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  Member ID <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={data.memberId || ''}
                  onChange={(e) => onChange('memberId', e.target.value)}
                  className={`w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-primary-500 ${
                    errors['insurance.memberId']
                      ? 'border-red-500'
                      : 'border-neutral-300'
                  }`}
                />
                {errors['insurance.memberId'] && (
                  <p className="mt-1 text-sm text-red-500">
                    {errors['insurance.memberId']}
                  </p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  Group Number
                </label>
                <input
                  type="text"
                  value={data.groupNumber || ''}
                  onChange={(e) => onChange('groupNumber', e.target.value)}
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
                />
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

// Visit Concerns Section Component
function VisitConcernsSection({
  data,
  errors,
  onChange,
}: {
  data: ManualIntakeFormData['visitConcerns'];
  errors: Record<string, string>;
  onChange: (field: string, value: string) => void;
}) {
  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-neutral-900">Visit Concerns</h2>
      <p className="text-sm text-neutral-600">
        Tell us about your reason for this visit and any concerns you have.
      </p>

      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Chief Complaint <span className="text-red-500">*</span>
          </label>
          <p className="text-xs text-neutral-500 mb-2">
            What is the main reason for your visit today?
          </p>
          <textarea
            value={data.chiefComplaint}
            onChange={(e) => onChange('chiefComplaint', e.target.value)}
            rows={3}
            className={`w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-primary-500 ${
              errors['visitConcerns.chiefComplaint']
                ? 'border-red-500'
                : 'border-neutral-300'
            }`}
            placeholder="e.g., Persistent headaches, Follow-up on blood pressure"
            required
          />
          {errors['visitConcerns.chiefComplaint'] && (
            <p className="mt-1 text-sm text-red-500">
              {errors['visitConcerns.chiefComplaint']}
            </p>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Symptom Duration
            </label>
            <input
              type="text"
              value={data.symptomDuration || ''}
              onChange={(e) => onChange('symptomDuration', e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
              placeholder="e.g., 2 weeks, 3 months"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-2">
              Symptom Severity
            </label>
            <div className="flex gap-4">
              {['mild', 'moderate', 'severe'].map((severity) => (
                <label key={severity} className="flex items-center">
                  <input
                    type="radio"
                    name="symptomSeverity"
                    value={severity}
                    checked={data.symptomSeverity === severity}
                    onChange={(e) => onChange('symptomSeverity', e.target.value)}
                    className="mr-2 text-primary-600 focus:ring-primary-500"
                  />
                  <span className="text-sm text-neutral-700 capitalize">
                    {severity}
                  </span>
                </label>
              ))}
            </div>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Additional Concerns
          </label>
          <textarea
            value={data.additionalConcerns || ''}
            onChange={(e) => onChange('additionalConcerns', e.target.value)}
            rows={3}
            className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
            placeholder="Any other symptoms, questions, or concerns you want to discuss"
          />
        </div>
      </div>
    </div>
  );
}

// Review Section Component
function ReviewSection({
  data,
  onEdit,
}: {
  data: ManualIntakeFormData;
  onEdit: (step: number) => void;
}) {
  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-neutral-900">Review Your Information</h2>
      <p className="text-sm text-neutral-600">
        Please review all information before submitting. Click "Edit" to make changes.
      </p>

      {/* Demographics summary */}
      <SummaryCard
        title="Demographics"
        onEdit={() => onEdit(0)}
        items={[
          { label: 'Preferred Name', value: data.demographics.preferredName || 'Not provided' },
          { label: 'Date of Birth', value: data.demographics.dateOfBirth },
          { label: 'Phone', value: data.demographics.phone },
          { label: 'Emergency Contact', value: data.demographics.emergencyContact || 'Not provided' },
          { label: 'Emergency Phone', value: data.demographics.emergencyPhone || 'Not provided' },
        ]}
      />

      {/* Medical History summary */}
      <SummaryCard
        title="Medical History"
        onEdit={() => onEdit(1)}
        items={[
          { label: 'Conditions', value: data.medicalHistory.currentConditions || 'None reported' },
          { label: 'Medications', value: data.medicalHistory.medications || 'None reported' },
          { label: 'Allergies', value: data.medicalHistory.allergies || 'None reported' },
          { label: 'Surgeries', value: data.medicalHistory.surgicalHistory || 'None reported' },
          { label: 'Smoking', value: data.medicalHistory.lifestyle.smoking },
          { label: 'Alcohol', value: data.medicalHistory.lifestyle.alcohol },
        ]}
      />

      {/* Insurance summary */}
      <SummaryCard
        title="Insurance"
        onEdit={() => onEdit(2)}
        items={[
          { label: 'Has Insurance', value: data.insurance.hasInsurance ? 'Yes' : 'No' },
          ...(data.insurance.hasInsurance
            ? [
                { label: 'Provider', value: data.insurance.providerName || '' },
                { label: 'Member ID', value: data.insurance.memberId || '' },
                { label: 'Group Number', value: data.insurance.groupNumber || 'Not provided' },
              ]
            : []),
        ]}
      />

      {/* Visit Concerns summary */}
      <SummaryCard
        title="Visit Concerns"
        onEdit={() => onEdit(3)}
        items={[
          { label: 'Chief Complaint', value: data.visitConcerns.chiefComplaint },
          { label: 'Duration', value: data.visitConcerns.symptomDuration || 'Not specified' },
          { label: 'Severity', value: data.visitConcerns.symptomSeverity || 'Not specified' },
          { label: 'Additional Concerns', value: data.visitConcerns.additionalConcerns || 'None' },
        ]}
      />
    </div>
  );
}

// Summary Card Component
function SummaryCard({
  title,
  items,
  onEdit,
}: {
  title: string;
  items: { label: string; value: string }[];
  onEdit: () => void;
}) {
  return (
    <div className="border border-neutral-200 rounded-lg p-4">
      <div className="flex justify-between items-center mb-3">
        <h3 className="font-medium text-neutral-900">{title}</h3>
        <button
          type="button"
          onClick={onEdit}
          className="text-sm text-primary-600 hover:text-primary-700 font-medium"
        >
          Edit
        </button>
      </div>
      <dl className="space-y-1">
        {items.map((item, index) => (
          <div key={index} className="flex text-sm">
            <dt className="w-1/3 text-neutral-500">{item.label}:</dt>
            <dd className="w-2/3 text-neutral-900">{item.value}</dd>
          </div>
        ))}
      </dl>
    </div>
  );
}
