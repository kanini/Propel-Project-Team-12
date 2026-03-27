# Pre-push CI validation script
Write-Host '========================================'
Write-Host 'Pre-Push CI Validation'
Write-Host '========================================'
Set-Location $PSScriptRoot
Write-Host '[1/4] Frontend Build'
Set-Location 'src/frontend'
npm run build
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host '[2/4] Frontend Lint'
npm run lint
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host '[3/4] Backend Build'
Set-Location '../backend'
dotnet build PatientAccess.sln --configuration Release --warnaserror
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host '[4/4] Backend Tests'
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj --configuration Release
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host 'All checks passed!'
