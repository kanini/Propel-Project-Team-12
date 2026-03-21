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
$securityDir = Join-Path $PSScriptRoot "..\security\rsa-keys"
$privateKeyPath = Join-Path $securityDir "private-key.xml"
$publicKeyPath = Join-Path $securityDir "public-key.xml"

# Ensure directory exists
if (-not (Test-Path $securityDir)) {
    New-Item -ItemType Directory -Path $securityDir -Force | Out-Null
}

# Save keys to files
Set-Content -Path $privateKeyPath -Value $privateKeyXml -Encoding UTF8
Set-Content -Path $publicKeyPath -Value $publicKeyXml -Encoding UTF8

Write-Host "✓ Private key saved to: $privateKeyPath" -ForegroundColor Green
Write-Host "✓ Public key saved to: $publicKeyPath" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANT SECURITY NOTICE:" -ForegroundColor Yellow
Write-Host "- Keep private-key.xml secure and NEVER commit to version control" -ForegroundColor Yellow
Write-Host "- The .gitignore already excludes security/rsa-keys/*.xml" -ForegroundColor Yellow
Write-Host "- For production, store keys in Azure Key Vault or environment variables" -ForegroundColor Yellow
Write-Host ""
Write-Host "Key Generation Complete!" -ForegroundColor Cyan
