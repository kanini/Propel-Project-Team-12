import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

/**
 * Upload state for tracking individual file upload
 */
export interface UploadState {
  sessionId: string | null;
  file: File | null;
  progress: number; // 0-100
  status: 'idle' | 'validating' | 'uploading' | 'complete' | 'error' | 'paused';
  error: string | null;
  chunksReceived: number;
  totalChunks: number;
  documentId: string | null;
  pusherChannel: string | null;
}

/**
 * Documents slice state
 */
interface DocumentsState {
  uploads: Record<string, UploadState>; // Key: file name or session ID
  currentUploadId: string | null;
}

const initialState: DocumentsState = {
  uploads: {},
  currentUploadId: null,
};

const documentsSlice = createSlice({
  name: 'documents',
  initialState,
  reducers: {
    // Initialize a new upload
    initializeUpload: (state, action: PayloadAction<{ uploadId: string; file: File }>) => {
      const { uploadId, file } = action.payload;
      state.uploads[uploadId] = {
        sessionId: null,
        file,
        progress: 0,
        status: 'idle',
        error: null,
        chunksReceived: 0,
        totalChunks: 0,
        documentId: null,
        pusherChannel: null,
      };
      state.currentUploadId = uploadId;
    },

    // Set upload session details from backend response
    setUploadSession: (
      state,
      action: PayloadAction<{
        uploadId: string;
        sessionId: string;
        totalChunks: number;
        pusherChannel: string;
      }>
    ) => {
      const { uploadId, sessionId, totalChunks, pusherChannel } = action.payload;
      if (state.uploads[uploadId]) {
        state.uploads[uploadId].sessionId = sessionId;
        state.uploads[uploadId].totalChunks = totalChunks;
        state.uploads[uploadId].pusherChannel = pusherChannel;
        state.uploads[uploadId].status = 'uploading';
      }
    },

    // Update upload progress from chunk upload or Pusher event
    updateUploadProgress: (
      state,
      action: PayloadAction<{ uploadId: string; chunksReceived: number; percentComplete: number }>
    ) => {
      const { uploadId, chunksReceived, percentComplete } = action.payload;
      if (state.uploads[uploadId]) {
        state.uploads[uploadId].chunksReceived = chunksReceived;
        state.uploads[uploadId].progress = percentComplete;
        state.uploads[uploadId].status = percentComplete >= 100 ? 'complete' : 'uploading';
      }
    },

    // Mark upload as complete
    completeUpload: (state, action: PayloadAction<{ uploadId: string; documentId: string }>) => {
      const { uploadId, documentId } = action.payload;
      if (state.uploads[uploadId]) {
        state.uploads[uploadId].documentId = documentId;
        state.uploads[uploadId].status = 'complete';
        state.uploads[uploadId].progress = 100;
      }
    },

    // Set upload error
    setUploadError: (state, action: PayloadAction<{ uploadId: string; error: string }>) => {
      const { uploadId, error } = action.payload;
      if (state.uploads[uploadId]) {
        state.uploads[uploadId].status = 'error';
        state.uploads[uploadId].error = error;
      }
    },

    // Pause upload (network interruption)
    pauseUpload: (state, action: PayloadAction<{ uploadId: string }>) => {
      const { uploadId } = action.payload;
      if (state.uploads[uploadId]) {
        state.uploads[uploadId].status = 'paused';
      }
    },

    // Resume upload
    resumeUpload: (state, action: PayloadAction<{ uploadId: string }>) => {
      const { uploadId } = action.payload;
      if (state.uploads[uploadId]) {
        state.uploads[uploadId].status = 'uploading';
        state.uploads[uploadId].error = null;
      }
    },

    // Remove upload from state
    removeUpload: (state, action: PayloadAction<{ uploadId: string }>) => {
      const { uploadId } = action.payload;
      delete state.uploads[uploadId];
      if (state.currentUploadId === uploadId) {
        state.currentUploadId = null;
      }
    },

    // Clear all uploads
    clearUploads: (state) => {
      state.uploads = {};
      state.currentUploadId = null;
    },
  },
});

export const {
  initializeUpload,
  setUploadSession,
  updateUploadProgress,
  completeUpload,
  setUploadError,
  pauseUpload,
  resumeUpload,
  removeUpload,
  clearUploads,
} = documentsSlice.actions;

export default documentsSlice.reducer;
