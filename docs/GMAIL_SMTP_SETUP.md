# Gmail SMTP Email Setup Guide

## Overview

The Patient Access Platform uses Gmail SMTP for sending transactional emails including:
- User verification emails
- Password reset emails
- Appointment confirmation emails with PDF attachments

## Gmail SMTP Configuration

### Current Setup
- **Email**: mridhul35@gmail.com
- **SMTP Host**: smtp.gmail.com
- **Port**: 587 (TLS)
- **EnableSSL**: true

### Application Password
Gmail App Password has been configured for secure authentication:
- **App Password**: rjql ajfr xclz dtxa (configured in appsettings.Development.json)
- For production, use environment variable: `SMTP__PASSWORD`

## Configuration Files

### Development Environment (appsettings.Development.json)
```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "Username": "mridhul35@gmail.com",
  "Password": "rjql ajfr xclz dtxa",
  "SenderEmail": "mridhul35@gmail.com",
  "SenderName": "Patient Access Platform - Dev"
}
```

### Production Environment (appsettings.json)
```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "Username": "mridhul35@gmail.com",
  "Password": "SET_VIA_ENV_SMTP__PASSWORD",
  "SenderEmail": "mridhul35@gmail.com",
  "SenderName": "Patient Access Platform"
}
```

## Setting Up Gmail App Password

If you need to create a new App Password or use a different Gmail account:

### Step 1: Enable 2-Factor Authentication
1. Go to your Google Account: https://myaccount.google.com/
2. Navigate to **Security**
3. Enable **2-Step Verification** if not already enabled

### Step 2: Generate App Password
1. In Security settings, scroll to **2-Step Verification**
2. At the bottom, select **App passwords**
3. Select **Mail** as the app
4. Select **Windows Computer** or **Other (Custom name)** as the device
5. Click **Generate**
6. Copy the 16-character password (it will look like: `xxxx xxxx xxxx xxxx`)

### Step 3: Update Configuration
1. Open `src/backend/PatientAccess.Web/appsettings.Development.json`
2. Update the `SmtpSettings:Password` field with your new app password
3. Update `SmtpSettings:Username` and `SmtpSettings:SenderEmail` if using a different email

## Environment Variables (Production)

For production deployment, set the following environment variables:

```bash
# Windows PowerShell
$env:SMTP__PASSWORD="your-app-password-here"

# Linux/Mac
export SMTP__PASSWORD="your-app-password-here"
```

Or use Azure App Service Configuration:
1. Go to Azure Portal → App Service → Configuration
2. Add New Application Setting:
   - Name: `SMTP__PASSWORD`
   - Value: Your Gmail App Password

## Testing Email Functionality

### Test User Registration
1. Start the backend application
2. Register a new user via the frontend or API
3. Check the application logs for:
   ```
   Sending email via SMTP. To: user@example.com, Subject: Verify Your Patient Access Account
   Email sent successfully via SMTP to user@example.com
   ```
4. Check the recipient's inbox for the verification email

### Test Appointment Confirmation
1. Book an appointment through the application
2. Check logs for appointment confirmation email
3. Verify PDF attachment is included in the email

## Troubleshooting

### Error: "SMTP authentication failed"
- Verify the App Password is correct (16 characters, no spaces)
- Ensure 2-Step Verification is enabled on the Gmail account
- Check if the App Password has been revoked in Google Account settings

### Error: "Unable to connect to SMTP server"
- Verify network connectivity
- Check if port 587 is blocked by firewall
- Try alternative port 465 (update `SmtpSettings:Port` to 465)

### Error: "Message not delivered"
- Check Gmail's sending limits (500 emails per day for free accounts)
- Verify the sender email is correct
- Check Gmail's Sent folder to confirm delivery attempts

### Emails going to Spam
- Add SPF, DKIM records to your domain (if using custom domain)
- Request recipients to mark emails as "Not Spam"
- Consider using Google Workspace for better deliverability

## Daily Sending Limits

**Gmail Free Account**: 500 emails per day
**Google Workspace**: 2,000 emails per day

For higher volume, consider:
- Using a dedicated email service (SendGrid, Mailgun, AWS SES)
- Implementing rate limiting in the application
- Batching emails during off-peak hours

## Security Best Practices

1. **Never commit passwords to version control**
   - Use environment variables for production
   - Keep appsettings.Development.json in .gitignore if it contains real credentials

2. **Rotate App Passwords regularly**
   - Regenerate App Passwords every 90 days
   - Revoke unused App Passwords

3. **Monitor email logs**
   - Review SMTP logs for suspicious activity
   - Set up alerts for high sending volumes

4. **Use HTTPS for frontend**
   - Ensures token links in emails are secure
   - Prevents token interception

## Support

For issues with Gmail SMTP setup:
- Gmail Help: https://support.google.com/mail/answer/7126229
- App Password Help: https://support.google.com/accounts/answer/185833

For application-specific issues:
- Check application logs in `src/backend/PatientAccess.Web`
- Review EmailService.cs for detailed error messages
- Contact development team at support@propeliq.com
