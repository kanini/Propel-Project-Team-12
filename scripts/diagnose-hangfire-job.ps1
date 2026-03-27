#!/usr/bin/env pwsh
# Hangfire "Always Enqueued" Job Diagnostic & Fix Script
# Use this when jobs are stuck in Enqueued state and never process

param(
    [Parameter(Mandatory=$false)]
    [int]$JobId = 400,
    
    [Parameter(Mandatory=$false)]
    [string]$DocumentId = "062e30f2-6b6d-4bac-8f6d-01ca3ead3017"
)

Write-Host "🔍 Hangfire 'Always Enqueued' Diagnostic Tool" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Job ID: $JobId" -ForegroundColor Yellow
Write-Host "Document ID: $DocumentId" -ForegroundColor Yellow

# Check 1: Backend process is running
Write-Host "`n[1/6] Checking if backend is running..." -ForegroundColor Cyan
$backendProcess = Get-Process -Name "PatientAccess.Web" -ErrorAction SilentlyContinue
if ($backendProcess) {
    Write-Host "✅ Backend process is running (PID: $($backendProcess.Id))" -ForegroundColor Green
    Write-Host "   Started: $($backendProcess.StartTime)" -ForegroundColor Gray
} else {
    Write-Host "❌ Backend process NOT running" -ForegroundColor Red
    Write-Host "   Fix: cd src\backend\PatientAccess.Web; dotnet run" -ForegroundColor Yellow
    exit 1
}

# Check 2: Backend API is responsive
Write-Host "`n[2/6] Checking if backend API is responsive..." -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✅ Backend API is responding" -ForegroundColor Green
    Write-Host "   Status: $($health.status)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Backend API not responding" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "   Backend may still be starting up. Wait 30 seconds and try again." -ForegroundColor Yellow
    exit 1
}

# Check 3: Hangfire dashboard accessible
Write-Host "`n[3/6] Checking Hangfire dashboard..." -ForegroundColor Cyan
try {
    $hangfireResponse = Invoke-WebRequest -Uri "http://localhost:5000/hangfire" -Method GET -TimeoutSec 5 -ErrorAction Stop
    if ($hangfireResponse.StatusCode -eq 200) {
        Write-Host "✅ Hangfire dashboard is accessible" -ForegroundColor Green
        Write-Host "   URL: http://localhost:5000/hangfire" -ForegroundColor Gray
    }
} catch {
    Write-Host "⚠️  Hangfire dashboard not accessible" -ForegroundColor Yellow
    Write-Host "   This might be normal if authentication is required" -ForegroundColor Gray
}

# Check 4: Gemini API key is set
Write-Host "`n[4/6] Checking environment variables..." -ForegroundColor Cyan
if ($env:GEMINIAI__APIKEY) {
    Write-Host "✅ GEMINIAI__APIKEY is set" -ForegroundColor Green
    $keyPreview = $env:GEMINIAI__APIKEY.Substring(0, [Math]::Min(10, $env:GEMINIAI__APIKEY.Length)) + "..."
    Write-Host "   Key preview: $keyPreview" -ForegroundColor Gray
} else {
    Write-Host "❌ GEMINIAI__APIKEY NOT set" -ForegroundColor Red
    Write-Host "   This will cause jobs to fail!" -ForegroundColor Yellow
    Write-Host "   Fix: `$env:GEMINIAI__APIKEY = 'YOUR_API_KEY'" -ForegroundColor Yellow
    Write-Host "   Get key: https://aistudio.google.com/app/apikey" -ForegroundColor Cyan
}

# Check 5: Database queries for job status
Write-Host "`n[5/6] Generating database diagnostic queries..." -ForegroundColor Cyan

Write-Host "Copy and run this SQL in Supabase SQL Editor:" -ForegroundColor Yellow
Write-Host ""
Write-Host "-- Job #$JobId Status" -ForegroundColor Gray
Write-Host "SELECT " -ForegroundColor White
Write-Host "    j.id," -ForegroundColor White
Write-Host "    j.createdat as job_created," -ForegroundColor White
Write-Host "    s.name as current_state," -ForegroundColor White
Write-Host "    s.reason as state_reason," -ForegroundColor White
Write-Host "    s.createdat as state_time," -ForegroundColor White
Write-Host "    j.invocationdata::json->>'Type' as job_type," -ForegroundColor White
Write-Host "    j.invocationdata::json->>'Method' as job_method" -ForegroundColor White
Write-Host "FROM hangfire.job j" -ForegroundColor White
Write-Host "LEFT JOIN hangfire.state s ON j.stateid = s.id" -ForegroundColor White
Write-Host "WHERE j.id = $JobId;" -ForegroundColor White
Write-Host ""
Write-Host "-- All state transitions for Job #$JobId" -ForegroundColor Gray
Write-Host "SELECT " -ForegroundColor White
Write-Host "    s.id," -ForegroundColor White
Write-Host "    s.name as state," -ForegroundColor White
Write-Host "    s.reason," -ForegroundColor White
Write-Host "    s.createdat," -ForegroundColor White
Write-Host "    s.data::json->>'ExceptionType' as exception_type," -ForegroundColor White
Write-Host "    s.data::json->>'ExceptionMessage' as error_message" -ForegroundColor White
Write-Host "FROM hangfire.state s" -ForegroundColor White
Write-Host "WHERE s.jobid = $JobId" -ForegroundColor White
Write-Host "ORDER BY s.createdat DESC" -ForegroundColor White
Write-Host "LIMIT 10;" -ForegroundColor White
Write-Host ""
Write-Host "-- Check Hangfire servers" -ForegroundColor Gray
Write-Host "SELECT " -ForegroundColor White
Write-Host "    id," -ForegroundColor White
Write-Host "    data::json->>'WorkerCount' as worker_count," -ForegroundColor White
Write-Host "    data::json->>'Queues' as queues," -ForegroundColor White
Write-Host "    lastheartbeat," -ForegroundColor White
Write-Host "    EXTRACT(EPOCH FROM (NOW() - lastheartbeat)) as seconds_since_heartbeat" -ForegroundColor White
Write-Host "FROM hangfire.server" -ForegroundColor White
Write-Host "ORDER BY lastheartbeat DESC;" -ForegroundColor White
Write-Host ""
Write-Host "-- Document status" -ForegroundColor Gray
Write-Host "SELECT " -ForegroundColor White
Write-Host '    "DocumentId"::text,' -ForegroundColor White
Write-Host '    "FileName",' -ForegroundColor White
Write-Host '    "ProcessingStatus",' -ForegroundColor White
Write-Host '    CASE "ProcessingStatus"' -ForegroundColor White
Write-Host "        WHEN 1 THEN 'Uploaded'" -ForegroundColor White
Write-Host "        WHEN 2 THEN 'Processing'" -ForegroundColor White
Write-Host "        WHEN 3 THEN 'Completed'" -ForegroundColor White
Write-Host "        WHEN 4 THEN 'Failed'" -ForegroundColor White
Write-Host "    END as status_text," -ForegroundColor White
Write-Host '    "ErrorMessage",' -ForegroundColor White
Write-Host '    "UploadedAt",' -ForegroundColor White
Write-Host '    "ProcessedAt",' -ForegroundColor White
Write-Host '    EXTRACT(EPOCH FROM (NOW() - "UploadedAt")) / 60 as minutes_since_upload' -ForegroundColor White
Write-Host 'FROM "ClinicalDocuments"' -ForegroundColor White
Write-Host "WHERE `"DocumentId`" = '$DocumentId';" -ForegroundColor White

# Check 6: Provide action items
Write-Host "`n[6/6] Recommended Actions" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Gray

Write-Host "`n📋 IMMEDIATE ACTIONS:" -ForegroundColor Yellow
Write-Host "1. Open Hangfire Dashboard: http://localhost:5000/hangfire" -ForegroundColor White
Write-Host "   - Go to 'Servers' tab" -ForegroundColor Gray
Write-Host "   - Verify: Should show 1 server with 'Workers: 2'" -ForegroundColor Gray
Write-Host "   - If Workers = 0 or no servers: RESTART BACKEND" -ForegroundColor Red

Write-Host "`n2. Check Job #$JobId in Hangfire:" -ForegroundColor White
Write-Host "   - Go to 'Jobs' > 'Enqueued'" -ForegroundColor Gray
Write-Host "   - Find Job #$JobId" -ForegroundColor Gray
Write-Host "   - If still enqueued after 1 minute: Check backend logs for errors" -ForegroundColor Gray

Write-Host "`n3. Run diagnostic SQL queries (shown above)" -ForegroundColor White
Write-Host "   - Check if job has any Failed states" -ForegroundColor Gray
Write-Host "   - Check error messages" -ForegroundColor Gray
Write-Host "   - Verify Hangfire servers are active (lastheartbeat < 30 seconds)" -ForegroundColor Gray

Write-Host "`n🔧 IF JOB REMAINS STUCK:" -ForegroundColor Yellow

Write-Host "`nOption A: Restart Backend Properly" -ForegroundColor Cyan
Write-Host "1. Set Gemini API key:" -ForegroundColor White
Write-Host '   $env:GEMINIAI__APIKEY = "YOUR_API_KEY"' -ForegroundColor Gray
Write-Host "2. Stop backend (Ctrl+C)" -ForegroundColor White
Write-Host "3. Restart: dotnet run" -ForegroundColor White
Write-Host "4. Wait for 'Hangfire Server started' log" -ForegroundColor White
Write-Host "5. Job should auto-process within 30 seconds" -ForegroundColor White

Write-Host "`nOption B: Delete and Re-trigger Job" -ForegroundColor Cyan
Write-Host "1. In Hangfire Dashboard, navigate to Jobs, then Enqueued" -ForegroundColor White
Write-Host "2. Click 'Delete' on Job #$JobId" -ForegroundColor White
Write-Host "3. Manually trigger new job:" -ForegroundColor White
Write-Host "   # PowerShell command to re-enqueue" -ForegroundColor Gray
Write-Host "   `$docId = `"$DocumentId`"" -ForegroundColor Gray
Write-Host '   Invoke-RestMethod -Uri "http://localhost:5000/api/documents/$docId/retry" -Method POST -Headers @{"Authorization"="Bearer YOUR_JWT_TOKEN"}' -ForegroundColor Gray

Write-Host "`nOption C: Force Job Processing (Advanced)" -ForegroundColor Cyan
Write-Host "   -- SQL: Reset document status and create new job" -ForegroundColor Gray
Write-Host '   UPDATE "ClinicalDocuments"' -ForegroundColor Gray
Write-Host '   SET "ProcessingStatus" = 1,' -ForegroundColor Gray
Write-Host '       "ErrorMessage" = NULL,' -ForegroundColor Gray
Write-Host '       "ProcessedAt" = NULL' -ForegroundColor Gray
Write-Host "   WHERE `"DocumentId`" = '$DocumentId';" -ForegroundColor Gray

# Monitor instructions
Write-Host "`n👀 MONITORING:" -ForegroundColor Yellow
Write-Host "After applying fixes, monitor job progress:" -ForegroundColor White
Write-Host "1. Hangfire Dashboard, then Jobs, then Processing (job should appear here)" -ForegroundColor Gray
Write-Host "2. Backend console logs (watch for 'Starting document processing')" -ForegroundColor Gray
Write-Host "3. Database query to check ProcessingStatus changes:" -ForegroundColor Gray
Write-Host '   SELECT "ProcessingStatus", "ErrorMessage"' -ForegroundColor Gray
Write-Host '   FROM "ClinicalDocuments"' -ForegroundColor Gray
Write-Host "   WHERE `"DocumentId`" = '$DocumentId';" -ForegroundColor Gray

Write-Host "`n✅ EXPECTED OUTCOME:" -ForegroundColor Green
Write-Host "Job #$JobId moves from: Enqueued to Processing to Succeeded" -ForegroundColor White
Write-Host "ProcessingStatus changes: 1 (Uploaded) to 2 (Processing) to 3 (Completed)" -ForegroundColor White
Write-Host "Hangfire shows job in 'Succeeded' tab with completion time" -ForegroundColor White

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Diagnostic completed. Review results above." -ForegroundColor Green
Write-Host "If issue persists, check backend logs for exceptions." -ForegroundColor Yellow
