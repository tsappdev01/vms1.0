# System Architecture

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — System Architecture Document (Controlled, required)

| | |
|---|---|
| Application | Visitor Management System (VMS), DIP office reception |
| Classification | Controlled — handles employee and visitor personal data |
| Repository | `tsappdev01/vms1.0` |
| Document owner | IT (to be named on `11-it-asset-registration.md`) |
| Generated from | the codebase at commit recorded in `20-change-log.md` |

`src/DI.Vms.Blazor/README.md` is the engineering record and remains the place to read
*why* each non-obvious decision was made. This document is the shape of the system.

---

## 1. What it does

A visitor arrives at the DIP reception desk. Reception reads the visitor's Emirates ID —
from the card's chip where a reader is present, from the card's camera image where it is
not, or by typing it — records who they are visiting and why, and the visit is written to
SQL Server. When the visitor leaves, reception signs them out. A supervisor can read a
report of every visit by entity and date.

## 2. Stack

| Layer | Technology | Why |
|---|---|---|
| UI | Blazor Server (.NET 8), `@rendermode InteractiveServer` | The card reader is attached to the desk, and server-rendered Blazor keeps the reading logic on the server while the UI stays live. |
| API | ASP.NET Core Minimal API, same process | The Android tablets need the same operations the screens perform. One implementation, not two. |
| Data | EF Core 8 → SQL Server | |
| Identity | Microsoft Entra ID, app roles, OpenID Connect | §3 of the policy: Entra ID SSO is mandatory for Controlled. |
| Card reading (desk) | ICP ID Card Toolkit v3.1.6, Windows, PC/SC reader | |
| Card reading (tablet) | ICP ID Card Toolkit v3.1.6 Android AAR + ACS plugin | |
| Card reading (fallback) | ML Kit Text Recognition on-device (tablet), Tesseract WASM (browser) | No card data leaves the device during a camera read. |
| Tablet app | Android, Kotlin, Jetpack Compose, Material 3 | |

This matches the policy's approved stack (§3: C# / .NET, Blazor, SQL Server, Entra ID)
with one addition — the Android client — which exists because the ICP toolkit's mobile
reading path has no .NET binding.

## 3. Components

```
                       ┌───────────────────────────────────────────┐
                       │          Microsoft Entra ID               │
                       │  app roles: Officer, Supervisor, Admin,   │
                       │  SystemAdmin, UnmaskedId                  │
                       └───────────────┬───────────────────────────┘
                                       │ OpenID Connect (browser only)
                                       │
  ┌─────────────────┐          ┌───────▼───────────────────────────┐
  │  Reception PC   │  HTTPS   │   DI.Vms.Blazor  (one process)    │
  │  desk browser   ├─────────►│                                   │
  │                 │          │  Components/Pages/  — 5 screens   │
  │  ICP desk agent │  wss://  │  Api/VisitsApi.cs   — /api/*      │
  │  + PC/SC reader │◄─────────┤  Services/          — rules        │
  └─────────────────┘          │  Data/              — EF model     │
                               │  Data/DbBootstrapper.cs            │
  ┌─────────────────┐  HTTPS   │                                   │
  │ Android tablet  ├─────────►│                                   │
  │ ICP toolkit AAR │  X-Api-  │                                   │
  │ ACS reader      │  Key     │                                   │
  │ CameraX+ML Kit  │          └───────┬───────────────────────────┘
  └────────┬────────┘                  │ EF Core 8
           │                           │
           │ ICP Validation            ▼
           │ Gateway (when   ┌──────────────────────┐
           │ online)         │  SQL Server          │
           ▼                 │  database VMS        │
  ┌──────────────────┐       │  schema vms          │
  │ ICP (Federal     │       │  4 tables            │
  │ Authority for    │       └──────────────────────┘
  │ Identity)        │
  └──────────────────┘       ┌──────────────────────┐
                             │ Azure Blob Storage   │
                             │ (card images, where  │
                             │  configured)         │
                             └──────────────────────┘
```

### Projects

| Project | Target | Role |
|---|---|---|
| `src/DI.Vms.Blazor` | `net8.0-windows`, or `net8.0` with `-p:VmsAgentOnly=true` | The whole application: screens and `/api`. The Windows target links ICP's `IDCardToolkit.dll`; the agent-only target leaves it out and reads cards through the desk agent or the tablet instead. |
| `src/DI.Vms.Api` | `net8.0` | The four tablet endpoints alone, for a Linux API-only host. Not deployed today; kept building in CI so it does not rot. |
| `android/` | Android, minSdk per ICP's requirements | The tablet reception app. |

### Where card reading happens

Three paths, and the distinction matters because only one of them produces a response the
server can verify:

1. **Desk, in-process** — reader on the server, ICP toolkit in the server process. Used
   when the application runs on the reception PC itself.
2. **Desk, agent mode** — reader on the reception PC, ICP's own agent service reads the
   chip and the browser relays signed XML to the server over a WebSocket. The server
   verifies the signature. This is what allows the application to be hosted centrally.
3. **Tablet** — the Android app reads the chip through ICP's AAR and posts the fields to
   `/api/visits`. **The tablet's read is not signed**: the offline bundle returns unsigned
   responses, which is tracked in `docs/icp-signed-response-request.md` and recorded as a
   risk in `10-risk-assessment.md`.

The camera paths (ML Kit on the tablet, Tesseract in the browser) are read aids, not
identity proof. What they produce is a *suggestion the officer confirms*, and the visit is
recorded with `CaptureMethod` saying so.

## 4. Schema ownership

There is one definition of the table schema: the EF model in `src/DI.Vms.Blazor/Data/`.
`Data/DbBootstrapper.cs` creates absent tables, adds absent **nullable** columns, and
creates absent **indexes** at startup, logging each thing it makes.

It will not make any change that could lose data. A required column, a widened type, a
rename or a drop needs a script in `db/`, and startup refuses to run — naming the missing
script — rather than failing later with `Invalid column name` on a reception desk.

Index creation has one guard: above 500,000 rows the table is left alone and a warning
names the script, because `CREATE INDEX` takes a schema lock and when to take one becomes
a decision rather than a detail.

Reference data is not schema. `vms.Entity` is maintained by script in `db/` so the group's
companies can change without a rebuild and a redeploy.

## 5. Request paths

| Actor | Enters by | Authenticated by | Authorised by |
|---|---|---|---|
| Desk browser | Blazor circuit over HTTPS | Entra ID cookie (OpenID Connect) | `CanCheckIn` / `CanViewReport` policy |
| Android tablet | `/api/*` over HTTPS | `X-Api-Key` header, one key per tablet | the same `CanCheckIn` policy |
| Another service | `/api/*` over HTTPS | JWT bearer (Entra) | the same `CanCheckIn` policy |

Both tablet key and bearer token are named on the API group and the **same policy decides
afterwards**, so the key is a way of arriving rather than a way around the rules.

`options.FallbackPolicy = options.DefaultPolicy` — nothing is anonymous. A page added later
is protected by default rather than by whoever remembers the attribute, which is the right
way round for a visitor log.

## 6. Cross-cutting

| Concern | Where | Note |
|---|---|---|
| Field rules | `Services/VisitorFields.cs` | One definition for the desk, the tablet and the API. A *problem* blocks (Emirates ID shape and Luhn check digit); a *concern* warns and never refuses a visitor. |
| Security headers | `Services/SecurityHeaders.cs` | CSP, HSTS, X-Frame-Options DENY, nosniff, Referrer-Policy, Permissions-Policy `camera=(self)`. CSP is composed from configuration so the OCR CDN and the ICP agent WebSocket are not blocked. |
| Rate limiting | `Program.cs` | 300 requests/minute per caller IP on `/api`, on the internet-facing host only. Not registered on UATWEB01, which is reachable only from the office network. |
| Cookies | `Program.cs` | HttpOnly, `Secure` always, `SameSite=Strict`. |
| Time | `Services/VisitorFields.Today` | UTC+4, fixed offset. Stored UTC, displayed Gulf Standard Time. A card expiring today reads as expired for the first four hours of the working day if this is UTC. |

## 7. Build and CI

| Workflow | Runs |
|---|---|
| `.github/workflows/dotnet.yml` | restore + build both .NET projects, `dotnet list package --vulnerable`, `--deprecated`, and a tracked-file secret scan |
| `.github/workflows/android.yml` | builds the APK |

The policy (§7) requires a NuGet audit, a SAST scan and build verification on every merge.
Build verification and the NuGet audit are in place. **Semgrep SAST is not yet wired** —
see `10-risk-assessment.md`.

## 8. Known architectural gaps

Stated here rather than discovered later. Each is carried as a row in
`10-risk-assessment.md`.

1. **Tablet card reads are unsigned.** The offline bundle returns unsigned responses, so a
   tablet read cannot be proven genuine. Open with ICP.
2. **One tablet returns toolkit error 233** (`ETSTATUS_SERVER_RESPONSE_ERROR`) while
   another on the same network reads normally. Root cause established — the toolkit
   reaches ICP's Validation Gateway during `readPublicData` and that tablet cannot — but
   *why that tablet and not the other* is unresolved.
3. **Emirates ID numbers are stored and displayed in full.** `Vms.UnmaskedId` exists as a
   role and nothing enforces it yet.
4. **No audit trail of reads.** Visits cannot be edited or deleted through the application,
   so the row is its own record; but nobody records who *read* the report or exported it.
5. **No Semgrep SAST step** in CI.
6. **Four credentials pasted into chat during development have not yet been rotated**, and
   one SQL password is in this repository's git history permanently.
