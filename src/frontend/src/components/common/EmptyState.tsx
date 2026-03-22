/**
 * EmptyState Component for US_023 - Provider Browser
 * Displays when no providers match search/filter criteria (AC4)
 * Shows illustration, message, and CTA to clear filters
 */

interface EmptyStateProps {
    /**
     * Title text for empty state
     */
    title?: string;
    /**
     * Description message for empty state
     */
    message?: string;
    /**
     * Show "Clear Filters" button (default: true)
     */
    showClearButton?: boolean;
    /**
     * Callback when "Clear Filters" button is clicked
     */
    onClearFilters?: () => void;
    /**
     * Icon or illustration to display
     */
    icon?: React.ReactNode;
}

/**
 * EmptyState component with illustration and CTA (FR-006, AC4)
 */
export function EmptyState({
    title = 'No providers found',
    message = 'No providers match your current search and filter criteria. Try adjusting your filters or search terms.',
    showClearButton = true,
    onClearFilters,
    icon,
}: EmptyStateProps) {
    return (
        <div
            className="flex flex-col items-center justify-center py-12 px-4 text-center"
            role="status"
            aria-live="polite"
        >
            {/* Illustration */}
            <div className="mb-6 text-neutral-300" aria-hidden="true">
                {icon || (
                    <svg
                        className="w-24 h-24 mx-auto"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                        strokeWidth={1.5}
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                        />
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            d="M9 9l6 6m0-6l-6 6"
                            opacity={0.5}
                        />
                    </svg>
                )}
            </div>

            {/* Title */}
            <h3 className="text-xl font-semibold text-neutral-800 mb-2">
                {title}
            </h3>

            {/* Message */}
            <p className="text-sm text-neutral-500 max-w-md mb-6">
                {message}
            </p>

            {/* Clear Filters CTA */}
            {showClearButton && onClearFilters && (
                <button
                    onClick={onClearFilters}
                    className="px-5 py-2.5 bg-primary-500 text-white text-sm font-medium rounded-lg 
                     hover:bg-primary-600 focus:outline-none focus:ring-2 focus:ring-primary-500 
                     focus:ring-offset-2 transition-colors duration-200"
                    type="button"
                >
                    Clear Filters
                </button>
            )}
        </div>
    );
}
