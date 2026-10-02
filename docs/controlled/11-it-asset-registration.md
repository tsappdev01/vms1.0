# IT Asset Registration

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — IT Asset Registration (Controlled, required)

The entry for the IT Asset Inventory. The technical facts are filled in from the codebase;
the **people and dates are for IT to complete** and are marked `[ ]`.

---

## Application

| Field | Value |
|---|---|
| Asset name | Visitor Management System (VMS) |
| Asset ID | `[ ]` *(assigned by IT Asset Inventory)* |
| Classification | **Controlled** |
| Purpose | Visitor registration and sign-out at the Dubai Investments Park office reception, with Emirates ID captured from the card chip |
| Business function | Site security / facilities |
| Data classification | Confidential — Personal Data (see `08-data-classification.md`) |
| Built with | Claude Code, per DI-IT-POL-AIDEV-001 |
| Status | **Pre-go-live.** Not in production |

## Ownership

| Role | Name | Contact |
|---|---|---|
| Business Owner | `[ ]` | `[ ]` |
| IT Owner | `[ ]` | `[ ]` |
| Digital Champion (domain knowledge, UAT) | `[ ]` | `[ ]` |
| Day-to-day support | `[ ]` | `[ ]` |
| Policy Owner (approves deviations) | `[ ]` | `[ ]` |

## Users

| Group | Count | Entra app role |
|---|---|---|
| Reception officers | `[ ]` | `Vms.Officer` |
| Reception supervisors | `[ ]` | `Vms.Supervisor` |
| IT | `[ ]` | `Vms.Admin`, `Vms.SystemAdmin` |
| Permitted to see an unmasked Emirates ID | `[ ]` | `Vms.UnmaskedId` |

## Technical

| Field | Value |
|---|---|
| Stack | Blazor Server (.NET 8), ASP.NET Core Minimal API, EF Core 8, SQL Server |
| Client | Android (Kotlin, Jetpack Compose) on 3 reception tablets |
| Repository | GitHub `tsappdev01/vms1.0` — **deviation from policy §7 (Azure DevOps), requires Policy Owner approval** |
| Identity | Microsoft Entra ID, app roles, OpenID Connect |
| Source documentation | `docs/controlled/` (this set); engineering record in `src/DI.Vms.Blazor/README.md` |

## Hosting

| | On premises | Azure |
|---|---|---|
| Host | UATWEB01 (IIS, Windows) | Web App **VMS**, RG `DotNetSites`, plan `ASP-DotNetSites-8061`, UAE North |
| URL | `[ ]` | `vms-cebrd3evb0cyg0gn.uaenorth-01.azurewebsites.net` |
| Database | SQL Server on UATWEB01, database **VMS** | `ts-db.database.windows.net`, database **vms** |
| Reachable from | Office network only | Internet |
| Production host at go-live | `[ ]` *(decide: one of the two, or both)* | |

## Dependencies

| Dependency | Version | Supplier | Expiry / renewal |
|---|---|---|---|
| ICP ID Card Toolkit | **v3.1.6** | ICP (Federal Authority for Identity and Citizenship) | — |
| ICP Service Provider licence | `[ ]` | ICP | **`[ ]` — card reading stops at every desk when this expires. See R-06** |
| ICP Validation Gateway | — | ICP | Reachable from reception PCs; **not** from the tablets, which have no internet |
| PC/SC card readers | `[ ]` | `[ ]` | — |
| ACS tablet readers | `[ ]` | ACS | — |
| Microsoft Entra ID | — | Microsoft | App registration client secret: **`[ ]` expiry — diarise** |
| Azure Blob Storage | — | Microsoft | Account key rotation: `[ ]` |
| TLS certificate (UATWEB01) | — | `[ ]` | `[ ]` |

> The two expiry dates in this table — the ICP licence and the Entra client secret — are the
> ones that take the system down without warning. Diarise both, 60 days ahead, owned by a
> role rather than by a person.

## Devices

| Device | Identifier | API key | Reader | Notes |
|---|---|---|---|---|
| Reception tablet 1 | `[ ]` | own key | ACS | |
| Reception tablet 2 | `[ ]` | own key | ACS | |
| Reception tablet 3 | `[ ]` | own key | ACS | **Returns toolkit error 233 — see R-08** |
| Reception PC(s) | `[ ]` | — | PC/SC | ICP desk agent service installed |

One key per tablet is deliberate, so a visit records which desk saved it. Tablets have
access to the backend only and not to the internet; they are secure devices by design.

## Lifecycle

| Event | Date |
|---|---|
| Build started | 2026-09-01 *(first commit)* |
| Intake / classification recorded | `[ ]` |
| UAT completed | `[ ]` — `13-uat-signoff.md` |
| Security review completed | `[ ]` — `07-security-review-checklist.md` |
| Go-live approved | `[ ]` — `14-go-live-approval.md` |
| In production since | `[ ]` |
| Next quarterly review | `[ ]` |

## Registration

| | Name | Signature | Date |
|---|---|---|---|
| Registered in IT Asset Inventory by | | | |
