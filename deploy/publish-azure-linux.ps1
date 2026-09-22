<#
.SYNOPSIS
    Publishes the full VMS app - reception screens and /api - for a LINUX Azure Web App.

.DESCRIPTION
    Same application as deploy\publish-azure.ps1, built portable: -p:VmsAgentOnly=true
    gives plain net8.0 with ICP's toolkit left out, no Windows service support and no
    runtime identifier, so it runs on a Linux App Service plan.

    Nothing is missing from it. Every screen, the visitor report and /api are all there.
    The one thing it cannot do is read a card from a reader plugged into the server, and no
    server does that: in agent mode the desk browser reads the card through ICP's agent and
    the tablet reads it through the Android SDK, and what reaches the server either way is
    signed XML it verifies itself. That path never touches the toolkit.

    So Toolkit:Mode MUST be Agent on this deployment. The script refuses to package a build
    whose own appsettings.json says InProcess without saying so first, because that setting
    on this build is a host configured to use a reader it cannot have.

.EXAMPLE
    .\deploy\publish-azure-linux.ps1 -Output C:\Deploy\vms-linux
#>
[CmdletBinding()]
param(
    [string] $Output = (Join-Path $PSScriptRoot '..\artifacts\azure-linux'),
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

<# No -r. A portable publish runs on whatever the Web App's runtime is, which for a Linux
   plan on "DOTNETCORE|8.0" is the Linux one. Naming win-x64 here is exactly the mistake
   that produces a 503 with nothing in the log to explain it. #>
& dotnet publish $project -c $Configuration -p:VmsAgentOnly=true --self-contained false -o $site
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE." }

# --- verification -----------------------------------------------------------------

foreach ($required in 'DI.Vms.Blazor.dll', 'appsettings.json') {
    if (-not (Test-Path (Join-Path $site $required))) {
        throw "$required is not in the published output."
    }
}

<# The toolkit must NOT be here. Its presence would mean VmsAgentOnly did not take - the
   build would be the Windows one under a Linux script's name, and it would fail on the
   Web App rather than here. #>
foreach ($windowsOnly in 'IDCardToolkit.dll', 'EIDAToolkit.dll', 'PCSCLib.dll') {
    if (Test-Path (Join-Path $site $windowsOnly)) {
        throw @"
$windowsOnly is in the published output, so this is the Windows build.

-p:VmsAgentOnly=true did not take. Check the VmsAgentOnly conditions in
DI.Vms.Blazor.csproj - the toolkit reference and the ToolkitNative ItemGroup are both
supposed to be excluded when it is true.
"@
    }
}

<# Toolkit:Mode. InProcess on this build is a host told to use a reader it does not have.
   NoCardReader says so at the desk rather than failing obscurely, but it is still a
   deployment that will not read cards, so it is worth catching here. #>
$settings = Get-Content (Join-Path $site 'appsettings.json') -Raw | ConvertFrom-Json
if ($settings.Toolkit.Mode -eq 'InProcess') {
    Write-Warning @"
appsettings.json says Toolkit:Mode = InProcess, and this build has no reader in it.

Set Toolkit__Mode=Agent in the Web App's configuration - it overrides the file - or the
desk will be told there is no reader here. docs/azure-deployment.md has the full list.
"@
}

<# appsettings.Production.json must NOT ship. In Azure the settings live in the Web App's
   configuration; a file in the package would silently win over some of them and would put
   a connection string into a zip that gets emailed around. #>
$leaked = Join-Path $site 'appsettings.Production.json'
if (Test-Path $leaked) {
    Remove-Item $leaked -Force
    Write-Warning 'Removed appsettings.Production.json from the package - Azure settings come from the Web App configuration.'
}

<# web.config is IIS's, and there is no IIS on a Linux Web App. Harmless, but leaving it
   invites the belief that it is doing something. #>
Remove-Item (Join-Path $site 'web.config') -Force -ErrorAction SilentlyContinue

# --- package ----------------------------------------------------------------------

$zip = Join-Path $Output 'DI.Vms.zip'
Compress-Archive -Path (Join-Path $site '*') -DestinationPath $zip -Force

$size = [math]::Round((Get-Item $zip).Length / 1MB, 1)
$dlls = (Get-ChildItem $site -Filter *.dll).Count

Write-Host ""
Write-Host "Published to $zip ($dlls DLLs, $size MB) - portable, no ICP toolkit." -ForegroundColor Green
Write-Host ""
Write-Host "Deploy it:" -ForegroundColor Cyan
Write-Host "  az webapp deploy --resource-group DotNetSites --name VMS --src-path `"$zip`" --type zip"
Write-Host ""
Write-Host "The settings that are not defaults, on a Linux Web App:" -ForegroundColor Cyan
Write-Host "  az webapp config set -g DotNetSites -n VMS --linux-fx-version `"DOTNETCORE|8.0`" ``"
Write-Host "    --web-sockets-enabled true --always-on true --min-tls-version 1.2 ``"
Write-Host "    --generic-configurations '{\`"healthCheckPath\`": \`"/health\`"}'"
Write-Host ""
Write-Host "  Toolkit__Mode=Agent is required on this build - it has no reader compiled in."
Write-Host ""
Write-Host "If it answers 503, read the container log:" -ForegroundColor Cyan
Write-Host "  az webapp log tail --resource-group DotNetSites --name VMS"
Write-Host ""
Write-Host "docs/azure-deployment.md has the environment variables and the database order."
