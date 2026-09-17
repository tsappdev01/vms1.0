<#
.SYNOPSIS
    Publishes the standalone visitor API and packages it for an Azure Web App.

.DESCRIPTION
    src\DI.Vms.Api is the tablet API on its own - the same endpoints the Blazor app
    serves, compiled from the same files, without the UI or the Windows card reader.
    docs\azure-deployment.md is the runbook; read the section on the database before
    deploying anything, because an Azure Web App cannot reach SQL Server on UATWEB01
    and that has to be solved first.

    Portable output, no runtime identifier: the Web App is Linux, and this project
    targets plain net8.0 so that it can be. The verification below is what catches the
    day someone adds a Windows-only dependency and only finds out when the site returns
    503 in Azure with nothing useful in the log.

.EXAMPLE
    .\deploy\publish-api.ps1 -Output C:\Deploy\vms-api
#>
[CmdletBinding()]
param(
    [string] $Output = (Join-Path $PSScriptRoot '..\artifacts\api'),
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$root    = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $root 'src\DI.Vms.Api\DI.Vms.Api.csproj'

if (-not (Test-Path $project)) {
    throw "Project not found at $project. Run this from a checkout of the repository."
}

if (Test-Path $Output) { Remove-Item $Output -Recurse -Force }
New-Item -ItemType Directory -Force -Path $Output | Out-Null

$site = Join-Path $Output 'site'

& dotnet publish $project -c $Configuration -o $site
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE." }

# --- verification -----------------------------------------------------------------

# The entry point, by the name Azure will look for.
$entry = Join-Path $site 'DI.Vms.Api.dll'
if (-not (Test-Path $entry)) { throw "DI.Vms.Api.dll is not in the output. The publish did not produce a runnable app." }

<# Nothing Windows-only may be in here. The API path uses only System.Security.
   Cryptography.Xml and EF Core; ICP's toolkit belongs to CardReaderService.cs, which
   this project deliberately does not compile. If one of these appears, something has
   started depending on the desk reader and the Web App will fail at runtime on Linux
   rather than here. #>
$forbidden = @('IDCardToolkit.dll', 'EIDAToolkit.dll', 'PCSCLib.dll')
$found = $forbidden | Where-Object { Test-Path (Join-Path $site $_) }

if ($found) {
    throw @"
Windows-only toolkit assemblies are in the published output: $($found -join ', ')

This project must stay free of them - it is built for a Linux Web App. Check whether a
new Compile Include in DI.Vms.Api.csproj has pulled in CardReaderService.cs, or whether
a linked file has grown a dependency on it.
"@
}

# appsettings.json must ship; without it Toolkit:Mode is unset and the app assumes the
# in-process reader, which does not exist here.
if (-not (Test-Path (Join-Path $site 'appsettings.json'))) {
    throw "appsettings.json is not in the output."
}

# --- package ----------------------------------------------------------------------

$zip = Join-Path $Output 'DI.Vms.Api.zip'
Compress-Archive -Path (Join-Path $site '*') -DestinationPath $zip -Force

$size = [math]::Round((Get-Item $zip).Length / 1MB, 1)
$dlls = (Get-ChildItem $site -Filter *.dll).Count

Write-Host ""
Write-Host "Published to $zip ($dlls DLLs, $size MB, no Windows-only assemblies)." -ForegroundColor Green
Write-Host ""
Write-Host "Next:" -ForegroundColor Cyan
Write-Host "  az webapp deploy --resource-group <rg> --name <app> --src-path `"$zip`" --type zip"
Write-Host ""
Write-Host "Then check it before pointing a tablet at it:" -ForegroundColor Cyan
Write-Host "  Invoke-RestMethod https://<app>.azurewebsites.net/health"
Write-Host ""
Write-Host "docs/azure-deployment.md has the configuration keys and the database decision."
