/**
 * Document Upload page wrapper (US_042, SCR-014).
 * Matches wireframe SCR-014: Upload clinical documents.
 * "Submit all documents" triggers batch finalize + RAG pipeline (EP006-EP008).
 */

import { useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../store';
import { DocumentUpload } from '../components/documents/DocumentUpload';
import { submitAllDocuments } from '../store/documentsSlice';

export function DocumentUploadPage() {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const uploads = useSelector((state: RootState) => state.documents.uploads);
  const isSubmittingAll = useSelector((state: RootState) => state.documents.isSubmittingAll);
  const submitError = useSelector((state: RootState) => state.documents.submitError);

  // Count uploads ready for submission (chunks fully uploaded, not yet finalized)
  const readyCount = Object.values(uploads).filter(
    (u) => u.status === 'chunks_uploaded'
  ).length;
  const hasAnyUpload = Object.keys(uploads).length > 0;
  const isUploading = Object.values(uploads).some((u) => u.status === 'uploading');

  const handleSubmitAll = useCallback(async () => {
    const result = await dispatch(submitAllDocuments());
    if (submitAllDocuments.fulfilled.match(result)) {
      // Navigate to document status page after successful submission
      navigate('/documents/status');
    }
  }, [dispatch, navigate]);

  return (
    <div className="min-h-screen bg-neutral-100">
      {/* Page header matching wireframe */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Page title and description matching SCR-014 */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-neutral-900 mb-2">
            Upload clinical documents
          </h1>
          <p className="text-sm text-neutral-600">
            Upload your medical records, lab results, or clinical documents for processing
          </p>
        </div>

        {/* Info alert matching wireframe */}
        <div className="mb-6 flex items-start gap-2 px-4 py-3 bg-blue-50 border border-blue-200 rounded-md text-sm text-blue-800">
          <svg className="w-5 h-5 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
          </svg>
          <span>Accepted format: PDF only. Maximum file size: 10 MB per file.</span>
        </div>

        {/* Upload component */}
        <DocumentUpload />

        {/* Submit error alert */}
        {submitError && (
          <div className="mt-4 flex items-start gap-2 px-4 py-3 bg-red-50 border border-red-200 rounded-md text-sm text-red-800" role="alert">
            <svg className="w-5 h-5 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            </svg>
            <span>{submitError}</span>
          </div>
        )}

        {/* Action buttons at bottom matching wireframe */}
        <div className="flex justify-end gap-3 mt-6">
          <Link
            to="/documents/status"
            className="px-6 py-2.5 bg-white text-neutral-700 border border-neutral-300 rounded-md hover:bg-neutral-50 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 font-medium"
          >
            View document status
          </Link>
          <button
            onClick={handleSubmitAll}
            disabled={readyCount === 0 || isSubmittingAll || isUploading}
            className="px-6 py-2.5 bg-primary-500 text-white rounded-md hover:bg-primary-600 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmittingAll
              ? 'Submitting...'
              : `Submit all documents${readyCount > 0 ? ` (${readyCount})` : ''}`}
          </button>
        </div>
      </div>
    </div>
  );
}
