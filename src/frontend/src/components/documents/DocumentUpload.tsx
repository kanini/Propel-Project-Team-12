/**
 * DocumentUpload container component for clinical document upload (US_042, SCR-014).
 * Manages chunked upload lifecycle with real-time progress tracking.
 * Matches wireframe SCR-014 design.
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
    <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-6">
      {!upload || upload.status === 'idle' ? (
        <FileDropZone onFileSelected={handleFileSelected} disabled={!!upload} />
      ) : (
        <div className="space-y-4" role="region" aria-live="polite">
          <UploadProgressBar
            progress={upload.progress}
            status={upload.status}
            fileName={upload.file?.name || 'Unknown file'}
            chunksReceived={upload.chunksReceived}
            totalChunks={upload.totalChunks}
          />

          {upload.status === 'complete' && (
            <div className="flex items-start gap-3 p-4 bg-green-50 border border-green-200 rounded-md">
              <svg className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
              <div className="flex-1">
                <h3 className="font-semibold text-green-800">Uploaded</h3>
                <p className="mt-1 text-sm text-green-700">
                  <strong>{upload.file?.name}</strong> ({((upload.file?.size || 0) / 1024 / 1024).toFixed(1)} MB)
                </p>
              </div>
              <button
                onClick={handleRemoveUpload}
                className="text-neutral-400 hover:text-neutral-600 transition-colors"
                aria-label="Remove file"
              >
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                </svg>
              </button>
            </div>
          )}

          {upload.status === 'error' && (
            <div className="p-4 bg-red-50 border border-red-200 rounded-md" role="alert">
              <div className="flex items-start gap-3">
                <svg className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                </svg>
                <div className="flex-1">
                  <h3 className="font-semibold text-red-800">Upload Failed</h3>
                  <p className="mt-1 text-sm text-red-700">{upload.error}</p>
                </div>
              </div>
              <button
                onClick={handleRemoveUpload}
                className="mt-3 px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition-colors"
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
