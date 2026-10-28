# Step 1 of 2. Creates the migration FILES (C# code) and a SQL preview. It does NOT change any database.
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)

if (Test-Path .\src\KivaBelion.API\Data\Migrations) {
    Write-Host "A Migrations folder already exists. Not creating another initial migration." -ForegroundColor Yellow
    Write-Host "Existing migrations:"; Get-ChildItem .\src\KivaBelion.API\Data\Migrations -Filter *.cs | Where-Object Name -notlike '*Designer*' | Select-Object -ExpandProperty Name
    return
}

dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed (NuGet/internet problem?)." }

dotnet ef migrations add InitialCreate --project src\KivaBelion.API --output-dir Data\Migrations
if ($LASTEXITCODE -ne 0) { throw "dotnet ef migrations add failed. Copy the error text to Claude." }

dotnet ef migrations script --idempotent --project src\KivaBelion.API --output migration-preview.sql
if ($LASTEXITCODE -ne 0) { throw "Could not create the SQL preview." }

Write-Host "Created src\KivaBelion.API\Data\Migrations and migration-preview.sql (open it to read what will be created)." -ForegroundColor Green
Write-Host "No database was changed. Next: phase-b-2-apply-migration.ps1"
