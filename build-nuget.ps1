#!/usr/bin/env pwsh
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Invoke-Step([string] $Name, [scriptblock] $Command) {
    Write-Host $Name -ForegroundColor Cyan
    & $Command
    if ($LASTEXITCODE -ne 0) {
        Write-Host "$Name failed" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Invoke-Step 'Building solution (Release)' { dotnet build ZauberRTE.sln -c Release }
Invoke-Step 'Running tests' { dotnet test --solution ZauberRTE.sln -c Release --no-build }
Invoke-Step 'Packing ZauberCMS.RTE' { dotnet pack ZauberCMS.RTE/ZauberCMS.RTE.csproj -c Release --no-build --output nupkgs }

Write-Host 'Packages created in ./nupkgs/' -ForegroundColor Green
Get-ChildItem nupkgs -Filter 'ZauberCMS.RTE.*' | Sort-Object LastWriteTime -Descending | Select-Object -First 2 | ForEach-Object { Write-Host "  $($_.Name)" }
