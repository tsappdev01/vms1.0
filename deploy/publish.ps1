<#
.SYNOPSIS
    Publishes the VMS reception app and verifies the native toolkit DLLs came with it.

.DESCRIPTION
    Run on a Windows machine with the .NET 8 SDK and this repository checked out - the
    build reads the ICP SDK from id-card-toolkit-windows-sdk-v3.1.6\, so it cannot be
    published from anywhere else.

    The same output serves both deployments: the app on UATWEB01 with the reader on each
    desk, or the app installed on a reception PC. Toolkit:Mode decides which it is.

    The verification is the point. The app loads EIDAToolkit.dll by P/Invoke, which
    Windows resolves from the executable's own directory; if those files are absent the
    app starts, serves both screens, and fails only when someone inserts a card, with a
    message that names IDCardToolkit rather than the file that was missing. Better to
    fail here.

.EXAMPLE
    .\deploy\publish.ps1 -Output C:\Deploy\vms
#>
[CmdletBinding()]
param(
    # Where to publish. Copy this folder to the reception PC.
    [string] $Output = (Join-Path $PSScriptRoot '..\artifacts\publish'),
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$repo    = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $repo 'src\DI.Vms.Blazor\DI.Vms.Blazor.csproj'

if (-not (Test-Path $project)) { throw "Project not found at $project." }

# win-x64 explicitly: the toolkit binding and its native dependencies are x64 only, and
# an AnyCPU host would load neither.
dotnet publish $project `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained false `
    --output $Output
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

# Everything the reader path needs at runtime, learned by making it fail: the toolkit
# itself, its managed binding, the PC/SC reader plugin and the Morpho runtime.
$required = @(
    'DI.Vms.Blazor.exe',
    'IDCardToolkit.dll',
    'EIDAToolkit.dll',
    'PCSCLib.dll'
)

$missing = $required | Where-Object { -not (Test-Path (Join-Path $Output $_)) }
if ($missing) {
    throw @"
Publish output is incomplete - card reading would fail at runtime with 0x8007007E.
Missing: $($missing -join ', ')
Check that id-card-toolkit-windows-sdk-v3.1.6\quickstart\64 is present in the repository
and that the ToolkitNative items in DI.Vms.Blazor.csproj still set CopyToPublishDirectory.
"@
}

$count = (Get-ChildItem -Path $Output -Filter *.dll -File).Count
Write-Host ''
Write-Host "Published to $Output ($count DLLs, all required toolkit files present)."
Write-Host 'Next: copy the folder to UATWEB01, then follow docs/deployment.md.'
