<#
.SYNOPSIS
    Publishes the full VMS app - screens and API - and zips it for an Azure Web App.

.DESCRIPTION
    This is the single-Web-App deployment: DI.Vms.Blazor serves the reception screens and
    /api together, so one App Service carries both. It requires a WINDOWS App Service
    plan, because the project is net8.0-windows and references ICP's toolkit.

    deploy\publish.ps1 produces the same output for UATWEB01; this wraps it and adds the
    zip and the checks that matter in Azure. docs\azure-deployment.md is the runbook -
    read the App Service settings section, because Blazor Server does not work properly
    without WebSockets turned on and that is not the default.

.EXAMPLE
    .\deploy\publish-azure.ps1 -Output C:\Deploy\vms-azure
#>
[CmdletBinding()]
param(
    [string] $Output = (Join-Path $PSScriptRoot '..\artifacts\azure'),
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$root    = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $root 'src\DI.Vms.Blazor\DI.Vms.Blazor.csproj'

if (-not (Test-Path $project)) {
    throw "Project not found at $project. Run this from a checkout of the repository."
}

if (Test-Path $Output) { Remove-Item $Output -Recurse -Force }
New-Item -ItemType Directory -Force -Path $Output | Out-Null

$site = Join-Path $Output 'site'

& dotnet publish $project -c $Configuration -r win-x64 --self-contained false -o $site
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE." }

# --- verification -----------------------------------------------------------------

foreach ($required in 'DI.Vms.Blazor.dll', 'web.config', 'appsettings.json') {
    if (-not (Test-Path (Join-Path $site $required))) {
        throw "$required is not in the published output."
    }
}

<# The native toolkit DLLs. Agent mode never loads them, so Azure does not strictly need
   them - but the same output is what goes to UATWEB01 and to a reception PC, where their
   absence is a runtime failure on the first card read that names IDCardToolkit rather
   than the file that was missing. Verified here for the same reason publish.ps1 does. #>
foreach ($native in 'EIDAToolkit.dll', 'PCSCLib.dll') {
    if (-not (Test-Path (Join-Path $site $native))) {
        throw @"
$native is missing from the published output.

Agent mode does not need it, but this same folder is what gets deployed to UATWEB01 and to
reception PCs, where the app would start and then fail on the first card read. Check the
ToolkitNative ItemGroup in DI.Vms.Blazor.csproj still has CopyToPublishDirectory.
"@
    }
}

<# appsettings.Production.json must NOT ship. In Azure the settings live in the Web App's
   configuration; a file in the package would silently win over some of them and would put
   a connection string into a zip that gets emailed around. #>
$leaked = Join-Path $site 'appsettings.Production.json'
if (Test-Path $leaked) {
    Remove-Item $leaked -Force
    Write-Warning 'Removed appsettings.Production.json from the package - Azure settings come from the Web App configuration.'
}

# --- package ----------------------------------------------------------------------

$zip = Join-Path $Output 'DI.Vms.zip'
Compress-Archive -Path (Join-Path $site '*') -DestinationPath $zip -Force

$size = [math]::Round((Get-Item $zip).Length / 1MB, 1)
$dlls = (Get-ChildItem $site -Filter *.dll).Count

Write-Host ""
Write-Host "Published to $zip ($dlls DLLs, $size MB)." -ForegroundColor Green
Write-Host ""
Write-Host "Deploy it:" -ForegroundColor Cyan
Write-Host "  az webapp deploy --resource-group DotNetSites --name VMS --src-path `"$zip`" --type zip"
Write-Host ""
Write-Host "Before the first run, in the Web App:" -ForegroundColor Cyan
Write-Host "  Configuration > General settings > Web sockets  : ON   (Blazor Server needs it)"
Write-Host "  Configuration > General settings > Always On    : ON"
Write-Host "  Configuration > General settings > HTTPS Only   : ON"
Write-Host "  Monitoring > Health check > Path                : /health"
Write-Host ""
Write-Host "docs/azure-deployment.md has the environment variables and the database order."
