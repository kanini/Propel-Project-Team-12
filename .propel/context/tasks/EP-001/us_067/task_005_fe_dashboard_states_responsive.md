# Task - task_005_fe_dashboard_states_responsive

## Task ID

* ID: task_005_fe_dashboard_states_responsive

## Task Title

* Implement Dashboard Loading, Error, Empty States and Mobile Responsiveness (Frontend)

## Parent User Story

* US_067 - Patient Dashboard - Post-Login Landing Page

## Description

Implement comprehensive state management for the Patient Dashboard including skeleton loading states for all data sections, error handling with retry functionality, empty states for new patients, session timeout warning modal, and mobile responsive layout adaptations ensuring single-column stacking on mobile viewports.

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-003-patient-dashboard.html |
| **Screen Spec** | figma_spec.md#SCR-003 |
| **UXR Requirements** | UXR-301, UXR-303, UXR-502, UXR-603, UXR-604, UXR-605 |
| **Design Tokens** | designsystem.md#breakpoints, designsystem.md#motion, designsystem.md#spacing |

## Technology Layer

* Frontend (React 18.x + TypeScript 5.x + Redux Toolkit 2.x + Tailwind CSS)

## Acceptance Criteria

1. **Given** the dashboard is loading data, **When** API response time exceeds 300ms, **Then** the system displays skeleton loading states for stat cards, appointment list, and notification panel per UXR-502.

2. **Given** appointment data fetch fails, **When** error occurs, **Then** the system displays error banner with retry action per UXR-603 while showing empty placeholder sections.

3. **Given** I am a new patient with no data, **When** the dashboard loads, **Then** the system displays empty state illustrations with guiding CTAs per UXR-605: "No appointments yet. Browse providers to schedule your first visit."

4. **Given** I access the dashboard on mobile viewport (320px-767px), **When** the page renders, **Then** the system adapts layout to single column stacking stat cards and quick actions per UXR-301 and UXR-303.

5. **Given** I have been inactive for 13 minutes, **When** session timeout warning triggers, **Then** the system displays modal at 13-minute mark with "Extend Session" button per UXR-604.

6. **Given** the session timeout modal is displayed, **When** I click "Extend Session," **Then** the system refreshes the authentication token and dismisses the modal within 200ms.

7. **Given** the dashboard is in error state, **When** I click the "Retry" button, **Then** the system refetches all dashboard data and transitions to loading state.

8. **Given** I switch device orientation from portrait to landscape, **When** the viewport changes, **Then** the system seamlessly adapts the layout without content reflow or flicker.

## Implementation Checklist

- [ ] Create DashboardSkeleton component at `src/components/dashboard/DashboardSkeleton.tsx` displaying skeleton loaders for all dashboard sections
- [ ] Implement ErrorBanner component at `src/components/dashboard/ErrorBanner.tsx` with retry button triggering refetch across all data hooks
- [ ] Add SessionTimeoutModal component at `src/components/modals/SessionTimeoutModal.tsx` with countdown timer and "Extend Session" action
- [ ] Implement session timeout logic in AuthContext triggering modal at 13-minute mark (2 minutes before 15-minute timeout)
- [ ] Add mobile responsive styles using Tailwind breakpoints ensuring single-column layout on mobile (< 768px)
- [ ] Create EmptyDashboard component displaying welcome message and quick action CTAs for new patients with no data
- [ ] Implement error boundary wrapper for dashboard catching React errors and displaying fallback UI
- [ ] Add prefers-reduced-motion media query support disabling skeleton shimmer animations for accessibility

## Estimated Effort

* 6 hours

## Dependencies

- task_001_fe_dashboard_page_layout.md - Dashboard page structure
- task_002_fe_stat_cards.md - Stat card skeleton integration
- task_003_fe_quick_actions_appointments.md - Appointments list skeleton integration
- task_004_fe_notifications_documents.md - Notifications panel skeleton integration

## Technical Context

### Architecture Patterns

* **Pattern**: Compound Component Pattern for modal state management
* **State Management**: Redux Toolkit for session timeout tracking
* **Error Handling**: React Error Boundary with fallback UI
* **Responsive Design**: Mobile-first approach with Tailwind breakpoints

### Related Requirements

* FR-002: Secure session management with 15-minute timeout
* UXR-301: Responsive layout adaptation
* UXR-303: Multi-column to single-column on mobile
* UXR-502: Skeleton loading states
* UXR-603: Error banner with retry action
* UXR-604: Session timeout warning modal
* UXR-605: Empty state illustrations
* NFR-005: Automatic session timeout after 15 minutes of inactivity

### Implementation References

**DashboardSkeleton Component:**
```typescript
// src/components/dashboard/DashboardSkeleton.tsx
export const DashboardSkeleton: React.FC = () => (
  <div className="space-y-6 animate-pulse">
    {/* Stat Cards Skeleton */}
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {[...Array(3)].map((_, i) => (
        <div key={i} className="h-24 bg-neutral-200 rounded-md"></div>
      ))}
    </div>

    {/* Quick Actions Skeleton */}
    <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
      {[...Array(3)].map((_, i) => (
        <div key={i} className="h-20 bg-neutral-200 rounded-md"></div>
      ))}
    </div>

    {/* Content Grid Skeleton */}
    <div className="grid grid-cols-1 lg:grid-cols-[2fr_1fr] gap-6">
      <div className="space-y-3">
        {[...Array(3)].map((_, i) => (
          <div key={i} className="h-20 bg-neutral-200 rounded-md"></div>
        ))}
      </div>
      <div className="space-y-3">
        {[...Array(2)].map((_, i) => (
          <div key={i} className="h-16 bg-neutral-200 rounded-md"></div>
        ))}
      </div>
    </div>
  </div>
);
```

**ErrorBanner Component:**
```typescript
// src/components/dashboard/ErrorBanner.tsx
interface ErrorBannerProps {
  message: string;
  onRetry: () => void;
}

export const ErrorBanner: React.FC<ErrorBannerProps> = ({ message, onRetry }) => (
  <div className="mb-6 p-4 bg-error-light border border-error rounded-md flex items-center justify-between">
    <div className="flex items-center gap-3">
      <span className="text-error text-2xl" aria-hidden="true">⚠️</span>
      <div>
        <p className="text-body font-medium text-error-dark">Error Loading Dashboard</p>
        <p className="text-body-sm text-error">{message}</p>
      </div>
    </div>
    <button
      onClick={onRetry}
      className="px-4 py-2 bg-error text-neutral-0 rounded-md font-medium hover:bg-error-dark transition-fast"
      aria-label="Retry loading dashboard data"
    >
      Retry
    </button>
  </div>
);
```

**SessionTimeoutModal Component:**
```typescript
// src/components/modals/SessionTimeoutModal.tsx
import { useState, useEffect } from 'react';
import { useAuth } from '../../hooks/useAuth';

interface SessionTimeoutModalProps {
  isOpen: boolean;
  onExtend: () => void;
}

export const SessionTimeoutModal: React.FC<SessionTimeoutModalProps> = ({ isOpen, onExtend }) => {
  const [countdown, setCountdown] = useState(120); // 2 minutes = 120 seconds

  useEffect(() => {
    if (!isOpen) return;

    const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-neutral-900/50 flex items-center justify-center z-50">
      <div className="bg-neutral-0 rounded-lg p-6 max-w-md w-full shadow-3">
        <h2 className="text-h4 text-neutral-900 mb-2">Session Expiring Soon</h2>
        <p className="text-body text-neutral-600 mb-4">
          Your session will expire in <strong>{countdown} seconds</strong> due to inactivity. 
          Click "Extend Session" to continue.
        </p>
        <div className="flex gap-3">
          <button
            onClick={onExtend}
            className="flex-1 px-4 py-2 bg-primary-500 text-neutral-0 rounded-md font-medium hover:bg-primary-600 transition-fast"
          >
            Extend Session
          </button>
          <button
            onClick={() => window.location.href = '/logout'}
            className="px-4 py-2 bg-neutral-200 text-neutral-700 rounded-md font-medium hover:bg-neutral-300 transition-fast"
          >
            Logout
          </button>
        </div>
      </div>
    </div>
  );
};
```

**Session Timeout Logic in AuthContext:**
```typescript
// src/context/AuthContext.tsx
import { createContext, useState, useEffect } from 'react';

export const AuthProvider: React.FC = ({ children }) => {
  const [showTimeoutWarning, setShowTimeoutWarning] = useState(false);
  const [lastActivity, setLastActivity] = useState(Date.now());

  useEffect(() => {
    // Track user activity
    const updateActivity = () => setLastActivity(Date.now());
    
    window.addEventListener('mousedown', updateActivity);
    window.addEventListener('keydown', updateActivity);
    window.addEventListener('scroll', updateActivity);
    window.addEventListener('touchstart', updateActivity);

    const checkInactivity = setInterval(() => {
      const inactiveTime = Date.now() - lastActivity;
      const thirteenMinutes = 13 * 60 * 1000;
      const fifteenMinutes = 15 * 60 * 1000;

      if (inactiveTime >= thirteenMinutes && inactiveTime < fifteenMinutes) {
        setShowTimeoutWarning(true);
      }

      if (inactiveTime >= fifteenMinutes) {
        handleLogout();
      }
    }, 10000); // Check every 10 seconds

    return () => {
      window.removeEventListener('mousedown', updateActivity);
      window.removeEventListener('keydown', updateActivity);
      window.removeEventListener('scroll', updateActivity);
      window.removeEventListener('touchstart', updateActivity);
      clearInterval(checkInactivity);
    };
  }, [lastActivity]);

  const extendSession = async () => {
    // Refresh auth token
    await refreshAuthToken();
    setLastActivity(Date.now());
    setShowTimeoutWarning(false);
  };

  return (
    <AuthContext.Provider value={{ extendSession }}>
      {children}
      <SessionTimeoutModal 
        isOpen={showTimeoutWarning} 
        onExtend={extendSession} 
      />
    </AuthContext.Provider>
  );
};
```

**Responsive Mobile Layout:**
```typescript
// src/pages/PatientDashboard.tsx
export const PatientDashboard: React.FC = () => {
  const { data: stats, isLoading: statsLoading, error: statsError, refetch: refetchStats } = useGetDashboardStatsQuery();
  const { data: appointments, isLoading: apptLoading, error: apptError, refetch: refetchAppts } = useGetUpcomingAppointmentsQuery({ limit: 5 });

  const hasError = statsError || apptError;
  const isLoading = statsLoading || apptLoading;

  const handleRetry = () => {
    refetchStats();
    refetchAppts();
  };

  if (isLoading) {
    return (
      <div className="p-6 lg:p-8">
        <DashboardSkeleton />
      </div>
    );
  }

  return (
    <div className="p-4 md:p-6 lg:p-8">
      {hasError && (
        <ErrorBanner 
          message="Unable to load dashboard data. Please try again." 
          onRetry={handleRetry} 
        />
      )}

      {/* Single column on mobile, 3 columns on desktop */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
        {/* Stat cards */}
      </div>

      {/* Single column on mobile, 3 columns on desktop */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-3 mb-6">
        {/* Quick actions */}
      </div>

      {/* Single column on mobile, 2-column grid on desktop */}
      <div className="grid grid-cols-1 lg:grid-cols-[2fr_1fr] gap-6">
        {/* Appointments list */}
        {/* Notifications panel */}
      </div>
    </div>
  );
};
```

### Documentation References

* **React Error Boundaries**: https://react.dev/reference/react/Component#catching-rendering-errors-with-an-error-boundary
* **Tailwind Responsive Design**: https://tailwindcss.com/docs/responsive-design
* **Tailwind Animation**: https://tailwindcss.com/docs/animation
* **MDN prefers-reduced-motion**: https://developer.mozilla.org/en-US/docs/Web/CSS/@media/prefers-reduced-motion

### Edge Cases

* **What happens if user extends session multiple times within timeout window?** Reset last activity timestamp on each extend; allow unlimited extensions during active use.
* **How does skeleton loading behave with very slow API responses (>5s)?** Display skeleton for maximum 5 seconds, then show error state with retry option.
* **What happens during rapid viewport resize (e.g., browser DevTools)?** Debounce layout recalculations by 200ms to prevent excessive re-renders and UI flicker.
* **How does error boundary handle errors in specific dashboard sections?** Use granular error boundaries per section; one section failure doesn't crash entire dashboard.

## Traceability

### Parent Epic

* EP-001

### Requirement Tags

* FR-002, UXR-301, UXR-303, UXR-502, UXR-603, UXR-604, UXR-605, NFR-005

### Related Tasks

* task_001_fe_dashboard_page_layout.md - Dashboard page structure
* task_002_fe_stat_cards.md - Stat card skeleton states
* task_003_fe_quick_actions_appointments.md - Appointments list skeleton states
* task_004_fe_notifications_documents.md - Notifications panel skeleton states

## Story Points

* 3

## Status

* not-started
