# Deploying VMS

**The decision taken:** the app runs on **UATWEB01**, and each reception desk runs
**ICP's agent** so the browser there can read the card. This is ICP's own architecture for
a server-hosted web app, and it is what the rest of this document describes.

Everything about it follows from one fact, so it is worth stating before the steps.

## Why the desk needs anything installed at all

The Emirates ID is read from the **chip**, over PC/SC, by a reader plugged into a physical
machine. In Blazor Server every line of component code runs on the server, so a server
that calls the toolkit in-process is a server looking for a reader in the data centre.

ICP solves this by putting a small agent on the machine the reader is on:

| Piece | Where it lives |
|---|---|
| The agent's installer | `id-card-toolkit-windows-sdk-v3.1.6\installer\ICAToolkitService\64\ICAToolkitService.msi` |
| The browser library | `id-card-toolkit-windows-web-javascript-sdk-v3.1.6\...\lib\web\eidatoolkit.js`, now served from `wwwroot/lib/eidatoolkit/` |

`eidatoolkit.js` never talks to our server. It opens a WebSocket to `127.0.0.1` — or to
`toolkitagent.emiratesid.ae`, which resolves to loopback, when TLS is on — on ports 9004,
9005 and 9020 in turn, subprotocol `eida-toolkit`. The desk reads the card; the page posts
the result to UATWEB01.

So: **the app is deployed once, to UATWEB01. The agent is deployed to every reception PC.**

## The security consequence, which is not optional

In-process, the toolkit's output can be trusted because it never left the process. Through
an agent it arrives **from a browser**, and a browser is a program someone can replace.
Three things stand between that and a fabricated visitor record, and all three are
implemented in `Services/AgentCardReader.cs`:

1. **The server issues the request ID.** It is random, single-use, and expires in five
   minutes. The gateway stamps it into the signed response, so a response captured from an
   earlier visitor no longer matches anything outstanding.
2. **The signature is verified server-side, against a signer we name.** This is the part
   that is easy to get wrong: the certificate that signed the response travels *inside*
   the response, so `CheckSignature()` on its own says only "this document has not changed
   since whoever signed it did". Anyone can produce a document that passes. Only the
   signer's identity separates a real card from an invention — which is
   `Toolkit:Agent:TrustedSignerThumbprints`, and **step 6 below is where you fill it in**.
3. **Nothing the browser parsed is believed.** `wwwroot/js/card-agent.js` returns exactly
   one thing: the signed XML. Every field the screen shows — name, ID number, photograph,
   signature image — is parsed on the server out of that document
   (`Services/CardResponseParser.cs`). A field taken from browser-side JSON would make the
   signature decorative.

Until a thumbprint is pinned, reads still work — refusing them would leave the deployment
with no working path at all — but each one is flagged on screen and the observed thumbprint
is logged with the exact configuration line to paste in.

---

## A: deploy the app to UATWEB01

### 1. Publish

On a machine with the .NET 8 SDK and this repository (the build reads the ICP SDK out of
`id-card-toolkit-windows-sdk-v3.1.6\`, so it cannot be published from anywhere else):

```powershell
.\deploy\publish.ps1 -Output C:\Deploy\vms
```

The script verifies that `EIDAToolkit.dll` and `PCSCLib.dll` came with the publish.
UATWEB01 does not use them — nothing there reads a card — but the same output is what a
reception-PC install would need, and a publish that silently drops them is the defect that
prompted the check.

UATWEB01 needs the **ASP.NET Core 8 Hosting Bundle** (`dotnet-hosting-8.0.x-win.exe`).
That installs the runtime and the IIS module together; without the module every request
returns 500.19 or 502.5.

### 2. Copy it over

Copy the folder to UATWEB01, e.g. `C:\inetpub\vms`.

### 3. Configure

Copy `deploy\appsettings.Production.uatweb01.json.template` into that folder as
`appsettings.Production.json` and edit it. The three things that matter:

```jsonc
"ConnectionStrings": { "Vms": "Server=localhost;Database=VMS;Trusted_Connection=True;…" },
"Toolkit": {
  "Mode": "Agent",
  "Agent": { "TlsEnabled": true, "TrustedSignerThumbprints": [] }
}
```

`Mode: Agent` is the one that must not be missed. Left at `InProcess`, UATWEB01 will hunt
for a card reader it does not have and report it as a card that will not read.

### 4. IIS

```powershell
.\install-iis.ps1 -Path 'C:\inetpub\vms' -HostHeader vms.dubaiinvestments.local `
                  -CertificateThumbprint <thumbprint in LocalMachine\My>
```

**HTTPS is required, not preferred.** A page served over plain HTTP cannot open the
`wss://` socket to the agent — and a page served over HTTPS cannot open a plain `ws://`
one, because the browser blocks it as mixed content and gives no visible reason. HTTPS on
the site plus `TlsEnabled: true` is the combination that works.

The script also, by default, turns on **Windows Authentication and turns Anonymous off**.
Read the next paragraph before overriding that.

> The application has **no authentication of its own**. On a reception PC bound to
> loopback that was contained by the network. On UATWEB01 it is not: every desk, and
> everything else that can route to the server, can reach it. IIS Windows Authentication
> is the cheapest real control available and needs no change to the app — it makes the
> server demand a domain identity before a request reaches a page. Without it, every
> visitor record, Emirates ID numbers and photographs included, is readable by anyone on
> the network.

It also disables app-pool idle timeout and nightly recycling. Blazor Server keeps a
circuit per open screen in memory; a recycle drops the desk's screen mid-check-in.

### 5. The database

SQL Server is on this same machine, so the app pool identity is a principal it can see.
Set `@Login` at the top of `db\006_grant_app_login.sql` to `IIS APPPOOL\DIVms` and run it:

```
sqlcmd -S localhost -E -i db\006_grant_app_login.sql
```

Then the rest of `db/`, in number order — see [`db/README.md`](../db/README.md). Still
outstanding: `004_seed_people.sql` needs a re-run (the first hit the `GO` bug and set no
entity links), and `005_add_group_companies.sql` is a decision rather than a step.

### 6. Pin the signer

Do this once a desk has read one real card, and do not skip it — it is control 2 above.

1. Read a card at a desk that has the agent installed.
2. On UATWEB01, find the warning in the log. It names the certificate and prints the line
   to add:
   `Toolkit:Agent:TrustedSignerThumbprints to [ "‹thumbprint›" ]`
3. Put that in `appsettings.Production.json` and restart the app pool.
4. Read a card again. The on-screen warning should be gone.

From then on a response signed by anything else is refused rather than warned about.

---

## B: the agent, on every reception PC

Elevated, on each desk:

```powershell
.\install-desk-agent.ps1 -MsiPath \\uatweb01\deploy\ICAToolkitService.msi
```

It installs ICP's MSI, optionally trusts an agent certificate you supply
(`-AgentCertificatePath`), checks that `toolkitagent.emiratesid.ae` resolves to loopback on
that PC, starts the service, and reports whether anything is listening on 9004/9005/9020.

Two things it cannot do for you:

- **The reader driver**, if Windows has not found the reader by itself. An ACS ACR39U works
  with the driver Windows supplies.
- **The certificate**, if ICP does not ship one. `wss://` to a certificate the desk does
  not trust fails *silently* — indistinguishable, from the page's side, from an agent that
  was never installed. That is why `install-desk-agent.ps1` checks name resolution and why
  the app's own `/agent-required` page lists it as a cause.

Then open the site on that PC. The reader panel should name the reader and show the licence
date. If it does not, `/agent-required` on the site itself is the checklist.

### What the desk sees when the agent is missing

Not three failed reads. The page probes the agent before offering to read, so a desk with
no agent gets **"This PC cannot read the card"**, the reason, and **Enter manually**
straight away. A visitor should not wait on a rollout.

---

## The fallback, and the alternative

**`Toolkit:Mode` has three values**, and they are how this deployment degrades:

| Mode | Reader | Use |
|---|---|---|
| `Agent` | On the desk, via ICP's agent | UATWEB01. What this document is about. |
| `InProcess` | On this machine | The app installed on a reception PC; `deploy\install-service.ps1` and `appsettings.Production.reception-pc.json.template` are for that. |
| `Off` | None | A server before the agent rollout. Card reading disappears from the screen entirely and every entry is typed — honestly labelled as such, rather than a Read Card button that cannot work. |

`Off` is worth knowing about: it makes UATWEB01 useful on day one, before a single MSI has
been installed, instead of blocking the deployment on the desks. Switch to `Agent` when the
first desk is ready — a restart, not a rebuild.

---

## What is not ready, and is a decision rather than a step

1. **The app has no authentication.** IIS Windows Authentication (step 4) is a real
   control, but it is authentication *of the domain*, not authorisation: any domain user
   who finds the URL sees every visitor record. Roles belong in the app.
2. **Emirates ID numbers, photographs and dates of birth are stored in plain text.** No
   column encryption, no Always Encrypted, no TDE unless UATWEB01 already has it.
3. **No audit trail.** Nothing records who read the report, or when.
4. **No retention limit.** The screen tells the visitor their data is kept "only for this
   visit". Nothing deletes it. Either add the job or change the wording.
5. **`DbBootstrapper` is not a migration tool.** It creates absent tables and never alters
   a present one, so a new property needs an `ALTER TABLE` script in `db/`. Move to EF
   migrations before the database holds anything you cannot drop.
6. **Pre-reset `vms` tables are still in the database** from the earlier design.

## Not yet verified against a real desk

The agent path is written from ICP's SDK — `eidatoolkit.js`'s own request formats,
callbacks and defaults, and its `NonModifiablePublicData` / `HomeAddress` /
`CardPublicData` accessors, which is where the XML element names in
`CardResponseParser.cs` come from. None of it has been run against a real agent, a real
reader or a real card, because that needs Windows, the MSI and a card, and none of the
three is available here.

What to expect to adjust on the first real read, in order of likelihood:

1. **`Toolkit:Agent:ToolkitConfig`.** Left blank, on the assumption that the MSI's agent
   carries its own configuration and that the paths in it are the desk's. If the agent
   reports missing or incomplete configuration, this is where the newline-separated
   `key = value` block goes — the format, and why it is not JSON, is in
   [`src/DI.Vms.Blazor/README.md`](../src/DI.Vms.Blazor/README.md).
2. **Element names in the response.** If a field comes through blank while the photograph
   arrives, the name in `CardResponseParser.Parse` is wrong for the gateway's actual
   document. `Toolkit:Agent:DebugEnabled: true` puts the SDK's own log in the desk's
   browser console, where the raw response is visible.
3. **The certificate and the host name**, per the section above.

Turn `DebugEnabled` back off afterwards: it logs card contents to the console of a machine
a visitor stands in front of.
