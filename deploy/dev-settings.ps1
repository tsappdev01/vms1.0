<#
.SYNOPSIS
    Creates src\DI.Vms.Blazor\appsettings.Development.Local.json - the gitignored file a
    development machine keeps its connection string and secrets in.

.DESCRIPTION
    There used to be an appsettings.Development.Local.json.example beside appsettings.json
    to copy. It was edited in place three times instead of copied, each time putting a live
    SQL password into a committed file. A sample file sitting next to the real thing, named
    almost the same, is an invitation to edit it - so there is no sample file any more.
    There is this, which cannot be edited by mistake because running it is the only thing
    it does.

    It refuses to overwrite an existing file. Nothing here is worth losing somebody's
    working settings for.

.EXAMPLE
    .\deploy\dev-settings.ps1

.EXAMPLE
    .\deploy\dev-settings.ps1 -Force        # replace the file that is already there
#>
[CmdletBinding()]
param(
    [switch] $Force
)

$ErrorActionPreference = 'Stop'

$root   = Resolve-Path (Join-Path $PSScriptRoot '..')
$target = Join-Path $root 'src\DI.Vms.Blazor\appsettings.Development.Local.json'

if ((Test-Path $target) -and -not $Force) {
    Write-Host ""
    Write-Host "Already there: $target" -ForegroundColor Yellow
    Write-Host "Opening it. Re-run with -Force to start again from a blank one."
    Write-Host ""
    Start-Process notepad.exe $target
    return
}

<# Every value is left empty rather than filled with a plausible-looking one. An empty
   string fails at startup with a message saying what is missing; a half-right default
   fails somewhere further in, at a point that no longer names the setting. #>
$template = @'
{
  // This file is gitignored (appsettings.*.Local.json) and is the only place on a
  // development machine that a password belongs. It is read only when
  // ASPNETCORE_ENVIRONMENT is Development, so it can never override Azure.
  //
  // Required. Azure SQL:
  //   Server=tcp:ts-db.database.windows.net,1433;Initial Catalog=vms;User ID=sqladmin;Password=...;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30
  // SQL Server on UATWEB01, or a local instance:
  //   Server=UATWEB01;Database=VMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
  "ConnectionStrings": {
    "Vms": ""
  },

  // Optional. Without these the "Person to visit" list falls back to the vms.Person table
  // instead of reading the Entra ID directory. The registration needs User.Read.All as an
  // application permission with admin consent - see docs/entra-id-setup.md.
  "Directory": {
    "Source": "EntraId",
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": ""
  },

  // Optional. Without this the card image is stored in the database, as it is on UATWEB01.
  "Storage": {
    "ConnectionString": "",
    "Container": "vms"
  },

  // Optional. "Off" lets the card step accept typed details, so the rest of the screens can
  // be worked on without a reader attached. "InProcess" is the default and wants one.
  "Toolkit": {
    "Mode": "InProcess"
  }
}
'@

Set-Content -Path $target -Value $template -Encoding UTF8

Write-Host ""
Write-Host "Created $target" -ForegroundColor Green
Write-Host ""
Write-Host "Fill in ConnectionStrings:Vms at least - the app refuses to start without it."
Write-Host "It is gitignored, so what you put in it stays on this machine."
Write-Host ""

# Proves it rather than asserting it: a file this ends up tracking is a password published.
Push-Location $root
try {
    $ignored = & git check-ignore $target 2>$null
    if (-not $ignored) {
        Write-Warning "git does not consider that file ignored. Do NOT commit it. Check .gitignore has appsettings.*.Local.json."
    }
    else {
        Write-Host "Confirmed ignored by git." -ForegroundColor DarkGray
    }
}
finally { Pop-Location }

Start-Process notepad.exe $target
