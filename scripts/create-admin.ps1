# Creates the FIRST admin account (role Admin). The password is typed hidden, is never saved in any file,
# and is passed to the app through a temporary environment variable. Run it only after the database migration is applied.
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)

$email = Read-Host "Admin email"
$name  = Read-Host "Display name"

function Read-Plain($prompt) {
    $s = Read-Host $prompt -AsSecureString
    $b = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($s)
    try { [Runtime.InteropServices.Marshal]::PtrToStringBSTR($b) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($b) }
}
$pw1 = Read-Plain "Password (12+ characters, letters and digits; typing is hidden)"
$pw2 = Read-Plain "Repeat password"
if ($pw1 -ne $pw2) { throw "The two passwords do not match." }

$oldEnv = $env:ASPNETCORE_ENVIRONMENT
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'      # so User Secrets are loaded
    $env:KIVA_ADMIN_PASSWORD = $pw1
    dotnet run --project src\KivaBelion.API --no-launch-profile -- create-admin --email $email --name $name
    if ($LASTEXITCODE -ne 0) { throw "Admin was not created (see the message above)." }
}
finally {
    Remove-Item Env:KIVA_ADMIN_PASSWORD -ErrorAction SilentlyContinue
    if ($null -eq $oldEnv) { Remove-Item Env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue } else { $env:ASPNETCORE_ENVIRONMENT = $oldEnv }
}
