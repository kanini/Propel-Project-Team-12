# Task - task_002_be_sms_email_reminder_services

## Requirement Reference

- User Story: us_037
- Story Location: .propel/context/tasks/EP-005/us_037/us_037.md
- Acceptance Criteria:
  - AC-1: System sends a reminder via both SMS (Twilio) and Email (SendGrid) with appointment date, time, provider, and location
- Edge Cases:
  - Patient has no phone number: Only email reminders are sent; SMS notification is skipped with a log entry

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

## Applicable Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET 8 ASP.NET Core Web API | .NET 8.0 |
| Library | Twilio SDK | Twilio 7.x |
| Library | SendGrid SDK | SendGrid 9.x |

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

Implement the SMS and email channel services required for the multi-channel reminder system. This creates `ISmsService` and `SmsService` for Twilio SMS delivery, extends the existing `IEmailService` with a `SendAppointmentReminderAsync` method for reminder-specific emails via SendGrid, adds Twilio/SendGrid configuration sections to appsettings, and registers the SMS service in the DI container. Both services include development-mode logging fallbacks when API keys are not configured, matching the existing `EmailService` pattern. The edge case (no phone number) is handled at the service level by returning a skip result when phone is null/empty.

## Dependent Tasks

- None within US_037 (services are standalone channel adapters)
- External: Existing `IEmailService` / `EmailService` from EP-001 (already implemented)

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/Interfaces/ISmsService.cs` — Interface for SMS delivery
- **NEW** `src/backend/PatientAccess.Business/Services/SmsService.cs` — Twilio SMS implementation with dev-mode fallback
- **MODIFY** `src/backend/PatientAccess.Business/Services/IEmailService.cs` — Add `SendAppointmentReminderAsync` method
- **MODIFY** `src/backend/PatientAccess.Business/Services/EmailService.cs` — Implement reminder email method
- **MODIFY** `src/backend/PatientAccess.Web/appsettings.json` — Add `TwilioSettings` and `SendGridSettings` configuration sections
- **MODIFY** `src/backend/PatientAccess.Web/appsettings.Development.json` — Add development Twilio/SendGrid placeholders
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register `ISmsService` in DI container

## Implementation Plan

1. **Define ISmsService interface**:
   ```csharp
   namespace PatientAccess.Business.Interfaces;

   public interface ISmsService
   {
       Task<bool> SendSmsAsync(string toPhoneNumber, string messageBody);
       Task<bool> SendAppointmentReminderSmsAsync(
           string toPhoneNumber,
           string patientName,
           string providerName,
           DateTime scheduledDateTime,
           string location);
   }
   ```
   Two methods: generic SMS and appointment-specific reminder. Returns `bool` for success/failure, matching `IEmailService` pattern.

2. **Implement SmsService**:
   - Inject `IConfiguration` and `ILogger<SmsService>`
   - Read `TwilioSettings:AccountSid`, `TwilioSettings:AuthToken`, `TwilioSettings:FromPhoneNumber`, `TwilioSettings:Enabled` from config
   - If `Enabled = false` or credentials missing: log the message content (development mode) and return `true`, matching `EmailService` dev pattern
   - If enabled: use Twilio REST API via `TwilioClient.Init(accountSid, authToken)` and `MessageResource.CreateAsync`
   - `SendAppointmentReminderSmsAsync`: format message body with appointment date, time, provider, and location (AC-1)
   - Handle `ApiException` from Twilio: log error details, return `false`
   - Handle missing/null phone number: log warning "SMS skipped — no phone number for patient", return `true` (edge case)

3. **Extend IEmailService with reminder method**:
   ```csharp
   Task<bool> SendAppointmentReminderAsync(
       string toEmail,
       string toName,
       string providerName,
       DateTime scheduledDateTime,
       string location);
   ```
   Separate from `SendAppointmentConfirmationAsync` — reminder emails have different content and no PDF attachment.

4. **Implement SendAppointmentReminderAsync in EmailService**:
   - Read `SendGridSettings:ApiKey`, `SendGridSettings:FromEmail`, `SendGridSettings:FromName`, `SendGridSettings:Enabled` from config
   - If `Enabled = false` or no API key: log email content (dev mode), return `true`
   - If enabled: use SendGrid `SendGridClient` to send reminder email
   - Email body includes: appointment date, time, provider name, location, and a "manage appointment" link to `{FrontendUrl}/appointments`
   - Return `true` on success, `false` on failure with logged error

5. **Add configuration sections to appsettings.json**:
   ```json
   "TwilioSettings": {
       "Enabled": false,
       "AccountSid": "your-twilio-account-sid",
       "AuthToken": "your-twilio-auth-token",
       "FromPhoneNumber": "+15551234567",
       "_comment": "Twilio SMS gateway for appointment reminders (TR-010). Set Enabled=true and configure credentials from Twilio console. Free tier: trial credits. Store credentials in environment variables for production."
   },
   "SendGridSettings": {
       "Enabled": false,
       "ApiKey": "your-sendgrid-api-key",
       "FromEmail": "noreply@patientaccess.com",
       "FromName": "Patient Access Platform",
       "_comment": "SendGrid email service for notifications (TR-011). Set Enabled=true and configure API key from SendGrid dashboard. Free tier: 100 emails/day. Store API key in environment variables for production."
   }
   ```

6. **Register ISmsService in Program.cs DI**:
   ```csharp
   builder.Services.AddSingleton<ISmsService, SmsService>(); // US_037 - SMS reminder delivery via Twilio
   ```
   Singleton matches `IEmailService` registration pattern (stateless, config-driven).

7. **Add NuGet package references** (if not already present):
   - `Twilio` package to `PatientAccess.Business.csproj`
   - `SendGrid` package to `PatientAccess.Business.csproj` (if not already referenced)

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Interfaces/
│   │   ├── IAppointmentService.cs       # EXISTS
│   │   └── ...                          # No ISmsService
│   ├── Services/
│   │   ├── IEmailService.cs             # EXISTS — SendVerificationEmailAsync, SendAppointmentConfirmationAsync
│   │   ├── EmailService.cs              # EXISTS — dev-mode logging implementation
│   │   └── ...                          # No SmsService
│   └── PatientAccess.Business.csproj    # EXISTS — may need Twilio/SendGrid packages
├── PatientAccess.Web/
│   ├── Program.cs                       # EXISTS — DI registrations, IEmailService as Singleton
│   ├── appsettings.json                 # EXISTS — no Twilio/SendGrid sections
│   └── appsettings.Development.json     # EXISTS
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Interfaces/ISmsService.cs | SMS delivery interface with generic and reminder-specific methods |
| CREATE | src/backend/PatientAccess.Business/Services/SmsService.cs | Twilio implementation with dev-mode fallback |
| MODIFY | src/backend/PatientAccess.Business/Services/IEmailService.cs | Add SendAppointmentReminderAsync method |
| MODIFY | src/backend/PatientAccess.Business/Services/EmailService.cs | Implement reminder email via SendGrid with dev fallback |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add TwilioSettings and SendGridSettings sections |
| MODIFY | src/backend/PatientAccess.Web/appsettings.Development.json | Add development Twilio/SendGrid placeholders |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register ISmsService in DI |
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add Twilio NuGet package reference |

## External References

- Twilio C# SDK: https://www.twilio.com/docs/libraries/csharp-dotnet
- Twilio MessageResource API: https://www.twilio.com/docs/sms/api/message-resource
- SendGrid C# SDK: https://docs.sendgrid.com/for-developers/sending-email/quickstart-csharp
- .NET 8 Configuration: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration

## Build Commands

```bash
cd src/backend
dotnet add PatientAccess.Business/PatientAccess.Business.csproj package Twilio
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [ ] `ISmsService` interface compiles with both method signatures
- [ ] `SmsService` logs message in dev mode when `TwilioSettings:Enabled = false`
- [ ] `SmsService` handles null/empty phone number gracefully (returns true, logs skip)
- [ ] `IEmailService.SendAppointmentReminderAsync` compiles and is implemented
- [ ] `EmailService` logs reminder email in dev mode when `SendGridSettings:Enabled = false`
- [ ] `ISmsService` is resolved from DI container without runtime errors
- [ ] `appsettings.json` has valid JSON after Twilio/SendGrid sections added
- [ ] Solution builds without warnings

## Implementation Checklist

- [x] Create `ISmsService` interface with `SendSmsAsync` and `SendAppointmentReminderSmsAsync` methods
- [x] Implement `SmsService` with Twilio client, dev-mode fallback, and null-phone handling
- [x] Add `SendAppointmentReminderAsync` method to `IEmailService` interface
- [x] Implement reminder email method in `EmailService` with SendGrid and dev-mode fallback
- [x] Add `TwilioSettings` and `SendGridSettings` configuration sections to appsettings files
- [x] Register `ISmsService` as Singleton in `Program.cs` DI container
- [x] Add Twilio NuGet package reference to `PatientAccess.Business.csproj`
