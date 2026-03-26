/**
 * ProviderSearch Component for US_023 - Provider Browser
 * Real-time search input with 300ms debounce (FR-006, AC3, UXR-004)
 * Matches against provider name and specialty fields
 */

import { useState, useEffect } from 'react';
import { useDebounce } from '../../hooks/useDebounce';

interface ProviderSearchProps {
    /**
     * Current search value
     */
    value: string;
    /**
     * Callback when debounced search value changes
     */
    onChange: (value: string) => void;
    /**
     * Placeholder text for search input
     */
    placeholder?: string;
}

/**
 * ProviderSearch with 300ms debounce for real-time filtering (UXR-004)
 */
export function ProviderSearch({
    value,
    onChange,
    placeholder = 'Search by name, specialty, or service...',
}: ProviderSearchProps) {
    const [localValue, setLocalValue] = useState(value);
    const debouncedValue = useDebounce(localValue, 300);

    // Sync local value when prop changes externally (including when filters are cleared)
    useEffect(() => {
        if (value !== localValue) {
            setLocalValue(value);  
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps -- Intentionally omitting localValue to avoid circular updates
    }, [value]);

    // Update parent when debounced value changes
    useEffect(() => {
        if (debouncedValue !== value) {
            onChange(debouncedValue);
        }
    }, [debouncedValue, onChange, value]);

    const handleClear = () => {
        setLocalValue('');
        onChange('');
    };

    return (
        <div className="relative flex-1">
            {/* Search icon */}
            <svg
                className="absolute left-4 top-1/2 transform -translate-y-1/2 h-5 w-5 text-neutral-400 pointer-events-none"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                aria-hidden="true"
            >
                <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                />
            </svg>

            {/* Search input */}
            <input
                type="search"
                value={localValue}
                onChange={(e) => setLocalValue(e.target.value)}
                placeholder={placeholder}
                className="w-full h-11 pl-11 pr-10 border border-neutral-300 rounded-lg text-sm 
                   text-neutral-800 placeholder-neutral-400 bg-neutral-0
                   focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500
                   transition-colors duration-200"
                aria-label="Search providers"
            />

            {/* Clear button */}
            {localValue && (
                <button
                    onClick={handleClear}
                    className="absolute right-3 top-1/2 transform -translate-y-1/2 p-1 
                     text-neutral-400 hover:text-neutral-600 focus:outline-none 
                     focus:ring-2 focus:ring-primary-500 rounded-full transition-colors duration-200"
                    aria-label="Clear search"
                    type="button"
                >
                    <svg
                        className="h-5 w-5"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M6 18L18 6M6 6l12 12"
                        />
                    </svg>
                </button>
            )}
        </div>
    );
}
