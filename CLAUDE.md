# VMS — working notes for Claude

Visitor Management System for the Dubai Investments DIP office. Blazor Server (.NET 8),
SQL Server on **UATWEB01**, database **VMS**. Emirates ID is read from the chip through the
ICP ID Card Toolkit v3.1.6.

`src/DI.Vms.Blazor/README.md` is the engineering record: what was learned from the SDK and
from real cards, and why each non-obvious decision was made. Read it before changing the
card-reading or database-bootstrap paths.

## SQL lives in `db/`

Every SQL script goes in `db/`, committed, so it can be pulled and run — never pasted into
chat only. One file per job, named for what it does.

Scripts must be **re-runnable**: guard DDL with `IF OBJECT_ID(...) IS NULL` and
`sys.indexes`, guard inserts with `NOT EXISTS`, and put `CREATE SCHEMA` and each
`CREATE INDEX` in its own `GO` batch. A batch aborts on error and takes the rest of the
batch with it, which hides later statements.

## Reference data is data

The entity list (`vms.Entity`) is maintained by script in `db/`, not seeded from code — no
`HasData`, no startup sync. It must be changeable without a rebuild and a redeploy.

Table *schema* is different: `Data/DbBootstrapper.cs` creates tables from the EF model at
startup, so there is only one definition of the schema. Do not hand-write DDL for
`vms.VisitorEntry`.
