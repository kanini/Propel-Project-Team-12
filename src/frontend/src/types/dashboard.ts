/**
 * TypeScript type definitions for US_067 - Patient Dashboard.
 * Matches backend DTOs for type safety.
 */

export interface DashboardStatsDto {
  totalAppointments: number;
  upcomingAppointments: number;
  waitlistEntries: number;
  totalDocuments: number;
  completedDocuments: number;
  trends: TrendsDto;
}

export interface TrendsDto {
  totalAppointmentsTrend: number;
  upcomingAppointmentsTrend: number;
  waitlistEntriesTrend: number;
}

export interface UpcomingAppointmentDto {
  appointmentId: string;
  providerName: string;
  providerSpecialty: string;
  scheduledDateTime: string; // ISO 8601 date string
  status: 'Scheduled' | 'Confirmed' | 'Pending' | 'Waitlist';
  confirmationNumber: string;
  visitReason: string;
}

export interface NotificationDto {
  notificationId: string;
  title: string;
  message: string;
  createdAt: string; // ISO 8601 date string
  actionLink?: string;
  actionLabel?: string;
  isRead: boolean;
  notificationType: string;
}

export interface UnreadCountDto {
  count: number;
}

export interface RecentDocumentDto {
  documentId: string;
  fileName: string;
  fileType: string;
  fileSize: number;
  processingStatus: 'Processing' | 'Completed' | 'Failed';
  uploadedAt: string; // ISO 8601 date string
  processedAt?: string;
  errorMessage?: string;
}
