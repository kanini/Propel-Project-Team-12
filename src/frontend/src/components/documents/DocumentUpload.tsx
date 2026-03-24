/**
 * DocumentUpload container component for clinical document upload (US_042, AC1-AC4).
 * Manages chunked upload lifecycle with real-time progress tracking.
 */

import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState } from '../../store';
import { FileDropZone } from './FileDropZone';
import { UploadProgressBar } from './UploadProgressBar';
import { usePusherUpload } from '../../hooks/usePusherUpload';
import { initializeUpload, setUploadSession, updateUploadProgress, completeUpload, setUploadError, removeUpload } from '../../store/documentsSlice';
import { initializeChunkedUpload, uploadChunk, finalizeUpload, calculateChunkCount, getChunkBlob } from '../../api/documentsApi';

export function DocumentUpload() {
  const dispatch = useDispatch();
  const [uploadId, setUploadId] = useState<string | null>(null);
  const upload = useSelector((state: RootState) => uploadId ? state.documents.uploads[uploadId] : null);
  const userId = useSelector((state: RootState) => state.auth.user?.userId);

  // Enable Pusher updates when session is established
  usePusherUpload({
    uploadId: uploadId || '',
    pusherChannel: upload?.pusherChannel || null,
    enabled: !!upload?.pusherChannel,
  });

  const handleFileSelected = async (file: File) => {
    if (!userId) {
      alert('Please log in to upload documents');
      return;
    }

    try {
      // Create upload ID
      const newUploadId = `${file.name}-${Date.now()}`;
      setUploadId(newUploadId);

      // Initialize upload in Redux
      dispatch(initializeUpload({ uploadId: newUploadId, file }));

      // Calculate chunks
      const chunkSize = 1048576; // 1MB
      const totalChunks = calculateChunkCount(file.size, chunkSize);

      // Initialize upload session with backend
      const initResponse = await initializeChunkedUpload({
        fileName: file.name,
        fileSize: file.size,
        mimeType: file.type,
        totalChunks,
        patientId: userId, // Using authenticated user as patient
      });

      dispatch(setUploadSession({
        uploadId: newUploadId,
        sessionId: initResponse.uploadSessionId,
        totalChunks,
        pusherChannel: initResponse.pusherChannel,
      }));

      // Upload chunks sequentially
      for (let i = 0; i < totalChunks; i++) {
        const chunkBlob = getChunkBlob(file, i, chunkSize);
        const chunkResponse = await uploadChunk(initResponse.uploadSessionId, i, chunkBlob);

        // Update progress (Pusher will also update, but this provides immediate feedback)
        dispatch(updateUploadProgress({
          uploadId: newUploadId,
          chunksReceived: chunkResponse.chunksReceived,
          percentComplete: chunkResponse.percentComplete,
        }));
      }

      // Finalize upload
      const finalResponse = await finalizeUpload(initResponse.uploadSessionId);

      dispatch(completeUpload({
        uploadId: newUploadId,
        documentId: finalResponse.documentId,
      }));

    } catch (error) {
      if (uploadId) {
        dispatch(setUploadError({
          uploadId,
          error: error instanceof Error ? error.message : 'Upload failed',
        }));
      }
      console.error('Upload error:', error);
    }
  };

  const handleRemoveUpload = () => {
    if (uploadId) {
      dispatch(removeUpload({ uploadId }));
      setUploadId(null);
    }
  };

  return (
    <div className="max-w-2xl mx-auto p-6 space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-gray-900">Upload Clinical Document</h2>
        <p className="mt-2 text-gray-600">Upload your PDF clinical documents for processing</p>
      </div>

      {!upload || upload.status === 'idle' ? (
        <FileDropZone onFileSelected={handleFileSelected} disabled={!!upload} />
      ) : (
        <div className="space-y-4">
          <UploadProgressBar
            progress={upload.progress}
            status={upload.status}
            fileName={upload.file?.name || 'Unknown file'}
            chunksReceived={upload.chunksReceived}
            totalChunks={upload.totalChunks}
          />

          {upload.status === 'complete' && (
            <div className="p-4 bg-green-50 border border-green-200 rounded-md" role="alert">
              <h3 className="font-semibold text-green-800">Upload Successful!</h3>
              <div className="mt-2 text-sm text-green-700">
                <p><strong>Document:</strong> {upload.file?.name}</p>
                <p><strong>Size:</strong> {(upload.file?.size || 0 / 1024 / 1024).toFixed(2)} MB</p>
                <p><strong>Status:</strong> Uploaded — Processing pending</p>
              </div>
              <button
                onClick={handleRemoveUpload}
                className="mt-3 px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
              >
                Upload Another Document
              </button>
            </div>
          )}

          {upload.status === 'error' && (
            <div className="p-4 bg-red-50 border border-red-200 rounded-md" role="alert">
              <h3 className="font-semibold text-red-800">Upload Failed</h3>
              <p className="mt-1 text-sm text-red-700">{upload.error}</p>
              <button
                onClick={handleRemoveUpload}
                className="mt-3 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
              >
                Try Again
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
