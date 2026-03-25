# Brevo Email Setup Guide

## Issue: Emails showing as "sent" but not being delivered

This happens because **the sender email address is not verified** in your Brevo account.

## Solution: Verify Your Sender Email

### Step 1: Log into Brevo
1. Go to [https://app.brevo.com](https://app.brevo.com)
2. Log in with your Brevo account credentials

### Step 2: Add and Verify Sender Email
1. Navigate to **Senders & IP** → **Senders** in the left sidebar
   - Or go directly to: [https://app.brevo.com/settings/senders](https://app.brevo.com/settings/senders)
2. Click **"Add a new sender"**
3. Enter an email address **you own and have access to**, such as:
   - Your personal Gmail (e.g., `yourname@gmail.com`)
   - Your company email
   - Any email address where you can receive verification emails
4. Enter a sender name (e.g., `Patient Access Platform`)
5. Click **"Add"**

### Step 3: Verify the Email
1. Brevo will send a verification email to the address you entered
2. Check your inbox (and spam folder) for the verification email from Brevo
3. Click the verification link in the email
4. Once verified, the sender will show as "Verified" in your Brevo dashboard

### Step 4: Update Configuration
1. Open `src/backend/PatientAccess.Web/appsettings.Development.json`
2. Update the `SenderEmail` field:
   ```json
   "BrevoSettings": {
     "ApiKey": "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",
     "ApiUrl": "https://api.brevo.com/v3/smtp/email",
     "SenderEmail": "your-verified-email@gmail.com",  // <-- Change this
     "SenderName": "Patient Access Platform - Dev"
   }
   ```
3. Save the file

### Step 5: Restart and Test
1. Restart your backend application
2. Register a new user
3. Check the application logs for detailed Brevo API responses
4. Check your email inbox for the verification email

## Checking Logs

The enhanced logging will now show:
- ✓ Success messages when emails are sent
- ✗ Error messages with detailed Brevo API responses
- ⚠️ Special warnings if sender verification is needed

To see detailed logs during registration:
```bash
cd src/backend/PatientAccess.Web
dotnet run
```

Watch for log messages like:
```
info: PatientAccess.Business.Services.EmailService[0]
      Sending email via Brevo API. Sender: your-email@gmail.com, API URL: https://api.brevo.com/v3/smtp/email

info: PatientAccess.Business.Services.EmailService[0]
      ✓ Email sent successfully via Brevo. Response: {"messageId": "..."}
```

## Common Issues

### Issue: "unauthorized_sender" error
**Cause**: The sender email is not verified in Brevo
**Solution**: Follow Steps 1-4 above to verify your sender email

### Issue: Email goes to spam
**Cause**: Using a free email provider (Gmail, Yahoo, etc.) as sender
**Solution**: This is normal for development. For production, use a custom domain email and set up SPF/DKIM records.

### Issue: API key invalid
**Cause**: The API key is incorrect or expired
**Solution**: 
1. Go to [https://app.brevo.com/settings/keys/api](https://app.brevo.com/settings/keys/api)
2. Create a new API key or verify the existing one
3. Update the `ApiKey` in your appsettings file

## Testing Tips

1. **Use a real email address you can access** for the sender (your personal email is fine for testing)
2. **Check spam folders** when testing email delivery
3. **Monitor the application logs** for detailed error messages
4. **Test with different recipient emails** to rule out recipient-side spam filters

## Brevo Free Tier Limits

- **300 emails per day**
- **Unlimited contacts**
- Basic email templates included

For production with higher volume, consider upgrading to a paid Brevo plan.

## Additional Resources

- [Brevo Documentation](https://developers.brevo.com/)
- [Brevo Sender Verification Guide](https://help.brevo.com/hc/en-us/articles/209551749)
- [Brevo API Documentation](https://developers.brevo.com/reference/sendtransacemail)
