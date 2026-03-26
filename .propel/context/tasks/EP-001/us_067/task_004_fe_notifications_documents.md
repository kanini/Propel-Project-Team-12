# Task - task_004_fe_notifications_documents

## Task ID

* ID: task_004_fe_notifications_documents

## Task Title

* Implement Notifications and Recent Documents Panels (Frontend)

## Parent User Story

* US_067 - Patient Dashboard - Post-Login Landing Page

## Description

Create a notification panel displaying the 5 most recent unread notifications with timestamps and action links, and a recent documents section showing the 3 most recent clinical documents with processing status badges. Implement skeleton loading states, empty states, and real-time notification badge updates in the header.

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-003-patient-dashboard.html |
| **Screen Spec** | figma_spec.md#SCR-003 |
| **UXR Requirements** | UXR-401, UXR-402, UXR-502, UXR-605 |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing, designsystem.md#elevation |

## Technology Layer

* Frontend (React 18.x + TypeScript 5.x + Redux Toolkit 2.x + Tailwind CSS)

## Acceptance Criteria

1. **Given** I have notifications (slot swap available, appointment reminders, document processing complete), **When** the dashboard loads, **Then** the system displays a notification panel showing the 5 most recent unread notifications with timestamps and action links.

2. **Given** I have recently uploaded clinical documents, **When** I view the dashboard, **Then** the system displays a "Recent Documents" section showing the 3 most recent documents with processing status badges (Processing/Completed/Failed) per UXR-402.

3. **Given** the notification panel is loading, **When** API response time exceeds 300ms, **Then** the system displays skeleton loading states for the notification list per UXR-502.

4. **Given** I have no notifications, **When** the panel renders, **Then** the system displays empty state message: "No new notifications" per UXR-605.

5. **Given** I have unread notifications, **When** the header notification bell renders, **Then** the system displays a red badge indicator with unread count (max display: 9+).

6. **Given** document processing status is "Processing," **When** the badge renders, **Then** the system displays amber badge with "Processing" label and optional spinner icon.

7. **Given** I click a notification action link (e.g., "View Appointment"), **When** the action executes, **Then** the system marks the notification as read and navigates to the target screen.

8. **Given** a new notification arrives via real-time update, **When** the notification payload is received, **Then** the system updates the notification list and badge count without page refresh.

## Implementation Checklist

- [ ] Create NotificationCard component at `src/components/dashboard/NotificationCard.tsx` displaying title, message, timestamp, and action link
- [ ] Implement NotificationsPanel container component fetching data via RTK Query from GET `/api/notifications/recent?limit=5`
- [ ] Add DocumentCard component at `src/components/dashboard/DocumentCard.tsx` displaying document name, upload date, and status badge
- [ ] Create documents API slice at `src/store/api/documentsApi.ts` with endpoint for GET `/api/documents/recent?limit=3`
- [ ] Implement RecentDocuments container component with skeleton loading and empty state handling
- [ ] Add notification badge to header notification bell button showing unread count with conditional rendering (hide if count is 0)
- [ ] Implement markNotificationAsRead mutation in notifications API slice (PATCH `/api/notifications/{id}/read`)
- [ ] Add real-time notification subscription using Pusher or WebSocket for live updates (optional enhancement)

## Estimated Effort

* 7 hours

## Dependencies

- task_001_fe_dashboard_page_layout.md - Header notification bell integration
- task_007_be_notifications_documents_api.md - Backend notifications and documents API endpoints

## Technical Context

### Architecture Patterns

* **Pattern**: Presentational Component Pattern with Container Components
* **State Management**: Redux Toolkit Query for data fetching and mutations
* **Real-time Updates**: Pusher Channels for notification push (optional)
* **Optimistic Updates**: RTK Query optimistic caching for mark-as-read

### Related Requirements

* UXR-401: Design system token consistency
* UXR-402: Document processing status badges
* UXR-502: Skeleton loading states
* UXR-605: Empty state illustrations
* NFR-001: API response time within 500ms

### Implementation References

**NotificationCard Component:**
```typescript
// src/components/dashboard/NotificationCard.tsx
import { formatDistanceToNow } from 'date-fns';
import { useMarkNotificationAsReadMutation } from '../../store/api/notificationsApi';
import { useNavigate } from 'react-router-dom';

interface NotificationCardProps {
  notification: {
    id: string;
    title: string;
    message: string;
    createdAt: string;
    actionLink?: string;
    actionLabel?: string;
    isRead: boolean;
  };
}

export const NotificationCard: React.FC<NotificationCardProps> = ({ notification }) => {
  const [markAsRead] = useMarkNotificationAsReadMutation();
  const navigate = useNavigate();

  const handleActionClick = async () => {
    if (!notification.isRead) {
      await markAsRead(notification.id);
    }
    if (notification.actionLink) {
      navigate(notification.actionLink);
    }
  };

  return (
    <div className={`
      p-3 rounded-md border transition-fast
      ${notification.isRead ? 'bg-neutral-50 border-neutral-200' : 'bg-primary-50 border-primary-200'}
    `}>
      <div className="flex justify-between items-start mb-1">
        <p className="text-body-sm font-medium text-neutral-900">{notification.title}</p>
        <span className="text-caption text-neutral-500">
          {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
        </span>
      </div>
      <p className="text-body-sm text-neutral-600 mb-2">{notification.message}</p>
      {notification.actionLink && (
        <button
          onClick={handleActionClick}
          className="text-body-sm text-primary-500 hover:text-primary-700 font-medium"
        >
          {notification.actionLabel || 'View Details'} →
        </button>
      )}
    </div>
  );
};
```

**NotificationsPanel Container:**
```typescript
// src/components/dashboard/NotificationsPanel.tsx
import { useGetRecentNotificationsQuery } from '../../store/api/notificationsApi';
import { NotificationCard } from './NotificationCard';
import { NotificationCardSkeleton } from './NotificationCardSkeleton';

export const NotificationsPanel: React.FC = () => {
  const { data: notifications, isLoading } = useGetRecentNotificationsQuery({ limit: 5 });

  if (isLoading) {
    return (
      <div className="space-y-2">
        {[...Array(3)].map((_, i) => <NotificationCardSkeleton key={i} />)}
      </div>
    );
  }

  if (!notifications?.length) {
    return (
      <div className="text-center py-6 text-body-sm text-neutral-500">
        No new notifications
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {notifications.map((notif) => (
        <NotificationCard key={notif.id} notification={notif} />
      ))}
    </div>
  );
};
```

**DocumentCard Component:**
```typescript
// src/components/dashboard/DocumentCard.tsx
import { format } from 'date-fns';

interface DocumentCardProps {
  document: {
    id: string;
    fileName: string;
    uploadedAt: string;
    processingStatus: 'Processing' | 'Completed' | 'Failed';
  };
}

export const DocumentCard: React.FC<DocumentCardProps> = ({ document }) => {
  const statusVariants = {
    Processing: 'bg-warning-light text-warning-dark',
    Completed: 'bg-success-light text-success-dark',
    Failed: 'bg-error-light text-error-dark'
  };

  return (
    <div className="p-3 bg-neutral-0 border border-neutral-200 rounded-md hover:bg-neutral-50 transition-fast">
      <div className="flex justify-between items-start mb-1">
        <p className="text-body-sm font-medium text-neutral-900 truncate">{document.fileName}</p>
        <span className={`
          inline-flex items-center px-2 py-0.5 rounded-full text-caption font-medium
          ${statusVariants[document.processingStatus]}
        `}>
          {document.processingStatus === 'Processing' && (
            <span className="mr-1 animate-spin" aria-hidden="true">⟳</span>
          )}
          {document.processingStatus}
        </span>
      </div>
      <p className="text-caption text-neutral-500">
        Uploaded {format(new Date(document.uploadedAt), 'MMM d, yyyy')}
      </p>
    </div>
  );
};
```

**Notifications API Slice:**
```typescript
// src/store/api/notificationsApi.ts
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export const notificationsApi = createApi({
  reducerPath: 'notificationsApi',
  baseQuery: fetchBaseQuery({ 
    baseUrl: '/api',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) headers.set('authorization', `Bearer ${token}`);
      return headers;
    },
  }),
  tagTypes: ['Notifications'],
  endpoints: (builder) => ({
    getRecentNotifications: builder.query({
      query: ({ limit = 5 }) => `notifications/recent?limit=${limit}`,
      providesTags: ['Notifications'],
    }),
    getUnreadCount: builder.query<{ count: number }, void>({
      query: () => 'notifications/unread-count',
      providesTags: ['Notifications'],
    }),
    markNotificationAsRead: builder.mutation({
      query: (id) => ({
        url: `notifications/${id}/read`,
        method: 'PATCH',
      }),
      invalidatesTags: ['Notifications'],
    }),
  }),
});

export const { 
  useGetRecentNotificationsQuery, 
  useGetUnreadCountQuery,
  useMarkNotificationAsReadMutation 
} = notificationsApi;
```

**Notification Badge in Header:**
```typescript
// Update to DashboardHeader component
import { useGetUnreadCountQuery } from '../../store/api/notificationsApi';

export const DashboardHeader: React.FC = () => {
  const { data: unreadData } = useGetUnreadCountQuery();
  const unreadCount = unreadData?.count ?? 0;

  return (
    <header className="...">
      <button className="notif-btn relative" aria-label={`Notifications — ${unreadCount} new`}>
        <span aria-hidden="true">🔔</span>
        {unreadCount > 0 && (
          <span className="absolute top-1 right-1 w-5 h-5 bg-error text-neutral-0 text-caption font-semibold rounded-full flex items-center justify-center">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>
    </header>
  );
};
```

### Documentation References

* **Redux Toolkit Query Mutations**: https://redux-toolkit.js.org/rtk-query/usage/mutations
* **Redux Toolkit Query Tags**: https://redux-toolkit.js.org/rtk-query/usage/automated-refetching
* **date-fns formatDistanceToNow**: https://date-fns.org/v2.30.0/docs/formatDistanceToNow
* **Pusher Channels**: https://pusher.com/docs/channels/getting_started/javascript/

### Edge Cases

* **What happens when notification timestamp is far in the past (>30 days)?** Display absolute date format instead of relative time (e.g., "Jan 15, 2026" instead of "32 days ago").
* **How does the system handle very long notification messages?** Truncate at 120 characters with "..." and show full message on hover tooltip or in notification detail view.
* **What happens if document file name is very long?** Truncate filename with CSS text-overflow ellipsis preserving file extension (e.g., "very-long-document-na....pdf").
* **How does real-time notification update handle duplicate notifications?** Deduplicate by notification ID; ignore duplicate real-time events to prevent UI flicker.

## Traceability

### Parent Epic

* EP-001

### Requirement Tags

* UXR-401, UXR-402, UXR-502, UXR-605, NFR-001

### Related Tasks

* task_001_fe_dashboard_page_layout.md - Header notification bell integration
* task_007_be_notifications_documents_api.md - Backend API endpoints

## Story Points

* 3

## Status

* not-started
