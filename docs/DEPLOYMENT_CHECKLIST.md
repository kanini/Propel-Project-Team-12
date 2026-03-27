# Backend Deployment Checklist

## 📋 Pre-Deployment Requirements

Before deploying the backend to MonsterASP.NET, ensure all GitHub Secrets are configured and all services are set up.

---

## 🔐 Required GitHub Secrets

Navigate to: **GitHub Repository → Settings → Secrets and variables → Actions → New repository secret**

### 1. **FTP Credentials** (MonsterASP.NET)

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `FTP_BACKEND_HOST` | FTP server URL | `ftp://site59724.siteasp.net` |
| `FTP_BACKEND_USERNAME` | FTP username | `site59724` |
| `FTP_BACKEND_PASSWORD` | FTP password | `your-ftp-password` |

**Where to get:** MonsterASP.NET control panel → FTP Accounts

---

### 2. **Database Credentials** (Supabase PostgreSQL)

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `DB_PASSWORD` | Supabase PostgreSQL password | `your-database-password` |

**Where to get:** 
- Supabase Dashboard → Project Settings → Database → Connection string
- Extract the password from the connection string

**Current Connection String (from appsettings.json):**
```
Host=aws-1-ap-northeast-2.pooler.supabase.com
Port=5432
Database=postgres
Username=postgres.dhbgcoscqujsfycytvns
Password=<YOUR_PASSWORD_HERE>
```

---

### 3. **Email Configuration** (Gmail SMTP)

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `SMTP__PASSWORD` | Gmail App Password (16 characters) | `rjql ajfr xclz dtxa` |

**Where to get:**
1. Go to Google Account → Security → 2-Step Verification
2. Generate App Password for "Mail" application
3. Copy the 16-character password (format: `xxxx xxxx xxxx xxxx`)

**Note:** Already configured with `mridhul35@gmail.com`

---

### 4. **AI Services** (Google Gemini)

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `GEMINI_API_KEY` | Google Gemini AI API key | `AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXXX` |

**Where to get:**
- Visit: https://aistudio.google.com/apikey
- Create new API key or use existing one

**Free Tier Limits:**
- 15 requests per minute (RPM)
- 1 million tokens per minute (TPM)
- 1,500 requests per day (RPD)

---

### 5. **File Storage** (Supabase Storage)

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `SUPABASE_API_KEY` | Supabase service_role (secret) key | `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` |

**Where to get:**
- Supabase Dashboard → Project Settings → API
- Copy the `service_role` key (NOT the `anon` public key)
- This key bypasses Row Level Security (RLS) for backend operations

**Storage Bucket:** `propeliq` (already configured)

---

### 6. **Real-time Updates** (Pusher - Optional)

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `PUSHER_APP_ID` | Pusher application ID | `1234567` |
| `PUSHER_KEY` | Pusher key | `xxxxxxxxxxxxxxxxxxx` |
| `PUSHER_SECRET` | Pusher secret | `xxxxxxxxxxxxxxxxxxx` |

**Where to get:**
- Pusher Dashboard: https://dashboard.pusher.com
- Create a new Channels app or use existing
- Cluster: `ap2` (Asia Pacific - Singapore)

**Free Tier:**
- 200,000 messages per day
- 100 concurrent connections

**Note:** Currently **disabled** in appsettings.json. Set `Pusher:Enabled` to `true` to enable.

---

### 7. **Caching** (Upstash Redis - Optional)

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `REDIS_CONNECTION_STRING` | Upstash Redis connection string | `redis://default:password@host:port` |

**Where to get:**
- Upstash Dashboard: https://console.upstash.com
- Create a Redis database
- Copy connection string from database details

**Note:** Currently **enabled** in appsettings.json but using placeholder. Disable or provide valid connection string.

---

## ✅ Deployment Verification Checklist

### Before Deployment

- [ ] All GitHub Secrets configured (at minimum: FTP, DB, SMTP, Gemini, Supabase)
- [ ] RSA keys generated (automatically done in workflow)
- [ ] Tesseract data files available (eng.traineddata)
- [ ] Frontend URL updated in appsettings.json (`https://propeliq.infinityfree.me`)
- [ ] Database migrations applied to Supabase PostgreSQL
- [ ] Supabase Storage bucket `propeliq` created

### Optional Services

- [ ] Pusher account created and configured (if using real-time features)
- [ ] Upstash Redis configured (if using caching)

### After Deployment

- [ ] Verify backend URL is accessible
- [ ] Check application logs in MonsterASP.NET control panel
- [ ] Test health check endpoint: `https://your-backend-url/health`
- [ ] Test API authentication with JWT
- [ ] Verify email sending works (register new user)
- [ ] Test file upload functionality
- [ ] Verify database connectivity

---

## 🚀 Deployment Process

### Manual Deployment

1. Go to **GitHub Actions** tab
2. Select **"CD - Deploy Backend"** workflow
3. Click **"Run workflow"**
4. Select `main` branch
5. Click **"Run workflow"** button

### Automatic Deployment

Deployment automatically triggers when:
- CI Pipeline completes successfully on `main` branch
- All tests pass

---

## 📊 Deployment Steps (Automated)

The workflow performs these steps:

1. ✅ Checkout repository
2. ✅ Setup .NET 8.0
3. ✅ Restore NuGet packages
4. ✅ Generate RSA keys for JWT
5. ✅ Publish application (Release mode, win-x86)
6. ✅ Copy RSA keys to publish folder
7. ✅ Inject secrets into appsettings.json
8. ✅ Configure web.config with environment variables
9. ✅ Take app offline (upload app_offline.htm)
10. ✅ Wait for app pool recycle (15 seconds)
11. ✅ Upload all files via FTP
12. ✅ Upload RSA keys
13. ✅ Remove app_offline.htm (bring app online)
14. ✅ Create deployment summary

---

## 🔧 Environment Variables Injected

The following environment variables are automatically injected into `web.config`:

```xml
<environmentVariables>
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  <environmentVariable name="SmtpSettings__Password" value="***" />
  <environmentVariable name="Pusher__AppId" value="***" />
  <environmentVariable name="Pusher__Key" value="***" />
  <environmentVariable name="Pusher__Secret" value="***" />
  <environmentVariable name="GeminiAI__ApiKey" value="***" />
  <environmentVariable name="Supabase__ApiKey" value="***" />
  <environmentVariable name="RedisSettings__ConnectionString" value="***" />
</environmentVariables>
```

---

## 🛠️ Troubleshooting

### Deployment Fails

1. **Check GitHub Actions logs** for specific error messages
2. **Verify all secrets are set** in GitHub repository settings
3. **Check FTP credentials** are correct
4. **Ensure MonsterASP.NET has enough storage space**

### Application Won't Start

1. **Check web.config** has correct environment variables
2. **Verify database password** is correct
3. **Check application logs** in MonsterASP.NET control panel
4. **Ensure RSA keys** are properly copied to `wwwroot/rsa-keys/`

### Email Not Sending

1. **Verify SMTP__PASSWORD** is the correct Gmail App Password
2. **Check 2-Step Verification** is enabled on Gmail account
3. **Ensure daily limit** (500 emails) not exceeded

### File Uploads Failing

1. **Verify Supabase API key** is the service_role key (not anon key)
2. **Check storage bucket** `propeliq` exists
3. **Verify bucket permissions** allow backend uploads

---

## 📞 Support Resources

- **MonsterASP.NET:** Control panel → Support tickets
- **Supabase:** https://supabase.com/docs
- **Google Gemini:** https://ai.google.dev/docs
- **Gmail SMTP:** https://support.google.com/mail/answer/7126229
- **Pusher:** https://pusher.com/docs/channels
- **Upstash Redis:** https://docs.upstash.com/redis

---

## 🔒 Security Notes

1. **Never commit secrets** to version control
2. **Never expose service_role key** (Supabase) to frontend
3. **Rotate passwords regularly** (every 90 days recommended)
4. **Monitor failed login attempts** in application logs
5. **Use environment-specific configurations** (Development vs Production)
6. **Enable HTTPS only** in production (already configured)

---

## 📝 Configuration Files Reference

- **appsettings.json** - Production configuration with placeholders
- **appsettings.Development.json** - Development configuration (not deployed)
- **web.config** - IIS configuration (auto-generated, modified by workflow)
- **cd-backend.yml** - Deployment workflow definition

---

**Last Updated:** March 27, 2026  
**Maintained by:** CareSync AI Team
