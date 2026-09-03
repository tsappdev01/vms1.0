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
  "ConfigDirectory": "C:\\Claude.AI\\vms1.0\\IDCARDOFFLINE_config_2026-04-14\\IDCARDOFFLINE_ag_config_2026-04-14"
}
```

It must be the folder containing `config_li` and `config_ag`.

**2. Check the connection string.** `Server=UATWEB01;Database=VMS`.

**3. Run.**

```
dotnet run --project src/DI.Vms.Blazor
```

`https://localhost:7100`. The schema is created on first start via `EnsureCreated`, and
the seven DI entities from BRD §6 are seeded with it — no separate script.

> `EnsureCreated` is right for a single-developer fresh start. Move to EF migrations before
> more than one person shares the database, or before it holds anything you cannot drop.

## Fields shown

Exactly what the chip returns, grouped as the vendor sample groups it: **Identity** (ID
number, card number, photograph, signature), **Non-Modifiable Data** (ID type, issue and
expiry dates, names in English and Arabic, gender, date of birth, nationality, title,
place of birth), and **Home Address**.

Two things were learned from a real card and are handled rather than assumed:

**Names arrive comma-delimited with empty positions.** `NAYYAR JAWAID,,,,,ALI KHAN,` is
one person, not seven fields. The non-empty segments are joined; the raw value is kept
because the positions carry given/middle/family meaning.

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
