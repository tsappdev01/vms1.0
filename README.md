# VMS 1.0 — Dubai Investments Visitor Management System

Digital registration and tracking of visitors entering the Dubai Investments office in DIP.

## Repository layout

```
docs/                       Design documentation — start with docs/README.md
db/schema/                  SQL Server DDL (source of truth for the schema)
db/seed/                    Development / UAT master data
db/checks/                  Schema verification
src/DI.Vms.Domain/          Entities, enums, value objects
src/DI.Vms.Application/     Use-case contracts and abstractions
src/DI.Vms.Infrastructure/  EF Core, SQL Server, ID protection
src/DI.Vms.Api/             ASP.NET Core minimal API
src/DI.Vms.Web/             React + TypeScript portal
id-card-toolkit-*/          ICP ID Card Toolkit v3.1.6 (Android, Windows, Web JS)
IDCARDOFFLINE_config_*      Toolkit configuration bundle (QA)
```

## Read first

1. [`docs/00-sdk-analysis.md`](docs/00-sdk-analysis.md) — constraints the BRD does not
   anticipate, including that Emirates ID is read from the card's chip rather than by OCR,
   and that no iOS toolkit exists.
2. [`docs/01-architecture.md`](docs/01-architecture.md).

## Database

UAT: server `UATWEB01`, database `VMS`, schema `vms`.

The DDL in `db/schema` is the source of truth; EF Core maps onto it rather than generating
it. All three steps are re-runnable — running them twice changes nothing.

```
sqlcmd -S UATWEB01 -d VMS -E -C -b -i db\schema\001_create_schema.sql,db\schema\002_create_visitor_visit.sql,db\schema\003_add_idnumber_masked.sql
sqlcmd -S UATWEB01 -d VMS -E -C -b -i db\seed\010_seed_master_data.sql
sqlcmd -S UATWEB01 -d VMS -E -C    -i db\checks\verify_schema.sql
```

`-C` trusts a self-signed certificate (needed on sqlcmd 18+); `-b` stops on the first
error, without which a failed script still reports success.

The seed is development and UAT data only. Real hosts come from an HR extract or an Entra
ID sync; real users come from Entra ID.

## Run

Requires the .NET 8 SDK and Node 20+.

```
dotnet run --project src/DI.Vms.Api
```
Swagger at `https://localhost:7001/swagger`, health at `/health`.

```
cd src/DI.Vms.Web
npm install
npm run dev
```
Portal at `http://localhost:5173`. It defaults to in-memory fixtures; to point it at the
API, create `.env` with `VITE_USE_MOCK=false` and `VITE_API_URL=https://localhost:7001`.

`DI.Vms.Api` must be the startup project in Visual Studio — `DI.Vms.Domain` is a class
library and cannot run.

## Authentication

Not yet implemented. Entra ID (BRD §23) needs an app registration that does not exist yet,
so the API uses a development stand-in that reads an `X-Dev-Role` header
(`SecurityOfficer`, `SecuritySupervisor`, `Admin`, `SystemAdministrator`) and defaults to
`SecuritySupervisor`. It refuses to start when the environment is Production.

Send `X-Dev-Role: SecurityOfficer` to see `GET /api/v1/visitors/{id}/id-number` return 403 —
unmasking an ID number is a per-user grant, not a role (BRD §22).

## Build

```
dotnet build DI.Vms.sln
cd src/DI.Vms.Web && npm run build
```
