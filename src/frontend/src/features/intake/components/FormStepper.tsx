/**
 * FormStepper component (US_034)
 * Step indicator with completed/active/pending states
 * Implements UXR-101 (progress indicator)
 */

import { memo } from 'react';

interface Step {
  id: number;
  label: string;
}

interface FormStepperProps {
  steps: Step[];
  currentStep: number;
}

/**
 * FormStepper - Horizontal stepper for multi-step form navigation
 */
function FormStepper({ steps, currentStep }: FormStepperProps) {
  return (
    <nav
      className="mb-8"
      role="navigation"
      aria-label="Form sections"
    >
      <ol className="flex items-center justify-between">
        {steps.map((step, index) => {
          const isCompleted = index < currentStep;
          const isActive = index === currentStep;
          const isPending = index > currentStep;

          return (
            <li key={step.id} className="flex items-center flex-1">
              {/* Step indicator */}
              <div className="flex flex-col items-center flex-1">
                <div
                  className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium border-2 
                    ${isCompleted ? 'bg-primary-600 border-primary-600 text-white' : ''}
                    ${isActive ? 'bg-white border-primary-600 text-primary-600' : ''}
                    ${isPending ? 'bg-white border-neutral-300 text-neutral-400' : ''}`}
                  aria-current={isActive ? 'step' : undefined}
                >
                  {isCompleted ? (
                    <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                      <path
                        fillRule="evenodd"
                        d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                        clipRule="evenodd"
                      />
                    </svg>
                  ) : (
                    step.id
                  )}
                </div>
                <span
                  className={`mt-2 text-xs font-medium text-center
                    ${isCompleted || isActive ? 'text-primary-600' : 'text-neutral-400'}`}
                >
                  {step.label}
                </span>
              </div>

              {/* Connector line (except last step) */}
              {index < steps.length - 1 && (
                <div
                  className={`flex-1 h-0.5 mx-2 ${
                    isCompleted ? 'bg-primary-600' : 'bg-neutral-200'
                  }`}
                  aria-hidden="true"
                />
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}

export default memo(FormStepper);
