/**
 * Documents API client for chunked file upload (US_042).
 * Handles initialization, chunk upload, and finalization with retry logic.
 */

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
const MAX_RETRY_ATTEMPTS = 3;
const RETRY_DELAYS = [1000, 2000, 4000]; // Exponential backoff in ms

/**
 * Initialize chunked upload session (AC1).
 * @param fileName - Original file name
 * @param fileSize - Total file size in bytes (max 10MB)
 * @param mimeType - MIME type (must be "application/pdf")
 * @param totalChunks - Number of chunks to be uploaded
 * @param patientId - Patient ID for whom the document is uploaded
 * @returns Upload session details
 */
export interface InitializeUploadRequest {
  fileName: string;
  fileSize: number;
  mimeType: string;
  totalChunks: number;
  patientId: string;
}

export interface InitializeUploadResponse {
  uploadSessionId: string;
  chunkSize: number;
  expiresAt: string;
  pusherChannel: string;
}

export async function initializeChunkedUpload(
  request: InitializeUploadRequest
): Promise<InitializeUploadResponse> {
  const token = localStorage.getItem('token');
  
  try {
    const response = await fetch(`${API_BASE_URL}/api/documents/upload/initialize`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }),
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      if (response.status === 400) {
        const error = await response.json();
        throw new Error(error.message || 'Only PDF files up to 10MB are supported');
      }
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      throw new Error(`Failed to initialize upload: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Upload a single chunk with retry logic (AC2).
 * @param sessionId - Upload session identifier
 * @param chunkIndex - Zero-based chunk index
 * @param chunkBlob - Chunk binary data
 * @param attempt - Current retry attempt (internal)
 * @returns Chunk upload status
 */
export interface ChunkUploadResponse {
  chunksReceived: number;
  totalChunks: number;
  percentComplete: number;
  status: string;
}

export async function uploadChunk(
  sessionId: string,
  chunkIndex: number,
  chunkBlob: Blob,
  attempt = 0
): Promise<ChunkUploadResponse> {
  const token = localStorage.getItem('token');

  try {
    // Create form data for multipart upload
    const formData = new FormData();
    formData.append('uploadSessionId', sessionId);
    formData.append('chunkIndex', chunkIndex.toString());
    formData.append('chunkData', chunkBlob, `chunk_${chunkIndex}.bin`);

    const response = await fetch(`${API_BASE_URL}/api/documents/upload/chunk`, {
      method: 'POST',
      headers: {
        ...(token && { Authorization: `Bearer ${token}` }),
      },
      body: formData,
    });

    if (!response.ok) {
      if (response.status === 404) {
        throw new Error('Upload session not found or expired');
      }
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }

      // Retry on 5xx errors or network issues
      if (response.status >= 500 && attempt < MAX_RETRY_ATTEMPTS) {
        const delay = RETRY_DELAYS[attempt];
        await new Promise(resolve => setTimeout(resolve, delay));
        return uploadChunk(sessionId, chunkIndex, chunkBlob, attempt + 1);
      }

      throw new Error(`Failed to upload chunk ${chunkIndex}: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    // Retry on network errors
    if (!(error instanceof Error && error.message.includes('Upload session')) &&
        attempt < MAX_RETRY_ATTEMPTS) {
      const delay = RETRY_DELAYS[attempt];
      await new Promise(resolve => setTimeout(resolve, delay));
      return uploadChunk(sessionId, chunkIndex, chunkBlob, attempt + 1);
    }

    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Finalize chunked upload and create document record (AC3).
 * @param sessionId - Upload session identifier
 * @returns Document metadata
 */
export interface FinalizeUploadResponse {
  documentId: string;
  fileName: string;
  fileSize: number;
  status: string;
  uploadedAt: string;
  patientId: string;
}

export async function finalizeUpload(sessionId: string): Promise<FinalizeUploadResponse> {
  const token = localStorage.getItem('token');

  try {
    const response = await fetch(`${API_BASE_URL}/api/documents/upload/finalize`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }),
      },
      body: JSON.stringify({ uploadSessionId: sessionId }),
    });

    if (!response.ok) {
      if (response.status === 400) {
        const error = await response.json();
        throw new Error(error.message || 'Upload incomplete');
      }
      if (response.status === 404) {
        throw new Error('Upload session not found or expired');
      }
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      throw new Error(`Failed to finalize upload: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Calculate number of chunks for a file.
 * @param fileSize - File size in bytes
 * @param chunkSize - Chunk size in bytes (default 1MB)
 * @returns Number of chunks
 */
export function calculateChunkCount(fileSize: number, chunkSize = 1048576): number {
  return Math.ceil(fileSize / chunkSize);
}

/**
 * Document status information for tracking processing state (US_044).
 */
export interface DocumentStatus {
  id: string;
  fileName: string;
  uploadedAt: string;
  fileSize: number;
  fileSizeFormatted: string;
  status: 'Uploaded' | 'Processing' | 'Completed' | 'Failed';
  processingTimeMs: number | null;
  errorMessage: string | null;
  isStuckProcessing: boolean;
  processedAt: string | null;
}

/**
 * Fetch all documents for the authenticated user (US_044, AC1).
 * @returns List of documents with processing status
 */
export async function fetchDocuments(): Promise<DocumentStatus[]> {
  const token = localStorage.getItem('token');

  try {
    const response = await fetch(`${API_BASE_URL}/api/documents`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }),
      },
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      throw new Error(`Failed to fetch documents: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Network error. Please check your connection and try again.');
  }
}

/**
 * Retry processing for a failed document (US_044, Edge Case).
 * @param documentId - Document identifier
 * @returns Updated document status
 */
export async function retryDocumentProcessing(documentId: string): Promise<DocumentStatus> {
  const token = localStorage.getItem('token');

  try {
    const response = await fetch(`${API_BASE_URL}/api/documents/${documentId}/retry`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }),
      },
    });

    if (!response.ok) {
      if (response.status === 400) {
        const error = await response.json();
        throw new Error(error.error || 'Cannot retry document. Only Failed documents can be retried.');
      }
      if (response.status === 403) {
        throw new Error('You do not have permission to retry this document.');
      }
      if (response.status === 404) {
        throw new Error('Document not found.');
      }
      if (response.status === 401) {
        throw new Error('Unauthorized. Please log in again.');
      }
      throw new Error(`Failed to retry document: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Network error. Please check your connection and try again.');
  }
}


/**
 * Slice file into chunk blob.
 * @param file - File object
 * @param chunkIndex - Zero-based chunk index
 * @param chunkSize - Chunk size in bytes (default 1MB)
 * @returns Chunk blob
 */
export function getChunkBlob(file: File, chunkIndex: number, chunkSize = 1048576): Blob {
  const start = chunkIndex * chunkSize;
  const end = Math.min(start + chunkSize, file.size);
  return file.slice(start, end);
}
