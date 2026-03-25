/**
 * Staff Dashboard Page Component for US_068 - Staff Dashboard
 * Main dashboard orchestrator - fetches data and renders dashboard widgets
 */

import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import {
  loadDashboardData,
  selectDashboardMetrics,
  selectQueuePreview,
  selectDashboardLoading,
  selectDashboardError,
} from '../../../store/slices/staffDashboardSlice';
import { DashboardMetrics } from './DashboardMetrics';
import { QuickActions } from './QuickActions';
import { QueuePreview } from './QueuePreview';
import { PendingVerifications } from './PendingVerifications';
import { DashboardSkeleton } from './DashboardSkeleton';
import { EmptyDashboard } from './EmptyDashboard';

/**
 * Staff Dashboard page - Operations hub (US_068)
 */
export function StaffDashboard() {
  const dispatch = useAppDispatch();
  const metrics = useAppSelector(selectDashboardMetrics);
  const queuePreview = useAppSelector(selectQueuePreview);
  const isLoading = useAppSelector(selectDashboardLoading);
  const error = useAppSelector(selectDashboardError);

  useEffect(() => {
    // Load dashboard data on mount
    dispatch(loadDashboardData());

    // Set up polling interval (30 seconds)
    const interval = setInterval(() => {
      dispatch(loadDashboardData());
    }, 30000);

    return () => clearInterval(interval);
  }, [dispatch]);

  // Loading state
  if (isLoading && !metrics) {
    return (
      <>
        <header className="bg-neutral-0 border-b border-neutral-200 shadow-sm mb-8">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <h1 className="text-2xl font-bold text-neutral-900">
              Staff Dashboard
            </h1>
            <p className="mt-1 text-sm text-neutral-600">
              Operations hub for managing daily workflows
            </p>
          </div>
        </header>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <DashboardSkeleton />
        </div>
      </>
    );
  }

  // Error state
  if (error) {
    return (
      <>
        <header className="bg-neutral-0 border-b border-neutral-200 shadow-sm mb-8">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <h1 className="text-2xl font-bold text-neutral-900">
              Staff Dashboard
            </h1>
          </div>
        </header>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
            <p className="text-red-700 font-medium mb-2">Failed to load dashboard</p>
            <p className="text-red-600 text-sm mb-4">{error}</p>
            <button
              onClick={() => dispatch(loadDashboardData())}
              className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg text-sm font-medium"
            >
              Retry
            </button>
          </div>
        </div>
      </>
    );
  }

  // Empty state
  if (metrics && metrics.todayAppointments === 0 && queuePreview.length === 0) {
    return (
      <>
        <header className="bg-neutral-0 border-b border-neutral-200 shadow-sm mb-8">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <h1 className="text-2xl font-bold text-neutral-900">
              Staff Dashboard
            </h1>
            <p className="mt-1 text-sm text-neutral-600">
              Operations hub for managing daily workflows
            </p>
          </div>
        </header>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <EmptyDashboard />
        </div>
      </>
    );
  }

  // Success state with data
  return (
    <>
      <header className="bg-neutral-0 border-b border-neutral-200 shadow-sm mb-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <h1 className="text-2xl font-bold text-neutral-900">
            Staff Dashboard
          </h1>
          <p className="mt-1 text-sm text-neutral-600">
            Operations hub for managing daily workflows
          </p>
        </div>
      </header>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Stat Cards */}
        {metrics && <DashboardMetrics metrics={metrics} />}

        {/* Quick Actions */}
        <QuickActions />

        {/* Main Content Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Queue Preview - Takes 2 columns */}
          <div className="lg:col-span-2">
            <QueuePreview queue={queuePreview} />
          </div>

          {/* Pending Verifications - Takes 1 column */}
          <div>
            <PendingVerifications />
          </div>
        </div>
      </div>
    </>
  );
}
