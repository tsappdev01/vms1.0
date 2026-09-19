# Hosting the tablet API on Azure

`src/DI.Vms.Api` is the visitor API on its own, for reception tablets that are not on the
office LAN.

It is the same code as the API inside the Blazor app — `Api/VisitsApi.cs`, the response
parser, the reader and the data model are compiled into both from one set of files. There
is one definition of what a valid visit is, and it does not fork.

---

## One Web App, or two

The Blazor app **already contains the API**. `Program.cs` calls `MapVisitsApi`, so
`vms.dipark.com` serves the reception screens and `/api/*` from one process today. Putting
both on a single Azure Web App therefore needs no new project — it needs the existing app
published to Azure.

| | What runs | Web App OS | When |
|---|---|---|---|
| **One app** | `src/DI.Vms.Blazor` — screens and API together | **Windows** | The desk browser and the tablets should share one hostname and one deployment. |
| **Two apps** | Blazor on UATWEB01, `src/DI.Vms.Api` in Azure | Linux | Only the tablets need to reach Azure; the screens stay on-premises. |

**Windows is not a preference, it is a constraint.** `DI.Vms.Blazor` targets
`net8.0-windows` and references ICP's `IDCardToolkit.dll`, which P/Invokes Windows native
code. It cannot run on a Linux App Service. `DI.Vms.Api` targets plain `net8.0` precisely
so that it can, which is the only reason it exists as a separate project.

The desk browser still works from Azure, and this is worth knowing rather than assuming:
ICP's agent runs on the *attendant's own PC*, and the page talks to it at
`ws://127.0.0.1:9004`. The loopback address is a trustworthy origin, so an HTTPS page
served from Azure may open that socket. Where the server lives makes no difference to
card reading.

### Before putting the screens on the internet

An API key guards `/api`. **It does not guard the screens.** The Visitor Report is a Blazor
page, and on a public Web App with `Authentication:Enabled=false` it is served to anyone
who finds the URL — every visitor's name, Emirates ID number, date of birth and
photograph, with a CSV export button.

That is a materially different exposure from the API alone, and the API key does nothing
about it, because a browser cannot send one. So if the screens are going on a public
hostname, one of these has to be true first:

- **`Authentication:Enabled=true`.** Finish `docs/entra-id-setup.md`. This is the answer.
- **Access restrictions** — Networking → Access restrictions, limited to the office's
  outbound addresses. Works only while every user is on that network, so tablets on mobile
  data stop working.
- **App Service authentication** (Easy Auth) in front of the whole site. Zero code, but it
  challenges `/api` too, so the tablet needs a real token — which is Entra again, the long
  way round.

Hosting the API alone in Azure and leaving the screens on UATWEB01 avoids the question
entirely, which is the argument for two apps.

## Why this exists

`vms.dipark.com` resolves to `192.168.28.13`. That is an internal address on internal DNS,
so a tablet can reach it from the office network and nowhere else. If reception's tablets
will only ever sit at a DIP desk on the office wifi, **this project is not needed** — point
them at the existing host and skip everything below.

It earns its place when a tablet has to work from somewhere else: a different site, a
guest network, an event desk, a phone hotspot.

---

## 1. The database

**Decided: Azure SQL, created in the portal alongside the Web App.**

Create the SQL Database and its logical server first — the Web App needs its connection
string, and the database needs schema in it before the app will serve anything. Basic or
S0 is plenty for a visitor log; it can be scaled later without redeploying.

On the server's **Networking** blade, allow Azure services to reach it (or add the Web
App's outbound IPs). On its **Microsoft Entra ID** blade, set an Entra admin — that is
what lets you create the managed-identity user in step 3, and it is worth doing now
because it cannot be done from a SQL-authenticated session.

### There must be exactly one VMS database

This is the part to be deliberate about. If Azure SQL becomes the tablets' database while
UATWEB01 keeps its local one, the visitor log splits in half: a visitor checked in at the
desk browser is invisible to the tablet, the report shows some of the day's visits, and
nothing looks broken enough to investigate for weeks.

So when the Azure database goes live, **point the Blazor app on UATWEB01 at it too** —
`ConnectionStrings:Vms` in `appsettings.Production.json`, then recycle the pool. The desk
browser and the tablets then read and write the same rows, which is the only arrangement
that is actually a visitor management system.

Two consequences worth accepting knowingly: every desk check-in then depends on the
internet link, and any visits already recorded on UATWEB01 need migrating across or
abandoning. For UAT, abandoning them is usually fine — but decide, rather than discover.

### Set the schema up, in this order

There is no schema-creation script. `Data/DbBootstrapper.cs` builds the tables from the EF
model at startup, which is right for a brand new database and is exactly what a fresh
Azure SQL database is.

1. Point `ConnectionStrings__Vms` at the **SQL admin** account and start the Web App once.
   The log names each table it creates.
2. Run `db\001`–`005` and `db\007` against the database — the reference data, the entity
   list and the columns added since. Connect to **VMS**; the scripts do not switch
   databases, because Azure SQL cannot.
3. Run `db\008_grant_app_user_azure.sql` to create the app's own least-privileged user.
   `db\006` is for SQL Server and does not work here — Azure SQL has no Windows logins.
4. Change `ConnectionStrings__Vms` to that user and restart. The admin credential is not
   what the app should run as.

Managed identity is worth the extra five minutes: enable the Web App's system-assigned
identity, use Option A in `008`, and the connection string carries no secret at all —

```
Server=tcp:<server>.database.windows.net,1433;Database=VMS;Authentication=Active Directory Default;Encrypt=True;
```

---

## 2. Create the Web App

Azure portal → Create → **Web App**.

| | |
|---|---|
| Publish | Code |
| Runtime stack | **.NET 8 (LTS)** |
| Operating System | **Windows** for one app (Blazor + API), **Linux** for the API alone — see the top of this file |
| Plan | B1 to start. Not Free — F1 sleeps, and a sleeping API means the first check-in of the morning times out. |

`DI.Vms.Api` targets plain `net8.0` precisely so it can be Linux. `DI.Vms.Blazor` cannot:
it is `net8.0-windows` and references ICP's toolkit, so hosting the screens means a
Windows plan.

## 3. Configuration

Web App → **Settings → Environment variables**. Azure maps `__` to the `:` in
configuration keys.

| Name | Value |
|---|---|
| `ConnectionStrings__Vms` | The database, per the decision above. Mark it a **Connection string** of type SQLAzure rather than an app setting if you prefer; either is read. |
| `Api__Key` | A long random string — see below. `DI.Vms.Api` refuses to start without it while sign-in is off. `DI.Vms.Blazor` treats it as optional and enforces it when set, so that UATWEB01 — which has never had one — keeps working; set it on any public host. |
| `Authentication__Enabled` | `false` for now. `true` once the Entra work in `docs/entra-id-setup.md` is finished. |
| `Toolkit__Agent__RequireSignature` | `false`, while ICP's licence is the offline bundle and responses come back unsigned. |
| `AzureAd__TenantId` / `AzureAd__ClientId` | Only when `Authentication__Enabled` is `true`. Values are in `docs/entra-id-setup.md`. |

Generate the key on the machine you will build the tablet from, so it is never typed twice:

```powershell
$key = [Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
$key
```

Paste it into `Api__Key`, and build the tablet with the same value —
`-PVMS_API_KEY="$key"`. It goes in neither repository.

Then **Settings → Configuration → General settings**: **HTTPS Only** on, **Minimum inbound
TLS** 1.2. The app does no HTTPS redirect of its own; App Service terminates TLS and
forwards plain HTTP internally, so redirecting in the app would either do nothing or loop.

## 4. Publish

For the **API alone** (Linux Web App):

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
.\deploy\publish-api.ps1 -Output C:\Deploy\vms-api
```

For **both on one Web App** (Windows), it is the existing script — the same output that
goes to UATWEB01, zipped:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
.\deploy\publish.ps1 -Output C:\Deploy\vms
Compress-Archive -Path C:\Deploy\vms\* -DestinationPath C:\Deploy\vms.zip -Force
```

That produces a zip. Deploy it with the Azure CLI:

```powershell
az webapp deploy --resource-group <rg> --name <app-name> --src-path C:\Deploy\vms-api\DI.Vms.Api.zip --type zip
```

Or drag the zip into the portal's Advanced Tools → Kudu. Publishing from Visual Studio
works too; the project is an ordinary ASP.NET Core app.

## 5. Check it before involving a tablet

```powershell
Invoke-RestMethod https://<app-name>.azurewebsites.net/health
```

```json
{ "status": "ok", "database": "ok", "authentication": "api-key" }
```

`database: unreachable` means the connection has not been solved — go back to the decision
above. The endpoint deliberately does not say *why* it is unreachable: that would describe
the inside of the network to anyone who asks.

Then prove the key is actually required:

```powershell
# no key - expect 401
Invoke-WebRequest https://<app-name>.azurewebsites.net/api/reference -SkipHttpErrorCheck |
  Select-Object StatusCode

# with the key - expect entities and purposes
Invoke-RestMethod https://<app-name>.azurewebsites.net/api/reference -Headers @{ 'X-Vms-Key' = $key }
```

If the first one returns data, stop and fix it before the tablet gets near it.

## 6. Point the tablet at it

```powershell
cd android
.\gradlew.bat :app:assembleDebug --console=plain `
  -PVMS_API_BASE_URL="https://<app-name>.azurewebsites.net/" `
  -PVMS_API_KEY="$key"
```

The trailing slash matters — Retrofit drops the last path segment without it.

`*.azurewebsites.net` carries a certificate from a public CA, so Android trusts it with no
network security config. That is the second thing this host fixes, after routing: a tablet
on any network can verify it.

---

## What is not solved

**The API key is not sign-in.** It is the same secret on every tablet, it does not expire,
it names a fleet rather than a person, and anyone who unpacks the APK can read it. It
stops the endpoint answering strangers; it does not tell you who checked a visitor in —
every visit records `(not signed in)`. Turning `Authentication__Enabled` on is the fix, and
it is the reason the Entra work is worth finishing.

**Keep the Web App at one instance.** `AgentCardReader` holds the outstanding read request
IDs in memory. Scaled out, a read begun on one instance and completed on another is
rejected, because the second never issued the ID. Scale up, not out — or move that state to
a shared store first.

**Visitor data would now cross the internet.** Emirates ID numbers and photographs go from
the tablet to Azure and from Azure to the database. TLS covers them in transit; nothing
here encrypts them at rest, which is an open item on the on-premises deployment too.
Worth a word with whoever signs off ITGC before this carries real visitors.

**Restrict who can reach it.** Networking → Access restrictions, if reception's tablets
come from known addresses. An API on `*.azurewebsites.net` is found by scanners within
hours of existing.
