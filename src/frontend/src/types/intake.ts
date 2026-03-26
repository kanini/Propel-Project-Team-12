/**
 * TypeScript types for AI conversational intake (US_033)
 * Defines interfaces for intake sessions, messages, and extracted data
 */

/**
 * Chat message sender type
 */
export type MessageSender = 'ai' | 'user';

/**
 * Intake mode - AI conversational or Manual form
 */
export type IntakeMode = 'ai' | 'manual';

/**
 * Intake session status
 */
export type IntakeSessionStatus = 'idle' | 'active' | 'completed' | 'error';

/**
 * Chat message in the conversational intake
 */
export interface ChatMessage {
  id: string;
  sender: MessageSender;
  content: string;
  timestamp: string;
  extractedData?: ExtractedDataItem[];
}

/**
 * Individual extracted data item from AI
 */
export interface ExtractedDataItem {
  field: string;
  value: string;
  confidence: number;
}

/**
 * Complete extracted intake data from AI conversation
 */
export interface ExtractedIntakeData {
  chiefComplaint?: string;
  symptoms?: string[];
  medications?: MedicationInfo[];
  allergies?: AllergyInfo[];
  medicalHistory?: MedicalHistoryItem[];
  familyHistory?: string[];
  lifestyle?: LifestyleInfo;
  insuranceInfo?: InsuranceInfo;
  additionalConcerns?: string;
}

/**
 * Medication information
 */
export interface MedicationInfo {
  name: string;
  dosage?: string;
  frequency?: string;
}

/**
 * Allergy information
 */
export interface AllergyInfo {
  allergen: string;
  reaction?: string;
  severity?: 'mild' | 'moderate' | 'severe';
}

/**
 * Medical history item
 */
export interface MedicalHistoryItem {
  condition: string;
  diagnosedYear?: number;
  status: 'active' | 'resolved' | 'managed';
}

/**
 * Lifestyle information
 */
export interface LifestyleInfo {
  smokingStatus?: 'never' | 'former' | 'current';
  alcoholUse?: 'none' | 'occasional' | 'moderate' | 'heavy';
  exerciseFrequency?: string;
}

/**
 * Insurance information
 */
export interface InsuranceInfo {
  providerName?: string;
  memberId?: string;
  groupNumber?: string;
}

/**
 * Intake session data
 */
export interface IntakeSession {
  sessionId: string;
  appointmentId: string;
  patientId: string;
  mode: IntakeMode;
  status: IntakeSessionStatus;
  progress: number; // 0-100
  confidenceLevel: number; // 0-100
  createdAt: string;
  updatedAt?: string;
}

/**
 * Intake summary data for review
 */
export interface IntakeSummaryData {
  chiefComplaint: string;
  symptoms: string[];
  medications: MedicationInfo[];
  allergies: AllergyInfo[];
  medicalHistory: MedicalHistoryItem[];
  familyHistory: string[];
  lifestyle: LifestyleInfo;
  insuranceInfo?: InsuranceInfo;
  additionalConcerns?: string;
}

/**
 * Intake category for progress tracking
 */
export type IntakeCategory =
  | 'chiefComplaint'
  | 'symptoms'
  | 'medications'
  | 'allergies'
  | 'medicalHistory'
  | 'familyHistory'
  | 'lifestyle'
  | 'insurance';

/**
 * Category progress tracking
 */
export interface CategoryProgress {
  category: IntakeCategory;
  completed: boolean;
  label: string;
}

/**
 * Redux state for intake feature
 */
export interface IntakeState {
  // Session info
  sessionId: string | null;
  appointmentId: string | null;
  mode: IntakeMode;
  status: 'idle' | 'loading' | 'error' | 'complete';
  
  // Chat state
  messages: ChatMessage[];
  isTyping: boolean;
  
  // Extracted data
  extractedData: ExtractedIntakeData;
  
  // Progress tracking
  progress: number;
  confidenceLevel: number;
  consecutiveFailures: number;
  categoryProgress: CategoryProgress[];
  
  // Error handling
  error: string | null;
}

// API Request/Response types

/**
 * Request to start an intake session
 */
export interface StartIntakeRequest {
  appointmentId: string;
  mode: IntakeMode;
}

/**
 * Response from starting an intake session
 */
export interface StartIntakeResponse {
  sessionId: string;
  welcomeMessage: string;
  status: IntakeSessionStatus;
}

/**
 * Request to send a message in intake chat
 */
export interface SendMessageRequest {
  sessionId: string;
  message: string;
}

/**
 * Response from sending a message
 */
export interface SendMessageResponse {
  aiMessage: string;
  extractedData?: ExtractedDataItem[];
  progress: number;
  confidenceLevel: number;
  currentCategory: IntakeCategory;
  isComplete: boolean;
}

/**
 * Request to update intake data
 */
export interface UpdateIntakeRequest {
  sessionId: string;
  data: Partial<ExtractedIntakeData>;
}

/**
 * Request to complete intake
 */
export interface CompleteIntakeRequest {
  sessionId: string;
  summary: IntakeSummaryData;
}

/**
 * Response from completing intake
 */
export interface CompleteIntakeResponse {
  success: boolean;
  intakeRecordId: string;
  message: string;
}

// Manual form types for US_034

/**
 * Manual intake form data structure
 */
export interface ManualIntakeFormData {
  demographics: DemographicsData;
  medicalHistory: ManualMedicalHistoryData;
  insurance: ManualInsuranceData;
  visitConcerns: VisitConcernsData;
}

/**
 * Demographics section data
 */
export interface DemographicsData {
  preferredName?: string;
  dateOfBirth: string;
  phone: string;
  emergencyContact?: string;
  emergencyPhone?: string;
}

/**
 * Manual medical history form data
 */
export interface ManualMedicalHistoryData {
  currentConditions: string;
  medications: string;
  allergies: string;
  surgicalHistory: string;
  familyHistory: FamilyHistoryCheckboxes;
  lifestyle: LifestyleRadios;
}

/**
 * Family history checkboxes
 */
export interface FamilyHistoryCheckboxes {
  heartDisease: boolean;
  diabetes: boolean;
  cancer: boolean;
  hypertension: boolean;
  stroke: boolean;
  mentalHealth: boolean;
  other: string;
}

/**
 * Lifestyle radio options
 */
export interface LifestyleRadios {
  smoking: 'never' | 'former' | 'current';
  alcohol: 'none' | 'occasional' | 'moderate' | 'heavy';
}

/**
 * Manual insurance form data
 */
export interface ManualInsuranceData {
  hasInsurance: boolean;
  providerName?: string;
  memberId?: string;
  groupNumber?: string;
}

/**
 * Visit concerns form data
 */
export interface VisitConcernsData {
  chiefComplaint: string;
  symptomDuration?: string;
  symptomSeverity?: 'mild' | 'moderate' | 'severe';
  additionalConcerns?: string;
}

/**
 * Form validation errors mapping
 */
export type FormValidationErrors = Partial<Record<string, string>>;
