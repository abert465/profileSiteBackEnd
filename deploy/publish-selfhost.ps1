# Build the SPA, publish the API, and stage both into the self-hosted app
# directory. Mirrors what Dockerfile does in three stages, for a Windows host
# with no container runtime.
#
# Safe to re-run: the app directory is replaced, the data directory never is.
#
#   .\deploy\publish-selfhost.ps1
#   .\deploy\publish-selfhost.ps1 -Root D:\ProfileSite

[CmdletBinding()]
param(
    [string]$Root = 'C:\ProfileSite',
    [string]$ServiceName = 'ProfileSite'
)

$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$appDir = Join-Path $Root 'app'
$dataDir = Join-Path $Root 'data'
$stageDir = Join-Path $env:TEMP "profilesite-publish-$(Get-Date -Format yyyyMMddHHmmss)"

Write-Host "repo:  $repo"
Write-Host "app:   $appDir"
Write-Host "data:  $dataDir"

# ---- 1. build the React SPA ----------------------------------------------
Write-Host "`n[1/4] building SPA" -ForegroundColor Cyan
Push-Location (Join-Path $repo 'web')
try {
    if (-not (Test-Path 'node_modules')) {
        npm ci
        if ($LASTEXITCODE -ne 0) { throw "npm ci failed ($LASTEXITCODE)" }
    }
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build failed ($LASTEXITCODE)" }
}
finally { Pop-Location }

# ---- 2. publish the API ---------------------------------------------------
# UseAppHost stays on here, unlike the container build: the service needs a real
# .exe for the Service Control Manager to start.
Write-Host "`n[2/4] publishing API" -ForegroundColor Cyan
dotnet publish (Join-Path $repo 'profileSiteBackEnd\profileSiteBackEnd.csproj') `
    -c Release -o $stageDir --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }

# ---- 3. fold the SPA into wwwroot ----------------------------------------
Write-Host "`n[3/4] copying SPA into wwwroot" -ForegroundColor Cyan
$wwwroot = Join-Path $stageDir 'wwwroot'
New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
Copy-Item -Path (Join-Path $repo 'web\dist\*') -Destination $wwwroot -Recurse -Force

# A stray development database inside the image would shadow the real one only
# if the connection string were ever left at its relative default. Drop it.
Remove-Item (Join-Path $stageDir 'app.db') -Force -ErrorAction SilentlyContinue

# ---- 4. swap into place ---------------------------------------------------
Write-Host "`n[4/4] installing to $appDir" -ForegroundColor Cyan
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
$wasRunning = $svc -and $svc.Status -eq 'Running'
if ($wasRunning) {
    Write-Host "  stopping $ServiceName"
    Stop-Service -Name $ServiceName -Force
    # Release the file locks before overwriting the binaries.
    (Get-Service -Name $ServiceName).WaitForStatus('Stopped', '00:00:30')
}

New-Item -ItemType Directory -Force -Path $dataDir | Out-Null
if (Test-Path $appDir) { Remove-Item $appDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $appDir | Out-Null
Copy-Item -Path (Join-Path $stageDir '*') -Destination $appDir -Recurse -Force
Remove-Item $stageDir -Recurse -Force

if ($wasRunning) {
    Write-Host "  starting $ServiceName"
    Start-Service -Name $ServiceName
}

Write-Host "`ndone." -ForegroundColor Green
Write-Host "exe: $(Join-Path $appDir 'profileSiteBackEnd.exe')"
if (-not $svc) {
    Write-Host "service not installed yet - run deploy\install-service.ps1 next" -ForegroundColor Yellow
}
