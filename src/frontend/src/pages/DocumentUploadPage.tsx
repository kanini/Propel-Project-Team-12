/**
 * Document Upload page wrapper (US_042).
 */

import { DocumentUpload } from '../components/documents/DocumentUpload';

export function DocumentUploadPage() {
  return (
    <div className="min-h-screen bg-gray-100">
      <div className="container mx-auto py-8">
        <DocumentUpload />
      </div>
    </div>
  );
}
