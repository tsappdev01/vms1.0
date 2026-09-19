# db

SQL for the **VMS** database. Run them in number order. Every script is re-runnable: a
second run changes nothing that has not changed at the source.

**Connect to the VMS database before running any of them.** None of them switch databases:
Azure SQL does not support `USE`, so the same file has to work against SQL Server on
UATWEB01 and against Azure SQL. `sqlcmd -d VMS`, or pick it in SSMS. Each script guards
against being run in `master` and stops without changing anything if it is.

| Script | What it does |
|---|---|
| `001_seed_entities.sql` | Creates `vms.Entity` if absent and inserts the DI entity list. Also carries the add / deactivate / rename statements for later. |
| `002_add_visit_purpose.sql` | Adds `Purpose` and `PurposeOther` to `vms.VisitorEntry`, backfilling existing rows as `Not recorded`. |
| `003_add_people.sql` | Creates `vms.Person` and adds the host snapshot columns to `vms.VisitorEntry`. |
| `004_seed_people.sql` | The 725 people from the AD export. **Generated — see below.** |
| `005_add_group_companies.sql` | **Optional.** Adds the 13 group companies the address list has and the entity list does not. A decision, not a fix — the script explains it. |
| `006_grant_app_login.sql` | **Deployment, SQL Server only.** Creates the login and user for the Windows identity the app runs as, and grants it reader/writer on `vms` — not `db_owner`. Edit `@Login` at the top first. See [docs/deployment.md](../docs/deployment.md) step 4. Does not work on Azure SQL — use `008`. |
| `007_add_recorded_by.sql` | **Run before deploying the build that signs users in.** Adds `RecordedBy` to `vms.VisitorEntry` — who saved each entry. Startup refuses to run without it, which is the intended failure. |
| `008_grant_app_user_azure.sql` | **Deployment, Azure SQL only.** The counterpart of `006`: a contained database user for the Web App, by managed identity for preference. Its header carries the order the whole Azure database has to be set up in. See [docs/azure-deployment.md](../docs/azure-deployment.md). |

## Which scripts for which database

| | UATWEB01 (SQL Server) | Azure SQL |
|---|---|---|
| Schema and reference data | `001`–`005`, `007` | `001`–`005`, `007` — the same files |
| Access for the app | `006` | `008` |

There is no schema-creation script: `Data/DbBootstrapper.cs` creates the tables from the
EF model at startup. On a brand new Azure SQL database that means starting the app once
with an administrator's connection string, then switching it to the least-privileged user
`008` creates. `008`'s header sets out the order.

`Data/DbBootstrapper.cs` creates tables that are absent but never alters ones that are
present, so a property added to the EF model needs a script here. Startup checks the
model's tables and columns against the database and refuses to run if any are missing,
naming them and pointing back at this folder.

## The people list

`AD Export 03_07_2026.xlsx` is the source. **`004_seed_people.sql` is generated from it —
do not edit the SQL by hand.** When a new export lands, replace the workbook and run:

```
python3 db/tools/generate_seed_people.py "db/AD Export 2026-11-01.xlsx"
```

The generator reads the `.xlsx` directly, with no dependencies, and expects sheet 1 to
carry exactly these four columns: `Display Name`, `Title`, `Email Address`,
`Company Name`. It fails loudly if they differ rather than writing a wrong script.

Re-running `004` is safe and worthwhile: it inserts people who are new, refreshes the
title and company of people already there — a job changes more often than a name — and
re-resolves every `DiEntityId`.

### Company names, and why 436 people have no entity

The export names companies in full (*Dubai Investments Park*) where the entity list uses
short forms (`DIP`). Twelve companies map; **13 have no entity at all**, and they account
for 436 of the 725 people — the majority. Largest first: Emirates Building System (189),
Emirates Glass (98), Emirates Extrusion Factory (48), White Aluminium Extrusion (29).

Those people are **kept**, with a null `DiEntityId`, and found in the picker under
*Search all entities* rather than dropped. But *Entity being visited* cannot name their
company, so a visit to Emirates Building System has to be filed against something else.
`005` fixes that if those companies do receive visitors at the DIP desk. The mapping lives
in `COMPANY_TO_ENTITY` in the generator, and a company with no mapping still links if an
entity of exactly that name exists — so `005` needs no edit to the map.
