# Register the published app as a Windows service and load its configuration
# from deploy\profilesite.env.
#
# Run from an elevated PowerShell, after publish-selfhost.ps1:
#
#   .\deploy\install-service.ps1
#   .\deploy\install-service.ps1 -Root D:\ProfileSite
#
# Safe to re-run: an existing service is updated in place, not recreated, so
# re-running after editing profilesite.env just reloads the settings.

[CmdletBinding()]
param(
    [string]$Root = 'C:\ProfileSite',
    [string]$ServiceName = 'ProfileSite',
    [string]$EnvFile
)

$ErrorActionPreference = 'Stop'

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this from an elevated PowerShell - creating a service requires administrator.'
}

$repo = Split-Path -Parent $PSScriptRoot
if (-not $EnvFile) { $EnvFile = Join-Path $PSScriptRoot 'profilesite.env' }

$exe = Join-Path $Root 'app\profileSiteBackEnd.exe'
if (-not (Test-Path $exe)) {
    throw "No published app at $exe - run publish-selfhost.ps1 first."
}
if (-not (Test-Path $EnvFile)) {
    throw "No $EnvFile - copy profilesite.env.example to profilesite.env and fill it in."
}

# ---- 1. parse the env file ------------------------------------------------
# Split on the first '=' only: base64 hashes end in '=' padding and would be
# truncated by a greedy split.
$pairs = [ordered]@{}
$lineNo = 0
foreach ($line in Get-Content -LiteralPath $EnvFile) {
    $lineNo++
    $trimmed = $line.Trim()
    if ($trimmed -eq '' -or $trimmed.StartsWith('#')) { continue }

    $split = $trimmed.IndexOf('=')
    if ($split -lt 1) { throw "$EnvFile line ${lineNo}: expected KEY=VALUE, got '$trimmed'" }

    $key = $trimmed.Substring(0, $split).Trim()
    $value = $trimmed.Substring($split + 1)
    $pairs[$key] = $value
}

$blank = $pairs.Keys | Where-Object { $pairs[$_] -eq '' }
if ($blank) {
    throw "$EnvFile has empty values for: $($blank -join ', '). Fill them in first."
}
Write-Host "loaded $($pairs.Count) settings from $EnvFile"

# ---- 2. data directory ----------------------------------------------------
# Created before first start so the service does not have to, and so the ACL
# below applies to the directory the database is actually written into.
$dataDir = Join-Path $Root 'data'
New-Item -ItemType Directory -Force -Path $dataDir, (Join-Path $dataDir 'uploads'),
    (Join-Path $dataDir 'keys') | Out-Null

# ---- 3. create or update the service --------------------------------------
# NETWORK SERVICE rather than LOCAL SYSTEM: the app needs outbound SMTP and its
# own data directory, nothing more, and a web-facing process should not run with
# full machine authority.
$account = 'NT AUTHORITY\NETWORK SERVICE'
$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($existing) {
    Write-Host "updating existing service $ServiceName"
    if ($existing.Status -eq 'Running') {
        Stop-Service -Name $ServiceName -Force
        (Get-Service -Name $ServiceName).WaitForStatus('Stopped', '00:00:30')
    }
    # binPath is reset too, in case -Root changed since the last install.
    & sc.exe config $ServiceName binPath= "`"$exe`"" start= auto obj= "$account" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "sc.exe config failed ($LASTEXITCODE)" }
}
else {
    Write-Host "creating service $ServiceName"
    & sc.exe create $ServiceName binPath= "`"$exe`"" start= auto obj= "$account" `
        DisplayName= 'Profile Site' | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "sc.exe create failed ($LASTEXITCODE)" }
    & sc.exe description $ServiceName 'Portfolio site (ASP.NET Core), fronted by cloudflared.' | Out-Null
}

# Restart on crash rather than leaving the site down: after 5s, then 5s, then
# every 60s, with the failure count reset each day.
& sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/5000/restart/60000 | Out-Null

# ---- 4. lock down the install tree ----------------------------------------
# Administrators and the service account only, so a standard user cannot read
# the SQLite database or the Data Protection key ring off disk.
#
# Set at the root and inherited downward, deliberately NOT with /T. Walking the
# tree applying an (OI)(CI) grant to every item looks equivalent and is not:
# those are inheritance flags, they convey no access on a leaf file, and paired
# with /inheritance:r they replace each file's inherited ACEs with an ACE that
# grants nothing. The result is an empty ACL on every file - including the
# service executable, which then cannot be launched by anyone, SYSTEM included.
# Reset first: /reset /T applies to the root as well as its children, so doing
# it after the grant below would discard that grant and restore inheritance from
# C:\ - handing Authenticated Users modify rights on the database.
& icacls.exe $Root /reset /T /Q | Out-Null
if ($LASTEXITCODE -ne 0) { throw "icacls (reset) failed ($LASTEXITCODE)" }

# Now set the root. Children inherit from it automatically; Windows propagates
# inheritable ACEs down as soon as the parent's are replaced.
& icacls.exe $Root /inheritance:r `
    /grant 'BUILTIN\Administrators:(OI)(CI)F' "${account}:(OI)(CI)M" /Q | Out-Null
if ($LASTEXITCODE -ne 0) { throw "icacls (root) failed ($LASTEXITCODE)" }

# Prove it rather than trust it: this is the exact failure mode described above,
# and it is silent until the service refuses to start with exit code 0.
$exeAcl = (& icacls.exe $exe) -join ' '
if ($exeAcl -notmatch 'NETWORK SERVICE') {
    throw "ACL did not inherit to $exe - it would be unlaunchable. Got: $exeAcl"
}

# ---- 5. write the environment ---------------------------------------------
# Per-service, in the service's own registry key, so these never become
# machine-wide variables readable by every process on the box.
$serviceKey = "HKLM:\SYSTEM\CurrentControlSet\Services\$ServiceName"
$environment = $pairs.Keys | ForEach-Object { "$_=$($pairs[$_])" }
Set-ItemProperty -Path $serviceKey -Name 'Environment' -Value $environment -Type MultiString

Write-Host "`nservice configured." -ForegroundColor Green
Write-Host "start it:  Start-Service $ServiceName"
Write-Host "check it:  curl.exe -s -H 'Host: tedko.dev' -H 'X-Forwarded-Proto: https' http://127.0.0.1:8080/api/health"
Write-Host "logs:      Get-EventLog -LogName Application -Source $ServiceName -Newest 20"
