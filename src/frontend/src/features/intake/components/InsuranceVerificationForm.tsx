/**
 * InsuranceVerificationForm component (US_036)
 * Form to initiate insurance precheck with card capture
 */

import { useState, useCallback, useRef } from 'react';
import InsurancePrecheckCard, { type PrecheckResult } from './InsurancePrecheckCard';

interface InsuranceVerificationFormProps {
  appointmentId: number;
  onVerificationComplete?: (result: PrecheckResult) => void;
}

interface InsuranceFormData {
  providerId: string;
  memberId: string;
  groupNumber: string;
  dateOfBirth: string;
}

const initialFormData: InsuranceFormData = {
  providerId: '',
  memberId: '',
  groupNumber: '',
  dateOfBirth: '',
};

// Common insurance providers list
const INSURANCE_PROVIDERS = [
  { id: 'bcbs', name: 'Blue Cross Blue Shield' },
  { id: 'aetna', name: 'Aetna' },
  { id: 'cigna', name: 'Cigna' },
  { id: 'unitedhealthcare', name: 'UnitedHealthcare' },
  { id: 'humana', name: 'Humana' },
  { id: 'kaiser', name: 'Kaiser Permanente' },
  { id: 'anthem', name: 'Anthem' },
  { id: 'medicare', name: 'Medicare' },
  { id: 'medicaid', name: 'Medicaid' },
  { id: 'other', name: 'Other' },
];

/**
 * InsuranceVerificationForm - Initiates insurance precheck process
 */
export default function InsuranceVerificationForm({
  appointmentId: _appointmentId, // Reserved for future API integration
  onVerificationComplete,
}: InsuranceVerificationFormProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [formData, setFormData] = useState<InsuranceFormData>(initialFormData);
  const [precheckResult, setPrecheckResult] = useState<PrecheckResult | null>(null);
  const [isVerifying, setIsVerifying] = useState(false);
  const [cardImagePreview, setCardImagePreview] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleInputChange = useCallback(
    (field: keyof InsuranceFormData, value: string) => {
      setFormData((prev) => ({ ...prev, [field]: value }));
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    },
    []
  );

  const handleCardUpload = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>) => {
      const file = event.target.files?.[0];
      if (!file) return;

      // Validate file type and size
      if (!file.type.startsWith('image/')) {
        setErrors((prev) => ({ ...prev, cardImage: 'Please upload an image file' }));
        return;
      }
      if (file.size > 5 * 1024 * 1024) {
        setErrors((prev) => ({ ...prev, cardImage: 'File size must be under 5MB' }));
        return;
      }

      // Create preview
      const reader = new FileReader();
      reader.onload = () => {
        setCardImagePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    },
    []
  );

  const validateForm = useCallback((): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.providerId) {
      newErrors.providerId = 'Please select an insurance provider';
    }
    if (!formData.memberId) {
      newErrors.memberId = 'Member ID is required';
    }
    if (!formData.dateOfBirth) {
      newErrors.dateOfBirth = 'Date of birth is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, [formData]);

  const handleVerify = useCallback(async () => {
    if (!validateForm()) return;

    setIsVerifying(true);
    setPrecheckResult({ status: 'pending' });

    try {
      // Simulate API call - in production this would call the backend
      await new Promise((resolve) => setTimeout(resolve, 2000));

      // Simulated result - replace with actual API call
      const result: PrecheckResult = {
        status: 'verified',
        providerName: INSURANCE_PROVIDERS.find((p) => p.id === formData.providerId)?.name,
        memberId: formData.memberId,
        effectiveDate: '2024-01-01',
        expirationDate: '2024-12-31',
        copayAmount: 25.0,
        deductibleRemaining: 500.0,
      };

      setPrecheckResult(result);
      onVerificationComplete?.(result);
    } catch (error) {
      setPrecheckResult({
        status: 'failed',
        message: 'Unable to verify insurance at this time. Please try again or enter manually.',
      });
    } finally {
      setIsVerifying(false);
    }
  }, [formData, validateForm, onVerificationComplete]);

  const handleRetry = useCallback(() => {
    setPrecheckResult(null);
    setFormData(initialFormData);
    setCardImagePreview(null);
  }, []);

  const handleManualEntry = useCallback(() => {
    // Navigate to manual insurance entry in the form
    // This would typically trigger a mode change
    setPrecheckResult(null);
  }, []);

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-neutral-900">Insurance Verification</h3>
        <p className="text-sm text-neutral-600 mt-1">
          Let us verify your insurance coverage before your appointment.
        </p>
      </div>

      {/* Show result card if available */}
      {precheckResult && (
        <InsurancePrecheckCard
          result={precheckResult}
          onRetry={handleRetry}
          onManualEntry={handleManualEntry}
        />
      )}

      {/* Form - hidden when verified or pending */}
      {(!precheckResult || precheckResult.status === 'failed' || precheckResult.status === 'not_found') && 
       precheckResult?.status !== 'pending' && (
        <div className="space-y-4">
          {/* Insurance card upload */}
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-2">
              Upload Insurance Card (Optional)
            </label>
            <div
              className={`border-2 border-dashed rounded-lg p-4 text-center cursor-pointer hover:border-primary-400 transition-colors ${
                cardImagePreview ? 'border-primary-500' : 'border-neutral-300'
              }`}
              onClick={() => fileInputRef.current?.click()}
            >
              {cardImagePreview ? (
                <div className="space-y-2">
                  <img
                    src={cardImagePreview}
                    alt="Insurance card preview"
                    className="max-h-32 mx-auto rounded"
                  />
                  <p className="text-sm text-primary-600">Click to change</p>
                </div>
              ) : (
                <div className="space-y-1">
                  <svg
                    className="w-8 h-8 mx-auto text-neutral-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                    />
                  </svg>
                  <p className="text-sm text-neutral-500">
                    Take a photo of your insurance card
                  </p>
                  <p className="text-xs text-neutral-400">JPEG, PNG up to 5MB</p>
                </div>
              )}
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                onChange={handleCardUpload}
                className="hidden"
              />
            </div>
            {errors.cardImage && (
              <p className="mt-1 text-sm text-red-500">{errors.cardImage}</p>
            )}
          </div>

          {/* Insurance Provider Select */}
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Insurance Provider <span className="text-red-500">*</span>
            </label>
            <select
              value={formData.providerId}
              onChange={(e) => handleInputChange('providerId', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-primary-500 ${
                errors.providerId ? 'border-red-500' : 'border-neutral-300'
              }`}
            >
              <option value="">Select provider...</option>
              {INSURANCE_PROVIDERS.map((provider) => (
                <option key={provider.id} value={provider.id}>
                  {provider.name}
                </option>
              ))}
            </select>
            {errors.providerId && (
              <p className="mt-1 text-sm text-red-500">{errors.providerId}</p>
            )}
          </div>

          {/* Member ID */}
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Member ID <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={formData.memberId}
              onChange={(e) => handleInputChange('memberId', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-primary-500 ${
                errors.memberId ? 'border-red-500' : 'border-neutral-300'
              }`}
              placeholder="Enter your member ID"
            />
            {errors.memberId && (
              <p className="mt-1 text-sm text-red-500">{errors.memberId}</p>
            )}
          </div>

          {/* Group Number */}
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Group Number
            </label>
            <input
              type="text"
              value={formData.groupNumber}
              onChange={(e) => handleInputChange('groupNumber', e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:ring-2 focus:ring-primary-500"
              placeholder="Enter group number (if applicable)"
            />
          </div>

          {/* Date of Birth for verification */}
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Date of Birth <span className="text-red-500">*</span>
            </label>
            <input
              type="date"
              value={formData.dateOfBirth}
              onChange={(e) => handleInputChange('dateOfBirth', e.target.value)}
              className={`w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-primary-500 ${
                errors.dateOfBirth ? 'border-red-500' : 'border-neutral-300'
              }`}
            />
            {errors.dateOfBirth && (
              <p className="mt-1 text-sm text-red-500">{errors.dateOfBirth}</p>
            )}
          </div>

          {/* Verify Button */}
          <button
            type="button"
            onClick={handleVerify}
            disabled={isVerifying}
            className="w-full py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed font-medium"
          >
            {isVerifying ? (
              <span className="flex items-center justify-center gap-2">
                <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle
                    className="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    strokeWidth="4"
                  />
                  <path
                    className="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                  />
                </svg>
                Verifying...
              </span>
            ) : (
              'Verify Insurance'
            )}
          </button>

          <p className="text-xs text-neutral-500 text-center">
            Your information is encrypted and securely transmitted.
          </p>
        </div>
      )}
    </div>
  );
}
