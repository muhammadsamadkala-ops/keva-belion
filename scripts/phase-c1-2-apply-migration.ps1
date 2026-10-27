# Step 2 of 2. Applies ONLY the pending migration (AddAdminUsers) to kivabelion_dev. Creates one table. Drops/deletes nothing.
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
$a = Read-Host "This will create the AdminUsers table in your local development database. Type YES to continue"
if ($a -ne 'YES') { Write-Host "Cancelled. Nothing changed."; return }
dotnet ef database update --project src\KivaBelion.API
if ($LASTEXITCODE -ne 0) { throw "dotnet ef database update failed. Copy the error text to Claude." }
Write-Host "Done. Next: scripts\set-jwt-secret.ps1, then scripts\create-admin.ps1" -ForegroundColor Green
