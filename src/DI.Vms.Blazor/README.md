# DI.Vms.Blazor

Visitor Management System — Blazor Server, .NET 8.

## Two screens

| Route | Screen |
|---|---|
| `/` | **New Visitor** — insert card → read card → visitor information → entity and person → save |
| `/report` | **Visitor Details by Entity** — all information plus the date-time stamp |

## Why one project, and why no bridge

The toolkit is called **in-process**. That works because the application runs *on the
reception machine*, so its server-side code is on the same machine as the reader. No
separate bridge process, no browser interop, no CORS.

The consequence is that this application must run on the reception PC. It is not a
server-hosted portal that reception connects to from elsewhere — the reader is local by
nature, so the app is too.

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

`https://localhost:7100`. The schema is created on first start via `EnsureCreated`, and
the DI entities are synced on **every** start by `Data/EntitySeeder.cs` — no separate
script. Deliberately not EF's `HasData`, which seeds only at database creation: the list
would then never reach a database that already exists.

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
