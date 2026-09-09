# deploy

Scripts for deploying VMS. The runbook they belong to is
[`docs/deployment.md`](../docs/deployment.md) — read that first: the card reader decides
where this app can run, and the answer is not "wherever you like".

The deployment taken is **the app on UATWEB01, ICP's agent on every reception desk**.

| File | Run where | What it does |
|---|---|---|
| `publish.ps1` | Build machine, .NET 8 SDK, this repo | `dotnet publish -r win-x64`, then verifies the native toolkit DLLs actually came with it. |
| `appsettings.Production.uatweb01.json.template` | Copy beside the published app on UATWEB01 | Agent mode, the local database, and the signer to pin. |
| `update-server.ps1` | UATWEB01, elevated | Mirrors a publish folder over the deployment with robocopy, keeping `appsettings.Production.json`. Use this for every update — `Copy-Item -Recurse` into an existing tree leaves nested files stale. |
| `install-iis.ps1` | UATWEB01, elevated | Site and app pool, SNI HTTPS binding, **Anonymous authentication on and Windows off**, no idle recycle. |
| `install-desk-agent.ps1` | Each reception PC, elevated | Installs ICP's `ICAToolkitService.msi`, trusts its certificate, checks the agent is listening. |
| `appsettings.Production.reception-pc.json.template` | The other deployment | For running the app *on* a reception PC (`Toolkit:Mode: InProcess`), loopback-bound. |
| `install-service.ps1` | A reception PC, elevated | Same: installs the app there as a Windows Service. |

The SQL half is [`db/006_grant_app_login.sql`](../db/006_grant_app_login.sql) — on UATWEB01
the identity to grant is the IIS app pool, `IIS APPPOOL\DIVms`.

## PowerShell will refuse to run these

```
File ...\publish.ps1 cannot be loaded. The file is not digitally signed.
```

The execution policy on these machines is `AllSigned`, and none of these scripts are
signed. Lift it for the session — note that `Bypass` is a *value* for the first positional
parameter, not a switch, so `-Scope Process -Bypass` is the form that fails:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
```

It lasts for that window only, so it has to be done again in a new one. If even that is
refused, the policy comes from Group Policy, which outranks Process scope; bypass it per
invocation instead:

```powershell
powershell.exe -ExecutionPolicy Bypass -File .\deploy\publish.ps1 -Output C:\Deploy\vms
```

`Get-ExecutionPolicy -List` says which it is: a value against `MachinePolicy` or
`UserPolicy` means Group Policy, and then the second form is the only one that works.

Signing the scripts with the organisation's code-signing certificate would end this
properly, and is worth doing if VMS outlives UAT.
