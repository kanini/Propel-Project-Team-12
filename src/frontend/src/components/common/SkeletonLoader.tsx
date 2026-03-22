/**
 * Skeleton Loader Component for US_023 - Provider Browser
 * Displays skeleton placeholder while data is loading (UXR-502)
 * Shows after 300ms delay to prevent flash for fast-loading content
 */

import { useEffect, useState } from 'react';

interface SkeletonLoaderProps {
    /**
     * Number of skeleton cards to display (default: 6)
     */
    count?: number;
    /**
     * Delay before showing skeleton in ms (default: 300ms per UXR-502)
     */
    delay?: number;
}

/**
 * SkeletonLoader component with 300ms delay (UXR-502)
 */
export function SkeletonLoader({ count = 6, delay = 300 }: SkeletonLoaderProps) {
    const [shouldShow, setShouldShow] = useState(false);

    useEffect(() => {
        // Show skeleton after 300ms delay (UXR-502)
        const timer = setTimeout(() => {
            setShouldShow(true);
        }, delay);

        return () => clearTimeout(timer);
    }, [delay]);

    // Don't render anything during delay period
    if (!shouldShow) {
        return null;
    }

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {Array.from({ length: count }).map((_, index) => (
                <div
                    key={index}
                    className="bg-neutral-0 border border-neutral-200 rounded-lg p-5 animate-pulse"
                    role="status"
                    aria-label="Loading provider information"
                >
                    {/* Provider avatar and name skeleton */}
                    <div className="flex gap-4 mb-3">
                        <div className="w-12 h-12 bg-neutral-200 rounded-full flex-shrink-0" />
                        <div className="flex-1">
                            <div className="h-5 bg-neutral-200 rounded w-3/4 mb-2" />
                            <div className="h-4 bg-neutral-200 rounded w-1/2" />
                        </div>
                    </div>

                    {/* Rating skeleton */}
                    <div className="h-4 bg-neutral-200 rounded w-2/3 mb-3" />

                    {/* Availability skeleton */}
                    <div className="border-t border-neutral-100 pt-3 mt-3">
                        <div className="flex justify-between items-center">
                            <div className="h-3 bg-neutral-200 rounded w-1/2" />
                            <div className="h-6 bg-neutral-200 rounded w-20" />
                        </div>
                    </div>
                </div>
            ))}
        </div>
    );
}
