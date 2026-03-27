/**
 * Conflict types and enums for US_048 (Conflict Alerts UI)
 */

export const enum ConflictSeverity {
  Critical = "Critical",
  Warning = "Warning",
  Info = "Info",
}

export const enum ResolutionStatus {
  Unresolved = "Unresolved",
  Resolved = "Resolved",
  Dismissed = "Dismissed",
}

export interface DataConflict {
  id: string;
  patientProfileId: number;
  conflictType: string;
  entityType: string;
  entityId: string;
  description: string;
  severity: ConflictSeverity;
  sourceDataIds: string[];
  resolutionStatus: ResolutionStatus;
  resolvedBy?: string;
  resolvedAt?: string;
  createdAt: string;
}

export interface ConflictSummary {
  totalUnresolved: number;
  criticalCount: number;
  warningCount: number;
  infoCount: number;
  oldestConflictDate?: string;
}

export interface ResolveConflictRequest {
  resolution: string;
  chosenEntityId?: string;
}
