/**
 * Medical Code Verification Types
 * Types for medical code suggestions, verification, and audit trail
 */

export type CodeSystem = 'ICD10' | 'CPT';
export type VerificationStatus = 'AISuggested' | 'Accepted' | 'Modified' | 'Rejected';
export type VerificationAction = 'Accepted' | 'Modified' | 'Rejected';

export interface MedicalCodeSuggestion {
  id: string;
  code: string; // "E11.9", "99213"
  description: string;
  codeSystem: CodeSystem;
  confidenceScore: number; // 0-100
  rationale: string;
  rank: number;
  isTopSuggestion: boolean;
  verificationStatus: VerificationStatus;
  verifiedBy?: string; // User ID (GUID)
  verifiedAt?: string; // ISO timestamp
  extractedClinicalDataId: string;
  sourceClinicalText: string; // For side panel display
  retrievedContext?: string; // RAG retrieval context for audit
}

export interface VerificationAuditEntry {
  userId: number;
  userName: string;
  action: VerificationAction;
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

export interface AcceptCodeRequest {
  codeId: string;
}

export interface CodeSearchResult {
  code: string;
  description: string;
  codeSystem: CodeSystem;
  similarityScore: number;
}

export interface CodeMappingResponse {
  suggestions: MedicalCodeSuggestion[];
  suggestionCount: number;
  message?: string;
  isAmbiguous: boolean;
}
