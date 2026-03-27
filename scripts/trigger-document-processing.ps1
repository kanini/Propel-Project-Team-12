#!/usr/bin/env pwsh
# Manual Document Processing Trigger Script
# Use this to manually trigger processing for stuck documents (ProcessingStatus = 1)

param(
    [Parameter(Mandatory=$false)]
    [string]$DocumentId,
    
    [Parameter(Mandatory=$false)]
    [string]$JwtToken
)

Write-Host "🔧 Manual Document Processing Trigger" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Get stuck documents if no DocumentId provided
if (-not $DocumentId) {
    Write-Host "`n📋 Fetching stuck documents (ProcessingStatus = 1)..." -ForegroundColor Yellow
    
    $query = @"
SELECT 
    "DocumentId"::text,
    "FileName",
    "UploadedAt",
    EXTRACT(EPOCH FROM (NOW() - "UploadedAt")) / 60 as minutes_stuck
FROM "ClinicalDocuments"
WHERE "ProcessingStatus" = 1
ORDER BY "UploadedAt" DESC
LIMIT 5;
"@

    Write-Host "Run this SQL in Supabase SQL Editor to find stuck documents:" -ForegroundColor Cyan
    Write-Host $query -ForegroundColor White
    
    $inputDocId = Read-Host "`nEnter DocumentId to process (or press Enter to exit)"
    if ([string]::IsNullOrWhiteSpace($inputDocId)) {
        Write-Host "Exiting..." -ForegroundColor Gray
        exit 0
    }
    $DocumentId = $inputDocId
}

Write-Host "`n📄 Processing Document: $DocumentId" -ForegroundColor Green

# Step 2: Check backend is running
Write-Host "`n🔍 Checking backend status..." -ForegroundColor Yellow
try {
    $healthCheck = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET -TimeoutSec 5
    Write-Host "✅ Backend is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Backend is not running or not reachable" -ForegroundColor Red
    Write-Host "Start backend with: cd src/backend/PatientAccess.Web; dotnet run" -ForegroundColor Yellow
    exit 1
}

# Step 3: Check Gemini API key
Write-Host "`n🔑 Checking Gemini API Key..." -ForegroundColor Yellow
if (-not $env:GEMINIAI__APIKEY) {
    Write-Host "❌ GEMINIAI__APIKEY not set" -ForegroundColor Red
    Write-Host "Fix: `$env:GEMINIAI__APIKEY = 'YOUR_API_KEY'" -ForegroundColor Yellow
    Write-Host "Get key from: https://aistudio.google.com/app/apikey" -ForegroundColor Cyan
    $continue = Read-Host "`nContinue anyway? Jobs will fail without API key (y/N)"
    if ($continue -ne "y") {
        exit 1
    }
} else {
    Write-Host "✅ Gemini API key is set" -ForegroundColor Green
}

# Step 4: Verify document exists
Write-Host "`n📊 Verifying document in database..." -ForegroundColor Yellow
$verifyQuery = @"
SELECT 
    "DocumentId"::text,
    "FileName",
    "ProcessingStatus",
    "ErrorMessage"
FROM "ClinicalDocuments"
WHERE "DocumentId" = '$DocumentId';
"@

Write-Host "Run this SQL to verify document exists:" -ForegroundColor Cyan
Write-Host $verifyQuery -ForegroundColor White

# Step 5: Option to retry via API (requires JWT token)
if ($JwtToken) {
    Write-Host "`n🔄 Triggering retry via API..." -ForegroundColor Yellow
    try {
        $headers = @{
            "Authorization" = "Bearer $JwtToken"
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/documents/$DocumentId/retry" `
            -Method POST -Headers $headers
        
        Write-Host "✅ Retry job enqueued!" -ForegroundColor Green
        Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor White
    } catch {
        Write-Host "❌ Retry API call failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Note: Retry endpoint only works for Failed documents (ProcessingStatus = 4)" -ForegroundColor Yellow
    }
}

# Step 6: Manual Hangfire job enqueue (for developers)
Write-Host "`n🔧 Manual Job Enqueue Options:" -ForegroundColor Cyan
Write-Host "1. Via Hangfire Dashboard (Recommended)" -ForegroundColor White
Write-Host "   - Open: http://localhost:5000/hangfire" -ForegroundColor Yellow
Write-Host "   - Go to: Jobs > Recurring Jobs" -ForegroundColor Yellow
Write-Host "   - Click: Trigger Now on DocumentProcessingJob" -ForegroundColor Yellow

Write-Host "`n2. Via Database Update + New Upload" -ForegroundColor White
$manualEnqueueSql = @"
-- Reset document to allow re-processing
UPDATE "ClinicalDocuments"
SET 
    "ProcessingStatus" = 1,
    "ErrorMessage" = NULL,
    "ProcessedAt" = NULL,
    "UpdatedAt" = NOW()
WHERE "DocumentId" = '$DocumentId';

-- Then re-upload the document or manually trigger via code
"@
Write-Host $manualEnqueueSql -ForegroundColor Yellow

Write-Host "`n3. Via C# Code (in DocumentUploadService)" -ForegroundColor White
Write-Host '   Hangfire.BackgroundJob.Enqueue<DocumentProcessingJob>(job => job.Execute(Guid.Parse("' -NoNewline -ForegroundColor Yellow
Write-Host $DocumentId -NoNewline -ForegroundColor Cyan
Write-Host '")));' -ForegroundColor Yellow

# Step 7: Monitor progress
Write-Host "`n👀 Monitor Job Progress:" -ForegroundColor Cyan
Write-Host "1. Hangfire Dashboard: http://localhost:5000/hangfire" -ForegroundColor Yellow
Write-Host "   - Check: Jobs > Processing (should show job running)" -ForegroundColor White
Write-Host "   - Check: Jobs > Succeeded (job completed)" -ForegroundColor White
Write-Host "   - Check: Jobs > Failed (if job errored)" -ForegroundColor White

Write-Host "`n2. Database Query:" -ForegroundColor Yellow
$monitorQuery = @"
SELECT 
    "DocumentId"::text,
    "ProcessingStatus",
    CASE "ProcessingStatus"
        WHEN 1 THEN 'Uploaded'
        WHEN 2 THEN 'Processing'
        WHEN 3 THEN 'Completed'
        WHEN 4 THEN 'Failed'
    END as status_text,
    "ErrorMessage",
    "ProcessedAt"
FROM "ClinicalDocuments"
WHERE "DocumentId" = '$DocumentId';
"@
Write-Host $monitorQuery -ForegroundColor White

Write-Host "`n3. Check Extracted Data (after job completes):" -ForegroundColor Yellow
$extractedQuery = @"
SELECT 
    COUNT(*) as total_extracted,
    COUNT(DISTINCT "DataType") as data_types,
    COUNT(CASE WHEN "DataType" = 4 THEN 1 END) as diagnoses
FROM "ExtractedClinicalData"
WHERE "DocumentId" = '$DocumentId';
"@
Write-Host $extractedQuery -ForegroundColor White

Write-Host "`n✅ Script completed. Good luck!" -ForegroundColor Green
Write-Host "If you encounter errors, check:" -ForegroundColor Yellow
Write-Host "  - Backend logs (console output)" -ForegroundColor White
Write-Host "  - Hangfire dashboard (failed jobs)" -ForegroundColor White
Write-Host "  - Gemini API key is set and valid" -ForegroundColor White
