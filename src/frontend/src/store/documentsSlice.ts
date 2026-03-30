import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { DocumentStatus } from '../api/documentsApi';
import * as documentsApi from '../api/documentsApi';

/**
 * Upload state for tracking individual file upload
 */
export interface UploadState {
  sessionId: string | null;
  file: File | null;
  progress: number; // 0-100
  status: 'idle' | 'validating' | 'uploading' | 'chunks_uploaded' | 'submitting' | 'complete' | 'error' | 'paused';
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
  documents: DocumentStatus[]; // All user documents (US_044)
  documentsLoading: boolean;
  documentsError: string | null;
  retryingDocumentId: string | null; // Track retry in progress
  isSubmittingAll: boolean; // Track batch submit in progress
  submitError: string | null;
}

const initialState: DocumentsState = {
  uploads: {},
  currentUploadId: null,
  documents: [],
  documentsLoading: false,
  documentsError: null,
  retryingDocumentId: null,
  isSubmittingAll: false,
  submitError: null,
};

/**
 * Async thunk: Fetch all user documents (US_044, AC1)
 */
export const fetchUserDocuments = createAsyncThunk(
  'documents/fetchUserDocuments',
  async (_, { rejectWithValue }) => {
    try {
      return await documentsApi.fetchDocuments();
    } catch (error) {
      if (error instanceof Error) {
        return rejectWithValue(error.message);
      }
      return rejectWithValue('Failed to fetch documents');
    }
  }
);

/**
 * Async thunk: Retry failed document processing (US_044, Edge Case)
 */
export const retryDocumentProcessing = createAsyncThunk(
  'documents/retryDocumentProcessing',
  async (documentId: string, { rejectWithValue }) => {
    try {
      return await documentsApi.retryDocumentProcessing(documentId);
    } catch (error) {
      if (error instanceof Error) {
        return rejectWithValue(error.message);
      }
      return rejectWithValue('Failed to retry document processing');
    }
  }
);

/**
 * Async thunk: Submit all uploaded documents for processing (EP006-EP008).
 * Finalizes all upload sessions with 'chunks_uploaded' status, triggering the RAG pipeline for each.
 */
export const submitAllDocuments = createAsyncThunk(
  'documents/submitAllDocuments',
  async (_, { getState, rejectWithValue }) => {
    try {
      const state = getState() as { documents: DocumentsState };
      // Note: pending reducer already changed chunks_uploaded → submitting before this runs
      const sessionIds = Object.values(state.documents.uploads)
        .filter((u) => u.status === 'submitting' && u.sessionId)
        .map((u) => u.sessionId as string);

      if (sessionIds.length === 0) {
        return rejectWithValue('No documents ready to submit');
      }

      return await documentsApi.submitAllDocuments(sessionIds);
    } catch (error) {
      if (error instanceof Error) {
        return rejectWithValue(error.message);
      }
      return rejectWithValue('Failed to submit documents');
    }
  }
);

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
        // When all chunks uploaded, mark as 'chunks_uploaded' (awaiting submit)
        state.uploads[uploadId].status = percentComplete >= 100 ? 'chunks_uploaded' : 'uploading';
      }
    },

    // Explicitly mark upload as chunks_uploaded (all chunks received, not yet submitted)
    markChunksUploaded: (state, action: PayloadAction<{ uploadId: string }>) => {
      const { uploadId } = action.payload;
      if (state.uploads[uploadId]) {
        state.uploads[uploadId].status = 'chunks_uploaded';
        state.uploads[uploadId].progress = 100;
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

    // Update document status from Pusher event (US_044, AC2)
    updateDocumentStatus: (
      state,
      action: PayloadAction<{
        documentId: string;
        status: 'Uploaded' | 'Processing' | 'Completed' | 'Failed';
        errorMessage?: string | null;
        processedAt?: string | null;
        processingTimeMs?: number | null;
      }>
    ) => {
      const { documentId, status, errorMessage, processedAt, processingTimeMs } = action.payload;
      const docIndex = state.documents.findIndex((d) => d.id === documentId);
      
      if (docIndex !== -1) {
        const doc = state.documents[docIndex];
        if (doc) {
          doc.status = status;
          doc.errorMessage = errorMessage ?? null;
          doc.processedAt = processedAt ?? null;
          doc.processingTimeMs = processingTimeMs ?? null;
          doc.isStuckProcessing = false; // Reset on update
        }
      }
    },
  },
  extraReducers: (builder) => {
    // Fetch user documents
    builder
      .addCase(fetchUserDocuments.pending, (state) => {
        state.documentsLoading = true;
        state.documentsError = null;
      })
      .addCase(fetchUserDocuments.fulfilled, (state, action) => {
        state.documentsLoading = false;
        state.documents = action.payload;
      })
      .addCase(fetchUserDocuments.rejected, (state, action) => {
        state.documentsLoading = false;
        state.documentsError = action.payload as string;
      });

    // Retry document processing
    builder
      .addCase(retryDocumentProcessing.pending, (state, action) => {
        state.retryingDocumentId = action.meta.arg;
      })
      .addCase(retryDocumentProcessing.fulfilled, (state, action) => {
        state.retryingDocumentId = null;
        // Update document in list with new status
        const docIndex = state.documents.findIndex((d) => d.id === action.payload.id);
        if (docIndex !== -1) {
          state.documents[docIndex] = action.payload;
        }
      })
      .addCase(retryDocumentProcessing.rejected, (state) => {
        state.retryingDocumentId = null;
      });

    // Submit all documents (batch finalize + trigger processing)
    builder
      .addCase(submitAllDocuments.pending, (state) => {
        state.isSubmittingAll = true;
        state.submitError = null;
        // Mark all chunks_uploaded sessions as submitting
        for (const key of Object.keys(state.uploads)) {
          if (state.uploads[key]?.status === 'chunks_uploaded') {
            state.uploads[key].status = 'submitting';
          }
        }
      })
      .addCase(submitAllDocuments.fulfilled, (state, action) => {
        state.isSubmittingAll = false;
        // Mark each successfully finalized upload as complete
        for (const result of action.payload.results) {
          const uploadEntry = Object.entries(state.uploads).find(
            ([, u]) => u.sessionId === result.sessionId
          );
          if (uploadEntry) {
            const [uploadId] = uploadEntry;
            if (result.success && state.uploads[uploadId]) {
              state.uploads[uploadId].status = 'complete';
              state.uploads[uploadId].documentId = result.documentId ?? null;
            } else if (state.uploads[uploadId]) {
              state.uploads[uploadId].status = 'error';
              state.uploads[uploadId].error = result.error ?? 'Submission failed';
            }
          }
        }
      })
      .addCase(submitAllDocuments.rejected, (state, action) => {
        state.isSubmittingAll = false;
        state.submitError = action.payload as string;
        // Revert submitting uploads back to chunks_uploaded
        for (const key of Object.keys(state.uploads)) {
          if (state.uploads[key]?.status === 'submitting') {
            state.uploads[key].status = 'chunks_uploaded';
          }
        }
      });
  },
});

export const {
  initializeUpload,
  setUploadSession,
  updateUploadProgress,
  markChunksUploaded,
  completeUpload,
  setUploadError,
  pauseUpload,
  resumeUpload,
  removeUpload,
  clearUploads,
  updateDocumentStatus,
} = documentsSlice.actions;

export default documentsSlice.reducer;
