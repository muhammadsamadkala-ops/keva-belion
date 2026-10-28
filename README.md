# KIVA BELION — Phase A: Backend Foundation (local development only)

**Status: written but NOT compiled or tested by the author.** The author's workspace had no .NET SDK and no
NuGet access. Nothing below is claimed to work until YOU run it and see it work. Report any error text back.

What Phase A is: an ASP.NET Core Web API on .NET 10 with logging, validation, safe error handling, CORS,
a basic rate limit, health checks and automated tests. It has NO products, orders or database tables yet (Phase B).
It never touches your website, GitHub, or any online service. Orders and online payments are switched OFF.

Run every command in **Windows PowerShell** (Start menu > type "PowerShell" > open). Copy one block at a time.

---------------------------------------------------------------------------------------------------

## 1. Required software and version checks

```powershell
dotnet --list-sdks        # must include a line starting with 10.   (you had 10.0.301)
dotnet --version          # expected 10.0.x
git --version             # optional
Get-Service postgresql*   # Status should be Running  (you had postgresql-x64-18)
```
If `dotnet --list-sdks` has no 10.x line: install ".NET 10 SDK" from https://dotnet.microsoft.com/download then reopen PowerShell.
`psql` does not need to be on PATH; the script finds it by itself.

## 2. Folder structure (check yours matches)

```powershell
cd C:\Dev\KivaBelion
Get-ChildItem -Recurse -File | Select-Object -ExpandProperty FullName
```
```
KivaBelion\
  README.md  global.json  KivaBelion.slnx  .gitignore  .env.example
  docs\TEST-REPORT.md
  requests\phase-a.http  KivaBelion-PhaseA.postman_collection.json
  scripts\create-dev-database.ps1  set-dev-secrets.ps1
  src\KivaBelion.API\
      KivaBelion.API.csproj  Program.cs  appsettings.json  appsettings.Development.json
      Properties\launchSettings.json
      Common\  (CorrelationIdMiddleware, SecurityHeadersMiddleware, GlobalExceptionHandler, StartupChecks, CorsPolicies)
      Controllers\  (SystemController, DevToolsController)
      HealthChecks\  (DatabaseHealthCheck)
  tests\KivaBelion.API.Tests\  KivaBelion.API.Tests.csproj  ApiFactory.cs  FoundationTests.cs
```
Tip: keep the project outside OneDrive and in a short path such as `C:\Dev\KivaBelion`.

## 3. Safe PostgreSQL setup

Check which port PostgreSQL listens on (expected 5432):
```powershell
Get-NetTCPConnection -State Listen -LocalPort 5432,5433 -ErrorAction SilentlyContinue | Select-Object LocalPort
```
Create a SEPARATE local development database and a limited user (this does NOT touch any existing database):
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\create-dev-database.ps1
```
It asks for: the port, the `postgres` admin password (hidden), and a NEW password you invent for user `kiva_app`
(8+ characters, avoid `;` `=` `'` `"`). It creates role `kiva_app` and database `kivabelion_dev` only if missing,
then tests the login. Safe to re-run (a re-run resets the `kiva_app` password to what you type).
Success looks like: `SUCCESS: connected to kivabelion_dev as kiva_app`.

Optional check in pgAdmin: Servers > PostgreSQL 18 > Databases > `kivabelion_dev` (empty in Phase A).

## 4. .NET user-secrets (keeps the password out of the project and Git)

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\set-dev-secrets.ps1
dotnet user-secrets list --project src\KivaBelion.API
```
Expected: `Saved.` Note the `list` command PRINTS your connection string including the password,
so run it only when nobody can see your screen, and never paste its output to anyone.
Secrets live in `%APPDATA%\Microsoft\UserSecrets\kivabelion-api-dev-secrets\secrets.json`.

## 5. Restore, build, test, run

```powershell
cd C:\Dev\KivaBelion
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet run --project src\KivaBelion.API
```
What to expect IF everything works (not verified by the author): restore/build end with `Build succeeded`,
tests end with 10 passed, and the run prints `Now listening on: http://localhost:5080`.
Leave that window open; stop the API with Ctrl+C. Run tests in a second PowerShell window if the API is running.
The automated tests use a fake unreachable database address on purpose; they do not use your real database.

## 6. URLs and expected results (API running)

Browser or PowerShell (`Invoke-RestMethod http://localhost:5080/...`):

| URL | Expected |
|---|---|
| http://localhost:5080/health/live | `Healthy` (200) |
| http://localhost:5080/health/ready | `Healthy` if the database is reachable; `Unhealthy` (503) if not |
| http://localhost:5080/api/v1/system/ping | JSON with `service: KivaBelion.API`, `testMode: true`, `ordersEnabled: false`, `onlinePaymentsEnabled: false` |
| http://localhost:5080/api/v1/nothing-here | JSON error, status 404 |
| http://localhost:5080/api/v1/dev/throw | JSON error, status 500, "An unexpected error occurred.", a traceId, no stack trace |
| POST /api/v1/dev/validate with `{"name":"A","quantity":99}` | 400 with messages about Name and Quantity |
| POST /api/v1/dev/validate with `{"name":"Test","quantity":2}` | 200 "Valid request." |

PowerShell POST example:
```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/v1/dev/validate -ContentType 'application/json' -Body '{"name":"Test","quantity":2}'
```
(For the 400/500 cases PowerShell shows an error; that is expected. Postman or `requests\phase-a.http` show the body nicely.)
The `/api/v1/dev/*` practice endpoints only work in Development and are removed before launch.

## 7. Troubleshooting

**.NET SDK missing / wrong:** `dotnet : not recognized` or `global.json ... no SDK` -> install the .NET 10 SDK, reopen PowerShell,
run `dotnet --list-sdks`. `global.json` needs SDK 10.0.100 or newer 10.x.

**NuGet / restore errors (NU1301, "unable to load service index", timeouts):** check internet, VPN or proxy; then
```powershell
dotnet nuget list source
dotnet nuget locals all --clear
dotnet restore
```
Error NU1102 / "Unable to find package X with version" means a package version I guessed does not exist.
Copy the full message to me; I will correct the version. Do not edit versions by guessing.

**PostgreSQL port / login problems:** `Get-Service postgresql*` must say Running. If the script says connection refused,
the port differs: find it with `Get-NetTCPConnection -State Listen | Where-Object LocalPort -in 5432,5433,5434`, or in pgAdmin
> server > Properties > Connection. Re-run both scripts and type that port. "password authentication failed" for
`postgres` = wrong admin password. `/health/ready` Unhealthy while the API runs = wrong port/password in the secret;
re-run `set-dev-secrets.ps1`.

**Scripts blocked ("running scripts is disabled"):** always use `powershell -ExecutionPolicy Bypass -File .\scripts\...`.

**Startup error "ConnectionStrings:Default is missing":** run section 4 again.

**Port 5080 in use:** `Get-NetTCPConnection -LocalPort 5080` then close that program, or change the URL in
`src\KivaBelion.API\Properties\launchSettings.json`.

**Build/test failures:** run `dotnet build` and copy the FIRST `error CSxxxx` lines (file name and line number) to me.
For tests, `dotnet test --logger "console;verbosity=normal"` and send the failing test names and messages.
Do not hide failures or delete tests to make them pass.

## 8. Which commands are safe

**Do not change any database (safe):** `dotnet --list-sdks`, `dotnet --version`, `git --version`, `Get-Service`,
`Get-NetTCPConnection`, `Get-ChildItem`, `dotnet restore`, `dotnet build`, `dotnet test` (fake DB address only),
`dotnet run` (only does `SELECT 1`; no tables are created), all URLs in section 6, `dotnet user-secrets list`
(prints a secret, so private use only), `dotnet nuget list source`.

**Change things on your PC (not the website or GitHub):**
- `create-dev-database.ps1` — CREATES role `kiva_app` and database `kivabelion_dev` if missing; resets that role's password. Touches nothing else.
- `set-dev-secrets.ps1` — writes the connection string to your user-secrets folder.
- `dotnet nuget locals all --clear` — deletes the local package download cache (only slows the next restore).

**Never run in Phase A:** `DROP DATABASE`, `DROP ROLE`, anything against your other PostgreSQL databases, `git push`,
anything that deploys. Phase A contains no migrations and no code that creates or deletes tables or data.

## Safety switches (`src\KivaBelion.API\appsettings.json`)
`Features:TestMode=true`, `Features:OrdersEnabled=false`, `Features:OnlinePaymentsEnabled=false`. The app refuses to start if
online payments are enabled, or if live orders are enabled outside Development. No secrets belong in these files.

Optional: `git init` in this folder to keep local history. Do NOT push anywhere yet; `.gitignore` already excludes secrets.
