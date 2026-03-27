# GitHub Secrets Quick Setup Guide

## 🚀 Add These Secrets to Deploy

**Location:** GitHub Repository → Settings → Secrets and variables → Actions → New repository secret

---

## ✅ Required Secrets (Must Have)

```
Secret Name: FTP_BACKEND_HOST
Value: ftp://site59724.siteasp.net
```

```
Secret Name: FTP_BACKEND_USERNAME
Value: <your-monsterasp-ftp-username>
```

```
Secret Name: FTP_BACKEND_PASSWORD
Value: <your-monsterasp-ftp-password>
```

```
Secret Name: DB_PASSWORD
Value: <your-supabase-postgres-password>
```

```
Secret Name: SMTP__PASSWORD
Value: rjql ajfr xclz dtxa
(Your Gmail App Password - already generated)
```

```
Secret Name: GEMINI_API_KEY
Value: <your-google-gemini-api-key>
Get it from: https://aistudio.google.com/apikey
```

```
Secret Name: SUPABASE_API_KEY
Value: <your-supabase-service-role-key>
Get from: Supabase Dashboard → Settings → API → service_role (secret) key
```

---

## 📦 Optional Secrets (Can Add Later)

```
Secret Name: PUSHER_APP_ID
Value: <your-pusher-app-id>
Only needed if enabling real-time features
```

```
Secret Name: PUSHER_KEY
Value: <your-pusher-key>
Only needed if enabling real-time features
```

```
Secret Name: PUSHER_SECRET
Value: <your-pusher-secret>
Only needed if enabling real-time features
```

```
Secret Name: REDIS_CONNECTION_STRING
Value: redis://default:<password>@<host>:<port>
Only needed if using Redis caching
Get from: https://console.upstash.com
```

---

## 🎯 Quick Steps

1. Copy each secret name exactly as shown
2. Paste corresponding value
3. Click "Add secret"
4. Repeat for all required secrets
5. Run deployment workflow!

---

## ✅ Current Status

Based on your appsettings.json:

- ✅ **SMTP already configured:** mridhul35@gmail.com with app password
- ✅ **Database configured:** Supabase PostgreSQL connection ready
- ✅ **Frontend URL set:** https://propeliq.infinityfree.me
- ⚠️ **Pusher:** Disabled (can enable later)
- ⚠️ **Redis:** Enabled but needs connection string or disable it

---

## 🔍 Where to Find Values

### MonsterASP.NET (FTP)
- Login to control panel
- Go to FTP Accounts section
- Use the credentials shown

### Supabase (Database + Storage)
- **DB Password:** Dashboard → Settings → Database → Connection string
- **API Key:** Dashboard → Settings → API → service_role key

### Gmail (SMTP)
- Already have: `rjql ajfr xclz dtxa`
- If need new: Google Account → Security → App Passwords

### Google Gemini (AI)
- Visit: https://aistudio.google.com/apikey
- Create new API key

---

## 🚀 After Adding Secrets

Go to **GitHub Actions** → **CD - Deploy Backend** → **Run workflow** → Select `main` → **Run workflow**

Monitor the deployment logs to ensure success!
