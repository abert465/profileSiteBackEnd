# Push seed content (Models.cs SampleData) into the live database.
#
# publish-selfhost.ps1 replaces code, not data. Content edited in SampleData -
# the profile summary, project copy, experience bullets - only reaches the site
# when the seeder runs, and Seed:RunOnStartup is false everywhere by design so
# an ordinary service start never rewrites the database.
#
# Run from an elevated PowerShell, after publishing:
#
#   .\deploy\publish-selfhost.ps1
#   .\deploy\reseed.ps1
#
# What it is safe about:
#   - The service is stopped first, so nothing else holds the SQLite file.
#   - The seeder upserts. It does not drop the database, and it leaves
#     ImageUrl alone on any project that already has one, because the admin
#     panel owns that field.
#   - The service is restarted afterwards even if the seed run fails.

[CmdletBinding()]
param(
    [string]$Root = 'C:\ProfileSite',
    [string]$ServiceName = 'ProfileSite',
    [string]$EnvFile,
    # A port of our own, so this never collides with the service's 8080 if the
    # stop did not take effect for some reason.
    [int]$Port = 8099
)

$ErrorActionPreference = 'Stop'

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this from an elevated PowerShell - the data directory is ACLed to the service account.'
}

if (-not $EnvFile) { $EnvFile = Join-Path $PSScriptRoot 'profilesite.env' }
if (-not (Test-Path $EnvFile)) {
    throw "No $EnvFile - this script needs the same configuration the service runs with."
}

$exe = Join-Path $Root 'app\profileSiteBackEnd.exe'
if (-not (Test-Path $exe)) {
    throw "No published app at $exe - run publish-selfhost.ps1 first."
}

# ---- 1. parse the env file ------------------------------------------------
# Same rules as install-service.ps1: split on the first '=' only, because
# BCrypt and base64 values carry '=' padding.
$pairs = [ordered]@{}
$lineNo = 0
foreach ($line in Get-Content -LiteralPath $EnvFile) {
    $lineNo++
    $trimmed = $line.Trim()
    if ($trimmed -eq '' -or $trimmed.StartsWith('#')) { continue }

    $split = $trimmed.IndexOf('=')
    if ($split -lt 1) { throw "$EnvFile line ${lineNo}: expected KEY=VALUE, got '$trimmed'" }

    $pairs[$trimmed.Substring(0, $split).Trim()] = $trimmed.Substring($split + 1)
}
Write-Host "loaded $($pairs.Count) settings from $EnvFile"

# ---- 2. stop the service --------------------------------------------------
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
$wasRunning = $svc -and $svc.Status -eq 'Running'
if ($wasRunning) {
    Write-Host "stopping $ServiceName"
    Stop-Service -Name $ServiceName -Force
    (Get-Service -Name $ServiceName).WaitForStatus('Stopped', '00:00:30')
}

$log = Join-Path $env:TEMP 'profilesite-reseed.log'
$errLog = Join-Path $env:TEMP 'profilesite-reseed.err.log'
$base = "http://127.0.0.1:$Port"
$proc = $null

try {
    # ---- 3. run the published app once, with seeding on -------------------
    # The host has no seed-and-exit mode, so this boots the real web host and
    # stops it once the seeded data answers.
    $vars = [ordered]@{}
    foreach ($k in $pairs.Keys) { $vars[$k] = $pairs[$k] }
    $vars['Seed__RunOnStartup'] = 'true'
    $vars['ASPNETCORE_URLS'] = $base
    # The service config pins AllowedHosts to tedko.dev; this run is reached as
    # 127.0.0.1 and would get 400 on every request under that rule.
    $vars['AllowedHosts'] = '*'
    # Nothing here serves traffic, and the redirect would only fight the probe.
    $vars.Remove('ASPNETCORE_HTTPS_PORT')

    $saved = @{}
    foreach ($k in $vars.Keys) {
        $saved[$k] = [Environment]::GetEnvironmentVariable($k)
        [Environment]::SetEnvironmentVariable($k, $vars[$k])
    }

    try {
        Write-Host "seeding via $base"
        $proc = Start-Process -FilePath $exe `
            -WorkingDirectory (Split-Path -Parent $exe) `
            -RedirectStandardOutput $log -RedirectStandardError $errLog `
            -NoNewWindow -PassThru

        $profile = $null
        foreach ($i in 1..40) {
            Start-Sleep -Milliseconds 750
            if ($proc.HasExited) { break }
            try { $profile = Invoke-RestMethod "$base/api/profile" -TimeoutSec 3; break } catch { }
        }

        if (-not $profile) {
            Write-Host '=== the app never answered ===' -ForegroundColor Red
            Get-Content $log -Tail 40 -ErrorAction SilentlyContinue
            Get-Content $errLog -Tail 40 -ErrorAction SilentlyContinue
            throw 'seed run failed'
        }

        Write-Host "`nseeded:" -ForegroundColor Green
        Select-String -Path $log -Pattern '\[SEED\]' | ForEach-Object { "  $($_.Line)" }
        Write-Host "`nsummary now reads:`n$($profile.summary)`n"
    }
    finally {
        foreach ($k in $saved.Keys) { [Environment]::SetEnvironmentVariable($k, $saved[$k]) }
    }
}
finally {
    if ($proc -and -not $proc.HasExited) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
    Get-Process -Name 'profileSiteBackEnd' -ErrorAction SilentlyContinue |
        Where-Object { $_.Path -eq $exe } | Stop-Process -Force -ErrorAction SilentlyContinue

    if ($wasRunning) {
        Write-Host "starting $ServiceName"
        Start-Service -Name $ServiceName
    }
}

Write-Host "`ndone - verify with: node deploy\smoke-test.mjs" -ForegroundColor Green
