<#
.SYNOPSIS
    Creates or updates the VMS site in IIS on UATWEB01.

.DESCRIPTION
    Run elevated, on UATWEB01, after copying the publish output there.

    IIS rather than a Windows Service on this machine: it already serves web
    applications, it owns the certificate store binding, and - the reason that matters -
    it can put Windows Authentication in front of an app that has none of its own.

    The app has NO AUTHENTICATION. On a reception PC bound to loopback that is contained.
    On a server every desk can reach, it is not. -WindowsAuth (the default) makes IIS
    demand a domain identity before a request reaches the app, which is the cheapest real
    control available and needs no application change.

    Re-runnable: an existing site and pool are reconfigured, not duplicated.

.EXAMPLE
    .\install-iis.ps1 -Path 'C:\inetpub\vms' -HostHeader vms.dubaiinvestments.local -CertificateThumbprint A1B2...

.EXAMPLE
    # HTTP, for a first smoke test only. Agent reads will not work over plain HTTP with
    # TlsEnabled true, and visitor data crosses the network in clear.
    .\install-iis.ps1 -Path 'C:\inetpub\vms' -HttpOnly
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $Path,

    [string] $SiteName = 'DI VMS',
    [string] $AppPoolName = 'DIVms',

    # Leave blank to answer on any host name.
    [string] $HostHeader = '',

    [int] $HttpsPort = 443,
    [int] $HttpPort = 80,

    # SHA-1 thumbprint of a certificate already in LocalMachine\My.
    [string] $CertificateThumbprint = '',

    [switch] $HttpOnly,

    <#
        The application signs users in with Entra ID over OpenID Connect, so IIS must let
        the request through: Anonymous ON, Windows Authentication OFF. With Windows
        Authentication on, IIS challenges the browser before the request reaches the
        OpenID Connect handler and sign-in never happens.

        Set this true only if the app has been switched back to Windows Authentication.
    #>
    [bool] $WindowsAuth = $false
)

$ErrorActionPreference = 'Stop'

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this in an elevated PowerShell - configuring IIS needs administrator.'
}

Import-Module WebAdministration -ErrorAction Stop

if (-not (Test-Path (Join-Path $Path 'DI.Vms.Blazor.dll'))) {
    throw "DI.Vms.Blazor.dll not found in $Path. Publish with deploy\publish.ps1 and copy the output here first."
}

if (-not (Test-Path (Join-Path $Path 'web.config'))) {
    throw "web.config not found in $Path. `dotnet publish` writes it for IIS; the folder looks like a plain build output rather than a publish."
}

# The ASP.NET Core Module is what hands a request from IIS to the app. Without the
# hosting bundle the site starts and every request returns 500.19 or 502.5.
if (-not (Get-WebGlobalModule | Where-Object Name -eq 'AspNetCoreModuleV2')) {
    throw 'AspNetCoreModuleV2 is not installed in IIS. Install the ASP.NET Core 8 Hosting Bundle (dotnet-hosting-8.0.x-win.exe) and re-run - an IIS reset is part of that installer.'
}

# --- application pool -------------------------------------------------------------
# No managed runtime: the app is out-of-process from the CLR's point of view, .NET 8
# runs in its own process, and leaving this at v4.0 loads a runtime for nothing.
if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    New-WebAppPool -Name $AppPoolName | Out-Null
    Write-Host "Created application pool $AppPoolName."
}
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ''
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name enable32BitAppOnWin64 -Value $false
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name startMode -Value 'AlwaysRunning'

# Blazor Server holds a circuit per open screen, in memory. A pool that recycles nightly
# drops every screen mid-check-in, and an idle timeout drops the desk over lunch - so
# both are off, and the pool is kept warm instead.
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name processModel.idleTimeout -Value ([TimeSpan]::Zero)
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name recycling.periodicRestart.time -Value ([TimeSpan]::Zero)
Clear-ItemProperty "IIS:\AppPools\$AppPoolName" -Name recycling.periodicRestart.schedule -ErrorAction SilentlyContinue

# --- site -------------------------------------------------------------------------
$site = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
if (-not $site) {
    New-Website -Name $SiteName -PhysicalPath $Path -ApplicationPool $AppPoolName -Port $HttpPort -HostHeader $HostHeader | Out-Null
    Write-Host "Created site $SiteName."
}
else {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $Path
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
}

if (-not $HttpOnly) {
    if (-not $CertificateThumbprint) {
        throw 'Pass -CertificateThumbprint (a certificate in LocalMachine\My), or -HttpOnly for a smoke test. An HTTPS page is required: a browser will not open the wss:// socket to the desk agent from a plain HTTP page.'
    }

    $thumbprint = $CertificateThumbprint -replace '[^0-9A-Fa-f]', ''
    if (-not (Test-Path "Cert:\LocalMachine\My\$thumbprint")) {
        throw "No certificate with thumbprint $thumbprint in LocalMachine\My."
    }

    <#
        SNI whenever there is a host name, which is what lets several HTTPS sites share
        one address. Without it a binding claims the whole IP:port and the single
        certificate that goes with it - so on a server that already serves another site
        on 443, creating the binding fails with "Cannot create a file when that file
        already exists", naming neither the port nor the site that already has it.

        Bound through the binding object rather than through IIS:\SslBindings, because
        that provider path IS the IP:port mapping and cannot express "this certificate,
        for this host name".
    #>
    $sniFlag = if ($HostHeader) { 1 } else { 0 }

    # Dropped and recreated, so the flags and host name are what this run says rather
    # than what an earlier one left behind. Both spellings, since an earlier run may have
    # made the binding without a host header.
    Remove-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -HostHeader $HostHeader -ErrorAction SilentlyContinue
    Remove-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -HostHeader '' -ErrorAction SilentlyContinue

    New-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -HostHeader $HostHeader -SslFlags $sniFlag | Out-Null

    # Re-pointed on every run, so replacing an expiring certificate is just a re-run.
    $binding = Get-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort
    $binding.AddSslCertificate($thumbprint, 'My')

    if ($sniFlag -eq 1) {
        Write-Host "HTTPS bound on port $HttpsPort for $HostHeader (SNI)."
    }
    else {
        Write-Warning "HTTPS bound on port $HttpsPort with no host name, which claims the whole address. Pass -HostHeader on a server that hosts more than this site."
    }
}

# --- authentication ---------------------------------------------------------------
$authPath = "/system.webServer/security/authentication"
if ($WindowsAuth) {
    Set-WebConfigurationProperty -Filter "$authPath/windowsAuthentication" -Name enabled -Value $true -PSPath 'IIS:\' -Location $SiteName
    Set-WebConfigurationProperty -Filter "$authPath/anonymousAuthentication" -Name enabled -Value $false -PSPath 'IIS:\' -Location $SiteName
    Write-Host 'Windows Authentication on, Anonymous off.'
    Write-Warning 'The application signs users in with Entra ID. With Windows Authentication on, IIS challenges first and Entra sign-in never runs.'
}
else {
    Set-WebConfigurationProperty -Filter "$authPath/anonymousAuthentication" -Name enabled -Value $true -PSPath 'IIS:\' -Location $SiteName
    Set-WebConfigurationProperty -Filter "$authPath/windowsAuthentication" -Name enabled -Value $false -PSPath 'IIS:\' -Location $SiteName
    Write-Host 'Anonymous on, Windows Authentication off - the application signs users in with Entra ID.'
    Write-Host '  Anonymous here means IIS lets the request through; the app then requires a signed-in user for every page.'
}

# --- WebSockets -------------------------------------------------------------------
# Blazor Server IS a WebSocket. Without the IIS WebSocket feature the circuit falls back
# to long polling, and the desk gets a screen that is intermittently a second behind.
if (-not (Get-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets -ErrorAction SilentlyContinue |
          Where-Object State -eq 'Enabled')) {
    Write-Warning 'The IIS WebSocket Protocol feature is not enabled. Blazor will fall back to long polling. Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets'
}

Restart-WebAppPool -Name $AppPoolName
Start-Website -Name $SiteName -ErrorAction SilentlyContinue

Write-Host ''
Write-Host "$SiteName is $((Get-Website -Name $SiteName).State), pool $AppPoolName."
Write-Host "App pool identity: IIS AppPool\$AppPoolName"
Write-Host "  -> set that as @Login in db\006_grant_app_login.sql and run it, or the first page will fail on the database."
