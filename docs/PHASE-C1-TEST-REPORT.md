# Phase C1 Test Report
## Actually run by the author: NOTHING (no .NET SDK / NuGet in the author's workspace). Code was only re-read by eye.
## Written, NOT run
- PasswordPolicyTests (7), AdminAuthServiceTests (9: login, case-insensitive email, lockout after 5, lock expiry, disabled, hash not password, token role/expiry),
  AdminAuthApiTests (13: 200/401/400, same answer for wrong password vs unknown email, 401 no/garbage/forged/expired token, 403 Staff vs 200 Admin, no hash/password in response, lockout via API),
  LoginRateLimitTests (1: 3 allowed then 429), StartupSafetyTests (1: refuses short signing key).
- Phase A/B tests (38) must still pass; DataModelTests was edited only to permit the column name `PasswordHash`.
- None need a database (admin accounts are faked in memory).
## Cannot be tested without your PC
Migration `AddAdminUsers`, table check constraints, `create-admin.ps1` end-to-end, real login against PostgreSQL.
## Known risks
Package version `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0` unverified; PasswordHasher is assumed to be in the ASP.NET Core shared framework (if the build says it is missing, I will add the package).
Security note: not penetration-tested; behind-proxy IP handling and HTTPS are deployment tasks.

## Update 1 (after your first run)
- `DataModelTests.Order_enums_are_stored_as_text` failed (null converter). The configuration was correct (your migration shows `character varying(20)` for Status and PaymentMethod); the test read an annotation the finished model does not keep. Test now reads the converter from the property's type mapping. Test strength is unchanged. NOT compiled/run by the author.
- Added direct pins of System.Security.Cryptography.Pkcs 10.0.13 (CVE-2026-84008 high, CVE-2026-70162 medium; fixed 10.0.13 / 9.0.21 / 8.0.2). Warnings not suppressed. Not restored by the author.
