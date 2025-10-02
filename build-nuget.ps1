#!/usr/bin/env pwsh

# Build and pack NuGet package for ZauberCMS.RTE

Write-Host "Building ZauberCMS.RTE..." -ForegroundColor Cyan

# Step 1: Build first (creates the DLLs)
dotnet build ZauberCMS.RTE/ZauberCMS.RTE.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""
Write-Host "Packing NuGet package..." -ForegroundColor Cyan

# Step 2: Then pack (creates the NuGet package)
dotnet pack ZauberCMS.RTE/ZauberCMS.RTE.csproj -c Release --output nupkgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Pack failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "NuGet package created successfully in ./nupkgs/" -ForegroundColor Green

