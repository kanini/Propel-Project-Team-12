import { type ChangeEvent } from 'react';

/**
 * Custom toggle switch component matching SCR-026 wireframe design.
 * Provides accessible toggle functionality with proper ARIA attributes.
 */
interface ToggleProps {
    id: string;
    checked: boolean;
    onChange: (checked: boolean) => void;
    label: string;
    sublabel?: string;
    disabled?: boolean;
}

export const Toggle = ({ id, checked, onChange, label, sublabel, disabled = false }: ToggleProps) => {
    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        onChange(e.target.checked);
    };

    return (
        <div className="flex items-center justify-between py-3.5 border-b border-gray-100 last:border-b-0">
            <div className="flex-1">
                <label htmlFor={id} className="block text-sm font-medium text-gray-900 cursor-pointer">
                    {label}
                </label>
                {sublabel && (
                    <p className="mt-0.5 text-xs text-gray-500">{sublabel}</p>
                )}
            </div>
            <div className="flex-shrink-0">
                <input
                    type="checkbox"
                    id={id}
                    checked={checked}
                    onChange={handleChange}
                    disabled={disabled}
                    className="sr-only peer"
                    role="switch"
                    aria-checked={checked}
                    aria-label={label}
                />
                <label
                    htmlFor={id}
                    className={`
                        block relative w-11 h-6 rounded-full cursor-pointer transition-colors
                        ${checked ? 'bg-green-600' : 'bg-gray-300'}
                        ${disabled ? 'opacity-50 cursor-not-allowed' : ''}
                        peer-focus-visible:ring-2 peer-focus-visible:ring-blue-500 peer-focus-visible:ring-offset-2
                    `}
                >
                    <span
                        className={`
                            block absolute top-0.5 left-0.5 w-5 h-5 bg-white rounded-full shadow-sm transition-transform
                            ${checked ? 'translate-x-5' : 'translate-x-0'}
                        `}
                        aria-hidden="true"
                    />
                </label>
            </div>
        </div>
    );
};
