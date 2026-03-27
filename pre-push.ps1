# Pre-push CI validation script
Write-Host '========================================'
Write-Host 'Pre-Push CI Validation'
Write-Host '========================================'
Set-Location $PSScriptRoot

Write-Host '[1/7] Frontend Build' -ForegroundColor Yellow
Set-Location 'src/frontend'
npm run build
if ($LASTEXITCODE -ne 0) { Write-Host 'FAILED' -ForegroundColor Red; exit 1 }
Write-Host 'PASSED' -ForegroundColor Green

Write-Host '[2/7] Frontend Lint' -ForegroundColor Yellow
npm run lint
if ($LASTEXITCODE -ne 0) { Write-Host 'FAILED' -ForegroundColor Red; exit 1 }
Write-Host 'PASSED' -ForegroundColor Green

Write-Host '[3/7] Frontend Security - Dependency Audit' -ForegroundColor Yellow
npm audit --audit-level=high
if ($LASTEXITCODE -ne 0) { Write-Host 'FAILED - High-severity vulnerabilities found' -ForegroundColor Red; exit 1 }
Write-Host 'PASSED' -ForegroundColor Green

Write-Host '[4/7] Backend Build' -ForegroundColor Yellow
Set-Location '../backend'
dotnet build PatientAccess.sln --configuration Release --warnaserror
if ($LASTEXITCODE -ne 0) { Write-Host 'FAILED' -ForegroundColor Red; exit 1 }
Write-Host 'PASSED' -ForegroundColor Green

Write-Host '[5/7] Backend Tests' -ForegroundColor Yellow
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj --configuration Release
if ($LASTEXITCODE -ne 0) { Write-Host 'FAILED' -ForegroundColor Red; exit 1 }
Write-Host 'PASSED' -ForegroundColor Green

Write-Host '[6/7] Backend Security - Dependency Vulnerabilities' -ForegroundColor Yellow
dotnet list PatientAccess.sln package --vulnerable --include-transitive 2>&1 | Tee-Object -Variable auditOutput | Out-Null
if ($auditOutput -match 'has the following vulnerable packages') {
    Write-Host $auditOutput
    Write-Host 'FAILED - Vulnerable dependencies found' -ForegroundColor Red
    exit 1
}
Write-Host 'PASSED' -ForegroundColor Green

Write-Host '[7/7] Secrets Detection - GitLeaks' -ForegroundColor Yellow
Set-Location $PSScriptRoot

# Check if gitleaks is available
$gitleaksPath = $null
if (Test-Path "gitleaks-bin\gitleaks.exe") {
    $gitleaksPath = "gitleaks-bin\gitleaks.exe"
} elseif (Get-Command gitleaks -ErrorAction SilentlyContinue) {
    $gitleaksPath = "gitleaks"
} else {
    Write-Host 'SKIPPED - GitLeaks not found (install from https://github.com/gitleaks/gitleaks/releases)' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '========================================'
    Write-Host 'All checks passed!' -ForegroundColor Green
    Write-Host '========================================'
    exit 0
}

# Run GitLeaks
& $gitleaksPath detect --source . --no-git --verbose --redact --config .gitleaks.toml
if ($LASTEXITCODE -ne 0) {
    Write-Host 'FAILED - Secrets detected in code!' -ForegroundColor Red
    exit 1
}
Write-Host 'PASSED' -ForegroundColor Green

Write-Host ''
Write-Host '========================================'
Write-Host 'All checks passed!' -ForegroundColor Green
Write-Host '========================================'
