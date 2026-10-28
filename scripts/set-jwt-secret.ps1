# Creates a random signing key for admin login tokens and stores it in .NET User Secrets (never in a file in the project).
# The key is never printed. Running it again REPLACES the key and signs everyone out (harmless in development).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$proj = Join-Path $root 'src\KivaBelion.API'
$csproj = Join-Path $proj 'KivaBelion.API.csproj'
$id = ([xml](Get-Content $csproj -Raw)).Project.PropertyGroup.UserSecretsId | Where-Object { $_ } | Select-Object -First 1
$file = Join-Path $env:APPDATA "Microsoft\UserSecrets\$id\secrets.json"

if (Test-Path $file) {
    $existing = Get-Content $file -Raw | ConvertFrom-Json
    if ($existing.PSObject.Properties.Name -contains 'Jwt:SigningKey') {
        $a = Read-Host "A signing key already exists. Replace it? (type YES to replace)"
        if ($a -ne 'YES') { Write-Host "Kept the existing key. Nothing changed."; return }
    }
}

$bytes = New-Object byte[] 64
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes); $rng.Dispose()
$key = [Convert]::ToBase64String($bytes)
dotnet user-secrets set "Jwt:SigningKey" $key --project $proj | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet user-secrets failed." }
Write-Host "Saved a new random signing key (not shown)." -ForegroundColor Green
