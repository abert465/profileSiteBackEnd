# Generate a BCrypt hash for Admin__PasswordHash or Private__PasscodeHash.
#
# The app verifies both with BCrypt.Net (AdminAuthController.cs, PasscodeController.cs),
# so the value in profilesite.env has to be a BCrypt hash, not the password itself
# and not a SHA of it.
#
#   .\deploy\new-hash.ps1
#
# The password is read without echo and never appears in the console, in
# PSReadLine history, or in a script argument. Only the hash is printed - that
# is the part that goes in profilesite.env, and it is safe to paste there.
#
# Re-runnable: BCrypt salts each call, so the same password yields a different
# hash every time. Both verify. That is expected, not a bug.

[CmdletBinding()]
param(
    # Cost 12 is roughly 0.3s per verification on current hardware: slow enough
    # to make offline cracking expensive, fast enough for a login form.
    [ValidateRange(10, 16)]
    [int]$WorkFactor = 12
)

$ErrorActionPreference = 'Stop'

# Deliberately the net35 asset out of the NuGet cache, not the net10.0 copy in
# bin\. Windows PowerShell 5.1 runs on .NET Framework and cannot load a .NET 10
# assembly at all ("Unable to load one or more of the requested types"), and the
# net48 asset fails differently, wanting a System.Memory that 5.1 does not ship.
# net35 has no dependencies and loads clean. BCrypt hashes are portable across
# targets - a hash produced here verifies against the app's .NET 10 BCrypt,
# which was checked rather than assumed.
$version = '4.0.3'   # keep in step with the PackageReference in the csproj
$dll = Join-Path $env:USERPROFILE ".nuget\packages\bcrypt.net-next\$version\lib\net35\BCrypt.Net-Next.dll"

if (-not (Test-Path $dll)) {
    throw @"
BCrypt.Net-Next $version not in the NuGet cache at:
  $dll

Restore it first:
  dotnet restore profileSiteBackEnd\profileSiteBackEnd.csproj

If the csproj now references a different version, update `$version in this script.
"@
}

Add-Type -Path $dll

$secure = Read-Host -Prompt 'Password (not echoed)' -AsSecureString
$confirm = Read-Host -Prompt 'Confirm' -AsSecureString

# SecureString is not a security boundary in .NET Core - it is used here only to
# keep the password off the screen and out of shell history. The plaintext is
# unavoidably in memory for the moment BCrypt needs it; it is zeroed straight
# after rather than left for a crash dump to pick up.
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
$ptr2 = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($confirm)
try {
    $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    $plain2 = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr2)

    if ($plain -cne $plain2) { throw 'Passwords do not match.' }
    if ($plain.Length -lt 12) {
        throw "Password is $($plain.Length) characters. Use at least 12 - this one guards the admin panel and is reachable from the public internet."
    }

    $hash = [BCrypt.Net.BCrypt]::HashPassword($plain, $WorkFactor)
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr2)
}

# Verify rather than assume: a hash that does not validate its own input would
# lock you out of the admin panel with no obvious cause.
if (-not [BCrypt.Net.BCrypt]::Verify($plain, $hash)) {
    throw 'Generated hash failed its own verification - do not use it.'
}

Write-Host "`nhash (work factor $WorkFactor):" -ForegroundColor Green
Write-Host $hash
Write-Host "`nPaste into deploy\profilesite.env after Admin__PasswordHash= or Private__PasscodeHash=,"
Write-Host "then re-run install-service.ps1 to load it."
