/**
 * ProviderFilters Component for US_023 - Provider Browser
 * Filter panel with specialty, availability, gender, and service type filters (FR-006, AC2)
 * Updates complete within 300ms (UXR-004)
 */

interface ProviderFiltersProps {
    /**
     * Current specialty filter value
     */
    specialty: string;
    /**
     * Current availability filter value
     */
    availability: string;
    /**
     * Current gender filter value
     */
    gender: string;
    /**
     * Callback when specialty filter changes
     */
    onSpecialtyChange: (value: string) => void;
    /**
     * Callback when availability filter changes
     */
    onAvailabilityChange: (value: string) => void;
    /**
     * Callback when gender filter changes
     */
    onGenderChange: (value: string) => void;
    /**
     * Callback when "Clear All Filters" button is clicked
     */
    onClearAll: () => void;
}

/**
 * ProviderFilters panel with dropdown filters (FR-006, AC2)
 */
export function ProviderFilters({
    specialty,
    availability,
    gender,
    onSpecialtyChange,
    onAvailabilityChange,
    onGenderChange,
    onClearAll,
}: ProviderFiltersProps) {
    // Check if any filters are active
    const hasActiveFilters =
        specialty !== 'all' ||
        availability !== 'any-time' ||
        gender !== 'any';

    return (
        <div
            className="bg-neutral-0 border border-neutral-200 rounded-lg p-5 shadow-sm h-fit"
            role="search"
            aria-label="Provider filters"
        >
            {/* Filter header */}
            <div className="flex items-center justify-between mb-4">
                <h3 className="text-base font-semibold text-neutral-900">Filters</h3>
                {hasActiveFilters && (
                    <button
                        onClick={onClearAll}
                        className="text-sm text-primary-500 hover:text-primary-700 font-medium 
                       focus:outline-none focus:ring-2 focus:ring-primary-500 rounded px-1
                       transition-colors duration-200"
                        type="button"
                    >
                        Clear All
                    </button>
                )}
            </div>

            {/* Specialty filter */}
            <div className="mb-4">
                <label
                    htmlFor="specialty-filter"
                    className="block text-sm font-medium text-neutral-700 mb-1.5"
                >
                    Specialty
                </label>
                <select
                    id="specialty-filter"
                    value={specialty}
                    onChange={(e) => onSpecialtyChange(e.target.value)}
                    className="w-full h-9 px-3 border border-neutral-300 rounded-lg text-sm 
                     text-neutral-800 bg-neutral-0 focus:outline-none focus:ring-2 
                     focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
                >
                    <option value="all">All Specialties</option>
                    <option value="family-medicine">Family Medicine</option>
                    <option value="internal-medicine">Internal Medicine</option>
                    <option value="cardiology">Cardiology</option>
                    <option value="orthopedics">Orthopedics</option>
                    <option value="pediatrics">Pediatrics</option>
                    <option value="dermatology">Dermatology</option>
                    <option value="psychiatry">Psychiatry</option>
                    <option value="neurology">Neurology</option>
                </select>
            </div>

            {/* Availability filter */}
            <div className="mb-4">
                <label
                    htmlFor="availability-filter"
                    className="block text-sm font-medium text-neutral-700 mb-1.5"
                >
                    Availability
                </label>
                <select
                    id="availability-filter"
                    value={availability}
                    onChange={(e) => onAvailabilityChange(e.target.value)}
                    className="w-full h-9 px-3 border border-neutral-300 rounded-lg text-sm 
                     text-neutral-800 bg-neutral-0 focus:outline-none focus:ring-2 
                     focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
                >
                    <option value="any-time">Any Time</option>
                    <option value="today">Today</option>
                    <option value="this-week">This Week</option>
                    <option value="this-month">This Month</option>
                </select>
            </div>

            {/* Gender filter */}
            <div className="mb-4">
                <label
                    htmlFor="gender-filter"
                    className="block text-sm font-medium text-neutral-700 mb-1.5"
                >
                    Gender
                </label>
                <select
                    id="gender-filter"
                    value={gender}
                    onChange={(e) => onGenderChange(e.target.value)}
                    className="w-full h-9 px-3 border border-neutral-300 rounded-lg text-sm 
                     text-neutral-800 bg-neutral-0 focus:outline-none focus:ring-2 
                     focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
                >
                    <option value="any">Any</option>
                    <option value="male">Male</option>
                    <option value="female">Female</option>
                </select>
            </div>
        </div>
    );
}
