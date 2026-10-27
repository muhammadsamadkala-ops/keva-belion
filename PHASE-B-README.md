# KIVA BELION — Phase B: Database foundation (local development only)

**Status: written, NOT compiled or tested by the author** (no .NET/NuGet in the author's workspace). Phase A was
verified by you; Phase B has not been. Run the steps below and send me any error text.
Built on your verified Phase A source (my v3 ZIP). If you changed any Phase A file locally, tell me or send it.

Phase B adds 7 tables (Categories, Products, ProductInventories, Customers, CustomerAddresses, Orders, OrderItems),
EF Core model code and tests. No endpoints, no checkout, no payments. `OrdersEnabled` stays `false`.
No passwords are in any file; the connection string still comes from your User Secrets.

## 0. Put the files in place
Extract `KivaBelion-PhaseB.zip` to a NEW folder (do not overwrite your verified Phase A):
```powershell
Expand-Archive "$HOME\Downloads\KivaBelion-PhaseB.zip" C:\Dev\KivaBelion-B -Force
cd C:\Dev\KivaBelion-B\KivaBelion
```
(User Secrets are per-user, not per-folder, so your saved connection string is reused.)

## 1. Restore, build, test (changes no database)
```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```
Phase A's 10 tests must still pass, plus the new Phase B tests. Send me the summary line and any `error`/`warning NU` lines.

## 2. Create the migration files (changes NO database) — manual
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\phase-b-1-create-migration.ps1
```
Creates `src\KivaBelion.API\Data\Migrations\*` and `migration-preview.sql`. Open the SQL and read it:
you should see only `CREATE TABLE`, `CREATE INDEX`, and the `__EFMigrationsHistory` table. Keep the Migrations folder (commit it with the project).
Manual equivalent: `dotnet tool restore` then `dotnet ef migrations add InitialCreate --project src\KivaBelion.API --output-dir Data\Migrations`

## 3. Apply it to kivabelion_dev — manual, THIS CHANGES THE DATABASE (creates tables only)
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\phase-b-2-apply-migration.ps1
```
Type `YES` when asked. Manual equivalent: `dotnet ef database update --project src\KivaBelion.API`.
Nothing is dropped or reset. Running it again changes nothing once applied.

## 4. Verify (read-only)
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\phase-b-3-verify-schema.ps1
```
Expect 8 tables (7 + `__EFMigrationsHistory`), migration `...InitialCreate`, every count 0 (no data is added).
Then `dotnet run --project src\KivaBelion.API` and `/health/ready` should still be Healthy.

## Files changed / added
- Changed: `src\KivaBelion.API\KivaBelion.API.csproj` (adds `Microsoft.EntityFrameworkCore.Design` for migrations only; InternalsVisibleTo for tests), `src\KivaBelion.API\Program.cs` (registers the DbContext), `.gitignore`.
- Added: `src\KivaBelion.API\Data\` — `KivaBelionDbContext.cs`, `DesignTimeDbContextFactory.cs`, `Entities\` (9 files), `Configurations\` (7 files).
- Added: `tests\...\DataModelTests.cs`, `ValidationTests.cs`; `.config\dotnet-tools.json` (dotnet-ef 10.0.0); `scripts\phase-b-*.ps1`; this README; `docs\PHASE-B-TEST-REPORT.md`.
- Unchanged: all Phase A code, health endpoints and Phase A tests.

## Schema summary (PostgreSQL, money = numeric(12,2) in PKR, times = timestamptz in UTC)
- **Categories**: Name, Slug (both unique), Description, SortOrder, IsActive.
- **Products**: CategoryId → Categories (Restrict), Sku & Slug unique, SizeMl > 0, Price ≥ 0, IsActive; index (CategoryId, IsActive).
- **ProductInventories**: one row per product (unique ProductId, cascade); QuantityOnHand ≥ 0, 0 ≤ Reserved ≤ OnHand, LowStockThreshold. Quantities are whole bottles, so integers (not decimals).
- **Customers**: Email unique and must be lower case, FullName, Phone.
- **CustomerAddresses**: CustomerId → Customers (cascade); at most one default address per customer.
- **Orders**: PublicId (Guid) and OrderNumber unique, optional unique IdempotencyKey (stops duplicate orders in Phase C), CustomerId (Restrict), Status and PaymentMethod stored as text, IsTestOrder (default true), PKR amounts with check Total = Subtotal + Shipping − Discount, copied shipping address.
- **OrderItems**: OrderId (cascade), ProductId (Restrict), copied SKU/name/price, Quantity > 0, LineTotal = UnitPrice × Quantity, one line per product per order.
- All tables except OrderItems have CreatedAtUtc/UpdatedAtUtc set automatically on save. No card, token or credential columns exist (a test guards this).

## Not included on purpose
Seed data (no sample products — tell me if you want dev-only seed data next), admin users, payments, stock movements, audit log, any API endpoint.

## Troubleshooting
- `dotnet tool restore` fails: NuGet/internet issue; or dotnet-ef 10.0.0 is not found → tell me.
- Build error: copy the first `error CSxxxx` lines. Package version problem: copy the NU error.
- `dotnet ef` says connection string missing: run `scripts\set-dev-secrets.ps1`.
- `password authentication failed`: wrong password in the secret; re-run `set-dev-secrets.ps1`.
- Applied the wrong thing / want to start over: do NOT drop anything yourself; ask me first.
