<#
.SYNOPSIS
    Shows the most recent ID Card Toolkit log, newest entries last.

.DESCRIPTION
    The sample's UI reports a short message such as "Failed to get response from
    server", which names a symptom rather than a cause. The log records the
    endpoint that was actually contacted and the status behind it.
#>

[CmdletBinding()]
param(
    [string]$LogDirectory = 'C:\ProgramData\EIDAToolkit\logs',
    [int]$Tail = 80
)

if (-not (Test-Path $LogDirectory)) {
    Write-Host "No log directory at $LogDirectory" -ForegroundColor Yellow
    Write-Host 'Run Set-ToolkitConfig.ps1 first, then reproduce the failure.' -ForegroundColor Yellow
    exit 1
}

$log = Get-ChildItem -Path $LogDirectory -File -ErrorAction SilentlyContinue |
       Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $log) {
    Write-Host "No log files in $LogDirectory yet." -ForegroundColor Yellow
    Write-Host 'Reproduce the failure in the sample app, then run this again.' -ForegroundColor Yellow
    exit 1
}

Write-Host ''
Write-Host "$($log.FullName)  ($([math]::Round($log.Length/1KB,1)) KB, $($log.LastWriteTime))" -ForegroundColor Cyan
Write-Host ('-' * 78)
Get-Content $log.FullName -Tail $Tail
Write-Host ('-' * 78)
Write-Host ''
Write-Host 'Lines mentioning a URL, host or failure:' -ForegroundColor Cyan
Get-Content $log.FullName |
    Select-String -Pattern 'http|https|url|host|connect|fail|error|timeout|refused|certificate|offline' |
    Select-Object -Last 25 |
    ForEach-Object { Write-Host "  $_" -ForegroundColor DarkGray }
Write-Host ''
