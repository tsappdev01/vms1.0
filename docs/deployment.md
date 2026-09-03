# Deploying VMS

## Read this first: the reader decides where the app runs

The app reads the Emirates ID **chip**, in-process, through the ICP toolkit
(`application_type = APP_INPROC`). In Blazor Server every line of component code runs on
the server. So the machine that runs the app is the machine that must have the card
reader plugged into it.

Deploy this build to a central server and the two screens will render perfectly, the
database will fill, and **Read Card will look for a reader attached to the server**. It
will report no reader, or read whatever card is sitting in a reader in the data centre.

That leaves three options, and they are different amounts of work.

### A. Run it on the reception PC — works today, no code change

The app runs as a Windows Service on the reception machine, serving `http://127.0.0.1`
to the browser on that same desk. The **database stays central** on UATWEB01, so the
report is the whole office's data however many desks there are.

This is what the current build is designed for and what the rest of this document covers.
For one or two reception desks it is the right answer, and it is the only option that is
ready now.

Cost: an app folder per desk, and a redeploy per desk when a version ships. `deploy\`
scripts make each one a two-command job.

### B. Central server + the ICP agent on each desk — the vendor's web architecture

ICP ships this for exactly this problem. The SDK includes:

| Piece | Where |
|---|---|
| The agent | `id-card-toolkit-windows-sdk-v3.1.6\bin\agent\64\EIDAToolkitService.exe` |
| Its installer | `...\installer\ICAToolkitService\64\ICAToolkitService.msi` |
| The browser library | `id-card-toolkit-windows-web-javascript-sdk-v3.1.6\...\lib\web\eidatoolkit.js` |

`eidatoolkit.js` does not talk to a server. It opens a WebSocket to an agent on the
**user's own machine** — `ws://` or `wss://` per `agent_tls_enabled`, against
`127.0.0.1:9004`, `:9005` or `:9020`, protocol `eida-toolkit`, with a JNLP download as
the fallback when no agent answers. The card is read by the desk, the result is posted to
the server.

What it costs:

1. `ICAToolkitService.msi` installed on every reception PC, and the reader driver with it.
   The app folder disappears from the desks; an MSI takes its place.
2. `Services/CardReaderService.cs` is replaced by JS interop against `eidatoolkit.js`.
   The service's shape — one call in, a filled model out, progress reported per phase —
   can survive; its body cannot.
3. **The signed XML has to be validated on the server.** Today the read happens inside
   the app's own process, so the data is trusted by construction. In B it arrives from a
   browser, which can send anything: a forged visitor record is a POST away unless the
   server checks ICP's signature over the XML itself.
4. Certificates: `wss://` from an HTTPS page needs the agent's certificate trusted on
   every desk, or the page must be served over HTTP, or mixed content blocks the socket.

Point 3 is the real work, and it is worth doing if there will be more than a handful of
desks — but it is a project, not a deployment.

### C. A bridge process of our own — don't

A small local HTTP service on each desk wrapping the in-process toolkit. This is what
`src/DI.Vms.CardBridge/` was going to be. It is option B with a component we would have
to write, sign, install and maintain, replacing one ICP already ships and supports. The
empty folder is removed in this commit.

---

## A: deploying to a reception PC

### 1. Publish (on a machine with the .NET 8 SDK and this repo)

```powershell
.\deploy\publish.ps1 -Output C:\Deploy\vms
```

`publish.ps1` runs `dotnet publish -r win-x64 --self-contained false` and then **verifies
that `EIDAToolkit.dll` and `PCSCLib.dll` are in the output**. That check is not
ceremonial: `CopyToOutputDirectory` does not imply `CopyToPublishDirectory`, and a publish
without those files starts, serves both screens, and fails on the first card with
`0x8007007E`, blaming an assembly that is present for a native file that is not. The
csproj now sets both; the script is the belt to that braces.

Self-contained is off, so the reception PC needs the **ASP.NET Core 8 Runtime**
(`dotnet-hosting-8.0.x-win.exe` or the Windows Hosting Bundle) and, for the toolkit, the
**VC++ 2013 x64 redistributable**. Publish with `--self-contained true` instead if
installing a runtime on the desk is not an option; the toolkit's native dependencies come
along either way.

### 2. Copy to the reception PC

Copy `C:\Deploy\vms` to, say, `C:\Program Files\DI VMS`. Also copy the ICP toolkit config
bundle — the folder holding `config_li`, ideally the complete one with `config_ag` beside
it — somewhere stable such as `C:\Program Files\DI VMS\toolkit-config`.

### 3. Configure

Copy `deploy\appsettings.Production.json.template` next to `DI.Vms.Blazor.exe` as
`appsettings.Production.json` and edit three things: the connection string, the toolkit
config directory on that machine, and the URL.

Keep the URL on **loopback**:

```json
"Kestrel": { "Endpoints": { "Http": { "Url": "http://127.0.0.1:5100" } } }
```

The app has **no authentication**. On loopback the only client is the browser on that
desk. Bind `0.0.0.0` and every visitor record — Emirates ID numbers, photographs, dates
of birth — is readable by anyone who can route to the PC. Do not change this without
adding sign-in first.

The service reads `appsettings.Production.json` only when `ASPNETCORE_ENVIRONMENT` is
`Production`, which is the default when the variable is unset. Leave it unset.

### 4. Give the service identity a SQL login

The service will not run as the receptionist, so the connection string's
`Trusted_Connection=True` authenticates as whatever the service runs as. Edit `@Login` at
the top of `db\006_grant_app_login.sql` and run it on UATWEB01:

```
sqlcmd -S UATWEB01 -E -i db\006_grant_app_login.sql
```

A **domain service account** (`DI\svc-vms`) is the simpler grant and the clearer audit
trail. The alternative is the PC's machine account (`DI\RECEPTION1$`) with the service
left as LocalSystem — one login per desk, and it changes if the PC is rebuilt.

It grants `db_datareader` + `db_datawriter` and `SELECT, INSERT, UPDATE` on schema `vms`.
Not `db_owner`: see the note in the script about what a permission error from the
bootstrapper is telling you.

### 5. Run the database scripts

In number order, once, on UATWEB01 — see [`db/README.md`](../db/README.md). Outstanding as
of this commit: `004_seed_people.sql` needs a re-run (the first run hit the `GO` bug and
set no entity links), and `005_add_group_companies.sql` is a decision, not a step.

### 6. Install the service

Elevated, on the reception PC:

```powershell
.\install-service.ps1 -Path 'C:\Program Files\DI VMS' -ServiceAccount 'DI\svc-vms' -PromptForPassword
```

It refuses to install if `EIDAToolkit.dll` is not beside the exe, sets automatic start,
configures restart-on-failure, starts the service, and tells you where the reason is if it
did not start. `Program.cs` calls `AddWindowsService`, so the process reports ready to the
service control manager instead of being killed after 30 seconds — and its content root
becomes the exe's folder rather than `C:\Windows\System32`.

To reinstall a new version: stop the service, copy the files over, start it. The script is
re-runnable and reconfigures rather than duplicating.

### 7. The desk browser

Open `http://127.0.0.1:5100`. For a kiosk, Edge in kiosk mode against that URL:

```
msedge.exe --kiosk http://127.0.0.1:5100 --edge-kiosk-type=fullscreen --no-first-run
```

### 8. Check it

- The sidebar's build stamp matches what you published. If it does not, you are looking at
  an older folder — that has happened before.
- The reader shows ready and the licence date reads *14 Apr 2027*.
- Insert a real card: photograph, signature, both names, all present.
- Save a visitor, then open the report and see the row.

If the service will not start, the reason is in the Application event log — the template
turns on the Event Log provider at Warning for this. The three that actually happen are a
wrong connection string, a service identity with no SQL login (step 4), and
`Toolkit:ConfigDirectory` pointing at a folder with no `config_li`.

---

## What is not ready for a live deployment

These are in the design document's section 9 and none of them is fixed by this commit.
They are decisions, and they belong to you, not to the deployment:

1. **No authentication.** Anyone who reaches the app can read every visitor record. The
   loopback binding is a containment measure, not a control.
2. **Emirates ID numbers, photographs and dates of birth are stored in plain text.** No
   column encryption, no Always Encrypted, no TDE unless UATWEB01 already has it.
3. **No audit trail.** Nothing records who read the report or when.
4. **No retention limit.** The screen tells the visitor their data is kept "only for this
   visit". Nothing deletes it. Either add the job or change the wording.
5. **`DbBootstrapper` is not a migration tool.** It creates absent tables and never alters
   a present one. Move to EF migrations before the database holds anything you cannot
   drop — a new property on `VisitorEntry` needs an `ALTER TABLE` script in `db/` today,
   and if one is forgotten the app fails at startup naming the missing column. That is the
   design working, but it is not a migration story.
6. **Pre-reset `vms` tables are still in the database** from the earlier design.

Item 1 is the one that should block a deployment reachable from the network. On loopback
at a staffed desk it is a documented risk; on `0.0.0.0` it is an incident waiting.
