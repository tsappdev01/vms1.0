# Admin Guide

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Admin Guide (Controlled, required · IT)

For IT. Deployment itself is in `05-infrastructure.md` and the runbooks it points at; this
is the things IT has to do **after** it is running.

---

## 1 · Granting and removing access

Roles are **Entra ID app roles** on the VMS app registration, not security groups.
`../entra-id-setup.md` is how the registration and the roles are created.

| To | Assign |
|---|---|
| Work a reception desk | `Vms.Officer` |
| Work a desk **and** read the report | `Vms.Supervisor` |
| Administer | `Vms.Admin` or `Vms.SystemAdmin` |
| See an Emirates ID number in full | `Vms.UnmaskedId` — **granted per person, never by seniority** |

**Removing access:** remove the app-role assignment in Entra. The change takes effect on
their next sign-in; an open Blazor circuit keeps its claims until the session ends.

Review assignments **quarterly** against the reception roster (policy §2). Record the review
on `09-access-control-matrix.md` §7.

## 2 · Changing the entity list

`vms.Entity` is **reference data, not schema**. It is maintained by script in `db/` so the
group's companies can change without a rebuild and a redeploy.

Add a script in `db/`, numbered, guarded with `NOT EXISTS`, and run it. Do not insert by
hand at the server — a script is the record of what was done.

```sql
IF NOT EXISTS (SELECT 1 FROM vms.Entity WHERE Name = N'New Company LLC')
    INSERT INTO vms.Entity (Name, IsActive) VALUES (N'New Company LLC', 1);
```

To retire an entity, set `IsActive = 0`. **Do not delete it** — visits reference it, and
last year's report must still read correctly.

The new entity appears at the next page load. No restart.

## 3 · Updating the staff directory

Two sources, and the application prefers the first:

1. **Entra ID**, live, when the tenant is configured. Nothing to maintain.
2. **`vms.Person`**, from the AD export, otherwise.

For (2): the export is `db/AD Export 03_07_2026.xlsx` and `db/004_seed_people.sql` is
**generated from it**:

```
python3 db/tools/generate_seed_people.py
```

**Never hand-edit `004_seed_people.sql`.** Replace the export and regenerate. The generator
fails loudly if the export's shape is not what it expects, which is the point of it.

## 4 · Schema changes

One definition of the schema: the EF model in `src/DI.Vms.Blazor/Data/`.

| Change | What happens |
|---|---|
| New table | `DbBootstrapper` creates it at startup, and logs that it did |
| New **nullable** column | Added at startup, logged |
| New index | Created at startup, logged |
| New **required** column, a widened type, a rename, a drop | **Startup refuses and names the script you must write.** Not a crash later at a desk |

Write the script in `db/` either way — SQL lives in `db/` whether or not the application
also applies it — but for an additive change nobody has to run it.

**One guard on indexes:** above 500,000 rows the table is left alone and a warning names the
script. `CREATE INDEX` takes a schema lock, and when to take one is a decision rather than a
detail. Below that it is milliseconds.

After any model change, regenerate the data dictionary:

```
python3 docs/tools/generate_data_dictionary.py
```

## 5 · Rotating credentials

| Credential | Where | Rotate by |
|---|---|---|
| SQL connection string | Azure app settings, or `appsettings.Production.json` (gitignored) | Change the password, update the setting, restart |
| Entra client secret | Azure app settings | New secret in the app registration, update the setting, restart. **Diarise the expiry** |
| Blob storage AccountKey | Azure app settings | Rotate key, update the setting, restart |
| Tablet API keys | Server setting **and** each tablet's Settings screen | Change server-side, then each tablet. One key per tablet — do not collapse them into one |

**A credential never goes in `appsettings.json` or `appsettings.Development.json`.** Both
are committed. The CI secret-scan step exists to catch exactly that.

## 6 · Setting up a tablet

1. Install the APK (CI builds it; `../android-api.md` has the detail).
2. Open **Settings** — set the server address and this tablet's **own** API key.
3. Press **Test**. It should report success. A wrong key says so specifically.
4. **Set a PIN.** This is what stops a visitor at the counter reading the API key.
5. Record the device and its key on `11-it-asset-registration.md`.

The tablet has access to the backend only, not to the internet. That is deliberate.

**A forgotten PIN cannot be recovered.** Clear the app's data in Android settings — which
also clears the server address and the key — and set the tablet up again.

## 7 · Reading the startup log

The first thing to look at when anything is wrong. It names:

- which card-reading mode the host is in (`InProcess` or `Agent`)
- every table, column and index the bootstrapper created
- any missing migration, by script name, followed by a refusal to start
- whether sign-in is on

`../android-api.md` names the one startup line to read before debugging anything on a
tablet.

## 8 · Toolkit error 233

`ETSTATUS_SERVER_RESPONSE_ERROR`. The toolkit reached for ICP's Validation Gateway during
`readPublicData` and could not get an answer.

**It is not a build problem.** The previous APK fails on the same tablet.

| Check | |
|---|---|
| Is `read_publicdata_offline` set? | It is, in the shipped config |
| Does the Settings switch help? | "Read cards without ICP's gateway" (default off) withholds `config_ag` |
| What does the reader panel say? | `<reader name> · Toolkit <version> · licence to <date>` — **compare it between a working tablet and a failing one** |
| What is `Toolkit:Agent:ToolkitConfig` on the server? | The desk reads offline successfully on the same licence; the value is worth comparing |

Why one tablet fails and another on the same network does not is **unresolved**. It is
carried as R-08 and the two facts above are what is needed to take it further with ICP.

> A note that is worth more than it looks: this flag was once removed on the strength of a
> search of the wrong binary — `libc++_shared.so` is the C++ runtime, and `classes.jar` is
> compressed so `strings` finds nothing in it. The binary that ships it is
> `libEIDAToolkitJNIWrapper.so`. **Search the binary that ships.**

## 9 · ICP licence

Every device is registered with ICP's Validation Gateway against a **Service Provider
licence, and it expires.** When it does, card reading stops at every desk at once.

- The expiry is shown on the reader panel and recorded on `11-it-asset-registration.md`
- **Diarise a reminder 60 days ahead, owned by a role, not by a person**
- Manual entry works without the licence; reception already knows how

## 10 · Routine checks

| When | Check |
|---|---|
| Daily | `/health` returns healthy |
| Weekly | The CI job summaries: vulnerable packages, deprecated packages, secret scan |
| Monthly | A restore from backup has been tested this quarter (`17-backup-and-recovery.md`) |
| Quarterly | Role assignments against the reception roster; risk register review; ICP licence and Entra secret expiry dates |

## 11 · What IT must not do

- **Do not edit a visit in the database.** There is no edit path through the application
  on purpose, which is why there is no audit table. A hand-edited row is a row nothing
  will ever show was changed.
- **Do not hand-write DDL for `vms.VisitorEntry`.** The EF model is the definition.
- **Do not hand-edit `db/004_seed_people.sql`.** Regenerate it.
- **Do not put a secret in a committed file.**
- **Do not give one API key to more than one tablet.** The key is how a visit records which
  desk saved it.
