<#
.SYNOPSIS
    Writes a config_ap for the ID Card Toolkit samples, with paths resolved for
    this machine.

.DESCRIPTION
    The .NET sample reads a file named config_ap from its working directory and
    passes the contents to the Toolkit constructor. Absolute paths must be exact,
    and the config folder in this repository is nested one level deeper than its
    name suggests, which is easy to get wrong by hand.

    This finds the folder that actually contains config_li, writes config_ap into
    the quickstart folder, and creates the log directory.

.PARAMETER Format
    json (default) or keyvalue. The quickstart README documents JSON; the Android
    sample builds newline-separated key = value pairs. If one fails, try the other.

.PARAMETER Offline
    Sets read_publicdata_offline. Default true - the supplied bundle is the
    IDCARDOFFLINE set, so a read should not need the Validation Gateway.

.EXAMPLE
    .\tools\eidatoolkit\Set-ToolkitConfig.ps1
    .\tools\eidatoolkit\Set-ToolkitConfig.ps1 -Format keyvalue
#>

[CmdletBinding()]
param(
    [ValidateSet('json', 'keyvalue')]
    [string]$Format = 'json',

    [bool]$Offline = $true,

    [string]$LogDirectory = 'C:\ProgramData\EIDAToolkit\logs'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

# The config folder is nested: IDCARDOFFLINE_config_<date>\IDCARDOFFLINE_config_<date>\
# Locate it by finding config_li rather than assuming the shape.
$candidates = @(Get-ChildItem -Path $repo -Filter 'config_li' -Recurse -File -ErrorAction SilentlyContinue)

if ($candidates.Count -eq 0) {
    Write-Host "Could not find config_li under $repo" -ForegroundColor Red
    Write-Host "Is the IDCARDOFFLINE_config folder present?" -ForegroundColor Red
    exit 1
}

# Prefer the delivered bundle over any copy someone has made into a sample folder,
# so the source of truth stays the ICP-supplied directory.
$preferred = $candidates | Where-Object { $_.FullName -like '*IDCARDOFFLINE*' } | Select-Object -First 1
$configLi  = if ($preferred) { $preferred } else { $candidates[0] }
$configDir = $configLi.Directory.FullName

if ($candidates.Count -gt 1) {
    Write-Host "Found $($candidates.Count) copies of config_li; using:" -ForegroundColor Yellow
    foreach ($c in $candidates) {
        $marker = if ($c.FullName -eq $configLi.FullName) { '->' } else { '  ' }
        Write-Host "  $marker $($c.Directory.FullName)" -ForegroundColor DarkGray
    }
}

Write-Host "Config directory : $configDir" -ForegroundColor Gray

# The quickstart folder holds the runnable samples.
$quickstart = Join-Path $repo 'id-card-toolkit-windows-sdk-v3.1.6\quickstart\64'
if (-not (Test-Path $quickstart)) {
    Write-Host "Could not find $quickstart" -ForegroundColor Red
    exit 1
}

New-Item -ItemType Directory -Path $LogDirectory -Force | Out-Null

$target = Join-Path $quickstart 'config_ap'

if ($Format -eq 'json') {
    $content = @"
{
  "config_directory": "$($configDir -replace '\\', '\\\\')",
  "log_directory": "$($LogDirectory -replace '\\', '\\\\')",
  "application_type": "APP_INPROC",
  "read_publicdata_offline": $($Offline.ToString().ToLower())
}
"@
} else {
    $content = @"
config_directory = $configDir
log_directory = $LogDirectory
application_type = APP_INPROC
read_publicdata_offline = $($Offline.ToString().ToLower())
"@
}

# No BOM: the toolkit reads this as plain bytes and a BOM can break the first key.
[System.IO.File]::WriteAllText($target, $content, (New-Object System.Text.UTF8Encoding($false)))

Write-Host ''
Write-Host "Wrote $target  ($Format)" -ForegroundColor Green
Write-Host ''
Get-Content $target | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkGray }
Write-Host ''
Write-Host 'Next:' -ForegroundColor Cyan
Write-Host "  cd `"$quickstart`""
Write-Host '  .\EIDAToolkitModernApp.exe'
Write-Host ''
Write-Host 'Logs will appear in:' -ForegroundColor Cyan
Write-Host "  $LogDirectory"
Write-Host ''
