import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import {
  fetchDashboardStats,
  fetchUpcomingAppointments,
  fetchRecentNotifications,
  fetchRecentDocuments,
} from '../api/dashboardApi';
import type {
  DashboardStatsDto,
  UpcomingAppointmentDto,
  NotificationDto,
  RecentDocumentDto,
} from '../types/dashboard';
import { RiskBadge } from '../components/common/RiskBadge';

/**
 * Patient Dashboard Page (US_067, SCR-003)
 * Post-login landing page displaying statistics, appointments, notifications, and documents.
 * 
 * This is a BASIC IMPLEMENTATION demonstrating the structure.
 * Complete implementation requires all components from tasks 001-005.
 */
export const PatientDashboard = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  // State management
  const [stats, setStats] = useState<DashboardStatsDto | null>(null);
  const [appointments, setAppointments] = useState<UpcomingAppointmentDto[]>([]);
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [documents, setDocuments] = useState<RecentDocumentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Fetch dashboard data on component mount
  useEffect(() => {
    const loadDashboardData = async () => {
      try {
        setIsLoading(true);
        setError(null);

        // Fetch all dashboard data in parallel
        const [statsData, appointmentsData, notificationsData, documentsData] = await Promise.all([
          fetchDashboardStats(),
          fetchUpcomingAppointments(5),
          fetchRecentNotifications(5),
          fetchRecentDocuments(3),
        ]);

        setStats(statsData);
        setAppointments(appointmentsData);
        setNotifications(notificationsData);
        setDocuments(documentsData);
      } catch (err) {
        console.error('Error loading dashboard data:', err);
        setError(err instanceof Error ? err.message : 'Failed to load dashboard data');
      } finally {
        setIsLoading(false);
      }
    };

    loadDashboardData();
  }, []);

  // Handle retry on error
  const handleRetry = () => {
    window.location.reload();
  };

  return (
    <div className="space-y-6">
      {/* Page Title Section */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-neutral-900">
            Welcome back, {user?.name || 'Patient'}
          </h1>
        </div>
        <button
          onClick={() => navigate('/providers')}
          className="px-5 py-2.5 bg-blue-600 text-white rounded-md font-medium hover:bg-blue-700 transition-colors"
        >
          Book appointment
        </button>
      </div>

      {/* Main Content */}
      {isLoading ? (
        <div className="space-y-6">
          <div className="text-body text-neutral-600">Loading dashboard...</div>
          {/* TODO: Implement DashboardSkeleton component */}
        </div>
      ) : error ? (
        <div className="p-4 bg-error-light border border-error rounded-md">
          <p className="text-body text-error font-medium mb-2">Error loading dashboard data</p>
          <p className="text-body-sm text-error mb-3">{error}</p>
          <button
            onClick={handleRetry}
            className="px-4 py-2 bg-error text-white rounded-md font-medium hover:bg-error-dark transition-colors"
          >
            Retry
          </button>
          {/* TODO: Implement ErrorBanner component */}
        </div>
      ) : (
        <div className="space-y-6">
          {/* Statistics Cards - Matches wireframe SCR-003 */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-white border border-neutral-200 rounded-md p-5 shadow-sm">
              <p className="text-xs text-neutral-500 uppercase tracking-wide mb-1">Upcoming Appointments</p>
              <p className="text-3xl text-neutral-900 font-semibold">{stats?.upcomingAppointments || 0}</p>
              <p className="text-xs text-neutral-500 mt-1">
                {stats?.upcomingAppointments && stats.upcomingAppointments > 0
                  ? `Next 30 days`
                  : 'No upcoming appointments'}
              </p>
            </div>
            <div className="bg-white border border-neutral-200 rounded-md p-5 shadow-sm">
              <p className="text-xs text-neutral-500 uppercase tracking-wide mb-1">Total Appointments</p>
              <p className="text-3xl text-neutral-900 font-semibold">{stats?.totalAppointments || 0}</p>
              <p className="text-xs text-neutral-500 mt-1">Past 6 months</p>
            </div>
            <div className="bg-white border border-neutral-200 rounded-md p-5 shadow-sm">
              <p className="text-xs text-neutral-500 uppercase tracking-wide mb-1">Documents</p>
              <p className="text-3xl text-neutral-900 font-semibold">{stats?.totalDocuments || 0}</p>
              <p className="text-xs text-success mt-1">✓ {stats?.completedDocuments || 0} processed</p>
            </div>
          </div>

          {/* Content Grid - Appointments and Notifications/Documents */}
          <div className="grid grid-cols-1 lg:grid-cols-[2fr_1fr] gap-6">
            {/* Left Column - Appointments and Quick Actions */}
            <div className="space-y-6">
              {/* Upcoming Appointments Table - Matches wireframe SCR-003 */}
              <div className="bg-white border border-neutral-200 rounded-md p-5">
                <div className="flex justify-between items-center mb-4">
                  <h2 className="text-lg font-semibold text-neutral-900">Upcoming Appointments</h2>
                  <button
                    onClick={() => navigate('/appointments')}
                    className="text-sm text-primary-500 hover:text-primary-700 font-medium"
                  >
                    View all
                  </button>
                </div>
                {appointments && appointments.length > 0 ? (
                  <div className="overflow-x-auto">
                    <table className="w-full border-collapse">
                      <thead>
                        <tr className="border-b border-neutral-200">
                          <th className="text-left text-xs font-semibold text-neutral-700 uppercase tracking-wide py-3 px-4">Date & Time</th>
                          <th className="text-left text-xs font-semibold text-neutral-700 uppercase tracking-wide py-3 px-4">Provider</th>
                          <th className="text-left text-xs font-semibold text-neutral-700 uppercase tracking-wide py-3 px-4">Service</th>
                          <th className="text-left text-xs font-semibold text-neutral-700 uppercase tracking-wide py-3 px-4">Status</th>
                          <th className="text-left text-xs font-semibold text-neutral-700 uppercase tracking-wide py-3 px-4">Action</th>
                        </tr>
                      </thead>
                      <tbody>
                        {appointments.map((apt: any) => (
                          <tr key={apt.appointmentId} className="border-b border-neutral-100 hover:bg-neutral-50">
                            <td className="py-3 px-4 text-sm text-neutral-900">
                              {new Date(apt.scheduledDateTime).toLocaleDateString('en-US', {
                                month: 'short',
                                day: 'numeric',
                                year: 'numeric',
                              })} · {new Date(apt.scheduledDateTime).toLocaleTimeString('en-US', {
                                hour: 'numeric',
                                minute: '2-digit',
                              })}
                            </td>
                            <td className="py-3 px-4 text-sm text-neutral-900">{apt.providerName}</td>
                            <td className="py-3 px-4 text-sm text-neutral-600">{apt.providerSpecialty}</td>
                            <td className="py-3 px-4">
                              <span className={`text-xs px-2 py-1 rounded-full font-medium ${apt.status === 'Confirmed' ? 'bg-success-light text-success' :
                                  apt.status === 'Scheduled' ? 'bg-info-light text-info' :
                                    'bg-warning-light text-warning'
                                }`}>
                                {apt.status}
                              </span>
                            </td>
                            <td className="py-3 px-4">
                              <button
                                onClick={() => navigate('/intake')}
                                className="text-sm text-primary-500 hover:text-primary-700 font-medium"
                              >
                                Complete intake
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <div className="text-center py-8">
                    <p className="text-neutral-600 mb-3">No appointments yet</p>
                    <button
                      onClick={() => navigate('/providers')}
                      className="text-primary-500 hover:text-primary-700 font-medium"
                    >
                      Browse providers to schedule your first visit →
                    </button>
                  </div>
                )}
              </div>

              {/* Quick Actions - Matches wireframe SCR-003 */}
              <div className="bg-white border border-neutral-200 rounded-md p-5">
                <h3 className="text-lg font-semibold text-neutral-900 mb-4">Quick Actions</h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                  <button
                    onClick={() => navigate('/providers')}
                    className="flex flex-col items-center gap-2 p-4 border border-neutral-200 rounded-md hover:border-primary-300 hover:bg-primary-50 transition-all"
                  >
                    <div className="w-10 h-10 rounded-md bg-primary-50 text-primary-500 flex items-center justify-center text-xl">
                      📅
                    </div>
                    <span className="text-sm font-medium text-neutral-700">Book Appointment</span>
                  </button>
                  <button
                    onClick={() => navigate('/documents')}
                    className="flex flex-col items-center gap-2 p-4 border border-neutral-200 rounded-md hover:border-primary-300 hover:bg-primary-50 transition-all"
                  >
                    <div className="w-10 h-10 rounded-md bg-primary-50 text-primary-500 flex items-center justify-center text-xl">
                      📄
                    </div>
                    <span className="text-sm font-medium text-neutral-700">Upload Documents</span>
                  </button>
                  <button
                    onClick={() => navigate('/intake')}
                    className="flex flex-col items-center gap-2 p-4 border border-neutral-200 rounded-md hover:border-primary-300 hover:bg-primary-50 transition-all"
                  >
                    <div className="w-10 h-10 rounded-md bg-primary-50 text-primary-500 flex items-center justify-center text-xl">
                      ❤️
                    </div>
                    <span className="text-sm font-medium text-neutral-700">Health Dashboard</span>
                  </button>
                </div>
              </div>
            </div>

            {/* Right column - Notifications and Documents */}
            <div className="space-y-6">
              {/* Notifications Panel - Enhanced with risk-based notifications (US_038) */}
              <div className="bg-white border border-neutral-200 rounded-md p-5">
                <h2 className="text-lg font-semibold text-neutral-900 mb-4">Notifications</h2>
                {notifications && notifications.length > 0 ? (
                  <div className="space-y-3">
                    {notifications.map((notif: NotificationDto) => {
                      // Determine notification styling based on type
                      const isHighRisk = notif.notificationType === 'high_risk_appointment';
                      const isWaitlist = notif.notificationType === 'waitlist_slot_available';
                      const isReminder = notif.notificationType === 'appointment_reminder';

                      let bgColor = 'bg-info-light';
                      let borderColor = 'border-info';
                      let textColor = 'text-info';

                      if (isHighRisk) {
                        bgColor = 'bg-error-light';
                        borderColor = 'border-error';
                        textColor = 'text-error-dark';
                      } else if (isWaitlist) {
                        bgColor = 'bg-warning-light';
                        borderColor = 'border-warning';
                        textColor = 'text-warning-dark';
                      } else if (isReminder) {
                        bgColor = 'bg-info-light';
                        borderColor = 'border-info';
                        textColor = 'text-info';
                      }

                      return (
                        <div
                          key={notif.notificationId}
                          className={`p-3 rounded-md ${bgColor} border-l-3 ${borderColor}`}
                        >
                          <div className="flex items-start justify-between gap-2">
                            <div className="flex-1">
                              <p className={`text-sm font-medium ${textColor}`}>{notif.title}</p>
                              <p className="text-xs text-neutral-600 mt-1">{notif.message}</p>
                            </div>
                            {/* Show risk badge for high-risk appointment notifications (US_038 AC-3) */}
                            {isHighRisk && notif.notificationType === 'high_risk_appointment' && (
                              <div className="flex-shrink-0">
                                <RiskBadge score={80} riskLevel="High" />
                              </div>
                            )}
                          </div>
                          {notif.actionLink && (
                            <button
                              onClick={() => navigate(notif.actionLink!)}
                              className={`mt-2 text-xs ${textColor} font-medium hover:underline`}
                            >
                              {notif.actionLabel || 'View Details'} →
                            </button>
                          )}
                        </div>
                      );
                    })}
                  </div>
                ) : (
                  <p className="text-sm text-neutral-600 text-center py-4">No new notifications</p>
                )}
              </div>

              {/* Recent Documents - Implement RecentDocuments component */}
              <div className="bg-white border border-neutral-200 rounded-md p-5">
                <h2 className="text-lg font-semibold text-neutral-900 mb-4">Recent Documents</h2>
                {documents && documents.length > 0 ? (
                  <div className="space-y-2">
                    {documents.map((doc: RecentDocumentDto) => (
                      <div key={doc.documentId} className="p-3 rounded-md border border-neutral-200">
                        <div className="flex justify-between items-start">
                          <p className="text-sm font-medium text-neutral-900 truncate">{doc.fileName}</p>
                          <span className={`text-xs px-2 py-1 rounded-full ml-2 ${doc.processingStatus === 'Completed' ? 'bg-success-light text-success' :
                              doc.processingStatus === 'Processing' ? 'bg-warning-light text-warning' :
                                'bg-error-light text-error'
                            }`}>
                            {doc.processingStatus}
                          </span>
                        </div>
                        <p className="text-xs text-neutral-600 mt-1">
                          {new Date(doc.uploadedAt).toLocaleDateString()}
                        </p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-neutral-600 text-center py-4">No documents uploaded</p>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

