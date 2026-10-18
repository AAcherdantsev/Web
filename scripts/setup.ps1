# setup.ps1
# =============================================
# Environment setup script for Aspire projects
# Installs .NET 10 SDK and Docker Desktop
# =============================================

Write-Host "=== Setting up environment for Aspire project (Windows) ===" -ForegroundColor Cyan

# --- Check and install .NET SDK ---
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $version = dotnet --version
    Write-Host ".NET SDK found: $version" -ForegroundColor Green
} else {
    Write-Host ".NET SDK not found. Installing .NET 10 SDK..." -ForegroundColor Yellow

    $dotnetInstallerUrl = "https://dot.net/v1/dotnet-install.ps1"
    $dotnetInstallerPath = "$env:TEMP\dotnet-install.ps1"
    Invoke-WebRequest $dotnetInstallerUrl -OutFile $dotnetInstallerPath

    & powershell -NoProfile -ExecutionPolicy Bypass -File $dotnetInstallerPath -Channel 10.0 -InstallDir "$env:USERPROFILE\.dotnet"

    # Add .NET to PATH if not already present
    $dotnetPath = "$env:USERPROFILE\.dotnet"
    if (-not ($env:PATH -match [regex]::Escape($dotnetPath))) {
        Write-Host "Adding .NET SDK to PATH..."
        [Environment]::SetEnvironmentVariable("PATH", $env:PATH + ";$dotnetPath", [EnvironmentVariableTarget]::User)
        # Also update the current session, so that the dotnet restore below can find dotnet
        $env:PATH += ";$dotnetPath"
    }

    Remove-Item $dotnetInstallerPath -Force
    Write-Host ".NET SDK installed successfully." -ForegroundColor Green
}

Write-Host ""

# --- Check and install Docker Desktop ---
if (Get-Command docker -ErrorAction SilentlyContinue) {
    Write-Host "Docker found: $(docker --version)" -ForegroundColor Green
} else {
    Write-Host "Docker not found. Installing Docker Desktop..." -ForegroundColor Yellow

    $dockerInstallerUrl = "https://desktop.docker.com/win/stable/Docker%20Desktop%20Installer.exe"
    $dockerInstallerPath = "$env:TEMP\DockerInstaller.exe"

    Write-Host "Downloading Docker Desktop installer..."
    Invoke-WebRequest $dockerInstallerUrl -OutFile $dockerInstallerPath

    Write-Host "Running Docker Desktop installer (this may take several minutes)..."
    Start-Process -FilePath $dockerInstallerPath -ArgumentList "install", "--quiet", "--accept-license" -Wait

    Remove-Item $dockerInstallerPath -Force
    Write-Host "Docker Desktop installed successfully." -ForegroundColor Green
}

# --- Restore NuGet packages ---
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan

$solution = Join-Path $PSScriptRoot "..\Web.slnx"

if (Test-Path $solution) {
    dotnet restore $solution
} else {
    Write-Host "No .slnx file found. Skipping 'dotnet restore'." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Setup complete! ===" -ForegroundColor Cyan
Write-Host "If Docker was installed for the first time, please restart your computer." -ForegroundColor Yellow
