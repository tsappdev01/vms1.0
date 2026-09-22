# Hosting the tablet API on Azure

`src/DI.Vms.Api` is the visitor API on its own, for reception tablets that are not on the
office LAN.

It is the same code as the API inside the Blazor app — `Api/VisitsApi.cs`, the response
parser, the reader and the data model are compiled into both from one set of files. There
is one definition of what a valid visit is, and it does not fork.

---

## This deployment

Decided and created in the portal:

| | |
|---|---|
| Web App | **VMS**, resource group `DotNetSites`, plan `ASP-DotNetSites-8061` |
| | `vms-cebrd3evb0cyg0gn.uaenorth-01.azurewebsites.net`, UAE North |
| OS / stack | **Windows**, .NET 8 — which is what allows the Blazor app to run there |

> **The Web App's operating system decides which project can go on it, and it cannot be
> changed after the Web App is created.**
>
> `DI.Vms.Blazor` is `net8.0-windows`. It has to be, because it references ICP's
> `IDCardToolkit.dll` — a .NET Framework assembly that P/Invokes native Windows DLLs — and
> because `publish-azure.ps1` publishes it `-r win-x64`. Deployed to a **Linux** Web App it
> does not start, and App Service answers **503** with "Issues Detected" on the overview
> blade. There is no setting that fixes this; the OS is fixed when the plan is created.
>
> So pick the script that matches the plan:
>
> | Plan OS | Script | What you get |
> |---|---|---|
> | **Windows** | `deploy\publish-azure.ps1` | Screens and `/api`, with the in-process card reader — the build that also goes to UATWEB01 and to reception PCs. |
> | **Linux** | `deploy\publish-azure-linux.ps1` | Screens and `/api`, portable, no toolkit. **Everything except reading a card from a reader plugged into the server** — which no server does. |
> | **Linux, API only** | `deploy\publish-api.ps1` | `DI.Vms.Api` — the tablet endpoints alone, no screens. |
>
> The middle row is the one to use for a Linux Web App that has to serve the desk as well as
> the tablet. `-p:VmsAgentOnly=true` builds the same application as plain `net8.0` with
> ICP's toolkit left out; in agent mode the server never touches it anyway, because the card
> is read by the desk browser through ICP's agent or by the tablet through the Android SDK
> and what arrives is signed XML this process verifies itself.
>
> **`Toolkit__Mode=Agent` is required on that build.** `InProcess` there is a host told to
> use a reader it cannot have — the screens say so plainly rather than failing obscurely,
> but it is still a deployment that will not read cards.

| SQL server | `ts-db.database.windows.net`, admin `sqladmin` |
| Database | **`vms`** |

So it is the **one app** case: `src/DI.Vms.Blazor` publishes to that Web App and serves the
reception screens and `/api` together. `src/DI.Vms.Api` is not needed for this and stays
for the day a Linux API-only host is wanted.

On a **Windows** Web App:

```powershell
git pull
Set-ExecutionPolicy Bypass -Scope Process -Force
.\deploy\publish-azure.ps1 -Output C:\Deploy\vms-azure

az webapp deploy --resource-group DotNetSites --name VMS --src-path C:\Deploy\vms-azure\DI.Vms.zip --type zip
```

On a **Linux** Web App — same app, portable build:

```powershell
git pull
Set-ExecutionPolicy Bypass -Scope Process -Force
.\deploy\publish-azure-linux.ps1 -Output C:\Deploy\vms-linux

az webapp deploy --resource-group DotNetSites --name VMS --src-path C:\Deploy\vms-linux\DI.Vms.zip --type zip

az webapp config set -g DotNetSites -n VMS --linux-fx-version "DOTNETCORE|8.0" `
  --web-sockets-enabled true --always-on true --min-tls-version 1.2 `
  --generic-configurations '{\"healthCheckPath\": \"/health\"}'

az webapp config appsettings set -g DotNetSites -n VMS --settings Toolkit__Mode=Agent
az webapp update -g DotNetSites -n VMS --https-only true
```

The platform settings below, in one command, for a Web App that has just been created:

```powershell
az webapp config set --resource-group DotNetSites --name VMS `
  --net-framework-version v8.0 `
  --use-32bit-worker-process false `
  --web-sockets-enabled true `
  --always-on true `
  --min-tls-version 1.2 `
  --generic-configurations '{\"healthCheckPath\": \"/health\"}'

az webapp update --resource-group DotNetSites --name VMS --https-only true
az webapp restart --resource-group DotNetSites --name VMS
```

### App Service settings that are not defaults

Configuration → General settings:

| | | |
|---|---|---|
| **Stack / .NET version** | **.NET 8 (LTS)** | The portal showed `Dotnet - v10.0` on the Web App as created. The app is `net8.0`, and .NET does not roll forward across a major version on its own, so it would be looking for a runtime the site is not configured for. Set it to 8. |
| **Platform** | **64 Bit** | `DI.Vms.Blazor` is `PlatformTarget x64` and its toolkit reference P/Invokes native x64 DLLs. In a 32-bit worker the process starts and fails on the first card read with `0x8007000B` — a message about a bad image format, not about the bitness. Basic and Free plans default to 32-bit. |
| **Startup Command** | **empty**, or `dotnet DI.Vms.Blazor.dll` | On Linux, a startup command of `dotnet DI.Vms.Blazor` - without the extension - makes the runtime look for a *tool* by that name and answer "The application 'DI.Vms.Blazor' does not exist. No .NET SDKs were found", which reads like a missing runtime and is not. Left empty, Oryx finds the assembly itself from `DI.Vms.Blazor.runtimeconfig.json`; empty is better, because there is then nothing to keep in step if the assembly is renamed. The site answers **503** while this is wrong, with nothing in the browser to say why - the reason is only in the container log. |
| **Web sockets** | **On** | Blazor Server is SignalR. Without WebSockets it falls back to long polling: the screens work, slowly, and drop their connection under any load. This is the one that gets missed. |
| **Always On** | **On** | Otherwise the app unloads after 20 minutes idle and the first check-in of the morning waits for a cold start. |
| **HTTPS Only** | **On** | The app does no redirect of its own, deliberately — App Service terminates TLS and forwards plain HTTP internally, so a redirect in the app either does nothing or loops. |
| **Minimum inbound TLS** | **1.2** | |
| **ARR affinity** | **On** (default) | Blazor circuits are stateful, and `AgentCardReader` holds outstanding read IDs in memory. Leave it on, and keep the plan at one instance. |

Monitoring → Health check → Path: **`/health`**. It answers `{"status":"ok"}` when the
database is reachable and `degraded` when it is not — 200 either way on purpose, so a brief
database outage does not turn into a restart loop on a single instance.

### Or the other way round: a settings file on the Web App

The settings above can live in `appsettings.Production.json` in `/site/wwwroot` instead,
uploaded over FTPS. `deploy/appsettings.Production.azure.json.template` is the starting
point; that file name is gitignored, which is what makes it a place a credential can go.

**One setting belongs in one place.** Configuration sources are read in order and the last
wins: `appsettings.json`, then `appsettings.Production.json`, then environment variables —
which is what App Service application settings *are*. So a setting left in the portal
silently overrides the same setting in the file, and you would be editing something that
does nothing. To use the file, delete the matching application settings in the portal.

What it costs, so it is a choice and not an accident: a file in `wwwroot` is readable by
anyone with FTP or Kudu access, is in any backup of the site, and persists in the file
system. Application settings are encrypted at rest, are not in the site's files, and a
change to one appears in the activity log. That is why this runbook prefers them — but a
single file next to the app, with FTP access restricted, is a defensible trade for not
having to go through the portal to change a setting.

It is deliberately not in the deployment package: `publish-azure-linux.ps1` strips it, so a
connection string never travels in a zip. Upload it separately — and again after any
deployment that empties `wwwroot`.

### Everything comes from Azure, nothing from a file

ASP.NET Core reads environment variables last and they win over every file, so App Service
configuration overrides `appsettings.json` without anything in the code. Two details make
that dependable rather than accidental:

- **`appsettings.Production.json` must not be in the package.** If one is present it is
  loaded, and it beats `appsettings.json` — so a stale local copy would silently override
  half of what you set in the portal. `publish-azure.ps1` deletes it from the package and
  says so.
- **`__` is the separator.** `Toolkit__Agent__TlsEnabled` becomes `Toolkit:Agent:TlsEnabled`.
  For a list it is an index: `Toolkit__Agent__TrustedSignerThumbprints__0`.

Two tabs, and the difference matters. **App settings** for everything; **Connection
strings** for the database. A connection string entered on its own tab arrives as
`SQLAZURECONNSTR_Vms`, which `GetConnectionString("Vms")` reads exactly as it reads the
file — and the portal treats it as a credential: masked by default, and it does not appear
in the app settings list where it is easy to screenshot.

Both tabs have an **Advanced edit** button that takes JSON in bulk, which beats adding nine
rows by hand. The two files to paste are in the repository:

| | |
|---|---|
| App settings → Advanced edit | [`deploy/azure-app-settings.template.json`](../deploy/azure-app-settings.template.json) |
| Connection strings → Advanced edit | [`deploy/azure-connection-strings.template.json`](../deploy/azure-connection-strings.template.json) |

Replace the two `REPLACE-WITH-` values before pasting — the API key and the SQL password.
Neither file carries a real secret, which is why they can live in git; do not paste the
filled-in versions back into them.

Generate the key once, on the machine that builds the tablet. **In PowerShell**, not the
Command Prompt:

```powershell
$bytes = New-Object byte[] 48
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

`RandomNumberGenerator.Create()`, rather than the static `GetBytes(48)` overload: that
static exists only on modern .NET, and Windows PowerShell 5.1 - the blue one, and the
default on a Windows desktop - runs on .NET Framework, where it fails with
"does not contain a method named 'GetBytes'". The form above works on both.

After Apply, App Service restarts the app. Confirm it took:

```powershell
Invoke-RestMethod https://vms-cebrd3evb0cyg0gn.uaenorth-01.azurewebsites.net/health
```

`{"status":"ok"}` means the app started and reached `vms` on `ts-db`. `degraded` means the
connection string or the SQL firewall.

### The settings themselves

Azure maps `__` to `:`.

| Name | Value |
|---|---|
| `ConnectionStrings__Vms` | `Server=tcp:ts-db.database.windows.net,1433;Initial Catalog=vms;User ID=sqladmin;Password=<the password>;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;Connection Timeout=30;` |
| `Toolkit__Mode` | **`Agent`** — not optional. Unset, the app assumes the in-process reader and looks for a smartcard reader in a datacentre. |
| `Toolkit__Agent__TlsEnabled` | `false` |
| `Toolkit__Agent__RequireSignature` | `false`, while ICP's licence is the offline bundle |
| `Authentication__Enabled` | `false` for now — **but read the warning below** |
| `Api__Key` | A long random string. Guards `/api`; see the security section. |

`Toolkit__Agent__HostName` stays unset — blank means the literal `127.0.0.1`, which is what
earns the loopback exemption that lets an HTTPS page open a plain `ws://` socket.

Card reading is unaffected by the move: ICP's agent runs on the attendant's own PC and the
page talks to it at `ws://127.0.0.1:9004`, wherever the server is.

### Two things to check on the portal side

**Public access.** The Web App shows a private endpoint and VNet integration
(`ApplicationGateway1VN/webapp1Subnet`). If public network access is disabled, tablets on
the internet cannot reach it and only traffic through the Application Gateway will. Confirm
which you want before wondering why the tablet cannot connect.

**SQL firewall.** On `ts-db` → Networking, allow Azure services (or the Web App's outbound
addresses), and your own address for SSMS.

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

**Decided: Azure SQL — server `ts-db.database.windows.net`, database `vms`.**

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
   list and the columns added since. Connect to **`vms`** in SSMS before executing: the
   scripts do not switch databases, because Azure SQL cannot, and they refuse to run in
   `master`.
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
| `Directory__Source` | `EntraId` — the "person to visit" list reads the SSO directory. `Database` uses the exported `vms.Person` table instead. |
| `Directory__TenantId` / `Directory__ClientId` / `Directory__ClientSecret` | The registration the directory is read with. Each falls back to its `AzureAd__` equivalent, so one registration serving both needs only `Directory__Source`. The registration needs **`User.Read.All` as an application permission with admin consent** — `docs/entra-id-setup.md`. |
| `Storage__ConnectionString` | The storage account the card images go to. Unset, they are stored in the database instead, which is what UATWEB01 does. |
| `Storage__Container` | `vms`. Created on first use if it is not there. |

Everything above is an **application setting**, including the two credentials. Nothing
belongs in `appsettings.json`: that file is committed, and a secret committed once is in the
history for good — a client secret and a storage account key are each a credential to the
whole thing they name.

The storage container is created private and stays private. Nothing is ever served from a
storage URL: `/visits/{id}/card` reads the blob and returns it through the app, behind the
report role and under a policy that forbids script. That matters because the card is an SVG,
which is a document a browser will execute — on the storage account's origin, if it were
ever fetched from there — and because a blob URL, signed or not, is a link to a visitor's
Emirates ID photograph that can be forwarded.

Generate the key on the machine you will build the tablet from, so it is never typed twice:

```powershell
$bytes = New-Object byte[] 48
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
$key
```

`RandomNumberGenerator`, not `Get-Random`. `Get-Random` is a general-purpose PRNG seeded
from the clock — fine for shuffling a list, not for the one credential standing in front of
the visitor database on a public host, where an attacker who can guess the seed can
enumerate the key. 48 bytes is 64 base64 characters; the app refuses anything under 32.

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

## When it answers 503

App Service returns 503 when the request reached it and no healthy worker answered - which
for this app always means it failed to start. The browser says nothing useful; the reason is
in the container log:

```
https://vms-cebrd3evb0cyg0gn.scm.uaenorth-01.azurewebsites.net/api/logs/docker
```

That returns a short JSON list; open the `_default_docker.log` href. Or **Monitoring → Log
stream** in the portal, with a restart to trigger it.

Every reason this app refuses to start is a written sentence, so the log names the cause:

| In the log | Cause |
|---|---|
| `The application 'DI.Vms.Blazor' does not exist` / `No .NET SDKs were found` | The **Startup Command**, missing `.dll` — see the settings table above. Not a missing runtime, despite what it says. |
| `No connection string named Vms` | Neither the app settings nor `appsettings.Production.json` supplied one. Note that `appsettings.json` ships in the package, so a deployment overwrites an edited copy in `wwwroot`. |
| `The database is missing N column(s) the code expects` | The `db/` scripts have not been run against this database. It names them. |
| `Api:Key is N characters` | Under 32, or missing while sign-in is off. |
| `Toolkit:Mode is '...'` | Not one of InProcess, Agent or Off. |

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

## Security on a public host

What the API does on its own, with no configuration:

| | |
|---|---|
| **Credential on every `/api` call** | Entra bearer token, or the API key. `DI.Vms.Api` refuses to start with neither. Compared in fixed time; rejections are logged with the caller's address, so someone trying keys leaves a visible trail. |
| **Rate limiting** | 300 requests a minute per calling address, with rejections logged. It is a flood limit, not a quota — a check-in is roughly twenty requests. |
| **Real client addresses** | `UseForwardedHeaders`, so the rate limiter and the logs see the caller rather than App Service's load balancer. Without it both are one global bucket and quietly useless. |
| **Request size cap** | 12 MB at Kestrel. The default is 30 MB, which is 30 MB buffered before any of our code looks at it. |
| **No stack traces** | `AddProblemDetails` plus `UseExceptionHandler`: a structured answer, and nothing about the inside of the server. |
| **Least data on the wire** | `/api/people` returns name, title and employer. No email addresses — the tablet never displayed them, and the saved visit takes the host's email from the database on the server. |
| **Health says one word** | `/health` is anonymous and answers `ok` or `degraded`. The detail — which authentication, whether the database is reachable, whether signatures are required — is at `/api/health`, behind the same credential as everything else. |
| **Single-use card reads** | The request ID is issued by the server, removed when redeemed, expires in five minutes, and must match the one inside the signed document. A captured response cannot be replayed. |

What is still yours to do in the portal, and each of these matters more than anything above:

1. **Turn Entra sign-in on.** `Authentication__Enabled=true`. The API key is a shared secret
   in an APK: it does not expire, it names a fleet rather than a person, and anyone who
   unpacks the app can read it. Every visit recorded with it says `(not signed in)`.
   Everything else on this page is mitigation for not having done this.
2. **HTTPS Only**, and **minimum inbound TLS 1.2**. Configuration → General settings.
3. **Access restrictions** if reception's addresses are known. Networking → Access
   restrictions. An API on `*.azurewebsites.net` is found by scanners within hours.
4. **Put `Api__Key` and the connection string in Key Vault** and reference them, rather
   than as plain App Service settings — so they are not readable by everyone with portal
   access to the Web App.
5. **Rotate the key** on a schedule, and whenever a tablet is lost. Rotating means changing
   it in the portal and rebuilding the APK; there is no revocation short of that, which is
   another reason (1) is the real answer.
6. **Turn on diagnostic logging** to a Log Analytics workspace, so the rejection and rate
   limit warnings survive a restart and can be alerted on.

### What this does not protect against

**A stolen tablet is a valid client.** The key is in the APK. Until Entra is on, losing a
device means rotating the key and rebuilding every other device. With Entra on it means
disabling one account.

**The key holder can read the staff directory.** `/api/people` answers name-fragment
queries across 725 people. Rate limiting makes scraping slow and noisy; it does not make it
impossible. Roles do.

**Nothing is encrypted at rest beyond what the database does.** Emirates ID numbers and
photographs sit in `vms.VisitorEntry` as they do on-premises. Azure SQL is encrypted at
rest by default (TDE), which is more than UATWEB01 offers, but column-level protection for
the ID number is still an open item.

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
