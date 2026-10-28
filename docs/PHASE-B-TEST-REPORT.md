# Phase B Test Report
## Actually run by the author
Nothing. No .NET SDK or NuGet in the author's workspace. Code was only re-read by eye.
## Written, NOT run
- `DataModelTests` (12 tests): entities exist with keys; money decimal(12,2); unique indexes; foreign keys and delete rules; required/length;
  enums as text; calculated column not mapped; no card/credential columns; order defaults; in-memory relationships; UTC timestamps.
- `ValidationTests` (10 tests): product, inventory, customer, address, order item, order validation rules.
- Phase A's 10 tests (unchanged).
- None of these open a database connection (fake address).
## Cannot be tested by tests alone / not run
- Migration generation and application, the SQL check constraints (`CK_*`), the filtered unique index (one default address), real inserts: need your PostgreSQL (README steps 2-4).
## Known risks
- Package versions are guesses until your restore succeeds: Microsoft.EntityFrameworkCore.Design 10.0.0, dotnet-ef 10.0.0.
- A few EF metadata APIs used in tests (`GetPrecision`, `GetValueConverter`) may need adjusting; send compiler errors.
- The migration is generated on your PC, so it is not in the ZIP.
