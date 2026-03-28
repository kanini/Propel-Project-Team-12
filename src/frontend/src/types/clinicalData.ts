/**
 * TypeScript type definitions for Health Dashboard 360° (SCR-016)
 * and Clinical Verification (SCR-023).
 * Matches backend DTOs for type safety.
 */

export interface HealthDashboard360Dto {
  demographics: PatientDemographicsDto;
  conditions: ClinicalItemDto[];
  medications: ClinicalItemDto[];
  allergies: ClinicalItemDto[];
  vitals: ClinicalItemDto[];
  labResults: ClinicalItemDto[];
  encounters: EncounterDto[];
  medicalCodes: MedicalCodeDto[];
  stats: DashboardStatsOverviewDto;
}

export interface PatientDemographicsDto {
  name: string;
  dateOfBirth?: string;
  mrn?: string;
  bloodType?: string;
  phone?: string;
  email?: string;
  address?: string;
  insurance?: string;
}

export interface ClinicalItemDto {
  extractedDataId: string;
  dataType: string;
  dataKey: string;
  dataValue: string;
  confidenceScore: number;
  verificationStatus: VerificationStatus;
  source?: string;
  onset?: string;
  sourcePageNumber?: number;
  sourceTextExcerpt?: string;
  structuredData?: Record<string, unknown>;
  medicalCodes: MedicalCodeDto[];
}

export interface MedicalCodeDto {
  medicalCodeId?: string;
  codeSystem: 'ICD10' | 'CPT';
  codeValue: string;
  codeDescription: string;
  confidenceScore: number;
  verificationStatus: VerificationStatus;
  sourceDataSummary?: string;
  verifiedBy?: string;
  verifiedAt?: string;
}

export interface EncounterDto {
  date: string;
  provider: string;
  type: string;
  notes?: string;
}

export interface DashboardStatsOverviewDto {
  totalExtractedItems: number;
  verifiedItems: number;
  pendingItems: number;
  totalDocuments: number;
  totalMedicalCodes: number;
}

export type VerificationStatus = 'AISuggested' | 'Verified' | 'Rejected' | 'Conflict';

// Verification Queue (SCR-023A)

export interface VerificationQueueItemDto {
  patientId: string;
  patientName: string;
  pendingClinicalDataCount: number;
  pendingMedicalCodesCount: number;
  conflictCount: number;
  priority: 'High' | 'Medium' | 'Low';
  lastUploadDate?: string;
}

export interface VerificationQueueResponseDto {
  items: VerificationQueueItemDto[];
  totalCount: number;
}

// Clinical Verification Dashboard (SCR-023)

export interface ClinicalVerificationDashboardDto {
  pendingCount: number;
  verifiedCount: number;
  rejectedCount: number;
  conflictCount: number;
  clinicalData: VerificationItemDto[];
  medicalCodes: VerificationMedicalCodeDto[];
}

export interface VerificationItemDto {
  extractedDataId: string;
  dataType: string;
  dataValue: string;
  confidenceScore: number;
  verificationStatus: string;
  sourceDocument?: string;
  sourcePageNumber?: number;
  sourceTextExcerpt?: string;
}

export interface VerificationMedicalCodeDto {
  medicalCodeId: string;
  codeSystem: string;
  codeValue: string;
  codeDescription: string;
  confidenceScore: number;
  verificationStatus: string;
  sourceDataSummary?: string;
  verifiedByName?: string;
  verifiedAt?: string;
  rejectionReason?: string;
}

export interface VerifyActionDto {
  id: string;
}

export interface RejectActionDto {
  id: string;
  reason?: string;
}

export interface ModifyCodeDto {
  medicalCodeId: string;
  codeValue: string;
  codeDescription: string;
}
