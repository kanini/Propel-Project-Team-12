/**
 * DocumentUpload container component for clinical document upload (US_042, SCR-014).
 * Supports multiple files with chunked upload lifecycle and real-time progress tracking.
 * Matches wireframe SCR-014: file list with individual progress bars.
 */

import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../store';
import { FileDropZone } from './FileDropZone';
import { initializeUpload, setUploadSession, updateUploadProgress, markChunksUploaded, setUploadError, removeUpload } from '../../store/documentsSlice';
import { initializeChunkedUpload, uploadChunk, calculateChunkCount, getChunkBlob } from '../../api/documentsApi';

export function DocumentUpload() {
  const dispatch = useDispatch<AppDispatch>();
  const [uploadIds, setUploadIds] = useState<string[]>([]);
  const uploads = useSelector((state: RootState) => state.documents.uploads);
  const userId = useSelector((state: RootState) => state.auth.user?.userId);

  const handleFilesSelected = async (files: File[]) => {
    if (!userId) {
      alert('Please log in to upload documents');
      return;
    }

    // Process each file concurrently
    const uploadPromises = files.map((file) => uploadSingleFile(file));
    await Promise.allSettled(uploadPromises);
  };

  const uploadSingleFile = async (file: File) => {
    const newUploadId = `${file.name}-${Date.now()}`;
    setUploadIds((prev) => [...prev, newUploadId]);

    dispatch(initializeUpload({ uploadId: newUploadId, file }));

    try {
      const chunkSize = 1048576; // 1MB
      const totalChunks = calculateChunkCount(file.size, chunkSize);

      // Initialize upload session with backend
      const initResponse = await initializeChunkedUpload({
        fileName: file.name,
        fileSize: file.size,
        mimeType: file.type,
        totalChunks,
        patientId: userId!,
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

        dispatch(updateUploadProgress({
          uploadId: newUploadId,
          chunksReceived: chunkResponse.chunksReceived,
          percentComplete: chunkResponse.percentComplete,
        }));
      }

      // Mark as chunks_uploaded (NOT finalized yet — waits for "Submit all documents")
      dispatch(markChunksUploaded({ uploadId: newUploadId }));

    } catch (error) {
      dispatch(setUploadError({
        uploadId: newUploadId,
        error: error instanceof Error ? error.message : 'Upload failed',
      }));
      console.error('Upload error:', error);
    }
  };

  const handleRemoveUpload = (uploadId: string) => {
    dispatch(removeUpload({ uploadId }));
    setUploadIds((prev) => prev.filter((id) => id !== uploadId));
  };

  const activeUploads = uploadIds
    .map((id) => ({ id, upload: uploads[id] }))
    .filter((entry) => entry.upload != null);

  const hasActiveUploads = activeUploads.length > 0;

  return (
    <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-6">
      {/* Always show drop zone to allow adding more files */}
      <FileDropZone
        onFilesSelected={handleFilesSelected}
        disabled={false}
      />

      {/* File list matching wireframe SCR-014 */}
      {hasActiveUploads && (
        <div className="mt-6 space-y-3" role="list" aria-label="Uploaded files" aria-live="polite">
          {activeUploads.map(({ id, upload }) => (
            <div
              key={id}
              className="flex items-center gap-3 p-3 border border-neutral-200 rounded-lg bg-white"
              role="listitem"
            >
              {/* PDF icon */}
              <div className="w-9 h-9 rounded-lg bg-red-50 text-red-600 flex items-center justify-center text-xs font-bold flex-shrink-0">
                PDF
              </div>

              {/* File info + progress */}
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between mb-1">
                  <span className="text-sm font-medium text-neutral-800 truncate">
                    {upload!.file?.name || 'Unknown file'}
                  </span>
                  <span className="text-xs text-neutral-500 ml-2 flex-shrink-0">
                    {((upload!.file?.size || 0) / 1024 / 1024).toFixed(1)} MB
                  </span>
                </div>

                {/* Progress bar */}
                {(upload!.status === 'uploading' || upload!.status === 'idle' || upload!.status === 'validating') && (
                  <div className="w-full bg-neutral-200 rounded-full h-1.5 overflow-hidden">
                    <div
                      className="h-1.5 rounded-full bg-primary-500 transition-all duration-300"
                      style={{ width: `${upload!.progress}%` }}
                      role="progressbar"
                      aria-valuenow={upload!.progress}
                      aria-valuemin={0}
                      aria-valuemax={100}
                    />
                  </div>
                )}
                {upload!.status === 'chunks_uploaded' && (
                  <div className="w-full bg-neutral-200 rounded-full h-1.5 overflow-hidden">
                    <div className="h-1.5 rounded-full bg-green-500 w-full" />
                  </div>
                )}
                {upload!.status === 'submitting' && (
                  <div className="w-full bg-neutral-200 rounded-full h-1.5 overflow-hidden">
                    <div className="h-1.5 rounded-full bg-blue-500 w-full animate-pulse" />
                  </div>
                )}
                {upload!.status === 'complete' && (
                  <div className="w-full bg-neutral-200 rounded-full h-1.5 overflow-hidden">
                    <div className="h-1.5 rounded-full bg-green-500 w-full" />
                  </div>
                )}
                {upload!.status === 'error' && (
                  <div className="w-full bg-neutral-200 rounded-full h-1.5 overflow-hidden">
                    <div className="h-1.5 rounded-full bg-red-500 w-full" />
                  </div>
                )}
              </div>

              {/* Status indicator */}
              <div className="flex-shrink-0">
                {upload!.status === 'uploading' && (
                  <span className="text-xs font-medium text-primary-500">{upload!.progress}%</span>
                )}
                {upload!.status === 'chunks_uploaded' && (
                  <span className="text-xs font-medium text-green-600">&#10003; Uploaded</span>
                )}
                {upload!.status === 'submitting' && (
                  <span className="text-xs font-medium text-blue-600">Submitting...</span>
                )}
                {upload!.status === 'complete' && (
                  <span className="text-xs font-medium text-green-600">&#10003; Processing</span>
                )}
                {upload!.status === 'error' && (
                  <span className="text-xs font-medium text-red-600">Failed</span>
                )}
              </div>

              {/* Remove button */}
              {(upload!.status === 'chunks_uploaded' || upload!.status === 'error') && (
                <button
                  onClick={() => handleRemoveUpload(id)}
                  className="text-neutral-400 hover:text-red-500 transition-colors p-1"
                  aria-label={`Remove ${upload!.file?.name}`}
                >
                  <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                  </svg>
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
