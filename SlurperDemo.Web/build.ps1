<#
.SYNOPSIS
    Build script for Slurper Demo Web Application client-side assets

.DESCRIPTION
    This script automates the process of installing npm dependencies and building
    client-side assets using webpack. It handles both production and development builds.

.PARAMETER Mode
    Build mode: 'production' (default), 'development', or 'watch'

.PARAMETER Clean
    Clean node_modules and build output before building

.PARAMETER Install
    Force reinstall of npm packages

.EXAMPLE
    .\build.ps1
    Runs a production build

.EXAMPLE
    .\build.ps1 -Mode development
    Runs a development build with source maps

.EXAMPLE
    .\build.ps1 -Clean
    Cleans and rebuilds everything

.EXAMPLE
    .\build.ps1 -Mode watch
    Watches for file changes and rebuilds automatically
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('production', 'development', 'watch')]
    [string]$Mode = 'production',

    [Parameter()]
    [switch]$Clean,

    [Parameter()]
    [switch]$Install
)

# Set error action preference
$ErrorActionPreference = 'Stop'

# Script variables
$ScriptRoot = $PSScriptRoot
$ProjectRoot = $ScriptRoot
$NodeModulesPath = Join-Path $ProjectRoot 'node_modules'
$DistPath = Join-Path $ProjectRoot 'wwwroot\dist'

# Color output functions
function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "→ $Message" -ForegroundColor Blue
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

# Check if Node.js is installed
function Test-NodeInstalled {
    Write-Info "Checking Node.js installation..."
    
    try {
        $nodeVersion = node --version
        $npmVersion = npm --version
        Write-Success "Node.js $nodeVersion and npm $npmVersion are installed"
        return $true
    }
    catch {
        Write-ErrorMessage "Node.js is not installed or not in PATH"
        Write-Info "Please install Node.js from https://nodejs.org/ (v18 or higher)"
        return $false
    }
}

# Clean build artifacts
function Invoke-Clean {
    Write-Header "Cleaning build artifacts"
    
    # Clean node_modules if requested
    if ($Clean -and (Test-Path $NodeModulesPath)) {
        Write-Info "Removing node_modules..."
        Remove-Item -Path $NodeModulesPath -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "Removed node_modules"
    }
    
    # Clean dist folder
    if (Test-Path $DistPath) {
        Write-Info "Removing dist folder..."
        Remove-Item -Path $DistPath -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "Removed dist folder"
    }
    
    # Clean package-lock.json if forcing reinstall
    if ($Install) {
        $packageLock = Join-Path $ProjectRoot 'package-lock.json'
        if (Test-Path $packageLock) {
            Write-Info "Removing package-lock.json..."
            Remove-Item -Path $packageLock -Force -ErrorAction SilentlyContinue
            Write-Success "Removed package-lock.json"
        }
    }
}

# Install npm packages
function Install-NpmPackages {
    Write-Header "Installing npm packages"
    
    # Check if node_modules exists and Install flag is not set
    if ((Test-Path $NodeModulesPath) -and -not $Install -and -not $Clean) {
        Write-Info "node_modules already exists, skipping install (use -Install to force reinstall)"
        return
    }
    
    Write-Info "Running npm install..."
    
    try {
        npm install
        Write-Success "npm packages installed successfully"
    }
    catch {
        Write-ErrorMessage "Failed to install npm packages"
        throw
    }
}

# Run webpack build
function Invoke-WebpackBuild {
    param([string]$BuildMode)
    
    Write-Header "Building client-side assets ($BuildMode mode)"
    
    $buildCommand = switch ($BuildMode) {
        'production' { 'npm run build' }
        'development' { 'npm run build:dev' }
        'watch' { 'npm run watch' }
    }
    
    Write-Info "Running: $buildCommand"
    
    try {
        Invoke-Expression $buildCommand
        
        if ($BuildMode -ne 'watch') {
            Write-Success "Build completed successfully"
            
            # Show output files
            if (Test-Path $DistPath) {
                Write-Info "`nGenerated files:"
                Get-ChildItem -Path $DistPath -Recurse -File | ForEach-Object {
                    $size = [math]::Round($_.Length / 1KB, 2)
                    Write-Host "  $($_.Name) ($size KB)" -ForegroundColor Gray
                }
            }
        }
    }
    catch {
        Write-ErrorMessage "Build failed"
        throw
    }
}

# Display build summary
function Show-BuildSummary {
    Write-Header "Build Summary"
    
    Write-Info "Mode: $Mode"
    Write-Info "Project: SlurperDemo.Web"
    
    if (Test-Path $DistPath) {
        $totalSize = (Get-ChildItem -Path $DistPath -Recurse -File | Measure-Object -Property Length -Sum).Sum
        $totalSizeKB = [math]::Round($totalSize / 1KB, 2)
        Write-Info "Total output size: $totalSizeKB KB"
    }
    
    Write-Success "`nBuild process completed!"
    Write-Info "Next steps:"
    Write-Host "  1. Run the .NET application: dotnet run" -ForegroundColor Gray
    Write-Host "  2. Browse to: http://localhost:5000" -ForegroundColor Gray
}

# Main execution
try {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    Write-Header "Slurper Demo Web - Client Build Script"
    
    # Verify Node.js installation
    if (-not (Test-NodeInstalled)) {
        exit 1
    }
    
    # Clean if requested
    if ($Clean) {
        Invoke-Clean
    }
    
    # Install dependencies
    Install-NpmPackages
    
    # Run build
    Invoke-WebpackBuild -BuildMode $Mode
    
    # Show summary (unless in watch mode)
    if ($Mode -ne 'watch') {
        $stopwatch.Stop()
        $elapsed = $stopwatch.Elapsed.TotalSeconds
        Write-Host "`nBuild time: $([math]::Round($elapsed, 2)) seconds" -ForegroundColor Cyan
        Show-BuildSummary
    }
    
    exit 0
}
catch {
    Write-ErrorMessage "`nBuild failed with error:"
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
}
