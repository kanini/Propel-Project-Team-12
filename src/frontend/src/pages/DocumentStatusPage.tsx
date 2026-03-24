/**
 * DocumentStatusPage - Document Status Tracking page (US_044, SCR-015).
 * Displays all uploaded documents with real-time processing status.
 * Matches wireframe SCR-015.
 */

import { useNavigate } from 'react-router-dom';
import DocumentStatusList from '../components/documents/DocumentStatusList';

export default function DocumentStatusPage() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-neutral-100">
      {/* Main content matching wireframe SCR-015 */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Page header matching wireframe */}
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-3xl font-bold text-neutral-900">
            Document status
          </h1>
          <button
            onClick={() => navigate('/documents')}
            className="px-5 py-2.5 bg-primary-500 text-white rounded-md hover:bg-primary-600 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 font-medium"
          >
            Upload new
          </button>
        </div>

        {/* Document list table */}
        <DocumentStatusList />
      </main>
    </div>
  );
}
