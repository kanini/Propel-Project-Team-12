# Generate RSA key pair for JWT RS256 signing
# This script creates 2048-bit RSA keys and exports them to XML format

$ErrorActionPreference = "Stop"

Write-Host "Generating RSA Key Pair for JWT RS256 Signing..." -ForegroundColor Cyan

# Create RSA instance with 2048-bit key
$rsa = [System.Security.Cryptography.RSA]::Create(2048)

# Export private key (includes both private and public parameters)
$privateKeyXml = $rsa.ToXmlString($true)

# Export public key (only public parameters)
$publicKeyXml = $rsa.ToXmlString($false)

# Define output paths
$backendDir = Join-Path $PSScriptRoot "..\src\backend\rsa-keys"
$webProjectDir = Join-Path $PSScriptRoot "..\src\backend\PatientAccess.Web\rsa-keys"
$privateKeyPath = Join-Path $backendDir "private-key.xml"
$publicKeyPath = Join-Path $backendDir "public-key.xml"
$webPrivateKeyPath = Join-Path $webProjectDir "private-key.xml"
$webPublicKeyPath = Join-Path $webProjectDir "public-key.xml"

# Ensure directories exist
if (-not (Test-Path $backendDir)) {
    New-Item -ItemType Directory -Path $backendDir -Force | Out-Null
}
if (-not (Test-Path $webProjectDir)) {
    New-Item -ItemType Directory -Path $webProjectDir -Force | Out-Null
}

# Save keys to backend/rsa-keys directory
Set-Content -Path $privateKeyPath -Value $privateKeyXml -Encoding UTF8
Set-Content -Path $publicKeyPath -Value $publicKeyXml -Encoding UTF8

# Copy keys to PatientAccess.Web/rsa-keys directory (where app runs)
Set-Content -Path $webPrivateKeyPath -Value $privateKeyXml -Encoding UTF8
Set-Content -Path $webPublicKeyPath -Value $publicKeyXml -Encoding UTF8

Write-Host "Private key saved to: $privateKeyPath" -ForegroundColor Green
Write-Host "Public key saved to: $publicKeyPath" -ForegroundColor Green
Write-Host "Keys also copied to: $webProjectDir" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANT SECURITY NOTICE:" -ForegroundColor Yellow
Write-Host "- Keep private-key.xml secure and NEVER commit to version control" -ForegroundColor Yellow
Write-Host "- The .gitignore already excludes src/backend/**/rsa-keys/*.xml" -ForegroundColor Yellow
Write-Host "- For production, store keys in Azure Key Vault or environment variables" -ForegroundColor Yellow
Write-Host ""
Write-Host "Key Generation Complete!" -ForegroundColor Cyan
