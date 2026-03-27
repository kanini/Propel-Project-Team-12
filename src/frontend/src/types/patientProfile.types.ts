/**
 * Patient Profile 360° type definitions (US_049).
 * Matches backend DTOs from PatientProfile360Dto.cs
 */

export type VerificationBadge = "AISuggested" | "StaffVerified";

export interface Demographics {
  patientId: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string | null;
  gender: string | null;
  phoneNumber: string | null;
  email: string;
  emergencyContact: string | null;
}

export interface ConditionItem {
  id: string;
  conditionName: string;
  icd10Code: string | null;
  status: string; // "Active", "Resolved", "Historical"
  diagnosisDate: string | null;
  severity: string | null;
  badge: VerificationBadge;
  sourceDocumentIds: string[];
}

export interface Conditions {
  activeConditions: ConditionItem[];
  verifiedCount: number;
  totalCount: number;
}

export interface MedicationItem {
  id: string;
  drugName: string;
  dosage: string;
  frequency: string;
  routeOfAdministration: string | null;
  startDate: string | null;
  endDate: string | null;
  status: string; // "Active", "Discontinued", "Historical"
  hasConflict: boolean;
  badge: VerificationBadge;
  sourceDocumentIds: string[];
}

export interface Medications {
  activeMedications: MedicationItem[];
  verifiedCount: number;
  totalCount: number;
}

export interface AllergyItem {
  id: string;
  allergenName: string;
  reaction: string | null;
  severity: string; // "Critical", "Severe", "Moderate", "Mild"
  onsetDate: string | null;
  status: string; // "Active", "Resolved"
  badge: VerificationBadge;
  sourceDocumentIds: string[];
}

export interface Allergies {
  activeAllergies: AllergyItem[];
  verifiedCount: number;
  totalCount: number;
}

export interface VitalDataPoint {
  recordedAt: string;
  value: string;
  unit: string;
  sourceDocumentId: string;
}

export interface VitalTrends {
  bloodPressure: VitalDataPoint[];
  heartRate: VitalDataPoint[];
  temperature: VitalDataPoint[];
  weight: VitalDataPoint[];
  rangeStart: string; // ISO date string
  rangeEnd: string; // ISO date string
}

export interface EncounterItem {
  id: string;
  encounterDate: string;
  encounterType: string;
  provider: string | null;
  facility: string | null;
  chiefComplaint: string | null;
  sourceDocumentIds: string[];
}

export interface Encounters {
  recentEncounters: EncounterItem[];
  totalCount: number;
}

/**
 * Complete 360° Patient Profile response (FR-032, AIR-007).
 */
export interface PatientProfile360 {
  patientId: string;
  demographics: Demographics;
  conditions: Conditions;
  medications: Medications;
  allergies: Allergies;
  vitalTrends: VitalTrends;
  encounters: Encounters;
  profileCompleteness: number; // 0-100 percentage
  lastAggregatedAt: string;
  hasUnresolvedConflicts: boolean;
  totalDocumentsProcessed: number;
}
