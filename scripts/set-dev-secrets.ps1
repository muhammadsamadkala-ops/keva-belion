# Stores the database connection string in .NET "user-secrets" (outside the project folder, never in Git).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$proj = Join-Path $root 'src\KivaBelion.API'

$port = Read-Host "PostgreSQL port (press Enter for 5432)"
if ([string]::IsNullOrWhiteSpace($port)) { $port = '5432' }

$s = Read-Host "Password of the database user 'kiva_app' (typing is hidden)" -AsSecureString
$b = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($s)
try { $pw = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($b) }
finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($b) }
if ($pw -match "[;=`'`"]") { throw "Password must not contain ; = ' or `"" }

$cs = "Host=localhost;Port=$port;Database=kivabelion_dev;Username=kiva_app;Password=$pw"
dotnet user-secrets set "ConnectionStrings:Default" $cs --project $proj | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet user-secrets failed." }
Write-Host "Saved. (The password is stored in %APPDATA%\Microsoft\UserSecrets, not in the project.)" -ForegroundColor Green
