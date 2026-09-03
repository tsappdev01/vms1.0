# DI.Vms.Blazor

Visitor Management System — Blazor Server, .NET 8.

## Two screens

| Route | Screen |
|---|---|
| `/` | **New Visitor** — insert card → read card → visitor information → entity and person → save |
| `/report` | **Visitor Details by Entity** — all information plus the date-time stamp |

## Where the reader is: `Toolkit:Mode`

The Emirates ID is read from the **chip**, by a reader plugged into a physical machine. In
Blazor Server all component code runs server-side, so the machine running the app and the
machine holding the reader have to be reconciled somehow. One setting says how:

| Mode | Reader | Read path |
|---|---|---|
| `InProcess` | On this machine | `Services/CardReaderService.cs` calls the toolkit directly. The app runs at the desk. |
| `Agent` | On the desk; this process is on a server | The browser talks to ICP's agent over a WebSocket and posts the signed response back. `wwwroot/js/card-agent.js` + `Services/AgentCardReader.cs`. |
| `Off` | None | Details are typed in, and the entry says so. |

`InProcess` is what the system was built for and needs no bridge process, no browser
interop and no CORS. **`Agent` is what UATWEB01 uses** — see
[`docs/deployment.md`](../../docs/deployment.md).

### Agent mode inverts who is trusted, and that is the whole of the work

In-process the toolkit's output is trustworthy because it never left the process. Through
an agent it arrives from a browser, which is a program someone can replace. So the server
issues the request ID (random, single-use, five-minute life, stamped into the response by
the gateway, so an old response matches nothing outstanding), verifies the XML signature
itself, and parses **every** field out of that signed document.

The trap is the signature. The certificate that signed the response travels *inside* it,
so a bare `CheckSignature()` proves only that the document has not changed since whoever
signed it did — anyone can produce one that passes. `Toolkit:Agent:TrustedSignerThumbprints`
is therefore not hardening, it is the control: without it there is nothing to tell a real
card from an invention. Unpinned, reads are accepted and flagged, and the thumbprint is
logged with the line to paste in, because a deployment with no working read path is worse
than one with a stated risk.

`card-agent.js` returns the signed XML and nothing else, for the same reason: a field taken
from browser-side JSON would make the signature decorative. It also probes the agent before
constructing ICP's `Toolkit`, because `eidatoolkit.js` left to itself puts up a `confirm()`
and navigates to a JNLP download when no agent answers — an attendant mid-check-in should
get an explanation, which is what `/agent-required` is.

The two paths meet at `Services/CardResponseParser.cs`, which holds the element names (taken
from the vendor SDK's own accessors, not from a sample response) and the signature check.

## Setup

**1. Point it at your toolkit config.** `appsettings.Development.json`:

```json
"Toolkit": {
  "ConfigDirectory": "C:\\Claude.AI\\vms1.0\\id-card-toolkit-windows-sdk-v3.1.6\\quickstart\\64"
}
```

It must be the folder holding `config_li`. The default above is where the `config_ap` that
produced a successful read pointed; the ICP bundle folder
(`IDCARDOFFLINE_config_2026-04-14\IDCARDOFFLINE_ag_config_2026-04-14`) is the alternative.

Leave it blank and the app searches for a folder containing `config_li`, preferring one
that also has `config_ag` — that marks ICP's complete bundle, and the earlier partial
delivery carried the licence the Validation Gateway rejected.

### The config format is `key = value`, not JSON

```
config_directory = C:\...\quickstart\64
log_directory    = C:\ProgramData\EIDAToolkit\logs
application_type = APP_INPROC
read_publicdata_offline = true
```

The SDK's quickstart README documents a **JSON** example. The toolkit rejects it with
`Invalid or incomplete configuration data`. Newline-separated `key = value` is what the
working `config_ap` uses and what the Android sample builds, so that is what this passes.

**2. Check the connection string.** `Server=UATWEB01;Database=VMS`.

**3. Run.** The build copies the native toolkit DLLs (`EIDAToolkit.dll`, `PCSCLib.dll`,
the Morpho runtime and the VC++ 2013 redistributable) from the SDK's `quickstart\64` into
the output folder, because Windows resolves a P/Invoke target from the executable's own
directory. Without them the app starts and then fails on first use with:

```
Unable to load DLL 'EIDAToolkit.dll' or one of its dependencies (0x8007007E)
```

That message names the assembly it could not load, not the dependency that was actually
missing — which is why the service now reports the real cause instead. If a deployment
keeps those DLLs elsewhere, set `Toolkit:NativeDirectory`.

```
dotnet run --project src/DI.Vms.Blazor
```

`https://localhost:7100`. `Data/DbBootstrapper.cs` creates `vms.Entity` and
`vms.VisitorEntry` on startup if they are absent, so a fresh database needs no separate
step.

Deliberately not `EnsureCreated`, which does not do that job: it creates the schema only
when it creates the *database*, and does nothing at all against a database that already
exists — missing tables included. `VMS` on UATWEB01 exists and holds ten tables from an
earlier design, so it created nothing and the first query failed with
`Invalid object name 'vms.Entity'`. The bootstrapper checks `sys.tables` against the model
and generates the DDL from the model, so there is no second copy of the schema to drift.
It is a bootstrap, not a migration tool: it creates what is absent and never alters what
is present.

### The host list is data too

**Person to visit** is a type-ahead over `vms.Person`, seeded from the AD export by
`db/004_seed_people.sql` — 725 people. Suggestions are narrowed to the selected entity,
with *Search all entities* to widen them, because 436 of those people belong to a company
that has no entity in the list; see `db/README.md`. A name that is not in the list can
still be typed, and the entry records which it was: an unlisted contractor must not stop a
check-in at the desk.

The visit stores the host twice over — `PersonToVisitId` when one was picked, and a copy of
the name, title, email and company either way. The key makes the link traceable; the copy
makes the record stable, because a host who changes title or leaves must not rewrite what
an earlier report said.

### The entity list is data, not code

The companies in the **Entity being visited** dropdown live only in `vms.Entity`. Nothing
in the application seeds them — no `HasData`, no startup sync — so the list is changed with
a SQL insert and a page refresh, not a rebuild and a redeploy. The script is
[`db/001_seed_entities.sql`](../../db/001_seed_entities.sql), and it is re-runnable. `IsActive = 0` takes an
entity out of the dropdown while its visitor history stays intact; the report's entity
filter keeps such an entity while it still has visits behind it.

`EntitySeeder.Names` is the single source of truth for the dropdown. The sync inserts what
is missing, reactivates anything listed again, and **retires** — `IsActive = false`, never
deletes — anything no longer listed, so visitor entries keep pointing at a real row and an
old report can still name the entity that was visited. The dropdown on New Visitor reads
active rows only; the report's entity filter also keeps retired entities that still have
visits behind them.

> `EnsureCreated` is right for a single-developer fresh start. Move to EF migrations before
> more than one person shares the database, or before it holds anything you cannot drop.

## Fields shown

Exactly what the chip returns, grouped as the vendor sample groups it: **Identity** (ID
number, card number, photograph, signature), **Non-Modifiable Data** (ID type, issue and
expiry dates, names in English and Arabic, gender, date of birth, nationality, title,
place of birth), and **Home Address**.

These were learned from a real card and are handled rather than assumed:

**Names arrive comma-delimited with empty positions.** `NAYYAR JAWAID,,,,,ALI KHAN,` is
one person, not seven fields. The non-empty segments are joined; the raw value is kept
because the positions carry given/middle/family meaning.

**Arabic arrives double-decoded on .NET 8.** The native toolkit keeps every attribute as
UTF-8 bytes in a `char[]`; the managed binding reads it with `Marshal.PtrToStringAnsi`
(ANSI code page) and repairs that by re-encoding through `Encoding.GetEncoding(0)` and
decoding as UTF-8. On .NET Framework — the binding's own target, and the vendor samples' —
code page 0 is the OS ANSI code page and the repair works. On .NET 8 `GetEncoding(0)` is
UTF-8, so the round trip is the identity and the mangling survives: `نير جواد` shows as
`Ù†ÙŠØ± Ø¬ÙˆØ§Ø¯`. `Services/CardText.cs` undoes it, inverting the ANSI *decode* rather
than trusting the encoder — Windows-1252 leaves 0x81 undefined, and 0x81 is the second
byte of `ف`. It only rewrites text that really was UTF-8 read as ANSI, so correct text and
a future binding that fixes this itself both pass through untouched. The signed XML is
repaired the same way before signature validation, since a digest over mangled text cannot
match the one the card signed.

**The signature is TIFF**, which no browser renders — WPF does, which is why the vendor
sample shows it. It is converted to PNG server-side (`Services/ImageConverter.cs`). If
conversion fails the screen says the signature is not available rather than showing a
broken image.

**The postal address was empty on the card tested** — every field blank except mobile and
email. The screen says "No postal address held on this card" rather than rendering empty
boxes that look like a failed read. Mobile and email are highlighted, since they are the
fields that did carry data.

## Not built

Check-out, an occupancy view, the third module, authentication, and ID-number masking.
The previous build had all of those; they are in git history at `c98ce08` if wanted.
