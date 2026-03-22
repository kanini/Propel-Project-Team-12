/**
 * Pagination Component for US_023 - Provider Browser
 * Handles navigation through paginated provider lists (Edge Case: 100+ providers)
 * Displays 20 providers per page with page controls
 */

interface PaginationProps {
    /**
     * Current page number (1-indexed)
     */
    currentPage: number;
    /**
     * Total number of pages
     */
    totalPages: number;
    /**
     * Total number of items
     */
    totalItems: number;
    /**
     * Items per page
     */
    pageSize: number;
    /**
     * Callback when page changes
     */
    onPageChange: (page: number) => void;
}

/**
 * Pagination component with Previous/Next navigation (Edge Case requirement)
 */
export function Pagination({
    currentPage,
    totalPages,
    totalItems,
    pageSize,
    onPageChange,
}: PaginationProps) {
    // Calculate display range
    const startItem = (currentPage - 1) * pageSize + 1;
    const endItem = Math.min(currentPage * pageSize, totalItems);

    // Generate page numbers to display (max 5 pages shown)
    const getPageNumbers = (): number[] => {
        const pages: number[] = [];
        const maxPagesToShow = 5;

        if (totalPages <= maxPagesToShow) {
            // Show all pages if total is less than max
            for (let i = 1; i <= totalPages; i++) {
                pages.push(i);
            }
        } else {
            // Show current page with 2 pages on each side
            const startPage = Math.max(1, currentPage - 2);
            const endPage = Math.min(totalPages, currentPage + 2);

            for (let i = startPage; i <= endPage; i++) {
                pages.push(i);
            }
        }

        return pages;
    };

    const pageNumbers = getPageNumbers();

    // Hide pagination if only one page
    if (totalPages <= 1) {
        return null;
    }

    return (
        <nav
            className="flex items-center justify-between border-t border-neutral-200 px-4 py-3 sm:px-6 mt-6"
            aria-label="Pagination"
        >
            {/* Results info */}
            <div className="hidden sm:block">
                <p className="text-sm text-neutral-700">
                    Showing <span className="font-medium">{startItem}</span> to{' '}
                    <span className="font-medium">{endItem}</span> of{' '}
                    <span className="font-medium">{totalItems}</span> providers
                </p>
            </div>

            {/* Page controls */}
            <div className="flex flex-1 justify-between sm:justify-end gap-2">
                {/* Previous button */}
                <button
                    onClick={() => onPageChange(currentPage - 1)}
                    disabled={currentPage === 1}
                    className="relative inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg 
                     border border-neutral-300 bg-neutral-0 text-neutral-700 
                     hover:bg-neutral-50 focus:z-10 focus:outline-none focus:ring-2 
                     focus:ring-primary-500 focus:ring-offset-2 
                     disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-neutral-0
                     transition-colors duration-200"
                    aria-label="Previous page"
                    type="button"
                >
                    <svg
                        className="h-5 w-5 mr-1"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                        aria-hidden="true"
                    >
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                    </svg>
                    Previous
                </button>

                {/* Page numbers */}
                <div className="hidden sm:flex gap-1">
                    {pageNumbers.map((page) => (
                        <button
                            key={page}
                            onClick={() => onPageChange(page)}
                            className={`relative inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg 
                         border transition-colors duration-200 focus:z-10 focus:outline-none 
                         focus:ring-2 focus:ring-primary-500 focus:ring-offset-2
                         ${page === currentPage
                                    ? 'bg-primary-500 border-primary-500 text-white'
                                    : 'border-neutral-300 bg-neutral-0 text-neutral-700 hover:bg-neutral-50'
                                }`}
                            aria-label={`Page ${page}`}
                            aria-current={page === currentPage ? 'page' : undefined}
                            type="button"
                        >
                            {page}
                        </button>
                    ))}
                </div>

                {/* Mobile page indicator */}
                <div className="sm:hidden flex items-center px-3 text-sm text-neutral-700">
                    <span className="font-medium">{currentPage}</span>
                    <span className="mx-1">/</span>
                    <span className="font-medium">{totalPages}</span>
                </div>

                {/* Next button */}
                <button
                    onClick={() => onPageChange(currentPage + 1)}
                    disabled={currentPage === totalPages}
                    className="relative inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg 
                     border border-neutral-300 bg-neutral-0 text-neutral-700 
                     hover:bg-neutral-50 focus:z-10 focus:outline-none focus:ring-2 
                     focus:ring-primary-500 focus:ring-offset-2 
                     disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-neutral-0
                     transition-colors duration-200"
                    aria-label="Next page"
                    type="button"
                >
                    Next
                    <svg
                        className="h-5 w-5 ml-1"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                        aria-hidden="true"
                    >
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                </button>
            </div>
        </nav>
    );
}
