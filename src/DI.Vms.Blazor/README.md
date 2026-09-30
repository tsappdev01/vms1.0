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

## Light and dark

The palette is a set of custom properties defined three times: on `:root` for light, under
`prefers-color-scheme: dark` for a device that asks for dark, and again under
`:root[data-theme="dark"]` so an explicit choice wins in both directions. The
`prefers-color-scheme` block is guarded with `:not([data-theme="light"])`, so choosing
light on a device set to dark actually gives light.

The switch in the top bar is **plain HTML with no `@onclick`**, handled by a delegated
listener in `app.js`. It is a browser preference, not application state: it has to work
before the circuit connects and keep working if one drops, and Blazor re-rendering the
layout must not detach the handler.

Which segment looks pressed is decided by **CSS, from the attribute on `<html>`** - so it
is read from what is actually applied rather than from a second copy of the state in
script, and it cannot drift. Script only sets `aria-pressed`, for the screen reader.

The saved choice is applied by an inline script in `App.razor`'s `<head>`, before the
first paint. Loading it from `app.js` instead would flash a white page on every
navigation for a desk set to dark.

The choice is held in memory, with storage as the durable copy rather than the only copy.
Reading it back from storage on every check looked tidier and was wrong: where storage is
unavailable the write silently does nothing, the read returns null, and the switch clears
the attribute it has just set - so the control appears to do nothing at all and the device
preference wins. In memory it still works for the session, which is the honest
degradation.

**The choice sticks until it is changed** - across both screens, a reload, a circuit
dropping, and the back/forward cache. Storage is the record and the attribute is only how
it is applied, so anything that removes the attribute is repaired rather than obeyed: a
`MutationObserver` filtered to `data-theme` puts it back whenever it stops matching what
was chosen. That guards against Blazor's enhanced navigation, which replaces the document
and can take an attribute set from outside its render tree with it - a failure that would
otherwise show up as the theme reverting on one particular navigation and nowhere else.

Dark is not a neutral grey with a blue tint on top - the ground **is** the logo's navy,
`#0a3255`, and the surfaces are steps up from it. Text on it measures 11.8:1 for primary
and 6.8:1 for secondary. The status tints keep their own hue but are pulled towards the
navy, so a green "ready" panel belongs on the ground rather than sitting on it as a patch
of unrelated colour.

The same value is the light theme's `--di-navy`. It used to be `#123a5c`, an
approximation - which showed up as the lockup sitting on nearly-but-not-quite its own
colour in the sidebar.

One thing that is a token rather than a rule: the reader photograph is a product shot on
white, so light blends it in with `mix-blend-mode: multiply`. On dark that would blend the
reader into the dark with it, so `--photo-blend` and `--photo-plate` turn the blend off
and give it a light plate. Both palettes carry them, so the fix lives with the colours
rather than in a selector that has to be remembered.

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
sample shows it. Converting it server-side worked, and then the signature was dropped from
the screen entirely: a visitor log has no use for it, and `readPublicData` is asked not to
return it. `Services/ImageConverter.cs` and the `System.Drawing.Common` reference went with
it. If it is ever wanted back, the conversion is in git history.

**The postal address was empty on the card tested** — every field blank except mobile and
email. The screen says "No postal address held on this card" rather than rendering empty
boxes that look like a failed read. Mobile and email are highlighted, since they are the
fields that did carry data.

## The card is kept as an image

`Services/CardImageRenderer.cs` draws the card that was read as an SVG, and it is stored
in `vms.VisitorCardImage`, one row per visit, reachable from the report by clicking the
capture label.

Three decisions in it are worth knowing before changing any of them.

**The server draws it, not the browser.** The obvious approach is a screenshot library
against the card mock-up on the New Visitor screen. That would give the web desk an
artefact and the tablet none — the Android app's screen is Compose, not HTML — and one
visitor log would hold two kinds of record. Drawing it server-side means both clients
produce exactly the same thing without either of them cooperating, and it is drawn from the
data parsed out of the signed response rather than from something a client sent.

**SVG, not PNG.** No image library, so no new dependency and nothing needing a native codec
on a Linux Web App; self-contained, because the photograph is embedded; and sharp when
printed. The content type is stored beside the bytes, so moving to PNG later is a change of
value, not of schema.

**Its own table.** The visitor report loads whole `VisitorEntry` rows to render a table
that shows neither the photograph nor the card. A 40 KB image on that entity would be 40 KB
per row across the wire every time somebody opens a month — over the internet, now that the
app and the database are both in Azure. As a navigation it is loaded only when something
asks, and nothing but the download endpoint ever does.

It is a record, not a reproduction, and the image says so on its face: a banner reading
*record of an Emirates ID chip read, not an identity document*, with the timestamp and
whether the signature verified. An artefact that resembles an identity document and is not
one should say which it is — to the auditor who finds it, and to anyone it is ever shown
to. The endpoint serves it with a content security policy that forbids script, because an
SVG is a document a browser will execute script in and "nothing generates script today" is
not a control.

## Sign-in is built, and switched off

`Services/SignInOptions.cs` is the whole of it. `Authentication:Enabled` decides between
Entra ID over OpenID Connect — plus bearer tokens beside the cookie for the tablet — and
an open desk, and it currently says `false`. `docs/entra-id-setup.md` has the reasoning
and the two steps to turn it on.

The part worth knowing: switching it off does **not** relax the authorisation rules. It
registers one authentication scheme that always succeeds and carries every role, so every
`[Authorize]` attribute, every policy and every `AuthorizeView` stays exactly as written
and goes on being evaluated — they simply all pass. Rules that are bypassed rather than
satisfied are the ones nobody finds the bugs in until the day they are switched on.

Two consequences are deliberate and visible rather than quiet: `RecordedBy` reads
`(not signed in)` on every entry made in this period, so the report shows later which
records have an author behind them; and the layout carries a **Sign-in is off** badge on
every page.

## Not built

Check-out, an occupancy view, the third module, and ID-number masking. The previous build
had all of those; they are in git history at `c98ce08` if wanted.

## Reading a card from a photograph

A visitor increasingly carries the Emirates ID on a phone. Neither the reader nor manual
entry serves that, so there is a third path: hold the card up to the camera and let the form
fill itself.

**The QR code on the wallet card is useless for this.** Decoded, it holds only

```
https://beta.smartservices.icp.gov.ae/...?applicationRef=<token>&serviceType=APPLE_WALLET&login=true
```

No name, no ID number, and nothing at all without an ICP account. It is a verification link
for ICP's own portal. Anyone who tries this will try the QR first; it does not work.

**The machine-readable zone on the back does.** Three lines of thirty characters, ICAO 9303
TD1, and the reason to prefer it over OCR of the front is that it carries check digits. A
camera pointed at a phone screen misreads characters - glare, and the moiré of one pixel
grid photographed through another - and arithmetic over the result turns a wrong ID number
into a refusal instead of into a visitor record.

Recognition is Tesseract compiled to WebAssembly, in the browser. Free per read and nothing
to install on a host that runs App Service's built-in .NET image, where a native Tesseract
cannot go. `Services/MachineReadableZone.cs` does the parsing and the checking; nothing
about whether a read is acceptable lives in the script.

### What the first real read at the desk taught

The first photograph of a real card produced this:

```
truth  ILARE1400382963784198059198691
read   1LARE1400382963784198059198691      I read as 1
truth  8012137M2610106IND<<<<<<<<<<<9
read   8012137M2610106IND<<<<<<<<<<9       a chevron lost
truth  SAKTHIVEL<<SENTHIL<KUMAR<PONNU
read   SAKTHIVEL<SAKTHIVEL<<SENTHIL<K      line three read twice
```

Three different faults, and each needed its own answer.

- **Positional repair.** OCR-B confuses `I`/`1`, `O`/`0`, `S`/`5`, `B`/`8`. The standard says
  which positions can hold a digit and which a letter, so a character in the wrong class is
  corrected rather than refused - and the check digits still decide whether the correction
  was right. This is what turns `1LARE` back into `ILARE`.
- **Filler padding.** A chevron is the easiest character in the zone to lose: a run of eleven
  photographs as a dashed line. A line short only in its filler run is padded, and the
  composite check digit proves whether that was right.
- **Cropping to the band.** The zone is about a tenth of the card's height, so in a
  photograph of the whole card each character is a few pixels - which is where a recogniser
  drops a chevron or reads a line twice. The bottom 45% is cropped and doubled, and tried
  first, because it is the attempt that usually holds.

With those, that exact misread now reads correctly. The name still comes out wrong, and
always will when line three is garbled: **line three has no check digit**, so there is
nothing to correct it against. The officer fixes the name on the form; the ID number, card
number, date of birth, expiry and nationality are all protected.

Mirroring is defeated rather than detected. A laptop preview is conventionally mirrored and
whether the captured frame is too depends on browser, driver and application. A still
photograph is recognised in all four orientations and the check digits pick the real one, so
nothing has to know. (The live loop below does less than this on purpose, and why is worth
reading.)

### The shutter was the problem

The first version had the officer frame the card, press **Capture**, and wait. That is a
file-upload dressed as a scanner, and it failed for a reason no amount of tuning would have
fixed: the press *is* the misread. A hand moving to a button moves the card, and the one
frame that gets kept is the blurred one.

So there is no shutter. The camera is read continuously and the form fills when a read
holds. What that changed:

- **A frame is cheap, so it need not be good.** Most of what the still path does to squeeze a
  result out of one photograph - three thresholds tried in turn, four orientations - exists
  because there is only one picture. With a stream there is always another frame in 300ms, so
  the live loop does one threshold and one orientation and simply tries again. That is what
  brought a pass from seconds to a few hundred milliseconds.
- **The band is not one of those tricks, and dropping it was a bug.** The first live build read
  the whole card, and the desk reported the symptom precisely: *the ID number captured, name,
  date of birth, nationality and expiry not.* That is exactly what reading the whole card
  produces. The zone is under a third of the card's height, so most of the pixels and most of
  the time go on the photograph, the emblem, the Arabic and the notice about returning the card
  to a police station - and the recogniser is asked to find a uniform block of text on a page
  that is nothing of the sort. The number survives because it is also printed large and sits in
  line one; nothing else does.

  So a live pass reads the **bottom 45% of the card** at 1100 pixels across, which puts an MRZ
  character about 40 pixels tall - what the recogniser wants - on a canvas of about a third of
  a megapixel. That is *fewer* pixels than the whole card at 900 was, so the fix is faster than
  the fault. One pass in three is still the whole card at 760, because the number printed on
  the **front** is only found that way.
- **Only the guide box is read.** A few hundred pixels rather than a megapixel, which is
  where nearly all of the remaining time went - and it makes the failure legible, because
  "it is not reading" becomes "move the card into the box".

  The box is ID-1 shaped and computed from the frame, not written down as percentages. The
  first attempt did write it down, and at 16:9 that rectangle came out half again wider than
  a card: an officer who filled it width-wise pushed the bottom of the card - which is where
  the zone is - outside the region being read. `guideFor` in `mrz-scan.js` works it out and
  positions the on-screen overlay from the same numbers, so the box aimed at and the pixels
  recognised cannot drift apart. This is also why `.scan-view` has `height: auto` and no
  `max-height`: constraining the height letterboxes the picture inside the element, and then
  a percentage of the element is no longer a percentage of the frame.

  The card is read at 900 pixels across, which puts an OCR-B character at about 30 pixels -
  what the recogniser wants. More resolution buys nothing and costs the frame rate.
- **Upside-down is tried late.** Only after a few fruitless frames, and then on alternate
  frames. A card held the wrong way up is the rarer case, and trying both from the start
  halves the rate for everyone.
- **The kept picture is the frame that read**, not a fresh grab. By the time a second picture
  is taken the card has moved.

### Either side, because the front has a check of its own

The officer should not have to know which side matters, so both are read.

The back is preferred and carries everything. The front carries only the fifteen-digit ID
number - but that number ends in a **Luhn check digit**, verified here against eleven real
Emirates ID numbers: all eleven pass, and altering any single digit fails. So a front read is
held to the same standard as a back read. It is not a guess about what the picture looked
like; it is arithmetic, same as the MRZ.

A number-only read is not taken the instant it arrives. It is held first - three seconds when
the front is in frame, seven when the chevrons say the back is and only the photograph is
failing, because then everything the form wants is a few centimetres from the lens and worth
waiting for. A complete read in that window wins. If none comes, the checked number fills the
ID field, a banner says the name and dates still need the back or the keyboard, and the
officer carries on.

One trap this design walks into and out of: **the ID number is inside the MRZ as well as on
the front**, so a back photographed too poorly for the zone to parse still yields the number.
Called a front, that read would have had a "portrait" cut out of the middle of a block of
text. The sides are separated by counting chevrons - the zone is padded with dozens and
nothing printed on the front uses one.

### Two things that only failed on the second visit

Both were invisible on a fresh page load and broke on the way back to the screen, which is the
worst shape a fault can have: the officer's own description was *"the camera opens only after
a refresh"*.

- **The camera was asked for before the `<video>` existed.** `StateHasChanged` queues a render;
  it does not perform one. The code worked by accident, because awaiting the module import took
  long enough for the render to land first. Return to this screen without reloading and the
  module is already in the browser's registry, the import resolves on a microtask, the call
  beats the render, and `getElementById` returns null. It is started from
  `OnAfterRenderAsync` now, which is the only version of this that is not a coincidence.
- **The recogniser was torn down on the way out.** It belongs to the browser page, not to the
  component, and leaving this screen in a Blazor application is not leaving the page - so a
  desk that read a card, looked at the report and came back rebuilt several megabytes of
  WebAssembly every time. It is kept for the life of the page now, and the panel opening starts
  it downloading before there is anything to read.

A third of the same family: a failed load was remembered. `loading ??= ...` held the promise
whatever became of it, so one unanswered CDN request left a rejected promise that every later
call returned, and the scanner stayed broken until the page was reloaded with nothing on screen
saying why.

### The model is configurable, and the default is the wrong one

Recognition runs on Tesseract's `eng` model because that is what the public trained-data host
serves. It is the wrong shape for this job: trained on proportional print in two cases,
carrying an English dictionary, where the zone is one monospaced font with 37 characters and
no words.

A model trained on OCR-B alone - [Shreeshrii/tessdata_ocrb](https://github.com/Shreeshrii/tessdata_ocrb)
is the one everyone uses - is a fraction of the size and better at exactly the characters the
check digits keep rejecting. There is no CDN serving it, so using it means three steps: put
`ocrb_int.traineddata` under `wwwroot/lib/tesseract/`, point `DigitalCard:TrainedDataUrl` at
`/lib/tesseract`, and set `DigitalCard:Language` to `ocrb`. That repository states no licence,
so check that before shipping it.

### It is not the chip, and the record says so

These are recorded as `DigitalCard` - *"Digital card (photographed)"* in the report -
deliberately below `CardReaderUnverified`, because that at least came off a chip. The MRZ is
printed, not signed. The check digits prove the photograph was read correctly and nothing
whatever about whether the card is genuine or belongs to the person holding the phone.

Off unless `DigitalCard:Enabled` is true. A deployment that has not decided whether a
photographed card is acceptable evidence should not find the button there one morning.
