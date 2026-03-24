# Task - task_001_fe_dashboard_page_layout

## Task ID

* ID: task_001_fe_dashboard_page_layout

## Task Title

* Implement Patient Dashboard Page Layout and Navigation (Frontend)

## Parent User Story

* US_067 - Patient Dashboard - Post-Login Landing Page

## Description

Create the foundational page structure for the Patient Dashboard (SCR-003) including persistent navigation, breadcrumb, header with notifications and user avatar, and main content grid layout. Implement routing and role-based redirection after successful login.

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-003-patient-dashboard.html |
| **Screen Spec** | figma_spec.md#SCR-003 |
| **UXR Requirements** | UXR-001, UXR-002, UXR-003, UXR-301, UXR-303 |
| **Design Tokens** | designsystem.md#typography, designsystem.md#colors, designsystem.md#spacing |

## Technology Layer

* Frontend (React 18.x + TypeScript 5.x + Redux Toolkit 2.x + Tailwind CSS)

## Acceptance Criteria

1. **Given** a patient successfully authenticates via UC-002, **When** authentication completes, **Then** the system redirects to `/dashboard` route (SCR-003) within 500ms (NFR-001).

2. **Given** I am viewing the Patient Dashboard, **When** the page renders, **Then** the system displays a persistent navigation sidebar with active state on "Dashboard" and accessible links to "My Appointments," "Find Providers," and "Health Records" per UXR-003.

3. **Given** the dashboard page is loaded, **When** rendered on desktop (1024px+), **Then** the system displays a two-column grid layout with navigation sidebar (240px) and main content area following the wireframe structure.

4. **Given** the dashboard is viewed on mobile (320px-767px), **When** the layout adapts, **Then** the system hides the sidebar and displays a hamburger menu icon, stacking content in single column per UXR-303.

5. **Given** the page header is rendered, **When** displayed, **Then** the system shows breadcrumb navigation ("Home / Dashboard"), notification bell icon with badge (if unread notifications exist), and user avatar with initials.

6. **Given** I click the notification bell icon, **When** the action executes, **Then** the system provides visual feedback within 200ms (button state change) per UXR-501.

7. **Given** I am on the dashboard page, **When** I press Tab key, **Then** the system provides visible focus indicators with 2px outline following WCAG 2.2 AA standards.

8. **Given** the page is loading, **When** navigation elements render before data, **Then** the system applies design tokens (primary-500 for active nav, neutral-600 for inactive) per UXR-401.

## Implementation Checklist

- [ ] Create PatientDashboard page component at `src/pages/PatientDashboard.tsx` with protected route wrapper requiring `role: 'patient'`
- [ ] Implement PersistentNav component with navigation items (Dashboard, My Appointments, Find Providers, Health Records) using React Router NavLink with active state styling
- [ ] Add DashboardHeader component with logo, breadcrumb navigation, notification bell button (with conditional badge), and user avatar (displaying user initials)
- [ ] Implement responsive grid layout using CSS Grid (2-column desktop, 1-column mobile) with Tailwind breakpoints (md: 768px, lg: 1024px)
- [ ] Add mobile hamburger menu toggle with accessible aria-label and keyboard support (Enter/Space to toggle)
- [ ] Implement post-login redirect logic in AuthContext checking user role and navigating to `/dashboard` for patients
- [ ] Apply design tokens from designsystem.md for colors (primary-500, neutral-600), spacing (p-6, gap-4), and typography (font-h2 for page title)
- [ ] Add keyboard navigation support with focus management and skip-to-content link per WCAG 2.2 AA

## Estimated Effort

* 6 hours

## Dependencies

- US_020 (Role-based access control and navigation foundation)
- US_018 (Patient authentication system)
- AuthContext and user state management from authentication implementation

## Technical Context

### Architecture Patterns

* **Pattern**: Three-layer architecture with React component hierarchy (Page → Container → Presentational)
* **State Management**: Redux Toolkit for user session and authentication state
* **Routing**: React Router v6 with protected route wrapper checking user role
* **Responsive Design**: Mobile-first approach using Tailwind CSS breakpoints

### Related Requirements

* FR-002: Secure session management with 15-minute timeout
* UC-002: Patient Login - redirect to role-appropriate dashboard
* UXR-001: Max 3 clicks to any feature from dashboard
* UXR-002: Visual hierarchy distinguishing primary vs secondary actions
* UXR-003: Persistent role-based navigation
* UXR-301: Responsive layout adaptation
* UXR-303: Multi-column to single-column on mobile
* NFR-001: API response time within 500ms at 95th percentile

### Implementation References

**React Router Protected Routes:**
```typescript
// src/components/ProtectedRoute.tsx
import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: 'patient' | 'staff' | 'admin';
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRole }) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && user?.role !== requiredRole) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
};
```

**Persistent Navigation Pattern:**
```typescript
// src/components/PersistentNav.tsx
import { NavLink } from 'react-router-dom';

const navItems = [
  { label: 'Dashboard', path: '/dashboard', icon: 'grid' },
  { label: 'My Appointments', path: '/appointments', icon: 'calendar' },
  { label: 'Find Providers', path: '/providers', icon: 'search' },
  { label: 'Health Records', path: '/records', icon: 'document' }
];

export const PersistentNav: React.FC = () => (
  <nav className="w-60 bg-neutral-50 border-r border-neutral-200 p-4" aria-label="Patient navigation">
    <div className="space-y-1">
      {navItems.map((item) => (
        <NavLink
          key={item.path}
          to={item.path}
          className={({ isActive }) =>
            `flex items-center gap-3 px-3 py-2 rounded-md transition-colors ${
              isActive
                ? 'bg-primary-50 text-primary-500 font-medium border-l-3 border-primary-500'
                : 'text-neutral-600 hover:bg-neutral-100'
            }`
          }
          aria-current={({ isActive }) => (isActive ? 'page' : undefined)}
        >
          <span className="w-5 h-5" aria-hidden="true">{/* Icon */}</span>
          {item.label}
        </NavLink>
      ))}
    </div>
  </nav>
);
```

**Responsive Layout Pattern:**
```typescript
// src/pages/PatientDashboard.tsx
export const PatientDashboard: React.FC = () => {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  return (
    <div className="min-h-screen grid grid-cols-1 lg:grid-cols-[240px_1fr] grid-rows-[64px_1fr]">
      <DashboardHeader 
        onMenuToggle={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
      />
      
      {/* Desktop: Always visible, Mobile: Toggle */}
      <aside className={`
        lg:block bg-neutral-50 border-r border-neutral-200
        ${isMobileMenuOpen ? 'block' : 'hidden'}
      `}>
        <PersistentNav />
      </aside>

      <main id="main-content" className="p-6 lg:p-8 bg-neutral-100">
        {/* Dashboard content sections */}
      </main>
    </div>
  );
};
```

**Design Token Application:**
```typescript
// Example using Tailwind with design tokens
<button className="
  px-5 py-2 
  bg-primary-500 hover:bg-primary-600 
  text-neutral-0 
  rounded-md 
  font-medium text-body
  transition-fast
  focus:outline-2 focus:outline-primary-500 focus:outline-offset-2
">
  Book Appointment
</button>
```

### Documentation References

* **React Router v6 Protected Routes**: https://reactrouter.com/en/main/start/overview#protected-routes
* **Tailwind CSS Grid Layout**: https://tailwindcss.com/docs/grid-template-columns
* **Tailwind Responsive Design**: https://tailwindcss.com/docs/responsive-design
* **WCAG 2.2 Focus Visible**: https://www.w3.org/WAI/WCAG22/Understanding/focus-visible
* **Redux Toolkit Authentication**: https://redux-toolkit.js.org/tutorials/quick-start

### Edge Cases

* **What happens if user role is missing during redirect?** Default to login page and log warning; display "Role information missing, please log in again" message.
* **How does mobile menu behave on orientation change?** Close mobile menu automatically when viewport transitions from mobile to desktop breakpoint to prevent UI overlap.
* **What happens if navigation items exceed viewport height?** Implement vertical scrolling with overflow-y-auto on navigation container while keeping header fixed.
* **How does keyboard navigation work with mobile menu?** Trap focus within mobile menu when open; escape key closes menu and returns focus to hamburger button.

## Traceability

### Parent Epic

* EP-001

### Requirement Tags

* FR-002, UC-002, UXR-001, UXR-002, UXR-003, UXR-301, UXR-303, NFR-001

### Related Tasks

* task_002_fe_stat_cards.md - Statistics cards displayed in main content area
* task_003_fe_quick_actions_appointments.md - Quick action cards and appointments list
* task_006_be_dashboard_api.md - Backend API providing dashboard data

## Story Points

* 3

## Status

* not-started
