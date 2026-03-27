/**
 * Medical Codes API Client (EP-008-US-052)
 * Handles communication with medical code verification endpoints
 */

import type { 
  MedicalCodeSuggestion,
  ModifyCodeRequest,
  RejectCodeRequest  
} from '../types/medicalCode.types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Get authentication token from localStorage
 */
function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
  };
}

export const medicalCodesApi = {
  /**
   * Get AI-suggested codes for a patient's extracted clinical data
   * @param extractedDataId - ID of extracted clinical data
   * @returns Promise<MedicalCodeSuggestion[]>
   */
  getSuggestionsForPatient: async (
    extractedDataId: string
  ): Promise<MedicalCodeSuggestion[]> => {
    const response = await fetch(
      `${API_BASE_URL}/api/medical-codes/${extractedDataId}/suggestions`,
      {
        headers: getAuthHeaders(),
      }
    );
    
    if (!response.ok) {
      throw new Error('Failed to fetch suggestions');
    }
    
    return response.json();
  },

  /**
   * Accept an AI-suggested code (AC2)
   * @param codeId - Medical code ID
   */
  acceptCode: async (codeId: string): Promise<void> => {
    const response = await fetch(
      `${API_BASE_URL}/api/medical-codes/${codeId}/verify`,
      {
        method: 'PATCH',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          verificationStatus: 'StaffVerified',
        }),
      }
    );
    
    if (!response.ok) {
      throw new Error('Failed to accept code');
    }
  },

  /**
   * Modify an AI-suggested code with alternative (AC3)
   * @param request - Modification request with new code and rationale
   */
  modifyCode: async (request: ModifyCodeRequest): Promise<void> => {
    const response = await fetch(
      `${API_BASE_URL}/api/medical-codes/${request.codeId}/modify`,
      {
        method: 'PATCH',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          newCode: request.newCode,
          newDescription: request.newDescription,
          rationale: request.rationale,
        }),
      }
    );
    
    if (!response.ok) {
      throw new Error('Failed to modify code');
    }
  },

  /**
   * Reject an AI-suggested code (AC4)
   * @param request - Rejection request with reason
   */
  rejectCode: async (request: RejectCodeRequest): Promise<void> => {
    const response = await fetch(
      `${API_BASE_URL}/api/medical-codes/${request.codeId}/verify`,
      {
        method: 'PATCH',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          verificationStatus: 'StaffRejected',
          notes: request.reason,
        }),
      }
    );
    
    if (!response.ok) {
      throw new Error('Failed to reject code');
    }
  },

  /**
   * Bulk accept high-confidence codes (Edge Case 2)
   * @param codeIds - Array of code IDs to accept
   */
  bulkAcceptCodes: async (codeIds: string[]): Promise<void> => {
    await Promise.all(codeIds.map((id) => medicalCodesApi.acceptCode(id)));
  },
};
