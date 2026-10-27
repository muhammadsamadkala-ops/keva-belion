# KIVA BELION — Phase C1: Secure admin login (local development only)

**Status: written, NOT compiled or tested by the author** (no .NET/NuGet in the author's workspace).
Reviewed against YOUR uploaded source: it matched my Phase B exactly except your extra `ProductsController.cs`, which I did not change.
This step adds ONLY admin sign-in and role checks. Categories/Products write APIs come next (C2), protected by this.
Orders and payments stay disabled. No passwords or keys are in any file.

## How it works (plain words)
- Staff sign in with email + password at `POST /api/v1/admin/auth/login` and get a 15-minute access token.
- Passwords are stored only as slow salted hashes (ASP.NET Core's PasswordHasher, PBKDF2, 210,000 rounds).
- Wrong email and wrong password give the SAME answer. 5 wrong tries lock the account for 15 minutes. Login is also limited to 5 tries/minute per IP.
- Roles: `Admin` (can change the catalogue, next step) and `Staff` (read-only). New accounts default to Staff; the first account is created as Admin by script.
- The token signing key is random, stored only in your User Secrets, never printed.
- Only `/api/v1/admin/auth/*` is new. Your existing `/api/v1/products` stays public and unchanged.

## Steps — copy the overlay into your project, then run (PowerShell)
`KivaBelion-PhaseC1-overlay.zip` contains ONLY new/changed files (no migrations, no secrets). Extract it OVER your project:
```powershell
cd C:\Dev\KivaBelion-B\KivaBelion
Expand-Archive "$HOME\Downloads\KivaBelion-PhaseC1-overlay.zip" . -Force
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```
(Tests need no database and no admin account.) Send me the summary line and any `error`/`warning NU` lines.

**Then, manually — these change your LOCAL database / secrets:**
```powershell
# 1) Create the NEW migration (no DB change). Does not touch your applied InitialCreate. Read admin-users-preview.sql afterwards.
powershell -ExecutionPolicy Bypass -File .\scripts\phase-c1-1-create-migration.ps1
# 2) Apply it: CREATES one table, AdminUsers, in kivabelion_dev (asks you to type YES)
powershell -ExecutionPolicy Bypass -File .\scripts\phase-c1-2-apply-migration.ps1
# 3) Make the random token signing key (stored in User Secrets, not shown)
powershell -ExecutionPolicy Bypass -File .\scripts\set-jwt-secret.ps1
# 4) Create the first admin (asks for email, name, hidden password twice)
powershell -ExecutionPolicy Bypass -File .\scripts\create-admin.ps1
```
Without step 3 the API refuses to start (clear message). Without steps 1-2, login can't work (no table).

## Try it
```powershell
dotnet run --project src\KivaBelion.API
```
In a second PowerShell window (replace the email; you'll be asked the password hidden):
```powershell
$pw = Read-Host "Password" -AsSecureString
$plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($pw))
$r = Invoke-RestMethod -Method Post http://localhost:5080/api/v1/admin/auth/login -ContentType 'application/json' -Body (@{email='you@example.com'; password=$plain} | ConvertTo-Json)
$h = @{ Authorization = "Bearer $($r.accessToken)" }
Invoke-RestMethod http://localhost:5080/api/v1/admin/auth/me -Headers $h            # shows your email and role
Invoke-RestMethod http://localhost:5080/api/v1/admin/auth/admin-check -Headers $h   # "You are an Admin."
Invoke-RestMethod http://localhost:5080/api/v1/admin/auth/me                        # no token -> error 401 (expected)
```
Do not paste your password or the token anywhere public.

## Files
- **Changed:** `KivaBelion.API.csproj` (+JwtBearer package), `Program.cs`, `Common\StartupChecks.cs`, `appsettings.json` (Jwt issuer/audience, login limit; NO key), `Data\KivaBelionDbContext.cs` (+AdminUsers), `Data\Entities\Enums.cs` (+AdminRole), `tests\...\ApiFactory.cs`, `tests\...\DataModelTests.cs` (allows the `PasswordHash` column name in the "no credentials" check), `.gitignore`.
- **New:** `Data\Entities\AdminUser.cs`, `Data\Configurations\AdminUserConfiguration.cs`, `Security\*` (8 files), `Controllers\AdminAuthController.cs`, tests (`AdminAuthServiceTests`, `AdminAuthApiTests`, `PasswordPolicyTests`, `TestHelpers`), 5 scripts, this README, `docs\PHASE-C1-TEST-REPORT.md`.
- **Database change (after you run steps 1-2):** new table `AdminUsers` (Id, Email unique+lowercase, DisplayName, PasswordHash, Role text, IsActive, FailedLoginCount, LockoutEndUtc, LastLoginAtUtc, timestamps). Nothing else changes.

## Not done yet (on purpose)
Refresh tokens/logout (re-login every 15 min for now), password change, 2FA, audit-log table (events go to the log without secrets), Categories/Products write APIs (C2), Angular dashboard.
Note: the login limit counts by IP; behind a hosting proxy this must be configured at deployment (later phase).

## Troubleshooting
- Startup error about `Jwt:SigningKey`: run step 3. About `ConnectionStrings:Default`: run `scripts\set-dev-secrets.ps1`.
- `create-admin` says table does not exist: do steps 1-2 first.
- Login always 401: check email, password, and that the account was created (it prints "Created admin account for ...").
- NU/CS errors: copy the first lines to me; package versions (JwtBearer 10.0.0) are unverified guesses.
