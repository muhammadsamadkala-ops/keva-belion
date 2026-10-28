# Phase A Test Report

## What I actually ran
Nothing compiled or executed. My workspace has no .NET SDK and cannot reach NuGet.
I only re-read the code for mistakes by eye. **Treat all of it as untested.**

## What I did NOT run (written, waiting for your PC)
- `dotnet restore / build` — NOT RUN
- 10 automated tests in `tests/KivaBelion.API.Tests/FoundationTests.cs` — NOT RUN:
  live 200; ready 503 with unreachable DB (no secrets in body); ping flags; 404 problem JSON; safe 500 (no stack trace);
  400 validation; 200 valid; CORS allowed vs unknown origin; correlation id valid/invalid; security headers.
- Real database connection (`/health/ready` Healthy) — NOT RUN
- PowerShell scripts — NOT RUN

## Known risks
- Package versions are my best guess and may not exist: Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0,
  Microsoft.AspNetCore.OpenApi 10.0.0, Microsoft.AspNetCore.Mvc.Testing 10.0.0, xunit 2.9.2.
- Small compile errors are possible. Send me the exact output and I will fix it.
- Scripts assume PostgreSQL at `C:\Program Files\PostgreSQL\<ver>\bin\psql.exe`; port 5432 unverified.

## Please report back
Output of `dotnet build`, `dotnet test`, and what the three URLs show.

## Update 2 (after your first real run)
You reported: restore OK, API project compiled, test project failed with CS1061 (UseEnvironment) and NU1903 (Microsoft.OpenApi 2.0.0, GHSA-v5pm-xwqc-g5wc).
- Fix 1: ApiFactory.cs was missing `using Microsoft.AspNetCore.Hosting;` (the extension method lives in that namespace). Added. NOT compiled by the author.
- Fix 2: added a direct reference Microsoft.OpenApi 2.7.5 (advisory: affected 2.0.0-preview.11 to 2.7.4, fixed in 2.7.5). Warning not suppressed, auditing not disabled. NOT restored/built by the author.
