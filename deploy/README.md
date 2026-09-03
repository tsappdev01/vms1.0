# deploy

Scripts for putting VMS on a reception PC. The runbook they belong to is
[`docs/deployment.md`](../docs/deployment.md) — read that first, because where this app can
run is decided by the card reader, not by preference.

| File | Run where | What it does |
|---|---|---|
| `publish.ps1` | Build machine, .NET 8 SDK, this repo | `dotnet publish -r win-x64`, then verifies `EIDAToolkit.dll` and `PCSCLib.dll` actually came with it. |
| `appsettings.Production.json.template` | Copy beside the published exe | Connection string, toolkit config path, loopback URL. The real file is gitignored. |
| `install-service.ps1` | Reception PC, elevated | Creates or reconfigures the `DIVms` Windows Service, automatic start, restart on failure. |

The SQL half of a deployment is [`db/006_grant_app_login.sql`](../db/006_grant_app_login.sql).
