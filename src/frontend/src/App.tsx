import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { RegistrationPage } from './pages/Registration/RegistrationPage';
import { LoginPage } from './pages/Login/LoginPage';
import { RoleGuard } from './components/guards/RoleGuard';
import { Sidebar } from './components/layout/Sidebar';
import { PatientDashboardPage } from './pages/PatientDashboard/PatientDashboardPage';
import { StaffDashboardPage } from './pages/StaffDashboard/StaffDashboardPage';
import { AdminDashboardPage } from './pages/AdminDashboard/AdminDashboardPage';
import UserManagementPage from './pages/AdminPortal/UserManagementPage';
import { SessionTimeoutModal } from './components/modals/SessionTimeoutModal';
import { useSessionTimeout } from './hooks/useSessionTimeout';
import { setupAxiosInterceptors } from './utils/axiosInterceptor';

/**
 * Main App component with routing
 * FR-001: User Registration routing added
 * FR-002: User Authentication routing added
 * NFR-006: Role-Based Access Control (RBAC) implemented
 * UXR-604: Session timeout warning modal integrated
 * US_021: User Management page added
 */
function App() {
  // Setup axios interceptors on app mount
  useEffect(() => {
    setupAxiosInterceptors();
  }, []);

  return (
    <BrowserRouter>
      <AppContent />
    </BrowserRouter>
  );
}

/**
 * App content component - must be inside BrowserRouter to use React Router hooks
 */
function AppContent() {
  // Session timeout monitoring (UXR-604) - must be inside Router context
  const { showWarning, countdownTime, dismissWarning } = useSessionTimeout();

  return (
    <>
      {/* Session timeout warning modal (UXR-604) */}
      <SessionTimeoutModal 
        isOpen={showWarning} 
        countdownMs={countdownTime} 
        onExtend={dismissWarning} 
      />
      
      <Routes>
        {/* Public Routes */}
        <Route path="/" element={<HomePage />} />
        <Route path="/register" element={<RegistrationPage />} />
        <Route path="/login" element={<LoginPage />} />

        {/* Protected Patient Routes */}
        <Route
          path="/patient/*"
          element={
            <RoleGuard allowedRoles={['Patient']}>
              <DashboardLayout>
                <Routes>
                  <Route path="dashboard" element={<PatientDashboardPage />} />
                  <Route path="appointments" element={<PlaceholderPage title="My Appointments" />} />
                  <Route path="providers" element={<PlaceholderPage title="Find Providers" />} />
                  <Route path="health" element={<PlaceholderPage title="Health Dashboard" />} />
                  <Route path="documents" element={<PlaceholderPage title="Documents" />} />
                  <Route path="intake" element={<PlaceholderPage title="Intake" />} />
                  <Route path="*" element={<Navigate to="/patient/dashboard" replace />} />
                </Routes>
              </DashboardLayout>
            </RoleGuard>
          }
        />

        {/* Protected Staff Routes */}
        <Route
          path="/staff/*"
          element={
            <RoleGuard allowedRoles={['Staff']}>
              <DashboardLayout>
                <Routes>
                  <Route path="dashboard" element={<StaffDashboardPage />} />
                  <Route path="queue" element={<PlaceholderPage title="Patient Queue" />} />
                  <Route path="walk-in" element={<PlaceholderPage title="Walk-in Booking" />} />
                  <Route path="patients" element={<PlaceholderPage title="Patients" />} />
                  <Route path="verification" element={<PlaceholderPage title="Verification" />} />
                  <Route path="appointments" element={<PlaceholderPage title="Appointments" />} />
                  <Route path="*" element={<Navigate to="/staff/dashboard" replace />} />
                </Routes>
              </DashboardLayout>
            </RoleGuard>
          }
        />

        {/* Protected Admin Routes */}
        <Route
          path="/admin/*"
          element={
            <RoleGuard allowedRoles={['Admin']}>
              <DashboardLayout>
                <Routes>
                  <Route path="dashboard" element={<AdminDashboardPage />} />
                  <Route path="users" element={<UserManagementPage />} />
                  <Route path="audit" element={<PlaceholderPage title="Audit Logs" />} />
                  <Route path="settings" element={<PlaceholderPage title="Settings" />} />
                  <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
                </Routes>
              </DashboardLayout>
            </RoleGuard>
          }
        />

        {/* Catch-all route - redirect to home */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </>
  );
}

/**
 * Dashboard Layout with Sidebar Navigation
 */
function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen flex bg-gray-100">
      <Sidebar />
      <main className="flex-1 overflow-auto">
        {children}
      </main>
    </div>
  );
}

/**
 * Placeholder Page Component for routes under construction
 */
function PlaceholderPage({ title }: { title: string }) {
  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-semibold text-gray-900 mb-4">{title}</h1>
        <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
          <p className="text-gray-600">This page is under construction.</p>
        </div>
      </div>
    </div>
  );
}

/**
 * Home page landing
 */
function HomePage() {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="flex items-center justify-center gap-2 mb-8">
          <div className="w-8 h-8 bg-blue-500 rounded-lg flex items-center justify-center text-white text-xl font-bold">
            +
          </div>
          <span className="text-xl font-semibold text-gray-900">PatientAccess</span>
        </div>

        {/* Main Card */}
        <div className="bg-white border border-gray-200 rounded-2xl shadow-sm p-8">
          <h1 className="text-2xl font-semibold text-gray-900 mb-2">
            Welcome to PatientAccess
          </h1>
          <p className="text-sm text-gray-600 mb-6">
            Manage your healthcare appointments and access clinical intelligence
          </p>
          
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
            <h2 className="text-sm font-semibold text-blue-900 mb-2">
              Get Started
            </h2>
            <ul className="text-sm text-blue-800 space-y-1">
              <li>• Create an account to book appointments</li>
              <li>• Access your medical records</li>
              <li>• View health information and insights</li>
            </ul>
          </div>

          <div className="space-y-3">
            <a
              href="/register"
              className="w-full h-11 px-4 bg-blue-500 text-white font-medium text-sm rounded-lg hover:bg-blue-600 transition-colors flex items-center justify-center"
            >
              Create account
            </a>
            <a
              href="/login"
              className="w-full h-11 px-4 bg-white border border-gray-300 text-gray-700 font-medium text-sm rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors flex items-center justify-center"
            >
              Sign in
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;


