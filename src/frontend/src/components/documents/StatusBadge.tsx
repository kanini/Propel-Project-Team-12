/**
 * StatusBadge component for displaying document processing status (US_044, AC1).
 * Color-coded badges matching design system specifications.
 */

interface StatusBadgeProps {
  status: 'Uploaded' | 'Processing' | 'Completed' | 'Failed';
}

export default function StatusBadge({ status }: StatusBadgeProps) {
  // Map status to Tailwind classes from design system
  const statusClasses = {
    Uploaded: 'bg-blue-100 text-blue-800', // Primary blue
    Processing: 'bg-amber-100 text-amber-800', // Warning amber
    Completed: 'bg-green-100 text-green-800', // Success green
    Failed: 'bg-red-100 text-red-800', // Error red
  };

  const statusIcons = {
    Uploaded: (
      <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
        <path d="M3 17a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zM6.293 6.707a1 1 0 010-1.414l3-3a1 1 0 011.414 0l3 3a1 1 0 01-1.414 1.414L11 5.414V13a1 1 0 11-2 0V5.414L7.707 6.707a1 1 0 01-1.414 0z" />
      </svg>
    ),
    Processing: (
      <svg className="w-3 h-3 animate-spin" fill="none" viewBox="0 0 24 24" aria-hidden="true">
        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
      </svg>
    ),
    Completed: (
      <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
        <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
      </svg>
    ),
    Failed: (
      <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
      </svg>
    ),
  };

  return (
    <span
      className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${statusClasses[status]}`}
      role="status"
    >
      {statusIcons[status]}
      {status}
      <span className="sr-only">status</span>
    </span>
  );
}
