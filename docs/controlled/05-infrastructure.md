# Deployment & Infrastructure Guide

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Deployment & Infrastructure Guide (Controlled, required)

The **runbooks** are the existing documents and are not repeated here:

| Document | Covers |
|---|---|
| [`../deployment.md`](../deployment.md) | Running it locally, the UATWEB01 deployment, the desk agent on every reception PC, updating a deployment |
| [`../azure-deployment.md`](../azure-deployment.md) | The Azure Web App, the SQL database, app settings, publishing, and why the Web App's OS cannot be changed afterwards |
| [`../entra-id-setup.md`](../entra-id-setup.md) | The app registration, the five app roles, and who to assign them to |
| [`../android-api.md`](../android-api.md) | Pointing a tablet at a server |
| [`../../db/README.md`](../../db/README.md) | Every SQL script, what it is for, and the order to run them in |

This document is the **inventory**: what exists, where, and what each piece depends on.

---

## 1. Environments

| | On premises | Azure |
|---|---|---|
| Host | **UATWEB01**, IIS, Windows | Web App **VMS**, resource group `DotNetSites`, plan `ASP-DotNetSites-8061`, UAE North |
| URL | internal | `vms-cebrd3evb0cyg0gn.uaenorth-01.azurewebsites.net` |
| OS / stack | Windows, .NET 8 | **Linux**, .NET 8 — confirmed from the container log, not from a table |
| Database | SQL Server on UATWEB01, database **VMS** | `ts-db.database.windows.net`, database **vms**, admin `sqladmin` |
| Reachable from | the office network only | the internet |
| Rate limiting | **not registered** — it does not need it, and a policy name that is not registered throws at startup | 300 req/min per caller IP on `/api` |
| Card images | in `vms.VisitorCardImage.Image` (no storage account) | Azure Blob Storage where configured |

> **The Web App's operating system decides which project can go on it, and it cannot be
> changed after the Web App is created.** `DI.Vms.Blazor` is `net8.0-windows` because it
> references ICP's `IDCardToolkit.dll`. On a **Linux** Web App it does not start and App
> Service answers 503. Build it with `-p:VmsAgentOnly=true` for a Linux host — the same
> application with the toolkit left out — and set `Toolkit__Mode=Agent`.

## 2. Reception hardware

| | |
|---|---|
| Reception PCs | Windows, each with a PC/SC card reader and the ICP desk agent service (`deploy/ICAToolkitService.msi`, installed by `deploy/install-desk-agent.ps1`) |
| Tablets | 3 × Android, each with an ACS reader and its **own** API key, so a visit records which desk saved it |
| Card toolkit | ICP ID Card Toolkit **v3.1.6** — Windows SDK on the PCs, Android AAR + ACS plugin on the tablets |
| ICP licence | Service Provider licence, per device, **expires** — see `10-risk-assessment.md` R-06 |

The reader is local to the desk. That is the single fact the whole deployment shape follows
from, and `../deployment.md` §"Why the desk needs anything installed at all" is the
argument.

## 3. Publish scripts

All in `deploy/`. Pick the one that matches the plan:

| Script | Target | Produces |
|---|---|---|
| `publish.ps1` | reception PC / UATWEB01 | Screens and `/api`, in-process card reader |
| `publish-azure.ps1` | **Windows** Web App | the same build |
| `publish-azure-linux.ps1` | **Linux** Web App | screens and `/api`, portable, no toolkit |
| `publish-api.ps1` | Linux, API only | `DI.Vms.Api` — tablet endpoints, no screens |
| `install-iis.ps1` | UATWEB01 | IIS site and application pool |
| `install-desk-agent.ps1` | each reception PC | the ICP agent service |
| `install-service.ps1` | | the Windows service wrapper |
| `update-server.ps1` | UATWEB01 | pull, build, copy, restart |

## 4. Configuration

**Credentials are never in a committed file.** `appsettings.json` and
`appsettings.Development.json` are both committed; `appsettings.Production.json` is
gitignored. Templates for each environment are in `deploy/`:

- `appsettings.Production.uatweb01.json.template`
- `appsettings.Production.azure.json.template`
- `appsettings.Production.reception-pc.json.template`
- `azure-app-settings.template.json`, `azure-connection-strings.template.json`

On Azure the values belong in **app settings**, not in a file. On a reception PC or
UATWEB01 they belong in `appsettings.Production.json`, which is not in the repository.

Every delivered deployment zip **deliberately excludes** `appsettings.Production.json` and
`appsettings.Development.json`.

The Android app is a **public client**: there is no client secret in the APK, ever. The
tablet's `VMS_API_KEY` is read from `%USERPROFILE%\.gradle\gradle.properties` at build
time, never from the repository's `gradle.properties`.

### Settings that change behaviour

| Setting | Effect |
|---|---|
| `Toolkit:Mode` | `InProcess` (reader on this machine) or `Agent` (reader on the desk, relayed) |
| `Toolkit:Agent:ToolkitConfig` | which ICP config the desk agent uses — `config_ag`, `config_li`, `config_vg_qa` |
| `Authentication:Enabled` | when false, sign-in is off and `RecordedBy` records "(not signed in)" |
| `Api:Key` | the tablet key. Present → the tablet scheme is accepted alongside bearer |
| `Capture:*` | whether this host reads cards at all, and whether the camera scanner is offered |
| `Storage:*` | the blob account and container for card images; absent → images go in the database |

## 5. Database

Schema is created from the EF model by `Data/DbBootstrapper.cs` at startup — absent tables,
absent **nullable** columns, absent indexes. Each is logged, naming what it made.

Everything else needs a script in `db/`, numbered and re-runnable:

```
001_seed_entities.sql              009_add_card_image.sql
002_add_visit_purpose.sql          010_add_person_directory_object_id.sql
003_add_people.sql                 011_add_card_image_blob.sql
004_seed_people.sql  (generated)   012_add_visitor_contact_mobile.sql
005_add_group_companies.sql        013_add_visitor_sign_out.sql
006_grant_app_login.sql
007_add_recorded_by.sql
008_grant_app_user_azure.sql
```

`004_seed_people.sql` is **generated** from the AD export by
`db/tools/generate_seed_people.py` and must never be hand-edited — regenerate it when the
source changes.

Apply with `db/apply.cmd`. All scripts are re-runnable: DDL guarded with
`IF OBJECT_ID(...) IS NULL` and `sys.indexes`, inserts guarded with `NOT EXISTS`, and
`CREATE SCHEMA` and each `CREATE INDEX` in their own `GO` batch.

## 6. CI

| Workflow | Does |
|---|---|
| `.github/workflows/dotnet.yml` — job `build` | restore + build `DI.Vms.Blazor` (`-p:VmsAgentOnly=true`) and `DI.Vms.Api`; `dotnet list package --vulnerable --include-transitive`; `--deprecated`; a tracked-file secret scan |
| `.github/workflows/dotnet.yml` — job `publish` | on `windows-latest`: publishes `vms-web-FOR-WINDOWS-HOST-uatweb01-or-reception-pc` (ICP toolkit included) and `vms-web-FOR-LINUX-HOST-azure-web-app` (agent-only, portable). Named for where each goes — see K-27. Both are attached to the run. The job **fails** if either settings file reached the package, if the native toolkit DLLs did not, or if startup logging was not turned on |
| `.github/workflows/android.yml` | builds the APK |

The audit steps are `continue-on-error: true` and report into the job summary rather than
failing the build — a transitive advisory published overnight should not stop a reception
desk being fixed, and a failure nobody can act on immediately teaches people to ignore the
job.

**Semgrep SAST, required by policy §7, is not yet wired.** See `10-risk-assessment.md` R-07.

## 7. Certificates and network

| | |
|---|---|
| Azure | App Service managed certificate; HSTS set by `Services/SecurityHeaders.cs` |
| UATWEB01 | IIS binding — certificate managed by IT, outside this repository |
| Tablets | **Have access to the backend only, not to the internet.** This is deliberate: they are secure devices. It is also why ICP's Validation Gateway cannot be reached from them and `read_publicdata_offline` matters |
| Reception PCs | Need outbound HTTPS to ICP's gateway for a validated read |
