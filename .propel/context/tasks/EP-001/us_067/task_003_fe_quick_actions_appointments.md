# Task - task_003_fe_quick_actions_appointments

## Task ID

* ID: task_003_fe_quick_actions_appointments

## Task Title

* Implement Quick Action Cards and Appointments List (Frontend)

## Parent User Story

* US_067 - Patient Dashboard - Post-Login Landing Page

## Description

Create three quick action cards (Book Appointment, Complete Intake, Upload Documents) with navigation functionality and display upcoming appointments list showing next 5 appointments chronologically with provider details, date/time, specialty, and status badges. Implement visual feedback, skeleton loading states, and empty state handling.

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-003-patient-dashboard.html |
| **Screen Spec** | figma_spec.md#SCR-003 |
| **UXR Requirements** | UXR-001, UXR-002, UXR-401, UXR-502, UXR-605 |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing, designsystem.md#elevation |

## Technology Layer

* Frontend (React 18.x + TypeScript 5.x + Redux Toolkit 2.x + Tailwind CSS)

## Acceptance Criteria

1. **Given** I am viewing the dashboard, **When** the page loads, **Then** the system displays three quick action cards: "Book Appointment" (navigates to SCR-006), "Complete Intake" (navigates to SCR-012), and "Upload Documents" (navigates to SCR-014) per UXR-001.

2. **Given** I click any of the quick action cards, **When** the action executes, **Then** the system provides visual feedback within 200ms (button state change) and navigates to the target screen per UXR-501.

3. **Given** I have upcoming appointments, **When** the dashboard renders, **Then** the system displays my next 5 upcoming appointments in chronological order showing provider name, date/time, specialty, and appointment status badge (Confirmed/Waitlist/Pending) per UXR-401.

4. **Given** the appointments list is loading, **When** API response time exceeds 300ms, **Then** the system displays skeleton loading states for the appointment list per UXR-502.

5. **Given** I am a new patient with no appointments, **When** the dashboard loads, **Then** the system displays empty state illustration with guiding CTA: "No appointments yet. Browse providers to schedule your first visit" per UXR-605.

6. **Given** I hover over a quick action card, **When** mouse enters, **Then** the system applies hover state with elevated shadow (level-2) and border color change within 100ms (transition-fast).

7. **Given** the appointments list is displayed, **When** each appointment renders, **Then** the system shows status badges with distinct colors: Confirmed (success-light), Waitlist (warning-light), Pending (info-light) per UXR-401.

8. **Given** I use keyboard navigation, **When** I tab through quick actions, **Then** the system provides visible focus indicators with 2px outline per WCAG 2.2 AA.

## Implementation Checklist

- [ ] Create QuickActionCard component at `src/components/dashboard/QuickActionCard.tsx` with props for label, icon, and target route
- [ ] Implement QuickActions container component displaying 3 action cards in responsive grid (3 columns desktop, 1 column mobile)
- [ ] Add AppointmentCard component at `src/components/dashboard/AppointmentCard.tsx` displaying provider, date/time, specialty, and status badge
- [ ] Create appointments API slice at `src/store/api/appointmentsApi.ts` with endpoint for GET `/api/appointments/upcoming?limit=5`
- [ ] Implement UpcomingAppointments container component fetching data via RTK Query and displaying list with skeleton loading state
- [ ] Add StatusBadge component with variant prop (confirmed, waitlist, pending) applying appropriate semantic colors
- [ ] Implement empty state component with illustration placeholder and "Browse Providers" CTA button linking to `/providers`
- [ ] Add hover and focus states to action cards with transition-fast (100ms) and shadow elevation changes

## Estimated Effort

* 6 hours

## Dependencies

- task_001_fe_dashboard_page_layout.md - Dashboard page structure
- task_006_be_dashboard_api.md - Backend appointments API endpoint

## Technical Context

### Architecture Patterns

* **Pattern**: Presentational Component Pattern with Container Components
* **State Management**: Redux Toolkit Query for appointments data fetching
* **Navigation**: React Router useNavigate hook for programmatic routing
* **Loading States**: Skeleton UI pattern with content placeholders

### Related Requirements

* FR-002: Secure session management
* UC-003: Book Appointment - quick action navigation
* UXR-001: Max 3 clicks to any feature from dashboard
* UXR-002: Visual hierarchy distinguishing primary vs secondary actions
* UXR-401: Design system token consistency
* UXR-502: Skeleton loading states
* UXR-605: Empty state illustrations with guiding CTA
* NFR-001: API response time within 500ms

### Implementation References

**QuickActionCard Component:**
```typescript
// src/components/dashboard/QuickActionCard.tsx
import { useNavigate } from 'react-router-dom';

interface QuickActionCardProps {
  label: string;
  icon: React.ReactNode;
  targetRoute: string;
  primary?: boolean;
}

export const QuickActionCard: React.FC<QuickActionCardProps> = ({ 
  label, 
  icon, 
  targetRoute, 
  primary = false 
}) => {
  const navigate = useNavigate();

  return (
    <button
      onClick={() => navigate(targetRoute)}
      className={`
        flex flex-col items-center gap-2 p-4 
        bg-neutral-0 border border-neutral-200 rounded-md
        shadow-1 hover:shadow-2 hover:border-primary-300 hover:bg-primary-50
        transition-fast cursor-pointer
        focus:outline-2 focus:outline-primary-500 focus:outline-offset-2
        ${primary ? 'border-primary-200 bg-primary-50' : ''}
      `}
      aria-label={label}
    >
      <div className="w-10 h-10 rounded-md bg-primary-50 text-primary-500 flex items-center justify-center">
        {icon}
      </div>
      <span className="text-body-sm font-medium text-neutral-700">{label}</span>
    </button>
  );
};
```

**AppointmentCard Component:**
```typescript
// src/components/dashboard/AppointmentCard.tsx
import { format } from 'date-fns';
import { StatusBadge } from './StatusBadge';

interface AppointmentCardProps {
  appointment: {
    id: string;
    providerName: string;
    specialty: string;
    dateTime: string;
    status: 'Confirmed' | 'Waitlist' | 'Pending';
  };
}

export const AppointmentCard: React.FC<AppointmentCardProps> = ({ appointment }) => (
  <div className="p-4 bg-neutral-0 border border-neutral-200 rounded-md hover:bg-neutral-50 transition-fast">
    <div className="flex justify-between items-start mb-2">
      <div>
        <p className="text-body font-medium text-neutral-900">{appointment.providerName}</p>
        <p className="text-body-sm text-neutral-500">{appointment.specialty}</p>
      </div>
      <StatusBadge status={appointment.status} />
    </div>
    <p className="text-body-sm text-neutral-600">
      {format(new Date(appointment.dateTime), 'MMM d, yyyy • h:mm a')}
    </p>
  </div>
);
```

**StatusBadge Component:**
```typescript
// src/components/dashboard/StatusBadge.tsx
interface StatusBadgeProps {
  status: 'Confirmed' | 'Waitlist' | 'Pending';
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
  const variants = {
    Confirmed: 'bg-success-light text-success-dark',
    Waitlist: 'bg-warning-light text-warning-dark',
    Pending: 'bg-info-light text-info'
  };

  return (
    <span className={`
      inline-flex items-center px-2 py-0.5 rounded-full 
      text-caption font-medium
      ${variants[status]}
    `}>
      {status}
    </span>
  );
};
```

**UpcomingAppointments Container:**
```typescript
// src/components/dashboard/UpcomingAppointments.tsx
import { useGetUpcomingAppointmentsQuery } from '../../store/api/appointmentsApi';
import { AppointmentCard } from './AppointmentCard';
import { AppointmentCardSkeleton } from './AppointmentCardSkeleton';
import { EmptyAppointments } from './EmptyAppointments';

export const UpcomingAppointments: React.FC = () => {
  const { data: appointments, isLoading, error } = useGetUpcomingAppointmentsQuery({ limit: 5 });

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(3)].map((_, i) => <AppointmentCardSkeleton key={i} />)}
      </div>
    );
  }

  if (error || !appointments?.length) {
    return <EmptyAppointments />;
  }

  return (
    <div className="space-y-3">
      {appointments.map((apt) => (
        <AppointmentCard key={apt.id} appointment={apt} />
      ))}
    </div>
  );
};
```

**Empty State Component:**
```typescript
// src/components/dashboard/EmptyAppointments.tsx
import { useNavigate } from 'react-router-dom';

export const EmptyAppointments: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="text-center py-8 px-4 bg-neutral-50 border border-neutral-200 rounded-md">
      <div className="w-16 h-16 mx-auto mb-4 bg-primary-50 rounded-full flex items-center justify-center">
        <span className="text-2xl" aria-hidden="true">📅</span>
      </div>
      <p className="text-body text-neutral-700 mb-4">
        No appointments yet. Browse providers to schedule your first visit.
      </p>
      <button
        onClick={() => navigate('/providers')}
        className="px-5 py-2 bg-primary-500 text-neutral-0 rounded-md font-medium hover:bg-primary-600 transition-fast"
      >
        Browse Providers
      </button>
    </div>
  );
};
```

**Appointments API Slice:**
```typescript
// src/store/api/appointmentsApi.ts
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export const appointmentsApi = createApi({
  reducerPath: 'appointmentsApi',
  baseQuery: fetchBaseQuery({ 
    baseUrl: '/api',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) headers.set('authorization', `Bearer ${token}`);
      return headers;
    },
  }),
  endpoints: (builder) => ({
    getUpcomingAppointments: builder.query({
      query: ({ limit = 5 }) => `appointments/upcoming?limit=${limit}`,
    }),
  }),
});

export const { useGetUpcomingAppointmentsQuery } = appointmentsApi;
```

### Documentation References

* **React Router Navigation**: https://reactrouter.com/en/main/hooks/use-navigate
* **date-fns Formatting**: https://date-fns.org/v2.30.0/docs/format
* **Tailwind CSS Hover States**: https://tailwindcss.com/docs/hover-focus-and-other-states
* **Redux Toolkit Query**: https://redux-toolkit.js.org/rtk-query/usage/queries

### Edge Cases

* **What happens when appointment datetime is in the past?** Filter out past appointments on frontend; log warning if backend returns expired appointments.
* **How does the system handle appointments with missing provider data?** Display "Provider information unavailable" with neutral styling; log data integrity issue.
* **What happens if user clicks quick action before navigation setup completes?** Disable action cards until routing is ready; show visual disabled state with cursor-not-allowed.
* **How does the empty state behave when user has waitlist entries but no confirmed appointments?** Display modified message: "No confirmed appointments yet. You have N waitlist entries pending."

## Traceability

### Parent Epic

* EP-001

### Requirement Tags

* UC-003, UXR-001, UXR-002, UXR-401, UXR-502, UXR-605, NFR-001

### Related Tasks

* task_001_fe_dashboard_page_layout.md - Dashboard page structure
* task_006_be_dashboard_api.md - Backend appointments API endpoint

## Story Points

* 3

## Status

* not-started
