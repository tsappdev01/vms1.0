# Solution Architecture

Dubai Investments Visitor Management System, DIP office. Derived from the BRD and
constrained by the findings in [`00-sdk-analysis.md`](00-sdk-analysis.md).

## Components

```
┌──────────────────────────┐        ┌──────────────────────────┐
│  Reception Client        │        │  Web Portal              │
│  Android (Kotlin)        │        │  React + TypeScript SPA  │
│  + EIDAToolkit.aar       │        │  Security / Admin        │
│  + certified reader      │        │  no card reading         │
└───────────┬──────────────┘        └───────────┬──────────────┘
            │  HTTPS / JSON, mTLS + Entra ID    │
            └───────────────┬───────────────────┘
                            ▼
            ┌───────────────────────────────────┐
            │  DI.Vms.Api — ASP.NET Core 8      │
            │  Application / Domain / Infra     │
            └───────────────┬───────────────────┘
                            ▼
            ┌───────────────────────────────────┐
            │  SQL Server  UATWEB01 / VMS       │
            │  TDE + column-level encryption    │
            └───────────────────────────────────┘

  Reception client ──► ICP Validation Gateway (IsCardGenuine, CheckCardStatus)
                       direct from the device; the API never sees the card.
```

## Projects

| Project | Type | Responsibility |
|---|---|---|
| `DI.Vms.Domain` | class library | Entities, enums, value objects, business rules. No dependencies. |
| `DI.Vms.Application` | class library | Use cases, DTOs, validation, port interfaces. |
| `DI.Vms.Infrastructure` | class library | EF Core, SQL Server, encryption, notifications, Entra ID. |
| `DI.Vms.Api` | ASP.NET Core | REST API, authn/authz, audit middleware. |
| `DI.Vms.Web` | React + TS | Security/Admin portal (BRD §11, §13, §20, §24). |
| `DI.Vms.Reception` | Android (Kotlin) | Registration, ID read, signature, check-in/out (BRD §24). |

Dependencies point inward: `Api → Infrastructure → Application → Domain`.

## Reception client — decision pending the hardware spike

Toolkits ship for Android, iOS and Windows, so BRD §23's MAUI recommendation is achievable.
It costs a binding layer per mobile platform (a .NET for Android binding over the AAR, a
.NET for iOS binding over the framework), sitting on the critical path of the riskiest
feature. The Windows build is already an official .NET assembly and needs no binding at
all, and on Windows **any PC/SC reader** works rather than a plugin-matched one.

Settle this with the spike in SDK analysis §3a — read a real card with the shipped Windows
sample — before committing to a client platform. The table above names Android because
that is the current working assumption, not a closed decision.

## Where card reading happens

On the reception client only. The card is physically at the desk, the toolkit is a native
library requiring registered-device credentials, and the VG call is made from the registered
device. The API receives **already-extracted fields** plus the toolkit's verification verdicts.

This keeps the server free of native dependencies, and means a VG outage degrades one desk
rather than the whole system.

## Degraded mode

`read_publicdata_offline = true`, so the chip can be read with no network. When the VG is
unreachable the client still registers the visitor and check-in proceeds, with the visit
flagged `VerificationPending` for later reconciliation. Reception must never be blocked by
a network fault — a queue at the door is itself a security problem.

## Database

Target: **`UATWEB01`, database `VMS`** (UAT). Schema `vms`.

- Schema-first: the DDL in `db/schema` is the source of truth; EF Core maps to it.
  A security system's schema should be reviewable by a DBA without reading C#.
- ID numbers: encrypted at rest, looked up by keyed HMAC (`IdNumberHash`) so repeat-visitor
  lookup (BRD §14) never decrypts the table. See [`06-security-privacy-rbac.md`](06-security-privacy-rbac.md).
- `AuditLog` is append-only: grant `INSERT`/`SELECT` only.
- TDE at the database level, plus column encryption for the ID number specifically.

## Stack

| Layer | Choice | Note |
|---|---|---|
| API | ASP.NET Core 8 | BRD §23 |
| ORM | EF Core 8 | schema-first, no `EnsureCreated` |
| DB | SQL Server 2019+ | BRD §23 |
| Auth | Microsoft Entra ID | BRD §23; device certs for tablets |
| Portal | React 18 + TypeScript | assumption — see below |
| Reception | Android, Kotlin, minSdk 26 | SDK-driven |
| Reporting | Power BI over read-only views | BRD §20, §23 |
| Real-time | SignalR | live dashboard and evacuation list |

**Assumption flagged:** BRD §23 allows "ASP.NET Core / React or Angular" for the portal.
React + TypeScript is assumed. If DI standardises on Angular or on server-rendered Razor,
this is a contained change — the API contract is unaffected.
