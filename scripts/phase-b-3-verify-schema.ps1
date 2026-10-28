# Read-only check: lists the tables, the applied migrations and row counts. Changes nothing.
$ErrorActionPreference = 'Stop'
$psql = Get-ChildItem 'C:\Program Files\PostgreSQL\*\bin\psql.exe' -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Select-Object -First 1
if (-not $psql) { throw "psql.exe not found under C:\Program Files\PostgreSQL." }
$port = Read-Host "PostgreSQL port (Enter for 5432)"; if ([string]::IsNullOrWhiteSpace($port)) { $port = '5432' }
$s = Read-Host "Password of database user 'kiva_app' (hidden)" -AsSecureString
$b = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($s)
try {
    $env:PGPASSWORD = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($b)
    $sql = @'
SELECT table_name FROM information_schema.tables WHERE table_schema='public' ORDER BY table_name;
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
SELECT 'Categories' t, count(*) FROM "Categories" UNION ALL SELECT 'Products', count(*) FROM "Products"
UNION ALL SELECT 'ProductInventories', count(*) FROM "ProductInventories" UNION ALL SELECT 'Customers', count(*) FROM "Customers"
UNION ALL SELECT 'CustomerAddresses', count(*) FROM "CustomerAddresses" UNION ALL SELECT 'Orders', count(*) FROM "Orders"
UNION ALL SELECT 'OrderItems', count(*) FROM "OrderItems";
'@
    $sql | & $psql.FullName -h localhost -p $port -U kiva_app -d kivabelion_dev -v ON_ERROR_STOP=1
    if ($LASTEXITCODE -ne 0) { throw "Verification query failed." }
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($b)
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
