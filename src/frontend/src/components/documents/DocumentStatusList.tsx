/**
 * DocumentStatusList component for displaying all user documents (US_044).
 * Features: real-time Pusher updates, skeleton loading, empty state, ARIA live regions.
 */

import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import type { DocumentStatus } from '../../api/documentsApi';
import DocumentStatusRow from './DocumentStatusRow';
import { fetchUserDocuments } from '../../store/documentsSlice';
import { usePusherDocumentStatus } from '../../hooks/usePusherDocumentStatus';
import type { RootState } from '../../store';

export default function DocumentStatusList() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  
  const { documents, documentsLoading, documentsError } = useSelector(
    (state: RootState) => state.documents
  );

  // Get user ID from auth state for Pusher subscription
  const userId = useSelector((state: RootState) => state.auth?.user?.userId || null);

  // Fetch documents on mount
  useEffect(() => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    dispatch(fetchUserDocuments() as any);
  }, [dispatch]);

  // Subscribe to Pusher for real-time status updates (AC2)
  usePusherDocumentStatus({ userId, enabled: !!userId });

  // Skeleton loader (UXR-502)
  if (documentsLoading) {
    return (
      <div className="bg-white rounded-lg shadow border border-neutral-200 overflow-hidden">
        <table className="min-w-full divide-y divide-neutral-200" aria-label="Documents loading">
          <thead className="bg-neutral-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider">
                Filename
              </th>
              <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider">
                Upload Date
              </th>
              <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider">
                Size
              </th>
              <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider">
                Status
              </th>
              <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider">
                Extracted Data
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-neutral-100">
            {[...Array(3)].map((_, index) => (
              <tr key={index} className="animate-pulse">
                <td className="px-4 py-3">
                  <div className="h-4 bg-neutral-200 rounded w-3/4"></div>
                </td>
                <td className="px-4 py-3">
                  <div className="h-4 bg-neutral-200 rounded w-24"></div>
                </td>
                <td className="px-4 py-3">
                  <div className="h-4 bg-neutral-200 rounded w-16"></div>
                </td>
                <td className="px-4 py-3">
                  <div className="h-6 bg-neutral-200 rounded-full w-20"></div>
                </td>
                <td className="px-4 py-3">
                  <div className="h-4 bg-neutral-200 rounded w-16"></div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  }

  // Error state
  if (documentsError) {
    return (
      <div className="bg-white rounded-lg shadow border border-neutral-200 p-8 text-center">
        <div className="flex flex-col items-center gap-4">
          <svg className="w-12 h-12 text-error" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div>
            <h3 className="text-lg font-semibold text-neutral-900 mb-1">Unable to load documents</h3>
            <p className="text-sm text-neutral-600">{documentsError}</p>
          </div>
          <button
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            onClick={() => dispatch(fetchUserDocuments() as any)}
            className="mt-2 px-4 py-2 bg-primary-500 text-white rounded-md hover:bg-primary-600 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
          >
            Try again
          </button>
        </div>
      </div>
    );
  }

  // Empty state (UXR-605, AC4)
  if (documents.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow border border-neutral-200 p-12 text-center">
        <div className="flex flex-col items-center gap-4 max-w-md mx-auto">
          {/* Illustration placeholder - use actual illustration in production */}
          <svg className="w-24 h-24 text-neutral-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <div>
            <h3 className="text-xl font-semibold text-neutral-900 mb-2">No documents yet</h3>
            <p className="text-neutral-600 mb-6">
              Upload your first clinical document to start building your health dashboard.
            </p>
          </div>
          <button
            onClick={() => navigate('/documents/upload')}
            className="px-6 py-3 bg-primary-500 text-white rounded-md hover:bg-primary-600 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 font-medium"
          >
            Upload your first document
          </button>
        </div>
      </div>
    );
  }

  // Documents table (AC1)
  return (
    <div className="bg-white rounded-lg shadow border border-neutral-200 overflow-hidden">
      <table className="min-w-full divide-y divide-neutral-200" aria-label="Document processing status">
        <thead className="bg-neutral-50">
          <tr>
            <th 
              scope="col" 
              className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
            >
              Filename
            </th>
            <th 
              scope="col" 
              className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
            >
              Upload Date
            </th>
            <th 
              scope="col" 
              className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
            >
              Size
            </th>
            <th 
              scope="col" 
              className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
            >
              Status
            </th>
            <th 
              scope="col" 
              className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
            >
              Extracted Data
            </th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-neutral-100">
          {documents.map((document: DocumentStatus) => (
            <DocumentStatusRow key={document.id} document={document} />
          ))}
        </tbody>
      </table>
    </div>
  );
}
