<#
.SYNOPSIS
    Installs the published VMS app as a Windows Service on the reception PC.

.DESCRIPTION
    Run elevated, on the reception PC, after copying the publish output there.

    A service rather than a scheduled task or a shortcut, because the desk must be
    serving before anyone logs in and must come back by itself after a restart. The app
    calls AddWindowsService, so it reports READY to the service control manager instead
    of being killed after 30 seconds for failing to start.

    Re-runnable: an existing service is stopped and reconfigured, not duplicated.

.EXAMPLE
    .\install-service.ps1 -Path 'C:\Program Files\DI VMS'

.EXAMPLE
    # Under a domain account, which is what a SQL login on UATWEB01 is easiest to grant to.
    .\install-service.ps1 -Path 'C:\Program Files\DI VMS' -ServiceAccount 'DI\svc-vms'
#>
[CmdletBinding()]
param(
    # The folder holding DI.Vms.Blazor.exe on this machine.
    [Parameter(Mandatory = $true)][string] $Path,

    [string] $ServiceName = 'DIVms',
    [string] $DisplayName = 'DI Visitor Management',

    # Leave as LocalSystem only if UATWEB01 has a login for this PC's machine account
    # (DOMAIN\PCNAME$). A domain service account is usually the simpler grant.
    [string] $ServiceAccount = 'LocalSystem',

    # Prompted for, never passed on a command line that lands in PSReadLine history.
    [switch] $PromptForPassword
)

$ErrorActionPreference = 'Stop'

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this in an elevated PowerShell - creating a service needs administrator.'
}

$exe = Join-Path $Path 'DI.Vms.Blazor.exe'
if (-not (Test-Path $exe)) { throw "DI.Vms.Blazor.exe not found in $Path. Publish and copy it first." }
if (-not (Test-Path (Join-Path $Path 'EIDAToolkit.dll'))) {
    throw "EIDAToolkit.dll is not beside the exe in $Path. Card reading would fail with 0x8007007E; re-publish with deploy\publish.ps1."
}

$credential = $null
if ($PromptForPassword) {
    $credential = Get-Credential -UserName $ServiceAccount -Message 'Password for the VMS service account'
}

$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Service $ServiceName exists - stopping and reconfiguring."
    if ($existing.Status -ne 'Stopped') { Stop-Service -Name $ServiceName -Force }
}

# Set-Service cannot change a binary path on Windows PowerShell 5.1, which is what a
# reception PC has, so an existing service is reconfigured through sc.exe and only a new
# one goes through New-Service - which is the call that accepts a credential.
$description = 'Serves the DIP reception visitor check-in screens and reads Emirates ID cards from the local reader.'

if ($existing) {
    & sc.exe config $ServiceName binPath= "\"$exe\"" start= auto DisplayName= "$DisplayName" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "sc.exe config failed with exit code $LASTEXITCODE." }
    if ($credential) {
        & sc.exe config $ServiceName obj= $credential.UserName password= $credential.GetNetworkCredential().Password | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "sc.exe config (account) failed with exit code $LASTEXITCODE." }
    }
}
else {
    $arguments = @{
        Name           = $ServiceName
        BinaryPathName = "\"$exe\""
        DisplayName    = $DisplayName
        Description    = $description
        StartupType    = 'Automatic'
    }
    if ($credential) { $arguments.Credential = $credential }
    New-Service @arguments | Out-Null
}

& sc.exe description $ServiceName $description | Out-Null

# LocalSystem/NetworkService are set through sc.exe, which takes no password for them.
if (-not $credential -and $ServiceAccount -in @('LocalSystem', 'NetworkService', 'LocalService')) {
    & sc.exe config $ServiceName obj= $ServiceAccount | Out-Null
}

# The database and the reader are both outside this process. Restart rather than stay
# down when either takes the app with it; reset the count daily so a persistent fault
# still shows up as a stopped service rather than an endless loop.
& sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/15000/restart/60000 | Out-Null

# Not delayed-auto: reception wants the screen up as soon as the PC is.
Start-Service -Name $ServiceName
Start-Sleep -Seconds 3
$service = Get-Service -Name $ServiceName

Write-Host ''
Write-Host "$ServiceName is $($service.Status)."
if ($service.Status -ne 'Running') {
    Write-Warning 'It did not start. The reason is in the Windows Event Log:'
    Write-Warning "  Get-WinEvent -LogName Application -MaxEvents 20 | Where-Object Message -like '*DI*'"
    Write-Warning 'The usual causes are a wrong connection string, a SQL login the service identity does not have, or Toolkit:ConfigDirectory pointing at a folder without config_li.'
}
else {
    Write-Host 'Open the URL from appsettings.Production.json on this PC (default http://127.0.0.1:5100).'
}
