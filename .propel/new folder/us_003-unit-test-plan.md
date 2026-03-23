# Unit Test Plan - US_003: Session Timeout with Auto-Save and Re-Authentication

## Test Plan Metadata

| Attribute | Value |
|-----------|-------|
| **Story ID** | US_003 |
| **Story Title** | Session Timeout with Auto-Save and Re-Authentication |
| **Plan Version** | 1.0 |
| **Created Date** | 2026-03-17 |
| **Component Under Test** | Session Management & Auto-Save Service |
| **Test Coverage Target** | 95%+ branch coverage |

---

## Test Objectives

- Validate session timeout warning modal appears at 13-minute inactivity mark (2 minutes before 15-minute timeout)
- Verify session extension resets timer and closes modal
- Confirm logout on timeout with auto-save of form data
- Test form data restoration after re-authentication
- Validate inactivity timer resets on user actions
- Ensure modal is non-dismissable by clicking outside
- Test auto-save failure handling
- Verify timeout during API calls in progress
- Test multi-tab session synchronization
- Test network disconnection scenarios

---

## Test Scope

### In Scope
- Inactivity timer logic (15-minute total timeout)
- Session timeout warning modal (appears at 13 minutes)
- Extend session functionality
- Session logout on timeout
- Form data auto-save to storage
- Data restoration after re-authentication
- Inactivity detection on user actions (click, type, scroll)
- Modal non-dismissability
- Storage error handling
- API call completion during timeout
- Multi-tab session synchronization
- Offline indicator display

### Out of Scope
- UI modal rendering (separate E2E/integration tests)
- Form field validation (handled in respective feature stories)
- Backend session invalidation detail (integration tests)
- Network layer behavior (separate infrastructure testing)
- Storage layer persistence (integration tests)
- Analytics or logging of timeout events (monitoring)

---

## Test Suite Organization

### 1. Inactivity Timer & Warning Modal Tests

#### 1.1 Timer Initialization
| Test ID | Test Case | Condition | Expected Behavior | Acceptance Criteria |
|---------|-----------|-----------|-------------------|-------------------|
| TIMER-001 | Timer initialized on login | User authenticated | Timer starts with 15-minute countdown | AC-1 |
| TIMER-002 | Timer value confirmed | Timer running | Timer decrements every second | AC-1 |
| TIMER-003 | Timer accessible to monitoring | Timer active | Current remaining time readable from timer state | AC-1 |

#### 1.2 Warning Modal Display
| Test ID | Test Case | Condition | Expected Output | Acceptance Criteria |
|---------|-----------|-----------|-----------------|-------------------|
| MODAL-001 | Modal appears at 13-minute mark | 13 minutes of inactivity reached | Warning modal displayed with message | AC-1 |
| MODAL-002 | Warning message correct | Modal displayed | Message: "Your session will expire in 2 minutes" | AC-1 |
| MODAL-003 | Extend Session button present | Modal displayed | Button labeled "Extend Session" visible | AC-1 |
| MODAL-004 | Logout button present | Modal displayed | Button labeled "Logout" visible | AC-1 |
| MODAL-005 | Modal appears only once | Warning threshold reached | Modal displayed exactly once (no duplicates) | AC-1 |
| MODAL-006 | Modal z-index on top | Modal displayed | Modal overlays all page content | AC-1 |
| MODAL-007 | Modal displays correct timeout countdown | Modal shown | Countdown shows "2 minutes" or decreasing seconds | AC-1 |

#### 1.3 Modal Styling & Accessibility
| Test ID | Test Case | Scenario | Expected Behavior | Notes |
|---------|-----------|----------|-------------------|-------|
| MODAL-008 | Modal has focus trap | Modal displayed | Tab/focus stays within modal | Accessibility |
| MODAL-009 | Modal has contrast | Modal rendered | Text meets WCAG AA contrast ratios | A11Y |
| MODAL-010 | Modal is keyboard navigable | Modal displayed | Buttons accessible via keyboard (Tab, Enter) | A11Y |
| MODAL-011 | Modal has ARIA labels | Modal rendered | ARIA label: `role="alertdialog"`, `aria-label="Session timeout warning"` | A11Y |

---

### 2. Session Extension Tests

#### 2.1 Extend Session Button Functionality
| Test ID | Test Case | Action | Expected Result | Acceptance Criteria |
|---------|-----------|--------|-----------------|-------------------|
| EXTEND-001 | Click Extend Session button | User clicks button | Session timer resets to 15 minutes | AC-2 |
| EXTEND-002 | Modal closes on extend | Extend button clicked | Modal disappears from screen | AC-2 |
| EXTEND-003 | User can continue working | Session extended | Page remains accessible, no redirect | AC-2 |
| EXTEND-004 | Timer countdown visible after extend | Extended session active | New 15-minute timer visible/counting down | AC-2 |
| EXTEND-005 | Extend available multiple times | Active session | User can click Extend Session repeatedly | AC-2 |
| EXTEND-006 | Each extend resets full timer | Multiple extends | Each extension provides fresh 15 minutes | AC-2 |

#### 2.2 Extend Session - No Data Loss
| Test ID | Test Case | Setup | Expected Behavior | Acceptance Criteria |
|---------|-----------|-------|-------------------|-------------------|
| EXTEND-007 | Form data preserved on extend | User filling form, extends session | All entered data retained in form fields | AC-2 |
| EXTEND-008 | Session cookie updated | Extend clicked | Session cookie refresh timestamp updated | AC-2 |
| EXTEND-009 | JWT token refreshed | extend endpoint called | New JWT token issued with fresh TTL | AC-2 |

---

### 3. Session Timeout & Logout Tests

#### 3.1 Timeout Without Extension
| Test ID | Test Case | Condition | Expected Output | Acceptance Criteria |
|---------|-----------|-----------|-----------------|-------------------|
| TIMEOUT-001 | Auto-logout at 15-minute mark | No extend clicked, 2 minutes pass | User automatically logged out | AC-3 |
| TIMEOUT-002 | Redirect to sign-in page | Timeout triggered | Redirect to sign-in page occurs | AC-3 |
| TIMEOUT-003 | Session cleared server-side | Timeout triggered | Session invalidated on server | AC-3 |
| TIMEOUT-004 | Token invalidated | Session expires | JWT token no longer valid | AC-3 |
| TIMEOUT-005 | All API calls reject | Token expired | Subsequent API requests return 401 Unauthorized | AC-3 |

#### 3.2 Logout Button Functionality
| Test ID | Test Case | Action | Expected Result | Acceptance Criteria |
|---------|-----------|--------|-----------------|-------------------|
| LOGOUT-001 | Click Logout button | User clicks Logout button | Session immediately ended, redirect to sign-in | AC-3 |
| LOGOUT-002 | No auto-save delay | Logout clicked | Session ends immediately (no wait) | AC-3 |
| LOGOUT-003 | Token invalidated on logout | Logout triggered | Token removed from memory/storage | AC-3 |
| LOGOUT-004 | Session cleared immediately | Logout clicked | Server-side session cleared | AC-3 |

#### 3.3 Ignoring Warning Modal
| Test ID | Test Case | Scenario | Expected Behavior | Acceptance Criteria |
|---------|-----------|----------|-------------------|-------------------|
| IGNORE-001 | Ignore warning for 2 minutes | Modal displayed, user inactive | After 2 minutes, system logs out | AC-3 |
| IGNORE-002 | No action on modal for full duration | Timer expires | Auto-logout proceeds as planned | AC-3 |
| IGNORE-003 | Modal stays visible during countdown | User inactive, modal visible | Modal remains visible entire 2 minutes | AC-3 |

---

### 4. Auto-Save Functionality Tests

#### 4.1 Form Data Auto-Save on Timeout
| Test ID | Test Case | Setup | Expected Output | Acceptance Criteria |
|---------|-----------|-------|-----------------|-------------------|
| AUTOSAVE-001 | Auto-save intake form on timeout | User filling intake form, times out | Form data saved to localStorage | AC-4 |
| AUTOSAVE-002 | Auto-save booking form | User in booking, times out | All form fields saved to storage | AC-4 |
| AUTOSAVE-003 | Multiple form fields saved | User enters email, name, phone | All fields saved before redirect | AC-4 |
| AUTOSAVE-004 | Auto-save triggered before redirect | Timeout occurs | Data saved BEFORE redirect to sign-in | AC-4 |
| AUTOSAVE-005 | File upload data handled | User uploading file during timeout | File data or reference saved | AC-4 |
| AUTOSAVE-006 | Nested form data saved | Complex form structure | All nested fields/objects saved | AC-4 |

#### 4.2 Auto-Save Storage
| Test ID | Test Case | Condition | Expected Behavior | Storage |
|---------|-----------|-----------|-------------------|---------|
| STORAGE-001 | Data saved to localStorage | Auto-save triggered | Data persists in browser localStorage | AC-4 |
| STORAGE-002 | Data saved to sessionStorage | Auto-save triggered | Data persists in browser sessionStorage (tab-specific) | AC-4 |
| STORAGE-003 | Storage key structured | Data saved | Storage key format: `autosave_[userId]_[formId]` | AC-4 |
| STORAGE-004 | Timestamp included in saved data | Auto-save occurs | Saved object includes `timestamp: epoch` | AC-4 |
| STORAGE-005 | Form ID included with data | Auto-save triggered | Saved data includes `formId` identifier | AC-4 |

#### 4.3 Auto-Save Error Handling
| Test ID | Test Case | Condition | Expected Behavior | Edge Case |
|---------|-----------|-----------|-------------------|-----------|
| AUTOSAVE-007 | Storage quota exceeded | localStorage full | Warning displayed, error logged | E-001 |
| AUTOSAVE-008 | Auto-save fails gracefully | Storage write fails | Redirect to sign-in still occurs | E-001 |
| AUTOSAVE-009 | Error logged for support | Auto-save fails | Error entry created in logs | E-001 |
| AUTOSAVE-010 | User notified of save failure | Save attempt fails | Toast/notification: "Unable to save session data" | E-001 |

---

### 5. Data Restoration Tests

#### 5.1 Restoring Auto-Saved Data
| Test ID | Test Case | Setup | Expected Output | Acceptance Criteria |
|---------|-----------|-------|-----------------|-------------------|
| RESTORE-001 | Auto-saved data restored on login | User re-authenticates after timeout | Previous form data loaded into form | AC-5 |
| RESTORE-002 | All fields populated | Data restored | Each input field filled with saved value | AC-5 |
| RESTORE-003 | Correct form loaded | Multiple forms used | Correct form auto-populated (not wrong form) | AC-5 |
| RESTORE-004 | Restoration notification displayed | Login completes after timeout | Notification: "Your previous session data has been restored." | AC-5 |
| RESTORE-005 | Data valid and uncorrupted | Restore triggered | Restored data matches originally entered data | AC-5 |

#### 5.2 User Context Preservation
| Test ID | Test Case | Scenario | Expected Behavior | Acceptance Criteria |
|---------|-----------|----------|-------------------|-------------------|
| CONTEXT-001 | Previous page context preserved | User on intake page, timeout, re-login | Redirect to intake page (not dashboard) | AC-5 |
| CONTEXT-002 | Previous portal preserved | Patient on patient portal, timeout, re-login | Redirect to patient portal (not forced to staff) | AC-5 |
| CONTEXT-003 | Previous navigation state maintained | Page history intact | User can use back button appropriately | AC-5 |

#### 5.3 Stale Data Handling
| Test ID | Test Case | Condition | Expected Behavior | Notes |
|---------|-----------|-----------|-------------------|-------|
| RESTORE-006 | Stale data older than 24 hours | Auto-saved data > 24 hrs old | Data cleared, not restored | Data governance |
| RESTORE-007 | Data matches current schema | Restored data | Schema version checked; incompatible data rejected | Compatibility |
| RESTORE-008 | Clear auto-saved data option | After restoration | Button/option to clear saved data | UX |

---

### 6. Inactivity Detection Tests

#### 6.1 User Activity Events
| Test ID | Test Case | Action | Expected Behavior | Acceptance Criteria |
|---------|-----------|--------|-------------------|-------------------|
| ACTIVITY-001 | Mouse click resets timer | User clicks on page | Timer resets to 15 minutes | AC-6 |
| ACTIVITY-002 | Keyboard typing resets timer | User types in input field | Timer resets to 15 minutes | AC-6 |
| ACTIVITY-003 | Scroll action resets timer | User scrolls on page | Timer resets to 15 minutes | AC-6 |
| ACTIVITY-004 | Form input focus resets timer | User focuses form field | Timer resets to 15 minutes | AC-6 |
| ACTIVITY-005 | Button click resets timer | User clicks any button | Timer resets to 15 minutes | AC-6 |
| ACTIVITY-006 | Multiple quick clicks reset timer | User rapidly clicks | Timer resets once, not multiple times | AC-6 |

#### 6.2 Non-Activity Events (Should NOT Reset)
| Test ID | Test Case | Action | Expected Behavior | Notes |
|---------|-----------|--------|-------------------|-------|
| NOACTIVITY-001 | Modal interaction doesn't reset | User clicks modal button | Modal action handled, timer not reset | AC-6 |
| NOACTIVITY-002 | Toast/notification interaction | User interacts with toast | Toast cleared, timer not reset | AC-6 |
| NOACTIVITY-003 | Page visibility change | User switches browser tab | Inactivity continues (tab not visible) | AC-6 |

#### 6.3 Activity Tracking
| Test ID | Test Case | Condition | Expected Behavior | Notes |
|---------|-----------|-----------|-------------------|-------|
| ACTIVITY-007 | Event listener attached | Page loaded | Activity listeners registered globally | Implementation |
| ACTIVITY-008 | Event throttling applied | User continuously typing | Timer reset throttled (max 1/second) | Performance |
| ACTIVITY-009 | Timer reset logged | Activity detected | Activity timestamp logged internally | Audit |

---

### 7. Modal Non-Dismissability Tests

#### 7.1 Preventing Modal Dismissal
| Test ID | Test Case | Action | Expected Result | Acceptance Criteria |
|---------|-----------|--------|-----------------|-------------------|
| NODISMISS-001 | Clicking outside modal | User clicks page background | Modal remains visible (not closed) | AC-7 |
| NODISMISS-002 | ESC key ignored | User presses Escape | Modal remains visible | AC-7 |
| NODISMISS-003 | No close button on modal | Modal displayed | X/close button not present | AC-7 |
| NODISMISS-004 | Backdrop click prevented | User clicks backdrop area | Modal not dismissed | AC-7 |
| NODISMISS-005 | Modal stays on top of page | User attempts interaction | Modal blocks page interaction | AC-7 |

#### 7.2 Focus Trap
| Test ID | Test Case | Scenario | Expected Behavior | Notes |
|---------|-----------|----------|-------------------|-------|
| FOCUS-001 | Tab within modal | User presses Tab | Focus stays within modal buttons/inputs | A11Y |
| FOCUS-002 | Shift+Tab at start | User tabs backward from first item | Focus wraps to last item (Logout button) | A11Y |
| FOCUS-003 | Tab at end wraps | User tabs forward from last item | Focus wraps to first item (Extend button) | A11Y |

---

### 8. Edge Cases & Error Handling Tests

#### 8.1 Auto-Save During API Call
| Test ID | Test Case | Condition | Expected Behavior | Edge Case Reference |
|---------|-----------|-----------|-------------------|-------------------|
| APICALL-001 | Timeout during API request | API call in progress, inactivity timer expires | API call completes, then auto-save and logout | E-002 |
| APICALL-002 | API response after logout | API response arrives post-timeout | Response handled appropriately (not stored) | E-002 |
| APICALL-003 | API pending during timeout | Request pending with no response | Timeout still triggered after configured wait | E-002 |
| APICALL-004 | Partial response on timeout | API returns partial data | Partial data saved along with form data | E-002 |

#### 8.2 Multi-Tab Behavior
| Test ID | Test Case | Setup | Expected Behavior | Edge Case Reference |
|---------|-----------|-------|-------------------|-------------------|
| MULTITAB-001 | Activity in one tab | User active in Tab A, inactive in Tab B | Tab B timeout not affected by Tab A | E-003 |
| MULTITAB-002 | Sync session across tabs | User extends session in Tab A | Tab B timer resets to match Tab A | E-003 |
| MULTITAB-003 | Timeout synced across tabs | Timeout triggered in Tab A | Tab B logout also triggered | E-003 |
| MULTITAB-004 | Storage event triggers sync | Storage changed in Tab A | Tab B detects change via storage event | E-003 |
| MULTITAB-005 | Same auto-save key | Multiple tabs open | Both tabs use same storage key (no duplication) | E-003 |

#### 8.3 Network Disconnection
| Test ID | Test Case | Condition | Expected Behavior | Edge Case Reference |
|---------|-----------|-----------|-------------------|-------------------|
| OFFLINE-001 | Network disconnected before timeout | User loses connectivity | Timer continues locally | E-004 |
| OFFLINE-002 | Offline indicator displayed | Network lost | Offline indicator shown (per UXR-605) | E-004 |
| OFFLINE-003 | Auto-save works offline | Timeout occurs offline | Data saved to local storage | E-004 |
| OFFLINE-004 | Re-login attempted online | User comes back online | Re-authentication attempt succeeds | E-004 |
| OFFLINE-005 | Data restored after offline-then-online | Reconnect and login | Previous data restored | E-004 |

#### 8.4 Modal Display During Edge Cases
| Test ID | Test Case | Scenario | Expected Behavior | Notes |
|---------|-----------|----------|-------------------|-------|
| EDGE-001 | Modal displays while offline | Network lost at 13-min mark | Modal still displayed, buttons functional | Robustness |
| EDGE-002 | Modal during auto-save failure | Auto-save fails on logout | Modal closes when logout completes | Graceful |
| EDGE-003 | Multiple rapid logouts | User logs out, then immediately re-authenticates | Concurrent operations handled safely | Concurrency |

---

### 9. Performance & Monitoring Tests

#### 9.1 Timer Performance
| Test ID | Test Case | Condition | Expected Behavior | Notes |
|---------|-----------|-----------|-------------------|-------|
| PERF-001 | Timer CPU usage minimal | Timer running for 15 minutes | CPU usage < 1% | Performance |
| PERF-002 | Memory leak prevention | Timer cycles completed | No memory growth over time | Memory |
| PERF-003 | Accuracy of timer | Timer set for 15 minutes | Completes within ±1 second | Accuracy |

#### 9.2 Auto-Save Performance
| Test ID | Test Case | Condition | Expected Behavior | Notes |
|---------|-----------|-----------|-------------------|-------|
| PERF-004 | Auto-save completes quickly | Timeout triggered | Save to storage < 100ms | Performance |
| PERF-005 | Serialization efficient | Large form data | JSON serialize/save < 200ms | Performance |
| PERF-006 | No impact on page responsiveness | Auto-save running | Page UI remains responsive | UX |

#### 9.3 Monitoring & Logging
| Test ID | Test Case | Condition | Logged Event | Notes |
|---------|-----------|-----------|--------------|-------|
| LOG-001 | Session timeout event | Timeout triggered | `event: "session_timeout"`, `userId`, `timestamp` | Audit |
| LOG-002 | Session extension logged | Extend clicked | `event: "session_extended"`, `userId`, `newExpiry` | Audit |
| LOG-003 | Auto-save success logged | Data saved | `event: "autosave_success"`, `formId`, `dataSize` | Audit |
| LOG-004 | Auto-save failure logged | Save fails | `event: "autosave_failed"`, `error`, `formId` | Audit |

---

### 10. Integration Tests

#### 10.1 Cross-Feature Timeout Behavior
| Test ID | Test Case | Feature Context | Expected Behavior | Notes |
|---------|-----------|-----------------|-------------------|-------|
| INTEGRATE-001 | Timeout during patient intake | Patient filling intake form | Form auto-saved, data restored after login | Integration |
| INTEGRATE-002 | Timeout during booking confirmation | Staff confirming booking | Booking state saved, not duplicated on restore | Integration |
| INTEGRATE-003 | Timeout during document upload | Admin uploading documents | Upload state tracked, can resume | Integration |

#### 10.2 Role-Based Timeout Behavior
| Test ID | Test Case | Role | Expected Behavior | Notes |
|---------|-----------|------|-------------------|-------|
| ROLE-001 | Patient timeout | Patient inactive 15 min | Standard timeout, restore to patient portal | Role-specific |
| ROLE-002 | Staff timeout | Staff inactive 15 min | Standard timeout, restore to staff portal | Role-specific |
| ROLE-003 | Admin timeout | Admin inactive 15 min | Standard timeout, restore to admin portal | Role-specific |

---

## Test Data Requirements

### Valid Test Data Sets

```json
{
  "timerConfigurations": {
    "totalTimeoutMinutes": 15,
    "warningAtMinutes": 13,
    "warningDurationMinutes": 2,
    "inactivityThresholdSeconds": 1,
    "throttleMilliseconds": 1000
  },
  "formDataExamples": {
    "intakeForm": {
      "firstName": "John",
      "lastName": "Doe",
      "dateOfBirth": "1990-01-15",
      "medicalHistory": "None",
      "currentMedications": "Aspirin"
    },
    "bookingForm": {
      "appointmentDate": "2026-03-25",
      "appointmentTime": "14:30",
      "serviceType": "consultation",
      "notes": "Follow-up appointment"
    }
  },
  "autoSaveScenarios": [
    {
      "name": "Partial Intake",
      "formId": "intake_form_1",
      "fieldsCompleted": 3,
      "fieldsTotal": 8,
      "expectedSaveSize": "~500 bytes"
    },
    {
      "name": "Complete Booking",
      "formId": "booking_form_1",
      "fieldsCompleted": 6,
      "fieldsTotal": 6,
      "expectedSaveSize": "~350 bytes"
    }
  ],
  "userActivityEvents": [
    "click",
    "keydown",
    "keyup",
    "scroll",
    "focus",
    "input",
    "change",
    "mousemove"
  ],
  "storageKeys": {
    "autosave": "autosave_{userId}_{formId}",
    "sessionTimeout": "session_timeout_marker",
    "lastActivity": "last_activity_timestamp"
  },
  "offlineScenarios": [
    {
      "event": "offline",
      "timing": "at 10 minutes inactivity",
      "expectedBehavior": "Timer continues, offline indicator shown"
    },
    {
      "event": "online",
      "timing": "after timeout triggers",
      "expectedBehavior": "Re-auth attempted, data restored if login successful"
    }
  ]
}
```

---

## Test Execution Strategy

### Phase 1: Unit Tests (Isolated)
- Timer countdown logic
- Inactivity detection and reset
- Auto-save serialization
- Data restoration logic
- Modal non-dismissability
- Event throttling

### Phase 2: Integration Tests
- Timer with session management
- Auto-save with storage layer
- Modal with page interaction
- Data restoration with authentication
- Multi-tab synchronization
- Offline behavior

### Phase 3: E2E Tests
- Complete session timeout workflow
- User extends session before timeout
- User logs out via modal
- Session timeout with auto-save and re-login
- Form data restoration verification
- Multi-tab timeout synchronization

### Phase 4: Performance & Stress Tests
- Timer performance over extended periods
- Auto-save with large form data
- Memory leak detection
- Concurrent timeout/extension scenarios
- High-frequency activity events

---

## Acceptance Criteria Coverage

| AC # | Test IDs | Status | Coverage |
|------|----------|--------|----------|
| AC-1 | TIMER-001–003, MODAL-001–011, IGNORE-001–003 | Planned | ✓ |
| AC-2 | EXTEND-001–009 | Planned | ✓ |
| AC-3 | TIMEOUT-001–005, LOGOUT-001–004, IGNORE-001–003 | Planned | ✓ |
| AC-4 | AUTOSAVE-001–010, STORAGE-001–005 | Planned | ✓ |
| AC-5 | RESTORE-001–008, CONTEXT-001–003 | Planned | ✓ |
| AC-6 | ACTIVITY-001–009, NOACTIVITY-001–003 | Planned | ✓ |
| AC-7 | NODISMISS-001–005, FOCUS-001–003 | Planned | ✓ |

---

## Edge Cases Coverage

| Edge Case | Test IDs | Coverage |
|-----------|----------|----------|
| Auto-save fails (storage full) | AUTOSAVE-007–010, E-001 | ✓ |
| Timeout during API call | APICALL-001–004, E-002 | ✓ |
| Multiple browser tabs | MULTITAB-001–005, E-003 | ✓ |
| Network disconnection | OFFLINE-001–005, E-004 | ✓ |

---

## Dependencies & Assumptions

### Dependencies
- User authentication system from US_002 complete
- Session management service available
- Browser storage (localStorage/sessionStorage) available
- Form data structure consistent across features
- Network connectivity detection available

### Assumptions
- Timer uses system clock (not client time which can be manipulated)
- Auto-save uses JSON serialization
- Storage quota ≥ 5MB
- Activity events fire reliably
- Modal library available and supports focus trap
- Browser supports storage events for multi-tab communication

---

## Risk & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Timer inaccuracy | Low | Medium | Test timer against system clock, use setInterval appropriately |
| Storage quota exceeded | Medium | High | Monitor storage usage, implement cleanup on old data |
| Auto-save data corruption | Low | High | Validate data on restore, implement checksums |
| Multi-tab synchronization issues | Medium | Medium | Use storage events, implement retry logic |
| Modal blocking critical actions | Low | High | Test modal z-index, ensure buttons are accessible |
| API call timeout mismatch | Medium | Medium | Coordinate timer and API timeout configs |
| Memory leaks in event listeners | Low | High | Proper cleanup on logout/redirect |

---

## Success Criteria

- ✓ All test cases executed
- ✓ 95%+ code coverage
- ✓ All acceptance criteria verified (AC-1 through AC-7)
- ✓ All edge cases handled gracefully
- ✓ Timer accurate to ±1 second
- ✓ Auto-save completes before redirect
- ✓ Data restoration works after re-authentication
- ✓ Modal cannot be dismissed except by buttons
- ✓ No memory leaks in long-running sessions
- ✓ Performance meets <100ms auto-save requirement
- ✓ Multi-tab synchronization verified
- ✓ Offline scenarios handled appropriately
