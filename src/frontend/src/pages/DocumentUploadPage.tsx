/**
 * Document Upload page wrapper (US_042, SCR-014).
 * Matches wireframe SCR-014: Upload clinical documents.
 */

import { useRef, useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { DocumentUpload, type DocumentUploadHandle } from '../components/documents/DocumentUpload';

interface UploadMessage {
  type: 'success' | 'warning' | 'error';
  title: string;
  description: string;
}

export function DocumentUploadPage() {
  const uploadRef = useRef<DocumentUploadHandle>(null);
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [uploadMessage, setUploadMessage] = useState<UploadMessage | null>(null);

  // Auto-redirect after showing success message
  useEffect(() => {
    if (uploadMessage?.type === 'success' || uploadMessage?.type === 'warning') {
      const timer = setTimeout(() => {
        navigate('/documents/status');
      }, uploadMessage.type === 'success' ? 2000 : 3000);
      
      return () => clearTimeout(timer);
    }
  }, [uploadMessage, navigate]);

  const handleSubmitAll = async () => {
    if (uploadRef.current) {
      setIsSubmitting(true);
      setUploadMessage(null);
      
      try {
        const result = await uploadRef.current.submit();
        
        if (result.total > 0) {
          if (result.success === result.total) {
            // All files uploaded successfully
            setUploadMessage({
              type: 'success',
              title: 'Upload Successful!',
              description: `${result.success} file(s) uploaded successfully. Redirecting to document status...`
            });
          } else if (result.success > 0) {
            // Some files succeeded
            setUploadMessage({
              type: 'warning',
              title: 'Partial Upload Success',
              description: `${result.success} file(s) uploaded successfully, ${result.failed} failed. Redirecting to document status...`
            });
          } else {
            // All files failed
            setUploadMessage({
              type: 'error',
              title: 'Upload Failed',
              description: `${result.failed} file(s) could not be uploaded. Please review the errors and try again.`
            });
          }
        }
      } finally {
        setIsSubmitting(false);
      }
    }
  };

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

        {/* Upload status message */}
        {uploadMessage && (
          <div 
            className={`mb-6 flex items-start gap-3 px-4 py-4 border rounded-md ${
              uploadMessage.type === 'success' 
                ? 'bg-green-50 border-green-200' 
                : uploadMessage.type === 'warning'
                ? 'bg-yellow-50 border-yellow-200'
                : 'bg-red-50 border-red-200'
            }`}
            role="alert"
          >
            {uploadMessage.type === 'success' && (
              <svg className="w-6 h-6 text-green-600 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
            )}
            {uploadMessage.type === 'warning' && (
              <svg className="w-6 h-6 text-yellow-600 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            )}
            {uploadMessage.type === 'error' && (
              <svg className="w-6 h-6 text-red-600 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
            )}
            <div className="flex-1">
              <h3 className={`font-semibold ${
                uploadMessage.type === 'success' 
                  ? 'text-green-800' 
                  : uploadMessage.type === 'warning'
                  ? 'text-yellow-800'
                  : 'text-red-800'
              }`}>
                {uploadMessage.title}
              </h3>
              <p className={`mt-1 text-sm ${
                uploadMessage.type === 'success' 
                  ? 'text-green-700' 
                  : uploadMessage.type === 'warning'
                  ? 'text-yellow-700'
                  : 'text-red-700'
              }`}>
                {uploadMessage.description}
              </p>
            </div>
            <button
              onClick={() => setUploadMessage(null)}
              className={`flex-shrink-0 ${
                uploadMessage.type === 'success' 
                  ? 'text-green-600 hover:text-green-800' 
                  : uploadMessage.type === 'warning'
                  ? 'text-yellow-600 hover:text-yellow-800'
                  : 'text-red-600 hover:text-red-800'
              }`}
              aria-label="Dismiss message"
            >
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
              </svg>
            </button>
          </div>
        )}

        {/* Upload component */}
        <DocumentUpload ref={uploadRef} />

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
            disabled={isSubmitting}
            className="px-6 py-2.5 bg-primary-500 text-white rounded-md hover:bg-primary-600 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmitting ? 'Uploading...' : 'Submit all documents'}
          </button>
        </div>
      </div>
    </div>
  );
}
