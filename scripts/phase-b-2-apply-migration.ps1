# Step 2 of 2. APPLIES the migration to the database in your User Secrets (kivabelion_dev).
# It only CREATES tables. It never drops or deletes anything.
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)

$answer = Read-Host "This will create the Phase B tables in your local development database. Type YES to continue"
if ($answer -ne 'YES') { Write-Host "Cancelled. Nothing changed."; return }

dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed." }
dotnet ef database update --project src\KivaBelion.API
if ($LASTEXITCODE -ne 0) { throw "dotnet ef database update failed. Copy the error text to Claude." }
Write-Host "Done. Run scripts\phase-b-3-verify-schema.ps1 to check." -ForegroundColor Green
