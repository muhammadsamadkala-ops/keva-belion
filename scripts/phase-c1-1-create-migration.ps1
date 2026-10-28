# Step 1 of 2. Creates the NEW migration "AddAdminUsers" (code files + SQL preview). Changes NO database.
# It does not touch or re-run your existing InitialCreate migration.
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed." }
dotnet ef migrations add AddAdminUsers --project src\KivaBelion.API --output-dir Data\Migrations
if ($LASTEXITCODE -ne 0) { throw "dotnet ef migrations add failed. Copy the error text to Claude." }
dotnet ef migrations script InitialCreate AddAdminUsers --project src\KivaBelion.API --output admin-users-preview.sql
if ($LASTEXITCODE -ne 0) { throw "Could not create the SQL preview." }
Write-Host "Created the migration. Read admin-users-preview.sql: it should only CREATE TABLE/INDEX for AdminUsers and insert one history row." -ForegroundColor Green
Write-Host "No database was changed. Next: phase-c1-2-apply-migration.ps1"
