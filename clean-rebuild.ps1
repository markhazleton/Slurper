Write-Host "Starting clean rebuild process for WebSpark.Slurper..." -ForegroundColor Cyan

# Define solution directory
$solutionDir = $PSScriptRoot

# Remove all bin and obj directories
Write-Host "Removing all bin and obj directories..." -ForegroundColor Yellow
Get-ChildItem -Path $solutionDir -Include bin, obj -Directory -Recurse | ForEach-Object {
    Write-Host "Removing $_" -ForegroundColor Gray
    Remove-Item -Path $_ -Recurse -Force
}

# Delete any existing NuGet packages
Write-Host "Removing any existing NuGet packages..." -ForegroundColor Yellow
Get-ChildItem -Path $solutionDir -Include *.nupkg, *.snupkg -Recurse | ForEach-Object {
    Write-Host "Removing $_" -ForegroundColor Gray
    Remove-Item -Path $_ -Force
}

# Build the solution
Write-Host "Building solution..." -ForegroundColor Green
dotnet build "$solutionDir\WebSpark.Slurper.sln" -c Release

# Verify NuGet packages
Write-Host "Verifying NuGet packages..." -ForegroundColor Cyan
$nugetPackages = Get-ChildItem -Path "$solutionDir" -Include *.nupkg -Recurse

foreach ($package in $nugetPackages) {
    Write-Host "Generated package: $package" -ForegroundColor Green
    if ($package.Name -like "Dandraka*") {
        Write-Host "Warning: Dandraka package still present: $($package.Name)" -ForegroundColor Red
    }
}

Write-Host "Clean rebuild process completed." -ForegroundColor Cyan