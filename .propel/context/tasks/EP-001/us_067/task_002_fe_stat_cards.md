# Task - task_002_fe_stat_cards

## Task ID

* ID: task_002_fe_stat_cards

## Task Title

* Implement Dashboard Statistics Cards (Frontend)

## Parent User Story

* US_067 - Patient Dashboard - Post-Login Landing Page

## Description

Create three interactive statistics cards displaying total appointments (past 6 months), upcoming appointments (next 30 days), and waitlist entries (current count) with trend indicators. Cards should be responsive, accessible, and display skeleton loading states when data is being fetched.

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-003-patient-dashboard.html |
| **Screen Spec** | figma_spec.md#SCR-003 |
| **UXR Requirements** | UXR-401, UXR-502 |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing, designsystem.md#elevation |

## Technology Layer

* Frontend (React 18.x + TypeScript 5.x + Redux Toolkit 2.x + Tailwind CSS)

## Acceptance Criteria

1. **Given** I am viewing the Patient Dashboard, **When** the page renders, **Then** the system displays three statistical cards showing: total appointments (past 6 months), upcoming appointments (next 30 days), and waitlist entries (current count) per UXR-502.

2. **Given** the stat cards are displayed, **When** trend data is available, **Then** the system shows appropriate trend indicators (up/down arrow with percentage) using success color (green) for positive trends and neutral for others.

3. **Given** the dashboard is loading data, **When** API response time exceeds 300ms, **Then** the system displays skeleton loading states for stat cards per UXR-502.

4. **Given** I am viewing stats on mobile viewport (320px-767px), **When** the layout adapts, **Then** the system stacks stat cards vertically in single column per UXR-303.

5. **Given** the stat cards are rendered, **When** displayed, **Then** the system applies design tokens consistently (card background: neutral-0, border: neutral-200, shadow: level-1) per UXR-401.

6. **Given** stat data fetch fails, **When** error occurs, **Then** the system displays "Unable to load statistics" message in each card with retry button.

7. **Given** I use keyboard navigation, **When** I tab through the dashboard, **Then** the system skips stat cards (non-interactive) and maintains focus on actionable elements only.

## Implementation Checklist

- [ ] Create StatCard component at `src/components/dashboard/StatCard.tsx` with props for label, value, trend data, and loading state
- [ ] Implement StatCardSkeleton component displaying shimmer animation for loading state using CSS keyframes
- [ ] Add DashboardStats container component fetching statistics data via Redux RTK Query hook
- [ ] Create stats API slice at `src/store/api/statsApi.ts` with endpoint for GET `/api/dashboard/stats`
- [ ] Implement responsive grid layout (3 columns desktop, 2 columns tablet, 1 column mobile) using Tailwind CSS classes
- [ ] Add trend indicator component with conditional rendering (positive: green arrow up, negative: red arrow down, neutral: no arrow)
- [ ] Apply elevation shadow-1 (0 1px 3px rgba(0,0,0,0.08)) and border-radius-md (8px) from design tokens
- [ ] Implement error state with retry functionality calling refetch() from RTK Query

## Estimated Effort

* 5 hours

## Dependencies

- task_001_fe_dashboard_page_layout.md - Page layout and main dashboard structure
- task_006_be_dashboard_api.md - Backend API endpoint providing statistics data

## Technical Context

### Architecture Patterns

* **Pattern**: Presentational Component Pattern (StatCard) + Container Component (DashboardStats)
* **State Management**: Redux Toolkit Query for API data fetching and caching
* **Loading States**: Skeleton UI pattern with shimmer animation
* **Error Handling**: Optimistic UI with error boundary and retry logic

### Related Requirements

* UXR-401: Design system token consistency
* UXR-502: Skeleton loading states for data-fetching
* UXR-303: Multi-column to single-column on mobile
* NFR-001: API response time within 500ms at 95th percentile

### Implementation References

**StatCard Component:**
```typescript
// src/components/dashboard/StatCard.tsx
interface StatCardProps {
  label: string;
  value: number;
  trend?: {
    value: number; // percentage change
    direction: 'up' | 'down' | 'neutral';
  };
  loading?: boolean;
  error?: string;
}

export const StatCard: React.FC<StatCardProps> = ({ label, value, trend, loading, error }) => {
  if (loading) {
    return <StatCardSkeleton />;
  }

  if (error) {
    return (
      <div className="bg-neutral-0 border border-neutral-200 rounded-md p-5 shadow-1">
        <p className="text-error text-body-sm">{error}</p>
        <button className="text-primary-500 text-body-sm mt-2">Retry</button>
      </div>
    );
  }

  return (
    <div className="bg-neutral-0 border border-neutral-200 rounded-md p-5 shadow-1">
      <p className="text-overline text-neutral-500 uppercase tracking-wide mb-1">{label}</p>
      <p className="text-h2 text-neutral-900 font-semibold">{value}</p>
      {trend && (
        <div className={`flex items-center gap-1 mt-1 text-caption ${
          trend.direction === 'up' ? 'text-success' : 
          trend.direction === 'down' ? 'text-error' : 
          'text-neutral-500'
        }`}>
          {trend.direction !== 'neutral' && (
            <span aria-hidden="true">{trend.direction === 'up' ? '↑' : '↓'}</span>
          )}
          <span>{Math.abs(trend.value)}% {trend.direction === 'up' ? 'increase' : 'from last period'}</span>
        </div>
      )}
    </div>
  );
};
```

**Skeleton Loading Component:**
```typescript
// src/components/dashboard/StatCardSkeleton.tsx
export const StatCardSkeleton: React.FC = () => (
  <div className="bg-neutral-0 border border-neutral-200 rounded-md p-5 shadow-1 animate-pulse">
    <div className="h-3 w-24 bg-neutral-200 rounded mb-2"></div>
    <div className="h-8 w-16 bg-neutral-200 rounded mb-1"></div>
    <div className="h-3 w-20 bg-neutral-200 rounded"></div>
  </div>
);
```

**Redux RTK Query Stats API:**
```typescript
// src/store/api/statsApi.ts
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

interface DashboardStats {
  totalAppointments: number;
  upcomingAppointments: number;
  waitlistEntries: number;
  trends: {
    totalAppointmentsTrend: number;
    upcomingAppointmentsTrend: number;
    waitlistEntriesTrend: number;
  };
}

export const statsApi = createApi({
  reducerPath: 'statsApi',
  baseQuery: fetchBaseQuery({ 
    baseUrl: '/api',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  endpoints: (builder) => ({
    getDashboardStats: builder.query<DashboardStats, void>({
      query: () => 'dashboard/stats',
      keepUnusedDataFor: 300, // 5 minutes cache
    }),
  }),
});

export const { useGetDashboardStatsQuery } = statsApi;
```

**DashboardStats Container:**
```typescript
// src/components/dashboard/DashboardStats.tsx
import { useGetDashboardStatsQuery } from '../../store/api/statsApi';
import { StatCard } from './StatCard';

export const DashboardStats: React.FC = () => {
  const { data, isLoading, error, refetch } = useGetDashboardStatsQuery();

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
      <StatCard 
        label="Total Appointments"
        value={data?.totalAppointments ?? 0}
        trend={data?.trends ? {
          value: data.trends.totalAppointmentsTrend,
          direction: data.trends.totalAppointmentsTrend > 0 ? 'up' : 
                    data.trends.totalAppointmentsTrend < 0 ? 'down' : 'neutral'
        } : undefined}
        loading={isLoading}
        error={error ? 'Unable to load statistics' : undefined}
      />
      <StatCard 
        label="Upcoming Appointments"
        value={data?.upcomingAppointments ?? 0}
        trend={data?.trends ? {
          value: data.trends.upcomingAppointmentsTrend,
          direction: data.trends.upcomingAppointmentsTrend > 0 ? 'up' : 
                    data.trends.upcomingAppointmentsTrend < 0 ? 'down' : 'neutral'
        } : undefined}
        loading={isLoading}
        error={error ? 'Unable to load statistics' : undefined}
      />
      <StatCard 
        label="Waitlist Entries"
        value={data?.waitlistEntries ?? 0}
        loading={isLoading}
        error={error ? 'Unable to load statistics' : undefined}
      />
    </div>
  );
};
```

### Documentation References

* **Redux Toolkit Query**: https://redux-toolkit.js.org/rtk-query/overview
* **React Skeleton Loading Pattern**: https://www.smashingmagazine.com/2020/04/skeleton-screens-react/
* **Tailwind CSS Grid**: https://tailwindcss.com/docs/grid-template-columns
* **Tailwind CSS Animations**: https://tailwindcss.com/docs/animation

### Edge Cases

* **What happens when stat values are very large (>999)?** Format numbers using Intl.NumberFormat with locale-aware thousands separators (e.g., 1,234).
* **How does the system handle negative trend values?** Display absolute value with "decrease" label in error color (red arrow down).
* **What happens if only some stats load successfully?** Display loaded stats normally and show error state only for failed cards with independent retry buttons.
* **How does skeleton animation perform on low-end devices?** Use prefers-reduced-motion media query to disable shimmer animation and show static placeholder instead.

## Traceability

### Parent Epic

* EP-001

### Requirement Tags

* UXR-401, UXR-502, UXR-303, NFR-001

### Related Tasks

* task_001_fe_dashboard_page_layout.md - Dashboard page structure
* task_006_be_dashboard_api.md - Backend stats API endpoint

## Story Points

* 2

## Status

* not-started
