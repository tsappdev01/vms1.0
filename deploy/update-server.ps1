<#
.SYNOPSIS
    Updates the deployed VMS files on UATWEB01 from a publish folder.

.DESCRIPTION
    Run elevated, on the server.

    This exists because `Copy-Item -Recurse -Force` into an existing deployment is not a
    reliable update. Where a subdirectory already exists it can nest the source inside it
    (wwwroot\wwwroot\...) or leave nested files untouched, so root-level DLLs land while
    wwwroot\app.css does not. The result is new markup served with the old stylesheet -
    which does not look like a copy problem at all, it looks like the design broke:
    unstyled elements fall back to inline flow, and an <svg> with no width rule renders
    at its default 300x150 and blows a panel apart.

    robocopy /MIR mirrors instead, which also clears any nested folder a previous
    Copy-Item left behind. appsettings.Production.json is excluded from the mirror
    entirely, so it is neither overwritten nor purged - it exists only on the server and
    holds this machine's settings.

    The app pool is stopped first: it holds the DLLs open, and a copy that silently fails
    on those leaves new static files with old server code.

.EXAMPLE
    .\update-server.ps1 -Source C:\Deploy\vms
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Source,
    [string] $Path = 'C:\Websites\vms',
    [string] $AppPoolName = 'DIVms'
)

$ErrorActionPreference = 'Stop'

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this in an elevated PowerShell - stopping the app pool needs administrator.'
}

foreach ($required in @('DI.Vms.Blazor.dll', 'web.config', 'wwwroot\app.css')) {
    if (-not (Test-Path (Join-Path $Source $required))) {
        throw "$required is not in $Source. That is a build output or an incomplete copy, not a publish."
    }
}

Import-Module WebAdministration -ErrorAction Stop

$wasRunning = (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue).Value -eq 'Started'
if ($wasRunning) {
    Write-Host "Stopping $AppPoolName..."
    Stop-WebAppPool -Name $AppPoolName
    # The worker process does not exit the instant the pool is told to stop.
    $deadline = (Get-Date).AddSeconds(30)
    while ((Get-WebAppPoolState -Name $AppPoolName).Value -ne 'Stopped' -and (Get-Date) -lt $deadline) {
        Start-Sleep -Milliseconds 500
    }
}

Write-Host "Mirroring $Source -> $Path"
& robocopy $Source $Path /MIR /XF appsettings.Production.json /R:2 /W:2 /NFL /NDL /NJH /NJS

# robocopy uses 0-7 for success (8 and above is a real failure); 1 means files were copied.
if ($LASTEXITCODE -ge 8) { throw "robocopy failed with exit code $LASTEXITCODE." }

if (Test-Path (Join-Path $Path 'wwwroot\wwwroot')) {
    Write-Warning "A nested wwwroot\wwwroot was found and mirrored away. That is what an earlier Copy-Item left behind."
}

if (-not (Test-Path (Join-Path $Path 'appsettings.Production.json'))) {
    Write-Warning 'There is no appsettings.Production.json in the deployment. The app will fall back to appsettings.json, which says Toolkit:Mode InProcess - and this server has no reader.'
}

if ($wasRunning -or $true) {
    Start-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 2
}

Write-Host ''
Write-Host "$AppPoolName is $((Get-WebAppPoolState -Name $AppPoolName).Value)."

# The two files that have to move together. A DLL newer than the stylesheet is the
# failure this script exists to prevent, so it is checked rather than assumed.
$dll = Get-Item (Join-Path $Path 'DI.Vms.Blazor.dll')
$css = Get-Item (Join-Path $Path 'wwwroot\app.css')
Write-Host ("DI.Vms.Blazor.dll  {0}" -f $dll.LastWriteTime)
Write-Host ("wwwroot\app.css    {0}" -f $css.LastWriteTime)

if ([math]::Abs(($dll.LastWriteTime - $css.LastWriteTime).TotalMinutes) -gt 10) {
    Write-Warning 'Those timestamps are far apart. Check the source folder is a fresh publish rather than a partly updated one.'
}

Write-Host ''
Write-Host 'Now hard-reload the desk browser (Ctrl+Shift+R) and check the build stamp in the sidebar.'
