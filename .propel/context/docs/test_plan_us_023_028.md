---
id: test_plan_us_023_028
title: Test Plan - EP-002 Patient Appointment Booking (US_023-028)
version: 1.0.0
status: draft
author: AI Assistant
created: 2026-03-23
scope: "EP-002 appointment booking, availability, calendar sync, confirmation"
---

# Test Plan: EP-002 Patient Appointment Booking (US_023-028)

## Overview

This test plan covers **6 feature-critical user stories** implementing the complete patient appointment booking workflow, from viewing availability through confirmation and calendar synchronization. These stories directly impact business KPIs (booking conversion, no-show reduction).

**User Stories Covered:**
- US_023: Provider Availability Calendar
- US_024: Available Time Slot Display
- US_025: Appointment Booking
- US_026: Preferred Appointment Slot Swap
- US_027: Appointment Confirmation Email
- US_028: Google Calendar & Outlook Sync

---

## 1. US_023: Provider Availability Calendar

### Test Objectives
- Verify provider weekly calendar displays correct availability
- Test working hours configuration
- Confirm lunch break exclusions and buffer times
- Validate clinic holiday handling
- Test provider on-call scheduling

### Test Cases

#### TC-US-023-HP-01: Weekly Calendar Display
| Field | Value |
|-------|-------|
| Requirement | FR-011 |
| Type | happy_path |
| Priority | P0 |

**Given**: Provider "Dr. Sarah Johnson" selected  
**When**: Patient views provider calendar  
**Then**: Seven-day calendar displays with color-coded availability

**Provider Schedule:**
```yaml
dr_sarah_johnson:
  working_hours:
    monday: "9:00 AM - 5:00 PM"
    tuesday: "9:00 AM - 5:00 PM"
    wednesday: "9:00 AM - 1:00 PM"  # Half day
    thursday: "9:00 AM - 5:00 PM"
    friday: "9:00 AM - 5:00 PM"
    saturday: off
    sunday: off
  
  lunch_break: "12:00 PM - 1:00 PM"  # Daily
  buffer_time: 15  # minutes between appointments
  appointment_duration: 30  # minutes
```

**Expected Results:**
- [ ] Calendar displays 7 days (Mon-Sun)
- [ ] Green slots: Available (free)
- [ ] Gray slots: Booked
- [ ] Red slots: Unavailable (lunch, off)
- [ ] Working hours boundaries enforced
- [ ] Lunch break removed from availability
- [ ] Buffer time prevents back-to-back bookings
- [ ] Current day highlighted
- [ ] Navigation to next/previous week available

---

#### TC-US-023-HP-02: Lunch Break Exclusion
| Field | Value |
|-------|-------|
| Requirement | FR-011 |
| Type | happy_path |
| Priority | P1 |

**Given**: Provider with lunch break 12:00 PM - 1:00 PM  
**When**: View availability calendar  
**Then**: No slots available during lunch break

**Expected Results:**
- [ ] Lunch break period unavailable (no bookings)
- [ ] Slots before lunch end at 11:45 AM
- [ ] Slots after lunch start at 1:15 PM
- [ ] Clear visual indication (red/disabled)
- [ ] Tooltip: "Provider unavailable during lunch break"

---

#### TC-US-023-HP-03: Clinic Holiday Handling
| Field | Value |
|-------|-------|
| Requirement | FR-011 |
| Type | happy_path |
| Priority | P1 |

**Given**: Clinic closed on "2026-07-04" (Independence Day)  
**When**: View calendar for that date  
**Then**: Entire day unavailable

**Holiday Configuration:**
```yaml
clinic_holidays:
  - date: "2026-07-04"
    name: "Independence Day"
    recurring: true
  - date: "2026-12-25"
    name: "Christmas"
    recurring: true
```

**Expected Results:**
- [ ] Holiday date shows all red (unavailable)
- [ ] All providers blocked for that date
- [ ] Tooltip explains holiday closure
- [ ] Recurring holidays applied annually
- [ ] Admin can manage holiday calendar

---

#### TC-US-023-ER-01: Provider On-Call Blocking
| Field | Value |
|-------|-------|
| Requirement | FR-011 |
| Type | error |
| Priority | P1 |

**Given**: Dr. Johnson on-call Tuesday evening  
**When**: View availability for that time  
**Then**: Time slots blocked (unavailable)

**Expected Results:**
- [ ] On-call blocks entire time slot grid
- [ ] On-call slots return 0 availability
- [ ] Cannot book on-call providers
- [ ] Staff can still view (for records)

---

### Provider Availability Service Architecture
```csharp
public class ProviderAvailability
{
    public Guid ProviderId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    
    public TimeSpan WorkStartTime { get; set; }      // 9:00 AM
    public TimeSpan WorkEndTime { get; set; }        // 5:00 PM
    public TimeSpan LunchStartTime { get; set; }     // 12:00 PM
    public TimeSpan LunchEndTime { get; set; }       // 1:00 PM
    public int BufferMinutes { get; set; }           // 15 min
    public int AppointmentDurationMinutes { get; set; } // 30 min
    
    public bool IsOnCall { get; set; }
    public bool IsClosed { get; set; }               // Holiday/clinic closure
}

// Calculate available slots
public List<TimeSlot> GetAvailableSlots(
    Guid providerId, 
    DateTime date,
    int appointmentDurationMinutes)
{
    // 1. Load provider availability rules for day of week
    // 2. Check for holiday/clinic closure
    // 3. Load existing appointments for provider on date
    // 4. Generate time slots respecting:
    //    - Working hours boundaries
    //    - Lunch break
    //    - Buffer time between appointments
    //    - Appointment duration
    // 5. Filter out booked/unavailable slots
    // 6. Return available TimeSlot collection
}
```

---

## 2. US_024: Available Time Slot Display

### Test Objectives
- Verify time slots display correctly for selected date
- Test real-time availability updates
- Confirm slot filtering by appointment type
- Validate slot selection UI
- Test availability caching with TTL

### Test Cases

#### TC-US-024-HP-01: Time Slot Grid Display
| Field | Value |
|-------|-------|
| Requirement | FR-011 |
| Type | happy_path |
| Priority | P0 |

**Given**: Provider and date selected (e.g., "Dr. Sarah", March 24, 2026)  
**When**: View time slots for date  
**Then**: Available slots displayed in 30-minute increments

**Slot Display:**
```
Available Slots for Dr. Sarah Johnson - Tuesday, March 24
┌─────────────────┬─────────────────┐
│  9:00 AM - 9:30 │  9:30 AM - 10:00│
│  (Available)    │  (Available)    │
├─────────────────┼─────────────────┤
│ 10:00 AM - 10:30│ 10:30 AM - 11:00│
│  (Available)    │  (Available)    │
├─────────────────┼─────────────────┤
│ 12:00 PM - 12:30│ 1:00 PM - 1:30  │ ← Lunch skipped
│  (Lunch Break)  │  (Available)    │
```

**Expected Results:**
- [ ] Slots displayed in 30-minute increments
- [ ] Lunch break slots removed (not grayed out)
- [ ] 20+ available slots per day typical
- [ ] Green background (available slots)
- [ ] Gray background (booked slots)
- [ ] Clicked slot highlights in blue
- [ ] Hover shows provider name and duration
- [ ] Responsive layout (mobile: stacked, desktop: grid)

---

#### TC-US-024-HP-02: Real-Time Availability Sync
| Field | Value |
|-------|-------|
| Requirement | FR-011, NFR-004 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient viewing available slots  
**When**: Another patient books the last available slot  
**Then**: Slot updates to "Booked" within 2 seconds

**Expected Results:**
- [ ] WebSocket connection established (Pusher Channels)
- [ ] Real-time update received within 2 seconds
- [ ] Slot changes from green to gray
- [ ] UI updates without page refresh
- [ ] Notification shown if slot was selected

---

#### TC-US-024-HP-03: Filter by Appointment Type
| Field | Value |
|-------|-------|
| Requirement | FR-011 |
| Type | happy_path |
| Priority | P1 |

**Given**: Multiple appointment types available  
**When**: Filter by "Initial Consult" (60 min)  
**Then**: Only 60-minute slots displayed

**Appointment Types:**
```yaml
appointment_types:
  - type: "Telehealth Consult"
    duration: 30
  - type: "Initial Consult"
    duration: 60
  - type: "Follow-up"
    duration: 30
  - type: "Lab Review"
    duration: 30
```

**Expected Results:**
- [ ] Appointment type dropdown displayed
- [ ] Selected type filters slot duration
- [ ] "Initial Consult" shows only 60-min slots
- [ ] "Follow-up" shows only 30-min slots
- [ ] Filter applies instantly
- [ ] Provider availability adjusted per duration

---

#### TC-US-024-HP-04: Slot Availability Caching
| Field | Value |
|-------|-------|
| Requirement | NFR-004 |
| Type | happy_path |
| Priority | P1 |

**Given**: Availability data loaded from database  
**When**: Multiple patients view same provider/date  
**Then**: Cached in Redis with 5-minute TTL

**Expected Results:**
- [ ] First load queries database (may take 500ms)
- [ ] Subsequent loads from Redis (<100ms)
- [ ] Cache key: `availability:{provider_id}:{date}`
- [ ] TTL: 5 minutes (invalidated by new bookings)
- [ ] Real-time update pushes invalidate cache immediately

---

#### TC-US-024-ER-01: No Available Slots
| Field | Value |
|-------|-------|
| Requirement | FR-011 |
| Type | error |
| Priority | P1 |

**Given**: Provider fully booked for selected date  
**When**: View time slots  
**Then**: Message displayed with next available date

**Expected Results:**
- [ ] Empty slot grid shown
- [ ] Message: "No available slots on this date"
- [ ] "View next available" button offered
- [ ] Link to next date with availability

---

### Time Slot Service Architecture
```csharp
public class TimeSlot
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string AppointmentType { get; set; }
    
    public int DurationMinutes => 
        (int)(EndTime - StartTime).TotalMinutes;
}

// Availability Query with Caching
public async Task<List<TimeSlot>> GetAvailableSlotsAsync(
    Guid providerId,
    DateTime date,
    string appointmentType)
{
    var cacheKey = $"availability:{providerId}:{date:yyyy-MM-dd}:{appointmentType}";
    
    // 1. Check Redis cache
    var cached = await _cache.GetAsync(cacheKey);
    if (cached != null) return JsonConvert.DeserializeObject<List<TimeSlot>>(cached);
    
    // 2. Query database for available slots
    var slots = await _dbContext.TimeSlots
        .Where(s => s.ProviderId == providerId && 
                    s.StartTime.Date == date.Date &&
                    s.IsAvailable &&
                    s.AppointmentType == appointmentType)
        .ToListAsync();
    
    // 3. Cache with 5-minute TTL
    await _cache.SetAsync(cacheKey, JsonConvert.SerializeObject(slots),
        TimeSpan.FromMinutes(5));
    
    return slots;
}
```

---

## 3. US_025: Appointment Booking

### Test Objectives
- Verify appointment creation with proper status
- Confirm confirmation email sent immediately
- Test unique appointment ID generation
- Validate patient cannot double-book same slot
- Test transaction rollback on error

### Test Cases

#### TC-US-025-HP-01: Successful Appointment Booking
| Field | Value |
|-------|-------|
| Requirement | FR-012 |
| Type | happy_path |
| Priority | P0 |

**Given**: Available time slot selected  
**When**: Patient submits booking form  
**Then**: Appointment created with status "Scheduled"

**Booking Form:**
```yaml
booking_form:
  appointment_type: "Follow-up"
  provider: "Dr. Sarah Johnson"
  date: "2026-03-24"
  time: "2:00 PM - 2:30 PM"
  reason: "Check blood pressure follow-up"
  telehealth: false
```

**Expected Results:**
- [ ] Appointment record created in database
- [ ] Status set to "Scheduled"
- [ ] Unique appointment ID generated (UUID)
- [ ] Patient ID linked correctly
- [ ] Provider ID linked correctly
- [ ] Slot marked as unavailable immediately
- [ ] Confirmation email sent within 30 seconds
- [ ] Appointment confirmation number displayed to patient
- [ ] Calendar updated in real-time (Pusher notification)
- [ ] Patient redirected to confirmation page

---

#### TC-US-025-HP-02: Appointment Confirmation Email
| Field | Value |
|-------|-------|
| Requirement | FR-013 |
| Type | happy_path |
| Priority | P0 |

**Given**: Appointment successfully booked  
**When**: Confirmation email sent  
**Then**: Email contains required details

**Email Content:**
```
Subject: Appointment Confirmation - Dr. Sarah Johnson

Dear [Patient Name],

Your appointment has been confirmed!

Appointment Details:
  - Date: Tuesday, March 24, 2026
  - Time: 2:00 PM - 2:30 PM
  - Provider: Dr. Sarah Johnson
  - Location: 123 Health Clinic, Suite 100
  - Confirmation #: APT-20260324-0001
  - Type: Follow-up
  - Duration: 30 minutes

Actions:
  [Reschedule Appointment]  [Cancel Appointment]  [Add to Calendar]

Questions? Contact us at 1-800-CLINIC or support@clinic.com

Regards,
Health Clinic
```

**Expected Results:**
- [ ] Email sent to patient address within 30 seconds
- [ ] Date/time formatted clearly
- [ ] Confirmation number unique and memorable
- [ ] Provider name and location included
- [ ] Reschedule/cancel links functional
- [ ] Clinic contact information present
- [ ] Plain text and HTML versions included
- [ ] No PHI in email headers/metadata

---

#### TC-US-025-ER-01: Double-Booking Prevention
| Field | Value |
|-------|-------|
| Requirement | DR-007 |
| Type | error |
| Priority | P0 |

**Given**: Patient already has appointment at 2:00 PM  
**When**: Attempt to book same time slot  
**Then**: Error message; booking prevented

**Expected Results:**
- [ ] Unique constraint: (patient_id, start_time) prevents double-book
- [ ] Error: "You already have an appointment at this time"
- [ ] Transaction rolled back
- [ ] No partial records created
- [ ] Slot remains available for other patients
- [ ] User redirected to calendar view

---

#### TC-US-025-ER-02: Slot Expiration During Booking
| Field | Value |
|-------|-------|
| Requirement | FR-012 |
| Type | error |
| Priority | P1 |

**Given**: Patient booking slot that another patient just booked  
**When**: Submit booking form  
**Then**: Optimistic lock prevents double-booking

**Expected Results:**
- [ ] Second booking request fails
- [ ] Error: "This slot was just booked. Please select another."
- [ ] Transaction rolled back
- [ ] Calendar refreshed with updated availability
- [ ] Availability cache invalidated immediately

---

#### TC-US-025-ER-03: Invalid Appointment Duration
| Field | Value |
|-------|-------|
| Requirement | FR-012 |
| Type | error |
| Priority | P1 |

**Given**: Form submitted with mismatched duration  
**When**: Appointment type requires 60 min, slot is 30 min  
**Then**: Validation error; booking rejected

---

### Appointment Service Transaction Pattern
```csharp
public async Task<AppointmentDto> BookAppointmentAsync(
    Guid patientId,
    BookAppointmentRequestDto request)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    
    try
    {
        // 1. Verify slot still available (SELECT FOR UPDATE)
        var slot = await _dbContext.TimeSlots
            .FirstAsync(s => s.Id == request.SlotId && s.IsAvailable);
        
        // 2. Check for patient double-booking at same time
        var existingAppointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(a => 
                a.PatientId == patientId && 
                a.StartTime == slot.StartTime);
        if (existingAppointment != null)
            throw new AppointmentConflictException();
        
        // 3. Create appointment
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            ProviderId = slot.ProviderId,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Appointments.Add(appointment);
        
        // 4. Mark slot unavailable
        slot.IsAvailable = false;
        _dbContext.TimeSlots.Update(slot);
        
        // 5. Save and commit
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        // 6. Send confirmation email (outside transaction)
        await _emailService.SendConfirmationAsync(appointment);
        
        // 7. Invalidate cache and notify via WebSocket
        await _cache.RemoveAsync($"availability:{slot.ProviderId}:{slot.StartTime.Date}");
        await _pusherService.TriggerAsync("availability-updates", slot.ProviderId);
        
        return _mapper.Map<AppointmentDto>(appointment);
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## 4. US_026: Preferred Appointment Slot Swap

### Test Objectives
- Verify patient can request swap to preferred slot
- Test swap acceptance/rejection workflow
- Confirm provider approval required
- Ensure no double-booking during swap
- Validate appointment history preservation

### Test Cases

#### TC-US-026-HP-01: Request Appointment Swap
| Field | Value |
|-------|-------|
| Requirement | FR-024 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient with booked appointment at 2:00 PM  
**When**: View calendar and select preferred time (3:00 PM)  
**Then**: Swap request created and sent to provider

**Expected Results:**
- [ ] Swap request record created
- [ ] Current appointment status: "Swap Requested"
- [ ] New slot status: "Pending Swap Approval"
- [ ] Request ID generated
- [ ] Timestamp recorded
- [ ] Patient email sent confirming request
- [ ] Provider notified of pending swap

---

#### TC-US-026-HP-02: Provider Approves Swap
| Field | Value |
|-------|-------|
| Requirement | FR-024 |
| Type | happy_path |
| Priority | P1 |

**Given**: Provider reviews pending swap request  
**When**: Provider approves swap  
**Then**: Appointments updated atomically

**Expected Results:**
- [ ] Original appointment cancelled
- [ ] New appointment confirmed
- [ ] Both patients notified (original and new)
- [ ] Confirmation emails sent
- [ ] Swap history preserved
- [ ] Status: "Completed"
- [ ] Audit log entry created

---

#### TC-US-026-ER-01: Provider Rejects Swap
| Field | Value |
|-------|-------|
| Requirement | FR-024 |
| Type | error |
| Priority | P1 |

**Given**: Provider reviews swap request  
**When**: Provider rejects swap  
**Then**: Original appointment restored

**Expected Results:**
- [ ] Swap request status: "Rejected"
- [ ] Original appointment status: "Scheduled" (unchanged)
- [ ] Preferred slot released
- [ ] Patient notified of rejection
- [ ] Optional: Rejection reason message
- [ ] Patient can request swap again

---

#### TC-US-026-ER-02: Preferred Slot Becomes Unavailable
| Field | Value |
|-------|-------|
| Requirement | FR-024 |
| Type | error |
| Priority | P1 |

**Given**: Swap approved, but preferred slot booked by another patient  
**When**: Attempt to confirm swap  
**Then**: Swap fails gracefully

**Expected Results:**
- [ ] Swap cancellation with explanation
- [ ] Original appointment remains intact
- [ ] Patient offered alternative slots
- [ ] Notification sent to all parties
- [ ] No orphaned records

---

---

## 5. US_027: Appointment Confirmation Email & SMS

### Test Objectives
- Verify multi-channel confirmations (email + SMS)
- Test appointment reminder emails
- Confirm cancellation notifications
- Validate personalization and clarity
- Test error handling for failed sends

### Test Cases

#### TC-US-027-HP-01: Immediate Confirmation Email
| Field | Value |
|-------|-------|
| Requirement | FR-013 |
| Type | happy_path |
| Priority | P0 |

**Given**: Appointment booked successfully  
**When**: Confirmation email triggered  
**Then**: Email sent within 30 seconds

**Email Validation:**
- [ ] Recipient: patient email address (no CC/BCC)
- [ ] From: no-reply@clinic.com (DMARC compliant)
- [ ] Subject: Clear and scannable
- [ ] Body: Patient name, provider name, date, time, location
- [ ] Links: Functional (reschedule, cancel, add-to-calendar)
- [ ] HTML: Responsive design (mobile-optimized)
- [ ] Text: Plain text alternative included
- [ ] No PHI in headers/metadata
- [ ] Unsubscribe link: Optional (already enrolled)

---

#### TC-US-027-HP-02: 24-Hour Reminder Email
| Field | Value |
|-------|-------|
| Requirement | FR-013 |
| Type | happy_path |
| Priority | P1 |

**Given**: Appointment scheduled  
**When**: 24 hours before appointment  
**Then**: Reminder email sent

**Reminder Email:**
```
Subject: Reminder: Your Appointment with Dr. Sarah Tomorrow at 2:00 PM

Dear John,

This is a friendly reminder that you have an appointment tomorrow.

Date: Tuesday, March 25, 2026
Time: 2:00 PM - 2:30 PM
Provider: Dr. Sarah Johnson
Location: 123 Health Clinic, Suite 100

[Confirm Attendance]  [Reschedule]  [Cancel]

Arrive 10 minutes early.
```

**Expected Results:**
- [ ] Sent exactly 24 hours before start time
- [ ] Only if appointment status is "Scheduled"
- [ ] Not sent if already cancelled
- [ ] Patient name personalized
- [ ] No-show risk addressed in tone
- [ ] One-click confirmation link for attendance

---

#### TC-US-027-HP-03: SMS Confirmation (If Opted-In)
| Field | Value |
|-------|-------|
| Requirement | FR-013 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient opted into SMS notifications  
**When**: Appointment booked  
**Then**: SMS sent to phone number

**SMS Content:**
```
Hi John! Your appointment with Dr. Sarah is confirmed for 
Tue 3/25 at 2:00 PM. Arrive 10 min early. 
Reply CONFIRM or RESCHEDULE: https://clinic.link/apt-001
```

**Expected Results:**
- [ ] Sent via Twilio within 60 seconds
- [ ] Patient phone number verified
- [ ] SMS contains key details (date, time, provider)
- [ ] Short URL for mobile usability
- [ ] Error handling if SMS fails (fallback to email)
- [ ] Audit log of SMS send status

---

#### TC-US-027-ER-01: Email Delivery Failure
| Field | Value |
|-------|-------|
| Requirement | FR-013 |
| Type | error |
| Priority | P1 |

**Given**: Email send fails (e.g., bounced)  
**When**: Retry mechanism triggered  
**Then**: Retries with exponential backoff

**Expected Results:**
- [ ] First attempt immediate
- [ ] Retry 1: 5 minutes later
- [ ] Retry 2: 30 minutes later
- [ ] Retry 3: 4 hours later
- [ ] If all fail: Audit log + Staff alert
- [ ] Max 3 retries (configurable)
- [ ] No infinite loops

---

#### TC-US-027-HP-04: Cancellation Notification
| Field | Value |
|-------|-------|
| Requirement | FR-015 |
| Type | happy_path |
| Priority | P1 |

**Given**: Appointment cancelled  
**When**: Patient initiates cancellation  
**Then**: Cancellation email sent to patient and provider

**Email Content:**
```
Subject: Appointment Cancellation Confirmation

Your appointment with Dr. Sarah Johnson on Tuesday, March 25 
at 2:00 PM has been cancelled.

To reschedule, click here: [Link]

If you cancelled in error, contact us immediately.
```

**Expected Results:**
- [ ] Email sent immediately
- [ ] Sent to patient and provider
- [ ] Appointment marked "Cancelled"
- [ ] Reason recorded if provided
- [ ] Reschedule link provided
- [ ] No charge if cancelled >24 hours before

---

### Email Service Architecture
```csharp
public enum AppointmentEmailType
{
    Confirmation,
    ReminderDay,
    ReminderHour,
    Cancellation,
    RescheduleRequest,
    NoShowWarning
}

public class AppointmentEmailService
{
    public async Task SendConfirmationAsync(Appointment appointment)
    {
        var patient = await _dbContext.Patients.FindAsync(appointment.PatientId);
        var provider = await _dbContext.Providers.FindAsync(appointment.ProviderId);
        
        var emailTemplate = await _templateService.GetAsync("appointment-confirmation");
        var body = emailTemplate.Render(new {
            PatientName = patient.FullName,
            ProviderName = provider.FullName,
            Date = appointment.StartTime.ToString("dddd, MMMM d, yyyy"),
            Time = appointment.StartTime.ToString("h:mm tt"),
            ConfirmationNumber = appointment.ConfirmationNumber
        });
        
        var email = new Email
        {
            To = patient.Email,
            Subject = $"Appointment Confirmation - {provider.FullName}",
            Body = body,
            Type = AppointmentEmailType.Confirmation,
            ReferenceId = appointment.Id
        };
        
        await _emailService.SendAsync(email);
    }
    
    // Scheduled job for reminders
    [RecurringJob("appointment-reminders", "0 */6 * * *")]  // Every 6 hours
    public async Task SendRemindersAsync()
    {
        // Find appointments in next 24-25 hours
        var appointments = await _dbContext.Appointments
            .Where(a => 
                a.StartTime > DateTime.UtcNow.AddHours(24) &&
                a.StartTime < DateTime.UtcNow.AddHours(25) &&
                a.Status == AppointmentStatus.Scheduled &&
                a.ReminderSentAt == null)
            .ToListAsync();
        
        foreach (var appointment in appointments)
        {
            await SendReminderAsync(appointment);
        }
    }
}
```

---

## 6. US_028: Google Calendar & Outlook Sync

### Test Objectives
- Verify OAuth connection for Google Calendar
- Test appointment sync to external calendars
- Confirm bidirectional sync capability
- Test cancellation sync
- Validate conflict detection

### Test Cases

#### TC-US-028-HP-01: Google Calendar OAuth Initialization
| Field | Value |
|-------|-------|
| Requirement | FR-024, FR-025 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient on settings page  
**When**: Click "Connect Google Calendar"  
**Then**: OAuth flow initiated

**OAuth Flow:**
```
1. Patient clicks "Connect Google Calendar"
2. Redirect to Google OAuth consent screen
3. Patient approves clinic.com to access calendar
4. Google redirects back with authorization code
5. Backend exchanges code for access token
6. Access token stored securely
7. Calendar connected message shown
```

**Expected Results:**
- [ ] OAuth redirect works
- [ ] Consent screen shows appropriate permissions
- [ ] Authorization code exchanged for token
- [ ] Refresh token stored (for long-term access)
- [ ] Access token stored in encrypted form
- [ ] Confirmation message shown
- [ ] Settings page shows "Connected" status

---

#### TC-US-028-HP-02: Appointment Synced to Google Calendar
| Field | Value |
|-------|-------|
| Requirement | FR-024 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient with connected Google Calendar  
**When**: Appointment booked  
**Then**: Event created in patient's Google Calendar

**Calendar Event Details:**
```
Title: Appointment with Dr. Sarah Johnson
Date: Tuesday, March 25, 2026
Time: 2:00 PM - 2:30 PM
Location: 123 Health Clinic, Suite 100
Description: Follow-up appointment

Calendar: Primary
Visibility: Owner only (private)
Reminders: 10 minutes, 1 day
```

**Expected Results:**
- [ ] Event created in Google Calendar
- [ ] Title includes provider name
- [ ] Date/time correct
- [ ] Location address included
- [ ] Appears in primary calendar
- [ ] Private visibility (not shared)
- [ ] Sync happens within 5 minutes of booking

---

#### TC-US-028-HP-03: Appointment Cancellation Synced
| Field | Value |
|-------|-------|
| Requirement | FR-024 |
| Type | happy_path |
| Priority | P1 |

**Given**: Appointment synced to Google Calendar  
**When**: Patient cancels appointment  
**Then**: Calendar event deleted

**Expected Results:**
- [ ] Event removed from Google Calendar
- [ ] Removal happens within 5 minutes
- [ ] User sees "Calendar updated" confirmation
- [ ] If sync fails: Audit log + retry mechanism

---

#### TC-US-028-HP-04: Outlook Calendar Sync
| Field | Value |
|-------|-------|
| Requirement | FR-025 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient with Outlook/Office 365 account  
**When**: Click "Connect Outlook Calendar"  
**Then**: OAuth flow for Microsoft Graph initiated

**Expected Results:**
- [ ] Microsoft OAuth consent shown
- [ ] Access token obtained
- [ ] Refresh token stored
- [ ] Appointments synced to Outlook calendar
- [ ] Same event details as Google
- [ ] Bidirectional sync works (if available)

---

#### TC-US-028-ER-01: Calendar Sync Failure
| Field | Value |
|-------|-------|
| Requirement | FR-024, FR-025 |
| Type | error |
| Priority | P1 |

**Given**: Calendar sync API returns error  
**When**: Appointment synced  
**Then**: Graceful error handling

**Error Scenarios:**
- Google Calendar API quota exceeded
- Network timeout during sync
- Access token revoked

**Expected Results:**
- [ ] Clinic appointment still created successfully
- [ ] Sync failure logged to audit trail
- [ ] Retry mechanism triggered (exponential backoff)
- [ ] Staff alert if persistent failure
- [ ] No orphaned state
- [ ] User notified of sync failure (optional)

---

#### TC-US-028-ER-02: Access Token Expiration
| Field | Value |
|-------|-------|
| Requirement | FR-024 |
| Type | error |
| Priority | P1 |

**Given**: Google Calendar access token expired  
**When**: Attempt to sync appointment  
**Then**: Token refreshed automatically

**Expected Results:**
- [ ] Refresh token used to get new access token
- [ ] Sync retried with new token
- [ ] Transparent to user
- [ ] If refresh fails: "Reconnect your calendar" prompt
- [ ] No manual intervention required first time

---

### Calendar Sync Service Architecture
```csharp
public interface ICalendarProvider
{
    Task<AuthorizationResult> AuthorizeAsync(string authCode);
    Task<CalendarEvent> CreateEventAsync(Appointment appointment);
    Task DeleteEventAsync(string externalEventId);
    Task UpdateEventAsync(string externalEventId, Appointment appointment);
}

public class GoogleCalendarService : ICalendarProvider
{
    public async Task<CalendarEvent> CreateEventAsync(Appointment appointment)
    {
        var client = await GetAuthorizedClientAsync();
        
        var googleEvent = new Event
        {
            Summary = $"Appointment with {appointment.Provider.FullName}",
            Location = appointment.Provider.Clinic.Address,
            Description = appointment.AppointmentType,
            Start = new EventDateTime { DateTime = appointment.StartTime },
            End = new EventDateTime { DateTime = appointment.EndTime },
            Visibility = "private",
            Reminders = new Event.RemindersData
            {
                UseDefault = false,
                Items = new[] {
                    new EventReminder { Method = "popup", Minutes = 10 },
                    new EventReminder { Method = "email", Minutes = 1440 }
                }
            }
        };
        
        var request = client.Events.Insert(googleEvent, "primary");
        var created = await request.ExecuteAsync();
        
        return new CalendarEvent { ExternalId = created.Id };
    }
}

// Scheduled sync job
[RecurringJob("calendar-sync", "*/5 * * * *")]  // Every 5 minutes
public async Task SyncPendingAppointmentsAsync()
{
    var pending = await _dbContext.CalendarSyncQueue
        .Where(x => !x.SyncedAt.HasValue)
        .ToListAsync();
    
    foreach (var sync in pending)
    {
        try
        {
            var provider = _calendarProviderFactory.Create(sync.ProviderType);
            await provider.CreateEventAsync(sync.Appointment);
            sync.SyncedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            sync.FailureCount++;
            sync.LastError = ex.Message;
            if (sync.FailureCount >= 3)
                sync.Status = SyncStatus.Failed; // Requires manual intervention
        }
    }
    
    await _dbContext.SaveChangesAsync();
}
```

---

## Test Execution Strategy

### Execution Sequence
1. **US_023** (Availability): Foundation for all booking features
2. **US_024** (Time Slots): Depends on US_023
3. **US_025** (Booking): Depends on US_024
4. **US_026** (Swap): Depends on US_025
5. **US_027** (Email): Can run in parallel with US_025-026 (email validation)
6. **US_028** (Calendar Sync): Depends on US_025 (appointment creation)

### P0 Critical Path
```
US_023 → US_024 → US_025 → US_028
         (Foundation → Display → Create → Sync)
```

### User Journey E2E Test
```
1. Patient logs in
2. Searches for provider "Dr. Sarah Johnson"
3. Views 7-day availability calendar
4. Selects time slot (2:00 PM Tuesday, March 24)
5. Submits booking form
6. Receives confirmation email
7. Appointment synced to Google Calendar
8. Patient receives 24-hour reminder email
9. Staff sees appointment in queue
10. Patient arrives 10 minutes early
```

---

## Security Considerations

### Calendar Integration Security
- **OAuth Scope Limitation**: Request only calendar.events scope (not full profile access)
- **Token Storage**: Encrypted in database, never logged
- **Refresh Token Rotation**: Implement token renewal per OAuth best practices
- **Event Privacy**: Appointments marked private (not shared publicly)
- **Scope Escalation**: Patient cannot sync other users' calendars

### Data Minimization
- **External Calendar**: Sync appointment time, provider name, location only
- **No PHI Export**: Do not sync medical notes or conditions to calendar
- **Third-party Risk**: Monitor Google/Microsoft API changes
- **Rate Limiting**: Respect API quotas; handle 429 responses gracefully

---

## Success Criteria

- [ ] Complete appointment booking workflow tested
- [ ] 100% happy path user journey covered
- [ ] Real-time availability sync working (<2 sec)
- [ ] Email confirmations sent and validated
- [ ] SMS notifications optional and working
- [ ] Google Calendar sync functional
- [ ] Outlook Calendar sync functional
- [ ] Error handling and retry logic validated
- [ ] No double-booking vulnerabilities
- [ ] OWASP Top 10 mitigations verified (A04: Insecure Design)
- [ ] Calendar token security validated

---

## Sign-Off

**Status**: ✅ **READY FOR IMPLEMENTATION**  
**Scope**: EP-002 patient appointment booking workflow  
**Coverage**: 6 user stories, 32+ test cases  
**User Impact**: HIGH (primary revenue driver)  
**Completion Target**: After EP-001 (authentication)
