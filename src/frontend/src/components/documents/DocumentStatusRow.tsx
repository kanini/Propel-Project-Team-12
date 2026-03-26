/**
 * DocumentStatusRow component for displaying individual document status (US_044).
 * Shows metadata, status badge, and action buttons (View/Retry).
 */

import { format } from 'date-fns';
import { useDispatch, useSelector } from 'react-redux';
import type { DocumentStatus } from '../../api/documentsApi';
import StatusBadge from './StatusBadge';
import { retryDocumentProcessing } from '../../store/documentsSlice';
import type { RootState } from '../../store';

interface DocumentStatusRowProps {
  document: DocumentStatus;
}

export default function DocumentStatusRow({ document }: DocumentStatusRowProps) {
  const dispatch = useDispatch();
  const retryingDocumentId = useSelector((state: RootState) => state.documents.retryingDocumentId);
  
  const isRetrying = retryingDocumentId === document.id;

  // Format upload date
  const formattedDate = format(new Date(document.uploadedAt), 'MMM d, yyyy');

  // Handle retry button click
  const handleRetry = async () => {
    try {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      await dispatch(retryDocumentProcessing(document.id) as any);
    } catch (error) {
      console.error('Failed to retry document processing:', error);
      // Error handling is managed by Redux slice
    }
  };

  return (
    <tr className="hover:bg-neutral-50 transition-colors">
      {/* File name */}
      <td className="px-4 py-3 text-sm font-medium text-neutral-900">
        {document.fileName}
      </td>

      {/* Upload date */}
      <td className="px-4 py-3 text-sm text-neutral-600">
        {formattedDate}
      </td>

      {/* File size */}
      <td className="px-4 py-3 text-sm text-neutral-600">
        {document.fileSizeFormatted}
      </td>

      {/* Status badge with warning for stuck processing */}
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          <StatusBadge status={document.status} />
          {document.isStuckProcessing && (
            <div className="flex items-center group relative">
              <svg 
                className="w-4 h-4 text-amber-500" 
                fill="currentColor" 
                viewBox="0 0 20 20"
                aria-label="Processing is taking longer than expected"
              >
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
              {/* Tooltip */}
              <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 hidden group-hover:block z-10 w-48 px-3 py-2 text-xs text-white bg-neutral-900 rounded-md shadow-lg">
                Processing is taking longer than expected. Please contact support if this persists.
              </div>
            </div>
          )}
        </div>
      </td>

      {/* Actions */}
      <td className="px-4 py-3">
        {document.status === 'Completed' && (
          <a
            href="/health-dashboard"
            className="text-primary-500 hover:text-primary-700 font-medium text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-1 rounded"
          >
            View data
          </a>
        )}
        
        {document.status === 'Failed' && (
          <div className="flex items-center gap-3">
            <button
              onClick={handleRetry}
              disabled={isRetrying}
              className="text-error hover:text-error-dark font-medium text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-error focus:ring-offset-1 rounded disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isRetrying ? 'Retrying...' : 'Retry'}
            </button>
            <span className="text-neutral-300">·</span>
            <a
              href="/intake/manual"
              className="text-neutral-600 hover:text-neutral-800 text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-1 rounded"
            >
              Manual entry
            </a>
          </div>
        )}

        {document.status === 'Processing' && (
          <span className="text-neutral-400 text-sm">Pending...</span>
        )}

        {document.status === 'Uploaded' && (
          <span className="text-neutral-400 text-sm">Queued</span>
        )}

        {document.errorMessage && document.status === 'Failed' && (
          <div className="mt-1 text-xs text-error" role="alert">
            {document.errorMessage}
          </div>
        )}
      </td>
    </tr>
  );
}
