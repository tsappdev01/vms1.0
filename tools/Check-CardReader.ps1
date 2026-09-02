<#
.SYNOPSIS
    Checks that this machine can read an Emirates ID.

.DESCRIPTION
    Works through the chain the ID Card Toolkit depends on, stopping at the first
    thing that is actually broken rather than reporting everything at once:

        1. Smart Card service (SCardSvr) and the user-mode driver host (WUDFsvc)
        2. A smart card reader present as a PnP device
        3. The reader visible through the PC/SC API - the same call the toolkit
           makes via Toolkit.ListReaders()
        4. A card inserted and answering (ATR)
        5. The ICP toolkit agent, if ICAToolkitService.msi has been installed

    Read-only by default. Pass -Fix to start and enable the two services, which
    needs an elevated session.

.EXAMPLE
    .\Check-CardReader.ps1
    .\Check-CardReader.ps1 -Fix        # run as Administrator
#>

[CmdletBinding()]
param(
    [switch]$Fix
)

$ErrorActionPreference = 'Continue'
$problems = New-Object System.Collections.Generic.List[string]

# The two facts that decide readiness. Everything else is diagnosis for when
# these are false - never a reason to fail a machine whose reader demonstrably works.
$pcscOk = $false
$cardOk = $false

function Write-Result {
    param([string]$Label, [ValidateSet('ok','warn','fail','info')][string]$State, [string]$Detail = '')
    $mark, $colour = switch ($State) {
        'ok'   { '  OK  ', 'Green' }
        'warn' { ' WARN ', 'Yellow' }
        'fail' { ' FAIL ', 'Red' }
        'info' { ' INFO ', 'Gray' }
    }
    Write-Host "[$mark] " -ForegroundColor $colour -NoNewline
    Write-Host $Label -NoNewline
    if ($Detail) { Write-Host "  $Detail" -ForegroundColor DarkGray } else { Write-Host '' }
}

function Test-Elevated {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    (New-Object Security.Principal.WindowsPrincipal $id).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
}

Write-Host ''
Write-Host 'Emirates ID reader check' -ForegroundColor Cyan
Write-Host '========================'
Write-Host ''

# ---------------------------------------------------------------- 1. Services

# SCardSvr is required. WUDFsvc only matters if the reader turns out to be
# unusable - it is absent or renamed on some Windows builds, and a reader that
# PC/SC can already see plainly does not need it chased.
$serviceChecks = @(
    @{ Name = 'SCardSvr'; Required = $true },
    @{ Name = 'WUDFsvc';  Required = $false }
)

foreach ($check in $serviceChecks) {
    $name = $check.Name
    $required = $check.Required
    $svc = Get-Service -Name $name -ErrorAction SilentlyContinue

    if (-not $svc) {
        if ($required) {
            Write-Result "$name service" 'fail' 'not present on this machine'
            $problems.Add("$name is missing.")
        } else {
            Write-Result "$name service" 'info' 'not present - only relevant if the reader fails to appear'
        }
        continue
    }

    if ($svc.Status -eq 'Running') {
        Write-Result "$name service" 'ok' 'Running'
        continue
    }

    if ($Fix) {
        if (-not (Test-Elevated)) {
            Write-Result "$name service" 'fail' "$($svc.Status) - -Fix needs an elevated session"
            $problems.Add("Re-run as Administrator to start $name.")
            continue
        }
        try {
            # SCardSvr ships as trigger-start; Automatic survives a reboot with no reader attached.
            Set-Service -Name $name -StartupType Automatic -ErrorAction Stop
            Start-Service -Name $name -ErrorAction Stop
            Write-Result "$name service" 'ok' 'started and set to Automatic'
        } catch {
            Write-Result "$name service" $(if ($required) { 'fail' } else { 'info' }) $_.Exception.Message
            if ($required) { $problems.Add("Could not start $name.") }
        }
    } else {
        Write-Result "$name service" $(if ($required) { 'fail' } else { 'info' }) "$($svc.Status) - re-run with -Fix as Administrator"
        if ($required) { $problems.Add("$name is not running.") }
    }
}

# ------------------------------------------------------------- 2. PnP devices

$present = @(Get-PnpDevice -Class SmartCardReader -PresentOnly -ErrorAction SilentlyContinue)
$all     = @(Get-PnpDevice -Class SmartCardReader -ErrorAction SilentlyContinue)

if ($present.Count -gt 0) {
    foreach ($d in $present) {
        $state = if ($d.Status -eq 'OK') { 'ok' } else { 'warn' }
        Write-Result "Reader device" $state "$($d.FriendlyName) [$($d.Status)]"
        if ($d.Status -ne 'OK') {
            $problems.Add("$($d.FriendlyName) is present but its status is $($d.Status). Check Device Manager > Properties > Events.")
        }
    }
} elseif ($all.Count -gt 0) {
    # A record with Present=False is a leftover from hardware that is not attached now.
    foreach ($d in $all) {
        Write-Result "Reader device" 'fail' "$($d.FriendlyName) - recorded but NOT currently connected"
    }
    $problems.Add('The reader is not physically connected. Plug it into a direct USB port, not a hub.')
} else {
    Write-Result "Reader device" 'fail' 'no smart card reader found'
    $problems.Add('No reader device at all. Plug it in; Windows should install the USB CCID driver automatically.')
}

# ------------------------------------------------------- 3. PC/SC reader list

# This is the call the toolkit itself makes, so it is the one that actually matters.
$pcscSource = @'
using System;
using System.Runtime.InteropServices;

public static class PcSc
{
    [DllImport("winscard.dll")]
    public static extern int SCardEstablishContext(uint scope, IntPtr r1, IntPtr r2, out IntPtr ctx);

    [DllImport("winscard.dll", CharSet = CharSet.Unicode, EntryPoint = "SCardListReadersW")]
    public static extern int SCardListReaders(IntPtr ctx, string groups, char[] readers, ref uint len);

    [DllImport("winscard.dll")]
    public static extern int SCardReleaseContext(IntPtr ctx);

    public static string[] List(out int code)
    {
        IntPtr ctx;
        code = SCardEstablishContext(0, IntPtr.Zero, IntPtr.Zero, out ctx);
        if (code != 0) { return new string[0]; }

        uint len = 0;
        code = SCardListReaders(ctx, null, null, ref len);
        if (code != 0 || len == 0) { SCardReleaseContext(ctx); return new string[0]; }

        char[] buffer = new char[len];
        code = SCardListReaders(ctx, null, buffer, ref len);
        SCardReleaseContext(ctx);
        if (code != 0) { return new string[0]; }

        return new string(buffer, 0, (int)len).Split(new char[] { '\0' },
            StringSplitOptions.RemoveEmptyEntries);
    }
}
'@

try {
    if (-not ('PcSc' -as [type])) { Add-Type -TypeDefinition $pcscSource -ErrorAction Stop }

    $code = 0
    $readers = [PcSc]::List([ref]$code)

    if ($readers.Count -gt 0) {
        foreach ($r in $readers) { Write-Result 'PC/SC reader' 'ok' $r }
        $pcscOk = $true
    } else {
        $hex = '0x{0:X8}' -f $code
        $meaning = switch ($code) {
            -2146435026 { 'SCARD_E_NO_READERS_AVAILABLE - service running, but no reader attached' }
            -2146435071 { 'SCARD_E_NO_SERVICE - the Smart Card service is not running' }
            default     { 'see the WinSCard return codes' }
        }
        Write-Result 'PC/SC reader' 'fail' "none listed ($hex - $meaning)"
        $problems.Add('The toolkit will not find a reader until PC/SC lists one.')
    }
} catch {
    Write-Result 'PC/SC reader' 'warn' "could not query WinSCard: $($_.Exception.Message)"
}

# ------------------------------------------------------------ 4. Card present

$scinfo = & certutil.exe -scinfo 2>&1 | Out-String

if ($scinfo -match 'ATR:') {
    Write-Result 'Card in reader' 'ok' 'card detected and answering'
    $cardOk = $true
} elseif ($scinfo -match 'SCARD_E_NO_READERS_AVAILABLE') {
    Write-Result 'Card in reader' 'fail' 'no reader available'
} elseif ($scinfo -match 'SCARD_E_NO_SMARTCARD|No smart card') {
    Write-Result 'Card in reader' 'warn' 'reader found, but no card inserted'
    $problems.Add('Insert an Emirates ID and run this again.')
} else {
    Write-Result 'Card in reader' 'warn' 'could not determine card state from certutil'
}

# ----------------------------------------------------------- 5. Toolkit agent

try {
    $health = Invoke-WebRequest -Uri 'http://127.0.0.1:9006/health' -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
    if ($health.StatusCode -eq 200) {
        Write-Result 'ICP toolkit agent' 'ok' 'responding on 127.0.0.1:9006'
    } else {
        Write-Result 'ICP toolkit agent' 'warn' "unexpected status $($health.StatusCode)"
    }
} catch {
    Write-Result 'ICP toolkit agent' 'info' 'not responding - only needed for the browser reception client'
}

# ------------------------------------------------------------------- Verdict

Write-Host ''
if ($pcscOk) {
    if ($cardOk) {
        Write-Host 'Ready. PC/SC lists a reader and a card is answering.' -ForegroundColor Green
    } else {
        Write-Host 'Reader ready. Insert an Emirates ID to complete the check.' -ForegroundColor Green
    }

    if ($problems.Count -gt 0) {
        Write-Host ''
        Write-Host 'Noted, but not blocking:' -ForegroundColor DarkGray
        foreach ($p in $problems) { Write-Host "  - $p" -ForegroundColor DarkGray }
    }
    Write-Host ''
    Write-Host 'Next: prove a real read with the vendor sample, which needs no code:' -ForegroundColor Gray
    Write-Host '  id-card-toolkit-windows-sdk-v3.1.6\quickstart\64\EIDAToolkitApp.exe' -ForegroundColor Gray
    Write-Host '  List Readers -> Connect -> Read Public Data' -ForegroundColor Gray
} else {
    Write-Host "Not ready - $($problems.Count) thing(s) to fix:" -ForegroundColor Yellow
    $i = 1
    foreach ($p in $problems) { Write-Host "  $i. $p"; $i++ }
}
Write-Host ''
