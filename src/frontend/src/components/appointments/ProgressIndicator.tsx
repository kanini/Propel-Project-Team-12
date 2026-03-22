/**
 * ProgressIndicator Component for US_024 - Appointment Booking Calendar
 * Displays 4-step appointment booking wizard progress (UXR-101)
 * Shows: Provider → Date/Time → Details → Confirm
 */

import type { BookingStep } from '../../types/appointment';

interface ProgressIndicatorProps {
    currentStep: BookingStep;
}

interface Step {
    number: number;
    label: string;
}

const STEPS: Step[] = [
    { number: 1, label: 'Provider' },
    { number: 2, label: 'Date & Time' },
    { number: 3, label: 'Details' },
    { number: 4, label: 'Confirm' },
];

/**
 * ProgressIndicator displays 4-step booking wizard progress (UXR-101)
 * Responsive: horizontal on desktop, vertical on mobile
 */
export function ProgressIndicator({ currentStep }: ProgressIndicatorProps) {
    /**
     * Determine step state: completed, active, or future
     */
    const getStepState = (stepNumber: number): 'completed' | 'active' | 'future' => {
        if (stepNumber < currentStep) return 'completed';
        if (stepNumber === currentStep) return 'active';
        return 'future';
    };

    /**
     * Get step number styles based on state
     */
    const getStepNumberClasses = (state: 'completed' | 'active' | 'future'): string => {
        const baseClasses =
            'w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold flex-shrink-0 transition-all duration-200';

        if (state === 'completed') {
            return `${baseClasses} bg-success text-neutral-0 border-2 border-success`;
        }
        if (state === 'active') {
            return `${baseClasses} bg-primary-500 text-neutral-0 border-2 border-primary-500`;
        }
        return `${baseClasses} bg-neutral-0 text-neutral-400 border-2 border-neutral-300`;
    };

    /**
     * Get step label styles based on state
     */
    const getStepLabelClasses = (state: 'completed' | 'active' | 'future'): string => {
        const baseClasses = 'text-sm transition-all duration-200';

        if (state === 'completed') {
            return `${baseClasses} text-success font-normal`;
        }
        if (state === 'active') {
            return `${baseClasses} text-primary-500 font-medium`;
        }
        return `${baseClasses} text-neutral-500 font-normal`;
    };

    /**
     * Get connector line styles
     */
    const getLineClasses = (prevStepState: 'completed' | 'active' | 'future'): string => {
        const baseClasses = 'flex-1 h-0.5 mx-2 transition-all duration-200';
        return prevStepState === 'completed'
            ? `${baseClasses} bg-success`
            : `${baseClasses} bg-neutral-200`;
    };

    return (
        <div
            className="mb-8"
            role="navigation"
            aria-label="Booking progress: step {currentStep} of 4"
        >
            {/* Desktop: Horizontal stepper */}
            <div className="hidden md:flex items-center justify-between">
                {STEPS.map((step, index) => {
                    const stepState = getStepState(step.number);
                    const isLastStep = index === STEPS.length - 1;

                    return (
                        <div key={step.number} className="flex items-center flex-1">
                            {/* Step */}
                            <div className="flex items-center gap-2">
                                {/* Step number/checkmark */}
                                <div
                                    className={getStepNumberClasses(stepState)}
                                    aria-current={stepState === 'active' ? 'step' : undefined}
                                >
                                    {stepState === 'completed' ? (
                                        <svg
                                            className="w-5 h-5"
                                            fill="none"
                                            viewBox="0 0 24 24"
                                            stroke="currentColor"
                                            aria-hidden="true"
                                        >
                                            <path
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                                strokeWidth={2}
                                                d="M5 13l4 4L19 7"
                                            />
                                        </svg>
                                    ) : (
                                        <span>{step.number}</span>
                                    )}
                                </div>

                                {/* Step label */}
                                <span className={getStepLabelClasses(stepState)}>
                                    {step.label}
                                </span>
                            </div>

                            {/* Connector line */}
                            {!isLastStep && (
                                <div
                                    className={getLineClasses(stepState)}
                                    role="presentation"
                                    aria-hidden="true"
                                />
                            )}
                        </div>
                    );
                })}
            </div>

            {/* Mobile: Vertical stepper */}
            <div className="flex flex-col gap-4 md:hidden">
                {STEPS.map((step) => {
                    const stepState = getStepState(step.number);

                    return (
                        <div key={step.number} className="flex items-center gap-3">
                            {/* Step number/checkmark */}
                            <div
                                className={getStepNumberClasses(stepState)}
                                aria-current={stepState === 'active' ? 'step' : undefined}
                            >
                                {stepState === 'completed' ? (
                                    <svg
                                        className="w-5 h-5"
                                        fill="none"
                                        viewBox="0 0 24 24"
                                        stroke="currentColor"
                                        aria-hidden="true"
                                    >
                                        <path
                                            strokeLinecap="round"
                                            strokeLinejoin="round"
                                            strokeWidth={2}
                                            d="M5 13l4 4L19 7"
                                        />
                                    </svg>
                                ) : (
                                    <span>{step.number}</span>
                                )}
                            </div>

                            {/* Step label */}
                            <span className={getStepLabelClasses(stepState)}>{step.label}</span>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
