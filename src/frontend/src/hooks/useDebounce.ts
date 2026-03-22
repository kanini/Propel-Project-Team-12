/**
 * Custom hook for debouncing values (US_023, AC3)
 * Implements 300ms debounce for real-time search filtering
 */

import { useEffect, useState } from 'react';

/**
 * Debounce hook that delays updating a value until after specified delay
 * @param value - The value to debounce
 * @param delay - Delay in milliseconds (default: 300ms per UXR-004)
 * @returns Debounced value
 */
export function useDebounce<T>(value: T, delay: number = 300): T {
    const [debouncedValue, setDebouncedValue] = useState<T>(value);

    useEffect(() => {
        // Set up timeout to update debounced value after delay
        const handler = setTimeout(() => {
            setDebouncedValue(value);
        }, delay);

        // Clean up timeout if value changes before delay completes
        return () => {
            clearTimeout(handler);
        };
    }, [value, delay]);

    return debouncedValue;
}
