/**
 * Document Upload page wrapper (US_042, SCR-014).
 * Matches wireframe SCR-014: Upload clinical documents.
 */

import { Link } from 'react-router-dom';
import { DocumentUpload } from '../components/documents/DocumentUpload';

export function DocumentUploadPage() {
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

        {/* Action buttons at bottom matching wireframe */}
        <div className="flex justify-end gap-3 mt-6">
          <Link
            to="/documents/status"
            className="px-6 py-2.5 bg-white text-neutral-700 border border-neutral-300 rounded-md hover:bg-neutral-50 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 font-medium"
          >
            View document status
          </Link>
          <button
            className="px-6 py-2.5 bg-primary-500 text-white rounded-md hover:bg-primary-600 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Submit all documents
          </button>
        </div>
      </div>
    </div>
  );
}
