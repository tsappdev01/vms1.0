# VMS — working notes for Claude

Visitor Management System for the Dubai Investments DIP office. Blazor Server (.NET 8),
SQL Server on **UATWEB01**, database **VMS**. Emirates ID is read from the chip through the
ICP ID Card Toolkit v3.1.6.

`src/DI.Vms.Blazor/README.md` is the engineering record: what was learned from the SDK and
from real cards, and why each non-obvious decision was made. Read it before changing the
card-reading or database-bootstrap paths.

## SQL lives in `db/`

Every SQL script goes in `db/`, committed, so it can be pulled and run — never pasted into
chat only. One file per job, numbered and named for what it does, so the order to run them
in is obvious. `db/README.md` lists them and what each one is for.

A script derived from a data file is **generated, never hand-written** — see
`db/tools/generate_seed_people.py`, which turns the AD export into
`004_seed_people.sql`. Regenerate it when the source changes rather than editing the SQL,
and have the generator fail loudly if the source's shape is not what it expects.

Scripts must be **re-runnable**: guard DDL with `IF OBJECT_ID(...) IS NULL` and
`sys.indexes`, guard inserts with `NOT EXISTS`, and put `CREATE SCHEMA` and each
`CREATE INDEX` in its own `GO` batch. A batch aborts on error and takes the rest of the
batch with it, which hides later statements.

## Reference data is data

The entity list (`vms.Entity`) is maintained by script in `db/`, not seeded from code — no
`HasData`, no startup sync. It must be changeable without a rebuild and a redeploy.

Table *schema* is different: `Data/DbBootstrapper.cs` creates tables from the EF model at
startup, so there is only one definition of the schema. Do not hand-write DDL to create
`vms.VisitorEntry`.

It creates absent tables, adds absent **nullable** columns, and creates absent **indexes**
— so an ordinary addition to the model needs no manual step at all. None of the three can
lose data, and each is logged, naming what it made.

Indexes have one guard: above 500,000 rows the table is left alone and a warning names the
script, because `CREATE INDEX` holds a schema lock and when to take one is then a decision
rather than a detail. Below that it is milliseconds. A failure to create an index is logged
and the application carries on — a slow screen is not a reason to refuse a reception desk.

Everything else still needs a script in `db/`: a **required** column, a widened type, a
rename, a drop. Startup refuses to run when one of those is missing and names it, so the
failure is a clear message rather than `Invalid column name` on the first query. Write the
script anyway for an additive change — SQL lives in `db/` whether or not the application
also applies it — but do not expect anyone to have to run it.
