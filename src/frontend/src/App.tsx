import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { useDispatch } from "react-redux";
import { useEffect } from "react";
import RegisterPage from "./features/auth/pages/RegisterPage";
import LoginPage from "./features/auth/pages/LoginPage";
import ForgotPasswordPage from "./features/auth/pages/ForgotPasswordPage";
import ResetPasswordPage from "./features/auth/pages/ResetPasswordPage";
import VerifyEmailPage from "./features/auth/pages/VerifyEmailPage";
import UserManagementPage from "./features/admin/pages/UserManagementPage";
import { SystemSettingsPage } from "./features/admin/pages/SystemSettingsPage"; // US_037 - Admin settings
import ProviderBrowser from "./pages/ProviderBrowser";
import AppointmentBooking from "./pages/AppointmentBooking";
import { PatientDashboard } from "./pages/PatientDashboard";
import MyAppointments from "./pages/MyAppointments";
import { WalkinBooking } from "./features/staff/pages/WalkinBooking";
import { StaffDashboard } from "./features/staff/dashboard/StaffDashboard";
import { QueueManagement } from "./pages/staff/QueueManagement";
import { ArrivalManagement } from "./pages/staff/ArrivalManagement";
import { DocumentUploadPage } from "./pages/DocumentUploadPage";
import DocumentStatusPage from "./pages/DocumentStatusPage";
import AppointmentSelectionPage from "./features/intake/pages/AppointmentSelectionPage";
import IntakePage from "./features/intake/pages/IntakePage";
import AuditLogsPage from "./pages/AuditLogsPage";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { MainLayout } from "./components/layout/MainLayout";
import { SessionTimeoutModal } from "./components/modals/SessionTimeoutModal";
import { useSessionTimeout } from "./hooks/useSessionTimeout";
import {
  logout,
  refreshSession,
  restoreSession,
  completeInitialization,
} from "./features/auth/authSlice";
import type { AppDispatch } from "./store";

function App() {
  const dispatch = useDispatch<AppDispatch>();

  // Restore authentication state from localStorage on app load
  useEffect(() => {
    const token = localStorage.getItem("token");
    const userId = localStorage.getItem("userId");

    if (token && userId) {
      // Parse user data from token or localStorage
      // For now, we'll get basic user info from localStorage
      const userEmail = localStorage.getItem("userEmail");
      const userName = localStorage.getItem("userName");
      const userRole = localStorage.getItem("userRole");

      if (userEmail && userName && userRole) {
        dispatch(
          restoreSession({
            token,
            user: {
              userId,
              email: userEmail,
              name: userName,
              role: userRole,
            },
          }),
        );
      } else {
        // Incomplete data - mark initialization complete without restoring
        dispatch(completeInitialization());
      }
    } else {
      // No session to restore - mark initialization complete
      dispatch(completeInitialization());
    }
  }, [dispatch]);

  // Session timeout hook (US_022, AC4, AC5)
  const { showWarning, secondsRemaining, extendSession } = useSessionTimeout();

  // Handle extend session (US_022, AC5)
  const handleExtendSession = () => {
    dispatch(refreshSession());
    extendSession();
  };

  // Handle logout (US_022, AC5)
  const handleLogout = () => {
    dispatch(logout());
  };

  return (
    <>
      <Router>
        <Routes>
          {/* Public Authentication Routes */}
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          <Route path="/verify-email" element={<VerifyEmailPage />} />

          {/* Patient Dashboard Routes - US_067, AC1 */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute allowedRoles={["Patient"]}>
                <MainLayout>
                  <PatientDashboard />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/appointments"
            element={
              <ProtectedRoute allowedRoles={["Patient"]}>
                <MainLayout>
                  <MyAppointments />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/intake"
            element={
              <ProtectedRoute allowedRoles={["Patient"]}>
                <MainLayout>
                  <AppointmentSelectionPage />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Intake Form Route - US_033/US_034/US_035 */}
          <Route
            path="/intake/:appointmentId"
            element={
              <ProtectedRoute allowedRoles={["Patient"]}>
                <MainLayout>
                  <IntakePage />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/documents"
            element={
              <ProtectedRoute allowedRoles={["Patient"]}>
                <MainLayout>
                  <DocumentUploadPage />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Document Status Route - US_044 */}
          <Route
            path="/documents/status"
            element={
              <ProtectedRoute allowedRoles={["Patient"]}>
                <MainLayout>
                  <DocumentStatusPage />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Provider Browser Route - US_023 */}
          <Route
            path="/providers"
            element={
              <ProtectedRoute allowedRoles={["Patient"]}>
                <MainLayout>
                  <ProviderBrowser />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Appointment Booking Route - US_024 */}
          <Route
            path="/appointments/book/:providerId"
            element={
              <ProtectedRoute allowedRoles={["Patient"]}>
                <MainLayout>
                  <AppointmentBooking />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Staff Dashboard Routes - US_068 */}
          <Route
            path="/staff/dashboard"
            element={
              <ProtectedRoute allowedRoles={["Staff", "Admin"]}>
                <MainLayout>
                  <StaffDashboard />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Queue Management Route - US_030 */}
          <Route
            path="/staff/queue"
            element={
              <ProtectedRoute allowedRoles={["Staff", "Admin"]}>
                <MainLayout>
                  <QueueManagement />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Walk-in Booking Route - US_029 */}
          <Route
            path="/staff/walk-in"
            element={
              <ProtectedRoute allowedRoles={["Staff", "Admin"]}>
                <MainLayout>
                  <WalkinBooking />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Arrival Management Route - US_031 */}
          <Route
            path="/staff/arrivals"
            element={
              <ProtectedRoute allowedRoles={["Staff", "Admin"]}>
                <MainLayout>
                  <ArrivalManagement />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/staff/verification"
            element={
              <ProtectedRoute allowedRoles={["Staff", "Admin"]}>
                <MainLayout>
                  <div className="text-center">
                    <h1 className="text-2xl font-bold mb-2">Verification</h1>
                    <p className="text-neutral-500">Coming soon</p>
                  </div>
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Admin Dashboard Routes - US_020, AC2 (Admin-only) */}
          <Route
            path="/admin/dashboard"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <MainLayout>
                  <div className="text-center">
                    <h1 className="text-2xl font-bold mb-2">Admin Dashboard</h1>
                    <p className="text-neutral-500">Coming soon</p>
                  </div>
                </MainLayout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/users"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <MainLayout>
                  <UserManagementPage />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/audit"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <MainLayout>
                  <AuditLogsPage />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/settings"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <MainLayout>
                  <SystemSettingsPage />
                </MainLayout>
              </ProtectedRoute>
            }
          />

          {/* Default redirect to login */}
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </Router>

      {/* Session Timeout Warning Modal (US_022, UXR-604) */}
      <SessionTimeoutModal
        isOpen={showWarning}
        secondsRemaining={secondsRemaining}
        onExtendSession={handleExtendSession}
        onLogout={handleLogout}
      />
    </>
  );
}

export default App;
