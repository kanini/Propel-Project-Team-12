# Task - task_003_fe_screen_reader_compatibility

## Requirement Reference
- User Story: US_059 - WCAG 2.2 AA & Semantic HTML
- Story Location: .propel/context/tasks/EP-011-I/us_059/us_059.md
- Acceptance Criteria:
    - AC-3: **Given** screen reader compatibility (UXR-206), **When** a screen reader traverses the page, **Then** all interactive elements have accessible names, dynamic content updates announce via ARIA live regions, and decorative images have empty alt attributes.
- Edge Case:
    - Dynamic content loads (e.g., search results): An ARIA live region with role="status" announces the update count without interrupting the current reading flow.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/ (all 26 wireframes SCR-001 through SCR-026) |
| **Screen Spec** | figma_spec.md (All screens) |
| **UXR Requirements** | UXR-206 (Screen reader compatibility), UXR-207 (ARIA live regions for dynamic content), UXR-204 (Meaningful alt text) |
| **Design Tokens** | designsystem.md#accessibility-requirements |

> **Wireframe Status Legend:**
> - **AVAILABLE**: Local file exists at specified path
> - **PENDING**: UI-impacting task awaiting wireframe (provide file or URL)
> - **EXTERNAL**: Wireframe provided via external URL
> - **N/A**: Task has no UI impact

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**
**IF Wireframe Status = AVAILABLE or EXTERNAL:**
- **MUST** open and reference the wireframe file/URL during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, hover, focus, error, loading)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px
- Run `/analyze-ux` after implementation to verify pixel-perfect alignment

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React + TypeScript | React 18.x, TypeScript 5.x |
| Frontend | Redux Toolkit | 2.x |
| Frontend | Tailwind CSS | Latest |
| Library | React Router | v6 |
| Testing | Vitest + React Testing Library | Latest |
| Tool | axe-core | Latest |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview

Implement comprehensive screen reader support across all components and pages to meet WCAG 2.2 Level AA standards. This task ensures all interactive elements have accessible names (via visible labels, aria-label, or aria-labelledby), dynamic content updates announce appropriately through ARIA live regions, decorative images are hidden from screen readers (`alt=""`), and informative images have meaningful alt text. The implementation will be validated with NVDA, JAWS, and VoiceOver screen readers.

## Dependent Tasks
- task_001_fe_semantic_html_landmarks (provides semantic structure foundation)

## Impacted Components
- **All Interactive Components**: Buttons, links, form controls, modals, dropdowns
- **Dynamic Content Components**: Provider search, appointment booking, document upload status, queue management
- **Image Components**: Provider images, document thumbnails, icons, logos
- **Notification/Toast Components**: Success, error, warning messages
- **Loading States**: Skeleton loaders, spinners, progress indicators

## Implementation Plan

### 1. Audit Accessible Names
- Review all interactive elements (buttons, links, inputs, selects) for accessible names
- Identify icon-only buttons lacking aria-label attributes
- Check form controls for proper label associations
- Document components missing accessible names

### 2. Implement Accessible Names
- Add `aria-label` to icon-only buttons (e.g., "Close modal", "Search", "Delete appointment")
- Ensure form inputs use `<label for="inputId">` or `aria-labelledby`
- Add `aria-label` to custom controls (date pickers, time slot selectors)
- Use `aria-describedby` for supplementary help text
- Add `title` attribute to abbreviations and acronyms

### 3. Implement ARIA Live Regions
- Add `role="status" aria-live="polite"` for non-critical updates (search results count, filtered items)
- Add `role="alert" aria-live="assertive"` for critical errors requiring immediate attention
- Create reusable `<LiveRegion>` component for consistent implementation
- Add live regions to: Provider search results, appointment booking confirmation, document processing status, queue updates, form validation errors

### 4. Implement Image Accessibility
- Audit all `<img>` elements and background images
- Add meaningful alt text to informative images (provider photos: "Dr. Jane Smith", document thumbnails: "Lab results uploaded March 2024")
- Use `alt=""` for decorative images (icons, dividers, backgrounds)
- Add `aria-hidden="true"` to decorative SVG icons
- Ensure icon fonts/SVGs paired with text have `aria-hidden="true"` on icon

### 5. Enhance Modal and Dialog Accessibility
- Add `role="dialog"` and `aria-modal="true"` to modal components
- Implement `aria-labelledby` pointing to modal title
- Add `aria-describedby` for modal descriptions (optional)
- Ensure modals trap focus and restore focus on close
- Add visible close button with accessible name

### 6. Improve Navigation Announcements
- Add `aria-current="page"` to active navigation links
- Use `aria-expanded` for expandable navigation sections
- Add breadcrumb navigation with `aria-label="Breadcrumb"`
- Announce page transitions via document title changes

### 7. Enhance Form Accessibility
- Add `aria-required` to required form fields
- Link error messages to inputs via `aria-describedby`
- Add `aria-invalid="true"` to fields with validation errors
- Use `<fieldset>` and `<legend>` for radio/checkbox groups
- Add instructions via `aria-describedby` for complex inputs

### 8. Test with Screen Readers
- Test all 26 screens with NVDA (Windows)
- Test key workflows with JAWS (Windows) if available
- Test on macOS with VoiceOver
- Document screen reader testing results
- Fix identified issues

## Current Project State

```
src/frontend/src/
├── components/
│   ├── common/
│   │   ├── EmptyState.tsx (has aria-live)
│   │   ├── SkeletonLoader.tsx (has aria-label)
│   │   └── Pagination.tsx
│   ├── forms/
│   │   └── PasswordStrengthIndicator.tsx (has aria-label, aria-valuenow)
│   ├── providers/
│   │   ├── ProviderCard.tsx (has some aria-labels)
│   │   └── ProviderSearch.tsx (has aria-label on input)
│   ├── appointments/
│   │   ├── AppointmentCard.tsx (needs aria-labels)
│   │   └── TimeSlotGrid.tsx (needs accessible names)
│   └── modals/
│       └── SessionTimeoutModal.tsx (needs dialog role)
├── pages/
│   └── (various pages needing live regions)
└── __tests__/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/components/common/LiveRegion.tsx | Reusable ARIA live region component |
| MODIFY | src/components/common/EmptyState.tsx | Enhance with proper role and aria-live |
| MODIFY | src/components/modals/SessionTimeoutModal.tsx | Add dialog role, focus management |
| MODIFY | src/components/appointments/TimeSlotGrid.tsx | Add accessible names to time slot buttons |
| MODIFY | src/components/providers/ProviderSearch.tsx | Add live region for search results count |
| MODIFY | src/components/forms/*.tsx | Enhance form accessibility (aria-required, aria-invalid) |
| MODIFY | src/components/**/*.tsx | Add aria-label to icon-only buttons |
| CREATE | src/__tests__/accessibility/screenReader.test.tsx | Screen reader compatibility test suite |
| CREATE | docs/SCREEN_READER_TESTING.md | Screen reader testing guide for developers |
| MODIFY | src/components/README.md | Add ARIA usage guidelines |

## External References

### WCAG 2.2 Standards
- [Name, Role, Value (4.1.2)](https://www.w3.org/WAI/WCAG22/Understanding/name-role-value.html)
- [Non-text Content (1.1.1)](https://www.w3.org/WAI/WCAG22/Understanding/non-text-content.html)
- [Status Messages (4.1.3)](https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html)

### WAI-ARIA Authoring Practices
- [ARIA Live Regions](https://www.w3.org/WAI/ARIA/apg/practices/names-and-descriptions/)
- [Providing Accessible Names and Descriptions](https://www.w3.org/WAI/ARIA/apg/practices/names-and-descriptions/)
- [Dialog (Modal) Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/)
- [Alert Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/alert/)

### React & ARIA
- [Accessible Rich Internet Applications (ARIA) in React](https://react.dev/learn/accessibility#aria)
- [React ARIA (Adobe)](https://react-spectrum.adobe.com/react-aria/)

### Screen Reader Documentation
- [NVDA User Guide](https://www.nvaccess.org/files/nvda/documentation/userGuide.html)
- [JAWS Screen Reader](https://www.freedomscientific.com/products/software/jaws/)
- [VoiceOver User Guide (macOS)](https://support.apple.com/guide/voiceover/welcome/mac)

### Testing
- [Testing Library: ByRole Queries](https://testing-library.com/docs/queries/byrole/)
- [axe-core: Screen Reader Testing Rules](https://github.com/dequelabs/axe-core/blob/develop/doc/rule-descriptions.md)

### Project Documentation
- [Web Accessibility Standards Rule](.propel/rules/web-accessibility-standards.md)
- [Frontend Testing Guide](../../../docs/FRONTEND_TESTING.md)

## Build Commands

```bash
# Navigate to frontend directory
cd src/frontend

# Install dependencies
npm install

# Run type checking
npm run typecheck

# Run linting
npm run lint

# Run unit tests
npm test

# Run accessibility tests only
npm test -- --grep="screen reader"

# Run unit tests with coverage
npm run test:coverage

# Build for production
npm run build

# Development server (manual testing)
npm run dev
```

## Implementation Validation Strategy
- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] All axe-core label and name tests pass
- [ ] Manual testing with NVDA confirms all elements have accessible names
- [ ] Live region announcements verified with screen reader
- [ ] All images have appropriate alt text or aria-hidden
- [ ] Modal focus management works correctly
- [ ] Form validation errors announced properly

## Implementation Checklist
- [ ] Audit all interactive elements and document missing accessible names
- [ ] Create reusable `LiveRegion` component with polite/assertive modes
- [ ] Add `aria-label` to all icon-only buttons across components
- [ ] Ensure all form inputs have proper label associations (`<label for>` or `aria-labelledby`)
- [ ] Add `aria-describedby` for supplementary help text on complex form fields
- [ ] Implement ARIA live regions for provider search results count
- [ ] Add live region to appointment booking confirmation flow
- [ ] Implement live region for document upload/processing status updates
- [ ] Add live region to queue management for real-time updates
- [ ] Audit all `<img>` elements and add meaningful alt text or `alt=""`
- [ ] Add `aria-hidden="true"` to all decorative SVG icons
- [ ] Update modal components with `role="dialog"`, `aria-modal="true"`, `aria-labelledby`
- [ ] Implement focus trapping in modals using focus-trap-react or custom solution
- [ ] Add `aria-current="page"` to active navigation links
- [ ] Add `aria-expanded` to expandable navigation sections (if any)
- [ ] Implement `aria-required` and `aria-invalid` on form fields
- [ ] Link form error messages to inputs via `aria-describedby`
- [ ] Test all 26 screens with NVDA screen reader (document results)
- [ ] Test key workflows with VoiceOver (macOS) if available
- [ ] Create screen reader testing guide for developers (docs/SCREEN_READER_TESTING.md)
- [ ] Update component documentation with ARIA usage patterns
