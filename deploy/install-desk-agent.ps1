<#
.SYNOPSIS
    Installs ICP's ID Card Toolkit agent on a reception PC, so the browser there can read
    the card for a server-hosted VMS.

.DESCRIPTION
    Run elevated, on each reception PC. Nothing about VMS is installed here - the app
    stays on UATWEB01. What goes on the desk is ICP's agent, which listens on loopback
    and drives the card reader on behalf of the page.

    The MSI is ICP's, from the toolkit bundle:
        id-card-toolkit-windows-sdk-v3.1.6\installer\ICAToolkitService\64\ICAToolkitService.msi

    After this, open the VMS site on this PC and check the reader panel. If it still says
    no agent answered, the site's own /agent-required page lists what to look at.

.EXAMPLE
    .\install-desk-agent.ps1 -MsiPath \\uatweb01\deploy\ICAToolkitService.msi

.EXAMPLE
    # Also trust the agent's certificate, when ICP supplies one as a file.
    .\install-desk-agent.ps1 -MsiPath .\ICAToolkitService.msi -AgentCertificatePath .\toolkitagent.cer
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $MsiPath,

    # A certificate to put in LocalMachine\Root so the browser will accept the wss://
    # socket to the agent. Optional: the agent's installer may do this itself.
    [string] $AgentCertificatePath = '',

    # The name the page connects to under TLS. The default is the SDK's, and it must
    # resolve to this machine - hence the hosts check below.
    [string] $AgentHostName = 'toolkitagent.emiratesid.ae',

    [switch] $SkipHostsCheck
)

$ErrorActionPreference = 'Stop'

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this in an elevated PowerShell - installing the agent needs administrator.'
}

if (-not (Test-Path $MsiPath)) { throw "MSI not found at $MsiPath." }

$log = Join-Path $env:TEMP 'ICAToolkitService-install.log'
Write-Host "Installing $(Split-Path $MsiPath -Leaf)..."

$process = Start-Process msiexec.exe -Wait -PassThru -ArgumentList @(
    '/i', "`"$((Resolve-Path $MsiPath).Path)`"", '/quiet', '/norestart', '/l*v', "`"$log`"")

# 3010 is success-but-reboot-required, which is not a failure.
if ($process.ExitCode -notin @(0, 3010)) {
    throw "msiexec returned $($process.ExitCode). The verbose log is at $log."
}
if ($process.ExitCode -eq 3010) { Write-Warning 'The installer wants a reboot before the agent will run.' }

if ($AgentCertificatePath) {
    if (-not (Test-Path $AgentCertificatePath)) { throw "Certificate not found at $AgentCertificatePath." }

    # A browser will not open a wss:// socket to a certificate it does not trust, and it
    # fails silently - which looks exactly like an agent that is not installed.
    Import-Certificate -FilePath $AgentCertificatePath -CertStoreLocation Cert:\LocalMachine\Root | Out-Null
    Write-Host 'Agent certificate added to Trusted Root Certification Authorities.'
}

# The TLS host name has to resolve to this machine: that is how the agent can hold a
# certificate for a name a browser accepts while still being the agent on this desk.
if (-not $SkipHostsCheck) {
    $resolved = $null
    try { $resolved = [System.Net.Dns]::GetHostAddresses($AgentHostName) | ForEach-Object { $_.IPAddressToString } }
    catch { }

    $loopback = $resolved | Where-Object { $_ -in @('127.0.0.1', '::1') }

    if (-not $resolved) {
        Write-Warning "$AgentHostName does not resolve on this PC. Add '127.0.0.1 $AgentHostName' to C:\Windows\System32\drivers\etc\hosts, or set Toolkit:Agent:HostName on the server to something that does."
    }
    elseif (-not $loopback) {
        Write-Warning "$AgentHostName resolves to $($resolved -join ', '), which is not this machine. The page would try to read a card on whatever that is."
    }
    else {
        Write-Host "$AgentHostName resolves to loopback."
    }
}

$service = Get-Service -Name '*EIDA*', '*ICAToolkit*' -ErrorAction SilentlyContinue | Select-Object -First 1
if ($service) {
    if ($service.Status -ne 'Running') { Start-Service -Name $service.Name; Start-Sleep -Seconds 2 }
    Write-Host "Agent service '$($service.Name)' is $((Get-Service -Name $service.Name).Status)."
}
else {
    Write-Warning "No agent service was found by name. Check Services for the ICP toolkit agent, and the install log at $log."
}

# The ports eidatoolkit.js tries, in order. Something listening is the outcome that
# matters; which of the three it is does not. A raw TcpClient rather than a networking
# cmdlet, so this behaves the same on whatever PowerShell the desk happens to have.
$listening = @(9004, 9005, 9020) | Where-Object {
    $client = New-Object System.Net.Sockets.TcpClient
    try { $client.Connect('127.0.0.1', $_); $true }
    catch { $false }
    finally { $client.Dispose() }
}

Write-Host ''
if ($listening) {
    Write-Host "Agent is listening on port(s) $($listening -join ', '). Open the VMS site on this PC and read a card."
}
else {
    Write-Warning 'Nothing is listening on 9004, 9005 or 9020 yet. If the installer asked for a reboot, reboot; otherwise see the site''s /agent-required page.'
}
