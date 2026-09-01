# VMS 1.0 — Dubai Investments Visitor Management System

Digital registration and tracking of visitors entering the Dubai Investments office in DIP.

## Repository layout

```
docs/                  Design documentation - start with docs/README.md
db/schema/             SQL Server DDL (source of truth for the schema)
src/DI.Vms.Domain/     Domain model: entities, enums, value objects
id-card-toolkit-*/     ICP ID Card Toolkit v3.1.6 (Android, Windows, Web JS)
IDCARDOFFLINE_config_* Toolkit configuration bundle (QA)
```

## Getting started

1. Read [`docs/00-sdk-analysis.md`](docs/00-sdk-analysis.md) — it documents constraints the
   BRD does not anticipate.
2. Read [`docs/01-architecture.md`](docs/01-architecture.md).
3. Apply `db/schema/*.sql` in filename order.

## Database

UAT: server `UATWEB01`, database `VMS`, schema `vms`.

The DDL in `db/schema` is the source of truth; EF Core maps to it rather than generating it.

## Build

Requires the .NET 8 SDK.

```
dotnet build DI.Vms.sln
```
