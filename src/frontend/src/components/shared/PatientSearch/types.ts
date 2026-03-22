/**
 * PatientSearch Component Types for US_032
 * Reusable patient search interfaces and enums
 */

/**
 * Patient search result interface
 */
export interface PatientSearchResult {
  id: string;
  fullName: string;
  dateOfBirth: string; // YYYY-MM-DD format
  email?: string | null;
  phone?: string | null;
  lastAppointmentDate?: string | null; // YYYY-MM-DD format
}

/**
 * Search context - determines navigation behavior
 */
export const SearchContext = {
  WalkinBooking: "walkin-booking",
  ArrivalManagement: "arrival-management",
  QueueManagement: "queue-management",
  Dashboard: "dashboard",
} as const;

export type SearchContext = (typeof SearchContext)[keyof typeof SearchContext];

/**
 * Props for PatientSearch component
 */
export interface PatientSearchProps {
  /**
   * Callback when patient is selected from results
   */
  onSelectPatient: (patient: PatientSearchResult) => void;

  /**
   * Whether to show "Create New Patient" button in empty state
   */
  showCreateButton?: boolean;

  /**
   * Placeholder text for search input
   */
  placeholder?: string;

  /**
   * Search context for analytics/behavior customization
   */
  context?: SearchContext;

  /**
   * Optional callback for "Create Patient" button
   */
  onCreatePatient?: (query: string) => void;

  /**
   * Whether to clear input after selection
   */
  clearOnSelect?: boolean;

  /**
   * Optional CSS class for container
   */
  className?: string;
}

/**
 * Props for PatientSearchResult row component
 */
export interface PatientSearchResultProps {
  patient: PatientSearchResult;
  isSelected: boolean;
  onSelect: () => void;
}
