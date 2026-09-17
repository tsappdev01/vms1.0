# Hosting the tablet API on Azure

`src/DI.Vms.Api` is the visitor API on its own, for reception tablets that are not on the
office LAN.

It is the same code as the API inside the Blazor app — `Api/VisitsApi.cs`, the response
parser, the reader and the data model are compiled into both from one set of files. There
is one definition of what a valid visit is, and it does not fork.

---

## Why this exists

`vms.dipark.com` resolves to `192.168.28.13`. That is an internal address on internal DNS,
so a tablet can reach it from the office network and nowhere else. If reception's tablets
will only ever sit at a DIP desk on the office wifi, **this project is not needed** — point
them at the existing host and skip everything below.

It earns its place when a tablet has to work from somewhere else: a different site, a
guest network, an event desk, a phone hotspot.

---

## Decide the database first

**This is the decision the whole thing turns on, and it is not a deployment detail.** An
Azure Web App cannot reach SQL Server on UATWEB01. It is a private address behind the
company firewall, and nothing in Azure routes to it.

There must be **exactly one** VMS database. Two — one on-premises for the desk browser, one
in Azure for the tablets — would mean a visitor checked in at the desk is invisible to the
tablet, the report shows half the visits, and neither is wrong enough to notice quickly.
That is a worse outcome than the tablets not working.

Three ways to get there:

| | What it means | Cost |
|---|---|---|
| **Azure SQL Database** | The database moves to Azure. UATWEB01's app points at it too, over the internet with a firewall rule. | A monthly bill; every desk read now depends on the internet link; a migration. |
| **App Service Hybrid Connections** | The database stays on UATWEB01. A small relay agent runs on-premises and Azure reaches SQL through it. No VPN, no port opened inbound. | An agent to install and keep running; limited throughput; still a dependency on that one machine. |
| **VPN / private endpoint** | Proper network-level connectivity between Azure and the DIP network. | Infrastructure work and network-team involvement. |

**Hybrid Connections is usually the right first move** — it keeps the data where it is,
opens no inbound firewall hole, and can be undone by uninstalling an agent. Azure SQL is
the better end state if VMS is going to be used beyond DIP, but moving the database is a
change to the whole system, not to the tablets.

Do not start with the Web App. Answer this first.

---

## 1. Create the Web App

Azure portal → Create → **Web App**.

| | |
|---|---|
| Publish | Code |
| Runtime stack | **.NET 8 (LTS)** |
| Operating System | **Linux** |
| Plan | B1 to start. Not Free — F1 sleeps, and a sleeping API means the first check-in of the morning times out. |

The project targets plain `net8.0` precisely so this can be Linux. The Blazor app cannot:
it is `net8.0-windows` and P/Invokes ICP's toolkit. This one never touches it — cards are
read by the client and arrive as signed XML.

## 2. Configuration

Web App → **Settings → Environment variables**. Azure maps `__` to the `:` in
configuration keys.

| Name | Value |
|---|---|
| `ConnectionStrings__Vms` | The database, per the decision above. Mark it a **Connection string** of type SQLAzure rather than an app setting if you prefer; either is read. |
| `Api__Key` | A long random string — see below. Required while sign-in is off; the app refuses to start without it. |
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

## 3. Publish

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
.\deploy\publish-api.ps1 -Output C:\Deploy\vms-api
```

That produces a zip. Deploy it with the Azure CLI:

```powershell
az webapp deploy --resource-group <rg> --name <app-name> --src-path C:\Deploy\vms-api\DI.Vms.Api.zip --type zip
```

Or drag the zip into the portal's Advanced Tools → Kudu. Publishing from Visual Studio
works too; the project is an ordinary ASP.NET Core app.

## 4. Check it before involving a tablet

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

## 5. Point the tablet at it

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
