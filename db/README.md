# db

SQL for VMS on **UATWEB01**, database **VMS**. Run them in number order. Every script is
re-runnable: a second run changes nothing.

| Script | What it does |
|---|---|
| `001_seed_entities.sql` | Creates `vms.Entity` if absent and inserts the DI entity list. Also carries the add / deactivate / rename statements for later. |
| `002_add_visit_purpose.sql` | Adds `Purpose` and `PurposeOther` to `vms.VisitorEntry`, backfilling existing rows as `Not recorded`. |
| `003_add_people.sql` | Creates `vms.Person` and adds the host snapshot columns to `vms.VisitorEntry`. |
| `004_seed_people.sql` | The people themselves. **Generated from `people.csv` — not written by hand.** |

`Data/DbBootstrapper.cs` creates tables that are absent but never alters ones that are
present, so a property added to the EF model needs a script here. Startup checks the
model's columns against the database and refuses to run if any are missing, naming them.

## people.csv

The address list the **Person to visit** type-ahead searches. Four columns, header row
exactly as below:

```
DisplayName,Title,Email,CompanyName
```

`people.csv` in this folder is a two-row sample showing the shape. **Replace it with the
full export** and `004_seed_people.sql` is regenerated from it.

Notes on the data:

- `CompanyName` is matched to `vms.Entity` by name so the picker can narrow suggestions to
  the selected entity. The two lists do not fully agree — the export says
  *Dubai Investments Park* where the entity list says `DIP`, and **Emirates Building
  System has no entity at all**. Unmatched people get a null `DiEntityId` and are found
  only when *Search all entities* is ticked, rather than vanishing. Mapping lives in the
  generated 004 script, so it is visible and editable.
- Rows with a blank `DisplayName` are skipped; that is the only required column.
- Re-running 004 leaves existing people alone and inserts the ones that are new, matched
  on `Email` where there is one and on `DisplayName` where there is not.
