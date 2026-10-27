# Creates a LOCAL development database "kivabelion_dev" and a limited user "kiva_app".
# Safe to run again. Nothing here touches the website or the internet.
$ErrorActionPreference = 'Stop'

$psql = Get-ChildItem 'C:\Program Files\PostgreSQL\*\bin\psql.exe' -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending | Select-Object -First 1
if (-not $psql) { throw "psql.exe not found under C:\Program Files\PostgreSQL. Is PostgreSQL installed?" }
Write-Host "Using $($psql.FullName)"

$port = Read-Host "PostgreSQL port (press Enter for 5432)"
if ([string]::IsNullOrWhiteSpace($port)) { $port = '5432' }

function Read-Plain($prompt) {
    $s = Read-Host $prompt -AsSecureString
    $b = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($s)
    try { [Runtime.InteropServices.Marshal]::PtrToStringBSTR($b) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($b) }
}

$adminPw = Read-Plain "Password of the PostgreSQL 'postgres' admin user (typing is hidden)"
$appPw   = Read-Plain "NEW password for the app user 'kiva_app' (invent one; avoid ; = ' `" characters)"
if ($appPw -match "[;=`'`"]" -or $appPw.Length -lt 8) { throw "Password must be 8+ characters and avoid ; = ' and `"" }

$sql = @'
\getenv app_pw KIVA_APP_PW
SELECT format('CREATE ROLE kiva_app LOGIN PASSWORD %L', :'app_pw')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname='kiva_app') \gexec
SELECT format('ALTER ROLE kiva_app PASSWORD %L', :'app_pw') \gexec
SELECT 'CREATE DATABASE kivabelion_dev OWNER kiva_app'
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname='kivabelion_dev') \gexec
'@

try {
    $env:PGPASSWORD = $adminPw
    $env:KIVA_APP_PW = $appPw
    $sql | & $psql.FullName -h localhost -p $port -U postgres -d postgres -v ON_ERROR_STOP=1 -q
    if ($LASTEXITCODE -ne 0) { throw "psql failed (exit $LASTEXITCODE). Check the port and the admin password." }

    $env:PGPASSWORD = $appPw
    $check = & $psql.FullName -h localhost -p $port -U kiva_app -d kivabelion_dev -tAc "SELECT current_database() || ' as ' || current_user"
    if ($LASTEXITCODE -ne 0) { throw "Could not connect as kiva_app." }
    Write-Host "SUCCESS: connected to $check" -ForegroundColor Green
    Write-Host "Next: run scripts\set-dev-secrets.ps1"
}
finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    Remove-Item Env:KIVA_APP_PW -ErrorAction SilentlyContinue
}
