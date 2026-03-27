/**
 * Medical Codes API Client
 * Handles API communication for medical code verification workflows
 */

import axios from 'axios';
import type {
  MedicalCodeSuggestion,
  ModifyCodeRequest,
  RejectCodeRequest,
  AcceptCodeRequest,
  CodeSearchResult,
} from '../types/medicalCode.types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
const API_BASE = `${API_BASE_URL}/api/medical-codes`;
const KNOWLEDGE_BASE_API = `${API_BASE_URL}/api/knowledge`;

/**
 * Get authorization headers with JWT token
 */
function getAuthHeaders() {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` }),
  };
}

/**
 * Medical Codes API endpoints
 */
export const medicalCodesApi = {
  /**
   * Get all code suggestions for a specific extracted clinical data entry
   */
  getSuggestionsForPatient: async (
    extractedDataId: string
  ): Promise<MedicalCodeSuggestion[]> => {
    const response = await axios.get<MedicalCodeSuggestion[]>(
      `${API_BASE}/${extractedDataId}/suggestions`,
      { headers: getAuthHeaders() }
    );
    return response.data;
  },

  /**
   * Get the top suggestion for a specific extracted clinical data entry
   */
  getTopSuggestion: async (
    extractedDataId: string,
    codeSystem: 'ICD10' | 'CPT'
  ): Promise<MedicalCodeSuggestion> => {
    const response = await axios.get<MedicalCodeSuggestion>(
      `${API_BASE}/${extractedDataId}/top-suggestion`,
      {
        params: { codeSystem },
        headers: getAuthHeaders(),
      }
    );
    return response.data;
  },

  /**
   * Accept a medical code suggestion (AC2)
   * Sets verification status to "StaffVerified"
   */
  acceptCode: async (request: AcceptCodeRequest): Promise<void> => {
    await axios.patch(
      `${API_BASE}/${request.codeId}/verify`,
      {
        verificationStatus: 'StaffVerified',
      },
      { headers: getAuthHeaders() }
    );
  },

  /**
   * Modify a medical code suggestion (AC3)
   * Allows staff to select alternative code with rationale
   */
  modifyCode: async (request: ModifyCodeRequest): Promise<void> => {
    await axios.patch(
      `${API_BASE}/${request.codeId}/modify`,
      {
        newCode: request.newCode,
        newDescription: request.newDescription,
        rationale: request.rationale,
      },
      { headers: getAuthHeaders() }
    );
  },

  /**
   * Reject a medical code suggestion (AC4)
   * Sets verification status to "StaffRejected" with reason
   */
  rejectCode: async (request: RejectCodeRequest): Promise<void> => {
    await axios.patch(
      `${API_BASE}/${request.codeId}/verify`,
      {
        verificationStatus: 'StaffRejected',
        notes: request.reason,
      },
      { headers: getAuthHeaders() }
    );
  },

  /**
   * Bulk accept high-confidence codes (Edge case: confidence >95%)
   */
  bulkAcceptCodes: async (codeIds: string[]): Promise<void> => {
    await Promise.all(
      codeIds.map((id) => medicalCodesApi.acceptCode({ codeId: id }))
    );
  },

  /**
   * Search for alternative medical codes (for Modify action)
   * Uses hybrid retrieval service from US_050
   */
  searchCodes: async (
    query: string,
    codeSystem: 'ICD10' | 'CPT',
    topK: number = 10
  ): Promise<CodeSearchResult[]> => {
    const response = await axios.get<{
      chunks: CodeSearchResult[];
    }>(`${KNOWLEDGE_BASE_API}/search`, {
      params: {
        query,
        codeSystem,
        topK,
      },
      headers: getAuthHeaders(),
    });
    return response.data.chunks;
  },
};
