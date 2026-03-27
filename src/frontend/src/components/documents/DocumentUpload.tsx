/**
 * DocumentUpload container component for clinical document upload (US_042, SCR-014).
 * Manages chunked upload lifecycle with real-time progress tracking.
 * Supports multiple files with upload on submit.
 * Matches wireframe SCR-014 design.
 */

import { useState, useEffect, useRef, useImperativeHandle, forwardRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState } from '../../store';
import { FileDropZone } from './FileDropZone';
import { UploadProgressBar } from './UploadProgressBar';
import Pusher from 'pusher-js';
import { initializeUpload, setUploadSession, updateUploadProgress, completeUpload, setUploadError, removeUpload, pauseUpload } from '../../store/documentsSlice';
import { initializeChunkedUpload, uploadChunk, finalizeUpload, calculateChunkCount, getChunkBlob } from '../../api/documentsApi';

interface PendingFile {
  id: string;
  file: File;
  status: 'pending' | 'uploading' | 'complete' | 'error';
}

export interface DocumentUploadResult {
  success: number;
  failed: number;
  total: number;
}

export interface DocumentUploadHandle {
  submit: () => Promise<DocumentUploadResult>;
  getPendingCount: () => number;
  isUploading: () => boolean;
}

export const DocumentUpload = forwardRef<DocumentUploadHandle>((_props, ref) => {
  const dispatch = useDispatch();
  const [pendingFiles, setPendingFiles] = useState<PendingFile[]>([]);
  const [isUploading, setIsUploading] = useState(false);
  const uploads = useSelector((state: RootState) => state.documents.uploads);
  const userId = useSelector((state: RootState) => state.auth.user?.userId);
  const pusherRef = useRef<Pusher | null>(null);
  const channelsRef = useRef<Map<string, ReturnType<Pusher['subscribe']>>>(new Map());

  // Handle Pusher subscriptions for all active uploads
  useEffect(() => {
    const pusherKey = import.meta.env.VITE_PUSHER_KEY;
    const pusherCluster = import.meta.env.VITE_PUSHER_CLUSTER || 'us2';

    if (!pusherKey) {
      console.warn('Pusher key not configured. Upload progress will use polling fallback.');
      return;
    }

    // Initialize Pusher if not already initialized
    if (!pusherRef.current) {
      pusherRef.current = new Pusher(pusherKey, {
        cluster: pusherCluster,
        forceTLS: true,
      });
    }

    const pusher = pusherRef.current;
    const channels = channelsRef.current;

    // Subscribe to channels for uploads that have pusher channels
    Object.entries(uploads).forEach(([uploadId, upload]) => {
      if (upload?.pusherChannel && !channels.has(uploadId)) {
        const channel = pusher.subscribe(upload.pusherChannel);
        channels.set(uploadId, channel);

        // Bind to upload progress events
        channel.bind('chunk-uploaded', (data: {
          chunksReceived: number;
          totalChunks: number;
          percentComplete: number;
          status: string;
        }) => {
          dispatch(updateUploadProgress({
            uploadId,
            chunksReceived: data.chunksReceived,
            percentComplete: data.percentComplete,
          }));
        });

        // Bind to upload complete event
        channel.bind('upload-complete', (data: {
          documentId: string;
          fileName: string;
          fileSize: number;
          status: string;
        }) => {
          dispatch(completeUpload({
            uploadId,
            documentId: data.documentId,
          }));
        });

        // Bind to upload failed event
        channel.bind('upload-failed', (data: { error: string; status: string }) => {
          dispatch(setUploadError({
            uploadId,
            error: data.error,
          }));
        });

        // Bind to upload paused event
        channel.bind('upload-paused', () => {
          dispatch(pauseUpload({ uploadId }));
        });
      }
    });

    // Cleanup on unmount
    return () => {
      channels.forEach((channel, uploadId) => {
        const upload = uploads[uploadId];
        if (upload?.pusherChannel) {
          channel.unbind_all();
          pusher.unsubscribe(upload.pusherChannel);
        }
      });
      channels.clear();
      
      if (pusherRef.current) {
        pusherRef.current.disconnect();
        pusherRef.current = null;
      }
    };
  }, [uploads, dispatch]);

  const handleFilesSelected = (files: File[]) => {
    const newFiles: PendingFile[] = files.map(file => ({
      id: `${file.name}-${Date.now()}-${Math.random()}`,
      file,
      status: 'pending' as const,
    }));
    setPendingFiles(prev => [...prev, ...newFiles]);
  };

  const handleRemoveFile = (fileId: string) => {
    setPendingFiles(prev => prev.filter(f => f.id !== fileId));
  };

  const uploadSingleFile = async (pendingFile: PendingFile) => {
    const { id: uploadId, file } = pendingFile;

    try {
      // Initialize upload in Redux
      dispatch(initializeUpload({ uploadId, file }));

      // Calculate chunks
      const chunkSize = 1048576; // 1MB
      const totalChunks = calculateChunkCount(file.size, chunkSize);

      // Initialize upload session with backend
      const initResponse = await initializeChunkedUpload({
        fileName: file.name,
        fileSize: file.size,
        mimeType: file.type,
        totalChunks,
        patientId: userId || '', // Using authenticated user as patient
      });

      dispatch(setUploadSession({
        uploadId,
        sessionId: initResponse.uploadSessionId,
        totalChunks,
        pusherChannel: initResponse.pusherChannel,
      }));

      // Upload chunks sequentially
      for (let i = 0; i < totalChunks; i++) {
        const chunkBlob = getChunkBlob(file, i, chunkSize);
        const chunkResponse = await uploadChunk(initResponse.uploadSessionId, i, chunkBlob);

        // Update progress
        dispatch(updateUploadProgress({
          uploadId,
          chunksReceived: chunkResponse.chunksReceived,
          percentComplete: chunkResponse.percentComplete,
        }));
      }

      // Finalize upload after all chunks are uploaded
      const finalResponse = await finalizeUpload(initResponse.uploadSessionId);

      dispatch(completeUpload({
        uploadId,
        documentId: finalResponse.documentId,
      }));

      // Update pending file status
      setPendingFiles(prev =>
        prev.map(f => f.id === uploadId ? { ...f, status: 'complete' } : f)
      );

    } catch (error) {
      dispatch(setUploadError({
        uploadId,
        error: error instanceof Error ? error.message : 'Upload failed',
      }));

      // Update pending file status
      setPendingFiles(prev =>
        prev.map(f => f.id === uploadId ? { ...f, status: 'error' } : f)
      );

      console.error('Upload error:', error);
      throw error; // Re-throw to handle in handleSubmit
    }
  };

  const handleSubmit = async (): Promise<DocumentUploadResult> => {
    if (!userId) {
      console.warn('Upload attempted without user authentication');
      return { success: 0, failed: 0, total: 0 };
    }

    if (pendingFiles.length === 0) {
      console.warn('Upload attempted with no files selected');
      return { success: 0, failed: 0, total: 0 };
    }

    setIsUploading(true);

    let successCount = 0;
    let failedCount = 0;
    const totalFiles = pendingFiles.filter(f => f.status === 'pending').length;

    // Upload all files sequentially
    for (const pendingFile of pendingFiles) {
      if (pendingFile.status === 'pending') {
        setPendingFiles(prev =>
          prev.map(f => f.id === pendingFile.id ? { ...f, status: 'uploading' } : f)
        );
        
        try {
          await uploadSingleFile(pendingFile);
          successCount++;
        } catch (error) {
          // Error already handled in uploadSingleFile
          console.error(`Failed to upload ${pendingFile.file.name}:`, error);
          failedCount++;
        }
      }
    }

    setIsUploading(false);

    return {
      success: successCount,
      failed: failedCount,
      total: totalFiles
    };
  };

  const handleClearAll = () => {
    // Remove completed and error uploads
    pendingFiles.forEach(file => {
      if (uploads[file.id]) {
        dispatch(removeUpload({ uploadId: file.id }));
      }
    });
    setPendingFiles([]);
  };

  // Expose methods to parent component via ref
  useImperativeHandle(ref, () => ({
    submit: handleSubmit,
    getPendingCount: () => pendingFiles.filter(f => f.status === 'pending').length,
    isUploading: () => isUploading,
  }));

  const hasErrors = pendingFiles.some(f => f.status === 'error');

  return (
    <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-6">
      {/* File drop zone - always visible */}
      <FileDropZone
        onFileSelected={(file) => handleFilesSelected([file])}
        onFilesSelected={handleFilesSelected}
        disabled={isUploading}
        multiple={true}
      />

      {/* List of selected files */}
      {pendingFiles.length > 0 && (
        <div className="mt-6 space-y-3">
          <div className="flex justify-between items-center mb-3">
            <h3 className="text-sm font-semibold text-neutral-700">
              Selected Files ({pendingFiles.length})
            </h3>
            {!isUploading && (
              <button
                onClick={handleClearAll}
                className="text-sm text-neutral-500 hover:text-neutral-700 transition-colors"
              >
                Clear All
              </button>
            )}
          </div>

          {pendingFiles.map((pendingFile) => {
            const upload = uploads[pendingFile.id];
            const isFileUploading = pendingFile.status === 'uploading';

            return (
              <div key={pendingFile.id} className="border border-neutral-200 rounded-lg p-4">
                {isFileUploading && upload ? (
                  <UploadProgressBar
                    progress={upload.progress}
                    status={upload.status}
                    fileName={pendingFile.file.name}
                    chunksReceived={upload.chunksReceived}
                    totalChunks={upload.totalChunks}
                  />
                ) : (
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3 flex-1">
                      {/* Status icon */}
                      {pendingFile.status === 'pending' && (
                        <svg className="w-5 h-5 text-neutral-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                      )}
                      {pendingFile.status === 'complete' && (
                        <svg className="w-5 h-5 text-green-600 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                        </svg>
                      )}
                      {pendingFile.status === 'error' && (
                        <svg className="w-5 h-5 text-red-600 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                        </svg>
                      )}

                      {/* File info */}
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-neutral-700 truncate">
                          {pendingFile.file.name}
                        </p>
                        <p className="text-xs text-neutral-500">
                          {(pendingFile.file.size / 1024 / 1024).toFixed(2)} MB
                        </p>
                        {pendingFile.status === 'error' && upload?.error && (
                          <p className="text-xs text-red-600 mt-1">{upload.error}</p>
                        )}
                      </div>
                    </div>

                    {/* Remove button (only for pending files) */}
                    {pendingFile.status === 'pending' && !isUploading && (
                      <button
                        onClick={() => handleRemoveFile(pendingFile.id)}
                        className="text-neutral-400 hover:text-neutral-600 transition-colors ml-2"
                        aria-label="Remove file"
                      >
                        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                        </svg>
                      </button>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Error summary */}
      {hasErrors && !isUploading && (
        <div className="mt-6 p-4 bg-yellow-50 border border-yellow-200 rounded-md" role="alert">
          <div className="flex items-start gap-3">
            <svg className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
            <div className="flex-1">
              <h3 className="font-semibold text-yellow-800">Some Uploads Failed</h3>
              <p className="mt-1 text-sm text-yellow-700">
                Please review the errors above and try uploading the failed files again.
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
});

DocumentUpload.displayName = 'DocumentUpload';
