/**
 * Type definitions for Medical Code Verification (EP-008-US-052)
 * Used for ICD-10 and CPT code verification workflow
 */

export interface MedicalCodeSuggestion {
  id: string;
  code: string; // "E11.9", "99213"
  description: string;
  codeSystem: 'ICD10' | 'CPT';
  confidenceScore: number; // 0-100
  rationale: string;
  rank: number;
  isTopSuggestion: boolean;
  verificationStatus: 'Pending' | 'StaffVerified' | 'StaffRejected';
  verifiedBy?: number; // User ID
  verifiedAt?: string; // ISO timestamp
  extractedClinicalDataId: string;
  sourceClinicalText: string; // For side panel display
}

export interface VerificationAuditEntry {
  userId: number;
  userName: string;
  action: 'Accepted' | 'Modified' | 'Rejected';
  timestamp: string;
  previousCode?: string; // For modified codes
  reason?: string; // For rejected codes
}

export interface ModifyCodeRequest {
  codeId: string;
  newCode: string;
  newDescription: string;
  rationale: string;
}

export interface RejectCodeRequest {
  codeId: string;
  reason: string;
}

export interface CodeSearchResult {
  code: string;
  description: string;
  similarityScore: number;
  codeSystem: 'ICD10' | 'CPT';
}
