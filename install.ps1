<#
.SYNOPSIS
    Installs Box ID (MachineLabel) from the latest GitHub release.
.DESCRIPTION
    Downloads the latest release from GitHub, extracts it to %LOCALAPPDATA%\BoxId,
    and adds it to the user PATH. Run with:
      irm https://raw.githubusercontent.com/angshuman/box-id/main/install.ps1 | iex
#>

$ErrorActionPreference = 'Stop'
$repo = 'angshuman/box-id'
$installDir = Join-Path $env:LOCALAPPDATA 'BoxId'

# Detect architecture
$arch = if ([Environment]::Is64BitOperatingSystem) {
    if ($env:PROCESSOR_ARCHITECTURE -eq 'ARM64') { 'win-arm64' } else { 'win-x64' }
} else {
    Write-Error 'Box ID requires a 64-bit version of Windows.'
    return
}

$assetName = "MachineLabel-$arch.zip"

Write-Host ""
Write-Host "  📦 Box ID Installer" -ForegroundColor Cyan
Write-Host "  ===================" -ForegroundColor Cyan
Write-Host ""

# Get latest release
Write-Host "  → Finding latest release..." -ForegroundColor Gray
$release = Invoke-RestMethod "https://api.github.com/repos/$repo/releases/latest" -ErrorAction Stop
$asset = $release.assets | Where-Object { $_.name -eq $assetName }

if (-not $asset) {
    Write-Error "Could not find $assetName in release $($release.tag_name). Available: $($release.assets.name -join ', ')"
    return
}

$version = $release.tag_name
$downloadUrl = $asset.browser_download_url

Write-Host "  → Downloading $assetName ($version)..." -ForegroundColor Gray

# Download
$zipPath = Join-Path $env:TEMP "boxid-$version.zip"
Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath -UseBasicParsing

# Extract
Write-Host "  → Installing to $installDir..." -ForegroundColor Gray

if (Test-Path $installDir) {
    # Stop running instance
    Get-Process MachineLabel -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
    Remove-Item "$installDir\*" -Recurse -Force
} else {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

Expand-Archive -Path $zipPath -DestinationPath $installDir -Force
Remove-Item $zipPath -Force

# Add to PATH if not already there
$userPath = [Environment]::GetEnvironmentVariable('PATH', 'User')
if ($userPath -notlike "*$installDir*") {
    Write-Host "  → Adding to PATH..." -ForegroundColor Gray
    [Environment]::SetEnvironmentVariable('PATH', "$userPath;$installDir", 'User')
    $env:PATH = "$env:PATH;$installDir"
}

Write-Host ""
Write-Host "  ✅ Box ID $version installed!" -ForegroundColor Green
Write-Host ""
Write-Host "  Run it:" -ForegroundColor White
Write-Host "    MachineLabel.exe" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Or restart your terminal and run:" -ForegroundColor White
Write-Host "    MachineLabel" -ForegroundColor Yellow
Write-Host ""
Write-Host "  To uninstall:" -ForegroundColor Gray
Write-Host "    Remove-Item '$installDir' -Recurse -Force" -ForegroundColor Gray
Write-Host ""

# Offer to launch now
$launch = Read-Host "  Launch Box ID now? (Y/n)"
if ($launch -ne 'n') {
    Start-Process (Join-Path $installDir 'MachineLabel.exe')
    Write-Host "  🚀 Launched! Look for the label on your taskbar." -ForegroundColor Cyan
}
