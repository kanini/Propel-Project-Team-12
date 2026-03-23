---
id: test_plan_us_029_032
title: Test Plan - EP-003 Staff Appointment Management (US_029-032)
version: 1.0.0
status: draft
author: AI Assistant
created: 2026-03-23
scope: "EP-003 staff workflows, walk-in booking, queue management, check-in, performance dashboards"
---

# Test Plan: EP-003 Staff Appointment Management (US_029-032)

## Overview

This test plan covers **4 operational efficiency user stories** enabling clinic staff to manage same-day appointments, handle walk-in patients, track queue status, and monitor performance metrics. These stories optimize clinical workflow and reduce wait times.

**User Stories Covered:**
- US_029: Staff Walk-in Booking
- US_030: Same-Day Queue Management
- US_031: Arrival Check-in System
- US_032: Staff Performance Dashboard

---

## 1. US_029: Staff Walk-in Booking

### Test Objectives
- Verify staff can create walk-in (unscheduled) appointments
- Test immediate availability checks
- Confirm patient entry creation
- Validate same-day appointment scheduling
- Test conflict detection

### Test Cases

#### TC-US-029-HP-01: Create Walk-in Appointment
| Field | Value |
|-------|-------|
| Requirement | FR-014 |
| Type | happy_path |
| Priority | P0 |

**Given**: Staff at front desk with available provider slot
**When**: Staff submits walk-in form with patient details
**Then**: Walk-in appointment created with status "Arrived" (walk-in typically seen immediately)

**Walk-in Form:**
```yaml
walk_in_appointment:
  patient_name: "Jane Doe"
  date_of_birth: "1985-03-20"
  phone: "+1-555-0105"
  provider_id: "dr-sarah-001"
  reason: "Acute chest pain"
  appointment_type: "Urgent Care"
  source: "walk_in"
```

**Expected Results:**
- [ ] Appointment created with status "Arrived"
- [ ] No confirmation email sent (walk-in, immediate)
- [ ] Added to queue immediately
- [ ] Unique confirmation number generated
- [ ] Timestamp recorded
- [ ] Staff member ID recorded (who created)
- [ ] Walking-in flag set to true
- [ ] No-show risk assessment triggered

---

#### TC-US-029-HP-02: Walk-in with Existing Patient Record
| Field | Value |
|-------|-------|
| Requirement | FR-014 |
| Type | happy_path |
| Priority | P0 |

**Given**: Walk-in patient with existing medical record
**When**: Search for patient and create walk-in
**Then**: Existing record linked; no duplicate patient created

**Expected Results:**
- [ ] Search finds patient by name or phone
- [ ] Existing record loaded
- [ ] Walk-in logged to patient appointment history
- [ ] Medical records accessible to provider
- [ ] No duplicate patient records created
- [ ] Previous medications/allergies visible

---

#### TC-US-029-ER-01: No Available Provider
| Field | Value |
|-------|-------|
| Requirement | FR-014 |
| Type | error |
| Priority | P0 |

**Given**: All providers fully booked or unavailable
**When**: Attempt to create walk-in
**Then**: Error message; queue to next available

**Expected Results:**
- [ ] Error: "No providers available. Patient added to queue for next available."
- [ ] Patient queued for next available slot or provider
- [ ] Notification sent to staff
- [ ] Estimated wait time provided
- [ ] Patient informed of wait

---

#### TC-US-029-ER-02: Invalid Patient Information
| Field | Value |
|-------|-------|
| Requirement | FR-014 |
| Type | error |
| Priority | P1 |

**Given**: Form submitted with missing/invalid data
**When**: Staff skips required fields
**Then**: Validation errors shown

**Required Walk-in Fields:**
- Patient name
- Date of birth (valid date)
- At least phone OR email
- Reason for visit
- Provider or specialty selection

---

#### TC-US-029-HP-03: Walk-in Urgent Care Flag
| Field | Value |
|-------|-------|
| Requirement | FR-014 |
| Type | happy_path |
| Priority | P1 |

**Given**: Walk-in marked as "Urgent Care"
**When**: Appointment created
**Then**: Prioritized in queue; provider alerted

**Expected Results:**
- [ ] Urgent flag set in database
- [ ] Queue position adjusted (move to front or near-front)
- [ ] Visual indicator (red/high priority) in UI
- [ ] Provider notification sent
- [ ] Estimated wait time reduced
- [ ] Documentation requirement: reason for urgency

---

### Walk-in Service Architecture
```csharp
public class WalkInAppointmentService
{
    public async Task<AppointmentDto> CreateWalkInAsync(
        CreateWalkInRequestDto request,
        Guid staffMemberId)
    {
        // 1. Find or create patient record
        var patient = await _patientService.FindOrCreateAsync(request);
        
        // 2. Find next available provider/slot
        var nextAvailable = await _appointmentService
            .GetNextAvailableSlotAsync(
                request.ProviderIdOrSpecialty,
                request.AppointmentType);
        
        if (nextAvailable == null)
            throw new NoProviderAvailableException();
        
        // 3. Create appointment with status "Arrived"
        var appointment = new Appointment
        {
            PatientId = patient.Id,
            ProviderId = nextAvailable.ProviderId,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(30),
            Status = AppointmentStatus.Arrived,  // Walk-in is immediately "Arrived"
            IsWalkIn = true,
            UrgencyLevel = request.UrgencyLevel ?? 0,
            CreatedByStaffId = staffMemberId,
            CreatedAt = DateTime.UtcNow
        };
        
        // 4. Add to queue
        await _queueService.AddToQueueAsync(appointment);
        
        // 5. Audit log
        await _auditService.LogAsync(
            "CREATE_WALK_IN",
            patient.Id,
            details: new { UrgencyLevel = request.UrgencyLevel });
        
        return _mapper.Map<AppointmentDto>(appointment);
    }
}
```

---

## 2. US_030: Same-Day Queue Management

### Test Objectives
- Verify queue displays all same-day appointments
- Test real-time queue status updates
- Confirm appointment status transitions
- Test queue position management
- Validate average wait time calculations

### Test Cases

#### TC-US-030-HP-01: View Same-Day Queue
| Field | Value |
|-------|-------|
| Requirement | FR-015 |
| Type | happy_path |
| Priority | P0 |

**Given**: Multiple appointments scheduled for today
**When**: Staff member opens queue view
**Then**: All same-day appointments displayed with status and wait time

**Queue Display:**
```
TODAY'S QUEUE - Dr. Sarah Johnson
────────────────────────────────────────
Position │ Patient      │ Time    │ Status    │ Type
────────────────────────────────────────
1        │ John Smith   │ 2:00 PM │ Arrived   │ Follow-up (🔴 Waiting 15 min)
2        │ Jane Doe     │ 2:30 PM │ Ready     │ Intake (🟡 Next - 10 min wait)
3        │ Bob Johnson  │ 3:00 PM │ Scheduled │ Urgent (🔴 Urgent)
4        │ Carol White  │ 3:30 PM │ Scheduled │ Follow-up (⚫ 45 min wait)
```

**Expected Results:**
- [ ] Queue sorted by appointment time
- [ ] Urgent appointments highlighted
- [ ] Wait times calculated correctly
- [ ] Status color-coded (red: waiting, yellow: next, blue: scheduled)
- [ ] Patient names visible (considering privacy)
- [ ] Provider filter shows only their queue
- [ ] Real-time updates as status changes
- [ ] Scroll if >10 appointments

---

#### TC-US-030-HP-02: Update Appointment Status in Queue
| Field | Value |
|-------|-------|
| Requirement | FR-015 |
| Type | happy_path |
| Priority | P0 |

**Given**: Appointment in "Arrived" status
**When**: Staff marks "Being Seen" or "Complete"
**Then**: Status updated immediately; next patient alerted

**Status Transitions:**
```
Scheduled → Arrived → Being Seen → Complete
        ↓           ↓              ↓
      (walk-in)  (check-in)    (discharge)
        
No Show / Cancelled / Did Not Arrive
```

**Expected Results:**
- [ ] Status dropdown available for each appointment
- [ ] Status change validated (no backwards transitions)
- [ ] Queue refreshes immediately
- [ ] Next patient marked "Next" (visual prominence)
- [ ] Staff member ID recorded with status change
- [ ] Timestamp of each transition recorded
- [ ] Real-time notification sent to affected providers
- [ ] Audit log entry created

---

#### TC-US-030-HP-03: Average Wait Time Calculation
| Field | Value |
|-------|-------|
| Requirement | FR-015, NFR-002 |
| Type | happy_path |
| Priority | P1 |

**Given**: Multiple appointments completed during day
**When**: Calculate average wait time
**Then**: Metric updated for performance dashboard

**Calculation:**
```
Wait Time = (Arrival Time - Scheduled Time) 
           OR
           (Scheduled Time - Now) for future appointments

Example:
  John: Scheduled 2:00 PM, Arrived 2:10 PM = 10 min wait
  Jane: Scheduled 2:30 PM, Arrived 2:35 PM = 5 min wait
  Bob: Scheduled 3:00 PM, Arrived 3:00 PM = 0 min wait
  
Average = (10 + 5 + 0) / 3 = 5 minutes
```

**Expected Results:**
- [ ] Wait time calculated per appointment
- [ ] Average calculated for provider daily
- [ ] Average calculated for clinic daily
- [ ] 0 wait time if arrived early
- [ ] Tracked for performance dashboard
- [ ] No-show not included in average

---

#### TC-US-030-HP-04: Priority Queue Reordering
| Field | Value |
|-------|-------|
| Requirement | FR-015 |
| Type | happy_path |
| Priority | P1 |

**Given**: New urgent walk-in added to queue
**When**: Urgent appointment created
**Then**: Queue reordered; urgent moves near front

**Expected Results:**
- [ ] Urgent appointments prioritized (move up in queue)
- [ ] Non-urgent after current patient
- [ ] Visual indicator distinguishes urgent
- [ ] Staff can manually reorder if necessary (with audit)
- [ ] Reorder triggers notification to affected patients

---

#### TC-US-030-ER-01: Queue Sync Issues
| Field | Value |
|-------|-------|
| Requirement | FR-015, NFR-005 |
| Type | error |
| Priority | P1 |

**Given**: Network disconnection while updating queue
**When**: Staff marks appointment status
**Then**: Local cache updated; sync on reconnect

**Expected Results:**
- [ ] Optimistic UI update (status changes immediately)
- [ ] Background sync with server
- [ ] Conflict resolution if other staff makes change
- [ ] User alerted if sync fails
- [ ] Retry mechanism automatic

---

### Queue Service Architecture
```csharp
public class QueueService
{
    public async Task<QueueStatusDto> GetQueueAsync(
        Guid providerId,
        DateTime date)
    {
        // 1. Get all appointments for provider on date
        var appointments = await _dbContext.Appointments
            .Where(a => a.ProviderId == providerId &&
                       a.StartTime.Date == date.Date)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
        
        // 2. Calculate wait times
        var queueItems = appointments.Select(a => new QueueItem
        {
            Id = a.Id,
            PatientName = a.Patient.FullName,
            ScheduledTime = a.StartTime,
            Status = a.Status,
            UrgencyLevel = a.UrgencyLevel,
            WaitMinutes = CalculateWaitTime(a),
            Position = appointments.IndexOf(a) + 1
        }).ToList();
        
        // 3. Calculate average wait time
        var completedAppointments = appointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .ToList();
        
        var avgWaitTime = completedAppointments.Any()
            ? completedAppointments.Average(a => a.ActualWaitMinutes)
            : 0;
        
        return new QueueStatusDto
        {
            QueueItems = queueItems,
            TotalAppointments = queueItems.Count,
            AverageWaitMinutes = avgWaitTime,
            NextPatient = queueItems.FirstOrDefault(q => 
                q.Status == AppointmentStatus.Ready)
        };
    }
    
    public async Task UpdateAppointmentStatusAsync(
        Guid appointmentId,
        AppointmentStatus newStatus,
        Guid staffMemberId)
    {
        var appointment = await _dbContext.Appointments
            .FindAsync(appointmentId);
        
        // Validate status transition
        ValidateStatusTransition(appointment.Status, newStatus);
        
        // Record wait time if transitioning to Complete
        if (newStatus == AppointmentStatus.Completed)
        {
            appointment.ActualWaitMinutes = 
                (int)(DateTime.UtcNow - appointment.StartTime).TotalMinutes;
        }
        
        appointment.Status = newStatus;
        appointment.UpdatedByStaffId = staffMemberId;
        appointment.UpdatedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket
        await _pusherService.TriggerAsync(
            $"queue-{appointment.ProviderId}",
            "status-updated",
            new { appointmentId, newStatus });
    }
}
```

---

## 3. US_031: Arrival Check-in System

### Test Objectives
- Verify patient check-in flow (self-service or staff)
- Test appointment status transition to "Arrived"
- Confirm wait time tracking
- Validate staff notification workflow
- Test no-show detection

### Test Cases

#### TC-US-031-HP-01: Patient Self-Service Check-in via Kiosk
| Field | Value |
|-------|-------|
| Requirement | FR-016 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient arrives at clinic with scheduled appointment
**When**: Patient checks in via kiosk/tablet
**Then**: Appointment status changed to "Arrived"; staff notified

**Check-in Flow:**
```
1. Patient tap screen: "I have an appointment"
2. Kiosk prompts: Phone number / Email / Name
3. System finds matching appointment
4. Kiosk displays: "Welcome [Patient Name]. You're checked in!"
5. Appointment status → "Arrived"
6. Queue updated
7. Staff receives notification
```

**Expected Results:**
- [ ] Appointment found (tolerates typos via fuzzy match)
- [ ] Status changed to "Arrived"
- [ ] Arrival time recorded
- [ ] Queue updated in real-time
- [ ] Staff notification sent (visual + optional sound)
- [ ] Confirmation message shown to patient
- [ ] Wait time calculation begins

---

#### TC-US-031-HP-02: Staff Manual Check-in
| Field | Value |
|-------|-------|
| Requirement | FR-016 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient arrives and staff assists with check-in
**When**: Staff searches for patient and marks "Arrived"
**Then**: Status updated; patient moved to queue

**Expected Results:**
- [ ] Staff accesses "Check In Patient" form
- [ ] Quick search by phone/name/ID
- [ ] One-click "Mark Arrived" button
- [ ] Status change confirms with "Checked in at 2:15 PM"
- [ ] Queue updated immediately
- [ ] Staff member ID recorded
- [ ] Audit log entry created

---

#### TC-US-031-HP-03: Late Arrival (After Appointment Time)
| Field | Value |
|-------|-------|
| Requirement | FR-016, FR-036 |
| Type | happy_path |
| Priority | P1 |

**Given**: Patient arrives 15+ minutes after scheduled time
**When**: Patient checks in
**Then**: Late arrival recorded; no-show risk flagged

**Expected Results:**
- [ ] Arrival time compared to scheduled time
- [ ] Late minutes calculated
- [ ] "Late Arrival" flag set if >5 minutes late
- [ ] Notation on queue: "Arrived 15 min late"
- [ ] No-show risk assessment triggered
- [ ] Historical data tracked for patient pattern
- [ ] Provider can view arrival history

---

#### TC-US-031-ER-01: No Matching Appointment
| Field | Value |
|-------|-------|
| Requirement | FR-016 |
| Type | error |
| Priority | P1 |

**Given**: Patient attempts check-in but no appointment found
**When**: Kiosk search returns no results
**Then**: Fallback to staff assistance; error handling

**Expected Results:**
- [ ] Kiosk shows: "No appointment found. Please ask staff for assistance."
- [ ] Staff access to "Create Walk-in" from failed check-in
- [ ] Patient not stranded
- [ ] Multiple search attempts logged (potential duplicate patient issue)

---

#### TC-US-031-ER-02: Double Check-in Prevention
| Field | Value |
|-------|-------|
| Requirement | FR-016 |
| Type | error |
| Priority | P1 |

**Given**: Patient already checked in
**When**: Attempt to check in again
**Then**: Error message; prevent status change

**Expected Results:**
- [ ] System detects "already checked in"
- [ ] Kiosk shows: "You're already checked in. Your wait time is [X] min"
- [ ] Status not changed twice
- [ ] Database idempotency check prevents duplicate entries

---

### Check-in Service Architecture
```csharp
public class CheckInService
{
    public async Task<CheckInResultDto> CheckInPatientAsync(
        CheckInRequestDto request,  // phone, email, or name
        CheckInSource source)       // Kiosk or Staff
    {
        // 1. Find appointment
        var appointment = await FindAppointmentAsync(request);
        if (appointment == null)
            throw new AppointmentNotFoundException();
        
        // 2. Verify not already checked in
        if (appointment.Status == AppointmentStatus.Arrived)
            return new CheckInResultDto
            {
                Success = false,
                Message = $"Already checked in. Wait time: {CalculateWaitTime(appointment)} min"
            };
        
        // 3. Record arrival time
        var arrivalTime = DateTime.UtcNow;
        var lateMinutes = (int)(arrivalTime - appointment.StartTime).TotalMinutes;
        
        appointment.Status = AppointmentStatus.Arrived;
        appointment.ActualArrivalTime = arrivalTime;
        appointment.IsLateArrival = lateMinutes > 5;
        appointment.LateMinutes = lateMinutes > 0 ? lateMinutes : 0;
        appointment.CheckInSource = source.ToString();
        
        // 4. No-show risk assessment
        if (appointment.IsLateArrival)
        {
            await _noShowRiskService.AssessLateArrivalAsync(appointment);
        }
        
        await _dbContext.SaveChangesAsync();
        
        // 5. Notify staff
        await _pusherService.TriggerAsync(
            $"queue-{appointment.ProviderId}",
            "patient-arrived",
            new { appointment.Id, appointment.PatientName, lateMinutes });
        
        return new CheckInResultDto
        {
            Success = true,
            AppointmentId = appointment.Id,
            PatientName = appointment.Patient.FullName,
            ProviderName = appointment.Provider.FullName,
            EstimatedWaitMinutes = CalculateEstimatedWait(appointment)
        };
    }
}
```

---

## 4. US_032: Staff Performance Dashboard

### Test Objectives
- Verify dashboard displays key performance metrics
- Test real-time metric updates
- Confirm individual and team statistics
- Validate time period filtering
- Test data accuracy

### Test Cases

#### TC-US-032-HP-01: View Daily Performance Dashboard
| Field | Value |
|-------|-------|
| Requirement | FR-017 |
| Type | happy_path |
| Priority | P1 |

**Given**: Staff member viewing performance dashboard
**When**: Open dashboard for today
**Then**: Performance metrics displayed in real-time

**Dashboard Content:**
```
TODAY'S PERFORMANCE DASHBOARD
┌────────────────────────────────────────────────────────────────┐
│ Provider: Dr. Sarah Johnson          │ Date: March 23, 2026   │
├────────────────────────────────────────────────────────────────┤
│ METRICS                                                        │
│  Total Appointments: 14                                        │
│  Completed: 12                                                 │
│  In Progress: 1                                                │
│  No-Shows: 1                                                   │
│                                                                │
│ AVERAGE WAIT TIME: 6.2 minutes  (Target: <10 min) ✓           │
│ AVERAGE VISIT TIME: 18.5 minutes                              │
│ ON-TIME COMPLETION: 87% (Target: >85%) ✓                      │
│ PATIENT SATISFACTION: 4.7/5 stars (12 reviews)               │
│                                                                │
│ EFFICIENCY: 94% (appointments completed on schedule) ✓         │
│ NO-SHOW RATE: 7.1% (Historical avg: 9.2%)                    │
└────────────────────────────────────────────────────────────────┘
```

**Expected Results:**
- [ ] Metrics calculated correctly
- [ ] Real-time updates (refresh every 30 seconds)
- [ ] Color-coded: Green (good), Yellow (acceptable), Red (needs improvement)
- [ ] Targets displayed for comparison
- [ ] Historical trend visible (inline chart)
- [ ] Time period selector (today, this week, this month)
- [ ] Drill-down capability (click metric to see detail)

---

#### TC-US-032-HP-02: Team Performance Comparison
| Field | Value |
|-------|-------|
| Requirement | FR-017 |
| Type | happy_path |
| Priority | P1 |

**Given**: Manager viewing team dashboard
**When**: Compare performance across providers
**Then**: Ranked leaderboard shown with metrics

**Team Dashboard:**
```
CLINIC PERFORMANCE - Today
┌─────────────────────────────────────────────────────────────────┐
│ Provider          │ Appointments │ Avg Wait │ Satisfaction │
├─────────────────────────────────────────────────────────────────┤
│ 🥇 Dr. Sarah J.   │ 14/14 (100%) │ 6.2 min  │ ⭐ 4.7/5     │
│ 🥈 Dr. Michael B. │ 12/13 (92%)  │ 8.5 min  │ ⭐ 4.4/5     │
│ 🥉 PA Jennifer    │ 10/10 (100%) │ 7.1 min  │ ⭐ 4.5/5     │
└─────────────────────────────────────────────────────────────────┘
```

**Expected Results:**
- [ ] All providers ranked by metric
- [ ] Metrics comparable (same standards)
- [ ] No privacy violations (only names visible, no patient data)
- [ ] Sortable by each metric column
- [ ] Historical comparison (same day last week, last month)
- [ ] Trends visible (improving/declining)

---

#### TC-US-032-HP-03: Historical Performance Report
| Field | Value |
|-------|-------|
| Requirement | FR-017 |
| Type | happy_path |
| Priority | P1 |

**Given**: Staff member filtering by date range
**When**: Select "Last 30 Days" or "Last Quarter"
**Then**: Aggregated performance data displayed

**Expected Results:**
- [ ] Date range selector (day picker)
- [ ] Metrics aggregated correctly
- [ ] Trends visible (line chart)
- [ ] Drill-down to daily details
- [ ] Export to CSV/PDF available
- [ ] No performance data for future dates (prevents forecasting confusion)

---

#### TC-US-032-HP-04: No-Show Prediction Dashboard
| Field | Value |
|-------|-------|
| Requirement | FR-036, AIR-O01 |
| Type | happy_path |
| Priority | P1 |

**Given**: Dashboard displaying no-show metrics
**When**: View no-show risk assessment data
**Then**: Risk scores and patterns visible

**No-Show Dashboard:**
```
NO-SHOW ANALYTICS - This Month
┌─────────────────────────────────────────────────────┐
│ Total Appointments: 287                             │
│ No-Shows: 22 (7.7%)  vs Target: <5%  ⚠️ Above      │
│                                                     │
│ HIGH-RISK PATIENTS                                  │
│ Risk Score: >80%                                    │
│   1. John Smith (3 no-shows in 6 months)           │
│   2. Jane Doe (Late by 15+ min 80% of time)       │
│   3. Bob Johnson (Cancelled last 2 appointments)  │
│                                                     │
│ PATTERNS DETECTED                                   │
│  - Morning appointments (9-10 AM): 12% no-show    │
│  - Friday appointments: 9.2% no-show              │
│  - Telehealth: 4.2% no-show (lower!)             │
│                                                     │
│ STAFF METRICS                                       │
│  - Dr. Sarah: 5.6% no-show rate ✓                 │
│  - Dr. Michael: 8.2% no-show rate ⚠️              │
└─────────────────────────────────────────────────────┘
```

**Expected Results:**
- [ ] No-show rate calculated per provider
- [ ] Risk scoring for high-risk patients
- [ ] Pattern detection (time, day, type)
- [ ] Staff-specific metrics visible
- [ ] Trend over time (improving/declining)
- [ ] Actionable insights ("Morning appointments have higher no-show rate")

---

#### TC-US-032-HP-05: Real-Time Live Dashboard
| Field | Value |
|-------|-------|
| Requirement | FR-017, NFR-004 |
| Type | happy_path |
| Priority | P1 |

**Given**: Manager monitoring clinic operations
**When**: Watch live dashboard during business hours
**Then**: Metrics update in real-time without page refresh

**Expected Results:**
- [ ] WebSocket connection established (Pusher)
- [ ] Metrics update every 30 seconds
- [ ] No page reload required
- [ ] Smooth animations for metric changes
- [ ] Alert if target breached (red flash)
- [ ] Bandwidth optimized (delta updates)

---

### Performance Dashboard Service Architecture
```csharp
public class PerformanceDashboardService
{
    public async Task<DashboardMetricsDto> GetDailyMetricsAsync(
        Guid providerId,
        DateTime date)
    {
        // 1. Get all appointments for day
        var appointments = await _dbContext.Appointments
            .Where(a => a.ProviderId == providerId &&
                       a.StartTime.Date == date.Date)
            .ToListAsync();
        
        // 2. Calculate metrics
        var completed = appointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .ToList();
        
        var avgWaitTime = completed.Any()
            ? completed.Average(a => a.ActualWaitMinutes ?? 0)
            : 0;
        
        var avgVisitTime = completed.Any()
            ? completed.Average(a => 
                (a.EndTime - a.StartTime).TotalMinutes)
            : 0;
        
        var noShowCount = appointments
            .Count(a => a.Status == AppointmentStatus.NoShow);
        
        // 3. Get satisfaction scores
        var reviews = await _dbContext.PatientReviews
            .Where(r => r.ProviderId == providerId &&
                       r.CreatedAt.Date == date.Date)
            .ToListAsync();
        
        var avgSatisfaction = reviews.Any()
            ? reviews.Average(r => r.Rating)
            : 0;
        
        // 4. Calculate efficiency
        var onTimeCount = completed.Count(a => 
            (a.EndTime - a.StartTime).TotalMinutes <= 30);
        var efficiency = completed.Any()
            ? onTimeCount / (double)completed.Count
            : 0;
        
        return new DashboardMetricsDto
        {
            ProviderId = providerId,
            Date = date,
            TotalAppointments = appointments.Count,
            CompletedAppointments = completed.Count,
            AverageWaitMinutes = avgWaitTime,
            AverageVisitMinutes = avgVisitTime,
            NoShowCount = noShowCount,
            NoShowRate = appointments.Any()
                ? noShowCount / (double)appointments.Count
                : 0,
            PatientSatisfactionRating = avgSatisfaction,
            EfficiencyPercentage = efficiency,
            MetricTrend = CalculateTrend(providerId, date)
        };
    }
    
    // Scheduled job for real-time updates
    [RecurringJob("dashboard-updates", "*/1 * * * *")]  // Every minute
    public async Task UpdateLiveDashboardMetricsAsync()
    {
        var providers = await _providerRepository.GetAllAsync();
        
        foreach (var provider in providers)
        {
            var metrics = await GetDailyMetricsAsync(
                provider.Id,
                DateTime.UtcNow.Date);
            
            // Push to frontend via WebSocket
            await _pusherService.TriggerAsync(
                $"dashboard-{provider.Id}",
                "metrics-updated",
                metrics);
        }
    }
}
```

---

## Test Execution Strategy

### Execution Sequence
1. **US_029** (Walk-in): Foundation for all staff flows
2. **US_031** (Check-in): Depends on US_029
3. **US_030** (Queue): Depends on US_031  
4. **US_032** (Dashboard): Depends on US_030

### P0 Critical Path
```
US_029 → US_031 → US_030 → US_032
(Create) (Check In) (Manage) (Report)
```

### Staff Workflow E2E Test
```
1. Walk-in patient arrives: John Smith (no appointment)
2. Staff uses walk-in form to check John in
3. John added to queue for Dr. Sarah
4. John checks in via kiosk (optional staff assist)
5. Appointment status → "Arrived"
6. Queue view shows John waiting (6 min estimated)
7. Dr. Sarah sees John in queue
8. Dr. Sarah marks John "Being Seen"
9. After visit: Mark "Complete"
10. Dashboard shows completed appointment
11. Wait time calculated and logged for Dr. Sarah
12. No-show risk for John assessed (on-time arrival = lower risk)
```

---

## Security Considerations

### Staff Role Enforcement (RBAC)
- Patient check-in: Staff + Admin only
- Queue modification: Staff + Admin only
- Dashboard: Staff (own), Manager (team), Admin (all)
- Walk-in creation: Staff + Admin only

### Data Privacy
- Queue view: Patient names only (no medical details)
- Dashboard: Aggregated metrics only (no patient PII)
- Staff cannot view metrics for other providers (unless manager)
- Audit log: Track all queue/status changes

### OWASP Coverage
- A04: Insecure Design - RBAC on queue modification
- A06: Vulnerable and Outdated Components - WebSocket auth
- A09: Logging & Monitoring - Staff actions logged

---

## Success Criteria

- [ ] Walk-in appointments created immediately
- [ ] Same-day queue displays all appointments
- [ ] Check-in transitions appointment status correctly
- [ ] Real-time updates via WebSocket (<2 sec)
- [ ] Performance metrics calculated accurately
- [ ] Dashboard refreshes automatically
- [ ] No-show risk assessment triggered
- [ ] RBAC enforced on all operations
- [ ] Audit logs comprehensive
- [ ] Zero data privacy issues
- [ ] Performance: <500ms for queue load, <200ms for metric update

---

## Sign-Off

**Status**: ✅ **READY FOR IMPLEMENTATION**  
**Scope**: EP-003 staff appointment management workflow  
**Coverage**: 4 user stories, 26+ test cases  
**User Impact**: HIGH (clinic staff daily workflow)  
**Complexity**: Medium (core state management, real-time updates)  
**Completion Target**: After EP-001 & EP-002 implementation
