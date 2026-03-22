/**
 * ProviderCard Component for US_023 - Provider Browser
 * Displays provider information with name, specialty, ratings, and availability (FR-006, AC1)
 * Handles "No availability" state with "Join Waitlist" option (Edge Case)
 * Updated for US_025 - Join Waitlist integration
 */

import { Link } from 'react-router-dom';
import { useAppDispatch } from '../../store/hooks';
import { openEnrollmentModal } from '../../store/slices/waitlistSlice';
import type { Provider } from '../../types/provider';

interface ProviderCardProps {
    /**
     * Provider data to display
     */
    provider: Provider;
}

/**
 * Format next available slot date/time for display
 */
function formatAvailabilitySlot(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const tomorrow = new Date(now);
    tomorrow.setDate(tomorrow.getDate() + 1);

    // Format time
    const timeString = date.toLocaleTimeString('en-US', {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true,
    });

    // Check if today or tomorrow
    if (date.toDateString() === now.toDateString()) {
        return `Today, ${timeString}`;
    } else if (date.toDateString() === tomorrow.toDateString()) {
        return `Tomorrow, ${timeString}`;
    } else {
        const dateString = date.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
        });
        return `${dateString}, ${timeString}`;
    }
}

/**
 * Get provider initials for avatar
 */
function getInitials(name: string): string {
    return name
        .split(' ')
        .map((part) => part[0])
        .join('')
        .toUpperCase()
        .slice(0, 2);
}

/**
 * Render star rating
 */
function StarRating({ rating }: { rating: number }) {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return (
        <div className="flex items-center gap-1" aria-label={`Rating: ${rating} out of 5 stars`}>
            {/* Full stars */}
            {Array.from({ length: fullStars }).map((_, i) => (
                <svg
                    key={`full-${i}`}
                    className="w-4 h-4 text-warning-default"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                    aria-hidden="true"
                >
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                </svg>
            ))}
            {/* Half star */}
            {hasHalfStar && (
                <svg
                    className="w-4 h-4 text-warning-default"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                    aria-hidden="true"
                >
                    <defs>
                        <linearGradient id="half-star">
                            <stop offset="50%" stopColor="currentColor" />
                            <stop offset="50%" stopColor="transparent" />
                        </linearGradient>
                    </defs>
                    <path
                        fill="url(#half-star)"
                        d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
                    />
                </svg>
            )}
            {/* Empty stars */}
            {Array.from({ length: emptyStars }).map((_, i) => (
                <svg
                    key={`empty-${i}`}
                    className="w-4 h-4 text-neutral-300"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                    aria-hidden="true"
                >
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                </svg>
            ))}
        </div>
    );
}

/**
 * ProviderCard displays provider details with availability and booking CTA (FR-006, AC1)
 */
export function ProviderCard({ provider }: ProviderCardProps) {
    const dispatch = useAppDispatch();
    const hasAvailability = provider.nextAvailableSlot !== null;

    /**
     * Handle join waitlist button click
     */
    const handleJoinWaitlist = (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        dispatch(
            openEnrollmentModal({
                providerId: provider.id,
                providerName: provider.name,
                providerSpecialty: provider.specialty,
            })
        );
    };

    return (
        <Link
            to={`/providers/${provider.id}/book`}
            className="block bg-neutral-0 border border-neutral-200 rounded-lg p-5 shadow-sm 
                 hover:border-primary-300 hover:shadow-md focus:outline-none focus:ring-2 
                 focus:ring-primary-500 focus:ring-offset-2 transition-all duration-200 
                 cursor-pointer group"
            aria-label={`View ${provider.name}, ${provider.specialty}`}
        >
            {/* Provider info */}
            <div className="flex gap-4 mb-3">
                {/* Avatar */}
                <div
                    className="w-12 h-12 rounded-full bg-primary-50 text-primary-500 flex items-center 
                     justify-center font-semibold text-base flex-shrink-0"
                    aria-hidden="true"
                >
                    {provider.avatarUrl ? (
                        <img
                            src={provider.avatarUrl}
                            alt=""
                            className="w-full h-full rounded-full object-cover"
                        />
                    ) : (
                        getInitials(provider.name)
                    )}
                </div>

                {/* Name and specialty */}
                <div className="flex-1 min-w-0">
                    <h4 className="text-base font-semibold text-neutral-900 truncate group-hover:text-primary-600 transition-colors">
                        {provider.name}
                    </h4>
                    <p className="text-sm text-neutral-500 truncate">{provider.specialty}</p>
                </div>
            </div>

            {/* Rating */}
            <div className="flex items-center gap-2 mb-3">
                <StarRating rating={provider.rating} />
                <span className="text-sm text-neutral-600">
                    {provider.rating.toFixed(1)} ({provider.reviewCount} reviews)
                </span>
            </div>

            {/* Availability */}
            <div className="border-t border-neutral-100 pt-3 mt-3 flex justify-between items-center">
                <div>
                    {hasAvailability ? (
                        <p className="text-xs text-neutral-500">
                            Next available:{' '}
                            <span className="font-medium text-neutral-700">
                                {formatAvailabilitySlot(provider.nextAvailableSlot!)}
                            </span>
                        </p>
                    ) : (
                        <p className="text-xs text-neutral-500 font-medium">No availability</p>
                    )}
                </div>

                {/* Badge or Join Waitlist Button */}
                {hasAvailability ? (
                    <span
                        className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-success-light text-success-dark"
                    >
                        Available
                    </span>
                ) : (
                    <button
                        onClick={handleJoinWaitlist}
                        className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-warning-light text-warning-dark hover:bg-warning-default hover:text-neutral-0 transition-colors focus:outline-none focus:ring-2 focus:ring-warning-default focus:ring-offset-2"
                        aria-label={`Join waitlist for ${provider.name}`}
                    >
                        Join Waitlist
                    </button>
                )}
            </div>
        </Link>
    );
}
