# Put the site on the public internet through a Cloudflare Tunnel, with no
# inbound port open on this machine.
#
# cloudflared dials out to Cloudflare and holds the connection open; requests
# for the hostname arrive down that connection and are proxied to the app on
# loopback. Nothing listens on the WAN side, so there is no port to forward and
# nothing for a scanner to find.
#
# Prerequisite: the domain is added to Cloudflare and using Cloudflare's
# nameservers. Registrar does not matter; nameservers do.
#
# Run from an elevated PowerShell:
#
#   .\deploy\install-tunnel.ps1 -Hostname tedko.dev
#
# Safe to re-run: an existing tunnel and its DNS records are reused.

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Hostname,
    [string]$TunnelName = 'profilesite',
    [string]$LocalUrl = 'http://127.0.0.1:8080',
    [switch]$SkipWww
)

$ErrorActionPreference = 'Stop'

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this from an elevated PowerShell - installing the tunnel service requires administrator.'
}

# ---- 0. is the zone actually on Cloudflare -------------------------------
# Checked up front because the failure otherwise surfaces late and unhelpfully:
# the zone simply does not appear in the login picker, or route dns fails with a
# generic error. A nameserver lookup says plainly whether the move has landed.
$zone = ($Hostname -split '\.' | Select-Object -Last 2) -join '.'
try {
    $ns = (Resolve-DnsName -Name $zone -Type NS -Server 8.8.8.8 -ErrorAction Stop |
        Where-Object { $_.QueryType -eq 'NS' }).NameHost
}
catch {
    throw "Cannot resolve nameservers for $zone - is the domain registered and its DNS live?"
}

if (-not ($ns -match 'ns\.cloudflare\.com$')) {
    throw @"
$zone is not on Cloudflare's nameservers yet. Currently: $($ns -join ', ')

Add the site in the Cloudflare dashboard, then set those nameservers at your
registrar. See deploy\README.md. Re-run this once the zone reads Active.
"@
}
Write-Host "zone $zone on Cloudflare ($($ns -join ', '))" -ForegroundColor Green

# ---- 1. cloudflared ------------------------------------------------------
Write-Host "`n[1/6] cloudflared" -ForegroundColor Cyan
if (-not (Get-Command cloudflared -ErrorAction SilentlyContinue)) {
    Write-Host '  installing via winget'
    & winget install --id Cloudflare.cloudflared --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -ne 0) { throw "winget install failed ($LASTEXITCODE)" }

    # winget updates the machine PATH, but not this already-running session.
    $env:PATH = [Environment]::GetEnvironmentVariable('PATH', 'Machine') + ';' +
                [Environment]::GetEnvironmentVariable('PATH', 'User')
    if (-not (Get-Command cloudflared -ErrorAction SilentlyContinue)) {
        throw 'cloudflared installed but not on PATH - open a new PowerShell and re-run.'
    }
}
Write-Host "  $(cloudflared --version)"

$cfDir = Join-Path $env:USERPROFILE '.cloudflared'

# ---- 2. authorise this machine against the zone --------------------------
# Opens a browser to pick the zone, then writes cert.pem - the credential that
# lets this machine create tunnels and DNS records for that zone.
Write-Host "`n[2/6] account authorisation" -ForegroundColor Cyan
$certPath = Join-Path $cfDir 'cert.pem'
if (Test-Path $certPath) {
    Write-Host "  already authorised ($certPath)"
}
else {
    Write-Host '  a browser will open - sign in and pick the zone for this domain'
    & cloudflared tunnel login
    if ($LASTEXITCODE -ne 0) { throw "cloudflared tunnel login failed ($LASTEXITCODE)" }
    if (-not (Test-Path $certPath)) { throw "login reported success but $certPath is missing." }
}

# ---- 3. the tunnel -------------------------------------------------------
Write-Host "`n[3/6] tunnel '$TunnelName'" -ForegroundColor Cyan

# A live tunnel reports deleted_at as Go's zero time, "0001-01-01T00:00:00Z",
# not as null or an empty string. Testing the field for truthiness therefore
# marks every existing tunnel as deleted and finds nothing.
function Get-LiveTunnelId([string]$name) {
    $all = & cloudflared tunnel list --output json | ConvertFrom-Json
    ($all | Where-Object {
        $_.name -eq $name -and $_.deleted_at -match '^0001-01-01'
    } | Select-Object -First 1).id
}

$tunnelId = Get-LiveTunnelId $TunnelName

if ($tunnelId) {
    Write-Host "  reusing $tunnelId"
}
else {
    & cloudflared tunnel create $TunnelName
    if ($LASTEXITCODE -ne 0) { throw "cloudflared tunnel create failed ($LASTEXITCODE)" }

    $tunnelId = Get-LiveTunnelId $TunnelName
    if (-not $tunnelId) { throw "created '$TunnelName' but cannot find its id." }
    Write-Host "  created $tunnelId"
}

$credentials = Join-Path $cfDir "$tunnelId.json"
if (-not (Test-Path $credentials)) { throw "Missing tunnel credentials at $credentials." }

# ---- 4. DNS --------------------------------------------------------------
# Each route is a proxied CNAME to <id>.cfargotunnel.com. That target only
# resolves for Cloudflare's own edge, so the hostname cannot be used to reach
# this machine except through the tunnel.
Write-Host "`n[4/6] DNS routes" -ForegroundColor Cyan
$hostnames = @($Hostname)
if (-not $SkipWww -and $Hostname -notlike 'www.*') { $hostnames += "www.$Hostname" }

# --overwrite-dns because an existing A/CNAME on the hostname must lose. A
# domain moved to Cloudflare arrives with whatever the old registrar had parked
# on it, and without this the command fails on that record and the site silently
# keeps resolving to a parking page. Only the record for this exact hostname is
# replaced; MX, TXT, and every other name in the zone are untouched.
# cloudflared logs its INF lines to stderr even on success. Merging those with
# 2>&1 while ErrorActionPreference is Stop turns each one into a terminating
# error, so a route that worked is reported as a failure. Drop to Continue for
# the call and judge the result by exit code, which is what actually says.
foreach ($name in $hostnames) {
    Write-Host "  $name"

    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & cloudflared tunnel route dns --overwrite-dns $TunnelName $name 2>&1 | Out-String
    }
    finally { $ErrorActionPreference = $previous }

    if ($LASTEXITCODE -ne 0) { throw "route dns $name failed ($LASTEXITCODE): $output" }
    Write-Host "    routed"
}

# ---- 5. config -----------------------------------------------------------
# Written into the LOCAL SYSTEM profile because that is the account the service
# runs as, and where it looks for its config and credentials.
Write-Host "`n[5/6] config" -ForegroundColor Cyan
$systemDir = 'C:\Windows\System32\config\systemprofile\.cloudflared'
New-Item -ItemType Directory -Force -Path $systemDir | Out-Null

$ingress = ($hostnames | ForEach-Object {
    "  - hostname: $_`n    service: $LocalUrl"
}) -join "`n"

# The trailing catch-all is required: cloudflared refuses to start without a
# final rule that has no hostname. 404 rather than the app, so a request that
# arrives under any other name is not served the site.
$config = @"
# Generated by deploy\install-tunnel.ps1. Re-run that script to regenerate.
tunnel: $tunnelId
credentials-file: $systemDir\$tunnelId.json

# Identify the real client to the app. The app trusts these headers only from
# 127.0.0.1, which is where cloudflared connects from.
originRequest:
  connectTimeout: 30s
  noTLSVerify: false

ingress:
$ingress
  - service: http_status:404
"@

Set-Content -LiteralPath (Join-Path $systemDir 'config.yml') -Value $config -Encoding utf8
Copy-Item -LiteralPath $credentials -Destination $systemDir -Force
Write-Host "  wrote $systemDir\config.yml"

# The credentials file is a bearer token for the tunnel. Administrators and
# SYSTEM only.
#
# Root grant plus /reset on children, not an (OI)(CI) grant walked with /T -
# that combination leaves leaf files with an empty ACL, which here would make
# the credentials unreadable by the very service that needs them.
# Reset before granting: /reset /T covers the root too, so the reverse order
# would throw the grant away.
& icacls.exe $systemDir /reset /T /Q | Out-Null
if ($LASTEXITCODE -ne 0) { throw "icacls (reset) failed ($LASTEXITCODE)" }

& icacls.exe $systemDir /inheritance:r `
    /grant 'BUILTIN\Administrators:(OI)(CI)F' 'NT AUTHORITY\SYSTEM:(OI)(CI)F' /Q | Out-Null
if ($LASTEXITCODE -ne 0) { throw "icacls (root) failed ($LASTEXITCODE)" }

# ---- 6. service ----------------------------------------------------------
Write-Host "`n[6/6] service" -ForegroundColor Cyan
if (-not (Get-Service -Name 'Cloudflared' -ErrorAction SilentlyContinue)) {
    & cloudflared service install
    if ($LASTEXITCODE -ne 0) { throw "cloudflared service install failed ($LASTEXITCODE)" }
}

# Set the command line explicitly rather than trusting what `service install`
# recorded. It derives the arguments from a config in the *installing user's*
# .cloudflared directory, and the config this script writes lives in the LOCAL
# SYSTEM profile instead - so it registers a bare `cloudflared.exe`, which
# prints help, exits, and surfaces only as service error 1067.
$cloudflaredExe = (Get-Command cloudflared).Source
$binPath = "`"$cloudflaredExe`" --config `"$systemDir\config.yml`" tunnel run"

# Written straight to the registry rather than through `sc.exe config binPath=`.
# The value has to carry its own quotes, because cloudflared installs under
# "Program Files (x86)", and sc.exe rejects that nested quoting when PowerShell
# passes it through - error 1639, ERROR_INVALID_COMMAND_LINE.
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\Cloudflared' `
    -Name 'ImagePath' -Value $binPath -Type ExpandString

& sc.exe config Cloudflared start= auto | Out-Null
if ($LASTEXITCODE -ne 0) { throw "sc.exe config Cloudflared failed ($LASTEXITCODE)" }

# Same restart-on-crash policy as the app: a dropped tunnel takes the site
# offline just as surely as a dead app.
& sc.exe failure Cloudflared reset= 86400 actions= restart/5000/restart/5000/restart/60000 | Out-Null

Restart-Service -Name 'Cloudflared' -ErrorAction SilentlyContinue
if ((Get-Service -Name 'Cloudflared').Status -ne 'Running') { Start-Service -Name 'Cloudflared' }

Start-Sleep -Seconds 5
$status = (Get-Service -Name 'Cloudflared').Status
Write-Host "  Cloudflared: $status"
if ($status -ne 'Running') {
    throw "Cloudflared did not start. Check: Get-EventLog -LogName Application -Source Cloudflared -Newest 20"
}

Write-Host "`ndone." -ForegroundColor Green
Write-Host "tunnel:  $TunnelName ($tunnelId)"
Write-Host "routes:  $($hostnames -join ', ')"
Write-Host "`nverify:  curl.exe -sI https://$Hostname/api/health"
Write-Host "logs:    Get-EventLog -LogName Application -Source Cloudflared -Newest 20"
